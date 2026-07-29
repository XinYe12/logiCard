# 2-Week Demo Scope

**Doc ID:** D2  
**Status:** Updated 2026-07-27 — aligned to GDD v0.1 revision (wound/RPS kit)  
**Goal:** Prove **timeline math**, **Time Resource scrubber UI**, **cornering/RPS**, and **2.5D map interaction**.

Companion: [VISION.md](VISION.md) · Authority for rules/numbers: [GDD.md](GDD.md)

---

## IN (Must Have for Demo)

### The Diorama Micro-Map
- **5×5 ground** + **5×5 attic** stacked grids.

### Interactive Map Elements
- **Doors:** open/close via Interact; block move + LoS when closed.
- **1 Vent:** Interact → teleport to mirrored other-floor tile.
- **1 Monitor Terminal:** Interact → highlight opponent for rest of round (FoW still Out).

### Multiplayer
- **1v1 online:** Attacker vs Defender.
- **Attack/Defend:** Labels + spawn sides only — same kit (**C18**).
- **Bots:** Nice-to-have (**C19**), not Must-Have.

### Core Game Loop
- **Program (30 real-world seconds) → Reveal → continuous Time Resource resolve → Playback** (Playback Duration tunable) → repeat until Dead.

### Visual timeline
- Continuous **Time Resource** scrubber (seconds); readable cause/effect. Not a 12-tick discrete clock.

### Tiny / Tactical programming (IN)
- **Character Card** pick (Scout / Juggernaut).
- **Path + time allotment** → stance (Sprint / Tactical Walk / Stealth Crawl). **No Walk/Dash cards.**
- **Aim + time allotment** → Shoot mode (Snap Shot / Hold Angle). **No Snap/Hold cards** — base verb, same pattern as movement.
- **Cards** (max 3/round) dropped on path: Bandage, Interact, Flashbang (1/match), Adrenaline (1/match instant).
- **Otherwise:** Invalid → Stop.
- Health states: Healthy / Wounded / Dead.

### Visual Master Clock
- *(Superseded)* Use continuous **Time Resource** timeline scrubber — see above. **C28**.

### Win Condition
- Opponent reaches **Dead** (headshot / bled out). Mutual lethal same tick → **Draw**.

---

## OUT (Do NOT Build for Demo)

- Final clay art/shaders/rigging/complex animation (primitives OK).
- Fog of war & decoys (both fully visible).
- Sprawl maps; laser grids; alarms; hostages; extraction.
- Gear progression, loot, asymmetrical classes.
- Escalation/noise track.
- Full 2–8 / 4v4.
- Facing/turning radius (360° vision).
- Armor / multi-hit numeric HP bars.

---

## LATER (Post-Demo Roadmap)

- Polished clay/diorama presentation.
- Hidden movement, decoys, Defense bluffing.
- 3-Act heist structure; extraction/loot.
- Asymmetrical classes.
- Full 8-player / 4v4.
- Onion map; acoustic listen zones.
- Richer Otherwise library.
- **High-speed rail / 高铁 (**C31**):** side track; board + ride; Shoot while riding; bulletproof car; **1× per match**. Confirmed design — ship only after Slice Move/Shoot/Clock/doors are green.

---

## Clarifications (still in force)

1. Attack/Defend = labels + spawns (**C18**).  
2. Bots = nice to have (**C19**).  
3. Personal timelines + shared continuous **Time Resource** clock; **Playback Duration** separate (**C27**).
