# UI — STATUS

**Wave / Day:** Bandage HUD-side (C63) — **Ready for Integrator merge**; chrome collection resumes after
**Branch / worktree:** `feat/modal-restyle` @ `D:\projects\Game\logiCard-modal-restyle`
**Mandate:** All UI surfaces (lobby, Character Select, Map Select, HUD/dock, modals).
**Last cross-reviewed:** 2026-08-14 — Bandage HUD-side contract implemented against `BANDAGE_HUD_AGENT_BRIEF.md` / `docs/contracts/CURRENT.md`

## Owned files (this seat)

- `Assets/_Project/UI/**` (ModalDialog, GearHandView, ProgramHud, Modal* tokens on this branch; inherit rest on merge)
- `Assets/_Project/Timeline/PawnProgram.cs` (`TryQueueBandage`/`IsMidSprintAt`/`BandageSeconds` only — Bandage HUD-side carve-out, C63)
- `Assets/_Project/Board/BoardInputController.cs` (Bandage placement only)
- `Assets/_Project/Boot/RoundPlayback.cs` (`BandageChargeOf` reader only — avoided rematch methods per contract)
- `docs/UI_CHROME_COLLECTION.md` + `docs/ui-collection/**`
- `docs/UI_STACK_COMPARISON.md`, mirrored Toolkit proposal / Kenney THIRD_PARTY
- `docs/departments/ui/STATUS.md`

## Done

- **Bandage HUD-side (C63), Ready for Integrator merge** — `PawnProgram.TryQueueBandage`/`IsMidSprintAt`/`BandageSeconds`
  (mid-Sprint gate: strictly inside a Sprint leg only; arrival/departure instants are legal); `RoundPlayback.BandageChargeOf`
  mirroring `WoundsOf`; `BoardInputController.TryQueueBandageAt` + Bandage board-tap (nearest booked Move node's
  ExecuteTime, else schedule tip) via `ResolveBandageExecuteTime`; `ProgramHud` docks `GearHandView` into the queue
  column (top 40%, queue readout keeps the rest — `ControlsColumnContentHeight`/`ProgramHudLayoutTests` untouched),
  arms Bandage through the existing `SetMode` path (commits any pending Move draft first), places via board tap or
  scrubber click, auto-clears arm + returns Mode to Move on a successful place, and gates arming on Wounded +
  unused charge + no Bandage node already queued this Program (mid-Sprint is checked at place-time in `PawnProgram`
  itself). Interact/Flashbang permanently blocked via `GearHandView.SetSpent` (no contract this wave); Adrenaline
  untouched (existing `GearHandPhase` rules already gate it to Execute).
  Batchmode: EditMode **153/153**, PlayMode **49/49** (this worktree, Editor closed on it during the run).
  Commit: see branch log — "Bandage HUD-side: dock GearHandView, arm/place, budget+mid-Sprint+charge gates (C63)".
- Modal cardstock — human Play signed off.
- Stack: uGUI; Toolkit parked.
- Collection process live. Catalogued: specials, deck motion, buttons, loader, **Iomanoid CC0 display font**, **normal-card**, **resource-bank-card-flip**, first icon **`icon_bandage.png`**.

## Deviations from contract (flag for Integrator)

- **`RoundPlayback` reference avoided in `ProgramHud`** — `LogiCard.UI` cannot reference `LogiCard.Boot` (Boot already
  depends on UI, not the reverse), so `RegisterMatchState` takes `Func<int> woundsOf, Func<int> bandageChargeOf`
  delegates instead of a `RoundPlayback` param ("or equivalent" per the contract). **One-line `GameBootstrap` hook
  still needed** (not made — Integrator-dirty file per boundary):
  ```csharp
  _programHud.RegisterMatchState(
      () => _playback.WoundsOf(AttackerPawnId),
      () => _playback.BandageChargeOf(AttackerPawnId));
  ```
  Call after both `_playback` and `_programHud` exist. Until wired, Bandage stays safely blocked (grey/non-interactable)
  rather than guessing legal — confirmed via the PlayMode smoke test, which injects synthetic delegates directly.
- No other deviations — signatures match the frozen contract verbatim.

## In progress

- Chrome collection resumes now that Bandage HUD-side is Ready — see gap matrix in `UI_CHROME_COLLECTION.md`.
- **Icons (5) started** — bandage sets the clay style; still need Interact / Flashbang / Adrenaline / stance×3 / Snap·Hold / door / wound / Lock In.
- **Still missing for stop bar:** rest of icons, in-match HUD chrome (**7**), lobby/shell layout refs (**8**), body font, warmer panel family if normal-card is kept.

## Blocked

- Stop bar not met → no Unity chrome import yet.
- Integrator merge of this branch + `feat/char-select-motion` (`UiStyle` overlap).

## Offers

- Bandage HUD-side ready for Integrator merge whenever convenient — then Integrator's Healed presenter follow-up unblocks.
- Categorize next human deliveries immediately. Say “collection complete for first chrome pass” only when stop bar is met.
