# Departments — Active Index

**Updated:** 2026-08-10 — vibrancy pass + map continuation kicked off (human decision, next
`PRODUCT_MEMORY.md` C-row). Human pushed back on the current desaturated "wet-dusk" look and asked for a
recolor pass back toward a vibrant, Link's-Awakening-punchy palette (amends **C53**'s color choice, does
not revert its mesh/detail work), denser/more-cartoon clouds, floor grid-line cleanup, and the map-select
UI the prior map-roster plan deferred. Both worker slots in use.
**Ops constitution:** [`../PARALLEL_OPS.md`](../PARALLEL_OPS.md) · human-side playbook: [`../DIRECTING_AGENTS.md`](../DIRECTING_AGENTS.md)
**Contracts:** [`../contracts/CURRENT.md`](../contracts/CURRENT.md)
**Human rollup:** [`../DRAFT_HANDOFF.md`](../DRAFT_HANDOFF.md)

## Capacity

Integrator + up to **2** coding workers — **2 of 2 in use.** Check `git worktree list` and
`DRAFT_HANDOFF.md`'s top section before assuming this table is current.

## Active agents / worktrees

| Dept | Branch | Status file | Notes |
|------|--------|--------------|-------|
| Vibrancy pass | `feat/vibrancy-pass` | `logiCard-vibrancy-pass` worktree | Post-processing color grade retint, `BoardSurfaceMaterials.cs` retint, denser/cartoon-style clouds. Brief: `VIBRANCY_AGENT_BRIEF.md` at the worktree root. |
| Map continuation | `feat/map-continuation` | `logiCard-map-continuation` worktree | Delete floor grid-line clutter (`BoardView.PlacePaintedGrid`), build a local-only map-select UI replacing the hardcoded `ActiveMap` constant. Brief: `MAP_CONTINUATION_AGENT_BRIEF.md` at the worktree root. |

Two prior finished worktrees (`logiCard-env-lookfeel`, `logiCard-ui-dock-polish` — directory names predate
this wave) still exist on disk as empty shells; pending cleanup, blocked on a transient file lock
(OneDrive/Search Indexer), not a git or project-state issue.

## Ownership matrix (write locks)

Wave-specific rows on top of the evergreen ones below:

| Path / concern | Owner now |
|----------------|-----------|
| `Assets/_Project/Art/Editor/UrpPostProcessingBootstrap.cs`, `Assets/_Project/Board/BoardSurfaceMaterials.cs`, `Assets/_Project/Board/BoardWeatherPocket.cs`, `Assets/_Project/Art/Environment/THIRD_PARTY.md` | Vibrancy pass (`feat/vibrancy-pass`) |
| `Assets/_Project/Board/BoardView.cs` (grid-line removal only), `Assets/_Project/UI/**`, minimal `GameBootstrap.cs` change to accept a runtime `MapId` | Map continuation (`feat/map-continuation`) |
| `Assets/_Project/Art/Characters/**`, `Assets/_Project/Art/URP/**`, `Assets/_Project/Rendering/**`, `PrimitiveMaterialFactory.cs` | Open, no worker assigned this pass |
| `GameBootstrap.BuildBoard(MapId)`'s switch, `BuildPawns()`, `BuildDefenderPayload()`, `MapDefinitions.ForId`'s switch, per-map geometry methods, `Assets/_Project/Board/BoardCameraRig.cs` | Integrator-only edit target — the shared map-dispatch wiring point; not re-opened by either worker this wave |
| `Boot/`, `Net/`, `Timeline/`, `Sim/` (fixes) | Core — paused this wave, not touched unless something breaks |
| `docs/DRAFT_HANDOFF.md`, `docs/SCHEDULE.md` ticks, `docs/contracts/CURRENT.md`, `docs/PRODUCT_MEMORY.md` | Core / Integrator |
| `docs/departments/<dept>/STATUS.md` | That dept only |

## Cross-review checklist (session start)

- [ ] Read DRAFT_HANDOFF
- [ ] Read this INDEX
- [ ] Read peer STATUS for every **In progress** row above
- [ ] Read contracts/CURRENT
- [ ] Confirm no file overlap before editing
