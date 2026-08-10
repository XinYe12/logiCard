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

| Phase | Theme | Exit criteria (what must be true to start the next phase) | Status |
|-------|-------|-------------------------------------------------------------|--------|
| **Phase 0** | Pivot Lock (docs) | This docs rewrite fully landed: `PRODUCT_MEMORY.md` C46–C51 confirmed, `SCOPE`/`RISKS`/`GDD`/`CORE_LOOP` updated, `MONETIZATION`/`NETWORKING_DESIGN`/`AI_FALLBACK_BOT` exist (even with OPEN numerics). No code work starts before this gate — same "docs before code" discipline as the original pre-implementation gate. | **Done** (2026-08-08) |
| **Phase 1** | Landscape Desktop UI | `UI_FLOW.md`'s new layout implemented; playable mouse+keyboard only; PlayMode tests updated/green against landscape geometry. | **Mechanical bar met** (2026-08-09, `feat/phase1-landscape-ui` merged) — `ProgramHud`/`AppFlowController` landscape rework + click-through Boot→Match End shell, EditMode 108/108 + PlayMode 32/32 on `master` post-merge. Still wants a human Editor look (dock overlap, readability at real window size) before calling the *visual* bar done — same gate this project has used for every prior presentation change. |
| **Phase 2** | Real Networking Foundation | `NETWORKING_DESIGN.md`'s transport/topology decided and its first slice built — a real, two-process, real-transport tape-synced match replacing the same-process `GhostResolver` stand-in; host-integrity question has an explicit answer. | **Paused (2026-08-09)** — not abandoned, already-landed work stays merged (transport+host-integrity locked **C52**; `IMatchResolver`/`RelayMatchResolver`/relay process merged and wired live). Human call: stop advancing core gameplay/networking until look-and-feel and UI (Phase 5 + UI work below) are in a good place. Resume when told to. |
| **Phase 3** | Matchmaking Fallback Bot | `AI_FALLBACK_BOT.md`'s bot substitutes in behind the same interfaces Phase 2 built, invisibly, meeting its defined difficulty/behavior bounds. | Not started (depends on Phase 2, which is paused). |
| **Phase 4** | Monetization Foundation | `MONETIZATION.md`'s economy model implemented at skeleton depth — at least one earned and one purchasable cosmetic wired through Steam's IAP sandbox; no-pay-to-win guardrails enforced in code, not just docs. | Not started. |
| **Phase 5** | Commercial Art Bar | `ART_DIRECTION.md`'s raised bar met for the shipped roster — placeholder Quaternius meshes replaced/substantially reworked; a cold observer calls it "a real game," not "a prototype." | **Active, top priority.** Env checkpoints 1-3 (weather/lighting, Poly Haven PBR surfaces, Quaternius door/prop meshes), URP post-processing (Volume Profile/SSAO/MSAA/soft shadows + photo-mode), camera rotation, and smooth pawn animation (**C55**) all landed. Scout re-outfitted to fix a genre-clash (**C56**). **2026-08-10 (later same day): a real screenshot caught two of these as visually broken despite every batchmode check passing** — reflection probes read as unchanged (root cause: probe `clearFlags` never set, rendering the wrong environment) and clouds rendered as solid black rectangles (root cause: particle material never configured for alpha blending). Both fixed and verified via disposable-worktree batchmode; a third, related bug (window glass also opaque, silently blocking checkpoint 3's "lit window" glow dressing) was found proactively via code audit and fixed too — first attempt used the wrong shader keyword, caught by inspecting the regenerated asset before shipping it. `orthographicSize` resolved analytically (projection math confirms ~79% vertical board coverage, matching its documented calibration target) — no change needed. **Character material fidelity gap deliberately left open** — no textures exist in the source pack to wire up, closing it means either sourcing new assets (a real art-direction call) or blind procedural tuning with no way to verify it during a human-unavailable window; logged as a conscious "don't guess" decision, not an oversight. **Still open before calling this phase's bar met:** an actual human/Editor look at everything above — none of today's fixes have been visually confirmed, only verified as "correct by direct inspection of serialized state." **See `DRAFT_HANDOFF.md`'s "⚠️ Awaiting human review" section (top of file) for the exact parked checklist** — kept unmonitored per explicit human instruction, not forgotten. Core gameplay stays paused until this phase is genuinely done, not just improved. |
| **Phase 6** | Steam Certification & Ship | Steamworks integration complete, store page live, cert/review passed, `SHIP_README_DRAFT.md`/`CAPTURE_CHECKLIST.md` produced for the real product. | Not started. |

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
