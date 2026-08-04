using System;
using System.Collections.Generic;

namespace LogiCard.Sim
{
    /// <summary>
    /// A Move booked against the Time Resource budget: waypoints plus the cumulative
    /// second at which the pawn reaches each one. Sampling this is what playback reads,
    /// so the same structure serves the Host ghost sim and the client tape (C23).
    ///
    /// Retargeted onto continuous <see cref="PlanarPosition"/> (C35/C39 pivot) — the embedded
    /// PlanarPosition struct that used to live in this file moved to its own file in Phase 1;
    /// evaluation no longer round-trips through a grid coordinate, which also fixes the round-carry
    /// rounding drift the old <c>ToNearestCoordinate()</c> snap introduced between rounds.
    /// </summary>
    public sealed class ScheduledPath
    {
        private readonly List<PlanarPosition> nodes = new List<PlanarPosition>();
        private readonly List<float> arrivalSeconds = new List<float>();

        public IReadOnlyList<PlanarPosition> Nodes => nodes;

        public IReadOnlyList<float> ArrivalSeconds => arrivalSeconds;

        public float StartSeconds => arrivalSeconds.Count == 0 ? 0f : arrivalSeconds[0];

        public float EndSeconds => arrivalSeconds.Count == 0 ? 0f : arrivalSeconds[arrivalSeconds.Count - 1];

        public static ScheduledPath FromWaypoints(
            IReadOnlyList<PlanarPosition> waypoints,
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
            IReadOnlyList<PlanarPosition> waypoints,
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
                return nodes[0];
            }

            int last = nodes.Count - 1;
            if (seconds >= arrivalSeconds[last])
            {
                return nodes[last];
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
                return PlanarPosition.Lerp(nodes[i - 1], nodes[i], t);
            }

            return nodes[last];
        }
    }
}
