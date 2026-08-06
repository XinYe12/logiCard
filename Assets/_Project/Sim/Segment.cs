using System;

namespace LogiCard.Sim
{
    /// <summary>
    /// A straight continuous-space edge: wall face, door, aim line, or pathfinder visibility edge.
    /// Pure cross/dot-product math — never <c>Physics</c>/<c>Physics2D</c> (C32 Host-resolve
    /// determinism). <see cref="Intersects"/> is inclusive of shared endpoints / corner grazes
    /// (<b>C41</b>) — a bare touch counts as intersecting and therefore blocks LoS/movement.
    /// </summary>
    public readonly struct Segment
    {
        private const float Epsilon = 1e-6f;

        public PlanarPosition A { get; }

        public PlanarPosition B { get; }

        public Segment(PlanarPosition a, PlanarPosition b)
        {
            A = a;
            B = b;
        }

        /// <summary>
        /// True if the segments cross, including shared endpoints / corner grazes (C41).
        /// </summary>
        public bool Intersects(Segment other)
        {
            float d1 = Orientation(other.A, other.B, A);
            float d2 = Orientation(other.A, other.B, B);
            float d3 = Orientation(A, B, other.A);
            float d4 = Orientation(A, B, other.B);

            if (((d1 > 0f && d2 < 0f) || (d1 < 0f && d2 > 0f))
                && ((d3 > 0f && d4 < 0f) || (d3 < 0f && d4 > 0f)))
            {
                return true;
            }

            if (Math.Abs(d1) <= Epsilon && OnSegment(other.A, other.B, A))
            {
                return true;
            }

            if (Math.Abs(d2) <= Epsilon && OnSegment(other.A, other.B, B))
            {
                return true;
            }

            if (Math.Abs(d3) <= Epsilon && OnSegment(A, B, other.A))
            {
                return true;
            }

            if (Math.Abs(d4) <= Epsilon && OnSegment(A, B, other.B))
            {
                return true;
            }

            return false;
        }

        /// <summary>Closest distance from <paramref name="point"/> to this segment (clamped to A..B).</summary>
        public float DistanceToPoint(PlanarPosition point)
        {
            return point.DistanceTo(ClosestPointOnSegment(point));
        }

        public PlanarPosition ClosestPointOnSegment(PlanarPosition point)
        {
            float t = ProjectParam(point);
            float clamped = t < 0f ? 0f : (t > 1f ? 1f : t);
            return PlanarPosition.Lerp(A, B, clamped);
        }

        /// <summary>
        /// Unclamped parameter t such that A + t*(B-A) is the closest point on the infinite line
        /// through A/B. Hold Angle lane coverage checks t against [0,1] itself.
        /// </summary>
        public float ProjectParam(PlanarPosition point)
        {
            float abx = B.X - A.X;
            float aby = B.Y - A.Y;
            float lengthSq = (abx * abx) + (aby * aby);
            if (lengthSq <= Epsilon)
            {
                return 0f;
            }

            float apx = point.X - A.X;
            float apy = point.Y - A.Y;
            return ((apx * abx) + (apy * aby)) / lengthSq;
        }

        /// <summary>
        /// Parametric crossing point between this segment (<paramref name="t"/>) and
        /// <paramref name="other"/> (<paramref name="u"/>), both in [0,1] (epsilon-tolerant) when a
        /// transversal crossing exists. Unlike <see cref="Intersects"/> (a legality check that also
        /// treats touches/corner grazes as blocking), this needs the actual crossing location — used
        /// to find *where and when* a movement leg crosses a door segment (GDD.md "stop before the
        /// block"), not whether it's legal to draft at all. Returns false for parallel/collinear
        /// segments (including exact overlap), where there's no single well-defined crossing point.
        /// </summary>
        public bool TryIntersectionParams(Segment other, out float t, out float u)
        {
            float d1x = B.X - A.X;
            float d1y = B.Y - A.Y;
            float d2x = other.B.X - other.A.X;
            float d2y = other.B.Y - other.A.Y;

            float denom = (d1x * d2y) - (d1y * d2x);
            if (Math.Abs(denom) <= Epsilon)
            {
                t = 0f;
                u = 0f;
                return false;
            }

            float dx = other.A.X - A.X;
            float dy = other.A.Y - A.Y;

            t = ((dx * d2y) - (dy * d2x)) / denom;
            u = ((dx * d1y) - (dy * d1x)) / denom;

            return t >= -Epsilon && t <= 1f + Epsilon && u >= -Epsilon && u <= 1f + Epsilon;
        }

        public override string ToString()
        {
            return $"[{A} -> {B}]";
        }

        private static float Orientation(PlanarPosition o, PlanarPosition a, PlanarPosition b)
        {
            return ((a.X - o.X) * (b.Y - o.Y)) - ((a.Y - o.Y) * (b.X - o.X));
        }

        private static bool OnSegment(PlanarPosition a, PlanarPosition b, PlanarPosition point)
        {
            float minX = Math.Min(a.X, b.X) - Epsilon;
            float maxX = Math.Max(a.X, b.X) + Epsilon;
            float minY = Math.Min(a.Y, b.Y) - Epsilon;
            float maxY = Math.Max(a.Y, b.Y) + Epsilon;
            return point.X >= minX && point.X <= maxX && point.Y >= minY && point.Y <= maxY;
        }
    }
}
