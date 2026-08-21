# Map Authoring Guide

**Status:** Authoring contract for implementers (2026-08-12).  
**Locked by:** `PRODUCT_MEMORY.md` **C45**, **C57**, **C59** (plus continuous geometry **C35/C39**, Door API / inclusive segments **C41**).  
**Not this doc:** weather/atmosphere (`BoardWeatherPocket`), Net/Timeline resume, or inventing a data-driven map pipeline.

This is an **extension of how maps are authored today**, not a redesign of C57. Future agents add a fourth map the same way the three existing ones were built: bespoke C# geometry + defender payload in `GameBootstrap`, room rectangles only in `MapDefinitions`.

---

## 1. Locked product rules (cite C#)

Do **not** reopen these without human confirm → a new `PRODUCT_MEMORY` row.

| Lock | Meaning for map work |
|------|----------------------|
| **C57 — hand-authored roster** | One bespoke `GameBootstrap.BuildXxxGeometry()` + `BuildXxxDefenderPayload()` (or Freight Yard's `BuildDefenderPayload`) per map. **No** JSON / ScriptableObject / shared level-editor format. `MapDefinitions` is **not** a map format — it only centralizes **room rectangles** (+ `MapSurfaceRole`). |
| **C57 — complexity = interactive terrain** | Each map needs real Vent/Breach (or equivalent reskinned `Door`s) usable by **both** sides. Not a difficulty ladder, not a larger map *pool*. |
| **C57 — Door kinds** | `DoorKind.Standard` / `Vent` / `Breach` only today (`Assets/_Project/Sim/Door.cs`). Zero new Sim types for vents/breaches. New kinds need Integrator + human lock. |
| **C59 — map select is local** | `AppFlowController.Screen.MapSelect` picks a `MapId` that drives this client's `GameBootstrap.BuildBoard(MapId)` only. **No** network map sync while Net/Timeline work stays paused. |
| **C45 — Scout/Juggernaut spatial lever** | Every map must still express short/guarded vs longer/safer (or equivalent Sprint-speed asymmetry). Do not ship a map where both archetypes feel identical. |
| **C35/C39 — continuous arena** | `PlanarPosition` + wall/door `Segment`s. Pathfinding = visibility graph + Dijkstra (`ContinuousPathfinder`), **not** navmesh or a revived grid. Door interact = radius (`ArenaBoard.TryGetNearestDoor` / `PawnProgram` `InteractRadius`). LoS = continuous segment-vs-obstacle, never a physics raycast. |
| **C41 — Door API + inclusive touch** | Register/query doors as `Door` objects (not int ids). `Segment.Intersects` is **inclusive** — shared corner/endpoint blocks move + LoS. |
| **C40 — no pawn blocking** | Walls/closed doors only. Props/dressing must not invent collision. |

**Footprint note:** C45 locked Freight Yard's original multi-room story on `[0,8]×[0,10]`. C57 maps already vary bounds (`BuildRailPlatformGeometry` → `[0,8]×[0,13]`, `BuildVaultComplexGeometry` → `[0,8]×[0,9]`). A fourth map may choose its own `ArenaBoard` min/max, but must stay continuous, single-floor for now (`Floor.Ground` only — attic still out), and keep room rects in `MapDefinitions` in sync with walls.

---

## 2. Anatomy of one map (checklist)

Ordered touchpoints to **add or change** a map at tip `b32eda2`. Every item names a real symbol — skip none.

1. **`MapId` enum** — `Assets/_Project/Board/MapDefinitions.cs` (`FreightYard`, `RailPlatform`, `VaultComplex`).
2. **`MapDefinitions.Xxx()`** — return a `MapLayout` of `MapRoom` rectangles + `MapSurfaceRole` (Yard / Hall / Vault / Flank). Names are designer-facing; roles drive floor materials.
3. **`MapDefinitions.ForId(MapId)`** — add the new `case`; default still `throw NotImplementedException`.
4. **`GameBootstrap.BuildXxxGeometry()`** — `new ArenaBoard(minX, minY, maxX, maxY, floors)`, then `RegisterWall` / `RegisterDoor` only. Walls and doors are **not** stored in `MapDefinitions`.
5. **Door registration** — each gap is a `Door` segment between wall stubs; set `displayName` (UI identity) and `kind:` (`DoorKind.Standard` default, or `Vent` / `Breach`). See §3.
6. **`GameBootstrap.BuildBoard(MapId)` switch** — call the new geometry builder; then `_board.Build(model, MapDefinitions.ForId(mapId))`.
7. **`GameBootstrap.BuildPawns()` switch** — attacker/defender `PlanarPosition` spawns + which defender payload builder to use.
8. **`BuildXxxDefenderPayload()`** — scripted local defender (open the “official” gate, shoot, leave Vent/Breach as player-discoverable). Pattern: `BuildDefenderPayload` / `BuildRailPlatformDefenderPayload` / `BuildVaultComplexDefenderPayload`.
9. **Map Select UI** — `AppFlowController.BuildMapSelect`: add a `SelectionOption(MapId.Xxx.ToString(), "DISPLAY NAME")`, and a detail string in `OnMapSelectionChanged`.
10. **Presentation consumers of `MapLayout` / `MapSurfaceRole`** — already driven by layout if you pass it into `BoardView.Build(ArenaBoard, MapLayout)`:
    - `BoardView.PlaceRoomFloors` — floor slabs per room + role materials.
    - `BoardReflectionProbes.Build(BoardView)` — one probe per `MapRoom` via `board.Layout`.
    - Door meshes / Vent-Breach tint via `BoardView` + `DoorKind` (not layout).

**Optional but easy to forget:** if Integrator later resumes Net, `Relay/LogiCard.Relay/DemoArenaBoard.CreateDemo()` must stay in agreement with the board the client resolves — today it only mirrors a **pre-Vent/Breach** Freight Yard (see watch-outs).

### Watch-outs (fragile / incomplete — do **not** “fix” in a docs-only pass)

- **`MapId` XML comment is stale** — still says selection UI “doesn't exist yet”; **C59** added `AppFlowController.Screen.MapSelect`.
- **`MapDefinitions.RailPlatform` comment is stale** — still claims “Not yet wired into `ForId`”; `ForId` already returns `RailPlatform()`.
- **`BuildRailPlatformDefenderPayload` comment is stale** — still says not wired into dispatch; `BuildPawns` already selects it for `MapId.RailPlatform`.
- **`BoardView.Build(ArenaBoard)` back-compat overload** always pairs geometry with `MapDefinitions.FreightYard()`. Tests or callers that build Rail/Vault geometry then call the one-arg `Build` will **desync floors/probes** from walls.
- **`BoardView.PlaceRoomDressing` is Freight-Yard-shaped** — hardcoded Yard/Hall/Vault prop coordinates; on Rail Platform / Vault Complex props sit in wrong rooms or outside usable space. New maps need map-aware dressing or an empty dressing path until art is authored.
- **`DemoArenaBoard.CreateDemo()` drifted** — still the old Yard/Hall/Vault **without** Vent/Breach wall splits; live `BuildFreightYardGeometry` has both. Relay smoke vs local Freight Yard will disagree until Integrator syncs them.
- **`EnsureMatchSceneBuilt` is one-shot** (`_matchSceneBuilt`) — changing `MapId` after the first match build does nothing; rematch/map re-pick rebuild is not authored.
- **Camera default is map-agnostic** — `ConfigureCamera` sets `orthographicSize = 3.4f` from board center; taller boards (Rail Platform `MaxY = 13`) need human framing/zoom check (`BoardCameraRig` min/max zoom exists, but default framing is not per-map).
- **Breach “permanent” is UI-only** — `ProgramHud.RefreshDoorPrompt` hides Close when `DoorKind.Breach` is open; the Sim/resolver would still honor a Close if queued. Do not assume Sim enforces one-way.
- **No map-roster EditMode/PlayMode coverage** — tests exercise doors/pathfinding generically; nothing asserts `MapId` geometries, `ForId`, or per-map defender scripts by name.
- **GDD §7 table still lists Attic/Vent/Monitor as future** — C57 Vent/Breach as `DoorKind`s are live; treat GDD row as stale wording, not a reason to invent a second vent system.

---

## 3. Geometry & Door rules

### Continuous arena

- Author walls as `Segment` chains. Leave a gap for each door, then `RegisterDoor(new Door(gapSegment, DoorState.Closed, displayName, kind))`.
- Closed door segments and walls both block movement and LoS via `Segment.Intersects` (**inclusive** touch — C41). Shared endpoints with adjacent wall stubs are intentional; do not “fix” inclusive intersection.
- Spawns and scripted approach points must land inside bounds and typically within `InteractRadius` (0.7f in `PawnProgram`) of the door they mean to toggle. Scripted AI uses `TryGetNearestDoor(..., float.MaxValue)` — keep the intended door **nearest** from the approach point or the script will open the wrong one.

### Door kinds (same Sim pipeline)

| `DoorKind` | Design use | Open/Close |
|------------|------------|------------|
| **Standard** | Official room-boundary gates | Both always offered |
| **Vent** | Narrow repeatable shortcut / bypass | Both legal; presentation = grate (narrower mesh) |
| **Breach** | Pay once for a permanent new route | Starts Closed; UI stops offering Close after Open |

Do not add stance-gated vents (would need `StanceType` through `ContinuousPathfinder`) without a separate product lock — C57 explicitly deferred that.

### Breach points (C36 — a `BreachPoint`, *not* `DoorKind.Breach`)

Same English word, unrelated concept (see `BreachPoint`'s own doc comment): `DoorKind.Breach` is a shipped
one-way *door*; a `BreachPoint` is a wall segment a Bomber can blow open with Attach → Detonate.

- Author it like a door gap: leave the segment **out of** `RegisterWall`, then
  `RegisterBreachPoint(new BreachPoint(segment, BreachState.Intact, displayName))`. Registering a segment
  as both a wall and a breach point would leave the wall blocking (and drawn) after a detonation.
- **`BoardView` draws the wall body for the breach point itself** (`PlaceBreachMesh` → the ordinary
  `PlaceWallFence` geometry) — that is why the segment must not also be a registered wall, and why a
  breach point registered before this presenter existed was an invisible blocker. Nothing else needs
  authoring for the visual.
- Presentation states (`BoardView.RefreshBreachVisuals`): **Intact** and **Damaged** both draw the plain
  wall (Damaged is reserved by C36, unexercised by the wall-only v1 verb); **Breached** hides the wall and
  shows scorched end stubs + floor rubble; `HasAttachedBomb` adds a charge marker with a red arming light.
  The marker straddles the wall rather than sitting on one face — the board camera orbits (`BoardCameraRig`
  yaw), so a one-face marker is invisible from the other side (caught in the 2026-08-21 look-check).
- That refresh runs from `BoardView.LateUpdate` and re-derives everything from `ArenaBoard.GetBreachState`
  / `HasAttachedBomb` each frame, because `RoundPlayback.SyncBreachToSeconds` — which owns the model state
  at any scrubber second — is frozen and has no BoardView hook of its own for breach the way it has
  `RefreshDoorVisuals` for doors. Keep it a pure function of the model: a one-shot detonation FX fired on
  the event crossing would be the door-hinge / "Healed presenter" bug class
  (`docs/core/PLAYBACK_CONTRACT.md` §2 rules 2/4). Any FX added later must be gated on the displayed-state
  change, the way `ApplyDoorVisualState` gates the hinge swing.
- **No map registers one yet** — which wall on which map is an open human content decision, deliberately
  deferred. Tests register a scratch point directly on the live board
  (`BoardViewBreachVisualsPlayModeTests`, `RoundPlaybackPlayModeTests`, `GhostResolverBombTests`).

### Pathfinding (high level)

`ContinuousPathfinder` builds a visibility graph over obstacle endpoints and runs Dijkstra. More doors/walls = denser graph; keep gaps consistent (door width ~0.4–0.5 for Vent/Standard as in existing maps). No new Sim types, no navmesh, no grid rediscretization.

### Board-anchored UI

Any new interactable that is still a `Door` reuses the existing door prompt. If you ever add a **non-Door** board object players confirm against, follow `docs/UI_BOARD_ANCHORED_COMPONENTS.md` (identity / live-or-scheduled state / explicit options) before inventing HUD-dock controls.

---

## 4. Spatial design brief template

Fill this **before** coding a fourth map. Paste into the AGENT_BRIEF.

```text
MAP NAME / MapId:
FOOTPRINT (ArenaBoard min/max):
ROOMS (name, rect [minX,minY]–[maxX,maxY], MapSurfaceRole):
  -
  -
SIGHTLINE THESIS (Scout Snap vs Juggernaut Hold — what length/shape forces the lever?):
OFFICIAL ROUTE (Standard doors the scripted defender may touch):
VENT PLACEMENT INTENT (what bypass does it create? both sides usable?):
BREACH PLACEMENT INTENT (what permanent reshape after open?):
SPAWN ASYMMETRY (attacker home / defender spawn + why):
DEFENDER CHOREOGRAPHY (approach point, door opened, Snap vs Hold, aim point; what it must NEVER touch):
WHAT THIS MAP IS *NOT* (explicitly not a harder Freight Yard / not a pool-size race):
PRESENTATION NOTES (dressing plan — or “none until map-aware PlaceRoomDressing”):
HUMAN OPEN QUESTIONS:
  -
```

---

## 5. Do / Don’t

**Do**

- Add one `BuildXxxGeometry` + layout method + spawn/payload/UI wiring per map (checklist §2).
- Keep room rectangles **only** in `MapDefinitions`; walls/doors/spawns/AI stay in `GameBootstrap`.
- Give every door a stable `DisplayName` for the board-anchored prompt identity leg.
- Leave Vent/Breach as player-discoverable depth on scripted defenders (Freight Yard Door #2 / Rail Vent+Breach / Vault Vent+Breach pattern).
- Reuse `MapSurfaceRole` so floors stay on the four existing PBR materials.

**Don’t**

- Invent a data-driven map asset format, SO catalog, or shared “map builder” DSL (rejected in C57).
- Revive grid movement, Bresenham LoS, or Unity navmesh for resolve/pathfinding.
- Duplicate room rect literals in `BoardView` / `BoardReflectionProbes` (that was the pre-`MapLayout` drift bug).
- Add a fourth `DoorKind` or non-Door Sim interactable without Integrator + PRODUCT_MEMORY.
- Wire network map sync / relay board handshake while Net remains paused (C59 local-only).
- Assume `BoardView.Build(model)` (one-arg) is safe for non–Freight Yard geometry.
- Restate C45’s `[0,8]×[0,10]` as a hard max for every future map — C57 already varied height; sync layout rects to whatever `ArenaBoard` you choose.
- Grow “complexity” by enlarging the match Time Resource pool or stacking more Standard doors without a Vent/Breach thesis.

---

## 6. Verification bar for a new map

### EditMode / PlayMode (batchmode)

- Existing door / pathfinder / `ArenaBoard.TryGetNearestDoor` suites must stay green (generic geometry).
- Prefer adding focused EditMode tests that construct the new `BuildXxxGeometry` equivalent (or call the static builder if made `internal`/`public` for tests) and assert: door count/kinds, a known Vent/Breach gap exists, and at least one blocked-vs-open path across a closed Standard door.
- Prefer a PlayMode (or EditMode program) smoke that runs the map’s defender payload builder once and asserts it books Open + Shoot without throwing / empty program.
- **Batchmode note:** Unity Editor must be **closed** on the project path under test. Disposable worktrees do not share the main tree’s Editor lock — run batchmode against **this** worktree path only when the Editor isn’t holding it. Do not claim green without an actual run.

### Human Play only

- Framing: does the default ortho size + zoom range make the whole board readable?
- Dressing: props not sitting in corridors / wrong rooms (especially until `PlaceRoomDressing` is map-aware).
- Scout vs Juggernaut lever actually feels different (C45/C57 thesis).
- Vent/Breach readable as interactables (names, meshes, Breach no-Close after open).
- Scripted defender ambush still teaches the “official” door without stealing shortcuts.

### Out of scope for a map PR alone

- Relay `DemoArenaBoard` sync (Integrator when Net smoke matters).
- Rematch / rebuild-on-map-change (`EnsureMatchSceneBuilt` one-shot).
- Weather / vibrancy / new `MapSurfaceRole` materials.
