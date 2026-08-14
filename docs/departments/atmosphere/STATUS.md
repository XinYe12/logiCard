# Atmosphere — STATUS

**Wave / Day:** Permanent department seat (GDD §11 / PARALLEL_OPS) — **In progress** 2026-08-14
**Branch / worktree:** `feat/atmosphere-stylized` @ `D:\projects\Game\logiCard-atmosphere-stylized`
**Last cross-reviewed:** 2026-08-14 — Storm parked. Sunny look pass + weather toggle + cloud motion Phase A.

## Owned files (this seat)

- `BoardWeatherPocket.cs`, `Resources/Weather/**`, `WeatherPackImportTool`, weather PlayMode smoke, this STATUS
- Mood lighting overrides while a weather module is active
- `CLOUD_MOTION.md` — clay bank animation plan

## Committed (Storm — do not reopen unless asked)

- Zap tip ↔ cloud shelf (`b62b48a`); cloud energize rim groups (`45ccbc1`).

## Dirty (this pass)

1. **`BoardWeatherMood.Sunny`** — Sunshine / 万里无云; mood-owned `SunnySun` / `SunnySkyFill`; crush baseline directionals.
2. **Weather toggle button** — top-right `WeatherToggleUi` on the pocket host; Sunny ↔ Storm.
3. **Cloud motion Phase A** — `ClayCloudDrift` per mass (bob / drift / yaw). See `CLOUD_MOTION.md` for B/C.
4. Bootstrap default Sunny; PlayMode Sunny + Storm-apply smokes.

## Blocked

- Human Play — Sunny punch + Storm cloud drift via toggle; then decide Phase B puff breathe.
