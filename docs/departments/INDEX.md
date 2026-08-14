# Departments — Active Index

**Updated:** 2026-08-14 — Integrator committed dirty rematch/floors/lighting (`a419ad4`), clearing the
`Board*` conflict; wrote **C65** (C53 surface-material amendment, human YES) to `PRODUCT_MEMORY.md`; opened
**Map Phase 2 contract**. UI still coding-hot (Bandage HUD); Map now coding-hot too (Phase 2 unblocked).

**Ops constitution:** [`../PARALLEL_OPS.md`](../PARALLEL_OPS.md) · human-side playbook: [`../DIRECTING_AGENTS.md`](../DIRECTING_AGENTS.md)
**Contracts:** [`../contracts/CURRENT.md`](../contracts/CURRENT.md) · Playback: [`../core/PLAYBACK_CONTRACT.md`](../core/PLAYBACK_CONTRACT.md)
**Human rollup:** [`../DRAFT_HANDOFF.md`](../DRAFT_HANDOFF.md) · pillars: [`../core/GDD.md`](../core/GDD.md) §11 · map: [`../map/MAP_AUTHORING.md`](../map/MAP_AUTHORING.md) · [`../map/MAP_PRESENTATION_STANDARD.md`](../map/MAP_PRESENTATION_STANDARD.md)

## Capacity

**Permanent seats:** 5 (Atm/Cards/Character/UI/**Map**) + Integrator. **Coding-hot preference:** ≤2 — currently **UI + Map** (Bandage HUD, Map Phase 2), both against frozen contracts.

## Live folders ↔ seats

| Seat | Canonical | **Live folder now** | Tip / state | STATUS |
|------|-----------|---------------------|-------------|--------|
| **Integrator** | `logiCard` | `D:\projects\Game\logiCard` | `master` @ `a419ad4` — clean | [`core/STATUS.md`](core/STATUS.md) |
| **Atmosphere** | `logiCard-atmosphere` | `logiCard-atmosphere-stylized` | `feat/atmosphere-stylized` @ `fac245a` + dirty weather polish — merge after human Play | [`atmosphere/STATUS.md`](atmosphere/STATUS.md) |
| **Cards** | `logiCard-cards` | `logiCard-cards-collection` | `feat/cards-collection-docs` — C64 docs follow-up | [`cards/STATUS.md`](cards/STATUS.md) |
| **Character** | `logiCard-character` | `logiCard-char-select-motion` | `feat/char-select-motion` — 4 briefs; idle until human answers | [`character/STATUS.md`](character/STATUS.md) |
| **UI** | `logiCard-ui` | `logiCard-modal-restyle` | `feat/modal-restyle` — **Bandage HUD coding** | [`ui/STATUS.md`](ui/STATUS.md) |
| **Map** | `logiCard-map` | `D:\projects\Game\logiCard-map` | `dept/map` @ `d632d3b` — **Phase 2 contract open, unblocked** | [`map/STATUS.md`](map/STATUS.md) |

## Ownership matrix (write locks)

| Path / concern | Owner now |
|----------------|-----------|
| `docs/CARD_COLLECTION.md`, `docs/CARD_SYSTEM_MODEL_COMPARISON.md` | **Cards** |
| Weather / `BoardWeatherPocket` / `Resources/Weather/**` | **Atmosphere** until merged |
| Bandage HUD: `ProgramHud`, `GearHandView`, `PawnProgram.TryQueueBandage`, `BoardInputController` Bandage place, related tests | **UI** this wave |
| `RoundPlayback.BandageChargeOf` only (tiny reader) | **UI** this wave — avoid rematch methods |
| `Assets/_Project/Board/BoardSurfaceMaterials.cs`, `BoardView.cs` material/dressing call sites | **Map** this wave — Phase 2 contract (`contracts/CURRENT.md`), material/mesh-skin only |
| `MapDefinitions` / `GameBootstrap.BuildXxxGeometry` / Sim door walls | **Integrator** (C57) — Map reads only |
| `GameBootstrap` (rematch, lighting grade, probes, camera) | **Integrator** — clean on main; Map does not touch |
| `CharacterSelectView` / char-select art / `UiMotion` | **UI** (mandate; not this slice) |
| `Assets/_Project/Characters/**`, ability briefs | **Character** |
| `PRODUCT_MEMORY`, `DRAFT_HANDOFF`, contracts, INDEX | **Integrator** |

## Integrator merge gates

1. **UI Bandage HUD** — merge when Ready + batchmode green; then Integrator Healed presenter.
2. **Map Phase 2** — merge when Ready + batchmode green + human screenshot check against the Link's Awakening reference.
3. Atmosphere — after human look (`fac245a`+ dirty).
4. Character ability Sim — blocked on brief answers + carve-out.
5. Batchmode re-verify `a419ad4` (rematch/relight commit's new tests not yet run in batchmode).
