using System;
using UnityEngine;

namespace LogiCard.Timeline
{
    /// <summary>
    /// MonoBehaviour shell around the pure <see cref="TimeResourceClock"/>.
    /// Owns Playback Duration (C27): how fast real-world seconds scrub Time Resource.
    /// Day 11 can keep this shell and swap what drives <see cref="Seek"/> without
    /// changing what the HUD listens to.
    /// </summary>
    public sealed class TimeResourceClockDriver : MonoBehaviour
    {
        [Tooltip("Time Resource budget in continuous seconds (C20 placeholder until confirmed).")]
        public float BudgetSeconds = 60f;

        [Tooltip("Real-world seconds to play the full Time Resource budget (Playback Duration, C27).")]
        public float PlaybackSecondsForFullBudget = 8f;

        private TimeResourceClock _clock;
        private bool _isPlaying;

        public float CurrentSeconds => _clock != null ? _clock.Current : 0f;

        public float Normalized => _clock != null ? _clock.NormalizedProgress : 0f;

        public bool IsPlaying => _isPlaying;

        public event Action<float> TimeChanged;

        private void Awake()
        {
            EnsureClock();
        }

        public void Play()
        {
            EnsureClock();
            _isPlaying = true;
        }

        public void Pause()
        {
            _isPlaying = false;
        }

        public void Rewind()
        {
            EnsureClock();
            _isPlaying = false;
            _clock.Reset();
            RaiseTimeChanged();
        }

        public void SetSeconds(float seconds)
        {
            EnsureClock();
            _clock.Seek(seconds);
            RaiseTimeChanged();
        }

        public void SetNormalized(float normalized)
        {
            EnsureClock();
            float clamped = Mathf.Clamp01(normalized);
            _clock.Seek(clamped * _clock.Budget);
            RaiseTimeChanged();
        }

        private void Update()
        {
            if (!_isPlaying || _clock == null)
            {
                return;
            }

            if (PlaybackSecondsForFullBudget <= 0f || _clock.Budget <= 0f)
            {
                _clock.Seek(_clock.Budget);
                _isPlaying = false;
                RaiseTimeChanged();
                return;
            }

            float deltaTimeResource = Time.deltaTime * (_clock.Budget / PlaybackSecondsForFullBudget);
            _clock.Advance(deltaTimeResource);
            RaiseTimeChanged();

            if (_clock.Current >= _clock.Budget)
            {
                _isPlaying = false;
            }
        }

        private void EnsureClock()
        {
            float budget = BudgetSeconds < 0f ? 0f : BudgetSeconds;
            if (_clock != null && Mathf.Approximately(_clock.Budget, budget))
            {
                return;
            }

            // Rebuild when BudgetSeconds is assigned after AddComponent (Awake ran with defaults).
            float previous = _clock != null ? _clock.Current : 0f;
            _clock = new TimeResourceClock(budget);
            _clock.Seek(previous);
        }

        private void RaiseTimeChanged()
        {
            TimeChanged?.Invoke(CurrentSeconds);
        }

        private void OnValidate()
        {
            if (BudgetSeconds < 0f)
            {
                BudgetSeconds = 0f;
            }

            if (PlaybackSecondsForFullBudget < 0f)
            {
                PlaybackSecondsForFullBudget = 0f;
            }

            // Rebuild when the Inspector budget changes in edit mode / after domain reload.
            if (_clock != null && !Mathf.Approximately(_clock.Budget, BudgetSeconds))
            {
                float previous = _clock.Current;
                _clock = new TimeResourceClock(BudgetSeconds < 0f ? 0f : BudgetSeconds);
                _clock.Seek(previous);
            }
        }
    }
}
