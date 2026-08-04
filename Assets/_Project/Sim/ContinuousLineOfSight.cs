using System;

namespace LogiCard.Sim
{
    /// <summary>
    /// Continuous replacement for <see cref="GridLineOfSight"/> (C35/C39). Pure segment-vs-obstacle
    /// math against <see cref="ArenaBoard"/> — never a physics raycast (C32). Same-floor only.
    /// </summary>
    public static class ContinuousLineOfSight
    {
        public static bool HasLineOfSight(ArenaBoard board, PlanarPosition from, PlanarPosition to)
        {
            if (board == null || from.Floor != to.Floor)
            {
                return false;
            }

            if (!board.InBounds(from) || !board.InBounds(to))
            {
                return false;
            }

            return !board.IsBlocking(new Segment(from, to));
        }

        /// <summary>
        /// Hold Angle cover lane: true if <paramref name="point"/> is within
        /// <paramref name="halfWidth"/> of the origin→aim segment and strictly after the origin
        /// (excluded — mirrors the grid lane excluding the shooter's own tile) through the aim
        /// point (included). Points beyond either end are off-lane even if near the infinite line.
        /// </summary>
        public static bool IsOnLane(PlanarPosition origin, PlanarPosition aim, PlanarPosition point, float halfWidth)
        {
            if (origin.Floor != point.Floor)
            {
                return false;
            }

            var lane = new Segment(origin, aim);
            float t = lane.ProjectParam(point);
            if (t <= 0f || t > 1f)
            {
                return false;
            }

            return lane.DistanceToPoint(point) <= halfWidth;
        }
    }
}
