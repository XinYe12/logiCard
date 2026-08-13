# Atmosphere — STATUS

**Wave / Day:** Permanent department seat (GDD §11 / PARALLEL_OPS) — **In progress** 2026-08-13
**Branch / worktree:** `feat/atmosphere-stylized` @ `D:\projects\Game\logiCard-atmosphere-stylized`
**Last cross-reviewed:** 2026-08-13 — storm Play: clouds too spread out; want centered bank + modular weather for cards; Zap White+Yellow only

## Owned files (this seat)

- `BoardWeatherPocket.cs`, `Resources/Weather/**`, `WeatherPackImportTool`, `Tools/gen_soft_cloud_atlas.py`, `Tools/gen_clay_sphere_shade.py`, weather PlayMode smoke, this STATUS

## Done

- Fair clay bank signed `af7b2e5`; storm mood landed `9f6c88d` (pre-center / pre-module).

## In progress / just landed (unverified)

1. **Centered cloud shelf** — mass X span ~±0.26 board width (was ±0.68–0.78); Fair + Storm share placement.
2. **Modular weather API for cards** — `Build(board)` binds host; `ApplyWeather(Clear|Fair|Storm)` swaps a self-contained `Weather_*` child; `ClearWeather()` tears down + restores lighting. Bootstrap still mounts Storm for Play.
3. **Storm Zap = Yellow only** — tip scale `StormZapVerticalScale` **0.52 → 0.34** after Re-Play still showed tip in the void above the bank.

## Blocked

- Human Re-Play — confirm tip now inside the Storm clouds (not above).

## Offers

- If tip still above: lower further (~0.28) or raise cloud shelf slightly.
- If tip too short / buried: ease scale toward 0.40.
- Card hook: `ApplyWeather(BoardWeatherMood.Storm)`.
