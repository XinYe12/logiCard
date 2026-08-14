# Card System — C64 OPEN Decision Menu

**Status:** Live one-page menu for human answers. Docs-only. Does **not** amend PRODUCT_MEMORY until Integrator promotes.  
**Date:** 2026-08-13 (links refreshed 2026-08-14)  
**Depends on:** [`PRODUCT_MEMORY.md`](../core/PRODUCT_MEMORY.md) **C64**, **C47**, **C62**/**C63**; [`CARD_SYSTEM_MODEL_COMPARISON.md`](CARD_SYSTEM_MODEL_COMPARISON.md); [`CARD_COLLECTION.md`](CARD_COLLECTION.md).

C64 locked the **hybrid** model (signature cards + shared deckbuilding library). These questions size the deckbuilder / in-match hand layer. Reply with letters / short notes.

---

## Already locked (do not re-litigate)

| Fact | Source |
|------|--------|
| Hybrid: signature + personal deck from shared library | **C64** |
| Players may bring different cards in one match | **C64** / Q3-a |
| Signature = unique verb **armed by** playing a card | **C64** |
| Deck + hand hidden; Character pick public | **C64** |
| Entire gameplay library + signatures free forever | **C64** / **C47** |
| First-wave staples (Bandage…) ship on **transitional full-hand** until deckbuilder lands | **C64** / **C63** |

---

## Decision menu

**Q1 — Deck size (constructed list brought to a match)**  
- (a) Small — e.g. **5–8** cards (tight, readable, fast builder)  
- (b) Medium — e.g. **10–15**  
- (c) Larger — e.g. **20+** (Hearthstone-ish; heavier UI/teach)  
- (d) Other number / range: ___

**Q2 — Copies of the same library card in one deck**  
- (a) **1** max (collection of uniques)  
- (b) **2** max  
- (c) **3** max  
- (d) Other: ___

**Q3 — In-match access each Program (after deckbuilder ships)**  
- (a) **Always-have constructed hand** — every card in your deck is available each Program (charges still limit spend); no draw RNG  
- (b) **Draw N** from deck into hand each Program / round; rest stay in deck  
- (c) **Draw once at match start**, then that hand persists across rounds (charges persist)  
- (d) Other: ___

**Q4 — Signature card availability (signature-in-hand)**  
- (a) **Always in hand** when you pick that Character (not drawn; not optional in builder)  
- (b) **Must include** exactly one signature in the deck; drawn/accessed like other cards per Q3  
- (c) Signature is **extra** — outside deck size cap, always available  
- (d) Other: ___

**Q5 — Signature Time Resource cost**  
- (a) Costs TR like other Program gear (number OPEN per signature)  
- (b) **Charge-only** / once-per-match, **0 TR** (Adrenaline-adjacent)  
- (c) Mix — some signatures cost TR, some don’t (state rule)  
- (d) Defer until Bomber/Time Player briefs close  

**Q6 — Reveal — when does the opponent see a played card?**  
- (a) At **Reveal** (with the rest of the program) — hand stays hidden through Program  
- (b) **On place** during Program (armed card becomes public immediately)  
- (c) Only during **Playback** when the tape event fires  
- (d) Other: ___

**Q7 — Attacker / Defender labels vs decks**  
- (a) **No interaction** — labels stay spawns/who Allots first; decks unconstrained by role  
- (b) Soft role staples recommended but not enforced  
- (c) Hard role-locked library subsets (would need a new C# — fights current C18 leftover spirit)  

**Q8 — First-wave four under C64 (Bandage / Flashbang / Interact / Adrenaline)**  
- (a) All are **shared-library** cards (deck tech); none are signatures  
- (b) Same as (a), but Adrenaline stays a special Execute slot (may bypass normal deck rules)  
- (c) Other split: ___  

---

## Explicit non-goals

- Does not greenlight Flashbang effect shape (brief stays paused; re-derive as library tech later).  
- Does not start deckbuilder UI or Sim code.  
- Does not promote Bomber/Time Player into active build.  
- Does not touch Bandage HUD (UI seat).

---

## See also

- [`CARD_COLLECTION.md`](CARD_COLLECTION.md) — catalog + C64 hybrid + C62 transitional  
- [`CARD_SYSTEM_MODEL_COMPARISON.md`](CARD_SYSTEM_MODEL_COMPARISON.md) — hybrid conversation record  
- [`../departments/cards/STATUS.md`](../departments/cards/STATUS.md)
