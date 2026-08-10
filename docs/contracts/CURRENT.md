# Cross-Dept Contracts — Current Wave

**Wave:** Look-and-feel + UI, started 2026-08-09. Core-gameplay/networking work explicitly paused by the
human until this wave lands — see `SCHEDULE.md`'s Cadence section. **UI side landed and merged twice**
(`feat/ui-component-system`, then a same-session dock move from right-edge to bottom band). **Environment
side: checkpoint 1 merged 2026-08-09; checkpoint 2 (asset packs/door models/character rework) started
2026-08-10** on human go-ahead ("still bad, continue with the implementation") without an explicit
hero-shot-vs-readability answer — proceeding on the assumption that more richness is wanted, flagged as an
inference, not confirmed.
**Updated:** 2026-08-10 by Integrator.
**Rule:** Only Integrator edits this file after a merge. Workers implement against the frozen signatures
below.

## Frozen contracts this wave

### Camera viewport-rect, bottom-band shape (closed 2026-08-10)

- Dock moved from a right-edge margin (`HudDockWidth`) to a bottom band (`HudDockHeight = 0.34f`) — direct
  playtest feedback ("put the control at the bottom... vertical alignment generally"), not a worker decision.
- `GameBootstrap.cs`: `cam.rect = new Rect(0f, ProgramHud.HudDockHeight, 1f, 1f - ProgramHud.HudDockHeight -
  ProgramHud.TopStripHeight)` — board region is full width now, from the top of the dock to the bottom of the
  top strip.
- **Flagged, not yet resolved:** the board's visible aspect ratio changed meaningfully with this move (was
  narrower-than-tall, now wider-than-tall) — `orthographicSize` (5.0) was tuned against the old shape and
  likely needs retuning. Env worker's checkpoint 2 brief has permission to retune it if the framing looks off.
- Verification pending — Editor was open on the main tree (live playtest) both times batchmode was attempted
  after this change; self-reviewed carefully in its place, not yet independently confirmed.

## Ownership reminders this wave

- `Assets/_Project/Board/**`, `PrimitiveMaterialFactory.cs`, `Assets/_Project/Art/**`: environment worker only
  (`feat/env-lookfeel-overhaul`, still active).
- `Assets/_Project/UI/**`: closed out — `feat/ui-component-system` merged, no worker currently assigned.
- `Assets/_Project/Boot/GameBootstrap.cs`: Integrator-only edit target; the env worker may still need
  lighting/weather tuning wired in.
- `Sim/`, `Net/`, `Timeline/`, `GhostResolver`: untouched by this wave — core gameplay is paused.

## Closed contracts (reference)

### Camera viewport-rect ↔ `ProgramHud`'s dock-width constants, UI side (closed 2026-08-09)

- `feat/ui-component-system` widened `HudDockWidth` 0.30 → 0.34 for cell readability; `TopStripHeight`
  unchanged at 0.08.
- **No `GameBootstrap.cs` rewrite needed** — `cam.rect = new Rect(0f, 0f, 1f - ProgramHud.HudDockWidth, 1f -
  ProgramHud.TopStripHeight)` reads the constant symbolically, confirmed by Integrator before merge, not just
  taken on the worker's word.
- The env worker's `orthographicSize`/`BuildLighting`/`BuildDioramaVolume` changes (checkpoint 1,
  `feat/env-lookfeel-overhaul`) land independently — different lines of the same method, no overlap.

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
