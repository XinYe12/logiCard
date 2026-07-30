using System;
using System.Collections.Generic;

namespace LogiCard.Sim
{
    /// <summary>
    /// Integer grid line of sight (TDD D6 §4). Deliberately not a physics raycast: the Host's ghost
    /// resolve must produce the same answer on every machine, so sight is pure Bresenham over tiles.
    /// Sight is same-floor only, and blocked by any impassable tile between the endpoints (GDD §5).
    /// </summary>
    public static class GridLineOfSight
    {
        public static bool HasLineOfSight(GridBoard board, GridCoordinate from, GridCoordinate to)
        {
            if (board == null || from.Floor != to.Floor)
            {
                return false;
            }

            if (!board.InBounds(from) || !board.InBounds(to))
            {
                return false;
            }

            foreach (GridCoordinate step in TilesBetween(from, to))
            {
                if (!board.TryGetTile(step, out Tile tile) || !tile.IsPassable)
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Bresenham walk of the tiles strictly between the endpoints, so a shooter and its target
        /// never block their own shot.
        /// </summary>
        public static IEnumerable<GridCoordinate> TilesBetween(GridCoordinate from, GridCoordinate to)
        {
            if (from.Floor != to.Floor || from == to)
            {
                yield break;
            }

            int x = from.X;
            int y = from.Y;
            int deltaX = Math.Abs(to.X - x);
            int deltaY = Math.Abs(to.Y - y);
            int stepX = to.X > x ? 1 : -1;
            int stepY = to.Y > y ? 1 : -1;
            int error = deltaX - deltaY;

            while (true)
            {
                int doubled = 2 * error;
                if (doubled > -deltaY)
                {
                    error -= deltaY;
                    x += stepX;
                }

                if (doubled < deltaX)
                {
                    error += deltaX;
                    y += stepY;
                }

                if (x == to.X && y == to.Y)
                {
                    yield break;
                }

                yield return new GridCoordinate(x, y, from.Floor);
            }
        }
    }
}
