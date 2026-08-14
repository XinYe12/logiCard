# Departments — Active Index

**Updated:** 2026-08-14 — Integrator committed dirty rematch/floors/lighting (`a419ad4`), clearing the
`Board*` conflict; wrote **C65** (C53 surface-material amendment, human YES) to `PRODUCT_MEMORY.md`; opened
**Map Phase 2 contract**; merged `feat/cards-collection-docs` (**C66** deckbuilder sizing + C64 catalog
sync, `4a355dd`), `feat/atmosphere-stylized` (storm Zap tip + cloud energize, human Play-signed, `668b162`),
and `dept/map`'s Phase 2 flat/toon floors + toy fence walls (human Play-signed, `a76f006`) on human approval.
UI still coding-hot (Bandage HUD); Cards, Atmosphere, and Map all merged and idle.

**Ops constitution:** [`../PARALLEL_OPS.md`](../PARALLEL_OPS.md) · human-side playbook: [`../DIRECTING_AGENTS.md`](../DIRECTING_AGENTS.md)
**Contracts:** [`../contracts/CURRENT.md`](../contracts/CURRENT.md) · Playback: [`../core/PLAYBACK_CONTRACT.md`](../core/PLAYBACK_CONTRACT.md)
**Human rollup:** [`../DRAFT_HANDOFF.md`](../DRAFT_HANDOFF.md) · pillars: [`../core/GDD.md`](../core/GDD.md) §11 · map: [`../map/MAP_AUTHORING.md`](../map/MAP_AUTHORING.md) · [`../map/MAP_PRESENTATION_STANDARD.md`](../map/MAP_PRESENTATION_STANDARD.md)

## Capacity

**Permanent seats:** 5 (Atm/Cards/Character/UI/**Map**) + Integrator. **Coding-hot preference:** ≤2 — currently **UI** (Bandage HUD).

## Live folders ↔ seats

| Seat | Canonical | **Live folder now** | Tip / state | STATUS |
|------|-----------|---------------------|-------------|--------|
| **Integrator** | `logiCard` | `D:\projects\Game\logiCard` | `master` @ `a76f006` — clean | [`core/STATUS.md`](core/STATUS.md) |
| **Atmosphere** | `logiCard-atmosphere` | `logiCard-atmosphere-stylized` | `feat/atmosphere-stylized` @ `45ccbc1` — **merged (`668b162`)**, idle; worktree still has unrelated dirty (mats/ProjectSettings/`_Recovery`) left out on purpose | [`atmosphere/STATUS.md`](atmosphere/STATUS.md) |
| **Cards** | `logiCard-cards` | `logiCard-cards-collection` | `feat/cards-collection-docs` @ `8b5e86d` — **merged (`4a355dd`)**, idle | [`cards/STATUS.md`](cards/STATUS.md) |
| **Character** | `logiCard-character` | `logiCard-char-select-motion` | `feat/char-select-motion` — 4 briefs; idle until human answers | [`character/STATUS.md`](character/STATUS.md) |
| **UI** | `logiCard-ui` | `logiCard-modal-restyle` | `feat/modal-restyle` — **Bandage HUD coding** | [`ui/STATUS.md`](ui/STATUS.md) |
| **Map** | `logiCard-map` | `D:\projects\Game\logiCard-map` | `dept/map` @ `565583f` — **Phase 2 merged (`a76f006`)**, idle | [`map/STATUS.md`](map/STATUS.md) |

## Ownership matrix (write locks)

| Path / concern | Owner now |
|----------------|-----------|
| `docs/cards/CARD_COLLECTION.md`, `docs/cards/CARD_SYSTEM_MODEL_COMPARISON.md`, `docs/cards/CARD_SYSTEM_OPENS.md` | **Cards** |
| Weather / `BoardWeatherPocket` / `Resources/Weather/**` | **Integrator** — merged, clean on main; Atmosphere idle |
| Bandage HUD: `ProgramHud`, `GearHandView`, `PawnProgram.TryQueueBandage`, `BoardInputController` Bandage place, related tests | **UI** this wave |
| `RoundPlayback.BandageChargeOf` only (tiny reader) | **UI** this wave — avoid rematch methods |
| `Assets/_Project/Board/BoardSurfaceMaterials.cs`, `BoardView.cs` material/dressing call sites | **Integrator** — Phase 2 merged, clean on main; Map idle |
| `MapDefinitions` / `GameBootstrap.BuildXxxGeometry` / Sim door walls | **Integrator** (C57) — Map reads only |
| `GameBootstrap` (rematch, lighting grade, probes, camera) | **Integrator** — clean on main; Map does not touch |
| `CharacterSelectView` / char-select art / `UiMotion` | **UI** (mandate; not this slice) |
| `Assets/_Project/Characters/**`, ability briefs | **Character** |
| `PRODUCT_MEMORY`, `DRAFT_HANDOFF`, contracts, INDEX | **Integrator** |

## Integrator merge gates

1. **UI Bandage HUD** — merge when Ready + batchmode green; then Integrator Healed presenter.
2. Character ability Sim — blocked on brief answers + carve-out.
3. Batchmode re-verify `a76f006` (new tests from rematch/relight, Atmosphere storm, and Map Phase 2 not yet run in batchmode).
4. Optional: `GameBootstrap` lighting/`BuildDioramaVolume` re-grade against Map's new saturated flat materials — Map flagged it, not required; human already likes the Play look.
5. ~~Cards `feat/cards-collection-docs`~~ — **merged** (`4a355dd`, C66 + C64 catalog sync).
6. ~~Atmosphere `feat/atmosphere-stylized`~~ — **merged** (`668b162`, storm Zap tip + cloud energize, human Play-signed).
7. ~~Map Phase 2 `dept/map`~~ — **merged** (`a76f006`, flat/toon floors + toy fence walls, human Play-signed).
