# UI — STATUS

**Wave / Day:** Bandage HUD-side (C63) + Storm HUD-side (C67) — **both Ready for Integrator merge**; chrome
collection resumes after.
**Branch / worktree:** `feat/modal-restyle` @ `D:\projects\Game\logiCard-modal-restyle`
**Mandate:** All UI surfaces (lobby, Character Select, Map Select, HUD/dock, modals).
**Last cross-reviewed:** 2026-08-14 — merged `master` to pick up Storm Sim-side (C67) before building UI-side
against `docs/contracts/CURRENT.md`'s Storm contract.

## Owned files (this seat)

- `Assets/_Project/UI/**` (ModalDialog, GearHandView, ProgramHud, Modal* tokens on this branch; inherit rest on merge)
- `Assets/_Project/Timeline/PawnProgram.cs` (`TryQueueBandage`/`IsMidSprintAt`/`BandageSeconds` + `TryQueueStorm`/`StormSeconds` — Bandage C63 + Storm C67 carve-outs)
- `Assets/_Project/Board/BoardInputController.cs` (Bandage + Storm placement only)
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
- **Storm HUD-side (C67), Ready for Integrator merge** — merged `master` first to pick up Sim-side
  (`ActionVerb.Storm`, `CardId.Storm = 4`, `TapeEventType.StormCast`, `GhostResolverStormTests`) already
  landed there. `PawnProgram.TryQueueStorm(executeTime, out reason)` mirrors `TryQueueDoor`/`TryQueueShoot`'s
  guard-first shape (commits pending draft first); no Sprint gate, no board position. `StormSeconds = 0f`
  — Cards' numerics recommendation is still an OPEN "TR —" placeholder, so this is a non-inventing default
  (not a locked balance number), same two-step C62→C63 shape as Bandage. `BoardInputController.TryQueueStormAt`
  mirrors `TryQueueBandageAt` exactly — Storm has **no board-tap path at all** (self-targeting, nothing to
  aim at), the scrubber-click path is the only way in. `ProgramHud` docks a fifth `GearHandView.FirstWave`
  slot, arms via the existing `SetMode`/`OnGearCardArmed` path (extended for both Bandage and Storm), places
  via scrubber click only, auto-clears arm + returns Mode to Move on success, and gates arming on "no Storm
  node already queued this Program" (`RefreshGearHandLegality`'s `HasNodeQueued` helper, generalized from
  the old Bandage-only `HasBandageNodeQueued`). `GearHandViewTests`'s `FirstWaveRosterIs...` hard-coded
  roster test updated (DoD item 4) — renamed + extended to assert 5 entries ending in `CardId.Storm`; the
  file's other tests iterate `FirstWave` generically and needed no changes, as the contract predicted.
  Batchmode: EditMode **166/166**, PlayMode **51/51** (this worktree, real run — `D:\unity\Editor\6000.5.5f1\Editor\Unity.exe -batchmode -runTests`, no `-quit` combined with `-runTests` this time; the first attempt used `-quit` alongside `-runTests` and exited before writing results, a known bad combo).
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
- **Storm's "once-per-match" gate is per-Program (this round) only, not true cross-round once-per-match** —
  `RoundPlayback` has no `StormCastCountOf`-style reader the way it does `BandageChargeOf`, and the frozen
  contract's Sim-side is fully permissive by design (no once-per-match re-check in `GhostResolver`, confirmed
  by `GhostResolverStormTests`). A new round resets the gate. If true once-per-match across rounds is wanted,
  it needs a Sim/RoundPlayback counter added — flagging rather than silently claiming full compliance.
- No other deviations — signatures match the frozen contract verbatim.

## In progress

- Chrome collection resumes now that Bandage + Storm HUD-side are both Ready — see gap matrix in `UI_CHROME_COLLECTION.md`.
- **Icons (5 of 10 collected)** — `icon_bandage` (style lock), `icon_flashbang`, `icon_adrenaline`,
  `icon_stance_stand`, `icon_stance_crouch` all match the lock. `icon_interact_draft01` rejected (style-lock
  mismatch — glossy render, grey hand, blue accent), superseded by a tightened prompt; not yet regenerated.
  Still need: Interact (retry), stance-prone, Snap/Hold, door, wound, Lock In.
- **Still missing for stop bar:** rest of icons, in-match HUD chrome (**7**), lobby/shell layout refs (**8**), body font, warmer panel family if normal-card is kept.

## Blocked

- Stop bar not met → no Unity chrome import yet.
- Integrator merge of this branch + `feat/char-select-motion` (`UiStyle` overlap).

## Offers

- Bandage + Storm HUD-side both ready for Integrator merge whenever convenient — then Integrator's Healed
  presenter follow-up (Bandage) and Cards/Atmosphere seats' own Storm DoD items (still open on their
  branches) unblock.
- Categorize next human deliveries immediately. Say “collection complete for first chrome pass” only when stop bar is met.
