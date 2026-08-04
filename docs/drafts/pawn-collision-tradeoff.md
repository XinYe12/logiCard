# Pawn-vs-pawn collision — tradeoff draft (DECIDED → C40)

**Status:** **Option A confirmed 2026-08-04** as **C40** in `PRODUCT_MEMORY.md`. `GDD.md` §3.3 updated. This file is the archive of the rejected alternatives.
**Date:** 2026-08-04.
**Does not block:** continuous pivot Phases 1–6. Walls/doors already block movement + LoS; this is only about two pawns occupying the same space.

---

## What the grid actually did

The GDD line about an old "cannot share a tile" rule is **aspirational / tabletop-adjacent**, not something the digital grid demo enforced:

- `PawnProgram` / `OrthogonalPathfinder` never treated the other pawn as an impassable tile.
- `GhostResolver` never rejected overlapping end positions.
- Pawns do **not** block LoS (`DAY4_GHOST_RESOLVER_RESEARCH.md` E.3 — GDD §5 lists only closed doors).

So the continuous board is not losing a working collision system — it's deciding whether to **add** one for the first time.

Demo constraint that matters: **1v1** (two pawns). Multi-pawn body-blocking is out of 14-day scope.

---

## Option A — No pawn-vs-pawn blocking (recommended default for demo)

**Rule:** Two pawns may occupy the same point (or cross paths) freely. Wounds come only from Shoot (Snap / Hold). Contact is not a combat verb.

| Pros | Cons |
|---|---|
| Zero new resolve math; pathfinder / authoring stay obstacle-only (walls + closed doors) | Visual overlap can look broken if pawns stack mid-playback |
| Matches current grid behavior → no silent rule change mid-pivot | "Stand on them" can't be used as a soft body-block / choke |
| Keeps epistemics clean: Shoot is the only harm channel (C32/C39) | |
| Fits compressed schedule — nothing to tune in Phase 6 for collision | |

**Impl sketch (if chosen):** Explicit no-op — document in GDD §3.3 as CONFIRMED "pawns do not block each other." Optional later AV: slight Y-offset / ghost transparency when distance &lt; ε so stacking reads intentional.

---

## Option B — Minimum-separation radius

**Rule:** During resolve (and optionally during Program preview), two pawns may not come closer than `SeparationRadius` (start ~0.3–0.5 world units, near `HitRadius` scale). Paths that would violate it are illegal at authoring time, or are clipped / rejected at resolve.

| Pros | Cons |
|---|---|
| Reads more "physical" on a continuous board | New algorithm: continuous path–path distance over time (similar cost class to Hold Angle sweep) |
| Soft body-block / doorway contests become a spatial mind-game | Authoring UX hard under **blind programming** — you don't know the opponent's path, so rejection at Program time is incomplete; resolve-time rejection changes outcomes after Lock In |
| | Conflicts with "player controls route shape" (C21) if the system auto-reroutes around the ghost |
| | Needs Phase 6 tuning + tests; schedule risk under C34 compression |

**Sub-variants (if B is preferred):**

1. **Authoring-only soft warning** — never reject; HUD flashes "paths may collide" using last-known opponent position (weak under fog / blind program).
2. **Resolve-time hard reject** — illegal overlapping schedule becomes a no-op Move or stops at contact — surprises the player after Lock In (usually bad for a demo).
3. **Resolve-time slide-along** — push apart along the contact normal — looks like physics, fights C35's "no engine physics in resolve" discipline unless done with pure float math.

---

## Option C — Hybrid (block end-position only)

**Rule:** Paths may cross mid-route; pawns may not **finish** a Move (or end the round) within `SeparationRadius` of each other.

| Pros | Cons |
|---|---|
| Cheaper check (sample end points / held positions, not full sweep) | Mid-route stacking still looks odd |
| Stops "camp on their spawn" end-state cheese | Still needs a number + authoring/resolve policy under blind program |

---

## Decision (locked)

**Option A → C40.** No pawn-vs-pawn blocking for the 14-day ship. If stacking looks bad after Phase 4/5, prefer a **presentation fix** (offset / silhouette) over a gameplay collision rule.

If Option B is ever wanted post-demo, treat it as a new CONFIRMED row (not a silent GDD tweak) — it touches `ContinuousPathfinder` / `PawnProgram` / `GhostResolver` and needs the same determinism bar as Hold Angle.
