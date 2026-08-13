# Vision One-Pager: Untitled Tactical Heist Game

**Doc ID:** D1  
**Status:** Updated 2026-08-08 (**C46 — full scope pivot**: monetizing PvP game shipping to Steam, supersedes the 14-day portfolio-demo framing; core loop unchanged. See `PRODUCT_MEMORY.md` C46–C51.) Prior: 2026-07-27 stakeholder rewrite.
**Next:** D2 Scope (companion) · then D3 Core loop  

Working title in repo: **logiCard** (may change).

---

## What the Game Is

A tactical, **2.5D asymmetric multiplayer heist** game driven by a unique **Timeline Programming** system. Teams secretly allocate a shared **Time Resource** budget to schedule operations on a timeline, which then resolves simultaneously; clients watch a cinematic **Playback** (duration may differ from Time Resource). It combines the spatial mind games of tabletop stealth with the synchronized, cinematic breach mechanics of *Door Kickers*.

**Core verb (product memory C15):** **Cards** are how the player authors/builds the timeline; the **timeline** is the main gameplay surface and the system’s execution logic.

## Who It’s For

Fans of tactical extraction shooters (*Escape from Tarkov*, *Rainbow Six Siege*) and heavy strategy games who want deep, psychological PvP mind games without twitch-reflex requirements or heavy math.

## Players & Platforms

- **Players:** 2–8 players online (supporting 1v1 up to 4v4 team play). Current build is 1v1 (**C2**).
- **Platforms:** **Windows via Steam**, landscape desktop, is the active ship target (**C48**). Android/mobile
  is a separate future consideration, not part of this pivot's scope (**C6**/**C48**).

## Business Model

**Free-to-play, cosmetic-only in-app purchases, no pay-to-win** (**C47**). Nothing purchasable may affect
`HitRadius`/`LaneHalfWidth`/movement speed/costs, or reduce a pawn's board visibility/contrast below the
floor `ART_DIRECTION.md` sets — the core loop's fairness depends on both being read correctly by both
players. Detail, guardrails, and open economy numerics: [MONETIZATION.md](MONETIZATION.md).

## Art Direction

*[Superseded by **C53**, 2026-08-09 — the toy/chibi framing below no longer binds; see `ART_DIRECTION.md`'s
"Digital Claymation" section for the current target. Kept here as historical record of the original demo-era
direction, not deleted.]*

~~A tactile, tilt-shift **claymation/diorama** style heavily inspired by the *Link's Awakening* Switch remake. The map feels like a physical miniature dollhouse on a table, and characters look like tangible tabletop plastic/clay miniatures. This communicates that this is a **board-driven tactical game** while staying readable and striking.~~

**Current direction (C53):** the board stays a bounded, physically-edged chunk floating in a dark void — that
structural shape is unchanged — but the fidelity target moves from toy-chibi to a richly detailed, grounded
look (real architecture/material language, a contained sky/cloud/weather system directly above the board, not
an infinite horizon), against a locked visual reference. Detail: `PRODUCT_MEMORY.md` C53, `ART_DIRECTION.md`.

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
- Deep single-player PvE campaigns. *(An invisible matchmaking-fallback bot, **C49**, is not this — see
  [AI_FALLBACK_BOT.md](AI_FALLBACK_BOT.md). It substitutes for a missing PvP opponent; it is not a marketed
  mode, tutorial, or content pipeline of its own.)*

## Confirmed pillars (product memory)

| ID | Lock |
|----|------|
| C13 | Lightweight **2.5D** + clear 2D timeline/HUD |
| C15 | **Cards → timeline**; timeline = gameplay + Master Clock execution |
| C16 | **Attack vs Defend PvP**; bots are a matchmaking fallback only, never a marketed mode (**C49**) |

## Long-Term Systems (Future Roadmap)

Confirmed direction for the full game. None of it is promoted into active build scope by the pivot itself
(**C46**) — sequenced by the phase model in `SCHEDULE.md`, see `PRODUCT_MEMORY.md` C36–C38/C42–C44 for detail.
(**C35**, continuous movement replacing the tile grid, is **not** in this list — it was promoted to current
scope and shipped back on 2026-08-03; see `CONTINUOUS_PIVOT_PLAN.md`. The board these systems would build on
top of already exists.)

- **Destructible geometry** as discrete breach states — the map as a weapon, made literal (**C36**).
- **Asymmetric objective win condition** (vault/cashbox-style) — the actual long-term realization of the "Asymmetric Heist" pillar above, once the demo's elimination-only win ships (**C37**).
- **Revive**, via a Downed state and a tile-targeted revive action, plus a "Detonator" martyr archetype that turns a finished-off ally into a tactical weapon (**C38**).
- **Unique-verb character roster** — beyond Scout/Juggernaut's shared-verb, different-attributes model, characters carrying an ability no other character has (**C42**), starting with **Bomber** (floor breach + fall-through-floor, **C43**) and **Time Player** (rewind/fast-forward an object's state, **C44**). Detail: [CHARACTER_ROSTER_LONGTERM.md](../character/CHARACTER_ROSTER_LONGTERM.md).

## Out of this doc

Exact numbers, full card lists, FoW rules, net architecture — GDD / TDD / Scope / Core loop. Monetization
economy — `MONETIZATION.md`. Networking/matchmaking design — `NETWORKING_DESIGN.md`. UI layout — `UI_FLOW.md`.
