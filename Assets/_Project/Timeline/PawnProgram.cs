using System.Collections.Generic;
using LogiCard.Net;
using LogiCard.Sim;

namespace LogiCard.Timeline
{
    /// <summary>
    /// Running Program-phase state for one pawn: draft a multi-waypoint path (each tap appends a
    /// waypoint, connected via the shortest leg between it and the previous one) and a directly
    /// picked stance, then commit Move/Shoot/Door against the round budget into a
    /// <see cref="TimelinePayload"/> (TDD D6 §2/§3). Cost is automatic — there is no time-allotment
    /// step (C21, amended 2026-08-03). Pure C# so it is unit-testable without a scene.
    ///
    /// Retargeted onto continuous space (C35/C39 pivot, Phase 3): <see cref="PlanarPosition"/> and
    /// <see cref="ArenaBoard"/> replace <c>GridCoordinate</c>/<c>GridBoard</c>;
    /// <see cref="ContinuousPathfinder"/> replaces <c>OrthogonalPathfinder</c>. The old "reject a tap
    /// that revisits or crosses a prior waypoint" guard is gone outright — nothing replaces it,
    /// revisiting/crossing a prior point is legal now. Shoot targeting is free-aim (Decision 1): the
    /// row/column constraint is gone. Door interaction is radius-based (Decision 4): the caller
    /// resolves a <see cref="Door"/> reference itself (e.g. via <see cref="ArenaBoard.TryGetNearestDoor"/>)
    /// before calling <see cref="TryQueueDoor"/>.
    /// </summary>
    public sealed class PawnProgram
    {
        private const float BudgetEpsilon = 0.001f;

        /// <summary>
        /// Door interaction reach (Decision 4) — start ~half a pawn-width per C39, tune in Phase 6
        /// against real play (PRODUCT_MEMORY.md open numeric #5), same scale as GhostResolver's HitRadius.
        /// </summary>
        private const float InteractRadius = 0.45f;

        private readonly List<ActionNode> _nodes = new List<ActionNode>();
        private readonly List<PlanarPosition> _draftWaypoints = new List<PlanarPosition>();
        private readonly List<PlanarPosition> _pathBuffer = new List<PlanarPosition>();
        private readonly ArenaBoard _board;

        public PlanarPosition CurrentPosition { get; private set; }

        /// <summary>Stance of the last committed action (or the starting stance before any).</summary>
        public StanceType CurrentStance { get; private set; }

        /// <summary>Stance directly picked for the current draft (C21).</summary>
        public StanceType DraftStance { get; private set; }

        /// <summary>Snap or Hold used for the next queued Shoot (C25).</summary>
        public ShootMode CurrentShootMode { get; private set; }

        public float UsedSeconds { get; private set; }

        public float BudgetSeconds { get; }

        public float BaseSecondsPerTile { get; }

        /// <summary>Base Time Resource cost to Open or Close the map door (GDD §4/§6, Strength).</summary>
        public float DoorInteractSeconds { get; }

        public IReadOnlyList<ActionNode> Nodes => _nodes;

        /// <summary>Draft waypoints after <see cref="CurrentPosition"/> (destination last).</summary>
        public IReadOnlyList<PlanarPosition> DraftWaypoints => _draftWaypoints;

        public int DraftWaypointCount => _draftWaypoints.Count;

        public bool HasDraft => _draftWaypoints.Count > 0;

        public float DraftAllottedSeconds { get; private set; }

        /// <summary>
        /// Sum of Euclidean leg lengths in the current draft (C35/C39 pivot) — replaces the old
        /// "one cost-unit per waypoint" count, which was only correct because each grid waypoint
        /// happened to be exactly one tile.
        /// </summary>
        public float DraftDistance { get; private set; }

        public PawnProgram(
            PlanarPosition start,
            float baseSecondsPerTile,
            float budgetSeconds,
            StanceType startingStance = StanceType.Walk,
            ArenaBoard board = null,
            float doorInteractBaseSeconds = 4f)
        {
            CurrentPosition = start;
            BaseSecondsPerTile = baseSecondsPerTile;
            BudgetSeconds = budgetSeconds;
            CurrentStance = startingStance;
            DraftStance = startingStance;
            CurrentShootMode = ShootMode.SnapShot;
            DoorInteractSeconds = doorInteractBaseSeconds;
            _board = board ?? new ArenaBoard(floors: new[] { Floor.Ground });
        }

        /// <summary>
        /// Replaces the draft with the shortest route to <paramref name="destination"/>. Used by
        /// <see cref="TryQueueMove"/> for scripted/single-shot moves. Interactive per-tap authoring
        /// goes through <see cref="TryAddWaypoint"/> instead, which appends rather than replaces.
        /// Does not spend budget until <see cref="TryCommitDraft"/>.
        /// </summary>
        public bool TryDraftPath(PlanarPosition destination, out string rejectionReason)
        {
            if (destination.Equals(CurrentPosition))
            {
                rejectionReason = "Destination matches current position.";
                return false;
            }

            if (!ContinuousPathfinder.TryFindPath(_board, CurrentPosition, destination, _pathBuffer))
            {
                rejectionReason = "No route to destination.";
                return false;
            }

            _draftWaypoints.Clear();
            _draftWaypoints.AddRange(_pathBuffer);
            RecomputeDraftCost();
            rejectionReason = null;
            return true;
        }

        /// <summary>
        /// Adds one waypoint to the draft (C21, amended 2026-08-03): the shortest legal route from
        /// the draft's current tip (or <see cref="CurrentPosition"/> if the draft is empty) to
        /// <paramref name="point"/> is appended. The player controls the route's shape by choosing
        /// where waypoints land — the system fills in each leg, it never replaces the whole draft
        /// with its own single shortest path to one destination. Re-tapping the immediately previous
        /// waypoint undoes it; revisiting or crossing any other prior point is legal (C35/C39 pivot —
        /// the old reject-on-revisit guard is gone, nothing replaces it).
        /// </summary>
        public bool TryAddWaypoint(PlanarPosition point, out string rejectionReason)
        {
            PlanarPosition tip = HasDraft ? _draftWaypoints[_draftWaypoints.Count - 1] : CurrentPosition;

            // Backtrack one waypoint if the player re-taps the previous stop.
            if (HasDraft && point.Equals(DraftWaypointCount == 1 ? CurrentPosition : _draftWaypoints[DraftWaypointCount - 2]))
            {
                _draftWaypoints.RemoveAt(_draftWaypoints.Count - 1);
                RecomputeDraftCost();
                rejectionReason = null;
                return true;
            }

            if (!ContinuousPathfinder.TryFindPath(_board, tip, point, _pathBuffer))
            {
                rejectionReason = "No route to that point.";
                return false;
            }

            _draftWaypoints.AddRange(_pathBuffer);
            RecomputeDraftCost();
            rejectionReason = null;
            return true;
        }

        public void ClearDraft()
        {
            _draftWaypoints.Clear();
            DraftAllottedSeconds = 0f;
            DraftDistance = 0f;
        }

        /// <summary>Sets the draft's stance directly — no time-allotment step (C21).</summary>
        public bool TrySetDraftStance(StanceType stance, out string rejectionReason)
        {
            if (!HasDraft)
            {
                rejectionReason = "No draft path to stance.";
                return false;
            }

            DraftStance = stance;
            RecomputeDraftCost();
            rejectionReason = null;
            return true;
        }

        /// <summary>Preferred stance used when a fresh draft is created.</summary>
        public void SetPreferredStance(StanceType stance)
        {
            DraftStance = stance;
            RecomputeDraftCost();
        }

        private void RecomputeDraftCost()
        {
            DraftDistance = ComputeDraftDistance();
            DraftAllottedSeconds = HasDraft
                ? StanceAllotment.CostForTiles(DraftDistance, BaseSecondsPerTile, DraftStance)
                : 0f;
        }

        private float ComputeDraftDistance()
        {
            if (!HasDraft)
            {
                return 0f;
            }

            float distance = 0f;
            PlanarPosition from = CurrentPosition;
            for (int i = 0; i < _draftWaypoints.Count; i++)
            {
                distance += from.DistanceTo(_draftWaypoints[i]);
                from = _draftWaypoints[i];
            }

            return distance;
        }

        public void SetShootMode(ShootMode mode)
        {
            if (mode == ShootMode.None)
            {
                mode = ShootMode.SnapShot;
            }

            CurrentShootMode = mode;
        }

        public bool TryCommitDraft(out string rejectionReason)
        {
            if (!HasDraft)
            {
                rejectionReason = "No draft path to commit.";
                return false;
            }

            if (!CanReserve(DraftAllottedSeconds, out rejectionReason))
            {
                return false;
            }

            PlanarPosition stepFrom = CurrentPosition;
            float running = UsedSeconds;
            for (int i = 0; i < _draftWaypoints.Count; i++)
            {
                PlanarPosition stepTo = _draftWaypoints[i];
                running += TimeResourceMath.SegmentSeconds(stepFrom, stepTo, BaseSecondsPerTile, DraftStance);
                _nodes.Add(new ActionNode(ActionVerb.Move, running, stepTo, DraftStance));
                stepFrom = stepTo;
            }

            UsedSeconds = running;
            CurrentPosition = stepFrom;
            CurrentStance = DraftStance;
            ClearDraft();
            rejectionReason = null;
            return true;
        }

        /// <summary>
        /// Convenience for scripted opponents and tests: draft the shortest path and commit at
        /// <see cref="CurrentStance"/> (or <paramref name="stance"/> when supplied).
        /// </summary>
        public bool TryQueueMove(PlanarPosition destination, out string rejectionReason, StanceType? stance = null)
        {
            StanceType previousPreferred = DraftStance;
            if (stance.HasValue)
            {
                DraftStance = stance.Value;
            }
            else
            {
                DraftStance = CurrentStance;
            }

            if (!TryDraftPath(destination, out rejectionReason))
            {
                DraftStance = previousPreferred;
                return false;
            }

            bool committed = TryCommitDraft(out rejectionReason);
            if (!committed)
            {
                ClearDraft();
                DraftStance = previousPreferred;
            }

            return committed;
        }

        /// <summary>Free-aim Shoot (Decision 1) — <paramref name="aimPoint"/> may be any in-bounds point, no row/column constraint.</summary>
        public bool TryQueueShoot(PlanarPosition aimPoint, out string rejectionReason, ShootMode? mode = null)
        {
            if (HasDraft && !TryCommitDraft(out rejectionReason))
            {
                return false;
            }

            if (CurrentStance == StanceType.Sprint)
            {
                rejectionReason = "Cannot fire while sprinting.";
                return false;
            }

            if (!_board.InBounds(aimPoint))
            {
                rejectionReason = "Aim point must be in bounds.";
                return false;
            }

            ShootMode shootMode = mode ?? CurrentShootMode;
            if (shootMode == ShootMode.None)
            {
                shootMode = ShootMode.SnapShot;
            }

            float cost = ShootCost.SecondsFor(shootMode);
            if (!TryReserve(cost, out rejectionReason))
            {
                return false;
            }

            CurrentShootMode = shootMode;
            _nodes.Add(new ActionNode(ActionVerb.Shoot, UsedSeconds, aimPoint, CurrentStance, modifier: null, shootMode));
            rejectionReason = null;
            return true;
        }

        /// <summary>
        /// Books an Open or Close on <paramref name="door"/> — a base map action (GDD §4), not a gear
        /// card. Legal within <see cref="InteractRadius"/> of the pawn's current position (Decision 4);
        /// the caller resolves which door via <see cref="ArenaBoard.TryGetNearestDoor"/> before calling
        /// this, since a continuous click essentially never lands exactly on a door's geometry.
        /// </summary>
        public bool TryQueueDoor(Door door, DoorAction action, out string rejectionReason)
        {
            if (HasDraft && !TryCommitDraft(out rejectionReason))
            {
                return false;
            }

            if (door == null)
            {
                rejectionReason = "No door to interact with.";
                return false;
            }

            if (door.Segment.DistanceToPoint(CurrentPosition) > InteractRadius)
            {
                rejectionReason = "Door is out of interaction range.";
                return false;
            }

            if (!TryReserve(DoorInteractSeconds, out rejectionReason))
            {
                return false;
            }

            PlanarPosition doorPosition = PlanarPosition.Lerp(door.Segment.A, door.Segment.B, 0.5f);
            _nodes.Add(new ActionNode(ActionVerb.Door, UsedSeconds, doorPosition, CurrentStance, doorAction: action));
            rejectionReason = null;
            return true;
        }

        public TimelinePayload Build()
        {
            return new TimelinePayload(_nodes);
        }

        /// <summary>
        /// Preview path for the board: committed Move waypoints plus the current draft, timed with
        /// each node's own stance (draft uses <see cref="DraftStance"/>).
        /// </summary>
        public ScheduledPath BuildMovePreviewPath(PlanarPosition origin)
        {
            var waypoints = new List<PlanarPosition> { origin };
            var arrivals = new List<float> { 0f };
            PlanarPosition at = origin;
            float t = 0f;

            foreach (ActionNode node in _nodes)
            {
                if (node.Verb != ActionVerb.Move)
                {
                    continue;
                }

                t = node.ExecuteTime;
                at = node.Position;
                waypoints.Add(at);
                arrivals.Add(t);
            }

            for (int i = 0; i < _draftWaypoints.Count; i++)
            {
                PlanarPosition next = _draftWaypoints[i];
                t += TimeResourceMath.SegmentSeconds(at, next, BaseSecondsPerTile, DraftStance);
                at = next;
                waypoints.Add(at);
                arrivals.Add(t);
            }

            return ScheduledPath.FromTimedWaypoints(waypoints, arrivals);
        }

        private bool CanReserve(float cost, out string rejectionReason)
        {
            if (UsedSeconds + cost > BudgetSeconds + BudgetEpsilon)
            {
                rejectionReason = $"Would exceed Time Resource budget ({UsedSeconds + cost:0.0}s of {BudgetSeconds:0.0}s).";
                return false;
            }

            rejectionReason = null;
            return true;
        }

        private bool TryReserve(float cost, out string rejectionReason)
        {
            if (!CanReserve(cost, out rejectionReason))
            {
                return false;
            }

            UsedSeconds += cost;
            rejectionReason = null;
            return true;
        }
    }
}
