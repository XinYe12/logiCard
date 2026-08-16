# Departments — Active Index

**Updated:** 2026-08-16 — **Match Shell Layout wave: merged to master, human Play-signed.**
Five-band `ProgramHud` merged (`c9925b1`), docs peers folded in (`a21b29c`), post-merge batchmode
re-verified by Integrator: EditMode 174/174, PlayMode 56/56. First all-department collaboration
called a success. Camera freecam commit `2b06a3a` now unblocked — `ProgramHud.MapViewport` rect is
real; letterbox wiring is next. Plan: [`../ui/MATCH_SHELL_LAYOUT.md`](../ui/MATCH_SHELL_LAYOUT.md).

**Ops constitution:** [`../PARALLEL_OPS.md`](../PARALLEL_OPS.md) · human-side playbook: [`../DIRECTING_AGENTS.md`](../DIRECTING_AGENTS.md)
**Contracts:** [`../contracts/CURRENT.md`](../contracts/CURRENT.md) · Playback: [`../core/PLAYBACK_CONTRACT.md`](../core/PLAYBACK_CONTRACT.md)
**Human rollup:** [`../DRAFT_HANDOFF.md`](../DRAFT_HANDOFF.md)

## Capacity

**Permanent seats:** 5 + Integrator. **Coding-hot:** none right now — Match Shell wave closed.
Camera is next to go hot (letterbox + freecam reconcile).

## Live folders ↔ seats

| Seat | Canonical | **Live folder now** | Tip / state | STATUS |
|------|-----------|---------------------|-------------|--------|
| **Integrator** | `logiCard` | `D:\projects\Game\logiCard` | `master` @ `a21b29c` — Match Shell wave closed | [`core/STATUS.md`](core/STATUS.md) |
| **Atmosphere** | `logiCard-atmosphere` | `logiCard-atmosphere-stylized` | Docs contribution merged; worktree still holds its own uncommitted Sunny-mood code (not merged, separate decision) | [`atmosphere/STATUS.md`](atmosphere/STATUS.md) |
| **Cards** | `logiCard-cards` | `logiCard-cards-collection` | Docs contribution merged; idle | [`cards/STATUS.md`](cards/STATUS.md) |
| **Character** | `logiCard-character` | `logiCard-char-select-motion` | Docs contribution merged; worktree still holds its own older, larger, unmerged Char Select carousel feature (separate workstream) | [`character/STATUS.md`](character/STATUS.md) |
| **UI** | `logiCard-ui` | `logiCard-modal-restyle` | Fully merged to master (`e1c80fb`); idle | [`ui/STATUS.md`](ui/STATUS.md) |
| **Map** | `logiCard-map` | `D:\projects\Game\logiCard-map` | Docs contribution merged; idle | [`map/STATUS.md`](map/STATUS.md) |
| **Camera** | — | `logiCard-camera-control` | @ `2b06a3a` freecam+TPS committed — **unblocked**, reconcile against real `MapViewport` next | — |

## Ownership matrix (write locks)

| Path / concern | Owner now |
|----------------|-----------|
| `docs/ui/MATCH_SHELL_LAYOUT.md`, `docs/contracts/CURRENT.md`, `DRAFT_HANDOFF`, INDEX | **Integrator** |
| `ProgramHud` / `GearHandView` / match shell bands / `UiStyle` dock tokens | **Integrator** (merged; UI idle unless restaffed) |
| `BoardCameraRig` / `GameBootstrap.ConfigureCamera` | **Camera** — now active: wire letterbox to `MapViewport`, reconcile `2b06a3a` |
| Sim/Net/Timeline resolve | **Integrator** — frozen; UI calls only |

## Integrator merge gates

1. ~~Storm / Bandage HUD / hand-deck~~ — merged.
2. ~~HUD Chrome alone~~ — absorbed by Match Shell.
3. ~~Match Shell Layout (UI)~~ — **merged `c9925b1`**, human Play-signed, docs peers folded in `a21b29c`, batchmode green (174/174, 56/56).
4. **Camera** — MapViewport API now exists; wire `ConfigureCamera` letterbox, then reconcile/merge `2b06a3a` freecam+TPS.
5. Healed presenter; Atmosphere Sunny decision; Storm numerics — unchanged backlog.
