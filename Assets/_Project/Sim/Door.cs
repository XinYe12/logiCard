namespace LogiCard.Sim
{
    public enum DoorState
    {
        Open,
        Closed,
    }

    /// <summary>
    /// Presentation/interaction flavor of a <see cref="Door"/> — all three still resolve through the
    /// exact same Sim/resolve pipeline (<see cref="ArenaBoard"/> registration, radius interact,
    /// <c>GhostResolver</c>'s door-transition sweep, cross-round state persistence). Only what the UI
    /// offers and what <c>BoardView</c> renders differ per kind.
    /// </summary>
    public enum DoorKind
    {
        /// <summary>A standard room-boundary door — both Open and Close are always legal.</summary>
        Standard,

        /// <summary>
        /// A narrow interactive shortcut (e.g. a vent grate) placed off a room-boundary line. Both
        /// Open and Close are legal, same as Standard — the only difference is presentation
        /// (<c>BoardView</c> renders a narrower grate mesh instead of a door leaf).
        /// </summary>
        Vent,

        /// <summary>
        /// A one-way permanent shortcut through a wall. Starts Closed; once Opened, the UI never
        /// offers Close again (enforced at the prompt layer, not the resolver — the resolver treats it
        /// as an ordinary door and would honor a Close if one were ever queued). Reads as "pay Time
        /// Resource once to permanently open a new route."
        /// </summary>
        Breach,
    }

    /// <summary>
    /// One map door (GDD §4): a continuous-space segment whose passability the resolver toggles over
    /// time (C35/C39 pivot). Registered on an <see cref="ArenaBoard"/>; interaction is radius-based
    /// (Decision 4) rather than tile-adjacency, since a continuous click will essentially never land
    /// exactly on the door's geometry.
    /// </summary>
    public sealed class Door
    {
        public Segment Segment { get; }

        public DoorState InitialState { get; }

        /// <summary>
        /// Player-facing identity for interaction UI — e.g. "Door #1" (the "Identity" leg of the
        /// content contract in docs/UI_BOARD_ANCHORED_COMPONENTS.md: every interaction prompt must
        /// be able to say *what* it's acting on, not just show a generic verb). Optional so the many
        /// existing single-door call sites (tests, in particular) don't need updating; falls back to
        /// a generic label at the UI layer when null.
        /// </summary>
        public string DisplayName { get; }

        /// <summary>Presentation/interaction flavor — see <see cref="DoorKind"/>. Defaults to
        /// <see cref="DoorKind.Standard"/> so every existing call site stays valid unchanged.</summary>
        public DoorKind Kind { get; }

        public Door(Segment segment, DoorState initialState, string displayName = null, DoorKind kind = DoorKind.Standard)
        {
            Segment = segment;
            InitialState = initialState;
            DisplayName = displayName;
            Kind = kind;
        }
    }
}
