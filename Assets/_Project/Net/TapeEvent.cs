using LogiCard.Sim;

namespace LogiCard.Net
{
    /// <summary>
    /// Discrete outcomes a <see cref="ReplayTape"/> can carry. Invalid is reserved for Day 7+
    /// Otherwise Stop; Hold Angle lethality uses <see cref="Killed"/> (Day 6).
    /// </summary>
    public enum TapeEventType
    {
        MoveArrive = 0,
        ShootFire = 1,
        Wounded = 2,
        Killed = 3,
        Invalid = 4,
    }

    /// <summary>
    /// One resolved moment on the tape, stamped with the Time Resource second it happens at (C27) —
    /// never a Playback Duration second.
    /// </summary>
    public readonly struct TapeEvent
    {
        public const int NoPawn = -1;

        public float Seconds { get; }

        public int PawnId { get; }

        public TapeEventType Type { get; }

        /// <summary>Arrival tile for a Move, aimed tile for a Shoot, victim's tile for a hit.</summary>
        public GridCoordinate Coordinate { get; }

        public int TargetPawnId { get; }

        public TapeEvent(float seconds, int pawnId, TapeEventType type, GridCoordinate coordinate, int targetPawnId = NoPawn)
        {
            Seconds = seconds;
            PawnId = pawnId;
            Type = type;
            Coordinate = coordinate;
            TargetPawnId = targetPawnId;
        }

        public override string ToString()
        {
            return TargetPawnId == NoPawn
                ? $"{Seconds:0.00}s P{PawnId} {Type} {Coordinate}"
                : $"{Seconds:0.00}s P{PawnId} {Type} {Coordinate} -> P{TargetPawnId}";
        }
    }
}
