# logiCard — README case-study draft (Day 14)

**Status:** DRAFT — ready for Integrator merge as writing; promote to root `README.md` only after human capture + Windows candidate exist.  
**Sources:** `docs/VERTICAL_SLICE.md`, `docs/ART_DIRECTION.md`, `docs/SCHEDULE.md`, `docs/SCOPE.md`, C34 Polished Core Demo.  
**Last polished:** 2026-08-07 (`feat/ship-docs`)

---

## One-liner

**logiCard** is a portrait, one-handed tactics prototype: program Move / Shoot / Door against a shared **Time Resource** budget, then watch a deterministic ReplayTape play back on a desk-lamp diorama board.

## Problem / pitch

Most tactics games let you react mid-fight. logiCard locks the plan first: both sides spend a **Time Card** allotment from a shared match pool, draft path / stance / aim / door in secret, then resolve simultaneously on continuous Time Resource. The 14-day demo proves that loop is readable without network — local 1v1 (or a scripted second seat) on Windows — and that a cold observer can tell *scheduled Move* from *scheduled Shoot* on the scrubber with no narration.

## What this vertical slice ships (C34)

Fill checkboxes as Days 10–14 close; leave unchecked until the Windows candidate actually demonstrates them.

- [ ] Time Card → Program → Lock In → resolve → Playback → Aftermath → next round  
- [ ] Continuous arena (C35/C39): path waypoints, Snap / Hold Angle, one door that changes move or LoS  
- [ ] Desk-Lamp Diorama floor: warm URP lighting, painted board, 线稿涂鸦 ink paths, cardstock Time Card, AR scrubber  
- [ ] Stepped clay playback + physical muzzle flash + wound splat  
- [ ] Distinct tactile foley stubs (footstep / shot / Time Card / Lock In)  
- [ ] Windows playable build  

**Explicitly out (C34):** Fusion online, full Android polish, gear cards, Bandage/Otherwise, vent/monitor/Flashbang/Adrenaline, attic, loot, high-speed rail / 高铁.

## Case study

### Hook (10 seconds)

A warm lamp over a small board in a dark void. Clay pawns. A thin ink path on the floor. Someone plays a Time Card in the thumb zone — paper, not a HUD chip — and the match clock commits. That should already feel like a handmade miniature game, not a default Unity scene.

### Constraint

Fourteen implementation days, Windows-only ship bar, portrait one-handed layout. Mid-schedule the board left the grid: continuous space, free-aim Shoot, visibility-graph pathfinding (C35/C39). The art/polish pass was compressed on purpose so Day 14 did not move. Cut order under pressure: Android smoke and optional DoF/SSS first — never the Time Card loop, Move/Shoot readability, warm diorama composition, 线稿涂鸦 path, physical shot feedback, or Windows stability.

### Core loop

1. **Allot** — one side plays a Time Card: commit **N** seconds from the shared pool (demo 900s; MinRound 30s). **N** is spent in full.  
2. **Program** — both sides draft simultaneously inside **N**: tap waypoints (ink grows), pick Sprint/Walk/Crawl, free-aim Snap or Hold Angle, optional door open/close. Costs deduct automatically.  
3. **Lock In** — physical-switch moment; plans freeze.  
4. **Resolve / Playback** — Host ghost-sim writes a ReplayTape; the scrubber walks Time Resource (Playback Duration may compress).  
5. **Aftermath** — wounds and positions carry; next Time Card or match over (Dead / pool cannot fund MinRound).

Time Card is the match metronome, not a gear card. Move and Shoot are base verbs.

### Readable combat

The Slice 1 bar still owns Day 14: a first-time observer on the scrubber must say “that Move made them move” and “that Shoot made them shoot.” Core combat adds stance bands, Snap vs Hold Angle, Healthy → Wounded → Dead, and **one door** that changes a fight once (blocks move or LoS when closed). Pawns may share space; wounds come only from Shoot radii — walls and closed doors are the blockers.

### Presentation bets

Theme: **Desk-Lamp Diorama** — digital clay under a warm key light (`docs/ART_DIRECTION.md`). Paths are thin, slightly wobbly hand-drawn ink (FragPunk / 界外狂潮-style 线稿涂鸦), not neon. HUD Time Card reads as cardstock; the Time Resource scrubber stays AR-clean against the messy board. Playback motion is stepped (stop-motion feel). Muzzle flash is a short physical mesh pop; hits leave a clay wound splat. Foley aims for miniature tactility (clay footstep, cap-gun shot, paper card, Lock In snap) even when still stubbed.

### What you’d do next (post-demo)

Fusion Host + Win/Android cross-play; gear cards and map toys deferred under C34; long-term objective / Downed-revive / discrete breach (C36–C38); high-speed rail (C31) as a once-per-match ride — confirmed design, not this ship.

## Architecture (short)

| Layer | Role |
|-------|------|
| `Sim/` | Engine-free geometry, pathfinding, costs |
| `Net/` | GhostResolver + ReplayTape (deterministic resolve) |
| `Timeline/` | PawnProgram authoring against Time Resource |
| `Board/` + `Boot/` | Views + composition root / RoundPlayback |
| `UI/` | Portrait thumb-zone HUD |

Host-authoritative ghost sim; presentation is scrubber-driven (Time Resource), not wall-clock AI. Engine version: Unity **6000.5.5f1**.

## How to run (draft — verify on Day 12 candidate)

1. Install Unity **6000.5.5f1**; open the project folder (Hub: add parent `…/Game` if the project does not appear).  
2. Enter Play from the Bootstrap composition root (`GameBootstrap` builds the slice scene in Play Mode — confirm the entry scene name on the Day 12 candidate).  
3. Use a **portrait / tall** game view (C30).  
4. Play a Time Card → draft Move / Shoot / Door in the thumb zone → Lock In → scrub or autoplay Playback → Aftermath.

## Capture

See [`docs/CAPTURE_CHECKLIST.md`](CAPTURE_CHECKLIST.md) for the 60–90s shot list and stills. Embed final video link + 3–5 still paths here before promoting this file to root `README.md`.

**Video:** _(pending)_  
**Stills:** _(pending)_

## Credits / links

- Design: `docs/GDD.md`, `docs/CORE_LOOP.md`, `docs/PRODUCT_MEMORY.md`  
- Slice + art floor: `docs/VERTICAL_SLICE.md`, `docs/ART_DIRECTION.md`  
- Schedule / scope: `docs/SCHEDULE.md`, `docs/SCOPE.md`  
- Parallel multi-agent ops: `docs/PARALLEL_OPS.md`
