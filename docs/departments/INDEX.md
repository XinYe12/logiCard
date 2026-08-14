# Departments — Active Index

**Updated:** 2026-08-14 — Integrator committed dirty rematch/floors/lighting (`a419ad4`), clearing the
`Board*` conflict; wrote **C65** (C53 surface-material amendment, human YES) to `PRODUCT_MEMORY.md`; opened
**Map Phase 2 contract**; merged `feat/cards-collection-docs` (**C66** deckbuilder sizing + C64 catalog
sync, `4a355dd`), `feat/atmosphere-stylized` (storm Zap tip + cloud energize, human Play-signed, `668b162`),
and `dept/map`'s Phase 2 flat/toon floors + toy fence walls (human Play-signed, `a76f006`) on human approval.
**Storm card wave opened** (C67, human-directed) — Sim-side landed on `master` directly by Integrator
(mirrors C63's Bandage Sim-side carve-out); **Cards + UI + Atmosphere all coding-hot at once** against the
Storm contract in `contracts/CURRENT.md` — a deliberate exception to the ≤2 default (see that file's
capacity note).

**Ops constitution:** [`../PARALLEL_OPS.md`](../PARALLEL_OPS.md) · human-side playbook: [`../DIRECTING_AGENTS.md`](../DIRECTING_AGENTS.md)
**Contracts:** [`../contracts/CURRENT.md`](../contracts/CURRENT.md) · Playback: [`../core/PLAYBACK_CONTRACT.md`](../core/PLAYBACK_CONTRACT.md)
**Human rollup:** [`../DRAFT_HANDOFF.md`](../DRAFT_HANDOFF.md) · pillars: [`../core/GDD.md`](../core/GDD.md) §11 · map: [`../map/MAP_AUTHORING.md`](../map/MAP_AUTHORING.md) · [`../map/MAP_PRESENTATION_STANDARD.md`](../map/MAP_PRESENTATION_STANDARD.md)

## Capacity

**Permanent seats:** 5 (Atm/Cards/Character/UI/**Map**) + Integrator. **Coding-hot preference:** ≤2 —
**exception this wave:** Cards + UI + Atmosphere all hot at once for the Storm card, each against its own
frozen slice of `contracts/CURRENT.md`'s Storm contract so there's no file overlap between them.

## Live folders ↔ seats

| Seat | Canonical | **Live folder now** | Tip / state | STATUS |
|------|-----------|---------------------|-------------|--------|
| **Integrator** | `logiCard` | `D:\projects\Game\logiCard` | `master` — Storm Sim-side landed | [`core/STATUS.md`](core/STATUS.md) |
| **Atmosphere** | `logiCard-atmosphere` | `logiCard-atmosphere-stylized` | `feat/atmosphere-stylized` — **Storm card follow-up: idempotency + lighting-dim round-trip check** | [`atmosphere/STATUS.md`](atmosphere/STATUS.md) |
| **Cards** | `logiCard-cards` | `logiCard-cards-collection` | `feat/cards-collection-docs` — **Storm card: CardId + catalog entry + numerics brief** | [`cards/STATUS.md`](cards/STATUS.md) |
| **Character** | `logiCard-character` | `logiCard-char-select-motion` | `feat/char-select-motion` — 4 briefs; idle until human answers | [`character/STATUS.md`](character/STATUS.md) |
| **UI** | `logiCard-ui` | `logiCard-modal-restyle` | `feat/modal-restyle` — **Bandage HUD + Storm card HUD wiring, same pass** | [`ui/STATUS.md`](ui/STATUS.md) |
| **Map** | `logiCard-map` | `D:\projects\Game\logiCard-map` | `dept/map` @ `565583f` — Phase 2 merged, idle | [`map/STATUS.md`](map/STATUS.md) |

## Ownership matrix (write locks)

| Path / concern | Owner now |
|----------------|-----------|
| `docs/cards/CARD_COLLECTION.md`, `docs/cards/CARD_SYSTEM_MODEL_COMPARISON.md`, `docs/cards/CARD_SYSTEM_OPENS.md` | **Cards** — Storm card catalog entry this wave |
| `Assets/_Project/Cards/CardData.cs` | **Integrator** — `CardId.Storm` pre-landed to avoid a Cards/UI cross-worktree ordering dependency; Cards reads, does not edit |
| Weather / `BoardWeatherPocket` / `Resources/Weather/**` | **Atmosphere** this wave — Storm idempotency/lighting-dim check only, no new VFX |
| Bandage HUD + Storm card HUD: `ProgramHud`, `GearHandView`, `PawnProgram.TryQueueBandage`/`TryQueueStorm`, `BoardInputController` Bandage/Storm place, related tests | **UI** this wave |
| `RoundPlayback.BandageChargeOf` only (tiny reader) | **UI** this wave — avoid rematch/weather methods |
| `Assets/_Project/Board/BoardSurfaceMaterials.cs`, `BoardView.cs` material/dressing call sites | **Integrator** — Phase 2 merged, clean on main; Map idle |
| `MapDefinitions` / `GameBootstrap.BuildXxxGeometry` / Sim door walls | **Integrator** (C57) — Map reads only |
| `GameBootstrap` (rematch, lighting grade, probes, camera, weather boot mood) | **Integrator** — clean on main; no other dept touches it |
| `Net/ActionVerb.cs`, `Net/TapeEvent.cs`, `Net/GhostResolver.cs`, `Boot/RoundPlayback.cs` (Storm Sim-side) | **Integrator** — closed, reference only |
| `CharacterSelectView` / char-select art / `UiMotion` | **UI** (mandate; not this slice) |
| `Assets/_Project/Characters/**`, ability briefs | **Character** |
| `PRODUCT_MEMORY`, `DRAFT_HANDOFF`, contracts, INDEX | **Integrator** |

## Integrator merge gates

1. **Storm card** — merge Cards catalog + UI HUD wiring + any Atmosphere idempotency fix together
   (they're one feature) when all three report Ready + batchmode green + human look/feel check.
2. **UI Bandage HUD** — merge when Ready + batchmode green; then Integrator Healed presenter.
3. Character ability Sim — blocked on brief answers + carve-out.
4. Batchmode re-verify current tip (rematch/relight, Atmosphere storm, Map Phase 2, and today's Storm
   Sim-side — nothing landed today has been run in batchmode yet).
5. Optional: `GameBootstrap` lighting/`BuildDioramaVolume` re-grade against Map's new saturated flat
   materials — Map flagged it, not required; human already likes the Play look.
