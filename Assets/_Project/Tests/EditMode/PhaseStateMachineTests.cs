using System;
using LogiCard.Timeline;
using NUnit.Framework;

namespace LogiCard.Tests.EditMode
{
    [TestFixture]
    public sealed class PhaseStateMachineTests
    {
        [Test]
        public void NewStateMachine_StartsInProgram()
        {
            var stateMachine = new PhaseStateMachine();

            Assert.That(stateMachine.CurrentPhase, Is.EqualTo(RoundPhase.Program));
        }

        [Test]
        public void Advance_MovesThroughRevealThenExecute()
        {
            var stateMachine = new PhaseStateMachine();

            stateMachine.Advance();
            Assert.That(stateMachine.CurrentPhase, Is.EqualTo(RoundPhase.Reveal));

            stateMachine.Advance();
            Assert.That(stateMachine.CurrentPhase, Is.EqualTo(RoundPhase.Execute));
        }

        [Test]
        public void TryAdvance_ReturnsFalseAtExecute()
        {
            var stateMachine = new PhaseStateMachine();
            stateMachine.Advance();
            stateMachine.Advance();

            Assert.That(stateMachine.TryAdvance(), Is.False);
            Assert.That(stateMachine.CurrentPhase, Is.EqualTo(RoundPhase.Execute));
        }

        [Test]
        public void Advance_ThrowsAtExecute()
        {
            var stateMachine = new PhaseStateMachine();
            stateMachine.Advance();
            stateMachine.Advance();

            Assert.Throws<InvalidOperationException>(() => stateMachine.Advance());
        }

        [Test]
        public void TransitionTo_RejectsSkippedAndBackwardPhases()
        {
            var stateMachine = new PhaseStateMachine();

            Assert.That(stateMachine.TryTransitionTo(RoundPhase.Execute), Is.False);
            Assert.Throws<InvalidOperationException>(() => stateMachine.TransitionTo(RoundPhase.Execute));

            stateMachine.Advance();

            Assert.That(stateMachine.TryTransitionTo(RoundPhase.Program), Is.False);
            Assert.That(stateMachine.CurrentPhase, Is.EqualTo(RoundPhase.Reveal));
        }

        [Test]
        public void PhaseChanged_ReportsPreviousAndNewPhases()
        {
            var stateMachine = new PhaseStateMachine();
            RoundPhase observedPrevious = RoundPhase.Execute;
            RoundPhase observedNext = RoundPhase.Program;
            int eventCount = 0;
            stateMachine.PhaseChanged += (previous, next) =>
            {
                observedPrevious = previous;
                observedNext = next;
                eventCount++;
            };

            stateMachine.Advance();

            Assert.That(eventCount, Is.EqualTo(1));
            Assert.That(observedPrevious, Is.EqualTo(RoundPhase.Program));
            Assert.That(observedNext, Is.EqualTo(RoundPhase.Reveal));
        }

        [Test]
        public void RejectedTransitionsAndProgramReset_DoNotRaiseEvent()
        {
            var stateMachine = new PhaseStateMachine();
            int eventCount = 0;
            stateMachine.PhaseChanged += (_, __) => eventCount++;

            stateMachine.Reset();
            stateMachine.TryTransitionTo(RoundPhase.Program);
            stateMachine.TryTransitionTo(RoundPhase.Execute);

            Assert.That(eventCount, Is.EqualTo(0));
        }

        [Test]
        public void Reset_ReturnsToProgramAndRaisesEventOnlyWhenChanged()
        {
            var stateMachine = new PhaseStateMachine();
            RoundPhase observedPrevious = RoundPhase.Program;
            RoundPhase observedNext = RoundPhase.Execute;
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

            Assert.That(stateMachine.CurrentPhase, Is.EqualTo(RoundPhase.Program));
            Assert.That(eventCount, Is.EqualTo(1));
            Assert.That(observedPrevious, Is.EqualTo(RoundPhase.Reveal));
            Assert.That(observedNext, Is.EqualTo(RoundPhase.Program));
        }
    }
}
