using System;
using System.Collections.Generic;

namespace LogiCard.Sim
{
    /// <summary>
    /// Minimal board tile state for Day 2 path validation.
    /// </summary>
    public readonly struct Tile
    {
        public bool IsPassable { get; }

        public Tile(bool isPassable)
        {
            IsPassable = isPassable;
        }
    }

    /// <summary>
    /// Configurable orthogonal board containing the demo's stacked ground and attic grids (C17).
    /// </summary>
    public sealed class GridBoard
    {
        private readonly Dictionary<Floor, Tile[,]> tilesByFloor;
        private readonly Floor[] floors;
        private readonly Dictionary<GridCoordinate, Door> doorsByCoordinate = new Dictionary<GridCoordinate, Door>();
        private readonly List<Door> doors = new List<Door>();

        public int Width { get; }

        public int Height { get; }

        public IReadOnlyList<Floor> Floors => floors;

        public IReadOnlyList<Door> Doors => doors;

        public GridBoard(int width = 5, int height = 5, IEnumerable<Floor> floors = null)
        {
            if (width <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(width), "Width must be positive.");
            }

            if (height <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(height), "Height must be positive.");
            }

            Width = width;
            Height = height;

            var requestedFloors = floors == null
                ? new[] { Floor.Ground, Floor.Attic }
                : new List<Floor>(floors).ToArray();

            if (requestedFloors.Length == 0)
            {
                throw new ArgumentException("A board must contain at least one floor.", nameof(floors));
            }

            tilesByFloor = new Dictionary<Floor, Tile[,]>();
            var uniqueFloors = new List<Floor>();
            foreach (Floor floor in requestedFloors)
            {
                if (!Enum.IsDefined(typeof(Floor), floor))
                {
                    throw new ArgumentOutOfRangeException(nameof(floors), floor, "Floor is not defined.");
                }

                if (tilesByFloor.ContainsKey(floor))
                {
                    continue;
                }

                var floorTiles = new Tile[width, height];
                for (int x = 0; x < width; x++)
                {
                    for (int y = 0; y < height; y++)
                    {
                        floorTiles[x, y] = new Tile(true);
                    }
                }

                tilesByFloor.Add(floor, floorTiles);
                uniqueFloors.Add(floor);
            }

            this.floors = uniqueFloors.ToArray();
        }

        public Tile this[GridCoordinate coordinate]
        {
            get
            {
                EnsureInBounds(coordinate);
                return tilesByFloor[coordinate.Floor][coordinate.X, coordinate.Y];
            }
            set
            {
                EnsureInBounds(coordinate);
                tilesByFloor[coordinate.Floor][coordinate.X, coordinate.Y] = value;
            }
        }

        public bool InBounds(GridCoordinate coordinate)
        {
            return coordinate.X >= 0
                && coordinate.X < Width
                && coordinate.Y >= 0
                && coordinate.Y < Height
                && tilesByFloor.ContainsKey(coordinate.Floor);
        }

        public Tile GetTile(GridCoordinate coordinate)
        {
            return this[coordinate];
        }

        public bool TryGetTile(GridCoordinate coordinate, out Tile tile)
        {
            if (!InBounds(coordinate))
            {
                tile = default;
                return false;
            }

            tile = tilesByFloor[coordinate.Floor][coordinate.X, coordinate.Y];
            return true;
        }

        /// <summary>
        /// Registers a door and immediately applies its <see cref="Door.InitialState"/> to the
        /// door's tile (Closed ⇒ impassable, Open ⇒ ordinary floor).
        /// </summary>
        public void RegisterDoor(Door door)
        {
            if (door == null)
            {
                throw new ArgumentNullException(nameof(door));
            }

            EnsureInBounds(door.Coordinate);
            if (doorsByCoordinate.ContainsKey(door.Coordinate))
            {
                throw new InvalidOperationException($"A door is already registered at {door.Coordinate}.");
            }

            doorsByCoordinate.Add(door.Coordinate, door);
            doors.Add(door);
            this[door.Coordinate] = new Tile(door.InitialState == DoorState.Open);
        }

        public bool TryGetDoor(GridCoordinate coordinate, out Door door)
        {
            return doorsByCoordinate.TryGetValue(coordinate, out door);
        }

        /// <summary>
        /// Tile-state copy used by <see cref="LogiCard.Net.GhostResolver"/> as a resolve-local
        /// scratch board (Day 7 research note §F/H2, Option 1) — doors toggle mid-resolve without
        /// mutating the shared board instance. Door registrations are not copied; scratch boards
        /// only need passability, never door identity.
        /// </summary>
        public GridBoard Clone()
        {
            var clone = new GridBoard(Width, Height, floors);
            foreach (GridCoordinate coordinate in GetAllCoordinates())
            {
                clone[coordinate] = this[coordinate];
            }

            return clone;
        }

        public IEnumerable<GridCoordinate> GetAllCoordinates()
        {
            foreach (Floor floor in floors)
            {
                for (int y = 0; y < Height; y++)
                {
                    for (int x = 0; x < Width; x++)
                    {
                        yield return new GridCoordinate(x, y, floor);
                    }
                }
            }
        }

        private void EnsureInBounds(GridCoordinate coordinate)
        {
            if (!InBounds(coordinate))
            {
                throw new ArgumentOutOfRangeException(nameof(coordinate), coordinate, "Coordinate is outside this board.");
            }
        }
    }
}
