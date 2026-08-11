# Third-party assets — Characters

Provenance record for external asset packs sourced/evaluated for the pawn art rework
(`docs/PAWN_ART_REWORK_PLAN.md`). This repo is otherwise "everything procedural, nothing imported"
(board materials, path line, Foley audio are all code-generated) — character models are a deliberate,
approved exception. CC0 needs no attribution, but a portfolio-ship repo should keep provenance traceable
regardless, per the plan's step 2.

## Kenney "Blocky Characters" — REJECTED, kept for provenance only

- **Author / source:** Kenney (kenney.nl/assets, search "Blocky Characters")
- **License:** CC0 1.0 (per the pack's own `License.txt`)
- **Date evaluated:** 2026-08-08
- **Status:** Downloaded and previewed against the Link's Awakening (2019) target. Rejected — genuinely
  blocky/Minecraft-style: rectangular boxes, flat hand-painted texture skins, square heads/limbs. Close to
  the *opposite* of the rounded/glossy target. Also a single rigid base mesh with only skin/color variety,
  so Scout vs. Juggernaut would never read as a different silhouette, only a different paint job — the same
  trap the original rejected primitive-assembly attempt (`377029f`) fell into.
- **Not imported.** No files from this pack live in this repo.

## Quaternius "Ultimate Modular Men" — SELECTED (geometry base)

- **Author / source:** Quaternius (quaternius.com)
- **License:** CC0 1.0 Universal / Public Domain Dedication (per the pack's own `License.txt`)
- **Date evaluated:** 2026-08-08
- **Status:** Downloaded, previewed, and selected as the geometry base. Genuinely modular — separate
  head/torso/legs/feet skeletal meshes across roughly a dozen outfits (Adventurer, Swat, Farmer, King,
  Punk, Worker, Spacesuit, Beach, Suit, Casual, Casual_2, Casual_Hoodie, Horse) — so Scout (lean/fast) and
  Juggernaut (bulky/armored) can genuinely differ in silhouette, not just color. Current front-runner
  pairing: Adventurer (lean) for Scout vs. Swat (bulky tactical armor, shoulder/knee pads) for Juggernaut.
- **Important caveat:** out of the box this pack is *not* a visual match for the Link's Awakening target
  either — it's faceted/angular low-poly (visible flat triangle shading) with adult/realistic proportions,
  not chibi, and matte, not glossy. Picking this pack does not by itself solve the art-direction gap. The
  plan compensates downstream of the mesh rather than chasing geometry no free pack has: smoothed normals
  on import, a glossy URP Lit material with pushed `_Smoothness`, and the existing Day 9 lighting/post
  Volume. Chibi proportions (scaling the separately-meshed head relative to the body) is a separate,
  still-untested lever. See `docs/PAWN_ART_REWORK_PLAN.md` for the full plan and `docs/DRAFT_HANDOFF.md` for
  current status.
- **Import status:** meshes/materials/prefabs not yet in this repo — importing them is the main worktree's
  ongoing work, not this document's.

---

## 2026-08-10 reassessment — post-C53/C55, before any board-realism replacement decision

Context: the board underwent a substantial realism pass this session (real Poly Haven PBR surface
textures, real Quaternius door/prop meshes, a real post-processing stack) and `PawnView.ApplyTime` moved
from stepped 8–12fps playback to smooth per-frame interpolation (**C55**) — all landed 2026-08-10, all
after the Quaternius pawn import above. `ART_DIRECTION.md`'s Characters ship-bar row already flags the
current import as "fine for the current build but likely too generic/undifferentiated for a paid,
distinctly-branded product." This section is that flag's follow-up: a concrete look at what's actually in
the repo now, done by reading `PawnView.cs`, the imported prefabs/materials, and `BoardView.cs` (this
agent has no Editor/screenshot access this session — see `CHARACTER_MODEL_REWORK_AGENT_BRIEF.md`; findings
below are from asset/code inspection, not an in-Editor look, and should be treated as a hypothesis a
sighted verification pass still needs to confirm per `docs/PAWN_ART_REWORK_PLAN.md`'s explicit "don't
declare this done from tests alone" warning).

**Concretely wrong, in order of how much it likely reads:**

1. **Scout's outfit is genre-wrong, not just under-detailed.** `Assets/_Project/Art/Characters/Resources/Scout/Adventurer.fbx`
   is the pack's "Adventurer" outfit — picked back when the target was toy-chibi Link's Awakening (any
   fantasy-adjacent read was fine). The board direction has since moved to a grounded SWAT-facility
   Yard/Hall/Vault language (C53). An RPG-adventurer silhouette dropped into a realistic tactical interior is a
   direct thematic clash, independent of poly count or material quality — this is likely the single biggest
   "reads wrong" contributor, bigger than material fidelity. Juggernaut's "Swat" outfit
   (`Resources/Juggernaut/Swat.fbx`) does not have this problem — it was already the closest thing to
   "tactical operator" in the source pack.
2. **No albedo/detail textures anywhere.** Every per-part material under both `Resources/Scout/Materials/`
   and `Resources/Juggernaut/Materials/` (Skin, Hair, Black, Brown, Gold, Green, Grey, Swat, Swat_Black,
   Visor, ...) has `_BaseMap` bound to nothing (`m_Texture: {fileID: 0}`) — flat solid-color URP/Lit
   materials, no albedo, no normal detail beyond import-time smoothed vertex normals. That was an acceptable
   trade against a toy-figurine target (toys read as flat-colored plastic). Next to Poly Haven PBR board
   surfaces and real door meshes it will read as placeholder/generic low-poly, which matches
   `ART_DIRECTION.md`'s own "likely too generic" flag.
3. **Faceted low-poly geometry, adult-human proportions.** `THIRD_PARTY.md`'s own original entry above
   already flagged this at selection time ("faceted/angular low-poly ... not chibi") as a gap the plan
   accepted and meant to compensate downstream (smoothed normals + glossy material), not fix at the
   geometry level. `PawnImportTool.cs` does apply 60°-angle normal smoothing and URP/Lit materials at
   `_Smoothness: 0.5–0.6` (see below) — the compensation happened — but it's compensating for a toy-plastic
   read, not a "real tactical operator" read, and the brief's new target (realistic, not toy) is a
   different bar than what this compensation was tuned for.
4. **Smoothness is already reasonably pushed, contrary to what Stage 2 might have guessed blind.** Every
   per-part `.mat` inspected is at `_Smoothness: 0.5` (the dozen `Resources/*/Materials/*.mat` files, set by
   `PawnImportTool.ImportArchetype`'s `smoothness` parameter) — this was already tuned toward "glossy toy
   plastic" per the original plan's step 4, not left at a primitive-era low value. There's no obvious
   further "push smoothness up" win available without a screenshot to compare against, so this agent did
   not touch it blind (see report-back / Stage 2 notes).
5. **Minor, not confirmed as wrong:** `PawnView.TargetVisualHeight` normalizes every imported archetype to
   1.0 world units tall; `BoardView.WallHeight` is 0.85. Pawns end up slightly taller than the room walls.
   This predates this pass (same ratio existed for the old primitive pawns, whose torso+head also landed
   near ~0.92) so it isn't a regression introduced by the Quaternius import, but it's worth a look next time
   someone has eyes on the running board.

**Small, safe items found but *not* touched this pass (no visual verification available):**
`Assets/_Project/Art/Characters/Resources/Scout/Scout_Body.mat` is an orphaned material — grep across the
whole repo finds no reference to it (by path or GUID) outside its own `.mat`/`.meta` pair. It looks like a
leftover from an earlier iteration before `PawnImportTool` switched to external per-part materials
(`Materials/Skin.mat` etc.). Safe to delete as repo hygiene whenever someone's next in this folder — this
agent left it in place rather than run a delete command speculatively during a research-only pass.

### Candidate replacement packs (proposed, not imported — human decision needed, same discipline as the door-mesh pick)

None of these is a slam-dunk "the" answer; each trades something. Ranked by how directly they solve the
Adventurer/genre-clash + flat-material problem above, not by how novel the pack is.

**A. Re-outfit within the already-owned Quaternius "Ultimate Modular Men" pack (not a new pack)**
- **What:** swap Scout's source FBX from `Adventurer` to a more contemporary outfit already cataloged in
  the same pack — `Worker`, `Casual`, or `Casual_Hoodie` are the closest to plain-clothes/civilian-operator
  reads; none of the un-imported outfits are a perfect "tactical" match the way `Swat` already is for
  Juggernaut, but any of them reads less fantasy than `Adventurer`. The specific outfit FBX files aren't in
  this repo (only `Adventurer.fbx`/`Swat.fbx` were ever imported, per the "import minimally" plan step) —
  they'd need re-downloading from the same already-vetted CC0 source.
- **License:** CC0 1.0, already recorded above, zero new provenance work.
- **Fit:** fixes the single biggest concrete issue (#1) at effectively zero license/pipeline risk — reuses
  `PawnImportTool` unchanged, same tinting logic, same scale normalization. Does **not** fix #2/#3 (still
  flat-material faceted low-poly) — it's a genre fix, not a fidelity fix.
- **Caveat:** cheapest option by far, but the ART_DIRECTION "likely too generic" flag is really about
  fidelity vs. the now-realistic board, and this doesn't move that needle. Good candidate for "fix the worst
  offender now, revisit fidelity later" rather than a final answer.

**B. Mixamo (Adobe) rigged human characters**
- **Source:** https://www.mixamo.com — free Adobe service, browser-based character/animation library.
- **License:** not CC0 — Adobe's own Mixamo terms. Free for unlimited commercial/non-commercial use,
  royalty-free, no attribution required, characters/animations may be embedded in a shipped game; the
  restriction is on *redistributing the raw FBX/animation files as a standalone asset pack or asset-store
  product* (selling the files themselves), which doesn't apply to shipping them baked into this game.
  Verify current terms at https://helpx.adobe.com/creative-cloud/faq/mixamo-faq.html before committing —
  Adobe has called Mixamo a "technology preview" historically and terms are Adobe's to change.
- **Fit:** real human proportions, actual textured materials (not flat per-part colors), much closer to
  "real tactical operator" than any faceted CC0 low-poly pack — directly answers #2/#3. Several catalog
  characters read as plausible tactical/utility figures once re-clothed/recolored (the catalog itself is
  mostly base humans + a few themed outfits, not a dedicated SWAT lineup — searched and could not confirm a
  purpose-built "SWAT operator" named asset in the catalog, so this would still need an outfit/texture pass,
  not a drop-in).
- **Caveats:** (a) real-human-realistic is a bigger swing than "glossy toy sheen" — `ART_DIRECTION.md` still
  frames characters as the sole "approved exception" using URP/Lit + pushed smoothness for a *toy* read, not
  photoreal skin/fabric; picking Mixamo changes the pitch, not just the fidelity, and that's a call for the
  human, not this agent. (b) requires an Adobe account + the Mixamo web downloader (not a single static ZIP
  URL like the CC0 packs), so import tooling/process changes slightly. (c) heavier per-character texture
  footprint than the current flat-color materials — repo-size tradeoff to weigh (this project has been
  size-conscious about imports, e.g. "curated subset only" language in `Art/Environment/THIRD_PARTY.md`).

**C. Quaternius "Universal Base Characters"**
- **Source:** https://quaternius.com/packs/universalbasecharacters.html /
  https://quaternius.itch.io/universal-base-characters
- **License:** CC0 1.0 (same author/license family as the currently-imported pack — zero new legal
  diligence).
- **Fit:** same author as the current pack but a different, more recent base mesh: "Superhero / Regular /
  Teen" body-proportion variants, ~13k tris, humanoid-rigged, 20 hairstyles + skin/eye color mixing for
  variety. The "Regular" proportion variant is less exaggerated/faceted than Ultimate Modular Men's
  adventurer-fantasy geometry and could read more grounded. Companion "Universal Animation Library" pack
  exists if animation is ever wired up (not currently — `PawnView` explicitly uses no `Animator`, C55 plan
  step 5).
- **Caveats:** this is a **base body** pack (skin/hair/eye customization), not a clothed-outfit pack the way
  Ultimate Modular Men is — there's no off-the-shelf "tactical operator" clothing here. It would need
  pairing with a separate clothing/outfit source (possibly back to option A/B, or a texture-paint pass) to
  read as armed operators rather than bare civilians, which is real extra work this agent did not scope
  further. Doesn't obviously beat option A on its own merits unless paired with real outfit work, and
  doesn't solve the flat-material problem any more than the current pack (no confirmation it ships textures
  beyond flat color/skin tone in the free tier).

**Ruled out without a full write-up (genre/style mismatch confirmed by the same discipline
`Art/Environment/THIRD_PARTY.md` already used for the door-pack pick):**
- **Kenney "Mini Characters" / "Modular Characters" / "Roguelike Characters"** — same toy/blocky-voxel
  family as the already-rejected "Blocky Characters" (`docs/PAWN_ART_REWORK_PLAN.md` step 1); CC0 but wrong
  direction.
- **KayKit (Kay Lousberg)** — confirmed via kaylousberg.com/itch.io catalog: Adventurers, Skeletons,
  Dungeon Remastered, seasonal "Mystery Monthly" packs — entirely fantasy/dungeon themed, no modern/tactical
  character pack exists in this author's catalog. Same genre mismatch that lost KayKit's *environment* pack
  (Dungeon Remastered) to Quaternius House Interior for this project's checkpoint 3 door-mesh pick
  (`Art/Environment/THIRD_PARTY.md`).
- **Synty Studios "POLYGON" series (e.g. a SWAT/Tactical pack)** — closest genre match by far, but paid and
  not CC0 (Synty's standard EULA has per-seat/revenue-tier restrictions); breaks this project's "CC0/free"
  constraint outright, not evaluated further.
- **itch.io/CGTrader/Fab "SWAT"-tagged low-poly singles** (e.g. "SWAT Character - 3D Voxel Low Poly Model",
  various CGTrader/Fab "SWAT Operator" listings) — genre-relevant by name but every one surfaced in this
  search is either paid, ambiguously licensed (marketplace "Standard License" style terms, not CC0), or a
  single unrigged prop-quality model rather than a pack — none cleared this project's CC0/free bar on
  inspection of their listing pages.

**Recommendation (not locked):** if a decision is needed with minimal churn, (A) re-outfitting Scout within
the pack already in this repo is the safest immediate fix for the worst concrete problem (#1, genre clash)
at effectively zero new risk. If the human wants to actually close the "generic/undifferentiated" gap
`ART_DIRECTION.md` flags rather than defer it again, (B) Mixamo is the strongest fidelity jump but is a
real art-direction conversation (toy sheen vs. realism), not just an asset swap — that conversation should
happen before any import. (C) is listed for completeness but needs outfit work either way and doesn't
obviously outperform (A) unless someone commits to sourcing clothing for it too. None of these three is
imported; all await the human's call per the brief.

---

## 2026-08-10 — Scout re-outfit: Adventurer → Worker (option A applied, C56)

Human decision (`PRODUCT_MEMORY.md` C56): apply option A above — re-outfit Scout within the already-owned
CC0 Quaternius "Ultimate Modular Men" pack rather than adopt a new pack. This entry records what changed.

- **What changed:** `PawnImportTool.ImportScoutBatch()` now points at
  `Assets/_Project/Art/Characters/Resources/Scout/Worker.fbx` instead of `Adventurer.fbx`; `Scout.prefab`
  was rebuilt from it via the unchanged `ImportArchetype` pipeline (same 60°-angle normal smoothing, same
  `smoothness: 0.6f`, same output path `Resources/Scout`, same archetype name `Scout`). No pipeline code
  changed — this was a source-asset swap only, per the brief's explicit scope.
- **Which outfit and why:** picked `Worker` over the other two candidates named in the brief
  (`Casual`/`Casual_2`, `Casual_Hoodie`). Inspected each candidate FBX's part names before choosing:
  `Worker` is `Worker_Body`/`Worker_Feet`/`Worker_Head`/`Worker_Legs` plus two extra parts,
  `Worker_Vest`/`Worker_Yellow` (a high-vis work vest) — reads as utility/maintenance-operator attire, i.e.
  someone with a working role inside a facility, not off-duty wear. `Casual_Hoodie` (explicitly a hoodie)
  and `Casual_2` (plain T-shirt/pants) both read as leisurewear, which the brief explicitly said to avoid.
  Note the pack's actual file set (verified against the live Google Drive distribution, see below) is
  `Casual_2`/`Casual_Hoodie`, not a plain `Casual` — the un-suffixed `Casual` named in the original catalog
  list above and in the brief does not exist as a separate file in the pack; `Casual_2` was evaluated in its
  place.
- **Source:** quaternius.com's official distribution — the site's own "Ultimate Modular Men" pack page
  (`https://quaternius.com/packs/ultimatemodularcharacters.html`) links a public Google Drive folder as its
  only download path; `Worker.fbx` was pulled from that folder's `Individual Characters/FBX/` subfolder.
  The poly.pizza mirror named as an alternative source in the brief returned an HTTP 403 (Cloudflare bot
  challenge) this session and was not reachable — not used. Same author, same pack, same CC0 1.0 Universal
  license already recorded above; no new provenance/licensing work needed.
- **File size note (not a defect, flagged for awareness):** `Worker.fbx` is ~8.1MB versus `Adventurer.fbx`'s
  582KB. Inspecting both files' FBX AnimStack counts explains why: `Adventurer.fbx` (already in this repo,
  presumably from the poly.pizza mirror per the brief) carries only 1 AnimStack, essentially a static
  bind-pose export; `Worker.fbx`, pulled from quaternius.com's `Individual Characters/FBX` folder, carries
  the pack's full 26 baked animation clips, same as `Juggernaut/Swat.fbx` (also 8.1MB, 26 AnimStacks,
  already in this repo from the same official source). `PawnView` never uses an `Animator` (C55) and
  `PawnImportTool` only extracts meshes/materials into a static prefab, so the extra animation data is
  inert — it costs import time and repo size, not correctness. Consistent with `Swat.fbx`'s existing
  precedent rather than a new problem; not addressed further (repo-size cleanup is out of this brief's
  scope, same discipline as leaving `Scout_Body.mat` orphaned above).
- **Team-color tint hook:** verified, no code change needed. `PawnView.TintedPartNameMarker` matches any
  renderer whose name contains `"Body"` (case-insensitive). `Worker.fbx`'s six part nodes are
  `Worker_Body`, `Worker_Feet`, `Worker_Head`, `Worker_Legs`, `Worker_Vest`, `Worker_Yellow` — `Worker_Body`
  matches the same way `Adventurer_Body` did, and only that one part. `PawnView.cs` was not touched.
- **Materials:** `PawnImportTool.ImportArchetype`'s manual Standard→URP/Lit shader-swap loop reported
  "re-hooked 0 material(s)" in this run's log — inspecting the resulting `.mat` files shows this is *not*
  a failure: in this Unity 6000.5.5f1 project (URP active), the model importer already assigns extracted
  materials directly to `Universal Render Pipeline/Lit` with `_BaseColor` populated from the FBX during
  `SaveAndReimport()`, before the tool's loop runs — so the loop's `material.shader == urpLit` guard skips
  every material as a no-op, both the reused per-part materials (`Skin.mat`, `Black.mat`, etc., shared by
  name with the old Adventurer materials) and the two new ones this import created (`Worker_Vest.mat`,
  `Worker_Yellow.mat`, plus `LightBrown.mat`/`Moustache.mat` extracted alongside). All four confirmed on
  `Universal Render Pipeline/Lit` (shader guid `933532a4fcc9baf4fa0491de14d08ed7`, same as every other
  archetype material including `Juggernaut/Materials/Swat.mat`) with sensible per-part `_BaseColor` values
  already set. One side effect worth flagging: every material's `_Smoothness` sits at URP/Lit's default
  `0.5`, not the `0.6f` `ImportScoutBatch` passes — because the tool's smoothness-set line lives inside the
  same skipped loop. This is a **pre-existing** condition, not introduced by this swap: the reassessment
  section above already found the *previous* Adventurer-era materials sitting at `0.5` despite the same
  `0.6f` parameter, for the same reason. Not fixed here — the brief scoped this pass to the source-asset
  swap only, not a pipeline/fidelity fix.
- **`Adventurer.fbx`:** left in place, not deleted, per the brief. A repo-wide grep for `Adventurer` found
  only `PawnImportTool.cs` (now repointed to `Worker.fbx`) referencing it — no other file uses it after this
  change, but deleting it is repo-hygiene work outside this brief's scope.
- **Verification performed:** Unity 6000.5.5f1 batchmode from this worktree
  (`D:\projects\Game\logiCard-env-lookfeel`) — `-executeMethod LogiCard.EditorTools.PawnImportTool.ImportScoutBatch`
  rebuilt `Scout.prefab` cleanly (confirmed `Scout.prefab`'s `PrefabInstance.m_SourcePrefab` guid now matches
  `Worker.fbx.meta`'s guid). EditMode suite: 124/124 passed. PlayMode suite: 37/37 passed. **Not verified:**
  how this actually looks in the Editor/game view — this agent has no screenshot/Editor-interactive access
  this session, same limitation the reassessment above notes. A human sighted pass is still needed to
  confirm the Worker outfit actually reads as "plainclothes/civilian-operator" against the SWAT-facility
  board rather than something else unanticipated from asset inspection alone.

---

## 2026-08-11 — Quaternius "Ultimate Modular Men" retired, ithappy "Creative Characters FREE" adopted (character-pack-swap)

Human decision (per `docs/ART_PACK_RESEARCH.md`'s Characters section): stop compensating for Quaternius's
faceted/matte/adult-proportioned geometry downstream and switch the base pack instead. `Assets/ithappy/
Creative_Characters_FREE/` (publisher ithappy, 85,000+ ratings) landed for this — confirmed Humanoid rig,
Mixamo-compatible, 420 combinable modular parts across 15 slot categories, ships its own Editor assembly
tool (`CharacterCustomizationWindow`/`CustomizableCharacter`) rather than one finished FBX per archetype.

- **Author / source:** ithappy (Unity Asset Store, "Creative Characters FREE").
- **License:** Unity Asset Store standard EULA (free asset). Already present in the repo prior to this
  pass (`b3d9eb7`/`0c430a3`); this entry records its adoption as the Scout/Juggernaut base, not its import.
- **What changed:** `PawnImportTool.cs` (Quaternius's one-FBX-in pipeline) does not fit a slot-based
  modular system, so a sibling tool, `Assets/_Project/Editor/CharacterCustomizationImportTool.cs`, drives
  the pack's own `CustomizableCharacter`/`SlotLibrary` assembly API directly — same discipline as
  `InteriorPackImportTool`/`PawnImportTool`'s "reuse the pack's intended workflow" precedent. `Tools/
  LogiCard/Import Scout (Creative Characters Pack)` and `Tools/LogiCard/Import Juggernaut (Creative
  Characters Pack)` rebuild `Resources/Scout/Scout.prefab` and `Resources/Juggernaut/Juggernaut.prefab` in
  place at the exact paths `PawnView.cs` already expects — `PawnView.cs` itself needed zero changes.
- **Part picks:** Scout (lean/civilian) — `Outfit_010` (the pack's only Outfit-group option),
  `Pants_009`, `Shoe_Slippers_005`, `Hairstyle_Male_005`, no headgear (bare head reads lean next to
  Juggernaut's helmet). Juggernaut (bulky/armored) — `Outwear_050`, `Pants_010`, `Shoe_Sneakers_009`,
  `Hat_Single_016`, `Gloves_014`, no hairstyle (helmet covers the head instead). Every ambiguous pick
  (Outwear/Pants/Shoes/Hat/Gloves variant) was chosen by comparing `SkinnedMeshRenderer.sharedMesh.bounds`
  volume across that slot's real variants (`Tools/LogiCard/Diagnostics/Log Character Part Bounds`) and
  taking the largest for Juggernaut / smallest for Scout, not guessed from filenames — see
  `CharacterCustomizationImportTool.BuildScout`/`BuildJuggernaut`'s inline comments for the exact numbers.
  Slots the library never assigns to any pickable variant at all (`Mustache`/`T_Shirt` — no `SlotType`
  entry exists for either in `SlotLibrary.asset`) and slots left toggled off (`Full_body`, `Accessories`,
  `Glasses`) keep whatever `Base_Mesh.fbx` ships by default for that node — confirmed by direct inspection
  (`Tools/LogiCard/Diagnostics/Log Baked Prefab Renderer Meshes`) to be degenerate 4-vertex/zero-volume
  placeholders on both baked prefabs, not real leftover geometry, so leaving them unassigned is silent by
  construction.
- **Material/shader:** confirmed directly on both baked prefabs — every `SkinnedMeshRenderer` (all slots,
  enabled or not) shares the pack's single `Materials/Color.mat` (shader guid
  `933532a4fcc9baf4fa0491de14d08ed7`, `Universal Render Pipeline/Lit` — same shader already verified on
  every other archetype material in this project). No shader conversion step needed, unlike every prior
  pack this project re-sourced; `CustomizableCharacter`'s own assembly forces every part onto that one
  material regardless of which parts are picked, matching the pack's texture-atlas-driven, not
  per-part-unique-material, design.
- **Team-color tint hook:** verified, no `PawnView.cs` change needed. Both baked prefabs have a
  `SkinnedMeshRenderer` named exactly `Body` (torso mesh `Body_011`), which `PawnView.TintedPartNameMarker`
  ("Body", case-insensitive substring) matches directly. Note for future readers: the same substring also
  matches the `Full_body` renderer (case-insensitive `"Full_body".Contains("Body")`) — currently harmless
  since neither archetype selects a `FullBody` costume and that renderer stays at its degenerate
  placeholder mesh, but a future archetype using a Costume variant would tint `Full_body` too. Not changed
  here since it's inert for the current two archetypes.
- **Scale sanity check:** combined renderer bounds before `PawnView.TryBuildImported`'s rescale — Scout
  `(1.4407, 1.8521, 0.4134)`, Juggernaut `(1.4454, 2.0887, 0.4559)` — against `TargetVisualHeight = 1.0f`
  gives scale factors of ~0.54x and ~0.48x respectively. Neither is absurdly tiny/huge; both land in the
  same range the old Quaternius archetypes did.
- **Animator:** both baked prefabs keep the `Animator` component `Base_Mesh.prefab` ships with (harmless,
  keeps the door open for a future real animation pass) but with `m_Controller` cleared to `{fileID: 0}`
  and `m_ApplyRootMotion: 0` — confirmed directly in both prefabs' serialized YAML. No `CharacterController`/
  `CharacterMover`/`MovePlayerInput` components from the pack's own `CharacterCustomizationWindow.SavePrefab`
  workflow were added; this project's own `PawnView`/`RoundPlayback` drives position, not the pack's
  built-in player-input movement scripts.
- **Quaternius "Ultimate Modular Men" status:** retired as the Scout/Juggernaut base. Its entries above are
  kept for provenance per this doc's own stated purpose; the source FBX/materials under
  `Resources/Scout/Worker.fbx` and `Resources/Juggernaut/Swat.fbx` (plus their per-part `.mat` files) are
  left in the repo, now unreferenced by the rebuilt prefabs — deliberately not deleted this pass, same
  discipline as this doc's earlier "not addressed further, out of scope" notes on `Scout_Body.mat`/
  `Adventurer.fbx`. Repo-hygiene cleanup (deleting the now-orphaned Quaternius source assets) is a safe
  follow-up whenever someone's next in this folder.
- **Verification performed:** Unity 6000.5.5f1 batchmode from this worktree
  (`D:\projects\Game\logiCard-character-pack-swap`) — both archetypes rebuilt via `Tools/LogiCard/Import
  Scout (Creative Characters Pack)` / `...Juggernaut...`, confirmed via direct inspection of the baked
  prefabs' serialized YAML (mesh/material assignments, `Animator` state) and a dedicated diagnostic
  (`Tools/LogiCard/Diagnostics/Log Baked Prefab Renderer Meshes`), not just "the menu item ran without an
  exception." EditMode/PlayMode batchmode results recorded in `docs/DRAFT_HANDOFF.md`. **Not verified:** how
  this actually looks rendered — same standing limitation as every entry above; a human sighted pass in the
  Editor is still needed.
