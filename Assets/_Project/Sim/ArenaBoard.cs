using System;
using System.Collections.Generic;

namespace LogiCard.Sim
{
    /// <summary>
    /// Continuous-space replacement for <see cref="GridBoard"/> (C35/C39): static wall segments plus
    /// doors whose passability toggles over time. Default bounds are [0,4]×[0,4] (Decision 7).
    /// Door API is <see cref="Door"/>-typed (<b>C41</b>) — not an int-id registry.
    /// </summary>
    public sealed class ArenaBoard
    {
        private readonly List<Segment> walls = new List<Segment>();
        private readonly List<Door> doors = new List<Door>();
        private readonly Dictionary<Door, DoorState> doorStates = new Dictionary<Door, DoorState>();
        private readonly Floor[] floors;

        public float MinX { get; }

        public float MinY { get; }

        public float MaxX { get; }

        public float MaxY { get; }

        public IReadOnlyList<Floor> Floors => floors;

        public IReadOnlyList<Segment> Walls => walls;

        public IReadOnlyList<Door> Doors => doors;

        public ArenaBoard(float minX = 0f, float minY = 0f, float maxX = 4f, float maxY = 4f, IEnumerable<Floor> floors = null)
        {
            if (maxX <= minX)
            {
                throw new ArgumentOutOfRangeException(nameof(maxX), "maxX must exceed minX.");
            }

            if (maxY <= minY)
            {
                throw new ArgumentOutOfRangeException(nameof(maxY), "maxY must exceed minY.");
            }

            MinX = minX;
            MinY = minY;
            MaxX = maxX;
            MaxY = maxY;

            var requestedFloors = floors == null
                ? new[] { Floor.Ground, Floor.Attic }
                : new List<Floor>(floors).ToArray();

            if (requestedFloors.Length == 0)
            {
                throw new ArgumentException("A board must contain at least one floor.", nameof(floors));
            }

            this.floors = requestedFloors;
        }

        public bool InBounds(PlanarPosition point)
        {
            return point.X >= MinX && point.X <= MaxX
                && point.Y >= MinY && point.Y <= MaxY
                && Array.IndexOf(floors, point.Floor) >= 0;
        }

        public void RegisterWall(Segment wall)
        {
            EnsureInBounds(wall);
            walls.Add(wall);
        }

        public void RegisterDoor(Door door)
        {
            if (door == null)
            {
                throw new ArgumentNullException(nameof(door));
            }

            EnsureInBounds(door.Segment);
            doors.Add(door);
            doorStates[door] = door.InitialState;
        }

        /// <summary>
        /// Exact reverse lookup by segment midpoint (within float tolerance). Not a proximity search —
        /// see <see cref="TryGetNearestDoor"/>.
        /// </summary>
        public bool TryGetDoor(PlanarPosition point, out Door door)
        {
            foreach (Door candidate in doors)
            {
                PlanarPosition mid = PlanarPosition.Lerp(candidate.Segment.A, candidate.Segment.B, 0.5f);
                if (mid.SqrDistanceTo(point) <= 1e-6f)
                {
                    door = candidate;
                    return true;
                }
            }

            door = null;
            return false;
        }

        /// <summary>Radius-based door interaction (Decision 4) — closest door within maxDistance.</summary>
        public bool TryGetNearestDoor(PlanarPosition point, float maxDistance, out Door door)
        {
            Door best = null;
            float bestDistance = float.MaxValue;
            foreach (Door candidate in doors)
            {
                float distance = candidate.Segment.DistanceToPoint(point);
                if (distance <= maxDistance && distance < bestDistance)
                {
                    bestDistance = distance;
                    best = candidate;
                }
            }

            door = best;
            return best != null;
        }

        public DoorState GetDoorState(Door door)
        {
            return doorStates.TryGetValue(door, out DoorState state) ? state : door.InitialState;
        }

        public void SetDoorState(Door door, DoorState state)
        {
            doorStates[door] = state;
        }

        /// <summary>Blocked by any wall, or any currently-closed door, that the probe crosses.</summary>
        public bool IsBlocking(Segment probe)
        {
            for (int i = 0; i < walls.Count; i++)
            {
                if (walls[i].Intersects(probe))
                {
                    return true;
                }
            }

            for (int i = 0; i < doors.Count; i++)
            {
                if (GetDoorState(doors[i]) == DoorState.Closed && doors[i].Segment.Intersects(probe))
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Resolve-local scratch copy: only door *state* is snapshotted (walls never move mid-match).
        /// </summary>
        public ArenaBoard Clone()
        {
            var clone = new ArenaBoard(MinX, MinY, MaxX, MaxY, floors);
            clone.walls.AddRange(walls);
            clone.doors.AddRange(doors);
            foreach (Door door in doors)
            {
                clone.doorStates[door] = GetDoorState(door);
            }

            return clone;
        }

        private void EnsureInBounds(Segment segment)
        {
            if (!InBounds(segment.A) || !InBounds(segment.B))
            {
                throw new ArgumentOutOfRangeException(nameof(segment), segment, "Segment endpoint is outside this board.");
            }
        }
    }
}
