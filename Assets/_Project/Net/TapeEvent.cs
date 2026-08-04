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

        /// <summary>Arrival point for a Move, aim point for a Shoot, victim's position for a hit.</summary>
        public PlanarPosition Position { get; }

        public int TargetPawnId { get; }

        public TapeEvent(float seconds, int pawnId, TapeEventType type, PlanarPosition position, int targetPawnId = NoPawn)
        {
            Seconds = seconds;
            PawnId = pawnId;
            Type = type;
            Position = position;
            TargetPawnId = targetPawnId;
        }

        public override string ToString()
        {
            return TargetPawnId == NoPawn
                ? $"{Seconds:0.00}s P{PawnId} {Type} {Position}"
                : $"{Seconds:0.00}s P{PawnId} {Type} {Position} -> P{TargetPawnId}";
        }
    }
}
