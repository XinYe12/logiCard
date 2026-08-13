# Third-party assets — Environment

Provenance record for external environment/prop assets sourced or evaluated for the C53
environment detail pass. Same discipline as `Assets/_Project/Art/Characters/THIRD_PARTY.md`:
pack name, source URL, license, date. CC0 needs no attribution, but portfolio-ship provenance
must stay traceable.

---

## Poly Haven PBR textures — SELECTED for materials (in repo)

- **Author / source:** Poly Haven (polyhaven.com) — individual texture assets listed below
- **License:** CC0 1.0 (per https://polyhaven.com/license)
- **Date sourced:** 2026-08-10; **runtime re-fetch 2K:** 2026-08-12 (ART_PACK_RESEARCH Lighting+Ground use-now #1)
- **Status:** Runtime `Resources/BoardSurfaces/` holds **2K** JPG (diff / nor_gl / rough), wired via
  `BoardSurfaceMaterials`. Used to translate the reference's wet asphalt / masonry / wood material
  language onto the indoor facility without transplanting a city. 1K copies kept locally under
  `Textures/_1k_backup_2026-08-12/` (not required in git).
- **Assets in use:**
  | Runtime name | Poly Haven asset | URL |
  |---|---|---|
  | `asphalt_*` | Asphalt 02 | https://polyhaven.com/a/asphalt_02 |
  | `concrete_*` | Concrete Floor | https://polyhaven.com/a/concrete_floor |
  | `brick_*` | Brick Wall 02 | https://polyhaven.com/a/brick_wall_02 |
  | `wood_*` | Wood Planks | https://polyhaven.com/a/wood_planks |
- **On disk:** originals under `Textures/<asset>/`; runtime copies under `Resources/BoardSurfaces/` (**2K**).

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

## CloudAtlas — soft stylized discs (Link's Awakening pillow aim)

- **Date:** 2026-08-12 (supersedes the Kenney whitePuff composite for the *cloud bank*)
- **Status:** `Resources/Weather/CloudAtlas.png` is now a generated 4x2 atlas of soft circular
  cream discs (`Tools/gen_soft_cloud_atlas.py`) — smooth alpha falloff, near-white RGB at edges
  (no dark smoke rim). Human Play `image copy 10` rejected the Kenney "White puff" atlas: jagged
  silhouettes + grey/black fringe read as outlined "broken cloth," not glued LA-style pillows.
  `BoardWeatherPocket` cloud bank uses Additive particle blend + few large overlapping masses.
- **Kenney Smoke Particles (CC0):** originals still under `Textures/kenney_smoke_particles/` for
  provenance; rim mist may still share the soft atlas. Kenney pack page:
  https://kenney.nl/assets/smoke-particles (CC0 1.0).

---

## Rejected / not pursued

- **Kenney Building Kit / Furniture Kit** — CC0, modular, but deliberately toy/blocky; fights the
  C53 grounded-detail pivot the same way Kenney Blocky Characters lost to Quaternius for pawns.
- **Quaternius Downtown City MegaKit** — matches the outdoor reference literally; brief says
  translate material language onto the indoor layout, not transplant a city onto the board.
