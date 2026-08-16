# Atmosphere — STATUS

**Wave / Day:** Match Shell Layout (weather ↔ MapViewport docs) — **Ready** 2026-08-15  
**Branch / worktree:** `feat/atmosphere-stylized` @ `D:\projects\Game\logiCard-atmosphere-stylized`  
**Last cross-reviewed:** 2026-08-15 — Match Shell framing note landed; Storm DoD 1–2 still dirty/uncommitted (separate from this docs wave).

## Owned files (this seat)

- `BoardWeatherPocket.cs`, `Resources/Weather/**`, `WeatherPackImportTool`, weather PlayMode smoke, this STATUS
- Mood lighting overrides while a weather module is active
- `CLOUD_MOTION.md` — clay bank animation plan
- `WEATHER_MAP_VIEWPORT.md` — Match Shell MapViewport framing + letterbox assumptions

## This wave (Match Shell — docs)

**Goal:** Sky/clouds/rain stay attached to the diorama inside **MapViewport**, not full-screen wallpaper under Hand/Tool/Timeline (~45% chrome).

| Deliverable | Status |
|-------------|--------|
| Framing note: pocket board-local; avoid full-screen clear as sky | **Done** — `WEATHER_MAP_VIEWPORT.md` |
| List `BoardWeatherPocket` assumptions that break under MapViewport letterbox | **Done** — same doc (Sunny `Camera.main` clear = highest risk) |
| Optional local Play visual | **Skipped** — code-reviewed only; recheck when UI freezes band fractions |
| Code / Sunny land / Fair-lightning / ProgramHud / merge / push | **Not done (by brief)** |

### Integrator summary

- **Recommend:** Keep weather as board-footprint scene content; map camera `rect` = MapViewport; SolidColor/Sunny clear only on that camera.
- **Highest blocker for shell:** Sunny mood writes `Camera.main.backgroundColor` — full-bleed or wrong-camera clear will wash chrome. Fix when Camera slice retargets rect (Atmosphere can tighten later; not this wave).
- **Also watch:** global ambient/Volume grade through translucent HUD; `WeatherToggleUi` overlay vs InfoBar; ortho retune when MapViewport ≠ today’s ~58% middle band.
- **No code merge from this seat for this wave.** Storm contract DoD 1–2 remains separate dirty work (unrelated).

## Committed

- Zap tip / energize / Sunny mood + toggle + Phase A drift (`25bd79b` and earlier).
- Fix: Fair (and Sunny) no longer spawn the white-Zap lightning loop — `PlaceLightning` is now Storm-only; dead `LightningWhiteResourcePath`/`_lightningWhitePrefab`/`FairLightningInterval*` constants removed; PlayMode assertion added (`afb3f15`). Note: batchmode PlayMode couldn't fully verify this pass — pre-existing `ApplyWeatherSameMoodKeepsCloudBankInstance` (uncommitted DoD 1 work below) fails to compile on 6000.5.5f1 (`GetInstanceID()` is now error-level obsolete); unrelated to this fix, flagged for whoever picks up DoD 1–2.

## Dirty (carry — Storm contract DoD; not this wave)

1. **Same-mood early-out** in `ApplyWeather` — repeated Storm/Fair/Sunny no-ops; PlayMode asserts `CloudBank` instance identity.
2. **Fair↔Storm lighting round-trip** — `ApplyStormLightingDim` / `RestoreLightingIfOverridden` hardened (force restore before re-capture); PlayMode cycles Fair→Storm→Fair ×3 and checks Key + ambient restore.
3. **Deferred (optional DoD #3):** brief “storm rolling in” transition — still instant module swap.

## Blocked

- Match Shell visual sign-off: blocked on UI frozen MapViewport fractions + Camera `rect` retarget (Integrator).
- None for the docs deliverable itself.
