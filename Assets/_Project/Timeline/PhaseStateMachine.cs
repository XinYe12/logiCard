using System;

namespace LogiCard.Timeline
{
    /// <summary>
    /// Enforces the forward-only Program → Reveal → Execute round flow.
    /// </summary>
    public sealed class PhaseStateMachine
    {
        public RoundPhase CurrentPhase { get; private set; } = RoundPhase.Program;

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
                case RoundPhase.Program:
                    return TryTransitionTo(RoundPhase.Reveal);
                case RoundPhase.Reveal:
                    return TryTransitionTo(RoundPhase.Execute);
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

        public void Reset()
        {
            if (CurrentPhase == RoundPhase.Program)
            {
                return;
            }

            RoundPhase previousPhase = CurrentPhase;
            CurrentPhase = RoundPhase.Program;
            PhaseChanged?.Invoke(previousPhase, CurrentPhase);
        }

        private static bool IsNextPhase(RoundPhase currentPhase, RoundPhase nextPhase)
        {
            return (currentPhase == RoundPhase.Program && nextPhase == RoundPhase.Reveal)
                || (currentPhase == RoundPhase.Reveal && nextPhase == RoundPhase.Execute);
        }
    }
}
