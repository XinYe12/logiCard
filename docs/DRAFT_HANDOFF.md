# Draft Handoff — 2026-08-15

**Milestone:** Phase 5 Commercial Art Bar (active). Phase 2 Net paused (C63/C67 gear carve-out).  
**Integrator tip:** `master` @ `164012f` (hand-deck drag-to-play + Storm/Bandage HUD + Fair-lightning).  
**Active wave:** **Match Shell Layout** — vertical in-match stack (layout only; not per-widget chrome).  
Plan: [`docs/ui/MATCH_SHELL_LAYOUT.md`](ui/MATCH_SHELL_LAYOUT.md) · contract: [`docs/contracts/CURRENT.md`](contracts/CURRENT.md) (open section at top).

**Locked stack (top → bottom):** InfoBar → MapViewport (diorama, **not** HS battleground) → HandBand → ToolBar → TimelineSchedule (YOU/ENEMY/EFFECTS; playhead = TR scrubber).  
**Refs:** `screenshots/image copy 18.png` / `19.png`.  
**Pending human:** PRODUCT_MEMORY C-row to lock this stack (provisional `UI_FLOW.md` §4 amend; C48 landscape canvas kept unless portrait reopened).

**Read first next session:** this file → `PARALLEL_OPS.md` → `departments/INDEX.md` → `contracts/CURRENT.md` → `ui/MATCH_SHELL_LAYOUT.md` → UI worktree STATUS / uncommitted `ProgramHud`.

## Live folders

| Seat | Folder | Tip / state |
|------|--------|-------------|
| Integrator | `logiCard` | `master` @ `164012f` — layout docs/contracts dirty uncommitted |
| **UI** | `logiCard-modal-restyle` | `feat/modal-restyle` @ `3f77b6c` + **uncommitted Match Shell code — Ready**, EditMode 174/174 / PlayMode 53/53 (Editor closed to batchmode, human-approved) |
| Atmosphere | `logiCard-atmosphere-stylized` | Docs **Ready** — `docs/departments/atmosphere/WEATHER_MAP_VIEWPORT.md` (uncommitted); Sunny mood still held back |
| Cards | `logiCard-cards-collection` | Docs **Ready** — `CARD_COLLECTION.md` §13 schedule chip taxonomy (uncommitted) |
| Character | `logiCard-char-select-motion` | Docs **Ready** — `CHARACTER_FANTASY.md` §4.1 InfoBar field sheet (uncommitted) |
| Map | `logiCard-map` | Docs **Ready** — `MAP_PRESENTATION_STANDARD.md` §6 MapViewport framing (uncommitted) |
| Camera | `logiCard-camera-control` | `feat/camera-freecam-tps` @ `2b06a3a` (freecam+TPS **already committed**); brief said pause for MapViewport — reconcile before merge |

## Implemented

- **On master:** C62–C68 gear/deck packaging; Storm + Bandage HUD; hand-deck drag-to-play (`164012f`); Fair no-lightning (`561e7fd`); chrome ship pass **not** on master (`3f77b6c` on UI branch only — absorbed into layout).
- **Match Shell wave opened by Integrator:** plan doc, contract, `MATCH_SHELL_LAYOUT_AGENT_BRIEF.md` in every worktree; INDEX/HANDOFF updated.
- **Docs peers (in their worktrees, not merged to master):** Atmosphere viewport framing; Cards schedule language; Character InfoBar sheet; Map camera-framing §6.
- **UI Match Shell — Ready (uncommitted on `feat/modal-restyle`):** `ProgramHud` rebuilt to five locked bands, fractions 0.07/0.48/0.14/0.17/0.14 (InfoBar/MapViewport/HandBand/ToolBar/TimelineSchedule — MapViewport largest). `MapViewport` is a real empty UI hole with a new public `ProgramHud.MapViewport` rect for Camera/Integrator letterboxing later; `GameBootstrap.ConfigureCamera` needed **zero edits** (`HudDockHeight`/`TopStripHeight` repurposed, numerically equal to the new rect). InfoBar gained real wounds + TR-used/budget fields (stubbed pending the `RegisterMatchState` wiring below). ToolBar merges old Controls+Action columns into one row (SET PATH folded into stance row as 4th cell). TimelineSchedule reuses the YOU-track scrubber code verbatim plus stub ENEMY/EFFECTS rows; queue readout moved here from HandBand. All existing GameObject names tests depend on (`Gear_Bandage`, `QueueReadout`, etc.) preserved — no drag-play test behavior changes. Only `ProgramHudLayoutTests.cs` (rewritten for new geometry) + `AppFlowPlayModeTests.cs` (two assertions) changed. **Verified:** Editor was closed on this worktree (human-approved) — EditMode 174/174, PlayMode 53/53, 0 failures including all Hand Deck Drag Play tests. STATUS.md updated to Ready. **Still open before merge:** one-line `GameBootstrap` hook to wire `RegisterMatchState` (delegates, not a `RoundPlayback` param — UI can't reference `Boot`) — flagged by UI as Integrator-dirty file, not made.

## Verification

- Master @ `164012f`: EditMode 173/173, PlayMode 56/56 (hand-deck cherry-pick verify earlier today).
- Match Shell UI: EditMode 174/174, PlayMode 53/53 — **UI-reported**, Editor closed on that worktree for the run; **not yet independently re-run by Integrator**; no human Play on new stack yet.
- Docs peers: content written in worktrees; not independently reviewed/merged.

## Still unfinished

1. ~~UI finish Match Shell~~ — **done**, reported Ready 2026-08-15 (see Implemented above).
2. **Human Play sign-off** on shell layout → Integrator merges UI (+ fold peer docs) → wire one-line `GameBootstrap.RegisterMatchState` hook → camera letterbox to `MapViewport` (Atmosphere: do **not** let Sunny/`Camera.main.backgroundColor` wash HUD chrome).
3. **Camera branch reconcile** — `2b06a3a` landed freecam/TPS while shell was opening; gate merge on MapViewport API + human Play.
4. Backlog: Healed presenter; Atmosphere Sunny keep/drop; Storm numerics lock; prune dead Bandage/Storm Mode board-tap paths in `BoardInputController`; URP shadow tune on main (uncommitted `LogiCardURP.asset` 50→20 distance, 2048→4096 map) — not Play-verified.

## Tomorrow

1. **Human Play** the Match Shell stack in `logiCard-modal-restyle` (Editor will need to be closed again for any further batchmode) — sign off or flag issues.
2. Integrator: after sign-off, cherry-pick/merge shell to master; wire the one-line `RegisterMatchState` hook + `ConfigureCamera` to `MapViewport`; pull docs peer deliverables (Cards §13, Character §4.1, Map §6, Atmosphere `WEATHER_MAP_VIEWPORT.md`).
3. Only then resume Camera freecam merge / chrome-collection backlog.

## Blockers / notes

- **You are Integrator on main** unless continuing UI coding in the modal-restyle worktree — never two agents on the same tree.
- Main dirty (do not commit unless asked): `LogiCardURP.asset`, `BoardInputController.cs` (friendlier Storm rejection string), `ProjectSettings.asset`, docs (`DRAFT_HANDOFF`, `INDEX`, `contracts/CURRENT`, untracked `MATCH_SHELL_LAYOUT.md`), `ExplosiveLLC/`, extra screenshots.
- UI worktree also has unrelated pack `.meta` deletes + `ProjectSettings` noise — don’t ship those with the shell.
- No push unless asked.
