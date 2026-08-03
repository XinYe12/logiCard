# 2-Week Demo Scope

**Doc ID:** D2  
**Status:** Updated 2026-08-03 — continuous-space pivot (**C35/C39**, see [CONTINUOUS_PIVOT_PLAN.md](CONTINUOUS_PIVOT_PLAN.md)); Updated 2026-07-30 — **C34 Polished Core Demo** (art + tight gameplay)  
**Goal:** Ship a **portfolio-ready local Windows** vertical slice: Desk-Lamp Diorama presentation + readable Time Card / Move / Shoot duel.

Companion: [VISION.md](VISION.md) · Authority for rules/numbers: [GDD.md](GDD.md) · Art floor: [ART_DIRECTION.md](ART_DIRECTION.md)

---

## IN (Must Have for Demo)

### The Diorama Micro-Map
- **Continuous ground arena** (`[0,4]×[0,4]` footprint — was a 5×5 grid, see **C35/C39**) on a physical base in a dark void.
- **One door** (wall segment, radius-based interact) that blocks move + LoS when closed (contextual open/close).

### Presentation (required — C34 / C29)
- Warm desk-lamp lighting, clay-like materials, yarn/chalk paths.
- Cardstock Time Card + tactile thumb-zone HUD; clean AR Time Resource scrubber.
- Stepped pawn motion; physical muzzle flash; clay wound splat.
- Basic tactile foley (footsteps, shot, Time Card, Lock In).
- Detail floor: [ART_DIRECTION.md](ART_DIRECTION.md) § Demo art floor.

### Multiplayer / platforms
- **1v1 local** (Attacker vs Defender labels + spawns — **C18**).
- **Windows** polished ship build.
- **Android:** optional smoke build only if time remains — not a polish target for Day 14 (**C34**).
- **Bots:** Nice-to-have (**C19**), not Must-Have.
- **Fusion online:** deferred (**C5** / **C34**).

### Core Game Loop
- **Allot (Time Card) → Program (30 real-world s) → Reveal → continuous Time Resource resolve → Playback → Aftermath** → repeat until Dead or pool empty (**C33** / **C4**).

### Visual timeline
- Continuous **Time Resource** scrubber (seconds); readable cause/effect. Not a 12-tick discrete clock.

### Tiny / Tactical programming (IN)
- **Character Card** pick (Scout / Juggernaut).
- **Time Card** allotment from shared **900s** match pool.
- **Multi-waypoint path** (tap to add each waypoint, continuous — **C21/C35**) + direct stance pick (Sprint / Tactical Walk / Stealth Crawl), automatic cost.
- **Free-aim point** (**C39**) + direct Shoot mode pick (Snap Shot / Hold Angle), automatic cost.
- Health states: Healthy / Wounded / Dead (simplified wound stakes — Bandage deferred).

### Win Condition
- Opponent reaches **Dead**. Mutual lethal same second → **Draw**.

---

## OUT (Do NOT Build for 14-day ship — C34)

- Photon Fusion online multiplayer.
- Full Android UI/polish / dual-platform feature parity.
- Attic floor, vent, monitor.
- Gear cards: Bandage, Interact-as-card, Flashbang, Adrenaline.
- Otherwise Invalid→Stop library.
- Final SSS / thumbprint maps / complex character rigs (optional if time; not required).
- Fog of war & decoys; sprawl maps; laser grids; alarms; hostages; extraction.
- Gear progression, loot, asymmetrical classes beyond Scout/Juggernaut attrs.
- Escalation/noise track; full 2–8 / 4v4; facing/turning radius; armor / HP bars.
- 高铁 / high-speed rail (**C31** — confirmed design, post-demo).

---

## LATER (Post-Demo Roadmap)

- Fusion Host 1v1 + Android cross-play polish (**C5** / **C6**).
- Attic + vent + monitor; Bandage / Otherwise / Flashbang / Adrenaline.
- Full clay SSS, thumbprints, bespoke models.
- Hidden movement, decoys, Defense bluffing.
- 3-Act heist structure; extraction/loot; asymmetrical classes; 4v4.
- Richer Otherwise library.
- **High-speed rail / 高铁 (**C31**).**
- Destructible geometry via discrete breach states (**C36**).
- Asymmetric objective win condition (vault/cashbox-style), restoring the Asymmetric Heist pillar (**C37**).
- Downed state + tile-targeted revive + Detonator martyr archetype (**C38**).

---

## Clarifications (still in force)

1. Attack/Defend = labels + spawns (**C18**).  
2. Bots = nice to have (**C19**).  
3. Personal timelines + shared continuous **Time Resource** clock; **Playback Duration** separate (**C27**).  
4. **C34** supersedes older “primitives OK / full GDD Section 9” ship language for the 14-day artifact.
5. **C35/C39 (2026-08-03):** the board is continuous, not a grid — reverses C35's original "long-term only" framing. Art/polish pass (Days 8–14) is now planned **compressed** to absorb the pivot's cost, per the schedule-handling call in `CONTINUOUS_PIVOT_PLAN.md`.
