# Map — STATUS

**Wave / Day:** New permanent seat, stood up 2026-08-13. **Not yet registered in `docs/departments/INDEX.md`**
(Integrator-owned file — this seat cannot self-register; flagged below, not assumed done).
**Branch / worktree:** `logiCard-map` / `dept/map` @ `d605789` (matches `master` tip at seat start; docs-only
commit(s) on top, no code yet)
**Last cross-reviewed:** 2026-08-13 — first session, self-review only (no peer department has reviewed this
seat's output yet)

## Scope (per seat brief)

- **Owns:** map/room/floor **presentation** construction standard and (Phase 2) rebuild — materials, prop
  dressing, per-`MapSurfaceRole` visual language. `BoardView.cs`, `BoardSurfaceMaterials.cs` presentation
  logic.
- **Explicitly does not own (read-only):** `MapDefinitions.cs`/room-rectangle authorship, `GameBootstrap`'s
  `BuildXxxGeometry()`/wall/door Sim authoring, `ArenaBoard`, pathfinding, Door API. Those stay Sim-layer,
  locked by **C57/C35/C39/C41**, and are not this department's to rewrite per the seat brief.
- **Adjacent, not owned:** weather/cloud style (Atmosphere dept lane — `BoardWeatherPocket`), character pawn
  materials (Character dept lane), HUD/board-anchored prompts (UI dept lane).

## Done

- Read-in-order pass: `PARALLEL_OPS.md` → `departments/INDEX.md` → (this file didn't exist yet — see
  Blocked) → `GDD.md` full pass (§8/§11) → `ART_DIRECTION.md` full → `PRODUCT_MEMORY.md` C29/C53/C54/C57/C58/C60
  → `MAP_AUTHORING.md` → code read (`BoardView.cs`, `BoardSurfaceMaterials.cs`, `BoardReflectionProbes.cs`,
  `GameBootstrap.cs`'s three `BuildXxxGeometry()` methods, `MapDefinitions.cs`) → asset-pack survey
  (`Assets/ithappy/Cartoon_City_Free`, `Assets/nappin/**`, both `THIRD_PARTY.md`s, `ART_PACK_RESEARCH.md`).
- Wrote `docs/MAP_PRESENTATION_STANDARD.md` (Phase 1 deliverable): diagnoses why **C58**/**C60**'s vibrancy
  passes didn't hold (photographic-PBR-plus-tint and an HDRP-sourced muted "modern office" pack were graded
  harder twice instead of swapped for an inherently saturated flat/toon material family — see that doc §1 for
  the full trace, including the finding that `ART_PACK_RESEARCH.md`'s own 2026-08-10/11 "soft clay, not wet
  PBR" verdict was never actually implemented in `BoardSurfaceMaterials.cs`, which still runs
  `BuildWetSurface()` for every room floor). Proposes a material-family standard (§2) and explicitly
  disambiguates the "rebuild with blocks" instruction as modular-pieces-within-C57, not a data-driven map
  format reversal (§0).

## In progress

- None — Phase 1 is docs-only per the brief; stopping here for human/Integrator review before Phase 2 code.

## Blocked

- **This seat is not yet listed in `docs/departments/INDEX.md`'s live-folders table or ownership matrix.**
  Per `PARALLEL_OPS.md`, only Integrator edits that file — flagging for Integrator to add a fifth permanent-seat
  row (or fold this under an existing seat, if that's the human's call) rather than self-registering.
- **`docs/MAP_PRESENTATION_STANDARD.md` §4's open question is not resolved.** It proposes amending part of
  **C53** (materials move toward flat/toon, away from photographic-real, for board surfaces specifically) —
  that needs explicit human confirm → a new `PRODUCT_MEMORY.md` C-row before Phase 2 starts recoloring
  anything, per this project's save-file-rule governance. Phase 2 should not start until that answer lands.

## Offers

- Phase 2 (once §4 is confirmed): rebuild the three existing maps' floors/walls/door materials and make
  `PlaceRoomDressing` map-aware, per `MAP_PRESENTATION_STANDARD.md` §5's ordered plan.
