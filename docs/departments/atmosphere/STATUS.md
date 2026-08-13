# Atmosphere — STATUS

**Wave / Day:** Permanent department seat (GDD §11 / PARALLEL_OPS) — **In progress** 2026-08-13
**Branch / worktree:** `feat/atmosphere-stylized` @ `D:\projects\Game\logiCard-atmosphere-stylized`
**Last cross-reviewed:** 2026-08-13 — human Play, third `image copy 15` re-take: shade-contrast + haze direction both confirmed good; asked for smaller/more/wider-spread masses and denser haze (screenshot cleaned up after review, per this lane's pattern)

## Owned files (this seat)

- `BoardWeatherPocket.cs`, `Resources/Weather/**`, `WeatherPackImportTool`, `Tools/gen_soft_cloud_atlas.py`, weather PlayMode smoke, this STATUS

## Verdict (prior pass, confirmed)

Shade-contrast fix and the first edge-haze pass both landed well: lobes read as separate glued pillows, haze direction liked. Two refinements asked from that round, both landed this pass (unverified — see Blocked):

1. **Smaller, more, wider-spread masses** — `CloudMasses` replaced the old 4 hand-placed masses (one ~0.85x-board-width lobe) with a data-driven `CloudMassSpec[]` of 7 smaller masses spanning roughly -0.85..+0.85 of board width (added `Mass_W2`/`Mass_NE`/`Mass_E2`). Biggest single lobe is now ~0.4x board width. `InterimCloudScale` (the old single global size knob) is gone — each mass's scale factors are explicit in its spec now, since a shared multiplier doesn't work once masses have deliberately different sizes. Pattern assignment now cycles the shuffled 6-pattern pool (`ShuffledPatternCycle`) to cover 7 masses without immediate repeats.
2. **Denser haze** — `PlaceCloudEdgeHaze` startColor alpha raised 0.14-0.30 → 0.34-0.58, envelope multiplier tightened 1.1x → 1.05x (less "floating separate dot" look, more hugging the mass), particle count/size bumped slightly for tighter coverage on the now-smaller masses.

## Blocked

- Human Re-Play needed — untested since edit (Editor has this worktree open per `Assets/_Recovery` crash-snapshot, so no batchmode this pass either; this is a pure look call same as always). No screenshot yet.

## Offers

- If 7 masses still don't read as "continuous," add more (the spec array is now trivial to extend) rather than growing individual masses back up.
- If haze is now too heavy/foggy: pull the new 0.58 alpha ceiling back down before touching count/size.
- Composition (X/Z spread, depth/height per mass) is a first pass at "spread wide" — likely needs another iteration once seen in Play.
