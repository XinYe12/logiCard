# Departments — Active Index

**Updated:** 2026-08-10 — `feat/env-lookfeel-overhaul` checkpoint 2 merged (real Poly Haven board-surface
textures); `feat/ui-dock-polish` merged (ultrawide dock-overflow fix, dialog tightening). Both worker slots
now open. Checkpoint 3 (door/prop meshes) and the camera hero-shot-vs-readability question are both blocked
on human decisions, not worker capacity — see `DRAFT_HANDOFF.md`. Core-gameplay work stays paused. URP render
pipeline audited 2026-08-10 — no post-processing Volume Profile, no renderer features, MSAA off; flagged as a
real, unaddressed gap. Wave 1+2 (Day 10 VFX / Day 11 Audio / Ship docs) and Phase 1/Phase 2-first-slice are
fully shipped and merged — see git history for those rows if needed.
**Ops constitution:** [`../PARALLEL_OPS.md`](../PARALLEL_OPS.md) · human-side playbook: [`../DIRECTING_AGENTS.md`](../DIRECTING_AGENTS.md)
**Contracts:** [`../contracts/CURRENT.md`](../contracts/CURRENT.md)
**Human rollup:** [`../DRAFT_HANDOFF.md`](../DRAFT_HANDOFF.md)

## Capacity

Integrator + up to **2** coding workers — **0 of 2 in use**. Both worktrees merged and closed out this pass.
Next assignments waiting on human input: checkpoint 3 door/prop models (needs mesh-pack choice) and URP
post-processing setup (no decision needed, could start any time). Check `git worktree list` and
`DRAFT_HANDOFF.md`'s top section before assuming this table is current.

## Active agents / worktrees

_(none active — both worker slots closed out this pass, worktrees not yet deleted; see Capacity above)_

## Ownership matrix (write locks)

Wave-specific rows on top of the evergreen ones below:

| Path / concern | Owner now |
|----------------|-----------|
| `Assets/_Project/Board/**`, `PrimitiveMaterialFactory.cs`, `Assets/_Project/Art/**` | Presentation (`feat/env-lookfeel-overhaul`) |
| `Assets/_Project/UI/**` | Closed out — `feat/ui-dock-polish` merged, open for the next assignment |
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
