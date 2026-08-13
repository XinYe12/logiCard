# UI chrome / resource collection

**Status:** Active collection — human supplies resources/code; UI seat categorizes.  
**Started:** 2026-08-13 on `feat/modal-restyle` (full UI ownership seat).  
**Target feel:** Link’s Awakening–like toy / diorama UI — **same language in lobby shell and in-match HUD**.  
**Not a PRODUCT_MEMORY row** until human confirms a locked chrome family.

## Working rule

1. Human keeps sending resources (links, packs, screenshots, fonts, audio, **code**).
2. UI seat **categorizes only** into the buckets below — no blind Asset Store shopping.
3. Keep collecting until UI seat says **we have what we need** (see “Stop collecting”).
4. Do **not** adopt a pack or stack change from an earlier rejected shortlist without a fresh human pick.

## Rejected (do not reopen as defaults)

Human rejected the 2026-08-13 browsing shortlist (Kenney Adventure as *the* direction for the whole game, Storybook/Enchanted Forest/Mystic Realm/Soft Touch/etc. as recommended buys). Those links are **not** the collection target. Character Select may still carry Kenney Adventure sprites from an earlier seat — that is historical import, not a locked product chrome decision for all UI.

## Buckets (catalog every item into one or more)

| # | Bucket | What to collect |
|---|--------|-----------------|
| **1** | Visual language refs | Screens / clips of the target look (menu + HUD). Zelda LA or any other diorama/toy UI the human likes. |
| **2** | Panel / window chrome | 9-slice or mesh frames: wood rim, cream face, soft shadow — lobby cards, modals, Time Card, character/map panels. |
| **3** | Buttons | Primary / secondary / disabled / pressed — same material language as panels (Confirm, Lock In, Rematch, Quit, Prev/Next). |
| **4** | Slots / wells | Round or square item wells (gear hand, future inventory) — empty, hover, selected, locked. |
| **5** | Icons | Bandage, Interact, Flashbang, Adrenaline; stance (Sprint/Walk/Crawl); Snap/Hold; door; wound; Lock In. Prefer one consistent set. |
| **6** | Type / fonts | Display + body that read as toy/storybook — not default Unity / Inter. |
| **7** | In-match HUD pieces | Scrubber track + playhead, stance/shoot chips, wound badge, phase label, Adrenaline primary — LA-like but for logiCard verbs. |
| **8** | Lobby / shell screens | Boot, Character Select, Map Select, Lobby, Match Over — full-screen or panel layouts to echo. |
| **9** | Motion / feel | Short refs or code for open/close, card arm, carousel/crossfade, button press (soft + physical, not flashy). |
| **10** | Implementation code | uGUI / UI Toolkit snippets, shaders, 9-slice helpers, layout scrapers — tag as *layout / chrome / motion / input*. |
| **11** | Audio (optional) | Paper/wood UI clicks, soft open/close — only if human cares this wave. |
| **12** | License / provenance | Source + license (CC0 / paid / owned) for **every** asset or borrowed code. Unknowns are not usable. |

## Special / held (not default chrome)

Items the human wants kept for a possible later role (event card, rare reward, Time Card flourish, etc.) — **do not** treat as the lobby+HUD family until explicitly promoted.

| Id | Path | Intent |
|----|------|--------|
| `special-holographic-ticket` | [`ui-collection/special-holographic-ticket-card.css`](ui-collection/special-holographic-ticket-card.css) + [`.png`](ui-collection/special-holographic-ticket-card.png) | Perforated holographic ticket card; parked. Role TBD. |

## Catalog log

Append one row per human delivery. Do not delete rejected items — mark `Rejected` with a one-line why.

| Date | Source / link / path | Bucket(s) | Notes | License | Status |
|------|----------------------|-----------|-------|---------|--------|
| 2026-08-13 | Human CSS paste + `logiCard-cards-collection/screenshots/image copy 13.png` → `docs/ui-collection/special-holographic-ticket-card.{css,png}` | **10** (impl: CSS mask perforations, conic holographic foil, hover float), **9** (motion), **2** (ticket silhouette as panel shape — special only) | Keep as **special card**, not default chrome. Techniques: SVG `filter` bump, `mask-composite` notches/perfs, layered blend modes for foil. Fonts in sample (Inter/Impact) are demo-only — not adopted for product type (**6** still open). | **OPEN** — original author/URL not given | **Held** — special/parked |

## Stop collecting (minimum to build)

Enough of **2 + 3 + 5 + 6 + 7 + 8** that lobby and HUD can share one chrome family, and **12** clear on each item.  
**1** and **9** sharpen taste; **4 / 10 / 11** fill gaps as they arrive.

When that bar is met, UI seat writes **“Collection complete for first chrome pass”** here and in `docs/departments/ui/STATUS.md`, then waits for human/Integrator brief before implementing.

## See also

- [`UI_STACK_COMPARISON.md`](UI_STACK_COMPARISON.md) — stack recommendation (uGUI backbone; Toolkit parked). Chrome art is a separate decision tracked **here**.
- [`UI_FLOW.md`](UI_FLOW.md) — screen map.
- [`ART_DIRECTION.md`](ART_DIRECTION.md) §4 — Desk-Lamp / cardstock notes (may be amended once this collection locks).
- Character seat `Assets/_Project/Art/UI/THIRD_PARTY.md` — prior Kenney Adventure import provenance only.
