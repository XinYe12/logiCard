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

`feat/asset-pack-audit`, `feat/interior-props-wiring` (PolygonOffice re-source, later superseded by the
Synty purge — pipeline now on Quaternius CC0), and `feat/heist-character-swap` (closed out unused, its
premise died with the Synty deletion) are all resolved and removed. Six free art packs landed 2026-08-11
(checkpoint `971985c`): nappin House Interior/Weapons, Cinematic Weather VFX (`RainSnowCloudEffect`), Zap
VFX, ithappy Creative Characters FREE, ithappy Cartoon City Free — see `docs/ART_PACK_RESEARCH.md`. Two
new worker slices wire them in:

- **`logiCard-nappin-interior-wiring`** (branch `feat/nappin-interior-wiring`, off `master` @ `971985c`)
  — re-source `InteriorPackImportTool.cs` + `Resources/Interior/*.prefab` from nappin
  (`OfficeEssentialsPack` + `HouseInteriorPack`) instead of Quaternius, same pattern as the earlier
  PolygonOffice re-source. Brief at worktree root (`NAPPIN_INTERIOR_WIRING_AGENT_BRIEF.md`). Owns
  `Assets/_Project/Art/Editor/InteriorPackImportTool.cs` +
  `Assets/_Project/Art/Environment/Resources/Interior/**` this wave.
- **`logiCard-weather-fx-wiring`** (branch `feat/weather-fx-wiring`, off `master` @ `971985c`) — replace
  `BoardWeatherPocket.cs`'s fully-procedural cloud/rain particle code with `RainSnowCloudEffect`'s real
  prefabs (fit to the board footprint, preserving the already-tuned "contained sky pocket, not looming"
  framing), add lightning via Zap VFX (new feature, nothing existing to replace). Brief at worktree root
  (`WEATHER_FX_WIRING_AGENT_BRIEF.md`). Owns `Assets/_Project/Board/BoardWeatherPocket.cs` this wave.

Not yet scoped this wave (queued for next): Characters wiring (`ithappy/Creative_Characters_FREE` into
`PawnImportTool.cs`/`PawnView.cs`) and exterior/city dressing (`ithappy/Cartoon_City_Free` as Yard/board-
edge backdrop) — no file overlap with either slot above, safe to pick up once a slot frees.

- **`logiCard-art-pack-research` (branch `feat/art-pack-research`) — human-run, active.** Produced the
  current `docs/ART_PACK_RESEARCH.md`; Integrator pulls its content into `master` directly rather than
  merging the branch, so it may be redundant — human's call whether to keep it running.

**Deliberately left uncommitted on `master` this wave:** `Assets/ExplosiveLLC/` (Warrior character packs
+ `SuperCharacterController`) — has real compile errors that abort Unity batchmode entirely, unresolved
origin/purpose, excluded from the checkpoint both new worktrees forked from so it can't break their
batchmode too.

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
