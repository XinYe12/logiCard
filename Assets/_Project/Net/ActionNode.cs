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

        /// <summary>Nullable — always null for Day 3, reserved for card interrupts.</summary>
        public CardData Modifier { get; }

        public ActionNode(ActionVerb verb, float executeTime, GridCoordinate gridPosition, StanceType stance, CardData modifier = null)
        {
            Verb = verb;
            ExecuteTime = executeTime;
            GridPosition = gridPosition;
            Stance = stance;
            Modifier = modifier;
        }
    }
}
