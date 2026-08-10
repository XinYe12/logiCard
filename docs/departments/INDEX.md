# Departments — Active Index

**Updated:** 2026-08-10 — `feat/ui-dock-polish` merged (ultrawide dock-overflow fix, dialog tightening);
`Assets/_Project/UI/**` closed out again, one worker slot open. `feat/env-lookfeel-overhaul` still active on
checkpoint 2 (Poly Haven board-surface textures landed, not yet merged; mesh-pack choice pending human
decision). Core-gameplay work stays paused. URP render pipeline audited 2026-08-10 — no post-processing
Volume Profile, no renderer features, MSAA off; flagged as a real, unaddressed gap (see DRAFT_HANDOFF). Wave
1+2 (Day 10 VFX / Day 11 Audio / Ship docs) and Phase 1/Phase 2-first-slice are fully shipped and merged —
see git history for those rows if needed.
**Ops constitution:** [`../PARALLEL_OPS.md`](../PARALLEL_OPS.md) · human-side playbook: [`../DIRECTING_AGENTS.md`](../DIRECTING_AGENTS.md)
**Contracts:** [`../contracts/CURRENT.md`](../contracts/CURRENT.md)
**Human rollup:** [`../DRAFT_HANDOFF.md`](../DRAFT_HANDOFF.md)

## Capacity

Integrator + up to **2** coding workers — **1 of 2 in use**. `feat/env-lookfeel-overhaul` active on checkpoint
2. One worker slot open — candidates: URP post-processing setup, or checkpoint 3 door models once the mesh
pack decision lands. Check `git worktree list` and `DRAFT_HANDOFF.md`'s top section before assuming this
table is current.

## Active agents / worktrees

| Dept | Branch | Status file | Notes |
|------|--------|--------------|-------|
| Presentation (environment/art) | `feat/env-lookfeel-overhaul` | `logiCard-env-lookfeel` worktree (no dept STATUS.md — same ad-hoc pattern as Phase 1/2's worker slices) | Checkpoint 1 merged. Checkpoint 2: Poly Haven CC0 PBR board-surface textures landed (`BoardSurfaceMaterials.cs`), not yet merged to master. Two mesh-pack candidates (Quaternius Ultimate House Interior vs. KayKit Dungeon Remastered) proposed for door models, awaiting human decision before checkpoint 3. Brief: `ENV_LOOKFEEL_AGENT_BRIEF.md` at the worktree root. |

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
