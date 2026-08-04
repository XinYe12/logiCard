using System;
using System.Collections.Generic;

namespace LogiCard.Sim
{
    /// <summary>
    /// Continuous replacement for <see cref="OrthogonalPathfinder"/> (C39 Decision 3): visibility
    /// graph over start, goal, and every closed-obstacle endpoint (nudged by clearance epsilon),
    /// solved by Dijkstra with Euclidean weights and deterministic insertion-order tie-break.
    /// Clearance nodes that would leave the arena are skipped (not clamped) — clamping cancelled
    /// clearance at boundary walls (Phase 1 verification fix).
    /// </summary>
    public static class ContinuousPathfinder
    {
        private const float ClearanceEpsilon = 0.05f;

        /// <summary>
        /// Fills <paramref name="path"/> with waypoints from the step after <paramref name="from"/>
        /// through <paramref name="to"/> inclusive. Returns false when no legal route exists.
        /// </summary>
        public static bool TryFindPath(
            ArenaBoard board,
            PlanarPosition from,
            PlanarPosition to,
            List<PlanarPosition> path)
        {
            if (path == null)
            {
                return false;
            }

            path.Clear();

            if (board == null || from.Floor != to.Floor || !board.InBounds(from) || !board.InBounds(to))
            {
                return false;
            }

            if (from.Equals(to))
            {
                return true;
            }

            if (!board.IsBlocking(new Segment(from, to)))
            {
                path.Add(to);
                return true;
            }

            List<PlanarPosition> nodes = BuildNodes(board, from, to);
            int nodeCount = nodes.Count;
            List<int>[] adjacency = BuildAdjacency(board, nodes);

            var distance = new float[nodeCount];
            var previous = new int[nodeCount];
            var visited = new bool[nodeCount];
            for (int i = 0; i < nodeCount; i++)
            {
                distance[i] = float.MaxValue;
                previous[i] = -1;
            }

            distance[0] = 0f;

            for (int iteration = 0; iteration < nodeCount; iteration++)
            {
                int current = -1;
                float best = float.MaxValue;
                for (int i = 0; i < nodeCount; i++)
                {
                    if (!visited[i] && distance[i] < best)
                    {
                        best = distance[i];
                        current = i;
                    }
                }

                if (current == -1)
                {
                    break;
                }

                visited[current] = true;
                if (current == 1)
                {
                    break;
                }

                List<int> neighbours = adjacency[current];
                for (int n = 0; n < neighbours.Count; n++)
                {
                    int neighbour = neighbours[n];
                    if (visited[neighbour])
                    {
                        continue;
                    }

                    float candidate = distance[current] + nodes[current].DistanceTo(nodes[neighbour]);
                    if (candidate < distance[neighbour] - 1e-6f)
                    {
                        distance[neighbour] = candidate;
                        previous[neighbour] = current;
                    }
                }
            }

            if (!(distance[1] < float.MaxValue))
            {
                return false;
            }

            var reverse = new List<int>();
            int step = 1;
            while (step != 0)
            {
                reverse.Add(step);
                step = previous[step];
                if (step == -1)
                {
                    path.Clear();
                    return false;
                }
            }

            for (int i = reverse.Count - 1; i >= 0; i--)
            {
                path.Add(nodes[reverse[i]]);
            }

            return true;
        }

        /// <summary>
        /// Nodes: index 0 = from, 1 = to, then wall/closed-door endpoints nudged outward in
        /// Walls-then-Doors order (deterministic Dijkstra tie-break).
        /// </summary>
        private static List<PlanarPosition> BuildNodes(ArenaBoard board, PlanarPosition from, PlanarPosition to)
        {
            var nodes = new List<PlanarPosition> { from, to };

            IReadOnlyList<Segment> walls = board.Walls;
            for (int i = 0; i < walls.Count; i++)
            {
                Segment wall = walls[i];
                AddNudgedNodeIfClearanceFits(nodes, board, wall.A, wall.B, from.Floor);
                AddNudgedNodeIfClearanceFits(nodes, board, wall.B, wall.A, from.Floor);
            }

            IReadOnlyList<Door> doors = board.Doors;
            for (int i = 0; i < doors.Count; i++)
            {
                Door door = doors[i];
                if (board.GetDoorState(door) != DoorState.Closed)
                {
                    continue;
                }

                AddNudgedNodeIfClearanceFits(nodes, board, door.Segment.A, door.Segment.B, from.Floor);
                AddNudgedNodeIfClearanceFits(nodes, board, door.Segment.B, door.Segment.A, from.Floor);
            }

            return nodes;
        }

        /// <summary>
        /// Adds a clearance-nudged node only if it stays in-bounds. Skipping (not clamping) avoids
        /// pivoting through zero clearance at a wall foot on the arena boundary.
        /// </summary>
        private static void AddNudgedNodeIfClearanceFits(
            List<PlanarPosition> nodes, ArenaBoard board, PlanarPosition point, PlanarPosition awayFrom, Floor floor)
        {
            PlanarPosition nudged = Nudge(point, awayFrom, ClearanceEpsilon);
            var withFloor = new PlanarPosition(nudged.X, nudged.Y, floor);
            if (board.InBounds(withFloor))
            {
                nodes.Add(withFloor);
            }
        }

        private static List<int>[] BuildAdjacency(ArenaBoard board, List<PlanarPosition> nodes)
        {
            int count = nodes.Count;
            var adjacency = new List<int>[count];
            for (int i = 0; i < count; i++)
            {
                adjacency[i] = new List<int>();
            }

            for (int i = 0; i < count; i++)
            {
                for (int j = i + 1; j < count; j++)
                {
                    var edge = new Segment(nodes[i], nodes[j]);
                    if (!board.IsBlocking(edge))
                    {
                        adjacency[i].Add(j);
                        adjacency[j].Add(i);
                    }
                }
            }

            return adjacency;
        }

        private static PlanarPosition Nudge(PlanarPosition point, PlanarPosition awayFrom, float epsilon)
        {
            float dx = point.X - awayFrom.X;
            float dy = point.Y - awayFrom.Y;
            float length = (float)Math.Sqrt((dx * dx) + (dy * dy));
            if (length <= 1e-6f)
            {
                return point;
            }

            return new PlanarPosition(
                point.X + (dx / length * epsilon),
                point.Y + (dy / length * epsilon),
                point.Floor);
        }
    }
}
