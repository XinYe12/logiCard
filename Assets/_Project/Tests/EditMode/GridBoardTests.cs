using System;
using System.Collections.Generic;
using System.Linq;
using LogiCard.Sim;
using NUnit.Framework;

namespace LogiCard.Tests.EditMode
{
    [TestFixture]
    public sealed class GridBoardTests
    {
        [Test]
        public void DefaultBoard_HasFiveByFiveGroundAndAtticFloors()
        {
            var board = new GridBoard();

            Assert.That(board.Width, Is.EqualTo(5));
            Assert.That(board.Height, Is.EqualTo(5));
            Assert.That(board.Floors, Is.EqualTo(new[] { Floor.Ground, Floor.Attic }));
            Assert.That(board.GetAllCoordinates().Count(), Is.EqualTo(50));
        }

        [Test]
        public void InBounds_RejectsCoordinatesOutsideEitherAxis()
        {
            var board = new GridBoard();

            Assert.That(board.InBounds(new GridCoordinate(0, 0)), Is.True);
            Assert.That(board.InBounds(new GridCoordinate(4, 4, Floor.Attic)), Is.True);
            Assert.That(board.InBounds(new GridCoordinate(-1, 0)), Is.False);
            Assert.That(board.InBounds(new GridCoordinate(0, -1)), Is.False);
            Assert.That(board.InBounds(new GridCoordinate(5, 0)), Is.False);
            Assert.That(board.InBounds(new GridCoordinate(0, 5)), Is.False);
        }

        [Test]
        public void InBounds_RejectsFloorNotIncludedByBoard()
        {
            var board = new GridBoard(floors: new[] { Floor.Ground });

            Assert.That(board.InBounds(new GridCoordinate(0, 0, Floor.Ground)), Is.True);
            Assert.That(board.InBounds(new GridCoordinate(0, 0, Floor.Attic)), Is.False);
        }

        [Test]
        public void Indexer_CanReadAndReplaceTile()
        {
            var board = new GridBoard();
            var coordinate = new GridCoordinate(2, 3);

            Assert.That(board[coordinate].IsPassable, Is.True);

            board[coordinate] = new Tile(false);

            Assert.That(board.GetTile(coordinate).IsPassable, Is.False);
        }

        [Test]
        public void OutOfBoundsLookup_ThrowsArgumentOutOfRangeException()
        {
            var board = new GridBoard();

            Assert.Throws<ArgumentOutOfRangeException>(() => board.GetTile(new GridCoordinate(5, 0)));
        }

        [Test]
        public void TryGetTile_ReturnsTileOrFalseWithoutThrowing()
        {
            var board = new GridBoard();
            board[new GridCoordinate(1, 1)] = new Tile(false);

            bool found = board.TryGetTile(new GridCoordinate(1, 1), out Tile tile);
            bool missing = board.TryGetTile(new GridCoordinate(-1, 1), out Tile missingTile);

            Assert.That(found, Is.True);
            Assert.That(tile.IsPassable, Is.False);
            Assert.That(missing, Is.False);
            Assert.That(missingTile.IsPassable, Is.False);
        }

        [Test]
        public void GridCoordinate_EqualityOperatorsAndHashingIncludeFloor()
        {
            var first = new GridCoordinate(2, 4, Floor.Ground);
            var equal = new GridCoordinate(2, 4, Floor.Ground);
            var otherFloor = new GridCoordinate(2, 4, Floor.Attic);
            var set = new HashSet<GridCoordinate> { first, equal, otherFloor };

            Assert.That(first.Equals(equal), Is.True);
            Assert.That(first == equal, Is.True);
            Assert.That(first != otherFloor, Is.True);
            Assert.That(first.GetHashCode(), Is.EqualTo(equal.GetHashCode()));
            Assert.That(set.Count, Is.EqualTo(2));
        }

        [Test]
        public void OrthogonalNeighbours_ContainCardinalTilesOnlyAndKeepFloor()
        {
            var center = new GridCoordinate(2, 2, Floor.Attic);
            GridCoordinate[] neighbours = center.GetOrthogonalNeighbours().ToArray();

            Assert.That(neighbours, Is.EquivalentTo(new[]
            {
                new GridCoordinate(2, 3, Floor.Attic),
                new GridCoordinate(3, 2, Floor.Attic),
                new GridCoordinate(2, 1, Floor.Attic),
                new GridCoordinate(1, 2, Floor.Attic),
            }));
            Assert.That(neighbours.Any(coordinate => coordinate == new GridCoordinate(3, 3, Floor.Attic)), Is.False);
            Assert.That(neighbours.All(coordinate => coordinate.Floor == Floor.Attic), Is.True);
        }

        [Test]
        public void OffsetTranslateAndManhattanDistance_UseOrthogonalGridMath()
        {
            var start = new GridCoordinate(1, 1, Floor.Attic);
            GridCoordinate destination = start.Translate(3, 2);

            Assert.That(destination, Is.EqualTo(new GridCoordinate(4, 3, Floor.Attic)));
            Assert.That(start.Offset(-1, 1), Is.EqualTo(new GridCoordinate(0, 2, Floor.Attic)));
            Assert.That(start.ManhattanDistanceTo(destination), Is.EqualTo(5));
            Assert.Throws<ArgumentException>(
                () => start.ManhattanDistanceTo(new GridCoordinate(1, 1, Floor.Ground)));
        }
    }
}
