using System.Collections.Generic;

namespace LogiCard.Sim
{
    /// <summary>
    /// Shortest orthogonal routes on a <see cref="GridBoard"/> (GDD §3.1). Diagonals are illegal;
    /// closed / impassable tiles block the search. Pure C# so Program and resolve share one path.
    /// </summary>
    public static class OrthogonalPathfinder
    {
        /// <summary>
        /// Fills <paramref name="path"/> with waypoints from the step after <paramref name="from"/>
        /// through <paramref name="to"/> inclusive. Returns false when no legal route exists.
        /// </summary>
        public static bool TryFindPath(
            GridBoard board,
            GridCoordinate from,
            GridCoordinate to,
            List<GridCoordinate> path)
        {
            if (path == null)
            {
                return false;
            }

            path.Clear();

            if (board == null || !board.InBounds(from) || !board.InBounds(to) || from.Floor != to.Floor)
            {
                return false;
            }

            if (from == to)
            {
                return true;
            }

            if (!board.GetTile(to).IsPassable)
            {
                return false;
            }

            var cameFrom = new Dictionary<GridCoordinate, GridCoordinate>();
            var queue = new Queue<GridCoordinate>();
            queue.Enqueue(from);
            cameFrom[from] = from;

            while (queue.Count > 0)
            {
                GridCoordinate current = queue.Dequeue();
                if (current == to)
                {
                    break;
                }

                foreach (GridCoordinate neighbour in current.GetOrthogonalNeighbours())
                {
                    if (cameFrom.ContainsKey(neighbour) || !board.InBounds(neighbour))
                    {
                        continue;
                    }

                    if (!board.GetTile(neighbour).IsPassable)
                    {
                        continue;
                    }

                    cameFrom[neighbour] = current;
                    queue.Enqueue(neighbour);
                }
            }

            if (!cameFrom.ContainsKey(to))
            {
                return false;
            }

            var reverse = new List<GridCoordinate>();
            GridCoordinate step = to;
            while (step != from)
            {
                reverse.Add(step);
                step = cameFrom[step];
            }

            for (int i = reverse.Count - 1; i >= 0; i--)
            {
                path.Add(reverse[i]);
            }

            return true;
        }
    }
}
