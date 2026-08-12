# Departments — Active Index

**Updated:** 2026-08-12 — Character + UI report-back **idle-ready** (docs tips on deliverables). Atmosphere polish @ `083d50f` still weather-locked. Cards research advanced @ `14db79e`.
**Ops constitution:** [`../PARALLEL_OPS.md`](../PARALLEL_OPS.md) · human-side playbook: [`../DIRECTING_AGENTS.md`](../DIRECTING_AGENTS.md)
**Contracts:** [`../contracts/CURRENT.md`](../contracts/CURRENT.md) · Playback: [`../PLAYBACK_CONTRACT.md`](../PLAYBACK_CONTRACT.md)
**Human rollup:** [`../DRAFT_HANDOFF.md`](../DRAFT_HANDOFF.md) · pillars: [`../GDD.md`](../GDD.md) §11

## Capacity

**Permanent seats:** 4 + Integrator. **Live folders:** 5 (slice names below).  
**Coding-hot:** Atmosphere may still be hot; Character + UI **idle** (await Play); Cards docs-only.

## Live folders ↔ seats

| Seat | Canonical (GDD §11) | **Live folder now** | Tip / state | STATUS |
|------|---------------------|---------------------|-------------|--------|
| **Integrator** | `logiCard` | `D:\projects\Game\logiCard` | `master` @ `5d610b1` + dirty rematch/floors/lighting | [`core/STATUS.md`](core/STATUS.md) |
| **Atmosphere** | `logiCard-atmosphere` | `logiCard-atmosphere-stylized` | `feat/atmosphere-stylized` @ **`083d50f`** — weather lock; await look → merge | [`atmosphere/STATUS.md`](atmosphere/STATUS.md) |
| **Cards** | `logiCard-cards` | `logiCard-cards-collection` | `feat/cards-collection-docs` @ **`14db79e`** | [`cards/STATUS.md`](cards/STATUS.md) |
| **Character** | `logiCard-character` | `logiCard-char-select-motion` | Deliverable **`b5d7c77`** + docs **`25244d7`** — **idle-ready**; merge only after human Play OK | [`character/STATUS.md`](character/STATUS.md) |
| **UI** | `logiCard-ui` | `logiCard-modal-restyle` | Deliverable **`492b8fe`** + docs **`6f1739c`** — **idle-ready**; merge only after human Play OK | [`ui/STATUS.md`](ui/STATUS.md) |

## Ownership matrix (write locks)

| Path / concern | Owner now |
|----------------|-----------|
| `docs/CARD_COLLECTION.md` | **Cards** |
| Weather / `BoardWeatherPocket` / `Resources/Weather/**` | **Atmosphere** until `083d50f` merged or Integrator reclaims |
| Char-select / `UiMotion` / `CharacterSelectView` / `CharSelect*` | **Character** until merge |
| `ModalDialog` / `Modal*` | **UI** until merge |
| Boot / Timeline / board surfaces (dirty rematch/floors) | **Integrator** |
| `DRAFT_HANDOFF`, `PRODUCT_MEMORY`, contracts, `PARALLEL_OPS`, `GDD`, this INDEX | **Integrator** |

## Integrator merge gates

1. Human Play Character (`PLAY_NOTES.md` in char-select worktree) → merge `feat/char-select-motion` first (`CharSelect*` tokens).
2. Human Play UI Quit modal (`PLAY_NOTES.md` in modal worktree) → merge `feat/modal-restyle` (`Modal*` tokens; expect clean `UiStyle` combine).
3. Atmosphere `083d50f` after look OK.
4. Rematch/floors dirty code — commit only when human asks.

## Cross-review checklist (session start)

- [ ] Read DRAFT_HANDOFF
- [ ] Read this INDEX
- [ ] Read GDD §11
- [ ] Read peer STATUS for hot seats
- [ ] Confirm no file overlap
