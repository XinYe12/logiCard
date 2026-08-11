# Departments — Active Index

**Updated:** 2026-08-11 — vibrancy (C60) + camera zoom (C61) merged after Integrator resume from a
prior-session limit stall. Both worker slots closed. Rain horizontal-direction fix (`7b07ab3`) was
already on master before the resume.
**Ops constitution:** [`../PARALLEL_OPS.md`](../PARALLEL_OPS.md) · human-side playbook: [`../DIRECTING_AGENTS.md`](../DIRECTING_AGENTS.md)
**Contracts:** [`../contracts/CURRENT.md`](../contracts/CURRENT.md)
**Human rollup:** [`../DRAFT_HANDOFF.md`](../DRAFT_HANDOFF.md)

## Capacity

Integrator + up to **2** coding workers — **0 of 2 in use.** Check `git worktree list` and
`DRAFT_HANDOFF.md`'s top section before assuming this table is current. (`logiCard-art-pack-research`
below runs in the human's own separate session and isn't counted against this cap.)

## Active agents / worktrees

`feat/vibrancy-relight` and `feat/camera-zoom` **merged** (`301df0d`, `535bfaa`) after Integrator
review + worktree batchmode. See `DRAFT_HANDOFF.md` top entry and `PRODUCT_MEMORY.md` C60/C61.

- **`logiCard-art-pack-research` (branch `feat/art-pack-research`) — human-run, active.** Produced the
  current `docs/ART_PACK_RESEARCH.md`; Integrator pulls its content into `master` directly rather than
  merging the branch, so it may be redundant — human's call whether to keep it running.

**`Assets/ExplosiveLLC/` blocker — resolved earlier; folder still untracked.** Combined batchmode on
`master` previously passed with it present as a compile-unblock. Origin/purpose still unexplained —
human's call whether to keep or remove it.

Finished worktree directories that may still sit on disk as empty shells (OneDrive/Search-Indexer lock
class): `logiCard-vibrancy-relight`, `logiCard-camera-zoom`, plus older
`logiCard-void-city-dressing` / `logiCard-character-pack-swap` / `logiCard-env-lookfeel` /
`logiCard-ui-dock-polish` / `logiCard-vibrancy-pass` / `logiCard-map-continuation` — deregister with
`git worktree remove` when the lock clears; safe to delete by hand.

## Ownership matrix (write locks)

| Path / concern | Owner now |
|----------------|-----------|
| `Assets/_Project/Art/Characters/**`, `Assets/_Project/Art/URP/**`, `Assets/_Project/Rendering/**`, `PrimitiveMaterialFactory.cs`, `Assets/_Project/UI/**` | Open, no worker assigned |
| `GameBootstrap.cs` (including `ConfigureCamera` / `BuildLighting` / `BuildDioramaVolume` and map-dispatch) | Integrator — C60 just landed; no worker owns it |
| `Assets/_Project/Board/BoardCameraRig.cs` | Integrator — C61 just landed; no worker owns it |
| `Assets/_Project/Board/BoardSurfaceMaterials.cs`, `BoardView.cs`, `BoardReflectionProbes.cs` | Integrator — C60 just landed |
| `Assets/_Project/Board/BoardWeatherPocket.cs` | Integrator — rain velocity fix already on master; leave unless a new rain screenshot fails |
| `Boot/`, `Net/`, `Timeline/`, `Sim/` (fixes) | Core — paused unless something breaks |
| `docs/DRAFT_HANDOFF.md`, `docs/SCHEDULE.md` ticks, `docs/contracts/CURRENT.md`, `docs/PRODUCT_MEMORY.md` | Core / Integrator |
| `docs/departments/<dept>/STATUS.md` | That dept only |

## Cross-review checklist (session start)

- [ ] Read DRAFT_HANDOFF
- [ ] Read this INDEX
- [ ] Read peer STATUS for every **In progress** row above
- [ ] Read contracts/CURRENT
- [ ] Confirm no file overlap before editing
