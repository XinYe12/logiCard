namespace LogiCard.Timeline
{
    /// <summary>
    /// Round flow phases. Lock is an instantaneous transition and is not a dwell phase.
    /// Allot = Time Card commit; Aftermath = post-playback; MatchOver = terminal (C33).
    /// </summary>
    public enum RoundPhase
    {
        Allot,
        Program,
        Reveal,
        Execute,
        Aftermath,
        MatchOver,
    }
}
