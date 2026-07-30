using System.Collections.Generic;
using LogiCard.Board;
using LogiCard.Sim;
using LogiCard.Timeline;
using LogiCard.UI;
using UnityEngine;

namespace LogiCard.Boot
{
    /// <summary>
    /// Vertical slice scaffold: builds the 5x5 board, the continuous Time Resource clock,
    /// the portrait HUD, and two pawns — the attacker is player-programmed via
    /// BoardInputController (Day 3), the defender stays on a hardcoded schedule until
    /// Day 11 networking supplies a second local input source.
    ///
    /// Everything is constructed at runtime on purpose — Slice 1-2 changes shape daily and
    /// code is easier to diff than scene YAML.
    /// </summary>
    [DefaultExecutionOrder(-100)]
    public sealed class GameBootstrap : MonoBehaviour
    {
        [Header("Time Resource (C20 placeholders)")]
        public float timeResourceBudgetSeconds = 60f;

        [Tooltip("Playback Duration: real-world seconds to play the full budget (C27).")]
        public float playbackSecondsForFullBudget = 8f;

        [Header("Board")]
        public int boardWidth = 5;
        public int boardHeight = 5;

        private BoardView _board;
        private TimeResourceClockDriver _clock;
        private RoundPhaseController _phase;
        private BoardInputController _attackerInput;
        private readonly List<PawnView> _pawns = new List<PawnView>();

        private void Awake()
        {
            _clock = gameObject.AddComponent<TimeResourceClockDriver>();
            _clock.BudgetSeconds = timeResourceBudgetSeconds;
            _clock.PlaybackSecondsForFullBudget = playbackSecondsForFullBudget;

            _phase = gameObject.AddComponent<RoundPhaseController>();

            BuildBoard();
            BuildPawns();
            ConfigureCamera();
            BuildLighting();

            var hud = new GameObject("ProgramHud").AddComponent<ProgramHud>();
            hud.transform.SetParent(transform, false);
            hud.Init(_clock, _phase, _attackerInput);

            _clock.TimeChanged += ApplyTimeToPawns;
            ApplyTimeToPawns(0f);

            Debug.Log($"[logiCard] Day 2 slice up: {boardWidth}x{boardHeight} board, " +
                      $"{_pawns.Count} pawns, budget {timeResourceBudgetSeconds}s TR " +
                      $"playing in {playbackSecondsForFullBudget}s real-world.");
        }

        private void BuildBoard()
        {
            var boardGo = new GameObject("Board");
            boardGo.transform.SetParent(transform, false);

            // Ground floor only for Day 2; the attic floor is already supported by the sim board.
            var model = new GridBoard(boardWidth, boardHeight, new[] { Floor.Ground });

            _board = boardGo.AddComponent<BoardView>();
            _board.Build(model, new Color(0.82f, 0.78f, 0.70f), new Color(0.62f, 0.58f, 0.52f));
        }

        private void BuildPawns()
        {
            // Attacker (Scout, 1s/tile) is player-programmed from Day 3 onward: it starts
            // stationary at its home tile and is scheduled via BoardInputController clicks.
            var attackerHome = new GridCoordinate(0, 0);
            const float attackerSecondsPerTile = 1f;
            ScheduledPath attackerPath = ScheduledPath.FromWaypoints(
                new List<GridCoordinate> { attackerHome },
                baseSecondsPerTile: attackerSecondsPerTile,
                stance: StanceType.Walk);

            // Defender (Juggernaut, 2s per tile) stays hardcoded until Day 11 networking
            // gives a second local input source.
            ScheduledPath defenderPath = ScheduledPath.FromWaypoints(
                new List<GridCoordinate> { new GridCoordinate(4, 4), new GridCoordinate(4, 2), new GridCoordinate(2, 2) },
                baseSecondsPerTile: 2f,
                stance: StanceType.Walk);

            PawnView attacker = SpawnPawn("Pawn_Attacker", new Color(0.90f, 0.35f, 0.28f), attackerPath);
            _pawns.Add(attacker);
            _pawns.Add(SpawnPawn("Pawn_Defender", new Color(0.32f, 0.58f, 0.86f), defenderPath));

            _attackerInput = attacker.gameObject.AddComponent<BoardInputController>();
            _attackerInput.Init(attacker, _phase, attackerHome, attackerSecondsPerTile, timeResourceBudgetSeconds);

            Debug.Log($"[logiCard] Attacker home {attackerHome}, budget {timeResourceBudgetSeconds:0.0}s TR; " +
                      $"defender move ends at {defenderPath.EndSeconds:0.0}s TR.");
        }

        private PawnView SpawnPawn(string name, Color color, ScheduledPath path)
        {
            var go = new GameObject(name);
            go.transform.SetParent(transform, false);
            var pawn = go.AddComponent<PawnView>();
            pawn.Init(_board, color, path);
            return pawn;
        }

        private void ApplyTimeToPawns(float seconds)
        {
            for (int i = 0; i < _pawns.Count; i++)
            {
                _pawns[i].ApplyTime(seconds);
            }
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

            // Board renders only in the middle band; the HUD panels cover the rest (C30).
            cam.rect = new Rect(0f, ProgramHud.ThumbZoneHeight, 1f, 1f - ProgramHud.ThumbZoneHeight - ProgramHud.TopStripHeight);
            cam.orthographic = true;
            cam.orthographicSize = 3.6f;
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(0.07f, 0.07f, 0.09f);

            // Tilt-shift diorama angle (C29) without needing any art yet.
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
