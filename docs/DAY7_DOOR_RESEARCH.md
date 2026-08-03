# Day 7 / M2 — One-Door Research Note

**Status:** Research only, 2026-07-31. Not a confirmed design change — see [PRODUCT_MEMORY.md](PRODUCT_MEMORY.md) (C26). No SCHEDULE.md checkbox ticked by this note.
**Scope:** Day 7 of [SCHEDULE.md](SCHEDULE.md) — the one-door micro-map (contextual open/close; blocks move + LoS) and local match E2E, exit = **M2** Core Combat local playable.

Sources read: [SCHEDULE.md](SCHEDULE.md) (D8 Day 7 row + M2 exit + C34 cut order), [GDD.md](GDD.md) (D4 §4 Door, §6 numerics), [PRODUCT_MEMORY.md](PRODUCT_MEMORY.md) (C17, C34, C32), [CORE_LOOP.md](CORE_LOOP.md) (D3 Program Timeline + §7 invalid moves), [TDD.md](TDD.md) (D6 §4 LoS/authority), [DAY4_GHOST_RESOLVER_RESEARCH.md](DAY4_GHOST_RESOLVER_RESEARCH.md) (style template + assembly-boundary precedent). Also read current code: `Sim/GridBoard.cs`, `Sim/GridLineOfSight.cs`, `Sim/ScheduledPath.cs`, `Sim/ShootCost.cs`, `Sim/ShootMode.cs`, `Net/ActionNode.cs`, `Net/ActionVerb.cs`, `Net/GhostResolver.cs`, `Net/ReplayTape.cs`, `Net/TapeEvent.cs`, `Timeline/PawnProgram.cs`, `UI/ProgramHud.cs`, `Board/BoardInputController.cs`, `Characters/CharacterData.cs`. This is a read-only survey — nothing under `Assets/_Project` was modified; Day 6 (Snap vs Hold) work in `PawnProgram`, `ProgramHud`, `GhostResolver`, `ActionNode`, `ShootMode`/`ShootCost`, `BoardInputController` is untouched.

---

## A. What Day 7 must prove

Day 7 DoD in `SCHEDULE.md`: "**One-door** micro-map (contextual open/close; blocks move + LoS); local match E2E", exit = **M2: Core Combat local playable**. `VERTICAL_SLICE.md` phrases the same milestone as "waypoint path + stance bands; Snap vs Hold Angle; wounds/death readability; **one door** that changes move/LoS. Local match playable end-to-end." `CORE_LOOP.md` §"What fun must prove" adds the bar this note is really aimed at: "The door changes a fight once" — readable in a single playtest, not proven by inspecting code.

Observable behaviors Day 7 should end with:

1. A pawn adjacent to (or on a legal tile relative to) the door can schedule an Open or Close during Program, booked on the Time Resource clock the same way Move/Shoot are (GDD §4, `CORE_LOOP.md` "optional door open/close booked on the timeline from a legal tile").
2. While Closed, the door's tile blocks orthogonal Move through it and blocks Bresenham LoS through it (GDD §4/§5, `GridLineOfSight.cs`'s existing algorithm — no new LoS algorithm).
3. A single 5×5 layout where the door is the only route/sightline between the two spawns, so one open/close decision visibly changes the round.
4. Scout/Juggernaut pay different Time Resource costs to interact the door, displayed on the scrubber/queue the same way Snap (`2s`) and Hold (`3s`) already are.
5. Cold-observer readability, matching the Day 4 bar: someone who didn't build it can point at the door and say "closing that blocked the shot" or "opening that let them through."

Not in scope for Day 7 (see Section G): reopen-cost asymmetry, interrupted toggles, vent/monitor variants, Otherwise-branch behavior.

---

## B. Door as a map action, not a gear card

GDD §4 is explicit: "For the 14-day ship, one door on the 5×5 board... Scheduled as a contextual map action from the pawn's current/adjacent tile during Program (tap door → open/close at a booked Time Resource second)... This is not the full Interact card / vent / monitor kit (C34)." `PRODUCT_MEMORY.md` C34 repeats it as "one contextual door (blocks move + LoS)," distinct from the deferred `Bandage`/`Interact-as-card`/`Flashbang`/`Adrenaline` gear list.

Concretely, this means the door action should sit alongside Move and Shoot as a third **base verb**, not as a `CardData` asset flowing through `ActionNode.Modifier`:

- `ActionNode.Modifier` is already documented as "reserved for card interrupts" and is `null` for every verb through Day 6. A door action should **not** claim that field — it would conflate a map action with the gear-card system C34 explicitly cut.
- The existing pattern to mirror is `ActionVerb.Shoot` + `ShootMode`: a third `ActionVerb` value, its own small enum for the action's flavor (open vs close), and a dedicated `PawnProgram.TryQueue*` method that reserves Time Resource cost the same way `TryQueueShoot` does today. Section F sketches this.
- Program-phase UX: `ProgramHud.BuildVerbRow` currently splits the thumb zone into `MOVE` / `SHOOT` (`ProgramHud.cs:317-327`, `SetMode(ActionVerb mode)`). A `DOOR` tap would be the same shape — select the verb, tap the door tile (only legal when adjacent/on a legal tile per GDD §4), pick Open or Close, and it books into the same `ActionNode` list that already backs `TimelinePayload`.
- It is committed the same way as Move/Shoot: reserved against `PawnProgram.BudgetSeconds` via `TryReserve`, and it becomes part of the same `TimelinePayload.Nodes` list `GhostResolver.Resolve` already consumes. There is no separate "map action" pipeline to build — the resolver's existing node-in-time-order shape (Section F) is the target.

---

## C. Closed-door semantics against the existing LoS/move code

`GridLineOfSight.HasLineOfSight` (`Sim/GridLineOfSight.cs:13-34`) already walks `TilesBetween(from, to)` and returns `false` the moment any intermediate tile's `TryGetTile(...).IsPassable` is `false`. `GhostResolver.ResolveSnapShot`/`ResolveHoldAngle` already call this exact method, and `OrthogonalPathfinder`/`BoardInputController` already gate path drafting on `_board.GetTile(tile).IsPassable` (`Timeline/PawnProgram.cs:100`, `TryExtendOrReplaceDraft`). **A closed door that sets its own tile's `Tile.IsPassable` to `false` is therefore blocked by both systems today, with zero changes to `GridLineOfSight.cs` or the pathfinder.** That is the cheapest, most grounded way to satisfy "closed blocks move + LoS" — reuse the impassable-tile mechanism that already blocks both, rather than inventing a second blocking concept.

One real gap this exposes: **`Tile` has no notion of an edge.** `Tile` (`Sim/GridBoard.cs:9-17`) is a per-coordinate struct (`IsPassable` only); `GridBoard` indexes tiles by `GridCoordinate`, not by the edge between two coordinates. GDD's wording — "blocks movement through that edge" — is naturally edge-shaped, but the codebase has no edge type to attach that to. The reconciliation Day 4's ghost-resolver precedent suggests (treat the simplest model that satisfies both existing consumers) is: **the door occupies its own single tile**, sitting as a one-tile-wide chokepoint between two areas. Closed ⇒ that tile's `Tile.IsPassable = false`, indistinguishable from a wall tile to both the pathfinder and `GridLineOfSight`. Open ⇒ `Tile.IsPassable = true`, ordinary floor tile. This needs no edge concept, but it does mean a pawn can never stand *on* the door tile while it's being toggled by someone else, and a Move that ends exactly on the door tile is legal once open — both are modeling choices, not derived facts, and are flagged in Section H.

The other gap: `GridLineOfSight`/`GridBoard` as used by `GhostResolver` are **not time-aware** — a shot's LoS check reads whatever `Tile.IsPassable` the shared `GridBoard` instance holds *at the moment the resolver calls it*, not "as of the shot's Time Resource second." Doors are the first thing in this codebase whose passability changes mid-resolve (Move/Shoot never mutate board tiles). Section F treats this as the central technical question for Day 7.

---

## D. Minimal 5×5 placement proposal

Goal: one board where a single door decision is legible in one playtest, per `CORE_LOOP.md`'s "door changes a fight once" bar. Coordinates use the existing `GridCoordinate(x, y, Floor.Ground)` convention (`Attic` stays out of scope per C17); `x, y ∈ [0, 4]`.

Proposal — a wall across the middle row with the door as its only gap:

```
y=4  .  .  .  .  .     <- Defender spawn (2,4)
y=3  .  .  .  .  .
y=2  #  #  D  #  #     <- wall row; D = door tile (2,2); # = permanently impassable
y=1  .  .  .  .  .
y=0  .  .  .  .  .     <- Attacker spawn (2,0)
     x=0 1  2  3  4
```

- **Wall tiles** `(0,2)`, `(1,2)`, `(3,2)`, `(4,2)`: `Tile.IsPassable = false` for the whole match — ordinary impassable tiles, not doors, built the same way Day 1–2 already builds the board (`GridBoard` constructor currently fills every tile passable; the wall is just four tiles flipped via the existing `this[coordinate] set` indexer).
- **Door tile** `(2,2)`: the single gap in the wall. Starts **Closed** (`Tile.IsPassable = false`) so the opening round has stakes both ways.
- **Spawns**: Attacker `(2,0)`, Defender `(2,4)` — both directly on the door's column, so the moment the door opens, the full-board LoS lane down `x=2` is live and readable without requiring either side to maneuver first. This keeps Day 7 minimal: the RPS tension is entirely about *when* to open/close and *what stance* to be in when it happens, not about pathing around the map to find the choke.

Why this shape: it is the smallest board that makes the door **load-bearing** — there is no route between the halves that doesn't cross `(2,2)`, so "the door changes a fight once" is structurally guaranteed rather than left to chance. A door placed off to one side of an otherwise-open 5×5 (no wall) would let players ignore it and route around, which would fail the single-playtest readability bar. The RPS reading: Attacker opens the door to get LoS/through-access but eats the Strength cost and reveals intent on the timeline; Defender can close it in response if their own booked second lands first, trapping or exposing whoever is mid-transition; both sides can also just wait at the door in Crawl (silent, per GDD §3.2) so the "who opens it" decision itself is the bluff. This is a placement proposal, not a locked layout — Section H asks whether the wall should be full (as above) or partial (leaving a second, unintended route that undermines the single-fight-changes reading).

---

## E. Strength costs and the Time Resource scrubber

GDD §6's character preset table already carries the requested placeholders: **Scout door 4s / Juggernaut door 2s**, and the codebase already has a matching (currently unused) field: `Characters/CharacterData.cs:26-28`:

```csharp
[Header("Strength")]
[Tooltip("Base Time Resource cost to Interact a door before Strength modifiers.")]
public float doorInteractBaseSeconds = 4f;
```

That default (`4f`) matches Scout's GDD number; a Juggernaut `CharacterData` asset would set it to `2f`. No new Strength stat needs inventing — this field is the intended source, and Day 7 would populate/read it rather than add a parallel number.

For how the cost should *sit on the scrubber*, the existing visual/numeric language to mirror is the Shoot-mode row in `ProgramHud.cs`:

- Button labels bake the cost directly into the label text: `"SNAP  2s"` / `"HOLD  3s"` (`ProgramHud.cs:375-381`, `Shoot_Snap` / `Shoot_Hold` buttons).
- A context label above the buttons states the full sentence: `$"SNAP SHOT  {cost:0}s — aimed tile only; wounds; misses Sprint"` (`RefreshShootModeControls`, `ProgramHud.cs:565-583`).
- The committed-queue readout (`OnQueueChanged`, `ProgramHud.cs:625-654`) prints one line per booked node: `"{index}: {Verb} -> {GridPosition} @{ExecuteTime}s ({detail})"`, where `detail` is `ShootModeMath.Label(node.ShootMode)` for Shoot and `StanceMath.Label(node.Stance)` for Move.
- The scrubber's own label (`OnClockTime`, `ProgramHud.cs:745-759`) only ever shows the running Time Resource total — `"Time Resource  {seconds}s / {budget}s"` — it has no per-action awareness; per-action cost is entirely the queue readout's and the verb row's job.

A Door verb fits the same three places without inventing new UI language: a `DOOR` button next to `MOVE`/`SHOOT` in the verb row; an Open/Close sub-row shaped like the Snap/Hold row with labels such as `"OPEN  4s"` / `"CLOSE  4s"` (character-specific — a Juggernaut's HUD would read `"OPEN  2s"`); and a queue line `"2: Door -> (2,2) @6.0s (Open)"` alongside the existing `Move ->` / `Shoot ->` lines. This is a layout observation for whoever builds Day 7's HUD, not a proposal to touch `ProgramHud.cs` now.

Whether Open and Close cost the same `doorInteractBaseSeconds` (both directions charged identically) or something asymmetric is explicitly **not** decided here — see Section G (reopen nuance is cut) and Section H (open question 3).

---

## F. Types/API sketch (signatures only — no implementation)

This section is grounded in the actual current shapes of `Sim/GridBoard.cs`, `Net/ActionNode.cs`, and `Net/GhostResolver.cs` read above, extended the same way `ShootMode` was bolted onto `ActionNode`/`PawnProgram`/`GhostResolver` for Day 6. No networking is introduced — `GhostResolver.Resolve` stays a pure function of `(board, inputs)`.

**`Sim/Door.cs`** (new file, same assembly as `GridBoard`/`GridLineOfSight` — `LogiCard.Sim.asmdef` has `references: []`, so this must stay engine-free like its neighbors):

```csharp
namespace LogiCard.Sim
{
    public enum DoorState { Open, Closed }

    /// <summary>One map door: a single tile whose passability the resolver toggles over time.</summary>
    public sealed class Door
    {
        public GridCoordinate Coordinate { get; }
        public DoorState InitialState { get; }

        public Door(GridCoordinate coordinate, DoorState initialState);
    }
}
```

**`Sim/GridBoard.cs`** additions (existing members unchanged; `Tile`/`this[GridCoordinate]` indexer already does everything needed to flip passability):

```csharp
public sealed class GridBoard
{
    // existing: Width, Height, Floors, this[GridCoordinate], InBounds, GetTile, TryGetTile, GetAllCoordinates

    public IReadOnlyList<Door> Doors { get; }                                   // registered doors
    public void RegisterDoor(Door door);                                        // sets initial Tile.IsPassable too
    public bool TryGetDoor(GridCoordinate coordinate, out Door door);
}
```

**`Net/ActionVerb.cs`** — one new value, same enum shape:

```csharp
public enum ActionVerb
{
    Move,
    Shoot,
    Door,   // NEW (Day 7) — contextual map action, GDD §4
}
```

**`Net/ActionNode.cs`** — extended the same way `ShootMode` was: `GridPosition` is reused as the door's tile (mirrors the existing Move-destination / Shoot-target overload `DAY4_GHOST_RESOLVER_RESEARCH.md` §B already documents), plus one new field meaningful only when `Verb == ActionVerb.Door`:

```csharp
public enum DoorAction { Open, Close }

public readonly struct ActionNode
{
    // existing: Verb, ExecuteTime, GridPosition, Stance, ShootMode, Modifier
    public DoorAction Door { get; }   // meaningful only when Verb == ActionVerb.Door

    public ActionNode(
        ActionVerb verb,
        float executeTime,
        GridCoordinate gridPosition,
        StanceType stance,
        CardData modifier = null,
        ShootMode shootMode = ShootMode.None,
        DoorAction doorAction = DoorAction.Open);   // ignored for Move/Shoot, mirrors the ShootMode default pattern
}
```

**`Timeline/PawnProgram.cs`** — a `TryQueueDoor` alongside `TryQueueMove`/`TryQueueShoot`, same reserve-then-append shape as `TryQueueShoot` (`PawnProgram.cs:259-300`):

```csharp
public sealed class PawnProgram
{
    // existing members unchanged

    public bool TryQueueDoor(GridCoordinate doorTile, DoorAction action, out string rejectionReason);
    // cost = character's doorInteractBaseSeconds (CharacterData), reserved via the existing
    // CanReserve/TryReserve pair; legality = doorTile is CurrentPosition or an orthogonal
    // neighbour of it (GDD §4 "current/adjacent tile"), and doorTile is a registered Door.
}
```

**`Net/TapeEventType`** — two new outcomes, same enum shape as the existing five (`Net/TapeEvent.cs:9-16`):

```csharp
public enum TapeEventType
{
    MoveArrive = 0,
    ShootFire = 1,
    Wounded = 2,
    Killed = 3,
    Invalid = 4,
    DoorOpened = 5,   // NEW
    DoorClosed = 6,   // NEW
}
```

**`Net/GhostResolver.cs`** — the load-bearing decision. `GhostResolver` currently takes one `GridBoard _board` at construction (`GhostResolver.cs:49,52`) and treats it as constant for the whole `Resolve` call — every `GridLineOfSight.HasLineOfSight(_board, ...)` check in `ResolveSnapShot`/`ResolveHoldAngle` reads whatever passability `_board` holds *right now*. Doors are the first feature whose passability changes **during** a resolve, at a specific `ExecuteTime`, so a shot fired before a door opens must see it closed, and a shot fired after must see it open. Two shapes satisfy this without changing `GridLineOfSight`'s signature:

```csharp
public sealed class GhostResolver
{
    // existing: GhostResolver(GridBoard board, float simultaneityEpsilon), Resolve(IReadOnlyList<GhostInput>)

    // Option 1 — scratch board, mutated in ExecuteTime order as the resolver already walks
    // shots in order (ResolveShots, GhostResolver.cs:115-170): collect Door ActionNodes the same
    // way ShotIntent is collected, sort with the shot/move events, and apply each toggle to a
    // private GridBoard clone (via the existing `this[GridCoordinate] set` indexer) immediately
    // before any LoS/passability check whose instant is >= the toggle's ExecuteTime.

    // Option 2 — time-indexed query instead of mutation: give GridBoard (or a resolver-local
    // wrapper) an `IsPassableAt(GridCoordinate coordinate, float seconds)` that consults each
    // Door's toggle history, and have GhostResolver pass `seconds` through to LoS checks instead
    // of relying on GridLineOfSight's board-only signature. This avoids mutating the shared board
    // (safer if a board instance is ever reused across resolves) but needs a new LoS entry point
    // (`GridLineOfSight.HasLineOfSightAt(board, from, to, seconds)`), which Option 1 does not.
}
```

Section H flags this as the one decision worth locking before Day 7 coding starts, since it changes `GhostResolver`'s internal shape (owned by the concurrent Day 6 work) rather than just adding a new verb.

---

## G. Explicitly out of scope under C34 rescope

Per `PRODUCT_MEMORY.md` C34's cut order — "Android smoke → **door reopen nuance** → Crawl AV nuance → optional DoF/SSS" — reopen nuance is already named as the first thing to cut if Day 7 runs long, which means it should not be designed now regardless of schedule pressure. Deferred, not designed:

- **Reopen nuance**: whether opening costs a different amount than closing (Section E leaves this open rather than asserting symmetry); whether a toggle can be interrupted mid-window (e.g., by taking a wound while reaching for the door, echoing the Hold Angle contact-window logic `GhostResolver.TryFindHoldContact` already implements for shots); whether a door can be toggled more than once per round.
- **Vent / monitor door variants**: GDD §7 and `CORE_LOOP.md` "Map loop" both list Attic/Vent/Monitor as post-demo (C34, C17). This note's door is the single ground-floor contextual door only.
- **"Otherwise" branch behavior**: GDD §4 and `CORE_LOOP.md` §7 both say the full Otherwise/Interact card library is post-demo; Day 7's simplification for a blocked path (whether blocked by a wall, a closed door, or a door that closes mid-move) is "stop before the block" per the existing demo-simplification rule, not a branching Otherwise Stop.

---

## H. Open questions for user confirmation before coding begins

1. **Tile-occupies-the-door vs. true edge model (Section C).** The proposal treats the door as one impassable/passable tile (reuses the existing `Tile.IsPassable` mechanism both `GridLineOfSight` and the pathfinder already honor). This means no pawn can ever stand on the door tile while it's closed, and standing on it while open is legal like any floor tile. Confirm this is acceptable, or whether the door must behave as a true edge between two adjacent tiles (would require a new edge-keyed data structure alongside `GridBoard`'s per-coordinate `Tile[,]`).
2. **Time-varying board state during resolve (Section F).** `GhostResolver` currently resolves against one static `GridBoard`. Doors need either (a) a scratch board mutated in `ExecuteTime` order, or (b) a new `IsPassableAt(coordinate, seconds)` query threaded through LoS checks. This is an architectural choice inside `GhostResolver`, which Day 6 is actively extending for Snap/Hold — needs a decision (and probably a short handoff note to whoever picks up Day 7) before either shape is committed to.
3. **Symmetric open/close cost?** GDD §6 gives one number per character (`doorInteractBaseSeconds`) with no open/close split. Confirm both directions cost the same, or that asymmetry is intentionally deferred to the cut "reopen nuance" item (Section G) and Day 7 should just charge the single base cost either way.
4. **Default door state and cross-round persistence.** Starts Closed (Section D assumes this for stakes on the opening round) — confirm. Also confirm door state carries between rounds the same way position/wounds do under C33 ("Board state carries between rounds"), rather than resetting to Closed every Allot.
5. **Simultaneous Open vs. Close on the same door.** `GhostResolver` already groups near-simultaneous shots by an epsilon and resolves them against a pre-group snapshot (`ResolveShots`, `_simultaneityEpsilon`). If both players book a door toggle at the same second (one Open, one Close), Day 7 needs an explicit tie-break rule mirroring that pattern — not specified here.
6. **Legal tiles for interacting.** GDD §4 says "current/adjacent tile." Confirm "adjacent" means the four orthogonal neighbours of the door tile (consistent with `GridCoordinate.GetOrthogonalNeighbours`), and confirm whether both players can interact the same door in the same round if both are adjacent, or whether only one toggle per door per round is legal.
7. **Board layout lock.** Section D's full-wall-with-one-gap layout is a proposal chosen to guarantee single-playtest readability, not a locked design — confirm the wall/spawn placement (or provide an alternative) before it's wired into whatever board-building code Day 7 touches.
