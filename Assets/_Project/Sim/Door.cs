namespace LogiCard.Sim
{
    public enum DoorState
    {
        Open,
        Closed,
    }

    /// <summary>
    /// One map door (GDD §4): a single tile whose passability the resolver toggles over time.
    /// Registered on a <see cref="GridBoard"/>; the door occupies its own tile rather than a true
    /// edge between two tiles (Day 7 research note §C/H1) — reuses the existing
    /// <see cref="Tile.IsPassable"/> mechanism both <see cref="GridLineOfSight"/> and
    /// <see cref="OrthogonalPathfinder"/> already honor, so no LoS/pathfinder changes are needed.
    /// </summary>
    public sealed class Door
    {
        public GridCoordinate Coordinate { get; }

        public DoorState InitialState { get; }

        public Door(GridCoordinate coordinate, DoorState initialState)
        {
            Coordinate = coordinate;
            InitialState = initialState;
        }
    }
}
