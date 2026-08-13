# UI — STATUS

**Wave / Day:** Full UI ownership — chrome collection (active)
**Branch / worktree:** `feat/modal-restyle` @ `D:\projects\Game\logiCard-modal-restyle`
**Mandate:** All UI surfaces (lobby, Character Select, Map Select, HUD/dock, modals).
**Last cross-reviewed:** 2026-08-13 — collection push; gap matrix in `UI_CHROME_COLLECTION.md`

## Owned files (this seat)

- `Assets/_Project/UI/**` (ModalDialog, GearHandView, Modal* tokens on this branch; inherit rest on merge)
- `docs/UI_CHROME_COLLECTION.md` + `docs/ui-collection/**`
- `docs/UI_STACK_COMPARISON.md`, mirrored Toolkit proposal / Kenney THIRD_PARTY
- `docs/departments/ui/STATUS.md`

## Done

- Modal cardstock — human Play signed off.
- C62 `GearHandView` scaffold — not dock-wired.
- Stack: uGUI; Toolkit parked.
- Collection process live. Catalogued: specials, deck motion, buttons, loader, **Iomanoid CC0 display font**, **normal-card**, **resource-bank-card-flip**, first icon **`icon_bandage.png`**.

## In progress

- Chrome collection — see gap matrix in `UI_CHROME_COLLECTION.md`.
- **Icons (5) started** — bandage sets the clay style; still need Interact / Flashbang / Adrenaline / stance×3 / Snap·Hold / door / wound / Lock In.
- **Still missing for stop bar:** rest of icons, in-match HUD chrome (**7**), lobby/shell layout refs (**8**), body font, warmer panel family if normal-card is kept.

## Blocked

- Stop bar not met → no Unity chrome import yet.
- Integrator merge of this branch + `feat/char-select-motion` (`UiStyle` overlap).

## Offers

- Categorize next human deliveries immediately. Say “collection complete for first chrome pass” only when stop bar is met.
