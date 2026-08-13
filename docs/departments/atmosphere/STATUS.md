# Atmosphere — STATUS

**Wave / Day:** Permanent department seat (GDD §11 / PARALLEL_OPS) — **In progress** 2026-08-13
**Branch / worktree:** `feat/atmosphere-stylized` @ `D:\projects\Game\logiCard-atmosphere-stylized`
**Last cross-reviewed:** 2026-08-13 — human Play, second `image copy 15` re-take: "looks better" (shade-contrast fix confirmed working; screenshot cleaned up after review, per this lane's pattern)

## Owned files (this seat)

- `BoardWeatherPocket.cs`, `Resources/Weather/**`, `WeatherPackImportTool`, `Tools/gen_soft_cloud_atlas.py`, weather PlayMode smoke, this STATUS

## Verdict (shade-contrast fix, confirmed)

Human Play on the redrawn `ClaySphereShade.png` + 2.0x lobe size (previous pass) came back positive ("looks better") — lobes read as separate glued pillows again, board's back wall visible. No further action needed on that regression.

## In progress / just landed (this pass, unverified — see Blocked)

Two follow-up asks from the same Play round: soften the cloud silhouette edges, and add more shape variety so repeat matches don't look identical.

1. **Edge haze** — new `PlaceCloudEdgeHaze`: a thin CloudAtlas billboard fringe (shell-emission `Box` shape, low alpha 0.14-0.30, slow drift) riding just outside each clay mass's envelope. The opaque Unlit spheres have no Fresnel/rim-alpha option without a custom shader (too risky to author blind, no way to compile-check this session), so this reuses the already-proven `PlaceRimMistPuff` billboard technique instead of new shader code — same material (`MistMaterial()`), same alpha-blend/no-additive reasoning as the existing rim mist.
2. **Pattern variety** — added `PatternAnvil` (flat wide shelf) and `PatternDrift` (loose scattered cluster) to the existing 4, so there are 6 distinct lobe-cluster silhouettes. `PlaceCloudBank` now shuffles the pool and assigns 4 distinct patterns to the 4 fixed mass slots (position/size/tint composition unchanged — that framing is Play-approved) plus a random Y yaw per mass, once per `Build()`.

## Blocked

- Human Re-Play needed for both — untested since edit (Editor has this worktree open per `Assets/_Recovery` crash-snapshot, so no batchmode this pass either; this is a pure look call same as always). No screenshot yet.

## Offers

- If haze reads as fog rather than blur, or the board readability regresses: cut `PlaceCloudEdgeHaze`'s alpha range or shrink the 1.1x envelope multiplier first, before touching the clay lobes themselves.
- If variety still reads same-y at a glance: the 4 mass slots keep fixed position/size, only pattern+yaw randomize — could also vary `InterimCloudScale` per-mass slightly next pass if that's not enough.
