# Card Collection & Gear Deck — Design Research

**Status:** **C62** first-wave rules (2026-08-12) + **C64** long-term hybrid model (2026-08-13).  
Shipping staples use **transitional full-hand** (C62/C63). Long-term target = personal decks from a shared library + Character **signature cards** (C64). Gear **numerics** (§6A, except Bandage/C63) and Adrenaline real-effect redesign remain OPEN. Deckbuilder sizing → [`CARD_SYSTEM_OPENS.md`](CARD_SYSTEM_OPENS.md).  
**Worktree:** `D:\projects\Game\logiCard-cards-collection` / `feat/cards-collection-docs`  
**Depends on:** [`PRODUCT_MEMORY.md`](PRODUCT_MEMORY.md) **C15**, **C18** *(amended by C64)*, **C33**, **C42–C44**, **C47**, **C62**, **C63**, **C64**; [`CARD_SYSTEM_MODEL_COMPARISON.md`](CARD_SYSTEM_MODEL_COMPARISON.md); [`GDD.md`](GDD.md); [`CORE_LOOP.md`](CORE_LOOP.md); [`UI_FLOW.md`](UI_FLOW.md); [`MONETIZATION.md`](MONETIZATION.md); [`CHARACTER_ROSTER_LONGTERM.md`](CHARACTER_ROSTER_LONGTERM.md).

---

## 1. Why this doc exists

Gear cards need a catalog, an access model, and an in-match economy before HUD/Sim work sprawls. This file holds taxonomy + first-wave answers (**C62**) and overlays the long-term hybrid direction (**C64**). It does **not** implement code.

Live questions this doc answers (with which C# owns the answer):

1. What cards exist in the first wave? → **C62** catalog  
2. Same universal gear list forever? → **C64** amends: long-term **no**; transitional shipping **yes**  
3. Hold / draw / spend? → transitional full-hand+charges (**C62**); long-term deckbuilder OPENs in [`CARD_SYSTEM_OPENS.md`](CARD_SYSTEM_OPENS.md)  
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
| **C42–C44** | Unique-verb Characters (Bomber / Time Player) — long-term; event-stream only. |
| **C47** | F2P cosmetic-only IAP. Gameplay cards / signatures / unique verbs are **not** sellable power. |
| **C62** | First-wave catalog + transitional economy (full hand + charges; no binder; Interact = future stations; Adrenaline Execute stub). |
| **C63** | Bandage: 3s TR, 1×/Character/match; HUD-gated not-mid-Sprint; Sim carve-out for Bandage. |
| **C64** | Long-term = **hybrid**: signature cards (verb armed by card) + personal decks from shared library; hands/decks hidden; free forever. |
| **PLAYBACK_CONTRACT** | Adrenaline today = Execute-only UI gate + **stub** effect. |

**Two horizons (read every section with this split):**

| Horizon | Access model |
|---------|----------------|
| **Shipping / transitional** | Full visible hand of first-wave staples + charges (**C62**/**C63**) — Bandage HUD etc. |
| **Long-term (C64)** | Constructed personal deck from shared library + Character signature card(s); opponent does not see deck/hand |

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
| **Personal deck** | Pre-match constructed list from the shared library (**C64** long-term). Sizing OPEN — see [`CARD_SYSTEM_OPENS.md`](CARD_SYSTEM_OPENS.md). |
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

## 5. Access model — transitional vs C64 hybrid

### 5.1 What is locked now

| Layer | Rule |
|-------|------|
| **Long-term (C64)** | Shared **library** + **personal deck** + Character **signature** card. Players **may bring different cards**. Deck/hand **hidden**. |
| **Shipping (C62/C63)** | First-wave staples on **transitional full-hand** — both sides see the same Bandage/Flashbang/Interact/Adrenaline set until a deckbuilder ships. Bandage numerics locked (C63). |
| **Attacker/Defender** | Still labels + spawns + Allot chooser — **not** role-locked kits (unchanged by C64). |

Conversation record: [`CARD_SYSTEM_MODEL_COMPARISON.md`](CARD_SYSTEM_MODEL_COMPARISON.md). Sizing OPENs: [`CARD_SYSTEM_OPENS.md`](CARD_SYSTEM_OPENS.md).

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

---

## 6. In-match economy

**Shipping (C62):** full hand every Program + per-card charges. Do not build draw RNG into the Bandage HUD path.

**Long-term (C64):** personal deck; draw vs always-have-constructed-hand is **OPEN** — answer in [`CARD_SYSTEM_OPENS.md`](CARD_SYSTEM_OPENS.md).

| Model | Role now |
|-------|----------|
| **1. Full hand every Program** | **Transitional shipping** (C62) |
| **2. Draw N each Program** | Long-term candidate (OPEN) |
| **3. Match loadout / constructed deck** | **C64 long-term** (sizing OPEN) |
| **4. Staples + flex** | Possible hybrid of transitional → C64; not locked |

**Adrenaline:** Execute-only, 1/match, stub effect until PLAYBACK_CONTRACT redesign.

**Time Card relation:** gear still burns **Time Resource** inside round **N** unless a future C# says otherwise for a specific signature.

---

## 6A. Strawman charge table (numerics OPEN — not CONFIRMED)

**Purpose:** a concrete starting point for the human §8 answers and for whoever eventually writes the `GhostResolver` resolve logic — not a locked spec. Every number below is a placeholder pulled from the same numeric family as existing verbs (GDD §6), marked **OPEN**.

| Gear | Phase (locked) | Time Resource cost (strawman) | Charges / match (strawman) | Cooldown | Resolve note |
|---|---|---|---|---|---|
| **Bandage** | Program | **3s** (~1.5× Snap Shot) | **1 per Character per match** | n/a | Heals Wounded → Healthy; must land before the *next* round starts (§4.2). Full bleed/surcharge system is separate future scope (**C46**, GDD §5 note) — do not build that alongside this strawman. |
| **Flashbang** | Program | **2s** (~1× Snap Shot) | **2 per match** | n/a | Soft control/vision — blast radius, duration, and what "soft control" *does* mechanically are all OPEN, not just the cost. Needs its own effect design, not only a numeric. |
| **Interact-as-card** | Program | **2–4s**, same family as Door's Strength-scaled open/close (GDD §6) | Unlimited **uses**, gated by a legal target existing (mirrors Door's `InteractRadius` gate, **C39**) | n/a | Closest to an existing resolve shape (Door) — lowest design risk of the four. Needs a real target (vent/monitor/station) to exist before it does anything; **not** meaningful until those exist per §4.4. |
| **Adrenaline** | **Execute only** (locked, not OPEN — GDD/`PLAYBACK_CONTRACT`) | Not time-budgeted the same way — it's a mid-cinema interrupt, not a Program-armed cost | **1 per match** (locked — GDD §4.2, `PLAYBACK_CONTRACT` §4) | n/a | Effect itself is **stub** today (`AdrenalineUsed` event, no mechanical change). Any real effect needs the explicit redesign `PLAYBACK_CONTRACT` §2 rule 5 requires before it can do more than log an event — this is a bigger question than "what number," flagged separately in §8 Q6. |

**What's locked vs open in this table:** phase column + Adrenaline 1×/match locked; **Bandage cost/charges locked by C63**. Flashbang/Interact numerics + Flashbang effect shape still OPEN (OPEN #16). Under C64 these four are **shared-library** candidates, not signatures.

---

## 7. Meta “collection” (binder) — separate from match hand

“Cards collection system” can mean three different products. Pick which we mean:

| Layer | Question | Monetization note |
|-------|----------|-------------------|
| **Rules catalog** | Which gear definitions exist in the game? | Gameplay — free |
| **Match access** | Which of those can I bring / draw this match? | Gameplay — free; must not paywall power (**C47**) |
| **Cosmetic binder** | Skins for gear / Time Card backs / sleeve art | Sellable if readability-safe |

**C64 / C47:** gameplay library + signatures stay free forever — no gacha unlock for power. Cosmetic binder (Time Card backs, gear skins) is the only sellable layer.

Ship order for *systems*: transitional staple resolve (Bandage…) → deckbuilder/hand rules (after [`CARD_SYSTEM_OPENS.md`](CARD_SYSTEM_OPENS.md)) → cosmetic binder.

---

## 8. Decision menu — C62 answers (2026-08-12) + C64 overlay

**§8 answered 2026-08-12 → C62.** **Long-term access model reopened and answered 2026-08-13 → C64** (see §10). New sizing questions live in [`CARD_SYSTEM_OPENS.md`](CARD_SYSTEM_OPENS.md), not here.

1. **Catalog scope for first gear wave** — **(a)** Bandage, Interact-as-card, Flashbang, Adrenaline only. No Otherwise card this wave. **Still holds under C64** (library candidates).
2. **Same deck?** — **C62 answered (a)** same legal list + Interact Strength carve-out. **C64 amends long-term:** personal decks may differ; signatures are Character-exclusive. **Transitional shipping still behaves like C62 full-hand staples.**
3. **In-match economy** — **C62 (a)** full hand + charges for first wave. **C64** makes constructed decks the long-term target; draw vs always-have = OPEN (`CARD_SYSTEM_OPENS.md`).
4. **Meta collection** — **(a)** no gameplay binder/unlock wall. **C64** restates: library + signatures free forever; cosmetics only (**C47**).
5. **Interact-as-card vs Door/Vent** — **(a)** Door/Vent/Breach stay map actions; Interact = future stations. **Still holds.**
6. **Adrenaline** — **(a)** Execute stub until PLAYBACK redesign. **Still holds.**

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

## 10. One-page direction — C62 shipping + C64 long-term

**Shipping / transitional (C62 + C63):**

- Catalog staples: Bandage, Flashbang, Interact-as-card, Adrenaline (stations later for Interact).
- Full visible hand + charges; Bandage 3s / 1× match locked.
- Interact cost may scale by Strength; Door/Vent/Breach stay map actions.
- Adrenaline Execute stub until PLAYBACK redesign.
- No gameplay unlock binder.

**Long-term (C64):**

- **Hybrid:** shared free library → personal decks; Character **signature** = unique verb armed by a card.
- Players may bring **different** cards; deck/hand **hidden**; Character pick public.
- Does **not** block Bandage HUD or other staple Sim/HUD under transitional full-hand.
- Deck size / draw / signature-in-hand / Reveal → [`CARD_SYSTEM_OPENS.md`](CARD_SYSTEM_OPENS.md).

---

## 11. Recommended first-ship sequence (proposal only)

Ranked by resolve risk for **library staples** under transitional full-hand (deckbuilder is a later systems layer per C64):

1. **Bandage** — **Sim landed (C63)**; HUD-side open in `contracts/CURRENT.md`.
2. **Interact-as-card** — Door-like resolve; blocked on a real station target.
3. **Flashbang** — new effect design (paused brief: `GEAR_FLASHBANG_AGENT_BRIEF.md`); treat as library tech under C64.
4. **Adrenaline (real effect)** — last; needs PLAYBACK_CONTRACT redesign.
5. **Deckbuilder + signatures** — after staple conventions exist; Bomber/Time Player still need C43/C44 prereqs.
6. **Otherwise library** — separate project after the four named cards.

---

## 12. Next human menu

→ [`CARD_SYSTEM_OPENS.md`](CARD_SYSTEM_OPENS.md) (deck size, copies, draw vs hand, signature availability, Reveal, role vs decks).

---

## See also

- [`PRODUCT_MEMORY.md`](PRODUCT_MEMORY.md) — C62, C63, **C64**, OPEN #16  
- [`CARD_SYSTEM_MODEL_COMPARISON.md`](CARD_SYSTEM_MODEL_COMPARISON.md) — hybrid conversation  
- [`CARD_SYSTEM_OPENS.md`](CARD_SYSTEM_OPENS.md) — live C64 sizing menu  
- [`GEAR_BANDAGE_AGENT_BRIEF.md`](GEAR_BANDAGE_AGENT_BRIEF.md) / [`GEAR_FLASHBANG_AGENT_BRIEF.md`](GEAR_FLASHBANG_AGENT_BRIEF.md)  
- [`GDD.md`](GDD.md) · [`UI_FLOW.md`](UI_FLOW.md) · [`PLAYBACK_CONTRACT.md`](PLAYBACK_CONTRACT.md) · [`MONETIZATION.md`](MONETIZATION.md)  
- [`CHARACTER_ROSTER_LONGTERM.md`](CHARACTER_ROSTER_LONGTERM.md) — Bomber / Time Player  

