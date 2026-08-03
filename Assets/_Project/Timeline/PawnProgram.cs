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
    /// </summary>
    public sealed class PawnProgram
    {
        private const float BudgetEpsilon = 0.001f;

        private readonly List<ActionNode> _nodes = new List<ActionNode>();
        private readonly List<GridCoordinate> _draftWaypoints = new List<GridCoordinate>();
        private readonly List<GridCoordinate> _pathBuffer = new List<GridCoordinate>();
        private readonly GridBoard _board;

        public GridCoordinate CurrentPosition { get; private set; }

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
        public IReadOnlyList<GridCoordinate> DraftWaypoints => _draftWaypoints;

        public int DraftTileCount => _draftWaypoints.Count;

        public bool HasDraft => _draftWaypoints.Count > 0;

        public float DraftAllottedSeconds { get; private set; }

        public PawnProgram(
            GridCoordinate start,
            float baseSecondsPerTile,
            float budgetSeconds,
            StanceType startingStance = StanceType.Walk,
            GridBoard board = null,
            float doorInteractBaseSeconds = 4f)
        {
            CurrentPosition = start;
            BaseSecondsPerTile = baseSecondsPerTile;
            BudgetSeconds = budgetSeconds;
            CurrentStance = startingStance;
            DraftStance = startingStance;
            CurrentShootMode = ShootMode.SnapShot;
            DoorInteractSeconds = doorInteractBaseSeconds;
            _board = board ?? new GridBoard(5, 5, new[] { Floor.Ground });
        }

        /// <summary>
        /// Replaces the draft with the shortest orthogonal route to <paramref name="destination"/>.
        /// Used by <see cref="TryQueueMove"/> for scripted/single-shot moves. Interactive per-tap
        /// authoring goes through <see cref="TryAddWaypoint"/> instead, which appends rather than
        /// replaces. Does not spend budget until <see cref="TryCommitDraft"/>.
        /// </summary>
        public bool TryDraftPath(GridCoordinate destination, out string rejectionReason)
        {
            if (destination == CurrentPosition)
            {
                rejectionReason = "Destination matches current position.";
                return false;
            }

            if (!OrthogonalPathfinder.TryFindPath(_board, CurrentPosition, destination, _pathBuffer))
            {
                rejectionReason = "No orthogonal path to destination.";
                return false;
            }

            _draftWaypoints.Clear();
            _draftWaypoints.AddRange(_pathBuffer);
            RecomputeDraftCost();
            rejectionReason = null;
            return true;
        }

        /// <summary>
        /// Adds one waypoint to the draft (C21, amended 2026-08-03): the shortest legal orthogonal
        /// route from the draft's current tip (or <see cref="CurrentPosition"/> if the draft is
        /// empty) to <paramref name="tile"/> is appended. The player controls the route's shape by
        /// choosing where waypoints land — the system fills in each leg, it never replaces the whole
        /// draft with its own single shortest path to one destination. Re-tapping the immediately
        /// previous waypoint undoes it.
        /// </summary>
        public bool TryAddWaypoint(GridCoordinate tile, out string rejectionReason)
        {
            GridCoordinate tip = HasDraft ? _draftWaypoints[_draftWaypoints.Count - 1] : CurrentPosition;

            // Backtrack one waypoint if the player re-taps the previous stop.
            if (HasDraft && tile == (DraftTileCount == 1 ? CurrentPosition : _draftWaypoints[DraftTileCount - 2]))
            {
                _draftWaypoints.RemoveAt(_draftWaypoints.Count - 1);
                RecomputeDraftCost();
                rejectionReason = null;
                return true;
            }

            if (tile == tip || _draftWaypoints.Contains(tile))
            {
                rejectionReason = "Path already visits that tile.";
                return false;
            }

            if (!OrthogonalPathfinder.TryFindPath(_board, tip, tile, _pathBuffer))
            {
                rejectionReason = "No orthogonal path to that tile.";
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
            DraftAllottedSeconds = HasDraft
                ? StanceAllotment.CostForTiles(DraftTileCount, BaseSecondsPerTile, DraftStance)
                : 0f;
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

            float cost = StanceAllotment.CostForTiles(DraftTileCount, BaseSecondsPerTile, DraftStance);
            if (!CanReserve(cost, out rejectionReason))
            {
                return false;
            }

            GridCoordinate stepFrom = CurrentPosition;
            float running = UsedSeconds;
            for (int i = 0; i < _draftWaypoints.Count; i++)
            {
                GridCoordinate stepTo = _draftWaypoints[i];
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
        public bool TryQueueMove(GridCoordinate destination, out string rejectionReason, StanceType? stance = null)
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

        public bool TryQueueShoot(GridCoordinate target, out string rejectionReason, ShootMode? mode = null)
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

            if (target == CurrentPosition)
            {
                rejectionReason = "Cannot target own tile.";
                return false;
            }

            if (target.Floor != CurrentPosition.Floor || (target.X != CurrentPosition.X && target.Y != CurrentPosition.Y))
            {
                rejectionReason = "Shoot target must be on the shooter's row or column.";
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
            _nodes.Add(new ActionNode(ActionVerb.Shoot, UsedSeconds, target, CurrentStance, modifier: null, shootMode));
            rejectionReason = null;
            return true;
        }

        /// <summary>
        /// Books an Open or Close on <paramref name="doorTile"/> — a base map action (GDD §4), not
        /// a gear card, legal from the pawn's current tile or an orthogonal neighbour of it.
        /// </summary>
        public bool TryQueueDoor(GridCoordinate doorTile, DoorAction action, out string rejectionReason)
        {
            if (HasDraft && !TryCommitDraft(out rejectionReason))
            {
                return false;
            }

            if (!_board.TryGetDoor(doorTile, out _))
            {
                rejectionReason = "Tile is not a registered door.";
                return false;
            }

            if (!IsCurrentOrAdjacent(doorTile))
            {
                rejectionReason = "Door must be on the pawn's current or adjacent tile.";
                return false;
            }

            if (!TryReserve(DoorInteractSeconds, out rejectionReason))
            {
                return false;
            }

            _nodes.Add(new ActionNode(ActionVerb.Door, UsedSeconds, doorTile, CurrentStance, doorAction: action));
            rejectionReason = null;
            return true;
        }

        private bool IsCurrentOrAdjacent(GridCoordinate tile)
        {
            if (tile == CurrentPosition)
            {
                return true;
            }

            foreach (GridCoordinate neighbour in CurrentPosition.GetOrthogonalNeighbours())
            {
                if (neighbour == tile)
                {
                    return true;
                }
            }

            return false;
        }

        public TimelinePayload Build()
        {
            return new TimelinePayload(_nodes);
        }

        /// <summary>
        /// Preview path for the board: committed Move waypoints plus the current draft, timed with
        /// each node's own stance (draft uses <see cref="DraftStance"/>).
        /// </summary>
        public ScheduledPath BuildMovePreviewPath(GridCoordinate origin)
        {
            var waypoints = new List<GridCoordinate> { origin };
            var arrivals = new List<float> { 0f };
            GridCoordinate at = origin;
            float t = 0f;

            foreach (ActionNode node in _nodes)
            {
                if (node.Verb != ActionVerb.Move)
                {
                    continue;
                }

                t = node.ExecuteTime;
                at = node.GridPosition;
                waypoints.Add(at);
                arrivals.Add(t);
            }

            for (int i = 0; i < _draftWaypoints.Count; i++)
            {
                GridCoordinate next = _draftWaypoints[i];
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
