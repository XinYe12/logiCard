# Atmosphere — STATUS

**Wave / Day:** Permanent department seat (GDD §11 / PARALLEL_OPS) — **In progress** 2026-08-13
**Branch / worktree:** `feat/atmosphere-stylized` @ `D:\projects\Game\logiCard-atmosphere-stylized`
**Last cross-reviewed:** 2026-08-13 — human Play `image copy 15` (self-reviewed this pass, screenshot cleaned up after diagnosis)

## Owned files (this seat)

- `BoardWeatherPocket.cs`, `Resources/Weather/**`, `WeatherPackImportTool`, `Tools/gen_soft_cloud_atlas.py`, weather PlayMode smoke, this STATUS

## Verdict (`image copy 15`)

**Regression, not progress.** The Unlit + `ClaySphereShade` fix from the prior pass (`19c4a99`) killed the board-darkening shadow/terminator as intended, but overcorrected: the shade map's crown→belly gradient was only ~231-254 of 255 (near-invisible), so every lobe rendered flat white with no internal volume. Combined with the 2.15x lobe-oversize bump, the raft read as one blown-out white mass swallowing the board's back wall — worse than `image copy 14`, and a repeat of the earlier `playtest-2026-08-10-clouds-blocking-board` complaint.

## In progress / just landed (this pass, unverified — see Blocked)

1. `ClaySphereShade.png` redrawn with real contrast (~152-255, was ~231-254) — crown cream-white, belly clearly pale lavender-grey, mid-latitude (the limb, most visible from the 52° camera) kept light so it doesn't reintroduce the old Lit "边缘" grey ring.
2. Lobe diameter multiplier back to 2.0x (was 2.15x) — claws back the size bump that came with the flat-white pass.
3. No code changes to mass position/footprint — `image copy 12`/`14` framing (clouds hugging the upper board, not the geometry) wasn't the complaint; the flat shading was.

## Blocked

- Human Re-Play needed — untested since last Editor import (batchmode look gate doesn't apply; this is a pure look call). No screenshot yet for this pass.

## Offers

- If still too big/flat after next screenshot: shrink `InterimCloudScale` (currently 0.9) and/or `PatternRaft`'s central lobe (`RadiusNorm` 0.42) rather than touching mass position — the raft's one big central lobe is the widest single sphere in the bank.
