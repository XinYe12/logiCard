# Atmosphere — STATUS

**Wave / Day:** Permanent department seat (GDD §11 / PARALLEL_OPS) — **In progress** 2026-08-14
**Branch / worktree:** `feat/atmosphere-stylized` @ `D:\projects\Game\logiCard-atmosphere-stylized`
**Last cross-reviewed:** 2026-08-14 — thunder height is now dynamic off the cloud shelf: `shape.length = cloudRise` on ConeVolume bolt layers (`FitZapHeightToCloudRise`), tip Y = mass `center.y` from fixed `HeightUnits × InterimCloudHeightBoost` placement.

## Owned files (this seat)

- `BoardWeatherPocket.cs`, `Resources/Weather/**`, `WeatherPackImportTool`, `Tools/gen_soft_cloud_atlas.py`, `Tools/gen_clay_sphere_shade.py`, weather PlayMode smoke, this STATUS

## Dirty (uncommitted)

1. **Centered cloud shelf** + **modular `ApplyWeather` API**.
2. **Storm Zap height ↔ cloud height** — ground spawn, upright, scale 1. `StrikeTipWorldY` = mass bounds center (the fixed shelf Y). `FitZapHeightToCloudRise`: bolt Cone layers → `ConeVolume`, `shape.length = cloudRise`, stretch `lengthScale = 0.75` (prefab 2 overshot). `startSize` untouched. Fair unchanged.
3. **PlayMode smoke** — ground spawn, upright, scale 1, ConeVolume length tracks cloud rise (not prefab 5).

## Blocked

- Human Re-Play — tip should sit in the clay and move with shelf height if that constant changes.
