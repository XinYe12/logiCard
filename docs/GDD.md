# D4: Game Design Document (v0.2) — Polished Core Demo

**Doc ID:** D4  
**Status:** Revised 2026-08-03 — **C21 amended** (multi-waypoint path + automatic Time Resource cost, no allotment slider); Revised 2026-07-30 — **C34 Polished Core Demo** (art + tight gameplay); Time Card match loop (**C33**)  
**Depends on:** [VISION.md](VISION.md), [SCOPE.md](SCOPE.md), [CORE_LOOP.md](CORE_LOOP.md), [PRODUCT_MEMORY.md](PRODUCT_MEMORY.md), [ART_DIRECTION.md](ART_DIRECTION.md)

**Scope note:** the tile/grid rules below (5×5 board, orthogonal Bresenham LoS, Healthy→Wounded→Dead) describe the **14-day demo only**. The long-term map/win/revive model differs — continuous movement, destructible geometry, an asymmetric objective win, and a Downed+revive+Detonator system — see `PRODUCT_MEMORY.md` **C35–C38**.

This document defines rules, numeric tuning, and content for the **14-day portfolio prototype**. Focus: a **readable timeline duel** that looks like a handmade desk-lamp miniature — not a feature checklist.

**Core model:**
1. **Character Card** sets base attributes (Speed / Agility / Strength).  
2. **Time Card** commits **N** seconds from a shared match Time Resource pool (**C33**).  
3. **Movement** = draw path + allot time → stance (Sprint / Tactical Walk / Stealth Crawl). Base verb, not a card.  
4. **Shoot** = aim (target tile / LoS) + allot time → mode (Snap Shot / Hold Angle). Base verb, not a card.  
5. **One door** = contextual map action (open/close) that blocks move + LoS — **not** a full Interact card system for this ship.

Gear cards (Bandage / Flashbang / Adrenaline / Interact-as-card), Otherwise Stop, attic/vent/monitor, Fusion online, and full Android polish are **post-demo** (**C34**).

---

## 1. Structure & Match Flow

- **Player Count:** 1v1 (Attacker vs Defender) — spawn labels; same *rules*; Character Cards may differ (Section 2). Local play for the 14-day ship; online is post-demo (**C34**).
- **Win Condition:** Opponent Physical State → **Dead**.
- **Map:** One **5×5** ground grid for the demo ship. Attic / vent / monitor deferred (**C34**). Distances use **tiles**.
- **Match Loop (C33 / C4):**
  1. **Select Character Card** (pre-match).
  2. **Allot (Time Card):** current chooser commits **N** from the shared match pool (round 1 = Attacker; then alternates). **N spent in full.**
  3. **Program Phase (30 real-world seconds):** Both sides simultaneously draw path(s), allot stance time, schedule Shoot mode(s), optionally toggle the door from a legal tile; lock.
  4. **Reveal Phase:** Paths + scheduled actions face-up.
  5. **Execution Phase:** Host (local authority for demo) resolves continuous **Time Resource**; clients/UI play **ReplayTape** using separate **Playback Duration** (**C27**).
  6. **Aftermath:** Carry positions + wounds; if pool can fund another MinRound and nobody is Dead → next Allot; else Match Over.

---

## 2. The Character Card (Base Attributes)

Before the match, each player selects a **Character Card (Loadout)**. Attributes convert the round allotment **N** into map action.

| Attribute | Meaning | Demo examples |
|-----------|---------|----------------|
| **Speed** | How far they move per unit time allotted | Scout: **1.5 tiles / s base** · Juggernaut: **0.75 tiles / s base** *(demo-tuned; legacy docs said “tiles/tick” — digital uses continuous seconds)* |
| **Agility (Handling)** | Transition cost between stances and between Shoot modes (Snap ↔ Hold Angle) | Scout: **0s** · Juggernaut: **+1s** once when leaving Sprint / when switching Snap ↔ Hold (**C25**; placeholder magnitude) |
| **Strength** | Physical interaction time | Door open/close: Scout **slower** · Juggernaut **faster** (Section 6) |

**Demo cast (IN):** **Scout** and **Juggernaut** (same Move/Shoot verbs; different attributes only).

---

## 3. How Movement Works (UI & Rules)

There is **no Walk card**. Movement is built-in programming:

### 3.1 Draw the Path (amended 2026-08-03, C21)
- Select your character → tap a tile to add a **waypoint**; tap again elsewhere to add another. The path is the ordered sequence of tapped waypoints, not a single system-computed shortest route to one destination.
- Consecutive waypoints connect leg-by-leg via the shortest legal orthogonal route between them, so the player isn't forced to tap every intermediate tile — but the player chooses the **shape** of the route (which corners to go around, which order to visit tiles) by choosing where the waypoints land, rather than the system always picking its own single path.
- Path cannot enter a **closed door** or illegal tiles; if blocked at resolve → movement fails at the block (demo: stop before the door — full Otherwise card library is post-demo).

### 3.2 Pick a Stance → Automatic Cost (amended 2026-08-03, C21 — supersedes the original time-allotment-slider model)
- Player picks **Sprint / Tactical Walk / Stealth Crawl directly** for the path. There is no manual time-allotment slider or step where the player pre-commits seconds — the player never chooses "how much time to allot"; they choose a stance, and cost follows.

| Stance | Effects (demo) |
|--------|----------------|
| **Sprint** | Fastest; loud; **cannot fire** while sprinting; **evasive** vs Snap Shot |
| **Tactical Walk** | Medium; gun up / ready; can Shoot; **not** evasive |
| **Stealth Crawl** | Slowest; silent; not evasive vs Hold Angle |

- Cost is computed automatically — `seconds = tiles * BaseSecondsPerTile * StanceMult` per leg (align with paper D5), summed across every leg of the waypoint path — and **deducted from the round's Time Resource the instant the Move is scheduled**. The HUD/scrubber shows the running used/budget total as it depletes; it never shows a pre-commit allotment value to confirm. Must stay readable on the **timeline scrubber**. Playback Duration is independent (**C27**).
- Shoot (§3A.2) already worked this way — pick Snap or Hold, cost follows automatically — so this brings Move in line with Shoot rather than introducing a second new pattern.

### 3.3 Collision
- Cannot share a tile. Forced enter occupied / closed door → movement blocked at that edge.

---

## 3A. How Shooting Works (UI & Rules)

There is **no Snap Shot / Hold Angle card**. Shooting is a base verb every Character has.

### 3A.1 Aim
- Declare a **shoot** action aimed at a **tile** (or LoS direction that resolves to a tile).
- Requires clear LoS at resolve time (**C32** Bresenham, same floor, doors block).

### 3A.2 Allot the Time → Mode

| Mode | Relative allotment | Effects (demo) |
|------|--------------------|-----------------|
| **Snap Shot** | Fast (minimal time) | Wounds on hit; **misses** targets in **Sprint** stance; hits only the **aimed tile** at completion (**C32**) |
| **Hold Angle** | Slow (aim-lock window) | Lethal on hit; **hits** targets in **Sprint** stance; covers the aimed lane for its window (Day 6 detail) |

Base Time Resource costs (Section 6) are placeholders; Agility scaling (**C25**) must stay readable on the scrubber.

---

## 4. Door (Map Action — Not a Gear Card)

For the 14-day ship, **one door** on the 5×5 board:

- Scheduled as a **contextual map action** from the pawn’s current/adjacent tile during Program (tap door → open/close at a booked Time Resource second).
- **Closed** blocks movement through that edge and **blocks LoS** across it.
- **Open** allows move + LoS.
- Strength modifies open/close Time Resource cost (Section 6).
- This is **not** the full Interact card / vent / monitor kit (**C34**).

---

## 5. Wound System & Combat Resolution

### Physical states
| State | Effect |
|-------|--------|
| **Healthy** | Normal. |
| **Wounded** | Visible wounded state; second wound (or Hold Angle lethal) → **Dead**. *(Full +1s surcharge and Bandage-by-next-round bleed are **post-demo** under C34 — demo still shows wound → death so combat stakes read.)* |
| **Dead** | Eliminated (Hold Angle lethality / second wound / mutual kill). |

### Shooting & LoS
- Same floor only; orthogonal Bresenham LoS; blocked by closed doors (**C32**).
- **Snap Shot:** completes → target on aimed tile **Wounded** if LoS; **misses Sprint**.
- **Hold Angle:** lethal on LoS hit; **hits Sprint**; duration as scheduled.
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
| Scout | 1.0 s / tile Walk baseline *(tune)* | Stance / Shoot-mode change **0s** | Door **4s** |
| Juggernaut | 2.0 s / tile Walk baseline *(tune)* | Stance leave-Sprint **+1s**; Snap↔Hold **+1s** | Door **2s** |

Placeholder magnitudes — tune in playtest. Legacy “tiles/tick” tables map to continuous seconds.

### Shoot modes (base verb)

| Mode | Base cost / duration | Effect |
|------|----------------------|--------|
| **Snap Shot** | **2s** | Wound on LoS to aimed tile; misses Sprint |
| **Hold Angle** | **3s** aim lock | Lethal on LoS; hits Sprint |

### Deferred cards (post-demo — do not implement for C34 ship)

| Card | Notes |
|------|-------|
| Bandage / Interact-as-card / Flashbang / Adrenaline | Confirmed long-term design; **out of 14-day ship** |

---

## 7. Map Elements

| Element | 14-day ship |
|---------|-------------|
| **One Door** | Contextual open/close; blocks move + LoS when closed |
| **5×5 ground** | Yes |
| Attic / Vent / Monitor | **Post-demo** |
| 高铁 / High-speed rail (**C31**) | Confirmed design; **post-demo** |

---

## 8. Presentation (ship requirement — C34)

The demo must read as a **Desk-Lamp Diorama**, not a default Unity prototype. Binding art/audio floor lives in [ART_DIRECTION.md](ART_DIRECTION.md) § Demo art floor. Summary:

- Board on a physical base in a dark void; warm desk-lamp lighting; clay-like materials.
- Yarn/chalk paths; cardstock Time Card; AR scrubber contrast.
- Stepped pawn motion; physical muzzle flash; clay wound splat.
- Tactile foley for move, shot, Time Card, Lock In.

Full SSS, thumbprint maps, and bespoke character rigs remain optional.

---

## 9. Out of Scope (14-day ship — C34)

- Photon Fusion online multiplayer (**C5** deferred).
- Full Android polish / dual-platform ship (**C6** → Windows polished; Android smoke optional).
- FoW, decoys, facing cones, numeric HP bars, armor.
- Gear cards: Bandage, Flashbang, Adrenaline, Interact-as-card.
- Otherwise library; attic; vent; monitor; loot; 高铁.
- Final Link’s Awakening–level clay shaders / complex mocap.

---

## 10. Acceptance (14-day ship)

1. A new player can complete **at least two Time Card rounds** locally (Allot → Program → Reveal → Execute → Aftermath → Allot).  
2. Path + stance and Snap / Hold produce understandable tactical consequences on the **Time Resource scrubber**.  
3. **One door** materially changes movement or LoS at least once.  
4. Without narration, an observer reads the scene as a **handmade desk-lamp miniature** — not stock Unity primitives.  
5. Move, Shoot, hit, Time Card, and Lock In each have **distinct** visual and/or audio feedback.  
6. **Windows build** runs reliably and is presentation-ready for a 60–90s capture.

Scout vs Juggernaut feel different; cause/effect order remains readable even when Playback Duration compresses Time Resource.
