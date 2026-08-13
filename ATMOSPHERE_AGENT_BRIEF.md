# Atmosphere stylized — AGENT BRIEF

**Where:** `D:\projects\Game\logiCard-atmosphere-stylized`  
**Branch:** `feat/atmosphere-stylized` @ `b32eda2`  
**Dept:** Presentation  
**Unity:** 6000.5.5f1 (`D:\unity\Editor\6000.5.5f1\Editor\Unity.exe`) — open **this** worktree path only, never `D:\projects\Game\logiCard`.

## Context (read first)

Human rejected Integrator scratch cotton (CreatePrimitive spheres + default particle squares) — screenshot `screenshots/image copy 8.png` on main. Rule locked in `docs/GDD.md` §8 + `docs/ART_DIRECTION.md`: atmosphere must be **animated, artistic, noticeable**, not realistic volumetric mush, **not** from-scratch primitives.

Main Integrator already **rolled back** `BoardWeatherPocket.cs` on master working tree to the committed pack-wired version so Play is usable again. Your job is the real atmosphere pass using **owned assets only**.

Also read: `docs/ART_PACK_RESEARCH.md` (USE NOW fog rows), `docs/PLAYBACK_CONTRACT.md` (don't touch), `docs/PARALLEL_OPS.md`.

## Research already done (do not re-invent)

| Asset | Path | Use |
|-------|------|-----|
| CloudAtlas (textured billboards) | `Assets/_Project/Art/Environment/Resources/Weather/CloudAtlas.png` | **Preferred clouds** — see commit `c0c4f39` `PlaceCloudPuff` / `CloudMaterial()` |
| Kenney smoke (CC0) | `Assets/_Project/Art/Environment/Textures/kenney_smoke_particles/whitePuff*.png` | Soft mist sprites — copy into `Resources/Weather/` if needed |
| Pack fog | `PF_Fog_Ground`, `PF_RainMist` already in Resources; **also** `Assets/RainSnowCloudEffect/Prefabs/PF_Fog_Main.prefab`, `PF_Fog_Distant.prefab` | Rim / distant mist only — never cover board center |
| Pack clouds | `PF_CloudLayer` | Demoted — open-world scale; human wants stylized toy read |
| Import tool | `Assets/_Project/Art/Editor/WeatherPackImportTool.cs` | Extend to copy `PF_Fog_Main` / `PF_Fog_Distant` if you use them |

**Do not** `CreatePrimitive` spheres/cubes as fog. **Do not** buy new packs this wave.

## The job (numbered)

1. Restore **CloudAtlas** cloud bank from `c0c4f39` (git show that commit's `BoardWeatherPocket` cloud helpers) — keep current rain + lightning wiring.
2. Replace pack `PlaceFogMist` with **rim-only** stylized mist: Kenney puff billboards and/or tuned `PF_Fog_Main`/`PF_Fog_Distant` at board edge, low particle count, clearly animated, **board center stays readable**.
3. If importing new prefabs, extend `WeatherPackImportTool` and run it in this worktree Editor once.
4. Play-mode visual check yourself (ortho board): no default white squares, no full-board white-out. Screenshot into worktree `screenshots/` if useful.
5. Update `docs/departments/presentation/STATUS.md` only (In progress → Done notes). Do **not** edit `DRAFT_HANDOFF.md`.

## Tests

- Disposable batchmode on **this** worktree if you touch code: EditMode + PlayMode that already exist (no new PlayMode required unless you add a smoke assert that WeatherPocket builds without throw).
- Do not claim green without a run. Editor on main project must stay closed for that path; your worktree is fine.

```
D:\unity\Editor\6000.5.5f1\Editor\Unity.exe -batchmode -nographics -acceptSoftwareTermsForThisRunOnly -projectPath D:\projects\Game\logiCard-atmosphere-stylized -runTests -testPlatform EditMode -testResults TestResults-EditMode.xml -logFile Logs\EditMode.log
```

(Same for PlayMode. Prefer **not** pairing `-quit` with `-runTests` if your machine hangs — match `docs/PARALLEL_OPS.md`.)

## Boundary — do not touch

| Path | Why (Integrator / other) |
|------|---------------------------|
| `Assets/_Project/Boot/GameBootstrap.cs` | Lighting / match-reset still dirty on main |
| `Assets/_Project/Boot/RoundPlayback.cs` | Rematch wound reset |
| `Assets/_Project/Board/BoardSurfaceMaterials.cs` | Urban floors on main |
| `Assets/_Project/Board/BoardReflectionProbes.cs` | Void clear sync on main |
| `Assets/_Project/Timeline/MatchClock.cs` | Reset API on main |
| `docs/DRAFT_HANDOFF.md`, `docs/SCHEDULE.md`, `docs/contracts/` | Integrator-only |
| `docs/GDD.md` / `ART_DIRECTION.md` atmosphere rows | Already locked; don't rewrite unless one-line sync required by brief |

**You own:** `BoardWeatherPocket.cs`, `WeatherPackImportTool.cs`, optional new files under `Assets/_Project/Board/` or `Resources/Weather/` for Kenney copies, `docs/departments/presentation/STATUS.md`.

No push, no merge to master, no force-push, no other worktrees.

## Why safe

Separate directory + Library; file scope above does not overlap Integrator's dirty floors/lighting/death-reset files.

## Report back

- What you wired (CloudAtlas? Kenney? which PF_Fog_*)
- Particle counts / placement notes
- Screenshot path if any
- Test results
- Commit on `feat/atmosphere-stylized` only; human merges
