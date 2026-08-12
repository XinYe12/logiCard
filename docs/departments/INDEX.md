# Departments — Active Index

**Updated:** 2026-08-12 — **C62** gear rules confirmed; `feat/cards-collection-docs` merged. Character + UI idle-ready (Play gate). Atmosphere Ready @ `c3296dc`.
**Ops constitution:** [`../PARALLEL_OPS.md`](../PARALLEL_OPS.md) · human-side playbook: [`../DIRECTING_AGENTS.md`](../DIRECTING_AGENTS.md)
**Contracts:** [`../contracts/CURRENT.md`](../contracts/CURRENT.md) · Playback: [`../PLAYBACK_CONTRACT.md`](../PLAYBACK_CONTRACT.md)
**Human rollup:** [`../DRAFT_HANDOFF.md`](../DRAFT_HANDOFF.md) · pillars: [`../GDD.md`](../GDD.md) §11 · cards: [`../CARD_COLLECTION.md`](../CARD_COLLECTION.md)

## Capacity

**Permanent seats:** 4 + Integrator. **Coding-hot preference:** ≤2. Character/UI idle; Cards research closed; Atmosphere Ready for merge.

## Live folders ↔ seats

| Seat | Canonical | **Live folder now** | Tip / state | STATUS |
|------|-----------|---------------------|-------------|--------|
| **Integrator** | `logiCard` | `D:\projects\Game\logiCard` | `master` @ `8b791d9`+ (C62 pending commit) + dirty rematch/floors | [`core/STATUS.md`](core/STATUS.md) |
| **Atmosphere** | `logiCard-atmosphere` | `logiCard-atmosphere-stylized` | `feat/atmosphere-stylized` @ **`c3296dc` Ready** — weather lock | [`atmosphere/STATUS.md`](atmosphere/STATUS.md) |
| **Cards** | `logiCard-cards` | `logiCard-cards-collection` (docs merged) | Research **merged**; **C62** locked; seat idle until gear Sim brief | [`cards/STATUS.md`](cards/STATUS.md) |
| **Character** | `logiCard-character` | `logiCard-char-select-motion` | `b5d7c77`+`25244d7` idle-ready — **Play gate** | [`character/STATUS.md`](character/STATUS.md) |
| **UI** | `logiCard-ui` | `logiCard-modal-restyle` | `492b8fe`+`6f1739c` idle-ready — **Play gate** | [`ui/STATUS.md`](ui/STATUS.md) |

## Ownership matrix (write locks)

| Path / concern | Owner now |
|----------------|-----------|
| `docs/CARD_COLLECTION.md` | **Integrator** (merged; Cards may reclaim when next brief opens) |
| Weather / `BoardWeatherPocket` / `Resources/Weather/**` | **Atmosphere** until `c3296dc` merged |
| Char-select / `CharSelect*` / `UiMotion` | **Character** until merge |
| `ModalDialog` / `Modal*` | **UI** until merge |
| Boot/Timeline rematch dirty | **Integrator** |
| `PRODUCT_MEMORY`, `DRAFT_HANDOFF`, contracts, this INDEX | **Integrator** |

## Integrator merge gates

1. Human Play Character → merge `feat/char-select-motion`
2. Human Play UI → merge `feat/modal-restyle`
3. Atmosphere `c3296dc` after look OK
4. Rematch/floors dirty — commit when human asks
5. Gear Sim/HUD — only after C62 numerics / station brief (OPEN #16)
