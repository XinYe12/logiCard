# Time Player — Character Ability Implementation Brief (C44)

**Status:** Draft 2026-08-13 — **long-term / not active Sim scope.** Docs-only; no resolver code.
**Scope:** Time Player's unique verb — target **objects, not pawns**, and move an interactable
object's discrete state **backward** (and possibly **forward**) along its own timeline — e.g. reverse
a **C36** geometry breach `Breached → Damaged → Intact`.
**Depends on:** [`PRODUCT_MEMORY.md`](PRODUCT_MEMORY.md) **C44**, **C36**, OPEN #6 / #10;
[`CHARACTER_ROSTER_LONGTERM.md`](CHARACTER_ROSTER_LONGTERM.md); [`CHARACTER_UNIQUE_VERB_OPERATORS_BRIEF.md`](CHARACTER_UNIQUE_VERB_OPERATORS_BRIEF.md);
[`VISION.md`](VISION.md) Success Metric (blind programming); [`PLAYBACK_CONTRACT.md`](PLAYBACK_CONTRACT.md).
**Does not touch:** Bomber attach/detonate (**C43**), pawn revive / Downed ladder (**C38**), gear
Adrenaline Execute stub, live `GhostResolver` code.

---

## 1. What's already locked (do not re-litigate)

From **C44** / roster doc / **C42**:

- Targets **objects**, not pawns.
- Structurally reuses schedulable events in the resolve stream (same family as Shoot/Door/Breach).
- Motivating example: rewind C36 breach states.
- Unique verb, not exclusive gear (**C62**). Monetization: free / skill-gated.
- **Hard design gate (not a numeric):** "fast-forward" that lets a player **preview** an object's
  future state inside the same resolve window may leak against blind simultaneous programming
  (`VISION.md` Success Metric). Needs an explicit design answer before this Character is buildable.
- Numerics OPEN (OPEN #10). **C46** did not promote C44 into active build scope.

**Still OPEN — this brief exists to make these concrete enough to decide, not to decide them:**
rewind-only vs rewind+fast-forward; cost; range/LoS; which object types are legal; whether object
"timeline" means match-persistent history or only the discrete state enum.

---

## 2. What already exists in the repo (read this before assuming a blank slate)

| File / area | What it says | Cross-check |
|---|---|---|
| `Door` + `DoorState` | Open/Closed; toggled by Door verb; state carries across rounds | Closest live "object state machine." Rewinding a door (un-close) is a possible scope expansion — **not** assumed by C44's worked example. |
| `DoorKind.Breach` | Map shortcut door — not geometry Intact/Damaged/Breached | Do not treat opening a Breach-door as Time Player content. |
| C36 geometry breach | Design-only; no Intact/Damaged/Breached entities in code | Time Player's primary example **cannot** ship before C36 primitives exist (same prerequisite as Bomber's geometry half). |
| `ActionVerb` / `TapeEventType` | Move/Shoot/Door only; no rewind/fast-forward events | New verb + tape rows required. |
| `GhostResolver` | Applies door transitions at ExecuteTime; no generic "set object state" API | Recommendation: a small object-timeline service used by breach **and** Time Player, rather than forking one-off mutators. |
| Program epistemic model | Both sides lock programs against **current** board; reveal/execute later | Any Program-time UI that shows "what this will be at t=40s if nobody else acts" is already a leak risk if it incorporates hidden opponent actions — see §3 Q1. |

**Bottom line:** engineering reuse is real (state-machine event), but **buildability is gated on a
design answer about fast-forward**, then on C36 existing at all. Rewind-only is the conservative path.

---

## 3. Open questions blocking a frozen contract

1. **Fast-forward survival (blocks buildability).** Pick one before any Sim contract:
   - **(A) Rewind only** — cut fast-forward entirely for v1.
   - **(B) Fast-forward allowed but only applies deterministic object-local transitions already
     scheduled by **public** board rules / the Time Player's **own** nodes — never peeks opponent
     payload.
   - **(C) Fast-forward only as a resolve-time event** with no Program-time preview beyond "I booked
     FF on object X at t" (same info opacity as booking a Door open).
   - **(D) Other** — write it down in PRODUCT_MEMORY when confirmed.

   Recommendation to start discussion: **(A) or (C)**. (B) is easy to get wrong in HUD copy.
2. **Cost strawman.** Rewind ~4s / Fast-forward ~4s (if it survives) — recommendation only.
3. **Range / LoS.** Unrestricted vs Door-like `InteractRadius` vs Shoot-like LoS to the object.
   Recommendation: radius interact (Door family) — keeps it an object verb, not a sniper verb.
4. **Object scope v1.** Breach geometry only (matches worked example) vs also Standard/Vent doors vs
   attached Bomber bombs vs C38 pawn states. **Recommendation: geometry breach states only for v1.**
   Door rewind and pawn-state rewind each collide with other systems (Door UX expectations; C38 revive).
5. **What "timeline" means.** Discrete enum step (±1 state) vs replay of the object's last N match
   events. Recommendation: ±1 along the defined state machine — no general rewind clock.
6. **Illegal targets.** Already Intact (rewind) / already terminal state (FF); object mid-transition
   from another node same second — Program gate vs resolve no-op?
7. **Interaction with Bomber.** If both exist: can Time Player rewind a floor mid-fall? Rewind an
   attached undetonated bomb? Flag as cross-brief; default **out of v1 scope**.

---

## 4. Proposed Sim resolve shape (recommendation, not locked)

**Recommended:** `ActionVerb.ObjectRewind` (and, only if §3 Q1 keeps it, `ObjectFastForward`) as
schedulable nodes targeting a board object id (breach point id), resolved in the time-ordered sweep:

- At ExecuteTime, step the object's state machine one step backward (or forward).
- Later Move/Shoot/Door nodes in the same round evaluate against the new geometry (C36 rule).
- Emit `TapeEventType` rows for the state change (`ObjectStateChanged` or specific
  `BreachRewound`) — `PLAYBACK_CONTRACT.md` §5.

**Shared primitive (recommendation):** introduce a tiny Host-side `IStatefulBoardObject` (name
illustrative) used by C36 breach points — Time Player becomes "Character-gated verb that steps that
interface," not a one-off mutator. Stops Door/Breach/Bomb each inventing private timelines.

**Not recommended:**

- Program-time simulation UI that runs a partial ghost of the **opponent's** locked program to show
  "future breach state."
- Encoding rewind as a gear card.
- Allowing pawn Healthy/Wounded/Downed/Dead as ObjectRewind targets (that's C38 territory).

**Prerequisite:** C36 breach points exist **or** the same Integrator contract delivers breach points +
rewind together. Do not open Time Player against doors-only as a fake substitute for the fantasy
(unless human explicitly re-scopes the Character).

---

## 5. Proposed HUD shape (recommendation, not locked)

- Character-gated mode control.
- Board-anchored prompt on legal objects: identity, **live** state from authoritative model, options
  Rewind / (optional) Fast-forward as explicit confirms — `UI_BOARD_ANCHORED_COMPONENTS.md`.
- If §3 Q1 = rewind-only, do not ship a disabled Fast-forward control that implies a future leak.
- No predictive "ghost future board" overlay unless human explicitly accepts that epistemic model.

---

## 6. Suggested contract split, once numerics + leak answer are greenlit

| Gate | Owner |
|------|-------|
| §3 Q1 fast-forward decision → PRODUCT_MEMORY | Human (required) |
| C36 breach primitive (shared with Bomber) | Integrator |
| Time Player Sim verb + tests | Character (when Sim carve-out exists) |
| Object prompts | UI |

Sim pause carve-out required — same bar as C57 / Bandage C63.

---

## 7. Explicit non-goals of this brief

- No live code.
- No Adrenaline / playback mid-Execute redesign (different leak/class of problem).
- No C38 revive-by-rewind.
- No promotion by doc alone.

---

## See also

- [`CHARACTER_UNIQUE_VERB_OPERATORS_BRIEF.md`](CHARACTER_UNIQUE_VERB_OPERATORS_BRIEF.md) — C42 gates
- [`CHARACTER_BOMBER_AGENT_BRIEF.md`](CHARACTER_BOMBER_AGENT_BRIEF.md) — shared C36 prerequisite
- [`CHARACTER_ROSTER_LONGTERM.md`](CHARACTER_ROSTER_LONGTERM.md) — Time Player section
- [`PRODUCT_MEMORY.md`](PRODUCT_MEMORY.md) — **C44**, OPEN #10
- [`VISION.md`](VISION.md) — blind-programming success metric
- Cards worktree `docs/GEAR_BANDAGE_AGENT_BRIEF.md` — doc shape reference
