# Cross-Dept Contracts — Current Wave

**Wave:** Phase 2, first slice — `feat/phase2-relay-slice`, started 2026-08-09. Building `RelayMatchResolver`
+ a minimal standalone resolve-relay process against the frozen `IMatchResolver` contract below.
**Updated:** 2026-08-09 by Integrator.
**Rule:** Only Integrator edits this file after a merge. Workers implement against the frozen signatures
below.

## Frozen contracts this wave

### `IMatchResolver` (landed on `master`, `Assets/_Project/Net/IMatchResolver.cs`)

```csharp
public interface IMatchResolver
{
    IEnumerator ResolveAsync(IReadOnlyList<GhostInput> inputs, Action<ReplayTape> onResolved);
}
```

- **Owner (interface + default impl):** Integrator — `IMatchResolver.cs` and `LocalMatchResolver.cs` are
  landed and frozen; nobody edits them this wave.
- **Owner (consumer):** Integrator — `RoundPlayback.ResolveAndArm()` already calls through this via
  `Init`'s `IMatchResolver matchResolver = null` param (defaults to `LocalMatchResolver`). Not touched by
  this wave's worker.
- **Owner (new implementation):** Core (`feat/phase2-relay-slice`) — builds `RelayMatchResolver` against this
  interface as-is, plus the standalone relay process it talks to.
- **Design pointer:** `docs/NETWORKING_DESIGN.md`'s "Phase 2, first slice" section — includes a coroutine-
  nesting gotcha worth reading before implementing `ResolveAsync`.
- **Merge status:** not yet landed — worktree just spun up, worker not yet reported back.

## Ownership reminders this wave

- `Assets/_Project/Net/RelayMatchResolver.cs` (new) + `Relay/**` (new, repo-root): Core only, this wave.
- `Assets/_Project/Boot/GameBootstrap.cs`, `RoundPlayback.cs`: stay Core/Integrator-owned — the seam is
  already wired, the worker builds against it, doesn't edit it.
- `Assets/_Project/Net/IMatchResolver.cs`, `LocalMatchResolver.cs`, `GhostResolver.cs`: frozen, landed.
- Everything else (`Sim/` gameplay logic, `Board/*View.cs`, docs): untouched by this wave.

## Closed contracts (reference)

### `ProgramHud`'s HUD-dock layout constants ↔ `GameBootstrap.ConfigureCamera()`'s camera viewport rect (Phase 1, closed 2026-08-09)

- `ProgramHud` landed `HudDockWidth = 0.30f` (right-edge dock), `HudDockHeight = 0f` (not a bottom band),
  `TopStripHeight = 0.08f`; kept `ThumbZoneHeight` as a compile-compat alias (`= HudDockHeight`), locked by
  `ProgramHudLayoutTests`/`AppFlowPlayModeTests`.
- Integrator rewired `GameBootstrap.cs:298-301`:
  `cam.rect = new Rect(0f, 0f, 1f - ProgramHud.HudDockWidth, 1f - ProgramHud.TopStripHeight)`.
- Re-verified post-merge on `master`: EditMode 108/108, PlayMode 32/32.
