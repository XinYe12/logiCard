# Art pack research — buy list for Link's Awakening / toy-diorama look

**Date:** 2026-08-10  
**Branch:** `feat/art-pack-research`  
**Scope:** Research only. No imports, no purchases, no code changes.

## Verdict (read this, then buy)

Buy **three Synty POLYGON packs** and grab **one free Synty sky pack**. Skip another color-grade pass on Poly Haven + Quaternius + Kenney smoke — that stack cannot deliver the vibrant toy look the human asked for.

| Priority | Pack | Approx. price (USD, list / common sale) | Buy? |
|---|---|---|---|
| 1 | **POLYGON — Heist Pack** | $29.99 / ~$15 | **Yes — first** |
| 2 | **POLYGON — Office Pack** | $49.99 | **Yes** |
| 3 | **POLYGON — City Pack** | $20 / ~$10 | **Yes** |
| 4 | **SIMPLE Sky — Cartoon assets** (Synty) | **Free** | **Yes** |
| — | ithappy Cartoon City | $489 / ~$147 sale | No |
| — | COZY: Stylized Weather 3 | $50 | No (overkill) |
| — | Kenney / KayKit free kits | $0 | No as primary look |

**Estimated cart:** ~$100 list, often ~$75 when City/Heist are on sale. Commercial Steam ship is fine under Unity Asset Store Single Entity (see License below).

**Why this stack:** Heist is genre-native (SWAT + vault + bank interiors). Office dresses Hall. City dresses Yard. All three share Synty's POLYGON art bible, so characters and rooms match. SIMPLE Sky kills the Kenney photo-smoke clouds for $0.

---

## Shopping list (mapped to what it replaces)

| Buy | Link | Price | License | Replaces |
|---|---|---|---|---|
| **POLYGON Heist Pack** | [Unity](https://assetstore.unity.com/packages/3d/environments/urban/polygon-heist-pack-art-by-synty-97949) · [Synty Store](https://syntystore.com/products/polygon-heist-pack) | $29.99 (often ~$15) | Unity Single Entity / Synty OTP — commercial OK, royalty-free | `Assets/_Project/Art/Characters/` Quaternius Scout/Juggernaut (`Worker`/`Swat` via `PawnImportTool`); Vault room props + vault door currently from Quaternius Ultimate House Interior; bank/vault modular walls/floors that currently fake via `BoardSurfaceMaterials` VaultFloor |
| **POLYGON Office Pack** | [Unity](https://assetstore.unity.com/packages/3d/props/interior/polygon-office-pack-art-by-synty-159492) · [Synty Store](https://syntystore.com/products/polygon-office-pack) | $49.99 | same | Quaternius interior prefabs under `Resources/Interior/` (doors, desks, chairs, shelves, lights); Hall/Flank floor look from `BoardSurfaceMaterials` + Poly Haven concrete/wood; most of `InteriorPackImportTool`'s curated subset |
| **POLYGON City Pack** | [Unity](https://assetstore.unity.com/packages/3d/environments/urban/polygon-city-pack-art-by-synty-95214) | $20 (often ~$10) | same | Yard floor asphalt/brick from Poly Haven via `BoardSurfaceMaterials`; outdoor strata dressing; optional extra civilian/police characters if Heist's roster feels thin |
| **SIMPLE Sky — Cartoon assets** | [Unity](https://assetstore.unity.com/packages/3d/environments/simple-sky-cartoon-assets-42373) · [Synty Store](https://syntystore.com/products/simple-sky-cartoon-assets) | **Free** | same family | `BoardWeatherPocket` cloud bank built from Kenney Smoke Particles (`CloudAtlas.png` / `whitePuff*`) |

### Character mapping (Heist)

| Archetype | Prefer from Heist | Notes |
|---|---|---|
| **Juggernaut** | SWAT Officer | Direct silhouette match; drop Quaternius `Swat.fbx` |
| **Scout** | Male/Female in Work Shirt or Overalls | Lean utility read; drop Quaternius `Worker.fbx` |
| Team color | Heist ships ×4 alt texture colors + skin tones | Prefer pack palette variants over fighting `_BaseColor` tint on mismatched mats |

Heist also includes FBI suits if you want a third archetype later — not needed for the demo.

---

## 1. Synty POLYGON — confirmed fit

### Style match

POLYGON is the industry-default "vibrant low-poly toy world" Unity line: saturated flat-ish materials, chunky readable silhouettes, modular rooms, URP-ready prefabs. That is closer to Link's Awakening (2019) toy-diorama energy than Photoreal Poly Haven wet asphalt + faceted Quaternius adults. Prior internal notes that ruled Synty out were under a **CC0-only** constraint; that constraint is gone — the human is willing to pay to finish art quickly.

### Pack → board room

| Board zone | Pack | What you pull |
|---|---|---|
| **Vault** | Heist | Modular bank vault, vault door, deposit boxes, security cams, money props |
| **Hall** | Office | Modular office walls/floors/ceilings, desks, chairs, kitchen props, lights, doors |
| **Yard** | City | Roads/asphalt, exterior building modules, vents/pipes/trash, park bits for strata edge |
| **Characters** | Heist (primary), City (backup civilians/cops) | Mecanim humanoid, Mixamo-friendly; no anims in pack (fine — `PawnView` still doesn't need Animator for demo) |
| **Sky / clouds** | SIMPLE Sky (free) | Sun/moon/stars/clouds; UV-offset time-of-day |

### Interoperability

Yes — POLYGON packs are designed to mix. Shared scale, material language, and alternate color variants. Heist is explicitly marketed as a companion to Gang Warfare; Office/City/Heist all sit in the same POLYGON urban family. Do **not** mix POLYGON with Kenney block kits or Quaternius matte CC0 in the same shot — that is the current disappointment.

### Unity-native?

Yes. Unity `.unitypackage` with prefabs, collisions, demo scenes, URP + Built-in support (current listings: Unity 2022.3+ packages; project is 6000.5.5f1 — expect a one-time URP material upgrade pass, not a format conversion). FBX sources also available from Synty Store if needed.

### License / commercial Steam

| Channel | What you get |
|---|---|
| **Unity Asset Store** | "Restricted Single Entity" = Unity Standard Asset Store EULA for content (not Extension). One legal entity may develop with the assets. **Royalty-free commercial ship** (Steam OK). Do not redistribute the raw pack as an asset. |
| **Synty Store direct** | One-Time Purchase EULA: perpetual, royalty-free, typically **5 seats** per licence; same commercial product rights. |

**Practical notes:**

- Solo / single-entity indie: Unity Single Entity is enough.
- You may ship the game. You may **not** put Synty's storefront screenshots/marketing media on your Steam page — shoot your own.
- No revenue share / royalty to Synty after purchase.
- Prefer buying on the **Unity Asset Store** for this project so packages land in the Unity account already used for the Editor.

There is **no dedicated POLYGON weather pack**. SIMPLE Sky (free) covers cartoon clouds/sky. COZY ($50) is a full weather *system* — wrong weight for a small board pocket that already has acceptable rain particles.

---

## 2. Alternatives (evaluated, not recommended as primary)

### A. ithappy — Cartoon City (~$489 / ~$147 on sale)

Huge stylized city + 300 characters + traffic systems. Wrong shape for a **tiny continuous tactics board** with interiors. Buildings are exterior shells (no real interiors). Price is 5–10× the Synty cart for less Heist/Vault relevance. **Reject.**

### B. KayKit — City Builder Bits (itch free / Unity ~€5.50)

Cute, CC0-adjacent, Unity prefab version exists. Outdoor city tiles only — no SWAT, no vault, no office interior. Same author family already rejected for Dungeon Remastered (genre) and for the toy-block feel vs. the previous C53 pass. Fine as a free experiment; **not** the cohesion buy.

### C. Kenney — City Kit + Furniture Kit (CC0, free)

Already evaluated in `Art/Environment/THIRD_PARTY.md` and character notes: deliberately blocky; fights a polished branded look; Blocky Characters already rejected for Link's Awakening. Smoke Particles are what made the weather read "photo puff on a toy board." **Do not expand Kenney as the art direction** — only keep rain if it still reads after the sky swap.

### D. Quaternius / Poly Haven (current)

Keep provenance docs for history; **do not keep as the visual base**. Quaternius Ultimate Modular Men + House Interior + Poly Haven PBR is exactly the "tedious / disappointing" stack after C53/C58. Re-tinting it will not finish the art part quickly.

### E. Mixamo humans

Previous character research floated Mixamo for realism. That is the **opposite** of the new Link's Awakening / vibrant toy ask. Skip.

---

## 3. What is fine to keep (don't replace)

| Keep | Why |
|---|---|
| URP pipeline assets (`Art/URP/`) | Foundation; retune Volume colors after pack swap |
| Path / 线稿涂鸦 materials (`Mat_PathYarn` etc.) | Game-specific readability, not stock CC0 scrap |
| Rain particle path in `BoardWeatherPocket` | Already called out as OK; only clouds are the weak weather piece |
| Door **interaction** code / radius API | Logic stays; only the mesh/prefab swaps |
| Match rules, continuous board math | Unrelated to art packs |

---

## 4. Integration sketch (effort only — no implementation)

### Characters (Heist → `PawnImportTool`)

Point `PawnImportTool` Scout/Juggernaut source FBX paths at Heist character FBXs (or drop Heist prefabs into `Resources/Scout` / `Resources/Juggernaut` and skip FBX rebake if scale matches). Keep `PawnView.TargetVisualHeight` normalization and team-tint hook — rebind tint to whichever mesh name contains the pack's body part (verify part names; may not be `"Body"`). Expect 0.5–1 day including scale/tint verify in Play Mode.

### Floors / walls (`BoardSurfaceMaterials` + Poly Haven)

Stop calling `BuildWetSurface` with Poly Haven asphalt/concrete for primary floors. Either (a) assign materials sampled from Office/Heist/City floor prefabs into the static getters (`YardFloor`/`HallFloor`/`VaultFloor`/`BrickWall`), or (b) replace extruded floor quads with modular POLYGON floor/wall prefabs placed by `BoardView` room tags. (a) is faster and preserves continuous collision meshes; (b) looks more "toy set" but needs placement rules. Prefer **(a) first** for Day-14 speed. Delete or stop loading `Resources/BoardSurfaces/` once materials are wired.

### Interior props (`Resources/Interior/` + Quaternius)

Retarget `BoardView`'s `Resources.Load("Interior/…")` names to new prefabs (Door → Heist/Office door prefab wrappers with the same pivot/height contract `InteriorPackImportTool` currently enforces). Either wrap Synty prefabs under the existing resource names or update the string table once. Retire Quaternius `Interior/Source` and the import tool after the swap.

### Weather (`BoardWeatherPocket`)

Replace Kenney atlas particle puffs with SIMPLE Sky cloud meshes (or billboard materials from that pack) parented in the existing weather pocket bounds. Leave `PlaceRain` alone initially. Skip COZY unless you later want a full day/night director.

### Lighting / post

After packs land, retune `LogiCardVolumeProfile` away from wet-dusk realism toward brighter, more saturated toy lighting (Link's Awakening reference). This is grading on **good** base assets, not another attempt to rescue bad ones.

---

## 5. Buy order for the human

1. **Heist** — biggest single visual + genre win (pawns + vault).
2. **Office** — Hall stops looking like a CC0 apartment kit.
3. **City** — Yard stops looking like tinted asphalt photos.
4. **SIMPLE Sky** (free) — clouds stop looking like smoke sprites.

Optional later (not for 14-day ship): Gang Warfare ($49.99) for more crime-street dressing; SyntyPass (~$30/mo) only if you expect to pull many more POLYGON packs.

**Do not buy:** Cartoon City ($489), COZY ($50) for this board, more Kenney/Quaternius "maybe this tint will work."

---

## Price / source caveat

Prices sampled 2026-08-10 from Unity Asset Store and Synty Store listings (sales rotate). Confirm cart totals at checkout. Prefer Unity Asset Store purchases for this Unity project.
