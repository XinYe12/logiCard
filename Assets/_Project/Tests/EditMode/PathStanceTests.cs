using System.Collections.Generic;
using LogiCard.Sim;
using NUnit.Framework;

namespace LogiCard.Tests.EditMode
{
    [TestFixture]
    public sealed class OrthogonalPathfinderTests
    {
        [Test]
        public void FindPath_OrthogonalShortest_ExcludesStartIncludesDestination()
        {
            var board = new GridBoard(5, 5, new[] { Floor.Ground });
            var path = new List<GridCoordinate>();

            bool ok = OrthogonalPathfinder.TryFindPath(
                board, new GridCoordinate(0, 0), new GridCoordinate(2, 1), path);

            Assert.That(ok, Is.True);
            Assert.That(path.Count, Is.EqualTo(3));
            Assert.That(path[path.Count - 1], Is.EqualTo(new GridCoordinate(2, 1)));
            Assert.That(path.Contains(new GridCoordinate(0, 0)), Is.False);

            for (int i = 1; i < path.Count; i++)
            {
                Assert.That(path[i - 1].ManhattanDistanceTo(path[i]), Is.EqualTo(1));
            }
        }

        [Test]
        public void FindPath_BlockedTile_RoutesAroundOrFails()
        {
            var board = new GridBoard(3, 1, new[] { Floor.Ground });
            board[new GridCoordinate(1, 0)] = new Tile(false);
            var path = new List<GridCoordinate>();

            bool ok = OrthogonalPathfinder.TryFindPath(
                board, new GridCoordinate(0, 0), new GridCoordinate(2, 0), path);

            Assert.That(ok, Is.False);
            Assert.That(path, Is.Empty);
        }

        [Test]
        public void FindPath_SameTile_ReturnsEmptySuccess()
        {
            var board = new GridBoard(5, 5, new[] { Floor.Ground });
            var path = new List<GridCoordinate>();
            var tile = new GridCoordinate(1, 1);

            Assert.That(OrthogonalPathfinder.TryFindPath(board, tile, tile, path), Is.True);
            Assert.That(path, Is.Empty);
        }
    }

    [TestFixture]
    public sealed class StanceAllotmentTests
    {
        [Test]
        public void FromAllottedSeconds_SnapsToNearestBand()
        {
            const float tiles = 2f;
            const float baseSec = 1f;

            Assert.That(
                StanceAllotment.FromAllottedSeconds(tiles, baseSec, StanceAllotment.MinSeconds(tiles, baseSec)),
                Is.EqualTo(StanceType.Sprint));
            Assert.That(
                StanceAllotment.FromAllottedSeconds(tiles, baseSec, StanceAllotment.CostForTiles(tiles, baseSec, StanceType.Walk)),
                Is.EqualTo(StanceType.Walk));
            Assert.That(
                StanceAllotment.FromAllottedSeconds(tiles, baseSec, StanceAllotment.MaxSeconds(tiles, baseSec)),
                Is.EqualTo(StanceType.Crawl));
        }

        [Test]
        public void CostForTiles_MatchesTimeResourceMathMultipliers()
        {
            Assert.That(StanceAllotment.CostForTiles(3f, 1f, StanceType.Sprint), Is.EqualTo(3f));
            Assert.That(StanceAllotment.CostForTiles(3f, 1f, StanceType.Walk), Is.EqualTo(6f));
            Assert.That(StanceAllotment.CostForTiles(3f, 1f, StanceType.Crawl), Is.EqualTo(12f));
        }
    }
}
