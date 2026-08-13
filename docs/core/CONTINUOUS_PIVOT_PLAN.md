# Continuous-Space Architecture Pivot — Phased Plan

**Status:** SHIPPED — verified against code on disk (2026-08-08); every phase in this plan is implemented and
live (`Assets/_Project/Sim/PlanarPosition.cs`, `Segment.cs`, `ArenaBoard.cs`, `ContinuousLineOfSight.cs`,
`ContinuousPathfinder.cs` all exist; the old grid files this plan deletes are gone). Kept as historical
reference for the continuous-space architecture's reasoning, not an in-flight plan. Not part of the **C46**
scope pivot's active work — that pivot doesn't touch movement/combat mechanics. Confirmed 2026-08-03 —
supersedes **C35**'s "long-term only" framing — continuous movement was, at the time, current demo scope.
**Depends on:** [PRODUCT_MEMORY.md](PRODUCT_MEMORY.md) (decisions recorded as CONFIRMED entries), [DAY7_DOOR_RESEARCH.md](DAY7_DOOR_RESEARCH.md) (the door mechanic being adapted here, not re-derived).

---

## A. Why this doc exists

The grid-based demo (Days 1–7 + the C21 waypoint-authoring rework) is functionally complete and fully test-verified (91 EditMode / 23 PlayMode tests, all green), but a cold-observer playtest surfaced that the underlying map/movement model itself doesn't match the intended design — the game needs continuous position, not a discrete grid. This reverses the earlier **C35** decision ("continuous movement... long-term only; demo stays grid-based"), made with the full cost understood: this pivot touches nearly every file in the movement/combat pipeline and was estimated at **5.5–7.5 engineer-days** against a 14-day schedule with **zero slack days**. The user chose to proceed and compress the art/polish pass (Days 8–14) to absorb the cost rather than push the ship date — see Decision 2 below and `SCHEDULE.md`.

A dedicated design pass found real leverage before any code was written: `Sim/ScheduledPath.cs` already defines `PlanarPosition`, a continuous position type, and it's already the *only* thing `Board/BoardView.cs`/`Board/PawnView.cs` render from — `GridCoordinate` is only the **authoring/resolve-time** source of truth, never the rendering path. This pivot is a **retarget of the authoring/resolve layer onto an already-continuous rendering layer**, not an invention of continuous math from nothing. Two direct consequences:

- `Board/PawnView.cs` needs **zero changes**.
- The only place currently snapping to a grid is `PlanarPosition.ToNearestCoordinate()`, called in exactly two places (`GhostResolver.GhostTrack.TileAt()`, `RoundPlayback.CommitRoundState()`) — both disappear cleanly, and dropping the round-carry snap is a correctness improvement (no more rounding drift between rounds), not just a simplification.

---

## B. Decisions locked (binding — see `PRODUCT_MEMORY.md` for the CONFIRMED entry)

1. **Shoot targeting → free-aim point.** Tap anywhere in bounds. Snap Shot hits if any pawn is within `HitRadius` of the aim point at completion; Hold Angle hits if a pawn's path crosses within `LaneHalfWidth` of the origin→aim line during its window. No pawn-ID lock, no target reference in `ActionNode` — the player is still betting on a *place*, not a *person*, which is the same epistemics the row/column rule gave the blind-programming bluff design. Tunable constants, start around 0.4–0.5 world units (~half a pawn-width), tune in Phase 6.
2. **Schedule handling → compress the art/polish pass, keep the Day 14 ship date.** `SCHEDULE.md`'s existing cut-order (Android smoke → door reopen nuance → Crawl AV nuance → optional DoF/SSS) gets used more aggressively than originally planned.
3. **Pathfinding → visibility graph + Dijkstra.** Graph nodes = start, goal, and every obstacle-segment endpoint (nudged outward by a small clearance epsilon); an edge exists between two nodes iff the straight segment between them doesn't intersect a blocking segment (reusing the same segment-intersection primitive LoS needs anyway); shortest path by Euclidean edge weight, deterministic tie-break on node insertion order. Right-sized for "an open arena with a couple of wall segments and one door" — not a navmesh, not a rediscretized grid.
4. **Door interaction → radius-based, not tile-adjacency.** A continuous click will essentially never land exactly on a door's geometry. `PawnProgram.TryQueueDoor` takes a `Door` reference (resolved by the input layer via a new `ArenaBoard.TryGetNearestDoor(point, maxDistance, out Door)`), legality = `Segment.DistanceToPoint(door.Segment, CurrentPosition) <= InteractRadius`.
5. **Hold Angle contact detection → analytic segment-vs-lane sweep, not instant-probing.** `GhostResolver.TryFindHoldContact` today samples a handful of discrete instants (window start/end + every victim `Move` node's `ExecuteTime` inside the window) — this only worked on the grid because `TryCommitDraft` emitted one `ActionNode` per tile, so sampling was densely spaced by coincidence. A continuous leg can be one long straight line between two samples, so a victim could sweep through the lane strictly between them undetected. Needs a closed-form point-to-line-distance-over-time check: the victim's position is linear in `t` over a leg, so distance-to-the-aim-line-squared is a quadratic in `t` — solve for where it drops below `LaneHalfWidth²` within the overlapping range. Still pure engine-free math, still deterministic (**C32**'s "never a physics raycast" carefulness extends here — no `Physics`/`Physics2D` calls anywhere in this pivot). This is the one genuinely new algorithm in the pivot, not a mechanical port — budget real test coverage for it.
6. **Multi-floor/Attic → keep the `Floor` field on `PlanarPosition`, build no per-floor obstacle infrastructure.** `Floor.Attic` is already vestigial (`GameBootstrap.BuildBoard()` only ever constructs `Floor.Ground`); keeping the field costs nothing and preserves `HasLineOfSight`'s same-floor check, but a second obstacle set for the attic is out of 14-day scope regardless of this pivot.
7. **Board scale unchanged** — keep the same `[0,4] × [0,4]` numeric footprint so camera framing, HUD numbers, and the `DAY7_DOOR_RESEARCH.md` wall/door layout port over at the same magnitudes rather than needing a rescale.

---

## C. Phase breakdown

Each phase compiles/tests independently. Phases 1→2→3→4 are a strict sequential critical path (each retargets the previous phase's consumers); **Phase 1 and Phase 5 are the two genuinely parallelizable slices**.

### Phase 0 — Decisions lock
Done — see Section B. Nothing below should start coding ahead of this, since Phase 1's public API shapes depend on it.

### Phase 1 — Continuous geometry primitives (`Sim/`, engine-free, no MonoBehaviour dependency)
New files, all in the `LogiCard.Sim` asmdef (`noEngineReferences: true` — must stay true):
- **`PlanarPosition.cs`** — promoted out of `ScheduledPath.cs` into its own file. Add `IEquatable<PlanarPosition>`, `DistanceTo`, `SqrDistanceTo`, `+`/`-` operators, `Lerp`, `ToString()`. Drop `ToNearestCoordinate()` entirely. Keeps the `Floor` enum (currently on `GridCoordinate.cs`, which is deleted in Phase 4).
- **`Segment.cs`** — `readonly struct Segment { PlanarPosition A, B; }` + `Intersects(Segment)`, `DistanceToPoint(PlanarPosition)`, `ClosestPointOnSegment(PlanarPosition)`, `ProjectParam(PlanarPosition)` (t along AB). Doc-commented the same way `GridLineOfSight` is today: pure cross/dot-product math, never `Physics`/`Physics2D`.
- **`ArenaBoard.cs`** (replaces `GridBoard`) — continuous bounds, `List<Segment> Walls` (static), `List<Door> Doors`, `RegisterWall`, `RegisterDoor`, `TryGetDoor`, `TryGetNearestDoor(point, maxDistance, out Door)` (Decision 4), `InBounds(PlanarPosition)`, `IsBlocking(Segment probe)` (checks walls + currently-closed doors). `Clone()` only needs to snapshot door *state* (walls never move) — simpler than today's full tile-array `GridBoard.Clone()`.
- **`ContinuousLineOfSight.cs`** (replaces `GridLineOfSight`) — `HasLineOfSight(ArenaBoard, from, to)` = same-floor + in-bounds + `!board.IsBlocking(new Segment(from, to))`; `IsOnLane(origin, aim, point, halfWidth)` replaces `IsOnCoveredLane` via point-to-segment distance + t-in-[0,1].
- **`ContinuousPathfinder.cs`** (replaces `OrthogonalPathfinder`) — visibility graph + Dijkstra per Decision 3.
- Tests: `ArenaBoardTests.cs`, `ContinuousLineOfSightTests.cs`, `ContinuousPathfinderTests.cs` — reuse the scenarios from `GridBoardTests`/`GridLineOfSightTests`/`OrthogonalPathfinderTests` (clear sight, blocked by obstacle, endpoints don't self-block, different floors, out of bounds, straight-line-when-clear, routes around a wall through the gap, fails when unreachable), reworded for points/segments.

Old grid files (`GridCoordinate.cs`, `GridBoard.cs`, `GridLineOfSight.cs`, `OrthogonalPathfinder.cs`) are **untouched** in this phase — nothing points at the new files yet, so this phase cannot break anything currently green.

**→ This is the slice handed to a second agent** (see `docs/DRAFT_HANDOFF.md` for the worktree/branch it's running in). No dependency on any other phase; spec'd entirely by Section B.

### Phase 2 — Sim/Net consumer retarget (sequential, depends on Phase 1)
- `ScheduledPath.cs`: `nodes` → `List<PlanarPosition>`; delete the embedded `PlanarPosition` struct (moved to its own file in Phase 1); `Evaluate()` gets simpler (no more grid-to-planar conversion step).
- `TimeResourceMath.cs`: the `GridCoordinate` overload of `SegmentSeconds` → a `PlanarPosition` overload using `DistanceTo` (Euclidean, not Manhattan). The dimension-agnostic `SegmentSeconds(float distanceTiles, ...)` primitive overload needs **zero changes**.
- `StanceAllotment.cs`, `StanceType.cs`, `ShootMode.cs`, `ShootCost.cs`, `Net/ActionVerb.cs`: **confirmed zero changes** — none reference a coordinate type.
- `Door.cs`: `Coordinate` (GridCoordinate) → `Segment`; `DoorState`/`InitialState` unchanged.
- `Net/ActionNode.cs`: `GridPosition` → `Position` (PlanarPosition); everything else unchanged.
- `Net/TapeEvent.cs`: `Coordinate` → `Position` (PlanarPosition); `TapeEventType` enum unchanged.
- `Net/GhostResolver.cs` — largest single file, but a **retarget, not a redesign**: the event-sweep / simultaneity-epsilon / group-then-apply / scratch-board-clone shape in `ResolveShots`/`ApplyDoorGroup` is preserved verbatim (this is exactly the Day-7 door machinery, just pointed at `ArenaBoard`). Changes: `_board` → `ArenaBoard`; `GhostTrack.TileAt` → `PositionAt` (drops `.ToNearestCoordinate()`); `ResolveSnapShot` becomes distance-based (`victim.PositionAt(t).DistanceTo(shot.Aim) <= HitRadius`); door toggles apply to `ArenaBoard`'s door-state snapshot instead of `scratch[coordinate] = new Tile(...)`; `TryFindHoldContact` gets the Decision-5 analytic sweep.
- `Net/ReplayTape.cs`, `Net/TimelinePayload.cs`: **confirmed zero changes**.

Test rewrite: `GhostResolverTests.cs` (every scenario carries over conceptually — retype literals, add explicit sweep-between-two-samples coverage for Decision 5); `DoorTests.cs`'s resolver section (retype helpers, keep the same 5 door-toggle narratives).

**Not whole-project-green at the end of this phase** — `Sim`/`Net` compile and pass; `Timeline`/`Board`/`Boot` still reference old grid types via `PawnProgram` and won't compile until Phase 3. Should be a same-day, short-lived state.

### Phase 3 — `Timeline/PawnProgram.cs` retarget (sequential, depends on Phase 2)
Types swap to `PlanarPosition`/`ArenaBoard`; `TryDraftPath`/`TryAddWaypoint` call `ContinuousPathfinder.TryFindPath`. **The `if (tile == tip || _draftWaypoints.Contains(tile)) reject` guard is deleted outright** — nothing replaces it, revisiting/crossing a prior point becomes legal (this resolves the user's second ask as a side effect of this phase). Draft cost changes from "1 cost-unit per waypoint" (`StanceAllotment.CostForTiles(DraftTileCount, ...)`, correct only because each waypoint happened to be exactly one grid tile) to "sum of Euclidean leg lengths" in `RecomputeDraftCost` — a real behavior fix needing its own new tests, not inherited from the grid ones. `TryQueueShoot` drops the row/column constraint (Decision 1). `TryQueueDoor` takes a `Door` reference + `InteractRadius` check (Decision 4).

Test rewrite: `PawnProgramTests.cs` — most scenarios carry over (move cost math, budget rejection, stance costs, shoot cost/mode, node-ordering on `Build()`); `AddWaypoint_NonAdjacentTile_AppendsShortestLegFromTip` needs a continuous-path assertion; `QueueShoot_OffRowAndColumn_IsRejected` is **deleted** (constraint no longer exists).

End of phase: `Timeline` compiles and passes; `Board`/`Boot`/`UI` still don't compile.

### Phase 4 — Unity view / composition-root retarget (sequential, depends on Phase 3)
- `Board/BoardView.cs`: `Build(GridBoard,...)` → `Build(ArenaBoard,...)`; delete the per-coordinate cube-per-tile loop + `TileMarker` attach; one ground-plane collider/mesh sized to `ArenaBoard`'s bounds, plus one thin box per wall `Segment` and one per door `Segment` (color reflects open/closed). `LocalFromPlanar`/`WorldFromPlanar`/`CenterWorld` keep their current shape almost unchanged. Add `PlanarFromWorld(Vector3)` (inverse, for the new raycast flow). Delete `WorldFromCoord(GridCoordinate)`.
- `Board/TileMarker.cs`: **deleted**.
- `Board/PathPreviewView.cs`: `Show(IReadOnlyList<GridCoordinate>,...)` → `Show(IReadOnlyList<PlanarPosition>,...)`; mechanical.
- `Board/PawnView.cs`: **confirmed zero changes.**
- `Board/BoardInputController.cs`: raycast target becomes the ground-plane collider; hit point converts via `PlanarFromWorld`; `Origin`/`_origin`/`TryTapTile` retype; door taps resolve through `ArenaBoard.TryGetNearestDoor` before calling `PawnProgram.TryQueueDoor`; rest of the draft/commit/preview flow structurally unchanged.
- `Boot/GameBootstrap.cs`: `BuildBoard()` constructs `ArenaBoard` with continuous bounds, registers the wall/door segments (continuous translation of the `DAY7_DOOR_RESEARCH.md` wall-with-gap layout); spawns and scripted defender waypoints become `PlanarPosition` literals.
- `Boot/RoundPlayback.cs`: `PawnEntry.CurrentPosition` → `PlanarPosition`; `CommitRoundState()` drops `.ToNearestCoordinate()` (correctness improvement); tracer building updates for `TapeEvent.Position`.
- **Delete** `Sim/GridCoordinate.cs`, `GridBoard.cs`, `GridLineOfSight.cs`, `OrthogonalPathfinder.cs` — first point nothing references them.
- `Board/ShotTracerView.cs`, `Board/PrimitiveMaterialFactory.cs`, `Characters/CharacterData.cs`, `Cards/CardData.cs`: **confirmed zero changes.**

**First whole-project-green checkpoint since before Phase 2** — the game is compilable and playable again here.

### Phase 5 — HUD wording + PlayMode test rewrite (parallelizable once Phase 4 lands)
- `UI/ProgramHud.cs`: display-string-only changes, no control-flow changes. `node.GridPosition` → `node.Position`; `"PATH {DraftTileCount} tile(s)"` → `"PATH {DraftDistance:0.0}m"`; shoot-mode label wording updates per Decision 1 (no more "tap a tile on your row/col").
- PlayMode test fixtures: `SliceSceneFixture.cs` (`Home`/`MoveSeconds` retype), `BoardInputPlayModeTests.cs` (the two `TileMarker`-based tests deleted, replaced by a single ground-plane-raycast test; off-row/off-column rejection test deleted), `RoundPlaybackPlayModeTests.cs` (literal retypes, exact-tile assertions → distance-within-tolerance), `ProgramHudPlayModeTests.cs` (label-text updates).

Essentially zero architectural risk — cleanest handoff to a second engineer, needs only Phase 4's final API surface, nothing in-flight.

### Phase 6 — Tuning pass
Tune `HitRadius`/`LaneHalfWidth`/`InteractRadius` against real play; place wall/door segments for the same "door changes a fight once" single-playtest readability bar `CORE_LOOP.md` already sets — the direct continuous-space equivalent of the original Day 7 exit criterion, validated the same way (cold-observer playtest, not code inspection).

---

## D. Confirmed zero-change files (verified by grep, not assumed)

`MatchClock.cs`, `MatchSide.cs`, `PhaseStateMachine.cs`, `RoundPhase.cs`, `RoundPhaseController.cs`, `TimeResourceClock.cs`, `TimeResourceClockDriver.cs`, `Cards/CardData.cs`, `Net/ReplayTape.cs`, `Net/TimelinePayload.cs`, `Net/ActionVerb.cs`, `Sim/StanceType.cs`, `Sim/ShootMode.cs`, `Sim/ShootCost.cs`, `Board/PawnView.cs`, `Board/ShotTracerView.cs`, `Board/PrimitiveMaterialFactory.cs`, `Characters/CharacterData.cs`, and their tests (`MatchClockTests.cs`, `PhaseStateMachineTests.cs`, `TimeResourceClockTests.cs`).

## E. Test strategy

- **Delete outright** (grid-specific, no continuous equivalent worth keeping): `GridBoardTests.cs`, `GridLineOfSightTests.cs`, the `OrthogonalPathfinderTests` fixture inside `PathStanceTests.cs`, the two `TileMarker`-based tests in `BoardInputPlayModeTests.cs`, `PawnProgramTests.QueueShoot_OffRowAndColumn_IsRejected`.
- **Keep verbatim**: `StanceAllotmentTests` (already parametrized on `float distanceTiles`, zero grid dependency), `MatchClockTests.cs`, `PhaseStateMachineTests.cs`, `TimeResourceClockTests.cs`.
- **Reword in place**: `GhostResolverTests.cs`, `DoorTests.cs`, most of `PawnProgramTests.cs`, `SliceSceneFixture.cs`, remaining PlayMode scenarios.
- **New**: `ArenaBoardTests.cs`, `ContinuousLineOfSightTests.cs`, `ContinuousPathfinderTests.cs`, explicit segment-sweep coverage for Decision 5.

## F. Schedule risk (stated plainly, per the locked Decision 2)

Per `SCHEDULE.md`, this pivot starts at Day 7 (Door in progress) with Days 8–14 (7 days) originally earmarked for URP/art/audio/ship. The sequential critical path (Phase 1→2→3→4) is ~5.5–7.5 engineer-days on its own, before Phase 5/6 — i.e., most of the remaining runway. Running Phase 1 in a parallel worktree and handing Phase 5 to a second engineer once Phase 4 lands compresses this, but the art/polish pass (Days 8–14) should be planned as **compressed**, not full-scope, from here forward. `SCHEDULE.md`'s existing cut-order applies more aggressively than originally planned.
