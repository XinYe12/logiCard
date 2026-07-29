namespace LogiCard.Sim
{
    /// <summary>
    /// Time Resource costs in seconds (C27) — planning math only, never Playback Duration.
    /// </summary>
    public static class TimeResourceMath
    {
        public static float SegmentSeconds(float distanceTiles, float baseSecondsPerTile, StanceType stance)
        {
            return distanceTiles * baseSecondsPerTile * StanceMath.MultiplierFor(stance);
        }

        /// <summary>
        /// Movement is orthogonal on the grid, so tile distance is Manhattan distance.
        /// </summary>
        public static float SegmentSeconds(GridCoordinate from, GridCoordinate to, float baseSecondsPerTile, StanceType stance)
        {
            return SegmentSeconds(from.ManhattanDistanceTo(to), baseSecondsPerTile, stance);
        }
    }
}
