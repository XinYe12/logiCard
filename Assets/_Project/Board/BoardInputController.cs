using System;
using System.Collections.Generic;
using LogiCard.Net;
using LogiCard.Sim;
using LogiCard.Timeline;
using UnityEngine;
using UnityEngine.EventSystems;

namespace LogiCard.Board
{
    /// <summary>
    /// Program input for one pawn: raycasts the continuous ground plane, drafts multi-waypoint
    /// paths at a directly picked stance, and commits Move/Shoot/Door into a
    /// <see cref="PawnProgram"/> (C21 + C35/C39 Phase 4).
    /// </summary>
    public sealed class BoardInputController : MonoBehaviour
    {
        private const float DoorPickRadius = 0.55f;

        private PawnView _pawn;
        private RoundPhaseController _phase;
        private PlanarPosition _origin;
        private float _baseSecondsPerTile;
        private float _budgetSeconds;
        private ArenaBoard _board;
        private BoardView _boardView;
        private bool _locked;
        private StanceType _preferredStance = StanceType.Walk;
        private ShootMode _preferredShootMode = ShootMode.SnapShot;
        private DoorAction _preferredDoorAction = DoorAction.Open;
        private PathPreviewView _pathPreview;

        public ActionVerb Mode { get; set; } = ActionVerb.Move;

        /// <summary>Read-only access for the HUD to describe actual door state (not just the player's selected action).</summary>
        public ArenaBoard Board => _board;

        public PawnProgram Program { get; private set; }

        public PlanarPosition Origin => _origin;

        public StanceType PreferredStance
        {
            get => _preferredStance;
            set
            {
                _preferredStance = value;
                if (Program != null)
                {
                    Program.SetPreferredStance(value);
                    RefreshPreview();
                    QueueChanged?.Invoke(Program);
                }
            }
        }

        public ShootMode PreferredShootMode
        {
            get => _preferredShootMode;
            set
            {
                _preferredShootMode = value == ShootMode.None ? ShootMode.SnapShot : value;
                if (Program != null)
                {
                    Program.SetShootMode(_preferredShootMode);
                    QueueChanged?.Invoke(Program);
                }
            }
        }

        public DoorAction PreferredDoorAction
        {
            get => _preferredDoorAction;
            set => _preferredDoorAction = value;
        }

        public event Action<PawnProgram> QueueChanged;

        public void Init(
            PawnView pawn,
            RoundPhaseController phase,
            PlanarPosition origin,
            float baseSecondsPerTile,
            float budgetSeconds,
            BoardView boardView = null)
        {
            _pawn = pawn;
            _phase = phase;
            _origin = origin;
            _baseSecondsPerTile = baseSecondsPerTile;
            _budgetSeconds = budgetSeconds;
            _boardView = boardView;
            _board = boardView != null ? boardView.Model : null;

            if (_pathPreview == null && boardView != null)
            {
                // Parented to the board, not this pawn's own transform (BUG FOUND 2026-08-05):
                // beads are drawn at absolute board positions via WorldFromPlanar, but this
                // component lives on the pawn's GameObject. Parenting under the pawn made every
                // bead a rigid child of it, so scrubbing the Time Resource slider during Program
                // (which moves the pawn along its live draft/preview path, see RoundPlayback.ApplyTime)
                // dragged the whole bead constellation along with the pawn instead of leaving them
                // fixed on the board.
                var previewGo = new GameObject("PathPreview");
                previewGo.transform.SetParent(boardView.transform, false);
                _pathPreview = previewGo.AddComponent<PathPreviewView>();
                _pathPreview.Init(boardView);
            }

            _phase.PhaseChanged += OnPhaseChanged;
            ResetProgram();
        }

        /// <summary>Updates the carried start point and round budget before Program rebuilds (C33).</summary>
        public void PrepareRound(PlanarPosition origin, float budgetSeconds)
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

        public void CommitToPlayback()
        {
            TryCommitDraftPath();
            _locked = true;
            _pathPreview?.Clear();
        }

        public bool TryCommitDraftPath()
        {
            if (_locked || Program == null || !Program.HasDraft)
            {
                return false;
            }

            if (!Program.TryCommitDraft(out string reason))
            {
                Debug.Log($"[logiCard] Commit draft rejected: {reason}");
                return false;
            }

            RefreshPreview();
            QueueChanged?.Invoke(Program);
            return true;
        }

        /// <summary>
        /// Undoes the whole program back one step at a time: draft waypoints/legs first if any are
        /// in progress, then previously committed Move/Shoot/Door actions (BUG FOUND 2026-08-05 —
        /// this used to require an active draft, so it dead-ended at the first Shoot/Door or any
        /// already-committed Move; <see cref="PawnProgram.TryUndoLastStep"/> now covers both).
        /// </summary>
        public bool TryUndoLastStep()
        {
            if (_locked || Program == null || !Program.CanUndoLastStep)
            {
                return false;
            }

            if (!Program.TryUndoLastStep(out string reason))
            {
                Debug.Log($"[logiCard] Undo step rejected: {reason}");
                return false;
            }

            RefreshPreview();
            QueueChanged?.Invoke(Program);
            return true;
        }

        public bool TrySetDraftStance(StanceType stance)
        {
            if (_locked || Program == null)
            {
                return false;
            }

            _preferredStance = stance;
            if (!Program.HasDraft)
            {
                Program.SetPreferredStance(stance);
                QueueChanged?.Invoke(Program);
                return true;
            }

            if (!Program.TrySetDraftStance(stance, out string reason))
            {
                Debug.Log($"[logiCard] Set draft stance rejected: {reason}");
                return false;
            }

            RefreshPreview();
            QueueChanged?.Invoke(Program);
            return true;
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
            Program = new PawnProgram(_origin, _baseSecondsPerTile, _budgetSeconds, _preferredStance, _board);
            Program.SetShootMode(_preferredShootMode);
            _locked = false;
            RefreshPreview();
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
        /// Queues the current <see cref="Mode"/> at an already-resolved continuous point.
        /// Split from the raycast so PlayMode tests can drive scheduling without a real pointer.
        /// </summary>
        public bool TryTapPoint(PlanarPosition point)
        {
            if (_locked || _phase.Phase != RoundPhase.Program)
            {
                return false;
            }

            if (Mode == ActionVerb.Shoot || Mode == ActionVerb.Door)
            {
                TryCommitDraftPath();
            }

            string reason;
            bool queued;
            if (Mode == ActionVerb.Move)
            {
                queued = Program.TryAddWaypoint(point, out reason);
            }
            else if (Mode == ActionVerb.Shoot)
            {
                queued = Program.TryQueueShoot(point, out reason, _preferredShootMode);
            }
            else if (Mode == ActionVerb.Door)
            {
                queued = TryQueueNearestDoor(point, out reason);
            }
            else
            {
                reason = $"Unsupported verb {Mode}.";
                queued = false;
            }

            if (!queued)
            {
                Debug.Log($"[logiCard] {Mode} rejected at {point}: {reason}");
                return false;
            }

            RefreshPreview();
            QueueChanged?.Invoke(Program);
            return true;
        }

        private bool TryQueueNearestDoor(PlanarPosition point, out string reason)
        {
            if (_board == null || !_board.TryGetNearestDoor(point, DoorPickRadius, out Door door))
            {
                reason = "No door near tap.";
                return false;
            }

            DoorAction action = _preferredDoorAction;
            DoorState state = _board.GetDoorState(door);
            // If the preferred action is already the live state, toggle so a second tap still does work.
            if ((action == DoorAction.Open && state == DoorState.Open)
                || (action == DoorAction.Close && state == DoorState.Closed))
            {
                action = state == DoorState.Open ? DoorAction.Close : DoorAction.Open;
            }

            return Program.TryQueueDoor(door, action, out reason);
        }

        private void RefreshPreview()
        {
            if (_pawn == null || Program == null)
            {
                return;
            }

            _pawn.SetPath(Program.BuildMovePreviewPath(_origin));
            float previewTime = Program.UsedSeconds;
            if (Program.HasDraft)
            {
                previewTime += Program.DraftAllottedSeconds;
            }

            _pawn.ApplyTime(previewTime);
            RefreshPathBeads();
        }

        private void RefreshPathBeads()
        {
            if (_pathPreview == null || Program == null)
            {
                return;
            }

            // BUG FOUND 2026-08-05: this used to seed the list with _origin, so every draft always
            // rendered a spurious extra bead at/near the pawn's own starting point — the pawn already
            // marks that, and the stray bead was easy to mistake for a wrongly-placed destination
            // marker (most visible when a route behind a wall collapses to a single real waypoint).
            var points = new List<PlanarPosition>();
            foreach (ActionNode node in Program.Nodes)
            {
                if (node.Verb == ActionVerb.Move)
                {
                    points.Add(node.Position);
                }
            }

            if (Program.HasDraft)
            {
                for (int i = 0; i < Program.DraftWaypoints.Count; i++)
                {
                    points.Add(Program.DraftWaypoints[i]);
                }

                _pathPreview.Show(points, isDraft: true);
                return;
            }

            if (points.Count > 0)
            {
                _pathPreview.Show(points, isDraft: false);
                return;
            }

            _pathPreview.Clear();
        }

        private void TryHandleClick()
        {
            Camera cam = Camera.main;
            if (cam == null || _boardView == null)
            {
                return;
            }

            if (!Physics.Raycast(cam.ScreenPointToRay(Input.mousePosition), out RaycastHit hit))
            {
                return;
            }

            // Only accept hits on this board's ground (or any collider under the BoardView root).
            if (!hit.collider.transform.IsChildOf(_boardView.transform)
                && hit.collider.transform != _boardView.transform)
            {
                return;
            }

            TryTapPoint(_boardView.PlanarFromWorld(hit.point));
        }
    }
}
