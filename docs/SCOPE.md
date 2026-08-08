# Product Scope — Steam F2P PvP Ship

**Doc ID:** D2  
**Status:** Updated 2026-08-08 — **C46 full scope pivot** (supersedes the 14-day-demo framing; see `PRODUCT_MEMORY.md` C46–C51). Prior: 2026-08-03 continuous-space pivot (**C35/C39**, see [CONTINUOUS_PIVOT_PLAN.md](CONTINUOUS_PIVOT_PLAN.md)); 2026-07-30 **C34 Polished Core Demo** (superseded).  
**Goal:** Ship a **free-to-play PvP** game to **Steam** (landscape desktop): Desk-Lamp Diorama presentation, raised to a commercial art bar, wrapped around the same Time Card / Move / Shoot duel — see "What must not change" in `PRODUCT_MEMORY.md` C46.

Companion: [VISION.md](VISION.md) · Authority for rules/numbers: [GDD.md](GDD.md) · Art floor: [ART_DIRECTION.md](ART_DIRECTION.md)

---

## IN (Must Have for Ship)

### The Diorama Map
- **Continuous, multi-room ground arena** (`[0,8]×[0,10]` footprint, Yard/Hall/Vault + flank corridors — **C45**) on a physical base in a dark void.
- **Two doors** (wall segments, radius-based interact) that block move + LoS when closed (contextual open/close).

### Presentation (required — commercial ship bar, C46/C29)
- Warm desk-lamp lighting, clay-like materials, **线稿涂鸦** paths (FragPunk-style ink on board — see ART_DIRECTION).
- Cardstock Time Card + clean AR Time Resource scrubber, now laid out landscape desktop-first (**C48**).
- Stepped pawn motion; physical muzzle flash; clay wound splat.
- Basic tactile foley (footsteps, shot, Time Card, Lock In).
- Detail bar: [ART_DIRECTION.md](ART_DIRECTION.md) § Commercial ship art bar.

### Multiplayer / platforms
- **1v1 real online PvP** (Attacker vs Defender labels + spawns — **C18**), landscape desktop, Steam.
- **Windows via Steam** ship build.
- **Android:** separate future consideration, not part of this pivot's scope (**C6**/**C48**).
- **Matchmaking-fallback bot:** required, invisible substitute when no PvP opponent is available — not
  optional, not a marketed mode (**C49**). Detail: [AI_FALLBACK_BOT.md](AI_FALLBACK_BOT.md).
- **Real networking (Fusion or confirmed alternative):** in scope, biggest open build item — see
  [NETWORKING_DESIGN.md](NETWORKING_DESIGN.md) (**C51**).
- **Free-to-play, cosmetic-only IAP, no pay-to-win:** see [MONETIZATION.md](MONETIZATION.md) (**C47**).

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

## OUT (Do NOT Build for this ship — C46)

- Full Android UI/polish / dual-platform feature parity (portrait UI is a separate future consideration, **C48**).
- Attic floor, vent, monitor.
- Gear cards: Bandage, Interact-as-card, Flashbang, Adrenaline.
- Otherwise Invalid→Stop library.
- Final SSS / thumbprint maps / complex character rigs (optional if time; not required).
- Fog of war & decoys; sprawl maps; laser grids; alarms; hostages; extraction.
- Gear progression, loot, asymmetrical classes beyond Scout/Juggernaut attrs.
- Escalation/noise track; full 2–8 / 4v4; facing/turning radius; armor / HP bars.
- 高铁 / high-speed rail (**C31** — confirmed design, post-demo).

---

## LATER (Future Roadmap — phase-sequenced, see SCHEDULE.md)

- Android cross-play polish (**C6**/**C48**, separate future consideration).
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
2. Bots = required matchmaking fallback, not a marketed mode (**C49**, amends **C19**'s "nice to have").  
3. Personal timelines + shared continuous **Time Resource** clock; **Playback Duration** separate (**C27**).  
4. **C46 (2026-08-08)** supersedes **C34**'s "primitives OK / 14-day artifact" ship language entirely — this
   is a commercial ship, not a portfolio demo. Art/polish is now in scope (Phase 5, `SCHEDULE.md`), not an
   optional stretch goal.
5. **C35/C39 (2026-08-03, historical):** the board is continuous, not a grid — this was itself once a
   "long-term only" item that got promoted mid-build; already shipped, see `CONTINUOUS_PIVOT_PLAN.md`.
6. **C48 (2026-08-08):** UI is landscape desktop-first for Steam, superseding **C30**'s portrait-lock. Portrait
   remains documented (`PRODUCT_MEMORY.md` C30) as a possible future mobile-port direction, not deleted.
