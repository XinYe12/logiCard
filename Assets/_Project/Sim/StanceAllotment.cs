using System;

namespace LogiCard.Sim
{
    /// <summary>
    /// Path length × base speed → stance band (C21). The Program UI allots Time Resource seconds
    /// for a draft path; this maps that allotment onto Sprint / Walk / Crawl breakpoints.
    /// </summary>
    public static class StanceAllotment
    {
        public static float CostForTiles(float distanceTiles, float baseSecondsPerTile, StanceType stance)
        {
            return TimeResourceMath.SegmentSeconds(distanceTiles, baseSecondsPerTile, stance);
        }

        public static float MinSeconds(float distanceTiles, float baseSecondsPerTile)
        {
            return CostForTiles(distanceTiles, baseSecondsPerTile, StanceType.Sprint);
        }

        public static float MaxSeconds(float distanceTiles, float baseSecondsPerTile)
        {
            return CostForTiles(distanceTiles, baseSecondsPerTile, StanceType.Crawl);
        }

        /// <summary>
        /// Picks the stance whose exact tile cost is closest to <paramref name="allottedSeconds"/>.
        /// Ties break toward the slower band so a midpoint allotment never silently sprints.
        /// </summary>
        public static StanceType FromAllottedSeconds(float distanceTiles, float baseSecondsPerTile, float allottedSeconds)
        {
            if (distanceTiles <= 0f)
            {
                return StanceType.Walk;
            }

            float sprint = CostForTiles(distanceTiles, baseSecondsPerTile, StanceType.Sprint);
            float walk = CostForTiles(distanceTiles, baseSecondsPerTile, StanceType.Walk);
            float crawl = CostForTiles(distanceTiles, baseSecondsPerTile, StanceType.Crawl);

            float dSprint = Math.Abs(allottedSeconds - sprint);
            float dWalk = Math.Abs(allottedSeconds - walk);
            float dCrawl = Math.Abs(allottedSeconds - crawl);

            if (dCrawl <= dWalk && dCrawl <= dSprint)
            {
                return StanceType.Crawl;
            }

            if (dWalk <= dSprint)
            {
                return StanceType.Walk;
            }

            return StanceType.Sprint;
        }

        /// <summary>0 = Sprint cost, 1 = Crawl cost for the given path length.</summary>
        public static float Normalize(float distanceTiles, float baseSecondsPerTile, float allottedSeconds)
        {
            float min = MinSeconds(distanceTiles, baseSecondsPerTile);
            float max = MaxSeconds(distanceTiles, baseSecondsPerTile);
            if (max <= min)
            {
                return 0f;
            }

            float t = (allottedSeconds - min) / (max - min);
            if (t < 0f)
            {
                return 0f;
            }

            if (t > 1f)
            {
                return 1f;
            }

            return t;
        }

        public static float LerpAllotment(float distanceTiles, float baseSecondsPerTile, float normalized)
        {
            float min = MinSeconds(distanceTiles, baseSecondsPerTile);
            float max = MaxSeconds(distanceTiles, baseSecondsPerTile);
            float t = normalized < 0f ? 0f : (normalized > 1f ? 1f : normalized);
            return min + ((max - min) * t);
        }
    }
}
