# Atmosphere — STATUS

**Wave / Day:** Permanent department seat (GDD §11 / PARALLEL_OPS) — **In progress** 2026-08-13
**Branch / worktree:** `feat/atmosphere-stylized` @ `D:\projects\Game\logiCard-atmosphere-stylized`
**Last cross-reviewed:** 2026-08-13 — fair signed off `af7b2e5`; storm committed `9f6c88d` for Integrator merge (storm look still awaiting human Re-Play)

## Owned files (this seat)

- `BoardWeatherPocket.cs`, `Resources/Weather/**`, `WeatherPackImportTool`, `Tools/gen_soft_cloud_atlas.py`, `Tools/gen_clay_sphere_shade.py`, weather PlayMode smoke, this STATUS

## Done

- **Fair clay bank locked** (`af7b2e5`) — two-layer mound, yaw-only ellipsoids, mass-height tint, near-flat shade. `Build(board, BoardWeatherMood.Fair)` keeps that look.

## In progress / just landed (unverified — see Blocked)

**Storm mood** (bootstrap default = `BoardWeatherMood.Storm`):

1. Slate-grey clay tints on the same mound shapes (fair geometry preserved).
2. Heavier pack rain (`PF_RainSystem`) + storm `PF_Fog_Main` / `PF_RainMist` volumes (rim-only mist still used; no FogGround slab).
3. Cooler/dimmer ambient + directional lights.
4. **LightningStorm** — 6 Vefects Zap rigs (`VFX_Zap_White` / `Blue` / `Yellow`) scattered over the board, ~0.9–2.8s intervals, ~55% double-strike. Import tool catalog updated for Blue/Yellow.

## Blocked

- Human Re-Play storm — say if grey is dark enough, lightning dense enough, or board too dim for readability.

## Offers

- If Zap too sparse: raise `StormLightningRigCount` or tighten intervals.
- If board unreadable: ease `ApplyStormLightingDim` (intensity multiply 0.62 → 0.75) before touching cloud tint.
- Fair remains one call away: `Build(_board, BoardWeatherMood.Fair)`.
