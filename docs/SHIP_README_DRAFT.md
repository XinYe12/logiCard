# logiCard — README draft

**Status:** DRAFT — ready for Integrator merge as writing; promote to root `README.md` only after real capture
+ a Steam-track build exist. Reframed 2026-08-08 (**C46**) from a portfolio case-study draft into a real
product README — this is no longer a 14-day demo write-up.
**Sources:** `docs/VERTICAL_SLICE.md` (historical), `docs/ART_DIRECTION.md`, `docs/SCHEDULE.md`, `docs/SCOPE.md`,
`docs/MONETIZATION.md`, `docs/NETWORKING_DESIGN.md`, `docs/PRODUCT_MEMORY.md` C46–C51.
**Last polished:** 2026-08-08.

---

## One-liner

**logiCard** is a landscape-desktop tactics PvP game: program Move / Shoot / Door against a shared **Time
Resource** budget, then watch a deterministic ReplayTape play back on a desk-lamp diorama board. Free-to-play,
cosmetic-only IAP.

## Problem / pitch

Most tactics games let you react mid-fight. logiCard locks the plan first: both sides spend a **Time Card**
allotment from a shared match pool, draft path / stance / aim / door in secret, then resolve simultaneously on
continuous Time Resource. The core loop is proven and shipped — 1v1 local play reads clearly with no
narration, a cold observer can tell *scheduled Move* from *scheduled Shoot* on the scrubber. What's ahead is
turning that proven loop into a real online PvP product: real networking, a landscape desktop UI, a commercial
art bar, and a cosmetic economy on Steam.

## What ships (C46)

Fill checkboxes as each `SCHEDULE.md` phase closes; leave unchecked until a real build actually demonstrates
them.

- [x] Time Card → Program → Lock In → resolve → Playback → Aftermath → next round (proven, local)
- [x] Continuous multi-room arena (C35/C39/C45): path waypoints, Snap / Hold Angle, two doors that change move or LoS
- [x] Desk-Lamp Diorama presentation floor: warm URP lighting, painted board, 线稿涂鸦 ink paths, cardstock Time Card, AR scrubber
- [x] Stepped clay playback + physical muzzle flash + wound splat
- [x] Distinct tactile foley (footstep / shot / Time Card / Lock In)
- [ ] Landscape desktop UI (Phase 1)
- [ ] Real online 1v1 PvP over a real transport (Phase 2)
- [ ] Matchmaking-fallback bot, invisible (Phase 3)
- [ ] Cosmetic-only IAP economy live on Steam (Phase 4)
- [ ] Commercial-grade character/board art (Phase 5)
- [ ] Steam store page, certified and live (Phase 6)

**Explicitly out:** gear cards (Bandage/Otherwise/vent/monitor/Flashbang/Adrenaline), attic, loot, high-speed
rail / 高铁, deep single-player PvE. Android/mobile is a separate future consideration, not this ship.

## Case study

### Hook (10 seconds)

A warm lamp over a small board in a dark void. Clay pawns. A thin ink path on the floor. Someone plays a Time
Card — cardstock, not a HUD chip — and the match clock commits. That should already feel like a handmade
miniature game, not a default Unity scene.

### Constraint

The core loop was proven inside an original 14-day sprint; that sprint framing is retired (**C46**) but the
loop it built is not — everything below still runs on it. Mid-build the board left the grid entirely:
continuous space, free-aim Shoot, visibility-graph pathfinding (C35/C39), later expanded to a multi-room
board with real tactical routing (C45). The remaining work isn't more mechanics — it's turning a proven local
loop into a real online, monetized product: networking that doesn't exist yet beyond a label, a UI built for
one-handed phone play that needs a real desktop redesign, and an art bar that needs to clear "not embarrassing"
and hit "worth paying for."

### Core loop

1. **Allot** — one side plays a Time Card: commit **N** seconds from the shared pool (900s pool, 30s minimum round). **N** is spent in full.
2. **Program** — both sides draft simultaneously inside **N**: click waypoints (ink grows), pick Sprint/Walk/Crawl, free-aim Snap or Hold Angle, optional door open/close. Costs deduct automatically.
3. **Lock In** — physical-switch moment; plans freeze.
4. **Resolve / Playback** — the authoritative resolver writes a ReplayTape; the scrubber walks Time Resource (Playback Duration may compress).
5. **Aftermath** — wounds and positions carry; next Time Card or match over (Dead / pool cannot fund another round).

Time Card is the match metronome, not a gear card. Move and Shoot are base verbs.

### Readable combat

A first-time observer on the scrubber should say "that Move made them move" and "that Shoot made them shoot."
Core combat adds stance bands, Snap vs Hold Angle, Healthy → Wounded → Dead, and two doors that change a fight
(block move or LoS when closed). Pawns may share space; wounds come only from Shoot radii — walls and closed
doors are the blockers.

### Presentation bets

Theme: **Desk-Lamp Diorama** — digital clay under a warm key light (`docs/ART_DIRECTION.md`), now raised to a
commercial ship bar rather than a "don't look like default Unity" floor. Paths are thin, slightly wobbly
hand-drawn ink (FragPunk / 界外狂潮-style 线稿涂鸦), not neon. Playback motion is stepped (stop-motion feel).
Muzzle flash is a short physical mesh pop; hits leave a clay wound splat. Foley aims for miniature tactility
(clay footstep, cap-gun shot, paper card, Lock In snap).

### What's next

Real networking (`docs/NETWORKING_DESIGN.md`) is the single biggest remaining gap — today's resolver is a
same-process local stand-in, not a real online match. Landscape desktop UI, a matchmaking-fallback bot, a
cosmetic-only IAP economy, and a commercial art bar round out the phase table (`docs/SCHEDULE.md`). Long-term
roadmap beyond this ship: destructible geometry, an asymmetric objective win condition, revive/Downed, a
unique-verb character roster (Bomber, Time Player — `docs/CHARACTER_ROSTER_LONGTERM.md`), high-speed rail.

## Architecture (short)

| Layer | Role |
|-------|------|
| `Sim/` | Engine-free geometry, pathfinding, costs |
| `Net/` | `GhostResolver` + `ReplayTape` (deterministic resolve) — today a same-process local stand-in; building toward a real transport per `docs/NETWORKING_DESIGN.md` |
| `Timeline/` | `PawnProgram` authoring against Time Resource |
| `Board/` + `Boot/` | Views + composition root / `RoundPlayback` |
| `UI/` | HUD — being redesigned landscape desktop-first per `docs/UI_FLOW.md` |

Host-authoritative ghost sim; presentation is scrubber-driven (Time Resource), not wall-clock AI. Engine
version: Unity **6000.5.5f1**.

## How to run (draft — verify on each phase's candidate build)

1. Install Unity **6000.5.5f1**; open the project folder (Hub: add parent `…/Game` if the project does not appear).
2. Enter Play from the Bootstrap composition root (`GameBootstrap` builds the scene in Play Mode).
3. Use a **landscape, 16:9** game view (**C48** — was portrait pre-pivot).
4. Play a Time Card → draft Move / Shoot / Door in the HUD dock → Lock In → scrub or autoplay Playback → Aftermath.

## Capture

See [`docs/CAPTURE_CHECKLIST.md`](CAPTURE_CHECKLIST.md) for the reusable shot list and stills, run at whichever
phase gate needs proof. Embed final video link + still paths here before promoting this file to root
`README.md`.

**Video:** _(pending)_
**Stills:** _(pending)_

## Credits / links

- Design: `docs/GDD.md`, `docs/CORE_LOOP.md`, `docs/PRODUCT_MEMORY.md`
- Slice history (shipped): `docs/VERTICAL_SLICE.md`; art bar: `docs/ART_DIRECTION.md`
- Schedule / scope: `docs/SCHEDULE.md`, `docs/SCOPE.md`
- Monetization / networking / AI: `docs/MONETIZATION.md`, `docs/NETWORKING_DESIGN.md`, `docs/AI_FALLBACK_BOT.md`
- Parallel multi-agent ops: `docs/PARALLEL_OPS.md`, `docs/DIRECTING_AGENTS.md`
