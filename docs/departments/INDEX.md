# Departments — Active Index

**Updated:** 2026-08-14 — **Map** registered as permanent seat (`logiCard-map`). UI still coding-hot (Bandage HUD).
Map Phase 1 docs done (`MAP_PRESENTATION_STANDARD.md`); Phase 2 blocked on human §4 (C53 surface-material amendment).
**Ops constitution:** [`../PARALLEL_OPS.md`](../PARALLEL_OPS.md) · human-side playbook: [`../DIRECTING_AGENTS.md`](../DIRECTING_AGENTS.md)
**Contracts:** [`../contracts/CURRENT.md`](../contracts/CURRENT.md) · Playback: [`../core/PLAYBACK_CONTRACT.md`](../core/PLAYBACK_CONTRACT.md)
**Human rollup:** [`../DRAFT_HANDOFF.md`](../DRAFT_HANDOFF.md) · pillars: [`../core/GDD.md`](../core/GDD.md) §11 · map: [`../map/MAP_AUTHORING.md`](../map/MAP_AUTHORING.md) · [`../map/MAP_PRESENTATION_STANDARD.md`](../map/MAP_PRESENTATION_STANDARD.md)

## Capacity

**Permanent seats:** 5 (Atm/Cards/Character/UI/**Map**) + Integrator. **Coding-hot preference:** ≤2. **Hot now:** UI (Bandage HUD). Map = docs/decision-prep until §4 confirmed (then Phase 2 needs Integrator contract + dirty Board* reclaim).

## Live folders ↔ seats

| Seat | Canonical | **Live folder now** | Tip / state | STATUS |
|------|-----------|---------------------|-------------|--------|
| **Integrator** | `logiCard` | `D:\projects\Game\logiCard` | `master` @ `e07be61` + dirty rematch/floors/lighting (uncommitted) | [`core/STATUS.md`](core/STATUS.md) |
| **Atmosphere** | `logiCard-atmosphere` | `logiCard-atmosphere-stylized` | `feat/atmosphere-stylized` @ `fac245a` + dirty weather polish — merge after human Play | [`atmosphere/STATUS.md`](atmosphere/STATUS.md) |
| **Cards** | `logiCard-cards` | `logiCard-cards-collection` | `feat/cards-collection-docs` — C64 docs follow-up | [`cards/STATUS.md`](cards/STATUS.md) |
| **Character** | `logiCard-character` | `logiCard-char-select-motion` | `feat/char-select-motion` — 4 briefs; idle until human answers | [`character/STATUS.md`](character/STATUS.md) |
| **UI** | `logiCard-ui` | `logiCard-modal-restyle` | `feat/modal-restyle` — **Bandage HUD coding** | [`ui/STATUS.md`](ui/STATUS.md) |
| **Map** | `logiCard-map` | `D:\projects\Game\logiCard-map` | `dept/map` @ `d632d3b` — Phase 1 standard landed; await human §4 | [`map/STATUS.md`](map/STATUS.md) |

## Ownership matrix (write locks)

| Path / concern | Owner now |
|----------------|-----------|
| `docs/CARD_COLLECTION.md`, `docs/CARD_SYSTEM_MODEL_COMPARISON.md` | **Cards** |
| Weather / `BoardWeatherPocket` / `Resources/Weather/**` | **Atmosphere** until merged |
| Bandage HUD: `ProgramHud`, `GearHandView`, `PawnProgram.TryQueueBandage`, `BoardInputController` Bandage place, related tests | **UI** this wave |
| `RoundPlayback.BandageChargeOf` only (tiny reader) | **UI** this wave — avoid rematch methods |
| `docs/map/MAP_PRESENTATION_STANDARD.md`, map presentation Phase 2 (`BoardView` / `BoardSurfaceMaterials` / dressing) | **Map** — Phase 2 only after §4 + Integrator reclaim of dirty Board* on main |
| `MapDefinitions` / `GameBootstrap.BuildXxxGeometry` / Sim door walls | **Integrator** (C57) — Map reads only |
| `GameBootstrap` rematch / lighting grade / probes dirty hold | **Integrator** until asked to commit or reclaim for Map |
| `CharacterSelectView` / char-select art / `UiMotion` | **UI** (mandate; not this slice) |
| `Assets/_Project/Characters/**`, ability briefs | **Character** |
| `PRODUCT_MEMORY`, `DRAFT_HANDOFF`, contracts, INDEX | **Integrator** |

## Integrator merge gates

1. **UI Bandage HUD** — merge when Ready + batchmode green; then Integrator Healed presenter.
2. Atmosphere — after human look (`fac245a`+ dirty).
3. **Map Phase 2** — only after human answers `MAP_PRESENTATION_STANDARD.md` §4 → C# row; Integrator opens contract and clears dirty Board* conflict on main.
4. Rematch/floors dirty — commit when human asks.
5. Character ability Sim — blocked on brief answers + carve-out.
