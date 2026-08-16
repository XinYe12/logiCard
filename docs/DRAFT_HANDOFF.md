# Draft Handoff — 2026-08-16

**Milestone:** Phase 5 Commercial Art Bar (active). Phase 2 Net paused (C63/C67 gear carve-out).  
**Integrator tip:** `master` @ `a21b29c` — **Match Shell Layout merged**, human Play-signed off.  
**Active wave:** Match Shell Layout closed. Follow-up dispatch round opened same day: Camera (letterbox + freecam reconcile) and Map (fence-shadow fix, done; chroma/floor-smoothness tweaks in flight) are coding-hot; four blocking human decisions resolved (Storm numerics, Sunny mode, Map tweaks, Character decision sheet).  
Plan: [`docs/ui/MATCH_SHELL_LAYOUT.md`](ui/MATCH_SHELL_LAYOUT.md) · contract: [`docs/contracts/CURRENT.md`](contracts/CURRENT.md).

**2026-08-16 decisions locked (all human-confirmed):**
- **Storm numerics** — free (0s), 1×/Character/match. `PRODUCT_MEMORY.md` **C69**.
- **Character decision sheet** — fully answered, human accepted every agent recommendation across Parts A–D. `PRODUCT_MEMORY.md` **C70–C73**. Unblocks nothing to code yet — still gated on C36 landing first, per C70/A1's build order.
- **Atmosphere Sunny mode** — stays dropped/parked, not merged. No change from prior state, just confirmed rather than left open.
- **Map's other two look-check tweaks** (Yard/Hall chroma separation, Vault floor smoothness) — approved; dispatched alongside the already-landed fence-shadow fix.

**Locked stack (top → bottom, live on master):** InfoBar → MapViewport (diorama) → HandBand → ToolBar → TimelineSchedule (YOU/ENEMY/EFFECTS; playhead = TR scrubber).  
**Human sign-off:** Played the five-band `ProgramHud` in the UI worktree 2026-08-16 — "quite good, satisfied preliminary design." First all-department collaboration wave (UI coding + Cards/Character/Map/Atmosphere docs + Camera paused) called a success.

**Read first next session:** this file → `PARALLEL_OPS.md` → `departments/INDEX.md` → `contracts/CURRENT.md` → `ui/MATCH_SHELL_LAYOUT.md`.

## Live folders

| Seat | Folder | Tip / state |
|------|--------|-------------|
| Integrator | `logiCard` | `master` @ `a21b29c` — Match Shell + docs peers merged, batchmode-verified (see Verification) |
| **UI** | `logiCard-modal-restyle` | `feat/modal-restyle` @ `e1c80fb` — fully merged to master; worktree can resync/idle |
| Atmosphere | `logiCard-atmosphere-stylized` | Docs contribution merged (`WEATHER_MAP_VIEWPORT.md`/`CLOUD_MOTION.md`); worktree still carries its own uncommitted Sunny-mood code + stray `Assets/_Recovery` scene — **not** merged, separate decision pending |
| Cards | `logiCard-cards-collection` | Docs contribution merged (`CARD_COLLECTION.md` §13); worktree otherwise idle |
| Character | `logiCard-char-select-motion` | Docs contribution merged (`CHARACTER_FANTASY.md` §4.1); worktree still carries its own large, older, unmerged Character Select carousel feature (12 commits) — separate workstream, untouched |
| Map | `logiCard-map` | Docs contribution merged (`MAP_PRESENTATION_STANDARD.md` §6 + look-check candidate tweaks); worktree otherwise idle |
| Camera | `logiCard-camera-control` | `feat/camera-freecam-tps` @ `2b06a3a` (freecam+TPS committed, still un-merged) — now unblocked to reconcile since `ProgramHud.MapViewport` rect exists on master |

## Implemented

- **Match Shell Layout merged to master (`c9925b1` + `a21b29c`).** UI's five-band `ProgramHud` (InfoBar/MapViewport/HandBand/ToolBar/TimelineSchedule) took over `GearHandView.cs`/`ProgramHud.cs` wholesale from `feat/modal-restyle` — it was the fully tested superset (absorbed both the hand-deck-drag-play work and the HUD chrome ship pass `3f77b6c`), over master's own earlier standalone hand-deck-drag-play (`164012f`), which the human hadn't actually reviewed in this form. Wired the previously-open one-line `GameBootstrap.RegisterMatchState` hook (delegates to `RoundPlayback.WoundsOf`/`BandageChargeOf` for the attacker pawn) so InfoBar's wounds/charge reads are now live instead of stubbed.
- **Docs peers folded in, scoped to their Match Shell contribution only** (each dept's other in-flight work left alone in its worktree — see Live folders): Cards §13 schedule-chip taxonomy, Character §4.1 InfoBar field sheet (new `CHARACTER_FANTASY.md`), Map §6 camera-framing recommendation + look-check findings, Atmosphere weather/MapViewport confinement notes.
- Kept master's `SENTIS_ANALYTICS_ENABLED` define on `ProjectSettings.asset` merge; restored six unrelated asset-pack `.meta` deletions that were UI-worktree noise, not part of the shell.

## Verification

- **Post-merge batchmode on `master` @ `a21b29c` (Editor closed on this exact path, independently re-run by Integrator):** EditMode **174/174** passed, PlayMode **56/56** passed, 0 failures. First independent confirmation of the combined tip (UI had only reported green on their own worktree before this).
- Docs peers: content reviewed diff-by-diff before pulling (see Implemented); not a code-risk item.

## Still unfinished

1. UI finish Match Shell / Human Play sign-off / Integrator merge UI + fold peer docs — all done.
2. Storm numerics — locked C69 (free, 1×/Character/match).
3. Character decision sheet — fully answered, C70–C73.
4. Map fence-shadow bug — fixed (`83d875a` on `dept/map`, EditMode 158/158 / PlayMode 49/49), not yet human-eyeballed.
5. **Camera reconcile — in flight.** Dispatched worker merged master into `feat/camera-freecam-tps`, edited `BoardCameraRig.cs`/`GameBootstrap.cs` for the retune. EditMode passed (188/188); PlayMode batchmode failed to start (stale-lockfile suspect) — sent back to retry. Nothing merged to master yet.
6. **Map's two remaining tweaks — in flight.** Yard/Hall chroma separation + Vault floor smoothness dispatched to the same Map worker; not yet reported back.
7. Backlog: Healed presenter; prune dead Bandage/Storm Mode board-tap paths in `BoardInputController`; URP shadow tune on main (uncommitted `LogiCardURP.asset` 50→20 distance, 2048→4096 map) — not Play-verified; Storm's HUD-side cast gate is still per-round not a true per-match counter (flagged in C69, unstarted).

## Tomorrow

1. Check back on the Camera and Map workers' reports; review diffs against their briefs before merging either.
2. Human Play/look pass needed on three things once workers report: Camera framing (esp. Rail Platform) + freecam feel; Map's fence-shadow fix; Map's chroma/floor-smoothness tweaks. None mergeable on batchmode-green alone.
3. Once Camera merges, this dispatch round is fully closed out.

## Blockers / notes

- **You are Integrator on main** unless continuing dept coding in a worktree — never two agents on the same tree.
- No push unless asked.
