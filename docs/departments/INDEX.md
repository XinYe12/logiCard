# Departments — Active Index

**Updated:** 2026-08-09 — Phase 1 (Landscape Desktop UI) shipped and merged; capacity fully open again.
Wave 1+2 (Day 10 VFX / Day 11 Audio / Ship docs) is fully shipped and merged — see git history for the old
rows if needed.
**Ops constitution:** [`../PARALLEL_OPS.md`](../PARALLEL_OPS.md) · human-side playbook: [`../DIRECTING_AGENTS.md`](../DIRECTING_AGENTS.md)
**Contracts:** [`../contracts/CURRENT.md`](../contracts/CURRENT.md)
**Human rollup:** [`../DRAFT_HANDOFF.md`](../DRAFT_HANDOFF.md)

## Capacity

Integrator + up to **2** coding workers. `feat/phase1-landscape-ui` merged 2026-08-09 (`ProgramHud`/
`AppFlowController` landscape rework + camera-rect wiring); post-merge batchmode on `master` confirmed green
(EditMode 108/108, PlayMode 32/32). No wave currently active — both coding worker slots are open. Check
`git worktree list` and `DRAFT_HANDOFF.md`'s top section before assuming this table is current.

## Active agents / worktrees

Blank pending the next wave.

| Dept | Branch | Status file | Notes |
|------|--------|--------------|-------|
| — | — | — | — |

## Ownership matrix (write locks)

Back to the evergreen rows — the Phase 1 wave-specific row is closed (see `docs/contracts/CURRENT.md`'s
Closed contracts section for the camera-rect coupling it needed):

| Path / concern | Owner now |
|----------------|-----------|
| `Boot/`, `Net/`, `Timeline/`, `Sim/` (fixes) | Core |
| `docs/DRAFT_HANDOFF.md`, `docs/SCHEDULE.md` ticks, `docs/contracts/CURRENT.md`, `docs/PRODUCT_MEMORY.md` | Core / Integrator |
| `docs/departments/<dept>/STATUS.md` | That dept only |

## Cross-review checklist (session start)

- [ ] Read DRAFT_HANDOFF
- [ ] Read this INDEX
- [ ] Read peer STATUS for every **In progress** row above
- [ ] Read contracts/CURRENT
- [ ] Confirm no file overlap before editing
