# Deckbuilder Systems Brief (C64 / C66)

**Status:** §6 closed → **C67** written into `PRODUCT_MEMORY.md` on this branch (2026-08-14). Catalog vocab synced. Integrator merge to master still needed. **Docs only** — no UI/Sim contract opened.

---

## 0. Design correction (read first)

Cards dept previously modeled:
- **Character** = pre-match pick (public), **not** a card inside the constructed deck  
- **Deck** = gear/library list only (5–8), signature extra outside  

**Human correction 2026-08-14 → C67:** that split is wrong for the intended design.
- **Each Character has a deck of 8 cards** to play in the game  
- **Characters are part of the card/deck system** (not a parallel non-card identity bolted onto a gear-only list)

Working model (Q1b **(c)**):
- Character = first-class card/identity in collection + deckbuilder  
- **Each Character fields exactly one 8-card play deck** for the match  
- Those 8 are what get armed into the **timeline** as that Character's behavior (GDD)

Save-file truth: **C67** in [`PRODUCT_MEMORY.md`](../core/PRODUCT_MEMORY.md) on this branch.

---

## 1. What's already locked (do not re-litigate)

| Fact | Source |
|------|--------|
| Hybrid long-term direction (signatures + shared library; personal decks may differ) | **C64** (detail may be amended by Character-in-deck correction) |
| Deck + hand **hidden** from opponent through Program | **C64** |
| Library + signatures **free forever** | **C64** / **C47** |
| In-match = **always-have** constructed hand; **no draw RNG** | **C66** |
| Attack/Defend labels do **not** constrain decks | **C66** |
| First-wave staples = shared-library cards; shipping = transitional full-hand until cutover | **C62**/**C63**/**C64** |
| Up to **10** saved decks per profile | Human §6 Q3 |
| Scout / Juggernaut are **preliminary** — no signature work for them | Human §6 Q4 |
| **Everything is a card** (incl. Adrenaline in the card/deck model) | Human §6 Q5 |
| Illegal loadouts rejected by **Host / resolve-relay** | Human §6 Q6 → **C52** |
| Played cards → **timeline / Character behavior** (GDD); not a separate Reveal-card question | Human §6 Q7 struck |
| Bring-in size = **8 cards per Character**; Characters are in the card/deck system; **each Character has an 8-card play deck** | Human 2026-08-14 (Q1 + Q1b-c) |

**Still OPEN (not blocking this brief's product lock):** per-signature TR; Flashbang/Interact numerics; Adrenaline real effect; Bomber/Time Player; Integrator C# to align C66/C64 with this model; 1v1 field count (working assumption: **one Character per side** unless UI/GDD says otherwise — human earlier "select characters → game" is UI flow).

---

## 2. Two horizons (do not collapse them)

| Horizon | What the player sees | Code implication |
|---------|----------------------|------------------|
| **Shipping / transitional** | Same staple hand for both sides (Bandage…) + charges | Keep `GearHandView` hard-coded staple list + Bandage HUD path. **Do not** invent deck RNG or loadout gates for Bandage. |
| **Long-term (this brief)** | **Each Character has an 8-card play deck**; Characters first-class in card/deck system → always-have hand; played cards → timeline | Loadout = per-Character 8. Staple resolve verbs stay; packaging changes. |

Transition rule: deckbuilder ships as a **cutover**, not a soft blend. Until cutover, transitional full-hand remains truth. After cutover, the **8-card-per-Character deck (Characters included)** is truth; hard-coded staple list retired.

---

## 3. Screen / loop placement — **UI owns this**

Human (2026-08-14): for 1v1, **select characters → move on to the game**. Screen chrome is **UI**.

Cards product fact (updated): before Program, each side has a **frozen deck loadout** of **8 cards per Character**, and that deck **includes Character card(s)** — not a gear-only list bolted onto a separate non-card Character pick. Exact UI for building/picking that deck is UI's job.

---

## 4. Data model (proposed — not frozen)

### 4.1 Catalog (shared library + signatures)

Extend beyond today's four `CardId`s without breaking Bandage:

| Concept | Shape | Notes |
|---------|-------|-------|
| **Character card** | First-class card/identity in collection + deckbuilder | Human: Characters are in the card/deck system. |
| **Library / gear card** | Bandage, Flashbang, etc. | Shared library; ≤2 copies still from C66 unless C# amends. |
| **Signature card** | Character-unique verb armed by a card | Later roster only; **not** Scout/Jug. Slot vs inside-8 TBD at signature design. |
| **Play deck** | **Exactly 8 cards per Character** for the match | Human Q1b-c: each Character has a deck of 8 to play. |
| **Saved decks** | Up to **10** named decks per profile | Human §6 Q3. |
| **Match loadout** | Per fielded Character: frozen 8-card play deck | Host/relay validates (**Q6**). |

### 4.2 In-match hand / timeline

- **Always-have:** cards in the loadout that are Program/Execute-legal are armable when charges + budget + phase allow.
- **No draw pile / discard RNG.**
- **Played cards** schedule into the **timeline** as that Character's behavior (GDD) — same family as Move/Shoot nodes, not a parallel Reveal gimmick.
- **Adrenaline:** a card in the deck model (human: everything is cards).

### 4.3 What already exists (read before assuming blank slate)

| Path | Relevance |
|------|-----------|
| `Assets/_Project/Cards/CardData.cs` + four `.asset`s | Pre-C62 scaffold; enum is the four staples only; no deck/loadout types. |
| `Assets/_Project/UI/GearHandView.cs` | Hard-coded staple hand UI — **transitional**. Long-term hand should be **data-driven from loadout**, not a second hard-coded list. |
| Bandage Sim (`ActionVerb.Bandage`, `BandageCharge`, `Healed`) | Pattern for per-match charge carry — reuse for other library charges later. |
| No `Deck` / `Loadout` / `Signature` types in Sim/Net yet | Greenfield for loadout serialization when a contract opens. |

---

## 5. Phased build order (Integrator-owned gates)

Do **not** start these until the prior gate is closed. Cards writes briefs; UI/Sim code only under frozen contracts.

| Phase | Deliverable | Owner seat | Gate |
|-------|-------------|------------|------|
| **A — now** | This brief complete (§6 closed) | **Cards** | — |
| **B** | Integrator C# amend (C66 → **8 per Character**; Character in card/deck system; Adrenaline-as-card) | Integrator + human | Human approve C# |
| **C** | Bandage HUD merged + Healed presenter | **UI** then Integrator | Existing Bandage contract |
| **D** | Loadout data + validation (EditMode): 8 per Character | Sim/Cards under contract | Phase B |
| **E** | Deckbuilder UI (Character + 8-card decks; up to 10 saved) | **UI** under contract | Phase D |
| **F** | Program arm from loadout → timeline (retire hard-coded staple list) | UI + Boot wire | Phase E |
| **G–H** | Later signatures (Bomber / Time Player only) | Character + Sim | C43/C44 |

**Explicit non-goals for Phase A (this doc):** no Unity scenes, no `GearHandView` rewrite, no Flashbang effect, no Bandage HUD edits, no Net/loadout RPCs.

---

## 6. Questions — answers + plain re-asks

### Locked (2026-08-14)

| Q | Topic | Answer |
|---|--------|--------|
| **Q1** | Bring-in size | **8 cards per Character.** |
| **Q1b** | Structure | **(c)** Each Character **has a deck of 8** to play in the game. Characters are first-class in the card/deck system (not a gear-only list after a non-card pick). |
| **Q2** | Flow / screen order | **UI owns.** Human: 1v1 = select Characters → enter the game. |
| **Q3** | Saved decks | **10** decks per profile. |
| **Q4** | Scout / Juggernaut signatures | **Forget them** — preliminary roster. |
| **Q5** | Adrenaline | **Everything is a card** — in the deck model. |
| **Q6** | Illegal loadout rejection | **Host / resolve-relay** validates (**C52**). |
| **Q7** | "Reveal card-flip UX" | **Struck — invalid.** Played cards → timeline behavior (GDD). |

### §6 complete

No further product questions from this brief. Next = Integrator C# amend + catalog vocab sync.

---

## 7. Contract sketch (for Integrator later — not opened)

Integrator should promote a C# that amends C66 (and C64 vocab as needed):

- **Each Character has an 8-card play deck**; Characters are first-class in the card/deck system.
- Up to **10** saved decks; Host/relay validates.
- Program arm → timeline nodes (GDD); everything is a card (incl. Adrenaline).
- Out of scope first contract: Flashbang resolve, Scout/Jug signatures, cosmetic binder.

---

## 8. Relationship to Bandage / staples

- **Bandage HUD (UI, in flight)** stays on transitional full-hand. This brief must not divert that seat.
- Cutover later: Bandage is one of the Character's 8, armed onto the timeline like other Program cards.
- Flashbang brief stays **paused**.

---

## 9. Next step

1. ~~Q1b~~ **done** → **C67** on this branch.  
2. Catalog vocab synced.  
3. Integrator: merge `feat/cards-collection-docs` when human approves.  
4. Phase D only when coding capacity allows.

---

## See also

- [`CARD_SYSTEM_OPENS.md`](CARD_SYSTEM_OPENS.md) — Q1–Q8 source for **C66** (sizing → **8 per Character** pending C# amend)  
- [`CARD_COLLECTION.md`](CARD_COLLECTION.md) — catalog; §3 Character vocab pending sync  
- [`CARD_SYSTEM_MODEL_COMPARISON.md`](CARD_SYSTEM_MODEL_COMPARISON.md) — hybrid conversation record  
- [`../departments/cards/STATUS.md`](../departments/cards/STATUS.md)
