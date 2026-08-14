# Card Collection & Gear Deck — Design Research

**Status:** **C64 hybrid** is the long-term target (2026-08-13). Deckbuilder sizing **locked as C65** (2026-08-14) — see [`CARD_SYSTEM_OPENS.md`](CARD_SYSTEM_OPENS.md) / PRODUCT_MEMORY. First-wave staples still ship on **transitional full-hand** (**C62**/**C63**) until a deckbuilder lands. Gear **numerics** (§6A, except Bandage/C63) and Adrenaline real-effect redesign remain OPEN.  
**Flashbang:** effect brief **paused** — when resumed, re-derive as **shared-library** tech under C64 (not a signature).  
**Worktree:** `D:\projects\Game\logiCard-cards-collection` / `feat/cards-collection-docs`  
**Depends on:** [`PRODUCT_MEMORY.md`](../core/PRODUCT_MEMORY.md) **C15**, **C18** *(amended by C64)*, **C33**, **C42–C44**, **C47**, **C62**, **C63**, **C64**, **C65**; [`CARD_SYSTEM_MODEL_COMPARISON.md`](CARD_SYSTEM_MODEL_COMPARISON.md); [`GDD.md`](../core/GDD.md); [`CORE_LOOP.md`](../core/CORE_LOOP.md); [`UI_FLOW.md`](../ui/UI_FLOW.md); [`MONETIZATION.md`](../core/MONETIZATION.md); [`CHARACTER_ROSTER_LONGTERM.md`](../character/CHARACTER_ROSTER_LONGTERM.md).

---

## 1. Why this doc exists

Gear cards need a catalog, an access model, and an in-match economy before HUD/Sim work sprawls. This file is the Cards-department catalog: **C64 hybrid** as the long-term frame, with **C62**/**C63** as the shipping transitional layer. It does **not** implement code.

Live questions this doc answers (with which C# owns the answer):

1. What cards exist in the first wave? → **C62** catalog (library candidates under C64)  
2. Same universal gear list forever? → **C64**: long-term **no** (personal decks + signatures); transitional shipping **yes** (full-hand staples)  
3. Hold / draw / spend? → transitional full-hand+charges (**C62**); long-term = always-have constructed hand from 5–8 deck (**C65**)  
4. Meta collection / paywall? → none for gameplay; free library + signatures (**C64** / **C47**)

---

## 2. Locked facts (do not re-litigate)

| ID | Fact |
|----|------|
| **C15** | Move + Shoot = **base verbs**; cards = **gear**; Characters differ by attrs and later unique verbs. |
| **C18** | Attacker/Defender are labels + spawns. *"Same gear deck" as a hard long-term rule is **retired by C64*** — players may bring different cards from a shared library; labels still don't role-lock kits. |
| **C33** | **Time Card** commits match-pool seconds — **not** a gear card. |
| **C21 / C25** | No Walk card, no Snap/Hold card. Stance and shoot mode are direct picks on base verbs. |
| **C34 / C46** | Named gear was roadmap under demo framing; C46 removed the calendar gate. Bandage Sim has since landed (**C63**); other first-wave cards still staged. |
| **C42–C44** | Unique-verb Characters (Bomber / Time Player) — long-term; event-stream only. Under C64 the exclusive power is still a unique verb, **armed by playing a signature card**. |
| **C47** | F2P cosmetic-only IAP. Gameplay cards / signatures / unique verbs are **not** sellable power. |
| **C62** | First-wave catalog + **transitional** economy (full hand + charges; no binder; Interact = future stations; Adrenaline Execute stub). Long-term "same deck / verbs-not-cards" clauses superseded by **C64**. |
| **C63** | Bandage: 3s TR, 1×/Character/match; HUD-gated not-mid-Sprint; Sim carve-out for Bandage. Unaffected by C64. |
| **C64** | Long-term = **hybrid**: Character **signature** cards (unique verb armed by card) + **personal decks** built from a **shared library**; deck/hand hidden; Character pick public; library + signatures free forever. Shipping staples stay on transitional full-hand until deckbuilder. |
| **PLAYBACK_CONTRACT** | Adrenaline today = Execute-only UI gate + **stub** effect. |

**Two horizons (read every section with this split):**

| Horizon | Access model |
|---------|----------------|
| **Long-term (C64)** | Constructed **personal deck** from shared **library** + Character **signature** card(s); players may bring different cards; opponent does not see deck/hand |
| **Shipping / transitional** | Full visible hand of first-wave staples + charges (**C62**/**C63**) — Bandage etc. — until deckbuilder ships |

---

## 3. Vocabulary (proposed)

Use these names in future docs so “card” stops meaning four different things:

| Term | Meaning |
|------|---------|
| **Time Card** | Allotment UI + match-pool commit (**C33**). Not gear. Cosmetic backs OK (**MONETIZATION**). |
| **Character Card** | Pre-match Scout / Juggernaut (attrs). Not a playable gear card. |
| **Gear / library card** | Shared-library schedulable (or Execute-gated) item — Bandage, Flashbang, etc. Under C64, chosen into a personal deck (long-term) or offered as transitional full-hand staples (shipping). |
| **Signature card** | Character-unique card that **arms a unique verb** (C64) — e.g. Bomber's Bomb. Not in other Characters' legal sets. |
| **Otherwise card** | Failure / contingency library (family; not first-wave). |
| **In-match hand** | What the player can arm during Program (or Execute for Adrenaline). Hidden from opponent under C64. |
| **Shared library** | Full free catalog of buildable non-signature cards (**C64**). |
| **Personal deck** | Pre-match constructed list from the shared library (**C64**/**C65**). Size **5–8**, ≤**2** copies per library card. |
| **Cosmetic binder** | Skins for gear / Time Card backs — sellable; not gameplay unlocks (**C47**). |

---

## 3A. Player-facing glossary (one-page)

**Status:** Draft strawman for a future in-game help/tooltip screen. Wording is a starting point for UI copy, not confirmed voice/tone.

Plain-language versions of §3's dev vocabulary — what a player would actually read in a tooltip or help screen, not what this doc's authors call it internally.

| Term (player-facing) | What it means to the player |
|---|---|
| **Character** | Who you're playing — Scout or Juggernaut today (attrs). Later Characters may also bring a **signature** ability card only they can play. |
| **Time Card** | The card you play each round to commit part of the shared match clock. Bigger commitment = more time to act this round, but your opponent plans against that same window. |
| **Program** | The planning phase — you secretly draw your path, aim your shots, and place any gear. Nobody sees your plan until it plays out. |
| **Gear / library cards** | Tools like Bandage or Flashbang from the shared pool. Long-term you **build a deck** from that pool; today both sides still see the same staple set while we ship the first cards. |
| **Signature** | Your Character's unique card — playing it fires that Character's special verb (e.g. Bomber's bomb). |
| **Hand** | The gear you can use this round (hidden from the opponent long-term). |
| **Otherwise** | What happens if your planned move gets blocked (like running into a closed door) — a backup rule, not something you choose in the moment. |
| **Reveal** | The instant both plans flip face-up, right before they play out. |
| **Playback** | The replay that shows both sides' round unfolding together, second by second. |
| **Aftermath** | The moment where the round's outcome — position, wounds, doors — carries into the next round. |

This is a starting vocabulary list, not locked UI copy — final tooltip wording is a UI/writing pass, not a design decision.

---

## 4. Catalog — cards already named in product docs

### 4.1 Not gear (keep out of the gear binder)

| Name | Role |
|------|------|
| Time Card | Round allotment |
| Character select (Scout / Juggernaut) | Attr preset |
| Map select “cards” | UI chrome only (**C59**) — not gameplay cards |

### 4.2 Gear already named (confirmed as design intent, not built)

| Card | Phase / use (from UI_FLOW / GDD) | Rough job |
|------|----------------------------------|-----------|
| **Bandage** | Program — arm then place on scrubber / path node | Heal / clear wound pressure before next round (GDD still notes bleed/surcharge as future) |
| **Interact-as-card** | Program | Generalized interact beyond contextual Door (vent / monitor / future stations) |
| **Flashbang** | Program | Soft control / vision / interrupt — numerics OPEN |
| **Adrenaline** | **Execution only**, **1× per match**, only while an active segment plays | Mid-cinema tool; effect resolve still stub |

### 4.3 Otherwise (library, not a single card)

Today resolve stops movement before a closed door / block. A full **Otherwise** library would be explicit contingency cards or automatic rules the player configures. Treat as a **family**, not one entry in the starter deck, until designed.

### 4.4 Adjacent systems that look like cards but aren’t yet

| Concept | Doc home | Notes |
|---------|----------|-------|
| Door open/close | GDD / C39 | Contextual map action, not a gear card for the current ship |
| Vent / Breach | C57 | Reskinned `Door.Kind` — map features, not hand cards |
| 高铁 ride | C31 | Match-limited map verb; not gear |
| Bomber / Time Player | CHARACTER_ROSTER_LONGTERM / **C64** | Long-term **signature**: unique verb **armed by** a Character-only card — free gameplay |

**No additional gear names are invented here.** Expanding the catalog is an explicit human call.

---

## 5. Access model — C64 hybrid (+ transitional full-hand)

### 5.1 What is locked now

| Layer | Rule |
|-------|------|
| **Long-term (C64)** | Shared **library** → each player builds a **personal deck** (**5–8**, ≤2 copies) before the match; Character also brings **signature** card(s) **extra** outside deck cap, always available. Players **may bring different cards**. Deck/hand **hidden**; Character select public. Played cards flip at **Reveal**. |
| **Shipping (C62/C63)** | First-wave staples on **transitional full-hand** — both sides see the same Bandage / Flashbang / Interact / Adrenaline set until a deckbuilder ships. Bandage numerics locked (C63). Does **not** wait on deckbuilder UX. |
| **Attacker/Defender** | Still labels + spawns + Allot chooser — **not** role-locked kits (**no** deck interaction — answered 2026-08-14). |
| **Monetization** | Entire gameplay library + all signatures **free forever** (**C64** / **C47**). Cosmetics only sellable. |

Conversation record: [`CARD_SYSTEM_MODEL_COMPARISON.md`](CARD_SYSTEM_MODEL_COMPARISON.md). Sizing: [`CARD_SYSTEM_OPENS.md`](CARD_SYSTEM_OPENS.md) / **C65**.

### 5.2 Historical options (pre-C64 research — kept for context)

| Option | Description | Status after C64 |
|--------|-------------|------------------|
| **A. Shared universal set** | Everyone always has every library card | **Transitional shipping only** |
| **B. Shared set + attr cost mods** | Same cards; Strength may scale Interact cost (C62 carve-out) | Still valid for Interact |
| **C. Per-Character exclusive library cards** | Scout-only Flashbang etc. | Not the C64 model — exclusives are **signatures**, not random library splits |
| **D. Unique verbs only (no signature card)** | Bomber bomb as verb with no hand card | **Superseded by C64** — signature = verb *and* card |

---

## 5A. Library gear vs signature (verb + card) — the boundary

**C64 test:**

| | Shared-library gear | Signature (verb + card) |
|---|---|---|
| **Example** | Bandage, Flashbang, Interact-as-card, Adrenaline | Bomber Bomb, Time Player rewind card (**C43/C44**, not active build) |
| **Who can include it** | Any Character (deck choice long-term; full-hand transitional) | Only the owning Character |
| **How you use it** | Play/arm as gear | Play the **card** → resolve runs the **unique verb** (event-stream, C42) |
| **Doc home** | This file | `CHARACTER_ROSTER_LONGTERM.md` + this file's signature rows |
| **Monetization** | Free forever (**C64**/**C47**) | Free forever — never paywalled |

Adding a **library** card is a catalog content add. Adding a **signature** is a roster decision (Character brief + C#) that also adds a card wrapper — don't smuggle exclusives into the library without calling them signatures.

**Flashbang note (paused):** do not open a Flashbang Sim/HUD contract from the parked brief. When effect design resumes, treat Flashbang as **shared-library** tech under C64 — re-derive the brief against that frame; it is not a signature.

---

## 6. In-match economy

**Long-term (C64 + C65):** personal constructed deck (**5–8**, ≤2 copies) from the shared library; **always-have** that constructed hand each Program (charges limit spend; no draw RNG). Signature is **extra** outside deck cap, always available, costs TR. Played cards public at **Reveal**.

**Shipping (C62):** full hand every Program + per-card charges for first-wave staples. Do **not** build draw RNG into the Bandage HUD path; transitional full-hand is intentional until the deckbuilder layer.

| Model | Role now |
|-------|----------|
| **1. Full hand every Program** | **Transitional shipping** (C62) — Bandage / Flashbang / Interact / Adrenaline |
| **2. Draw N each Program** | Rejected for long-term (answered Q3-a) |
| **3. Match loadout / constructed deck** | **C64 long-term** — 5–8, always-have hand (answered) |
| **4. Staples + flex** | Bridge only while transitional full-hand remains |

**Adrenaline:** Execute-only, 1/match, stub effect until PLAYBACK_CONTRACT redesign; under Q8-b may bypass normal deck rules.

**Time Card relation:** gear and signatures burn **Time Resource** inside round **N** (signature magnitudes OPEN per character).

---

## 6A. Strawman charge table (numerics OPEN — not CONFIRMED)

**Purpose:** a concrete starting point for the human §8 answers and for whoever eventually writes the `GhostResolver` resolve logic — not a locked spec. Every number below is a placeholder pulled from the same numeric family as existing verbs (GDD §6), marked **OPEN**.

| Gear | Phase (locked) | Time Resource cost (strawman) | Charges / match (strawman) | Cooldown | Resolve note |
|---|---|---|---|---|---|
| **Bandage** | Program | **3s** (~1.5× Snap Shot) | **1 per Character per match** | n/a | Heals Wounded → Healthy; must land before the *next* round starts (§4.2). Full bleed/surcharge system is separate future scope (**C46**, GDD §5 note) — do not build that alongside this strawman. |
| **Flashbang** | Program | **2s** (~1× Snap Shot) | **2 per match** | n/a | Soft control/vision — blast radius, duration, and what "soft control" *does* mechanically are all OPEN, not just the cost. Needs its own effect design, not only a numeric. |
| **Interact-as-card** | Program | **2–4s**, same family as Door's Strength-scaled open/close (GDD §6) | Unlimited **uses**, gated by a legal target existing (mirrors Door's `InteractRadius` gate, **C39**) | n/a | Closest to an existing resolve shape (Door) — lowest design risk of the four. Needs a real target (vent/monitor/station) to exist before it does anything; **not** meaningful until those exist per §4.4. |
| **Adrenaline** | **Execute only** (locked, not OPEN — GDD/`PLAYBACK_CONTRACT`) | Not time-budgeted the same way — it's a mid-cinema interrupt, not a Program-armed cost | **1 per match** (locked — GDD §4.2, `PLAYBACK_CONTRACT` §4) | n/a | Effect itself is **stub** today (`AdrenalineUsed` event, no mechanical change). Any real effect needs the explicit redesign `PLAYBACK_CONTRACT` §2 rule 5 requires before it can do more than log an event — this is a bigger question than "what number," flagged separately in §8 Q6. |

**What's locked vs open in this table:** phase column + Adrenaline 1×/match locked; **Bandage cost/charges locked by C63**. Flashbang/Interact numerics + Flashbang effect shape still OPEN (OPEN #16). Under C64 / Q8 these four are **shared-library** cards (Adrenaline keeps special Execute slot); none are signatures.

---

## 7. Meta “collection” (binder) — separate from match hand

“Cards collection system” can mean three different products. Pick which we mean:

| Layer | Question | Monetization note |
|-------|----------|-------------------|
| **Rules catalog** | Which gear definitions exist in the game? | Gameplay — free |
| **Match access** | Which of those can I bring / draw this match? | Gameplay — free; must not paywall power (**C47**) |
| **Cosmetic binder** | Skins for gear / Time Card backs / sleeve art | Sellable if readability-safe |

**C64 / C47:** gameplay library + signatures stay free forever — no gacha unlock for power. Cosmetic binder (Time Card backs, gear skins) is the only sellable layer.

Ship order for *systems*: transitional staple resolve (Bandage…) → deckbuilder/hand rules (**C65** locked; UI later) → cosmetic binder.

---

## 8. Decision menu — C62 answers (2026-08-12) + C64 overlay + sizing (2026-08-14)

**§8 answered 2026-08-12 → C62.** **Long-term access model answered 2026-08-13 → C64.** **Deckbuilder sizing → C65** (2026-08-14; detail [`CARD_SYSTEM_OPENS.md`](CARD_SYSTEM_OPENS.md)).

1. **Catalog scope for first gear wave** — **(a)** Bandage, Interact-as-card, Flashbang, Adrenaline only. No Otherwise card this wave. **Still holds** (shared-library under Q8-b; Adrenaline = special Execute slot).
2. **Same deck?** — **C62 answered (a)** same legal list + Interact Strength carve-out. **C64 amends long-term:** personal decks may differ; signatures are Character-exclusive. **Transitional shipping still behaves like C62 full-hand staples.**
3. **In-match economy** — **C62 (a)** full hand + charges for first wave. **Long-term:** always-have constructed hand from 5–8 deck (≤2 copies); signature extra + always on; no draw RNG.
4. **Meta collection** — **(a)** no gameplay binder/unlock wall. **C64** restates: library + signatures free forever; cosmetics only (**C47**).
5. **Interact-as-card vs Door/Vent** — **(a)** Door/Vent/Breach stay map actions; Interact = future stations. **Still holds.**
6. **Adrenaline** — **(a)** Execute stub until PLAYBACK redesign. **Still holds** (+ Q8-b may bypass normal deck rules).
7. **Reveal / role / signature TR** — played cards at **Reveal**; Attack/Defend **no** deck interaction; signatures **cost TR** (magnitudes OPEN).

---

## 9. Suggested doc / code ownership when this graduates

| Artifact | Owner |
|----------|-------|
| Confirmed C# row + OPEN #3 resolution | Integrator → PRODUCT_MEMORY |
| This research → rename to `CARD_SYSTEM.md` or split `GEAR_CATALOG.md` + `CARD_ECONOMY.md` | Integrator after human answers |
| Program HUD gear strip | UI worker against frozen arm/place contract |
| Resolve effects + ReplayTape events | Core — must follow PLAYBACK_CONTRACT |
| Cosmetic skins | Art / monetization — after rules exist |

**Do not** start Sim work from this draft alone.

---

## 10. One-page direction — C64 long-term + C62 transitional

**Long-term (C64 + 2026-08-14 sizing) — target model:**

- **Hybrid:** shared free **library** → **personal decks** (**5–8**, ≤**2** copies); Character **signature** = unique verb **armed by** a card — **extra** outside deck cap, always available, costs TR.
- Players may bring **different** cards; deck/hand **hidden**; Character pick public; played cards flip at **Reveal**.
- Attack/Defend labels do **not** constrain decks.
- Gameplay library + signatures free forever (**C47**).
- Bandage / Flashbang / Interact / Adrenaline = shared-library; Adrenaline keeps special Execute slot.
- Detail: [`CARD_SYSTEM_OPENS.md`](CARD_SYSTEM_OPENS.md) / **C65**.

**Shipping / transitional (C62 + C63) — what ships before the deckbuilder:**

- Catalog staples (library candidates): Bandage, Flashbang, Interact-as-card, Adrenaline (stations later for Interact).
- Full visible hand + charges; Bandage 3s / 1× match locked.
- Interact cost may scale by Strength; Door/Vent/Breach stay map actions.
- Adrenaline Execute stub until PLAYBACK redesign.
- No gameplay unlock binder.
- Does **not** block Bandage HUD or other staple Sim/HUD.

---

## 11. Recommended first-ship sequence (proposal only)

Ranked by resolve risk for **library staples** under transitional full-hand (deckbuilder is a later systems layer; **C65** sizes it but does not greenlight UI):

1. **Bandage** — **Sim landed (C63)**; HUD-side owned by UI seat (this dept does not touch Bandage HUD files).
2. **Interact-as-card** — Door-like resolve; blocked on a real station target.
3. **Flashbang** — **paused**; when resumed, re-derive brief as shared-library tech (`GEAR_FLASHBANG_AGENT_BRIEF.md`). Effect shape + numerics still OPEN.
4. **Adrenaline (real effect)** — last; needs PLAYBACK_CONTRACT redesign.
5. **Deckbuilder + signatures** — after staple conventions exist (**C65** rules locked); Bomber/Time Player still need C43/C44 prereqs.
6. **Otherwise library** — separate project after the four named cards.

---

## 12. Next step

→ Integrator: merge this branch to master (C65 already on branch). Cards seat idle on deckbuilder code until then.

---

## See also

- [`PRODUCT_MEMORY.md`](../core/PRODUCT_MEMORY.md) — C62, C63, **C64**, **C65**, OPEN #16  
- [`CARD_SYSTEM_MODEL_COMPARISON.md`](CARD_SYSTEM_MODEL_COMPARISON.md) — hybrid conversation  
- [`CARD_SYSTEM_OPENS.md`](CARD_SYSTEM_OPENS.md) — sizing answers (source for C65)  
- [`GEAR_BANDAGE_AGENT_BRIEF.md`](GEAR_BANDAGE_AGENT_BRIEF.md) / [`GEAR_FLASHBANG_AGENT_BRIEF.md`](GEAR_FLASHBANG_AGENT_BRIEF.md)  
- [`GDD.md`](../core/GDD.md) · [`UI_FLOW.md`](../ui/UI_FLOW.md) · [`PLAYBACK_CONTRACT.md`](../core/PLAYBACK_CONTRACT.md) · [`MONETIZATION.md`](../core/MONETIZATION.md)  
- [`CHARACTER_ROSTER_LONGTERM.md`](../character/CHARACTER_ROSTER_LONGTERM.md) — Bomber / Time Player  
