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
        private readonly List<BreachPoint> breachPoints = new List<BreachPoint>();
        private readonly Dictionary<BreachPoint, BreachState> breachStates = new Dictionary<BreachPoint, BreachState>();

        /// <summary>Whether a Bomber has an attached, undetonated bomb on this point (C71 — persists
        /// across rounds like <see cref="BreachState"/> and wounds, C33/C36). Independent of
        /// <see cref="BreachState"/>: attaching never changes geometry, only Detonate does.</summary>
        private readonly Dictionary<BreachPoint, bool> attachedBombs = new Dictionary<BreachPoint, bool>();

        private readonly Floor[] floors;

        public float MinX { get; }

        public float MinY { get; }

        public float MaxX { get; }

        public float MaxY { get; }

        public IReadOnlyList<Floor> Floors => floors;

        public IReadOnlyList<Segment> Walls => walls;

        public IReadOnlyList<Door> Doors => doors;

        public IReadOnlyList<BreachPoint> BreachPoints => breachPoints;

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

        public void RegisterBreachPoint(BreachPoint point)
        {
            if (point == null)
            {
                throw new ArgumentNullException(nameof(point));
            }

            EnsureInBounds(point.Segment);
            breachPoints.Add(point);
            breachStates[point] = point.InitialState;
            attachedBombs[point] = false;
        }

        /// <inheritdoc cref="TryGetDoor"/>
        public bool TryGetBreachPoint(PlanarPosition point, out BreachPoint breachPoint)
        {
            foreach (BreachPoint candidate in breachPoints)
            {
                PlanarPosition mid = PlanarPosition.Lerp(candidate.Segment.A, candidate.Segment.B, 0.5f);
                if (mid.SqrDistanceTo(point) <= 1e-6f)
                {
                    breachPoint = candidate;
                    return true;
                }
            }

            breachPoint = null;
            return false;
        }

        /// <inheritdoc cref="TryGetNearestDoor"/>
        public bool TryGetNearestBreachPoint(PlanarPosition point, float maxDistance, out BreachPoint breachPoint)
        {
            BreachPoint best = null;
            float bestDistance = float.MaxValue;
            foreach (BreachPoint candidate in breachPoints)
            {
                float distance = candidate.Segment.DistanceToPoint(point);
                if (distance <= maxDistance && distance < bestDistance)
                {
                    bestDistance = distance;
                    best = candidate;
                }
            }

            breachPoint = best;
            return best != null;
        }

        public BreachState GetBreachState(BreachPoint point)
        {
            return breachStates.TryGetValue(point, out BreachState state) ? state : point.InitialState;
        }

        public void SetBreachState(BreachPoint point, BreachState state)
        {
            breachStates[point] = state;
        }

        /// <summary>Whether a Bomber has an attached, undetonated bomb on this point right now.</summary>
        public bool HasAttachedBomb(BreachPoint point)
        {
            return attachedBombs.TryGetValue(point, out bool attached) && attached;
        }

        public void SetAttachedBomb(BreachPoint point, bool attached)
        {
            attachedBombs[point] = attached;
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

        /// <summary>Blocked by any wall, any currently-closed door, or any breach point not yet
        /// <see cref="BreachState.Breached"/>, that the probe crosses. An Intact/Damaged breach point
        /// reads exactly like a wall — only Breached opens it.</summary>
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

            for (int i = 0; i < breachPoints.Count; i++)
            {
                if (GetBreachState(breachPoints[i]) != BreachState.Breached && breachPoints[i].Segment.Intersects(probe))
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Nearest point along <paramref name="probe"/> (from <c>probe.A</c>) where a wall or
        /// currently-closed door crosses it — for truncating a shot tracer at the obstacle instead of
        /// drawing it straight through (playtest 2026-08-07: bullets visually passed through walls
        /// even though <see cref="IsBlocking"/> already denied the hit). Not used for hit resolution;
        /// <see cref="IsBlocking"/> stays the source of truth there.
        /// </summary>
        public bool TryGetNearestBlockPoint(Segment probe, out PlanarPosition point)
        {
            bool found = false;
            float bestT = float.MaxValue;
            point = default;

            for (int i = 0; i < walls.Count; i++)
            {
                if (probe.TryGetIntersection(walls[i], out PlanarPosition p))
                {
                    float t = probe.ProjectParam(p);
                    if (t < bestT)
                    {
                        bestT = t;
                        point = p;
                        found = true;
                    }
                }
            }

            for (int i = 0; i < doors.Count; i++)
            {
                if (GetDoorState(doors[i]) != DoorState.Closed)
                {
                    continue;
                }

                if (probe.TryGetIntersection(doors[i].Segment, out PlanarPosition p))
                {
                    float t = probe.ProjectParam(p);
                    if (t < bestT)
                    {
                        bestT = t;
                        point = p;
                        found = true;
                    }
                }
            }

            for (int i = 0; i < breachPoints.Count; i++)
            {
                if (GetBreachState(breachPoints[i]) == BreachState.Breached)
                {
                    continue;
                }

                if (probe.TryGetIntersection(breachPoints[i].Segment, out PlanarPosition p))
                {
                    float t = probe.ProjectParam(p);
                    if (t < bestT)
                    {
                        bestT = t;
                        point = p;
                        found = true;
                    }
                }
            }

            return found;
        }

        /// <summary>
        /// Resolve-local scratch copy: door/breach *state* and attached-bomb flags are snapshotted
        /// (walls, doors, and breach points themselves never move mid-match).
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

            clone.breachPoints.AddRange(breachPoints);
            foreach (BreachPoint point in breachPoints)
            {
                clone.breachStates[point] = GetBreachState(point);
                clone.attachedBombs[point] = HasAttachedBomb(point);
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
