# Card-System Model Comparison — Design Conversation

**Status:** Human answers closed 2026-08-13 — **promoted as C64** in `PRODUCT_MEMORY.md` (amends C18/C62).
§6A–§6D remain the conversation record; C64 is save-file truth.
**Date:** 2026-08-13  
**Worktree:** `logiCard-cards-collection` / `feat/cards-collection-docs`  
**Pauses:** Flashbang numerics / effect brief (`GEAR_FLASHBANG_AGENT_BRIEF.md`) — foundational model first.  
**Depends on:** [`CARD_COLLECTION.md`](CARD_COLLECTION.md) §5–§8; [`PRODUCT_MEMORY.md`](PRODUCT_MEMORY.md) **C15**, **C18**, **C42–C44**, **C47**, **C62**; [`CHARACTER_ROSTER_LONGTERM.md`](CHARACTER_ROSTER_LONGTERM.md); [`MONETIZATION.md`](MONETIZATION.md).

---

## 0. Why this doc exists

The human asked to design the **actual card-system model** before more gear-numerics work. Three families are on the table:

1. **Shared public pool** (三国杀-style) — one catalog / pile everyone draws from  
2. **Personal deckbuilding** (Hearthstone-style) — each player builds a private deck from a collection  
3. **Hybrid** — character-unique **signature** cards for identity + a shared Hearthstone-like library for deckbuilding freedom  

This file is a **comparison with tradeoffs** for a live conversation. It does **not** pick a winner. It **does** flag where option 3 collides with currently CONFIRMED product rules.

---

## 1. What is already CONFIRMED (today's baseline)

Do not treat the rows below as "stale defaults we can quietly rewrite." Changing them needs an explicit human call → Integrator C# row.

| ID | What it locks for cards / identity |
|----|-------------------------------------|
| **C15** | Move + Shoot = **base verbs**; cards = **gear**; Characters differ by attrs **and later unique verbs** |
| **C18** | Attacker/Defender labels + spawns; **same gear deck** (both sides / every Character — same legal gear list) |
| **C62** | First-wave catalog = Bandage / Interact-as-card / Flashbang / Adrenaline; **keeps C18**; unique-verb Characters stay **verbs, not exclusive gear packs**; economy = **full visible hand + charges** (no draw/RNG, no pre-match loadout for first wave); **no meta binder** this milestone |
| **C42–C44** | Long-term roster may add **unique verbs** (Bomber, Time Player) — schedulable event-stream only; free/skill-gated (**C47**), never paywalled power |
| **C47** | Cosmetic-only IAP; gameplay cards / unique verbs are **not** sellable power |

**What `CARD_COLLECTION.md` currently means by "same gear deck"** (§5 / §5A / §10):

> Every Character may use every gear card in the catalog. Character fantasy comes from Speed / Agility / Strength (**and later unique verbs**), **not** from exclusive Bandage-vs-Flashbang kits.

**In-match access today (C62):** not a shared draw pile and not a constructed deck — **full hand every Program**, spend charges. Closest analogy is "everyone owns the same four toys on the desk," not 三国杀's common deck and not Hearthstone's 30-card list.

**Gear vs unique verb (already spelled in §5A):**

| | Gear card | Unique Character verb (C42–C44) |
|---|---|---|
| Access | Every Character, every match (**C18**) | Only if you picked that Character |
| Example | Bandage, Flashbang | Bomber bomb-place, Time Player rewind |
| Doc home | `CARD_COLLECTION.md` | `CHARACTER_ROSTER_LONGTERM.md` |

**Early seed of "signature ability":** C42–C44 already reserved a **Character-only power slot** — but as a **verb**, not as a card in the hand. The hybrid model below would either (a) keep that slot as verbs and add *more* exclusives as cards, (b) **reclassify** signatures as cards, or (c) blur the boundary. That choice is part of the conversation, not assumed.

---

## 2. Three models — plain language

### Model A — Shared public pool (三国杀-flavored)

**Core idea:** There is **one** card universe (catalog and/or physical-style draw pile). Identity lives mainly on the **Character** (skills / unique verbs). Cards are a shared resource of the match (or a universal legal set), not a private constructed list.

| Variant | How it plays in logiCard terms |
|---------|--------------------------------|
| **A1. Universal legal set (≈ current C62)** | Same four gear always legal for both players; no draw pile; charges limit use. **This is what we have confirmed.** |
| **A2. Shared draw pile** | Match has a common deck; players draw N into hand each Program (or each round). RNG + hidden info enter the blind-program loop. |

三国杀 analogy maps cleanly to **Character skill + shared card pool**, not to "everyone starts with the same fixed hand forever." Today's C62 is **A1**, not classic 三国杀 draw (**A2**).

### Model B — Personal deckbuilding (Hearthstone-flavored)

**Core idea:** Each player owns a **collection**, builds a **deck** before the match, brings that private list into Program. Opponent does not share your list. Meta = collection + deck craft + matchup knowledge.

In logiCard terms this implies at least: collection UI, deckbuilder, pre-match loadout, and almost certainly **different gear legality per player** inside the same match — which already presses on **C18**'s "same gear deck" reading even before Character exclusives enter.

### Model C — Hybrid (signature + shared library)

**Core idea (as stated by human for discussion):**

- **Signature cards** — Character-unique, carry fantasy / identity  
- **Plus** a **shared Hearthstone-like library** — players deckbuild from a common pool for freedom  

This is the richest identity + expression model. It is also the one that **most clearly collides with CONFIRMED rules** — see §3.

---

## 3. ⚠ Explicit conflict flags (do not silently resolve)

### 3.1 Hybrid (and pure personal exclusives) vs **C18 / C62**

| Claim | Status |
|-------|--------|
| "Every Character has the same legal gear list" | **CONFIRMED** — C18 + C62 |
| "Character-unique signature **cards**" | **Contradicts** that rule if signatures are gear cards in the hand |
| "Personal deck from a library so two players can bring different lists" | **Presses / likely contradicts** C18's same-deck reading and C62's "no pre-match loadout for first wave" |

**Human call required** before any hybrid or Hearthstone-style model is treated as product truth. Options the Integrator would need, if chosen later (not recommending):

- Amend C18/C62 to allow Character-exclusive gear and/or divergent loadouts, **or**
- Keep C18 for **shared library cards** and put signatures only in the **unique-verb** slot (C42–C44) — hybrid identity without exclusive *cards*, **or**
- Reject hybrid / personal deckbuilding and stay on A1 (current)

Cards dept must **not** assume any of those. This paragraph is a flag, not a patch.

### 3.2 Unique verbs vs signature cards — same fantasy slot?

C42–C44 already seed "this Character can do a thing nobody else can." Turning that into **signature cards** is a **taxonomy change**, not a free rename:

| Keep signatures as **verbs** (current long-term plan) | Make signatures **cards** |
|---------------------------------------------------------|---------------------------|
| Fits C15 / C62 wording ("verbs, not exclusive gear packs") | Needs C18/C62 amendment (or a new carve-out) |
| No hand/economy for the signature | Signature spends charges / TR like gear |
| Roster doc owns Bomber / Time Player | Card catalog owns exclusives; roster still picks Character |
| Monetization: free gameplay verb (**C47**) | Same C47 pressure — exclusive card power still must not be paywalled |

Blurring "Bomber's bomb is a card in the shared library" vs "Bomber's bomb is a verb" without a decision will break §5A's gear-vs-verb test and confuse Sim/HUD ownership.

### 3.3 Blind-program pillar vs draw / hidden hands

VISION / core loop: simultaneous blind programming. **A2 shared draw** and **B/C constructed decks with private hands** add information asymmetry *before* Lock In. That can be a feature (card-game tension) or a fight with "I can read the board plan space." Flag for the conversation — not auto-reject, not auto-accept.

### 3.4 Monetization (**C47**)

Any model with **collection / deckbuilding** creates store pressure ("buy packs to complete the library"). Cosmetic binder for **skins** is already allowed; **gameplay card unlocks** were explicitly rejected in C62 §8 Q4. Hybrid/Hearthstone without a hard free-library rule drifts toward P2W-adjacent even if the letter of C47 is "cosmetic only."

---

## 4. Tradeoff matrix (conversation table)

| Axis | A1 Universal set (current C62) | A2 Shared draw pile | B Personal deckbuild | C Hybrid signatures + library |
|------|--------------------------------|---------------------|----------------------|-------------------------------|
| **Fits C18/C62 today?** | Yes | Mostly for *catalog*; draw economy reopens C62 economy | **No / needs amend** | **No / needs amend** (signatures as cards) |
| **Character identity** | Attrs + later unique **verbs** | Character skills + shared cards (三国杀 feel) | Deck is identity; Character can be secondary | Strongest fantasy split: signature + craft |
| **Blind-program clarity** | Highest — both know the legal set | RNG / unknown draws | Opponent deck unknown | Same as B + known signatures by roster |
| **Teachability** | Easiest (four toys) | Medium | Hard (collection + builder) | Hardest |
| **Balance surface** | Small catalog, charge gates | Draw variance + catalog | Matchups × decks × Characters | Signatures × library × decks |
| **UI / meta systems** | Hand strip + charges (in progress) | Draw / discard / pile UI | Collection + deckbuilder + loadout | All of B + signature slot UX |
| **Monetization risk** | Low (skins only) | Medium if pile expands via packs | High without free-complete library | High — signature power must stay free |
| **Reuse of C42–C44 seed** | Verbs stay verbs | Skills ≈ verbs; cards shared | Weak unless Character still matters | Must decide: signature = verb **or** card |
| **Ship cost vs now** | Lowest — already aimed here | Medium | High | Highest |

---

## 5. How each model would reinterpret the first-wave four

Not proposals — sanity checks for the talk.

| Card | Under A1 (current) | Under B / C library | Under C as signature? |
|------|--------------------|---------------------|------------------------|
| Bandage | Everyone, 1×/match (C63) | Staple include or tech choice | Unlikely signature — heal is generic |
| Flashbang | Everyone, numerics OPEN | Classic flex / tech card | Possible Scout-flavored exclusive — **would break C18** |
| Interact-as-card | Everyone when stations exist; Strength may scale cost | Station tech | Possible Juggernaut-flavored exclusive — **would break C18** |
| Adrenaline | Everyone, Execute stub | High-skill include | Possible signature — **would break C18** if exclusive |

Bomber bomb / Time Player rewind are **not** in the first-wave four; under current docs they are **verbs**. Hybrid talk should say whether they stay verbs or become the prototype signature **cards**.

---

## 6. Decision menu for the live conversation

Reply with letters / short notes. Integrator promotes only what you confirm into PRODUCT_MEMORY.

**Q1 — Which family are we exploring as the long-term card system?**  
- (a) Shared public pool — stay close to **A1** (current)  
- (b) Shared public pool with **draw** (**A2**)  
- (c) Personal deckbuilding (**B**)  
- (d) Hybrid signatures + shared library (**C**)  
- (e) Not sure — want a thinner spike / prototype discussion first  

**Q2 — If (d) hybrid: what is a "signature"?**  
- (a) Keep as **unique verb** only (C42–C44); library cards stay universal → **no C18 amend** for exclusives  
- (b) Signature = **exclusive gear card** → **requires amending C18/C62**  
- (c) Both (verb *and* a card) — explain how they differ so players aren't confused  

**Q3 — If (c) or (d): same-match legality**  
- (a) Amend C18 — players may bring different legal lists  
- (b) Soften C18 — same *library*, but loadout subsets may differ (still a C18/C62 economy reopen)  
- (c) Reject divergent lists — deckbuilding only as cosmetics / orderings (usually not worth it)  

**Q4 — Blind program**  
- (a) Keep full visibility of own legal set; no hidden draws  
- (b) Allow hidden hands / draws; accept card-game asymmetry  
- (c) Hybrid: signatures public by Character select; library hand may be hidden  

**Q5 — Scope timing**  
- (a) Lock long-term model now; freeze further gear numerics until C# exists  
- (b) Keep shipping **A1** first wave (Bandage HUD, etc.); park B/C as post-first-wave  
- (c) Spike one hybrid/signature prototype in docs only before any C#  

**Q6 — Monetization guard** (if B or C)  
- (a) Entire gameplay library + signatures free forever; only skins sell  
- (b) Something else (state it — will be stress-tested against C47)

---

## 6A. Human answers so far (2026-08-13 chat) — provisional

Human answered the three-family prompt as **hybrid** (Model C), not menu letter "(c) personal deckbuilding alone." Recorded that way below.

| Q | Answer | Notes |
|---|--------|-------|
| **Q1** | **Hybrid** (signature + shared Hearthstone-like library) | Long-term direction under discussion. **Conflicts with C18/C62** until amended. |
| **Q2** | **Both** — unique verb *and* a card | Example given: Bomber casts the signature by **playing a card** that attaches the bomb to the floor. Resolve stays a schedulable unique verb (C42/C43 discipline); the card is how you arm/spend it in the hand/economy. See §6B. |
| **Q3** | **Q3-a — Yes** | Each player builds a personal deck from the shared library; opponents can face different non-signature cards in one match. Classic hybrid. **Requires amending C18** (and C62 economy / same-deck wording). |
| **Q4** | Decks / draws / hands **hidden from the opponent** | Card-game asymmetry accepted for private deck + hand. Character select (and thus *which signature exists*) stays public. Exact Reveal timing for *played* cards still TBD. |
| **Q5** | Cards dept call (human deferred) | **Ship transitional A1 staples now; lock hybrid as documented long-term direction; do not freeze Bandage HUD / C63.** Deckbuilder + signature-card UX are a later systems layer after a C# row. Don't open Flashbang numerics until Q3 + C# shape are clear enough that Flashbang knows whether it's a universal staple or a deck-tech include. |
| **Q6** | **Yes** — entire gameplay library + signatures free forever; only skins sell | Aligns with **C47**; must travel with any C18/C62 amend. |

**Promoted:** Integrator wrote **C64** from §6D (2026-08-13). Q3 is closed (a). OPEN follow-ups stay in C64's parking list / §6D.

---

## 6B. Working picture of "signature = both" (Bomber example)

Not locked — restates the human's example so the next talk starts from the same sketch:

1. Player picks **Bomber** (public — opponent knows the signature *exists*).
2. Bomber's deck/hand may include the **Bomb** signature card (opponent does **not** see the hand).
3. During Program, Bomber **plays the card** onto the timeline / a floor point (same family as arming gear).
4. Resolve still runs a **unique verb** (attach bomb as a schedulable timed action — C43 shape): Host event-stream, no physics.
5. A non-Bomber Character **cannot** legally play that card — the verb and the card are both Character-gated.

**Open edges for later (not asked yet):** Is the signature card always in hand, or drawn from the personal deck? Does it cost Time Resource like Bandage, or only a charge? Can you include 0–N copies? Does Scout's deck simply omit Bomb, or is Bomb unsellable/ungraftable in the builder?

---

## 6C. Q3 clarified in plain language — please pick

Earlier "different lists" meant: **in the same match, can the two players bring different cards?**

Examples:

| Situation | Same cards both sides? |
|-----------|-------------------------|
| Scout vs Juggernaut, both built "Bandage + Flashbang + …" differently | One might bring 2× Flashbang and no Bandage; the other brings Bandage and Interact — **different cards in the match** |
| Bomber vs Scout | Bomber has Bomb signature; Scout does not — **already different** if signatures are cards |
| Both Bombers, same deck recipe | Could still be identical |

**Pick one for the shared library (signatures already differ by Character):**

- **Q3-a.** Yes — each player builds a personal deck from the shared library; opponents can face different non-signature cards in one match. (Classic Hearthstone freedom. Clearest hybrid. **Amends C18.**)
- **Q3-b.** No for library — every Character still has the **same** non-signature legal set (today's Bandage/Flashbang/… always available); only the **signature card** differs by Character. Deckbuilding might mean order/copies/cosmetics only, or is deferred. (**Narrower C18 amend** — exclusives for signatures only.)
- **Q3-c.** Something else — say it in a sentence.

Q4 already says the **hand/deck contents** stay hidden either way; Q3 is only about whether those hidden contents *may differ*.

**Answered 2026-08-13: Q3-a.**

---

## 6D. Proposed C# amend — for Integrator (Cards draft; not yet PRODUCT_MEMORY)

Human confirmed in chat 2026-08-13 via `CARD_SYSTEM_MODEL_COMPARISON.md` §6A–§6C. **Cards does not edit PRODUCT_MEMORY.** Integrator should promote (or revise) something in this shape after review:

**Working title C64 (number Integrator's to assign):** Card-system model — hybrid deckbuilding + signature cards (amends **C18**, **C62**; sits beside **C42–C44**, **C47**).

Proposed substance:

1. **Long-term card system = hybrid:** Character-unique **signature** cards for identity + a **shared library** from which each player builds a **personal deck** before the match.
2. **Same-match legality:** Players **may bring different cards** (Q3-a). C18's "same gear deck" reading is **amended** — Attack/Defend labels still don't lock kits, but Characters no longer share one universal in-match gear list; deck composition + signature gate access.
3. **Signature = both verb and card:** The exclusive power remains a **unique verb** in the resolve/event stream (C42 discipline — schedulable, Host-computed). The player **arms it by playing a signature card** (Bomber example: play Bomb card → attach bomb to floor). Non-owners cannot include or play that card. Amends C62's "unique verbs stay verbs, not exclusive gear packs" to: unique verbs are **expressed through** exclusive signature cards without dropping event-stream discipline.
4. **Hidden information:** Personal deck and hand are **hidden from the opponent**. Character select (hence which signature *exists*) stays public. Reveal rules for cards once played during Program/Playback — OPEN, follow-up.
5. **Monetization:** Entire gameplay library + all signatures remain **free forever**; only cosmetics/skins sell (**C47** restated). No paywalled cards or signatures.
6. **Sequencing (Cards Q5 call, human deferred):** Do **not** freeze already-shipping first-wave staple work (Bandage C63 / HUD). Treat Bandage / Flashbang / Interact / Adrenaline as early **shared-library** candidates under transitional full-hand access until deckbuilder ships. Deckbuilder UI, draw/hand rules, and signature-card UX are a later systems layer. Exact in-match draw vs full-constructed-hand — OPEN (C62 "full hand every Program" is superseded as the long-term economy once deckbuilding lands; transitional ship may keep full-hand staples).
7. **Does not** auto-promote Bomber/Time Player into active build (C43/C44 prerequisites unchanged) or greenlight Flashbang numerics.

**OPEN after C# (suggested parking):** deck size; copies allowed; draw vs always-have-constructed-hand each Program; signature always-in-hand vs draw; TR cost of signatures; Reveal of played cards; how Attacker/Defender labels interact with deckbuilding (if at all).

---

## 7. Explicit non-goals of this pass

- Does not greenlight Flashbang numerics or reopen Bandage C63.  
- Does not itself write PRODUCT_MEMORY — Integrator owns §6D promote.  
- Does not start collection / deckbuilder UI.  
- Does not implement Bomber.

---

## See also

- [`CARD_COLLECTION.md`](CARD_COLLECTION.md) — §5 same-deck options, §5A gear vs verb, §6 economy, §8 answered menu (C62)  
- [`CHARACTER_ROSTER_LONGTERM.md`](CHARACTER_ROSTER_LONGTERM.md) — Bomber / Time Player unique verbs  
- [`GEAR_FLASHBANG_AGENT_BRIEF.md`](GEAR_FLASHBANG_AGENT_BRIEF.md) — **paused** while this conversation is open  
- [`GEAR_BANDAGE_AGENT_BRIEF.md`](GEAR_BANDAGE_AGENT_BRIEF.md) — closed via C63; still valid under transitional A1 staples  
- [`docs/departments/cards/STATUS.md`](departments/cards/STATUS.md)
