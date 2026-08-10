using LogiCard.Audio;
using LogiCard.Board;
using LogiCard.Net;
using LogiCard.Rendering;
using LogiCard.Sim;
using LogiCard.Timeline;
using LogiCard.UI;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace LogiCard.Boot
{
    /// <summary>
    /// Vertical slice scaffold: builds the continuous arena, the Time Resource clock,
    /// the portrait HUD, two pawns, MatchClock (C33), and the local resolve/playback loop.
    ///
    /// The attacker is player-programmed via BoardInputController; the defender runs a
    /// scripted program rebuilt each round from its carried position. Both reach the
    /// resolver as ordinary payloads.
    /// </summary>
    [DefaultExecutionOrder(-100)]
    public sealed class GameBootstrap : MonoBehaviour
    {
        [Header("Match Time Resource (C33)")]
        [Tooltip("Shared match pool in Time Resource seconds (demo 15 minutes).")]
        public float matchPoolSeconds = 900f;

        [Tooltip("Smallest Time Card a chooser may play.")]
        public float minRoundSeconds = 30f;

        [Tooltip("Playback Duration seconds per one Time Resource second (C27). Default keeps the old 60s→8s feel.")]
        public float playbackSecondsPerTimeResourceSecond = 8f / 60f;

        [Header("Debug")]
        [Tooltip("Adds the Program/Reveal/Execute jump buttons to the thumb zone (Day 2 debug aid).")]
        public bool showPhaseDebugControls;

        public const int AttackerPawnId = 1;
        public const int DefenderPawnId = 2;

        private BoardView _board;
        private TimeResourceClockDriver _clock;
        private RoundPhaseController _phase;
        private BoardInputController _attackerInput;
        private RoundPlayback _playback;
        private MatchClock _matchClock;
        private IFoleyPlayer _foley;
        private BoardCameraRig _cameraRig;
        private const float DefenderSecondsPerTile = 2f;

        public MatchClock MatchClock => _matchClock;

        public RoundPlayback Playback => _playback;

        public BoardInputController AttackerInput => _attackerInput;

        public BoardCameraRig CameraRig => _cameraRig;

        /// <summary>
        /// Test/authoring helper: play a Time Card and enter Program without going through the HUD.
        /// </summary>
        public bool BeginRound(float seconds)
        {
            if (_phase.Phase != RoundPhase.Allot)
            {
                _phase.GoTo(RoundPhase.Allot);
            }

            if (!_matchClock.TryPlayTimeCard(seconds, out string reason))
            {
                Debug.LogWarning($"[logiCard] BeginRound rejected: {reason}");
                return false;
            }

            float allotment = _matchClock.RoundAllotment;
            _clock.ApplyBudget(allotment);
            _attackerInput.PrepareRound(_playback.PositionOf(AttackerPawnId), allotment);
            _phase.GoTo(RoundPhase.Program);
            return true;
        }

        /// <summary>Test helper: advance Aftermath → next Allot (or Match Over).</summary>
        public void RequestNextRound()
        {
            OnNextRoundRequested();
        }

        private void Awake()
        {
            _matchClock = new MatchClock(matchPoolSeconds, minRoundSeconds, MatchSide.Attacker);

            _clock = gameObject.AddComponent<TimeResourceClockDriver>();
            _clock.PlaybackSecondsPerTimeResourceSecond = playbackSecondsPerTimeResourceSecond;

            _phase = gameObject.AddComponent<RoundPhaseController>();
            _foley = gameObject.AddComponent<FoleyPlayer>();
            gameObject.AddComponent<PhotoModeController>();

            BuildBoard();
            BuildPawns();
            ConfigureCamera();
            BuildLighting();
            BuildWeatherPocket();
            // Wet-surface reflection follow-up (feat/wet-surface-reflections) — scoped, single-call
            // addition mirroring BuildWeatherPocket immediately above; all the actual work lives in
            // BoardReflectionProbes, not here.
            BuildReflectionProbes();

            var hud = new GameObject("ProgramHud").AddComponent<ProgramHud>();
            hud.transform.SetParent(transform, false);
            hud.Init(_clock, _phase, _attackerInput, _matchClock, showPhaseDebugControls, _foley);

            hud.LockedIn += _playback.ResolveAndArm;
            hud.TimeCardPlayed += OnTimeCardPlayed;
            hud.NextRoundRequested += OnNextRoundRequested;
            _playback.OutcomeReported += hud.ShowOutcome;

            // Camera rotation (C48/C53 playtest ask) is direct right-mouse-drag on BoardCameraRig
            // itself now — no HUD button/event to wire. After any rotation, the door prompt's cached
            // world-to-screen projection (docs/UI_BOARD_ANCHORED_COMPONENTS.md — "recompute only on
            // selection change") is stale, so re-run it through the same refresh path a selection
            // change already uses.
            _cameraRig.Rotated += hud.RefreshBoardAnchoredUI;

            // Find Match -> C52's resolve relay; Local Play -> same-process (unchanged). Board layout
            // must match Relay/LogiCard.Relay/DemoArenaBoard.CreateDemo() for a two-Unity smoke test to
            // resolve identically. Host/port not yet configurable from the Lobby (real matchmaking is
            // still OPEN, NETWORKING_DESIGN.md) - both instances default to the same localhost port.
            hud.AppFlow.EnteredMatch += viaRelay => _playback.SetMatchResolver(
                viaRelay
                    ? (IMatchResolver)new RelayMatchResolver()
                    : new LocalMatchResolver(new GhostResolver(_board.Model)));

            Debug.Log($"[logiCard] Slice up: continuous arena [{_board.Model.MinX},{_board.Model.MaxX}]×" +
                      $"[{_board.Model.MinY},{_board.Model.MaxY}], match pool {matchPoolSeconds:0}s TR, " +
                      $"min round {minRoundSeconds:0}s.");
        }

        private void OnTimeCardPlayed(float seconds)
        {
            if (!_matchClock.TryPlayTimeCard(seconds, out string reason))
            {
                Debug.LogWarning($"[logiCard] Time Card rejected: {reason}");
                return;
            }

            float allotment = _matchClock.RoundAllotment;
            _clock.ApplyBudget(allotment);

            PlanarPosition attackerOrigin = _playback.PositionOf(AttackerPawnId);
            _attackerInput.PrepareRound(attackerOrigin, allotment);

            Debug.Log($"[logiCard] Time Card played by {_matchClock.CurrentChooser}: {allotment:0.0}s " +
                      $"(match remaining {_matchClock.RemainingSeconds:0.0}s, round {_matchClock.RoundIndex}).");

            _phase.GoTo(RoundPhase.Program);
        }

        private void OnNextRoundRequested()
        {
            if (_phase.Phase != RoundPhase.Aftermath)
            {
                return;
            }

            if (_playback.AnyoneDead || !_matchClock.CanFundAnotherRound)
            {
                _phase.GoTo(RoundPhase.MatchOver);
                Debug.Log("[logiCard] Match Over.");
                return;
            }

            _matchClock.EndRound();
            _phase.GoTo(RoundPhase.Allot);
            Debug.Log($"[logiCard] Round {_matchClock.RoundIndex}: {_matchClock.CurrentChooser} picks the Time Card.");
        }

        // No map-select UI yet (docs/DRAFT_HANDOFF.md's map-roster plan flags this as an explicit
        // follow-up, not attempted here) — one constant default until that exists.
        private const MapId ActiveMap = MapId.FreightYard;

        private void BuildBoard()
        {
            BuildBoard(ActiveMap);
        }

        private void BuildBoard(MapId mapId)
        {
            var boardGo = new GameObject("Board");
            boardGo.transform.SetParent(transform, false);
            _board = boardGo.AddComponent<BoardView>();

            ArenaBoard model;
            switch (mapId)
            {
                case MapId.FreightYard:
                    model = BuildFreightYardGeometry();
                    break;
                default:
                    // RailPlatform/VaultComplex land in a follow-up checkpoint — fail loudly instead
                    // of silently falling back to the wrong map's geometry.
                    throw new System.NotImplementedException(
                        $"GameBootstrap.BuildBoard({mapId}): this map's geometry isn't authored yet.");
            }

            // C53: room-zoned wet-dusk surfaces + terrain edge — palette owned by BoardSurfaceMaterials.
            _board.Build(model, MapDefinitions.ForId(mapId));
        }

        /// <summary>
        /// Multi-room layout (C45, 2026-08-08 — supersedes the earlier single-room [0,4]x[0,4] board,
        /// C39 item 7): Yard (open, attacker spawn) -> Hall (walled kill-box, Door #1 frontal / Door #2
        /// rear) -> Vault (open), with unguarded flank corridors on either side of Hall (x&lt;2 / x&gt;6).
        /// Hall's side walls are solid, so a defender holed up inside has zero LoS into either flank —
        /// flanking is safe by construction, not AI restraint. Gives the Scout/Juggernaut Sprint-speed
        /// asymmetry (1s vs 2s per tile) an actual tactical lever: short-but-guarded center vs.
        /// longer-but-safe flank. Both doors start Closed (Phase 6 / CONTINUOUS_PIVOT_PLAN.md); the
        /// scripted defender opens Door #1 before its Snap so AmbushPoint LoS works. Door #2 is a
        /// player-discoverable depth objective the scripted defender never touches.
        /// </summary>
        private static ArenaBoard BuildFreightYardGeometry()
        {
            var model = new ArenaBoard(0f, 0f, 8f, 10f, new[] { Floor.Ground });

            model.RegisterWall(new Segment(new PlanarPosition(2f, 4f), new PlanarPosition(3.75f, 4f)));
            model.RegisterWall(new Segment(new PlanarPosition(4.25f, 4f), new PlanarPosition(6f, 4f)));
            model.RegisterDoor(new Door(
                new Segment(new PlanarPosition(3.75f, 4f), new PlanarPosition(4.25f, 4f)),
                DoorState.Closed,
                displayName: "Door #1"));

            // West wall split around a Vent (narrow grate bypass from the west flank straight into
            // Hall, past the frontal chokepoint) — same interact/resolve pipeline as a door, just
            // narrower (0.4 vs. a normal door's 0.5) and re-skinned in BoardView. Both sides can open
            // and close it repeatedly, unlike a Breach.
            model.RegisterWall(new Segment(new PlanarPosition(2f, 4f), new PlanarPosition(2f, 5.3f)));
            model.RegisterWall(new Segment(new PlanarPosition(2f, 5.7f), new PlanarPosition(2f, 7f)));
            model.RegisterDoor(new Door(
                new Segment(new PlanarPosition(2f, 5.3f), new PlanarPosition(2f, 5.7f)),
                DoorState.Closed,
                displayName: "Vent Cover",
                kind: DoorKind.Vent));

            // East wall split around a Breach (permanent flank route once someone pays to open it —
            // the UI never offers Close again for this one, see ProgramHud.RefreshDoorPrompt).
            model.RegisterWall(new Segment(new PlanarPosition(6f, 4f), new PlanarPosition(6f, 4.6f)));
            model.RegisterWall(new Segment(new PlanarPosition(6f, 5.0f), new PlanarPosition(6f, 7f)));
            model.RegisterDoor(new Door(
                new Segment(new PlanarPosition(6f, 4.6f), new PlanarPosition(6f, 5.0f)),
                DoorState.Closed,
                displayName: "Cracked Wall",
                kind: DoorKind.Breach));

            model.RegisterWall(new Segment(new PlanarPosition(2f, 7f), new PlanarPosition(3.75f, 7f)));
            model.RegisterWall(new Segment(new PlanarPosition(4.25f, 7f), new PlanarPosition(6f, 7f)));
            model.RegisterDoor(new Door(
                new Segment(new PlanarPosition(3.75f, 7f), new PlanarPosition(4.25f, 7f)),
                DoorState.Closed,
                displayName: "Door #2"));

            return model;
        }

        private void BuildPawns()
        {
            // Column-aligned with Hall's spine (C45 multi-room layout) — attacker starts at the south
            // edge of the Yard, defender spawns inside Hall near its north wall.
            var attackerHome = new PlanarPosition(4f, 0f);
            const float attackerSecondsPerTile = 1f;
            var defenderSpawn = new PlanarPosition(4f, 6f);

            // Speeds already match the Scout/Juggernaut CharacterData presets (1s vs 2s per tile) —
            // the visual build follows the same archetype so silhouette and movement read together.
            PawnView attacker = SpawnPawn("Pawn_Attacker", new Color(0.90f, 0.35f, 0.28f), attackerHome, attackerSecondsPerTile, PawnBuild.Scout);
            PawnView defender = SpawnPawn("Pawn_Defender", new Color(0.32f, 0.58f, 0.86f), defenderSpawn, DefenderSecondsPerTile, PawnBuild.Juggernaut);

            _attackerInput = attacker.gameObject.AddComponent<BoardInputController>();
            _attackerInput.Init(attacker, _phase, attackerHome, attackerSecondsPerTile, 0f, _board);

            _playback = gameObject.AddComponent<RoundPlayback>();
            _playback.Init(_board, _clock, _phase, _foley);
            _playback.Register(AttackerPawnId, attacker, attackerHome, () => _attackerInput.Program.Build());
            _playback.Register(DefenderPawnId, defender, defenderSpawn, BuildDefenderPayload);

            Debug.Log($"[logiCard] Attacker spawn {attackerHome}; defender spawn {defenderSpawn}.");
        }

        /// <summary>
        /// Scripted defender rebuilt each Lock In from its carried point and the round allotment.
        /// </summary>
        private LogiCard.Net.TimelinePayload BuildDefenderPayload()
        {
            PlanarPosition start = _playback.PositionOf(DefenderPawnId);
            float budget = _matchClock.RoundAllotment;
            var program = new PawnProgram(start, DefenderSecondsPerTile, budget, StanceType.Walk, _board.Model);

            // Approach Door #1 from inside Hall (north side), step into InteractRadius, open it, Snap
            // south onto AmbushPoint (4,3), then edge back. Opening is what makes the shot's LoS legal —
            // without it the Closed door blocks the wound the PlayMode suite asserts. Door #1 is always
            // the nearer door from this approach (0.35 units vs. Door #2's ~2.65), so
            // TryGetNearestDoor(..., float.MaxValue) inside TryScriptDoor still resolves it correctly
            // now that the board has two doors. The defender never interacts with Door #2 (Hall->Vault) —
            // that's a player-discoverable depth objective, not scripted-AI scope (C45).
            TryScriptMove(program, new PlanarPosition(4f, 4.6f));
            TryScriptMove(program, new PlanarPosition(4f, 4.35f));
            TryScriptDoor(program, DoorAction.Open);
            TryScriptShoot(program, new PlanarPosition(4f, 3f));
            TryScriptMove(program, new PlanarPosition(4f, 4.3f));

            return program.Build();
        }

        private static void TryScriptMove(PawnProgram program, PlanarPosition destination)
        {
            if (!program.TryQueueMove(destination, out _))
            {
                // Over-budget or unreachable — fine for a stub AI.
            }
        }

        private void TryScriptDoor(PawnProgram program, DoorAction action)
        {
            if (!_board.Model.TryGetNearestDoor(program.CurrentPosition, float.MaxValue, out Door door))
            {
                return;
            }

            // BUG FOUND 2026-08-06 (playtest): this used to queue every round unconditionally, so the
            // scripted defender re-opened the door every single round no matter what the player did —
            // including silently undoing a Close the player had just booked. Harmless before door
            // state persisted across rounds (nothing carried over to undo); a real bug once it did.
            // Now a no-op if the door's live state already matches what this action would produce.
            DoorState impliedState = action == DoorAction.Open ? DoorState.Open : DoorState.Closed;
            if (_board.Model.GetDoorState(door) == impliedState)
            {
                return;
            }

            if (!program.TryQueueDoor(door, action, out _))
            {
                // Out of range / over-budget — fine for a stub AI.
            }
        }

        private static void TryScriptShoot(PawnProgram program, PlanarPosition aim)
        {
            if (!program.TryQueueShoot(aim, out _))
            {
                // Over-budget or illegal aim — fine for a stub AI.
            }
        }

        private PawnView SpawnPawn(string name, Color color, PlanarPosition home, float baseSecondsPerTile, PawnBuild build)
        {
            var go = new GameObject(name);
            go.transform.SetParent(transform, false);
            var pawn = go.AddComponent<PawnView>();
            pawn.Init(_board, color, ScheduledPath.FromWaypoints(new[] { home }, baseSecondsPerTile, StanceType.Walk), build);
            return pawn;
        }

        private void ConfigureCamera()
        {
            Camera cam = Camera.main;
            if (cam == null)
            {
                var camGo = new GameObject("Main Camera", typeof(Camera));
                camGo.tag = "MainCamera";
                cam = camGo.GetComponent<Camera>();
            }

            // Landscape desktop (C48): board region is full width, from the top of the bottom HUD dock
            // to the bottom of the top strip (dock moved off the right edge to a bottom band,
            // 2026-08-10 — general vertical alignment) — see ProgramHud's HudDockHeight/TopStripHeight
            // constants and docs/contracts/CURRENT.md's frozen note on this coupling.
            cam.rect = new Rect(0f, ProgramHud.HudDockHeight, 1f, 1f - ProgramHud.HudDockHeight - ProgramHud.TopStripHeight);
            cam.orthographic = true;
            // Was 9.0f, a blind proportional-scale estimate never actually verified in the Editor (see
            // DRAFT_HANDOFF.md's long-standing "needs an eyeball check" flag on this line). A human
            // screenshot (2026-08-09) showed it badly over-zoomed — the board occupied only ~43% of the
            // board region's height, with the dark void apron (BoardView.PlaceVoidDressing) dominating
            // the frame above and below as what read like solid black bars, not a margin. Recalibrated
            // from that measurement (9.0 * 43/77, targeting ~75-80% board coverage) to 5.0f — still an
            // estimate pending another human look, not a final tuned value.
            cam.orthographicSize = 5.0f;
            // Solid-color void stays — C53 weather is a contained pocket above the board, not a skybox
            // horizon. Cooler near-black so the stormy pocket reads against the void (was warmer grey).
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(0.035f, 0.04f, 0.055f);

            // Pitch/distance/position are BoardCameraRig's job now (yaw rotation, C48) — it owns the
            // same Euler(52,0,0)-from-center-minus-forward*14 shape this used to set directly.
            _cameraRig = cam.GetComponent<BoardCameraRig>();
            if (_cameraRig == null)
            {
                _cameraRig = cam.gameObject.AddComponent<BoardCameraRig>();
            }

            _cameraRig.Init(cam, _board.CenterWorld);

            // Post-processing is off by default per camera (URP) even when the pipeline supports it;
            // without this the diorama Volume below has nothing to render through (playtest 2026-08-07:
            // "too plain and dull" — the scene had no shadows, fill light, or grade at all).
            cam.GetUniversalAdditionalCameraData().renderPostProcessing = true;

            // The scripted camera above skips Unity's "GameObject > Camera" menu path, which is the
            // only place Unity auto-adds this — without it FoleyPlayer.Play would silently produce no
            // audible sound (just a console "no audio listeners" warning), and the Day 11 human
            // ear-check would have nothing to listen to.
            if (cam.GetComponent<AudioListener>() == null)
            {
                cam.gameObject.AddComponent<AudioListener>();
            }
        }

        private void BuildLighting()
        {
            if (FindFirstObjectByType<Light>() != null)
            {
                return;
            }

            // C53 wet-dusk key: cool overcast, softer shadows than the old warm desk-lamp. Direction
            // stays high so the cloud pocket above the board casts readable contact on the chunk.
            var keyGo = new GameObject("Storm Key");
            keyGo.transform.SetParent(transform, false);
            var key = keyGo.AddComponent<Light>();
            key.type = LightType.Directional;
            key.color = new Color(0.58f, 0.66f, 0.82f);
            key.intensity = 0.95f;
            key.shadows = LightShadows.Soft;
            key.shadowStrength = 0.55f;
            key.transform.rotation = Quaternion.Euler(50f, -25f, 0f);

            // Warm practical fill — bounce from interior windows/lamps (reinforced by point lights below).
            var fillGo = new GameObject("Warm Practical Fill");
            fillGo.transform.SetParent(transform, false);
            var fill = fillGo.AddComponent<Light>();
            fill.type = LightType.Directional;
            fill.color = new Color(0.92f, 0.72f, 0.48f);
            fill.intensity = 0.22f;
            fill.shadows = LightShadows.None;
            fill.transform.rotation = Quaternion.Euler(28f, 145f, 0f);

            // Localized warm practicals at Hall/Vault window dressing — reference's lit-window language
            // translated indoors. Point lights (not more directionals) so wet floors catch glints.
            PlacePracticalPoint("Hall Practical W", new PlanarPosition(2.2f, 5.5f), 0.7f, 3.2f, 1.1f);
            PlacePracticalPoint("Hall Practical E", new PlanarPosition(5.8f, 5.5f), 0.7f, 3.2f, 1.1f);
            PlacePracticalPoint("Vault Practical", new PlanarPosition(4f, 9.4f), 0.7f, 4.0f, 1.35f);
            PlacePracticalPoint("Yard Spill", new PlanarPosition(4f, 2.2f), 0.55f, 3.5f, 0.55f);

            RenderSettings.ambientMode = AmbientMode.Flat;
            RenderSettings.ambientLight = new Color(0.09f, 0.11f, 0.15f);

            BuildDioramaVolume();
        }

        private void PlacePracticalPoint(string name, PlanarPosition planar, float height, float range, float intensity)
        {
            var go = new GameObject(name);
            go.transform.SetParent(transform, false);
            go.transform.position = _board.WorldFromPlanar(planar) + new Vector3(0f, height, 0f);
            var light = go.AddComponent<Light>();
            light.type = LightType.Point;
            light.color = new Color(1f, 0.72f, 0.42f);
            light.intensity = intensity;
            light.range = range;
            light.shadows = LightShadows.Soft;
            light.shadowStrength = 0.35f;
        }

        /// <summary>
        /// Contained stormy sky + rain above the board (C53). Sits in the space above the existing
        /// chunk; does not replace the dark void or change camera clear flags.
        /// </summary>
        private void BuildWeatherPocket()
        {
            var weatherGo = new GameObject("WeatherPocket");
            weatherGo.transform.SetParent(transform, false);
            weatherGo.AddComponent<BoardWeatherPocket>().Build(_board);
        }

        /// <summary>
        /// One Reflection Probe per room (Yard/Hall/Vault) so the wet-dusk floors in
        /// <see cref="BoardSurfaceMaterials"/> have a real reflection source instead of bare
        /// smoothness. See <see cref="LogiCard.Board.BoardReflectionProbes"/> for placement details.
        /// </summary>
        private void BuildReflectionProbes()
        {
            var probesGo = new GameObject("ReflectionProbes");
            probesGo.transform.SetParent(transform, false);
            probesGo.AddComponent<BoardReflectionProbes>().Build(_board);
        }

        /// <summary>
        /// Global post-process grade for the wet-dusk diorama (C53): cool color filter, higher
        /// contrast, restrained bloom for wet glints (not glow-game lasers — ART_DIRECTION §3), and
        /// a vignette that keeps the void reading as the stage surround. DoF kept for the miniature
        /// tilt-shift read; aperture slightly stopped down vs. the old toy pass so checkpoint-1
        /// gameplay readability stays available for the human look.
        /// </summary>
        private void BuildDioramaVolume()
        {
            var volumeGo = new GameObject("Diorama Volume");
            volumeGo.transform.SetParent(transform, false);
            var volume = volumeGo.AddComponent<Volume>();
            volume.isGlobal = true;

            var profile = ScriptableObject.CreateInstance<VolumeProfile>();

            var color = profile.Add<ColorAdjustments>(true);
            color.postExposure.overrideState = true;
            color.postExposure.value = -0.12f;
            color.contrast.overrideState = true;
            color.contrast.value = 16f;
            color.colorFilter.overrideState = true;
            color.colorFilter.value = new Color(0.82f, 0.88f, 1f);
            color.saturation.overrideState = true;
            color.saturation.value = -4f;

            var bloom = profile.Add<Bloom>(true);
            bloom.threshold.overrideState = true;
            bloom.threshold.value = 0.95f;
            bloom.intensity.overrideState = true;
            bloom.intensity.value = 0.32f;
            bloom.scatter.overrideState = true;
            bloom.scatter.value = 0.55f;

            var vignette = profile.Add<Vignette>(true);
            vignette.color.overrideState = true;
            vignette.color.value = new Color(0.01f, 0.015f, 0.03f);
            vignette.intensity.overrideState = true;
            vignette.intensity.value = 0.38f;
            vignette.smoothness.overrideState = true;
            vignette.smoothness.value = 0.7f;

            // Tilt-shift DoF retained (C53 keeps the miniature framing). Focus distance matches
            // ConfigureCamera's board-to-camera offset (14 world units). Aperture 3.5 (was 2.8)
            // softens the miniature blur a notch so the first wet-dusk look can still answer the
            // hero-shot-vs-readable-gameplay question without the board going mushy.
            var dof = profile.Add<DepthOfField>(true);
            dof.mode.overrideState = true;
            dof.mode.value = DepthOfFieldMode.Bokeh;
            dof.focusDistance.overrideState = true;
            dof.focusDistance.value = 14f;
            dof.aperture.overrideState = true;
            dof.aperture.value = 3.5f;
            dof.focalLength.overrideState = true;
            dof.focalLength.value = 135f;
            dof.bladeCount.overrideState = true;
            dof.bladeCount.value = 6;

            volume.sharedProfile = profile;
        }
    }
}
