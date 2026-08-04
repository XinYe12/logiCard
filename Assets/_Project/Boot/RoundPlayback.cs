using System;
using System.Collections.Generic;
using LogiCard.Board;
using LogiCard.Net;
using LogiCard.Sim;
using LogiCard.Timeline;
using UnityEngine;

namespace LogiCard.Boot
{
    /// <summary>
    /// Local stand-in for the Host's authority loop (C23). At Lock In it collects every pawn's
    /// payload, resolves them into one <see cref="ReplayTape"/>, and from then on the Time Resource
    /// scrubber simply reads that tape. Between rounds it carries positions and wounds (C33).
    /// </summary>
    public sealed class RoundPlayback : MonoBehaviour
    {
        /// <summary>How long a tracer stays lit after its shot, in Time Resource seconds.</summary>
        private const float TracerVisibleSeconds = 2.5f;

        private static readonly Color TracerColor = new Color(1f, 0.85f, 0.45f, 1f);

        private readonly List<PawnEntry> _pawns = new List<PawnEntry>();
        private readonly List<GhostInput> _inputs = new List<GhostInput>();
        private readonly List<TracerEntry> _tracers = new List<TracerEntry>();

        private BoardView _board;
        private TimeResourceClockDriver _clock;
        private RoundPhaseController _phase;
        private GhostResolver _resolver;
        private ReplayTape _tape;
        private int _eventCursor;
        private float _lastAppliedSeconds;
        private bool _anyoneDead;

        /// <summary>Stub outcome text for the HUD; empty string means "clear the banner".</summary>
        public event Action<string> OutcomeReported;

        public ReplayTape Tape => _tape;

        public bool AnyoneDead => _anyoneDead;

        public void Init(BoardView board, TimeResourceClockDriver clock, RoundPhaseController phase)
        {
            _board = board;
            _clock = clock;
            _phase = phase;
            _resolver = new GhostResolver(board.Model);

            _clock.TimeChanged += ApplyTime;
            _phase.PhaseChanged += OnPhaseChanged;
        }

        private void OnDestroy()
        {
            if (_clock != null)
            {
                _clock.TimeChanged -= ApplyTime;
            }

            if (_phase != null)
            {
                _phase.PhaseChanged -= OnPhaseChanged;
            }
        }

        public void Register(int pawnId, PawnView view, PlanarPosition home, Func<TimelinePayload> payloadSource)
        {
            _pawns.Add(new PawnEntry(pawnId, view, home, payloadSource));
        }

        public PlanarPosition PositionOf(int pawnId)
        {
            for (int i = 0; i < _pawns.Count; i++)
            {
                if (_pawns[i].PawnId == pawnId)
                {
                    return _pawns[i].CurrentPosition;
                }
            }

            return default;
        }

        public int WoundsOf(int pawnId)
        {
            for (int i = 0; i < _pawns.Count; i++)
            {
                if (_pawns[i].PawnId == pawnId)
                {
                    return _pawns[i].Wounds;
                }
            }

            return 0;
        }

        /// <summary>Lock In: freeze every program into one tape and arm playback from second zero.</summary>
        public void ResolveAndArm()
        {
            _inputs.Clear();
            for (int i = 0; i < _pawns.Count; i++)
            {
                PawnEntry pawn = _pawns[i];
                _inputs.Add(new GhostInput(pawn.PawnId, pawn.CurrentPosition, pawn.BuildPayload(), pawn.Wounds));
            }

            _tape = _resolver.Resolve(_inputs);
            _eventCursor = 0;
            _lastAppliedSeconds = 0f;
            _anyoneDead = _tape.AnyoneDead();

            for (int i = 0; i < _pawns.Count; i++)
            {
                if (_tape.Tracks.TryGetValue(_pawns[i].PawnId, out ScheduledPath track))
                {
                    _pawns[i].View.SetPath(track);
                }
            }

            BuildTracers();
            _board.RefreshDoorVisuals();

            Debug.Log($"[logiCard] Ghost resolve: {_tape.Events.Count} event(s), " +
                      $"{_tape.Tracks.Count} pawn(s), tape ends at {_tape.EndSeconds:0.0}s TR.");
            foreach (TapeEvent tapeEvent in _tape.Events)
            {
                Debug.Log($"[logiCard]   {tapeEvent}");
            }

            ApplyTime(0f);
        }

        /// <summary>
        /// Snapshot end-of-round positions and wounds from the armed tape so the next Program
        /// starts where this round left off (C33). Safe to call with no tape (no-op).
        /// </summary>
        public void CommitRoundState()
        {
            if (_tape == null)
            {
                return;
            }

            for (int i = 0; i < _pawns.Count; i++)
            {
                PawnEntry pawn = _pawns[i];
                if (_tape.Tracks.TryGetValue(pawn.PawnId, out ScheduledPath track) && track != null)
                {
                    // Continuous carry — no grid snap (C35/C39 Phase 4 correctness fix).
                    pawn.CurrentPosition = track.Evaluate(track.EndSeconds);
                }

                pawn.Wounds = _tape.WoundsFor(pawn.PawnId);
                _pawns[i] = pawn;
            }

            _board.RefreshDoorVisuals();
        }

        /// <summary>Back to Allot/Program: drop the tape and stand everyone on their carried point.</summary>
        public void Disarm()
        {
            _tape = null;
            _eventCursor = 0;
            _lastAppliedSeconds = 0f;
            ClearTracers();
            OutcomeReported?.Invoke(string.Empty);

            for (int i = 0; i < _pawns.Count; i++)
            {
                PawnEntry pawn = _pawns[i];
                pawn.View.SetPath(ScheduledPath.FromWaypoints(new[] { pawn.CurrentPosition }, 1f, StanceType.Walk));
                pawn.View.ApplyTime(0f);
            }
        }

        private void OnPhaseChanged(RoundPhase phase)
        {
            if (phase == RoundPhase.Allot || phase == RoundPhase.Program)
            {
                Disarm();
            }
            else if (phase == RoundPhase.Aftermath)
            {
                CommitRoundState();
            }
        }

        private void ApplyTime(float seconds)
        {
            for (int i = 0; i < _pawns.Count; i++)
            {
                _pawns[i].View.ApplyTime(seconds);
            }

            if (_tape == null)
            {
                _lastAppliedSeconds = seconds;
                return;
            }

            UpdateTracers(seconds);

            if (seconds < _lastAppliedSeconds)
            {
                _eventCursor = 0;
                while (_eventCursor < _tape.Events.Count && _tape.Events[_eventCursor].Seconds <= seconds)
                {
                    _eventCursor++;
                }

                OutcomeReported?.Invoke(string.Empty);
                _lastAppliedSeconds = seconds;
                return;
            }

            while (_eventCursor < _tape.Events.Count && _tape.Events[_eventCursor].Seconds <= seconds)
            {
                Report(_tape.Events[_eventCursor]);
                _eventCursor++;
            }

            _lastAppliedSeconds = seconds;
        }

        private void Report(TapeEvent tapeEvent)
        {
            switch (tapeEvent.Type)
            {
                case TapeEventType.Wounded:
                    OutcomeReported?.Invoke($"WOUNDED  P{tapeEvent.PawnId}  @{tapeEvent.Seconds:0.0}s");
                    break;
                case TapeEventType.Killed:
                    if (_tape != null && CountDeadOnTape() >= 2)
                    {
                        OutcomeReported?.Invoke($"DRAW  mutual kill @{tapeEvent.Seconds:0.0}s");
                    }
                    else
                    {
                        OutcomeReported?.Invoke($"DOWN  P{tapeEvent.PawnId}  @{tapeEvent.Seconds:0.0}s");
                    }

                    break;
            }
        }

        private int CountDeadOnTape()
        {
            int dead = 0;
            for (int i = 0; i < _pawns.Count; i++)
            {
                if (_tape.WoundsFor(_pawns[i].PawnId) >= GhostResolver.WoundsUntilDead)
                {
                    dead++;
                }
            }

            return dead;
        }

        private void BuildTracers()
        {
            ClearTracers();

            foreach (TapeEvent tapeEvent in _tape.Events)
            {
                if (tapeEvent.Type != TapeEventType.ShootFire)
                {
                    continue;
                }

                if (!_tape.Tracks.TryGetValue(tapeEvent.PawnId, out ScheduledPath shooter))
                {
                    continue;
                }

                var go = new GameObject($"Tracer_{tapeEvent.PawnId}_{tapeEvent.Seconds:0.00}");
                go.transform.SetParent(transform, false);
                var tracer = go.AddComponent<ShotTracerView>();
                tracer.Init(TracerColor);
                tracer.Aim(
                    _board.WorldFromPlanar(shooter.Evaluate(tapeEvent.Seconds)),
                    _board.WorldFromPlanar(tapeEvent.Position));

                _tracers.Add(new TracerEntry(tapeEvent.Seconds, tracer));
            }
        }

        private void UpdateTracers(float seconds)
        {
            for (int i = 0; i < _tracers.Count; i++)
            {
                TracerEntry tracer = _tracers[i];
                bool lit = seconds >= tracer.Seconds && seconds <= tracer.Seconds + TracerVisibleSeconds;
                tracer.View.SetVisible(lit);
            }
        }

        private void ClearTracers()
        {
            for (int i = 0; i < _tracers.Count; i++)
            {
                if (_tracers[i].View != null)
                {
                    Destroy(_tracers[i].View.gameObject);
                }
            }

            _tracers.Clear();
        }

        private struct PawnEntry
        {
            private readonly Func<TimelinePayload> _payloadSource;

            public int PawnId { get; }

            public PawnView View { get; }

            public PlanarPosition CurrentPosition { get; set; }

            public int Wounds { get; set; }

            public PawnEntry(int pawnId, PawnView view, PlanarPosition home, Func<TimelinePayload> payloadSource)
            {
                PawnId = pawnId;
                View = view;
                CurrentPosition = home;
                Wounds = 0;
                _payloadSource = payloadSource;
            }

            public TimelinePayload BuildPayload()
            {
                return _payloadSource != null ? _payloadSource() : null;
            }
        }

        private readonly struct TracerEntry
        {
            public float Seconds { get; }

            public ShotTracerView View { get; }

            public TracerEntry(float seconds, ShotTracerView view)
            {
                Seconds = seconds;
                View = view;
            }
        }
    }
}
