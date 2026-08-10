# Departments — Active Index

**Updated:** 2026-08-10 — both real subagents finished and merged: reflection probes for wet floors
(`feat/wet-surface-reflections`, `a531e90`) and Scout's re-outfit (`feat/scout-reoutfit`, `d5ee45e`,
resolves **C56**). Both worker slots open again. Re-verified on the combined merge via a disposable
worktree — EditMode 124/124, PlayMode 37/37. Core-gameplay work stays paused. **Still needs a human sighted
pass** — neither change has been visually confirmed (no Editor/screenshot access in either agent session).
**Ops constitution:** [`../PARALLEL_OPS.md`](../PARALLEL_OPS.md) · human-side playbook: [`../DIRECTING_AGENTS.md`](../DIRECTING_AGENTS.md)
**Contracts:** [`../contracts/CURRENT.md`](../contracts/CURRENT.md)
**Human rollup:** [`../DRAFT_HANDOFF.md`](../DRAFT_HANDOFF.md)

## Capacity

Integrator + up to **2** coding workers — **0 of 2 in use.** Both worktrees merged and closed out. Check
`git worktree list` and `DRAFT_HANDOFF.md`'s top section before assuming this table is current.

## Active agents / worktrees

_(none active — both worker slots closed out this pass, worktrees not yet deleted)_
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
