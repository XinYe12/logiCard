# Departments — Active Index

**Updated:** 2026-08-10 — both worker slots in use again. `feat/ui-dock-polish` spun for the new bottom-dock
layout + camera-rotation hint, never verified since landing. HUD dock moved right-edge → bottom band and
camera gained smooth right-drag rotation, both direct playtest feedback, both done by the Integrator directly
in the main tree (Boot/Board-owned). Core-gameplay work stays paused. Wave 1+2 (Day 10 VFX / Day 11 Audio /
Ship docs) and Phase 1/Phase 2-first-slice are fully shipped and merged — see git history for those rows if
needed.
**Ops constitution:** [`../PARALLEL_OPS.md`](../PARALLEL_OPS.md) · human-side playbook: [`../DIRECTING_AGENTS.md`](../DIRECTING_AGENTS.md)
**Contracts:** [`../contracts/CURRENT.md`](../contracts/CURRENT.md)
**Human rollup:** [`../DRAFT_HANDOFF.md`](../DRAFT_HANDOFF.md)

## Capacity

Integrator + up to **2** coding workers — **2 of 2 in use**. `feat/env-lookfeel-overhaul` active on checkpoint
2. `feat/ui-dock-polish` spun 2026-08-10 for a readability pass on the new bottom-dock layout. Check `git
worktree list` and `DRAFT_HANDOFF.md`'s top section before assuming this table is current.

## Active agents / worktrees

| Dept | Branch | Status file | Notes |
|------|--------|--------------|-------|
| Presentation (environment/art) | `feat/env-lookfeel-overhaul` | `logiCard-env-lookfeel` worktree (no dept STATUS.md — same ad-hoc pattern as Phase 1/2's worker slices) | Checkpoint 1 merged. Now on checkpoint 2: environment/prop asset pack sourcing, door models, character rework — proceeding on an inferred "more richness, not less" direction (human said "still bad, continue," didn't answer the hero-shot-vs-readability question explicitly). Brief: `ENV_LOOKFEEL_AGENT_BRIEF.md` at the worktree root. |
| Presentation (UI) | `feat/ui-dock-polish` | `logiCard-ui-dock-polish` worktree | Readability/polish pass on the new bottom-dock 3-column layout (never verified since it landed), Adrenaline primary-slot swap, Character Select grid, Quit confirm dialog. Brief: `UI_DOCK_POLISH_AGENT_BRIEF.md` at the worktree root. |

## Ownership matrix (write locks)

Wave-specific rows on top of the evergreen ones below:

| Path / concern | Owner now |
|----------------|-----------|
| `Assets/_Project/Board/**`, `PrimitiveMaterialFactory.cs`, `Assets/_Project/Art/**` | Presentation (`feat/env-lookfeel-overhaul`) |
| `Assets/_Project/UI/**` | Presentation (`feat/ui-dock-polish`) |
| `Assets/_Project/Boot/GameBootstrap.cs`, `Assets/_Project/Board/BoardCameraRig.cs` | Integrator-only edit target — actively worked on directly in the main tree |
| `Boot/`, `Net/`, `Timeline/`, `Sim/` (fixes) | Core — paused this wave, not touched unless something breaks |
| `docs/DRAFT_HANDOFF.md`, `docs/SCHEDULE.md` ticks, `docs/contracts/CURRENT.md`, `docs/PRODUCT_MEMORY.md` | Core / Integrator |
| `docs/departments/<dept>/STATUS.md` | That dept only |

## Cross-review checklist (session start)

- [ ] Read DRAFT_HANDOFF
- [ ] Read this INDEX
- [ ] Read peer STATUS for every **In progress** row above
- [ ] Read contracts/CURRENT
- [ ] Confirm no file overlap before editing
