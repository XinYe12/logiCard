namespace LogiCard.Net
{
    /// <summary>
    /// Distinguishes what an <see cref="ActionNode"/> does at its ExecuteTime. Not in the TDD's
    /// literal ActionNode table (D6 §2) — added because Day 4 playback needs to branch on it.
    /// </summary>
    public enum ActionVerb
    {
        Move,
        Shoot,
        Door,   // Day 7 — contextual map action, GDD §4. Not a gear card (C34).
    }
}
