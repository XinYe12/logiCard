namespace LogiCard.Sim
{
    /// <summary>
    /// Fixed Time Resource costs for the Shoot base verb (GDD §6 / §3A). Only SnapShotSeconds
    /// is consumed on Day 3 — HoldAngleSeconds is reserved for Day 6's Snap/Hold mode picker.
    /// </summary>
    public static class ShootCost
    {
        public const float SnapShotSeconds = 2f;
        public const float HoldAngleSeconds = 3f;
    }
}
