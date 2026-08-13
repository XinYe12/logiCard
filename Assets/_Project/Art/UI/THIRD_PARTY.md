# Third-party assets â€” UI

Provenance record for external UI chrome assets. Same discipline as
`Assets/_Project/Art/Environment/THIRD_PARTY.md` / `Assets/_Project/Art/Characters/THIRD_PARTY.md`:
pack name, source, license, date. CC0 needs no attribution, but portfolio-ship provenance must stay
traceable.

---

## Kenney "UI Pack - Adventure" â€” SELECTED, imported (2026-08-13)

- **Author / source:** Kenney (www.kenney.nl), part of the "Kenney Game Assets All-in-1" bundle
  (v2.8) â€” same CC0 library already trusted elsewhere in this project (`docs/ASSET_PACK_AUDIT.md`
  calls out Kenney All-in-1 as one of only two confirmed-CC0 external libraries on hand).
- **License:** CC0 1.0 Universal (per the pack's own `License.txt`). No attribution required.
- **Date sourced:** 2026-08-13, for the Character Select carousel â€” human Play feedback on the
  hand-drawn flat-color cards/buttons ("it is still bad") asked for existing, theme-fitting UI
  instead of continuing to hand-roll chrome from scratch.
- **Why this pack over the others surveyed:** compared Kenney's plain "UI Pack" (flat, cold blue,
  reads as generic mobile-game chrome â€” risks exactly the "cold observer describes it as default
  Unity" failure `docs/ART_DIRECTION.md` Â§7 warns against), "Fantasy UI Borders" (monochrome outline
  frames, no material read), and "Boardgame Pack" (great tabletop-piece iconography, but tokens/dice/
  cards, not panel/button chrome). "UI Pack - Adventure" ships warm wood-bordered, cream-parchment
  9-slice panels and buttons â€” closest existing match to this project's "cardstock Time Card" /
  desk-lamp-warm palette (`ART_DIRECTION.md` Â§4/Â§6) of anything in the CC0 library actually on hand.
  Not a perfect thematic fit (the pack reads fantasy-adventure, not SWAT-tactical) â€” flagged as a
  known compromise, not a claimed ideal.
- **Assets in use** (all from the pack's `PNG/Double` â€” 2x â€” resolution, for less blur once scaled up
  to the carousel's ~260â€“350px card width):

  | File | Runtime path | Role |
  |---|---|---|
  | `panel_brown.png` | `Resources/CharSelect/panel_brown` | Scout card face (9-slice) |
  | `panel_brown_dark.png` | `Resources/CharSelect/panel_brown_dark` | Juggernaut card face (9-slice) |
  | `button_brown.png` | `Resources/CharSelect/button_brown` | Prev/Next nav buttons (9-slice) |

- **Import tooling:** `Assets/_Project/Art/UI/Editor/UiKenneyImportTool.cs` (batchmode
  `-executeMethod LogiCard.Art.Editor.UiKenneyImportTool.Run` or menu
  **Tools â†’ LogiCard â†’ Import Character Select Kenney Sprites**) â€” sets each PNG to
  `TextureImporterType.Sprite` with a hand-measured 9-slice border (see that file's own doc comment
  for how the border pixels were determined; the pack doesn't ship per-file border metadata for
  individually-cropped PNGs, only for its packed spritesheet variant, which this project isn't using).
- **Not imported:** the rest of the pack (checkboxes, hexagons, minimap rings, progress bars,
  scrollbars, banners) â€” only the three files actually wired into `CharacterSelectView.cs` today.
  Revisit this list if/when other screens adopt the same pack (see `docs/UI_TOOLKIT_MIGRATION_PROPOSAL.md`
  for the broader "should the rest of the UI move off hand-rolled chrome too" question â€” a separate,
  Integrator-level call, not decided by this file).
