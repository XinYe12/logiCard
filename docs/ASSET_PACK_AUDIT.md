# Asset pack audit — `D:\XinyeData\projects\assets`

**Date:** 2026-08-11
**Branch:** `feat/asset-pack-audit`
**Scope:** Read-only inspection of the external folder the human has previously pointed the project at
(source of the already-imported `Assets/PolygonHeist/`, `Assets/PolygonOffice/`, `Assets/PolygonCity/`).
Nothing in this pass was imported into `Assets/`. Companion doc: `docs/ART_PACK_RESEARCH.md` (Asset
Store purchase research, part 2 of this task).

## Headline finding

**Confirmed: the reseller bundle already flagged for the three imported Synty packs is sitting in this
folder as its own top-level item** (`80套Unity模型lowpoly卡通风格人物动物自然场景食物资产U3D素材`),
and it contains those exact three packs — `POLYGON - Office Pack v1.03.unitypackage`,
`POLYGON - Heist Pack v1.4.unitypackage`, `POLYGON - City Pack v1.3.unitypackage` — each wrapped with a
Taobao shop shortcut (`Unity士多淘宝店.url` → `shop240527861.taobao.com`) and a `声明.txt` (“declaration”)
that reads, translated: *“Download link collected from the internet, for study/research/appreciation use
only, not for commercial use. If a copyright dispute arises it has nothing to do with this shop — please
be aware. Copyright belongs to the original author and their company; if you like it, please buy the
genuine version.”* This is about as unambiguous as a piracy disclaimer gets, and it directly confirms
`DRAFT_HANDOFF.md`'s existing flag on the three already-imported packs — not new information about them,
but hard confirmation of what was previously inferred from packaging patterns alone. **No folder in this
audit contains anything resembling a real Asset Store receipt or license file for Heist/Office/City** —
see the per-folder notes below. The ship-blocking TODO to buy real licenses for those three stands
unchanged.

---

## `0095城市` — POLY - Megapolis City Pack

- **What it is:** A single `.unitypackage` (`POLY - Megapolis City Pack v1.1.unitypackage`, ~135MB
  compressed, 1785 embedded assets) plus a preview `.png`. Internal asset root is
  `Assets/Polygon-City Megapolis/...` — airport, buildings, city props, ~800+ models. This is **not**
  the same product as Synty's `PolygonCity` already imported (different folder structure, different
  publisher naming: "Polygon-City Megapolis" vs Synty's "PolygonCity"). Identified via web search as
  **POLY - Megapolis City Pack by ANIMPIC STUDIO** (Unity Asset Store listing
  `poly-megapolis-city-pack-208916`, ~$49.99).
- **License signal:** No license/readme/receipt file of any kind inside the package (checked every
  `pathname` entry for `licen`/`readme`/`eula` — zero hits). Bare `.unitypackage` + preview image sitting
  in a numerically-coded folder (`0095`), which matches the same catalog-numbering convention used by the
  confirmed-pirated 80-pack bundle below. No confirmation either way from the filesystem; **treat as
  unconfirmed / prototype-only** until a real Asset Store purchase is confirmed.
- **Relevance:** Genuinely on-topic — a dense city pack (airport, skyscrapers, infrastructure) usable for
  Yard/street backdrop dressing, same category as the already-imported `PolygonCity`.
- **Verdict: prototype-only, flag for license purchase if kept.** Not the same SKU as the already-imported
  city pack, so it wouldn't resolve that TODO even if purchased — it would be a new purchase decision.

## `80套Unity模型lowpoly卡通风格人物动物自然场景食物资产U3D素材` — the 80-pack reseller bundle

- **What it is:** A Chinese reseller collection organized into four sub-groups: `POLYGON（48套）`
  (48 numbered `.rar` archives, 4001–4048), `SIMPLE(39套)` (39 numbered `.rar` archives, 4053–4091),
  `MINI（4套）` (4 archives, 4049–4052), and `4400_lowpoly超级合集` (a larger numbered "super collection").
  Every archive checked follows an identical structure: `<ProductName>.unitypackage` +
  `Unity士多淘宝店.url` (Taobao shop shortcut, same shop ID `240527861` every time) + `声明.txt`
  (the piracy disclaimer quoted above) + two preview `.png`s. Confirmed by direct extraction:
  - `4035_低面现代办公室资产.rar` → `POLYGON - Office Pack v1.03.unitypackage`
  - `4037_低面银行劫匪资产.rar` → `POLYGON - Heist Pack v1.4.unitypackage`
  - `4022_低多边形城市资产.rar` → `POLYGON - City Pack v1.3.unitypackage`

  These are the exact three packs already imported into `Assets/PolygonHeist|Office|City`. **This folder
  is confirmed to be (or be functionally identical to) the actual source of that import.**

  The `SIMPLE(39套)` group also contains real Synty "SIMPLE" line products in the same wrapper —
  `4059_简单办公室资产包.rar` → `Simple Office Interiors - Cartoon Assets v1.1.unitypackage`,
  `4060_简单城市建筑街道.rar` → `Simple Town - Cartoon Assets 12.1.unitypackage`,
  `4065_简单卡通城市资产.rar` → `Simple City - Cartoon Assets v1.1.unitypackage` — i.e. even the packs
  in this bundle that correspond to *free* Asset Store SKUs (like `SIMPLE Sky` in `ART_PACK_RESEARCH.md`)
  are sitting here as pirated copies rather than store-added ones; if any SIMPLE-line asset is wanted,
  add it from the Asset Store directly (it's free there) rather than using the copy in this folder.
- **License signal: confirmed unsafe.** Every archive carries the explicit "not for commercial use, buy
  the genuine version" disclaimer plus a live Taobao storefront link. Unambiguous reseller/pirated bundle.
- **Relevance:** The `POLYGON（48套）` and `SIMPLE(39套)` groups are largely off-topic for logiCard —
  fantasy kingdoms, zombies, Vikings, medieval knights, sci-fi spaceships, etc. — with the three
  already-imported packs being the actual relevant hits, plus the SIMPLE office/city/town trio as
  possible free-tier alternatives (available cleanly from the Store instead). `MINI（4套）` and
  `4400_lowpoly超级合集` are miscellaneous fantasy/mini character-and-scene packs, not inspected
  file-by-file (same wrapper pattern already confirmed) — nothing in their filenames suggests
  office/heist/city relevance.
- **Verdict: unsafe, prototype-only at best — same flag as the three already-imported packs, now with
  direct textual confirmation.** Do not treat this folder as a source for anything new without buying the
  real SKU first.

## `B840 Anime City Pack 1.2`

- **What it is:** A single bare `.unitypackage` (`Anime City Pack 1.2.unitypackage`) with internal asset
  root `Assets/KawaiiCity/...` — modular anime-styled city kit (buildings, street, tram, vehicles, signs,
  neon materials). Identified via web search: **"Anime City Pack" by PolySquid**, Unity Asset Store
  listing `anime-city-pack-199255`, described as "fully modular and super kawaii, anime city environment
  set."
  the exact current price wasn't confirmed via a direct Asset Store fetch this pass — check the listing.
- **License signal:** No license/readme/receipt file embedded in the package at all (checked all
  `pathname` entries, zero `licen`/`readme`/`txt`/`url` hits). The catalog-style folder name (`B840` — a
  numeric/letter product code, not the product's actual name) matches the same naming convention used by
  known-pirated items elsewhere in this same parent folder (`0095城市`, `G108、卡通城镇`), which is a
  meaningful signal even without a smoking-gun disclaimer file. **Unconfirmed — treat as unsafe until an
  actual purchase is confirmed.**
- **Relevance:** Off-topic-ish — anime/neon Japanese-city aesthetic doesn't match logiCard's office/heist
  reference direction, though it could work as distant Yard skyline dressing if the style read small
  enough. Low priority either way.
- **Verdict: prototype-only-flag-for-license-purchase, low relevance — likely skip.**

## `G108、卡通城镇` — forum-scraped raw 3D models (not even Unity-packaged)

- **What it is:** Four ZIPs (`1.zip`–`4.zip`), each containing a raw `.max` (3ds Max scene) +
  `.fbx` + texture, wrapped in "forum attachment" folder names (`attachment_<timestamp>/`) rather than
  any Unity package structure. `1.zip`'s `MAXFILES.TXT` lists the original file's local path on someone
  else's machine: `E:\模型\4月15日\科幻城市卡通\...\科幻城市卡通.max` ("Models\April 15\Sci-fi cartoon
  city\..."). Each zip also carries its own Taobao shortcut
  (`淘宝店：多维空间设计.url` → a *different* Taobao shop, "Duowei Space Design") — a second, distinct
  reseller signal from the one in the 80-pack bundle.
- **License signal: confirmed unsafe.** This is raw scraped/resold 3D-modeler content, not even
  Store-packaged — no plausible path to a legitimate license here at all. Worse provenance than the
  `.unitypackage`-wrapped items elsewhere in this folder.
- **Relevance:** Sci-fi/cartoon city buildings — same general category as the other city packs, but
  given the license signal this isn't worth evaluating further for actual use, and it isn't even in a
  Unity-importable format without manual FBX cleanup.
- **Verdict: unsafe, skip.** Don't invest integration effort regardless of licensing — wrong format,
  wrong provenance.

## `Kenney_Extracted` — genuine, verified

- **What it is:** Two real sub-packs: `3D assets/Blocky Characters` (Kenney + Casper Jorissen, full
  `Faces/Models/Skins/Unity` structure matching Kenney's standard package layout, includes
  `blockyCharacters.unitypackage`) and `Ultimate Modular Men/Ultimate Modular Men- Feb 2022` (despite
  living inside a folder named "Kenney_Extracted," this one is **not actually a Kenney product** — its
  own `License.txt` identifies it as **"Ultimate Modular Males by @Quaternius,"** same publisher as the
  game's current placeholder Scout/Juggernaut art).
- **License signal: confirmed safe.** `Blocky Characters/License.txt` is a genuine Kenney CC0 license
  (verbatim: *"License: (Creative Commons Zero, CC0) ... free to use in personal, educational and
  commercial projects"*). `Ultimate Modular Men/.../License.txt` is genuine Quaternius CC0
  ("CC0 1.0 Universal ... Public Domain Dedication"). Both are real, unmodified license files matching
  each publisher's known real distribution format — not just labeled, actually verified by content.
- **Relevance:** `Blocky Characters` — low relevance (blocky voxel-ish humanoid, doesn't match either the
  old Quaternius-realistic or the nappin-clay direction; a fallback option at best). `Ultimate Modular Men`
  — **directly relevant and already effectively in use**: its file-naming (`Worker_Body.fbx`,
  `Swat_Body.fbx`, etc.) matches `PawnImportTool.cs`'s existing source paths
  (`Assets/_Project/Art/Characters/Resources/Scout/Worker.fbx`,
  `.../Juggernaut/Swat.fbx`) almost certainly one-for-one — this folder is very likely where the current
  Scout/Juggernaut source FBXs originally came from.
- **Verdict: safe to use now (CC0, no action needed).** Both sub-packs.

## `Tem_0230_Kenney 游戏素材大全Kenney Game Assets All-in-1` — genuine, verified

- **What it is:** Kenney's actual "Game Assets All-in-1 v2.8" bundle, a single 445MB zip containing
  79,441 files across 2,041 folders — 2D, 3D, audio, fonts, UI, organized exactly as Kenney's own
  distribution structure (per-sub-pack folders, each with its own preview and license file). **227
  separate `License.txt` files** were found inside, one per bundled sub-pack — consistent with an
  unmodified official Kenney release, not a relabeled substitute.
- **License signal: confirmed safe.** Same CC0 terms as all Kenney content; the sheer count and
  structure of embedded per-pack license files is strong evidence this is the real, complete Kenney
  bundle and not a stripped/relabeled fake.
- **Relevance:** Broad general-purpose low-poly/2D kit — likely has usable pieces for UI icons, simple
  city/road kit assets (already recommended as free options in `ART_PACK_RESEARCH.md`'s Buildings/Streets
  sections — e.g. Kenney City Kit), fonts. Not a targeted office/heist match but a legitimately safe
  fallback library.
- **Verdict: safe to use now (CC0, no action needed).**

## `Ultimate Modular Men- Feb 2022-*.zip` (top-level file)

- **What it is:** The same "Ultimate Modular Men- Feb 2022" package as the one nested inside
  `Kenney_Extracted` (identical `License.txt`, identical file naming down to `Worker_Body.fbx` /
  `Swat_Body.fbx`) — this copy's filename carries a Google-Drive-export-style timestamp suffix
  (`20260808T071700Z-1-001`), suggesting it was re-downloaded from a shared Drive folder rather than
  Asset Store, but the content itself is the same Quaternius CC0 pack either way.
- **License signal: confirmed safe** — `License.txt` inside is genuine Quaternius CC0, same text as
  verified above.
- **Relevance:** Same as above — this is (almost certainly) the actual current source of the placeholder
  Scout/Juggernaut FBXs already wired through `PawnImportTool.cs`.
- **Verdict: safe to use now (CC0, no action needed).** Redundant with the copy in `Kenney_Extracted` —
  no need to keep both once confirmed identical, but neither is a licensing risk.

---

## Summary table

| Folder | What it is | License signal | Relevance | Verdict |
|---|---|---|---|---|
| `0095城市` | POLY - Megapolis City Pack (ANIMPIC STUDIO, ~$49.99 real price) | No license file, catalog-numbered folder — unconfirmed | On-topic (city dressing) | Prototype-only, flag for purchase |
| `80套...U3D素材` (80-pack bundle) | Reseller bundle — **contains the exact 3 already-imported Synty packs** + Synty SIMPLE line + ~120 unrelated fantasy/genre packs | **Confirmed unsafe** — explicit "not for commercial use" disclaimer + Taobao link in every archive | Mixed: 3 items directly relevant (already in hand), rest mostly off-topic | Unsafe, prototype-only — same flag as existing Synty import, now with hard confirmation |
| `B840 Anime City Pack 1.2` | Anime City Pack (PolySquid) | No license file, catalog-style folder name — unconfirmed | Low (style mismatch) | Prototype-only, likely skip |
| `G108、卡通城镇` | Raw scraped .max/.fbx forum files, not Unity-packaged | **Confirmed unsafe** — different Taobao shop link, personal-machine file paths in metadata | Low (wrong format, marginal relevance) | Unsafe, skip |
| `Kenney_Extracted` | Kenney "Blocky Characters" + Quaternius "Ultimate Modular Men" | **Confirmed safe** — verified CC0 license text for both | Ultimate Modular Men directly relevant (current Scout/Juggernaut source) | Safe to use now |
| `Tem_0230_Kenney...All-in-1` | Kenney "Game Assets All-in-1 v2.8" (real, 227 embedded license files) | **Confirmed safe** — CC0 | General fallback library | Safe to use now |
| `Ultimate Modular Men-*.zip` | Same Quaternius pack as above, Drive-exported copy | **Confirmed safe** — CC0 | Same as above (current character source) | Safe to use now |

## Does anything here resolve the Synty licensing TODO?

**No.** Checked specifically for this, per the brief. Nothing in `D:\XinyeData\projects\assets` contains
an actual Unity Asset Store receipt, invoice, or license file for POLYGON Heist/Office/City. The one
folder that does contain those exact three packages (the 80-pack bundle) carries the opposite of a
license — an explicit "this is not for commercial use, buy the genuine version" disclaimer. The
ship-blocking TODO in `docs/DRAFT_HANDOFF.md` (buy real Asset Store licenses for the three packs already
imported, same publisher account as `OfficeEssentialsPack`) remains open and unresolved by this audit.
Current real Asset Store prices for those three (confirmed live 2026-08-11, see
`docs/ART_PACK_RESEARCH.md` for detail): **Heist $29.99 + Office $49.99 + City $20.00 ≈ $99.98 total.**
