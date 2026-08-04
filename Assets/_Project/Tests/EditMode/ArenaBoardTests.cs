using LogiCard.Sim;
using NUnit.Framework;

namespace LogiCard.Tests.EditMode
{
    [TestFixture]
    public sealed class ArenaBoardTests
    {
        private static ArenaBoard NewBoard()
        {
            return new ArenaBoard();
        }

        [Test]
        public void DefaultBoard_HasZeroToFourBoundsOnBothAxes()
        {
            var board = NewBoard();

            Assert.That(board.MinX, Is.EqualTo(0f));
            Assert.That(board.MinY, Is.EqualTo(0f));
            Assert.That(board.MaxX, Is.EqualTo(4f));
            Assert.That(board.MaxY, Is.EqualTo(4f));
        }

        [Test]
        public void InBounds_RejectsPointsOutsideEitherAxis()
        {
            var board = NewBoard();

            Assert.That(board.InBounds(new PlanarPosition(0f, 0f)), Is.True);
            Assert.That(board.InBounds(new PlanarPosition(4f, 4f)), Is.True);
            Assert.That(board.InBounds(new PlanarPosition(-0.01f, 0f)), Is.False);
            Assert.That(board.InBounds(new PlanarPosition(0f, -0.01f)), Is.False);
            Assert.That(board.InBounds(new PlanarPosition(4.01f, 0f)), Is.False);
            Assert.That(board.InBounds(new PlanarPosition(0f, 4.01f)), Is.False);
        }

        [Test]
        public void RegisterWall_BlocksProbeSegmentsThatCrossIt()
        {
            var board = NewBoard();
            board.RegisterWall(new Segment(new PlanarPosition(2f, 0f), new PlanarPosition(2f, 4f)));

            bool blocked = board.IsBlocking(new Segment(new PlanarPosition(1f, 2f), new PlanarPosition(3f, 2f)));
            bool clear = board.IsBlocking(new Segment(new PlanarPosition(0.5f, 2f), new PlanarPosition(1.5f, 2f)));

            Assert.That(blocked, Is.True);
            Assert.That(clear, Is.False);
        }

        [Test]
        public void RegisterDoor_ClosedDoorBlocksButOpenDoorDoesNot()
        {
            var board = NewBoard();
            var doorSegment = new Segment(new PlanarPosition(2f, 1.5f), new PlanarPosition(2f, 2.5f));
            var door = new Door(doorSegment, DoorState.Closed);
            board.RegisterDoor(door);

            bool blockedWhileClosed = board.IsBlocking(new Segment(new PlanarPosition(1f, 2f), new PlanarPosition(3f, 2f)));

            board.SetDoorState(door, DoorState.Open);
            bool blockedWhileOpen = board.IsBlocking(new Segment(new PlanarPosition(1f, 2f), new PlanarPosition(3f, 2f)));

            Assert.That(blockedWhileClosed, Is.True);
            Assert.That(blockedWhileOpen, Is.False);
        }

        [Test]
        public void TryGetDoor_ReturnsRegisteredDoorByMidpointOrFailsWhenMissing()
        {
            var board = NewBoard();
            var doorSegment = new Segment(new PlanarPosition(2f, 1.5f), new PlanarPosition(2f, 2.5f));
            var door = new Door(doorSegment, DoorState.Open);
            board.RegisterDoor(door);
            PlanarPosition mid = PlanarPosition.Lerp(doorSegment.A, doorSegment.B, 0.5f);

            bool found = board.TryGetDoor(mid, out Door lookedUp);
            bool missing = board.TryGetDoor(new PlanarPosition(0.5f, 0.5f), out Door missingDoor);

            Assert.That(found, Is.True);
            Assert.That(lookedUp, Is.SameAs(door));
            Assert.That(missing, Is.False);
            Assert.That(missingDoor, Is.Null);
        }

        [Test]
        public void TryGetNearestDoor_FindsClosestDoorWithinRadiusAndRejectsBeyondIt()
        {
            var board = NewBoard();
            var nearSegment = new Segment(new PlanarPosition(2f, 1.5f), new PlanarPosition(2f, 2.5f));
            var farSegment = new Segment(new PlanarPosition(0f, 0f), new PlanarPosition(0f, 1f));
            var near = new Door(nearSegment, DoorState.Closed);
            var far = new Door(farSegment, DoorState.Closed);
            board.RegisterDoor(far);
            board.RegisterDoor(near);

            bool found = board.TryGetNearestDoor(new PlanarPosition(2.2f, 2f), 0.5f, out Door nearest);
            bool tooFar = board.TryGetNearestDoor(new PlanarPosition(2.2f, 2f), 0.1f, out Door _);

            Assert.That(found, Is.True);
            Assert.That(nearest, Is.SameAs(near));
            Assert.That(tooFar, Is.False);
        }

        [Test]
        public void Clone_SnapshotsDoorStateIndependently()
        {
            var board = NewBoard();
            var doorSegment = new Segment(new PlanarPosition(2f, 1.5f), new PlanarPosition(2f, 2.5f));
            var door = new Door(doorSegment, DoorState.Closed);
            board.RegisterDoor(door);
            board.RegisterWall(new Segment(new PlanarPosition(0f, 0f), new PlanarPosition(0f, 1f)));

            ArenaBoard clone = board.Clone();
            clone.SetDoorState(door, DoorState.Open);

            Assert.That(board.GetDoorState(door), Is.EqualTo(DoorState.Closed));
            Assert.That(clone.GetDoorState(door), Is.EqualTo(DoorState.Open));
            Assert.That(clone.Walls.Count, Is.EqualTo(board.Walls.Count));
        }
    }
}
