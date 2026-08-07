# D8: Schedule + Milestone DoD — 14-Day Implementation

**Doc ID:** D8  
**Status:** Revised 2026-08-03 — **continuous-space pivot inserted** (C35/C39, see [CONTINUOUS_PIVOT_PLAN.md](CONTINUOUS_PIVOT_PLAN.md)) — art pass now **compressed**, not full-scope, to keep Day 14; Revised 2026-07-30 — **C34 Polished Core Demo** (art-protected remaining days)  
**Depends on:** [VERTICAL_SLICE.md](VERTICAL_SLICE.md), [TDD.md](TDD.md), [GDD.md](GDD.md), [SCOPE.md](SCOPE.md), [ART_DIRECTION.md](ART_DIRECTION.md)  
**Assumes:** Pre-implementation gate passed; ~6 focused hours/day (~84h).

Clock starts on **Implementation Day 1** (first Unity scaffold commit), not calendar date of this doc.

---

## Ship bar (end of Day 14) — C34

Must have:

1. **Windows build** playable locally through Allot (Time Card) → Program → Reveal → Time Resource resolve → Playback → Aftermath → next round.  
2. Core combat readable: path/stance Move, Snap/Hold Shoot, wounds/death, **one door** changes move or LoS once.  
3. **Desk-Lamp Diorama presentation floor** met ([ART_DIRECTION.md](ART_DIRECTION.md) § Demo art floor) — cold observer does not call it a default Unity prototype.  
4. README case study + 60–90s capture video.  
5. Repo pushed with docs + architecture notes.

Nice-to-have (not ship blockers): Android smoke build, bots (**C19**), optional DoF/SSS, Fusion online (**C5** deferred).

---

## Milestone map (C34)

| Milestone | Days | Theme | Exit criteria |
|-----------|------|--------|----------------|
| **M0** | 1 | Project + folders | Unity project, platforms listed, folders, stubs |
| **M1 / Slice 1** | 2–4 | Pipeline proof | Time Card + Move + Shoot → Playback; second round; LoS Wound stub |
| **M2 / Core Combat** | 5–7 | Path, RPS, door | Stance bands; Snap vs Hold; one door; local match end-to-end (grid-based — later retargeted by M2.5) |
| **M2.5 / Continuous Pivot** | 7b–7g | Grid → continuous | See Phase 1–6 in [CONTINUOUS_PIVOT_PLAN.md](CONTINUOUS_PIVOT_PLAN.md) — geometry primitives, resolver/authoring retarget, Unity view retarget, whole-project-green |
| **M3 / Diorama Art** | 8–11 (**compressed**) | Presentation | URP/lighting, board/UI identity, clay motion/VFX, tactile audio — scope trimmed per the cut order below to absorb M2.5's cost without moving Day 14 |
| **M4 / Ship** | 12–14 | Build + portfolio | Windows candidate, playtest, capture, README |

If behind: **freeze at last green milestone**; do not start the next. Cut order: Android smoke → door reopen nuance → Crawl AV nuance → optional DoF/SSS → **never** cut Time Card loop, Move/Shoot readability, warm diorama composition, **线稿涂鸦** path, physical shot feedback, or Windows build stability (**C34**). **As of the continuous pivot (2026-08-03), this cut order is expected to be used more aggressively than originally planned** — M2.5 (~5.5–7.5 engineer-days) eats most of the runway that was going to Week 2's art pass; that tradeoff was made explicitly (see `CONTINUOUS_PIVOT_PLAN.md` §F), not discovered late.

**Day 3b is absorbed, not a 15th calendar day.** M1/Slice 1 still spans **Days 2–4** — 3b is scoped as same-day-or-buffer insert work within that window (C33 filled a gap C4 already implied but D8 never scheduled), not an extra day tacked onto the plan. **M2.5 (7b–7g) works the same way** — inserted days, not a 15th-day extension; Day 14 ship date is unchanged, absorbed by compressing M3 instead.

---

## Day-by-day

### Week 1 — Prove the tape + core combat

| Day | Focus | DoD (exit) |
|-----|--------|------------|
| **1** | Unity 6 project; Win + Android modules; **portrait lock (C30)**; gitignore; `_Project` folders; Bootstrap; SO stubs | Project opens; platforms listed; portrait-only; folders committed |
| **2** | Grid board 5×5; pawn; **Time Resource timeline scrubber**; phase enum (local) | Scrubber advances; phases switch |
| **3** | Program UI: schedule Move + Shoot; Lock; build `TimelinePayload` locally | Payload logs ExecuteTime + GridPosition + Stance + Modifier |
| **3b** | **Match lifecycle (C33):** shared pool; Time Card allotment; Allot→…→Aftermath; carry state | Two consecutive rounds playable |
| **4** | **Slice 1 green:** ghost resolve + playback; Wound stub; cold-observer Move/Shoot/Time Card test — **no new breadth** | **M1:** D7 Slice 1 checklist |
| **5** | Path drawing (waypoints) + time allotment → stance band | Sprint/Walk/Crawl change Move timing on Clock |
| **6** | Hold Angle vs Snap Shot; mutual same-second rule | RPS readable in one playtest |
| **7** | **One-door** micro-map (contextual open/close; blocks move + LoS); local match E2E | **M2:** Core Combat local playable |
| **7b** | Continuous pivot Phase 1: geometry primitives (`PlanarPosition`, `Segment`, `ArenaBoard`, `ContinuousLineOfSight`, `ContinuousPathfinder`) — handed to a second agent in a parallel worktree | New EditMode suites green in isolation; no other file touched |
| **7c** | Continuous pivot Phase 2: Sim/Net consumer retarget (`ScheduledPath`, `GhostResolver`, `Door`, `ActionNode`, `TapeEvent`) | `Sim`/`Net` compile + pass; `Timeline`/`Board`/`Boot` not yet green |
| **7d** | Continuous pivot Phase 3: `PawnProgram` retarget; revisit-tile guard removed; draft cost → Euclidean sum | `Timeline` compiles + passes |
| **7e–7f** | Continuous pivot Phase 4: Unity view/composition-root retarget (`BoardView`, `BoardInputController`, `GameBootstrap`, `RoundPlayback`); old grid files deleted | **First whole-project-green checkpoint since before 7c** |
| **7g** | Continuous pivot Phase 5–6: HUD wording + PlayMode test rewrite (parallelizable), tuning pass on `HitRadius`/`LaneHalfWidth`/`InteractRadius` | **M2.5:** continuous-space pivot complete, cold-observer playtest of the "door changes a fight once" bar |

### Week 2 — Diorama art pass (compressed — see M2.5 cost above) + ship

| Day | Focus | DoD |
|-----|--------|-----|
| **8** | Render/art foundation: **URP** migration, portrait camera composition, diorama base, warm lighting, material palette | Board reads as desk diorama under lamp light |
| **9** | Board/UI identity: handmade board dressing, painted grid, **线稿涂鸦** paths (FragPunk-A ink on clay), cardstock Time Card, AR scrubber polish | Paths + Time Card match ART_DIRECTION floor |
| **10** | Character/VFX motion: clay pawn silhouettes, **stepped** playback, physical muzzle flash, wound splat | Move vs Shoot vs hit visually distinct |
| **11** | Audio + feedback: tactile foley, transitions, hit feedback, visual hierarchy pass | Footsteps / shot / Time Card / Lock In distinct |
| **12** | Windows release-candidate; optional Android smoke; perf — drop costly FX before readability | **M3→ship candidate** Windows build runs |
| **13** | Cold observer + friend playtest; fix comprehension & presentation blockers only | Findings filed; blockers fixed |
| **14** | Final Windows build; 60–90s capture; README/case study; screenshots | **M4:** portfolio ship |

---

## Cadence

- **Daily:** ≥1 commit; tick checkboxes in this file when a Day DoD is met.  
- **Playtests:** end of Day 4 (Slice 1), Day 7 (Core Combat, grid), Day 7g (**M2.5** continuous pivot — same "door changes a fight once" bar, continuous version), Day 13 (presentation). Three written findings each.  
- **Scope knife owner:** you — when late, follow **C34** cut order above.

---

## Day DoD checklist (living)

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

## Risks that own calendar days

| Risk | Hits days | Mitigation in schedule |
|------|-----------|-------------------------|
| Art pass overrun | 8–11 | Required floor only; cut optional DoF/SSS first (**C34**) |
| Path UI overruns | 5–6 | Keep Day 3 click-destination if needed; 线稿涂鸦 path is Day 9 |
| URP migration pain | 8 | Start Day 8 early; primitives stay playable until materials land |
| Scope creep (full old GDD) | any | **C34** freeze: no Fusion/gear cards/attic in 14-day ship |
| Android SDK pain | 12 | Smoke only if Windows candidate is already green |
| Continuous pivot overrun | 7b–7g | Phase 1 and Phase 5 run in parallel worktrees to compress the critical path; if still behind, art pass (8–11) absorbs the overflow per the cut order — Day 14 ship date does not move |

Detail register: [RISKS.md](RISKS.md).
