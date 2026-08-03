namespace LogiCard.Sim
{
    /// <summary>
    /// Path length × base speed → stance cost (C21, amended 2026-08-03). The player picks a stance
    /// directly; this computes what that stance costs for a given path length. No longer derives a
    /// stance from a player-allotted second count — there is no allotment step.
    /// </summary>
    public static class StanceAllotment
    {
        public static float CostForTiles(float distanceTiles, float baseSecondsPerTile, StanceType stance)
        {
            return TimeResourceMath.SegmentSeconds(distanceTiles, baseSecondsPerTile, stance);
        }
    }
}
