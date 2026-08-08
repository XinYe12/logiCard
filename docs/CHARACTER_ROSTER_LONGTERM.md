# Character Roster — Long-Term Unique-Verb Operators

**Status:** Drafted 2026-08-06 — **long-term/future roadmap**, sequenced by **C50**'s phase model, not tied to
a retired calendar (updated 2026-08-08 for the **C46** scope pivot — see `PRODUCT_MEMORY.md`). Design-phase
only; nothing here is scheduled or implemented. See scope note below before assuming otherwise.
**Depends on:** [VISION.md](VISION.md) (Long-Term Systems list), [PRODUCT_MEMORY.md](PRODUCT_MEMORY.md) **C36–C38** (existing long-term systems this roster builds on: destructible geometry, objective win, revive/martyr), **C42–C44** (the binding pointer entries for this doc), [MONETIZATION.md](MONETIZATION.md) (the guardrail below).

---

## Scope note — why this is future roadmap, not active build scope

The user described these characters as ones they "want for this demo" back when this doc was drafted, but
both concepts require systems still not built:

- **Bomber** needs real multi-floor obstacle/occupancy infrastructure. Today, `Floor` is a vestigial field on `PlanarPosition` — `GameBootstrap.BuildBoard()` only ever constructs `Floor.Ground`, and **C39 item 6** explicitly locked "no per-floor obstacle infrastructure built." Attic/vent/monitor are out-of-scope per **C17**, and multi-floor is called out as long-term in `VISION.md` ("ground floor and an attic").
- **Time Player** needs a generalized "reverse an object's state" mechanic layered on **C36** (destructible geometry), which is itself already long-term-only, not built yet.

Filed here as long-term vision (same bucket as **C36–C38**), pointed to from `PRODUCT_MEMORY.md` **C42–C44**.
These prerequisites are unchanged by the **C46** scope pivot — neither character is promoted into active build
scope by the pivot itself. If either becomes real work, that's an explicit scope conversation against
`SCHEDULE.md`'s phase table — say so and this doc gets reclassified.

## Monetization guardrail (added 2026-08-08, C47)

Both Bomber and Time Player are **gameplay content — a genuinely new verb — not cosmetics.** Per
`MONETIZATION.md`'s no-pay-to-win rule, if/when either gets promoted out of this doc's long-term-vision
status into a shippable roster, it must ship **free** or gated only by skill/grind every player can complete
without paying. Putting a unique verb behind a paywall breaks the F2P no-pay-to-win guarantee, even if the
store frames the unlock as a "character pack." Whoever eventually promotes either character out of this doc
owns satisfying this constraint, not just the technical prerequisites above.

---

## The roster model: unique verbs, not just attribute variants

Scout and Juggernaut (the demo cast, `GDD.md` §2) share the exact same verbs — Move, Shoot, Door — and differ only in numeric attributes (Speed / Agility / Strength). Nothing in the demo cast has an ability another character lacks entirely.

Bomber and Time Player break that pattern: each carries a **verb no other character has**. This is a real model expansion, not a numeric reskin, and it's the first time this project has needed it. The constraint that makes it safe to consider at all: **a unique verb must still resolve through the existing deterministic event-stream** — schedulable (booked at a Time Resource second, same as Shoot/Door/Breach), Host-computed on plain float math, no engine/physics calls (`C23`/`C32`/`C35`'s "never a physics raycast" discipline). A unique verb that can't be expressed that way isn't just a numerics question — it needs its own architecture pass before it's promotable.

---

## Bomber

**Verb:** attach a bomb to any surface — wall or floor — as a schedulable timed action, same shape as `C36`'s breach action (Intact → Damaged → Breached). Detonation is presumably a second scheduled event (attach, then detonate), mirroring how Door already splits into separate Open/Close actions rather than one instantaneous toggle.

**The new consequence — floor breach drops the floor above:** `C36` as currently written only says post-breach geometry changes what later Move/Shoot nodes evaluate against (a breached wall opens a route/LoS, for instance). Bomber's floor case is a step further: breaching a floor segment should **drop any pawn standing on the floor directly above it through to the floor below**, relocating that pawn across floors, not just changing what blocks them. Named inspiration: *THE FINALS'* destructible-floor verticality — breach a hole, the floor above becomes unstandable, whoever's on it falls.

**Why this is the hard part, not the bomb itself:** the "attach + detonate" action is a straightforward extension of the already-proven `C36` breach pattern. The floor-drop consequence is not — it requires:
1. Real per-floor pawn occupancy (who's currently on which floor, at which point) — doesn't exist today.
2. A resolve-time rule for what happens to a falling pawn: is it instant (teleport to the equivalent point one floor down), or a scheduled fall event with its own duration; does the fall interrupt/cancel that pawn's own remaining queued actions (mirrors the "death freezes remaining queue" rule already agreed for `GhostResolver` per `C37`); can a fall itself wound on landing.
3. Reconciling with `C39` item 6's explicit "no per-floor obstacle infrastructure" call — this is the same wall that's blocked Attic since day one, not a Bomber-specific problem.

**Open questions (not resolved here — parking lot):**
- Time Resource cost to attach vs. detonate.
- Blast radius, or is it exactly the attached surface's footprint.
- Does every floor tile support an attach point, or only designed breach points (mirrors `C36`'s existing "scoped to designed breach points, not freeform destruction everywhere")?
- Can Bomber breach walls too (per `C36`'s original framing) in addition to floors, or is the floor-drop the whole point of giving this character a name?

---

## Time Player

**Verb:** targets **objects, not pawns** — fast-forward or reverse an interactable object's state along its own timeline. Worked example from the user: reverse a bombed floor so it reads as `Breached → Damaged → Intact`, i.e. never bombed.

**Why this is promotable at all:** `C36` already models breach as a small discrete state machine (Intact/Damaged/Breached), and state-machine transitions are already just events in the resolve event-stream (same shape as a Shoot hit, a Door toggle). "Reverse" is structurally just another scheduled event that moves an object backward through states it's already defined to have, rather than a new kind of system. That reuse is what makes this a roster-fit character concept instead of a request for a new engine.

**The real risk — this one needs resolving before it's buildable, not just tuned:**
The game's entire epistemic model (`VISION.md` Success Metric, the "blind programming" pillar referenced throughout `CONTINUOUS_PIVOT_PLAN.md`) depends on both players secretly committing a program against the *current* board state, with neither side able to see the other's plan or the future before resolve. A "fast-forward" that lets a player preview what an object's state *will be* later in the same resolve window is a potential leak against that pillar — it's not obviously fine the way "reverse" is. This needs an explicit design answer (e.g. fast-forward only affects the object's *own* future state deterministically and reveals nothing about the opponent's plan; or fast-forward is cut and only reverse ships) before this character is more than a name.

**Open questions (parking lot):**
- Time Resource cost for rewind vs. fast-forward (if fast-forward survives the risk above).
- Range / LoS requirement to target an object, or is it unrestricted like a Door toggle's radius-based model.
- Scope of "interactable object" — breach state only, or does it extend to doors (un-close a door someone else closed) or anything carrying `C38`-style state (Downed, revived)? Reversing a pawn's own state edges directly into `C38`'s revive system and should not be assumed in scope without reconciling the two.

---

## Summary table

| | Bomber | Time Player |
|---|---|---|
| Verb shape | Schedulable action, extends `C36` breach | Schedulable action, extends `C36` breach (reverse direction) |
| Hard prerequisite | Real per-floor occupancy/obstacle infrastructure | An explicit answer to the future-state-leak risk above |
| Reuses | `C36` discrete breach state machine | `C36` discrete breach state machine |
| Net-new | Cross-floor pawn relocation on breach | None structurally — the risk is design, not engineering |
