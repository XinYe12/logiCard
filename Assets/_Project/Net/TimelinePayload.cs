using System.Collections.Generic;

namespace LogiCard.Net
{
    /// <summary>
    /// Compiled local program for one pawn (TDD D6 §2/§3). Scoped to a single client's pawn —
    /// the Day 11 RPC carries the sender's identity separately.
    /// </summary>
    public sealed class TimelinePayload
    {
        public IReadOnlyList<ActionNode> Nodes { get; }

        public TimelinePayload(IReadOnlyList<ActionNode> nodes)
        {
            Nodes = nodes;
        }
    }
}
