namespace LogiCard.Sim
{
    /// <summary>
    /// Shoot base-verb modes (GDD §3A / C25). Stored on Shoot <see cref="LogiCard.Net.ActionNode"/>s;
    /// Move nodes leave this as <see cref="None"/>.
    /// </summary>
    public enum ShootMode
    {
        None = 0,
        SnapShot = 1,
        HoldAngle = 2,
    }

    public static class ShootModeMath
    {
        public static string Label(ShootMode mode)
        {
            switch (mode)
            {
                case ShootMode.SnapShot: return "Snap";
                case ShootMode.HoldAngle: return "Hold";
                default: return mode.ToString();
            }
        }
    }
}
