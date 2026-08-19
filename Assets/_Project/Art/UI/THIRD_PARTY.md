# Third-party assets — UI

Provenance record for external UI chrome assets. Same discipline as
`Assets/_Project/Art/Environment/THIRD_PARTY.md` / `Assets/_Project/Art/Characters/THIRD_PARTY.md`:
pack name, source, license, date. CC0 needs no attribution, but portfolio-ship provenance must stay
traceable.

---

## Kenney "UI Pack - Adventure" — SELECTED, imported (2026-08-13)

- **Author / source:** Kenney (www.kenney.nl), part of the "Kenney Game Assets All-in-1" bundle
  (v2.8) — same CC0 library already trusted elsewhere in this project (`docs/ASSET_PACK_AUDIT.md`
  calls out Kenney All-in-1 as one of only two confirmed-CC0 external libraries on hand).
- **License:** CC0 1.0 Universal (per the pack's own `License.txt`). No attribution required.
- **Date sourced:** 2026-08-13, for the Character Select carousel — human Play feedback on the
  hand-drawn flat-color cards/buttons ("it is still bad") asked for existing, theme-fitting UI
  instead of continuing to hand-roll chrome from scratch.
- **Why this pack over the others surveyed:** compared Kenney's plain "UI Pack" (flat, cold blue,
  reads as generic mobile-game chrome — risks exactly the "cold observer describes it as default
  Unity" failure `docs/ART_DIRECTION.md` §7 warns against), "Fantasy UI Borders" (monochrome outline
  frames, no material read), and "Boardgame Pack" (great tabletop-piece iconography, but tokens/dice/
  cards, not panel/button chrome). "UI Pack - Adventure" ships warm wood-bordered, cream-parchment
  9-slice panels and buttons — closest existing match to this project's "cardstock Time Card" /
  desk-lamp-warm palette (`ART_DIRECTION.md` §4/§6) of anything in the CC0 library actually on hand.
  Not a perfect thematic fit (the pack reads fantasy-adventure, not SWAT-tactical) — flagged as a
  known compromise, not a claimed ideal.
- **Assets in use** (all from the pack's `PNG/Double` — 2x — resolution, for less blur once scaled up
  to the carousel's ~260–350px card width):

  | File | Runtime path | Role |
  |---|---|---|
  | `panel_brown.png` | `Resources/CharSelect/panel_brown` | Scout card face (9-slice) |
  | `panel_brown_dark.png` | `Resources/CharSelect/panel_brown_dark` | Juggernaut card face (9-slice) |
  | `button_brown.png` | `Resources/CharSelect/button_brown` | ~~Prev/Next nav buttons~~ — **unused since 2026-08-18**; the shell chrome pass moved Prev/Next onto `UiFactory.CreateShellButton` so every shell button in the game is one family. File kept imported: it is the only warm 9-slice button art on hand if a future screen wants it back. |

- **Import tooling:** `Assets/_Project/Art/UI/Editor/UiKenneyImportTool.cs` (batchmode
  `-executeMethod LogiCard.Art.Editor.UiKenneyImportTool.Run` or menu
  **Tools → LogiCard → Import Character Select Kenney Sprites**) — sets each PNG to
  `TextureImporterType.Sprite` with a hand-measured 9-slice border (see that file's own doc comment
  for how the border pixels were determined; the pack doesn't ship per-file border metadata for
  individually-cropped PNGs, only for its packed spritesheet variant, which this project isn't using).
- **Not imported:** the rest of the pack (checkboxes, hexagons, minimap rings, progress bars,
  scrollbars, banners) — only the three files actually wired into `CharacterSelectView.cs` today.
  Revisit this list if/when other screens adopt the same pack (see `docs/UI_TOOLKIT_MIGRATION_PROPOSAL.md`
  for the broader "should the rest of the UI move off hand-rolled chrome too" question — a separate,
  Integrator-level call, not decided by this file).

---

## Iomanoid (display font) — imported (2026-08-18)

- **Author / source:** Raymond Larabie. Collected by the human into
  `docs/ui-collection/fonts/iomanoid/` (catalog bucket 6 in `docs/UI_CHROME_COLLECTION.md`).
- **License:** CC0 1.0 Universal (`docs/ui-collection/fonts/iomanoid/license.txt`). No attribution
  required; recorded here for provenance only.
- **Date sourced:** 2026-08-13 (collection) / imported to `Assets/` 2026-08-18 for the shell chrome
  restyle (`docs/ui/UI_SHELL_CHROME.md`).
- **Assets in use:**

  | File | Runtime path | Role |
  |---|---|---|
  | `Iomanoid.otf` | `Resources/Fonts/Iomanoid` | Shell display/headline face (`UiFactory.Display`) |

- **Not imported:** the `Front` / `Back` / `Shine` layered variants. Those are meant to be stacked as
  three separately-coloured text layers for a chromed arcade read — worth revisiting if headlines ever
  want that treatment, but the shell restyle gets its depth from a uGUI `Shadow` component on the
  single base face instead, which stays in sync automatically when label text changes at runtime.
- **Body copy is deliberately NOT Iomanoid** — it stays on Unity's `LegacyRuntime.ttf`. Iomanoid is a
  wide display face; at 20–26pt paragraph sizes it loses legibility fast. Display face for headlines,
  neutral face for everything you actually read.
