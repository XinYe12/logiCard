# Vision One-Pager: Untitled Tactical Heist Game

**Doc ID:** D1  
**Status:** Updated 2026-07-27 (stakeholder rewrite)  
**Next:** D2 Scope (companion) · then D3 Core loop  

Working title in repo: **logiCard** (may change).

---

## What the Game Is

A tactical, **2.5D asymmetric multiplayer heist** game driven by a unique **Timeline Programming** system. Teams secretly allocate a shared **Time Resource** budget to schedule operations on a timeline, which then resolves simultaneously; clients watch a cinematic **Playback** (duration may differ from Time Resource). It combines the spatial mind games of tabletop stealth with the synchronized, cinematic breach mechanics of *Door Kickers*.

**Core verb (product memory C15):** **Cards** are how the player authors/builds the timeline; the **timeline** is the main gameplay surface and the system’s execution logic.

## Who It’s For

Fans of tactical extraction shooters (*Escape from Tarkov*, *Rainbow Six Siege*) and heavy strategy games who want deep, psychological PvP mind games without twitch-reflex requirements or heavy math.

## Players & Platforms

- **Players:** 2–8 players online (supporting 1v1 up to 4v4 team play).
- **Platforms:** Cross-platform on **Windows** and **Android**.

## Art Direction

A tactile, tilt-shift **claymation/diorama** style heavily inspired by the *Link's Awakening* Switch remake. The map feels like a physical miniature dollhouse on a table, and characters look like tangible tabletop plastic/clay miniatures. This communicates that this is a **board-driven tactical game** while staying readable and striking.

*(Demo uses primitives — see Scope. Final clay shaders are Later.)*

## Core Direction & Hook

- **Asymmetric Heist:** Offense vs. Defense on a single, highly detailed **fixed** map featuring a **ground floor and an attic**.
- **The Map as a Weapon:** The environment is a primary mechanic, not just a backdrop. Complex layouts feature interactive elements (vents for vertical flanking, monitor rooms for intel, choke points players can manipulate).
- **The "Secret Sauce" (Otherwise System):** When a planned timeline operation fails (e.g., the target dies or moves), spent time flows into a backup **"Otherwise" condition** (e.g., `If Target Invalid → Overwatch`), eliminating wasted turns and keeping action cinematic.

## Success Metric

**"A friend can play 15 minutes without me."**

The **Time Resource** timeline UI must be visual and intuitive enough that a new player instantly understands cause and effect (e.g., "My 2-second grenade goes off before your 4-second sniper shot") — even when **Playback Duration** compresses the cinema.

## Non-Goals

- Procedurally generated maps (want tightly crafted, learnable puzzle-box spaces).
- Real-time twitch shooting (combat is completely **deterministic** based on the timeline).
- Dice-based RNG combat.
- Deep single-player PvE campaigns.

## Confirmed pillars (product memory)

| ID | Lock |
|----|------|
| C13 | Lightweight **2.5D** + clear 2D timeline/HUD |
| C15 | **Cards → timeline**; timeline = gameplay + Master Clock execution |
| C16 | **Attack vs Defend PvP**; bots fill for testing |

## Long-Term Systems (Post-Demo)

Confirmed direction for the full game, none of it required or built for the 14-day demo (see `PRODUCT_MEMORY.md` C35–C38 and C42–C44 for detail):

- **Continuous movement** over a navmesh, replacing the tile grid (**C35**).
- **Destructible geometry** as discrete breach states — the map as a weapon, made literal (**C36**).
- **Asymmetric objective win condition** (vault/cashbox-style) — the actual long-term realization of the "Asymmetric Heist" pillar above, once the demo's elimination-only win ships (**C37**).
- **Revive**, via a Downed state and a tile-targeted revive action, plus a "Detonator" martyr archetype that turns a finished-off ally into a tactical weapon (**C38**).
- **Unique-verb character roster** — beyond Scout/Juggernaut's shared-verb, different-attributes model, characters carrying an ability no other character has (**C42**), starting with **Bomber** (floor breach + fall-through-floor, **C43**) and **Time Player** (rewind/fast-forward an object's state, **C44**). Detail: [CHARACTER_ROSTER_LONGTERM.md](CHARACTER_ROSTER_LONGTERM.md).

## Out of this doc

Exact numbers, full card lists, FoW rules, net architecture — GDD / TDD / Scope / Core loop.
