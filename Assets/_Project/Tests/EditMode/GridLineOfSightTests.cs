using System.Collections.Generic;
using LogiCard.Sim;
using NUnit.Framework;

namespace LogiCard.Tests.EditMode
{
    [TestFixture]
    public sealed class GridLineOfSightTests
    {
        private static GridBoard NewBoard()
        {
            return new GridBoard(5, 5, new[] { Floor.Ground, Floor.Attic });
        }

        [Test]
        public void ClearRowAndColumnHaveSight()
        {
            GridBoard board = NewBoard();

            Assert.That(GridLineOfSight.HasLineOfSight(board, new GridCoordinate(0, 0), new GridCoordinate(4, 0)), Is.True);
            Assert.That(GridLineOfSight.HasLineOfSight(board, new GridCoordinate(2, 0), new GridCoordinate(2, 4)), Is.True);
        }

        [Test]
        public void AdjacentTilesHaveSight()
        {
            GridBoard board = NewBoard();

            Assert.That(GridLineOfSight.HasLineOfSight(board, new GridCoordinate(1, 1), new GridCoordinate(1, 2)), Is.True);
        }

        [Test]
        public void ImpassableTileBetweenBlocksSight()
        {
            GridBoard board = NewBoard();
            board[new GridCoordinate(0, 2)] = new Tile(false);

            Assert.That(GridLineOfSight.HasLineOfSight(board, new GridCoordinate(0, 0), new GridCoordinate(0, 4)), Is.False);
        }

        /// <summary>Endpoints are excluded, so a shooter can aim at a blocked tile without self-blocking.</summary>
        [Test]
        public void ImpassableEndpointDoesNotBlockSight()
        {
            GridBoard board = NewBoard();
            board[new GridCoordinate(0, 4)] = new Tile(false);

            Assert.That(GridLineOfSight.HasLineOfSight(board, new GridCoordinate(0, 0), new GridCoordinate(0, 4)), Is.True);
        }

        [Test]
        public void DifferentFloorsHaveNoSight()
        {
            GridBoard board = NewBoard();

            Assert.That(
                GridLineOfSight.HasLineOfSight(
                    board,
                    new GridCoordinate(1, 1, Floor.Ground),
                    new GridCoordinate(1, 3, Floor.Attic)),
                Is.False);
        }

        [Test]
        public void OutOfBoundsHasNoSight()
        {
            GridBoard board = NewBoard();

            Assert.That(GridLineOfSight.HasLineOfSight(board, new GridCoordinate(0, 0), new GridCoordinate(9, 0)), Is.False);
        }

        [Test]
        public void TilesBetweenExcludesBothEndpoints()
        {
            var walked = new List<GridCoordinate>(
                GridLineOfSight.TilesBetween(new GridCoordinate(0, 0), new GridCoordinate(0, 3)));

            Assert.That(walked, Is.EqualTo(new[] { new GridCoordinate(0, 1), new GridCoordinate(0, 2) }));
        }

        [Test]
        public void TilesBetweenAdjacentTilesIsEmpty()
        {
            var walked = new List<GridCoordinate>(
                GridLineOfSight.TilesBetween(new GridCoordinate(2, 2), new GridCoordinate(2, 3)));

            Assert.That(walked, Is.Empty);
        }
        [Test]
        public void CoveredLane_IncludesAimExcludesOrigin()
        {
            var tiles = new List<GridCoordinate>();
            foreach (GridCoordinate step in GridLineOfSight.CoveredLane(
                         new GridCoordinate(0, 0), new GridCoordinate(0, 3)))
            {
                tiles.Add(step);
            }

            Assert.That(tiles, Does.Not.Contain(new GridCoordinate(0, 0)));
            Assert.That(tiles, Is.EqualTo(new[]
            {
                new GridCoordinate(0, 1),
                new GridCoordinate(0, 2),
                new GridCoordinate(0, 3),
            }));
        }
    }
}
