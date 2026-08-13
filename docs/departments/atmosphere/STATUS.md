# Atmosphere — STATUS

**Wave / Day:** Permanent department seat (GDD §11 / PARALLEL_OPS) — **In progress** 2026-08-13
**Branch / worktree:** `feat/atmosphere-stylized` @ `D:\projects\Game\logiCard-atmosphere-stylized`
**Last cross-reviewed:** 2026-08-13 — human Play, fifth round: height/gluing now reading OK; two new asks from a wide shot + a close-up crop + a reference image: more stylized (close-up showed glossy render-ball highlights), and clouds should be built from small irregular pieces assembled into a triangular dense-middle/loose-edge shape (screenshots cleaned up after review, per this lane's pattern)

## Owned files (this seat)

- `BoardWeatherPocket.cs`, `Resources/Weather/**`, `WeatherPackImportTool`, `Tools/gen_soft_cloud_atlas.py`, weather PlayMode smoke, this STATUS

## Verdict (prior pass — height/gluing landed, shading/shape didn't go far enough)

Wide shot showed height and gluing both reading fine now (no further complaint on either). A close-up crop exposed the shading: the smooth continuous crown→belly gradient reads as a glossy 3D-render highlight at close range, not painted/stylized. And a reference image (anime sky, fluffy painted clouds) named the target composition directly: individual clouds should be visibly built from small irregular pieces, arranged dense/thick in the middle and loose/thin at the sides — "almost triangular," not a uniform cauliflower ball of same-size lobes (which is what the previous `GenerateCloudCluster` produced, even with the narrowed radius band).

## In progress / just landed (this pass, unverified — see Blocked)

1. **Two-layer cloud system, as requested.** `SpawnCloudPuff` (Layer 1) is the old cluster generator, renamed and now used at *puff* scale — 2-5 lobes, a small irregular chunk, not a whole cloud. `PlaceClayMass` (Layer 2) is new: it assembles 7-10 puffs per formation using `TriangularSample()` (symmetric triangular distribution, peaks at 0) along local X, so puffs land denser and bigger near the formation's center and sparser/smaller toward its edges — the "dense middle, loose sides, almost triangular" shape from the reference. Fringe puffs also drift slightly upward (wispy tendrils).
2. **Posterized shading** — `ClaySphereShade.png` redrawn as 3 flat bands (crown/mid/belly) with a soft feather between them instead of one continuous gradient. Same color direction (human said they're happy with the color) — this only changes how the tone steps, from smooth/glossy to flat/painted.

## Blocked

- Human Re-Play needed — untested since edit (Editor has this worktree open per `Assets/_Recovery` crash-snapshot, so no batchmode this pass either; this is a pure look call same as always). No screenshot yet.

## Offers

- If puffs still read too uniform/round: narrow `puffMinR`/`puffMaxR` further or drop `puffLobes` at the fringe (currently lerps 5→2).
- If the triangular shape isn't legible enough: strengthen the `puffScale` falloff (currently 1→0.4) or the `TriangularSample` peak (could exponentiate `u` for a sharper center bias).
- If shading still doesn't read as "stylized" enough: the bands can be widened/narrowed or a fourth band added — `ClaySphereShade.png` generation is a short inline Python script, easy to re-tune blind next round too, but a real screenshot is the only way to know which direction to push.
