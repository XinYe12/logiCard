# Departments — Active Index

**Updated:** 2026-08-15 — **Match Shell Layout wave: UI Ready, awaiting human Play sign-off.**
Five-band `ProgramHud` in `logiCard-modal-restyle` verified (EditMode 174/174, PlayMode 53/53),
uncommitted pending sign-off. Cards / Character / Map / Atmosphere docs deliverables **Ready in
their worktrees** (not merged). Camera has freecam commit `2b06a3a` — reconcile after MapViewport
freezes. Plan: [`../ui/MATCH_SHELL_LAYOUT.md`](../ui/MATCH_SHELL_LAYOUT.md).

**Ops constitution:** [`../PARALLEL_OPS.md`](../PARALLEL_OPS.md) · human-side playbook: [`../DIRECTING_AGENTS.md`](../DIRECTING_AGENTS.md)
**Contracts:** [`../contracts/CURRENT.md`](../contracts/CURRENT.md) · Playback: [`../core/PLAYBACK_CONTRACT.md`](../core/PLAYBACK_CONTRACT.md)
**Human rollup:** [`../DRAFT_HANDOFF.md`](../DRAFT_HANDOFF.md)

## Capacity

**Permanent seats:** 5 + Integrator. **Coding-hot:** **UI only** (docs peers do not count as coding-hot).
Camera ephemeral paused.

## Live folders ↔ seats

| Seat | Canonical | **Live folder now** | Tip / state | STATUS |
|------|-----------|---------------------|-------------|--------|
| **Integrator** | `logiCard` | `D:\projects\Game\logiCard` | `master` @ `164012f` — layout wave opened | [`core/STATUS.md`](core/STATUS.md) |
| **Atmosphere** | `logiCard-atmosphere` | `logiCard-atmosphere-stylized` | **Ready** — `WEATHER_MAP_VIEWPORT.md` (uncommitted); Sunny held back | [`atmosphere/STATUS.md`](atmosphere/STATUS.md) |
| **Cards** | `logiCard-cards` | `logiCard-cards-collection` | **Ready** — `CARD_COLLECTION.md` §13 (uncommitted) | [`cards/STATUS.md`](cards/STATUS.md) |
| **Character** | `logiCard-character` | `logiCard-char-select-motion` | **Ready** — `CHARACTER_FANTASY.md` §4.1 InfoBar (uncommitted) | [`character/STATUS.md`](character/STATUS.md) |
| **UI** | `logiCard-ui` | `logiCard-modal-restyle` | `3f77b6c` + **uncommitted Match Shell — Ready**, awaiting human Play sign-off | [`ui/STATUS.md`](ui/STATUS.md) |
| **Map** | `logiCard-map` | `D:\projects\Game\logiCard-map` | **Ready** — `MAP_PRESENTATION_STANDARD.md` §6 (uncommitted) | [`map/STATUS.md`](map/STATUS.md) |
| **Camera** | — | `logiCard-camera-control` | @ `2b06a3a` freecam+TPS committed — hold merge for MapViewport | — |

## Ownership matrix (write locks)

| Path / concern | Owner now |
|----------------|-----------|
| `docs/ui/MATCH_SHELL_LAYOUT.md`, `docs/contracts/CURRENT.md` Match Shell section, `DRAFT_HANDOFF`, INDEX | **Integrator** |
| `ProgramHud` / `GearHandView` / match shell bands / `UiStyle` dock tokens | **UI** (Match Shell) |
| Cards catalog docs (schedule chip notes) | **Cards** |
| Character InfoBar field sheet (docs) | **Character** |
| Map framing docs | **Map** |
| Weather confinement notes | **Atmosphere** |
| `BoardCameraRig` / `GameBootstrap.ConfigureCamera` | **Camera** (paused) → Integrator wires letterbox after UI Ready |
| Sim/Net/Timeline resolve | **Integrator** — frozen; UI calls only |

## Integrator merge gates

1. ~~Storm / Bandage HUD / hand-deck~~ — merged.
2. ~~HUD Chrome alone~~ — **deferred**; absorbed by Match Shell.
3. **Match Shell Layout (UI)** — **UI Ready**; awaiting human Play sign-off; then merge + camera letterbox.
4. Docs peer recommendations — fold into plan/UI_FLOW when UI lands (no solo merge required).
   Atmosphere Ready: keep weather pocket board-local, camera `rect` = MapViewport, Sunny clear bound
   to the map camera only (`atmosphere/WEATHER_MAP_VIEWPORT.md`, worktree-side, not yet merged).
5. Camera freecam/TPS — after MapViewport API exists.
6. Healed presenter; Atmosphere Sunny decision; Storm numerics — unchanged backlog.
