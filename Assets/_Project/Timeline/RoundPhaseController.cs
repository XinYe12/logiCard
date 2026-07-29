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
        /// Debug/authoring entry point. The state machine is forward-only, so reaching an
        /// earlier phase rewinds to Program first.
        /// </summary>
        public void GoTo(RoundPhase target)
        {
            if (_machine.CurrentPhase == target)
            {
                return;
            }

            if (target == RoundPhase.Program)
            {
                _machine.Reset();
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
            while (_machine.CurrentPhase != target)
            {
                if (!_machine.TryAdvance())
                {
                    return false;
                }
            }

            return true;
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
