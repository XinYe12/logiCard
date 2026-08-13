# AI Fallback Bot — Invisible Matchmaking Substitute

**Doc ID:** D15  
**Status:** Drafted 2026-08-08 (**C49**)  
**Depends on:** [VISION.md](VISION.md), [SCOPE.md](SCOPE.md), [PRODUCT_MEMORY.md](PRODUCT_MEMORY.md), [NETWORKING_DESIGN.md](NETWORKING_DESIGN.md), [SCHEDULE.md](SCHEDULE.md)  
**Authority:** Matchmaking-fallback bot only — **not** a marketed single-player / practice mode (**C49**). Greenfield beyond today's scripted-defender stub.

---

## Scope (read first)

This bot is an **invisible substitute opponent** when matchmaking cannot find a human within the queue timeout. It exists so F2P PvP queues stay livable at low population (**R17** / **R18** territory in [RISKS.md](RISKS.md)).

It is **not**:

- The deep single-player PvE campaign explicitly excluded in [VISION.md](VISION.md) Non-Goals.
- A marketed "Practice vs AI" mode, tutorial campaign, or bot-difficulty select screen.
- A content pipeline with its own maps, verbs, or balance track separate from PvP.

`VISION.md`'s non-goal **stays**. **C49** is a narrow carve-out: an invisible seat-fill is a different class of feature from a marketed PvE mode. If a future design starts needing bot-only missions, difficulty ladders, or bot-specific cosmetics, that design has left this doc's scope and must reopen VISION — do not quietly grow into PvE here.

---

## Disclosure policy — recommendation (human confirm)

**Recommendation: never disclose.** The UI must not label the match "vs AI," "Practice," or "Bot match." From the human player's point of view it is a normal Attacker/Defender queue result.

**Why recommend no disclosure:**

- Preserves F2P trust in matchmaking ("I got a game") without over-promising a PvE mode that does not exist and is a VISION non-goal.
- Avoids training players to dodge "real" matches or farm bots for cosmetics ([MONETIZATION.md](MONETIZATION.md) free-earn track must assume some bot matches will happen).
- Keeps the product pitch honest: this is a PvP game with a population safety net, not an AI tactics game.

**This is a recommendation for the human to confirm, not a locked CONFIRMED row.** If disclosure is later required (platform policy, regional rule, store page honesty), update this doc and add a PRODUCT_MEMORY confirmation — do not treat the recommendation as already binding.

---

## Difficulty / behavior bounds

| Bound | Requirement | Why |
|-------|-------------|-----|
| **Floor — competent enough** | A real player should not *immediately* clock it as fake (no spinning in place, no never-shooting, no ignoring an open door LoS for three rounds straight) | Protects matchmaking trust under the no-disclosure recommendation |
| **Ceiling — not a PvE product** | Must **not** require its own balance spreadsheet, custom maps, unique-verb kit, or content cadence the way a marketed PvE mode would | Protects the VISION non-goal in practice, not just in name |
| **Verb parity** | Uses the same Move / Shoot / Door verbs and Scout/Juggernaut attrs as humans | Stays inside the shipping core loop (**C46** — loop unchanged) |
| **No bot-only cheats** | No omniscient future-sight into the human's locked payload, no ignoring LoS/walls, no free Time Resource | Fairness + same resolve pipeline |

Tune toward "plausible mediocre human," not "perfect information solver" and not "afk dummy."

---

## Reference implementation (do not design from scratch)

A working scripted choreography already exists:

- `Assets/_Project/Boot/GameBootstrap.cs` → `BuildDefenderPayload`
- Helpers: `TryScriptMove` / `TryScriptDoor` / `TryScriptShoot` against `PawnProgram`

Today it rebuilds each Lock In from the defender's carried point and the round allotment: approach Door #1, open if needed, Snap toward an ambush point, edge back. It already respects over-budget / out-of-range failure as soft no-ops, and it learned (2026-08-06 playtest) not to blindly re-open a door whose live state already matches.

**Use that shape as the starting reference** for Phase 3 — a deterministic program builder that emits a normal `TimelinePayload` — not a from-scratch GOAP / ML project. Expanding it (react to open doors, vary Snap vs Hold, play Attacker seat too) is expected; replacing the whole resolve path is not.

---

## Trigger condition

| Item | Status |
|------|--------|
| Trigger | Matchmaking **queue timeout** with no human opponent ([NETWORKING_DESIGN.md](NETWORKING_DESIGN.md) session flow) |
| Timeout length | **OPEN numeric** — do not invent a value here |
| Mid-match substitute | OPEN — tied to networking reconnect/disconnect policy; default assumption is bot fills **at match start only**, not mid-round, until that policy lands |

---

## Hard technical constraint — same resolve pipeline

The bot **must** resolve through the same deterministic event-stream / host-authoritative model a real player's program uses:

1. Bot builds a `TimelinePayload` (`ActionNode[]`) through the same scheduling APIs (`PawnProgram` or successor).
2. Authority (`GhostResolver` or its networked successor) ghost-sims bot + human payloads together.
3. Outcomes arrive only as `ReplayTape` events — never as bot-side wound RPCs.

This is the same discipline [CHARACTER_ROSTER_LONGTERM.md](../character/CHARACTER_ROSTER_LONGTERM.md) states for unique verbs, and the same "Host never trusts client math, only tape events" invariant in [TDD.md](TDD.md) / [NETWORKING_DESIGN.md](NETWORKING_DESIGN.md).

**A bot that special-cases outside that pipeline** (applies damage directly, skips LoS, mutates board state without a tape event) is a hard reject — it would break determinism, desync replay, and make host-integrity auditing impossible.

---

## Phase placement

Per [SCHEDULE.md](SCHEDULE.md): **Phase 3 — Matchmaking Fallback Bot**, after Phase 2's real transport / session interfaces exist. The bot substitutes in **behind those interfaces**, invisibly, meeting the bounds above.

---

## OPEN summary (Integrator tracking)

| # | Decision |
|---|----------|
| 1 | Disclosure policy — confirm or reject the "never disclose" recommendation |
| 2 | Queue timeout length (seconds) |
| 3 | Whether bot may fill mid-match on opponent disconnect (depends on networking reconnect policy) |
| 4 | Minimum behavior bar acceptance test ("cold observer can't clock it in N seconds") — qualitative until playtest |
| 5 | Whether bot may earn/grant cosmetics progress for the human (anti-farm implications for [MONETIZATION.md](MONETIZATION.md)) |
