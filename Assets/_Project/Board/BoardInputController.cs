using System;
using LogiCard.Net;
using LogiCard.Sim;
using LogiCard.Timeline;
using UnityEngine;
using UnityEngine.EventSystems;

namespace LogiCard.Board
{
    /// <summary>
    /// Click-to-schedule Program input for one pawn (Day 3): raycasts board tiles, queues
    /// Move/Shoot actions into a <see cref="PawnProgram"/>, and hands off the committed
    /// path to <see cref="PawnView"/> at Lock-In. Only active during the Program phase.
    /// </summary>
    public sealed class BoardInputController : MonoBehaviour
    {
        private PawnView _pawn;
        private RoundPhaseController _phase;
        private GridCoordinate _home;
        private float _baseSecondsPerTile;
        private float _budgetSeconds;
        private bool _locked;

        public ActionVerb Mode { get; set; } = ActionVerb.Move;

        public PawnProgram Program { get; private set; }

        public event Action<PawnProgram> QueueChanged;

        public void Init(PawnView pawn, RoundPhaseController phase, GridCoordinate home, float baseSecondsPerTile, float budgetSeconds)
        {
            _pawn = pawn;
            _phase = phase;
            _home = home;
            _baseSecondsPerTile = baseSecondsPerTile;
            _budgetSeconds = budgetSeconds;

            _phase.PhaseChanged += OnPhaseChanged;
            ResetProgram();
        }

        private void OnDestroy()
        {
            if (_phase != null)
            {
                _phase.PhaseChanged -= OnPhaseChanged;
            }
        }

        /// <summary>Rebuilds the pawn's real timed path from the queued Move nodes and stops further input.</summary>
        public void CommitToPlayback()
        {
            _locked = true;
            _pawn.SetPath(Program.BuildMovePreviewPath(_home));
        }

        private void OnPhaseChanged(RoundPhase phase)
        {
            if (phase == RoundPhase.Program)
            {
                ResetProgram();
            }
        }

        private void ResetProgram()
        {
            Program = new PawnProgram(_home, _baseSecondsPerTile, _budgetSeconds);
            _locked = false;
            _pawn.SetPath(ScheduledPath.FromWaypoints(new[] { _home }, _baseSecondsPerTile, StanceType.Walk));
            _pawn.ApplyTime(0f);
            QueueChanged?.Invoke(Program);
        }

        private void Update()
        {
            if (_locked || _phase.Phase != RoundPhase.Program)
            {
                return;
            }

            if (!Input.GetMouseButtonDown(0))
            {
                return;
            }

            if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
            {
                return;
            }

            TryHandleClick();
        }

        private void TryHandleClick()
        {
            Camera cam = Camera.main;
            if (cam == null || !Physics.Raycast(cam.ScreenPointToRay(Input.mousePosition), out RaycastHit hit))
            {
                return;
            }

            var marker = hit.collider.GetComponent<TileMarker>();
            if (marker == null)
            {
                return;
            }

            string reason;
            bool queued = Mode == ActionVerb.Move
                ? Program.TryQueueMove(marker.Coordinate, out reason)
                : Program.TryQueueShoot(marker.Coordinate, out reason);

            if (!queued)
            {
                Debug.Log($"[logiCard] {Mode} rejected at {marker.Coordinate}: {reason}");
                return;
            }

            if (Mode == ActionVerb.Move)
            {
                _pawn.SetPath(Program.BuildMovePreviewPath(_home));
                _pawn.ApplyTime(Program.UsedSeconds);
            }

            QueueChanged?.Invoke(Program);
        }
    }
}
