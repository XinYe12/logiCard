# Third-party assets — Environment

Provenance record for external environment/prop assets sourced or evaluated for the C53
environment detail pass. Same discipline as `Assets/_Project/Art/Characters/THIRD_PARTY.md`:
pack name, source URL, license, date. CC0 needs no attribution, but portfolio-ship provenance
must stay traceable.

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

## Quaternius "Ultimate House Interior Pack" — SELECTED, imported (2026-08-10)

- **Author / source:** Quaternius
  - Official: https://quaternius.com/packs/ultimatehomeinterior.html
  - poly.pizza: https://poly.pizza/bundle/Ultimate-House-Interior-Pack-2SXnFbwFzm
  - Downloaded via OpenGameArt mirror (same pack, June 2020 zip):
    https://opengameart.org/content/lowpoly-house-interior-pack
    (`ultimate_house_interior_pack_-_june_2020.zip`)
- **License:** CC0 / Public Domain (per Quaternius / poly.pizza / OpenGameArt listings)
- **Date evaluated:** 2026-08-10
- **Date imported:** 2026-08-10 (checkpoint 3 — human selected this pack over KayKit)
- **Status:** Curated FBX subset imported under `Interior/Source/`; URP/Lit prefabs baked by
  `InteriorPackImportTool` into `Resources/Interior/` for runtime `BoardView` loading.
- **In use (runtime prefab → source FBX):**
  | Prefab | Source |
  |---|---|
  | `Door` / `DoorAlt` / `DoorDouble` | `Door_1` / `Door_2` / `Door_Double` |
  | `WindowSmall` / `WindowLarge` | `Window_Small1` / `Window_Large1` |
  | `LightCeiling` / `LightCeilingAlt` / `LightDesk` | `Light_CeilingSingle` / `Light_Ceiling1` / `Light_Desk` |
  | `Shelf` / `ShelfLarge` / `Bookshelf` | `Shelf_1` / `Shelf_Large` / `Bookshelf` |
  | `Cabinet` / `Table` / `Chair` | `Kitchen_Cabinet1` / `Table_RoundSmall` / `Chair_1` |
- **Import tooling:** `Assets/_Project/Art/Editor/InteriorPackImportTool.cs` (batchmode
  `-executeMethod LogiCard.Art.Editor.InteriorPackImportTool.Run` or menu
  **Tools → LogiCard → Import Interior Pack Prefabs**). Mirrors `PawnImportTool` FBX → URP Lit.
- **Not imported:** Blends/OBJ/full 120+ catalog — only the curated subset above (repo size).

---

## KayKit "Dungeon Remastered" — REJECTED (human chose Quaternius)

- **Author / source:** Kay Lousberg / KayKit
  https://kaylousberg.com/game-assets/dungeon-remastered
- **License:** CC0 1.0 Universal
- **Date evaluated:** 2026-08-10
- **Status:** Proposed as checkpoint-2 alternate; human selected Quaternius House Interior
  (C54 / checkpoint 3). Not imported. Fantasy-dungeon read was the weaker genre match for
  Yard/Hall/Vault.

---

## Kenney "Smoke Particles" — SELECTED for cloud-bank particle sprites (in repo)

- **Author / source:** Kenney Vleugels (www.kenney.nl)
  - Pack page: https://kenney.nl/assets/smoke-particles
- **License:** CC0 1.0 Universal (public domain dedication,
  http://creativecommons.org/publicdomain/zero/1.0/) — attribution appreciated, not required.
  Also evaluated Kenney's broader "Particle Pack" (https://kenney.nl/assets/particle-pack, same CC0
  license, 80 sprites) as an alternate source; not used — its `smoke_*` sprites read more like
  campfire/explosion smoke wisps than storm cumulus, "White puff" from Smoke Particles was the closer
  silhouette match for a cloud bank.
- **Date sourced:** 2026-08-10
- **Status:** Replaces the primitive-sphere cloud puffs in `BoardWeatherPocket.PlaceCloudPuff` (C53
  "clouds don't read as real weather" callout — the most-cited unrealistic element in a playtest
  screenshot). 8 frames from the pack's "White puff" set (`whitePuff00/03/05/09/12/15/18/21.png`,
  chosen for silhouette variety across the 25 available) were composed into one 4x2 grid atlas
  (`Assets/_Project/Art/Environment/Resources/Weather/CloudAtlas.png`, 1024x512, alpha preserved,
  8px transparent padding per cell to avoid `TextureSheetAnimation` bleeding between tiles). Each
  cloud "puff" is now a burst-spawned, non-moving `ParticleSystem` cluster of billboarded, randomly
  per-particle-framed atlas sprites within the puff's original bounding box, tinted per-particle via
  `startColor` (reusing the original per-layer tint values) rather than one flat-shaded sphere —
  the wispy alpha silhouette reads as soft volumetric mass instead of a hard primitive outline.
  `PlaceRain` / the rain particle system were **not** touched (already read fine per the human's
  prior feedback).
- **On disk:** curated originals (8 `whitePuff*.png` frames + `license.txt`) under
  `Textures/kenney_smoke_particles/`; composed runtime atlas under
  `Resources/Weather/CloudAtlas.png` (not a raw copy of a single source file — a derived composite,
  documented here since it doesn't fit the "originals under Textures/, runtime copies under
  Resources/" 1:1 pattern the Poly Haven entry above uses).
- **Not imported:** the other categories in the pack (Black smoke, Explosion, Fart, Flash) and the
  full 25-frame White puff set — only the 8 frames actually baked into the atlas.

---

## Rejected / not pursued

- **Kenney Building Kit / Furniture Kit** — CC0, modular, but deliberately toy/blocky; fights the
  C53 grounded-detail pivot the same way Kenney Blocky Characters lost to Quaternius for pawns.
- **Quaternius Downtown City MegaKit** — matches the outdoor reference literally; brief says
  translate material language onto the indoor layout, not transplant a city onto the board.
