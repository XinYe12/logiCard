# Draft Handoff — 2026-08-14

**Milestone:** Phase 5 Commercial Art Bar (active). Phase 2 Net paused (Bandage carve-out via C63).  
**This seat:** Character — `logiCard-char-select-motion` / `feat/char-select-motion` @ **`1f9f785`** + **dirty** decision-sheet answers (uncommitted).  
**Tip (last known master):** `dcffe23` (C64). Prior combined batchmode green @ `7213d98` — not re-run after docs-only C64.  
**Ops:** Atmosphere / Cards / Character / UI + Integrator (`PARALLEL_OPS.md`). Prefer ≤2 coding-hot.  
**Read first next session:** this file → `departments/character/STATUS.md` → `CHARACTER_DECISION_SHEET.md` (resume **A3**) → `PARALLEL_OPS.md` / `contracts/CURRENT.md` if cross-seat.

## Live folders (last known; verify tips before merge)

| Seat | Folder | Tip / state |
|------|--------|-------------|
| Integrator | `logiCard` | `master` @ `dcffe23` + dirty rematch/floors/lighting (as of 08-13) |
| Atmosphere | `logiCard-atmosphere-stylized` | `feat/atmosphere-stylized` @ `af7b2e5` + dirty (as of 08-13) |
| Cards | `logiCard-cards-collection` | `feat/cards-collection-docs` @ `dc631ce` (as of 08-13) |
| Character | `logiCard-char-select-motion` | `feat/char-select-motion` @ `1f9f785` + dirty decision sheet |
| UI | `logiCard-modal-restyle` | `feat/modal-restyle` @ `8f4b406` + dirty; Bandage HUD not started (as of 08-13) |

## Implemented

- Carryover still true: C62/C63 Bandage Sim merged; UI modal + `GearHandView` scaffold merged but not wired to `ProgramHud`; C64 hybrid card model on master; Char Select chrome → UI; Character owns abilities/attrs docs.
- Character concept pack complete (plan / fantasy / Cards boundary / C36 dependency / Time Player epistemics / Detonator vs Bomber + four impl briefs).
- **Decision sheet** (`docs/departments/character/CHARACTER_DECISION_SHEET.md`): human walk started; **partial answers recorded** (not promoted to PRODUCT_MEMORY yet — nothing locked):
  - **C1 = C** — Time Player FF = resolve-time booking only; no Program-time preview.
  - **A1 = Bomber first** — first unique-verb Sim slice is Bomber (not Time Player / not defer).
  - **A2 = hardcoded archetype→verb map** in Boot (not SO ability-id list / not separate ability assets).
- Walk **paused at A3** (ActionVerb vs side channel).

## Verification

- No Unity batchmode this session (docs-only). Last green remains `7213d98` EditMode 149 / PlayMode 48.
- Decision-sheet answers exist only in working tree until committed.

## Still unfinished

- **Finish decision-sheet walk** — resume **A3 → A5**, then Part B (Bomber), rest of Part C, Part D (attrs). Then Fantasy §6 + Epistemics §4 framing if not covered.
- Promote finished answers → `PRODUCT_MEMORY` C-rows (Integrator) + open Sim carve-out — **blocked until sheet complete**.
- **Bandage HUD** (UI / open contract): dock `GearHandView` → `ProgramHud`, timeline place, 3 legality gates.
- **Healed presenter** (Integrator after HUD); dirty rematch/floors/lighting on main (incl. `BandageCharge` reset).
- Atmosphere merge after human Play; Cards optional C64 catalog / OPEN menu; Flashbang paused.
- Interact station; Adrenaline PLAYBACK redesign; Phase 2 Net paused.
- Older unmonitored: door tape Open second; south-edge Move-click; zoom-fill/soft-rain/reflections.

## Tomorrow

1. **Character:** say “walk the sheet” → continue at **A3** (simple-language prompts). After sheet done, ask Integrator to promote answers.
2. Other seats (unchanged priority): UI Bandage HUD; Integrator INDEX tips + rematch commit when asked; Atmosphere Play→merge; Cards docs-only if staffed.

## Blockers / notes

- Character still **pre-code** — no Sim resolve until human finishes sheet + Integrator carve-out.
- Dirty uncommitted: `CHARACTER_DECISION_SHEET.md` only (commit when asked).
- Main Editor lock → batchmode on other worktrees; ≤2 coding-hot.
- C64 does not unlock deckbuilder coding.
- Untracked junk historically: `Assets/ExplosiveLLC/`, screenshots — human keep/delete.
- Prior handoff archived: `docs/drafts/2026-08-13.md`.
- No push unless asked.
