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
        /// Continuous-space movement (C35/C39 pivot): distance is Euclidean, not Manhattan. Draft cost
        /// becomes "sum of leg lengths" rather than "one cost-unit per waypoint" as a direct consequence.
        /// </summary>
        public static float SegmentSeconds(PlanarPosition from, PlanarPosition to, float baseSecondsPerTile, StanceType stance)
        {
            return SegmentSeconds(from.DistanceTo(to), baseSecondsPerTile, stance);
        }
    }
}
