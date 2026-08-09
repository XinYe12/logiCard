# Cross-Dept Contracts — Current Wave

**Wave:** none active as of this reset (2026-08-09). Phase 2 first slice (resolve relay) shipped and merged —
see git history (`47f4534`, `685f542`, merge commit) if you need the old signatures for reference.
**Updated:** 2026-08-09 by Integrator.
**Rule:** Only Integrator edits this file after a merge. Workers implement against the frozen signatures
below.

## Frozen contracts this wave

*(none yet — next wave gets its own frozen contract here once briefed. Candidates per `SCHEDULE.md`: wiring
`RelayMatchResolver` into the live Find Match flow, a Phase 5 art-bar slice, or Phase 2's remaining OPEN items
once a human is ready to spend time on them — see `NETWORKING_DESIGN.md`'s OPEN summary.)*

## Ownership reminders this wave

*(populate once a wave starts.)*

## Closed contracts (reference)

### `IMatchResolver` + `RelayMatchResolver` (Phase 2 first slice, closed 2026-08-09)

- `Assets/_Project/Net/IMatchResolver.cs` / `LocalMatchResolver.cs`: landed (Integrator), frozen, default
  everywhere. `RoundPlayback.ResolveAndArm()` calls through it via `Init`'s `matchResolver` param.
- `Assets/_Project/Net/RelayMatchResolver.cs` + `Relay/LogiCard.Relay/**` (new net8.0 console project, repo
  root, sibling to `Assets/`): landed (worker, `feat/phase2-relay-slice`). Networked `IMatchResolver` over raw
  TCP (4-byte length-prefixed JSON envelopes, `RelayProtocol.cs`), talking to a minimal standalone relay that
  pairs exactly two connections and runs the real shared `GhostResolver` once as authority. Reviewed in depth
  before merge — see `DRAFT_HANDOFF.md` for the two specific correctness checks done (CardData/Modifier
  dead-path verification, connection-order independence via `GhostResolver`'s internal `PawnId` sort).
- Re-verified independently post-merge on `master`: EditMode 110/110, PlayMode 32/32, standalone xUnit 2/2
  (`dotnet test Relay/LogiCard.Relay.sln`).

### `AppFlowController.EnteredMatch` gains `bool viaRelay` + `RoundPlayback.SetMatchResolver` (closed 2026-08-09)

- `AppFlowController.EnteredMatch` is now `Action<bool>` — Find Match fires `true`, Local Play and every
  test/`SliceSceneFixture`'s `BypassToMatch()` fire `false`.
- `RoundPlayback.SetMatchResolver(IMatchResolver)`: swaps the resolver used by the next `ResolveAndArm()`.
  Safe any time before the match's first Lock In.
- `GameBootstrap` subscribes and picks `RelayMatchResolver()` (defaults `127.0.0.1:7777`) for Find Match,
  `LocalMatchResolver` for Local Play — landed live, `RelayMatchResolver` is no longer dormant.
- `RoundPlayback.ResolveAndArmRoutine()`'s resolver pump now catches a failed `MoveNext()` and reports it via
  the existing `OutcomeReported` banner instead of an unhandled exception.
- Verified: EditMode 110/110, PlayMode 32/32. Real two-process network round-trip through a live relay not
  yet human-smoke-tested.

### `ProgramHud`'s HUD-dock layout constants ↔ `GameBootstrap.ConfigureCamera()`'s camera viewport rect (Phase 1, closed 2026-08-09)

- `ProgramHud` landed `HudDockWidth = 0.30f` (right-edge dock), `HudDockHeight = 0f` (not a bottom band),
  `TopStripHeight = 0.08f`; kept `ThumbZoneHeight` as a compile-compat alias (`= HudDockHeight`), locked by
  `ProgramHudLayoutTests`/`AppFlowPlayModeTests`.
- Integrator rewired `GameBootstrap.cs:298-301`:
  `cam.rect = new Rect(0f, 0f, 1f - ProgramHud.HudDockWidth, 1f - ProgramHud.TopStripHeight)`.
- Re-verified post-merge on `master`: EditMode 108/108, PlayMode 32/32.
