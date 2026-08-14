# Character — Human Decision Sheet (C42 / C43 / C44 / Attrs)

**Status:** Ready for human answers — 2026-08-14.
**Purpose:** Every OPEN question from the four Character implementation briefs, in one pass, so the
human can answer once instead of re-reading four docs. **Nothing here is locked.** Recommended
defaults are carried over verbatim from each brief's own "recommendation, not locked" language —
this sheet does not invent new numerics or resolve-shape opinions.
**What happens after:** Human fills in "Decision" column (or writes free text) → Integrator promotes
answers into `PRODUCT_MEMORY.md` as C-numbered rows → Integrator opens the relevant Sim-pause
carve-out(s) → Character/UI contracts can open against frozen signatures.
**Out of scope for this sheet:** two other open items already tracked on
[`STATUS.md`](STATUS.md) — `CHARACTER_FANTASY.md` §6 (roster naming/pitches) and
`CHARACTER_TIME_PLAYER_EPISTEMICS.md` §4 (same fast-forward question as Part C Q1 below, from a
narrative-framing angle) — answer those alongside this sheet if convenient, but they aren't
re-derived here.

---

## Part A — Shared gates for *any* unique verb (C42, applies to Bomber + Time Player both)

| # | Question | Recommended default (brief's own words) | Decision |
|---|----------|------------------------------------------|----------|
| A1 | **Promotion gate.** Which `SCHEDULE.md` phase (or Integrator carve-out) is allowed to open the **first** unique-verb Sim contract — and is that Bomber, Time Player, or a thinner vertical slice (e.g. wall-only geometry breach without floor-drop)? | *No default offered — this is the human's call to sequence.* | |
| A2 | **Data shape on Character.** How does a Character declare its unique verb(s)? | Not decided between: (a) ScriptableObject ability-id list on `CharacterData`, (b) hardcoded archetype→verb map in Boot, (c) separate `CharacterAbility` assets. Brief leans toward (a) for consistency with existing asset-driven attrs but does not lock it. | |
| A3 | **`ActionVerb` vs parallel channel.** Grow the `ActionVerb` enum (one value per unique verb, e.g. `BombAttach`) vs. a side-band "ability node" list on the payload? | **Grow `ActionVerb`** — matches existing Door-style verb-driven resolve, keeps Host sort/filter simple, avoids inventing a second timeline. | |
| A4 | **Program HUD affordance ownership.** When Character (rules) and UI (chrome) are both hot on the same unique-verb slice, who owns the mode button / board-anchored prompt? | **Integrator freezes the node signature → Character proposes resolve → UI implements Program-arm UI against the frozen signature** (same split as Bandage). | |
| A5 | **Monetization check at promotion.** | Explicit Integrator+human checklist item at merge time: confirm the unlock path is free/skill-gated before merge — don't leave it to store copy later. *(Process confirmation, not a design fork — flag if you want it handled differently.)* | |

**Prerequisite the human should see before answering A1:** per C42 §4, the recommended build order is
(1) freeze this Part A, (2) land C36 geometry-breach primitives (or a scoped subset — both Bomber and
Time Player depend on it), (3) then open Bomber and/or Time Player contracts, (4) per-floor occupancy
only if Bomber's floor-drop is in the same contract. C36 itself is not one of these four briefs and has
its own OPEN #6 in `PRODUCT_MEMORY.md`.

---

## Part B — Bomber (C43)

| # | Question | Recommended default (brief's own words) | Decision |
|---|----------|------------------------------------------|----------|
| B1 | **Attach vs detonate — one node or two?** Roster assumes two scheduled events (mirrors Door Open/Close). If two: can another Character's Move/Shoot interact with an attached-but-live bomb? Can Time Player rewind an attached bomb? | Two nodes (assumed, not confirmed). Interaction sub-questions have **no default offered**. | |
| B2 | **Cost strawman.** | Attach ~3s, Detonate ~1s — **recommendation only**, replace freely. | |
| B3 | **Blast footprint.** Exact attached-surface footprint only, or a radius that can wound/breach adjacent points? | Footprint-only — the floor-drop rule as written assumes footprint, not radius (radius would expand collision tests). | |
| B4 | **Attach eligibility.** Every floor/wall sample vs. designed breach points only. | **Designed points only** — matches C36's scoped destruction, avoids freeform mesh cutting. | |
| B5 | **Walls too, or floor-drop only?** If walls-only ships first, floor-drop and per-floor infra can defer. | *No default offered* — brief flags that a walls-only-forever Character may not deserve the "Bomber" name; human call. | |
| B6 | **Fall transition.** Instant teleport to equivalent XY one floor down vs. scheduled fall duration; does fall cancel the victim's remaining queue (mirrors the death-freeze precedent); can landing wound? | *No default offered* on any of the three sub-questions. | |
| B7 | **Who can be dropped?** Enemy only, any pawn (incl. self/ally), objects? | Brief leans toward "drop is a geometry consequence, not a targeted attack" (aligns with blind-programming / free-aim philosophy) — implies **any pawn in the footprint**, not enemy-only, but this is inference, not a stated default. | |
| B8 | **Cross-round state.** Do attached-undetonated bombs and geometry breach state carry across rounds like wounds (C33/C36), or must a bomb detonate the same round it's attached? | *No default offered.* | |
| B9 | **Prerequisite ordering.** Must C36 geometry breach merge before any Bomber contract opens, or may one Integrator contract deliver "breach points + Bomber verb" together? | *No default offered* — Integrator sequencing call, informed by A1. | |

---

## Part C — Time Player (C44)

| # | Question | Recommended default (brief's own words) | Decision |
|---|----------|------------------------------------------|----------|
| C1 | **Fast-forward survival — blocks buildability, answer this first.** | Four options: **(A)** rewind only, cut FF for v1. **(B)** FF allowed but only for deterministic transitions already scheduled by public board rules or the Time Player's own nodes — never peeks opponent payload. **(C)** FF only as a resolve-time booking (no Program-time preview), same info-opacity as booking a Door open. **(D)** other, write into PRODUCT_MEMORY once confirmed. Brief recommends **(A) or (C)** to start discussion — flags (B) as easy to get wrong in HUD copy. | |
| C2 | **Cost strawman.** | Rewind ~4s / Fast-forward ~4s if it survives C1 — **recommendation only**. | |
| C3 | **Range / LoS.** Unrestricted vs. Door-like `InteractRadius` vs. Shoot-like LoS to the object. | **Radius interact (Door family)** — keeps it an object verb, not a sniper verb. | |
| C4 | **Object scope v1.** Breach geometry only vs. also Standard/Vent doors vs. attached Bomber bombs vs. C38 pawn states. | **Geometry breach states only for v1** — Door rewind collides with Door UX expectations, pawn-state rewind collides with C38 revive design. | |
| C5 | **What "timeline" means.** Discrete ±1 state step vs. replay of the object's last N match events. | **±1 along the defined state machine** — no general rewind clock. | |
| C6 | **Illegal targets.** Already-Intact (rewind) / already-terminal (FF) objects; object mid-transition from another node the same second — Program-time gate (can't even queue it) vs. resolve-time no-op? | *No default offered.* | |
| C7 | **Interaction with Bomber** (only matters if both Characters ship). Can Time Player rewind a floor mid-fall? Rewind an attached-but-undetonated bomb? | **Default: out of v1 scope** — flagged as cross-brief, not designed here. | |

**Prerequisite:** per C44 §4, Time Player's primary example cannot ship before C36 breach primitives
exist (same as Bomber). Do not open Time Player against doors-only as a substitute fantasy unless the
human explicitly re-scopes the Character.

---

## Part D — Scout / Juggernaut attrs (current cast, *not* a unique-verb operator)

| # | Question | Recommended default (brief's own words) | Decision |
|---|----------|------------------------------------------|----------|
| D1 | **Doc numeric authority.** GDD §2 (1.5 / 0.75 units/s) and §6 (1.0s / 2.0s per unit) disagree — reciprocals only if Walk baseline is the rate, but §6's 1.0s/unit ≡ 1.0 u/s, not 1.5. Which wins? | **§6 + the live assets win** (1s / 2s per unit) — Integrator amends §2 on confirm. | |
| D2 | **Wire Agility now or later?** C25 (stance-change / Snap↔Hold switch penalty) is design-locked but the fields are read nowhere in `Assets/_Project` except the ScriptableObject assets themselves. Is fixing that a narrow bug-fix Sim carve-out, or does it wait for a full Character-attrs contract? | *No default offered* — flagged only as "confirm before treating as a quick fix," since attrs wiring is Sim/Timeline-adjacent and needs an explicit carve-out either way. | |
| D3 | **When does the penalty apply?** Sprint-only vs. any stance change; Snap↔Hold every switch vs. only the first switch per round. | C25 says "once when switching" / "same shape as the stance-change penalty" — brief's reading: **charge each switch event that matches the rule, not once per match.** Confirm this reading. | |
| D4 | **Character Select → live attrs.** Does `SelectedArchetype` actually drive `PawnProgram` construction for the local player? For a local 2nd pawn / scripted defender? For future net opponents? | *No default offered* — brief flags this as an unaudited wiring gap (`GameBootstrap` currently hardcodes attacker/defender speeds rather than loading `Scout.asset`/`Juggernaut.asset` by reference). | |
| D5 | **Vent / Breach doors and Strength.** Do Vent/Breach doors use the same `doorInteractBaseSeconds` as Standard doors, or kind-specific costs? | *No default offered* — map feature, but Strength is Character-owned, so flagged here. | |
| D6 | **Interact-as-card Strength carve-out (C62).** Not this brief's implement scope — but should a future attrs contract be constrained not to rename/remove `doorInteractBaseSeconds` in a way that blocks that later gear hook? | Brief's stance: **yes, preserve the field/shape** so C62's later hook isn't blocked. Flagging for explicit sign-off since it constrains implementation freedom now for a not-yet-built feature. | |

---

## Not covered here (already answered, or tracked elsewhere — do not re-litigate)

- Unique verbs resolve through the deterministic Host event-stream, no physics calls (C42, locked).
- Unique-verb Characters ship free/skill-gated, never paywalled (C42/monetization guardrail, locked).
- Bomber/Time Player get no exclusive gear — their difference is the verb (C62, locked).
- `DoorKind.Breach` (map shortcut door) is unrelated to C36 geometry breach — naming collision only,
  not a design question (all three briefs flag this).
- Scout/Juggernaut share Move/Shoot/Door; only attr magnitudes differ (GDD §2/§6, locked — see D1 for
  the one remaining numeric-authority conflict).

---

## See also

- [`CHARACTER_UNIQUE_VERB_OPERATORS_BRIEF.md`](../../CHARACTER_UNIQUE_VERB_OPERATORS_BRIEF.md) — Part A source
- [`CHARACTER_BOMBER_AGENT_BRIEF.md`](../../CHARACTER_BOMBER_AGENT_BRIEF.md) — Part B source
- [`CHARACTER_TIME_PLAYER_AGENT_BRIEF.md`](../../CHARACTER_TIME_PLAYER_AGENT_BRIEF.md) — Part C source
- [`CHARACTER_ATTRS_SCOUT_JUGGERNAUT_BRIEF.md`](../../CHARACTER_ATTRS_SCOUT_JUGGERNAUT_BRIEF.md) — Part D source
- [`STATUS.md`](STATUS.md) — other outstanding human-facing items (Fantasy §6, Epistemics §4)
- `PRODUCT_MEMORY.md` — where confirmed answers get promoted to C-numbered rows
