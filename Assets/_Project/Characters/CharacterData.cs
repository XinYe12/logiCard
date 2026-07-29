using UnityEngine;

namespace LogiCard.Characters
{
    /// <summary>
    /// Character attribute preset (Scout / Juggernaut). Costs use Time Resource seconds (C27/C28).
    /// Numerics are demo placeholders — tune in playtesting (C20/C25).
    /// </summary>
    [CreateAssetMenu(fileName = "CharacterData", menuName = "logiCard/Character Data", order = 0)]
    public sealed class CharacterData : ScriptableObject
    {
        [Header("Identity")]
        public string displayName = "Character";

        [Header("Movement (Time Resource)")]
        [Tooltip("Seconds of Time Resource per tile at Walk-equivalent baseline (paper: Scout 1s, Heavy 2s).")]
        public float baseSecondsPerTile = 1f;

        [Header("Agility (placeholders — C25)")]
        [Tooltip("Extra Time Resource seconds when changing stance (Sprint/Walk/Crawl). Scout 0; Juggernaut +1.")]
        public float stanceChangePenaltySeconds;

        [Tooltip("Extra Time Resource seconds when switching Snap Shot ↔ Hold Angle. Mirrors stance penalty.")]
        public float shootModeChangePenaltySeconds;

        [Header("Strength")]
        [Tooltip("Base Time Resource cost to Interact a door before Strength modifiers.")]
        public float doorInteractBaseSeconds = 4f;
    }
}
