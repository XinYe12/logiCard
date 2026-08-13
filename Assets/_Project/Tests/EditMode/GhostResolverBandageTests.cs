using System.Collections.Generic;
using LogiCard.Net;
using LogiCard.Sim;
using NUnit.Framework;

namespace LogiCard.Tests.EditMode
{
    /// <summary>
    /// C63 — Bandage resolve rules: self-targeting, clears one Wounded stack, 1 charge per
    /// Character per match. Resolver stays permissive (no Sprint/already-Healthy re-check —
    /// those are HUD-side gates per docs/contracts/CURRENT.md's Bandage contract); these tests
    /// only cover what the resolver itself must guarantee.
    /// </summary>
    [TestFixture]
    public sealed class GhostResolverBandageTests
    {
        private const int Pawn = 1;

        private static ArenaBoard NewBoard()
        {
            return new ArenaBoard(floors: new[] { Floor.Ground });
        }

        private static ActionNode Bandage(float seconds, float x, float y)
        {
            return new ActionNode(ActionVerb.Bandage, seconds, new PlanarPosition(x, y), StanceType.Walk);
        }

        private static GhostInput Input(int pawnId, PlanarPosition start, int startingWounds, int startingBandageCharge, params ActionNode[] nodes)
        {
            return new GhostInput(pawnId, start, new TimelinePayload(new List<ActionNode>(nodes)), startingWounds, startingBandageCharge);
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
        public void BandageClearsOneWoundedStackAndEmitsHealedAtItsExecuteTime()
        {
            var resolver = new GhostResolver(NewBoard());

            ReplayTape tape = resolver.Resolve(new[]
            {
                Input(Pawn, new PlanarPosition(0, 0), startingWounds: 1, startingBandageCharge: 0, Bandage(3f, 0, 0)),
            });

            Assert.That(tape.WoundsFor(Pawn), Is.EqualTo(0));
            Assert.That(tape.BandageChargeFor(Pawn), Is.EqualTo(1));

            List<TapeEvent> healed = EventsOfType(tape, TapeEventType.Healed);
            Assert.That(healed.Count, Is.EqualTo(1));
            Assert.That(healed[0].PawnId, Is.EqualTo(Pawn));
            Assert.That(healed[0].Seconds, Is.EqualTo(3f).Within(0.0001f));
        }

        [Test]
        public void BandageOnAnAlreadyHealthyPawnClampsAtZeroButStillSpendsTheCharge()
        {
            var resolver = new GhostResolver(NewBoard());

            ReplayTape tape = resolver.Resolve(new[]
            {
                Input(Pawn, new PlanarPosition(0, 0), startingWounds: 0, startingBandageCharge: 0, Bandage(3f, 0, 0)),
            });

            Assert.That(tape.WoundsFor(Pawn), Is.EqualTo(0));
            Assert.That(tape.BandageChargeFor(Pawn), Is.EqualTo(1), "Resolver stays permissive — HUD is responsible for not offering this in the first place.");
        }

        [Test]
        public void ASecondBandageNodeInTheSameResolveIsANoOp()
        {
            var resolver = new GhostResolver(NewBoard());

            ReplayTape tape = resolver.Resolve(new[]
            {
                Input(Pawn, new PlanarPosition(0, 0), startingWounds: 1, startingBandageCharge: 0, Bandage(2f, 0, 0), Bandage(5f, 0, 0)),
            });

            Assert.That(tape.WoundsFor(Pawn), Is.EqualTo(0), "Only the first Bandage node should have consumed the single charge.");
            Assert.That(tape.BandageChargeFor(Pawn), Is.EqualTo(1));
            Assert.That(EventsOfType(tape, TapeEventType.Healed).Count, Is.EqualTo(1));
        }

        [Test]
        public void StartingBandageChargeCarriedInFromPriorRoundsBlocksFurtherUse()
        {
            var resolver = new GhostResolver(NewBoard());

            ReplayTape tape = resolver.Resolve(new[]
            {
                Input(Pawn, new PlanarPosition(0, 0), startingWounds: 1, startingBandageCharge: 1, Bandage(3f, 0, 0)),
            });

            Assert.That(tape.WoundsFor(Pawn), Is.EqualTo(1), "Charge was already spent in an earlier round — this Bandage node must be a no-op.");
            Assert.That(tape.BandageChargeFor(Pawn), Is.EqualTo(1));
            Assert.That(EventsOfType(tape, TapeEventType.Healed).Count, Is.EqualTo(0));
        }
    }
}
