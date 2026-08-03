# Draft Handoff — 2026-08-03 (continuous-space pivot)

**Current focus:** the grid-based demo (Days 1–7 + C21 waypoint rework) is complete and fully verified (91 EditMode / 23 PlayMode green), but a cold-observer playtest surfaced that the map/movement model itself needs to be continuous, not a grid — see `docs/CONTINUOUS_PIVOT_PLAN.md` for the full phased plan and `PRODUCT_MEMORY.md` **C35** (promoted from long-term-only to current demo scope) / **C39** (the specific technical decisions). This reverses C35's original framing, deliberately, with the schedule cost (~5.5–7.5 engineer-days) accepted and absorbed by compressing the art/polish pass rather than moving the Day 14 ship date — see `SCHEDULE.md`'s new **M2.5** milestone (Days 7b–7g).

**Git state:** `master` HEAD is now the Day 7 door + C21 checkpoint (see History below) plus doc commits recording the pivot. Two other worktrees exist:
- `D:/projects/Game/logiCard-verify` (branch `verify/day5-6-tests`) — Day 5/6 verification, **done and green**, not yet merged into `master`.
- `D:/projects/Game/logiCard-continuous-phase1` (branch to be created) — Phase 1 of the pivot (continuous geometry primitives), handed to a second agent. See the brief written into that worktree once it exists.

## What's still open (needs a human, not automatable)

1. **Reconcile `verify/day5-6-tests` into `master`** — still not done, user's call on how (was already open before the pivot, still open now).
2. **Day 4/5/6 cold-observer DoD** on the grid version — logic verified green, nobody watched it run. Somewhat moot now that the board is being rebuilt continuous, but the DoD bar itself (readable Sprint/Walk/Crawl, readable Snap/Hold) carries forward to the **M2.5 playtest** (Day 7g) instead.
3. **Pawn-vs-pawn collision on a continuous board is undecided** — flagged as OPEN in `GDD.md` §3.3. The old "cannot share a tile" rule has no direct continuous equivalent and wasn't addressed by the Plan agent's design pass. Needs a decision before Phase 6 tuning (minimum-separation radius vs. no pawn-blocking at all, since wounds already come from Shoot not contact).
4. Re-save `Bootstrap.unity` if stale pre-MatchClock serialized fields remain (flagged 07-30, never re-checked — still open, unrelated to the pivot).

## Continuous pivot — where to pick up

Read `docs/CONTINUOUS_PIVOT_PLAN.md` first — it has the full phase breakdown, the 7 locked technical decisions, file-by-file change list, and confirmed zero-change files. Short version:

- **Phase 1** (geometry primitives — `PlanarPosition`, `Segment`, `ArenaBoard`, `ContinuousLineOfSight`, `ContinuousPathfinder`) → second agent, parallel worktree, self-contained.
- **Phase 2→3→4** (Sim/Net retarget → PawnProgram retarget → Unity views) → sequential, main worktree, blocked on Phase 1 landing.
- **Phase 5** (HUD wording + PlayMode tests) → parallelizable again once Phase 4 lands.
- **Phase 6** — tuning pass + the pawn-collision decision above.

---

## History (pre-pivot, for context — grid-based work, now being superseded)

**Committed at `d474bde`:** doc-consistency sync from 07-30/07-31 + original C35–C38 long-term-vision entries (before today's promotion).

**Committed at `1b1eb1f`:** Day 5 (path + stance, grid) + Day 6 (Hold Angle vs Snap Shot).

**Committed at `2e40e60`:** Editor bumped to 6000.5.5f1 (test-framework 1.7.0, ugui 2.5.0) — picked up automatically when the project was opened with the newer locally-installed Editor version.

**Committed at `53b117d`:** Day 7 door (Sim/Net layer only — `Door`/`DoorState`, `GridBoard` registration + `Clone()`, `ActionVerb.Door`, `ActionNode.DoorAction`, `TapeEvent` Door events, `GhostResolver`'s time-ordered door-toggle sweep) + the C21 amendment (removed the time-allotment slider; `TryAddWaypoint` replaces `TryExtendOrReplaceDraft`, appending the shortest leg from the draft's tip instead of replacing the whole draft on a non-adjacent tap). Verified 91/91 EditMode, 23/23 PlayMode at the time. Door UI wiring (`BoardInputController`/`ProgramHud`) and board layout (`GameBootstrap`) were never done on the grid model — moot now, since Phase 4 of the pivot rebuilds that layer continuous from the start rather than porting a grid UI that would just be deleted again.

Two pre-existing test bugs found and fixed along the way (unrelated to Day 7/C21, now historical): a stale NUnit `Does.Not.Contain(GridCoordinate)` overload in `GridLineOfSightTests.cs`, and a stale `SliceSceneFixture.FindByName` missing inactive objects. Both independently rediscovered and fixed by the parallel verify-worktree agent too.

SET PATH = explicit "book draft into budget"; Lock In / Shoot also commit the draft — this authoring shape survives the pivot (Phase 3 keeps `PawnProgram`'s draft/commit flow, just retargets its coordinate type).
