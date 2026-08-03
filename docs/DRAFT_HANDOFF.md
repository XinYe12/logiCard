# Draft Handoff — 2026-08-03

**Schedule position:** Day 5 (path + stance) and Day 6 (Snap vs Hold Angle) are both implemented in code and now **committed** to `master`. Day 7 (one door + local E2E, M2) is starting now. Days 4–6 still unchecked in `SCHEDULE.md` — code exists but no cold-observer / DoD sign-off pass has happened yet. Stick with plan — core combat through Day 7, art Day 8+.

**Two active worktrees as of today (do not cross the streams — see Blockers):**
- `D:/projects/Game/logiCard` (`master`, HEAD `1b1eb1f`) — main worktree. Day 7 door implementation happening here.
- `D:/projects/Game/logiCard-verify` (branch `verify/day5-6-tests`, forked from `1b1eb1f`) — isolated worktree for a second agent to batch-verify Day 5/6 without fighting the main worktree's open Unity Editor for the project lock. Scope: `Assets/_Project/Tests/**` only, plus SCHEDULE.md checkbox ticks once verified. See the brief handed to that agent for the full boundary.

## Implemented

**Committed at `d474bde`:** doc-consistency sync carried over from 07-30/07-31 (GDD §6 Time Card presets, PRODUCT_MEMORY C33 numerics, Day-3b-absorbed schedule note) plus **C35–C38** — long-term-only vision entries (continuous movement, destructible geometry, asymmetric objective win, Downed/revive/Detonator). None of C35–C38 touch the 14-day demo; C17/C34 still govern the ship.

**Committed at `1b1eb1f`:**
- Day 5 — path + stance (C21): `Sim/OrthogonalPathfinder.cs` (BFS orthogonal routes), `Sim/StanceAllotment.cs` (tile count × base → Sprint/Walk/Crawl bands), `Timeline/PawnProgram.cs` draft→commit flow, `Board/BoardInputController.cs` + `Board/PathPreviewView.cs` (path bead preview), `UI/ProgramHud.cs` stance slider/SET PATH.
- Day 6 — Hold Angle vs Snap Shot (C25/C32): `Sim/ShootMode.cs`, `Sim/ShootCost.cs`, `Net/GhostResolver.cs` now distinguishes `ResolveSnapShot` (completion-second, aimed-tile-only, misses Sprint) from `ResolveHoldAngle` (covers the aim lane across its window, hits Sprint, lethal on contact via `TryFindHoldContact`), with same-second shots grouped by `_simultaneityEpsilon` before wounds apply.
- Tests: `PathStanceTests.cs` (new) + updated `GhostResolverTests`, `GridLineOfSightTests`, `PawnProgramTests`, `BoardInputPlayModeTests`, `ProgramHudPlayModeTests`, `RoundPlaybackPlayModeTests`.

**Day 7 research (committed at `d474bde`, code not started until this session):** `docs/DAY7_DOOR_RESEARCH.md` — read-only survey of the door feature against current code, with 7 open questions in its Section H.

## Day 7 open questions — resolved today (see research doc Section H for full context)

Defaults adopted so coding can proceed without blocking on all 7:

1. **Tile-occupies-the-door model** (not a true edge type) — adopted as proposed. Reuses `Tile.IsPassable`, zero new data structures.
2. **Time-varying board state during resolve** — adopted **Option 1**: a scratch `GridBoard` mutated in `ExecuteTime` order as `GhostResolver` walks door-toggle nodes alongside shots/moves, applied via the existing `this[GridCoordinate]` indexer. Chosen over Option 2 (a new `IsPassableAt(coord, seconds)` query) because it needs no new `GridLineOfSight` signature — lower surface area against a determinism-sensitive API (**C32**'s "never a physics raycast" carefulness extends to not casually growing that class's surface either).
3. **Symmetric open/close cost** — adopted. GDD §6 gives one `doorInteractBaseSeconds` number per character; both directions charge it.
4. **Default Closed + cross-round persistence** — adopted. Door starts Closed each match; state **carries** across rounds the same way position/wounds already do under C33.
5. **Simultaneous Open vs Close tie-break** — adopted: Close wins over Open when both land in the same `_simultaneityEpsilon` group (fails toward blocking, not exposing); ties within that broken by lower `PawnId`, mirroring the existing shot-ordering tiebreak in `GhostResolver.ResolveShots`.
6. **Legal interact tiles** — adopted: door tile itself or its four orthogonal neighbours (`GridCoordinate` orthogonal-neighbour convention already used by `OrthogonalPathfinder`). Both players may interact the same door in the same round; question 5's tiebreak resolves conflicts.
7. **Board layout** — adopted the research doc's proposed 5×5 wall-with-single-gap layout (door at `(2,2)`, wall at `(0,2)/(1,2)/(3,2)/(4,2)`, spawns `(2,0)`/`(2,4)`) — smallest board that makes the door load-bearing per `CORE_LOOP.md`'s "door changes a fight once" bar.

**Not yet resolved / still needs a human look:** none of these are irreversible — flag if any read wrong once playable.

## Day 7 — implemented this session (uncommitted, main worktree)

Sim/Net/Timeline layer only — the deterministic core, done first because it's the part correctness actually depends on:

- `Sim/Door.cs` (new) — `Door`/`DoorState`, tile-occupies-the-door model (H1).
- `Sim/GridBoard.cs` — `RegisterDoor`/`TryGetDoor`/`Doors`, plus `Clone()` for the resolver's scratch board.
- `Net/ActionVerb.cs` — added `Door`.
- `Net/ActionNode.cs` — added `DoorAction` enum + `Door` field (mirrors the `ShootMode` bolt-on pattern).
- `Net/TapeEvent.cs` — added `DoorOpened`/`DoorClosed`.
- `Net/GhostResolver.cs` — `ResolveShots` now merges door toggles and shots into one time-ordered sweep against a per-resolve scratch board (`_board.Clone()`); toggles land before same-instant shots; simultaneous Open+Close on the same door resolves Closed. `ResolveSnapShot`/`ResolveHoldAngle`/`TryFindHoldContact` take the scratch board instead of reading `_board` directly.
- `Timeline/PawnProgram.cs` — `TryQueueDoor(doorTile, action, out reason)`, `DoorInteractSeconds` (ctor param, default 4s matching Scout).
- `Tests/EditMode/DoorTests.cs` (new) — board registration, closed-blocks/open-allows/close-blocks-again LoS, simultaneous tie-break, and `TryQueueDoor` legality/budget cases. **Verified green** — see Verification below.

**Not started yet (explicitly deferred to the next pass, not forgotten):**
- `Board/BoardInputController.cs` + `UI/ProgramHud.cs` wiring — a `DOOR` verb button + Open/Close sub-row (mirrors the existing Snap/Hold row exactly; `ProgramHud` builds its UI procedurally in code, no scene editing needed).
- `Boot/GameBootstrap.cs` board layout — the research doc's wall-with-single-gap 5×5 (door at `(2,2)`, wall at `(0,2)/(1,2)/(3,2)/(4,2)`, spawns `(2,0)`/`(2,4)`) is not wired in yet; **current spawns are still `(0,0)`/`(4,4)` with no wall at all**. This also touches `BuildDefenderPayload`'s scripted route, which will need to route through the door once the wall exists — flagged so whoever does this doesn't get surprised by the scripted defender silently failing to move.

## C21 amended today — waypoint authoring + automatic cost (implemented, verified green)

User playtested Sprint/Walk/Crawl (confirmed working) but found the Move-authoring flow "messy": a manual time-allotment slider that derives stance, redundant with the SPRINT/WALK/CRAWL buttons that already existed. Design call: drop the slider entirely — player picks a stance directly, cost is computed and deducted automatically, no allotment step. Also changed path drawing from "tap a destination, system computes the one shortest path" to "each tap appends a waypoint; consecutive waypoints connect via their own shortest leg" — player controls route shape, not just a single endpoint. Docs amended first (`GDD.md` §3.1/3.2, `CORE_LOOP.md`, `PRODUCT_MEMORY.md` C21, `.cursor/rules/logicard-product-memory.mdc`), then implemented same session:

- `Sim/StanceAllotment.cs` — stripped to just `CostForTiles`; removed `FromAllottedSeconds`/`Normalize`/`LerpAllotment`/`MinSeconds`/`MaxSeconds` (all slider-only, now unused).
- `Timeline/PawnProgram.cs` — removed `TryAllotDraftSeconds` + private `ApplyDraftAllotment`; replaced `TryExtendOrReplaceDraft` with `TryAddWaypoint` (appends the shortest leg from the draft's tip to the tapped tile, instead of replacing the whole draft when the tap isn't adjacent); `TrySetDraftStance`/`SetPreferredStance` now route through a shared `RecomputeDraftCost()`.
- `Board/BoardInputController.cs` — removed `TryAllotDraftSeconds`; `TryTapTile` now calls `Program.TryAddWaypoint`.
- `UI/ProgramHud.cs` — removed the stance-allotment slider (`_stanceAllotSlider`, `OnStanceAllotSliderMoved`, `_suppressStanceSliderCallback`); `_stanceAllotLabel` renamed `_stanceLabel`; `BuildStanceRow` layout tightened now that neither Move nor Shoot has a slider.
- Tests: `PawnProgramTests.cs` — replaced the allotment-forces-stance test with a direct `TrySetDraftStance` equivalent; replaced `ExtendDraft_AddsAdjacentWaypoint` with `AddWaypoint_AdjacentTile_AppendsOneStep` plus two new tests covering the actual behavior change (`AddWaypoint_NonAdjacentTile_AppendsShortestLegFromTip`, `AddWaypoint_RetapPreviousWaypoint_Backtracks`). `PathStanceTests.cs` — removed the now-impossible `FromAllottedSeconds_SnapsToNearestBand` test.

## Verification (this session)

- **Compile:** clean, zero errors, on both the Day 7 Sim/Net layer and the C21 rework.
- **EditMode:** 91/91 green (includes all 13 `DoorTests` and the new waypoint-authoring tests).
- **PlayMode:** 23/23 green.
- Two pre-existing bugs found and fixed along the way (neither related to Day 7 or C21): a stale NUnit `Does.Not.Contain(GridCoordinate)` overload in `GridLineOfSightTests.cs` (→ `CollectionAssert.DoesNotContain`), and a stale `SliceSceneFixture.FindByName` that missed inactive objects (→ added `FindObjectsInactive.Include`), which was hiding `Shoot_Snap`/`Shoot_Hold` while Move mode's controls were showing. Both independently rediscovered and fixed by the parallel verify-worktree agent too — good cross-validation.
- **Not covered by automation:** manual cold-observer read of the new waypoint-tap UX and the stance buttons in a live Editor session — batch mode can't judge feel.

## Still unfinished

- Day 4/5/6 cold-observer DoD passes + SCHEDULE checkboxes — logic verified green, nobody has watched it run yet (this is the verify worktree's job, see above) — now also true for the C21 rework's UX specifically.
- Day 7 UI wiring + board layout (see above).
- Re-save `Bootstrap.unity` if stale pre-MatchClock serialized fields remain (flagged 07-30, never re-checked).
- Art pass stays Days 8–11 — primitives until then by plan.
- Reconcile `master` (Day 7 + C21, uncommitted) with `verify/day5-6-tests` (committed, green) — not done yet, user's call on how.

## Blockers / notes

- **Two worktrees exist as of today** (see above). Main worktree (`logiCard`) owns all production code under `Assets/_Project/{Sim,Net,Board,UI,Timeline,Boot}/**` for Day 7. The verify worktree (`logiCard-verify`) owns only `Assets/_Project/Tests/**` reads/fixes on branch `verify/day5-6-tests` — it must not touch production code or push/merge on its own; the user reconciles both branches by hand once each is done.
- The verify worktree's `Library/` is gitignored and starts empty — first Unity open there will trigger a full asset reimport, expect it to take a while.
- SET PATH = explicit "book draft into budget"; Lock In / Shoot also commit the draft.
