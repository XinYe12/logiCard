# Third-party assets — Environment

Provenance record for external environment/prop assets sourced or evaluated for the C53
environment detail pass (`ENV_LOOKFEEL_AGENT_BRIEF.md` checkpoint 2). Same discipline as
`Assets/_Project/Art/Characters/THIRD_PARTY.md`: pack name, source URL, license, date.
CC0 needs no attribution, but portfolio-ship provenance must stay traceable.

## Open pack choice (human decide before deeper mesh import)

Checkpoint 2 applies Poly Haven **textures** now (material/detail language on the existing
Yard/Hall/Vault layout). Two **mesh** pack candidates are proposed below — **do not treat either
as locked** until the human picks one (or rejects both). Checkpoint 3 (door models) should reuse
whichever mesh pack is chosen.

---

## Poly Haven PBR textures — SELECTED for materials (in repo)

- **Author / source:** Poly Haven (polyhaven.com) — individual texture assets listed below
- **License:** CC0 1.0 (per https://polyhaven.com/license)
- **Date sourced:** 2026-08-10
- **Status:** Downloaded at 1K JPG (diff / nor_gl / rough) and wired into runtime board surfaces
  via `BoardSurfaceMaterials` + `Resources/BoardSurfaces/`. Used to translate the reference's wet
  asphalt / masonry / wood material language onto the indoor facility without transplanting a city.
- **Assets in use:**
  | Runtime name | Poly Haven asset | URL |
  |---|---|---|
  | `asphalt_*` | Asphalt 02 | https://polyhaven.com/a/asphalt_02 |
  | `concrete_*` | Concrete Floor | https://polyhaven.com/a/concrete_floor |
  | `brick_*` | Brick Wall 02 | https://polyhaven.com/a/brick_wall_02 |
  | `wood_*` | Wood Planks | https://polyhaven.com/a/wood_planks |
- **On disk:** originals under `Textures/<asset>/`; runtime copies under `Resources/BoardSurfaces/`.

---

## Candidate A — Quaternius "Ultimate House Interior Pack" — PROPOSED (not imported)

- **Author / source:** Quaternius via poly.pizza
  https://poly.pizza/bundle/Ultimate-House-Interior-Pack-2SXnFbwFzm
  (also listed on quaternius.com as Ultimate House Interior Pack)
- **License:** CC0 / Public Domain (per poly.pizza listing)
- **Date evaluated:** 2026-08-10
- **Why it fits:** 80+ indoor models — doors, windows, furniture, ceiling lights — maps directly
  onto Yard/Hall/Vault dressing and feeds checkpoint 3 door replacement. Same author pipeline as
  the existing character pack (`PawnImportTool` / FBX → URP Lit).
- **Caveat:** low-poly stylized (same family as Modular Men), not photogrammetry. Still a large
  fidelity jump over tinted cubes; materials can be pushed wet-dusk via URP Lit + Poly Haven
  overlays where needed.
- **Not imported yet.** Awaiting human pack choice.

---

## Candidate B — KayKit "Dungeon Remastered" — PROPOSED alternate (not imported)

- **Author / source:** Kay Lousberg / KayKit
  https://kaylousberg.com/game-assets/dungeon-remastered
  (GitHub mirror: https://github.com/KayKit-Game-Assets/KayKit-Dungeon-Remastered-1.0)
- **License:** CC0 1.0 Universal
- **Date evaluated:** 2026-08-10
- **Why it fits:** 200+ modular walls/floors/doors/props, FBX/GLTF, designed for kitbash interiors.
- **Caveat:** fantasy-dungeon read (stone, banners, chests) — further from a modern SWAT-facility
  Yard/Hall/Vault than Quaternius House Interior. Stronger modular wall kit, weaker genre match.
- **Not imported yet.** Awaiting human pack choice.

---

## Rejected / not pursued this pass

- **Kenney Building Kit / Furniture Kit** — CC0, modular, but deliberately toy/blocky; fights the
  C53 grounded-detail pivot the same way Kenney Blocky Characters lost to Quaternius for pawns.
- **Quaternius Downtown City MegaKit** — matches the outdoor reference literally; brief says
  translate material language onto the indoor layout, not transplant a city onto the board.
