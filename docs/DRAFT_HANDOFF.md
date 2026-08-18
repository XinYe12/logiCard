# Draft Handoff — 2026-08-18

**2026-08-18 (last item on the backlog): camera control-hint moved off IMGUI onto real UI chrome.**
`BoardCameraRig.OnGUI()` (the always-on IMGUI legend, a deliberate stopgap since 2026-08-16 per its own
doc comment) is gone. In its place: `BoardCameraRig.ControlHintText` (new computed property, single
source of truth for the mode→text mapping), read live every frame by `ProgramHud.Update()` into the
`CameraControlHint` `Text` label — which turned out to already half-exist: `BuildMapViewport` had a
*static*, never-updated `CameraRotateHint` label ("RIGHT-DRAG TO ROTATE VIEW") sitting right next to
where the IMGUI legend was rendering, both saying roughly the same thing. Consolidated onto the one real
label rather than shipping two overlapping hints — renamed it, made its content dynamic (now shows the
full command set and switches to the TPS-lock hint when locked, matching what the IMGUI version did),
and wired it with a new `ProgramHud.RegisterCameraRig(BoardCameraRig)` call from `GameBootstrap` (a
direct reference, not a delegate — `LogiCard.UI` already references `LogiCard.Board`, so there's no
backward-dependency problem to route around the way `RegisterMatchState` has to for `LogiCard.Boot`).
New PlayMode test `CameraControlHintTracksLiveCameraMode` proves the label is actually live (asserts it
equals `ControlHintText` before and after `CycleTpsLock()`/`ExitTpsLock()`, not just present) and that
`raycastTarget` stays off so it can never block a board tap underneath it (the exact bug class
`docs/UI_BOARD_ANCHORED_COMPONENTS.md` warns about). Batchmode-verified fresh, Editor closed: EditMode
190/190, PlayMode 62/62. **Not yet human/Editor-verified** — same standing caveat as every other Phase 5
presentation change in this file; batchmode can't see pixels, and text placement/readability over the
live 3D board is worth a real look before calling this fully done.

**2026-08-18 (later still): pruned dead Bandage/Storm board-tap paths in `BoardInputController`.**
Since the Hand Deck Drag Play brief (2026-08-15) moved both cards to drag-out-of-hand-only placement,
`Mode` (`BoardInputController.Mode`) can never be `ActionVerb.Bandage`/`ActionVerb.Storm` anymore —
`ProgramHud.SetMode` is only ever called with Move/Shoot/Door. Confirmed via grep (no other call site
anywhere sets `.Mode = ActionVerb.Bandage`/`.Storm`) before removing `TryTapPoint`'s two dead branches
and the now-unreachable `ResolveBandageExecuteTime` helper they alone called. No test exercised the
removed paths (checked — all Bandage/Storm test coverage goes through `TryQueueBandageAt`/
`TryQueueStormAt`/drag-out gestures, never `Mode = Bandage/Storm` + `TryTapPoint`), so no test changes
needed. Batchmode-verified fresh, Editor closed: EditMode 190/190, PlayMode 61/61 — same counts as
before the prune, confirming it was genuinely dead code, not a coverage gap.

**2026-08-18 (later same day): Storm's real per-match counter landed** — closes the deviation C69
flagged: the HUD gate previously only enforced "not already queued this Program" (per-round; a fresh
round always reset it), not the actual 1×/Character/match rule. Mirrored Bandage's shape exactly:
`RoundPlayback.StormCastCountOf` (new), `GhostResolver` now enforces the cast itself the same way it
already enforced Bandage's charge (`GhostInput.StartingStormCastCount` → `ReplayTape.StormCastCountFor`
→ `PawnEntry.StormCastCount`, carried round-to-round the same way wounds/Bandage charge already are),
`ProgramHud.RegisterMatchState` grew a third `stormCastCountOf` delegate (`GameBootstrap` updated).
New coverage: `GhostResolverStormTests` (EditMode, mirrors `GhostResolverBandageTests`) and
`StormStaysBlockedInASecondRoundAfterBeingCastInTheFirst` (PlayMode — proves the actual bug: a fresh,
empty round-2 Program used to make the card interactable again). Updated `contracts/CURRENT.md`,
`PRODUCT_MEMORY.md` C69, `CARD_COLLECTION.md`, `GEAR_STORM_AGENT_BRIEF.md`, `departments/ui/STATUS.md`
to reflect the deviation is closed rather than leaving them describing stale behavior. Batchmode-verified
fresh, Editor closed: EditMode 190/190, PlayMode 61/61.

**2026-08-18: Healed presenter landed** (backlog item from 2026-08-17, no dispatch needed — small,
single-file-scoped, done directly on `master`). `RoundPlayback.Report` now fires a one-shot `"HEALED
P{id} @{s}s"` banner for `TapeEventType.Healed`, same shape as Wounded/Killed. **Real finding, not just
implementation:** the doc comment reserving this slot said the presenter should "hide/restore the
specific wound splat" Bandage clears — traced the actual resolver semantics (`GhostResolver.CompileTrack`
resolves a Bandage node's heal from `GhostInput.StartingWounds`, i.e. only wounds carried in from a
*prior* round, entirely before this round's own `ResolveShots` pass ever applies a hit) and found that a
`Healed` event can therefore never correspond to a splat that exists — `BuildHitVfx` only ever splats
*this* round's own Wounded/Killed events, and those are structurally disjoint from what Bandage can heal.
Documented the reasoning in `PLAYBACK_CONTRACT.md` §3 rather than build speculative hide/restore logic for
a case that can't occur. `TapeEventType.Healed` moved from `ReservedNoPresenterYet` to
`PresentedAtScrubber` in `TapeEventPlaybackCoverageTests`. New PlayMode test
`CrossingTheHealedSecondShowsStubTextAndRewindClearsIt` (two-round flow: get wounded in round 1, commit
via Aftermath, Bandage-only round 2, scrub across the Healed second). **Batchmode-verified fresh, Editor
closed:** EditMode 188/188, PlayMode 60/60 (one `BoardWeatherPocketPlayModeTests` flake on an earlier run
— unrelated file, clean on immediate rerun, not caused by this change).

**Milestone:** Phase 5 Commercial Art Bar (active). Phase 2 Net paused (C63/C67 gear carve-out).
**Integrator tip:** `master` — **camera control-hint moved to real UI chrome** (see 2026-08-18 notes above, this is the last backlog item), on top of dead Bandage/Storm board-tap prune, Storm per-match counter `6d17776`, Healed presenter `c81aa3e`, and Camera merge `e594c51`. Batchmode-verified independently on master post-commit (EditMode 190/190, PlayMode 62/62). **Not yet Play-verified** — see "Still unfinished."
**Active wave: this dispatch round is closed.** Match Shell Layout, Map, and Camera are all merged to master. Camera landed via human hands-on iteration during the actual re-test — the human found the control-hint overlay's rotate-only right-drag didn't feel right and iterated on it live: first to a combined pan+rotate gesture (`169a55f`), then further to right-drag doing pitch tilt between top/front view rather than pan (`2e2d022`, `CAMERA_VERTICAL_DRAG_PAN_BRIEF.md`). Integrator re-ran batchmode fresh against each commit as it landed and again on `master` after the merge — every pass green. No paused dept work outstanding.
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
| Integrator | `logiCard` | `master` @ `e594c51` — Match Shell + Map + Camera all merged, batchmode-verified (see Verification) |
| **UI** | `logiCard-modal-restyle` | `feat/modal-restyle` @ `e1c80fb` — fully merged to master; worktree can resync/idle |
| Atmosphere | `logiCard-atmosphere-stylized` | Docs contribution merged; worktree still carries its own uncommitted Sunny-mood code + stray `Assets/_Recovery` scene — **not** merged, stays parked per human decision; branch pushed to origin for safekeeping |
| Cards | `logiCard-cards-collection` | Docs contribution merged; worktree otherwise idle; branch pushed to origin for safekeeping (branch itself is stale vs. master — do not merge as-is) |
| Character | `logiCard-char-select-motion` | Docs + decision sheet merged; worktree still carries its own large, older, unmerged Character Select carousel feature (12 commits) — separate workstream, untouched; branch pushed to origin for safekeeping |
| Map | `logiCard-map` | **Fully merged to master** (`07501d7`) — fence-shadow fix + Hall/Vault material tweaks; worktree idle |
| Camera | `logiCard-camera-control` | `feat/camera-freecam-tps` @ `2e2d022` — **fully merged to master** (`e594c51`); worktree idle |

## Implemented

- **Match Shell Layout merged to master (`c9925b1` + `a21b29c`).** UI's five-band `ProgramHud` took over `GearHandView.cs`/`ProgramHud.cs` wholesale from `feat/modal-restyle`. Wired the previously-open `GameBootstrap.RegisterMatchState` hook so InfoBar's wounds/charge reads are live.
- **Docs peers folded in** scoped to their Match Shell contribution only: Cards §13, Character §4.1 (`CHARACTER_FANTASY.md`), Map §6, Atmosphere weather/MapViewport confinement notes.
- **Storm numerics locked (C69)**, **Character decision sheet fully answered (C70–C73)** — see decisions above.
- **Map merged to master (`07501d7`):** fence Panel/Rail parts no longer cast shadows (fixed a jet-black shadow-acne burst at wall junctions); Hall floor darkened/more saturated to separate from Yard; Vault floor smoothness 0.22→0.13 to match siblings.
- **Camera reconcile — math done.** Worker merged master into `feat/camera-freecam-tps`, re-derived `orthographicSize` default (3.4→2.8) and `BoardCameraRig` min/max bounds (2.6→2.15 / 8.0→6.6) by the exact ratio the MapViewport rect's height shrank (0.48/0.58≈0.828) — correctly reasoned that shrinking only rect *height* (not width) widens camera aspect. Verified against real door/corridor coordinates for all three maps. Integrator-reviewed: exactly in-lane.
- **Camera control-hint overlay + gesture iteration — merged to master (`e594c51`).** Human tested the worktree and initially reported "camera not working at all" — root cause was a real UX gap, not a functional bug: right-click-drag rotates (human tried left-drag, which is reserved for board taps and does nothing for the camera), zero on-screen indication of any of this. Added a small IMGUI legend anchored to the camera's own `pixelRect` (self-contained in `BoardCameraRig.cs`, no `LogiCard.UI` dependency). From there the human iterated on gesture feel live, hands-on, across two more commits: combined pan+rotate on one right-drag (`169a55f`), then further to right-drag doing pitch tilt between top/front view (`2e2d022`, see `CAMERA_VERTICAL_DRAG_PAN_BRIEF.md`). Integrator re-ran batchmode fresh against each commit before merging, then merged with `--no-ff` and independently re-verified on `master` itself: **EditMode 188/188, PlayMode 59/59.**

## Verification

- **Post-Match-Shell-merge batchmode on `master` @ `a21b29c`:** EditMode 174/174, PlayMode 56/56.
- **Post-Map-merge batchmode on `master` @ `07501d7`:** EditMode 174/174, PlayMode 56/56.
- **Post-Camera-merge batchmode on `master` @ `e594c51`:** EditMode 188/188, PlayMode 59/59. All three independently re-run by Integrator, Editor closed on this path each time.

## Still unfinished

1. Backlog: ~~Healed presenter~~, ~~dead Bandage/Storm board-tap paths~~, ~~Storm per-match counter~~,
   ~~IMGUI control-hint → real UI chrome~~ — all landed 2026-08-18 (see above). Every code item originally
   listed here is now closed; the URP shadow-tune item turned out to already be committed (correction
   logged above, `bd5fad8`), not actually a pending task. **What's left is not code — it's a human/Editor
   look:** the shadow tune, and today's control-hint chrome move, are both batchmode-green but not
   Play-verified. Neither has been claimed as fully done for that reason.
2. Atmosphere's Sunny mode (parked) and Character's carousel feature (separate workstream) remain uncommitted-or-unmerged in their worktrees, unchanged — their branches were pushed to origin as-is for safekeeping (human switching machines), not merged into master. Cards' branch was also pushed as-is; it's stale against master and would need real reconciliation, not a fast merge, before it could land.

## Tomorrow

1. This dispatch round is closed — Match Shell, Map, and Camera all merged to master, all independently batchmode-verified. No paused dept work outstanding.
2. Every backlog item from 2026-08-17 (Healed presenter, Storm per-match counter, dead board-tap prune, IMGUI control-hint → UI chrome) closed 2026-08-18. **Next actual next-step is a human Play pass** over the shadow tune + control-hint chrome (both batchmode-green, neither eyeballed yet) — after that, restaff an idle department (Atmosphere Sunny decision, Character carousel, Cards reconciliation) or open a new Phase 5 wave.

## Blockers / notes

- **You are Integrator on main** unless continuing dept coding in a worktree — never two agents on the same tree.
- All worktree branches pushed to `origin` 2026-08-17 (human switching machines) — `master` itself still needs an explicit push, see note below.
