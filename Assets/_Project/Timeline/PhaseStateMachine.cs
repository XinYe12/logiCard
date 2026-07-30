using System;

namespace LogiCard.Timeline
{
    /// <summary>
    /// Enforces the cyclic Allot → Program → Reveal → Execute → Aftermath round flow,
    /// with Aftermath → Allot (next round) or Aftermath → MatchOver (C33).
    /// </summary>
    public sealed class PhaseStateMachine
    {
        public RoundPhase CurrentPhase { get; private set; } = RoundPhase.Allot;

        public event Action<RoundPhase, RoundPhase> PhaseChanged;

        public void Advance()
        {
            if (!TryAdvance())
            {
                throw new InvalidOperationException($"Cannot advance beyond {CurrentPhase}.");
            }
        }

        public bool TryAdvance()
        {
            switch (CurrentPhase)
            {
                case RoundPhase.Allot:
                    return TryTransitionTo(RoundPhase.Program);
                case RoundPhase.Program:
                    return TryTransitionTo(RoundPhase.Reveal);
                case RoundPhase.Reveal:
                    return TryTransitionTo(RoundPhase.Execute);
                case RoundPhase.Execute:
                    return TryTransitionTo(RoundPhase.Aftermath);
                case RoundPhase.Aftermath:
                    // Prefer next round; callers that want MatchOver use TryTransitionTo explicitly.
                    return TryTransitionTo(RoundPhase.Allot);
                default:
                    return false;
            }
        }

        public void TransitionTo(RoundPhase nextPhase)
        {
            if (!TryTransitionTo(nextPhase))
            {
                throw new InvalidOperationException($"Cannot transition from {CurrentPhase} to {nextPhase}.");
            }
        }

        public bool TryTransitionTo(RoundPhase nextPhase)
        {
            if (!IsNextPhase(CurrentPhase, nextPhase))
            {
                return false;
            }

            RoundPhase previousPhase = CurrentPhase;
            CurrentPhase = nextPhase;
            PhaseChanged?.Invoke(previousPhase, CurrentPhase);
            return true;
        }

        /// <summary>
        /// Returns to Allot so a fresh Time Card can be played. No-op when already in Allot.
        /// </summary>
        public void Reset()
        {
            if (CurrentPhase == RoundPhase.Allot)
            {
                return;
            }

            RoundPhase previousPhase = CurrentPhase;
            CurrentPhase = RoundPhase.Allot;
            PhaseChanged?.Invoke(previousPhase, CurrentPhase);
        }

        private static bool IsNextPhase(RoundPhase currentPhase, RoundPhase nextPhase)
        {
            return (currentPhase == RoundPhase.Allot && nextPhase == RoundPhase.Program)
                || (currentPhase == RoundPhase.Program && nextPhase == RoundPhase.Reveal)
                || (currentPhase == RoundPhase.Reveal && nextPhase == RoundPhase.Execute)
                || (currentPhase == RoundPhase.Execute && nextPhase == RoundPhase.Aftermath)
                || (currentPhase == RoundPhase.Aftermath && nextPhase == RoundPhase.Allot)
                || (currentPhase == RoundPhase.Aftermath && nextPhase == RoundPhase.MatchOver);
        }
    }
}
