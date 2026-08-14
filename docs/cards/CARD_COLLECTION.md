# Card Collection & Gear Deck — Design Research

**Status:** **C64 hybrid** + **C68** packaging + **C67** Storm (2026-08-14): **each Character has an 8-card play deck**; Storm = presentation-only weather gear. First-wave staples still ship on **transitional full-hand** (**C62**/**C63**) until a deckbuilder lands. Gear **numerics** (§6A, except Bandage/C63; Storm OPEN) and Adrenaline real-effect redesign remain OPEN.  
**Flashbang:** effect brief **paused** — when resumed, re-derive as **shared-library** tech under C64 (not a signature).  
**Storm:** catalog + numerics recommendation in [`GEAR_STORM_AGENT_BRIEF.md`](GEAR_STORM_AGENT_BRIEF.md); HUD/Atmosphere per Storm contract.  
**Worktree:** `D:\projects\Game\logiCard-cards-collection` / `feat/cards-collection-docs`  
**Depends on:** [`PRODUCT_MEMORY.md`](../core/PRODUCT_MEMORY.md) **C15**, **C18** *(amended by C64)*, **C33**, **C42–C44**, **C47**, **C62**, **C63**, **C64**, **C66**, **C67** (Storm), **C68**; [`CARD_SYSTEM_MODEL_COMPARISON.md`](CARD_SYSTEM_MODEL_COMPARISON.md); [`DECKBUILDER_SYSTEMS_BRIEF.md`](DECKBUILDER_SYSTEMS_BRIEF.md); [`GDD.md`](../core/GDD.md); [`CORE_LOOP.md`](../core/CORE_LOOP.md); [`UI_FLOW.md`](../ui/UI_FLOW.md); [`MONETIZATION.md`](../core/MONETIZATION.md); [`CHARACTER_ROSTER_LONGTERM.md`](../character/CHARACTER_ROSTER_LONGTERM.md).

---

## 1. Why this doc exists

Gear cards need a catalog, an access model, and an in-match economy before HUD/Sim work sprawls. This file is the Cards-department catalog: **C64 hybrid** + **C68** (8 per Character / Character-in-deck) as the long-term frame, with **C62**/**C63** as the shipping transitional layer. It does **not** implement code.

Live questions this doc answers (with which C# owns the answer):

1. What cards exist in the first wave? → **C62** catalog (library candidates under C64)  
2. Same universal gear list forever? → **C64**: long-term **no** (personal decks + signatures); transitional shipping **yes** (full-hand staples)  
3. Hold / draw / spend? → transitional full-hand+charges (**C62**); long-term = always-have **8-card play deck per Character** (**C68**)  
4. Meta collection / paywall? → none for gameplay; free library + signatures (**C64** / **C47**); up to **10** saved decks (**C68**)

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
| **C64** | Long-term = **hybrid**: signatures + personal decks from shared library; deck/hand hidden; library + signatures free forever. Shipping staples stay on transitional full-hand until deckbuilder. *[Packaging amended by **C68** — Character first-class in card/deck system; 8-card play deck per Character.]* |
| **C66** | Sizing defaults (always-have, ≤2 copies, role, first-wave library). *[Amended by **C68** for fixed 8/Character + Character-in-deck + Adrenaline-as-card.]* |
| **C67** | **Storm** — Program gear; presentation-only board weather → Storm mood; self-targeting; Sim-side landed; numerics OPEN (see [`GEAR_STORM_AGENT_BRIEF.md`](GEAR_STORM_AGENT_BRIEF.md)). |
| **C68** | **Each Character has an 8-card play deck**; Characters first-class in card/deck system; 10 saved decks; everything is a card; Host/relay validates; played cards → timeline (GDD). |
| **PLAYBACK_CONTRACT** | Adrenaline today = Execute-only UI gate + **stub** effect (timing may remain; **C68** puts Adrenaline in the deck model). StormCast is a continuous weather presenter. |

**Two horizons (read every section with this split):**

| Horizon | Access model |
|---------|----------------|
| **Long-term (C64 + C68)** | **Each Character has an 8-card play deck**; Characters first-class in card/deck system; always-have that hand; players may bring different cards; play deck/hand hidden |
| **Shipping / transitional** | Full visible hand of first-wave staples + charges (**C62**/**C63**) — Bandage etc. — until deckbuilder ships |

---

## 3. Vocabulary (proposed)

Use these names in future docs so “card” stops meaning four different things:

| Term | Meaning |
|------|---------|
| **Time Card** | Allotment UI + match-pool commit (**C33**). Not gear. Cosmetic backs OK (**MONETIZATION**). |
| **Character Card** | First-class card/identity in the collection + deckbuilder (**C68**). Attrs / who you field. **Not** a parallel non-card pick outside the deck system. Scout/Jug preliminary — no signatures for them. |
| **Gear / library card** | Shared-library schedulable (or Execute-phase) item — Bandage, Flashbang, etc. Chosen into a Character's 8-card play deck (long-term) or transitional full-hand (shipping). |
| **Signature card** | Character-unique card that **arms a unique verb** (C64) — e.g. Bomber's Bomb. Later roster only. Whether inside the 8 or extra stays OPEN after C68. |
| **Otherwise card** | Failure / contingency library (family; not first-wave). |
| **In-match hand** | The Character's always-have play deck during Program (charges/phase gate spend). Hidden from opponent under C64. |
| **Shared library** | Full free catalog of buildable non-signature cards (**C64**). |
| **Play deck** | **Exactly 8 cards per Character** for the match (**C68**). ≤**2** copies per library card (**C66**). |
| **Saved decks** | Up to **10** named decks per profile (**C68**). |
| **Cosmetic binder** | Skins for gear / Time Card backs — sellable; not gameplay unlocks (**C47**). |

---

## 3A. Player-facing glossary (one-page)

**Status:** Draft strawman for a future in-game help/tooltip screen. Wording is a starting point for UI copy, not confirmed voice/tone.

Plain-language versions of §3's dev vocabulary — what a player would actually read in a tooltip or help screen, not what this doc's authors call it internally.

| Term (player-facing) | What it means to the player |
|---|---|
| **Character** | Who you're playing — a Character card in the collection. Each has an **8-card play deck**. Later Characters may also have a **signature** ability card. |
| **Time Card** | The card you play each round to commit part of the shared match clock. Bigger commitment = more time to act this round, but your opponent plans against that same window. |
| **Program** | The planning phase — you secretly draw your path, aim your shots, and place any cards from your play deck onto the timeline. Nobody sees your plan until it plays out. |
| **Gear / library cards** | Tools like Bandage or Flashbang from the shared pool. Long-term they go into your Character's **8-card deck**; today both sides still see the same staple set while we ship the first cards. |
| **Signature** | A Character's unique card — playing it fires that Character's special verb (e.g. Bomber's bomb). |
| **Hand / play deck** | The 8 cards that Character can use this match (hidden from the opponent long-term). Played cards become timeline behavior. |
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
| **Storm** | Program — arm then place on scrubber only (**C67**) | Presentation-only: switch board weather mood to Storm for the rest of the match. Self-targeting; no LoS / target pawn / board tap. No combat effect this wave. Numerics OPEN — see [`GEAR_STORM_AGENT_BRIEF.md`](GEAR_STORM_AGENT_BRIEF.md). |

### 4.3 Otherwise (library, not a single card)

Today resolve stops movement before a closed door / block. A full **Otherwise** library would be explicit contingency cards or automatic rules the player configures. Treat as a **family**, not one entry in the starter deck, until designed.

### 4.4 Adjacent systems that look like cards but aren’t yet

| Concept | Doc home | Notes |
|---------|----------|-------|
| Door open/close | GDD / C39 | Contextual map action, not a gear card for the current ship |
| Vent / Breach | C57 | Reskinned `Door.Kind` — map features, not hand cards |
| 高铁 ride | C31 | Match-limited map verb; not gear |
| Bomber / Time Player | CHARACTER_ROSTER_LONGTERM / **C64** | Long-term **signature**: unique verb **armed by** a Character-only card — free gameplay |

**No additional gear names are invented here.** Expanding the catalog is an explicit human call — **Storm** is that call (**C67**).

---

## 5. Access model — C64 hybrid (+ transitional full-hand)

### 5.1 What is locked now

| Layer | Rule |
|-------|------|
| **Long-term (C64 + C68)** | **Each Character has an 8-card play deck** (≤2 library copies); Characters first-class in card/deck system; always-have that hand; no draw RNG. Players **may bring different cards**. Play deck/hand **hidden**; fielded Character may stay public. Played cards → **timeline** behavior (GDD). Up to **10** saved decks. Host/relay validates. |
| **Shipping (C62/C63)** | First-wave staples on **transitional full-hand** — both sides see the same Bandage / Flashbang / Interact / Adrenaline set until a deckbuilder ships. Bandage numerics locked (C63). Does **not** wait on deckbuilder UX. |
| **Attacker/Defender** | Still labels + spawns + Allot chooser — **not** role-locked kits (**no** deck interaction — answered 2026-08-14). |
| **Monetization** | Entire gameplay library + all signatures **free forever** (**C64** / **C47**). Cosmetics only sellable. |

Conversation record: [`CARD_SYSTEM_MODEL_COMPARISON.md`](CARD_SYSTEM_MODEL_COMPARISON.md). Sizing → packaging: [`CARD_SYSTEM_OPENS.md`](CARD_SYSTEM_OPENS.md) / **C66**, [`DECKBUILDER_SYSTEMS_BRIEF.md`](DECKBUILDER_SYSTEMS_BRIEF.md) / **C68**.

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
| **Example** | Bandage, Flashbang, Interact-as-card, Adrenaline, Storm | Bomber Bomb, Time Player rewind card (**C43/C44**, not active build) |
| **Who can include it** | Any Character (deck choice long-term; full-hand transitional) | Only the owning Character |
| **How you use it** | Play/arm as gear | Play the **card** → resolve runs the **unique verb** (event-stream, C42) |
| **Doc home** | This file | `CHARACTER_ROSTER_LONGTERM.md` + this file's signature rows |
| **Monetization** | Free forever (**C64**/**C47**) | Free forever — never paywalled |

Adding a **library** card is a catalog content add. Adding a **signature** is a roster decision (Character brief + C#) that also adds a card wrapper — don't smuggle exclusives into the library without calling them signatures.

**Flashbang note (paused):** do not open a Flashbang Sim/HUD contract from the parked brief. When effect design resumes, treat Flashbang as **shared-library** tech under C64 — re-derive the brief against that frame; it is not a signature.

---

## 6. In-match economy

**Long-term (C64 + C68):** **each Character has an 8-card play deck** (≤2 library copies); **always-have** that hand each Program (charges limit spend; no draw RNG). Characters first-class in card/deck system. Everything is a card (Adrenaline in deck model; Execute timing may remain). Played cards → timeline (GDD). Later signatures cost TR (magnitudes OPEN; inside-8 vs extra OPEN).

**Shipping (C62):** full hand every Program + per-card charges for first-wave staples. Do **not** build draw RNG into the Bandage HUD path; transitional full-hand is intentional until the deckbuilder layer.

| Model | Role now |
|-------|----------|
| **1. Full hand every Program** | **Transitional shipping** (C62) — Bandage / Flashbang / Interact / Adrenaline |
| **2. Draw N each Program** | Rejected for long-term (answered Q3-a) |
| **3. Match loadout / constructed deck** | **C68** — **8 per Character**, always-have hand |

**Adrenaline:** card in the deck model (**C68**); Execute *timing* + 1/match stub until PLAYBACK_CONTRACT redesign (**C62**).

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
| **Storm** | Program | **TR —** (placeholder; do not invent a number) | **Recommend 1× / Character / match** (OPEN) | n/a | Presentation-only weather trigger (**C67**). Cast → board mood Fair→Storm for remainder of match; self-targeting; scrubber second only. No combat effect. Detail + defaults: [`GEAR_STORM_AGENT_BRIEF.md`](GEAR_STORM_AGENT_BRIEF.md). |

**What's locked vs open in this table:** phase column + Adrenaline 1×/match locked; **Bandage cost/charges locked by C63**. Flashbang/Interact numerics + Flashbang effect shape still OPEN (OPEN #16). **Storm** TR cost + charge count OPEN (C67) — Cards recommends defaults only. Under C64 / Q8 the first four are **shared-library** cards; Storm is also shared-library presentation gear (not a signature).

---

## 7. Meta “collection” (binder) — separate from match hand

“Cards collection system” can mean three different products. Pick which we mean:

| Layer | Question | Monetization note |
|-------|----------|-------------------|
| **Rules catalog** | Which gear definitions exist in the game? | Gameplay — free |
| **Match access** | Which of those can I bring / draw this match? | Gameplay — free; must not paywall power (**C47**) |
| **Cosmetic binder** | Skins for gear / Time Card backs / sleeve art | Sellable if readability-safe |

**C64 / C47:** gameplay library + signatures stay free forever — no gacha unlock for power. Cosmetic binder (Time Card backs, gear skins) is the only sellable layer.

Ship order for *systems*: transitional staple resolve (Bandage…) → deckbuilder/hand rules (**C68** locked; UI later) → cosmetic binder.

---

## 8. Decision menu — C62 answers (2026-08-12) + C64 overlay + sizing (2026-08-14)

**§8 answered 2026-08-12 → C62.** **Long-term access model answered 2026-08-13 → C64.** **Deckbuilder sizing → C66** then **packaging → C68** (2026-08-14; [`DECKBUILDER_SYSTEMS_BRIEF.md`](DECKBUILDER_SYSTEMS_BRIEF.md)).

1. **Catalog scope for first gear wave** — **(a)** Bandage, Interact-as-card, Flashbang, Adrenaline only. No Otherwise card this wave. **Still holds** (shared-library under Q8-b; Adrenaline = card in deck model per **C68**, Execute timing may remain).
2. **Same deck?** — **C62 answered (a)** same legal list + Interact Strength carve-out. **C64 amends long-term:** personal decks may differ; signatures are Character-exclusive. **Transitional shipping still behaves like C62 full-hand staples.**
3. **In-match economy** — **C62 (a)** full hand + charges for first wave. **Long-term (C68):** always-have **8-card play deck per Character** (≤2 library copies); no draw RNG; Characters in card/deck system.
4. **Meta collection** — **(a)** no gameplay binder/unlock wall. **C64**/**C68**: library + signatures free forever; up to **10** saved decks; cosmetics only (**C47**).
5. **Interact-as-card vs Door/Vent** — **(a)** Door/Vent/Breach stay map actions; Interact = future stations. **Still holds.**
6. **Adrenaline** — **(a)** Execute stub until PLAYBACK redesign. **C68:** in the deck model (not outside-deck magic slot); Execute timing may remain.
7. **Reveal / role / signature TR** — played cards → timeline at program flip (**GDD** / **C68**); Attack/Defend **no** deck interaction; later signatures **cost TR** (magnitudes OPEN; inside-8 vs extra OPEN).

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

## 10. One-page direction — C64/C68 long-term + C62 transitional

**Long-term (C64 + C68) — target model:**

- **Hybrid:** shared free **library** → **each Character has an 8-card play deck** (≤**2** copies); Characters first-class in card/deck system.
- Up to **10** saved decks; Host/relay validates; everything is a card (Adrenaline in deck model).
- Players may bring **different** cards; play deck/hand **hidden**; played cards → **timeline** behavior (GDD).
- Attack/Defend labels do **not** constrain decks.
- Gameplay library + signatures free forever (**C47**). Later signatures cost TR (Scout/Jug: no signature work).
- Detail: [`DECKBUILDER_SYSTEMS_BRIEF.md`](DECKBUILDER_SYSTEMS_BRIEF.md) / **C68**; [`CARD_SYSTEM_OPENS.md`](CARD_SYSTEM_OPENS.md) / **C66**.

**Shipping / transitional (C62 + C63) — what ships before the deckbuilder:**

- Catalog staples (library candidates): Bandage, Flashbang, Interact-as-card, Adrenaline, **Storm** (stations later for Interact).
- Full visible hand + charges; Bandage 3s / 1× match locked; Storm numerics OPEN (brief recommends 1× match, TR —).
- Interact cost may scale by Strength; Door/Vent/Breach stay map actions.
- Adrenaline Execute stub until PLAYBACK redesign.
- No gameplay unlock binder.
- Does **not** block Bandage HUD or other staple Sim/HUD.

---

## 11. Recommended first-ship sequence (proposal only)

Ranked by resolve risk for **library staples** under transitional full-hand (deckbuilder is a later systems layer; **C68** locks packaging but does not greenlight UI):

1. **Bandage** — **Sim landed (C63)**; HUD-side owned by UI seat (this dept does not touch Bandage HUD files).
2. **Storm** — **Sim landed (C67)**; catalog + numerics recommendation (**this seat**); HUD/Atmosphere per Storm contract.
3. **Interact-as-card** — Door-like resolve; blocked on a real station target.
4. **Flashbang** — **paused**; when resumed, re-derive brief as shared-library tech (`GEAR_FLASHBANG_AGENT_BRIEF.md`). Effect shape + numerics still OPEN.
5. **Adrenaline (real effect)** — last; needs PLAYBACK_CONTRACT redesign.
6. **Deckbuilder + signatures** — after staple conventions exist (**C68** rules locked); Bomber/Time Player still need C43/C44 prereqs.
7. **Otherwise library** — separate project after the named cards.

---

## 12. Next step

→ Storm catalog + numerics recommendation landed (Cards Storm DoD). **C68** packaging on this branch. Awaiting Integrator merge. Deckbuilder UI/Sim still needs a separate contract.

---

## See also

- [`PRODUCT_MEMORY.md`](../core/PRODUCT_MEMORY.md) — C62, C63, **C64**, **C66**, **C67** (Storm), **C68**, OPEN #16  
- [`CARD_SYSTEM_MODEL_COMPARISON.md`](CARD_SYSTEM_MODEL_COMPARISON.md) — hybrid conversation  
- [`CARD_SYSTEM_OPENS.md`](CARD_SYSTEM_OPENS.md) — sizing answers (source for C66)  
- [`DECKBUILDER_SYSTEMS_BRIEF.md`](DECKBUILDER_SYSTEMS_BRIEF.md) — packaging source for C68  
- [`GEAR_STORM_AGENT_BRIEF.md`](GEAR_STORM_AGENT_BRIEF.md) — Storm numerics recommendation  
- [`GEAR_BANDAGE_AGENT_BRIEF.md`](GEAR_BANDAGE_AGENT_BRIEF.md) / [`GEAR_FLASHBANG_AGENT_BRIEF.md`](GEAR_FLASHBANG_AGENT_BRIEF.md)  
- [`GDD.md`](../core/GDD.md) · [`UI_FLOW.md`](../ui/UI_FLOW.md) · [`PLAYBACK_CONTRACT.md`](../core/PLAYBACK_CONTRACT.md) · [`MONETIZATION.md`](../core/MONETIZATION.md)  
- [`CHARACTER_ROSTER_LONGTERM.md`](../character/CHARACTER_ROSTER_LONGTERM.md) — Bomber / Time Player  
