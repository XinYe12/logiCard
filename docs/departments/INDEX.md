# Departments — Active Index

**Updated:** 2026-08-16 — **Match Shell Layout wave: merged. Map's follow-up tweaks: merged.**
Camera's follow-up (zoom retune) is Integrator-reviewed and batchmode-green but **blocked on a real
UX bug**: the human tested it and the controls didn't respond at all (right-click-drag required, no
on-screen hint — human tried left-drag, which is reserved for board taps). A control-hint overlay is
being added now; needs a fresh batchmode pass + human re-test before merge. Plan:
[`../ui/MATCH_SHELL_LAYOUT.md`](../ui/MATCH_SHELL_LAYOUT.md).

**Ops constitution:** [`../PARALLEL_OPS.md`](../PARALLEL_OPS.md) · human-side playbook: [`../DIRECTING_AGENTS.md`](../DIRECTING_AGENTS.md)
**Contracts:** [`../contracts/CURRENT.md`](../contracts/CURRENT.md) · Playback: [`../core/PLAYBACK_CONTRACT.md`](../core/PLAYBACK_CONTRACT.md)
**Human rollup:** [`../DRAFT_HANDOFF.md`](../DRAFT_HANDOFF.md)

## Capacity

**Permanent seats:** 5 + Integrator. **Coding-hot:** Camera only (control-hint fix in flight).

## Live folders ↔ seats

| Seat | Canonical | **Live folder now** | Tip / state | STATUS |
|------|-----------|---------------------|-------------|--------|
| **Integrator** | `logiCard` | `D:\projects\Game\logiCard` | `master` @ `07501d7` — Match Shell + Map merged | [`core/STATUS.md`](core/STATUS.md) |
| **Atmosphere** | `logiCard-atmosphere` | `logiCard-atmosphere-stylized` | Docs contribution merged; worktree still holds its own uncommitted, explicitly-parked Sunny-mood code | [`atmosphere/STATUS.md`](atmosphere/STATUS.md) |
| **Cards** | `logiCard-cards` | `logiCard-cards-collection` | Docs contribution merged; idle | [`cards/STATUS.md`](cards/STATUS.md) |
| **Character** | `logiCard-character` | `logiCard-char-select-motion` | Docs + decision sheet merged; worktree still holds its own older, larger, unmerged Char Select carousel feature (separate workstream) | [`character/STATUS.md`](character/STATUS.md) |
| **UI** | `logiCard-ui` | `logiCard-modal-restyle` | Fully merged to master (`e1c80fb`); idle | [`ui/STATUS.md`](ui/STATUS.md) |
| **Map** | `logiCard-map` | `D:\projects\Game\logiCard-map` | **Fully merged to master** (`07501d7`); idle | [`map/STATUS.md`](map/STATUS.md) |
| **Camera** | — | `logiCard-camera-control` | Zoom retune done + Integrator-reviewed; **control-hint fix in flight** (batchmode pending); not merged | — |

## Ownership matrix (write locks)

| Path / concern | Owner now |
|----------------|-----------|
| `docs/ui/MATCH_SHELL_LAYOUT.md`, `docs/contracts/CURRENT.md`, `DRAFT_HANDOFF`, INDEX | **Integrator** |
| `ProgramHud` / `GearHandView` / match shell bands / `UiStyle` dock tokens | **Integrator** (merged; UI idle unless restaffed) |
| `Assets/_Project/Board/BoardSurfaceMaterials.cs`, `BoardView.cs` fence code | **Integrator** (Map merged; idle unless restaffed) |
| `BoardCameraRig` / `GameBootstrap.ConfigureCamera` | **Camera** — active: control-hint fix in flight |
| Sim/Net/Timeline resolve | **Integrator** — frozen; UI calls only |

## Integrator merge gates

1. ~~Storm / Bandage HUD / hand-deck~~ — merged.
2. ~~HUD Chrome alone~~ — absorbed by Match Shell.
3. ~~Match Shell Layout (UI)~~ — **merged `c9925b1`**, human Play-signed, docs peers folded in `a21b29c`, batchmode green (174/174, 56/56).
4. ~~Map fence-shadow + material tweaks~~ — **merged `07501d7`**, human-approved, batchmode green (174/174, 56/56).
5. **Camera** — zoom math done and reviewed; **blocked on the control-hint fix + a real human re-test** (first test found the controls entirely unresponsive from the player's perspective — right-click-drag with zero on-screen discoverability).
6. Healed presenter; Atmosphere Sunny decision (parked, not scheduled); Storm HUD-side per-match counter (flagged in C69, unstarted) — unchanged backlog.
