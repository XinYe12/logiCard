# Card System — C64 OPEN Decision Menu

**Status:** **Answered 2026-08-14** — human confirmed plan defaults (Q1–Q8). **C66** on `master`. Follow-on: [`DECKBUILDER_SYSTEMS_BRIEF.md`](DECKBUILDER_SYSTEMS_BRIEF.md).  
**Depends on:** [`PRODUCT_MEMORY.md`](../core/PRODUCT_MEMORY.md) **C64**, **C66**, **C47**, **C62**/**C63**; [`CARD_SYSTEM_MODEL_COMPARISON.md`](CARD_SYSTEM_MODEL_COMPARISON.md); [`CARD_COLLECTION.md`](CARD_COLLECTION.md).

C64 locked the **hybrid** model (signature cards + shared deckbuilding library). These answers size the deckbuilder / in-match hand layer.

---

## Already locked (do not re-litigate)

| Fact | Source |
|------|--------|
| Hybrid: signature + personal deck from shared library | **C64** |
| Players may bring different cards in one match | **C64** |
| Signature = unique verb **armed by** playing a card | **C64** |
| Deck + hand hidden; Character pick public | **C64** |
| Entire gameplay library + signatures free forever | **C64** / **C47** |
| First-wave staples (Bandage…) ship on **transitional full-hand** until deckbuilder lands | **C64** / **C63** |

---

## Answers (2026-08-14)

| Q | Topic | Answer |
|---|--------|--------|
| **Q1** | Deck size | **(a)** Small — **5–8** cards |
| **Q2** | Copies per library card | **(b)** **2** max |
| **Q3** | In-match access each Program | **(a)** Always-have constructed hand (charges still limit spend); no draw RNG |
| **Q4** | Signature-in-hand | **(c)** Signature is **extra** — outside deck size cap, always available |
| **Q5** | Signature TR cost | **(a)** Costs TR like other Program gear (per-signature numbers OPEN) |
| **Q6** | Reveal of played cards | **(a)** At **Reveal** with the rest of the program |
| **Q7** | Attack/Defend vs decks | **(a)** **No interaction** — labels = spawns / who Allots; decks unconstrained |
| **Q8** | First-wave four | **(b)** All **shared-library**; Adrenaline stays special **Execute** slot |

---

## Decision menu (historical — answered above)

**Q1 — Deck size (constructed list brought to a match)**  
- **(a) Small — e.g. 5–8** ← chosen  
- (b) Medium — e.g. 10–15  
- (c) Larger — e.g. 20+  
- (d) Other

**Q2 — Copies of the same library card in one deck**  
- (a) 1 max  
- **(b) 2 max** ← chosen  
- (c) 3 max  
- (d) Other

**Q3 — In-match access each Program (after deckbuilder ships)**  
- **(a) Always-have constructed hand** ← chosen  
- (b) Draw N each Program / round  
- (c) Draw once at match start  
- (d) Other

**Q4 — Signature card availability**  
- (a) Always in hand (not drawn; not optional)  
- (b) Must include exactly one in the deck  
- **(c) Extra outside deck size cap, always available** ← chosen  
- (d) Other

**Q5 — Signature Time Resource cost**  
- **(a) Costs TR like other Program gear** ← chosen  
- (b) Charge-only / 0 TR  
- (c) Mix  
- (d) Defer until Bomber/Time Player briefs close  

**Q6 — When does the opponent see a played card?**  
- **(a) At Reveal** ← chosen  
- (b) On place during Program  
- (c) Only during Playback  
- (d) Other

**Q7 — Attacker / Defender labels vs decks**  
- **(a) No interaction** ← chosen  
- (b) Soft role staples  
- (c) Hard role-locked library subsets  

**Q8 — First-wave four under C64**  
- (a) All shared-library  
- **(b) All shared-library; Adrenaline stays special Execute slot** ← chosen  
- (c) Other split  

---

## C66 — promoted on master

Human confirmed 2026-08-14 via plan defaults on this menu. **C66** lives in [`PRODUCT_MEMORY.md`](../core/PRODUCT_MEMORY.md) on `master` (Integrator assigned the number; Cards had drafted it as C65 before Map claimed C65).

**C66:** Deckbuilder sizing + hand/Reveal rules (closes C64's parked OPENs; sits beside **C64**, **C62**, **C47**).

Proposed substance:

1. **Deck size:** personal constructed library deck is **5–8** cards (exact count within range may be fixed later in builder UI; range is binding).
2. **Copies:** at most **2** of the same shared-library card in one deck. Signatures are not library copies.
3. **In-match access:** after deckbuilder ships, every card in the constructed deck is **always available each Program** (charges still limit spend). **No draw RNG.** Transitional full-hand staples (**C62**/**C63**) unchanged until that layer lands.
4. **Signature availability:** Character signature is **extra** — outside the 5–8 deck-size cap — and **always available** when that Character is picked (not optional in builder, not drawn).
5. **Signature TR:** signatures **cost Time Resource** like other Program gear; per-signature magnitudes stay OPEN until Bomber/Time Player briefs close.
6. **Reveal:** opponent sees a **played** card at **Reveal** with the rest of the program; hand stays hidden through Program.
7. **Attacker/Defender:** **no** interaction with deckbuilding — labels remain spawns + Allot chooser only.
8. **First-wave four:** Bandage / Flashbang / Interact-as-card / Adrenaline are **shared-library** cards (none are signatures). **Adrenaline** remains a special **Execute-only** slot and may bypass normal deck rules (aligns **C62** / PLAYBACK_CONTRACT).
9. **Does not** greenlight deckbuilder UI, Flashbang numerics, or Bomber/Time Player active build.

**Still OPEN after C66 (suggested):** exact fixed deck count inside 5–8; per-signature TR numbers; Flashbang/Interact numerics (OPEN #16); Adrenaline real effect. Tracked for build in [`DECKBUILDER_SYSTEMS_BRIEF.md`](DECKBUILDER_SYSTEMS_BRIEF.md) §6.

---

## Explicit non-goals

- Does not greenlight Flashbang effect shape (brief stays paused; re-derive as library tech later).  
- Does not start deckbuilder UI or Sim code (systems brief is the next doc, not a contract).  
- Does not promote Bomber/Time Player into active build.  
- Does not touch Bandage HUD (UI seat).

---

## See also

- [`CARD_COLLECTION.md`](CARD_COLLECTION.md) — catalog + C64 hybrid + C62 transitional  
- [`CARD_SYSTEM_MODEL_COMPARISON.md`](CARD_SYSTEM_MODEL_COMPARISON.md) — hybrid conversation record  
- [`DECKBUILDER_SYSTEMS_BRIEF.md`](DECKBUILDER_SYSTEMS_BRIEF.md) — post-C66 systems shape  
- [`../departments/cards/STATUS.md`](../departments/cards/STATUS.md)
