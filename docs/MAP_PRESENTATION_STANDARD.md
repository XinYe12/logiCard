# Map Presentation Standard — vibrant/cute diorama target

**Status:** Phase 1 (docs-only) — Map department, 2026-08-13.
**Depends on:** [GDD.md](GDD.md) §8/§11, [ART_DIRECTION.md](ART_DIRECTION.md), [MAP_AUTHORING.md](MAP_AUTHORING.md),
[ART_PACK_RESEARCH.md](ART_PACK_RESEARCH.md), [PRODUCT_MEMORY.md](PRODUCT_MEMORY.md) **C29 / C45 / C53 / C54 / C57 / C58 / C60**.
**Does not amend** C57 (hand-authored map geometry) — see §0. **Proposes amending** C53's material-realism
clause for board *surfaces* only — see §4, flagged for human confirm, not self-locked by this doc.

This is Phase 1 of a two-phase job: this doc only. Phase 2 (a real implementation pass rebuilding the three
existing maps' presentation against this standard) does not start until this doc is read and the §4 open
question is answered.

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

## 4. Open question for human confirm before Phase 2 (flagged, not resolved here)

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

---

## 5. Phase 2 preview (not started — docs-only per this brief)

Once §4 is confirmed:

1. Add a `Solid()`/gradient-based floor+wall material set to `BoardSurfaceMaterials`, keyed by the same four
   `MapSurfaceRole`s, replacing `BuildWetSurface()` as the default path (keep `BuildWetSurface` code only if a
   future non-board surface actually wants a wet-photo look — don't delete working code speculatively).
2. Re-skin nappin door/prop materials via the pack's own `(Mat)Gradient*` variants or a flattened duplicate,
   through the existing `InteriorPackImportTool` duplicate-and-convert pattern.
3. Make `PlaceRoomDressing` map-aware so Rail Platform / Vault Complex get real, in-room dressing instead of
   Freight-Yard-shaped coordinates or nothing.
4. Re-run `BuildDioramaVolume`/`BuildLighting` grading *after* the material swap, not before — grade a
   saturated base, don't re-grade the same muted one a third time.
5. Human screenshot check against the Link's Awakening reference (`ART_DIRECTION.md` Moodboard) before calling
   it done — every presentation change this project has shipped has needed one; batchmode green is not a look
   check (`docs/DIRECTING_AGENTS.md`).
