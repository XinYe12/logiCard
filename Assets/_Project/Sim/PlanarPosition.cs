using System;

namespace LogiCard.Sim
{
    /// <summary>
    /// Ground vs attic plane (C17 / C39 Decision 6). Lives with continuous position now that
    /// <c>GridCoordinate</c> is gone (Phase 4).
    /// </summary>
    public enum Floor
    {
        Ground = 0,
        Attic = 1,
    }

    /// <summary>
    /// Continuous-space position (C35/C39 pivot). Promoted out of <see cref="ScheduledPath"/> —
    /// <c>ToNearestCoordinate()</c> is deliberately gone; there is no grid to snap to anymore.
    /// Squared-distance helpers ignore <see cref="Floor"/>; callers that care about same-floor-ness
    /// (LoS, pathfinding) check <see cref="Floor"/> separately.
    /// </summary>
    public readonly struct PlanarPosition : IEquatable<PlanarPosition>
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

        public float DistanceTo(PlanarPosition other)
        {
            return (float)Math.Sqrt(SqrDistanceTo(other));
        }

        public float SqrDistanceTo(PlanarPosition other)
        {
            float dx = X - other.X;
            float dy = Y - other.Y;
            return (dx * dx) + (dy * dy);
        }

        /// <summary>
        /// Linear interpolation keeping <paramref name="a"/>'s floor. Used by resolve/path evaluation
        /// where both ends already share a floor.
        /// </summary>
        public static PlanarPosition Lerp(PlanarPosition a, PlanarPosition b, float t)
        {
            return new PlanarPosition(
                a.X + ((b.X - a.X) * t),
                a.Y + ((b.Y - a.Y) * t),
                a.Floor);
        }

        public static PlanarPosition operator +(PlanarPosition a, PlanarPosition b)
        {
            return new PlanarPosition(a.X + b.X, a.Y + b.Y, a.Floor);
        }

        public static PlanarPosition operator -(PlanarPosition a, PlanarPosition b)
        {
            return new PlanarPosition(a.X - b.X, a.Y - b.Y, a.Floor);
        }

        public bool Equals(PlanarPosition other)
        {
            return X.Equals(other.X) && Y.Equals(other.Y) && Floor == other.Floor;
        }

        public override bool Equals(object obj)
        {
            return obj is PlanarPosition other && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hash = X.GetHashCode();
                hash = (hash * 397) ^ Y.GetHashCode();
                hash = (hash * 397) ^ (int)Floor;
                return hash;
            }
        }

        public static bool operator ==(PlanarPosition left, PlanarPosition right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(PlanarPosition left, PlanarPosition right)
        {
            return !left.Equals(right);
        }

        public override string ToString()
        {
            return $"({X:0.###}, {Y:0.###}, {Floor})";
        }
    }
}
