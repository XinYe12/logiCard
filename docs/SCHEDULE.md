# D8: Schedule + Milestone DoD — 14-Day Implementation

**Doc ID:** D8  
**Status:** Drafted 2026-07-29  
**Depends on:** [VERTICAL_SLICE.md](VERTICAL_SLICE.md), [TDD.md](TDD.md), [GDD.md](GDD.md), [SCOPE.md](SCOPE.md)  
**Assumes:** Pre-implementation gate passed; ~6 focused hours/day (~84h).

Clock starts on **Implementation Day 1** (first Unity scaffold commit), not calendar date of this doc.

---

## Ship bar (end of Day 14)

Must have:

1. **Windows build** playable 1v1 through Program → Reveal → Time Resource resolve → Playback → visible outcomes.
2. **Android APK** that can join the same Fusion room as Windows (cross-play) **or**, if net slips, Android at least runs Slice 2+ local playback — **prefer full cross-play**; cut content before cutting Android entirely (**C6**).  
3. Slice pipeline proven: Move + Shoot base verbs → timeline → Clock → visible board (**C24**).  
4. README case study + 60–90s capture video.  
5. Repo pushed with docs + architecture notes.

Nice-to-have (not ship blockers): bots (**C19**), full GDD Section 9, clay polish.

---

## Milestone map (aligned to D7 slices)

| Milestone | Days | Theme | Exit criteria |
|-----------|------|--------|----------------|
| **M0** | 1 | Project + folders | Unity project, Win+Android in Build Settings, `.gitignore`, `_Project` folders per TDD, Character/Card SO stubs |
| **M1 / Slice 1** | 2–4 | Pipeline proof | Schedule Move + Shoot → Lock/Reveal → Time Resource resolve → Playback shows move + shoot; LoS Wound; cold observer test |
| **M2 / Slice 2** | 5–7 | Path + stance + RPS | Real path-draw UI; Sprint/Walk/Crawl; Snap vs Hold Angle; Windows playable end-to-end local or hotseat |
| **M3 / Slice 3** | 8–10 | Wound + map interact | Wound/Bandage/Otherwise→Stop; doors; second gadget optional; Android **portrait one-handed** UI pass started (**C30**) |
| **M4 / Slice 4** | 11–12 | Net + gadgets | Fusion Host 1v1; `TimelinePayload` → ghost sim → `ReplayTape` playback; vent/monitor and/or Flashbang/Adrenaline as time allows |
| **M5 Ship** | 13–14 | Builds + portfolio | Win + Android artifacts, friend playtest, README + video |

If behind: **freeze at last green Slice**; do not start next Slice. Cut order: gadgets → vent/monitor → doors → stance polish → **never** cut Clock visibility or Move/Shoot proof.

---

## Day-by-day

### Week 1 — Prove the tape

| Day | Focus | DoD (exit) |
|-----|--------|------------|
| **1** | Unity 6 project; Win + Android modules; **portrait lock (C30)**; gitignore; `Assets/_Project/{Net,Sim,Timeline,UI,Board,Characters,Cards}`; empty Bootstrap scene; ScriptableObject stubs (Character Scout/Heavy, CardData) | Project opens; both platforms listed; portrait-only in Player Settings; folders committed |
| **2** | Grid board 5×5; pawn; **Time Resource timeline scrubber**; phase enum Program/Reveal/Execute (local) | Scrubber advances; phases switch with debug buttons |
| **3** | Program UI: schedule Move (click destination) + Shoot (pick direction/tile); Lock; build `TimelinePayload` locally | Payload prints/logs with ExecuteTime + GridPosition + Stance + Modifier |
| **4** | **Slice 1 complete:** local Host-style ghost resolve (even offline) + playback moves/shoots; Wound stub text on hit | **M1:** D7 Slice 1 checklist all checked (except net) |

### Week 1 continued — Path & stance

| Day | Focus | DoD |
|-----|--------|-----|
| **5** | Path drawing (waypoints) + time allotment → stance band; replace hardcode stance | Sprint/Walk/Crawl change Move timing on Clock |
| **6** | Hold Angle vs Snap Shot; Shoot as base verb modes; mutual same-tick rule | RPS readable in one playtest |
| **7** | Buffer / bugfix; Android smoke build (empty or Slice 1 scene OK); playtest notes | **M2:** Slice 2 stable on Windows; Android project builds once |

### Week 2 — Depth, net, ship

| Day | Focus | DoD |
|-----|--------|-----|
| **8** | Wound states + Bandage + Otherwise Invalid→Stop | Wounded surcharge or bleed rule plays; invalid Move→Stop |
| **9** | Doors (block move/LoS); Interact; optional attic stub | Door changes a fight once |
| **10** | Buffer + Android **portrait one-handed** UI pass (thumb zone, safe area, ≥48dp targets, tap-then-tap card placement) | **M3:** Slice 3 playable; phone playable single-thumb held upright |
| **11** | Photon Fusion Host Mode: create/join 1v1; RPC `TimelinePayload`; Host ghost → `ReplayTape` sync | Two clients: Program → same playback tape |
| **12** | Vent and/or Monitor and/or Flashbang/Adrenaline (pick by remaining time); disconnect handling stub | **M4:** online 1v1 completes a full round |
| **13** | Windows + Android release-candidate builds; friend playtest; crash pass | Both artifacts run; notes filed |
| **14** | README case study; architecture diagram; 60–90s video; GitHub polish | **M5:** portfolio ship |

---

## Cadence

- **Daily:** ≥1 commit; tick checkboxes in this file when a Day DoD is met.  
- **Playtests:** end of Day 4 (Slice 1), Day 7 (Slice 2), Day 11–12 (net). Three written findings each.  
- **Scope knife owner:** you — when late, drop Later-list features first (see cut order above).

---

## Day DoD checklist (living)

- [x] Day 1 — project + folders  
- [x] Day 2 — grid + clock + phases  
- [x] Day 3 — Program Move/Shoot + local payload  
- [ ] Day 4 — **M1 Slice 1**  
- [ ] Day 5 — path + stance  
- [ ] Day 6 — Snap vs Hold Angle  
- [ ] Day 7 — **M2** + Android smoke  
- [ ] Day 8 — Wound / Bandage / Otherwise  
- [ ] Day 9 — Doors / Interact  
- [ ] Day 10 — **M3** + Android UI  
- [ ] Day 11 — Fusion 1v1 tape pipeline  
- [ ] Day 12 — **M4** gadgets/map as able  
- [ ] Day 13 — dual builds + playtest  
- [ ] Day 14 — **M5** README + video  

---

## Risks that own calendar days

| Risk | Hits days | Mitigation in schedule |
|------|-----------|-------------------------|
| Fusion learning curve | 11–12 | Offline ghost+tape identical API by Day 4; swap transport Day 11 |
| Path UI overruns | 5–6 | Keep Day 3 click-destination forever if needed |
| Android SDK pain | 7, 10, 13 | Smoke on Day 7; don’t wait until 13 |
| Scope creep (full GDD) | any | Slice freeze rule |

Detail register: [RISKS.md](RISKS.md) (D11 — draft next if missing).
