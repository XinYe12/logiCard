using LogiCard.Audio;
using LogiCard.Board;
using LogiCard.Net;
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
        private const float DefenderSecondsPerTile = 2f;

        public MatchClock MatchClock => _matchClock;

        public RoundPlayback Playback => _playback;

        public BoardInputController AttackerInput => _attackerInput;

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

            BuildBoard();
            BuildPawns();
            ConfigureCamera();
            BuildLighting();

            var hud = new GameObject("ProgramHud").AddComponent<ProgramHud>();
            hud.transform.SetParent(transform, false);
            hud.Init(_clock, _phase, _attackerInput, _matchClock, showPhaseDebugControls, _foley);

            hud.LockedIn += _playback.ResolveAndArm;
            hud.TimeCardPlayed += OnTimeCardPlayed;
            hud.NextRoundRequested += OnNextRoundRequested;
            _playback.OutcomeReported += hud.ShowOutcome;

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

        private void BuildBoard()
        {
            var boardGo = new GameObject("Board");
            boardGo.transform.SetParent(transform, false);

            // Multi-room layout (C45, 2026-08-08 — supersedes the earlier single-room [0,4]x[0,4] board,
            // C39 item 7): Yard (open, attacker spawn) -> Hall (walled kill-box, Door #1 frontal / Door #2
            // rear) -> Vault (open), with unguarded flank corridors on either side of Hall (x<2 / x>6).
            // Hall's side walls are solid, so a defender holed up inside has zero LoS into either flank —
            // flanking is safe by construction, not AI restraint. Gives the Scout/Juggernaut Sprint-speed
            // asymmetry (1s vs 2s per tile) an actual tactical lever: short-but-guarded center vs.
            // longer-but-safe flank. Both doors start Closed (Phase 6 / CONTINUOUS_PIVOT_PLAN.md); the
            // scripted defender opens Door #1 before its Snap so AmbushPoint LoS works. Door #2 is a
            // player-discoverable depth objective the scripted defender never touches.
            var model = new ArenaBoard(0f, 0f, 8f, 10f, new[] { Floor.Ground });

            model.RegisterWall(new Segment(new PlanarPosition(2f, 4f), new PlanarPosition(3.75f, 4f)));
            model.RegisterWall(new Segment(new PlanarPosition(4.25f, 4f), new PlanarPosition(6f, 4f)));
            model.RegisterDoor(new Door(
                new Segment(new PlanarPosition(3.75f, 4f), new PlanarPosition(4.25f, 4f)),
                DoorState.Closed,
                displayName: "Door #1"));

            model.RegisterWall(new Segment(new PlanarPosition(2f, 4f), new PlanarPosition(2f, 7f)));
            model.RegisterWall(new Segment(new PlanarPosition(6f, 4f), new PlanarPosition(6f, 7f)));

            model.RegisterWall(new Segment(new PlanarPosition(2f, 7f), new PlanarPosition(3.75f, 7f)));
            model.RegisterWall(new Segment(new PlanarPosition(4.25f, 7f), new PlanarPosition(6f, 7f)));
            model.RegisterDoor(new Door(
                new Segment(new PlanarPosition(3.75f, 7f), new PlanarPosition(4.25f, 7f)),
                DoorState.Closed,
                displayName: "Door #2"));

            _board = boardGo.AddComponent<BoardView>();
            _board.Build(model, new Color(0.72f, 0.55f, 0.38f), new Color(0.42f, 0.38f, 0.34f));
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

            // Landscape desktop (C48): board region is full height minus the top strip, full width
            // minus the right-edge HUD dock — see ProgramHud's HudDock*/TopStripHeight constants and
            // docs/contracts/CURRENT.md's frozen note on this coupling.
            cam.rect = new Rect(0f, 0f, 1f - ProgramHud.HudDockWidth, 1f - ProgramHud.TopStripHeight);
            cam.orthographic = true;
            // Was 3.6f for the old 4x4 board; scaled proportionally to the new [0,8]x[0,10] board's
            // largest extent (4 -> 10 units, C45). First-pass estimate, not derived from the tilt/rect
            // math — needs an Editor eyeball check to confirm the whole Yard/Hall/Vault footprint frames
            // without the HUD's thumb-zone/top-strip cropping it; tune ±20% if not.
            cam.orthographicSize = 9.0f;
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(0.07f, 0.07f, 0.09f);

            var rotation = Quaternion.Euler(52f, 0f, 0f);
            cam.transform.rotation = rotation;
            cam.transform.position = _board.CenterWorld - (rotation * Vector3.forward * 14f);

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

            // Desk-lamp key: warm, angled, casts the soft contact shadows a flat single light can't.
            var keyGo = new GameObject("Desk Lamp Key");
            keyGo.transform.SetParent(transform, false);
            var key = keyGo.AddComponent<Light>();
            key.type = LightType.Directional;
            key.color = new Color(1f, 0.92f, 0.78f);
            key.intensity = 1.35f;
            key.shadows = LightShadows.Soft;
            key.shadowStrength = 0.75f;
            key.transform.rotation = Quaternion.Euler(55f, -35f, 0f);

            // Cool, dim fill so the key's shadows read as shape, not pure black (ART_DIRECTION §6:
            // "warm key, soft fill"). No shadows of its own — a second shadow direction would just
            // muddy the read.
            var fillGo = new GameObject("Soft Fill");
            fillGo.transform.SetParent(transform, false);
            var fill = fillGo.AddComponent<Light>();
            fill.type = LightType.Directional;
            fill.color = new Color(0.72f, 0.80f, 0.95f);
            fill.intensity = 0.35f;
            fill.shadows = LightShadows.None;
            fill.transform.rotation = Quaternion.Euler(35f, 150f, 0f);

            RenderSettings.ambientMode = AmbientMode.Flat;
            RenderSettings.ambientLight = new Color(0.16f, 0.14f, 0.13f);

            BuildDioramaVolume();
        }

        /// <summary>
        /// Global post-process grade for the "desk-lamp diorama" read: warm/saturated painted-
        /// miniature color, a restrained bloom (glints, not glow-game lasers — ART_DIRECTION §3 bans
        /// that), and a vignette that sells "lit stage in a dark room" around the void (§1 Environment).
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
            color.postExposure.value = 0.15f;
            color.contrast.overrideState = true;
            color.contrast.value = 8f;
            color.colorFilter.overrideState = true;
            color.colorFilter.value = new Color(1f, 0.96f, 0.9f);
            color.saturation.overrideState = true;
            color.saturation.value = 14f;

            var bloom = profile.Add<Bloom>(true);
            bloom.threshold.overrideState = true;
            bloom.threshold.value = 1.1f;
            bloom.intensity.overrideState = true;
            bloom.intensity.value = 0.25f;
            bloom.scatter.overrideState = true;
            bloom.scatter.value = 0.6f;

            var vignette = profile.Add<Vignette>(true);
            vignette.color.overrideState = true;
            vignette.color.value = new Color(0.02f, 0.02f, 0.02f);
            vignette.intensity.overrideState = true;
            vignette.intensity.value = 0.32f;
            vignette.smoothness.overrideState = true;
            vignette.smoothness.value = 0.65f;

            // Tilt-shift DoF: the moodboard's core visual identity ("blur near and far so pawns read
            // ~2-inch miniatures") had no implementation at all until now — Bokeh mode blurs both sides
            // of the focus plane (Gaussian mode only does background blur), which is what sells the
            // toy-scale illusion. Focus distance matches ConfigureCamera's board-to-camera offset (14
            // world units) so the board stays sharp and only its near/far edges soften.
            var dof = profile.Add<DepthOfField>(true);
            dof.mode.overrideState = true;
            dof.mode.value = DepthOfFieldMode.Bokeh;
            dof.focusDistance.overrideState = true;
            dof.focusDistance.value = 14f;
            dof.aperture.overrideState = true;
            dof.aperture.value = 2.8f;
            dof.focalLength.overrideState = true;
            dof.focalLength.value = 135f;
            dof.bladeCount.overrideState = true;
            dof.bladeCount.value = 6;

            volume.sharedProfile = profile;
        }
    }
}
