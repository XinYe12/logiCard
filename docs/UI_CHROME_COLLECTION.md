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
| `glass-effect-card` | [`ui-collection/glass-effect-card.css`](ui-collection/glass-effect-card.css) | Frosted glass panel (`backdrop-filter`); parked. Role TBD — may suit AR scrubber/overlay contrast more than wood/parchment cards. |
| `special-card-logo-reveal` | [`ui-collection/special-card-logo-reveal.css`](ui-collection/special-card-logo-reveal.css) | Gold-on-charcoal logo expand + border unfold + letter-spacing reveal on hover; parked. Role TBD. |
| `wallet-card-holder` | [`ui-collection/wallet-card-holder.html`](ui-collection/wallet-card-holder.html) + [`.css`](ui-collection/wallet-card-holder.css) | Stacked cards in a leather-ish pocket; wallet hover fans cards + reveals balance; per-card hover brings one forward. Parked — strong **hand / deck** motion reference for gear cards. |
| `loader-rotating-squares` | [`ui-collection/loader-rotating-squares.css`](ui-collection/loader-rotating-squares.css) | Diamond-rotated staggered square loader; parked. Role TBD — Waiting / Simulating / matchmaking. |
| `hands-deck-comic-swatches` | [`ui-collection/hands-deck-comic-swatches.html`](ui-collection/hands-deck-comic-swatches.html) + [`.css`](ui-collection/hands-deck-comic-swatches.css) | Overlapping strip with hover lift + neighbor fan-scale + hard comic shadow; parked as **hands/deck UI concept** for gear hand. |
| `button-bubbles-fill` | [`ui-collection/button-bubbles-fill.css`](ui-collection/button-bubbles-fill.css) | Outlined button; two circles scale in from corners on hover to flood-fill; parked. Role TBD — Lock In / Confirm motion. |
| `button-glass-pill` | [`ui-collection/button-glass-pill.html`](ui-collection/button-glass-pill.html) + [`.css`](ui-collection/button-glass-pill.css) | Soft glass pill + separate blurred shadow + press tilt; parked. Role TBD — shell primary (Confirm) more than chunky Lock In. |
| `button-gradient-pill` | [`ui-collection/button-gradient-pill.css`](ui-collection/button-gradient-pill.css) | Warm coral→red gradient pill; hover drops into its shadow; parked. Simple physical press feel. |
| `font-iomanoid` | [`ui-collection/fonts/iomanoid/`](ui-collection/fonts/iomanoid/) | Display font family (base + Front/Back/Shine layers); **first real bucket-6 hit**. Not imported to Unity yet. |
| `resource-bank-card-flip` | [`ui-collection/resource-bank-card-flip.html`](ui-collection/resource-bank-card-flip.html) + [`.css`](ui-collection/resource-bank-card-flip.css) | Gold bank-card front/back + `rotateY` flip. Human intent: **resource card** face language (not default lobby chrome). |

## Icons inventory (bucket 5 → future card faces)

| Id | Path | Notes |
|----|------|-------|
| `icon_bandage` | [`ui-collection/icons/icon_bandage.png`](ui-collection/icons/icon_bandage.png) | **Style lock.** Cream roll + red cross, soft clay 3D. Match all later Gemini icons to this. |

## Catalog log

Append one row per human delivery. Do not delete rejected items — mark `Rejected` with a one-line why.

| Date | Source / link / path | Bucket(s) | Notes | License | Status |
|------|----------------------|-----------|-------|---------|--------|
| 2026-08-13 | Human CSS paste + `logiCard-cards-collection/screenshots/image copy 13.png` → `docs/ui-collection/special-holographic-ticket-card.{css,png}` | **10** (impl: CSS mask perforations, conic holographic foil, hover float), **9** (motion), **2** (ticket silhouette as panel shape — special only) | Keep as **special card**, not default chrome. Techniques: SVG `filter` bump, `mask-composite` notches/perfs, layered blend modes for foil. Fonts in sample (Inter/Impact) are demo-only — not adopted for product type (**6** still open). | **OPEN** — original author/URL not given | **Held** — special/parked |
| 2026-08-13 | [Uiverse.io by joe-watson-sbf](https://uiverse.io) — glass effect cards → `docs/ui-collection/glass-effect-card.css` | **10** (impl: `backdrop-filter` blur, translucent fill, warm hover glow), **2** (panel chrome technique — glass, not wood), **9** (hover border/glow) | Human label: “glass effect cards.” Held — not default LA toy/wood family. Unity port note: uGUI has no CSS backdrop-filter; would need URP/fullscreen blur or fake frosted sprite if ever promoted. | **MIT** (Uiverse); see license note below | **Held** — special/parked |
| 2026-08-13 | Human CSS paste → `docs/ui-collection/special-card-logo-reveal.css` | **9** (motion: hover scale, border rotate→settle, logo width wipe, tracking reveal, trail gradient), **10** (impl), **2** (dark panel + gold rim — palette sample only) | Human label: “special card animation.” Warm gold `#bd9f67` on charcoal `#243137` is desk-lamp-adjacent; still **Held** until a role is assigned (brand splash / Character Select hover / rare card). SVG logo markup not included in paste — animation shell only. | **OPEN** — original author/URL not given | **Held** — special/parked |
| 2026-08-13 | [Uiverse.io by byllzz](https://uiverse.io) — wallet card holder → `docs/ui-collection/wallet-card-holder.{html,css}` | **9** (stack fan-out, slide-into-pocket entry, dual hover layers), **4** (hand/deck presentation), **10** (z-index stack, cubic-bezier lift), **8** (possible lobby “your cards” metaphor) | Human label: “card holder wallet.” Best collection hit so far for **gear-hand / deck** feel. Demo uses Stripe/Wise/PayPal **brand marks** — strip those if ever shipped (trademark ≠ MIT CSS). | **MIT** (Uiverse / galaxy collection); credit byllzz + Uiverse appreciated | **Held** — special/parked |
| 2026-08-13 | [Uiverse.io by ZacharyCrespin](https://uiverse.io) — loaders → `docs/ui-collection/loader-rotating-squares.css` | **9** (looping keyframe motion), **10** (staggered `animation-delay` on 8 squares), **8** (shell wait states) | Human label: “loaders.” Geometric tile dance — closer to puzzle/toy than spinner ring. Needs HTML of 8× `.loader-square` inside `.loader` if ever ported (markup not in paste). Retint off pure white toward cardstock/ink when promoted. | **MIT** (Uiverse); credit ZacharyCrespin + Uiverse appreciated | **Held** — special/parked |
| 2026-08-13 | [Uiverse.io by chase2k25](https://uiverse.io) — comic color strip → `docs/ui-collection/hands-deck-comic-swatches.{html,css}` | **4** (hand/deck strip), **9** (hover scale + neighbor `:has`/sibling fan), **3** (button press: shadow inset on active), **2** (comic panel: thick black border + offset shadow), **1** (cream `#f0e8d8` + Bangers — toy/comic ref, not locked type) | Human label: “hands deck UI concept.” Best **gear-hand strip** motion hit alongside wallet stack. Demo is color swatches — replace faces with gear cards; keep overlap/fan/tooltip pattern. Unity: emulate neighbor fan in C# (no CSS `:has`). | **MIT** (Uiverse); credit chase2k25 + Uiverse appreciated | **Held** — special/parked |
| 2026-08-13 | [Uiverse.io by nikk7007](https://uiverse.io) — bubbles button → `docs/ui-collection/button-bubbles-fill.css` | **3** (primary button chrome), **9** (corner-circle flood fill + press scale), **10** (pseudo-element expand) | Hover flood-fill button. Demo purple `#8685ef` — retint to ink/gold if promoted. Markup needs `.bubbles` wrapping `.text` / `span` (paste CSS expects both). | **MIT** (Uiverse); credit nikk7007 + Uiverse appreciated | **Held** — special/parked |
| 2026-08-13 | [Uiverse.io by shokat_2650](https://uiverse.io) — glass pill Generate → `docs/ui-collection/button-glass-pill.{html,css}` | **3** (button), **9** (hover compress, press rotate3d, sheen slide), **10** (`@property` angles, mask shadow, backdrop-filter), **1** (dotted-grid SVG backdrop sample) | Human label: “buttons.” High-polish glass CTA — closer to modern web than LA wood toy; keep for shell Confirm feel or reject later. Inter in sample not adopted (**6** still open). Unity port = approximate only. | **MIT** (Uiverse); credit shokat_2650 + Uiverse appreciated | **Held** — special/parked |
| 2026-08-13 | [Uiverse.io by Codecite](https://uiverse.io) — gradient pill → `docs/ui-collection/button-gradient-pill.css` | **3** (button), **9** (hover translateY into shadow, active opacity) | Human label: “button.” Soft “physical switch” press — closest simple motion to Lock In snap among button holds. Warm coral/red gradient; retint toward ink/gold if promoted. Dosis font in sample not adopted (**6**). Easy Unity port (color + shadow + UiMotion). | **MIT** (Uiverse); credit Codecite + Uiverse appreciated | **Held** — special/parked |
| 2026-08-13 | `c:\Users\Xinye\Downloads\iomanoid.zip` → `docs/ui-collection/fonts/iomanoid/` | **6** (type/fonts) | Human label: “font.” **Iomanoid** by Raymond Larabie — layered display OTFs (`Iomanoid`, `Front`, `Back`, `Shine`) for toy/arcade headline read. Strong candidate for shell titles / card names; may still want a separate body face for HUD density. Not wired into `UiFactory` yet. | **CC0 1.0** (`license.txt`) — commercial OK | **Collected** — bucket 6 open for body/companion faces |
| 2026-08-13 | `c:\Users\Xinye\Downloads\Gemini_Generated_Image_ygyuguygyuguygyu.png` → `docs/ui-collection/icons/icon_bandage.png` | **5** (icons), future card face | Human: “this is the bandage.” Soft clay 3D roll + unrolled tab + embossed red cross; transparent checkerboard in source. **Style lock for remaining Gemini icons** — match this camera, lighting, material. Also a strong silhouette reference for a future Bandage **3D prop** (separate track). Not imported to Unity `Assets/` yet (~5.5MB — may want a downscaled 256/512 HUD variant later). | Human / Gemini-generated — confirm commercial rights for Gemini outputs before ship | **Collected** — first of set |
| 2026-08-13 | [Uiverse.io by adamgiebl](https://uiverse.io) — plain elevated card → `docs/ui-collection/normal-card.css` | **2** (default panel/card face + soft drop + inset bottom edge), **10** (impl: layered `box-shadow` = lift + contact shadow + inset lip) | Human label: **“normal cards.”** First delivery tagged as default chrome (not special/held). Cool grey `rgb(236,236,236)` face — retint toward warm cardstock/cream when promoted so it matches Desk-Lamp paper. Unity port: stacked shadow Image under face + thin darker strip or inset-colored bottom edge (no CSS box-shadow). | **MIT** (Uiverse); credit adamgiebl + Uiverse appreciated | **Candidate** — default card |
| 2026-08-13 | [Uiverse.io by VassoD](https://uiverse.io) — gold flip credit card → `docs/ui-collection/resource-bank-card-flip.{html,css}` | **9** (Y-flip reveal), **2** (dual-face card chrome — gold gradient + mag stripe back), **4** (resource/gear card presentation), **10** (front/back hierarchy, `preserve-3d` / backface) | Human intent: **bank card = resource card** in-game. Keep flip + gold dual-face pattern; replace Mastercard-like marks, chip bitmap, and fake PAN/CVC with logiCard fiction before ship. Unity: two Image faces under a pivot; animate `localEulerAngles.y` 0→180 (or scale.x squash flip if we stay strictly flat). Pairs with wallet / comic-hand for draw/hand motion. | **MIT** (Uiverse); credit VassoD + Uiverse appreciated. Demo network/chip art ≠ cleared brands. | **Held** — resource-card role |

## License note — Uiverse.io

Uiverse elements (via [uiverse-io/galaxy](https://github.com/uiverse-io/galaxy)) are **MIT**: free for personal **and commercial** use, modify/ship OK. Keep the MIT copyright notice in a third-party attributions file when shipping substantial copies. Attribution to the author + Uiverse is requested by the community but not required by MIT.  
**Separate caution:** demo content that includes real brand names/logos (Stripe, PayPal, etc.) is **not** cleared by MIT — replace with logiCard fiction before any player-facing ship. This is not legal advice; if a component page shows a different license, that page wins.

## Coverage gap (push checklist)

| Bucket | Status | What’s in hand | Still need from human |
|--------|--------|----------------|------------------------|
| **1** refs | Thin | Comic cream + Bangers vibe from hand-strip; no LA screenshot pack | Target lobby + HUD screens/clips |
| **2** panels | Partial | `normal-card` candidate; comic panel border; resource-bank (held role) | Warmer wood/parchment 9-slice or approve retint of normal-card |
| **3** buttons | Partial | Bubbles / glass pill / gradient pill (motion refs) | One family matching chosen panels (primary + secondary + disabled) |
| **4** wells | Partial | Wallet + comic hand = deck motion, not empty/selected/locked wells | Slot/well states if inventory/gear needs them |
| **5** icons | **Started** | `icon_bandage.png` (sets clay/toy family) | Still need: Interact, Flashbang, Adrenaline, stance×3, Snap/Hold, door, wound, Lock In — **same style as bandage**. **Later use:** icons can skin gear-hand / resource **card faces** under `Assets/`. Physical props are a **separate 3D art track**. |
| **6** type | Partial | **Iomanoid** CC0 display (Front/Back/Shine) | Body/UI face for dense HUD; confirm Iomanoid for titles |
| **7** HUD | **Empty** | — | Scrubber / chips / wound badge / Adrenaline primary in target look |
| **8** lobby | Thin | Wallet as “your cards” metaphor only | Boot / Char / Map / Lobby / Match Over layout refs |
| **9** motion | Strong | Wallet fan, comic neighbor-scale, button presses, flips, loader | — optional more |
| **10** code | Strong | Many Uiverse CSS/HTML samples | — optional Unity scraps |
| **11** audio | Empty | — | Optional |
| **12** license | Mixed | MIT (Uiverse) + CC0 (Iomanoid); some OPEN pastes | Source URLs for OPEN items if we promote them |

## Stop collecting (minimum to build)

Enough of **2 + 3 + 5 + 6 + 7 + 8** that lobby and HUD can share one chrome family, and **12** clear on each item.  
**1** and **9** sharpen taste; **4 / 10 / 11** fill gaps as they arrive.

**Not complete yet** — **5** and **7** are empty; **8** and body type still thin. Keep collecting.

When that bar is met, UI seat writes **“Collection complete for first chrome pass”** here and in `docs/departments/ui/STATUS.md`, then waits for human/Integrator brief before implementing.

## See also

- [`UI_STACK_COMPARISON.md`](UI_STACK_COMPARISON.md) — stack recommendation (uGUI backbone; Toolkit parked). Chrome art is a separate decision tracked **here**.
- [`UI_FLOW.md`](UI_FLOW.md) — screen map.
- [`ART_DIRECTION.md`](ART_DIRECTION.md) §4 — Desk-Lamp / cardstock notes (may be amended once this collection locks).
- Character seat `Assets/_Project/Art/UI/THIRD_PARTY.md` — prior Kenney Adventure import provenance only.
