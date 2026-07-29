using System;

namespace LogiCard.Timeline
{
    /// <summary>
    /// Tracks continuous Time Resource seconds used for planning math (C27/C28).
    /// Time Resource is separate from tunable on-screen Playback Duration and from
    /// real-world wall-clock seconds.
    /// </summary>
    public sealed class TimeResourceClock
    {
        /// <summary>
        /// The unconfirmed demo placeholder is 60 Time Resource seconds per round.
        /// </summary>
        public TimeResourceClock(float budget = 60f)
        {
            if (float.IsNaN(budget) || float.IsInfinity(budget) || budget < 0f)
            {
                throw new ArgumentOutOfRangeException(nameof(budget), budget, "Budget must be finite and non-negative.");
            }

            Budget = budget;
        }

        public float Budget { get; }

        public float Current { get; private set; }

        public float NormalizedProgress => Budget <= 0f ? 0f : Current / Budget;

        public void Seek(float seconds)
        {
            EnsureFinite(seconds, nameof(seconds));
            Current = Clamp(seconds);
        }

        public void Advance(float deltaSeconds)
        {
            EnsureFinite(deltaSeconds, nameof(deltaSeconds));
            Current = Clamp(Current + deltaSeconds);
        }

        public void Reset()
        {
            Current = 0f;
        }

        private float Clamp(float seconds)
        {
            if (seconds <= 0f)
            {
                return 0f;
            }

            if (seconds >= Budget)
            {
                return Budget;
            }

            return seconds;
        }

        private static void EnsureFinite(float value, string parameterName)
        {
            if (float.IsNaN(value) || float.IsInfinity(value))
            {
                throw new ArgumentOutOfRangeException(parameterName, value, "Time values must be finite.");
            }
        }
    }
}
