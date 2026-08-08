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
