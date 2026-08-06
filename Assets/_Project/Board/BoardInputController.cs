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
        private Door _pendingDoor;
        private PathPreviewView _pathPreview;

        public ActionVerb Mode { get; set; } = ActionVerb.Move;

        /// <summary>Read-only access for the HUD to describe actual door state (not just the player's selected action).</summary>
        public ArenaBoard Board => _board;

        /// <summary>Lets the HUD project board-space points (e.g. the pending door) to screen space.</summary>
        public BoardView BoardView => _boardView;

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

        /// <summary>
        /// The door a board tap in Door mode most recently selected, awaiting an explicit
        /// OPEN/CLOSE confirm (BUG FOUND 2026-08-06, playtest: tapping the board used to queue an
        /// Open/Close immediately against a HUD-preselected action, silently flipped to its
        /// opposite whenever it matched the door's live state — the HUD label could show "OPEN"
        /// while the tap actually booked a Close). Null once confirmed, rejected, or cancelled.
        /// </summary>
        public Door PendingDoor => _pendingDoor;

        public event Action<PawnProgram> QueueChanged;

        /// <summary>
        /// Fires with a human-readable reason whenever a board tap is rejected outright (e.g. "No
        /// route to that point" when a closed door is the only way across) — previously only
        /// reached <c>Debug.Log</c>, so a rejected tap looked exactly like nothing happening at all
        /// (playtest feedback 2026-08-06: "why isn't anything happening" when a path was blocked by
        /// the closed door). The HUD surfaces this in the outcome banner.
        /// </summary>
        public event Action<string> ActionRejected;

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

        /// <summary>
        /// Lock In's entry point. Returns false — and leaves the round unlocked — if a pending
        /// draft exists but fails to commit (e.g. over budget), instead of silently discarding that
        /// draft and locking in anyway with whatever was already committed (BUG FOUND 2026-08-06,
        /// playtest: a drafted path that didn't fit the round's Time Resource budget vanished
        /// silently at Lock In, leaving the pawn with zero Move nodes — it just stood still through
        /// the whole playback with no on-screen explanation).
        /// </summary>
        public bool CommitToPlayback()
        {
            if (Program != null && Program.HasDraft && !TryCommitDraftPath())
            {
                return false;
            }

            _locked = true;
            _pathPreview?.Clear();
            return true;
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
                ActionRejected?.Invoke(reason);
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
            _pendingDoor = null;
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
                queued = TrySelectOrCancelDoor(point, out reason);
            }
            else
            {
                reason = $"Unsupported verb {Mode}.";
                queued = false;
            }

            if (!queued)
            {
                Debug.Log($"[logiCard] {Mode} rejected at {point}: {reason}");
                ActionRejected?.Invoke(reason);
                return false;
            }

            RefreshPreview();
            QueueChanged?.Invoke(Program);
            return true;
        }

        /// <summary>
        /// Door mode's first tap: selects the nearest door as <see cref="PendingDoor"/> without
        /// booking anything yet — <see cref="TryConfirmPendingDoor"/> (the HUD's OPEN/CLOSE
        /// buttons) does the actual queue. A tap that doesn't land near any door cancels whatever
        /// was pending, so tapping elsewhere is how the player backs out.
        /// </summary>
        private bool TrySelectOrCancelDoor(PlanarPosition point, out string reason)
        {
            if (_board != null && _board.TryGetNearestDoor(point, DoorPickRadius, out Door door))
            {
                _pendingDoor = door;
                reason = null;
                return true;
            }

            bool hadPending = _pendingDoor != null;
            _pendingDoor = null;
            reason = hadPending ? null : "No door near tap.";
            return hadPending;
        }

        /// <summary>
        /// The HUD's OPEN/CLOSE buttons call this with the player's explicit choice against
        /// whatever door <see cref="TrySelectOrCancelDoor"/> most recently selected — the only path
        /// that actually books a Door node, so there is no silent action-flipping.
        /// </summary>
        public bool TryConfirmPendingDoor(DoorAction action, out string reason)
        {
            if (_locked || Program == null)
            {
                reason = "Round is locked.";
                ActionRejected?.Invoke(reason);
                return false;
            }

            if (_pendingDoor == null)
            {
                reason = "No door selected — tap near a door first.";
                ActionRejected?.Invoke(reason);
                return false;
            }

            if (!Program.TryQueueDoor(_pendingDoor, action, out reason))
            {
                ActionRejected?.Invoke(reason);
                return false;
            }

            _pendingDoor = null;
            RefreshPreview();
            QueueChanged?.Invoke(Program);
            return true;
        }

        public void CancelPendingDoor()
        {
            _pendingDoor = null;
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
