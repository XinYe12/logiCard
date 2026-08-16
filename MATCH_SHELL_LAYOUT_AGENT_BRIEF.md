# MATCH_SHELL_LAYOUT — UI seat agent brief

**From:** Integrator  
**To:** UI (`D:\projects\Game\logiCard-modal-restyle`, `feat/modal-restyle`)  
**Wave:** Match Shell Layout — **layout / region geometry only**  
**Read first:** `docs/ui/MATCH_SHELL_LAYOUT.md` (copied onto master; pull or read from main if missing locally),
`docs/DRAFT_HANDOFF.md`, this brief. Refs: main-tree `screenshots/image copy 18.png` + `19.png`.

## Goal

Restructure in-match HUD from the old **3-column bottom dock** into the locked vertical stack:

**InfoBar → MapViewport → HandBand → ToolBar → TimelineSchedule**

Do **not** restyle card faces / icons / button art this wave. Move existing widgets into new regions.
Reuse chrome tokens from `3f77b6c` (`CreateBackingPanel`, `DockPanel*`) where they still fit.

## Do

1. Replace `BuildHudDock` / `BuildProgramControls` layout with five full-width bands per
   `MATCH_SHELL_LAYOUT.md` strawman fractions (tunable ±4%; MapViewport must stay largest).
2. **InfoBar:** phase / round / wounds / TR used·budget / side labels — read-only. Stub empty fields
   rather than invent fantasy HP/mana.
3. **MapViewport:** empty UI hole (no blocking Image) so the existing 3D board shows through; do not
   draw Hearthstone minion lanes. Expose a public `RectTransform MapViewport` (or equivalent) so
   Camera/Integrator can later letterbox the camera to that rect.
4. **HandBand:** host existing `GearHandView` (fan + drag-to-play unchanged).
5. **ToolBar:** Move / Shoot / Door, stance, Snap/Hold, Lock In, transport, Adrenaline — everything
   that lived in ControlsColumn + ActionColumn.
6. **TimelineSchedule:** multi-row schedule shell (YOU / ENEMY / EFFECTS) + playhead bound to the
   existing scrubber/`CurrentSeconds` path. Migrate queue-log content into YOU-row chips or a compact
   readout inside this band. Playful chrome = stub OK; **scrubbing must work**.
7. Update layout tests (`ProgramHudLayoutTests` etc.) for the new region geometry; keep drag-play tests green.
8. Update `docs/departments/ui/STATUS.md` → Ready when batchmode green + report back.

## Do not

- Touch Sim/Net/`GhostResolver`/`RoundPlayback` resolve math (call existing APIs only).
- Merge to master / push.
- Build Hearthstone battlefield card rows.
- Redesign GearHand card faces or icon set.
- Own `BoardCameraRig` / `GameBootstrap.ConfigureCamera` (Camera slice + Integrator).

## Baseline

Continue on current tip `3f77b6c`. **Chrome ship pass is not merging alone** — this layout absorbs it.

## Done when

- Play Mode shows the five bands in order; board visible in MapViewport; hand + toolbar usable;
  timeline playhead scrubs Time Resource.
- EditMode + PlayMode batchmode green on this worktree.
- STATUS + short Integrator report (deviations listed).

## Parallel peers (do not collide)

| Seat | Their job |
|------|-----------|
| Cards | Docs: schedule block taxonomy for gear cards |
| Character | Docs: InfoBar field sheet |
| Map | Docs: MapViewport framing |
| Atmosphere | Docs: weather read confined to map |
| Camera | Pause freecam until MapViewport rect frozen — then respect it |
