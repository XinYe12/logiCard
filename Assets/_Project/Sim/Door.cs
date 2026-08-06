namespace LogiCard.Sim
{
    public enum DoorState
    {
        Open,
        Closed,
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

        public Door(Segment segment, DoorState initialState, string displayName = null)
        {
            Segment = segment;
            InitialState = initialState;
            DisplayName = displayName;
        }
    }
}
