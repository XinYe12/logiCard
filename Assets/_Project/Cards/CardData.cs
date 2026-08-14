using UnityEngine;

namespace LogiCard.Cards
{
    public enum CardId
    {
        Bandage = 0,
        Interact = 1,
        Flashbang = 2,
        Adrenaline = 3,

        /// <summary>C67 — landed with the value pre-assigned (not left to Cards) so Cards' catalog
        /// work and UI's GearHandView dock work can build against it in parallel from separate
        /// worktrees without a cross-branch ordering dependency.</summary>
        Storm = 4,
    }

    /// <summary>
    /// Gear card definition. Move and Shoot are base verbs, not cards (C15).
    /// Costs are Time Resource seconds — placeholders until playtest lock (C20).
    /// </summary>
    [CreateAssetMenu(fileName = "CardData", menuName = "logiCard/Card Data", order = 1)]
    public sealed class CardData : ScriptableObject
    {
        public CardId cardId;
        public string displayName = "Card";

        [Tooltip("Time Resource cost in seconds. Adrenaline may be 0 (instant).")]
        public float timeResourceCostSeconds = 1f;

        [Tooltip("If true, limited uses per match (Flashbang / Adrenaline = 1).")]
        public bool oncePerMatch;

        [TextArea(2, 4)]
        public string effectSummary;
    }
}
