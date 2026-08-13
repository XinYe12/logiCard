# Departments — Active Index

**Updated:** 2026-08-13 — **Mandate shift:** Character now owns behavior/abilities (attrs + long-term
unique-verb operators), not Character Select presentation; UI now owns **all** screen presentation
(lobby, Character Select, Map Select, in-game HUD) and must research a mature UI approach before more
building. Cards' hybrid card-system model (**C64**) promoted to PRODUCT_MEMORY, amending C18/C62.
**Bandage Sim-side merged (`4e6bb66`) and UI modal restyle + gear-hand scaffold merged after human Play
sign-off (`7213d98`)**, combined state batchmode-verified (EditMode 149/149, PlayMode 48/48). Bandage
HUD-side slot open. Atmosphere has moved past its last-logged tip, not yet re-reviewed.
**Ops constitution:** [`../PARALLEL_OPS.md`](../PARALLEL_OPS.md) · human-side playbook: [`../DIRECTING_AGENTS.md`](../DIRECTING_AGENTS.md)
**Contracts:** [`../contracts/CURRENT.md`](../contracts/CURRENT.md) · Playback: [`../PLAYBACK_CONTRACT.md`](../core/PLAYBACK_CONTRACT.md)
**Human rollup:** [`../DRAFT_HANDOFF.md`](../DRAFT_HANDOFF.md) · pillars: [`../GDD.md`](../core/GDD.md) §11 · cards: [`../CARD_COLLECTION.md`](../cards/CARD_COLLECTION.md) · card-system model: [`../CARD_SYSTEM_MODEL_COMPARISON.md`](../cards/CARD_SYSTEM_MODEL_COMPARISON.md)

## Capacity

**Permanent seats:** 4 + Integrator. **Coding-hot preference:** ≤2. Character idle (briefs done, needs human answers + a Sim carve-out); UI has a big new research mandate; Cards idle (Flashbang paused pending C64 landing — now landed); Atmosphere Ready for merge.

## Live folders ↔ seats

| Seat | Canonical | **Live folder now** | Tip / state | STATUS |
|------|-----------|---------------------|-------------|--------|
| **Integrator** | `logiCard` | `D:\projects\Game\logiCard` | `master` @ `7213d98` (Bandage Sim-side + UI modal/gear-hand merged, batchmode-verified) + dirty rematch/floors | [`core/STATUS.md`](core/STATUS.md) |
| **Atmosphere** | `logiCard-atmosphere` | `logiCard-atmosphere-stylized` | `feat/atmosphere-stylized` @ `c96273a` (past `755fb21`) — clay polish continuing, dirty tree, not yet re-reviewed | [`atmosphere/STATUS.md`](atmosphere/STATUS.md) |
| **Cards** | `logiCard-cards` | `logiCard-cards-collection` | `feat/cards-collection-docs` @ `c307d77` — designed the hybrid card-system model with human (**C64**, promoted); Flashbang brief stays paused until Cards re-derives it against C64 | [`cards/STATUS.md`](cards/STATUS.md) |
| **Character** | `logiCard-character` | `logiCard-char-select-motion` | `feat/char-select-motion` @ `dec54e7` — mandate shift landed; 4 implementation briefs (Unique-Verb/C42, Bomber/C43, Time Player/C44, Scout-Juggernaut attrs) written, docs-only, no Sim code | [`character/STATUS.md`](character/STATUS.md) |
| **UI** | `logiCard-ui` | `logiCard-modal-restyle` | Prior wave merged to master (`7213d98`); new mandate: research a mature UI approach (read `UI_TOOLKIT_MIGRATION_PROPOSAL.md` first — already piloted + reverted once) before building lobby/full-UI ownership | [`ui/STATUS.md`](ui/STATUS.md) |

## Ownership matrix (write locks)

| Path / concern | Owner now |
|----------------|-----------|
| `docs/CARD_COLLECTION.md`, `docs/CARD_SYSTEM_MODEL_COMPARISON.md` | **Cards** |
| Weather / `BoardWeatherPocket` / `Resources/Weather/**` | **Atmosphere** until merged |
| `CharacterSelectView.cs` / `CharSelect*` `UiStyle` tokens / char-select art / `UiMotion.cs` | **UI** (moved from Character, 2026-08-13) |
| `Assets/_Project/Characters/**` (`CharacterData`, attrs assets), character ability briefs | **Character** |
| `Assets/_Project/UI/**` (all screens), `ModalDialog`/`Modal*` | **UI** |
| Boot/Timeline rematch dirty | **Integrator** |
| `PRODUCT_MEMORY`, `DRAFT_HANDOFF`, contracts, `GDD.md` §11, `PARALLEL_OPS.md`, this INDEX | **Integrator** |

## Integrator merge gates

1. Character/UI: no merge pending right now — both are in research/brief mode, not code-hot.
2. Atmosphere — after look OK (tip now `c96273a`, moved again since last review).
3. Rematch/floors dirty — commit when human asks.
4. Bandage HUD-side contract slot — open, unstaffed; natural next job once UI's research lands.
5. Character ability Sim contracts — blocked on human answers to the 4 briefs' open questions **and** an explicit Sim-pause carve-out per ability (mirror C57/C63) — not assumed.
