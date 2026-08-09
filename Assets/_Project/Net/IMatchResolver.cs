using System;
using System.Collections;
using System.Collections.Generic;

namespace LogiCard.Net
{
    /// <summary>
    /// Resolves one round's collected <see cref="GhostInput"/>s into an authoritative
    /// <see cref="ReplayTape"/>. Coroutine-shaped (not <c>Task</c>-based) to match this project's
    /// existing async idiom — UI/round flow already drives everything through <c>IEnumerator</c>
    /// coroutines (<c>ProgramHud.LockInRoutine</c>, <c>AppFlowController.PlayLockInBridge</c>), not
    /// <c>async</c>/<c>await</c>. A real network round-trip (Phase 2 / C52's resolve-relay) takes real
    /// time and must not block the main thread; <see cref="LocalMatchResolver"/> completes within the
    /// same coroutine step, so callers see no behavior change until a networked implementation exists.
    /// </summary>
    public interface IMatchResolver
    {
        IEnumerator ResolveAsync(IReadOnlyList<GhostInput> inputs, Action<ReplayTape> onResolved);
    }
}
