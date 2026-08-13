# Bandage — Gear Implementation Brief

**Status:** Closed 2026-08-13 via **C63** (human answered §3). Sim-side landed on `master` @ `4e6bb66`;
HUD-side still open in `docs/contracts/CURRENT.md` (UI seat). Kept as pattern reference for later gear briefs.
**Scope:** Bandage only — first of the C62 first-wave gear cards (Bandage, Interact-as-card, Flashbang,
Adrenaline). Originally written so Integrator could open a frozen Sim/HUD contract once the numerics
below were greenlit — that happened as C63.
**Depends on:** [`PRODUCT_MEMORY.md`](PRODUCT_MEMORY.md) **C62**, OPEN #16; [`CARD_COLLECTION.md`](CARD_COLLECTION.md)
§4.2, §6A, §8 Q1–Q3; [`GDD.md`](GDD.md) §5 (wound ladder); [`PLAYBACK_CONTRACT.md`](PLAYBACK_CONTRACT.md).
**Does not touch:** Flashbang, Interact-as-card, Adrenaline — each needs its own brief. No Sim/resolve
code is written or proposed as final in this document.

---

## 1. What's already locked (do not re-litigate)

From **C62** / `CARD_COLLECTION.md` §6A, specific to Bandage:

- **Phase:** Program-armed (not Execute-only — that's Adrenaline's special case).
- **Effect:** clears **Wounded → Healthy**. Does not touch **Dead**.
- **Same gear deck:** every Character can use it; no Scout/Juggernaut exclusive, no attr-scaled cost
  carve-out (that carve-out is Interact-as-card only, per C62).
- **Economy:** full visible hand, no draw/RNG, no pre-match loadout gate (C62 economy model 1).
- **Strawman cost (still OPEN, not locked):** ~3s, ~1 charge per Character per match. Treat as a
  starting number to greenlight or replace, not a spec.

**Still OPEN — this brief exists to make these concrete enough to decide, not to decide them:**
Time Resource cost, exact charge count, and the legality rule around "must be stationary" (§3 below —
this constraint isn't in `CARD_COLLECTION.md` at all; it surfaced from existing repo scaffolding, see §2).

---

## 2. What already exists in the repo (read this before assuming a blank slate)

A `LogiCard.Cards` assembly and four `CardData` ScriptableObject assets already exist
(`Assets/_Project/Cards/`) — placeholder scaffolding, not wired into any resolve path. Cross-checking
them against C62/§6A surfaced real findings worth knowing before writing a contract:

| File | What it says | Cross-check |
|---|---|---|
| `CardData.cs` | `CardId` enum (Bandage/Interact/Flashbang/Adrenaline), `timeResourceCostSeconds` (float), `oncePerMatch` (**bool**), `effectSummary` (free text) | Schema gap: `oncePerMatch` is a single boolean, but §6A's strawman charge counts aren't all 0/1 (Flashbang strawman = 2/match). A bool can't encode "N charges." Bandage's own "1 per Character per match" happens to fit a bool, but don't generalize the field to the other three cards without widening it to an int. |
| `Bandage.asset` | `timeResourceCostSeconds: 3`, `oncePerMatch: 0` (**false**), `effectSummary: "Clear Wounded. Must be stationary."` | Cost **matches** §6A's 3s strawman. `oncePerMatch: 0` **contradicts** §6A's "1 per Character per match" — either the asset is stale or the charge model is meant to be per-round, not per-match; needs a human answer, not an assumption. **"Must be stationary" is new information** — not written anywhere in `CARD_COLLECTION.md` §4.2/§6A. Someone encoded a real design constraint in scaffolding that the design doc never captured. See §3. |
| `Flashbang.asset` | `timeResourceCostSeconds: 3`, `oncePerMatch: 1`, `effectSummary: "Target room; Stun adds delay to target active segment. 1/match."` | **Diverges from §6A's strawman** (2s cost, 2 charges, vague "soft control/vision" effect). This asset describes a room-target stun, not what the design doc guesses. Out of scope for this brief, but flags that the `Cards/` scaffolding as a whole predates C62 and should not be trusted as current truth for any of the four cards without a reconciliation pass. |
| `Interact.asset` | `effectSummary: "Door / Vent / Monitor (current or adjacent tile)..."` | **Directly contradicts C62/§8 Q5**, which reserves Interact-as-card for future stations only and explicitly forbids migrating existing Door/Vent onto the hand. Also uses pre-continuous-space "tile" language (predates C35/C39). Confirms: this whole asset set is a pre-pivot, pre-C62 leftover — reconcile field-by-field when a contract actually opens, don't bulk-trust it. |
| `ActionNode.cs` | Has a `CardData Modifier` field, comment: *"Nullable — always null for Day 3, reserved for card interrupts."* | Confirmed dead: `RelayProtocol.cs:307` sends it as always-null on the wire. This is the one piece of scaffolding that looks like a real extension point (see §4) rather than stale content — but it's never been exercised, so its shape is a guess, not a proven contract. |
| `ActionVerb.cs` | `enum ActionVerb { Move, Shoot, Door }` | No gear verb exists yet. Adding Bandage means extending this enum (or an alternative shape — see §4). |
| `TapeEvent.cs` | `enum TapeEventType { MoveArrive, ShootFire, Wounded, Killed, Invalid, DoorOpened, DoorClosed }` | No heal/clear event exists. `PLAYBACK_CONTRACT.md` §5's extension checklist applies directly if one is added. |
| `GhostResolver.cs` | `WoundsUntilDead = 2`; wounds tracked as a plain `int` per pawn, only ever **incremented** (`victim.Wounds = WoundsUntilDead` / `Wounds++`-equivalent at the hit-resolve site) | No existing code path decrements wounds. Bandage is the **first** consumer of a wound-decrement — not just "reuse the existing wound system," a new direction through it. |
| `ReplayTape.cs` / `RoundPlayback.cs` | `EndWounds` (per-round-end wound count) flows into the next round via `GhostInput.StartingWounds`; `RoundPlayback`'s per-pawn runtime struct carries `Wounds` field-by-field across rounds (`RoundPlayback.cs:618`) | This is the pattern a persistent **charge count** (e.g. "Bandage used this match: yes/no") would need to follow — there is currently **no** persistent per-card-charge state anywhere in `GhostInput`/`RoundPlayback`'s runtime struct, only wounds. Confirmed by reading `GhostInput`'s full field list (`PawnId`, `Start`, `Payload`, `StartingWounds` — nothing card-related). |

**Bottom line:** don't treat `Assets/_Project/Cards/**` as already-decided data. Bandage's cost field
happens to agree with the design doc; everything else in that folder needs a human or Integrator pass
before it's trustworthy.

---

## 3. Open questions blocking a frozen contract

These need answers (human call, or Integrator judgment where flagged) before Sim/HUD signatures can be
frozen. None of these are "just numerics" — several are resolve-shape decisions.

1. **Cost + charge count.** Greenlight §6A's 3s / 1-per-Character-per-match strawman, or pick different
   numbers? (The existing `Bandage.asset` cost agrees; its `oncePerMatch` flag does not — see §2.)
2. **"Must be stationary" — what does that mean in continuous space?** This constraint exists only in
   the scaffolding's free-text `effectSummary`, never designed. Candidate readings, not yet decided:
   - The pawn has no other scheduled node (Move/Shoot/Door) whose time window overlaps Bandage's
     `ExecuteTime` — i.e., Bandage must be the only thing happening at that instant.
   - The pawn simply isn't mid-Sprint at that instant (weaker — allows Walk-stance micro-adjustments).
   - The pawn's position is unchanged for some window before/after `ExecuteTime` (needs a window length).

   This matters because it determines whether legality is a **Program-time client-side gate** (like the
   existing "must be within `InteractRadius` of a door" prompt gate) or a **resolve-time rule** the
   `GhostResolver` must enforce and potentially reject/no-op. Door's own precedent (`Door.cs`'s
   `DoorKind.Breach` comment) is that the resolver stays permissive and the *UI* enforces restrictions —
   worth defaulting to that same split unless there's a reason not to, but that's a call to confirm, not
   assume.
3. **Charge persistence granularity: per-match or per-round?** §6A says "per Character per match" (a
   single lifetime use across however many Time Card rounds the match has), which is a genuinely new
   category of state — see §2's `GhostInput` finding. Confirm this is really per-*match*, since that's
   the harder thing to build (needs new persistent state threaded through `RoundPlayback`), not per-round
   (which would fit the existing per-round `GhostInput` shape with zero new persistent fields).
4. **Legality when already Healthy.** Can a player arm Bandage on a pawn with 0 wounds? Presumably a
   UI-level no-op/disabled state (mirrors "why offer a control with no legal target," per
   `docs/UI_BOARD_ANCHORED_COMPONENTS.md`'s general content-contract principle even though that doc's
   *mandatory* board-object trigger doesn't technically apply here — see §5) rather than a resolver
   rejection. Confirm.
5. **Does board-anchored UI apply?** `CLAUDE.md` mandates reading
   `docs/UI_BOARD_ANCHORED_COMPONENTS.md` for any control that lets the player change **a board object's**
   state (door, station, pickup). Bandage targets **the player's own pawn**, not a board object — read
   literally, that doc's *mandatory* trigger doesn't fire. Flagging this explicitly so nobody either (a)
   skips reading it when a later card *does* need it (Interact-as-card almost certainly will, once a
   station exists), or (b) wastes time forcing Bandage's self-targeting UI through a board-object
   positioning pipeline it doesn't need. The doc's *content contract* (identity / live state / explicit
   options) is still good practice for Bandage's arm UI even though the doc's mandate doesn't strictly
   apply — see §5.

---

## 4. Proposed Sim resolve shape (recommendation, not locked)

Two shapes are available given what's already in the codebase; recommending one, flagging the other.

**Recommended: new `ActionVerb.Bandage`, own `ActionNode` row.** Same shape Door already uses — a
schedulable node with its own `ExecuteTime`, resolved in the existing per-pawn time-ordered sweep
(`GhostResolver.cs` already sorts each pawn's nodes by `ExecuteTime` before resolving — Bandage would
slot into that sweep like any other verb). Concretely, this would need:

- `ActionVerb.Bandage` added to `ActionVerb.cs`.
- A resolve step in `GhostResolver` that, at the node's `ExecuteTime`, decrements the acting pawn's
  wound count by 1 if `Wounds > 0` (first-ever wound-decrement in the codebase — not "reuse existing
  logic," write it carefully against the existing increment site's exact field mutation).
- A new `TapeEventType` (e.g. `Healed`) so `RoundPlayback.ApplyTime` has something to present — go
  through `PLAYBACK_CONTRACT.md` §5's extension checklist item by item (tape event with correct
  `Seconds`, a presenter row in the §3 matrix, no per-tick FX restart, PlayMode scrub tests at
  `event.Seconds - ε` / `event.Seconds`, enum-coverage test update).
- Whatever §3 Q3 resolves to (per-match charge) needs new persistent state threaded the same way
  `Wounds` already is: `GhostInput` gains a starting-charge field, `RoundPlayback`'s per-pawn runtime
  struct gains a charges-used field, `ReplayTape` gains an end-of-round charges-used map mirroring
  `EndWounds`. This is real new plumbing, not a one-line add — size it accordingly when scoping the
  contract.

**Not recommended without more justification: reusing `ActionNode.Modifier` (`CardData`).** It exists,
labeled "reserved for card interrupts," but it's never been exercised end-to-end, so its intended shape
(is it a modifier riding on a Move node? a standalone node with `Verb` still meaningless?) is a guess.
Piggybacking Bandage onto an unproven field risks discovering mid-implementation that the field's shape
doesn't fit a self-contained schedulable action. A dedicated `ActionVerb.Bandage` costs one enum value
and follows a pattern (Door) that's already shipped and tested twice.

---

## 5. Proposed HUD shape (recommendation, not locked)

`UI_FLOW.md` §6 item 3 already describes the general gear interaction: *"click Bandage ... to arm, then
click the path node or scrubber time to place."* Concretely, in `ProgramHud.cs` terms:

- A fourth mode button alongside the existing `Mode_Door`/Move/Shoot triplet (`ProgramHud.cs:536`
  pattern) — `SetMode(ActionVerb.Bandage)`.
- Unlike Door, Bandage doesn't need a board-anchored prompt cluster (`BuildDoorPrompt`) or the
  world→screen→canvas-local positioning pipeline `UI_BOARD_ANCHORED_COMPONENTS.md` covers — it has no
  board-object target. It's closer to a scrubber-time placement, so likely reuses whatever timeline/path
  placement affordance Move nodes already use for picking `ExecuteTime`, not the door-prompt affordance.
- Even without the board-object doc's mandatory trigger, keep its content-contract spirit for the arm UI:
  show **identity** (which pawn), **live state** read from the authoritative model — current wound state
  and charges remaining, never inferred from what the player has clicked — and **options** as an explicit
  confirm (arm → place → confirm), not an implicit single click that immediately commits Time Resource.
- Disabled/hidden state when the pawn is already Healthy or charges are exhausted (§3 Q4).

---

## 6. Suggested contract split, once numerics are greenlit

Mirrors this project's existing worker-pair pattern (`docs/PARALLEL_OPS.md`, `docs/contracts/CURRENT.md`
closed-contract examples like the Phase 2 relay slice): a Sim-side worker (`ActionVerb`, `GhostResolver`,
`TapeEvent`, persistence plumbing) and a HUD-side worker (`ProgramHud` mode + arm/place UI) against a
frozen node/event signature the Integrator writes first — same shape as the Door contract already used
once. **Before opening either slot, two gates need to clear, not one:**

1. Human greenlights §3's open numerics/design questions (this brief's whole point).
2. The standing core-gameplay/Sim pause (`SCHEDULE.md` Phase 2 row, `docs/contracts/CURRENT.md`'s
   "`Sim/`, `Net/`, `Timeline/`, `GhostResolver`: core gameplay/networking paused" line) gets an explicit
   carve-out for gear work — the same way **C57** explicitly carved out map/terrain Sim work from that
   pause rather than assuming it. Don't start a Bandage contract on the assumption the general pause
   doesn't apply; get the same kind of explicit lift C57 got.

---

## 7. Explicit non-goals of this brief

- Does not cover Flashbang, Interact-as-card, or Adrenaline's real-effect redesign — each needs its own
  brief when its turn comes (§11's suggested order in `CARD_COLLECTION.md` still applies).
- Does not lock any number. §3's questions are open until a human answers them.
- Does not write or scaffold Sim/HUD code. Everything in §4/§5 is a recommendation for the contract the
  Integrator would write, not a merged design.
- Does not reconcile the other three `Cards/*.asset` files found stale in §2 — flagged for awareness,
  fixing them is in scope only when their own briefs come up.

---

## See also

- [`CARD_COLLECTION.md`](CARD_COLLECTION.md) — §4.2, §6A, §8 (source of the confirmed direction this
  brief narrows to Bandage specifically)
- [`PRODUCT_MEMORY.md`](PRODUCT_MEMORY.md) — **C62**, OPEN #16
- [`PLAYBACK_CONTRACT.md`](PLAYBACK_CONTRACT.md) — §5 extension checklist, applies directly to any new
  `TapeEventType`
- [`GDD.md`](GDD.md) §5 — wound ladder (Healthy/Wounded/Dead) Bandage operates on
- [`docs/UI_BOARD_ANCHORED_COMPONENTS.md`](UI_BOARD_ANCHORED_COMPONENTS.md) — why its mandatory trigger
  doesn't fire for Bandage's self-targeting UI, and why its content-contract spirit still applies (§5)
- [`docs/contracts/CURRENT.md`](contracts/CURRENT.md) — where a real Bandage contract would be opened;
  Integrator-only to edit
- [`docs/departments/cards/STATUS.md`](departments/cards/STATUS.md) — Cards dept status, updated
  alongside this brief
