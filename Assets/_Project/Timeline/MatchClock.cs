namespace LogiCard.Timeline
{
    /// <summary>
    /// Shared match Time Resource pool and per-round Time Card allotment (C33).
    /// Pure C# so EditMode tests can drive it without a scene.
    /// </summary>
    public sealed class MatchClock
    {
        private const float Epsilon = 0.001f;

        public float PoolSeconds { get; }

        public float MinRoundSeconds { get; }

        public float RemainingSeconds { get; private set; }

        /// <summary>N committed for the current round; 0 before the first Time Card of that round.</summary>
        public float RoundAllotment { get; private set; }

        /// <summary>1-based round index. Advances in <see cref="EndRound"/>.</summary>
        public int RoundIndex { get; private set; }

        public MatchSide CurrentChooser { get; private set; }

        public bool CanFundAnotherRound => RemainingSeconds + Epsilon >= MinRoundSeconds;

        public MatchClock(float poolSeconds, float minRoundSeconds, MatchSide firstChooser = MatchSide.Attacker)
        {
            if (poolSeconds < 0f)
            {
                poolSeconds = 0f;
            }

            if (minRoundSeconds < 0f)
            {
                minRoundSeconds = 0f;
            }

            PoolSeconds = poolSeconds;
            MinRoundSeconds = minRoundSeconds;
            RemainingSeconds = poolSeconds;
            CurrentChooser = firstChooser;
            RoundIndex = 1;
            RoundAllotment = 0f;
        }

        /// <summary>
        /// Commits <paramref name="seconds"/> from the remaining pool as this round's budget.
        /// Deducts N in full — unused seconds inside the round are burned (C33).
        /// </summary>
        public bool TryPlayTimeCard(float seconds, out string rejectionReason)
        {
            if (RoundAllotment > Epsilon)
            {
                rejectionReason = "A Time Card has already been played for this round.";
                return false;
            }

            if (!CanFundAnotherRound)
            {
                rejectionReason = $"Match pool cannot fund another round ({RemainingSeconds:0.0}s left, need {MinRoundSeconds:0.0}s).";
                return false;
            }

            if (seconds + Epsilon < MinRoundSeconds)
            {
                rejectionReason = $"Time Card must be at least {MinRoundSeconds:0.0}s.";
                return false;
            }

            if (seconds > RemainingSeconds + Epsilon)
            {
                rejectionReason = $"Time Card exceeds remaining match pool ({seconds:0.0}s of {RemainingSeconds:0.0}s).";
                return false;
            }

            float clamped = seconds;
            if (clamped > RemainingSeconds)
            {
                clamped = RemainingSeconds;
            }

            RoundAllotment = clamped;
            RemainingSeconds -= clamped;
            if (RemainingSeconds < 0f)
            {
                RemainingSeconds = 0f;
            }

            rejectionReason = null;
            return true;
        }

        /// <summary>Advances to the next round and flips the Time Card chooser.</summary>
        public void EndRound()
        {
            RoundAllotment = 0f;
            RoundIndex++;
            CurrentChooser = CurrentChooser == MatchSide.Attacker ? MatchSide.Defender : MatchSide.Attacker;
        }

        /// <summary>
        /// Fresh match from the same pool/min settings (Rematch / Local Play again after Match Over).
        /// Does not change <see cref="PoolSeconds"/> or <see cref="MinRoundSeconds"/>.
        /// </summary>
        public void Reset(MatchSide firstChooser = MatchSide.Attacker)
        {
            RemainingSeconds = PoolSeconds;
            RoundAllotment = 0f;
            RoundIndex = 1;
            CurrentChooser = firstChooser;
        }
    }
}
