# Map Presentation Standard — vibrant/cute diorama target

**Status:** Phase 2 implementation landed in Map worktree (2026-08-14) — awaiting human screenshot look +
Integrator lighting/grade re-pass (`GameBootstrap.BuildLighting` / `BuildDioramaVolume`, not Map-owned).
**Depends on:** [GDD.md](../core/GDD.md) §8/§11, [ART_DIRECTION.md](../core/ART_DIRECTION.md), [MAP_AUTHORING.md](MAP_AUTHORING.md),
[ART_PACK_RESEARCH.md](ART_PACK_RESEARCH.md), [PRODUCT_MEMORY.md](../core/PRODUCT_MEMORY.md) **C29 / C45 / C53 / C54 / C57 / C58 / C60 / C65**.
**§6 (2026-08-15):** docs-only camera-framing recommendation for `docs/ui/MATCH_SHELL_LAYOUT.md`'s
MapViewport rect — see `MATCH_SHELL_LAYOUT_AGENT_BRIEF.md`. Recommendation only; Camera/Integrator implement.
**Does not amend** C57 (hand-authored map geometry) — see §0. **§4 resolved by C65** (human YES) — board
*surface materials* only move flat/toon; geometry density and Atmosphere untouched.

This doc was Phase 1 (standard). Phase 2 implementation follows C65 + `docs/contracts/CURRENT.md`
(Map Phase 2). Human screenshot look + Integrator lighting re-pass remain before Done.

---

## 0. What this doc is not proposing (read this first)

The seat brief that spawned this doc flagged a specific risk: "rebuild with blocks" could mean either (a) a
shared/data-driven map-authoring pipeline, which **C57 already explicitly rejected**, or (b) using modular
prop/mesh pieces from already-owned asset packs to physically build more vibrant-looking rooms inside the
existing hand-authored method-per-map pattern, which fits C57 fine.

**This doc proposes (b) only.** Nothing here touches:

- `GameBootstrap.BuildXxxGeometry()` — walls, doors, vents, breaches stay hand-authored C# per map, per
  **C57**. Sim-layer geometry is out of this department's scope entirely (see `docs/departments/map/STATUS.md`).
- `MapDefinitions` — stays a room-rectangle-only de-dupe struct, not a map format, per C57's own doc comment.
- Pathfinding, door API, LoS, `Segment.Intersects` — untouched, per **C35/C39/C41**.
- `docs/MAP_AUTHORING.md` §1's locked rules — this doc is downstream of that one, not a replacement.

What this doc **does** propose changing is narrower and purely presentational: which **materials** (and,
secondarily, which **prop family**) `BoardView`/`BoardSurfaceMaterials` draw from when it fills in a room that
`MapDefinitions` already describes. The `MapSurfaceRole` lookup pattern (`SurfaceMaterialFor(role)` in
`BoardView.cs`) stays exactly as-is — only what's behind that lookup changes.

---

## 1. Diagnosis: why C58 and C60 didn't hold

The seat brief asked for this explicitly — two prior vibrancy passes (**C58**, 2026-08-10; **C60**, 2026-08-11)
both shipped, both batchmode-verified, and the human still opened this session describing the board as dark/grey.
Reading the actual material code (not just the PRODUCT_MEMORY rows) surfaces a real, previously undocumented
reason:

### 1.1 The "vibrant" fix and the "realistic" fix have been running on different axes, and only one of them ever touched the actual albedo

- **C53** (2026-08-09) deliberately moved the board *away* from clay-primitive/toy-chibi and *toward*
  "materials/architecture move toward realistic detail" — real Poly Haven photographic PBR captures
  (`asphalt_02`, `concrete_floor`, `brick_wall_02`, `wood_planks` — real photographs of real surfaces, CC0)
  and a real imported interior mesh/material pack.
- **C58** and **C60** then tried to make that same photographic-PBR material stack read as "vibrant" by turning
  saturation/exposure/color-filter knobs on top of it — first in the *materials* (`BoardSurfaceMaterials.cs`'s
  `tint` parameters), then (C60) in the *post-process Volume* and *ambient/key/fill lighting* once C58's
  material change turned out to be getting overridden by a second, stale Volume built every boot.
- Both passes treated "not vibrant" as a **grading** problem. Neither questioned whether the base material
  choice itself — real photographs of asphalt/concrete plus a desaturated, HDRP-authored "modern office" prop
  pack — can be graded into a saturated toy-diorama look at all. It mostly can't: a photographic diffuse
  texture's local contrast and desaturation are baked into the texel data; pushing global saturation past a
  certain point just makes the same muted photo look more like a color filter was applied to concrete, not
  like hand-painted toy plastic. This matches `docs/ART_PACK_RESEARCH.md`'s own 2026-08-11 finding (Lighting +
  Ground section, "Diagnosis" table): *"Source material is a near-featureless matte slab... Tint brightens it;
  it never gains micro-detail."*

### 1.2 A material-philosophy pivot was already researched and effectively locked — but never implemented in code

`docs/ART_PACK_RESEARCH.md`'s own **Verdict** section (2026-08-10/11, "Style lock: nappin softpack family
(confirmed)") already states: *"Primary look = soft clay / curvy-minimal... — **not** wet PBR + Quaternius."*
That is a real, already-written direction change away from wet-photographic-PBR toward flat/matte clay
materials. **It was never carried into `BoardSurfaceMaterials.cs`.** The floor-building method there is still
literally named around the rejected direction — `BuildWetSurface()` — and every room floor (`YardFloor`,
`HallFloor`, `VaultFloor`, `FlankFloor`) still layers a `tint` Color on top of a photographic diffuse texture
with a `wetSmoothnessBoost` parameter explicitly tuned to "sell wetness" for rain reflections
(`BoardReflectionProbes` exists specifically to give these "wet ... floors" something to reflect). C58 and C60
both retinted the *same* wet-photo-PBR pipeline harder instead of swapping the pipeline itself — which is
exactly why two rounds of "make it more vibrant" tuning on the same underlying materials produced the same
underlying complaint each time.

### 1.3 The one part of the current scene that *is* flat/saturated sits at the edges, not the stage

`BoardView.PlaceVoidCityProp` already draws real ithappy **Cartoon_City_Free** fragments (debris pile, trash
can, broken streetlamp) around the board's outer void — and that pack's own materials are flat matte colors
with **no photographic base texture and very low smoothness** (e.g. `Grass.mat`: `_BaseColor (0.25, 0.445,
0.130)`, `_Smoothness 0.087`, no diffuse map at all — a real cartoon material, not a tinted photo). That's the
right material family for "vibrant and cute" — it is simply only used at the board's periphery (void dressing),
never on the floors/walls/props the player is actually standing on and looking at. `BoardSurfaceMaterials`'s
own `StrataRock` / `StrataDirt` / `StrataGrass` / `PropMetal` helpers (`Solid()` — flat color, no photo texture)
are the same right family and already exist in the same file, right next to `BuildWetSurface()` — they were
just never extended to the room floors/walls that dominate the player's view.

**Conclusion:** the recurring "still dark/grey" complaint is not a tuning-magnitude problem that a third
saturation pass will fix. It's a **base-material-family** problem: two of this project's three floor-building
paths (`BuildWetSurface`'s photographic PBR, and the nappin HDRP-sourced "modern office" pack whose own default
`(Mat)Floor` ships at `_BaseColor ≈ 0.043` near-black) are inherently muted by what they are, and grading them
harder hits diminishing returns fast. The fourth path (`Solid()`, and Cartoon_City_Free's own materials) is
already in the codebase, already license-cleared, and has never actually been tried on the stage itself.

---

## 2. Standard: material family per `MapSurfaceRole`

Going forward, every room surface a new or rebuilt map draws through `BoardSurfaceMaterials`/`BoardView` should
come from a **flat/gradient-shaded toon family**, not a photographic-PBR-plus-tint family. Concretely:

| Surface class | Current (rejected going forward) | Standard (Phase 2 target) |
|---|---|---|
| Room floors (Yard/Hall/Vault/Flank) | `BuildWetSurface()` — Poly Haven photo diffuse + tint + `wetSmoothnessBoost` (Yard/Flank); or nappin's baked, HDRP-sourced `(Mat)Floor_URP` retinted in place (Hall/Vault) | `Solid()`-family flat/gradient material per `MapSurfaceRole`, tuned for saturation at the albedo level, not via a grading pass on top of a photo |
| Walls | `BrickWall` — photo brick + tint | Flat/gradient wall material, same family as floor |
| Door leaves / casings | nappin mesh + runtime `Color.Lerp` open/closed tint over the pack's own (muted) baked material | Keep nappin/imported door **mesh** (geometry is fine); swap the **material** it's tinted against to the flat family so the green/red state-tint (already existing, `DoorOpenColor`/`DoorClosedColor`, `BoardView.cs:23-25`) reads against a saturated base instead of a muted one |
| Interior props (`PlaceRoomDressing`) | nappin interior prefabs, same muted default materials | Keep nappin **meshes**; either re-skin via the pack's own more-saturated `(Mat)Gradient*` variants (Orange/Green/Blue/Purple/Yellow — already in `Assets/nappin/OfficeEssentialsPack/Materials/`, zero new import) or a duplicated-and-flattened material the same way `InteriorPackImportTool` already duplicates-and-converts nappin materials today |
| Terrain-edge strata / void dressing | Already `Solid()` / Cartoon_City_Free flat materials | **No change** — already the right family; this is the reference point every other row should match |
| Wet-look reflections (`BoardReflectionProbes`) | Tuned for "sell wetness" | De-scope wetness as the presentation goal (matches ART_PACK_RESEARCH's own "soft clay, not wet PBR" verdict) — probes can stay (cheap, harmless) but stop being the thing floor smoothness is tuned around |

**Why keep the mesh imports (nappin doors/furniture) and only swap materials, rather than re-sourcing meshes
too:** the mesh catalog (doors, shelves, cabinets, windows, lights) is already imported, licensed
(`Art/Environment/THIRD_PARTY.md`), integrated with `InteriorPackImportTool`'s duplicate-and-convert pipeline,
and referenced by name throughout `BoardView.PlaceRoomDressing`/`PlaceDoorMesh`. Cartoon_City_Free's own mesh
catalog is exterior/city-scaled (buildings, cars, billboards, roads) and has no interior furniture — it's not a
drop-in replacement for the *geometry*, only a proof that the *material philosophy* (flat, saturated, no photo
diffuse) already exists in this project and already reads as intended (it's the one thing nobody has
complained about). The fix is swapping the **paint**, not the **furniture**.

---

## 3. Standard: keep the per-map hand-authored pattern; make dressing map-aware

Two standing items from `MAP_AUTHORING.md`'s own watch-out list are relevant here and should be picked up in
Phase 2, not re-litigated:

- **`BoardView.PlaceRoomDressing` is Freight-Yard-shaped** (hardcoded Yard/Hall/Vault coordinates) — Rail
  Platform and Vault Complex either get wrong-room props or none. A vibrancy rebuild is the natural point to
  make this **map-aware**: one `PlaceRoomDressing(MapId, ArenaBoard)` overload (or per-map dressing methods,
  matching C57's own "one bespoke method per map" discipline) rather than one method assuming Freight Yard's
  room names/coordinates.
- **`BoardSurfaceMaterials`'s per-role lookup stays** (`SurfaceMaterialFor(MapSurfaceRole)`) — this is the
  correct level of reuse per C57's own stated goal ("lets new maps reuse the same... floor looks... no new art
  asset work needed to add a map"). Phase 2 changes what's *behind* the four role slots, not the fact that
  there are four reusable slots.

No new `DoorKind`, no new Sim types, no touching `ArenaBoard`/wall/door authoring — this section is
presentation-only, matching this department's scope boundary.

---

## 4. §4 decision — resolved by C65 (2026-08-14)

Human confirmed YES on `C53_SURFACE_MATERIAL_DECISION.md`. Product memory row **C65** records the amendment:
board surface materials (floors/walls/door-leaf tint/interior-prop tint) → flat/toon; geometry density and
weather/atmosphere stay as C53 wrote them. Historical framing of the open question kept below for provenance.

<details><summary>Original open-question framing (historical)</summary>

**C53** (2026-08-09) is still the standing product-memory row for board materials, and its own text says
*"materials/architecture move toward realistic detail rather than clay-primitive/chibi."* §2 of this doc
proposes the opposite direction for **surface materials specifically** (flat/toon over photographic-PBR) while
explicitly keeping C53's other, unaffected clauses: the bounded-chunk/dark-void/no-skybox camera language, the
Yard/Hall/Vault/Flank room structure, and the imported-mesh geometry for doors/props.

This is a real amendment to part of C53, not a re-interpretation of it — per this project's own governance
(`PARALLEL_OPS.md`'s doc-ownership rule; the "save-file rule" referenced in `GDD.md`/`PRODUCT_MEMORY.md`),
**product-direction changes need human confirm → a new `PRODUCT_MEMORY.md` C-row before Phase 2 locks it in.**
This doc is not that row. Framed as a direct question:

> C53 asked for photographic-real materials; C58/C60/this session's feedback keep asking for Link's
> Awakening-style vibrant/cute. §1 argues those two asks are in tension for *surface materials* specifically,
> and recommends resolving it in favor of vibrant/cute (flat/toon materials) for floors, walls, door leaves,
> and interior prop tint — while keeping C53's realistic-detail intent for geometry density/room complexity
> (C57's vents/breaches) and the weather/atmosphere system (Atmosphere department's lane, not touched here).
> Confirm this reading before Phase 2 starts recoloring the board.

</details>

---

## 5. Phase 2 checklist (implemented in Map worktree 2026-08-14 — look check still open)

1. ~~Add a `Solid()`/gradient-based floor+wall material set…~~ done (`BoardSurfaceMaterials`).
2. ~~Re-skin nappin door/prop materials via Gradient*_URP / Solid…~~ done (`BoardView` runtime reskin; import tool remains the duplicate writer).
3. ~~Make `PlaceRoomDressing` map-aware…~~ done (Freight Yard / Rail Platform / Vault Complex).
4. **Integrator:** re-run `BuildDioramaVolume`/`BuildLighting` grading *after* this material swap — Map flagged, did not edit `GameBootstrap`.
5. **Human:** screenshot check against the **§2 flat/toon material-family table** (and the Link's
   Awakening / soft-clay language in §1–§2) before calling Phase 2 Done.
   **Doc mismatch to fix (flagged 2026-08-15 look-check):** this checklist historically pointed at
   `ART_DIRECTION.md`'s Moodboard image, but that hero ref was rewritten under **C53** to a photoreal
   floating city-block chunk — the opposite surface-material direction from **C65** / this doc's §2.
   Do **not** judge Phase 2 surfaces against that moodboard photo. Judge against §2. Integrator (or a
   later ART_DIRECTION pass) should update the Moodboard section so it no longer contradicts C65 for
   board *surfaces* (geometry density / weather bar can stay C53).

---

## 6. Match Shell Layout — MapViewport framing (docs-only recommendation, 2026-08-15)

`docs/ui/MATCH_SHELL_LAYOUT.md` locks a vertical band stack where **MapViewport is a mid-screen
rectangle (~48–55% window height), not the full window** — InfoBar above it, HandBand/ToolBar/
TimelineSchedule below. This section is Map's recommendation for how the board should read inside
that hole, per that doc's brief. **Recommendation only** — Camera slice / Integrator own
`GameBootstrap.ConfigureCamera` and `BoardCameraRig`; nothing in this section is implemented here.

### 6.1 Camera framing

`ConfigureCamera` already renders the board through a `cam.rect` sub-region, not the full window
(`cam.rect = new Rect(0f, ProgramHud.HudDockHeight, 1f, 1f - HudDockHeight - TopStripHeight)`) — a
MapViewport rect is the same mechanism, not a new one. What changes is magnitude: that rect was sized
against a full-width band leaving only a bottom dock + top strip cut out (most of the window height);
MapViewport's ~48–55% is a much tighter vertical crop. The current default
`orthographicSize = 3.4f` and `BoardCameraRig`'s `[MinOrthographicSize 2.6, MaxOrthographicSize 8.0]`
bounds were calibrated against that taller region (see `BoardCameraRig.cs`'s own worked comments) and
should **not** carry over unchecked — re-derive against the new rect height using the same
aspect-independent formula already in that file (`depth * sin(52°) / (2 * orthographicSize)` for
vertical coverage), once the MapViewport rect is real.

**Readability priority when trading zoom for fit** (tightest region wins, so order the trade-offs
this way, not evenly): **doors > flank/corridor sightline shape > full room-floor edge**. It's
acceptable for outer void or a room's far floor edge to crop slightly at default zoom before it's
acceptable for a door gap or a flank mouth to clip out of frame — those are the things a player
must read every round to plan a program.

**Tall-map risk gets worse, not new:** `MAP_AUTHORING.md` §2's own watch-out already flags that
camera framing is map-agnostic and taller boards (Rail Platform, depth 13) aren't individually
verified. A ~48–55%-height rect shrinks the margin for that same problem — Rail Platform is the one
to human-check first once the real rect lands, before assuming the Freight Yard framing generalizes.

**Pitch/yaw:** no change recommended. Fixed 52° pitch + free yaw (`BoardCameraRig`) already reads as
a diorama at any rect aspect; a shorter rect is a framing/zoom problem, not a reason to revisit pitch.

### 6.2 Full-bleed dressing check

Audited Map-owned placement code (`BoardView`, `BoardSurfaceMaterials`, room floors/walls/door and
prop dressing) — everything is positioned in board-local world space keyed off `ArenaBoard` bounds,
not screen space. **No Map-owned dressing assumes a full window.** Two flags for Camera/Atmosphere,
not Map fixes:

- **Weather (`BoardWeatherPocket`, Atmosphere-owned):** already world-space and deliberately
  "contained to the board, not an infinite horizon" per `ART_DIRECTION.md`'s Moodboard section — not
  full-bleed by design. But `cam.rect` **crops** rather than rescales, so a shorter MapViewport rect
  can clip the top of that above-board storm/cloud geometry out of frame even though the system
  itself was never full-bleed. Once the real rect lands, Atmosphere should re-check the storm cloud
  cap still sits inside frame at default zoom, and adjust cloud-pocket height together with
  Camera's zoom/rect change rather than each side compensating separately.
- **Void-edge dressing** (`BoardView.PlaceVoidCityProp`, Cartoon_City fragments at the board's outer
  edge): same crop risk at max zoom-out. Low priority — decorative periphery, not a readability item.
- **Post-process vignette/DoF** (`GameBootstrap.BuildDioramaVolume`) is inherently screen-space on the
  camera's own output, so it automatically scopes to whatever rect the camera renders through —
  no change or flag needed there.

### 6.3 No second "card battlefield" layer — explicit

`BoardView` (pawns, doors, room geometry) stays the **only** board representation inside
MapViewport. Per `MATCH_SHELL_LAYOUT.md`'s own explicit rejects: no Hearthstone-style card/minion
lane overlay drawn over or inside the diorama — card-language chrome (hand fan, schedule blocks)
belongs in HandBand/TimelineSchedule, not layered onto the map. Restated here so it's visible from
the map-doc side, not only the shell-layout doc.
