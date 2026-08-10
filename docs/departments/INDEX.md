# Departments — Active Index

**Updated:** 2026-08-10 — three-map roster landed and wired. `Vent`/`Breach` door kinds, `MapId`/`MapLayout`
groundwork, Freight Yard retrofit, and both new maps (Rail Platform, Vault Complex) are all merged to
`master`. Integrator has resolved the two workers' merge conflicts and wired the shared dispatch
(`BuildBoard(MapId)`, `MapDefinitions.ForId`, map-aware `BuildPawns()`) so all three maps are selectable
via the `ActiveMap` constant (still defaulted to `FreightYard` — no map-select UI exists yet, out of
scope per the approved plan). Batchmode verification of the fully-wired state in progress. Both worker
slots now closed — 0 of 2 in use. This wave's lifted core-gameplay-paused rule (map/terrain work only)
stays in effect until PRODUCT_MEMORY C57 is recorded closing it out.
**Ops constitution:** [`../PARALLEL_OPS.md`](../PARALLEL_OPS.md) · human-side playbook: [`../DIRECTING_AGENTS.md`](../DIRECTING_AGENTS.md)
**Contracts:** [`../contracts/CURRENT.md`](../contracts/CURRENT.md)
**Human rollup:** [`../DRAFT_HANDOFF.md`](../DRAFT_HANDOFF.md)

## Capacity

Integrator + up to **2** coding workers — **0 of 2 in use.** Check `git worktree list` and
`DRAFT_HANDOFF.md`'s top section before assuming this table is current.

## Active agents / worktrees

None. Both map-design slots closed out — `feat/map-rail-platform` and `feat/map-vault-complex` are
merged into `master` (commits `6c384d2`, `59b9a4e`). Their worktrees (`logiCard-env-lookfeel`,
`logiCard-ui-dock-polish` — directory names predate this wave) still exist on disk with each worker's
now-obsolete brief files and stray untracked artifacts; pending cleanup, not yet removed.

## Ownership matrix (write locks)

Wave-specific rows on top of the evergreen ones below:

| Path / concern | Owner now |
|----------------|-----------|
| `Assets/_Project/Art/Characters/**`, `Assets/_Project/Board/**`, `Assets/_Project/Art/URP/**`, `Assets/_Project/Rendering/**`, `PrimitiveMaterialFactory.cs`, `Assets/_Project/Art/Environment/**`, `Assets/_Project/UI/**` | Open, no worker assigned this pass |
| `GameBootstrap.BuildBoard(MapId)`'s switch, `BuildPawns()`, `BuildDefenderPayload()`, `MapDefinitions.ForId`'s switch, `Assets/_Project/Board/BoardCameraRig.cs` | Integrator-only edit target — the shared map-dispatch wiring point; wired 2026-08-10, stays Integrator-only for future map additions |
| `Boot/`, `Net/`, `Timeline/`, `Sim/` (fixes) | Core — paused this wave, not touched unless something breaks |
| `docs/DRAFT_HANDOFF.md`, `docs/SCHEDULE.md` ticks, `docs/contracts/CURRENT.md`, `docs/PRODUCT_MEMORY.md` | Core / Integrator |
| `docs/departments/<dept>/STATUS.md` | That dept only |

## Cross-review checklist (session start)

- [ ] Read DRAFT_HANDOFF
- [ ] Read this INDEX
- [ ] Read peer STATUS for every **In progress** row above
- [ ] Read contracts/CURRENT
- [ ] Confirm no file overlap before editing
