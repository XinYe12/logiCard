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
        Bandage, // C63 — first gear card. Self-targeting; clears one Wounded stack.
        Storm,   // C67 — gear card. Self-targeting; presentation-only board weather trigger, no wound/charge effect.

        /// <summary>C36/C71 — Bomber unique verb, wall-only v1. Targets a designed
        /// <see cref="LogiCard.Sim.BreachPoint"/> (Position = its segment midpoint, mirrors Door);
        /// books Time Resource, persists an attached-bomb flag on that point, no geometry change yet.</summary>
        BombAttach,

        /// <summary>C36/C71 — detonates a bomb this Bomber attached. Targets the same
        /// <see cref="LogiCard.Sim.BreachPoint"/> as its matching <see cref="BombAttach"/>; transitions
        /// that point straight from Intact to <see cref="LogiCard.Sim.BreachState.Breached"/> (v1 has
        /// no Damaged intermediate — reserved for a future two-hit mechanic).</summary>
        BombDetonate,
    }
}
