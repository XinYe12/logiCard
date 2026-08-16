# Draft Handoff — 2026-08-16

**Milestone:** Phase 5 Commercial Art Bar (active). Phase 2 Net paused (C63/C67 gear carve-out).
**Integrator tip:** `master` @ `07501d7` — **Match Shell Layout + Map's fence-shadow/material tweaks merged**, both batchmode-verified.
**Active wave:** Match Shell Layout closed. Follow-up dispatch round: Map fully merged. Camera reconcile is done and Integrator-reviewed but **blocked** — human tried it and the controls didn't respond (root cause found: right-click-drag required, human tried left-drag; also zero on-screen discoverability). A control-hint overlay is being added now; re-test needed before merge.
Plan: [`docs/ui/MATCH_SHELL_LAYOUT.md`](ui/MATCH_SHELL_LAYOUT.md) · contract: [`docs/contracts/CURRENT.md`](contracts/CURRENT.md).

**2026-08-16 decisions locked (all human-confirmed):**
- **Storm numerics** — free (0s), 1×/Character/match. `PRODUCT_MEMORY.md` **C69**.
- **Character decision sheet** — fully answered, human accepted every agent recommendation across Parts A–D. `PRODUCT_MEMORY.md` **C70–C73**. Unblocks nothing to code yet — still gated on C36 landing first, per C70/A1's build order.
- **Atmosphere Sunny mode** — stays dropped/parked, not merged. No change from prior state, just confirmed rather than left open.
- **Map's other two look-check tweaks** (Yard/Hall chroma separation, Vault floor smoothness) — approved, merged.

**Locked stack (top → bottom, live on master):** InfoBar → MapViewport (diorama) → HandBand → ToolBar → TimelineSchedule (YOU/ENEMY/EFFECTS; playhead = TR scrubber).
**Human sign-off:** Played the five-band `ProgramHud` in the UI worktree 2026-08-16 — "quite good, satisfied preliminary design." First all-department collaboration wave (UI coding + Cards/Character/Map/Atmosphere docs + Camera paused) called a success.

**Read first next session:** this file → `PARALLEL_OPS.md` → `departments/INDEX.md` → `contracts/CURRENT.md` → `ui/MATCH_SHELL_LAYOUT.md`.

## Live folders

| Seat | Folder | Tip / state |
|------|--------|-------------|
| Integrator | `logiCard` | `master` @ `07501d7` — Match Shell + Map merged, batchmode-verified (see Verification) |
| **UI** | `logiCard-modal-restyle` | `feat/modal-restyle` @ `e1c80fb` — fully merged to master; worktree can resync/idle |
| Atmosphere | `logiCard-atmosphere-stylized` | Docs contribution merged; worktree still carries its own uncommitted Sunny-mood code + stray `Assets/_Recovery` scene — **not** merged, stays parked per human decision |
| Cards | `logiCard-cards-collection` | Docs contribution merged; worktree otherwise idle |
| Character | `logiCard-char-select-motion` | Docs + decision sheet merged; worktree still carries its own large, older, unmerged Character Select carousel feature (12 commits) — separate workstream, untouched |
| Map | `logiCard-map` | **Fully merged to master** (`07501d7`) — fence-shadow fix + Hall/Vault material tweaks; worktree idle |
| Camera | `logiCard-camera-control` | `feat/camera-freecam-tps` @ (hint commit pending) — camera math done, **control-hint overlay being added**, not merged, human re-test needed |

## Implemented

- **Match Shell Layout merged to master (`c9925b1` + `a21b29c`).** UI's five-band `ProgramHud` took over `GearHandView.cs`/`ProgramHud.cs` wholesale from `feat/modal-restyle`. Wired the previously-open `GameBootstrap.RegisterMatchState` hook so InfoBar's wounds/charge reads are live.
- **Docs peers folded in** scoped to their Match Shell contribution only: Cards §13, Character §4.1 (`CHARACTER_FANTASY.md`), Map §6, Atmosphere weather/MapViewport confinement notes.
- **Storm numerics locked (C69)**, **Character decision sheet fully answered (C70–C73)** — see decisions above.
- **Map merged to master (`07501d7`):** fence Panel/Rail parts no longer cast shadows (fixed a jet-black shadow-acne burst at wall junctions); Hall floor darkened/more saturated to separate from Yard; Vault floor smoothness 0.22→0.13 to match siblings.
- **Camera reconcile — math done, not yet merged.** Worker merged master into `feat/camera-freecam-tps`, re-derived `orthographicSize` default (3.4→2.8) and `BoardCameraRig` min/max bounds (2.6→2.15 / 8.0→6.6) by the exact ratio the MapViewport rect's height shrank (0.48/0.58≈0.828) — correctly reasoned that shrinking only rect *height* (not width) widens camera aspect. Verified against real door/corridor coordinates for all three maps. Integrator-reviewed: exactly in-lane. EditMode 188/188, PlayMode 59/59.
- **Camera control-hint fix (in progress, this session):** human tested the actual worktree and reported "camera not working at all" — root cause is a real UX gap, not a functional bug: right-click-drag rotates (human tried left-drag, which is reserved for board taps and does nothing for the camera), and there was zero on-screen indication of any of this. Added a small IMGUI legend anchored to the camera's own `pixelRect` (self-contained in `BoardCameraRig.cs`, no `LogiCard.UI` dependency) showing "Right-drag: Rotate · Scroll: Zoom · WASD: Pan · T: Lock View" (swaps to a TPS-specific line while locked). Batchmode verification in progress.

## Verification

- **Post-Match-Shell-merge batchmode on `master` @ `a21b29c`:** EditMode 174/174, PlayMode 56/56.
- **Post-Map-merge batchmode on `master` @ `07501d7`:** EditMode 174/174, PlayMode 56/56. Both independently re-run by Integrator, Editor closed on this path.
- **Camera worktree (`feat/camera-freecam-tps` @ commit before the hint fix):** EditMode 188/188, PlayMode 59/59, Integrator-reviewed diff. Hint fix not yet batchmode-verified as of this writing.

## Still unfinished

1. **Camera control-hint fix — verify then re-test.** Batchmode run in flight; once green, human needs to re-test with the actual controls (right-click-drag / scroll / WASD / T) and confirm both that input now works as expected *and* that the framing itself (esp. Rail Platform) and freecam/TPS feel are good. This is the only thing blocking Camera's merge.
2. Backlog: Healed presenter; prune dead Bandage/Storm Mode board-tap paths in `BoardInputController`; URP shadow tune on main (uncommitted `LogiCardURP.asset` 50→20 distance, 2048→4096 map) — not Play-verified; Storm's HUD-side cast gate is still per-round not a true per-match counter (flagged in C69, unstarted); real on-screen control hint should eventually move to proper UI-owned chrome (current IMGUI version is a stopgap, not final presentation).

## Tomorrow

1. Confirm Camera worktree batchmode is green after the hint fix, then have the human re-test controls + framing.
2. Once approved: Integrator merges Camera to master, batchmode-reverifies, updates INDEX/contracts.
3. After that, this whole dispatch round is closed out — no paused dept work outstanding.

## Blockers / notes

- **You are Integrator on main** unless continuing dept coding in a worktree — never two agents on the same tree.
- No push unless asked.
