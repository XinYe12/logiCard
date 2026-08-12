# Departments — Active Index

**Updated:** 2026-08-12 — Integrator monitoring five live multi-root folders. Canonical permanent names vs current slice folders noted below. Weather **not** reclaimed — Atmosphere has post-merge tip `083d50f` awaiting report/Play.
**Ops constitution:** [`../PARALLEL_OPS.md`](../PARALLEL_OPS.md) · human-side playbook: [`../DIRECTING_AGENTS.md`](../DIRECTING_AGENTS.md)
**Contracts:** [`../contracts/CURRENT.md`](../contracts/CURRENT.md) · Playback: [`../PLAYBACK_CONTRACT.md`](../PLAYBACK_CONTRACT.md)
**Human rollup:** [`../DRAFT_HANDOFF.md`](../DRAFT_HANDOFF.md) · pillars: [`../GDD.md`](../GDD.md) §11

## Capacity

**Permanent seats:** 4 (Atmosphere / Cards / Character / UI) + Integrator boss.  
**Live folders now (multi-root workspace):** 5 — see table.  
**Coding-hot preference:** ≤2 depts. Atmosphere may be hot on follow-up; Character/UI idle-ready; Cards docs-only.

## Live folders ↔ seats

| Seat | Canonical (GDD §11) | **Live folder now** | Tip / state | STATUS |
|------|---------------------|---------------------|-------------|--------|
| **Integrator** | `logiCard` | `D:\projects\Game\logiCard` | `master` @ `de5e4fe` + dirty rematch/floors/lighting (**not** committing code) | [`core/STATUS.md`](core/STATUS.md) |
| **Atmosphere** | `logiCard-atmosphere` | **`logiCard-atmosphere-stylized`** (slice name; migrate later) | `feat/atmosphere-stylized` @ **`083d50f`** (post-merge polish; weather lock) | [`atmosphere/STATUS.md`](atmosphere/STATUS.md) |
| **Cards** | `logiCard-cards` | **`logiCard-cards-collection`** | `feat/cards-collection-docs` @ `d00acfc` | [`cards/STATUS.md`](cards/STATUS.md) |
| **Character** | `logiCard-character` | **`logiCard-char-select-motion`** | `feat/char-select-motion` @ `b5d7c77` — Done; **no merge until human Play OK** | [`character/STATUS.md`](character/STATUS.md) |
| **UI** | `logiCard-ui` | **`logiCard-modal-restyle`** | `feat/modal-restyle` @ `492b8fe` — Done; **no merge until human Play OK** | [`ui/STATUS.md`](ui/STATUS.md) |

## Ownership matrix (write locks)

| Path / concern | Owner now |
|----------------|-----------|
| `docs/CARD_COLLECTION.md` | **Cards** (`logiCard-cards-collection`) |
| `BoardWeatherPocket.cs`, `Resources/Weather/**`, weather tools/tests | **Atmosphere** (`logiCard-atmosphere-stylized`) until `083d50f` merged or Integrator reclaims |
| Char-select / `UiMotion` / `CharacterSelectView` / `CharSelect*` | **Character** (`logiCard-char-select-motion`) until merge |
| `ModalDialog` / `Modal*` | **UI** (`logiCard-modal-restyle`) until merge |
| `GameBootstrap`, `RoundPlayback`, `MatchClock`, board surfaces/probes | **Integrator** (dirty on main — uncommitted) |
| `DRAFT_HANDOFF`, `PRODUCT_MEMORY`, `contracts/CURRENT`, `PARALLEL_OPS`, `GDD`, this INDEX | **Integrator** |
| `docs/departments/<dept>/STATUS.md` | That dept only |

## Integrator merge gates

- **Do not merge** `feat/char-select-motion` or `feat/modal-restyle` until human says Play is OK.
- Atmosphere `083d50f` — wait for dept report-back / human look; then Integrator merges.
- Rematch/floors/lighting dirty code — commit only when human asks.

## Cross-review checklist (session start)

- [ ] Read DRAFT_HANDOFF
- [ ] Read this INDEX (live folder names, not only canonical)
- [ ] Read GDD §11
- [ ] Read peer STATUS for every hot seat
- [ ] Read contracts/CURRENT
- [ ] Confirm no file overlap before editing
