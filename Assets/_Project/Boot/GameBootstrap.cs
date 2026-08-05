using LogiCard.Board;
using LogiCard.Sim;
using LogiCard.Timeline;
using LogiCard.UI;
using UnityEngine;

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

            BuildBoard();
            BuildPawns();
            ConfigureCamera();
            BuildLighting();

            var hud = new GameObject("ProgramHud").AddComponent<ProgramHud>();
            hud.transform.SetParent(transform, false);
            hud.Init(_clock, _phase, _attackerInput, _matchClock, showPhaseDebugControls);

            hud.LockedIn += _playback.ResolveAndArm;
            hud.TimeCardPlayed += OnTimeCardPlayed;
            hud.NextRoundRequested += OnNextRoundRequested;
            _playback.OutcomeReported += hud.ShowOutcome;

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

            // Continuous translation of DAY7 wall-with-gap: walls along y=2 with a door gap at x≈2.
            // Door starts Open so scripted/demo LoS through the choke is readable without a forced
            // open action. Closed-start is still the Phase 6 design preference (CONTINUOUS_PIVOT_PLAN.md)
            // but flipping it ripples into the RoundPlaybackPlayModeTests "AmbushPoint" scenario (the
            // scripted defender's Snap Shot needs LoS through the door to wound the attacker) and a
            // few HUD test destinations that sit exactly on the door — tried it 2026-08-05, reverted;
            // needs deliberate test-fixture updates alongside a cold-observer playtest call, not a
            // blind flag flip.
            var model = new ArenaBoard(0f, 0f, 4f, 4f, new[] { Floor.Ground });
            model.RegisterWall(new Segment(new PlanarPosition(0f, 2f), new PlanarPosition(1.75f, 2f)));
            model.RegisterWall(new Segment(new PlanarPosition(2.25f, 2f), new PlanarPosition(4f, 2f)));
            model.RegisterDoor(new Door(
                new Segment(new PlanarPosition(1.75f, 2f), new PlanarPosition(2.25f, 2f)),
                DoorState.Open));

            _board = boardGo.AddComponent<BoardView>();
            _board.Build(model, new Color(0.82f, 0.78f, 0.70f), new Color(0.42f, 0.38f, 0.34f));
        }

        private void BuildPawns()
        {
            // Column-aligned with the door choke (DAY7 research layout).
            var attackerHome = new PlanarPosition(2f, 0f);
            const float attackerSecondsPerTile = 1f;
            var defenderSpawn = new PlanarPosition(2f, 4f);

            PawnView attacker = SpawnPawn("Pawn_Attacker", new Color(0.90f, 0.35f, 0.28f), attackerHome, attackerSecondsPerTile);
            PawnView defender = SpawnPawn("Pawn_Defender", new Color(0.32f, 0.58f, 0.86f), defenderSpawn, DefenderSecondsPerTile);

            _attackerInput = attacker.gameObject.AddComponent<BoardInputController>();
            _attackerInput.Init(attacker, _phase, attackerHome, attackerSecondsPerTile, 0f, _board);

            _playback = gameObject.AddComponent<RoundPlayback>();
            _playback.Init(_board, _clock, _phase);
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

            // Approach the door from the north, Snap south down the door column, then edge closer.
            TryScriptMove(program, new PlanarPosition(2f, 2.6f));
            TryScriptShoot(program, new PlanarPosition(2f, 1f));
            TryScriptMove(program, new PlanarPosition(2f, 2.3f));

            return program.Build();
        }

        private static void TryScriptMove(PawnProgram program, PlanarPosition destination)
        {
            if (!program.TryQueueMove(destination, out _))
            {
                // Over-budget or unreachable — fine for a stub AI.
            }
        }

        private static void TryScriptShoot(PawnProgram program, PlanarPosition aim)
        {
            if (!program.TryQueueShoot(aim, out _))
            {
                // Over-budget or illegal aim — fine for a stub AI.
            }
        }

        private PawnView SpawnPawn(string name, Color color, PlanarPosition home, float baseSecondsPerTile)
        {
            var go = new GameObject(name);
            go.transform.SetParent(transform, false);
            var pawn = go.AddComponent<PawnView>();
            pawn.Init(_board, color, ScheduledPath.FromWaypoints(new[] { home }, baseSecondsPerTile, StanceType.Walk));
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

            cam.rect = new Rect(0f, ProgramHud.ThumbZoneHeight, 1f, 1f - ProgramHud.ThumbZoneHeight - ProgramHud.TopStripHeight);
            cam.orthographic = true;
            cam.orthographicSize = 3.6f;
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(0.07f, 0.07f, 0.09f);

            var rotation = Quaternion.Euler(52f, 0f, 0f);
            cam.transform.rotation = rotation;
            cam.transform.position = _board.CenterWorld - (rotation * Vector3.forward * 14f);
        }

        private void BuildLighting()
        {
            if (FindFirstObjectByType<Light>() != null)
            {
                return;
            }

            var lightGo = new GameObject("Desk Lamp");
            lightGo.transform.SetParent(transform, false);
            var light = lightGo.AddComponent<Light>();
            light.type = LightType.Directional;
            light.color = new Color(1f, 0.95f, 0.85f);
            light.intensity = 1.1f;
            light.transform.rotation = Quaternion.Euler(50f, -35f, 0f);
        }
    }
}
