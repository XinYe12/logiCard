using System;
using System.Collections.Generic;

namespace LogiCard.Sim
{
    /// <summary>
    /// Identifies one tile on the ground or attic grid (C17).
    /// </summary>
    public enum Floor
    {
        Ground = 0,
        Attic = 1,
    }

    /// <summary>
    /// Immutable integer coordinate used by the deterministic orthogonal grid simulation.
    /// </summary>
    public readonly struct GridCoordinate : IEquatable<GridCoordinate>
    {
        public int X { get; }

        public int Y { get; }

        public Floor Floor { get; }

        public GridCoordinate(int x, int y, Floor floor = Floor.Ground)
        {
            X = x;
            Y = y;
            Floor = floor;
        }

        /// <summary>
        /// Returns the north, east, south, and west coordinates without changing floor.
        /// Bounds are intentionally left to <see cref="GridBoard"/>.
        /// </summary>
        public IEnumerable<GridCoordinate> GetOrthogonalNeighbours()
        {
            yield return Offset(0, 1);
            yield return Offset(1, 0);
            yield return Offset(0, -1);
            yield return Offset(-1, 0);
        }

        public int ManhattanDistanceTo(GridCoordinate other)
        {
            if (Floor != other.Floor)
            {
                throw new ArgumentException("Manhattan distance requires coordinates on the same floor.", nameof(other));
            }

            return Math.Abs(X - other.X) + Math.Abs(Y - other.Y);
        }

        public GridCoordinate Offset(int deltaX, int deltaY)
        {
            return new GridCoordinate(X + deltaX, Y + deltaY, Floor);
        }

        public GridCoordinate Translate(int deltaX, int deltaY)
        {
            return Offset(deltaX, deltaY);
        }

        public bool Equals(GridCoordinate other)
        {
            return X == other.X && Y == other.Y && Floor == other.Floor;
        }

        public override bool Equals(object obj)
        {
            return obj is GridCoordinate other && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hash = X;
                hash = (hash * 397) ^ Y;
                hash = (hash * 397) ^ (int)Floor;
                return hash;
            }
        }

        public static bool operator ==(GridCoordinate left, GridCoordinate right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(GridCoordinate left, GridCoordinate right)
        {
            return !left.Equals(right);
        }

        public override string ToString()
        {
            return $"({X}, {Y}, {Floor})";
        }
    }
}
