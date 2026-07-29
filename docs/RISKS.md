# D11: Risk Register — 2-Week Demo

**Doc ID:** D11  
**Status:** Drafted 2026-07-29  
**Depends on:** [PRODUCT_MEMORY.md](PRODUCT_MEMORY.md), [SCHEDULE.md](SCHEDULE.md), [TDD.md](TDD.md), [SCOPE.md](SCOPE.md)

Living register. Re-score after each milestone playtest (Days 4 / 7 / 11–12).

**Legend:** L = likelihood · I = impact (1–5). **Score** = L × I. Owner default: you + AI pair.

---

## Top risks

| ID | Risk | L | I | Score | Mitigation | Trigger to cut / pivot |
|----|------|---|---|-------|------------|------------------------|
| R1 | **Fusion learning curve** burns Days 11–12 | 5 | 5 | 25 | Offline Host-identical ghost→`ReplayTape` API by Day 4; swap transport Day 11 only | If no 2-client tape by end Day 12 → ship local/hotseat + document “net Week 3” |
| R2 | **Scope heavier than 14 days** (path UI, stances, wounds, doors, gadgets) | 5 | 4 | 20 | Strict Slice freeze ([VERTICAL_SLICE.md](VERTICAL_SLICE.md)); cut order: gadgets → vent/monitor → doors → stance polish | Never cut Slice 1 Move+Shoot visibility |
| R3 | **Time Resource vs Playback Duration** confuses players / designers | 4 | 4 | 16 | UI labels Time Resource scrubber; cinema uses separate Playback clock; tooltips once | If playtests misread order, slow Playback before changing math |
| R4 | **Android heat / FPS** from DoF/SSS/clay look | 4 | 4 | 16 | **C13** smooth-first; drop DoF→SSS→shadows before readability; portrait only (**C30**) | Mid-device &lt;30 FPS → primitives + flat lit |
| R5 | **Path-draw UI overrun** (Days 5–6) | 4 | 4 | 16 | Keep Day 3 click-destination forever if needed; yarn path is polish | Path polish slips → Slice 2 reduced to click+stance slider |
| R6 | **Determinism / desync** if any resolve uses physics or client math | 3 | 5 | 15 | Grid + Bresenham only on Host; clients playback-only (**C23**) | Any client-side hit detect → delete immediately |
| R7 | **Cheat / invalid payloads** | 3 | 4 | 12 | Host revalidates Speed×Stance×budget before accept | Reject + Otherwise Stop substitute |
| R8 | **Phone-as-Host instability** | 3 | 4 | 12 | Prefer Windows Host in demos; document it | Android-only lobby fails → require Win host |
| R9 | **Continuous Time Resource window too large** (15-min fantasy in one Program) | 4 | 3 | 12 | Demo per-round **60s TR** placeholder until confirmed | Playtests too long → shorter TR window |
| R10 | **Art ambition vs primitives** (D10 full clay) | 3 | 3 | 9 | D10 = target; demo evokes with lighting/camera | No shader rabbit holes before M3 |
| R11 | **Dual-build tax** (Win + Android Day 13–14) | 3 | 4 | 12 | Android smoke Day 7; debug APK only | Content cut before dropping Android (**C6**) |
| R12 | **GDD still shifting mid-impl** | 3 | 5 | 15 | D9 save-file rule (**C26**); no new CONFIRMED without chat confirm + C# row | Park ideas in OPEN |

---

## Risk responses by milestone

| Milestone | Primary watch |
|-----------|----------------|
| M1 Slice 1 | R3, R6 — tape readable, Host-only resolve |
| M2 Slice 2 | R5, R2 — path/stance scope |
| M3 Slice 3 | R4, R10 — Android UI + art cost |
| M4 Slice 4 | R1, R8, R7 — Fusion |
| M5 Ship | R11, R2 — builds + cut list |

---

## Accepted risks (conscious)

- No FoW in demo (visibility simplifies sync testing).  
- Bots nice-to-have only (**C19**).  
- Full 15-minute single Program not required for demo.  
- Clay final fidelity not required for ship bar.

---

## Log (append playtest notes)

| Date | Finding | Action |
|------|---------|--------|
| 2026-07-29 | Register created | — |
