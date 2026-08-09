namespace LogiCard.Cards
{
    /// <summary>
    /// Non-Unity stub for <c>ActionNode.Modifier</c>. The real <c>CardData</c> is a Unity
    /// <c>ScriptableObject</c>; gear cards are deferred from the 14-day ship (C34) and Modifier is
    /// always null on the resolve path today. This shim exists only so the shared
    /// <c>ActionNode.cs</c> source compiles into the headless relay without pulling UnityEngine.
    /// </summary>
    public sealed class CardData
    {
    }
}
