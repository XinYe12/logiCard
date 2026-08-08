using System.Collections.Generic;
using LogiCard.Net;
using LogiCard.Sim;
using NUnit.Framework;

namespace LogiCard.Tests.EditMode
{
    /// <summary>
    /// Day 4 resolve rules: a locked payload becomes ghost movement, line-of-sight checks and
    /// wound outcomes, with no engine or randomness involved. Retargeted onto continuous space
    /// (C35/C39 pivot) — <see cref="ArenaBoard"/>/<see cref="PlanarPosition"/> replace
    /// <c>GridBoard</c>/<c>GridCoordinate</c>.
    /// </summary>
    [TestFixture]
    public sealed class GhostResolverTests
    {
        private const int Attacker = 1;
        private const int Defender = 2;

        private static ArenaBoard NewBoard()
        {
            return new ArenaBoard(floors: new[] { Floor.Ground });
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

        private static List<TapeEvent> EventsOfType(ReplayTape tape, TapeEventType type)
        {
            var found = new List<TapeEvent>();
            foreach (TapeEvent tapeEvent in tape.Events)
            {
                if (tapeEvent.Type == type)
                {
                    found.Add(tapeEvent);
                }
            }

            return found;
        }

        [Test]
        public void MoveArrivesOnItsScheduledPositionAtItsExecuteTime()
        {
            var resolver = new GhostResolver(NewBoard());

            ReplayTape tape = resolver.Resolve(new[]
            {
                Input(Attacker, new PlanarPosition(0, 0), Move(4f, 0, 2)),
            });

            ScheduledPath track = tape.Tracks[Attacker];
            Assert.That(track.Evaluate(0f), Is.EqualTo(new PlanarPosition(0, 0)));
            Assert.That(track.Evaluate(4f), Is.EqualTo(new PlanarPosition(0, 2)));
            Assert.That(tape.EndSeconds, Is.EqualTo(4f).Within(0.0001f));

            List<TapeEvent> arrivals = EventsOfType(tape, TapeEventType.MoveArrive);
            Assert.That(arrivals.Count, Is.EqualTo(1));
            Assert.That(arrivals[0].Seconds, Is.EqualTo(4f).Within(0.0001f));
            Assert.That(arrivals[0].Position, Is.EqualTo(new PlanarPosition(0, 2)));
        }

        /// <summary>
        /// The bug the preview path has and the resolver must not: a Shoot spends Time Resource
        /// without covering distance, so the following Move may not start early.
        /// </summary>
        [Test]
        public void ShootBetweenMovesHoldsPositionThroughItsWindow()
        {
            var resolver = new GhostResolver(NewBoard());

            ReplayTape tape = resolver.Resolve(new[]
            {
                Input(Attacker, new PlanarPosition(0, 0), Move(2f, 0, 1), Shoot(4f, 3, 1), Move(6f, 0, 2)),
            });

            ScheduledPath track = tape.Tracks[Attacker];
            Assert.That(track.Evaluate(2f).Y, Is.EqualTo(1f).Within(0.0001f));
            Assert.That(track.Evaluate(3f).Y, Is.EqualTo(1f).Within(0.0001f), "Pawn drifted during its shoot window.");
            Assert.That(track.Evaluate(4f).Y, Is.EqualTo(1f).Within(0.0001f));
            Assert.That(track.Evaluate(6f).Y, Is.EqualTo(2f).Within(0.0001f));
        }

        [Test]
        public void ShotOnTheAimedTileWoundsItsOccupant()
        {
            var resolver = new GhostResolver(NewBoard());

            ReplayTape tape = resolver.Resolve(new[]
            {
                Input(Attacker, new PlanarPosition(0, 0), Shoot(2f, 0, 2)),
                Input(Defender, new PlanarPosition(0, 2)),
            });

            List<TapeEvent> wounds = EventsOfType(tape, TapeEventType.Wounded);
            Assert.That(wounds.Count, Is.EqualTo(1));
            Assert.That(wounds[0].PawnId, Is.EqualTo(Defender));
            Assert.That(wounds[0].TargetPawnId, Is.EqualTo(Attacker));
            Assert.That(wounds[0].Seconds, Is.EqualTo(2f).Within(0.0001f));
            Assert.That(EventsOfType(tape, TapeEventType.ShootFire).Count, Is.EqualTo(1));
        }

        [Test]
        public void ShotAtAnEmptyTileFiresWithoutWounding()
        {
            var resolver = new GhostResolver(NewBoard());

            ReplayTape tape = resolver.Resolve(new[]
            {
                Input(Attacker, new PlanarPosition(0, 0), Shoot(2f, 0, 4)),
                Input(Defender, new PlanarPosition(0, 2)),
            });

            Assert.That(EventsOfType(tape, TapeEventType.ShootFire).Count, Is.EqualTo(1));
            Assert.That(EventsOfType(tape, TapeEventType.Wounded), Is.Empty);
        }

        [Test]
        public void ShotThroughAWallDoesNotWound()
        {
            ArenaBoard board = NewBoard();
            board.RegisterWall(new Segment(new PlanarPosition(0, 1), new PlanarPosition(4, 1)));
            var resolver = new GhostResolver(board);

            ReplayTape tape = resolver.Resolve(new[]
            {
                Input(Attacker, new PlanarPosition(0, 0), Shoot(2f, 0, 2)),
                Input(Defender, new PlanarPosition(0, 2)),
            });

            Assert.That(EventsOfType(tape, TapeEventType.ShootFire).Count, Is.EqualTo(1));
            Assert.That(EventsOfType(tape, TapeEventType.Wounded), Is.Empty);
        }

        /// <summary>
        /// BUG FOUND 2026-08-07 (playtest): the tracer drew a straight line to the aim point even
        /// when the shot was blocked, so the beam visually passed through the wall even though it
        /// never actually wounded anyone. ShootFire's Position must stop at the wall.
        /// </summary>
        [Test]
        public void ShotThroughAWallTruncatesItsTracerAtTheWall()
        {
            ArenaBoard board = NewBoard();
            board.RegisterWall(new Segment(new PlanarPosition(0, 1), new PlanarPosition(4, 1)));
            var resolver = new GhostResolver(board);

            ReplayTape tape = resolver.Resolve(new[]
            {
                Input(Attacker, new PlanarPosition(0, 0), Shoot(2f, 0, 2)),
            });

            List<TapeEvent> fires = EventsOfType(tape, TapeEventType.ShootFire);
            Assert.That(fires.Count, Is.EqualTo(1));
            Assert.That(fires[0].Position, Is.EqualTo(new PlanarPosition(0, 1)));
        }

        [Test]
        public void ShotThroughAClosedDoorTruncatesItsTracerAtTheDoor()
        {
            ArenaBoard board = NewBoard();
            var door = new Door(new Segment(new PlanarPosition(0, 1), new PlanarPosition(4, 1)), DoorState.Closed);
            board.RegisterDoor(door);
            var resolver = new GhostResolver(board);

            ReplayTape tape = resolver.Resolve(new[]
            {
                Input(Attacker, new PlanarPosition(0, 0), Shoot(2f, 0, 2)),
            });

            List<TapeEvent> fires = EventsOfType(tape, TapeEventType.ShootFire);
            Assert.That(fires.Count, Is.EqualTo(1));
            Assert.That(fires[0].Position, Is.EqualTo(new PlanarPosition(0, 1)));
        }

        [Test]
        public void UnblockedShotTracerReachesTheAimPoint()
        {
            var resolver = new GhostResolver(NewBoard());

            ReplayTape tape = resolver.Resolve(new[]
            {
                Input(Attacker, new PlanarPosition(0, 0), Shoot(2f, 0, 4)),
            });

            List<TapeEvent> fires = EventsOfType(tape, TapeEventType.ShootFire);
            Assert.That(fires.Count, Is.EqualTo(1));
            Assert.That(fires[0].Position, Is.EqualTo(new PlanarPosition(0, 4)));
        }

        [Test]
        public void ShotMissesATargetThatHasAlreadyMovedOn()
        {
            var resolver = new GhostResolver(NewBoard());

            ReplayTape tape = resolver.Resolve(new[]
            {
                Input(Attacker, new PlanarPosition(0, 0), Shoot(2f, 0, 2)),
                Input(Defender, new PlanarPosition(0, 2), Move(1f, 3, 2)),
            });

            Assert.That(EventsOfType(tape, TapeEventType.Wounded), Is.Empty);
        }

        /// <summary>Trading Snap Shots on the same second wounds both sides (paper D5 §IV).</summary>
        [Test]
        public void SimultaneousShotsWoundBothSides()
        {
            var resolver = new GhostResolver(NewBoard());

            ReplayTape tape = resolver.Resolve(new[]
            {
                Input(Attacker, new PlanarPosition(0, 0), Shoot(2f, 0, 2)),
                Input(Defender, new PlanarPosition(0, 2), Shoot(2f, 0, 0)),
            });

            List<TapeEvent> wounds = EventsOfType(tape, TapeEventType.Wounded);
            Assert.That(wounds.Count, Is.EqualTo(2));
            Assert.That(EventsOfType(tape, TapeEventType.Killed), Is.Empty);
        }

        [Test]
        public void SecondWoundOnTheSamePawnBecomesKilled()
        {
            var resolver = new GhostResolver(NewBoard());

            ReplayTape tape = resolver.Resolve(new[]
            {
                Input(Attacker, new PlanarPosition(0, 0), Shoot(2f, 0, 2), Shoot(4f, 0, 2)),
                Input(Defender, new PlanarPosition(0, 2)),
            });

            Assert.That(EventsOfType(tape, TapeEventType.Wounded).Count, Is.EqualTo(1));
            List<TapeEvent> killed = EventsOfType(tape, TapeEventType.Killed);
            Assert.That(killed.Count, Is.EqualTo(1));
            Assert.That(killed[0].Seconds, Is.EqualTo(4f).Within(0.0001f));
        }

        [Test]
        public void EventsAreOrderedByTimeResourceSecond()
        {
            var resolver = new GhostResolver(NewBoard());

            ReplayTape tape = resolver.Resolve(new[]
            {
                Input(Attacker, new PlanarPosition(0, 0), Move(2f, 0, 1), Shoot(4f, 0, 3), Move(6f, 0, 2)),
                Input(Defender, new PlanarPosition(0, 3), Shoot(4f, 0, 1)),
            });

            for (int i = 1; i < tape.Events.Count; i++)
            {
                Assert.That(tape.Events[i].Seconds, Is.GreaterThanOrEqualTo(tape.Events[i - 1].Seconds));
            }
        }

        [Test]
        public void ResolvingTheSameInputsTwiceProducesTheSameTape()
        {
            GhostInput[] inputs =
            {
                Input(Attacker, new PlanarPosition(0, 0), Move(2f, 0, 1), Shoot(4f, 0, 3)),
                Input(Defender, new PlanarPosition(0, 3), Shoot(4f, 0, 1)),
            };

            ReplayTape first = new GhostResolver(NewBoard()).Resolve(inputs);
            ReplayTape second = new GhostResolver(NewBoard()).Resolve(inputs);

            Assert.That(second.Events.Count, Is.EqualTo(first.Events.Count));
            for (int i = 0; i < first.Events.Count; i++)
            {
                Assert.That(second.Events[i].ToString(), Is.EqualTo(first.Events[i].ToString()));
            }
        }

        [Test]
        public void SnapShotMissesASprintingTargetOnTheAimedTile()
        {
            var resolver = new GhostResolver(NewBoard());

            ReplayTape tape = resolver.Resolve(new[]
            {
                Input(Attacker, new PlanarPosition(0, 0), Shoot(2f, 0, 2)),
                Input(Defender, new PlanarPosition(0, 1), Move(1f, 0, 2, StanceType.Sprint)),
            });

            Assert.That(EventsOfType(tape, TapeEventType.ShootFire).Count, Is.EqualTo(1));
            Assert.That(EventsOfType(tape, TapeEventType.Wounded), Is.Empty);
            Assert.That(EventsOfType(tape, TapeEventType.Killed), Is.Empty);
        }

        [Test]
        public void HoldAngleKillsASprintingTargetOnTheAimedTile()
        {
            var resolver = new GhostResolver(NewBoard());

            ReplayTape tape = resolver.Resolve(new[]
            {
                Input(Attacker, new PlanarPosition(0, 0), Shoot(3f, 0, 2, ShootMode.HoldAngle)),
                Input(Defender, new PlanarPosition(0, 1), Move(1f, 0, 2, StanceType.Sprint)),
            });

            List<TapeEvent> killed = EventsOfType(tape, TapeEventType.Killed);
            Assert.That(killed.Count, Is.EqualTo(1));
            Assert.That(killed[0].PawnId, Is.EqualTo(Defender));
            Assert.That(tape.WoundsFor(Defender), Is.EqualTo(GhostResolver.WoundsUntilDead));
        }

        [Test]
        public void HoldAngleShootFireCarriesWindowStartSoPlaybackCanLitTracerBeforeContact()
        {
            // BUG FOUND 2026-08-06: ShootFire was stamped only at CompleteSeconds, so a mid-window
            // Hold kill could land on the tape before any tracer existed. WindowStartSeconds lets
            // RoundPlayback keep the same ShotTracerView lit across the whole hold.
            var resolver = new GhostResolver(NewBoard());

            ReplayTape tape = resolver.Resolve(new[]
            {
                Input(Attacker, new PlanarPosition(0, 0), Shoot(3f, 0, 4, ShootMode.HoldAngle)),
                Input(Defender, new PlanarPosition(0, 2)),
            });

            List<TapeEvent> fires = EventsOfType(tape, TapeEventType.ShootFire);
            Assert.That(fires.Count, Is.EqualTo(1));
            Assert.That(fires[0].Seconds, Is.EqualTo(3f).Within(0.0001f));
            Assert.That(fires[0].WindowStartSeconds, Is.EqualTo(0f).Within(0.0001f));

            List<TapeEvent> killed = EventsOfType(tape, TapeEventType.Killed);
            Assert.That(killed.Count, Is.EqualTo(1));
            Assert.That(killed[0].Seconds, Is.LessThan(fires[0].Seconds),
                "Contact must be allowed before CompleteSeconds — that is why the tracer needs WindowStart.");
            Assert.That(killed[0].Seconds, Is.GreaterThanOrEqualTo(fires[0].WindowStartSeconds));
        }

        [Test]
        public void HoldAngleCoversMidLaneNotOnlyTheAimedTile()
        {
            var resolver = new GhostResolver(NewBoard());

            // Aim past the defender; Snap would miss, Hold covers (0,2) on the way to (0,4).
            ReplayTape snapTape = resolver.Resolve(new[]
            {
                Input(Attacker, new PlanarPosition(0, 0), Shoot(2f, 0, 4)),
                Input(Defender, new PlanarPosition(0, 2)),
            });
            Assert.That(EventsOfType(snapTape, TapeEventType.Wounded), Is.Empty);

            ReplayTape holdTape = resolver.Resolve(new[]
            {
                Input(Attacker, new PlanarPosition(0, 0), Shoot(3f, 0, 4, ShootMode.HoldAngle)),
                Input(Defender, new PlanarPosition(0, 2)),
            });
            Assert.That(EventsOfType(holdTape, TapeEventType.Killed).Count, Is.EqualTo(1));
        }

        [Test]
        public void MutualHoldAngleOnTheSameSecondKillsBoth()
        {
            var resolver = new GhostResolver(NewBoard());

            ReplayTape tape = resolver.Resolve(new[]
            {
                Input(Attacker, new PlanarPosition(0, 0), Shoot(3f, 0, 2, ShootMode.HoldAngle)),
                Input(Defender, new PlanarPosition(0, 2), Shoot(3f, 0, 0, ShootMode.HoldAngle)),
            });

            List<TapeEvent> killed = EventsOfType(tape, TapeEventType.Killed);
            Assert.That(killed.Count, Is.EqualTo(2));
            Assert.That(tape.AnyoneDead(), Is.True);
            Assert.That(tape.WoundsFor(Attacker), Is.EqualTo(GhostResolver.WoundsUntilDead));
            Assert.That(tape.WoundsFor(Defender), Is.EqualTo(GhostResolver.WoundsUntilDead));
        }
    }
}
