# Departments — Active Index

**Updated:** 2026-08-09 — core-gameplay work paused; both worker slots now on a look-and-feel/UI wave
(**C53** art-direction broadening + a UI redesign effort). Wave 1+2 (Day 10 VFX / Day 11 Audio / Ship docs)
and Phase 1/Phase 2-first-slice are fully shipped and merged — see git history for those rows if needed.
**Ops constitution:** [`../PARALLEL_OPS.md`](../PARALLEL_OPS.md) · human-side playbook: [`../DIRECTING_AGENTS.md`](../DIRECTING_AGENTS.md)
**Contracts:** [`../contracts/CURRENT.md`](../contracts/CURRENT.md)
**Human rollup:** [`../DRAFT_HANDOFF.md`](../DRAFT_HANDOFF.md)

## Capacity

Integrator + up to **2** coding workers — **both in use this wave**. `feat/env-lookfeel-overhaul` and
`feat/ui-component-system` spun 2026-08-09. Check `git worktree list` and `DRAFT_HANDOFF.md`'s top section
before assuming this table is current.

## Active agents / worktrees

| Dept | Branch | Status file | Notes |
|------|--------|--------------|-------|
| Presentation (environment/art) | `feat/env-lookfeel-overhaul` | `logiCard-env-lookfeel` worktree (no dept STATUS.md — same ad-hoc pattern as Phase 1/2's worker slices) | Sky/weather + lighting mood pass (checkpointed), then environment detail/door models/character rework, toward the **C53** reference (`image.png`). Brief: `ENV_LOOKFEEL_AGENT_BRIEF.md` at the worktree root. |
| Presentation (UI) | `feat/ui-component-system` | `logiCard-ui-components` worktree | Shared UI factory extraction, real layout/readability pass, new dialog/selection components, missing Adrenaline button. Brief: `UI_COMPONENT_SYSTEM_AGENT_BRIEF.md` at the worktree root. |

## Ownership matrix (write locks)

Wave-specific rows on top of the evergreen ones below:

| Path / concern | Owner now |
|----------------|-----------|
| `Assets/_Project/Board/**`, `PrimitiveMaterialFactory.cs`, `Assets/_Project/Art/**` | Presentation (`feat/env-lookfeel-overhaul`) |
| `Assets/_Project/UI/**` | Presentation (`feat/ui-component-system`) |
| `Assets/_Project/Boot/GameBootstrap.cs`'s camera viewport-rect line (`cam.rect`) | Integrator — coupled to the UI worker's dock-width decisions, same pattern as Phase 1 |
| `Boot/`, `Net/`, `Timeline/`, `Sim/` (fixes) | Core — paused this wave, not touched unless something breaks |
| `docs/DRAFT_HANDOFF.md`, `docs/SCHEDULE.md` ticks, `docs/contracts/CURRENT.md`, `docs/PRODUCT_MEMORY.md` | Core / Integrator |
| `docs/departments/<dept>/STATUS.md` | That dept only |

## Cross-review checklist (session start)

- [ ] Read DRAFT_HANDOFF
- [ ] Read this INDEX
- [ ] Read peer STATUS for every **In progress** row above
- [ ] Read contracts/CURRENT
- [ ] Confirm no file overlap before editing
