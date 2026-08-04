using System.Collections.Generic;
using LogiCard.Sim;
using NUnit.Framework;

namespace LogiCard.Tests.EditMode
{
    [TestFixture]
    public sealed class ContinuousPathfinderTests
    {
        [Test]
        public void FindPath_SameStartAndGoal_ReturnsEmptySuccess()
        {
            var board = new ArenaBoard();
            var path = new List<PlanarPosition>();
            var point = new PlanarPosition(1f, 1f);

            Assert.That(ContinuousPathfinder.TryFindPath(board, point, point, path), Is.True);
            Assert.That(path, Is.Empty);
        }

        [Test]
        public void FindPath_ClearBoard_ReturnsDirectSingleWaypoint()
        {
            var board = new ArenaBoard();
            var path = new List<PlanarPosition>();
            var from = new PlanarPosition(0f, 0f);
            var to = new PlanarPosition(3f, 2f);

            bool ok = ContinuousPathfinder.TryFindPath(board, from, to, path);

            Assert.That(ok, Is.True);
            Assert.That(path.Count, Is.EqualTo(1));
            Assert.That(path[0], Is.EqualTo(to));
        }

        [Test]
        public void FindPath_WallWithGap_RoutesAroundTheWallRatherThanFailing()
        {
            var board = new ArenaBoard();
            board.RegisterWall(new Segment(new PlanarPosition(2f, 0f), new PlanarPosition(2f, 2.5f)));
            var path = new List<PlanarPosition>();
            var from = new PlanarPosition(1f, 1f);
            var to = new PlanarPosition(3f, 1f);

            bool ok = ContinuousPathfinder.TryFindPath(board, from, to, path);

            Assert.That(ok, Is.True);
            Assert.That(path, Is.Not.Empty);
            Assert.That(path[path.Count - 1], Is.EqualTo(to));
            Assert.That(path.Count, Is.GreaterThan(1));
            for (int i = 0; i < path.Count - 1; i++)
            {
                Assert.That(path[i].Y, Is.GreaterThan(2.4f));
            }

            PlanarPosition previous = from;
            foreach (PlanarPosition waypoint in path)
            {
                Assert.That(board.IsBlocking(new Segment(previous, waypoint)), Is.False);
                previous = waypoint;
            }
        }

        [Test]
        public void FindPath_FullHeightWallWithNoGap_FailsGracefully()
        {
            var board = new ArenaBoard();
            board.RegisterWall(new Segment(new PlanarPosition(2f, 0f), new PlanarPosition(2f, 4f)));
            var path = new List<PlanarPosition>();
            var from = new PlanarPosition(1f, 2f);
            var to = new PlanarPosition(3f, 2f);

            bool ok = ContinuousPathfinder.TryFindPath(board, from, to, path);

            Assert.That(ok, Is.False);
            Assert.That(path, Is.Empty);
        }

        [Test]
        public void FindPath_ClosedDoorBlocksButOpenDoorLetsTheDirectRouteThrough()
        {
            var board = new ArenaBoard();
            var doorSegment = new Segment(new PlanarPosition(2f, 0f), new PlanarPosition(2f, 4f));
            var door = new Door(doorSegment, DoorState.Closed);
            board.RegisterDoor(door);
            var path = new List<PlanarPosition>();
            var from = new PlanarPosition(1f, 2f);
            var to = new PlanarPosition(3f, 2f);

            bool blockedWhileClosed = ContinuousPathfinder.TryFindPath(board, from, to, path);

            board.SetDoorState(door, DoorState.Open);
            bool okWhileOpen = ContinuousPathfinder.TryFindPath(board, from, to, path);

            Assert.That(blockedWhileClosed, Is.False);
            Assert.That(okWhileOpen, Is.True);
            Assert.That(path.Count, Is.EqualTo(1));
            Assert.That(path[0], Is.EqualTo(to));
        }

        [Test]
        public void FindPath_DifferentFloors_Rejected()
        {
            var board = new ArenaBoard();
            var path = new List<PlanarPosition>();

            bool ok = ContinuousPathfinder.TryFindPath(
                board,
                new PlanarPosition(1f, 1f, Floor.Ground),
                new PlanarPosition(1f, 1f, Floor.Attic),
                path);

            Assert.That(ok, Is.False);
            Assert.That(path, Is.Empty);
        }

        [Test]
        public void FindPath_OutOfBoundsEndpoint_Rejected()
        {
            var board = new ArenaBoard();
            var path = new List<PlanarPosition>();

            bool ok = ContinuousPathfinder.TryFindPath(
                board, new PlanarPosition(0f, 0f), new PlanarPosition(9f, 0f), path);

            Assert.That(ok, Is.False);
            Assert.That(path, Is.Empty);
        }

        [Test]
        public void FindPath_NullOutputBuffer_ReturnsFalseWithoutThrowing()
        {
            var board = new ArenaBoard();

            bool ok = ContinuousPathfinder.TryFindPath(
                board, new PlanarPosition(0f, 0f), new PlanarPosition(1f, 1f), null);

            Assert.That(ok, Is.False);
        }
    }
}
