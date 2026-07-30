using System;
using UnityEngine;

namespace LogiCard.Timeline
{
    /// <summary>
    /// MonoBehaviour shell around the pure <see cref="PhaseStateMachine"/>. Day 11 replaces the
    /// local triggers with Fusion callbacks without changing what the HUD listens to.
    /// </summary>
    public sealed class RoundPhaseController : MonoBehaviour
    {
        [Tooltip("Program phase length in REAL-WORLD seconds (C20: 30).")]
        public float ProgramRealWorldSeconds = 30f;

        private readonly PhaseStateMachine _machine = new PhaseStateMachine();

        public RoundPhase Phase => _machine.CurrentPhase;

        public float ProgramSecondsRemaining { get; private set; }

        public event Action<RoundPhase> PhaseChanged;

        private void Awake()
        {
            ProgramSecondsRemaining = ProgramRealWorldSeconds;
            _machine.PhaseChanged += OnMachinePhaseChanged;
        }

        private void OnDestroy()
        {
            _machine.PhaseChanged -= OnMachinePhaseChanged;
        }

        /// <summary>
        /// Debug/authoring entry point. The state machine is mostly forward-only; reaching an
        /// earlier phase rewinds to Allot first. Bounded against the Aftermath→Allot cycle.
        /// MatchOver is only reachable from Aftermath via an explicit edge.
        /// </summary>
        public void GoTo(RoundPhase target)
        {
            if (_machine.CurrentPhase == target)
            {
                return;
            }

            if (target == RoundPhase.Allot)
            {
                _machine.Reset();
                return;
            }

            if (target == RoundPhase.MatchOver)
            {
                if (_machine.CurrentPhase != RoundPhase.Aftermath)
                {
                    if (!AdvanceUntil(RoundPhase.Aftermath))
                    {
                        _machine.Reset();
                        AdvanceUntil(RoundPhase.Aftermath);
                    }
                }

                _machine.TryTransitionTo(RoundPhase.MatchOver);
                return;
            }

            if (!AdvanceUntil(target))
            {
                _machine.Reset();
                AdvanceUntil(target);
            }
        }

        public void Advance() => _machine.TryAdvance();

        private bool AdvanceUntil(RoundPhase target)
        {
            // Aftermath → Allot is a cycle; never walk more steps than there are distinct phases.
            const int MaxSteps = 8;
            int steps = 0;
            while (_machine.CurrentPhase != target && steps < MaxSteps)
            {
                if (!_machine.TryAdvance())
                {
                    return false;
                }

                steps++;
            }

            return _machine.CurrentPhase == target;
        }

        private void OnMachinePhaseChanged(RoundPhase previous, RoundPhase next)
        {
            if (next == RoundPhase.Program)
            {
                ProgramSecondsRemaining = ProgramRealWorldSeconds;
            }

            PhaseChanged?.Invoke(next);
        }

        private void Update()
        {
            if (_machine.CurrentPhase != RoundPhase.Program)
            {
                return;
            }

            ProgramSecondsRemaining = Mathf.Max(0f, ProgramSecondsRemaining - Time.deltaTime);
        }
    }
}
