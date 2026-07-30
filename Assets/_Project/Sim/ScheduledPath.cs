using System;
using System.Collections.Generic;

namespace LogiCard.Sim
{
    public readonly struct PlanarPosition
    {
        public float X { get; }

        public float Y { get; }

        public Floor Floor { get; }

        public PlanarPosition(float x, float y, Floor floor = Floor.Ground)
        {
            X = x;
            Y = y;
            Floor = floor;
        }

        /// <summary>
        /// Tile a pawn counts as occupying while mid-move, used when the resolver has to decide
        /// whether a shot fired at an instant finds a body standing there.
        /// </summary>
        public GridCoordinate ToNearestCoordinate()
        {
            return new GridCoordinate(
                (int)Math.Round(X, MidpointRounding.AwayFromZero),
                (int)Math.Round(Y, MidpointRounding.AwayFromZero),
                Floor);
        }
    }

    /// <summary>
    /// A Move booked against the Time Resource budget: waypoints plus the cumulative
    /// second at which the pawn reaches each one. Sampling this is what playback reads,
    /// so the same structure serves the Host ghost sim and the client tape (C23).
    /// </summary>
    public sealed class ScheduledPath
    {
        private readonly List<GridCoordinate> nodes = new List<GridCoordinate>();
        private readonly List<float> arrivalSeconds = new List<float>();

        public IReadOnlyList<GridCoordinate> Nodes => nodes;

        public IReadOnlyList<float> ArrivalSeconds => arrivalSeconds;

        public float StartSeconds => arrivalSeconds.Count == 0 ? 0f : arrivalSeconds[0];

        public float EndSeconds => arrivalSeconds.Count == 0 ? 0f : arrivalSeconds[arrivalSeconds.Count - 1];

        public static ScheduledPath FromWaypoints(
            IReadOnlyList<GridCoordinate> waypoints,
            float baseSecondsPerTile,
            StanceType stance,
            float startSeconds = 0f)
        {
            var path = new ScheduledPath();
            if (waypoints == null || waypoints.Count == 0)
            {
                return path;
            }

            path.nodes.Add(waypoints[0]);
            path.arrivalSeconds.Add(startSeconds);

            float running = startSeconds;
            for (int i = 1; i < waypoints.Count; i++)
            {
                running += TimeResourceMath.SegmentSeconds(waypoints[i - 1], waypoints[i], baseSecondsPerTile, stance);
                path.nodes.Add(waypoints[i]);
                path.arrivalSeconds.Add(running);
            }

            return path;
        }

        /// <summary>
        /// Builds a path from waypoints that already carry their arrival second. The ghost resolver
        /// needs this because a Shoot between two Moves consumes Time Resource without covering
        /// distance, so arrival seconds cannot be recomputed from distance alone.
        /// </summary>
        public static ScheduledPath FromTimedWaypoints(
            IReadOnlyList<GridCoordinate> waypoints,
            IReadOnlyList<float> arrivalSeconds)
        {
            var path = new ScheduledPath();
            if (waypoints == null || arrivalSeconds == null || waypoints.Count == 0)
            {
                return path;
            }

            if (waypoints.Count != arrivalSeconds.Count)
            {
                throw new ArgumentException("Each waypoint needs exactly one arrival second.", nameof(arrivalSeconds));
            }

            for (int i = 0; i < waypoints.Count; i++)
            {
                if (i > 0 && arrivalSeconds[i] < arrivalSeconds[i - 1])
                {
                    throw new ArgumentException("Arrival seconds must not decrease.", nameof(arrivalSeconds));
                }

                path.nodes.Add(waypoints[i]);
                path.arrivalSeconds.Add(arrivalSeconds[i]);
            }

            return path;
        }

        /// <summary>Position at a Time Resource second, clamped outside the booked window.</summary>
        public PlanarPosition Evaluate(float seconds)
        {
            if (nodes.Count == 0)
            {
                return new PlanarPosition(0f, 0f);
            }

            if (seconds <= arrivalSeconds[0])
            {
                return ToPlanar(nodes[0]);
            }

            int last = nodes.Count - 1;
            if (seconds >= arrivalSeconds[last])
            {
                return ToPlanar(nodes[last]);
            }

            for (int i = 1; i <= last; i++)
            {
                float segmentStart = arrivalSeconds[i - 1];
                float segmentEnd = arrivalSeconds[i];
                if (seconds > segmentEnd)
                {
                    continue;
                }

                float span = segmentEnd - segmentStart;
                float t = span <= 0f ? 1f : (seconds - segmentStart) / span;
                GridCoordinate a = nodes[i - 1];
                GridCoordinate b = nodes[i];
                return new PlanarPosition(
                    a.X + ((b.X - a.X) * t),
                    a.Y + ((b.Y - a.Y) * t),
                    b.Floor);
            }

            return ToPlanar(nodes[last]);
        }

        private static PlanarPosition ToPlanar(GridCoordinate c)
        {
            return new PlanarPosition(c.X, c.Y, c.Floor);
        }
    }
}
