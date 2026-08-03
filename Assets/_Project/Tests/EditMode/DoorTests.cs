using System.Collections.Generic;
using LogiCard.Net;
using LogiCard.Sim;
using LogiCard.Timeline;
using NUnit.Framework;

namespace LogiCard.Tests.EditMode
{
    /// <summary>
    /// Day 7: one contextual door (GDD §4). Registration/passability on <see cref="GridBoard"/>,
    /// mid-resolve toggles in <see cref="GhostResolver"/>, and authoring via
    /// <see cref="PawnProgram.TryQueueDoor"/>.
    /// </summary>
    [TestFixture]
    public sealed class DoorTests
    {
        private const int Attacker = 1;
        private const int Defender = 2;
        private static readonly GridCoordinate DoorTile = new GridCoordinate(2, 2);

        private static GridBoard NewBoardWithDoor(DoorState initialState)
        {
            var board = new GridBoard(5, 5, new[] { Floor.Ground });
            board.RegisterDoor(new Door(DoorTile, initialState));
            return board;
        }

        private static ActionNode DoorNode(float seconds, DoorAction action)
        {
            return new ActionNode(ActionVerb.Door, seconds, DoorTile, StanceType.Walk, doorAction: action);
        }

        private static ActionNode Shoot(float seconds, int x, int y, ShootMode mode = ShootMode.SnapShot)
        {
            return new ActionNode(ActionVerb.Shoot, seconds, new GridCoordinate(x, y), StanceType.Walk, modifier: null, mode);
        }

        private static GhostInput Input(int pawnId, GridCoordinate start, params ActionNode[] nodes)
        {
            return new GhostInput(pawnId, start, new TimelinePayload(new List<ActionNode>(nodes)));
        }

        private static bool HasEventType(ReplayTape tape, TapeEventType type)
        {
            foreach (TapeEvent tapeEvent in tape.Events)
            {
                if (tapeEvent.Type == type)
                {
                    return true;
                }
            }

            return false;
        }

        // ---------- GridBoard registration ----------

        [Test]
        public void RegisterDoor_Closed_MakesTileImpassable()
        {
            GridBoard board = NewBoardWithDoor(DoorState.Closed);

            Assert.That(board.GetTile(DoorTile).IsPassable, Is.False);
            Assert.That(board.TryGetDoor(DoorTile, out Door door), Is.True);
            Assert.That(door.InitialState, Is.EqualTo(DoorState.Closed));
        }

        [Test]
        public void RegisterDoor_Open_LeavesTilePassable()
        {
            GridBoard board = NewBoardWithDoor(DoorState.Open);

            Assert.That(board.GetTile(DoorTile).IsPassable, Is.True);
        }

        [Test]
        public void RegisterDoor_Twice_ThrowsAtSameCoordinate()
        {
            GridBoard board = NewBoardWithDoor(DoorState.Closed);

            Assert.Throws<System.InvalidOperationException>(
                () => board.RegisterDoor(new Door(DoorTile, DoorState.Open)));
        }

        [Test]
        public void Clone_CopiesPassabilityButNotDoorRegistration()
        {
            GridBoard board = NewBoardWithDoor(DoorState.Closed);

            GridBoard clone = board.Clone();

            Assert.That(clone.GetTile(DoorTile).IsPassable, Is.False, "Clone must start with the same tile state.");
            Assert.That(clone.TryGetDoor(DoorTile, out _), Is.False, "Scratch boards only need passability, not door identity.");

            // Mutating the clone must never leak back to the original (GhostResolver's scratch-board isolation).
            clone[DoorTile] = new Tile(true);
            Assert.That(board.GetTile(DoorTile).IsPassable, Is.False);
        }

        // ---------- GhostResolver mid-resolve toggles ----------

        [Test]
        public void ClosedDoor_BlocksSnapShotThroughIt()
        {
            var resolver = new GhostResolver(NewBoardWithDoor(DoorState.Closed));

            ReplayTape tape = resolver.Resolve(new[]
            {
                Input(Attacker, new GridCoordinate(0, 2), Shoot(2f, 4, 2)),
                Input(Defender, new GridCoordinate(4, 2)),
            });

            Assert.That(HasEventType(tape, TapeEventType.Wounded), Is.False);
        }

        [Test]
        public void OpeningTheDoorBeforeAShot_LetsItThrough()
        {
            var resolver = new GhostResolver(NewBoardWithDoor(DoorState.Closed));

            ReplayTape tape = resolver.Resolve(new[]
            {
                Input(Attacker, new GridCoordinate(0, 2), DoorNode(1f, DoorAction.Open), Shoot(3f, 4, 2)),
                Input(Defender, new GridCoordinate(4, 2)),
            });

            Assert.That(HasEventType(tape, TapeEventType.DoorOpened), Is.True);
            Assert.That(HasEventType(tape, TapeEventType.Wounded), Is.True);
        }

        [Test]
        public void ClosingTheDoorBeforeAShot_BlocksIt()
        {
            var resolver = new GhostResolver(NewBoardWithDoor(DoorState.Open));

            ReplayTape tape = resolver.Resolve(new[]
            {
                Input(Attacker, new GridCoordinate(0, 2), DoorNode(1f, DoorAction.Close), Shoot(3f, 4, 2)),
                Input(Defender, new GridCoordinate(4, 2)),
            });

            Assert.That(HasEventType(tape, TapeEventType.DoorClosed), Is.True);
            Assert.That(HasEventType(tape, TapeEventType.Wounded), Is.False);
        }

        [Test]
        public void SimultaneousOpenAndClose_CloseWins()
        {
            var resolver = new GhostResolver(NewBoardWithDoor(DoorState.Open), simultaneityEpsilon: 0.01f);

            ReplayTape tape = resolver.Resolve(new[]
            {
                Input(Attacker, new GridCoordinate(0, 2), DoorNode(3.000f, DoorAction.Open), Shoot(5f, 4, 2)),
                Input(Defender, new GridCoordinate(4, 2), DoorNode(3.005f, DoorAction.Close)),
            });

            Assert.That(HasEventType(tape, TapeEventType.Wounded), Is.False, "Close must win a same-instant tie.");
        }

        // ---------- PawnProgram authoring ----------

        [Test]
        public void TryQueueDoor_FromAdjacentTile_ReservesCostAndBooksNode()
        {
            GridBoard board = NewBoardWithDoor(DoorState.Closed);
            var program = new PawnProgram(new GridCoordinate(2, 1), baseSecondsPerTile: 1f, budgetSeconds: 60f,
                board: board, doorInteractBaseSeconds: 4f);

            bool ok = program.TryQueueDoor(DoorTile, DoorAction.Open, out string reason);

            Assert.That(ok, Is.True, reason);
            Assert.That(program.UsedSeconds, Is.EqualTo(4f));
            Assert.That(program.Nodes.Count, Is.EqualTo(1));
            Assert.That(program.Nodes[0].Verb, Is.EqualTo(ActionVerb.Door));
            Assert.That(program.Nodes[0].GridPosition, Is.EqualTo(DoorTile));
            Assert.That(program.Nodes[0].Door, Is.EqualTo(DoorAction.Open));
        }

        [Test]
        public void TryQueueDoor_FromCurrentTile_IsLegal()
        {
            GridBoard board = NewBoardWithDoor(DoorState.Open);
            var program = new PawnProgram(DoorTile, baseSecondsPerTile: 1f, budgetSeconds: 60f,
                board: board, doorInteractBaseSeconds: 4f);

            bool ok = program.TryQueueDoor(DoorTile, DoorAction.Close, out string reason);

            Assert.That(ok, Is.True, reason);
        }

        [Test]
        public void TryQueueDoor_NotAdjacent_IsRejectedAndStateUnchanged()
        {
            GridBoard board = NewBoardWithDoor(DoorState.Closed);
            var program = new PawnProgram(new GridCoordinate(0, 0), baseSecondsPerTile: 1f, budgetSeconds: 60f,
                board: board, doorInteractBaseSeconds: 4f);

            bool ok = program.TryQueueDoor(DoorTile, DoorAction.Open, out string reason);

            Assert.That(ok, Is.False);
            Assert.That(reason, Is.Not.Null.And.Not.Empty);
            Assert.That(program.UsedSeconds, Is.EqualTo(0f));
            Assert.That(program.Nodes, Is.Empty);
        }

        [Test]
        public void TryQueueDoor_TileWithNoRegisteredDoor_IsRejected()
        {
            var board = new GridBoard(5, 5, new[] { Floor.Ground }); // no door registered
            var program = new PawnProgram(new GridCoordinate(1, 2), baseSecondsPerTile: 1f, budgetSeconds: 60f,
                board: board, doorInteractBaseSeconds: 4f);

            bool ok = program.TryQueueDoor(DoorTile, DoorAction.Open, out string reason);

            Assert.That(ok, Is.False);
            Assert.That(reason, Is.Not.Null.And.Not.Empty);
        }

        [Test]
        public void TryQueueDoor_ExceedingBudget_IsRejected()
        {
            GridBoard board = NewBoardWithDoor(DoorState.Closed);
            var program = new PawnProgram(new GridCoordinate(2, 1), baseSecondsPerTile: 1f, budgetSeconds: 2f,
                board: board, doorInteractBaseSeconds: 4f);

            bool ok = program.TryQueueDoor(DoorTile, DoorAction.Open, out string reason);

            Assert.That(ok, Is.False);
            Assert.That(program.UsedSeconds, Is.EqualTo(0f));
        }
    }
}
