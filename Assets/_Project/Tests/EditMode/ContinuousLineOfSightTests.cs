using LogiCard.Sim;
using NUnit.Framework;

namespace LogiCard.Tests.EditMode
{
    [TestFixture]
    public sealed class ContinuousLineOfSightTests
    {
        [Test]
        public void ClearLineHasSight()
        {
            var board = new ArenaBoard();

            Assert.That(
                ContinuousLineOfSight.HasLineOfSight(board, new PlanarPosition(0f, 0f), new PlanarPosition(4f, 0f)),
                Is.True);
            Assert.That(
                ContinuousLineOfSight.HasLineOfSight(board, new PlanarPosition(2f, 0f), new PlanarPosition(2f, 4f)),
                Is.True);
        }

        [Test]
        public void WallBetweenEndpointsBlocksSight()
        {
            var board = new ArenaBoard();
            board.RegisterWall(new Segment(new PlanarPosition(2f, 0f), new PlanarPosition(2f, 4f)));

            Assert.That(
                ContinuousLineOfSight.HasLineOfSight(board, new PlanarPosition(0f, 2f), new PlanarPosition(4f, 2f)),
                Is.False);
        }

        /// <summary>
        /// C41: a shared endpoint / corner graze counts as intersecting, so a wall that only
        /// touches the shooter's standing point still blocks that ray.
        /// </summary>
        [Test]
        public void WallTouchingAtEndpointBlocksSight()
        {
            var board = new ArenaBoard();
            board.RegisterWall(new Segment(new PlanarPosition(0f, 0f), new PlanarPosition(0f, 2f)));

            Assert.That(
                ContinuousLineOfSight.HasLineOfSight(board, new PlanarPosition(0f, 0f), new PlanarPosition(3f, 0f)),
                Is.False);
        }

        [Test]
        public void ClosedDoorBlocksButOpenDoorDoesNot()
        {
            var board = new ArenaBoard();
            var doorSegment = new Segment(new PlanarPosition(2f, 1.5f), new PlanarPosition(2f, 2.5f));
            var door = new Door(doorSegment, DoorState.Closed);
            board.RegisterDoor(door);

            Assert.That(
                ContinuousLineOfSight.HasLineOfSight(board, new PlanarPosition(1f, 2f), new PlanarPosition(3f, 2f)),
                Is.False);

            board.SetDoorState(door, DoorState.Open);

            Assert.That(
                ContinuousLineOfSight.HasLineOfSight(board, new PlanarPosition(1f, 2f), new PlanarPosition(3f, 2f)),
                Is.True);
        }

        [Test]
        public void DifferentFloorsNeverSeeEachOther()
        {
            var board = new ArenaBoard();

            Assert.That(
                ContinuousLineOfSight.HasLineOfSight(
                    board,
                    new PlanarPosition(1f, 1f, Floor.Ground),
                    new PlanarPosition(1f, 3f, Floor.Attic)),
                Is.False);
        }

        [Test]
        public void OutOfBoundsEndpointHasNoSight()
        {
            var board = new ArenaBoard();

            Assert.That(
                ContinuousLineOfSight.HasLineOfSight(board, new PlanarPosition(0f, 0f), new PlanarPosition(9f, 0f)),
                Is.False);
        }

        [Test]
        public void IsOnLane_PointWithinHalfWidthAndWithinSpanIsOnLane()
        {
            var origin = new PlanarPosition(0f, 0f);
            var aim = new PlanarPosition(4f, 0f);

            Assert.That(ContinuousLineOfSight.IsOnLane(origin, aim, new PlanarPosition(2f, 0.2f), 0.5f), Is.True);
            Assert.That(ContinuousLineOfSight.IsOnLane(origin, aim, new PlanarPosition(4f, 0f), 0.5f), Is.True);
        }

        [Test]
        public void IsOnLane_PointBeyondHalfWidthIsOffLane()
        {
            var origin = new PlanarPosition(0f, 0f);
            var aim = new PlanarPosition(4f, 0f);

            Assert.That(ContinuousLineOfSight.IsOnLane(origin, aim, new PlanarPosition(2f, 1f), 0.5f), Is.False);
        }

        [Test]
        public void IsOnLane_PointBeyondEitherEndOfSegmentIsOffLaneDespiteBeingOnTheInfiniteLine()
        {
            var origin = new PlanarPosition(0f, 0f);
            var aim = new PlanarPosition(4f, 0f);

            Assert.That(ContinuousLineOfSight.IsOnLane(origin, aim, new PlanarPosition(5f, 0f), 0.5f), Is.False);
            Assert.That(ContinuousLineOfSight.IsOnLane(origin, aim, new PlanarPosition(-1f, 0f), 0.5f), Is.False);
            Assert.That(ContinuousLineOfSight.IsOnLane(origin, aim, origin, 0.5f), Is.False);
        }
    }
}
