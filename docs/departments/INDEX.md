# Departments — Active Index

**Updated:** 2026-08-09 — board-rework wave confirmed merged and closed out; capacity now open for Phase 1
(`docs/SCHEDULE.md`'s phase table, `docs/PRODUCT_MEMORY.md` C46–C51). Wave 1+2 (Day 10 VFX / Day 11 Audio /
Ship docs) is fully shipped and merged — see git history for the old rows if needed.
**Ops constitution:** [`../PARALLEL_OPS.md`](../PARALLEL_OPS.md) · human-side playbook: [`../DIRECTING_AGENTS.md`](../DIRECTING_AGENTS.md)
**Contracts:** [`../contracts/CURRENT.md`](../contracts/CURRENT.md)
**Human rollup:** [`../DRAFT_HANDOFF.md`](../DRAFT_HANDOFF.md)

## Capacity

Integrator + up to **2** coding workers. `feat/board-edge-dressing` and `feat/playmode-board-rewrite`
(previously listed here as "queued but not yet started") are **merged** (`4a9a992`, `d81ffeb`) — that was
stale as of the 2026-08-08 reset; corrected here. Post-merge batchmode on `master` confirmed green
(EditMode 107/107, PlayMode 29/29, 2026-08-09). No wave currently active — both coding worker slots are open.
Check `git worktree list` and `DRAFT_HANDOFF.md`'s top section before assuming this table is current.

## Active agents / worktrees

| Dept | Branch | Status file | Notes |
|------|--------|--------------|-------|
| Presentation (UI) | `feat/phase1-landscape-ui` | `logiCard-phase1-landscape-ui` worktree (no dept STATUS.md yet — Phase 1 is the first slice of the new phase-based schedule) | Reworking `Assets/_Project/UI/ProgramHud.cs` from the old portrait/thumb-zone spec to `UI_FLOW.md`'s landscape desktop dock, plus minimal stub screens for the rest of the screen map. Brief: `PHASE1_LANDSCAPE_UI_AGENT_BRIEF.md` at the worktree root. |

## Ownership matrix (write locks)

Wave-specific row added on top of the evergreen ones below:

| Path / concern | Owner now |
|----------------|-----------|
| `Assets/_Project/UI/**` (+ its tests) | Presentation (`feat/phase1-landscape-ui`) — see brief for the one required exception (camera-rect wiring stays with Integrator) |
| `Boot/`, `Net/`, `Timeline/`, `Sim/` (fixes) | Core |
| `docs/DRAFT_HANDOFF.md`, `docs/SCHEDULE.md` ticks, `docs/contracts/CURRENT.md`, `docs/PRODUCT_MEMORY.md` | Core / Integrator |
| `docs/departments/<dept>/STATUS.md` | That dept only |

## Cross-review checklist (session start)

- [ ] Read DRAFT_HANDOFF
- [ ] Read this INDEX
- [ ] Read peer STATUS for every **In progress** row above
- [ ] Read contracts/CURRENT
- [ ] Confirm no file overlap before editing
