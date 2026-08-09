# Cross-Dept Contracts — Current Wave

**Wave:** Phase 1 — Landscape Desktop UI (`feat/phase1-landscape-ui`), started 2026-08-09. First slice of the
post-`C46` phase-based schedule (`docs/SCHEDULE.md`).
**Updated:** 2026-08-09 by Integrator.
**Rule:** Only Integrator edits this file after a merge. Workers implement against the frozen signatures
below.

## Frozen contracts this wave

### `ProgramHud`'s HUD-dock layout constants ↔ `GameBootstrap.ConfigureCamera()`'s camera viewport rect

- **Owner (definition):** Presentation (`feat/phase1-landscape-ui`) — replaces the current
  `ProgramHud.ThumbZoneHeight` / `ProgramHud.TopStripHeight` public constants (portrait/bottom-band shape)
  with whatever constants describe the new landscape dock's placement/size (bottom margin or side margin —
  worker's call per `UI_FLOW.md`, must be reported back explicitly).
- **Owner (consumer, wiring):** Integrator — `Assets/_Project/Boot/GameBootstrap.cs:298` currently does
  `cam.rect = new Rect(0f, ProgramHud.ThumbZoneHeight, 1f, 1f - ThumbZoneHeight - TopStripHeight)`. This line
  gets rewritten by Integrator at merge time to match whatever shape the new constants imply (still
  top/bottom math if the dock stayed a bottom band; left/right math if it moved to a side margin).
- **Design pointer:** `docs/UI_FLOW.md`'s Program Phase layout table (`C48`).
- **Merge status:** not yet landed — worktree just spun up, worker not yet reported back.

## Ownership reminders this wave

- `Assets/_Project/UI/**` (+ its PlayMode tests): Presentation only, this wave.
- `Assets/_Project/Boot/GameBootstrap.cs`: stays Core/Integrator-owned even though this wave depends on one
  line of it — see the frozen contract above for why that line isn't a free-for-all.
- Everything else (`Sim/`, `Net/`, `Timeline/`, `Board/*View.cs`, `GhostResolver`): untouched by this wave.
