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
