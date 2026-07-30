using System;
using LogiCard.Timeline;
using NUnit.Framework;

namespace LogiCard.Tests.EditMode
{
    [TestFixture]
    public sealed class PhaseStateMachineTests
    {
        [Test]
        public void NewStateMachine_StartsInAllot()
        {
            var stateMachine = new PhaseStateMachine();

            Assert.That(stateMachine.CurrentPhase, Is.EqualTo(RoundPhase.Allot));
        }

        [Test]
        public void Advance_MovesThroughProgramRevealExecuteAftermath()
        {
            var stateMachine = new PhaseStateMachine();

            stateMachine.Advance();
            Assert.That(stateMachine.CurrentPhase, Is.EqualTo(RoundPhase.Program));

            stateMachine.Advance();
            Assert.That(stateMachine.CurrentPhase, Is.EqualTo(RoundPhase.Reveal));

            stateMachine.Advance();
            Assert.That(stateMachine.CurrentPhase, Is.EqualTo(RoundPhase.Execute));

            stateMachine.Advance();
            Assert.That(stateMachine.CurrentPhase, Is.EqualTo(RoundPhase.Aftermath));
        }

        [Test]
        public void TryAdvance_FromAftermath_ReturnsToAllot()
        {
            var stateMachine = new PhaseStateMachine();
            AdvanceTo(stateMachine, RoundPhase.Aftermath);

            Assert.That(stateMachine.TryAdvance(), Is.True);
            Assert.That(stateMachine.CurrentPhase, Is.EqualTo(RoundPhase.Allot));
        }

        [Test]
        public void TryAdvance_ReturnsFalseAtMatchOver()
        {
            var stateMachine = new PhaseStateMachine();
            AdvanceTo(stateMachine, RoundPhase.Aftermath);
            Assert.That(stateMachine.TryTransitionTo(RoundPhase.MatchOver), Is.True);

            Assert.That(stateMachine.TryAdvance(), Is.False);
            Assert.That(stateMachine.CurrentPhase, Is.EqualTo(RoundPhase.MatchOver));
            Assert.Throws<InvalidOperationException>(() => stateMachine.Advance());
        }

        [Test]
        public void TransitionTo_RejectsSkippedAndBackwardPhases()
        {
            var stateMachine = new PhaseStateMachine();

            Assert.That(stateMachine.TryTransitionTo(RoundPhase.Execute), Is.False);
            Assert.Throws<InvalidOperationException>(() => stateMachine.TransitionTo(RoundPhase.Execute));

            stateMachine.Advance();

            Assert.That(stateMachine.TryTransitionTo(RoundPhase.Allot), Is.False);
            Assert.That(stateMachine.CurrentPhase, Is.EqualTo(RoundPhase.Program));
        }

        [Test]
        public void Aftermath_CanTransitionToMatchOver()
        {
            var stateMachine = new PhaseStateMachine();
            AdvanceTo(stateMachine, RoundPhase.Aftermath);

            Assert.That(stateMachine.TryTransitionTo(RoundPhase.MatchOver), Is.True);
            Assert.That(stateMachine.CurrentPhase, Is.EqualTo(RoundPhase.MatchOver));
        }

        [Test]
        public void PhaseChanged_ReportsPreviousAndNewPhases()
        {
            var stateMachine = new PhaseStateMachine();
            RoundPhase observedPrevious = RoundPhase.MatchOver;
            RoundPhase observedNext = RoundPhase.Allot;
            int eventCount = 0;
            stateMachine.PhaseChanged += (previous, next) =>
            {
                observedPrevious = previous;
                observedNext = next;
                eventCount++;
            };

            stateMachine.Advance();

            Assert.That(eventCount, Is.EqualTo(1));
            Assert.That(observedPrevious, Is.EqualTo(RoundPhase.Allot));
            Assert.That(observedNext, Is.EqualTo(RoundPhase.Program));
        }

        [Test]
        public void RejectedTransitionsAndAllotReset_DoNotRaiseEvent()
        {
            var stateMachine = new PhaseStateMachine();
            int eventCount = 0;
            stateMachine.PhaseChanged += (_, __) => eventCount++;

            stateMachine.Reset();
            stateMachine.TryTransitionTo(RoundPhase.Allot);
            stateMachine.TryTransitionTo(RoundPhase.Execute);

            Assert.That(eventCount, Is.EqualTo(0));
        }

        [Test]
        public void Reset_ReturnsToAllotAndRaisesEventOnlyWhenChanged()
        {
            var stateMachine = new PhaseStateMachine();
            RoundPhase observedPrevious = RoundPhase.Allot;
            RoundPhase observedNext = RoundPhase.MatchOver;
            int eventCount = 0;
            stateMachine.PhaseChanged += (previous, next) =>
            {
                observedPrevious = previous;
                observedNext = next;
                eventCount++;
            };
            stateMachine.Advance();
            eventCount = 0;

            stateMachine.Reset();
            stateMachine.Reset();

            Assert.That(stateMachine.CurrentPhase, Is.EqualTo(RoundPhase.Allot));
            Assert.That(eventCount, Is.EqualTo(1));
            Assert.That(observedPrevious, Is.EqualTo(RoundPhase.Program));
            Assert.That(observedNext, Is.EqualTo(RoundPhase.Allot));
        }

        private static void AdvanceTo(PhaseStateMachine stateMachine, RoundPhase target)
        {
            int guard = 0;
            while (stateMachine.CurrentPhase != target && guard++ < 8)
            {
                stateMachine.Advance();
            }

            Assert.That(stateMachine.CurrentPhase, Is.EqualTo(target));
        }
    }
}
