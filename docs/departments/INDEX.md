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

Integrator + up to **2** coding workers — **2 of 2 in use.** Check `git worktree list` and
`DRAFT_HANDOFF.md`'s top section before assuming this table is current. (`logiCard-art-pack-research`
below runs in the human's own separate session and isn't counted against this cap.)

## Active agents / worktrees

`feat/nappin-interior-wiring`, `feat/weather-fx-wiring`, `feat/void-city-dressing`, and
`feat/character-pack-swap` all merged clean. Rain then needed **three more rounds of direct Integrator
fixes** after real human screenshots (speed/lifetime, then renderer lengthScale/color, then a Cone-shape
emission-direction bug — "looks horizontal") — see `DRAFT_HANDOFF.md` for the full account, including the
`feat/character-pack-swap` two-agent-collision incident and the recurring `Glass_URP.mat` regression that's
now permanently fixed at its root.

Two new slices open, in response to direct human feedback ("lighting/floor/materials look bad, needs to be
vibrant" + "implement camera zoom"), both forked from `master` @ `7b07ab3`:

- **`logiCard-vibrancy-relight`** (branch `feat/vibrancy-relight`) — rebalance `GameBootstrap.cs`'s post-
  process grade (`BuildDioramaVolume`, currently `saturation: -4` + a cool blue `colorFilter` — the
  concrete cause of "not vibrant") and lighting (`BuildLighting`), plus `BoardSurfaceMaterials.cs`'s
  remaining dark/desaturated tints. Brief at worktree root (`VIBRANCY_RELIGHT_AGENT_BRIEF.md`). Owns
  `GameBootstrap.cs`'s `ConfigureCamera`/`BuildLighting`/`BuildDioramaVolume` methods (not the map-dispatch
  switches) + `BoardSurfaceMaterials.cs`.
- **`logiCard-camera-zoom`** (branch `feat/camera-zoom`) — add scroll-wheel zoom to `BoardCameraRig.cs`
  (orthographic camera, so `orthographicSize` is the only real lever), with analytically-derived min/max
  bounds (same projection-math discipline as the existing `orthographicSize = 5.0` calibration) and board-
  anchored-UI projection invalidation handled (per `docs/UI_BOARD_ANCHORED_COMPONENTS.md`, mandatory
  reading before this kind of change). Brief at worktree root (`CAMERA_ZOOM_AGENT_BRIEF.md`). Owns
  `Assets/_Project/Board/BoardCameraRig.cs` only — **no overlap with the vibrancy slot** despite both
  touching camera-adjacent territory (one's in `GameBootstrap.cs`, one's in `BoardCameraRig.cs`).

`logiCard-void-city-dressing` and `logiCard-character-pack-swap`'s old worktree directories are
deregistered from git but wouldn't delete on disk (same transient OneDrive/Search-Indexer lock class
documented elsewhere in this file) — harmless empty shells, safe to delete by hand whenever the lock clears.

- **`logiCard-art-pack-research` (branch `feat/art-pack-research`) — human-run, active.** Produced the
  current `docs/ART_PACK_RESEARCH.md`; Integrator pulls its content into `master` directly rather than
  merging the branch, so it may be redundant — human's call whether to keep it running.

**`Assets/ExplosiveLLC/` blocker — resolved.** Fixed both compile errors (added
`com.unity.modules.terrainphysics` to the package manifest; removed an invalid `[SerializeField]` on a
struct type in `SuperCharacterController.cs`, a pure error fix with no behavior change). Combined
batchmode on `master` now passes: EditMode 124/124, PlayMode 37/37 — first clean full run since the Synty
deletion. `ExplosiveLLC` itself is still untracked/uncommitted (this fix unblocks batchmode, it doesn't
adopt the folder) — its origin/purpose is still unexplained, human's call whether to keep or remove it.

Three other now-finished worktrees remain on disk as empty shells pending cleanup (`logiCard-env-lookfeel`,
`logiCard-ui-dock-polish` from the map-roster wave; `logiCard-vibrancy-pass`/`logiCard-map-continuation`
from the vibrancy wave) — deregistered from git cleanly, on-disk removal blocked on a transient file lock
(OneDrive/Search Indexer), not a git or project-state issue.

## Ownership matrix (write locks)

Wave-specific rows on top of the evergreen ones below:

| Path / concern | Owner now |
|----------------|-----------|
| `Assets/_Project/Art/Characters/**`, `Assets/_Project/Art/URP/**`, `Assets/_Project/Rendering/**`, `PrimitiveMaterialFactory.cs`, `Assets/_Project/UI/**` | Open, no worker assigned this pass |
| `GameBootstrap.BuildBoard(MapId)`'s switch, `BuildPawns()`, `BuildDefenderPayload()`, `MapDefinitions.ForId`'s switch, per-map geometry methods | Integrator-only edit target — the shared map-dispatch wiring point |
| `GameBootstrap.cs`'s `ConfigureCamera`/`BuildLighting`/`BuildDioramaVolume`, `BoardSurfaceMaterials.cs` | `logiCard-vibrancy-relight` this wave |
| `Assets/_Project/Board/BoardCameraRig.cs` | `logiCard-camera-zoom` this wave (was Integrator-only in an earlier map-roster wave; that restriction doesn't apply to this wave) |
| `Assets/_Project/Board/BoardWeatherPocket.cs` | Just landed a real fix (`7b07ab3`), leave alone this wave — not open for either worker above |
| `Boot/`, `Net/`, `Timeline/`, `Sim/` (fixes) | Core — paused this wave, not touched unless something breaks |
| `docs/DRAFT_HANDOFF.md`, `docs/SCHEDULE.md` ticks, `docs/contracts/CURRENT.md`, `docs/PRODUCT_MEMORY.md` | Core / Integrator |
| `docs/departments/<dept>/STATUS.md` | That dept only |

## Cross-review checklist (session start)

- [ ] Read DRAFT_HANDOFF
- [ ] Read this INDEX
- [ ] Read peer STATUS for every **In progress** row above
- [ ] Read contracts/CURRENT
- [ ] Confirm no file overlap before editing
