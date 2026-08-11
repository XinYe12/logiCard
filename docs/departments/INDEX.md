# Departments — Active Index

**Updated:** 2026-08-10 — vibrancy pass + map continuation landed and merged (`PRODUCT_MEMORY.md`
C58/C59). Recolor pass (post-processing grade, surface tints, denser clouds) and map-select UI + floor
grid-line removal + dialog restyle both delegated to worker worktrees, reviewed, batchmode-verified
(EditMode 124/124, PlayMode 37/37 combined), and merged to `master`. The map-continuation branch needed
real Integrator fixes before merge — see `DRAFT_HANDOFF.md`'s "continued 15" entry for the compile
error, two PlayMode regressions, and a shipped-build bug it caught. Both worker slots closed.
**Ops constitution:** [`../PARALLEL_OPS.md`](../PARALLEL_OPS.md) · human-side playbook: [`../DIRECTING_AGENTS.md`](../DIRECTING_AGENTS.md)
**Contracts:** [`../contracts/CURRENT.md`](../contracts/CURRENT.md)
**Human rollup:** [`../DRAFT_HANDOFF.md`](../DRAFT_HANDOFF.md)

## Capacity

Integrator + up to **2** coding workers — **1 of 2 in use.** Check `git worktree list` and
`DRAFT_HANDOFF.md`'s top section before assuming this table is current.

## Active agents / worktrees

`feat/asset-pack-audit` and `feat/interior-props-wiring` both merged clean — slots closed, worktrees
removed. Interior props: all 14 `Resources/Interior/*.prefab` names now sourced from `Assets/PolygonOffice/`
instead of Quaternius; window-glass transparency and the `Assets/PolygonOffice/**` read-only boundary
independently verified by the Integrator against the serialized assets before merge (see
`DRAFT_HANDOFF.md`).

- `logiCard-heist-character-swap` (branch `feat/heist-character-swap`, off `master`) — Phase 5 art-bar
  slice, **human-run in their own separate session, not Integrator-spawned.** Swaps Scout/Juggernaut's
  Quaternius placeholders for Heist's SWAT/Overall character prefabs — needs an outfit-isolation step
  since Heist's prefabs are a shared modular rig with every outfit variant nested in one file. Brief at
  worktree root (`HEIST_CHARACTER_SWAP_AGENT_BRIEF.md`). Owns `Assets/_Project/Editor/PawnImportTool.cs`
  + `Assets/_Project/Art/Characters/**` this wave.

Five now-finished worktrees exist on disk as empty shells pending cleanup (`logiCard-env-lookfeel`,
`logiCard-ui-dock-polish` from the map-roster wave; `logiCard-vibrancy-pass`, `logiCard-map-continuation`
from the vibrancy wave; `logiCard-art-pack-research` from the prior research pass) — all deregistered
from git cleanly, on-disk removal blocked on a transient file lock (OneDrive/Search Indexer), not a git
or project-state issue.

## Ownership matrix (write locks)

Wave-specific rows on top of the evergreen ones below:

| Path / concern | Owner now |
|----------------|-----------|
| `Assets/_Project/Art/Characters/**`, `Assets/_Project/Board/**`, `Assets/_Project/Art/URP/**`, `Assets/_Project/Rendering/**`, `PrimitiveMaterialFactory.cs`, `Assets/_Project/Art/Environment/**`, `Assets/_Project/UI/**` | Open, no worker assigned this pass |
| `GameBootstrap.BuildBoard(MapId)`'s switch, `BuildPawns()`, `BuildDefenderPayload()`, `MapDefinitions.ForId`'s switch, per-map geometry methods, `Assets/_Project/Board/BoardCameraRig.cs` | Integrator-only edit target — the shared map-dispatch wiring point |
| `Boot/`, `Net/`, `Timeline/`, `Sim/` (fixes) | Core — paused this wave, not touched unless something breaks |
| `docs/DRAFT_HANDOFF.md`, `docs/SCHEDULE.md` ticks, `docs/contracts/CURRENT.md`, `docs/PRODUCT_MEMORY.md` | Core / Integrator |
| `docs/departments/<dept>/STATUS.md` | That dept only |

## Cross-review checklist (session start)

- [ ] Read DRAFT_HANDOFF
- [ ] Read this INDEX
- [ ] Read peer STATUS for every **In progress** row above
- [ ] Read contracts/CURRENT
- [ ] Confirm no file overlap before editing
