# Card Collection & Gear Deck — Design Research

**Status:** Research + **CONFIRMED** direction via **C62** (2026-08-12). §8 answers promoted to `PRODUCT_MEMORY.md`. Gear **numerics** (§6A) and Adrenaline real-effect redesign remain OPEN.  
**Worktree:** `D:\projects\Game\logiCard-cards-collection` / `feat/cards-collection-docs`  
**Depends on:** [`PRODUCT_MEMORY.md`](PRODUCT_MEMORY.md) **C15**, **C18**, **C33**, **C34**/**C46**, OPEN #3; [`GDD.md`](GDD.md) § deferred cards; [`CORE_LOOP.md`](CORE_LOOP.md); [`UI_FLOW.md`](UI_FLOW.md); [`MONETIZATION.md`](MONETIZATION.md) **C47**; [`CHARACTER_ROSTER_LONGTERM.md`](CHARACTER_ROSTER_LONGTERM.md).

---

## 1. Why this doc exists

The live ship still runs **base verbs only** (Move, Shoot, Door) plus the **Time Card** allotment UI. Gear cards are named in GDD/UI_FLOW but never specified as a collection, deck-build, or per-character kit. Before any HUD strip or Sim verb lands, we need answers to:

1. What cards exist in the product (catalog)?
2. Does each Character own a **different** kit, or do both sides share the **same** gear set?
3. How does a player **hold / draw / spend** cards inside a match (economy)?
4. Is there a **meta collection** (unlock / inventory / cosmetics) separate from the in-match hand?

This file proposes a taxonomy and a short decision menu. It does **not** implement code.

---

## 2. Locked facts (do not re-litigate)

| ID | Fact |
|----|------|
| **C15** | Move + Shoot = **base verbs**; cards = **gear**; Characters differ by attrs (and later unique verbs — roster doc). |
| **C18** | Attacker/Defender are labels + spawns; **same gear deck** (both sides use the same gear set — not role-locked kits). |
| **C33** | **Time Card** commits match-pool seconds. It is a **round-allotment commit**, **not** a gear card. |
| **C21 / C25** | There is **no Walk card**, **no Snap/Hold card**. Stance and shoot mode are direct picks on base verbs. |
| **C34 / C46** | Named gear (Bandage / Flashbang / Adrenaline / Interact-as-card) + Otherwise library are **roadmap**, not current core-loop ship. C46 removed the artificial “after demo” calendar gate but did **not** auto-promote gear into active build. |
| **PLAYBACK_CONTRACT** | Adrenaline today = Execute-only UI gate + **stub** effect; mid-Playback resolve that mutates the armed tape needs an explicit redesign. |
| **C47** | F2P cosmetic-only IAP. **Gameplay cards / unique verbs are not sellable power.** Time Card *backs* may be cosmetic. |
| **OPEN #3** | **Card economy** (full hand vs draw each Program) is explicitly parked with gear under C34-era deferral. |

**Implication of C18:** the first answer to “does each character own different cards?” for **Attacker vs Defender** and for **Scout vs Juggernaut as attribute twins** is already: **same gear deck**. Per-character *unique cards* would be a **new** product decision (closer to unique-verb roster / loadout packs) and would need to amend or sit beside C18 carefully.

---

## 3. Vocabulary (proposed)

Use these names in future docs so “card” stops meaning four different things:

| Term | Meaning |
|------|---------|
| **Time Card** | Allotment UI + match-pool commit (**C33**). Not gear. Cosmetic backs OK (**MONETIZATION**). |
| **Character Card** | Pre-match Scout / Juggernaut (attrs). Not a playable gear card. |
| **Gear card** | Schedulable (or Execute-gated) item that spends Time Resource and/or a charge — Bandage, Flashbang, etc. |
| **Otherwise card** | Failure / contingency library (e.g. stop-before-block rules today are simplified; full library is roadmap). |
| **In-match hand** | What the player can arm during Program (or Execute for Adrenaline). |
| **Collection / binder** | Meta inventory of owned gear definitions + cosmetic skins — **OPEN**, not designed. |
| **Loadout** | Pre-match subset of the collection brought into a match — **OPEN**; conflicts with “same gear deck” if loadouts diverge by Character unless loadouts are identical for all. |

---

## 3A. Player-facing glossary (one-page)

**Status:** Draft strawman for a future in-game help/tooltip screen. Wording is a starting point for UI copy, not confirmed voice/tone.

Plain-language versions of §3's dev vocabulary — what a player would actually read in a tooltip or help screen, not what this doc's authors call it internally.

| Term (player-facing) | What it means to the player |
|---|---|
| **Character** | Who you're playing — Scout or Juggernaut today. Changes how fast you move and how quickly you handle doors and gear. Doesn't change what gear you can bring. |
| **Time Card** | The card you play each round to commit part of the shared match clock. Bigger commitment = more time to act this round, but your opponent plans against that same window. |
| **Program** | The planning phase — you secretly draw your path, aim your shots, and place any gear. Nobody sees your plan until it plays out. |
| **Gear** | Tools like Bandage or Flashbang. Every Character has access to the same gear — there's no "Scout-only" or "Juggernaut-only" item. |
| **Hand** | The gear you can use this round. |
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
| Bomber / Time Player unique verbs | CHARACTER_ROSTER_LONGTERM | Character verbs, **not** shared gear — monetization: free gameplay |

**No additional gear names are invented here.** Expanding the catalog is an explicit human call.

---

## 5. Same deck vs per-character kits

### 5.1 What is already locked

**C18 → same gear deck** for both sides (Attacker/Defender). Scout and Juggernaut today share verbs and differ by **attrs only**, so the natural reading is:

> Both Characters bring the **same legal gear list** into a match. Character fantasy comes from Speed / Agility / Strength (and later unique verbs), **not** from exclusive Bandage-vs-Flashbang kits.

### 5.2 Options if we reopen “who owns which cards?”

| Option | Description | Fits C18? | Risk |
|--------|-------------|-----------|------|
| **A. Shared universal deck (default)** | Every Character may use every gear card in the catalog. Attrs change *how well* you use time/doors, not *which* cards exist. | Yes | Gear can feel generic; Character identity stays numeric |
| **B. Shared deck + Character modifiers** | Same cards, but costs/charges scale with attrs (e.g. Juggernaut Bandage faster, Scout Flashbang cheaper). | Yes (same *set*) | Needs numerics; easy to accidentally P2W if sold |
| **C. Per-Character exclusive cards** | Scout-only / Juggernaut-only gear | **No** — needs new C# amending C18 | Splits balance matrix; loadout + roster explosion |
| **D. Shared deck + unique-verb Characters** | Scout/Juggernaut share gear; Bomber’s bomb is a **verb**, not a gear card | Yes for gear; unique verbs separate | Keep gear vs verb boundary sharp (C15) |

**Recommendation for first gear ship (research only):** **A**, optionally light **B**. Keep **C** out unless human explicitly wants to amend C18. Put Bomber/Time Player power in **verbs**, not exclusive gear packs (**D**).

---

## 5A. "Same gear deck" vs unique-verb Characters — the boundary, spelled out

§5 covers *whether Characters share a gear list*. This section is the sharper rule for telling **gear** apart from **unique Character verbs** (Bomber's bomb, Time Player's rewind — `CHARACTER_ROSTER_LONGTERM.md`, **C42–C44**) once both exist in the same product, since they will otherwise look like the same kind of thing to a player.

**The test:** if the capability exists in a match **regardless of which Character either player picked**, it's gear. If it exists **only because a specific Character was picked**, it's a unique verb.

| | Gear card | Unique Character verb |
|---|---|---|
| **Example** | Bandage, Flashbang, Interact-as-card, Adrenaline | Bomber's bomb-place, Time Player's rewind (**C43/C44**, long-term, not active build) |
| **Who has access** | Every Character, every match (**C18**) | Only a player who picked that specific Character |
| **What changes it** | Nothing — access is universal; attrs may scale *cost/speed* (§5.2 option B), never *legality* | Character selection itself |
| **Doc home** | This file → future `GEAR_CATALOG.md` | `CHARACTER_ROSTER_LONGTERM.md` |
| **C15 boundary** | "cards = gear" | "Characters differ by attrs (and later unique verbs)" |
| **Resolve shape** | Schedulable event, same event-stream discipline as Move/Shoot/Door | Same event-stream discipline (**C42**'s requirement), but the verb itself doesn't exist for a Character who didn't pick it |
| **Monetization** | Free gameplay; only the *skin* is sellable (**C47**, `MONETIZATION.md`) | Must ship free or skill-gated — never paywalled (`MONETIZATION.md` §Cross-reference) |

**Why this matters for a future catalog:** adding a new gear card is a content addition inside the existing "same deck" rule — no roster change needed. Adding a unique verb is a **roster decision** that touches `CHARACTER_ROSTER_LONGTERM.md`, needs its own C# row, and (per **C42**) must still resolve through the deterministic event-stream or get an architecture pass first. Don't let a new gear-card proposal quietly grow into a "well, what if only Juggernaut gets it" unique-verb proposal without recognizing that's a different, bigger decision (§5.2 option C, explicitly **not** recommended without a human amendment to C18).

---

## 6. In-match economy (OPEN #3)

Until this is confirmed, do not build draw RNG into Program HUD.

| Model | How it plays | Pros | Cons |
|-------|--------------|------|------|
| **1. Full hand every Program** | All owned/legal gear visible each round; spend charges / once-per-match limits | Readable; no RNG; matches “toy cards on the desk” | Clutter; strong cards need hard charges |
| **2. Draw N each Program** | Draw from deck into hand; discard / reshuffle rules | Card-game feel; variance | Hidden info + RNG fights blind-program clarity; harder to teach |
| **3. Match loadout, fixed charges** | Pre-match pick ≤K cards from catalog; charges persist across rounds | Collection/loadout meta; still deterministic in-round | Loadout screen; can violate “same deck” if picks differ a lot |
| **4. Hybrid** | Small fixed staples (e.g. Bandage always) + 1 flex slot | Identity + simplicity | Still a loadout UI |

**Adrenaline special case:** already specified as **Execute-only**, **1/match**, not Program-armed. Whatever economy ships must keep that split or explicitly rewrite PLAYBACK_CONTRACT.

**Time Card relation:** spending gear still burns **Time Resource** inside round **N** (or a free Execute interrupt for Adrenaline — TBD). Gear must not invent a second budget without a C# row.

---

## 6A. Strawman charge table (numerics OPEN — not CONFIRMED)

**Purpose:** a concrete starting point for the human §8 answers and for whoever eventually writes the `GhostResolver` resolve logic — not a locked spec. Every number below is a placeholder pulled from the same numeric family as existing verbs (GDD §6), marked **OPEN**.

| Gear | Phase (locked) | Time Resource cost (strawman) | Charges / match (strawman) | Cooldown | Resolve note |
|---|---|---|---|---|---|
| **Bandage** | Program | **3s** (~1.5× Snap Shot) | **1 per Character per match** | n/a | Heals Wounded → Healthy; must land before the *next* round starts (§4.2). Full bleed/surcharge system is separate future scope (**C46**, GDD §5 note) — do not build that alongside this strawman. |
| **Flashbang** | Program | **2s** (~1× Snap Shot) | **2 per match** | n/a | Soft control/vision — blast radius, duration, and what "soft control" *does* mechanically are all OPEN, not just the cost. Needs its own effect design, not only a numeric. |
| **Interact-as-card** | Program | **2–4s**, same family as Door's Strength-scaled open/close (GDD §6) | Unlimited **uses**, gated by a legal target existing (mirrors Door's `InteractRadius` gate, **C39**) | n/a | Closest to an existing resolve shape (Door) — lowest design risk of the four. Needs a real target (vent/monitor/station) to exist before it does anything; **not** meaningful until those exist per §4.4. |
| **Adrenaline** | **Execute only** (locked, not OPEN — GDD/`PLAYBACK_CONTRACT`) | Not time-budgeted the same way — it's a mid-cinema interrupt, not a Program-armed cost | **1 per match** (locked — GDD §4.2, `PLAYBACK_CONTRACT` §4) | n/a | Effect itself is **stub** today (`AdrenalineUsed` event, no mechanical change). Any real effect needs the explicit redesign `PLAYBACK_CONTRACT` §2 rule 5 requires before it can do more than log an event — this is a bigger question than "what number," flagged separately in §8 Q6. |

**What's locked vs open in this table:** the *phase* column (Program vs Execute-only) and Adrenaline's 1×/match cap are already decided (GDD, `PLAYBACK_CONTRACT`). Everything else — costs, charge counts, Flashbang's actual effect, Bandage's heal timing edge cases — is this doc's strawman guess, explicitly awaiting the human §8 answers (particularly Q1 catalog scope and Q6 Adrenaline effect design) before anyone treats it as buildable.

---

## 7. Meta “collection” (binder) — separate from match hand

“Cards collection system” can mean three different products. Pick which we mean:

| Layer | Question | Monetization note |
|-------|----------|-------------------|
| **Rules catalog** | Which gear definitions exist in the game? | Gameplay — free |
| **Match access** | Which of those can I bring / draw this match? | Gameplay — free; must not paywall power (**C47**) |
| **Cosmetic binder** | Skins for gear / Time Card backs / sleeve art | Sellable if readability-safe |

**Recommendation (research):** ship order should be **rules catalog → in-match economy → cosmetic binder**. Do not build a gacha collection UI before the four named gear cards have resolve rules.

If a Steam “collection” page is desired for marketing, it can start as **cosmetic Time Card backs + future gear skins**, not as purchasable Bandage unlocks.

---

## 8. Decision menu for human (please answer)

Reply with letters / short notes; Integrator will promote confirmed answers into PRODUCT_MEMORY.

**Answered by human in chat, 2026-08-12.** Each answer below is Cards dept's recommendation, accepted as-is. This resolves the doc's open questions; it does **not** by itself amend `PRODUCT_MEMORY.md` — that still needs an Integrator-written C# row per the save-file rule (`PRODUCT_MEMORY.md` §How to update: "Confirm in chat → edit C# row").

1. **Catalog scope for first gear wave** — **Answered: (a).** Only the four named: Bandage, Interact-as-card, Flashbang, Adrenaline. No Otherwise card in this wave — Otherwise is a family (§4.3), not a single card, and scoping it in now turns a bounded 4-item wave into an open-ended one.
   - ~~(b) Four + a minimal Otherwise rule card~~ — rejected, see above.
   - ~~(c) Larger catalog~~ — rejected; no new names invented per brief.

2. **Same deck?** — **Answered: (a), with one scoped carve-out.** Keep **C18** — every Character has the same legal gear list, no exclusives. Carve-out: **Interact-as-card**'s cost may scale by Strength, mirroring the existing Door open/close precedent (GDD §6 — Scout slower, Juggernaut faster) it's generalizing. Bandage and Flashbang stay flat-cost for everyone unless playtest indicates otherwise — this is not a blanket adoption of §5 option B, just applying an existing precedent where the card is a direct generalization of an already-scaled action.
   - ~~(b) Amend C18 — per-Character exclusives~~ — rejected.
   - ~~(c) Shared cards + attr-scaled costs, unscoped~~ — rejected as a blanket rule; see carve-out above instead.

3. **In-match economy** — **Answered: (a).** Full hand every Program + per-card charges (§6 model 1). No draw/RNG (fights blind-programming clarity) and no pre-match loadout screen prerequisite (§6 models 2–4 all rejected for the first wave).

4. **Meta collection** — **Answered: (a).** None this milestone — rules catalog only, no binder/UI. Revisit **(b)** cosmetic binder once `MONETIZATION.md`'s Phase 4 IAP skeleton lands. **(c)** unlock-to-use gear is rejected outright, not just deferred — gates gameplay cards behind grind/pay in a PvP game, which sits too close to C47's no-pay-to-win line even if the letter of C47 (cosmetic-only) doesn't technically forbid a free-only unlock grind.

5. **Interact-as-card vs contextual Door/Vent** — **Answered: (a).** Keep Door/Vent/Breach exactly as shipped (C57's `Door.Kind` reskin); Interact-as-card is reserved for **future stations that don't exist yet** (monitor / terminal / power station — `docs/UI_BOARD_ANCHORED_COMPONENTS.md`). Do not migrate any existing map interact onto the hand.

6. **Adrenaline** — **Answered: (a).** Keep the Execute-only stub. A real effect needs the explicit tape-branch/second-resolve redesign `PLAYBACK_CONTRACT.md` §2 rule 5 requires — that's an architecture decision, sequenced as its own pass *after* the other three cards ship (§11), not bundled into this wave.

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

## 10. One-page default — now the human-confirmed direction (§8, 2026-08-12)

What was a strawman guess is now what the human answered in §8:

- **Catalog:** Bandage, Flashbang, Adrenaline, Interact-as-card (stations later). No Otherwise card this wave.
- **Ownership:** **Same gear deck** for all Characters (**C18**), with one carve-out — Interact-as-card's cost may scale by Strength (Door precedent). Bandage/Flashbang stay flat.
- **Economy:** Full visible hand + per-card charges / Adrenaline 1× match (model **1**).
- **Collection:** No unlock wall, no binder UI this milestone — definitions only; cosmetic Time Card backs are a later Phase 4 item, not built now.
- **Interact-as-card scope:** future stations only; Door/Vent/Breach stay untouched.
- **Adrenaline:** stub stays a stub; real effect is a separate later design pass.
- **Unique roster power:** verbs (Bomber / Time Player), not exclusive gear.

**CONFIRMED as C62** — binding product direction. Numerics in §6A and Adrenaline redesign remain OPEN (see PRODUCT_MEMORY OPEN #16).

---

## 11. Recommended first-ship sequence (proposal only)

If/when the human confirms a first gear wave (§8 Q1), this is the suggested **build order**, ranked by resolve risk and reuse of existing shapes — not by "most exciting first":

1. **Interact-as-card** — reuses the Door contextual-action resolve shape almost exactly (`TryGetNearestDoor`-style radius gate, GDD §4/§6A above). Lowest new-mechanic risk. **Caveat:** per §4.4/§8 Q5, it isn't meaningful without a real target (vent/monitor/station) to interact with — may need to ship *after* a station exists, not before, despite being mechanically simplest.
2. **Bandage** — new state mutation (Wounded → Healthy) but on the existing wound ladder (GDD §5), Program-armed like Move/Shoot, no new targeting model. Second-lowest risk.
3. **Flashbang** — needs an actual new effect (soft control / vision), not just a state flip. Higher design risk than Bandage because "what does soft control do" is still an open mechanical question, not only a numeric one (§6A).
4. **Adrenaline (real effect)** — stays **stub** until last. It's Execute-only, mid-cinema, and `PLAYBACK_CONTRACT` §2 rule 5 requires an explicit redesign (tape branch or second resolve) before it can be more than a logged event. Building this before the other three establishes conventions risks having to redo it.
5. **Otherwise library** — treat as its own project after all four named cards, not a fifth card. It's a family (§4.3), and today's simplified "stop before block" behavior already covers the ship's actual need.

This sequence is **not confirmed** and assumes the §10 strawman defaults (catalog = the four named cards, same gear deck, full-hand economy). If the human answers §8 differently — e.g. picks pre-match loadouts (§6 model 3) instead of full-hand — this ordering may need to change, since loadout UI would become a prerequisite for shipping *any* of the four rather than an independent step.

---

## See also

- [`PRODUCT_MEMORY.md`](PRODUCT_MEMORY.md) — C15, C18, OPEN #3  
- [`GDD.md`](GDD.md) § deferred cards  
- [`CORE_LOOP.md`](CORE_LOOP.md) — base verbs vs future cards  
- [`UI_FLOW.md`](UI_FLOW.md) § Program / Execution card UX  
- [`PLAYBACK_CONTRACT.md`](PLAYBACK_CONTRACT.md) — Adrenaline gate  
- [`MONETIZATION.md`](MONETIZATION.md) — cosmetic Time Card backs; no pay-to-win gear  
- [`CHARACTER_ROSTER_LONGTERM.md`](CHARACTER_ROSTER_LONGTERM.md) — unique verbs ≠ gear exclusives  
