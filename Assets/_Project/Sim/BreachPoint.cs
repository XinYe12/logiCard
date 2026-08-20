namespace LogiCard.Sim
{
    /// <summary>
    /// Discrete geometry-destruction states (C36 — long-term roadmap, promoted to active build scope
    /// 2026-08-20 as Bomber's wall-only v1, PRODUCT_MEMORY C71). Not physics-simulated fracture — a
    /// state machine, same shape as <see cref="DoorState"/>. <see cref="Damaged"/> is reserved for a
    /// future two-hit demolition mechanic; wall-only Bomber v1 (C71) transitions Intact straight to
    /// Breached on a single Detonate, exercising Intact/Breached only.
    /// </summary>
    public enum BreachState
    {
        Intact,
        Damaged,
        Breached,
    }

    /// <summary>
    /// One designed geometry-breach point (C36) on a wall segment — a bomb can only attach here, not
    /// to an arbitrary wall sample (C71: "designed breach points only, not freeform mesh sampling",
    /// matching how <see cref="Door"/> is a designed interaction point, not a freeform wall click).
    /// Registered on an <see cref="ArenaBoard"/>; same radius-interact model as <see cref="Door"/>
    /// (Decision 4) — see <see cref="ArenaBoard.TryGetNearestBreachPoint"/>.
    ///
    /// Deliberately its own type, not a reuse of <see cref="DoorKind.Breach"/> — that's an unrelated,
    /// already-shipped one-way permanent *door* shortcut (C57). Same English word, different concept;
    /// do not conflate them (see `docs/CHARACTER_BOMBER_AGENT_BRIEF.md` §2).
    /// </summary>
    public sealed class BreachPoint
    {
        public Segment Segment { get; }

        public BreachState InitialState { get; }

        /// <inheritdoc cref="Door.DisplayName"/>
        public string DisplayName { get; }

        public BreachPoint(Segment segment, BreachState initialState = BreachState.Intact, string displayName = null)
        {
            Segment = segment;
            InitialState = initialState;
            DisplayName = displayName;
        }
    }
}
