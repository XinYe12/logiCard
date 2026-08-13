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
- Collection process live. Catalogued: specials (holo ticket, glass, logo reveal), deck motion (wallet, comic hand), buttons (bubbles / glass pill / gradient pill), loader, **Iomanoid CC0 display font**, **normal-card** (bucket-2 candidate), **resource-bank-card-flip** (resource-card role).

## In progress

- Chrome collection — see gap matrix in `UI_CHROME_COLLECTION.md`.
- **Still missing for stop bar:** icons (**5**), in-match HUD chrome (**7**), lobby/shell layout refs (**8**), body/companion font (display = Iomanoid only), warmer panel family if normal-card is kept.

## Blocked

- Stop bar not met → no Unity chrome import yet.
- Integrator merge of this branch + `feat/char-select-motion` (`UiStyle` overlap).

## Offers

- Categorize next human deliveries immediately. Say “collection complete for first chrome pass” only when stop bar is met.
