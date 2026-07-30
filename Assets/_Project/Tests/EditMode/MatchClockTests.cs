using LogiCard.Timeline;
using NUnit.Framework;

namespace LogiCard.Tests.EditMode
{
    [TestFixture]
    public sealed class MatchClockTests
    {
        [Test]
        public void NewClock_StartsWithFullPoolAndAttackerChooser()
        {
            var clock = new MatchClock(900f, 30f);

            Assert.That(clock.RemainingSeconds, Is.EqualTo(900f).Within(0.0001f));
            Assert.That(clock.RoundAllotment, Is.EqualTo(0f));
            Assert.That(clock.RoundIndex, Is.EqualTo(1));
            Assert.That(clock.CurrentChooser, Is.EqualTo(MatchSide.Attacker));
            Assert.That(clock.CanFundAnotherRound, Is.True);
        }

        [Test]
        public void TryPlayTimeCard_DeductsInFull()
        {
            var clock = new MatchClock(900f, 30f);

            Assert.That(clock.TryPlayTimeCard(60f, out string reason), Is.True);
            Assert.That(reason, Is.Null);
            Assert.That(clock.RoundAllotment, Is.EqualTo(60f).Within(0.0001f));
            Assert.That(clock.RemainingSeconds, Is.EqualTo(840f).Within(0.0001f));
        }

        [Test]
        public void TryPlayTimeCard_RejectsBelowMinRound()
        {
            var clock = new MatchClock(900f, 30f);

            Assert.That(clock.TryPlayTimeCard(10f, out string reason), Is.False);
            Assert.That(reason, Does.Contain("at least"));
            Assert.That(clock.RemainingSeconds, Is.EqualTo(900f).Within(0.0001f));
        }

        [Test]
        public void TryPlayTimeCard_RejectsAboveRemaining()
        {
            var clock = new MatchClock(50f, 30f);

            Assert.That(clock.TryPlayTimeCard(60f, out string reason), Is.False);
            Assert.That(reason, Does.Contain("exceeds"));
        }

        [Test]
        public void TryPlayTimeCard_RejectsSecondCardSameRound()
        {
            var clock = new MatchClock(900f, 30f);
            Assert.That(clock.TryPlayTimeCard(60f, out _), Is.True);

            Assert.That(clock.TryPlayTimeCard(30f, out string reason), Is.False);
            Assert.That(reason, Does.Contain("already"));
        }

        [Test]
        public void AllIn_EmptiesPool()
        {
            var clock = new MatchClock(120f, 30f);

            Assert.That(clock.TryPlayTimeCard(120f, out _), Is.True);
            Assert.That(clock.RemainingSeconds, Is.EqualTo(0f).Within(0.0001f));
            Assert.That(clock.CanFundAnotherRound, Is.False);
        }

        [Test]
        public void EndRound_FlipsChooserAndClearsAllotment()
        {
            var clock = new MatchClock(900f, 30f);
            Assert.That(clock.TryPlayTimeCard(60f, out _), Is.True);

            clock.EndRound();

            Assert.That(clock.RoundAllotment, Is.EqualTo(0f));
            Assert.That(clock.RoundIndex, Is.EqualTo(2));
            Assert.That(clock.CurrentChooser, Is.EqualTo(MatchSide.Defender));
            Assert.That(clock.RemainingSeconds, Is.EqualTo(840f).Within(0.0001f));
        }

        [Test]
        public void EndRound_AlternatesChooserEachRound()
        {
            var clock = new MatchClock(900f, 30f);
            Assert.That(clock.TryPlayTimeCard(30f, out _), Is.True);
            clock.EndRound();
            Assert.That(clock.TryPlayTimeCard(30f, out _), Is.True);
            clock.EndRound();

            Assert.That(clock.CurrentChooser, Is.EqualTo(MatchSide.Attacker));
            Assert.That(clock.RoundIndex, Is.EqualTo(3));
        }

        [Test]
        public void CanFundAnotherRound_FalseWhenBelowMin()
        {
            var clock = new MatchClock(40f, 30f);
            Assert.That(clock.TryPlayTimeCard(30f, out _), Is.True);

            Assert.That(clock.RemainingSeconds, Is.EqualTo(10f).Within(0.0001f));
            Assert.That(clock.CanFundAnotherRound, Is.False);
        }
    }
}
