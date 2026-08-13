# Unique-Verb Operators — Character Implementation Brief (C42)

**Status:** Draft 2026-08-13 — **long-term / not active Sim scope.** Docs-only; no resolver code.
**Scope:** The roster model that lets a Character carry a **verb no other Character has**, beyond
Scout/Juggernaut's shared Move/Shoot/Door + numeric attrs (`GDD.md` §2). Concrete first operators are
**Bomber** ([`CHARACTER_BOMBER_AGENT_BRIEF.md`](CHARACTER_BOMBER_AGENT_BRIEF.md) / **C43**) and
**Time Player** ([`CHARACTER_TIME_PLAYER_AGENT_BRIEF.md`](CHARACTER_TIME_PLAYER_AGENT_BRIEF.md) /
**C44**). This brief is the shared architecture gate those two (and any later unique-verb Character)
must pass before Integrator opens a Sim contract.
**Depends on:** [`PRODUCT_MEMORY.md`](PRODUCT_MEMORY.md) **C42**, **C23** / **C32** / **C35**
(deterministic Host event-stream); [`CHARACTER_ROSTER_LONGTERM.md`](CHARACTER_ROSTER_LONGTERM.md);
[`MONETIZATION.md`](MONETIZATION.md) (no-pay-to-win — unique verbs ship free / skill-gated);
[`PLAYBACK_CONTRACT.md`](PLAYBACK_CONTRACT.md); [`GDD.md`](GDD.md) §2 / §11.
**Does not touch:** Bomber floor-drop numerics, Time Player fast-forward leak answer, Scout/Juggernaut
attr wiring ([`CHARACTER_ATTRS_SCOUT_JUGGERNAUT_BRIEF.md`](CHARACTER_ATTRS_SCOUT_JUGGERNAUT_BRIEF.md)),
gear exclusives (**C62** — unique verbs stay verbs, not exclusive gear packs).

---

## 1. What's already locked (do not re-litigate)

From **C42** / `CHARACTER_ROSTER_LONGTERM.md` / **C62**:

- Future Characters **may** carry a unique verb, not only Speed/Agility/Strength variants.
- A unique verb **must** resolve through the existing deterministic event-stream: schedulable (booked
  at a Time Resource second), Host-computed on plain float math, **no** engine/physics calls — same
  discipline as Shoot LoS / continuous path (**C23** / **C32** / **C35**).
- A verb that cannot be expressed that way needs an **architecture pass** before promotion — not just
  a name + numerics.
- Unique-verb Characters are **gameplay content**, not cosmetics. Per monetization guardrail they must
  ship **free** or skill/grind-gated — never paywalled (**C47** / roster doc).
- Same gear deck still applies (**C18** / **C62**): Bomber/Time Player do **not** get exclusive gear;
  their difference is the verb.
- **C46** removed the artificial "after the 14-day demo" calendar gate but **did not** promote C42–C44
  into active build scope. Promotion still requires an explicit `SCHEDULE.md` phase conversation +
  human confirm → PRODUCT_MEMORY.

**Still OPEN — this brief exists to make the shared gates concrete enough to decide, not to decide them:**
how unique verbs appear on `ActionVerb` / payloads, how Character Select / Program HUD expose a
Character-specific mode without UI owning the resolve math, and when Integrator lifts the Sim pause
for a named operator.

---

## 2. What already exists in the repo (read this before assuming a blank slate)

| File / area | What it says | Cross-check |
|---|---|---|
| `ActionVerb.cs` | `Move`, `Shoot`, `Door` only | No extension point for Character-unique verbs yet. Adding one means an enum growth (or a parallel channel — see §4) that every Program/Resolve/Tape path must understand. |
| `ActionNode.cs` | Verb + ExecuteTime + position/stance/shoot/door fields; nullable `CardData Modifier` | Modifier is gear-interrupt scaffolding (Bandage brief already flagged it as unproven). Unique verbs should **not** piggyback Modifier — they are Character abilities, not cards. |
| `TapeEventType` | MoveArrive, ShootFire, Wounded, Killed, Invalid, DoorOpened, DoorClosed | No breach / bomb / rewind / fall events. Any unique verb that changes board or pawn state needs `PLAYBACK_CONTRACT.md` §5 checklist items. |
| `CharacterData.cs` + Scout/Juggernaut assets | Speed / Agility / Strength **numeric** fields only | No "has unique verb X" flag, no ability id list. Roster expansion needs a data shape beyond attrs (see §4). |
| `PawnProgram.cs` | Queues Move / Shoot / Door against budget | Character-specific queue methods do not exist. Pattern to mirror: `TryQueueDoor` (radius gate + cost + node). |
| `GhostResolver.cs` | Per-pawn time-ordered node sweep; door transitions; Shoot hit math | No Character-id branch today — resolve is verb-driven. Unique verbs should stay verb-driven so Host stays bit-identical regardless of cosmetic archetype id. |
| `DoorKind.Breach` (**C57**) | Map "breach" = one-way permanent **door** shortcut | **Name collision risk:** C36/C43 "breach" means Intact→Damaged→Breached **geometry**. C57's `DoorKind.Breach` is unrelated. Briefs/contracts must say **geometry breach** vs **map Breach-door** explicitly. |
| `Floor` on `PlanarPosition` | Field kept; maps only ever construct `Floor.Ground` | C39 item 6: **no** per-floor obstacle infra. Blocks Bomber's floor-drop prerequisite — not a C42-model issue, but a hard gate for C43. |

**Bottom line:** the codebase is built for "same verbs, different numbers." C42 is a real model expansion;
treat enum/payload/HUD/CharacterData growth as first-class contract work, not a one-line add.

---

## 3. Open questions blocking a frozen contract

1. **Promotion gate.** Which `SCHEDULE.md` phase (or Integrator carve-out) is allowed to open the
   **first** unique-verb Sim contract — and is that Bomber, Time Player, or a thinner vertical slice
   (e.g. wall-only geometry breach without floor-drop)?
2. **Data shape on Character.** How does a Character declare its unique verb(s)? Options (not decided):
   - ScriptableObject ability id list on `CharacterData`
   - Hard-coded archetype → verb map in Boot (mirrors today's hardcoded Scout/Juggernaut spawn speeds)
   - Separate `CharacterAbility` assets referenced by the Character card
3. **`ActionVerb` vs parallel channel.** Grow `ActionVerb` (recommended for Host simplicity) vs. a
   side-band "ability node" list on the payload. Wire format (`RelayProtocol` / `TimelinePayload`) must
   stay Host-revalidatable either way.
4. **Program HUD affordance ownership.** Character dept owns ability **rules**; UI dept owns chrome.
   Who owns the mode button / board-anchored prompt for a Character-unique verb when both seats are
   hot? Default recommendation: Integrator freezes the node signature; Character proposes resolve;
   UI implements Program arm UI against the frozen signature (same split as Bandage).
5. **Monetization check at promotion.** Explicit Integrator+human checklist item: confirm unlock path
   is free/skill-gated before merge — do not leave to store copy later.

---

## 4. Proposed Sim resolve shape (recommendation, not locked)

**Recommended:** each unique verb is a first-class `ActionVerb` value (e.g. `BombAttach`,
`BombDetonate`, `ObjectRewind`) with its own `PawnProgram.TryQueue…` gate and a `GhostResolver`
branch in the existing time-ordered sweep — same shape as Door.

Why:

- Host already sorts/filters by verb; tests already assert verb-specific tape events.
- Avoids overloading `ActionNode.Modifier` (gear) or inventing a second timeline of nodes.
- Keeps C62's "verbs ≠ exclusive gear" boundary crisp in the type system.

**Also required at the Character layer (still recommendation):**

- `CharacterData` (or sibling asset) gains an explicit ability grant list so Program UI can hide modes
  the active Character lacks — legality is Character-scoped, resolve stays verb-scoped.
- New tape event types per verb, each run through `PLAYBACK_CONTRACT.md` §5.

**Not recommended:** encoding unique verbs as gear `CardData` rows. That fights **C62** and conflates
hand economy with Character identity.

**Prerequisite stack (ordering recommendation):**

1. Freeze C42 data/`ActionVerb` conventions (this brief).
2. Land **C36 geometry-breach** primitives (or a scoped subset) — both Bomber and Time Player reuse them.
3. Then open Bomber and/or Time Player contracts against those primitives (see their briefs).
4. Per-floor occupancy only when Bomber's floor-drop is in the same contract — do not silently expand
   C39 item 6 inside a Time Player slice.

---

## 5. Proposed HUD shape (recommendation, not locked)

- Program dock gains a Character-gated mode control only when the selected Character grants that verb
  (Scout/Juggernaut show nothing new).
- Prefer Door-like board-anchored prompts when the verb targets a **board object** (bomb attach point,
  breachable surface, rewindable object) — read `docs/UI_BOARD_ANCHORED_COMPONENTS.md` (identity /
  live state / explicit confirm).
- Scrubber must show the booked Time Resource cost the same way Move/Shoot/Door already do.
- UI dept implements chrome; this brief does not assign `CharacterSelectView` work (select motion
  handed to UI under the 2026-08-13 mandate shift).

---

## 6. Suggested contract split, once gates clear

| Seat | Owns |
|------|------|
| **Integrator** | Freeze `ActionVerb` + node fields + tape events; lift Sim pause carve-out; update `contracts/CURRENT.md` |
| **Character** | Ability rules brief → resolve behavior tests against frozen signatures (when carve-out exists) |
| **UI** | Program mode + board-anchored prompts against frozen signatures |
| **Atmosphere / Cards** | Out of lane unless presentation or gear interaction is explicitly contracted |

**Gates before opening any unique-verb Sim slot:**

1. Human answers §3 (at least promotion target + ActionVerb shape).
2. Explicit Sim pause carve-out (mirror **C57** map/terrain and Bandage's C63 pattern — do not assume
   the general pause is soft).
3. For Bomber floor-drop: per-floor infra design answer (see Bomber brief). For Time Player: fast-forward
   leak answer (see Time Player brief). C36 numerics (OPEN #6) at least strawman-greenlit if the verb
   mutates geometry breach state.

---

## 7. Explicit non-goals of this brief

- Does not implement or scaffold Sim/HUD code.
- Does not promote Bomber / Time Player / Detonator into active scope.
- Does not resolve C36/C38/C43/C44 numerics.
- Does not redesign Scout/Juggernaut attrs (separate brief).
- Does not edit Integrator-owned `DRAFT_HANDOFF` / `INDEX` / `PARALLEL_OPS` — Character STATUS notes the
  mandate shift; Integrator syncs org docs.

---

## See also

- [`CHARACTER_ROSTER_LONGTERM.md`](CHARACTER_ROSTER_LONGTERM.md) — design source for C42–C44
- [`CHARACTER_BOMBER_AGENT_BRIEF.md`](CHARACTER_BOMBER_AGENT_BRIEF.md) — C43
- [`CHARACTER_TIME_PLAYER_AGENT_BRIEF.md`](CHARACTER_TIME_PLAYER_AGENT_BRIEF.md) — C44
- [`CHARACTER_ATTRS_SCOUT_JUGGERNAUT_BRIEF.md`](CHARACTER_ATTRS_SCOUT_JUGGERNAUT_BRIEF.md) — live cast attrs
- Cards worktree `docs/GEAR_BANDAGE_AGENT_BRIEF.md` (`logiCard-cards-collection`) — pattern reference for this doc's shape (not present on this branch)
- [`PRODUCT_MEMORY.md`](PRODUCT_MEMORY.md) — **C42**, OPEN #9 / #10
- [`docs/contracts/CURRENT.md`](contracts/CURRENT.md) — where a real contract would open (Integrator-only)
