# Character → C36 Dependency Sketch

**Status:** Concept draft 2026-08-13 — Character dept asking Core/Integrator, **not** authoring C36.  
**Purpose:** List exactly what Bomber (**C43**) and Time Player (**C44**) need from destructible
geometry (**C36**) and from verticality, so Character never quietly invents breach Sim inside a
roster branch.  
**Depends on:** [`PRODUCT_MEMORY.md`](PRODUCT_MEMORY.md) **C36**, **C39** item 6, **C43**, OPEN #6 / #9;
[`CHARACTER_ROSTER_LONGTERM.md`](CHARACTER_ROSTER_LONGTERM.md); [`CHARACTER_PLAN.md`](CHARACTER_PLAN.md).  
**Non-goal:** Numerics, map art, or a frozen Sim contract — those stay Integrator/human.

---

## 1. Why Character is writing this

Both unique-verb operators in the long-term roster **reuse** C36's discrete breach state machine.
Without C36 primitives, Character can only write fantasy and briefs — not ship verbs. This doc is the
**shopping list** Character will attach when asking Integrator whether C36 is sequenced before any
unique-verb carve-out.

**Character does not own:** `ArenaBoard` breach entities, pathfinder obstacle mutation, map-authored
breach points, Damaged-state rules.

**Character does own:** whether Bomber/Time Player fantasies still make sense given what Core can
deliver in a first slice.

---

## 2. What C36 already locks (do not re-litigate)

From **C36**:

- Discrete states: **Intact → Damaged → Breached** (not physics fracture).
- Breach is a **schedulable timed action** → event in the resolve stream.
- Later Move/Shoot nodes in the **same round** evaluate against **post-breach** geometry.
- Breach state **carries across rounds** (**C33**-style persistence).
- Scoped to **designed breach points**, not freeform destruction everywhere.
- Numerics OPEN (OPEN #6).

**Name collision:** C57 `DoorKind.Breach` is a one-way **map door** shortcut. It is **not** C36
geometry breach. Contracts must say **geometry breach point** vs **map Breach-door**.

---

## 3. What exists in the repo today (gap)

| Need | Today | Gap |
|------|-------|-----|
| Designed geometry breach points on maps | Only walls + Standard/Vent/Breach **doors** | No Intact/Damaged/Breached board entities |
| Obstacle segments that appear/disappear | Static wall segments + door open/close | Pathfinder/LoS must see post-breach geometry mid-round |
| Match-persistent breach state | Doors persist; wounds persist | No breach snapshot on tape / next round |
| Multi-floor obstacles / occupancy | `Floor` field; maps use `Ground` only | **C39** item 6 — attic vestigial; blocks Bomber floor-drop |
| Generic "step object state" API | Door toggle special case | Time Player wants ±1 on a state machine (see epistemic doc) |

---

## 4. Shopping list — minimum Core deliverable for Character verbs

### 4A. Shared primitive (needed by **both** Bomber and Time Player)

Call it a **geometry breach point** (name illustrative):

1. **Identity** — stable id + display name for board-anchored UI.
2. **State enum** — Intact / Damaged / Breached (C36).
3. **Geometry binding** — which obstacle segment(s) exist at each state (Host-deterministic).
4. **Schedulable transition** — something in the event stream can step state forward (who may initiate
   that transition is Character/Core policy — Bomber detonate vs a generic "breach" verb is OPEN).
5. **Mid-round evaluation** — after a transition at time `t`, Move/Shoot/LoS with `ExecuteTime > t`
   use the new geometry.
6. **Cross-round carry** — state in end-of-round snapshot like doors/wounds.
7. **Tape events** — presentable state-change moments (`PLAYBACK_CONTRACT.md` §5 when built).

Without 4A, **neither** unique verb should open a Sim contract.

### 4B. Bomber-only extras

| Extra | Why | Can defer? |
|-------|-----|------------|
| Attachable bomb entity on a breach point | Attach then detonate fantasy | No, if Bomber ships as two-phase verb |
| Floor-tagged breach points | Floor-drop fantasy | **Yes** if v1 is wall-only route cutter |
| Per-floor occupancy + fall relocate | THE FINALS-style drop (**C43**) | **Yes** with wall-only v1; **No** if name requires fall |
| Fall tape event + queue policy | Readable Playback | With floor-drop |

### 4C. Time Player-only extras

| Extra | Why | Can defer? |
|-------|-----|------------|
| Step state **backward** (and maybe forward) on breach points | Rewind fantasy | Backward required for v1 fantasy |
| Object-timeline rules that don't peek opponent plans | Epistemic safety | See [`CHARACTER_TIME_PLAYER_EPISTEMICS.md`](CHARACTER_TIME_PLAYER_EPISTEMICS.md) — **not deferrable** as a design answer |
| Non-breach objects (doors, bombs) | Expanded fantasy | **Yes** — v1 = breach points only (recommendation) |

---

## 5. Verticality / fall fantasy (Bomber floor-drop)

Plain language for what the player should think happened — numerics later (OPEN #9).

1. A floor breach point reaches **Breached**.
2. The footprint is no longer standable on the **upper** floor.
3. Any pawn whose position is in that footprint on the upper floor **falls through** to the lower floor
   at the same XY (fantasy: the miniature drops to the shelf below).
4. That is a **geometry consequence**, not a targeted grenade — you bet on space and timing, not pawn id
   (same epistemic family as Snap free-aim).

**Open (human):** Is wall-only Bomber allowed to keep the name, or must §5 ship in first reveal?
(`CHARACTER_FANTASY.md` §6 Q2.)

Until per-floor infra exists, Character will only ask Integrator for **4A + wall Bomber**, not floor-drop.

---

## 6. Recommended sequencing (Character's ask to Integrator)

```text
1. Core/Integrator: C36 geometry breach points (§4A) — may be its own contract
2. Human: Time Player epistemic answer → PRODUCT_MEMORY
3. Human: Bomber wall-only vs full verticality
4. Then Character unique-verb contracts against frozen breach signatures
5. Per-floor + fall only if (3) demands it — separate carve-out, do not smuggle into (1)
```

Character will **not** open Bomber/Time Player Sim PRs that embed a private breach system.

---

## 7. What Character will accept as "C36 enough"

A first Core slice is "enough" for Character docs→code transition when:

- [ ] At least one designed geometry breach point exists on a map used in PlayMode
- [ ] A Host event can step Intact→…→Breached and path/LoS honor it later in the round
- [ ] State carries to the next round
- [ ] Tape has a scrubber-stable presentation hook
- [ ] Naming in contracts distinguishes geometry breach from `DoorKind.Breach`

Floor-drop checkboxes are **additional**, not part of "C36 enough."

---

## 8. Explicit non-goals

- Does not propose Damaged duration numbers (OPEN #6).
- Does not author map layouts.
- Does not replace Bomber/Time Player implementation briefs.
- Does not lift the Sim pause.

---

## See also

- [`CHARACTER_BOMBER_AGENT_BRIEF.md`](CHARACTER_BOMBER_AGENT_BRIEF.md)
- [`CHARACTER_TIME_PLAYER_AGENT_BRIEF.md`](CHARACTER_TIME_PLAYER_AGENT_BRIEF.md)
- [`CHARACTER_TIME_PLAYER_EPISTEMICS.md`](CHARACTER_TIME_PLAYER_EPISTEMICS.md)
- [`CHARACTER_DETONATOR_VS_BOMBER.md`](CHARACTER_DETONATOR_VS_BOMBER.md)
- [`PRODUCT_MEMORY.md`](PRODUCT_MEMORY.md) — **C36**, OPEN #6
