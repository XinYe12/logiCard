# Art pack research — clay / stylized shopping + motion notes

**Date:** 2026-08-10 (clay/nappin pivot); extended 2026-08-11 (licensing audit, Synty purge, nappin URP check, chubby-character + motion strategy discussion).  
**Companion:** `docs/ASSET_PACK_AUDIT.md` (external folder licensing).  
**Scope:** Research and decisions. Human owns Asset Store purchases/imports.

---

## Session status (2026-08-11) — what is actually in the project

| Item | Status |
|---|---|
| Unlicensed Synty `PolygonHeist` / `PolygonOffice` / `PolygonCity` | **Deleted** from `master` + `feat/heist-character-swap`. Do not re-import from reseller bundles. |
| Interior props wiring | Rewound to **Quaternius CC0** pipeline (`InteriorPackImportTool` + `Resources/Interior/*`) after PolygonOffice re-source was undone |
| **nappin Office Essentials Pack** | **In project** at `Assets/nappin/OfficeEssentialsPack/` (~54 prefabs, gradients, demo scene). Style lock for interiors. |
| **nappin URP upgrade** | Downloaded: `OfficeEssentialsPack_URP.unitypackage` (Downloads + dropped under `Assets/nappin/`). **Checked OK** — overwrites same material GUIDs with URP Lit (matches project shader `933532a4…`). Glass transparent. Skybox stays built-in skybox shader. **Human still needs to Import Package in Unity**, then may delete the `.unitypackage` from `Assets/nappin/`. |
| Characters | Still Quaternius Scout/Juggernaut placeholders. No chubby pack purchased yet. |

**nappin Office pack check notes:** Materials were Built-in Standard until URP package is imported. Pack is **not wired** into `BoardView` / `Resources/Interior/` yet. Catalog gaps vs current 14 interior slots: no real door (only `SeparatorDoor`), one window, no dedicated cabinet — desks/chairs/lights/shelves/tables map fine.

---

## Buy list — Synty (only if you want that look back)

Unlicensed reseller copies were removed. Legitimate Asset Store total ≈ **$99.98**:

| Pack | Link | Price |
|---|---|---|
| POLYGON - Heist Pack | [Asset Store](https://assetstore.unity.com/packages/3d/environments/urban/polygon-heist-pack-art-by-synty-97949) | $29.99 |
| POLYGON - Office Pack | [Asset Store](https://assetstore.unity.com/packages/3d/props/interior/polygon-office-pack-art-by-synty-159492) | $49.99 |
| POLYGON - City Pack | [Asset Store](https://assetstore.unity.com/packages/3d/environments/urban/polygon-city-pack-art-by-synty-95214) | $20.00 |

Do **not** re-import from `D:\XinyeData\projects\assets` reseller bundles — see `docs/ASSET_PACK_AUDIT.md`.

**Current art direction prefers nappin clay over Synty.** Hold Synty until free clay stack is in-scene and still feels thin.

---

## Verdict (current)

**Primary look = soft clay / curvy-minimal** (office screenshot vibe: matte rounded props, soft lighting, glass UI) — **not** wet PBR + Quaternius, and **not** Synty-first.

| Priority | What | Buy? |
|---|---|---|
| Locked | **nappin Office Pack – Free** | In project — import URP upgrade, then wire |
| Next free (same style family) | nappin **House Interior** + **Weapons** | Yes — add from Asset Store while still free |
| Weather (rain/clouds/fog) | Free URP weather VFX + free clouds/sky | Yes — free first |
| Lightning | Free Zap / FX Lightning | Yes — free |
| Buildings + streets | Free low-poly city / Kenney city kits | Yes — free first; accept slightly blockier roads |
| Characters | See Characters section below | **Modular Stylized Character 1 ($40)** — confirmed Humanoid/Mixamo-compatible rig, primary pick |
| Tornado | Paid stylized VFX (~$40) | Only if a real match beat needs it |
| Railway / 高铁 | Paid modular railway (~$10–$30) | Catalog; **deferred (C31)** — motion is mostly **code**, not pack anims |
| Synty POLYGON Heist/Office/City | $99.98 | Optional upgrade path only |

Most of the “premium” feel in the reference is **lighting + matte materials + frosted UI**, not expensive meshes.

---

## Style lock: nappin softpack family (confirmed)

Publisher: **nappin** — curvy, gradient-matte, soft edges. Matches the clay screenshot better than Synty faceted POLYGON.

| Pack | Link | Price | Role for logiCard |
|---|---|---|---|
| **Office Pack – Free** | [Asset Store](https://assetstore.unity.com/packages/3d/props/interior/office-pack-free-258600) · [nappin.dev](https://nappin.dev/details/officeEssentialsPack.html) | Free (limited time) | **Hall / facility dressing**. In `Assets/nappin/OfficeEssentialsPack/`. |
| **URP materials upgrade** | [nappin.dev](https://nappin.dev) (site download) | Free with pack | `OfficeEssentialsPack_URP.unitypackage` — import over existing materials (same GUIDs). |
| **House Interior – Free** | [Asset Store](https://assetstore.unity.com/packages/3d/props/interior/house-interior-free-258782) | Free | Extra sofas/beds/appliances |
| **Weapons Pack – Free** | [Asset Store](https://assetstore.unity.com/packages/3d/props/weapons/weapons-pack-free-259025) | Free | Stylized guns for Shoot readability |
| Racing tileset (micro) | [itch](https://nappin.itch.io/racing-microset) | Free | Tiny road tiles only — skip unless experimenting |

**Does not include:** outdoor city shells, streets at scale, railway, storm systems, characters.

---

## Category shopping list

### 1. Weather — storm, clouds, rain, lightning, tornado

| Need | Recommend | Price | Notes |
|---|---|---|---|
| Rain + snow + fog/clouds (URP) | **[FREE] Cinematic Weather VFX Bundle** | Free | [Asset Store](https://assetstore.unity.com/packages/vfx/particles/free-cinematic-weather-vfx-bundle-rain-snow-fog-urp-particle-sys-382428) |
| Cartoon sky | **SIMPLE Sky – Cartoon assets** (Synty) | Free | [Asset Store](https://assetstore.unity.com/packages/3d/environments/simple-sky-cartoon-assets-42373) — add from Store, not reseller bundle |
| Mesh clouds | Stylized Low Poly Clouds | ~$7.50 | Optional |
| Lightning | **Zap VFX – URP** | Free | [Asset Store](https://assetstore.unity.com/packages/vfx/particles/spells/zap-vfx-urp-303479) |
| Tornado | Stylized Tornado VFX | ~$39.99 | Only if designed in |
| Full weather director | COZY: Stylized Weather 3 | $50 | **Overkill** — skip |

**Practical stack:** Cinematic Weather + Zap + optional SIMPLE Sky. Keep `BoardWeatherPocket` rain if OK; swap clouds first.

### 2. Buildings / streets

| Need | Recommend | Price |
|---|---|---|
| Urban shells | Free Low Poly Simple Urban City **or** Mini World City Starter | Free |
| Roads | Bundled with above, or Kenney City Kit Roads (CC0) | Free |
| Dense city / vault (optional) | Synty City / Heist — buy legitimate Store copies only | $20 / $29.99 |

**Rule:** One exterior language. Keep nappin for interiors.

### 3. Railway (高铁) — meshes vs motion

Confirmed design **C31**, deferred from ship. Mesh packs (~$10–$30) give tracks/cars; they usually do **not** include “ride the whole route” animation.

| Pack | Price | Notes |
|---|---|---|
| Modular Railway stylized lowpoly | ~$9.99 | [Asset Store](https://assetstore.unity.com/packages/3d/environments/modular-railway-stylized-lowpoly-280486) |
| Lowpoly Railway Pack | ~$25 | Fuller set |
| Stylized Railway Modular Constructor | ~$30 | Heavier kit |

**Motion model (same for buses):** move transforms in **code** along a path (matches Host/ReplayTape + **no root motion**, C23/C55). Optional tiny loops (wheels, sway, doors) only if they read. Do not block art on buying a pack that “includes ride animation.”

### 4. Characters — chubby / round / Link’s Awakening taste

**nappin has no character pack.** Target: soft, chubby, round toy figures (not Synty facets, not adult Quaternius realism) that can also carry future roster fantasy (tennis player, other unique operators) without a second character-pipeline rebuild.

**Primary recommendation, 2026-08-11: Modular Stylized Character 1 (Rukha93).**

| Pack | Link | Price | Fit |
|---|---|---|---|
| **Modular Stylized Character 1** | [Asset Store](https://assetstore.unity.com/packages/3d/characters/humanoids/humans/modular-stylized-character-1-255279) | $40 | **Confirmed Humanoid/Mecanim rig, explicitly Mixamo-compatible** — the key property Kotangent below only implies. Male + female base bodies, swappable modular outfits, custom 3-channel color shader (Shader Graph), facial blendshapes + 10 phoneme blendshapes for lip sync (unused by this project, harmless). Toon/casual/anime-influenced stylization — proportions are far less extreme than a "chubby blob" pack, so a retargeted Humanoid clip (Mixamo or a paid sports pack) is much less likely to foot-slide or clip than on an exaggerated chibi rig. **Ships with zero animations** — budget for Mixamo/retarget regardless, same as every option here. Sibling pack **Modular Stylized Character 2 – Fantasy** ([Asset Store](https://assetstore.unity.com/packages/3d/characters/humanoids/humans/modular-stylized-character-2-fantasy-294319), ~$24.80 on sale) exists if a fantasy-coded operator ever joins the roster. |
| Kotangent Chubby Characters | [Asset Store](https://assetstore.unity.com/packages/3d/characters/kotangent-chubby-characters-pack-271155) | ~$12.99 | Closest "round blob people" look, but rig type/animation contents aren't confirmed from the listing itself (tags only say "Rigged"/"Animated") — and if the proportions are as extreme as "chubby" implies, that's the exact shape that breaks Humanoid retargeting worst. Cheaper fallback, not the primary pick anymore. |
| Toony Tiny Citizens Megapack | [Asset Store](https://assetstore.unity.com/packages/3d/characters/toony-tiny-citizens-megapack-99854) | ~$30 | Chunky toony humans; good Scout/civilian alternate |
| Toony Tiny Soldiers | [Asset Store](https://assetstore.unity.com/packages/3d/characters/toony-tiny-soldiers-177336) | Check Store | Same family — Juggernaut / tactical alternate |
| PolyPeople City People [Free] | [Asset Store](https://assetstore.unity.com/packages/3d/characters/polypeople-series-city-people-free-325204) | Free | Free smoke-test of URP + rig fit before spending on anything above |
| Quaternius Ultimate Modular Men | (already in project) | Free CC0 | Honest fallback — wired today, style mismatch with nappin |

**Skip for this taste:** Synty POLYGON characters (too faceted); KayKit (cuter blocky, less clay-round).

**Pipeline note — real integration cost, not a drop-in:** `PawnImportTool.cs`/`PawnView.cs` currently do team-color tinting via a plain `MaterialPropertyBlock._BaseColor` swap on any renderer named `"Body"`. Modular Stylized Character 1's color customization runs through its own **Shader Graph-based multi-channel shader**, not a simple base color — wiring it in means either adapting `PawnView`'s tint call to drive that shader's actual color-channel property, or picking one channel to serve as the team-tint slot. Same shape of adaptation work as every other character-pipeline change this project has needed, just a different specific hookup than the Quaternius/Heist cases.

---

## Motions & animation — discussion notes (2026-08-11)

### Do character packs include motions?

| Need | Usually included? |
|---|---|
| Idle / walk / run | Sometimes |
| Gun reload / weapon switch | Rarely (soldier packs sometimes) |
| Full shoot / hit / down set | Rarely complete |
| Tennis serve / sport-specific | Almost never unless a sport pack |
| Every GDD fantasy verb | **No** |

Always read the listing for “animations included” / clip count. Synty-style characters often ship **mesh + Humanoid, no clips**.

### Can you use motions from another pack?

**Not always — but often yes** if both sides are **Mecanim Humanoid**:
- Character = Humanoid avatar  
- Clip pack = Humanoid  
→ retarget (shared Animator Controller, Mixamo, etc.)

Fails or looks bad when: Generic/custom skeleton, extreme chibi proportions (foot slide), Legacy-only clips.

### Can you design your own?

Yes: Blender/Maya → FBX Humanoid; Mixamo; Unity Animation window; paid anim packs. Human has no motion-design background — **do not plan to hand-key everything**.

### Project constraints (already locked)

- **C55:** smooth per-frame interpolation in Playback (not stepped stop-motion).  
- **C23 / ART_DIRECTION:** **no root motion** — Host/ReplayTape moves transforms; clips play in place.  
- Vehicles/高铁: same idea — **code moves the car**; decorative loops optional.

### GDD fantasy vs ship order

Long wishlist (soldier reload/switch, tennis player kit, railways, buses) is real product fantasy, but **mature solo sequencing** is:

1. **Now:** Scout + Juggernaut readable; idle + walk (+ shoot pose later); clay board + nappin props  
2. **Later:** reload/switch only if it reads in Playback  
3. **高铁 / buses:** code path motion when C31 (and any bus gadget) is scheduled  
4. **Tennis / unique operators:** only when that character is in active build — then buy/retarget or outsource a few signature clips  

Do not buy or author a full anim set for every future roster fantasy before the clay board + two pawns look right.

### How mature solo devs handle “many motions”

They rarely animate everything themselves. Typical pattern:

1. **Design for reuse** — one Humanoid body plan; shared walk/idle; unique clips only for signature moves  
2. **Buy / retarget** — character pack + Mixamo or anim pack  
3. **Fake polish early** — in-place cycles + code travel; procedural bob/lean for toy feel; VFX sells hits  
4. **Outsource selectively** — pay for 5–15 hero clips, not 200  
5. **Sequence** — fun loop → silhouettes → locomotion → combat beats → vehicle gadgets → roster flourishes  

| Approach | Solo norm |
|---|---|
| Hand-key every motion | Rare |
| Mixamo + Asset Store + retarget | Very common |
| Code-driven travel + few loops | Extremely common (tactics / board / vehicles) |
| Full custom mocap | Almost never alone |

### How the Cursor agent can help with motions

| Phase | Agent can |
|---|---|
| Planning | Must-have clip list vs defer; Humanoid vs Generic; match to `PawnView` / tape playback |
| Import | Avatar check, import settings, Animator Controllers, masks |
| Adaptation | Mixamo/Blender export steps; retarget guidance; fix foot slide / looping / root-motion-off |
| Wiring | Hook states to tape events (arrive, shoot, wounded); keep tint/height normalize on skinned meshes |
| Polish | Procedural toy motion in code if clips aren’t ready; batchmode/PlayMode guards |

**Agent cannot:** replace a full mocap studio or live-sculpt every Blender keyframe for the human. Split: human (or Mixamo/freelancer) authors clips; agent owns Unity import, retarget, Animator, and logiCard playback wiring.

**Best handoff:** drop character (or hierarchy/Avatar screenshot) in project → list 3–5 moves for Scout/Juggernaut → Agent mode for wiring.

---

## What this replaces / keeps

| Current | Action |
|---|---|
| Quaternius interior (`Resources/Interior/`) | Replace with nappin Office (after URP import + wire) |
| Poly Haven wet floors | Retire; matte/gradient clay (`Mat_Clay*` / nappin gradients) |
| Kenney smoke clouds | Replace with free weather/sky packs |
| Quaternius Scout/Juggernaut | Replace when a chubby pack is chosen; keep as fallback |
| Path 线稿涂鸦, URP volumes, rain if OK | Keep; softer brighter lighting |
| Door interaction code | Keep; swap mesh only |
| 高铁 / bus travel | Code pathing when scheduled — not pack ride anims |

---

## Integration sketch

1. Import nappin URP `.unitypackage` in Editor; delete leftover `.unitypackage` from `Assets/nappin/` if desired.  
2. Retarget `InteriorPackImportTool` / `Resources/Interior/` names to nappin prefabs (or wrap under existing names).  
3. Floors → matte clay materials; stop Poly Haven wet path.  
4. Weather → free rain/fog/lightning in `BoardWeatherPocket`.  
5. Characters → buy/test chubby Humanoid pack; adapt tint/import pipeline.  
6. Motions → Humanoid clips in place; tape drives transform; vehicles later via code.  
7. Railway → mesh purchase only when C31 is on; no anim-pack dependency.  
8. UI glass → UI work, not an asset pack.

---

## Buy / do order (current)

1. **Done:** nappin Office Essentials in project  
2. **Do now:** Import `OfficeEssentialsPack_URP.unitypackage` in Unity  
3. **Add free:** House Interior, Weapons, weather (Cinematic + Zap), optional SIMPLE Sky  
4. **Add free exterior:** one city/roads language  
5. **Characters:** buy **Modular Stylized Character 1 ($40)** — confirmed Humanoid/Mixamo-compatible, lower retarget risk than the chubby-proportion alternatives; Kotangent Chubby / Toony Tiny stay fallback options  
6. **Paid later:** Modular Railway when 高铁 returns; Synty only if clay stack still feels thin; tornado only if designed  

**Hold:** COZY ($50), ithappy Cartoon City ($489), unlicensed reseller anything, full custom anim library before two pawns look right.

---

## Earlier Synty recommendation (superseded as primary)

First pass recommended POLYGON Heist + Office + City (~$100). Still a valid **upgrade path** for denser modular heist architecture. Near-term target remains **nappin clay + free weather/city + chubby Humanoid characters**. Unlicensed copies were purged 2026-08-11 — buy from Asset Store only if revisiting Synty.

---

## Price / license caveat

Prices sampled 2026-08-10 / 2026-08-11. nappin free packs are limited-time — grab House + Weapons while free. Confirm URP packages from nappin.dev (Office URP package already downloaded and verified). Never use Taobao/reseller `.unitypackage` trees under `D:\XinyeData\projects\assets` for shippable content.
