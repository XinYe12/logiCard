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

        public Door(Segment segment, DoorState initialState)
        {
            Segment = segment;
            InitialState = initialState;
        }
    }
}
