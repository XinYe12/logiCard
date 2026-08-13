# UI — STATUS

**Wave / Day:** Full UI ownership — stack research (docs only)
**Branch / worktree:** `feat/modal-restyle` @ `D:\projects\Game\logiCard-modal-restyle`
**Mandate:** All UI surfaces (lobby, Character Select, Map Select, HUD/dock, modals). Research before new builds.
**Last cross-reviewed:** 2026-08-13 — synced Character Select tip (`feat/char-select-motion` @ `a707d9f`)

## Owned files (this seat)

- Entire `Assets/_Project/UI/**` (once Integrator merges sibling branches) — currently on this branch: `ModalDialog`, `Modal*` tokens, `GearHandView`
- `docs/UI_STACK_COMPARISON.md` (new — recommendation)
- Mirrored for self-contained research: `docs/UI_TOOLKIT_MIGRATION_PROPOSAL.md`, `Assets/_Project/Art/UI/THIRD_PARTY.md` (sourced from Character seat; no sprite import on this branch yet)
- `docs/departments/ui/STATUS.md`

## Done

- Modal cardstock restyle — human Play signed off (Match Over → Quit → confirm).
- C62 `GearHandView` UI-only scaffold (`54ae286`) — not dock-wired.
- **Stack comparison delivered** (`docs/UI_STACK_COMPARISON.md`): Toolkit vs continued uGUI vs third-party chrome/libs.
  - **Recommendation:** stay on **uGUI**; expand **Kenney “UI Pack - Adventure”** (already chosen on Character branch); park Toolkit after failed visual pilot; no third-party UI runtime.
- Synced Character latest: Toolkit pilot reverted (`a915bb7`); Kenney Adventure skin on uGUI carousel; awaiting human Play on parchment look; branch 5 ahead / 7 behind master.

## In progress

- Nothing code-side. Waiting on human confirm of `UI_STACK_COMPARISON.md` recommendation before building.

## Blocked

- Human: confirm or amend the stack recommendation.
- Integrator: merge `feat/modal-restyle` + `feat/char-select-motion` (order TBD; both touch `UiStyle`) so this seat can inherit Character Select without dual ownership.

## Offers

- After recommendation confirm: Kenney chrome pass on Modal/Lobby/Map, or dock-parent `GearHandView`, or TMP — whichever Integrator briefs first.
