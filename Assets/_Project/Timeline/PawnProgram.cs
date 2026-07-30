using System.Collections.Generic;
using LogiCard.Net;
using LogiCard.Sim;

namespace LogiCard.Timeline
{
    /// <summary>
    /// Running Program-phase state for one pawn: queued Move/Shoot actions accumulate against
    /// the Time Resource budget and compile into a <see cref="TimelinePayload"/> (TDD D6 §2/§3).
    /// Pure C# so it is unit-testable without a scene.
    /// </summary>
    public sealed class PawnProgram
    {
        private const float BudgetEpsilon = 0.001f;

        private readonly List<ActionNode> _nodes = new List<ActionNode>();

        public GridCoordinate CurrentPosition { get; private set; }

        public StanceType CurrentStance { get; }

        public float UsedSeconds { get; private set; }

        public float BudgetSeconds { get; }

        public float BaseSecondsPerTile { get; }

        public IReadOnlyList<ActionNode> Nodes => _nodes;

        public PawnProgram(GridCoordinate start, float baseSecondsPerTile, float budgetSeconds, StanceType startingStance = StanceType.Walk)
        {
            CurrentPosition = start;
            BaseSecondsPerTile = baseSecondsPerTile;
            BudgetSeconds = budgetSeconds;
            CurrentStance = startingStance;
        }

        public bool TryQueueMove(GridCoordinate destination, out string rejectionReason)
        {
            if (destination == CurrentPosition)
            {
                rejectionReason = "Destination matches current position.";
                return false;
            }

            float cost = TimeResourceMath.SegmentSeconds(CurrentPosition, destination, BaseSecondsPerTile, CurrentStance);
            if (!TryReserve(cost, out rejectionReason))
            {
                return false;
            }

            _nodes.Add(new ActionNode(ActionVerb.Move, UsedSeconds, destination, CurrentStance));
            CurrentPosition = destination;
            rejectionReason = null;
            return true;
        }

        public bool TryQueueShoot(GridCoordinate target, out string rejectionReason)
        {
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

            if (!TryReserve(ShootCost.SnapShotSeconds, out rejectionReason))
            {
                return false;
            }

            _nodes.Add(new ActionNode(ActionVerb.Shoot, UsedSeconds, target, CurrentStance));
            rejectionReason = null;
            return true;
        }

        public TimelinePayload Build()
        {
            return new TimelinePayload(_nodes);
        }

        /// <summary>Move-only waypoints for the on-screen preview; ignores time consumed by interleaved Shoot nodes.</summary>
        public ScheduledPath BuildMovePreviewPath(GridCoordinate origin)
        {
            var waypoints = new List<GridCoordinate> { origin };
            foreach (ActionNode node in _nodes)
            {
                if (node.Verb == ActionVerb.Move)
                {
                    waypoints.Add(node.GridPosition);
                }
            }

            return ScheduledPath.FromWaypoints(waypoints, BaseSecondsPerTile, CurrentStance);
        }

        private bool TryReserve(float cost, out string rejectionReason)
        {
            if (UsedSeconds + cost > BudgetSeconds + BudgetEpsilon)
            {
                rejectionReason = $"Would exceed Time Resource budget ({UsedSeconds + cost:0.0}s of {BudgetSeconds:0.0}s).";
                return false;
            }

            UsedSeconds += cost;
            rejectionReason = null;
            return true;
        }
    }
}
