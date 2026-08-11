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
removed. Interior props landed on PolygonOffice, then **superseded same day**: the human decided to
delete all three unlicensed Synty packs outright rather than carry the licensing TODO forward, so the
interior pipeline is now back on Quaternius CC0 (`master` @ `3e06126`) — see `DRAFT_HANDOFF.md`'s
"unlicensed Synty packs deleted" entry.

- **`logiCard-heist-character-swap` (branch `feat/heist-character-swap`) — brief now obsolete, not yet
  closed out.** Its whole premise (isolate SWAT/Overall renderers out of `Assets/PolygonHeist/`'s shared
  modular rig prefabs) no longer applies — that pack is deleted. No work had landed on the branch yet
  (still sitting at the pre-deletion merge commit, nothing lost). Needs a human call: repurpose this slot
  for a chubby/round character pack once one is purchased (see `ART_PACK_RESEARCH.md`'s Characters
  section — Kotangent Chubby / Toony Tiny candidates), or close the worktree out unused.
- **`logiCard-art-pack-research` (branch `feat/art-pack-research`) — human-run, active again**, not the
  empty finished shell it was before. Produced the current `docs/ART_PACK_RESEARCH.md` rewrite (session
  status, chubby-character shortlist, motion/animation strategy) — Integrator pulled that content
  directly into `master` (`403e9f8`) from the synced working-tree copy rather than merging this branch,
  so the branch itself may now be redundant; human's call whether to keep it running or fold it.

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
