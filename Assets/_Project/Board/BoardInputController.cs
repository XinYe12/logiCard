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
    /// Origin and budget are refreshed each round from carried state + Time Card (C33).
    /// </summary>
    public sealed class BoardInputController : MonoBehaviour
    {
        private PawnView _pawn;
        private RoundPhaseController _phase;
        private GridCoordinate _origin;
        private float _baseSecondsPerTile;
        private float _budgetSeconds;
        private bool _locked;

        public ActionVerb Mode { get; set; } = ActionVerb.Move;

        public PawnProgram Program { get; private set; }

        public GridCoordinate Origin => _origin;

        public event Action<PawnProgram> QueueChanged;

        public void Init(PawnView pawn, RoundPhaseController phase, GridCoordinate origin, float baseSecondsPerTile, float budgetSeconds)
        {
            _pawn = pawn;
            _phase = phase;
            _origin = origin;
            _baseSecondsPerTile = baseSecondsPerTile;
            _budgetSeconds = budgetSeconds;

            _phase.PhaseChanged += OnPhaseChanged;
            ResetProgram();
        }

        /// <summary>Updates the carried start tile and round budget before Program rebuilds (C33).</summary>
        public void PrepareRound(GridCoordinate origin, float budgetSeconds)
        {
            _origin = origin;
            _budgetSeconds = budgetSeconds < 0f ? 0f : budgetSeconds;
        }

        private void OnDestroy()
        {
            if (_phase != null)
            {
                _phase.PhaseChanged -= OnPhaseChanged;
            }
        }

        /// <summary>
        /// Stops further scheduling. The pawn's playback path now comes from the resolved ReplayTape
        /// rather than from this preview, so nothing is written to the view here.
        /// </summary>
        public void CommitToPlayback()
        {
            _locked = true;
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
            Program = new PawnProgram(_origin, _baseSecondsPerTile, _budgetSeconds);
            _locked = false;
            _pawn.SetPath(ScheduledPath.FromWaypoints(new[] { _origin }, _baseSecondsPerTile, StanceType.Walk));
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

        /// <summary>
        /// Queues the current <see cref="Mode"/> against an already-resolved tile and refreshes the
        /// preview. Split out from the raycast so the schedule path can be driven without a real
        /// pointer (mouse state cannot be synthesised through legacy <see cref="Input"/>).
        /// </summary>
        public bool TryTapTile(GridCoordinate coordinate)
        {
            if (_locked || _phase.Phase != RoundPhase.Program)
            {
                return false;
            }

            string reason;
            bool queued = Mode == ActionVerb.Move
                ? Program.TryQueueMove(coordinate, out reason)
                : Program.TryQueueShoot(coordinate, out reason);

            if (!queued)
            {
                Debug.Log($"[logiCard] {Mode} rejected at {coordinate}: {reason}");
                return false;
            }

            if (Mode == ActionVerb.Move)
            {
                _pawn.SetPath(Program.BuildMovePreviewPath(_origin));
                _pawn.ApplyTime(Program.UsedSeconds);
            }

            QueueChanged?.Invoke(Program);
            return true;
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

            TryTapTile(marker.Coordinate);
        }
    }
}
