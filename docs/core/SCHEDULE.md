# D8: Schedule — Phase / Milestone Plan

**Doc ID:** D8  
**Status:** Revised 2026-08-08 — **C46/C50 full scope pivot**: replaces the Day-1–14 calendar with a
phase/milestone model, no fixed date (see `PRODUCT_MEMORY.md` C46–C51). Prior: 2026-08-03 continuous-space
pivot inserted (C35/C39); 2026-07-30 C34 Polished Core Demo (both superseded).  
**Depends on:** [VERTICAL_SLICE.md](VERTICAL_SLICE.md), [TDD.md](TDD.md), [GDD.md](GDD.md), [SCOPE.md](SCOPE.md), [ART_DIRECTION.md](ART_DIRECTION.md), [MONETIZATION.md](MONETIZATION.md), [NETWORKING_DESIGN.md](NETWORKING_DESIGN.md), [AI_FALLBACK_BOT.md](AI_FALLBACK_BOT.md)

**No fixed calendar.** Progress is measured by phase exit criteria — what must be true before the next phase
starts — not by day count. The pre-pivot build (below, "Pre-pivot build history") already proved the core
loop end-to-end; this phase table is what's left to reach a real Steam ship.

---

## Ship bar — C46

Must have to ship:

1. Real online 1v1 PvP through the full loop (Allot → Program → Reveal → Resolve → Playback → Aftermath),
   landscape desktop, Steam.
2. Core combat unchanged and readable: path/stance Move, Snap/Hold Shoot, wounds/death, both doors change
   move or LoS.
3. **Commercial ship art bar** met ([ART_DIRECTION.md](ART_DIRECTION.md) § Commercial ship art bar) — reads
   as a real game, not a prototype.
4. Free-to-play, cosmetic-only IAP live and enforced (no pay-to-win).
5. Matchmaking-fallback bot live, invisible, meeting its difficulty/disclosure bounds.
6. Steam storefront certified and live.

## Phase table

Each phase's exit criteria are a literal checklist — check a box only when it's genuinely true, not
"mostly true" or "batchmode-verified but not eyeballed." Prose detail on how/when each item landed moves
to that phase's dated log entry below the table, not into the checklist itself — the checklist should
stay scannable in one glance.

### Phase 0 — Pivot Lock (docs) — **Done (2026-08-08)**

- [x] `PRODUCT_MEMORY.md` C46–C51 confirmed
- [x] `SCOPE`/`RISKS`/`GDD`/`CORE_LOOP` updated
- [x] `MONETIZATION`/`NETWORKING_DESIGN`/`AI_FALLBACK_BOT` exist (even with OPEN numerics)

### Phase 1 — Landscape Desktop UI — **Mechanical bar met (2026-08-09); visual bar open**

- [x] `UI_FLOW.md`'s new layout implemented (`feat/phase1-landscape-ui` merged)
- [x] Playable mouse+keyboard only
- [x] PlayMode tests updated/green against landscape geometry (EditMode 108/108, PlayMode 32/32 at merge)
- [ ] Human Editor look at real window size (dock overlap, readability) — same visual-confirm gate every
      presentation change in this project uses

### Phase 2 — Real Networking Foundation — **Paused (2026-08-09); narrow carve-outs only**

- [x] Transport/topology decided, host-integrity question answered (**C52**)
- [x] `IMatchResolver`/`RelayMatchResolver`/relay process merged and wired live
- [ ] Real, two-process, real-transport tape-synced match replacing the same-process `GhostResolver`
      stand-in — **not started, phase paused**

Human call (2026-08-09): stop advancing core gameplay/networking broadly until Phase 5 + UI work are in
a good place. **Standing carve-out (2026-08-20):** narrowly-scoped core-gameplay slices can land when the
human explicitly directs it (e.g. C36/Bomber, human said "character, GO") — this is per-slice permission,
not a phase resume. Resume the phase itself only when told to.

### Phase 3 — Matchmaking Fallback Bot — **Not started** (depends on Phase 2)

- [ ] Bot substitutes in behind Phase 2's interfaces, invisibly, meeting `AI_FALLBACK_BOT.md`'s
      difficulty/behavior bounds

### Phase 4 — Monetization Foundation — **Not started**

- [ ] At least one earned and one purchasable cosmetic wired through Steam's IAP sandbox
- [ ] No-pay-to-win guardrails enforced in code, not just docs

### Phase 5 — Commercial Art Bar — **Active, top priority**

- [x] Env checkpoints 1–3 (weather/lighting, Poly Haven PBR surfaces, Quaternius door/prop meshes)
- [x] URP post-processing (Volume Profile/SSAO/MSAA/soft shadows + photo-mode)
- [x] Camera rotation
- [x] Smooth pawn animation (**C55**)
- [x] Scout re-outfitted to fix genre-clash (**C56**)
- [x] Reflection-probe / cloud-alpha / window-glass render bugs found by real screenshot, fixed and
      re-verified (2026-08-10)
- [ ] **A cold observer calls the shipped roster "a real game," not "a prototype"** — the actual phase
      exit criterion; nothing below has been visually confirmed against it yet
- [ ] Human/Editor look at everything checked above — currently only "correct by direct inspection of
      serialized state," not eyeballed. See `DRAFT_HANDOFF.md` STATE block for the live parked checklist.
- [ ] Character material fidelity gap — deliberately left open, needs an art-direction call (source new
      textures) or explicit sign-off to blind-tune; not a blocker for the bar, tracked so it isn't lost

Core gameplay stays paused (Phase 2 rule above) until this phase's unchecked boxes are genuinely
checked, not just improved.

### Phase 6 — Steam Certification & Ship — **Not started**

- [ ] Steamworks integration complete
- [ ] Store page live
- [ ] Cert/review passed
- [ ] `SHIP_README_DRAFT.md`/`CAPTURE_CHECKLIST.md` produced for the real product

Phases 1, 2, and 5 have no hard interdependency and can run as parallel worker slices per `PARALLEL_OPS.md`.
Phase 3 depends on Phase 2's interfaces. Phase 4 and 6 depend on Steam integration groundwork that can start
as early as convenient. If a phase's scope turns out heavier than expected: **freeze at the last completed
phase**, don't half-start the next one — same discipline the old cut-order rule protected, now applied to
phases instead of days.

**Reprioritization (2026-08-09):** the human explicitly paused Phase 2 (and any further core-gameplay work)
to focus on look-and-feel and UI first — a real-time playtest showed visual/UI quality wasn't close to where
it needs to be. The concurrent UI effort (`feat/ui-component-system`) doesn't map to its own phase number —
it's UI-system debt/polish (shared component factory, layout pass, missing Adrenaline button) discovered
alongside Phase 5's art push, tracked via `contracts/CURRENT.md`/`departments/INDEX.md` like any other wave
rather than added as a new phase row. Core gameplay resumes once both this UI work and Phase 5's checkpointed
first pass are in a good place.

**Map roster (2026-08-10, `PRODUCT_MEMORY.md` C57):** same pattern as the UI work above — doesn't map to
its own phase number, tracked as its own wave. The human asked for logiCard's map roster to grow from one
hardcoded map to three, each with real interactive terrain (vents/breaches usable by both sides), and
explicitly lifted the core-gameplay-paused rule **for map/terrain Sim-layer work only** (Net/Timeline/other
Sim work stays paused). Landed and wired same day: two new `Door` kinds (Vent/Breach, zero new Sim types),
`MapId`/`MapLayout` groundwork, Freight Yard retrofitted with one of each, two new maps (Rail Platform,
Vault Complex) built in parallel by two workers and merged by the Integrator, shared dispatch
(`BuildBoard(MapId)`/`MapDefinitions.ForId`/map-aware `BuildPawns()`) wired so all three are selectable.

**Vibrancy pass + map-select UI (2026-08-10, `PRODUCT_MEMORY.md` C58/C59):** two more parallel waves,
same day. The human pushed back on the desaturated look ("big changes... Link's Awakening... vibrant")
and asked for a recolor-only pass (C58: post-processing grade + surface tints warmed/saturated, clouds
denser — cloud *style* only partially landed, still real-texture not cartoon, flagged as a follow-up)
plus closed C57's own deferred item (C59: floor grid-line clutter deleted, a real local-only map-select
screen built between Character Select and Lobby so `ActiveMap` is no longer a hardcoded constant, plus
a `ModalDialog` restyle toward a human-supplied reference). Both delegated to parallel worker worktrees;
the map-continuation one needed real Integrator fixes before merge (a compile error, two PlayMode
regressions from deferring `GameBootstrap`'s board-build until match start, and a bug — wrong builtin-
resource API — that would have broken in a shipped Player build, not just tests). Combined final state:
EditMode 124/124, PlayMode 37/37. Character-movement vibrancy stays explicitly deferred, not forgotten.
Same standing caveat as Phase 5's row above: batchmode-verified, **not yet visually confirmed** by a
human.

---

## Cadence

- Commit at natural checkpoints within a phase; tick a phase's exit criteria only when genuinely met.
- **Playtests:** at each phase gate, not a fixed calendar day — mirrors the "three written findings" cadence
  the pre-pivot build already used (see `DAY13_PLAYTEST_FINDINGS.md` for the template).
- **Scope knife owner:** you — when a phase is heavier than expected, freeze at the last completed phase
  rather than partially building the next one.

---

## Pre-pivot build history (historical appendix — the core loop below is already built and working)

Everything in this section predates the pivot and is kept as a real, valuable record — the core loop it
describes is exactly what Phase 1–6 above build on top of, unchanged. Do not re-litigate or rebuild any of
this; it's done.

### Day DoD checklist (historical)

- [x] Day 1 — project + folders  
- [x] Day 2 — grid + clock + phases  
- [x] Day 3 — Program Move/Shoot + local payload  
- [x] Day 3b — Match pool / Time Card lifecycle (**C33**)  
- [x] Day 4 — **M1 Slice 1** (cold observer; no new breadth)  
- [x] Day 5 — path + stance  
- [x] Day 6 — Snap vs Hold Angle  
- [x] Day 7 — **M2** one door + local E2E  
- [x] Day 7b — continuous pivot Phase 1 (geometry primitives)  
- [x] Day 7c — continuous pivot Phase 2 (Sim/Net retarget)  
- [x] Day 7d — continuous pivot Phase 3 (PawnProgram retarget)  
- [x] Day 7e–7f — continuous pivot Phase 4 (Unity views, whole-project-green)  
- [x] Day 7g — **M2.5** continuous pivot Phase 5–6 (HUD/tests, tuning) — Phase 6 human call landed 2026-08-07 (see `DRAFT_HANDOFF.md`)  
- [x] Day 8 — URP + diorama lighting foundation (compressed scope)  
- [x] Day 9 — board/UI identity (线稿涂鸦 path, Time Card) — human accepted 2026-08-07 with reservations on board polish; schedule takes priority over further art passes (see `DRAFT_HANDOFF.md`)  
- [ ] Day 10 — clay motion + physical VFX  
- [ ] Day 11 — audio + feedback  
- [ ] Day 12 — Windows candidate (+ optional Android smoke)  
- [ ] Day 13 — playtest / presentation bugfix  
- [ ] Day 14 — **M4** README + video  

---

## Risks

The old calendar-day risk table is retired along with the Day-1–14 schedule it was keyed to. Current risks
are tracked per-phase in [RISKS.md](RISKS.md), which was rewritten alongside this doc for the same pivot.
