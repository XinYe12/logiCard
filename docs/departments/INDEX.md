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

`feat/nappin-interior-wiring` and `feat/weather-fx-wiring` both merged clean — slots closed, worktrees
removed. See `DRAFT_HANDOFF.md`'s "nappin interior + weather VFX wiring merged" entry for what landed and
how it was verified (independently re-run batchmode in each worktree by the Integrator, not just taken on
the workers' reports). Two new slices open next:

- **`logiCard-character-pack-swap`** (branch `feat/character-pack-swap`, off `master` @ `23af934`) —
  assemble Scout/Juggernaut from `ithappy/Creative_Characters_FREE`'s modular parts (using the pack's own
  `CharacterCustomizationWindow` Editor tool), adapt `PawnImportTool.cs`, bake to the
  `Resources/<Scout|Juggernaut>` contract `PawnView.cs` already expects. Confirmed materials are already
  URP/Lit (this project's own shader GUID) — no conversion step needed, unlike every prior character/prop
  pack. Brief at worktree root (`CHARACTER_PACK_SWAP_AGENT_BRIEF.md`). Owns
  `Assets/_Project/Editor/PawnImportTool.cs` + `Assets/_Project/Art/Characters/**` this wave.
- **`logiCard-void-city-dressing`** (branch `feat/void-city-dressing`, off `master` @ `23af934`) — replace
  `BoardView.cs`'s `PlaceVoidDressing`/`PlaceVoidClutter` placeholder primitive cubes with real
  `ithappy/Cartoon_City_Free` prefabs (also already URP/Lit). Brief at worktree root
  (`VOID_CITY_DRESSING_AGENT_BRIEF.md`). Owns only `BoardView.cs`'s void-dressing methods this wave — no
  overlap with the character slot above.

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
