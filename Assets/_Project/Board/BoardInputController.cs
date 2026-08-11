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
        /// Lock In's entry point. Tries to commit any pending draft first. If that draft does not
        /// fit the budget but already-committed actions do, the draft is dropped and Lock In
        /// proceeds with the committed queue (playtest 2026-08-07: Used looked fine after UNDO of
        /// queue rows while an over-budget pending path silently kept failing Lock In at the same
        /// total). Returns false only when even the committed actions alone cannot lock.
        /// </summary>
        public bool CommitToPlayback()
        {
            if (Program != null && Program.HasDraft && !TryCommitDraftPath())
            {
                if (Program.UsedSeconds > Program.BudgetSeconds + 0.001f)
                {
                    return false;
                }

                Debug.Log(
                    "[logiCard] Dropping over-budget pending draft so Lock In can proceed with " +
                    $"committed actions ({Program.UsedSeconds:0.0}s / {Program.BudgetSeconds:0.0}s).");
                Program.ClearDraft();
                RefreshPreview();
                QueueChanged?.Invoke(Program);
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

            TryClickAtScreenPosition(Input.mousePosition);
        }

        /// <summary>
        /// Full click pipeline (UI absorb → ground ray / plane → <see cref="TryTapPoint"/>) at an
        /// explicit screen position. <see cref="Update"/> uses the live pointer; PlayMode tests drive
        /// south-edge / HUD-overlap cases without synthesizing <c>Input.mousePosition</c>.
        /// </summary>
        public bool TryClickAtScreenPosition(Vector3 screenPosition)
        {
            if (_locked || _phase.Phase != RoundPhase.Program)
            {
                return false;
            }

            if (IsScreenPositionAbsorbedByUi(screenPosition, out string uiName))
            {
                // Debug-only, not player-facing (this fires on every legitimate HUD dock click too,
                // which would make it noisy as a rejection toast) — but logged so a repeated "my board
                // click did nothing" report can be checked against the console: if this fires for a
                // click the player intended for the board, the HUD dock's raycast area is overlapping
                // board screen-space, not a raycast/pathfinding bug.
                Debug.Log(
                    $"[logiCard] Click at {screenPosition} absorbed by UI '{uiName}', never reached the board.");
                return false;
            }

            return TryHandleClickAtScreenPosition(screenPosition);
        }

        private static bool IsScreenPositionAbsorbedByUi(Vector3 screenPosition, out string uiName)
        {
            uiName = "(none)";
            if (EventSystem.current == null)
            {
                return false;
            }

            var pointer = new PointerEventData(EventSystem.current) { position = screenPosition };
            var hits = new List<RaycastResult>();
            EventSystem.current.RaycastAll(pointer, hits);
            if (hits.Count == 0)
            {
                return false;
            }

            uiName = hits[0].gameObject != null ? hits[0].gameObject.name : "(unknown)";
            return true;
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

            // Keep selection so the prompt stays up and can show the new scheduled status
            // (playtest 2026-08-07: clearing here forced a re-tap that still read live CLOSED).
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
            RefreshDoorSchedulePreview();
        }

        /// <summary>
        /// Tint doors from <see cref="PawnProgram.ScheduledDoorState"/> — visual only. The shared
        /// <see cref="ArenaBoard"/> stays at round-start until Aftermath so pathfinding cannot
        /// open-and-walk in the same draft once Open is booked (pathfinding uses a scheduled-state
        /// clone; this tint mirrors that). Shared <see cref="ArenaBoard"/> stays at round-start until
        /// Aftermath so Lock In still arms correctly (playtest 2026-08-06/07).
        /// </summary>
        private void RefreshDoorSchedulePreview()
        {
            if (_boardView == null || _board == null || Program == null)
            {
                return;
            }

            var states = new Dictionary<Door, DoorState>();
            IReadOnlyList<Door> doors = _board.Doors;
            for (int i = 0; i < doors.Count; i++)
            {
                Door door = doors[i];
                states[door] = Program.ScheduledDoorState(door);
            }

            _boardView.RefreshDoorVisuals(states);
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

        private bool TryHandleClickAtScreenPosition(Vector3 screenPosition)
        {
            Camera cam = Camera.main;
            if (cam == null || _boardView == null)
            {
                return false;
            }

            Ray ray = cam.ScreenPointToRay(screenPosition);
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                // Only accept hits on this board's ground (or any collider under the BoardView root).
                if (!hit.collider.transform.IsChildOf(_boardView.transform)
                    && hit.collider.transform != _boardView.transform)
                {
                    Debug.Log(
                        $"[logiCard] Click raycast hit '{hit.collider.gameObject.name}', which isn't part of this board.");
                    ActionRejected?.Invoke("click landed off the board");
                    return false;
                }

                return TryTapPoint(_boardView.PlanarFromWorld(hit.point));
            }

            // Physics miss: still resolve against the authored ground plane. Closer ortho fill
            // (3.4) pushes the south lip near/past the camera viewport bottom — rays that skim the
            // underlay's vertical face or graze void just past MinY used to fail silently-then-toast
            // even when the mathematical plane hit is still a legal in-bounds Move target.
            if (TryPlanarFromGroundPlane(ray, out PlanarPosition planar))
            {
                if (_board != null && !_board.InBounds(planar))
                {
                    Debug.Log($"[logiCard] Click ground-plane hit {planar} is outside the board.");
                    ActionRejected?.Invoke("click didn't land on the board");
                    return false;
                }

                return TryTapPoint(planar);
            }

            // Was totally silent before (2026-08-10) — a click that doesn't land on any collider
            // at all (e.g. past the board edge, or a real click-to-world bug) previously gave the
            // player zero feedback, indistinguishable from "nothing happened." Surface it so a
            // repeatedly-missed click is at least visible/diagnosable, not a mystery.
            Debug.Log("[logiCard] Click raycast hit nothing.");
            ActionRejected?.Invoke("click didn't land on the board");
            return false;
        }

        /// <summary>
        /// Intersect <paramref name="ray"/> with the board's ground plane (local Y = 0) and convert
        /// to planar coordinates. Used when the ground collider miss but the click still aims at the
        /// diorama floor — keeps south-edge taps working under fill zoom / pitched ortho.
        /// </summary>
        private bool TryPlanarFromGroundPlane(Ray ray, out PlanarPosition planar)
        {
            planar = default;
            if (_boardView == null)
            {
                return false;
            }

            // Ground plane through the board origin, normal = board up (handles any board tilt later).
            var plane = new Plane(_boardView.transform.up, _boardView.WorldFromPlanar(new PlanarPosition(0f, 0f)));
            if (!plane.Raycast(ray, out float enter) || enter < 0f)
            {
                return false;
            }

            planar = _boardView.PlanarFromWorld(ray.GetPoint(enter));
            return true;
        }
    }
}
