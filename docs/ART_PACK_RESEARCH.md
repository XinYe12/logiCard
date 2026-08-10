# Art pack research — clay / stylized shopping notes

**Date:** 2026-08-10 (updated same day after reference screenshot + Office Pack Free confirm)  
**Branch:** `feat/art-pack-research`  
**Scope:** Research only. No imports from this agent. Human already added Office Pack Free to Unity My Assets.

## Verdict (current)

**Primary look = soft clay / curvy-minimal** (the office screenshot vibe: matte rounded props, soft lighting, glass UI) — **not** wet PBR + Quaternius, and **not** Synty-first anymore.

| Priority | What | Buy? |
|---|---|---|
| Locked | **nappin Office Pack – Free** | Already in your Unity assets — use this as the style anchor |
| Next free (same style family) | nappin **House Interior** + **Weapons** | Yes — add from Asset Store while still free |
| Weather (rain/clouds/fog) | Free URP weather VFX + free clouds/sky | Yes — free first |
| Lightning | Free Zap / FX Lightning | Yes — free |
| Buildings + streets | Free low-poly city / Kenney city kits | Yes — free first; accept slightly blockier roads |
| Tornado | Paid stylized VFX (~$40) | Only if you actually need a tornado beat |
| Railway / 高铁 | Paid modular railway (~$10–$30) | Catalog now; **deferred from 14-day ship (C31/C34)** |
| Synty POLYGON Heist/Office/City | ~$75–$100 | **Hold** — denser modular rooms later if free clay pack feels empty |

Most of the “premium” feel in the reference is **lighting + matte materials + frosted UI**, not expensive meshes.

---

## Style lock: nappin softpack family (confirmed)

Publisher: **nappin** — curvy, gradient-matte, soft edges. Matches the clay screenshot better than Synty faceted POLYGON.

| Pack | Link | Price | Role for logiCard |
|---|---|---|---|
| **Office Pack – Free** | [Asset Store](https://assetstore.unity.com/packages/3d/props/interior/office-pack-free-258600) · [nappin.dev](https://nappin.dev/details/officeEssentialsPack.html) | Free (listed “limited time”) | **Hall / facility dressing**: desks, chairs, doors, plants, lights, kitchenette. Replaces Quaternius house-interior subset. |
| **House Interior – Free** | [Asset Store](https://assetstore.unity.com/packages/3d/props/interior/house-interior-free-258782) | Free | Extra sofas/beds/appliances if any room needs “lived-in” props |
| **Weapons Pack – Free** | [Asset Store](https://assetstore.unity.com/packages/3d/props/weapons/weapons-pack-free-259025) | Free | Stylized guns for Shoot readability / pawn props (same clay silhouette language) |
| Racing tileset (micro) | [itch](https://nappin.itch.io/racing-microset) | Free | Tiny road tiles only — **not** a city builder; skip unless you want grid road experiments |

**URP note:** Packs ship Built-in materials; nappin provides URP integration packages on nappin.dev — use those for this Unity 6000 URP project.

**Does not include:** outdoor city shells, streets at scale, railway, storm systems, characters. Those are separate categories below.

---

## Category shopping list

### 1. Weather — storm, clouds, rain, lightning, tornado

| Need | Recommend | Price | Notes |
|---|---|---|---|
| Rain + snow + fog/clouds (URP particles) | **[FREE] Cinematic Weather VFX Bundle** | Free | [Asset Store](https://assetstore.unity.com/packages/vfx/particles/free-cinematic-weather-vfx-bundle-rain-snow-fog-urp-particle-sys-382428) — drop-in ParticleSystem prefabs; good replacement for Kenney smoke-as-clouds + upgrades rain |
| Cartoon sky / sun-moon clouds | **SIMPLE Sky – Cartoon assets** (Synty) | Free | [Asset Store](https://assetstore.unity.com/packages/3d/environments/simple-sky-cartoon-assets-42373) — clay-friendly sky dome; not a storm system |
| Mesh clouds (toy puffs) | Stylized Low Poly Clouds | ~$7.50 | Optional if particle fog still feels thin |
| Lightning | **Zap VFX – URP** | Free | [Asset Store](https://assetstore.unity.com/packages/vfx/particles/spells/zap-vfx-urp-303479) — stylized bolts + SFX |
| Lightning (alt) | FX Lightning II free | Free | Older but usable |
| Stylized rain + lightning + snow bundle | Simple Weather and Environment 3D VFX | ~€4.59 | Cheap paid if free packs look too realistic |
| Stylized FX kit (rain/fog/lightning flash) | Fx Pack – Environment Effects | ~$12 | Broader ambient kit |
| Tornado / whirlwind | Stylized Tornado and Whirlwind Magic VFX | ~$39.99 | Fantasy-looking; buy **only** if tornado is a real match beat |
| Full weather director (day/night seasons) | COZY: Stylized Weather 3 | $50 | **Overkill** for a small board pocket — skip for now |

**Practical weather stack for this game:** Cinematic Weather free (rain/fog) + Zap free (lightning) + SIMPLE Sky free (backdrop). Keep existing `BoardWeatherPocket` rain path if it already reads; swap cloud source first. Tornado stays optional/paid.

### 2. Buildings (different kinds)

Office Pack covers **interior** dressing, not building shells.

| Need | Recommend | Price | Notes |
|---|---|---|---|
| Quick free urban shells | **Free Low Poly Simple Urban City** | Free | [Asset Store](https://assetstore.unity.com/packages/3d/environments/urban/free-low-poly-simple-urban-city-3d-asset-pack-239474) — houses, cottage, street props; flatter low-poly than nappin, OK for far Yard backdrop |
| Modular office buildings + roads | **Low Poly City Starter Pack** (Mini World Studio) | Free | [Asset Store](https://assetstore.unity.com/packages/3d/environments/urban/low-poly-city-starter-pack-mini-world-studio-380946) — 3 office buildings + modular roads |
| Industrial / commercial / suburban kits | **Kenney City Kit** (Industrial, Commercial, Suburban, Roads) | Free CC0 | [kenney.nl](https://kenney.nl/assets/city-kit-industrial) — blockier than clay; fine for board-edge skyline if materials are retinted matte |
| Dense modular city (paid upgrade) | POLYGON City Pack (Synty) | $20 / ~$10 sale | Hold until free shells fail the Yard read |
| Heist / vault building kit (paid) | POLYGON Heist Pack | $29.99 / ~$15 sale | Genre-perfect vault/bank modules — buy later if Vault needs more than Office desks |

**Rule:** Prefer **one exterior language** (pick either Free Urban City *or* Mini World *or* Kenney — don’t mix all three in one shot). Keep nappin for **interiors**.

### 3. Streets / roads

| Need | Recommend | Price | Notes |
|---|---|---|---|
| Modular roads (with free city starter) | Mini World City Starter | Free | Included road/sidewalk/crosswalk modules |
| Roads + highways + barriers | **Kenney City Kit (Roads)** | Free CC0 | [itch](https://kenney-assets.itch.io/city-kit-roads) / kenney.nl |
| Roads bundled with free urban pack | Free Low Poly Simple Urban City | Free | Includes road paths / crossroads |

Streets are **Yard / board-edge dressing**, not gameplay navmesh — continuous pathfinding stays code-owned.

### 4. Railway (高铁 / rail)

Confirmed design exists (**C31**) but is **deferred from the 14-day ship (C34)**. Catalog only; do not block demo art on this.

| Pack | Price | Notes |
|---|---|---|
| **Modular Railway stylized lowpoly** | ~$9.99 | [Asset Store](https://assetstore.unity.com/packages/3d/environments/modular-railway-stylized-lowpoly-280486) — cheapest stylized modular tracks |
| Lowpoly Railway Pack | ~$25 | Locos, wagons, signals, crossings — fuller set |
| Stylized Railway Modular Constructor | ~$30 | Heavier constructor kit |

No strong free clay-matched railway pack found. When 高铁 returns to scope, start with the ~$10 modular kit and tint materials toward nappin matte gradients.

---

## What this replaces / keeps

| Current | Action |
|---|---|
| Quaternius Ultimate House Interior (`Resources/Interior/`) | Replace with nappin Office (doors/desks/lights/plants) |
| Poly Haven wet floors via `BoardSurfaceMaterials` | Retire wet PBR; use flat/gradient clay mats (nappin gradients or `Mat_Clay*`) |
| Kenney smoke → `CloudAtlas` | Replace with Cinematic Weather fog/clouds or SIMPLE Sky / mesh clouds |
| Quaternius Scout/Juggernaut | Still open — clay pill/soft characters or keep temporarily; nappin has **no** character pack |
| Path 线稿涂鸦, URP volumes, rain if OK | Keep; retune lighting toward soft bright clay |
| Door interaction code | Keep; swap mesh only |

---

## Integration sketch (still no code)

1. **Office props:** Import nappin Office + URP materials. Wrap needed prefabs under existing `Resources/Interior/` names (`Door`, `Table`, …) or retarget `BoardView` load strings once.
2. **Floors:** Point `YardFloor`/`HallFloor`/`VaultFloor` at matte solid/gradient materials — stop `BuildWetSurface` Poly Haven path.
3. **Weather:** Parent free rain/fog/lightning prefabs inside `BoardWeatherPocket` bounds; kill Kenney atlas clouds.
4. **Buildings/streets:** Place as non-colliding (or lightly colliding) Yard dressing outside the playable continuous footprint; don’t let road meshes fight pathfinding.
5. **Railway:** Don’t integrate until C31 is scheduled; keep purchase optional.
6. **UI:** Glass panels are UI work, not an asset pack — separate from this list.

---

## Buy order (aligned with your current call)

1. **Already done:** Office Pack Free  
2. **Add free now:** House Interior Free, Weapons Pack Free (same nappin look, still free)  
3. **Add free weather:** Cinematic Weather VFX + Zap VFX (+ optional SIMPLE Sky)  
4. **Add free exterior:** one of Mini World City Starter **or** Free Simple Urban City (+ Kenney Roads if needed)  
5. **Paid later only if needed:** Modular Railway (~$10) when 高铁 returns; Synty Heist if Vault needs bank modules; tornado VFX if designed in  

**Hold:** COZY ($50), ithappy Cartoon City ($489), full Synty cart — wrong spend for clay-first.

---

## Earlier Synty recommendation (superseded for now)

The first pass recommended POLYGON Heist + Office + City (~$100). That remains a valid **upgrade path** for denser modular heist architecture, but your screenshot + Office Pack Free confirm push the near-term target to **nappin clay + free weather/city**. Revisit Synty only after the free clay stack is in-scene and still feels thin.

---

## Price caveat

Prices sampled 2026-08-10; nappin packs are marked free for a limited time — grab House + Weapons while they are. Confirm URP packages from nappin.dev before dropping into the Unity 6000 project.
