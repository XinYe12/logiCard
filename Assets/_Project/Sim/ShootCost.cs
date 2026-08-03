namespace LogiCard.Sim
{
    /// <summary>
    /// Fixed Time Resource costs for the Shoot base verb (GDD §6 / §3A).
    /// </summary>
    public static class ShootCost
    {
        public const float SnapShotSeconds = 2f;
        public const float HoldAngleSeconds = 3f;

        public static float SecondsFor(ShootMode mode)
        {
            return mode == ShootMode.HoldAngle ? HoldAngleSeconds : SnapShotSeconds;
        }
    }
}
