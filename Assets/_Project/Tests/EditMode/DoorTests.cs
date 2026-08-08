using System.Collections.Generic;
using LogiCard.Net;
using LogiCard.Sim;
using LogiCard.Timeline;
using NUnit.Framework;

namespace LogiCard.Tests.EditMode
{
    /// <summary>
    /// Day 7: one contextual door (GDD §4). Registration/passability on <see cref="ArenaBoard"/>,
    /// mid-resolve toggles in <see cref="GhostResolver"/>, and authoring via
    /// <see cref="PawnProgram.TryQueueDoor"/>.
    ///
    /// Retargeted onto continuous space (C35/C39 pivot, Phases 2-3) —
    /// <see cref="ArenaBoard"/>/<see cref="PlanarPosition"/> replace <c>GridBoard</c>/
    /// <c>GridCoordinate</c>, and <see cref="PawnProgram.TryQueueDoor"/> now takes a resolved
    /// <see cref="Door"/> reference plus an interaction-radius check (Decision 4) rather than a door
    /// tile plus adjacency check.
    /// </summary>
    [TestFixture]
    public sealed class DoorTests
    {
        private const int Attacker = 1;
        private const int Defender = 2;

        // Door segment perpendicular to the Attacker/Defender shooting lane below (y = 2), so its
        // midpoint — the interaction point ActionNode.Position and TryGetDoor both key on — lands at
        // (2,2), the same spot the old grid model's single DoorTile occupied.
        private static readonly Segment DoorSegment = new Segment(new PlanarPosition(2, 1), new PlanarPosition(2, 3));
        private static readonly PlanarPosition DoorPosition = PlanarPosition.Lerp(DoorSegment.A, DoorSegment.B, 0.5f);

        private static ArenaBoard NewBoardWithDoor(DoorState initialState)
        {
            var board = new ArenaBoard(floors: new[] { Floor.Ground });
            board.RegisterDoor(new Door(DoorSegment, initialState));
            return board;
        }

        private static ActionNode DoorNode(float seconds, DoorAction action)
        {
            return new ActionNode(ActionVerb.Door, seconds, DoorPosition, StanceType.Walk, doorAction: action);
        }

        private static ActionNode Shoot(float seconds, float x, float y, ShootMode mode = ShootMode.SnapShot)
        {
            return new ActionNode(ActionVerb.Shoot, seconds, new PlanarPosition(x, y), StanceType.Walk, modifier: null, mode);
        }

        private static ActionNode Move(float seconds, float x, float y, StanceType stance = StanceType.Walk)
        {
            return new ActionNode(ActionVerb.Move, seconds, new PlanarPosition(x, y), stance);
        }

        private static GhostInput Input(int pawnId, PlanarPosition start, params ActionNode[] nodes)
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

        // ---------- ArenaBoard registration ----------

        [Test]
        public void RegisterDoor_Closed_BlocksLineOfSightThroughIt()
        {
            ArenaBoard board = NewBoardWithDoor(DoorState.Closed);

            Assert.That(board.IsBlocking(new Segment(new PlanarPosition(0, 2), new PlanarPosition(4, 2))), Is.True);
            Assert.That(board.TryGetDoor(DoorPosition, out Door door), Is.True);
            Assert.That(door.InitialState, Is.EqualTo(DoorState.Closed));
        }

        [Test]
        public void RegisterDoor_Open_LeavesLineOfSightClear()
        {
            ArenaBoard board = NewBoardWithDoor(DoorState.Open);

            Assert.That(board.IsBlocking(new Segment(new PlanarPosition(0, 2), new PlanarPosition(4, 2))), Is.False);
        }

        [Test]
        public void Clone_CopiesDoorStateButMutationsDoNotLeakBack()
        {
            ArenaBoard board = NewBoardWithDoor(DoorState.Closed);

            ArenaBoard clone = board.Clone();

            Assert.That(clone.TryGetDoor(DoorPosition, out Door clonedDoor), Is.True, "Clone must start with the same doors registered.");
            Assert.That(clone.GetDoorState(clonedDoor), Is.EqualTo(DoorState.Closed));

            // Mutating the clone's door state must never leak back to the original (GhostResolver's scratch-board isolation).
            clone.SetDoorState(clonedDoor, DoorState.Open);
            Assert.That(board.GetDoorState(clonedDoor), Is.EqualTo(DoorState.Closed));
        }

        // ---------- GhostResolver mid-resolve toggles ----------

        [Test]
        public void ClosedDoor_BlocksSnapShotThroughIt()
        {
            var resolver = new GhostResolver(NewBoardWithDoor(DoorState.Closed));

            ReplayTape tape = resolver.Resolve(new[]
            {
                Input(Attacker, new PlanarPosition(0, 2), Shoot(2f, 4, 2)),
                Input(Defender, new PlanarPosition(4, 2)),
            });

            Assert.That(HasEventType(tape, TapeEventType.Wounded), Is.False);
        }

        /// <summary>
        /// Aim sits short of a closed door on the shooter's side; victim is just past it within
        /// HitRadius. Aim-only LoS would let the wound through — victim LoS must also be required.
        /// </summary>
        [Test]
        public void ClosedDoor_BlocksSnapWhoseAimIsShortOfTheDoorButVictimIsPastIt()
        {
            var resolver = new GhostResolver(NewBoardWithDoor(DoorState.Closed));

            ReplayTape tape = resolver.Resolve(new[]
            {
                Input(Attacker, new PlanarPosition(0, 2), Shoot(2f, 1.8f, 2f)),
                Input(Defender, new PlanarPosition(2.2f, 2f)),
            });

            Assert.That(HasEventType(tape, TapeEventType.Wounded), Is.False,
                "Snap must not wound a victim past a closed door just because HitRadius reaches them from an aim short of the door.");
        }

        [Test]
        public void OpeningTheDoorBeforeAShot_LetsItThrough()
        {
            var resolver = new GhostResolver(NewBoardWithDoor(DoorState.Closed));

            ReplayTape tape = resolver.Resolve(new[]
            {
                Input(Attacker, new PlanarPosition(0, 2), DoorNode(1f, DoorAction.Open), Shoot(3f, 4, 2)),
                Input(Defender, new PlanarPosition(4, 2)),
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
                Input(Attacker, new PlanarPosition(0, 2), DoorNode(1f, DoorAction.Close), Shoot(3f, 4, 2)),
                Input(Defender, new PlanarPosition(4, 2)),
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
                Input(Attacker, new PlanarPosition(0, 2), DoorNode(3.000f, DoorAction.Open), Shoot(5f, 4, 2)),
                Input(Defender, new PlanarPosition(4, 2), DoorNode(3.005f, DoorAction.Close)),
            });

            Assert.That(HasEventType(tape, TapeEventType.Wounded), Is.False, "Close must win a same-instant tie.");
        }

        // ---------- GhostResolver movement vs mid-resolve door state (2026-08-06) ----------
        // GDD.md §3.1 / CORE_LOOP.md: "Blocked path / closed door -> stop before the block." A route
        // can look clear when drafted (own board snapshot, plus this pawn's own already-queued Door
        // actions per PawnProgram's local-board fix) but the OTHER pawn's Door action is still fully
        // hidden until resolve — movement must be re-checked against what the door's state actually
        // turns out to be, not just played back as pre-baked waypoints.

        [Test]
        public void Move_CrossingADoorThatClosesBeforehand_StopsAtTheDoor()
        {
            var resolver = new GhostResolver(NewBoardWithDoor(DoorState.Open));

            ReplayTape tape = resolver.Resolve(new[]
            {
                Input(Attacker, new PlanarPosition(0, 2), Move(3f, 4, 2)),
                Input(Defender, new PlanarPosition(4, 0), DoorNode(1f, DoorAction.Close)),
            });

            Assert.That(tape.Tracks.TryGetValue(Attacker, out ScheduledPath path), Is.True);
            PlanarPosition finalPosition = path.Evaluate(path.EndSeconds);

            // The leg crosses the door (x=2, spanning the door's y=1..3) at its exact midpoint, 1.5s
            // in — after the door closes at 1s — so the pawn should stop right there, not at (4,2).
            Assert.That(finalPosition.X, Is.EqualTo(2f).Within(0.01f));
            Assert.That(finalPosition.Y, Is.EqualTo(2f).Within(0.01f));
        }

        [Test]
        public void Move_CrossingADoorThatOpensBeforehand_CompletesNormally()
        {
            var resolver = new GhostResolver(NewBoardWithDoor(DoorState.Closed));

            ReplayTape tape = resolver.Resolve(new[]
            {
                Input(Attacker, new PlanarPosition(0, 2), Move(3f, 4, 2)),
                Input(Defender, new PlanarPosition(4, 0), DoorNode(1f, DoorAction.Open)),
            });

            Assert.That(tape.Tracks.TryGetValue(Attacker, out ScheduledPath path), Is.True);
            PlanarPosition finalPosition = path.Evaluate(path.EndSeconds);

            Assert.That(finalPosition.X, Is.EqualTo(4f).Within(0.01f));
            Assert.That(finalPosition.Y, Is.EqualTo(2f).Within(0.01f));
        }

        [Test]
        public void Move_BlockedByADoor_CancelsThatPawnsLaterQueuedActions()
        {
            var resolver = new GhostResolver(NewBoardWithDoor(DoorState.Open));

            ReplayTape tape = resolver.Resolve(new[]
            {
                Input(Attacker, new PlanarPosition(0, 2), Move(3f, 4, 2), Shoot(5f, 4, 0)),
                Input(Defender, new PlanarPosition(4, 0), DoorNode(1f, DoorAction.Close)),
            });

            Assert.That(HasEventType(tape, TapeEventType.ShootFire), Is.False,
                "The Shoot queued after a blocked Move never happens — the pawn never reached where it needed to be to take it.");
        }

        // ---------- PawnProgram authoring ----------
        // Decision 4: TryQueueDoor takes an already-resolved Door reference (the input layer would
        // find it via ArenaBoard.TryGetNearestDoor before calling this) and checks InteractRadius
        // distance instead of tile adjacency; PawnProgram no longer consults the board's door
        // registry itself.

        [Test]
        public void TryQueueDoor_WithinInteractRadius_ReservesCostAndBooksNode()
        {
            var door = new Door(DoorSegment, DoorState.Closed);
            var program = new PawnProgram(new PlanarPosition(2.3f, 2f), baseSecondsPerTile: 1f, budgetSeconds: 60f,
                doorInteractBaseSeconds: 4f);

            bool ok = program.TryQueueDoor(door, DoorAction.Open, out string reason);

            Assert.That(ok, Is.True, reason);
            Assert.That(program.UsedSeconds, Is.EqualTo(4f));
            Assert.That(program.Nodes.Count, Is.EqualTo(1));
            Assert.That(program.Nodes[0].Verb, Is.EqualTo(ActionVerb.Door));
            Assert.That(program.Nodes[0].Position, Is.EqualTo(DoorPosition));
            Assert.That(program.Nodes[0].Door, Is.EqualTo(DoorAction.Open));
        }

        [Test]
        public void ScheduledDoorState_FollowsBookedTogglesWithoutMutatingLiveBoard()
        {
            ArenaBoard board = NewBoardWithDoor(DoorState.Closed);
            Assert.That(board.TryGetDoor(DoorPosition, out Door door), Is.True);
            var program = new PawnProgram(new PlanarPosition(2.3f, 2f), baseSecondsPerTile: 1f, budgetSeconds: 60f,
                board: board, doorInteractBaseSeconds: 4f);

            Assert.That(program.ScheduledDoorState(door), Is.EqualTo(DoorState.Closed));
            Assert.That(program.TryQueueDoor(door, DoorAction.Open, out _), Is.True);
            Assert.That(program.ScheduledDoorState(door), Is.EqualTo(DoorState.Open),
                "Prompt status must flip to OPEN after booking Open.");
            Assert.That(board.GetDoorState(door), Is.EqualTo(DoorState.Closed),
                "Live board must stay Closed until Aftermath.");

            Assert.That(program.TryQueueDoor(door, DoorAction.Close, out _), Is.True);
            Assert.That(program.ScheduledDoorState(door), Is.EqualTo(DoorState.Closed));
            Assert.That(board.GetDoorState(door), Is.EqualTo(DoorState.Closed));
        }

        /// <summary>
        /// Playtest 2026-08-07: after booking Open, Move drafting must path through the door even
        /// though the shared board is still Closed (resolve arm needs round-start).
        /// </summary>
        [Test]
        public void AfterBookingOpen_TryAddWaypoint_CanPathThroughDoor()
        {
            // Full-height closed door splits west/east — same shape as ContinuousPathfinderTests.
            var board = new ArenaBoard();
            var door = new Door(new Segment(new PlanarPosition(2f, 0f), new PlanarPosition(2f, 4f)), DoorState.Closed);
            board.RegisterDoor(door);
            var program = new PawnProgram(new PlanarPosition(1.5f, 2f), baseSecondsPerTile: 1f, budgetSeconds: 60f,
                board: board, doorInteractBaseSeconds: 4f);

            Assert.That(program.TryAddWaypoint(new PlanarPosition(3f, 2f), out string blockedReason), Is.False,
                blockedReason);
            Assert.That(blockedReason, Does.Contain("No route"));

            Assert.That(program.TryQueueDoor(door, DoorAction.Open, out string doorReason), Is.True, doorReason);
            Assert.That(board.GetDoorState(door), Is.EqualTo(DoorState.Closed),
                "Shared board must remain Closed for Lock In arm.");

            Assert.That(program.TryAddWaypoint(new PlanarPosition(3f, 2f), out string openReason), Is.True,
                openReason);
            Assert.That(program.HasDraft, Is.True);
        }

        [Test]
        public void TryQueueDoor_AtDoorSegment_IsLegal()
        {
            var door = new Door(DoorSegment, DoorState.Open);
            var program = new PawnProgram(DoorPosition, baseSecondsPerTile: 1f, budgetSeconds: 60f,
                doorInteractBaseSeconds: 4f);

            bool ok = program.TryQueueDoor(door, DoorAction.Close, out string reason);

            Assert.That(ok, Is.True, reason);
        }

        [Test]
        public void TryQueueDoor_TooFarFromDoor_IsRejectedAndStateUnchanged()
        {
            var door = new Door(DoorSegment, DoorState.Closed);
            var program = new PawnProgram(new PlanarPosition(0, 0), baseSecondsPerTile: 1f, budgetSeconds: 60f,
                doorInteractBaseSeconds: 4f);

            bool ok = program.TryQueueDoor(door, DoorAction.Open, out string reason);

            Assert.That(ok, Is.False);
            Assert.That(reason, Is.Not.Null.And.Not.Empty);
            Assert.That(program.UsedSeconds, Is.EqualTo(0f));
            Assert.That(program.Nodes, Is.Empty);
        }

        [Test]
        public void TryQueueDoor_NullDoor_IsRejected()
        {
            var program = new PawnProgram(DoorPosition, baseSecondsPerTile: 1f, budgetSeconds: 60f,
                doorInteractBaseSeconds: 4f);

            bool ok = program.TryQueueDoor(null, DoorAction.Open, out string reason);

            Assert.That(ok, Is.False);
            Assert.That(reason, Is.Not.Null.And.Not.Empty);
        }

        [Test]
        public void TryQueueDoor_ExceedingBudget_IsRejected()
        {
            var door = new Door(DoorSegment, DoorState.Closed);
            var program = new PawnProgram(new PlanarPosition(2.3f, 2f), baseSecondsPerTile: 1f, budgetSeconds: 2f,
                doorInteractBaseSeconds: 4f);

            bool ok = program.TryQueueDoor(door, DoorAction.Open, out string reason);

            Assert.That(ok, Is.False);
            Assert.That(program.UsedSeconds, Is.EqualTo(0f));
        }

        // ---------- draft-time self-consistency (playtest 2026-08-06) ----------
        // A player could book an Open on the shared board's Door but still couldn't draft a path
        // through it the same round, because pathfinding only ever saw the board as it was at round
        // start — with no visibility into this pawn's own not-yet-resolved Door action. PawnProgram
        // now pathfinds against a local clone with this pawn's own committed Door nodes applied.

        [Test]
        public void TryDraftPath_WithoutOwnQueuedDoorOpen_DetoursAroundTheStillClosedDoor()
        {
            ArenaBoard board = NewBoardWithDoor(DoorState.Closed);
            var program = new PawnProgram(new PlanarPosition(0f, 2f), baseSecondsPerTile: 1f, budgetSeconds: 60f,
                board: board, doorInteractBaseSeconds: 4f);

            bool ok = program.TryDraftPath(new PlanarPosition(4f, 2f), out string reason);

            Assert.That(ok, Is.True, reason);
            Assert.That(program.DraftWaypoints.Count, Is.GreaterThan(1),
                "A still-closed door forces a detour around it, not a straight line through it.");
        }

        [Test]
        public void TryDraftPath_AfterOwnQueuedDoorOpen_CrossesTheGapInOneRound()
        {
            ArenaBoard board = NewBoardWithDoor(DoorState.Closed);
            board.TryGetDoor(DoorPosition, out Door door);
            var program = new PawnProgram(new PlanarPosition(0f, 2f), baseSecondsPerTile: 1f, budgetSeconds: 60f,
                board: board, doorInteractBaseSeconds: 4f);

            // Step within interaction range of the still-closed door, then open it.
            Assert.That(program.TryQueueMove(new PlanarPosition(1.5f, 2f), out string moveReason), Is.True, moveReason);
            Assert.That(program.TryQueueDoor(door, DoorAction.Open, out string doorReason), Is.True, doorReason);

            bool ok = program.TryDraftPath(new PlanarPosition(4f, 2f), out string pathReason);

            Assert.That(ok, Is.True, pathReason);
            Assert.That(program.DraftWaypoints.Count, Is.EqualTo(1),
                "A straight shot through the now-(locally)-open door needs no detour waypoints.");
            Assert.That(program.DraftWaypoints[0], Is.EqualTo(new PlanarPosition(4f, 2f)));

            // This pawn's plan is local-only — the shared board itself never mutates from drafting;
            // that only happens for real at resolve (RoundPlayback.CommitRoundState).
            Assert.That(board.GetDoorState(door), Is.EqualTo(DoorState.Closed));
        }
    }
}
