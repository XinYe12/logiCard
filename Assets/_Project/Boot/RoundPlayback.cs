using System;
using System.Collections;
using System.Collections.Generic;
using LogiCard.Audio;
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
    ///
    /// Presentation contract: <c>docs/PLAYBACK_CONTRACT.md</c>. Continuous state (pawn pose, doors,
    /// tracers, muzzle, wound splats) is a pure function of scrubber seconds; one-shots (foley,
    /// outcome banners) advance on the forward event cursor only. Do not restart timed FX on every
    /// <see cref="ApplyTime"/> tick when tape-derived state is unchanged (door-bug class, 2026-08-12).
    /// </summary>
    public sealed class RoundPlayback : MonoBehaviour
    {
        /// <summary>How long a tracer stays lit after its shot, in Time Resource seconds.</summary>
        private const float TracerVisibleSeconds = 2.5f;

        /// <summary>
        /// How long a muzzle flash stays lit, in Time Resource seconds (ART_DIRECTION §3: "2 frames,
        /// then gone"). Short and fixed rather than converted from real playback fps — RoundPlayback
        /// only ever deals in Time Resource seconds (C27), never real/engine frames.
        /// </summary>
        private const float MuzzleFlashVisibleSeconds = 0.15f;

        private static readonly Color TracerColor = new Color(1f, 0.85f, 0.45f, 1f);

        private readonly List<PawnEntry> _pawns = new List<PawnEntry>();
        private readonly List<GhostInput> _inputs = new List<GhostInput>();
        private readonly List<TracerEntry> _tracers = new List<TracerEntry>();
        private readonly List<MuzzleFlashEntry> _muzzleFlashes = new List<MuzzleFlashEntry>();
        private readonly List<WoundSplatEntry> _woundSplats = new List<WoundSplatEntry>();
        private readonly Dictionary<Door, DoorState> _doorStateAtArm = new Dictionary<Door, DoorState>();

        private BoardView _board;
        private TimeResourceClockDriver _clock;
        private RoundPhaseController _phase;
        private IFoleyPlayer _foley;
        private IMatchResolver _matchResolver;
        private ReplayTape _tape;
        private int _eventCursor;
        private float _lastAppliedSeconds;
        private float _doorsSyncedToSeconds = float.NaN;
        private bool _anyoneDead;

        /// <summary>Stub outcome text for the HUD; empty string means "clear the banner".</summary>
        public event Action<string> OutcomeReported;

        public ReplayTape Tape => _tape;

        public bool AnyoneDead => _anyoneDead;

        /// <summary>
        /// <paramref name="matchResolver"/> defaults to a same-process <see cref="LocalMatchResolver"/>
        /// (today's behavior, unchanged) when null. A networked <see cref="IMatchResolver"/> (C52's
        /// resolve-relay) can be injected here once one exists, with no other change to this class.
        /// </summary>
        public void Init(
            BoardView board,
            TimeResourceClockDriver clock,
            RoundPhaseController phase,
            IFoleyPlayer foley = null,
            IMatchResolver matchResolver = null)
        {
            _board = board;
            _clock = clock;
            _phase = phase;
            _foley = foley;
            _matchResolver = matchResolver ?? new LocalMatchResolver(new GhostResolver(board.Model));

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

        /// <summary>
        /// Swaps the resolver used by the next <see cref="ResolveAndArm"/> — e.g. <c>GameBootstrap</c>
        /// wires this to <see cref="AppFlowController.EnteredMatch"/> so Local Play keeps
        /// <see cref="LocalMatchResolver"/> and Find Match switches to a networked resolver. Safe to
        /// call any time before the next Lock In; never mid-resolve.
        /// </summary>
        public void SetMatchResolver(IMatchResolver resolver)
        {
            _matchResolver = resolver ?? throw new ArgumentNullException(nameof(resolver));
        }

        /// <summary>
        /// Lock In: freeze every program into one tape and arm playback from second zero. Runs the
        /// resolve through <see cref="_matchResolver"/> via <see cref="ResolveAndArmRoutine"/> — with
        /// the default <see cref="LocalMatchResolver"/> this completes synchronously within this same
        /// call (it never yields before resolving), so callers observe no behavior change.
        /// </summary>
        public void ResolveAndArm()
        {
            StartCoroutine(ResolveAndArmRoutine());
        }

        private IEnumerator ResolveAndArmRoutine()
        {
            _inputs.Clear();
            for (int i = 0; i < _pawns.Count; i++)
            {
                PawnEntry pawn = _pawns[i];
                _inputs.Add(new GhostInput(pawn.PawnId, pawn.CurrentPosition, pawn.BuildPayload(), pawn.Wounds));
            }

            // A bare `yield return someEnumerator` does NOT drain it synchronously in Unity — it defers
            // to the next scheduler pump even when the inner enumerator never itself yields. Pumping it
            // manually here preserves LocalMatchResolver's "completes within this call" guarantee (its
            // MoveNext() returns false on the very first call, so this loop body never runs, and this
            // whole routine finishes inside the single StartCoroutine() call above); a resolver that
            // does yield a real wait still propagates it correctly to the outer coroutine.
            //
            // MoveNext() is called inside its own try/catch (not wrapping the yield return — C# forbids
            // yield inside a try with a catch clause) so a networked resolver's connection failure
            // reports through OutcomeReported instead of an unhandled exception freezing the round.
            ReplayTape resolved = null;
            IEnumerator resolve = _matchResolver.ResolveAsync(_inputs, tape => resolved = tape);
            while (true)
            {
                bool moved = false;
                Exception failure = null;
                try
                {
                    moved = resolve.MoveNext();
                }
                catch (Exception ex)
                {
                    failure = ex;
                }

                if (failure != null)
                {
                    Debug.LogError($"[logiCard] Match resolve failed: {failure}");
                    OutcomeReported?.Invoke($"Connection failed — {failure.Message}");
                    yield break;
                }

                if (!moved)
                {
                    break;
                }

                yield return resolve.Current;
            }

            _tape = resolved;
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
            BuildHitVfx();
            SnapshotDoorStatesAtArm();

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

            // Final authoritative apply (covers Aftermath without scrubbing to the tape end).
            SyncDoorsToSeconds(float.PositiveInfinity);
        }

        /// <summary>
        /// Snapshot live door state at Lock In so scrubbing can restore round-start (which may
        /// already differ from each door's <see cref="Door.InitialState"/> after prior rounds).
        /// </summary>
        private void SnapshotDoorStatesAtArm()
        {
            _doorStateAtArm.Clear();
            _doorsSyncedToSeconds = float.NaN;
            IReadOnlyList<Door> doors = _board.Model.Doors;
            for (int i = 0; i < doors.Count; i++)
            {
                Door door = doors[i];
                _doorStateAtArm[door] = _board.Model.GetDoorState(door);
            }
        }

        /// <summary>
        /// BUG FOUND 2026-08-06 (playtest): door toggles only ever mutated <see cref="GhostResolver"/>'s
        /// resolve-local scratch clone (by design, to keep Resolve a pure function of board+inputs) —
        /// nothing ever copied the result back onto the shared <see cref="ArenaBoard"/> that the next
        /// round's pathfinding and the door's rendered tint both read from. A player who booked and
        /// resolved an Open had it silently reset to Closed the instant the round ended; the door
        /// could never actually be gotten through.
        ///
        /// Additionally, tint used to refresh only at arm/Aftermath — during playback a Closed-looking
        /// door could show a tracer "through" it after the tape had already opened it. Syncing from the
        /// arm snapshot + tape events up to the scrubber second keeps model + tint matched while scrubbing.
        /// </summary>
        private void SyncDoorsToSeconds(float seconds)
        {
            // Scrubber can re-fire the same second; skip so we don't allocate fresh door materials
            // via RefreshDoorVisuals on every identical tick.
            if (_doorsSyncedToSeconds == seconds)
            {
                return;
            }

            foreach (KeyValuePair<Door, DoorState> pair in _doorStateAtArm)
            {
                _board.Model.SetDoorState(pair.Key, pair.Value);
            }

            if (_tape != null)
            {
                for (int i = 0; i < _tape.Events.Count; i++)
                {
                    TapeEvent tapeEvent = _tape.Events[i];
                    // Events are sorted by Seconds (GhostResolver.CompareEvents).
                    if (tapeEvent.Seconds > seconds)
                    {
                        break;
                    }

                    if (tapeEvent.Type != TapeEventType.DoorOpened && tapeEvent.Type != TapeEventType.DoorClosed)
                    {
                        continue;
                    }

                    if (_board.Model.TryGetDoor(tapeEvent.Position, out Door door))
                    {
                        _board.Model.SetDoorState(
                            door,
                            tapeEvent.Type == TapeEventType.DoorOpened ? DoorState.Open : DoorState.Closed);
                    }
                }
            }

            // BoardView.ApplyDoorVisualState ignores same-state refreshes so the hinge swing is not
            // restarted every ApplyTime tick (playtest 2026-08-12: all opens finished at reveal end).
            _board.RefreshDoorVisuals();
            _doorsSyncedToSeconds = seconds;
        }

        /// <summary>Back to Allot/Program: drop the tape and stand everyone on their carried point.</summary>
        public void Disarm()
        {
            _tape = null;
            _eventCursor = 0;
            _lastAppliedSeconds = 0f;
            ClearTracers();
            ClearHitVfx();
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
                // Safety net (playtest 2026-08-11): if Aftermath was skipped (debug jump, early
                // Allot, etc.) the tape would be dropped without carrying end positions/doors —
                // pawns snapped back to round-start/spawn and opens vanished. Commit while tape
                // still exists, then disarm.
                if (_tape != null)
                {
                    CommitRoundState();
                }

                Disarm();
            }
            else if (phase == RoundPhase.Aftermath)
            {
                CommitRoundState();
            }
        }

        /// <summary>
        /// Scrub the armed tape to <paramref name="seconds"/> (Time Resource). Continuous presenters
        /// re-derive visibility/state; one-shots only fire when the forward cursor crosses an event
        /// (see PLAYBACK_CONTRACT §2).
        /// </summary>
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

            // Build-once-at-arm + SetVisible from seconds — no coroutine restart (unlike the old door path).
            UpdateTracers(seconds);
            UpdateHitVfx(seconds);
            SyncDoorsToSeconds(seconds);

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
                case TapeEventType.MoveArrive:
                    _foley?.Play(FoleyId.Footstep);
                    break;
                case TapeEventType.ShootFire:
                    _foley?.Play(FoleyId.Shot);
                    break;
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
                // Origin = where the shooter stood when the window opened (matches
                // GhostResolver.ResolveHoldAngle / ResolveSnapShot), not CompleteSeconds —
                // same ShotTracerView path Snap already used; Hold just keeps it lit earlier.
                tracer.Aim(
                    _board.WorldFromPlanar(shooter.Evaluate(tapeEvent.WindowStartSeconds)),
                    _board.WorldFromPlanar(tapeEvent.Position));

                _tracers.Add(new TracerEntry(tapeEvent.WindowStartSeconds, tapeEvent.Seconds, tracer));
            }
        }

        private void UpdateTracers(float seconds)
        {
            for (int i = 0; i < _tracers.Count; i++)
            {
                TracerEntry tracer = _tracers[i];

                // Lit for the shooter's whole aim-in/hold window, not just after it completes — a
                // Hold Angle's contact (and the wound it causes) can land anywhere in that window,
                // and used to resolve well before the beam ever appeared (BUG FOUND 2026-08-06).
                bool lit = seconds >= tracer.WindowStartSeconds && seconds <= tracer.CompleteSeconds + TracerVisibleSeconds;
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

        /// <summary>
        /// Muzzle flash on <see cref="TapeEventType.ShootFire"/>, wound splat on
        /// <see cref="TapeEventType.Wounded"/>/<see cref="TapeEventType.Killed"/> — same
        /// build-once-at-arm pattern as <see cref="BuildTracers"/>.
        /// </summary>
        private void BuildHitVfx()
        {
            ClearHitVfx();

            foreach (TapeEvent tapeEvent in _tape.Events)
            {
                switch (tapeEvent.Type)
                {
                    case TapeEventType.ShootFire:
                        BuildMuzzleFlash(tapeEvent);
                        break;
                    case TapeEventType.Wounded:
                    case TapeEventType.Killed:
                        BuildWoundSplat(tapeEvent);
                        break;
                }
            }
        }

        private void BuildMuzzleFlash(TapeEvent tapeEvent)
        {
            if (!_tape.Tracks.TryGetValue(tapeEvent.PawnId, out ScheduledPath shooter))
            {
                return;
            }

            var go = new GameObject($"MuzzleFlash_{tapeEvent.PawnId}_{tapeEvent.Seconds:0.00}");
            go.transform.SetParent(transform, false);
            var flash = go.AddComponent<MuzzleFlashView>();
            flash.Init();
            // Shooter's position at the completion instant (when the shot actually fires), not the
            // aim-in/hold window start the tracer uses — the flash is the muzzle igniting, not the beam.
            flash.Place(
                _board.WorldFromPlanar(shooter.Evaluate(tapeEvent.Seconds)),
                _board.WorldFromPlanar(tapeEvent.Position));

            _muzzleFlashes.Add(new MuzzleFlashEntry(tapeEvent.Seconds, flash));
        }

        private void BuildWoundSplat(TapeEvent tapeEvent)
        {
            var go = new GameObject($"WoundSplat_{tapeEvent.TargetPawnId}_{tapeEvent.Seconds:0.00}");
            go.transform.SetParent(transform, false);
            var splat = go.AddComponent<WoundSplatView>();
            splat.Init();
            // Position is the victim's position for a hit event (TapeEvent doc comment) — no track
            // lookup needed, unlike the shooter-origin case above.
            splat.Place(_board.WorldFromPlanar(tapeEvent.Position));

            _woundSplats.Add(new WoundSplatEntry(tapeEvent.Seconds, splat));
        }

        private void UpdateHitVfx(float seconds)
        {
            for (int i = 0; i < _muzzleFlashes.Count; i++)
            {
                MuzzleFlashEntry flash = _muzzleFlashes[i];
                bool lit = seconds >= flash.Seconds && seconds <= flash.Seconds + MuzzleFlashVisibleSeconds;
                flash.View.SetVisible(lit);
            }

            for (int i = 0; i < _woundSplats.Count; i++)
            {
                WoundSplatEntry splat = _woundSplats[i];
                // Persistent once shown (ART_DIRECTION §3) — visible once the scrubber has passed the
                // hit, hidden again on rewind past it, same rule the tracer/banner logic already uses.
                splat.View.SetVisible(seconds >= splat.Seconds);
            }
        }

        private void ClearHitVfx()
        {
            for (int i = 0; i < _muzzleFlashes.Count; i++)
            {
                if (_muzzleFlashes[i].View != null)
                {
                    Destroy(_muzzleFlashes[i].View.gameObject);
                }
            }

            _muzzleFlashes.Clear();

            for (int i = 0; i < _woundSplats.Count; i++)
            {
                if (_woundSplats[i].View != null)
                {
                    Destroy(_woundSplats[i].View.gameObject);
                }
            }

            _woundSplats.Clear();
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
            public float WindowStartSeconds { get; }

            public float CompleteSeconds { get; }

            public ShotTracerView View { get; }

            public TracerEntry(float windowStartSeconds, float completeSeconds, ShotTracerView view)
            {
                WindowStartSeconds = windowStartSeconds;
                CompleteSeconds = completeSeconds;
                View = view;
            }
        }

        private readonly struct MuzzleFlashEntry
        {
            public float Seconds { get; }

            public MuzzleFlashView View { get; }

            public MuzzleFlashEntry(float seconds, MuzzleFlashView view)
            {
                Seconds = seconds;
                View = view;
            }
        }

        private readonly struct WoundSplatEntry
        {
            public float Seconds { get; }

            public WoundSplatView View { get; }

            public WoundSplatEntry(float seconds, WoundSplatView view)
            {
                Seconds = seconds;
                View = view;
            }
        }
    }
}
