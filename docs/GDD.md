# D4: Game Design Document (v0.1) — 2-Week Demo

**Doc ID:** D4  
**Status:** Revised 2026-07-29 — Shoot promoted from card to base verb (mirrors Move)  
**Depends on:** [VISION.md](VISION.md), [SCOPE.md](SCOPE.md), [CORE_LOOP.md](CORE_LOOP.md)

This document defines rules, numeric tuning, and content for the 14-day prototype. Focus: **Timeline Programming** math, continuous **Time Resource** sync, **path/stance movement**, and tactical **cornering**.

**Core model (supersedes “Walk/Dash as cards”, then “Snap/Hold Angle as cards”):**
1. **Character Card** sets base attributes (Speed / Agility / Strength).  
2. **Movement** = draw path + allot time → stance (Sprint / Tactical Walk / Stealth Crawl). Base verb, not a card.
3. **Shoot** = aim (target tile / LoS direction) + allot time → mode (Snap Shot / Hold Angle). Base verb, not a card — same attribute-driven pattern as Movement.
4. **Deck cards** = remaining Gear / Special Tactics dropped onto the path/timeline: Bandage, Interact, Flashbang, Adrenaline.

---

## 1. Structure & Match Flow

- **Player Count:** 1v1 (Attacker vs Defender) — spawn labels; same *rules*; Character Cards may differ (Section 2).
- **Win Condition:** Opponent Physical State → **Dead**.
- **Map:** Two stacked **5×5** grids (Ground + Attic). Demo distances use **tiles** (1 tile ≈ abstract meter for Speed math).
- **Match Loop:**
  1. **Select Character Card** (pre-match).
  2. **Program Phase (30s):** Draw path(s), allot time (stance), drop tactic cards on the timeline/path, lock.
  3. **Reveal Phase:** Paths + cards face-up.
  4. **Execution Phase:** Host resolves continuous **Time Resource** timeline; clients play **ReplayTape** using separate **Playback Duration** (may be compressed). See glossary in [PRODUCT_MEMORY.md](PRODUCT_MEMORY.md) (**C27**).
  5. Repeat until Dead.

---

## 2. The Character Card (Base Attributes)

Before the match, each player selects a **Character Card (Loadout)**. Attributes convert the shared time budget into map action.

| Attribute | Meaning | Demo examples |
|-----------|---------|----------------|
| **Speed** | How far they move per unit time allotted | Scout: **1.5 tiles / tick** · Juggernaut: **0.75 tiles / tick** *(demo-tuned; fantasy “m/s” maps to tiles/tick)* |
| **Agility (Handling)** | Transition cost between states (raise gun after sprint, vault, etc.) — also scales **Shoot mode** transition (Snap ↔ Hold Angle) since both are "gun handling" | Scout: **fast** transitions · Juggernaut: **slow** *(demo: flat +0 / +1 tick penalties on stance-change and shoot-mode-change — table in Section 6; exact Shoot-mode scaling still OPEN, ticks below are placeholders)* |
| **Strength** | Physical interaction time | Kick locked door: Scout **slow/pry** · Juggernaut **fast/kick** *(demo: Interact Door cost modified by Strength — Section 6)* |

**Demo cast (IN):** two presets both sides may pick — **Scout** and **Juggernaut** (same Move/Shoot base verbs + same deck of remaining cards; different attributes only).

---

## 3. How Movement Works (UI & Rules)

There is **no Walk card**. Movement is built-in programming:

### 3.1 Draw the Path
- Select your character → draw a **waypoint path** through legal tiles to a destination (orthogonal grid; no diagonal).
- Path cannot enter closed doors or illegal tiles; if blocked at resolve → **Invalid** (Otherwise).

### 3.2 Allot the Time
- Game computes path length (tiles).
- Using **Speed**, UI offers a **time-allotment slider** for that path segment. Allotted time vs distance forces a **Stance**:

| Stance | Relative allotment | Effects (demo) |
|--------|--------------------|----------------|
| **Sprint** | Fastest (minimal time for distance) | Loud; **cannot fire** while sprinting; treated as **Evasive** vs Snap Shot (like old Dash) |
| **Tactical Walk** | Medium | Gun up / ready; can combine with ready-fire tactics; **not** evasive |
| **Stealth Crawl** | Slowest (max time sink) | Silent; **immune to motion-sensor style reveals** (Monitor still highlights if used — FoW Out anyway); not evasive vs lethal Hold Angle |

Exact slider breakpoints derive from Time Resource: `seconds = tiles * BaseSpeed * StanceMult` (align with paper D5). Must be readable on the **timeline scrubber**. Playback Duration is independent (**C27**).

### 3.3 Collision
- Cannot share a tile. Forced enter occupied / closed door → movement **Invalid**.

---

## 3A. How Shooting Works (UI & Rules)

There is **no Snap Shot / Hold Angle card**. Like Movement, Shooting is built-in programming — a base verb every Character has, driven by attributes, not something drawn or held.

### 3A.1 Aim
- Select a scheduled moment (a path waypoint, or a stationary point) → declare a **shoot** action aimed down a tile/LoS direction.
- Requires LoS at resolve time; no LoS at the scheduled tick → **Invalid** (Otherwise).

### 3A.2 Allot the Time → Mode
- Time allotted for the shoot action forces a **Mode**, mirroring Stance:

| Mode | Relative allotment | Effects (demo) |
|------|--------------------|-----------------|
| **Snap Shot** | Fast (minimal time) | Wounds on hit; **misses** targets in **Sprint** stance |
| **Hold Angle** | Slow (aim-lock window) | Lethal on hit; **hits** targets in **Sprint** stance |

Base Time Resource costs (Section 6) are placeholders; exact Agility scaling still **OPEN** — must stay readable on the timeline scrubber, same as Stance bands.

---

## 4. What Cards Are For Now

Cards are the remaining **Gear and Special Tactics** — everything that isn't the two base verbs (Move, Shoot). Drop them onto the **movement path / timeline** at chosen clock times (or path waypoints).

Examples:
- **Flashbang** at a breach moment before a door.
- **Bandage** — force stop for scheduled duration to clear bleed.
- **Interact** — open/close a door, use the vent, use the monitor.

Movement and Shoot are **not** cards; cards modify / interrupt / arm the path around them.

**Otherwise (demo):** `If Invalid → Stop` — remaining time on the failed segment/card becomes wait; continue next segment/card.

---

## 5. Wound System & Combat Resolution

### Physical states
| State | Effect |
|-------|--------|
| **Healthy** | Normal. |
| **Wounded** | All scheduled **card** costs and **movement/shoot allotments** pay **+1 tick** surcharge (efficiency crippled). Must **Bandage** by end of **next** round or **bleed out → Dead**. |
| **Dead** | Eliminated (Hold Angle lethality / bleed-out / mutual kill). |

### Shooting & LoS
- Same floor only; orthogonal LoS; blocked by closed doors.
- **Snap Shot** (Shoot mode, Section 3A): completes → target **Wounded**; **misses** targets in **Sprint** stance during that tick.
- **Hold Angle** (Shoot mode, Section 3A): lethal; **hits Sprint**; duration as scheduled on timeline.
- **Mutual lethal** same tick → **Draw**.

### Instant
- **Adrenaline (1/match):** during Execution, −1 tick on **currently active** segment/card.

---

## 6. Main Numeric Setup (Demo)

| Attribute | Value | Notes |
|-----------|-------|-------|
| **Time Resource window (demo round)** | 60 seconds (placeholder) | Continuous budget this Program→Execute; ~15-min match = OPEN |
| **Program Timer** | 30 **real-world** seconds | Wall-clock planning (**C27**) |
| **Playback Duration** | Tunable | Cinema length ≠ Time Resource; do not force long TR = long wall animation |
| **Max cards / round** | 3 | Gear only — Move/Shoot not counted |
| **Instant uses** | 1 / match | Adrenaline |
| **Otherwise** | Invalid → Stop | Demo default |

**Removed:** discrete 12-tick Master Clock @ 1.5s/tick (**C28**).

### Character presets (demo)

| Preset | Speed (tiles/tick) | Agility | Strength (door Interact base) |
|--------|--------------------|---------|--------------------------------|
| Scout | 1.5 | Stance change penalty **0**; Shoot-mode change penalty **0** | Door Interact **4 ticks** |
| Juggernaut | 0.75 | Stance change penalty **+1 tick** once when leaving Sprint; Shoot-mode change penalty **+1 tick** once when switching Snap ↔ Hold Angle | Door Interact **2 ticks** |

Shoot-mode penalty mirrors the stance penalty (same Agility, same shape) — placeholder magnitude, tune together during playtesting.

### Shoot modes (base verb, not a card — Section 3A)

| Mode | Base cost / duration | Effect |
|------|----------------------|--------|
| **Snap Shot** | 2 ticks | Wound on LoS; misses Sprint |
| **Hold Angle** | 3 ticks (aim lock window) | Lethal on LoS; hits Sprint |

### Cards (remaining shared deck — gear/gadgets)

| Card | Base cost / duration | Effect |
|------|----------------------|--------|
| **Bandage** | 3 ticks (must be stationary) | Clear Wounded |
| **Interact** | 2 ticks base (doors modified by Strength) | Door / Vent / Monitor (current or adjacent tile) |
| **Throw Flashbang** | 3 ticks; **1/match** | Target room; Stun = +3 ticks to target’s **active** segment/card |
| **Adrenaline** | Instant; **1/match** | −1 tick active segment/card |

**Removed as cards:** Tactical Walk / Dash — replaced by **path + stance** (Section 3). Snap Shot / Hold Angle — replaced by **Shoot verb** (aim + time → mode, Section 3A), same pattern as Move.

---

## 7. Map Elements (Demo)

| Element | Behavior |
|---------|----------|
| **Doors** | Interact open/close; closed blocks move + LoS; Strength affects Interact time |
| **Vent (×1)** | Interact → teleport mirrored other floor |
| **Monitor (×1)** | Interact → highlight opponent for rest of round |

### Later map (confirmed design — C31)

| Element | Behavior |
|---------|----------|
| **高铁 / High-speed rail (×1 track, side of map)** | Board and ride as transport; rider **may Shoot while riding**; **car is bulletproof**; **1 use per match**. Numerics OPEN. Not required for Slice 1–3. |

---

## 8. Out of Scope (v0.1)

- FoW (both always visible).
- Numeric HP bars / armor / facing cone (360° vision).
- Free continuous meters off-grid (demo stays **tile grid** with Speed as tiles/tick).
- Full 15-minute real-time single budget across whole match (demo uses **per-round 12-tick** clock; fantasy “15-minute budget” = product vision for Later).
- Gear progression, loot, class kits beyond Scout/Juggernaut attrs, hostages, extraction.
- Final clay art (primitives OK).

---

## 9. Acceptance

1. Pick Character → Program path+stance (Move) + aim+mode (Shoot) + ≤3 cards in 30 **real-world** s → Reveal → continuous Time Resource resolve + Playback.  
2. Stance bands readable; affect Snap / noise rules.  
3. Scout vs Juggernaut feel different.  
4. Wound / Bandage / Hold Angle / Snap / Flashbang / Adrenaline / Otherwise Stop work.  
5. Doors / Vent / Monitor work.  
6. Cause/effect readable on **Time Resource timeline** (Playback may be faster).
