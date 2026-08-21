# Draft Handoff — 2026-08-21

## STATE (update every session — this is the first thing to read)

- **Current phase:** Phase 5 (Commercial Art Bar) is nominal top priority per `SCHEDULE.md`; Phase 2 (Net)
  stays paused except an explicit narrow carve-out for C36/Bomber core-gameplay work (human-directed
  "character, GO", 2026-08-20).
- **Current checkpoint:** C36/Bomber — Sim layer, RoundPlayback presenter, and now **BoardView visuals**
  are all landed and merged to `master` (`6a10534`, 2026-08-21). `feat/bomber-hud` (HUD board-anchored
  prompt) is built and batchmode-green in its own worktree, **not yet merged** — Integrator needs to
  verify and merge it next. Still open after that: a real map-authored `BreachPoint` (Map's
  `C36_FIRST_BREACH_POINT_PROPOSAL.md` recommends Rail Platform Pocket north wall, awaiting human
  sign-off — parked in worktree `agent-a53cde266d5a5ec90`), Character-gating.
- **Next single action:** verify + merge `feat/bomber-hud` from worktree `logiCard-bomber-hud`. Three
  completed docs-only recommendations are also awaiting human sign-off and are not yet actioned: Flashbang
  mechanic (worktree `agent-acb1b1a5ded3b2ead`), Time Player C36-readiness two-question gate (worktree
  `agent-af74d30717fa4309c`), and the breach-point map pick above.
- **If this block looks stale** (doesn't match the dated entries directly below it), trust the dated
  entries and fix this block — it's a summary of them, not an independent source of truth.

---

**2026-08-21 (latest): C36/Bomber `BoardView` breach-point visuals merged to master (`6a10534`).**
Picked up `docs/map/BREACH_VISUALS_AGENT_BRIEF.md` from `feat/breach-visuals`. `BoardView.RefreshBreachVisuals`
mirrors `RefreshDoorVisuals` exactly: Intact/Damaged both render as the ordinary wall (unchanged
`PlaceWallFence`), Breached hides it and shows scorched end stubs + floor rubble, an attached-but-not-
yet-detonated bomb shows a charge marker straddling the wall (a one-face-mounted first pass was
invisible from the camera's opposite orbit angle — caught by looking at a render, not by batchmode; see
`breach-02-bomb-attached.png`/`breach-03-breached.png`). Driven from `BoardView.LateUpdate` re-deriving
straight from `ArenaBoard` every frame, **not** a `RoundPlayback` hook — `RoundPlayback` is frozen/
Integrator-owned, so this reads the same already-scrubber-pure model one layer further out instead of
adding a second consumer to the frozen presenter class; flagged by the agent as an explicit architectural
deviation from the Door pattern and accepted as-is (Door itself has no `_board.Refresh…()` call at the
`RoundPlayback` layer either — `BoardView` deriving visuals from live model state each frame is the
existing shape, not a new one). `docs/core/PLAYBACK_CONTRACT.md` §3 and `docs/contracts/CURRENT.md`'s
C36 section both updated to record this landing and to document the earlier, previously-unrecorded
Detonate-consumes-the-attached-bomb fix (`6c7990a`) alongside it. Independently re-verified on `master`
after the merge, Editor closed: **EditMode 196/196, PlayMode 73/73** — exact match to the agent's
self-report. Worktree `logiCard-breach-visuals` and branch `feat/breach-visuals` removed post-merge.

**2026-08-21: UI's Bomber HUD prompt built, batchmode-green, awaiting Integrator merge.** Agent report
(worktree `logiCard-bomber-hud`, branch `feat/bomber-hud`, not yet merged): `ProgramHud` gained a
`Mode_Bomber` button/context row and a board-anchored `BuildBombPrompt`/`RefreshBombPrompt` mirroring the
Door prompt's identity/live-state/explicit-confirm shape per `UI_BOARD_ANCHORED_COMPONENTS.md`.
Deliberately added `PawnProgram.ScheduledBreachState`/`ScheduledHasAttachedBomb` (scheduled-state reads,
not live-`ArenaBoard` reads) — same reasoning as `ScheduledDoorState`: a live-only read would let a
player queue and pay for Attach twice before the resolver ever runs. Self-reported: EditMode 196/196,
PlayMode 74/74 (the +1 over the breach-visuals merge's 73 is a new bomb-prompt test). **Not yet
independently re-verified by Integrator or merged** — next action.

**2026-08-20 (latest): Atmosphere's "storm rolling in" transition merged to master (`ecf0093`).**
Picked up the `STORM_TRANSITION_AGENT_BRIEF.md` brief from a fresh `feat/storm-transition` worktree
(correctly forked off current `master`, not the stale `feat/atmosphere-stylized`, exactly as the brief
asked). Fair/Storm weather modules now slide in over 1.1s (ease-out quad) via a rigid translation of the
finished module back to origin, instead of popping in instantly — placement itself untouched, so the
locked Zap-to-cloud-shelf glue (`b62b48a`) stays intact. Real course-correction along the way: a first
scale-based version broke an existing test because Zap `ConeVolume.shape.length` is a raw local number,
not scale-adjusted, so it desynced from the shrunk live CloudBank bounds; switched to translation, which
has no such gap since every position *relative to* CloudBank stays correct at every instant of the
slide. `ClearWeather`'s existing `StopAllCoroutines()` clears any in-flight transition before the next
module builds, so PLAYBACK_CONTRACT rule 4 (no per-tick restart) holds — covered by a new test that
scrubs across a mood boundary and rewinds. Also caught and reverted an incidental `.meta` deletion
(same class of Editor-opens-project noise this session has hit before) before committing — didn't let it
ride along. Clean merge (zero file overlap with the C36/Bomber work that landed after this branch
forked); independently re-verified, not just trusted: **EditMode 196/196, PlayMode 67/67.**

**2026-08-20 (earlier): C36 geometry-breach + Bomber wall-only verb — Sim layer landed on master,
human-directed ("character, GO").** First core-gameplay work since the Phase 2/Net pause began; scoped
narrowly (Sim primitive only, see below) rather than the whole feature, given the size. Scope check
first surfaced an inefficiency worth naming: asked the human to confirm three scope questions (wall-only
vs floor-drop, designed points vs freeform, one node vs two) that **C71** (2026-08-16) had already
locked — should have read `PRODUCT_MEMORY.md` before asking. Answers matched C71 exactly, no harm done,
just a wasted round-trip.

**What landed:** `BreachPoint`/`BreachState` (mirrors `Door`/`DoorState` exactly — registration, exact-
match + radius lookup, `IsBlocking`/`TryGetNearestBlockPoint`/`Clone` all extended so an Intact/Damaged
point blocks Move/Shoot exactly like a wall, only Breached opens it). `ActionVerb.BombAttach`/
`BombDetonate`, `TapeEventType.BombAttached`/`GeometryBreached`. `PawnProgram.TryQueueBombAttach`/
`TryQueueBombDetonate` (same `InteractRadius`/board-tap shape as `TryQueueDoor`). `GhostResolver`'s
`ResolveShots` gained a third interleaved chronological stream (alongside shots and door toggles) so a
same-round Detonate correctly opens Shoot LoS for shots *after* it while leaving earlier shots blocked,
and a same-round Attach is visible to a later-same-round Detonate on the same point. **Resolve() never
mutates its own `ArenaBoard` input** — verified by test, same purity discipline Door already holds to;
persisting Attach/Detonate to the real board across rounds is presenter work, not resolver work, exactly
how Door state already persists (via `RoundPlayback` applying tape events during Execute, never via the
resolver mutating its input). Full frozen signatures + documented deviations:
`docs/contracts/CURRENT.md`'s new C36/Bomber section.

**Deliberately NOT built this wave — a Sim-layer-only slice, not the whole feature:** RoundPlayback
presenter (both new `TapeEventType`s are `ReservedNoPresenterYet`), `BoardView` visuals, any actual
map-authored `BreachPoint` (no map calls `RegisterBreachPoint` yet — needs a real per-map content
decision, not just code), the HUD board-anchored prompt (explicitly UI seat's slot per
`CHARACTER_BOMBER_AGENT_BRIEF.md` §6), and Character-gating (any pawn can currently queue these verbs).
This was a scope call, not an oversight — floor-drop/per-floor-occupancy was already ruled out by C71,
and building presenter+visuals+map-authoring+HUD blind in the same pass this session risked exactly the
kind of unverified, un-reviewable pile-up the rest of this session has been careful to avoid.

Batchmode-verified fresh, Editor closed: **EditMode 196/196** (188 baseline + 6 new
`GhostResolverBombTests`, including the two "Resolve() stays pure" cases and the same-round chronological
ordering case), **PlayMode 66/66** (unaffected — no PlayMode coverage this wave, by design).

**2026-08-20 (later): UI shell-chrome restyle merged to master (`546ba31`), human-approved "as is."**
Two commits: the full non-HUD restyle (Boot/Character Select/Map Select/Lobby/Match End — lit backdrops,
`ShellButton` family, Iomanoid display font) plus a live 3D character-model preview inside each
Character Select card (`CharacterPreviewRig.cs`, render-texture based, shares the exact same
`Resources` prefab/tint the match itself spawns via `PawnView.TryInstantiateArchetypeVisual`). Clean
auto-merge — hand-verified anyway given the corruption class found in the Atmosphere merge, plus a
repo-wide conflict-marker sweep; nothing found. Batchmode: **EditMode 190/190, PlayMode 66/66.** Worktree
`logiCard-ui-restyle` removed post-merge.

**Two real board-art bugs surfaced by the card preview, logged not fixed (human: "log and move on"):**
Scout's face/hands render bright orange-red (`PawnView`'s team-tint targets a mesh part named `"Body"`,
which on this model is skin, not torso) and Juggernaut's prefab has a bunny-ears hat mesh enabled — both
always true on the actual board, just invisible at top-down scale. See
`docs/departments/character/STATUS.md`'s new Backlog section.

**Also 2026-08-20: fixed a real camera bug** (`70745f3`) — TPS-lock follow camera wiggled/shook during
pawn movement. Root cause: `BoardCameraRig.ApplyTpsLock` snapped its facing direction instantly to each
frame's raw (noisy) position delta; fixed by turning toward the target at a bounded rate instead of
snapping. Batchmode: EditMode 190/190, PlayMode 65/65.

**2026-08-20: Atmosphere's Sunny weather mood merged to master (`0857b80`).** Human called Sunny "ok to
merge" after a Play look at the `feat/atmosphere-stylized` worktree. `BoardWeatherMood.Sunny` lands as a
third mood alongside Fair/Storm, sharing a new lighting-override subsystem
(`CaptureLightingBaseline`/`RestoreLightingIfOverridden`) with Storm's dim. **Boot default deliberately
stays Fair** — that was a specific C67 design call (Storm card needs something to visibly change on
cast), not something this merge should silently override; Sunny is reachable via `ApplyWeather`/
`ToggleSunnyStorm`. Real engineering finding along the way: git's automatic 3-way merge produced two
separate cases of silent corruption — a call to a method that no longer exists in
`BoardWeatherPocket.cs` (master and the branch had each independently rewritten the same lighting-restore
system under different names; the line-based merge spliced a call from the wrong side into the wrong
function, *outside* any marked conflict) and literal leftover conflict markers baked into
`Assets/ExplosiveLLC.meta`'s content after a rename/rename conflict. Both caught by hand-verifying the
merge result against each branch's clean version rather than trusting `git merge`'s silent regions —
worth remembering for any future merge of a long-diverged branch. Dropped the branch's stray
`Assets/_Recovery` scene (junk); resolved a root-level `MATCH_SHELL_LAYOUT_AGENT_BRIEF.md` naming
collision by relocating the branch's differently-scoped brief into `docs/departments/atmosphere/`.
Batchmode-verified fresh, Editor closed: **EditMode 190/190, PlayMode 65/65.**

**2026-08-19: dispatched a UI shell-chrome restyle** (Character Select, Play/Confirm buttons, Boot/
Lobby/Map Select/Match End — explicitly not the in-match HUD) after the human called the current look
"totally unacceptable." Drew from the human's collected `docs/UI_CHROME_COLLECTION.md`/`docs/ui-collection/`
resources (Uiverse card/button references, CC0 Iomanoid display font, clay-icon style lock). Landed on
its own worktree (`logiCard-ui-restyle`, branch `worktree-agent-a035c5b8c7a5428af`), **not yet merged to
master** — human is reviewing live in the Editor before that call. New `UiFactory.CreateShell*` helper
family + `ShellButton.cs`; new `docs/ui/UI_SHELL_CHROME.md` chrome contract (pointed to from root
`CLAUDE.md`). Caught a real bug via its own screenshot harness that every batchmode test missed
(Character Select rendered completely empty — a sibling-ordering bug buried content behind the new
backdrop). Batchmode on that worktree: EditMode 190/190, PlayMode 64/64 (independently re-verified, not
just self-reported).

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

**2026-08-18 (restaffing pass): Cards docs rebase closed + Character Select carousel merged.**
Cards' `feat/cards-collection-docs` rebased onto master with zero diff on real doc content (already
folded in earlier); only STATUS.md pulled forward (`47baf50`). Character's `feat/char-select-motion`
(the 2-item center/flank carousel, `CharacterSelectView.cs`/`UiMotion.cs`, Kenney CharSelect chrome)
rebased clean and was human-approved to merge (`9472783`) despite the branch's own later commits
recording that Character Select UI ownership had shifted to the UI department — landed as-is; **UI
department now owns this code going forward**, not Character. `ArchetypeOf(pawnId)` InfoBar reader
remains unwired (Match-Shell/UI scope, untouched by this branch). Batchmode-verified fresh on `master`
post-merge, Editor closed: **EditMode 190/190, PlayMode 63/63** (the +1 is
`CharacterSelectNextRotatesArchetypeAfterCrossfade`). Cards and Character worktrees can go idle;
Atmosphere stays parked (Sunny mode, blocked on human decision).

**Milestone:** Phase 5 Commercial Art Bar (active). Phase 2 Net paused (C63/C67 gear carve-out).
**Integrator tip:** `master` @ `ecf0093` — **Atmosphere's storm-rolling-in transition merged** (see 2026-08-20 note above), on top of C36/Bomber wall-only verb (Sim layer), the UI shell-chrome restyle, TPS camera-wiggle fix, Sunny weather mood, camera control-hint UI chrome, dead Bandage/Storm board-tap prune, Storm per-match counter, Healed presenter, and Camera merge. Batchmode-verified independently: EditMode 196/196, PlayMode 67/67. Nothing else in flight off `master` right now.
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

**(Table below reflects 2026-08-17 state; superseded by `departments/INDEX.md`'s Live Folders table, which the 2026-08-18 restaffing pass kept current — Cards and Character are merged/idle there, not pending as this older snapshot still reads.)**

| Seat | Folder | Tip / state |
|------|--------|-------------|
| Integrator | `logiCard` | `master` @ `bb6fcdf` — everything through the 2026-08-18 restaffing pass merged, batchmode-verified (see Verification) |
| **UI** | `logiCard-modal-restyle` | `feat/modal-restyle` @ `e1c80fb` — fully merged to master; worktree can resync/idle |
| Atmosphere | `logiCard-atmosphere-stylized` | **Sunny weather mood merged to master** (`0857b80`, 2026-08-20) — stray `Assets/_Recovery` scene dropped, not carried over; worktree idle |
| **UI (restyle)** | — | **Merged to master** (`546ba31`, 2026-08-20), human-approved; worktree removed |
| Cards | `logiCard-cards-collection` | **Rebased + fully reconciled onto master** (`47baf50`, 2026-08-18); worktree idle |
| Character | `logiCard-char-select-motion` | **Carousel feature rebased + merged to master** (`9472783`, 2026-08-18); UI dept now owns this code going forward, not Character; worktree idle |
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
   logged above, `bd5fad8`), not actually a pending task. **2026-08-18: human Play-approved** the shadow
   tune and the control-hint chrome move — both now fully closed, no further verification owed.
2. **Correction (2026-08-19):** this item was stale — Character's carousel merged (`9472783`) and Cards' branch rebased/reconciled (`47baf50`) in the 2026-08-18 restaffing pass (see entry above); both worktrees are idle, not pending. Only Atmosphere's Sunny mode remains genuinely parked (uncommitted in its worktree, blocked on a human merge/drop decision, branch pushed to origin for safekeeping).
3. **New flag (2026-08-19):** `CHARACTER_FANTASY.md` §4.1's `ArchetypeOf(pawnId)` InfoBar reader is still unwired (Character STATUS confirms, carried forward through their 2026-08-18 rebase without touching it). Investigated: this is entangled with C73's larger "Character Select → live attrs wiring gap" — `GameBootstrap` currently hardcodes both pawns' archetype (`PawnBuild.Scout`/`attackerSecondsPerTile = 1f` for the attacker, `PawnBuild.Juggernaut`/`DefenderSecondsPerTile = 2f` for the defender) rather than reading `AppFlowController.SelectedArchetype` or a real `CharacterData` asset at all — so a reader built today would honestly always report "Scout"/"Juggernaut" regardless of what the player picks at Char Select, and would need rework once C73's attrs contract actually lands. Also: `CHARACTER_FANTASY.md` §4.1's recommended two-column (Attacker\|Defender) InfoBar layout is an explicit "Waiting on human" item in Character's own STATUS.md (item 5, "Confirm or override InfoBar §4.1 recommendations") — not yet confirmed, so building real layout for it now would be presuming an undecided design, the same class of call this session has been checking with the human on rather than assuming. Not started; flagging rather than guessing.

## Tomorrow

1. This dispatch round is closed — Match Shell, Map, and Camera all merged to master, all independently batchmode-verified. No paused dept work outstanding.
2. Every backlog item from 2026-08-17 (Healed presenter, Storm per-match counter, dead board-tap prune, IMGUI control-hint → UI chrome) closed 2026-08-18. **Next actual next-step is a human Play pass** over the shadow tune + control-hint chrome (both batchmode-green, neither eyeballed yet) — after that, restaff an idle department (Atmosphere Sunny decision, Character carousel, Cards reconciliation) or open a new Phase 5 wave.

## Blockers / notes

- **You are Integrator on main** unless continuing dept coding in a worktree — never two agents on the same tree.
- All worktree branches pushed to `origin` 2026-08-17 (human switching machines) — `master` itself still needs an explicit push, see note below.
