# Bomber — Character Ability Implementation Brief (C43)

**Status:** Draft 2026-08-13 — **long-term / not active Sim scope.** Docs-only; no resolver code.
**Scope:** Bomber's unique verb — attach a bomb to a surface (wall or floor), then detonate as a
schedulable timed action that drives **C36** geometry-breach state (Intact → Damaged → Breached).
Floor detonations also **drop pawns** from the floor above through the breached footprint
(THE FINALS-style verticality).
**Depends on:** [`PRODUCT_MEMORY.md`](PRODUCT_MEMORY.md) **C43**, **C36**, **C39** item 6, OPEN #6 / #9;
[`CHARACTER_ROSTER_LONGTERM.md`](CHARACTER_ROSTER_LONGTERM.md); [`CHARACTER_UNIQUE_VERB_OPERATORS_BRIEF.md`](CHARACTER_UNIQUE_VERB_OPERATORS_BRIEF.md);
[`PLAYBACK_CONTRACT.md`](PLAYBACK_CONTRACT.md); [`GDD.md`](GDD.md) §2.
**Does not touch:** Time Player (**C44**), Detonator martyr (**C38** — different "bomb on death"
fantasy), C57 `DoorKind.Breach` map doors (name collision only), live `GhostResolver` code.

---

## 1. What's already locked (do not re-litigate)

From **C43** / roster doc / **C36** / **C42**:

- Verb shape = schedulable timed action(s), Host event-stream, no physics fracture.
- Geometry uses discrete breach states: **Intact → Damaged → Breached** (**C36**), not continuous
  destruction meshes.
- Floor-mounted detonation breaches that floor **and** relocates any pawn standing on the floor
  **directly above** the breached footprint to the floor below.
- Hard prerequisite: **real per-floor obstacle/occupancy infrastructure** — does not exist today
  (**C39** item 6; maps only build `Floor.Ground`).
- Unique verb, not exclusive gear (**C62**). Monetization: must ship free / skill-gated.
- Numerics OPEN (PRODUCT_MEMORY OPEN #9).

**Still OPEN — this brief exists to make these concrete enough to decide, not to decide them:**
attach vs detonate costs; whether attach and detonate are two nodes or one; blast = footprint-only vs
radius; designed breach points vs free attach; fall transition rules; wall-capable vs floor-only.

---

## 2. What already exists in the repo (read this before assuming a blank slate)

| File / area | What it says | Cross-check |
|---|---|---|
| `DoorKind.Breach` | One-way permanent **door** shortcut; UI hides Close after Open; resolver still treats it as a normal door | **Not** C36 geometry breach. Do not extend `DoorKind` to mean Bomber bombs. Keep names distinct in contracts ("geometry breach point" vs "map Breach-door"). |
| `ArenaBoard` / map builders | Walls + Standard/Vent/Breach **doors**; single `Floor.Ground` | No Intact/Damaged/Breached geometry objects. No bomb attach points. No attic obstacle set. |
| `PlanarPosition.Floor` | Enum includes Attic; LoS same-floor check exists | Occupancy is not tracked per floor beyond the coordinate field. Falling between floors has no resolve path. |
| `ActionVerb` | Move / Shoot / Door | No BombAttach / BombDetonate / GeometryBreach. |
| `TapeEventType` | No breach / fall / bomb events | Floor-drop needs presentable tape moments (breach state change + pawn relocate at minimum). |
| `GhostResolver` | Door toggle sweep; Move blocked by closed door segments; Shoot LoS vs obstacles | Post-breach geometry evaluation (**C36** "later nodes see new geometry") is **unimplemented** — Bomber cannot soft-land on Door toggles alone. |
| Death / queue cancel | Dead freezes remaining queue (C37 precedent referenced in design docs) | Fall-cancel-on-impact would mirror that rule — no shared helper exists yet; don't assume one. |

**Bottom line:** Bomber is blocked on **two** missing systems — C36 geometry-breach primitives **and**
per-floor occupancy — not merely on CharacterData fields. A wall-only Bomber without floor-drop is the
only conceivable thinner slice, and even that still needs C36.

---

## 3. Open questions blocking a frozen contract

These need answers before Sim/HUD signatures can freeze. Several are resolve-shape, not "just numerics."

1. **Attach vs detonate — one node or two?** Roster assumes two scheduled events (mirrors Door
   Open/Close). Confirm. If two: can another Character's Move/Shoot interact with an attached-but-live
   bomb? Can Time Player rewind an attached bomb?
2. **Cost strawman.** Propose numbers to greenlight or replace (OPEN #9): e.g. Attach ~3s, Detonate ~1s
   — **recommendation only**, not locked.
3. **Blast footprint.** Exact attached surface footprint only, or a radius that can wound/breach
   adjacent points? Floor-drop rule as written is footprint-based; radius would expand collision tests.
4. **Attach eligibility.** Every floor/wall sample vs **designed breach points only** (C36's scoped
   destruction). Recommendation: designed points only — matches C36 and avoids freeform mesh cutting.
5. **Walls too, or floor-drop only?** C43 text allows wall or floor. If walls-only ships first, floor-drop
   and per-floor infra can defer — confirm whether that Character still deserves the "Bomber" name.
6. **Fall transition.** Instant teleport to equivalent XY one floor down vs scheduled fall duration;
   does fall cancel the victim's remaining queue (death-freeze precedent); can landing wound?
7. **Who can be dropped?** Enemy only, any pawn including self/ally, objects? Blind-programming means
   Bomber cannot aim at a pawn id — drop is a **geometry consequence**, not a targeted attack (aligns
   with C32/C39 free-aim philosophy).
8. **Cross-round state.** Attached undetonated bombs and geometry breach state must carry like wounds
   (**C33** / **C36**). Confirm bomb-as-match-persistent entity vs must detonate same round.
9. **Prerequisite ordering.** Is C36 geometry breach required to merge before any Bomber contract, or
   may one Integrator contract deliver "breach points + Bomber verb" together?

---

## 4. Proposed Sim resolve shape (recommendation, not locked)

**Recommended slice order:**

1. **C36 geometry breach points** as board entities with Intact/Damaged/Breached, schedulable state
   transitions, obstacle segments that appear/disappear for pathfinder + LoS, state carried in
   `ReplayTape` / next-round board snapshot.
2. **Bomber verbs** as `ActionVerb.BombAttach` + `ActionVerb.BombDetonate` (or a single
   `GeometryBreach` verb used only by Bomber's Character grant — prefer Bomb* names for clarity).
3. **Floor-drop** only after per-floor occupancy exists: at detonate complete second, any pawn whose
   position projects into the breached footprint on `Floor.Above` is relocated to `Floor.Below` at the
   same XY (exact fall animation/wound rules per §3 Q6).

**Attach node (recommendation):**

- Requires Bomber Character grant + pawn within `InteractRadius` of a designed breach point (Door-like).
- Books Time Resource; emits no geometry change yet; persists an `AttachedBomb` match object on that
  point (new persistent state — size like Bandage charges / door state).

**Detonate node (recommendation):**

- Targets an attached bomb the acting Bomber owns (or any attached bomb — **open**, see §3 Q1).
- At complete second: advance geometry state toward Breached (how Damaged intermediate works = C36 OPEN #6);
  if floor footprint reaches Breached, enqueue fall relocations as tape events.

**Not recommended:** reusing `DoorKind.Breach` open/close as "bombing a wall." That conflates a
permanent map shortcut with destructible geometry and skips Damaged / floor-drop / designed-point
authorship.

**Tape (recommendation):** at least `GeometryBreached` (or state-changed) + `PawnFell` (or
`PawnRelocated`) with `PLAYBACK_CONTRACT.md` §5 checklist — presenters must be pure functions of
scrubber seconds.

---

## 5. Proposed HUD shape (recommendation, not locked)

- Mode button visible only for Bomber (Character-gated).
- Board-anchored prompt on designed breach points: identity (which point), live state (Intact/Damaged/
  Breached / bomb attached?), options Attach / Detonate as **explicit confirms** —
  `UI_BOARD_ANCHORED_COMPONENTS.md` applies (board object).
- Scrubber markers for attach + detonate nodes.
- UI implements chrome against Integrator-frozen signatures; Character owns legality rules in the brief /
  future resolve tests.

---

## 6. Suggested contract split, once numerics are greenlit

| Gate | Owner |
|------|-------|
| Answer §3 (especially Q5 walls-only vs floor, Q6 fall, Q4 designed points) | Human |
| C36 breach-point primitive contract (may be separate worker) | Integrator |
| Per-floor infra carve-out (only if floor-drop in scope) | Integrator + human |
| Bomber Sim (`ActionVerb`, attach state, detonate → geometry/falls) | Character (when Sim carve-out exists) |
| Bomber Program prompts | UI |

Do **not** start Bomber Sim work under the general look-and-feel pause without an explicit carve-out
(same bar as C57 / Bandage C63).

---

## 7. Explicit non-goals of this brief

- No live resolver / HUD / prefab work.
- No Detonator (**C38**) martyr blast — different trigger (Downed→Dead), different fantasy.
- No promotion into active `SCHEDULE.md` phase by this document alone.
- No reconciliation of C57 Breach-door naming beyond the collision warning in §2.

---

## See also

- [`CHARACTER_UNIQUE_VERB_OPERATORS_BRIEF.md`](CHARACTER_UNIQUE_VERB_OPERATORS_BRIEF.md) — C42 gates
- [`CHARACTER_ROSTER_LONGTERM.md`](CHARACTER_ROSTER_LONGTERM.md) — Bomber section
- [`PRODUCT_MEMORY.md`](PRODUCT_MEMORY.md) — **C36**, **C43**, OPEN #6 / #9
- [`PLAYBACK_CONTRACT.md`](PLAYBACK_CONTRACT.md) — §5 extension checklist
- Cards worktree `docs/GEAR_BANDAGE_AGENT_BRIEF.md` — doc shape reference
