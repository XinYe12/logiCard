# Draft Handoff — 2026-08-16

**Milestone:** Phase 5 Commercial Art Bar (active). Phase 2 Net paused (C63/C67 gear carve-out).  
**Integrator tip:** `master` @ `a21b29c` — **Match Shell Layout merged**, human Play-signed off.  
**Active wave:** Match Shell Layout is closed as a cross-dept collaboration. Next: Camera letterbox to `MapViewport`, then reconcile the paused freecam/TPS branch.  
Plan: [`docs/ui/MATCH_SHELL_LAYOUT.md`](ui/MATCH_SHELL_LAYOUT.md) · contract: [`docs/contracts/CURRENT.md`](contracts/CURRENT.md).

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

1. ~~UI finish Match Shell~~ — done.
2. ~~Human Play sign-off~~ — done 2026-08-16.
3. ~~Integrator merge UI + fold peer docs~~ — done.
4. **Camera letterbox to `MapViewport`** — `ProgramHud.MapViewport` rect now exists on master; wire `GameBootstrap.ConfigureCamera` to it, then re-derive `orthographicSize`/`BoardCameraRig` min/max zoom against the new, shorter rect height (Map's §6 priority order: doors > flank sightline > floor edge; Rail Platform is the tall-map risk case to check first).
5. **Camera branch reconcile** — `2b06a3a` (freecam/TPS) can now proceed against the real `MapViewport` API; still needs review + human Play before merge.
6. **Atmosphere Sunny-mood decision** — still uncommitted in the Atmosphere worktree, explicitly not part of this merge; needs its own human call (keep/drop) before anyone touches it again.
7. Backlog: Healed presenter; Storm numerics lock; prune dead Bandage/Storm Mode board-tap paths in `BoardInputController`; URP shadow tune on main (uncommitted `LogiCardURP.asset` 50→20 distance, 2048→4096 map) — not Play-verified; Map's fence soft-shadow "black burst" bug (candidate fix: stop shadow-casting on fence Panel/Rails).

## Tomorrow

1. Wire `ConfigureCamera` → `MapViewport` letterbox; retune ortho size per Map §6 guidance.
2. Reconcile/merge Camera freecam+TPS branch once letterbox lands.
3. Human decision on Atmosphere Sunny mood (merge as its own feature, or drop for good).

## Blockers / notes

- **You are Integrator on main** unless continuing dept coding in a worktree — never two agents on the same tree.
- No push unless asked.
