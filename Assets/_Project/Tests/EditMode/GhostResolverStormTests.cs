using System.Collections.Generic;
using LogiCard.Net;
using LogiCard.Sim;
using NUnit.Framework;

namespace LogiCard.Tests.EditMode
{
    /// <summary>
    /// C69 — Storm resolve rules: self-targeting, 1× per Character per match. Mirrors
    /// <see cref="GhostResolverBandageTests"/>'s shape exactly — Storm's once-per-match gate now lives
    /// resolver-side the same way Bandage's charge gate already did, replacing the old HUD-only
    /// "already queued this Program" dedup (per-round, not true once-per-match).
    /// </summary>
    [TestFixture]
    public sealed class GhostResolverStormTests
    {
        private const int Pawn = 1;

        private static ArenaBoard NewBoard()
        {
            return new ArenaBoard(floors: new[] { Floor.Ground });
        }

        private static ActionNode Storm(float seconds, float x, float y)
        {
            return new ActionNode(ActionVerb.Storm, seconds, new PlanarPosition(x, y), StanceType.Walk);
        }

        private static GhostInput Input(int pawnId, PlanarPosition start, int startingStormCastCount, params ActionNode[] nodes)
        {
            return new GhostInput(pawnId, start, new TimelinePayload(new List<ActionNode>(nodes)), startingWounds: 0, startingBandageCharge: 0, startingStormCastCount: startingStormCastCount);
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
        public void FirstStormNodeEmitsStormCastAndSpendsTheCharge()
        {
            var resolver = new GhostResolver(NewBoard());

            ReplayTape tape = resolver.Resolve(new[]
            {
                Input(Pawn, new PlanarPosition(0, 0), startingStormCastCount: 0, Storm(3f, 0, 0)),
            });

            Assert.That(tape.StormCastCountFor(Pawn), Is.EqualTo(1));

            List<TapeEvent> cast = EventsOfType(tape, TapeEventType.StormCast);
            Assert.That(cast.Count, Is.EqualTo(1));
            Assert.That(cast[0].PawnId, Is.EqualTo(Pawn));
            Assert.That(cast[0].Seconds, Is.EqualTo(3f).Within(0.0001f));
        }

        [Test]
        public void ASecondStormNodeInTheSameResolveIsANoOp()
        {
            var resolver = new GhostResolver(NewBoard());

            ReplayTape tape = resolver.Resolve(new[]
            {
                Input(Pawn, new PlanarPosition(0, 0), startingStormCastCount: 0, Storm(2f, 0, 0), Storm(5f, 0, 0)),
            });

            Assert.That(tape.StormCastCountFor(Pawn), Is.EqualTo(1), "Only the first Storm node should have spent the single per-match cast.");
            Assert.That(EventsOfType(tape, TapeEventType.StormCast).Count, Is.EqualTo(1));
        }

        [Test]
        public void StartingStormCastCountCarriedInFromPriorRoundsBlocksFurtherUse()
        {
            var resolver = new GhostResolver(NewBoard());

            ReplayTape tape = resolver.Resolve(new[]
            {
                Input(Pawn, new PlanarPosition(0, 0), startingStormCastCount: 1, Storm(3f, 0, 0)),
            });

            Assert.That(tape.StormCastCountFor(Pawn), Is.EqualTo(1));
            Assert.That(EventsOfType(tape, TapeEventType.StormCast).Count, Is.EqualTo(0), "Cast was already spent in an earlier round — this Storm node must be a no-op.");
        }
    }
}
