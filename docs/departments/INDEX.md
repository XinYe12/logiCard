# Departments — Active Index

**Updated:** 2026-08-10 — both worker slots landed and merged: checkpoint 3 door/prop meshes (Quaternius
Ultimate House Interior Pack) and real URP post-processing (Volume Profile, SSAO, MSAA, soft shadows, plus
the C54 photo-mode stretch goal). Both worker slots now open. Core-gameplay work stays paused. Still
outstanding: human screenshot sign-off on both checkpoints, and a character-model (Quaternius pawns) review
that's never happened since the C53 pivot.
**Ops constitution:** [`../PARALLEL_OPS.md`](../PARALLEL_OPS.md) · human-side playbook: [`../DIRECTING_AGENTS.md`](../DIRECTING_AGENTS.md)
**Contracts:** [`../contracts/CURRENT.md`](../contracts/CURRENT.md)
**Human rollup:** [`../DRAFT_HANDOFF.md`](../DRAFT_HANDOFF.md)

## Capacity

Integrator + up to **2** coding workers — **0 of 2 in use**. Both worktrees merged and closed out this
pass. Check `git worktree list` and `DRAFT_HANDOFF.md`'s top section before assuming this table is current.

## Active agents / worktrees

_(none active — both worker slots closed out this pass, worktrees not yet deleted)_

## Ownership matrix (write locks)

Wave-specific rows on top of the evergreen ones below:

| Path / concern | Owner now |
|----------------|-----------|
| `Assets/_Project/Board/**`, `PrimitiveMaterialFactory.cs`, `Assets/_Project/Art/**` | Closed out — `feat/env-checkpoint3-doors` merged, open for the next assignment |
| `Assets/_Project/Rendering/**` | Closed out — `feat/urp-post-processing` merged, open for the next assignment |
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
