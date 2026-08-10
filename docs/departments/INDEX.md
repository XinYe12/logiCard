# Departments — Active Index

**Updated:** 2026-08-10 — environment checkpoint 1 merged; checkpoint 2 (asset packs/door models/character
rework) started on human go-ahead. HUD dock moved right-edge → bottom band, direct playtest feedback. Core-
gameplay work stays paused. Wave 1+2 (Day 10 VFX / Day 11 Audio / Ship docs) and Phase 1/Phase 2-first-slice
are fully shipped and merged — see git history for those rows if needed.
**Ops constitution:** [`../PARALLEL_OPS.md`](../PARALLEL_OPS.md) · human-side playbook: [`../DIRECTING_AGENTS.md`](../DIRECTING_AGENTS.md)
**Contracts:** [`../contracts/CURRENT.md`](../contracts/CURRENT.md)
**Human rollup:** [`../DRAFT_HANDOFF.md`](../DRAFT_HANDOFF.md)

## Capacity

Integrator + up to **2** coding workers — **1 of 2 in use**. `feat/env-lookfeel-overhaul` active on checkpoint
2, fast-forwarded onto current `master` (dock move + UI factory). One worker slot open. Check `git worktree
list` and `DRAFT_HANDOFF.md`'s top section before assuming this table is current.

## Active agents / worktrees

| Dept | Branch | Status file | Notes |
|------|--------|--------------|-------|
| Presentation (environment/art) | `feat/env-lookfeel-overhaul` | `logiCard-env-lookfeel` worktree (no dept STATUS.md — same ad-hoc pattern as Phase 1/2's worker slices) | Checkpoint 1 merged. Now on checkpoint 2: environment/prop asset pack sourcing, door models, character rework — proceeding on an inferred "more richness, not less" direction (human said "still bad, continue," didn't answer the hero-shot-vs-readability question explicitly). Brief: `ENV_LOOKFEEL_AGENT_BRIEF.md` at the worktree root. |

## Ownership matrix (write locks)

Wave-specific row on top of the evergreen ones below:

| Path / concern | Owner now |
|----------------|-----------|
| `Assets/_Project/Board/**`, `PrimitiveMaterialFactory.cs`, `Assets/_Project/Art/**` | Presentation (`feat/env-lookfeel-overhaul`) |
| `Assets/_Project/UI/**` | Closed out — `feat/ui-component-system` merged, open for the next assignment |
| `Assets/_Project/Boot/GameBootstrap.cs` | Integrator-only edit target; env worker may still need lighting/weather tuning wired in |
| `Boot/`, `Net/`, `Timeline/`, `Sim/` (fixes) | Core — paused this wave, not touched unless something breaks |
| `docs/DRAFT_HANDOFF.md`, `docs/SCHEDULE.md` ticks, `docs/contracts/CURRENT.md`, `docs/PRODUCT_MEMORY.md` | Core / Integrator |
| `docs/departments/<dept>/STATUS.md` | That dept only |

## Cross-review checklist (session start)

- [ ] Read DRAFT_HANDOFF
- [ ] Read this INDEX
- [ ] Read peer STATUS for every **In progress** row above
- [ ] Read contracts/CURRENT
- [ ] Confirm no file overlap before editing
