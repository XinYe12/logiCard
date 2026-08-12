# Card Collection & Gear Deck — Design Research

**Status:** Research draft (2026-08-12). **Not CONFIRMED** — nothing here may be treated as locked until human confirm → `PRODUCT_MEMORY.md` C# row (save-file rule).  
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

1. **Catalog scope for first gear wave**  
   - (a) Only the four named: Bandage, Interact-as-card, Flashbang, Adrenaline  
   - (b) Four + a minimal Otherwise rule card  
   - (c) Larger catalog (list names)

2. **Same deck?**  
   - (a) Keep **C18** — Scout/Juggernaut identical gear legality  
   - (b) Amend C18 — per-Character exclusives (describe)  
   - (c) Shared cards + attr-scaled costs (**§5 option B**)

3. **In-match economy**  
   - (a) Full hand every Program + charges  
   - (b) Draw each Program  
   - (c) Pre-match loadout ≤K  
   - (d) Hybrid (specify staples)

4. **Meta collection**  
   - (a) None this milestone — definitions only  
   - (b) Cosmetic binder only (Time Card / gear skins)  
   - (c) Unlock-to-use gear (⚠️ P2W risk — needs strong free path or reject)

5. **Interact-as-card vs contextual Door/Vent**  
   - (a) Keep Door/Vent/Breach as map actions; Interact-card is for *future* stations only  
   - (b) Migrate some map interacts onto the hand  
   - (c) Defer Interact-card entirely until stations exist

6. **Adrenaline**  
   - (a) Keep Execute-only stub until effect design exists  
   - (b) Design effect now (must say whether tape may branch)

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

## 10. One-page default (if human wants a strawman)

Until overridden:

- **Catalog:** Bandage, Flashbang, Adrenaline, Interact-as-card (stations later).  
- **Ownership:** **Same gear deck** for all Characters (**C18**).  
- **Economy:** Full visible hand + per-card charges / Adrenaline 1× match (model **1**).  
- **Collection:** No unlock wall; cosmetic Time Card backs only for meta “collection” feel.  
- **Unique roster power:** verbs (Bomber / Time Player), not exclusive gear.

This strawman is **still not CONFIRMED**.

---

## See also

- [`PRODUCT_MEMORY.md`](PRODUCT_MEMORY.md) — C15, C18, OPEN #3  
- [`GDD.md`](GDD.md) § deferred cards  
- [`CORE_LOOP.md`](CORE_LOOP.md) — base verbs vs future cards  
- [`UI_FLOW.md`](UI_FLOW.md) § Program / Execution card UX  
- [`PLAYBACK_CONTRACT.md`](PLAYBACK_CONTRACT.md) — Adrenaline gate  
- [`MONETIZATION.md`](MONETIZATION.md) — cosmetic Time Card backs; no pay-to-win gear  
- [`CHARACTER_ROSTER_LONGTERM.md`](CHARACTER_ROSTER_LONGTERM.md) — unique verbs ≠ gear exclusives  
