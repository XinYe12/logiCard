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

        public int Width { get; }

        public int Height { get; }

        public IReadOnlyList<Floor> Floors => floors;

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
