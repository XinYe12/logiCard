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
- Fix: Fair (and Sunny) no longer spawn the white-Zap lightning loop — `PlaceLightning` is now Storm-only; dead `LightningWhiteResourcePath`/`_lightningWhitePrefab`/`FairLightningInterval*` constants removed; PlayMode assertion added (`afb3f15`).
- **Storm contract DoD 1–2** (idempotent same-mood `ApplyWeather` early-out + Fair↔Storm lighting round-trip) landed on `master` via `c051731`, merged in `0857b80` — no longer dirty/carry, both covered by `ApplyWeatherSameMoodKeepsCloudBankInstance` / `FairStormLightingRoundTripsAcrossRepeatedCycles` and green in every batchmode run since.
- **Optional DoD #3 — "storm rolling in" transition**, from `docs/departments/atmosphere/STORM_TRANSITION_AGENT_BRIEF.md`, worked from a fresh `feat/storm-transition` worktree off `master` (the old `feat/atmosphere-stylized` copy of `BoardWeatherPocket.cs` was 116 commits stale). `ApplyWeather` still builds the Fair/Storm module at its true final position first — unchanged, so every world-space measurement taken during placement (cloud envelope bounds, `PlaceLightning`'s glue-to-CloudBank height from `b62b48a`) is locked in exactly as before. Only afterward does `PlayModuleRollIn` offset the finished module transform (start position `(width * 0.85, -2.2, 0)`, ease-out quad, 1.1s) and slide it back to origin — a **rigid translation**, not a scale. That choice was deliberate: a first pass scaled the module root down/up instead, which broke `WeatherPocketBuildsCloudBankAndRimMistWithoutThrow` — shrinking the transform desynced the Zap `ConeVolume.shape.length` (a raw local number, unaffected by parent scale) from the live (now-shrunk) CloudBank bounds a test measures synchronously right after `ApplyWeather` returns. Translation has no such gap: every presenter/test only ever reads positions *relative* to the CloudBank, and a rigid translation preserves every such relative distance at every instant of the slide, not just once it settles. Sunny is untouched (no cloud content to roll in); Clear teardown is untouched (brief only asked for the build-in cue). Satisfies PLAYBACK_CONTRACT §2 rule 4 by construction: same-mood `ApplyWeather` calls already early-out before reaching `PlayModuleRollIn`, and `ClearWeather`'s existing `StopAllCoroutines()` cleanly kills any in-flight roll-in before the next module builds, so scrubbing back/forward across a mood boundary replays consistently with no stacked coroutines or stale offset. New PlayMode test `ModuleRollInSettlesAtFullScaleAfterRepeatedRewindAndReplay` covers exactly that rewind/replay shape. Batchmode-verified fresh on `feat/storm-transition`, Editor closed: **EditMode 190/190, PlayMode 67/67** (the +1 is the new test; the two DoD 1–2 tests below — `ApplyWeatherSameMoodKeepsCloudBankInstance`, `FairStormLightingRoundTripsAcrossRepeatedCycles` — pass unchanged). Commit `9adbe26`. Not merged/pushed — report-back for Integrator review, per the brief.

## Dirty (carry)

- None — Storm contract DoD 1–3 are all landed/committed (DoD 1–2 on `master` via `c051731`; DoD 3 on `feat/storm-transition`, see Committed above, awaiting Integrator review/merge).

## Blocked

- Match Shell visual sign-off: blocked on UI frozen MapViewport fractions + Camera `rect` retarget (Integrator).
- None for the docs deliverable itself.
