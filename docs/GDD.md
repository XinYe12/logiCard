# D4: Game Design Document (v0.2) — Core Ruleset

**Doc ID:** D4  
**Status:** Revised 2026-08-08 — **C46 full scope pivot** (14-day-demo framing retired; this ruleset is now the shipping product's binding rules, unchanged in substance — see `PRODUCT_MEMORY.md` C46). Prior: 2026-08-03 continuous-space pivot (C35 promoted to demo scope + C39 technical decisions — see [CONTINUOUS_PIVOT_PLAN.md](CONTINUOUS_PIVOT_PLAN.md)); C21 amended; 2026-07-30 C34 Polished Core Demo (superseded).  
**Depends on:** [VISION.md](VISION.md), [SCOPE.md](SCOPE.md), [CORE_LOOP.md](CORE_LOOP.md), [PRODUCT_MEMORY.md](PRODUCT_MEMORY.md), [ART_DIRECTION.md](ART_DIRECTION.md), [CONTINUOUS_PIVOT_PLAN.md](CONTINUOUS_PIVOT_PLAN.md)

**Scope note:** the board is **continuous position**, not a discrete grid (**C35/C39**, amended 2026-08-03 — this reverses an earlier "long-term only" framing after a cold-observer playtest). Distances/costs below still use the same numeric footprint and formulas the grid version used (`[0,4]×[0,4]`, `seconds = distance × BaseSecondsPerTile × StanceMult`) — only the underlying coordinate/LoS/pathfinding math changed, from tile-based to continuous. Full phased implementation: [CONTINUOUS_PIVOT_PLAN.md](CONTINUOUS_PIVOT_PLAN.md). Destructible geometry, an asymmetric objective win, and a Downed+revive+Detonator system remain **long-term-only**, not part of this pivot — see `PRODUCT_MEMORY.md` **C36–C38**.

This document defines rules, numeric tuning, and content for the **shipping product**. Focus: a **readable timeline duel** that looks like a handmade desk-lamp miniature — not a feature checklist.

**Core model:**
1. **Character Card** sets base attributes (Speed / Agility / Strength).  
2. **Time Card** commits **N** seconds from a shared match Time Resource pool (**C33**).  
3. **Movement** = draw path + allot time → stance (Sprint / Tactical Walk / Stealth Crawl). Base verb, not a card.  
4. **Shoot** = aim (free-aim point + LoS — **C35/C39**) + allot time → mode (Snap Shot / Hold Angle). Base verb, not a card.  
5. **One door** = contextual map action (open/close) that blocks move + LoS — **not** a full Interact card system for this ship.

Gear cards (Bandage / Flashbang / Adrenaline / Interact-as-card), Otherwise Stop, and attic/vent/monitor remain **future roadmap** (**C46**, amends **C34**). Fusion/real networking is now core ship scope (**C51**); Android is a separate future consideration (**C6**/**C48**).

---

## 1. Structure & Match Flow

- **Player Count:** 1v1 (Attacker vs Defender) — spawn labels; same *rules*; Character Cards may differ (Section 2). Local play remains supported for testing/offline; online PvP via real networking is the ship target (**C51**) — see `NETWORKING_DESIGN.md`. A matchmaking-fallback bot (**C49**) may substitute an opponent — see `AI_FALLBACK_BOT.md`.
- **Win Condition:** Opponent Physical State → **Dead**.
- **Map:** One **continuous ground arena** for the demo ship — a multi-room `[0,8]×[0,10]` footprint (Yard →
  a walled Hall kill-box with two doors → Vault, plus two unguarded flank corridors around Hall) per **C45**,
  superseding the earlier single-room `[0,4]×[0,4]` footprint. Attic / vent / monitor deferred (**C34**).
  Distances are **continuous (Euclidean)**, not tile counts (**C35/C39**).
- **Match Loop (C33 / C4):**
  1. **Select Character Card** (pre-match).
  2. **Allot (Time Card):** current chooser commits **N** from the shared match pool (round 1 = Attacker; then alternates). **N spent in full.**
  3. **Program Phase (30 real-world seconds):** Both sides simultaneously draw path(s), allot stance time, schedule Shoot mode(s), optionally toggle the door from within `InteractRadius`; lock.
  4. **Reveal Phase:** Paths + scheduled actions face-up.
  5. **Execution Phase:** Host (local authority for demo) resolves continuous **Time Resource**; clients/UI play **ReplayTape** using separate **Playback Duration** (**C27**).
  6. **Aftermath:** Carry positions + wounds; if pool can fund another MinRound and nobody is Dead → next Allot; else Match Over.

---

## 2. The Character Card (Base Attributes)

Before the match, each player selects a **Character Card (Loadout)**. Attributes convert the round allotment **N** into map action.

| Attribute | Meaning | Demo examples |
|-----------|---------|----------------|
| **Speed** | How far they move per second (continuous distance, not tile count — **C35/C39**) | Scout: **1.5 units / s base** · Juggernaut: **0.75 units / s base** *(same numeric magnitude as the old "tiles/s"; demo-tuned)* |
| **Agility (Handling)** | Transition cost between stances and between Shoot modes (Snap ↔ Hold Angle) | Scout: **0s** · Juggernaut: **+1s** once when leaving Sprint / when switching Snap ↔ Hold (**C25**; placeholder magnitude) |
| **Strength** | Physical interaction time | Door open/close: Scout **slower** · Juggernaut **faster** (Section 6) |

**Demo cast (IN):** **Scout** and **Juggernaut** (same Move/Shoot verbs; different attributes only).

---

## 3. How Movement Works (UI & Rules)

There is **no Walk card**. Movement is built-in programming:

### 3.1 Draw the Path (amended 2026-08-03, C21 + C35/C39)
- Select your character → tap anywhere on the continuous board to add a **waypoint**; tap again elsewhere to add another. The path is the ordered sequence of tapped waypoints, not a single system-computed shortest route to one destination.
- Consecutive waypoints connect leg-by-leg via the shortest legal route between them (continuous, any direction — not orthogonal-only), so the player isn't forced to tap every intermediate point — but the player chooses the **shape** of the route (which way to go around an obstacle, which order to visit points) by choosing where the waypoints land, rather than the system always picking its own single path.
- **Revisiting or crossing a previously-tapped point is legal** — no restriction against a route that loops back on itself.
- Path cannot cross a **closed door**'s segment or leave the arena bounds; if blocked at resolve → movement fails at the block (demo: stop before the door — full Otherwise card library is post-demo).

### 3.2 Pick a Stance → Automatic Cost (amended 2026-08-03, C21 — supersedes the original time-allotment-slider model)
- Player picks **Sprint / Tactical Walk / Stealth Crawl directly** for the path. There is no manual time-allotment slider or step where the player pre-commits seconds — the player never chooses "how much time to allot"; they choose a stance, and cost follows.

| Stance | Effects (demo) |
|--------|----------------|
| **Sprint** | Fastest; loud; **cannot fire** while sprinting; **evasive** vs Snap Shot |
| **Tactical Walk** | Medium; gun up / ready; can Shoot; **not** evasive |
| **Stealth Crawl** | Slowest; silent; not evasive vs Hold Angle |

- Cost is computed automatically — `seconds = distance * BaseSecondsPerTile * StanceMult` per leg (distance now Euclidean, not a tile count — **C35/C39**; align with paper D5's rate), summed across every leg of the waypoint path — and **deducted from the round's Time Resource the instant the Move is scheduled**. The HUD/scrubber shows the running used/budget total as it depletes; it never shows a pre-commit allotment value to confirm. Must stay readable on the **timeline scrubber**. Playback Duration is independent (**C27**).
- Shoot (§3A.2) already worked this way — pick Snap or Hold, cost follows automatically — so this brings Move in line with Shoot rather than introducing a second new pattern.

### 3.3 Collision
- Closed door segment blocks movement across it (§4).
- **Pawns do not block each other** (**C40**). Two pawns may occupy the same point or cross paths; contact is not a combat verb. Wounds come only from Shoot (`HitRadius` / `LaneHalfWidth` — **C39**). Matches the grid demo's actual behavior (the old "cannot share a tile" line was never enforced in digital code). Optional AV (slight offset when stacked) is presentation-only, not a gameplay rule. Tradeoff archive: [`docs/drafts/pawn-collision-tradeoff.md`](drafts/pawn-collision-tradeoff.md).

---

## 3A. How Shooting Works (UI & Rules)

There is **no Snap Shot / Hold Angle card**. Shooting is a base verb every Character has.

### 3A.1 Aim (amended 2026-08-03, C35/C39 — free-aim point)
- Declare a **shoot** action aimed at any **point** on the board — not a tile, and not a row/column-locked direction. This is a deliberate design choice (Decision 1 in `CONTINUOUS_PIVOT_PLAN.md`): the player is still betting on a *place*, not locking onto a specific pawn, matching the blind-programming bluff the row/column rule used to give.
- Requires clear LoS at resolve time (**C32**'s rules carry over; LoS math is now continuous segment-vs-obstacle intersection, not Bresenham — **C35/C39**), same floor, doors block.

### 3A.2 Allot the Time → Mode

| Mode | Relative allotment | Effects (demo) |
|------|--------------------|-----------------|
| **Snap Shot** | Fast (minimal time) | Wounds on hit; **misses** targets in **Sprint** stance; hits any pawn within **`HitRadius`** of the aim point at completion (**C32**, radius replaces "aimed tile" per **C39**) |
| **Hold Angle** | Slow (aim-lock window) | Lethal on hit; **hits** targets in **Sprint** stance; covers a **`LaneHalfWidth`**-wide lane along the origin→aim line for its window (**C39** — analytic sweep, not tile membership) |

Base Time Resource costs (Section 6) are placeholders; Agility scaling (**C25**) must stay readable on the scrubber.

---

## 4. Door (Map Action — Not a Gear Card)

For this ship, **two doors** on the continuous arena (each a wall **segment** with a gap, amended 2026-08-03 — was a grid tile, see **C35/C39**; expanded from one door on the earlier single-room board to two on the multi-room board, **C45**):

- Scheduled as a **contextual map action** when the pawn is within **`InteractRadius`** of the door's segment during Program (tap door → open/close at a booked Time Resource second) — replaces the old "current/adjacent tile" rule.
- **Closed** blocks movement across the segment and **blocks LoS** across it.
- **Open** allows move + LoS.
- Strength modifies open/close Time Resource cost (Section 6).
- This is **not** the full Interact card / vent / monitor kit (**C34**).

---

## 5. Wound System & Combat Resolution

### Physical states
| State | Effect |
|-------|--------|
| **Healthy** | Normal. |
| **Wounded** | Visible wounded state; second wound (or Hold Angle lethal) → **Dead**. *(Full +1s surcharge and Bandage-by-next-round bleed remain **future roadmap** (**C46**, amends C34) — this ship still shows wound → death so combat stakes read.)* |
| **Dead** | Eliminated (Hold Angle lethality / second wound / mutual kill). |

### Shooting & LoS
- Same floor only; continuous segment-vs-obstacle LoS (was orthogonal Bresenham — **C35/C39**); blocked by closed doors (**C32**).
- **Snap Shot:** completes → any target within `HitRadius` of the aim point **Wounded** if LoS; **misses Sprint**.
- **Hold Angle:** lethal on LoS hit within `LaneHalfWidth` of the aim line; **hits Sprint**; duration as scheduled.
- **Mutual lethal** same second → **Draw**.

---

## 6. Main Numeric Setup (Demo)

| Attribute | Value | Notes |
|-----------|-------|-------|
| **Match Time Resource pool** | **900 seconds** (15 min) | Shared; Time Cards carve rounds (**C33**) |
| **Min round / Time Card** | **30 seconds** | Clamp floor |
| **Time Card presets (Allot UI)** | **30s / 60s / 120s / ALL IN** (remaining pool) + custom slider | Slider ranges `[MinRoundSeconds, Remaining]`; ALL IN commits whatever pool remains |
| **Program Timer** | 30 **real-world** seconds | Wall-clock planning (**C27**) |
| **Playback Duration** | Tunable per-TR-second rate | Cinema length ≠ Time Resource |
| **Otherwise / gear cards** | — | Deferred (**C34**) |

**Removed:** discrete 12-tick Master Clock; fixed 60s-per-round-only model (superseded by C33).

### Character presets (demo)

| Preset | Base move | Agility | Door open/close |
|--------|-----------|---------|-----------------|
| Scout | 1.0 s / unit Walk baseline *(tune)* | Stance / Shoot-mode change **0s** | Door **4s** |
| Juggernaut | 2.0 s / unit Walk baseline *(tune)* | Stance leave-Sprint **+1s**; Snap↔Hold **+1s** | Door **2s** |

Placeholder magnitudes — tune in playtest. "unit" = continuous board distance at the same numeric scale the old "tile" did (**C35/C39**).

### Shoot modes (base verb)

| Mode | Base cost / duration | Effect |
|------|----------------------|--------|
| **Snap Shot** | **2s** | Wound on LoS within `HitRadius` of the aim point; misses Sprint |
| **Hold Angle** | **3s** aim lock | Lethal on LoS; hits Sprint |

### Deferred cards (future roadmap — not part of this ship's core loop)

| Card | Notes |
|------|-------|
| Bandage / Interact-as-card / Flashbang / Adrenaline | Confirmed long-term design; **not in this ship's core loop** |

---

## 7. Map Elements

| Element | This ship |
|---------|-------------|
| **Two Doors** | Contextual open/close (radius-based interact — **C39**); blocks move + LoS when closed — was one door pre-**C45** |
| **Continuous ground arena, multi-room** (`[0,8]×[0,10]` footprint — **C45**) | Yes |
| Attic / Vent / Monitor | **Future roadmap** |
| 高铁 / High-speed rail (**C31**) | Confirmed design; **future roadmap** |

---

## 8. Presentation (commercial ship art bar)

The ship must read as a **Desk-Lamp Diorama**, not a default Unity prototype. Binding art/audio bar lives in [ART_DIRECTION.md](ART_DIRECTION.md) § Commercial ship art bar. Summary:

- Board on a physical base in a dark void; warm desk-lamp lighting; clay-like materials.
- Sketchy **线稿涂鸦** paths on the clay board (FragPunk/界外狂潮-style ink line — see ART_DIRECTION; supersedes yarn/chalk); cardstock Time Card; AR scrubber contrast.
- Stepped pawn motion; physical muzzle flash; clay wound splat.
- Tactile foley for move, shot, Time Card, Lock In.

Full SSS, thumbprint maps, and bespoke character rigs are in scope for Phase 5 (Commercial Art Bar) — see `ART_DIRECTION.md` and `SCHEDULE.md`.

---

## 9. Out of Scope

- Full Android polish / dual-platform ship (**C6** → Windows polished; Android smoke optional).
- FoW, decoys, facing cones, numeric HP bars, armor.
- Gear cards: Bandage, Flashbang, Adrenaline, Interact-as-card.
- Otherwise library; attic; vent; monitor; loot; 高铁.
- Final Link’s Awakening–level clay shaders / complex mocap.

---

## 10. Acceptance

1. A new player can complete **at least two Time Card rounds** locally (Allot → Program → Reveal → Execute → Aftermath → Allot).  
2. Path + stance and Snap / Hold produce understandable tactical consequences on the **Time Resource scrubber**.  
3. **One door** materially changes movement or LoS at least once.  
4. Without narration, an observer reads the scene as a **handmade desk-lamp miniature** — not stock Unity primitives.  
5. Move, Shoot, hit, Time Card, and Lock In each have **distinct** visual and/or audio feedback.  
6. **Windows build** is ship-stable (reliable for repeated full-match play).

Scout vs Juggernaut feel different; cause/effect order remains readable even when Playback Duration compresses Time Resource.
