# Map — STATUS

**Wave / Day:** Map Phase 2 (C65) — **Ready for Integrator merge**, 2026-08-14.
**Branch / worktree:** `logiCard-map` / `dept/map` (master merges through Atmosphere/Cards tips; Phase 2 commits on this branch).
**Last cross-reviewed:** human Play look signed (`screenshots/image copy 15.png` — floors + fences good).

## Scope (per seat brief)

- **Owns:** map/room/floor **presentation** — materials, prop dressing, per-`MapSurfaceRole` visual language.
  `BoardView.cs`, `BoardSurfaceMaterials.cs`.
- **Does not own:** `MapDefinitions` room authorship, `GameBootstrap` geometry/lighting, Sim/Door API,
  weather (Atmosphere), HUD (UI).

## Done

- Phase 1 docs + §4 YES → **C65**; Integrator opened Phase 2 contract.
- **Phase 2 implementation:**
  1. Flat/`Solid()` floors + walls keyed by `MapSurfaceRole`; `BuildWetSurface` kept, unused for board surfaces.
  2. Door leaf + opaque props re-skinned to Gradient*_URP / Solid (glass/emissive preserved).
  3. Map-aware `PlaceRoomDressing` for Freight Yard / Rail Platform / Vault Complex.
  4. Walls drawn as toy fences (cream panel + honey rails + dark posts), not coral brick slabs.
  5. EditMode `BoardSurfaceMaterialsTests`.
- **Human look:** `image copy 15.png` — floors and fences approved (“good!”).

## In progress

- None on Map. Waiting on Integrator merge.

## Blocked / Integrator follow-ups

- Batchmode not run on this branch (do not claim green).
- Contract DoD #4: optional lighting/`BuildDioramaVolume` re-pass on the new saturated base — Integrator owns
  `GameBootstrap`; Map did not edit it. Human already likes the current Play look; treat as optional polish.

## Offers

- After merge: Map idle unless a prop/dressing follow-up is requested.
