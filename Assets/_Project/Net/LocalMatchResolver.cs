using System;
using System.Collections;
using System.Collections.Generic;

namespace LogiCard.Net
{
    /// <summary>
    /// Default <see cref="IMatchResolver"/>: wraps today's same-process <see cref="GhostResolver"/>
    /// call, unchanged in behavior or timing. Used for local hotseat, every existing test, and the
    /// matchmaking-fallback bot (Phase 3) — none of those need to know a networked resolver can exist.
    /// Never yields before invoking <paramref name="onResolved"/>, so a coroutine that does
    /// <c>yield return</c> on this completes within the same step, not a later frame.
    /// </summary>
    public sealed class LocalMatchResolver : IMatchResolver
    {
        private readonly GhostResolver _resolver;

        public LocalMatchResolver(GhostResolver resolver)
        {
            _resolver = resolver ?? throw new ArgumentNullException(nameof(resolver));
        }

        public IEnumerator ResolveAsync(IReadOnlyList<GhostInput> inputs, Action<ReplayTape> onResolved)
        {
            onResolved(_resolver.Resolve(inputs));
            yield break;
        }
    }
}
