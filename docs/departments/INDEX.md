# Departments — Active Index

**Updated:** 2026-08-10 — human resolved both open C53 questions (**C54**): mesh pack = Quaternius Ultimate
House Interior Pack; live camera stays readability-first, with a separate photo-mode as a new stretch goal.
Both worker slots re-spun on fresh branches off current `master`: checkpoint 3 (door/prop meshes) and URP
post-processing setup (Volume Profile + renderer features — the audited render-pipeline gap). Core-gameplay
work stays paused.
**Ops constitution:** [`../PARALLEL_OPS.md`](../PARALLEL_OPS.md) · human-side playbook: [`../DIRECTING_AGENTS.md`](../DIRECTING_AGENTS.md)
**Contracts:** [`../contracts/CURRENT.md`](../contracts/CURRENT.md)
**Human rollup:** [`../DRAFT_HANDOFF.md`](../DRAFT_HANDOFF.md)

## Capacity

Integrator + up to **2** coding workers — **2 of 2 in use**. Check `git worktree list` and
`DRAFT_HANDOFF.md`'s top section before assuming this table is current.

## Active agents / worktrees

| Dept | Branch | Status file | Notes |
|------|--------|--------------|-------|
| Presentation (environment/art) | `feat/env-checkpoint3-doors` | `logiCard-env-lookfeel` worktree | Checkpoint 3: import Quaternius Ultimate House Interior Pack (C54), replace tinted-box door placeholders with real meshes. Brief: `ENV_CHECKPOINT3_AGENT_BRIEF.md` at the worktree root. |
| Rendering | `feat/urp-post-processing` | `logiCard-ui-dock-polish` worktree (directory name stale, reused from the finished UI pass) | Configure the URP asset for real: Volume Profile (bloom/color grade/vignette/tonemap), renderer features (SSAO, SSR if feasible), MSAA, shadow quality. Stretch: photo-mode capability (C54). Brief: `URP_POSTPROCESSING_AGENT_BRIEF.md` at the worktree root. |

## Ownership matrix (write locks)

Wave-specific rows on top of the evergreen ones below:

| Path / concern | Owner now |
|----------------|-----------|
| `Assets/_Project/Board/**`, `PrimitiveMaterialFactory.cs`, `Assets/_Project/Art/**` except `Art/URP/**` | Presentation (`feat/env-checkpoint3-doors`) |
| `Assets/_Project/Art/URP/**`, new `Assets/_Project/Rendering/**` | Rendering (`feat/urp-post-processing`) |
| `Assets/_Project/UI/**` | Closed out — `feat/ui-dock-polish` merged, open for the next assignment |
| `Assets/_Project/Boot/GameBootstrap.cs`, `Assets/_Project/Board/BoardCameraRig.cs` | Integrator-only edit target — one scoped exception granted to `feat/urp-post-processing` for a single component-add line, see its brief |
| `Boot/`, `Net/`, `Timeline/`, `Sim/` (fixes) | Core — paused this wave, not touched unless something breaks |
| `docs/DRAFT_HANDOFF.md`, `docs/SCHEDULE.md` ticks, `docs/contracts/CURRENT.md`, `docs/PRODUCT_MEMORY.md` | Core / Integrator |
| `docs/departments/<dept>/STATUS.md` | That dept only |

## Cross-review checklist (session start)

- [ ] Read DRAFT_HANDOFF
- [ ] Read this INDEX
- [ ] Read peer STATUS for every **In progress** row above
- [ ] Read contracts/CURRENT
- [ ] Confirm no file overlap before editing
