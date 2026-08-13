# Atmosphere — STATUS

**Wave / Day:** Permanent department seat (GDD §11 / PARALLEL_OPS) — **In progress** 2026-08-13
**Branch / worktree:** `feat/atmosphere-stylized` @ `D:\projects\Game\logiCard-atmosphere-stylized`
**Last cross-reviewed:** 2026-08-13 — human Play, sixth round: triangular formation shape confirmed good ("that part you nailed it") — explicitly scoped this round to individual lobe shape only: every lobe is still a literal unmodified sphere primitive, and it shows (screenshot cleaned up after review, per this lane's pattern)

## Owned files (this seat)

- `BoardWeatherPocket.cs`, `Resources/Weather/**`, `WeatherPackImportTool`, `Tools/gen_soft_cloud_atlas.py`, weather PlayMode smoke, this STATUS

## Verdict (prior pass — formation shape confirmed, lobe shape is the remaining problem)

The triangular dense-middle/loose-edge formation assembly (`PlaceClayMass` Layer 2) is approved, unchanged this pass. Human was explicit: the remaining "looks like spheres" complaint is about each individual lobe's own geometry, not the macro arrangement. Asked for a "揉面团" (kneading dough) effect — squeeze/press/push/pound a sphere into an irregular lump, then round the result so it settles into a soft "sandbag," still roughly ball-shaped overall but no longer a sphere.

## In progress / just landed (this pass, unverified — see Blocked)

**`KneadClayLobeMesh`** — every lobe now gets its own deformed mesh (previously all lobes shared one `_unitSphereMesh` instance; that's why they all read as identical perfect spheres). Per lobe: clone the base sphere's vertices, apply 5 radial "dent" displacements (squeeze = two opposite-side pinches, press = one broad soft dent, push = one broad soft bulge, pound = one small sharp dent — the "pointy edges"), then a partial Laplacian relax (3 iterations) to round those sharp transitions into curves, then a uniform rescale back to the original average radius so relaxation's natural shrinkage doesn't desync the lobe from its tuned on-screen `RadiusNorm`/diameter. `intensity01: 0.24` is the overall knead strength knob. Non-shared meshes need explicit cleanup on rebuild — added `DestroyKneadedLobeMeshes`, called from `Build()` before the old children are destroyed, so repeated `Build()` calls (weather rebuilt between matches) don't leak `Mesh` objects.

## Blocked

- Human Re-Play needed — untested since edit (Editor has this worktree open per `Assets/_Recovery` crash-snapshot, so no batchmode this pass either; this is a pure look call same as always). No screenshot yet. This is also the first pass that couldn't be sanity-checked by reasoning about screen-space size/position math the way height/spacing fixes could — mesh deformation only shows itself in a render, so there's more uncertainty than usual in how far off `intensity01`/dent-angle ranges might be.

## Offers

- If lobes read barely different from spheres: raise `intensity01` (currently 0.24) — I deliberately started conservative given no way to preview the result, to avoid folded/self-intersecting geometry from stacked dents on a moderate-resolution primitive sphere.
- If lobes look broken/spiky/folded: lower `intensity01`, or raise `roundIterations` (currently 3) for a stronger relax pass.
- If the dough look is right but the *pattern* per lobe feels repetitive: dent angle ranges (`RandomFalloffAngle` calls in `KneadClayLobeMesh`) are the next thing to randomize further.
