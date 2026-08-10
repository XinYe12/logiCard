# Departments — Active Index

**Updated:** 2026-08-10 — human reviewed a real screenshot and called out that reflections show no visible
change and clouds are still the most obviously fake thing on screen. Integrator root-caused the reflection
issue directly (probe `clearFlags` was never set, so probes rendered a mismatched/undefined environment
instead of the actual dark-void background — fixed, awaiting fresh screenshot). One worker spun for real
cloud/weather models (still primitive tinted spheres, explicitly marked temporary since Day 8). Core-
gameplay work stays paused.
**Ops constitution:** [`../PARALLEL_OPS.md`](../PARALLEL_OPS.md) · human-side playbook: [`../DIRECTING_AGENTS.md`](../DIRECTING_AGENTS.md)
**Contracts:** [`../contracts/CURRENT.md`](../contracts/CURRENT.md)
**Human rollup:** [`../DRAFT_HANDOFF.md`](../DRAFT_HANDOFF.md)

## Capacity

Integrator + up to **2** coding workers — **1 of 2 in use** (Integrator doing the reflection fix directly
on the main tree instead of delegating it, per the human's explicit choice). Check `git worktree list` and
`DRAFT_HANDOFF.md`'s top section before assuming this table is current.

## Active agents / worktrees

| Dept | Branch | Status file | Notes |
|------|--------|--------------|-------|
| Presentation (weather) | `feat/real-cloud-models` | `logiCard-ui-dock-polish` worktree (directory name stale) | Replace the flat tinted-sphere cloud placeholder with real textured/volumetric-reading clouds. Rain stays untouched. Brief: `REAL_CLOUD_MODELS_AGENT_BRIEF.md` at the worktree root. |
| Rendering | `feat/wet-surface-reflections` | `logiCard-ui-dock-polish` worktree (directory name stale) | Reflection probes for wet board surfaces (SSR unavailable in this URP version); retune `BoardSurfaceMaterials`' wetness once real reflection exists. Brief: `WET_SURFACE_REFLECTIONS_AGENT_BRIEF.md` at the worktree root. |

## Ownership matrix (write locks)

Wave-specific rows on top of the evergreen ones below:

| Path / concern | Owner now |
|----------------|-----------|
| `Assets/_Project/Art/Characters/**`, `Assets/_Project/Board/**`, `Assets/_Project/Art/URP/**`, `Assets/_Project/Rendering/**`, `PrimitiveMaterialFactory.cs`, `Assets/_Project/Art/Environment/**` | Closed out — both slots merged, open for the next assignment |
| `Assets/_Project/UI/**` | Closed out — `feat/ui-dock-polish` merged, open for the next assignment |
| `Assets/_Project/Boot/GameBootstrap.cs`, `Assets/_Project/Board/BoardCameraRig.cs` | Integrator-only edit target |
| `Boot/`, `Net/`, `Timeline/`, `Sim/` (fixes) | Core — paused this wave, not touched unless something breaks |
| `docs/DRAFT_HANDOFF.md`, `docs/SCHEDULE.md` ticks, `docs/contracts/CURRENT.md`, `docs/PRODUCT_MEMORY.md` | Core / Integrator |
| `docs/departments/<dept>/STATUS.md` | That dept only |

## Cross-review checklist (session start)

- [ ] Read DRAFT_HANDOFF
- [ ] Read this INDEX
- [ ] Read peer STATUS for every **In progress** row above
- [ ] Read contracts/CURRENT
- [ ] Confirm no file overlap before editing
