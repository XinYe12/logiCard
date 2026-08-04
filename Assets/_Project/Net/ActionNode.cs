using LogiCard.Cards;
using LogiCard.Sim;

namespace LogiCard.Net
{
    /// <summary>Open or Close for Door nodes (Day 7, GDD §4).</summary>
    public enum DoorAction
    {
        Open,
        Close,
    }

    /// <summary>
    /// One row of a <see cref="TimelinePayload"/> (TDD D6 §2). Position holds the sim's continuous
    /// <see cref="PlanarPosition"/> (carries Floor) rather than the doc's illustrative Vector2Int
    /// (C35/C39 pivot — was GridPosition/GridCoordinate). For a Door node, Position is the door's
    /// interaction point (same Position-reuse pattern Shoot already uses for its aim point).
    /// </summary>
    public readonly struct ActionNode
    {
        public ActionVerb Verb { get; }

        public float ExecuteTime { get; }

        public PlanarPosition Position { get; }

        public StanceType Stance { get; }

        /// <summary>Snap / Hold for Shoot nodes; <see cref="ShootMode.None"/> on Moves/Doors.</summary>
        public ShootMode ShootMode { get; }

        /// <summary>Meaningful only when <see cref="Verb"/> is <see cref="ActionVerb.Door"/>.</summary>
        public DoorAction Door { get; }

        /// <summary>Nullable — always null for Day 3, reserved for card interrupts.</summary>
        public CardData Modifier { get; }

        public ActionNode(
            ActionVerb verb,
            float executeTime,
            PlanarPosition position,
            StanceType stance,
            CardData modifier = null,
            ShootMode shootMode = ShootMode.None,
            DoorAction doorAction = DoorAction.Open)
        {
            Verb = verb;
            ExecuteTime = executeTime;
            Position = position;
            Stance = stance;
            Modifier = modifier;
            // Legacy Shoot constructors omit the mode; treat that as Snap (Day 4 default).
            ShootMode = verb == ActionVerb.Shoot && shootMode == ShootMode.None
                ? ShootMode.SnapShot
                : shootMode;
            Door = doorAction;
        }
    }
}
