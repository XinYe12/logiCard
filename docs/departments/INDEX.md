# Departments — Active Index

**Updated:** 2026-08-17 — **Match Shell Layout, Map, and Camera all merged. Dispatch round closed.**
Camera landed via human hands-on iteration during the actual re-test: control-hint overlay, then a
combined pan+rotate gesture (`169a55f`), then right-drag doing pitch tilt between top/front view
(`2e2d022`). Integrator re-ran batchmode fresh against each commit and again on `master` after
merging with `--no-ff` (`e594c51`) — **EditMode 188/188, PlayMode 59/59, both green.** No paused dept
work outstanding. Plan: [`../ui/MATCH_SHELL_LAYOUT.md`](../ui/MATCH_SHELL_LAYOUT.md).

**Ops constitution:** [`../PARALLEL_OPS.md`](../PARALLEL_OPS.md) · human-side playbook: [`../DIRECTING_AGENTS.md`](../DIRECTING_AGENTS.md)
**Contracts:** [`../contracts/CURRENT.md`](../contracts/CURRENT.md) · Playback: [`../core/PLAYBACK_CONTRACT.md`](../core/PLAYBACK_CONTRACT.md)
**Human rollup:** [`../DRAFT_HANDOFF.md`](../DRAFT_HANDOFF.md)

## Capacity

**Permanent seats:** 5 + Integrator. **Coding-hot:** none — dispatch round closed, all seats idle unless restaffed.

## Live folders ↔ seats

| Seat | Canonical | **Live folder now** | Tip / state | STATUS |
|------|-----------|---------------------|-------------|--------|
| **Integrator** | `logiCard` | `D:\projects\Game\logiCard` | `master` @ `e594c51` — Match Shell + Map + Camera merged | [`core/STATUS.md`](core/STATUS.md) |
| **Atmosphere** | `logiCard-atmosphere` | `logiCard-atmosphere-stylized` | Docs contribution merged; worktree still holds its own uncommitted, explicitly-parked Sunny-mood code; branch pushed to origin for safekeeping | [`atmosphere/STATUS.md`](atmosphere/STATUS.md) |
| **Cards** | `logiCard-cards` | `logiCard-cards-collection` | Docs contribution merged; idle; branch pushed to origin (stale vs. master, needs reconciliation before it can merge) | [`cards/STATUS.md`](cards/STATUS.md) |
| **Character** | `logiCard-character` | `logiCard-char-select-motion` | Docs + decision sheet merged; worktree still holds its own older, larger, unmerged Char Select carousel feature (separate workstream); branch pushed to origin for safekeeping | [`character/STATUS.md`](character/STATUS.md) |
| **UI** | `logiCard-ui` | `logiCard-modal-restyle` | Fully merged to master (`e1c80fb`); idle | [`ui/STATUS.md`](ui/STATUS.md) |
| **Map** | `logiCard-map` | `D:\projects\Game\logiCard-map` | **Fully merged to master** (`07501d7`); idle | [`map/STATUS.md`](map/STATUS.md) |
| **Camera** | — | `logiCard-camera-control` | `2e2d022` — **fully merged to master** (`e594c51`); idle | — |

## Ownership matrix (write locks)

| Path / concern | Owner now |
|----------------|-----------|
| `docs/ui/MATCH_SHELL_LAYOUT.md`, `docs/contracts/CURRENT.md`, `DRAFT_HANDOFF`, INDEX | **Integrator** |
| `ProgramHud` / `GearHandView` / match shell bands / `UiStyle` dock tokens | **Integrator** (merged; UI idle unless restaffed) |
| `Assets/_Project/Board/BoardSurfaceMaterials.cs`, `BoardView.cs` fence code | **Integrator** (Map merged; idle unless restaffed) |
| `BoardCameraRig` / `GameBootstrap.ConfigureCamera` | **Integrator** (Camera merged; idle unless restaffed) |
| Sim/Net/Timeline resolve | **Integrator** — frozen; UI calls only |

## Integrator merge gates

1. ~~Storm / Bandage HUD / hand-deck~~ — merged.
2. ~~HUD Chrome alone~~ — absorbed by Match Shell.
3. ~~Match Shell Layout (UI)~~ — **merged `c9925b1`**, human Play-signed, docs peers folded in `a21b29c`, batchmode green (174/174, 56/56).
4. ~~Map fence-shadow + material tweaks~~ — **merged `07501d7`**, human-approved, batchmode green (174/174, 56/56).
5. ~~Camera~~ — **merged `e594c51`** (`--no-ff`), human-tested and hands-on iterated live during re-test (combined pan+rotate, then pitch-tilt right-drag), batchmode green on master post-merge (188/188, 59/59).
6. ~~Healed presenter~~ — **landed on `master` directly (2026-08-18, no dispatch needed)** — one-shot banner only (`RoundPlayback.Report`); no board-splat leg, since `Healed` can only ever clear a wound carried in from a prior round, and this round's own wound splats are built only from this round's own Wounded/Killed events (see `PLAYBACK_CONTRACT.md` §3). Batchmode green: EditMode 188/188, PlayMode 60/60.
7. ~~Storm per-match counter~~ — **landed on `master` directly (2026-08-18, no dispatch needed)** — `RoundPlayback.StormCastCountOf` + `GhostResolver`-side enforcement, mirroring Bandage's `BandageChargeOf`/charge-gate shape exactly; `RegisterMatchState` grew a third delegate. Closes the deviation flagged in C69/`contracts/CURRENT.md`'s Storm contract.
8. Atmosphere Sunny decision (parked, not scheduled) — unchanged backlog. This dispatch round is closed — no coding-hot seats remain.
