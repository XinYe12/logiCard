# Art pack research — clay / stylized shopping notes

**Date:** 2026-08-10 (updated same day after reference screenshot + Office Pack Free confirm); extended
2026-08-11 with a licensing note on the already-imported Synty packs, a Characters section, and
confirmed current Asset Store pricing (`docs/ASSET_PACK_AUDIT.md` is the companion doc — Part 1 of that
session's task, auditing what else is sitting in `D:\XinyeData\projects\assets`).
**Branch:** `feat/asset-pack-audit` (this update); originally `feat/art-pack-research`
**Scope:** Research only. No imports from this agent. Human already added Office Pack Free to Unity My Assets.

## Urgent: Synty POLYGON Heist/Office/City are already in the project, unlicensed — buy or replace

Three Synty packs (`Assets/PolygonHeist/`, `Assets/PolygonOffice/`, `Assets/PolygonCity/`) were already
imported raw from a local folder whose packaging strongly indicates a Chinese reseller bundle, not
individual Asset Store purchases — see `docs/DRAFT_HANDOFF.md`'s 2026-08-11 entry and
`docs/ASSET_PACK_AUDIT.md` for a follow-up audit that found the same reseller bundle sitting in the
external assets folder, containing those exact three packages each wrapped with an explicit "not for
commercial use, buy the genuine version" disclaimer and a Taobao storefront link. **This is a real
ship-blocking TODO**, not resolved by any research pass — the human made an informed call to prototype
with the raw import now and buy real licenses before any public release or Steam upload.

**If the human decides to keep the Synty look** (rather than pivot fully to the nappin clay direction
below), the direct action is buying legitimate Asset Store licenses for **the exact three packs already
in hand** — don't buy different Synty packs instead, these are already integrated in spirit even if not
yet wired. Current prices, confirmed live 2026-08-11:

| Pack | Asset Store link | Current price |
|---|---|---|
| POLYGON - Heist Pack - Art by Synty | [Asset Store](https://assetstore.unity.com/packages/3d/environments/urban/polygon-heist-pack-art-by-synty-97949) | $29.99 |
| POLYGON - Office Pack - Art by Synty | [Asset Store](https://assetstore.unity.com/packages/3d/props/interior/polygon-office-pack-art-by-synty-159492) | $49.99 |
| POLYGON - City Pack - Art by Synty | [Asset Store](https://assetstore.unity.com/packages/3d/environments/urban/polygon-city-pack-art-by-synty-95214) | $20.00 |
| **Total** | | **≈ $99.98** |

(All three list the standard Unity Asset Store EULA — commercial and Steam use is covered once purchased;
no separate paid-upgrade tier needed for this project's scale. Confirm against
[unity.com/legal/as-terms](https://unity.com/legal/as-terms) if terms matter for a contract/legal review,
but nothing found this pass suggests the standard EULA has changed from "commercial use OK once bought.")

If the nappin/clay pivot wins instead, these three packs become dead weight to strip back out — worth
deciding one way or the other before more integration work goes into either direction.

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
| Synty POLYGON Heist/Office/City | $99.98 confirmed (see urgent note above) | **Already imported raw, unlicensed — hold on *new* Synty purchases, but if keeping the Synty look, license these exact three, not different packs** |

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
| Dense modular city (already in hand, unlicensed) | POLYGON City Pack (Synty) | $20.00 confirmed, no sale active 2026-08-11 | Already imported raw — see urgent licensing note at top of doc |
| Heist / vault building kit (already in hand, unlicensed) | POLYGON Heist Pack | $29.99 confirmed, no sale active 2026-08-11 | Already imported raw — see urgent licensing note at top of doc |

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

### 5. Characters (Scout / Juggernaut) — new section, added 2026-08-11

**nappin has no character pack** — confirmed again this pass (checked nappin.dev's full product list
directly: Office Essentials, House Interior, Weapons Pack, plus non-art tools like Mobile Controller
Framework and GridForge — no characters, paid or free). Nothing stylistically identical to the
soft-clay/curvy-minimal look exists as a ready-made character line on the Asset Store as far as this pass
found. Two real candidates, neither a perfect match, both need real prep work:

| Pack | Link | Price | Fit notes |
|---|---|---|---|
| **Toony Tiny Citizens Megapack** (Polygon Blacksmith) | [Asset Store](https://assetstore.unity.com/packages/3d/characters/toony-tiny-citizens-megapack-99854) | $30 | Closest stylistic match found — soft, rounded, chunky-proportioned "toony" characters, reads closer to clay than any faceted-low-poly alternative. Delivered as a megapack of many character prefabs (not confirmed single-FBX-per-part-material-no-texture — likely does carry its own textures, unlike the current Quaternius pipeline's texture-free per-part-material approach). **Needs verification + probable rework** to fit `PawnImportTool.cs`'s expected shape (single FBX, per-part materials, no textures, a renderer literally named `"Body"` for `PawnView`'s `MaterialPropertyBlock` team-color tint) before it could drop in cleanly. |
| **Toony Tiny Soldiers** (same publisher/family) | [Asset Store](https://assetstore.unity.com/packages/3d/characters/toony-tiny-soldiers-177336) | Not confirmed this pass | Same family/style as above; worth checking specifically for Juggernaut (military-coded) if the Citizens pack doesn't have a suitable heavy/armored option. |
| **Stylized Character Pack** (Unity Technologies, official) | [Asset Store](https://assetstore.unity.com/packages/3d/characters/stylized-character-pack-360808) | Free | Cartoon/stylized, URP-only, rigged, 137.6MB. Free and from Unity directly so licensing is a non-issue, but style/material breakdown wasn't confirmed against the `"Body"`-renderer-naming and no-texture requirements this pass — would need hands-on inspection before committing. |

**Practical note:** the project's *current* Scout/Juggernaut placeholders (`Worker.fbx` / `Swat.fbx` under
`Assets/_Project/Art/Characters/Resources/`) are themselves CC0 — confirmed via `docs/ASSET_PACK_AUDIT.md`
that they trace back to Quaternius's **"Ultimate Modular Men"** pack (verified genuine CC0 license,
present twice in the external assets folder). That pack is safe and already fits `PawnImportTool.cs`'s
shape exactly (it's literally what the tool was built against) — if the clay-character search stalls, the
honest fallback is "keep Quaternius, it's free and already wired," accepting the style doesn't match
nappin's soft-clay direction as closely as the Toony Tiny candidates might.

**Interior 14-name gap (Cabinet/Chair/Table/Shelf/ShelfLarge/Bookshelf/LightCeiling/LightCeilingAlt/
LightDesk/WindowSmall/WindowLarge/Door/DoorAlt/DoorDouble):** no paid nappin-family pack was found this
pass — nappin's full catalog (checked directly on nappin.dev) is Office Essentials, House Interior, and
Weapons Pack, all free, nothing paid in the interior category. General office-prop packs exist at various
price points (e.g. **Office Pack v2** — [Asset Store](https://assetstore.unity.com/packages/3d/props/interior/office-pack-v2-130690),
"50+ Game-Ready Props") but none were confirmed to actually match nappin's soft-clay material language —
treat as an unverified fallback candidate, not a recommendation, until someone eyeballs it against the
reference screenshot. **Best current plan stays what `DRAFT_HANDOFF.md` already scoped:** Office Pack
Free + House Interior Free likely cover most of the 14 names; do a targeted search through both packs'
actual prefab lists before assuming a paid pack is needed at all.

---

## What this replaces / keeps

| Current | Action |
|---|---|
| Quaternius Ultimate House Interior (`Resources/Interior/`) | Replace with nappin Office (doors/desks/lights/plants) |
| Poly Haven wet floors via `BoardSurfaceMaterials` | Retire wet PBR; use flat/gradient clay mats (nappin gradients or `Mat_Clay*`) |
| Kenney smoke → `CloudAtlas` | Replace with Cinematic Weather fog/clouds or SIMPLE Sky / mesh clouds |
| Quaternius Scout/Juggernaut | Still open — see new "Characters" section above; nappin has **no** character pack, closest candidates found are Toony Tiny Citizens/Soldiers ($30, needs prep work) or keep Quaternius (free, already fits the pipeline, style mismatch) |
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

**2026-08-11 re-check:** nappin House Interior Free and Weapons Pack Free both confirmed still free (live
Asset Store fetch). Synty Heist/Office/City prices above confirmed live the same day — no sale active on
any of the three at time of check, so no urgency to "buy before a sale ends," but the licensing TODO
itself is time-sensitive in the sense that it's been open since 2026-08-11 and gets riskier the more
integration work goes on top of the unlicensed import.
