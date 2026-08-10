# Departments — Active Index

**Updated:** 2026-08-10 — three-map roster kicked off (human decision, next `PRODUCT_MEMORY.md` C-row).
`Vent`/`Breach` door kinds landed, `MapId`/`MapLayout` groundwork landed, Freight Yard retrofitted with
one of each — all Integrator-direct on `master`. Both worker slots back in use building the two new maps
(Rail Platform, Vault Complex) in parallel. This lifts the core-gameplay-paused rule for map/terrain work
specifically — Sim-layer `ArenaBoard`/`GameBootstrap` edits are in-bounds for this wave; Net/Timeline/other
Sim work stays paused.
**Ops constitution:** [`../PARALLEL_OPS.md`](../PARALLEL_OPS.md) · human-side playbook: [`../DIRECTING_AGENTS.md`](../DIRECTING_AGENTS.md)
**Contracts:** [`../contracts/CURRENT.md`](../contracts/CURRENT.md)
**Human rollup:** [`../DRAFT_HANDOFF.md`](../DRAFT_HANDOFF.md)

## Capacity

Integrator + up to **2** coding workers — **2 of 2 in use.** Check `git worktree list` and
`DRAFT_HANDOFF.md`'s top section before assuming this table is current.

## Active agents / worktrees

| Dept | Branch | Status file | Notes |
|------|--------|--------------|-------|
| Level design (map #2) | `feat/map-rail-platform` | `logiCard-env-lookfeel` worktree (directory name stale) | Long-sightline map: two platforms + a corridor, one Vent, one Breach. Self-contained new methods only — doesn't touch the shared `MapId`/dispatch switches. Brief: `MAP_RAIL_PLATFORM_AGENT_BRIEF.md` at the worktree root. |
| Level design (map #3) | `feat/map-vault-complex` | `logiCard-ui-dock-polish` worktree (directory name stale) | Dense maze map: 4-5 small rooms, 3-4 doors, one Vent, one Breach. Self-contained new methods only — doesn't touch the shared `MapId`/dispatch switches. Brief: `MAP_VAULT_COMPLEX_AGENT_BRIEF.md` at the worktree root. |

## Ownership matrix (write locks)

Wave-specific rows on top of the evergreen ones below:

| Path / concern | Owner now |
|----------------|-----------|
| `Assets/_Project/Art/Characters/**`, `Assets/_Project/Board/**` (non-map-dispatch), `Assets/_Project/Art/URP/**`, `Assets/_Project/Rendering/**`, `PrimitiveMaterialFactory.cs`, `Assets/_Project/Art/Environment/**` | Open, no worker assigned this pass |
| New self-contained methods in `GameBootstrap.cs`/`MapDefinitions.cs` (per-map geometry/AI, not the shared dispatch switches) | Level design (`feat/map-rail-platform`, `feat/map-vault-complex`) — no file overlap, each adds differently-named methods only |
| `Assets/_Project/UI/**` | Closed out — `feat/ui-dock-polish` merged, open for the next assignment |
| `GameBootstrap.BuildBoard(MapId)`'s switch, `BuildPawns()`, `BuildDefenderPayload()`, `MapDefinitions.ForId`'s switch, `Assets/_Project/Board/BoardCameraRig.cs` | Integrator-only edit target — the shared map-dispatch wiring point, deliberately not delegated to avoid a two-worker collision |
| `Boot/`, `Net/`, `Timeline/`, `Sim/` (fixes) | Core — paused this wave, not touched unless something breaks |
| `docs/DRAFT_HANDOFF.md`, `docs/SCHEDULE.md` ticks, `docs/contracts/CURRENT.md`, `docs/PRODUCT_MEMORY.md` | Core / Integrator |
| `docs/departments/<dept>/STATUS.md` | That dept only |

## Cross-review checklist (session start)

- [ ] Read DRAFT_HANDOFF
- [ ] Read this INDEX
- [ ] Read peer STATUS for every **In progress** row above
- [ ] Read contracts/CURRENT
- [ ] Confirm no file overlap before editing
