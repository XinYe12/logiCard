# Departments — Active Index

**Updated:** 2026-08-12 — Atmosphere merged to master (`5b2ee7c`). UI char-select + modal Done awaiting Play/merge. Cards-collection docs open.
**Ops constitution:** [`../PARALLEL_OPS.md`](../PARALLEL_OPS.md) · human-side playbook: [`../DIRECTING_AGENTS.md`](../DIRECTING_AGENTS.md)
**Contracts:** [`../contracts/CURRENT.md`](../contracts/CURRENT.md) · Playback: [`../PLAYBACK_CONTRACT.md`](../PLAYBACK_CONTRACT.md)
**Human rollup:** [`../DRAFT_HANDOFF.md`](../DRAFT_HANDOFF.md)

## Capacity

Integrator + up to **2** coding workers — **0 of 2** coding. Atmosphere closed. UI deliverables await human Play before merge.

## Active agents / worktrees

Paths under `D:\projects\Game\logiCard*`:

- **Integrator (main `logiCard`, `master` @ `5b2ee7c` + dirty rematch/floors/lighting)** — merge authority; weather reclaimed.
- **Presentation atmosphere (Merged)** — worktree `logiCard-atmosphere-stylized` may be pruned after confirm; branch `feat/atmosphere-stylized` merged.
- **UI char-select motion (Done — awaiting Play + merge)** — `D:\projects\Game\logiCard-char-select-motion` / `feat/char-select-motion` @ `b5d7c77`.
- **UI modal restyle (Done — awaiting Play + merge)** — `D:\projects\Game\logiCard-modal-restyle` / `feat/modal-restyle` @ `492b8fe`.
- **Docs cards collection (research draft)** — `D:\projects\Game\logiCard-cards-collection` / `feat/cards-collection-docs` @ `d00acfc`. Owns `docs/CARD_COLLECTION.md` until merge.

## Ownership matrix (write locks)

| Path / concern | Owner now |
|----------------|-----------|
| `docs/MAP_AUTHORING.md`, `docs/UI_TOOLS_RESEARCH.md` | Integrator (merged; closed) |
| `docs/CARD_COLLECTION.md` | Cards-collection worktree until merge |
| `BoardWeatherPocket.cs`, `WeatherPackImportTool.cs`, `Resources/Weather/**`, WeatherPocket PlayMode tests | **Integrator** (reclaimed after atmosphere merge) |
| `UiMotion.cs`, `CharacterSelectView.cs`, Character Select path, `CharSelect*` UiStyle tokens | Char-select worktree until merge |
| `ModalDialog.cs`, `Modal*` UiStyle tokens | Modal-restyle worktree until merge |
| `GameBootstrap.cs`, `RoundPlayback.cs`, `BoardSurfaceMaterials.cs`, `BoardReflectionProbes.cs`, `MatchClock.cs` | Integrator |
| `docs/DRAFT_HANDOFF.md`, `PRODUCT_MEMORY.md`, `contracts/CURRENT.md`, `PLAYBACK_CONTRACT.md`, this INDEX | Integrator |
| `docs/departments/<dept>/STATUS.md` | That dept only |

## Cross-review checklist (session start)

- [ ] Read DRAFT_HANDOFF
- [ ] Read this INDEX
- [ ] Read `PLAYBACK_CONTRACT.md` if touching Execute / ReplayTape / tape verbs
- [ ] Read peer STATUS for every **In progress** row above
- [ ] Read contracts/CURRENT
- [ ] Confirm no file overlap before editing
