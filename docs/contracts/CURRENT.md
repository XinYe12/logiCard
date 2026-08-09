# Cross-Dept Contracts — Current Wave

**Wave:** none active as of this reset (2026-08-09). Phase 1 (Landscape Desktop UI) shipped and merged — see
git history (`771db57`, merge commit, `GameBootstrap.cs` wiring commit) if you need the old signature for
reference.
**Updated:** 2026-08-09 by Integrator.
**Rule:** Only Integrator edits this file after a merge. Workers implement against the frozen signatures
below.

## Frozen contracts this wave

*(none yet — next wave, likely a Phase 2 networking foundation once the human locks transport +
host-integrity, or a Phase 5 art-bar slice, gets its own frozen contract here once briefed.)*

## Ownership reminders this wave

*(populate once a wave starts.)*

## Closed contracts (reference)

### `ProgramHud`'s HUD-dock layout constants ↔ `GameBootstrap.ConfigureCamera()`'s camera viewport rect (Phase 1, closed 2026-08-09)

- `ProgramHud` landed `HudDockWidth = 0.30f` (right-edge dock), `HudDockHeight = 0f` (not a bottom band),
  `TopStripHeight = 0.08f`; kept `ThumbZoneHeight` as a compile-compat alias (`= HudDockHeight`), locked by
  `ProgramHudLayoutTests`/`AppFlowPlayModeTests`.
- Integrator rewired `GameBootstrap.cs:298-301`:
  `cam.rect = new Rect(0f, 0f, 1f - ProgramHud.HudDockWidth, 1f - ProgramHud.TopStripHeight)`.
- Re-verified post-merge on `master`: EditMode 108/108, PlayMode 32/32.
