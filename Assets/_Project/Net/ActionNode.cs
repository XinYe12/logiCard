using LogiCard.Cards;
using LogiCard.Sim;

namespace LogiCard.Net
{
    /// <summary>
    /// One row of a <see cref="TimelinePayload"/> (TDD D6 §2). GridPosition reuses the sim's
    /// GridCoordinate (carries Floor) rather than the doc's illustrative Vector2Int.
    /// </summary>
    public readonly struct ActionNode
    {
        public ActionVerb Verb { get; }

        public float ExecuteTime { get; }

        public GridCoordinate GridPosition { get; }

        public StanceType Stance { get; }

        /// <summary>Snap / Hold for Shoot nodes; <see cref="ShootMode.None"/> on Moves.</summary>
        public ShootMode ShootMode { get; }

        /// <summary>Nullable — always null for Day 3, reserved for card interrupts.</summary>
        public CardData Modifier { get; }

        public ActionNode(
            ActionVerb verb,
            float executeTime,
            GridCoordinate gridPosition,
            StanceType stance,
            CardData modifier = null,
            ShootMode shootMode = ShootMode.None)
        {
            Verb = verb;
            ExecuteTime = executeTime;
            GridPosition = gridPosition;
            Stance = stance;
            Modifier = modifier;
            // Legacy Shoot constructors omit the mode; treat that as Snap (Day 4 default).
            ShootMode = verb == ActionVerb.Shoot && shootMode == ShootMode.None
                ? ShootMode.SnapShot
                : shootMode;
        }
    }
}
