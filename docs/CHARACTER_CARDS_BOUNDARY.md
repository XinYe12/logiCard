# Character ↔ Cards Boundary

**Status:** Concept draft 2026-08-13 — joint seam for Character + Cards depts. Not a Sim contract.
**Purpose:** Keep **Character Card / attrs / unique verbs** from blurring into **gear cards**, and name
the one place they *should* touch (Strength-scaled Interact).  
**Depends on:** [`PRODUCT_MEMORY.md`](PRODUCT_MEMORY.md) **C15**, **C18**, **C62**; [`CARD_COLLECTION.md`](CARD_COLLECTION.md)
§5 / §5A / §8; [`CHARACTER_FANTASY.md`](CHARACTER_FANTASY.md); [`CHARACTER_PLAN.md`](CHARACTER_PLAN.md).  
**Audience:** Character workers, Cards workers, Integrator reviewing cross-dept drift.

Cards seat: please treat this as a **review ask**, not a file grab — `CARD_COLLECTION.md` remains Cards'
catalog authority. Character will amend this boundary doc when roster fantasy changes.

---

## 1. Three different "cards" (glossary lock)

| Term | What it is | Dept truth |
|------|------------|------------|
| **Character Card** | Pre-match pick (Scout / Juggernaut today). Sets attrs; later may grant a unique verb. | **Character** |
| **Time Card** | Round allotment commit from the shared pool (**C33**). Not gear. | Match loop / Integrator; Cards may own *presentation as cardstock* |
| **Gear card** | In-hand Program/Execute tool (Bandage, Flashbang, …). Same legal list for every Character. | **Cards** |

If a pitch uses "card" without saying which row, reject it until classified.

---

## 2. The legality test (from C62 / CARD_COLLECTION §5A)

> If the capability exists in a match **regardless of which Character either player picked**, it's
> **gear**. If it exists **only because a specific Character was picked**, it's a **unique verb**
> (or a plain attr effect on shared verbs).

| | Gear | Attr effect | Unique verb |
|---|------|-------------|-------------|
| Example | Bandage | Juggernaut door 2s vs Scout 4s | Bomber attach/detonate |
| Access | Every Character | Every Character (numbers differ) | Only that Character pick |
| Hand | Yes | No | No |
| Monetization | Not P2W power sold | Not P2W | Must ship free / skill-gated |

**Rejected without a new C# amending C18:** Scout-only / Juggernaut-only gear kits.

---

## 3. What each dept owns on the seam

| Concern | Character | Cards | Shared / Integrator |
|---------|-----------|-------|---------------------|
| Speed / Agility / Strength meanings | **Owns** | Consumes Strength for Interact cost only | — |
| Unique-verb rules (C42–C44) | **Owns** | Must not catalog them as gear | Integrator contracts later |
| Gear catalog + charges + hand economy | — | **Owns** | — |
| Interact-as-card Strength scaling | Supplies attr hook + Door precedent | Supplies card cost formula + when stations exist | Freeze together when built |
| Bandage / Flashbang costs | — | Flat unless playtest reopens (**C62**) | — |
| Player-facing glossary ("Character vs Gear") | Fantasy blurbs | Collection / HUD gear copy | Keep one vocabulary |
| Character Select chrome | — | — | **UI** |

---

## 4. The intentional overlap — Interact × Strength

**C62 carve-out:** Interact-as-card Time Resource cost **may** scale by Strength, mirroring Door
open/close (GDD §6). Bandage / Flashbang stay flat.

### Locked direction

- Door contextual actions already use Character `doorInteractBaseSeconds` (Scout 4 / Jug 2 in assets).
- Interact-as-card is **future stations only** — not Door/Vent/Breach migration (**C62** §8 Q5).
- Scaling changes *cost*, never *legality* of holding the card.

### Still concept-open (joint parking lot)

| # | Question | Notes |
|---|----------|--------|
| J1 | Same numbers as Door, or a separate Interact curve? | Strawman: reuse Door seconds until playtest hurts |
| J2 | Does Agility ever scale a gear card? | **Default no** under C62; reopen only with human playtest note |
| J3 | When a station exists, who writes the first Interact brief? | Cards owns card; Character reviews Strength hook paragraph |
| J4 | HUD: show "because Juggernaut" on the cost chip? | UI; both depts agree copy must not imply exclusive gear |

### Proposed joint strawman (recommendation, not locked)

```text
Interact-as-card costSeconds = CharacterData.doorInteractBaseSeconds
  (or a named interactBaseSeconds if Door and station must diverge later)
```

No code. When stations approach ship, Cards opens an Interact brief that **quotes** this strawman;
Character countersigns the Strength paragraph.

---

## 5. Unique verbs — Cards must stay clear

Bomber / Time Player will look like "cards" to players if the HUD uses the same arm/place pattern as
Bandage. Boundary rules for future UX copy:

| Do | Don't |
|----|--------|
| Label mode **ability / verb** tied to Character name | Put Bomber charges in the gear hand strip |
| Keep gear hand identical for Bomber and Scout | Sell "Bomber pack = bomb card" |
| Let Cards catalog stay four-first-wave gear | Add `CardId.Bomb` as a shortcut around C42 |

Character implementation briefs already propose `ActionVerb` growth, not `CardData` rows — Cards
should reject PRs/docs that invert that.

---

## 6. Concept pack Cards might want from Character

When Cards is idle and wants "character details" for collection / glossary polish:

1. **`CHARACTER_FANTASY.md`** — one-line pitches + "not this" lines for Scout/Jug.
2. **This file** — glossary + Interact strawman.
3. **Attrs brief** — which numbers are real vs unwired (so gear copy doesn't claim Agility affects Bandage).

Cards should **not** wait on Bomber/Time Player numerics — those are long-term and not gear.

---

## 7. Review protocol (docs-only mode)

1. Character updates this file when roster fantasy or attr meanings change.
2. Cards comments / PRs against `CARD_COLLECTION.md` if catalog language drifts from §2's test.
3. Disagreement on "is it gear or verb?" → **Integrator + human** → PRODUCT_MEMORY C# if needed.
4. No parallel Sim work from either seat until Integrator contracts say so.

---

## 8. Explicit non-goals

- Does not open Bandage/Flashbang/Adrenaline design.
- Does not promote C42–C44.
- Does not give Cards ownership of `CharacterData.cs`.
- Does not give Character ownership of `CardData` / hand HUD.

---

## See also

- [`CARD_COLLECTION.md`](CARD_COLLECTION.md) §5, §5A, §8 — Cards authority
- [`CHARACTER_FANTASY.md`](CHARACTER_FANTASY.md)
- [`CHARACTER_ATTRS_SCOUT_JUGGERNAUT_BRIEF.md`](CHARACTER_ATTRS_SCOUT_JUGGERNAUT_BRIEF.md)
- [`CHARACTER_UNIQUE_VERB_OPERATORS_BRIEF.md`](CHARACTER_UNIQUE_VERB_OPERATORS_BRIEF.md)
- Cards worktree `docs/GEAR_BANDAGE_AGENT_BRIEF.md` — pattern for gear briefs
