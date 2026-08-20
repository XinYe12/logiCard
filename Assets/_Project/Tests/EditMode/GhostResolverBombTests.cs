using System.Collections.Generic;
using LogiCard.Net;
using LogiCard.Sim;
using NUnit.Framework;

namespace LogiCard.Tests.EditMode
{
    /// <summary>
    /// C36/C71 — Bomber wall-only v1 resolve rules: Attach books Time Resource and persists an
    /// attached-bomb flag with no geometry effect; Detonate transitions an attached point straight
    /// Intact→Breached (no Damaged intermediate this wave) and opens LoS/movement through it from that
    /// instant on. Resolver stays permissive (no Character-grant / already-attached / ownership
    /// re-check — HUD-side gates, same split every other verb here uses) and, like every other verb,
    /// never mutates its own <see cref="ArenaBoard"/> input directly — Resolve() stays a pure function
    /// of (board, inputs); persisting Attach/Detonate to the real board is RoundPlayback presenter
    /// follow-up, not built this wave (see TapeEventPlaybackCoverageTests).
    /// </summary>
    [TestFixture]
    public sealed class GhostResolverBombTests
    {
        private const int Attacker = 1;
        private const int Defender = 2;

        private static ArenaBoard NewBoard()
        {
            return new ArenaBoard(floors: new[] { Floor.Ground });
        }

        private static BreachPoint NewBreachPoint(ArenaBoard board, BreachState initialState = BreachState.Intact)
        {
            var point = new BreachPoint(new Segment(new PlanarPosition(0, 1), new PlanarPosition(4, 1)), initialState, "Breach #1");
            board.RegisterBreachPoint(point);
            return point;
        }

        private static ActionNode Attach(float seconds, float x, float y)
        {
            return new ActionNode(ActionVerb.BombAttach, seconds, new PlanarPosition(x, y), StanceType.Walk);
        }

        private static ActionNode Detonate(float seconds, float x, float y)
        {
            return new ActionNode(ActionVerb.BombDetonate, seconds, new PlanarPosition(x, y), StanceType.Walk);
        }

        private static ActionNode Shoot(float seconds, float x, float y)
        {
            return new ActionNode(ActionVerb.Shoot, seconds, new PlanarPosition(x, y), StanceType.Walk);
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
        public void BombAttachEmitsBombAttachedAtItsExecuteTimeAndStaysAPureFunction()
        {
            ArenaBoard board = NewBoard();
            BreachPoint point = NewBreachPoint(board);
            var resolver = new GhostResolver(board);

            PlanarPosition attachPoint = new PlanarPosition(2, 1);
            ReplayTape tape = resolver.Resolve(new[]
            {
                Input(Attacker, new PlanarPosition(2, 0), Attach(3f, 2, 1)),
            });

            List<TapeEvent> attaches = EventsOfType(tape, TapeEventType.BombAttached);
            Assert.That(attaches.Count, Is.EqualTo(1));
            Assert.That(attaches[0].PawnId, Is.EqualTo(Attacker));
            Assert.That(attaches[0].Seconds, Is.EqualTo(3f).Within(0.0001f));
            Assert.That(attaches[0].Position, Is.EqualTo(attachPoint));

            Assert.That(board.HasAttachedBomb(point), Is.False,
                "Resolve() must never mutate its own board input — persistence is a presenter's job.");
        }

        [Test]
        public void BombDetonateOnAttachedBombEmitsGeometryBreachedAndStaysAPureFunction()
        {
            ArenaBoard board = NewBoard();
            BreachPoint point = NewBreachPoint(board);
            board.SetAttachedBomb(point, true); // simulates a bomb attached in a prior round
            var resolver = new GhostResolver(board);

            ReplayTape tape = resolver.Resolve(new[]
            {
                Input(Attacker, new PlanarPosition(2, 0), Detonate(3f, 2, 1)),
            });

            List<TapeEvent> breaches = EventsOfType(tape, TapeEventType.GeometryBreached);
            Assert.That(breaches.Count, Is.EqualTo(1));
            Assert.That(breaches[0].Seconds, Is.EqualTo(3f).Within(0.0001f));

            Assert.That(board.GetBreachState(point), Is.EqualTo(BreachState.Intact),
                "Resolve() must never mutate its own board input — persistence is a presenter's job.");
            Assert.That(board.HasAttachedBomb(point), Is.True,
                "Resolve() must never mutate its own board input — persistence is a presenter's job.");
        }

        [Test]
        public void BombDetonateOnAnUnattachedPointIsANoOp()
        {
            ArenaBoard board = NewBoard();
            NewBreachPoint(board);
            var resolver = new GhostResolver(board);

            ReplayTape tape = resolver.Resolve(new[]
            {
                Input(Attacker, new PlanarPosition(2, 0), Detonate(3f, 2, 1)),
            });

            Assert.That(EventsOfType(tape, TapeEventType.GeometryBreached), Is.Empty,
                "Detonating an unattached point must be a silent no-op — resolver stays permissive.");
        }

        [Test]
        public void ShotThroughAnIntactBreachPointDoesNotWound()
        {
            ArenaBoard board = NewBoard();
            NewBreachPoint(board);
            var resolver = new GhostResolver(board);

            ReplayTape tape = resolver.Resolve(new[]
            {
                Input(Attacker, new PlanarPosition(0, 0), Shoot(2f, 0, 2)),
                Input(Defender, new PlanarPosition(0, 2)),
            });

            Assert.That(EventsOfType(tape, TapeEventType.ShootFire).Count, Is.EqualTo(1));
            Assert.That(EventsOfType(tape, TapeEventType.Wounded), Is.Empty,
                "An Intact breach point must block LoS exactly like a wall.");
        }

        /// <summary>The key integration case: a same-round detonation opens LoS for shots that
        /// complete after it, without disturbing shots that already resolved before it.</summary>
        [Test]
        public void ShotAfterDetonationPassesThroughButAnEarlierShotStaysBlocked()
        {
            ArenaBoard board = NewBoard();
            BreachPoint point = NewBreachPoint(board);
            board.SetAttachedBomb(point, true); // carried in from a prior round's Attach
            var resolver = new GhostResolver(board);

            ReplayTape tape = resolver.Resolve(new[]
            {
                // Attacker: an early shot (blocked), a detonate mid-round, a late shot (passes through).
                // Detonate's Position must be the breach point's segment midpoint (2,1) — the lookup
                // key, unrelated to where the shots geometrically cross the same wall at x=0.
                Input(Attacker, new PlanarPosition(0, 0), Shoot(1f, 0, 2), Detonate(3f, 2, 1), Shoot(5f, 0, 2)),
                Input(Defender, new PlanarPosition(0, 2)),
            });

            Assert.That(EventsOfType(tape, TapeEventType.GeometryBreached).Count, Is.EqualTo(1));

            List<TapeEvent> wounds = EventsOfType(tape, TapeEventType.Wounded);
            Assert.That(wounds.Count, Is.EqualTo(1), "Exactly one of the two shots should have connected.");
            Assert.That(wounds[0].Seconds, Is.EqualTo(5f).Within(0.0001f),
                "The shot after detonation must be the one that wounds, not the earlier blocked one.");
        }

        /// <summary>Same-round Attach must be visible to a later-same-round Detonate on the same
        /// point — the chronological sweep, not just prior-round carried-in state.</summary>
        [Test]
        public void AttachThenDetonateInTheSameRoundBreachesTheWall()
        {
            ArenaBoard board = NewBoard();
            NewBreachPoint(board);
            var resolver = new GhostResolver(board);

            ReplayTape tape = resolver.Resolve(new[]
            {
                Input(Attacker, new PlanarPosition(2, 0), Attach(2f, 2, 1), Detonate(4f, 2, 1)),
            });

            Assert.That(EventsOfType(tape, TapeEventType.BombAttached).Count, Is.EqualTo(1));
            Assert.That(EventsOfType(tape, TapeEventType.GeometryBreached).Count, Is.EqualTo(1),
                "Same-round Attach must be visible to a later Detonate on the same point.");
        }
    }
}
