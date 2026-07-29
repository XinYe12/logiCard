# logiCard — Pre-Implementation Project

**Status:** **GATE PASSED** 2026-07-29 — Unity implementation track is **UNLOCKED**. Follow [SCHEDULE.md](SCHEDULE.md) Day 1+.

**Purpose:** Industrial pre-production docs (D1–D12) are complete. This file remains the historical checklist.

**Related:** [PRODUCT_MEMORY.md](PRODUCT_MEMORY.md) (confirmed decisions) · plan file · `.cursor/rules/logicard-product-memory.mdc`

---

## Gate rule (binding)

Do **not** treat pre-impl as incomplete. Gate passed **2026-07-29**. Further design changes still go through PRODUCT_MEMORY CONFIRMED updates.

---

## Required docs (must write before implementation)

| # | Doc | Path (target) | What “Done” means |
|---|-----|---------------|-------------------|
| D1 | **Vision one-pager** | `docs/VISION.md` | Fantasy, audience, platforms, player count, session length, success metric, non-goals |
| D2 | **Scope box** | `docs/SCOPE.md` | In / out / later for the 2-week demo milestone only |
| D3 | **Core loop sheet** | `docs/CORE_LOOP.md` | Player-facing Plan→Lock→Reveal→Resolve→Aftermath steps; what you decide vs what system does |
| D4 | **GDD v0.1** | `docs/GDD.md` | Overview, match structure, FoW/board, cards, win/lose, v1 content list, out of scope — unambiguous enough to build |
| D5 | **Tabletop portability note** | `docs/TABLETOP_RULES.md` | Same rules with **zero** engine talk (tiles/tokens OK); proves offline table potential |
| D6 | **TDD one-pager** | `docs/TDD.md` | Authority model, synced state, major messages, folder architecture, mobile perf budget |
| D7 | **Vertical slice spec** | `docs/VERTICAL_SLICE.md` | Exact A→B→C play path, stub vs real, pass/fail checklist, Win+Android |
| D8 | **Schedule + milestone DoD** | `docs/SCHEDULE.md` | Day/milestone exit criteria aligned to slice |
| D9 | **Product memory (living)** | `docs/PRODUCT_MEMORY.md` | CONFIRMED vs OPEN kept current |
| D10 | **Art direction (short)** | `docs/ART_DIRECTION.md` | Style refs, palette intent, do/don’t — placeholders OK in production, direction locked here |
| D11 | **Risk register** | `docs/RISKS.md` | Top risks + mitigations for demo |
| D12 | **UI / UX flow** | `docs/UI_FLOW.md` | Screens: lobby → plan → reveal → end; touch targets / **portrait one-handed** (**C30**) |

### Checklist

- [x] D1 Vision one-pager — updated 2026-07-27 (`docs/VISION.md`)
- [x] D2 Scope box — locked 2026-07-27 (`docs/SCOPE.md`) + C18/C19
- [x] D3 Core loop sheet — drafted 2026-07-27 (`docs/CORE_LOOP.md`)
- [x] D4 GDD v0.1 — revised 2026-07-28 Character + path/stance + cards-as-gear (`docs/GDD.md`)
- [x] D5 Tabletop portability note — v2.0 Time Track; paper playtest considered done 2026-07-29
- [x] D6 TDD one-pager — Host ghost sim + ReplayTape 2026-07-29 (`docs/TDD.md`)
- [x] D7 Vertical slice spec — drafted 2026-07-29, Slice 1 = Move+Shoot pipeline proof (`docs/VERTICAL_SLICE.md`); card-phrasing fixed
- [x] D8 Schedule + milestone DoD — drafted 2026-07-29 (`docs/SCHEDULE.md`); M0–M5 / Days 1–14 aligned to Slices 1–4
- [x] D9 Product memory current — synced 2026-07-29 (`docs/PRODUCT_MEMORY.md`); C26 save-file rule; continuous Time Resource C27/C28
- [x] D10 Art direction short — Desk-Lamp Diorama bible 2026-07-29 (`docs/ART_DIRECTION.md`); moodboard `image.png`
- [x] D11 Risk register — drafted 2026-07-29 (`docs/RISKS.md`)
- [x] D12 UI / UX flow — drafted 2026-07-29 (`docs/UI_FLOW.md`)
- [x] **Gate:** `confirm: pre-implementation gate passed` — **2026-07-29**




---

## Recommended order (write in this sequence)

1. D9 memory (already started) → **D1 Vision** → **D2 Scope**  
2. **D3 Core loop** → **D4 GDD v0.1** → **D5 Tabletop** (GDD and tabletop must not contradict)  
3. **D7 Vertical slice** (cuts GDD down to what we build first)  
4. **D6 TDD** + **D12 UI flow** (how we build the slice)  
5. **D10 Art** + **D11 Risks** + **D8 Schedule** (production wrap)  
6. Gate confirm → implementation project begins  

---

## Explicitly not required before implementation

- Full card-by-card balance spreadsheet (stub numbers inside GDD are enough)  
- Final art bible / full audio design  
- Store listing, marketing site, legal package  
- Month-2 features documented in detail  

---

## Agent instructions

- Current project phase = **pre-implementation docs**.  
- Prefer writing/editing the docs above over Unity code.  
- When a doc is finished, check it off here and note date.  
- Do not invent final GDD content without user confirmation; draft with user, then confirm.  
