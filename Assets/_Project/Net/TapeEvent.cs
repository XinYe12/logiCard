using LogiCard.Sim;

namespace LogiCard.Net
{
    /// <summary>
    /// Discrete outcomes a <see cref="ReplayTape"/> can carry. Invalid is reserved for post-demo
    /// Otherwise Stop; Hold Angle lethality uses <see cref="Killed"/> (Day 6).
    /// </summary>
    public enum TapeEventType
    {
        MoveArrive = 0,
        ShootFire = 1,
        Wounded = 2,
        Killed = 3,
        Invalid = 4,
        DoorOpened = 5,
        DoorClosed = 6,

        /// <summary>C63 — Bandage clears one Wounded stack (always one carried in from a prior
        /// round — GhostResolver.CompileTrack resolves this from GhostInput.StartingWounds before
        /// this round's own ResolveShots pass ever runs). Presenter: one-shot banner only
        /// (RoundPlayback.Report) — no board splat to hide, since BuildHitVfx only ever splats
        /// this round's own Wounded/Killed events, never a prior-round wound.</summary>
        Healed = 7,

        /// <summary>C67 — Storm card cast. Continuous presenter (mirrors DoorOpened/Closed): board
        /// weather mood is Storm from this event's Seconds onward for the rest of the round; no
        /// wound/charge effect.</summary>
        StormCast = 8,
    }

    /// <summary>
    /// One resolved moment on the tape, stamped with the Time Resource second it happens at (C27) —
    /// never a Playback Duration second. Position replaces Coordinate (C35/C39 pivot).
    /// </summary>
    public readonly struct TapeEvent
    {
        public const int NoPawn = -1;

        public float Seconds { get; }

        public int PawnId { get; }

        public TapeEventType Type { get; }

        /// <summary>
        /// Arrival point for a Move; victim's position for a hit. For a Shoot, the tracer's visual
        /// endpoint — the aim point, or the nearest wall/closed-door crossing if the shot is blocked
        /// (2026-08-07) — not necessarily the raw aim point.
        /// </summary>
        public PlanarPosition Position { get; }

        public int TargetPawnId { get; }

        /// <summary>
        /// For <see cref="TapeEventType.ShootFire"/> only: when the shooter started pulling the
        /// trigger (aim-in start for Snap, hold start for Hold Angle) — <see cref="Seconds"/> is
        /// always the completion instant. Defaults to <see cref="Seconds"/> for every other event
        /// type. Lets playback keep a Hold Angle's tracer lit across its whole hold window instead
        /// of only after it completes, since a hold's contact can land anywhere in that window
        /// (BUG FOUND 2026-08-06: the wound could land before the tracer ever showed up).
        /// </summary>
        public float WindowStartSeconds { get; }

        public TapeEvent(float seconds, int pawnId, TapeEventType type, PlanarPosition position, int targetPawnId = NoPawn, float? windowStartSeconds = null)
        {
            Seconds = seconds;
            PawnId = pawnId;
            Type = type;
            Position = position;
            TargetPawnId = targetPawnId;
            WindowStartSeconds = windowStartSeconds ?? seconds;
        }

        public override string ToString()
        {
            return TargetPawnId == NoPawn
                ? $"{Seconds:0.00}s P{PawnId} {Type} {Position}"
                : $"{Seconds:0.00}s P{PawnId} {Type} {Position} -> P{TargetPawnId}";
        }
    }
}
