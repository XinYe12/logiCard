# Human decision sheet — C53 surface-material amendment

**For:** `MAP_PRESENTATION_STANDARD.md` §4. **One decision, one page.** Map department, 2026-08-14.

**ANSWERED 2026-08-14: YES** — human confirmed the amendment via this sheet. Board surface materials move to
flat/toon; geometry density and Atmosphere untouched, as scoped below. Human also chose, on the baseline
conflict, to **wait for Integrator to commit/reclaim** the dirty `BoardView.cs`/`BoardSurfaceMaterials.cs`/
`BoardReflectionProbes.cs` before Phase 2 code starts — see `departments/map/STATUS.md` for current status.
**Still outstanding before Phase 2 code:** a `PRODUCT_MEMORY.md` C-row recording this amendment (Integrator
writes this, not Map) and Integrator opening the Phase 2 contract.

---

## The question

**C53** (2026-08-09) says board materials/architecture should move *"toward realistic detail rather than
clay-primitive/chibi."* Two vibrancy passes since (**C58**, **C60**) tried to hit "vibrant/cute, Link's
Awakening" on top of that photographic-PBR base and both shipped without fixing the "board reads dark/grey"
complaint — because grading (tint/saturation/exposure) can't turn a real photo texture into flat toy-plastic;
the muted look is baked into the source texel data (see `MAP_PRESENTATION_STANDARD.md` §1 for the full trace).

**Amend C53, for board surface materials only:** switch floors, walls, door-leaf tint base, and interior-prop
tint from photographic-PBR-plus-tint to a flat/gradient toon material family (`Solid()`-style — already in
`BoardSurfaceMaterials.cs`, already used at the board's edges, never on the stage itself).

**Everything else in C53 stays locked as-is:** geometry density / room complexity (vents, breaches — C57),
bounded-chunk dark-void camera language, Yard/Hall/Vault/Flank room structure, and the weather/atmosphere
system (Atmosphere department's lane — untouched by this proposal).

## Recommended default: **YES**

- It targets the actual cause (base material family), not a fourth grading pass on the same photo textures.
- It's free — no new asset purchases; `Solid()` and Cartoon_City_Free's flat materials are already imported,
  licensed, and in the codebase, sitting unused on the stage.
- It matches a verdict `ART_PACK_RESEARCH.md` already reached and locked ("soft clay... not wet PBR") on
  2026-08-10/11 — this closes a gap between an already-decided direction and code that never implemented it.
- It's reversible at the material layer only: `MapSurfaceRole` lookup shape is untouched, so a future revert
  swaps materials back without touching geometry, Sim, or the room-role plumbing.

## Risk if the answer is **NO**

Phase 2 either stays blocked indefinitely, or ships a third grading pass on the same photographic-PBR/HDRP
base that the two prior C58/C60 passes already showed doesn't move the "dark/grey/nobody wants to play this"
complaint — spending another wave's effort on a change with a documented history of not working.

## What this decision does NOT cover

- No change to `MapDefinitions`, `GameBootstrap.BuildXxxGeometry()`, pathfinding, or Door API (C57/C35/C39/C41
  stay locked).
- No mesh re-sourcing — nappin/imported door and furniture meshes stay; only the material they're painted with
  changes.
- No weather/atmosphere change (Atmosphere department's lane).

## If yes: what happens next

A new `PRODUCT_MEMORY.md` C-row recording the amendment (Integrator writes this — save-file rule), then
Integrator opens a Phase 2 contract per the checklist below. Map does not start Phase 2 code before both of
those land.
