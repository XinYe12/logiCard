# Flashbang — Gear Implementation Brief

**Status:** **Paused** — C64 hybrid landed; this brief must be **re-derived as shared-library tech**
before any Sim/HUD contract. Do not implement from the draft body below. Draft retained, not discarded.
**Scope:** Flashbang only — next C62 first-wave gear card after Bandage (**C63** closed Bandage's
numerics; Sim-side Bandage is on `master` @ `4e6bb66`). Written so Integrator can open a frozen
Sim/HUD contract **once the effect shape + numerics below are greenlit by the human** — this brief
does not greenlight them itself.
**Depends on:** [`PRODUCT_MEMORY.md`](../core/PRODUCT_MEMORY.md) **C62**, OPEN #16; [`CARD_COLLECTION.md`](CARD_COLLECTION.md)
§4.2, §6A, §8, §11; [`TDD.md`](../core/TDD.md) §4 Card Effects; [`PLAYBACK_CONTRACT.md`](../core/PLAYBACK_CONTRACT.md);
[`GEAR_BANDAGE_AGENT_BRIEF.md`](GEAR_BANDAGE_AGENT_BRIEF.md) (closed via C63 — pattern reference).
**Does not touch:** Interact-as-card (needs a station), Adrenaline real effect (PLAYBACK_CONTRACT tape
branch), Bandage HUD-side (open contract for UI seat). No Sim/resolve code is written or proposed as
final in this document.

---

## 1. What's already locked (do not re-litigate)

From **C62** / `CARD_COLLECTION.md` §6A, specific to Flashbang:

- **Phase:** Program-armed (not Execute-only — that's Adrenaline's special case).
- **Same gear deck:** every Character can use it; no Scout/Juggernaut exclusive; **flat cost** (no
  Strength carve-out — that carve-out is Interact-as-card only).
- **Economy:** full visible hand, no draw/RNG, no pre-match loadout gate (C62 economy model 1).
- **Job (direction only, not mechanics):** soft control / vision / interrupt — named in GDD/UI_FLOW
  but never given a resolve rule.
- **Strawman cost (still OPEN, not locked):** ~2s Time Resource, ~2 charges per Character per match
  (`CARD_COLLECTION.md` §6A). Treat as a starting number to greenlight or replace, not a spec.

**Still OPEN — this brief exists to make these concrete enough to decide, not to decide them:**
what "soft control" *does*, blast targeting model, radius/duration/delay numbers, charge count, and
whether the standing Bandage-only Sim carve-out expands to Flashbang (see §3 / §6).

---

## 2. What already exists in the repo (read this before assuming a blank slate)

Bandage has already landed the first gear verb shape. Flashbang is **not** a second Bandage — its
canonical design notes imply **schedule mutation**, which the current resolver architecture does not
do. Cross-check before writing a contract:

| File / doc | What it says | Cross-check |
|---|---|---|
| `CARD_COLLECTION.md` §6A | Flashbang: Program, **2s**, **2/match**, soft control/vision — radius, duration, and mechanical effect all OPEN | Source of the strawman numbers. Explicitly says this card needs effect design, not only numerics. |
| `Flashbang.asset` | `timeResourceCostSeconds: 3`, `oncePerMatch: 1`, `effectSummary: "Target room; Stun adds delay to target active segment. 1/match."` | **Diverges from §6A on every axis** (3s vs 2s, 1 vs 2 charges, room-target + segment delay vs vague soft control). Same pre-C62 scaffolding class as the Bandage brief found — do not trust as current truth. |
| `CardData.oncePerMatch` | `bool` | **Schema gap confirmed by Bandage wave:** a bool cannot encode §6A's **2 charges**. Bandage happened to fit 0/1; Flashbang does not. Any Flashbang contract that keeps multi-charge must widen this field (or stop using the SO as authority and track charges in match state like `BandageCharge`). |
| `GearHandView.FirstWave` | Flashbang listed as Program, `oncePerMatch: true`, cost label `"TR —"` | UI roster placeholder only — not wired into `ProgramHud` yet. The bool spent-set also cannot express "1 of 2 charges used." |
| `TDD.md` §4 Card Effects | Example: Flashbang at `12.0s` stuns victims in radius → **+3.0s delay**, shifting subsequent `ActionNode` times backward on that ghost's schedule | The only concrete mechanical sketch in product docs. Not confirmed as C#. |
| `DAY4_GHOST_RESOLVER_RESEARCH.md` §E.5 | Wound surcharge / Flashbang schedule-shift would break a single-pass resolver; recommended a time-ordered event-queue loop so re-timing can be added | **Still true today.** `GhostResolver` compiles each pawn's track once (`CompileTrack`), then resolves shots against those frozen tracks (`ResolveShots`). There is **no** path that mutates another pawn's remaining `ExecuteTime`s mid-resolve. |
| `ActionVerb` / Bandage pattern | `ActionVerb.Bandage` + `TapeEventType.Healed` + per-match `BandageCharge` threaded `GhostInput`→`GhostResolver`→`ReplayTape`→`RoundPlayback` | Proven pattern for a **self-state-flip** gear card. Flashbang is a **cross-pawn interrupt** — reuses the verb/tape/charge plumbing idea, not the resolve body. |
| `ActionNode.Modifier` (`CardData`) | Still nullable / unused on the wire (`RelayProtocol` sends null) | Bandage contract explicitly forbade piggybacking on this field. Same recommendation here — dedicated `ActionVerb.Flashbang`. |
| Continuous board (`ArenaBoard`) | Walls, doors, floors; **no first-class "room" object** | Asset's "Target room" language is pre-pivot / pre-C45 leftover. Continuous free-aim (Shoot-style point) or radius-around-point is the natural continuous reading; "room" would need new authored regions. |
| ART_DIRECTION | Cotton-wool Flashbang smoke as future VFX | Presentation only — does not imply a resolve rule. |
| `contracts/CURRENT.md` (Integrator dirty on main) | Bandage Sim carve-out is **Bandage-only**, not "gear generally" | Flashbang Sim work needs an explicit pause carve-out / C# row the way C63 did for Bandage — do not assume the Bandage exception covers the next card. |

**Bottom line:** scaffolding disagrees with the design strawman; the TDD sketch is the most concrete
effect proposal and also the most expensive architecturally. Decide the effect shape first — the
contract size follows from that decision, not from the asset file.

---

## 3. Open questions blocking a frozen contract

These need answers (human call, or Integrator judgment where flagged) before Sim/HUD signatures can be
frozen. **Q1 is the load-bearing one** — every other question's answer shape depends on it.

1. **What does Flashbang do mechanically?** Three candidate families already live in the docs/scaffolding
   — pick one (or a hybrid), do not leave "soft control" as prose:

   | Candidate | Source | What it does | Resolver cost |
   |---|---|---|---|
   | **A. Schedule stun (TDD)** | `TDD.md` §4; `Flashbang.asset` "Stun adds delay…" | At detonation second, victims in range get **+D seconds** appended to their remaining program (shift later `ActionNode.ExecuteTime`s) | **High** — requires mid-resolve re-timing / event-queue rewrite the Day 4 research already flagged. Touches shot windows that were authored against pre-stun times. |
   | **B. Soft vision / info** | `CARD_COLLECTION.md` "soft control/vision" | e.g. briefly reveal opponent path, or suppress FoW / aim cues for D seconds | **Medium** — may be presentation + limited resolve state; FoW is currently Out of ship (C17), so "vision" needs a concrete substitute (Reveal flash? aim-marker hide?) or it invents a system. |
   | **C. Action interrupt / cancel** | Interrupt reading of "soft control" | Cancel or freeze the victim's *currently active* segment at detonation (Move mid-leg stops; Shoot window aborts) without rewriting the rest of the schedule | **Medium-high** — closer to Door-block's "drop remaining queue" cousin, but needs precise "active segment" rules and tape events for the cancelled work. |

   Recommendation for Integrator framing (not locked): if the human wants the TDD fantasy, accept
   candidate **A** and size the Sim slot as an architecture pass, not a one-verb add. If the human
   wants the smallest shippable control tool after Bandage, prefer **C** with a short duration and
   no schedule rewrite. Do **not** ship "vision" until FoW or a substitute info channel exists.

2. **Targeting model in continuous space.** Asset says "target room"; Shoot is free-aim point (C39 —
   no pawn-ID lock). Candidates:
   - Free-aim **detonation point** + blast radius (mirrors Snap's aim point + `HitRadius` family).
   - Radius around the **thrower's own position** at `ExecuteTime` (no board click — scrubber-time only).
   - Authored **room / zone** id (needs new map data — not on `ArenaBoard` today).
   - Pawn-ID lock (fights C39's "bet on a place" discipline — not recommended).

3. **Cost + charge count.** Greenlight §6A's **2s / 2-per-Character-per-match**, or the asset's
   **3s / 1-per-match**, or other numbers? Confirm charges are per-**match** (Bandage/C63 precedent)
   not per-round.

4. **Blast geometry + LoS.** If Q1 needs victims-in-range: radius value? Does closed-door / wall LoS
   block the bang (continuous LoS like Shoot), or is it an omnidirectional pressure wave through
   walls? Through open doors into the next chamber?

5. **Friendly fire / self-hit.** Can the thrower stun themselves? Can it hit a stacked/ally pawn
   (demo is 1v1, but C40 allows same-point occupancy)?

6. **Interaction with Shoot windows.** If a stun lands during an opponent Hold Angle window, does
   the window shorten, abort, or shift? Candidate A must answer this explicitly or shot outcomes
   become ambiguous.

7. **Pause carve-out.** C63 / the Bandage contract lifted the Sim pause for **Bandage only**. Confirm
   an explicit Flashbang (or "next gear card") carve-out before any Sim slot opens — same shape C57
   / C63 used, not assumed.

---

## 4. Proposed Sim resolve shape (recommendation, not locked)

**Depends on Q1.** Two shapes, ranked.

### 4A. If human picks schedule stun (candidate A) — architecture-first

Do **not** pretend this is a Bandage-sized verb add. Contract should split:

1. **Resolver re-timing foundation** (Integrator or a dedicated Sim slot): change `GhostResolver` from
   compile-all-tracks-then-shots into a structure that can apply a time-shift to a victim's remaining
   nodes and re-evaluate later intents (Day 4 research §E.5). Define determinism rules when two
   Flashbangs / a Flashbang + Shoot share a simultaneity group.
2. **Flashbang verb** on top: `ActionVerb.Flashbang`, detonation at `ExecuteTime`, victims via Q2/Q4
   rules, emit tape event(s) (e.g. `FlashbangDetonated` + per-victim `Stunned` with shift amount),
   persist charges (int, not bool — §6A's 2 charges).

### 4B. If human picks interrupt / cancel (candidate C) — Bandage-sized + cross-pawn

Closer to shipped patterns:

- `ActionVerb.Flashbang` node; `Position` = detonation point (free-aim) or thrower position (Q2).
- At `ExecuteTime`, find victims in radius with LoS rule from Q4; for each, cancel/freeze the
  segment that contains that second (mirror Door-block's "drop assumptions that didn't hold" spirit
  without shifting later timestamps).
- New `TapeEventType`s through `PLAYBACK_CONTRACT.md` §5 checklist.
- Charge persistence: generalize Bandage's match-carry pattern to an int (`FlashbangChargesUsed` or
  remaining), because 2 charges break the 0/1 `BandageCharge` shape if copied naively.
- **`modifier` stays null** — same Bandage recommendation.

### 4C. Explicitly not recommended without new systems

- "Target room" as a first-class select — no room objects exist; inventing them for one card is map
  authorship scope, not a Flashbang brief.
- Riding `ActionNode.Modifier` as a Shoot/Move interrupt tag — unproven field, fights the dedicated-verb
  pattern Bandage just established.

---

## 5. Proposed HUD shape (recommendation, not locked)

`UI_FLOW.md` §6: click Flashbang to arm, then click path node / scrubber time (and, if Q2 is free-aim,
a board point) to place.

Concretely, once Bandage's HUD slot lands (open contract — UI seat):

- Reuse `GearHandView` Program-phase arm for `CardId.Flashbang` (already in `FirstWave`).
- Placement:
  - **Time** — same scrubber / timeline placement Bandage uses.
  - **Point** (if Q2 = free-aim) — same board-click aim affordance Shoot already uses; Flashbang is
    closer to Shoot targeting than to Door's board-anchored prompt cluster.
- Live state on the card: charges remaining (must support N>1 — today's `SetSpent` bool set is
  insufficient; HUD contract should widen that API or track counts beside it).
- Content-contract spirit (identity / live state / explicit confirm) still applies; board-anchored
  doc's *mandatory* trigger fires only if Q2 invents a board-object target (room/zone pick) — free-aim
  point does not.

---

## 6. Suggested contract split, once Q1–Q7 are greenlit

Mirror Bandage's Sim / HUD split in `docs/contracts/CURRENT.md`, but **gate the Sim slot on Q1**:

| If Q1 is… | Sim slot size | HUD slot size |
|---|---|---|
| **A (schedule stun)** | Architecture pass + verb (likely Integrator-led or two-step) | Arm + free-aim/time place + charge UI |
| **C (interrupt)** | Verb + radius resolve + tape events + int charges (Bandage-sized+) | Same HUD shape |
| **B (vision)** | Blocked until an info channel exists | Do not open |

**Before opening either slot:**

1. Human answers §3 (especially Q1 effect shape + Q3 numerics).
2. Explicit Sim pause carve-out for Flashbang (Bandage's carve-out does not automatically widen).
3. Prefer landing Bandage HUD first so Flashbang HUD can reuse the docked `GearHandView` placement
   pipeline instead of inventing a second one — sequencing guidance, not a hard blocker if Integrator
   wants parallel docs work.

---

## 7. Explicit non-goals of this brief

- Does not cover Interact-as-card or Adrenaline's real-effect redesign.
- Does not lock any number or pick Q1's winner.
- Does not write Sim/HUD code.
- Does not reconcile `Flashbang.asset` / widen `CardData.oncePerMatch` until a contract says so.
- Does not open the Bandage HUD-side slot (UI seat / Integrator).
- Does not rewrite `GhostResolver` "just in case" before Q1 is answered.

---

## See also

- [`CARD_COLLECTION.md`](CARD_COLLECTION.md) — §4.2, §6A, §8, §11 (Flashbang = third in build-risk order;
  Interact is mechanically simpler but blocked on stations)
- [`GEAR_BANDAGE_AGENT_BRIEF.md`](GEAR_BANDAGE_AGENT_BRIEF.md) — closed via **C63**; pattern reference
- [`PRODUCT_MEMORY.md`](../core/PRODUCT_MEMORY.md) — **C62**, OPEN #16 (Flashbang effect + numerics still OPEN;
  C63 on Integrator dirty tree resolves Bandage only)
- [`TDD.md`](../core/TDD.md) §4 — schedule-stun sketch
- [`DAY4_GHOST_RESOLVER_RESEARCH.md`](../core/DAY4_GHOST_RESOLVER_RESEARCH.md) §E.5 — re-timing risk
- [`PLAYBACK_CONTRACT.md`](../core/PLAYBACK_CONTRACT.md) — §5 extension checklist
- [`ART_DIRECTION.md`](../core/ART_DIRECTION.md) — cotton smoke VFX (presentation only)
- [`docs/contracts/CURRENT.md`](../contracts/CURRENT.md) — Bandage contract (Integrator-owned); where a
  Flashbang contract would open
- [`docs/departments/cards/STATUS.md`](../departments/cards/STATUS.md) — Cards dept status
