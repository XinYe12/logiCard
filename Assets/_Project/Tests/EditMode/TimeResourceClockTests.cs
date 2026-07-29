using System;
using LogiCard.Timeline;
using NUnit.Framework;

namespace LogiCard.Tests.EditMode
{
    [TestFixture]
    public sealed class TimeResourceClockTests
    {
        private const float Tolerance = 0.0001f;

        [Test]
        public void DefaultClock_UsesSixtySecondPlaceholderAndStartsAtZero()
        {
            var clock = new TimeResourceClock();

            Assert.That(clock.Budget, Is.EqualTo(60f).Within(Tolerance));
            Assert.That(clock.Current, Is.EqualTo(0f).Within(Tolerance));
        }

        [Test]
        public void Constructor_AcceptsZeroBudget()
        {
            var clock = new TimeResourceClock(0f);

            Assert.That(clock.Budget, Is.EqualTo(0f).Within(Tolerance));
            Assert.That(clock.Current, Is.EqualTo(0f).Within(Tolerance));
        }

        [Test]
        public void Constructor_RejectsNegativeNaNAndInfiniteBudgets()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new TimeResourceClock(-1f));
            Assert.Throws<ArgumentOutOfRangeException>(() => new TimeResourceClock(float.NaN));
            Assert.Throws<ArgumentOutOfRangeException>(() => new TimeResourceClock(float.PositiveInfinity));
        }

        [Test]
        public void Seek_SetsCurrentWithinBudget()
        {
            var clock = new TimeResourceClock(20f);

            clock.Seek(7.5f);

            Assert.That(clock.Current, Is.EqualTo(7.5f).Within(Tolerance));
        }

        [Test]
        public void Seek_ClampsAtBothEnds()
        {
            var clock = new TimeResourceClock(20f);

            clock.Seek(30f);
            Assert.That(clock.Current, Is.EqualTo(20f).Within(Tolerance));

            clock.Seek(-5f);
            Assert.That(clock.Current, Is.EqualTo(0f).Within(Tolerance));
        }

        [Test]
        public void Advance_ClampsAtBothEnds()
        {
            var clock = new TimeResourceClock(10f);

            clock.Advance(12f);
            Assert.That(clock.Current, Is.EqualTo(10f).Within(Tolerance));

            clock.Advance(-15f);
            Assert.That(clock.Current, Is.EqualTo(0f).Within(Tolerance));
        }

        [Test]
        public void Reset_ReturnsCurrentToZero()
        {
            var clock = new TimeResourceClock(10f);
            clock.Seek(8f);

            clock.Reset();

            Assert.That(clock.Current, Is.EqualTo(0f).Within(Tolerance));
        }

        [Test]
        public void NormalizedProgress_TracksCurrentWithinZeroAndOne()
        {
            var clock = new TimeResourceClock(40f);

            clock.Seek(10f);
            Assert.That(clock.NormalizedProgress, Is.EqualTo(0.25f).Within(Tolerance));

            clock.Seek(100f);
            Assert.That(clock.NormalizedProgress, Is.EqualTo(1f).Within(Tolerance));
        }

        [Test]
        public void NormalizedProgress_IsZeroForZeroBudget()
        {
            var clock = new TimeResourceClock(0f);

            clock.Advance(5f);

            Assert.That(clock.Current, Is.EqualTo(0f).Within(Tolerance));
            Assert.That(clock.NormalizedProgress, Is.EqualTo(0f).Within(Tolerance));
        }

        [Test]
        public void SeekAndAdvance_RejectNonFiniteValues()
        {
            var clock = new TimeResourceClock();

            Assert.Throws<ArgumentOutOfRangeException>(() => clock.Seek(float.NaN));
            Assert.Throws<ArgumentOutOfRangeException>(() => clock.Advance(float.NegativeInfinity));
            Assert.That(clock.Current, Is.EqualTo(0f).Within(Tolerance));
        }
    }
}
