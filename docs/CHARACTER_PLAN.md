# Character Department — Pre-Code Plan

**Status:** Active docs/concepts mode (2026-08-13). Human chose **no Character coding** until
plans/concepts are fuller. Sim pause unchanged; no resolver work from this seat.
**Owner:** Character dept (`logiCard-char-select-motion`).  
**Not owner:** Character Select chrome (UI); gear catalog (Cards); Host resolve contracts (Integrator).

This file is the **map** of Character design work: what already exists, what still needs concept
depth, what must be true before any Sim carve-out, and how Cards/UI/Integrator fit. It does **not**
lock numerics or promote C42–C44 into active build scope.

---

## 1. Mode lock (2026-08-13)

| Do | Don't |
|----|--------|
| Deepen fantasy, boundaries, prerequisites, open-question trees | Edit `GhostResolver` / `PawnProgram` / new `ActionVerb`s |
| Write concept docs + implementation briefs | Assume Phase 5 art pause implies Character Sim is open |
| Flag Integrator/Cards/UI asks in STATUS | Merge Char Select or touch `CharSelect*` tokens |
| Keep unique verbs as **verbs**, not gear | Invent Scout-only gear packs (fights **C18** / **C62**) |

Exit from this mode = human says coding may start **and** Integrator opens an explicit carve-out /
contract. Docs alone never start Sim work.

---

## 2. Doc map (what exists today)

### Design source (binding / long-term)

| Doc | Role |
|-----|------|
| `GDD.md` §2 / §6 | Live cast attrs (Scout / Juggernaut) |
| `PRODUCT_MEMORY.md` **C15**, **C17**, **C25**, **C38**, **C42–C44**, **C62** | Binding rows |
| `CHARACTER_ROSTER_LONGTERM.md` | Bomber / Time Player design + monetization guardrail |
| `VISION.md` | Blind-programming pillar; unique-verb roster as long-term system |
| `CARD_COLLECTION.md` §5 / §5A / §8 | Same deck + gear↔verb boundary (**C62**) |
| `MONETIZATION.md` | Unique verbs not sellable power |

### Implementation briefs (recommendation-not-contract)

| Doc | Role |
|-----|------|
| `CHARACTER_UNIQUE_VERB_OPERATORS_BRIEF.md` | **C42** shared architecture gates |
| `CHARACTER_BOMBER_AGENT_BRIEF.md` | **C43** |
| `CHARACTER_TIME_PLAYER_AGENT_BRIEF.md` | **C44** |
| `CHARACTER_ATTRS_SCOUT_JUGGERNAUT_BRIEF.md` | Live attrs + unwired Agility finding |

### Concept pack (this wave — plans before code)

| Doc | Role |
|-----|------|
| **This file** | Pre-code roadmap + readiness gates |
| `CHARACTER_FANTASY.md` | Who each Character *is* in the heist fantasy |
| `CHARACTER_CARDS_BOUNDARY.md` | Joint Character↔Cards seam (attrs × gear, verb ≠ card) |
| `CHARACTER_C36_DEPENDENCY.md` | What Character needs from Core before unique verbs |
| `CHARACTER_TIME_PLAYER_EPISTEMICS.md` | C44 blind-programming / FF gate (design language) |
| `CHARACTER_DETONATOR_VS_BOMBER.md` | C38 martyr vs C43 Program bomb — keep separate |

### Handed off / not Character-owned

| Doc / path | Owner now |
|------------|-----------|
| `CharacterSelectView`, `CharSelect*`, `PLAY_NOTES.md` | **UI** |
| `GEAR_BANDAGE_AGENT_BRIEF.md` (Cards worktree) | **Cards** |
| `DRAFT_HANDOFF`, `INDEX`, `PARALLEL_OPS`, `contracts/CURRENT` | **Integrator** |

---

## 3. Concept backlog (write before coding)

Ordered by dependency — earlier items unblock clearer later briefs. None of these are scheduled into
a `SCHEDULE.md` phase by this list alone.

| # | Concept | Status | Doc |
|---|---------|--------|-----|
| 1 | Live cast fantasy | **Done** | `CHARACTER_FANTASY.md` |
| 2 | Character↔Cards boundary | **Done** | `CHARACTER_CARDS_BOUNDARY.md` |
| 3 | Geometry-breach dependency sketch | **Done** | `CHARACTER_C36_DEPENDENCY.md` |
| 4 | Epistemic rules for object-timeline verbs | **Done** | `CHARACTER_TIME_PLAYER_EPISTEMICS.md` |
| 5 | Verticality / fall fantasy | **Done** (folded into C36 dep §5) | `CHARACTER_C36_DEPENDENCY.md` §5 |
| 6 | Martyr / Detonator vs Bomber | **Done** | `CHARACTER_DETONATOR_VS_BOMBER.md` |
| 7 | Roster growth rules | **Done** (in fantasy doc) | `CHARACTER_FANTASY.md` §5 |
| 8 | Readiness checklist | **Done** (living) | §5 below |

**Remaining before code** is not more concept stubs — it is **human answers** (fantasy §6, epistemics Option A/B, Bomber wall-only vs fall) and Integrator sequencing (C36 before unique verbs).

---

## 4. Prerequisite stacks (no code until stacks are honest)

```text
Scout/Jug attrs wiring (future)
  └─ human greenlights attrs brief §3
  └─ Integrator Sim carve-out
  └─ CharacterData becomes live Program authority

Bomber (future)
  └─ C42 conventions frozen
  └─ C36 geometry-breach primitives (Core/Integrator)
  └─ if floor-drop in v1: per-floor occupancy (lifts C39 item 6)
  └─ human answers Bomber brief §3
  └─ Integrator Sim carve-out + PLAYBACK_CONTRACT tape rows

Time Player (future)
  └─ C42 conventions frozen
  └─ C36 geometry-breach primitives
  └─ human answers fast-forward leak (PRODUCT_MEMORY)
  └─ Integrator Sim carve-out + PLAYBACK_CONTRACT tape rows

Interact Strength scaling (future, with Cards)
  └─ station targets exist (map/content)
  └─ joint numerics strawman in CHARACTER_CARDS_BOUNDARY / CARD_COLLECTION
  └─ gear Sim carve-out (Cards + Character attrs hook)
```

---

## 5. Readiness checklist — when Character may ask to code

All boxes that apply to the chosen slice must be true. Default until human moves mode off **C**:
**do not ask for code yet.**

### Any Character Sim slice

- [ ] Human exited docs-only mode for that slice
- [ ] Integrator wrote a frozen contract + Sim pause carve-out
- [ ] `PLAYBACK_CONTRACT.md` §5 path identified if new tape events are needed
- [ ] UI seat aware if Program chrome is required (parallel, not blocking design)

### Attrs wiring slice (Scout/Jug)

- [ ] GDD §2 vs §6 speed framing reconciled (human or Integrator)
- [ ] Agility trigger rule confirmed (attrs brief §3)
- [ ] Character Select → `CharacterData` authority decided

### First unique-verb slice

- [ ] Fantasy + Cards boundary docs stable (no "actually it's a gear card" drift)
- [ ] C36 dependency acknowledged in SCHEDULE / Integrator plan (not Character inventing breach alone)
- [ ] Bomber **or** Time Player open questions answered at PRODUCT_MEMORY level for that Character
- [ ] Monetization check: unlock path free / skill-gated (**C47**)

---

## 6. Collaboration matrix

| Seat | Character asks them for | Character offers them |
|------|-------------------------|------------------------|
| **Cards** | Joint review of `CHARACTER_CARDS_BOUNDARY.md`; Interact Strength strawman when stations near | Attr definitions Strength can scale; verb≠gear enforcement on roster ideas |
| **UI** | Nothing while Select is their lane; later Program mode chrome against frozen signatures | Ability identity copy / legality rules |
| **Atmosphere** | Pawn look only if fantasy doc implies silhouette needs (usually not) | — |
| **Integrator** | Org-doc sync (PARALLEL_OPS / INDEX / GDD §11 handoff); future contracts | Briefs + this plan; escalate design locks to PRODUCT_MEMORY |

---

## 7. Suggested human review cadence

Not a calendar — a loop:

1. Read `CHARACTER_FANTASY.md` + `CHARACTER_CARDS_BOUNDARY.md` — does the cast *feel* right?
2. Skim Bomber / Time Player briefs §3 — answer only the **buildability** questions when ready.
3. When fantasy + boundary feel stable, pick the **first** coding slice (likely attrs wiring, not Bomber).
4. Integrator opens contract; Character leaves docs-only mode for that slice only.

---

## 8. Explicit non-goals of this plan

- Does not amend PRODUCT_MEMORY or SCHEDULE phase rows.
- Does not schedule Bomber/Time Player into Phase 5.
- Does not replace implementation briefs — it points at them.
- Does not assign Cards/UI workers; offers seams only.

---

## See also

- [`CHARACTER_FANTASY.md`](CHARACTER_FANTASY.md)
- [`CHARACTER_CARDS_BOUNDARY.md`](CHARACTER_CARDS_BOUNDARY.md)
- [`CHARACTER_C36_DEPENDENCY.md`](CHARACTER_C36_DEPENDENCY.md)
- [`CHARACTER_TIME_PLAYER_EPISTEMICS.md`](CHARACTER_TIME_PLAYER_EPISTEMICS.md)
- [`CHARACTER_DETONATOR_VS_BOMBER.md`](CHARACTER_DETONATOR_VS_BOMBER.md)
- [`CHARACTER_ROSTER_LONGTERM.md`](CHARACTER_ROSTER_LONGTERM.md)
- [`departments/character/STATUS.md`](departments/character/STATUS.md)
