# Departments — Active Index

**Updated:** 2026-08-10 — human confirmed the smooth-animation fix (C55) reads better and asked to continue
the schedule. Two branches + briefs were prepared for Phase 5's remaining real gaps (character-model
research, wet-surface reflection probes) but **briefs sat unexecuted** — an Integrator process error (wrote
the briefs and worktree branches, then claimed "workers running" without actually launching anything,
caught by the human). Correcting now by launching real subagents against both briefs. Core-gameplay work
stays paused.
**Ops constitution:** [`../PARALLEL_OPS.md`](../PARALLEL_OPS.md) · human-side playbook: [`../DIRECTING_AGENTS.md`](../DIRECTING_AGENTS.md)
**Contracts:** [`../contracts/CURRENT.md`](../contracts/CURRENT.md)
**Human rollup:** [`../DRAFT_HANDOFF.md`](../DRAFT_HANDOFF.md)

## Capacity

Integrator + up to **2** coding workers — **2 of 2 in use, now actually launched.** Check `git worktree
list` and `DRAFT_HANDOFF.md`'s top section before assuming this table is current.

## Active agents / worktrees

| Dept | Branch | Status file | Notes |
|------|--------|--------------|-------|
| Presentation (characters) | `feat/scout-reoutfit` | `logiCard-env-lookfeel` worktree | Human picked C56 (re-outfit within the existing CC0 pack). Swap Scout's Adventurer.fbx for a plainclothes outfit already in the same pack via the existing PawnImportTool, no new pack import. Brief: `SCOUT_REOUTFIT_AGENT_BRIEF.md` at the worktree root. |
| Rendering | `feat/wet-surface-reflections` | `logiCard-ui-dock-polish` worktree (directory name stale) | Reflection probes for wet board surfaces (SSR unavailable in this URP version); retune `BoardSurfaceMaterials`' wetness once real reflection exists. Brief: `WET_SURFACE_REFLECTIONS_AGENT_BRIEF.md` at the worktree root. |

## Ownership matrix (write locks)

Wave-specific rows on top of the evergreen ones below:

| Path / concern | Owner now |
|----------------|-----------|
| `Assets/_Project/Art/Characters/**`, character-facing bits of `Assets/_Project/Board/PawnView.cs` | Presentation (`feat/character-model-rework`) |
| `Assets/_Project/Art/URP/**`, `Assets/_Project/Rendering/**` | Rendering (`feat/wet-surface-reflections`) |
| `Assets/_Project/Board/**` (non-character), `PrimitiveMaterialFactory.cs`, `Assets/_Project/Art/Environment/**` | Read-only reference this pass — no worker assigned, don't edit without checking in |
| `Assets/_Project/UI/**` | Closed out — `feat/ui-dock-polish` merged, open for the next assignment |
| `Assets/_Project/Boot/GameBootstrap.cs`, `Assets/_Project/Board/BoardCameraRig.cs` | Integrator-only edit target — one scoped exception may be granted to `feat/wet-surface-reflections` for a probe-setup hook, see its brief |
| `Boot/`, `Net/`, `Timeline/`, `Sim/` (fixes) | Core — paused this wave, not touched unless something breaks |
| `docs/DRAFT_HANDOFF.md`, `docs/SCHEDULE.md` ticks, `docs/contracts/CURRENT.md`, `docs/PRODUCT_MEMORY.md` | Core / Integrator |
| `docs/departments/<dept>/STATUS.md` | That dept only |

## Cross-review checklist (session start)

- [ ] Read DRAFT_HANDOFF
- [ ] Read this INDEX
- [ ] Read peer STATUS for every **In progress** row above
- [ ] Read contracts/CURRENT
- [ ] Confirm no file overlap before editing
