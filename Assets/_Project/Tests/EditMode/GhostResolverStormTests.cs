using System.Collections.Generic;
using LogiCard.Net;
using LogiCard.Sim;
using NUnit.Framework;

namespace LogiCard.Tests.EditMode
{
    /// <summary>
    /// C67 — Storm resolve rules: self-targeting, no wound/charge effect, emits one
    /// <see cref="TapeEventType.StormCast"/> per node at its ExecuteTime. Resolver stays fully
    /// permissive (no once-per-match re-check — that is a HUD-side gate per
    /// docs/contracts/CURRENT.md's Storm contract), unlike Bandage there is no charge to clamp.
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
        public void StormEmitsStormCastAtItsExecuteTimeWithNoWoundOrChargeEffect()
        {
            var resolver = new GhostResolver(NewBoard());

            ReplayTape tape = resolver.Resolve(new[]
            {
                Input(Pawn, new PlanarPosition(0, 0), Storm(4f, 0, 0)),
            });

            Assert.That(tape.WoundsFor(Pawn), Is.EqualTo(0));
            Assert.That(tape.BandageChargeFor(Pawn), Is.EqualTo(0));

            List<TapeEvent> cast = EventsOfType(tape, TapeEventType.StormCast);
            Assert.That(cast.Count, Is.EqualTo(1));
            Assert.That(cast[0].PawnId, Is.EqualTo(Pawn));
            Assert.That(cast[0].Seconds, Is.EqualTo(4f).Within(0.0001f));
        }
    }
}
