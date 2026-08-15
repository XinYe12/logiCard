# UI — STATUS

**Wave / Day:** Match Shell Layout (2026-08-15, `MATCH_SHELL_LAYOUT_AGENT_BRIEF.md` /
`docs/ui/MATCH_SHELL_LAYOUT.md`) — **Ready for Integrator/human review**; supersedes the HUD Chrome
Ship Pass below (that chrome pass is not merging alone — this layout absorbs it, per the brief).
**Branch / worktree:** `feat/modal-restyle` @ `D:\projects\Game\logiCard-modal-restyle`
**Mandate:** All UI surfaces (lobby, Character Select, Map Select, HUD/dock, modals).
**Last cross-reviewed:** 2026-08-14 — merged `master` to pick up Storm Sim-side (C67) before building UI-side
against `docs/contracts/CURRENT.md`'s Storm contract.

## Done — Match Shell Layout (2026-08-15)

Restructured the in-match HUD from the old 3-column bottom dock into the locked vertical stack:
**InfoBar → MapViewport → HandBand → ToolBar → TimelineSchedule** (`docs/ui/MATCH_SHELL_LAYOUT.md`).
Layout/region geometry only — no card-face/icon/button-art restyle this wave. Scoped entirely to
`Assets/_Project/UI/ProgramHud.cs` + its EditMode/PlayMode tests; `GearHandView.cs`/`UiStyle.cs`/
`UiFactory.cs` untouched (`GearHandView.Build` already took an arbitrary parent rect, so it slots into
the shorter, full-width HandBand unchanged).

- **Five full-width bands**, top→bottom, each its own named GameObject: `InfoBar` (0.07),
  `MapViewport` (0.48, the largest — enforced by an EditMode test), `HandBand` (0.14), `ToolBar`
  (0.17), `TimelineSchedule` (0.14). `HandBand`+`ToolBar`+`TimelineSchedule` live inside one
  `BottomStack` GameObject (renamed from the old dock's `HudDock`) so Allot/Aftermath can still
  overlay the same rect by phase, same mutually-exclusive `SetActive` pattern as before.
- **`ProgramHud.MapViewport`** (new public `RectTransform`) exposes the empty UI hole — deliberately
  no `Image` component, so the 3D board shows through undecorated — for Camera/Integrator to letterbox
  `cam.rect` to directly in a later pass. This wave keeps `GameBootstrap.ConfigureCamera` untouched
  (not this seat's file): `HudDockHeight`/`TopStripHeight` are kept as public constants (repurposed,
  not renamed) and now equal the bottom-stack total / InfoBar height respectively, so the camera rect
  they already compute is numerically identical to the new `MapViewport` rect — zero Boot/Camera edits
  needed to keep the board framed correctly.
- **InfoBar** carries phase, round+chooser (existing `MatchLabel`), a new **wounds** field (stub
  `WOUNDS —` until `RegisterMatchState` wires a real read, refreshed in `Update()`), and a new **TR
  used/budget** field (`TrLabel`, refreshed in `OnQueueChanged` from the same `Program.UsedSeconds/
  BudgetSeconds` the queue readout already tracked) — no invented HP/mana per the doc's content
  contract. The camera-rotate hint moved from the old TopStrip into a non-interactive
  (`raycastTarget=false`) overlay inside MapViewport itself.
- **ToolBar** merges the old dock's ControlsColumn+ActionColumn side by side in one full-width row
  (verb+stance/shoot/door context left ~62%, Lock In/Adrenaline+transport right ~38%) instead of two
  of three narrow dock columns. Compacted to fit the shorter band: SET PATH folded into the stance
  row as a fourth split cell (was its own row below SPRINT/WALK/CRAWL); row height constants shrunk
  (verb 48→40, stance/context 44→34, action 56→40, etc.). All existing GameObject names
  (`Mode_Move/Shoot/Door`, `Stance_Sprint/Walk/Crawl`, `SetPathButton`, `Shoot_Snap/Hold`,
  `LockInButton`, `AdrenalineButton`, transport buttons) are unchanged, so no PlayMode test needed a
  behavioral rewrite — only relocation.
- **TimelineSchedule** is the Time Resource cinema as a schedule shell: a YOU track (the *same*
  `Scrubber` Slider + per-node colored markers the old dock used, just relocated and relabeled — so
  "scrubbing must work" holds by construction, not by new code) plus stub ENEMY (`locked until
  Reveal`) and EFFECTS rows, and the queue readout migrated out of HandColumn into a right-hand
  compact panel (same `QueueReadout` GameObject name/content logic, unchanged). Expand/collapse and
  ticket-style chip chrome are explicitly stub-OK this wave per the doc — not built.
- **HandBand** now gets the whole band (no more `GearHandAreaMinY` split with a queue-log strip) since
  the queue readout moved to TimelineSchedule — `GearHandView` unchanged.
- **New EditMode coverage** (`ProgramHudLayoutTests.cs`, rewritten): band fractions sum to 1 and
  MapViewport is strictly largest; `HudDockHeight`/`TopStripHeight` stay numerically equal to the
  MapViewport rect (the camera-compat contract above); ToolBar/TimelineSchedule content-height budgets
  fit their bands' UI-unit size at 1920×1080 / 1280×720 / 2560×1080 (ultrawide is tightest — all three
  budgets clear it with margin). `AppFlowPlayModeTests.cs`'s two dock-geometry assertions updated to
  the new band names/constants relationship instead of the old hardcoded `0.34f`.
- **Batchmode (this worktree, Editor closed on it before running): EditMode 174/174, PlayMode
  53/53 — 0 failures**, including all Hand Deck Drag Play tests
  (`GearBandageDragOutOfHandPlacesABandageNode`, `GearStormDragOutOfHandPlacesAStormNode`, short-drag
  and already-queued no-op cases) and `QueueReadoutShowsDraftThenCommittedActions` unchanged.

### Deviations from the brief (flag for Integrator/human)

- **ToolBar fits in one context row instead of two** — SET PATH now shares a 4-way split cell with
  SPRINT/WALK/CRAWL rather than sitting on its own row below them; functional, slightly denser than
  the old dock. Flagging as a deliberate compaction call (the shorter full-width ToolBar band has less
  vertical room than the old narrow ControlsColumn had), not a missed spec item.
- **ENEMY/EFFECTS TimelineSchedule rows are static stub text**, not wired to ghost-tape reveal or
  live wound/weather/door state — the brief marks this explicitly stub-OK this wave ("mechanize
  first" applies to YOU/playhead, which is real; "toy later" for the rest).
- **Camera letterboxing still reads `HudDockHeight`/`TopStripHeight`, not `ProgramHud.MapViewport`
  directly** — numerically identical today by construction (see above), but if a future pass makes
  MapViewport asymmetric (e.g. side margins), `GameBootstrap.ConfigureCamera` will need updating to
  read the rect directly instead. Flagging so that coupling isn't forgotten.
- Expand/collapse toggle for TimelineSchedule (image 18 ↔ 19 in the brief) not built — brief listed it
  as optional nice-to-have this wave.

## Previous wave

## Owned files (this seat)

- `Assets/_Project/UI/**` (ModalDialog, GearHandView, ProgramHud, Modal* tokens on this branch; inherit rest on merge)
- `Assets/_Project/Timeline/PawnProgram.cs` (`TryQueueBandage`/`IsMidSprintAt`/`BandageSeconds` + `TryQueueStorm`/`StormSeconds` — Bandage C63 + Storm C67 carve-outs)
- `Assets/_Project/Board/BoardInputController.cs` (Bandage + Storm placement only)
- `Assets/_Project/Boot/RoundPlayback.cs` (`BandageChargeOf` reader only — avoided rematch methods per contract)
- `docs/UI_CHROME_COLLECTION.md` + `docs/ui-collection/**`
- `docs/UI_STACK_COMPARISON.md`, mirrored Toolkit proposal / Kenney THIRD_PARTY
- `docs/departments/ui/STATUS.md`

## Done

- **HUD Chrome Ship Pass, Ready for Integrator/human review** (`HUD_CHROME_SHIP_PASS_AGENT_BRIEF.md`,
  human playtest ask 2026-08-15: "the shadow needs to be fixed... the current UI is unplayable... the
  cards and the bottom timeline and things needs to be separated"). Chrome only — drag-to-play logic,
  fan positions, and the 26/48/26 column fractions are untouched. Scoped to `GearHandView.cs`,
  `ProgramHud.cs`, `UiStyle.cs`, `UiFactory.cs`.
  1. **Every dock region now has a real backing panel** instead of a flat color abutting its
     neighbor. New `UiFactory.CreateBackingPanel` ports the layered-`box-shadow` technique from
     `docs/ui-collection/normal-card.css` (Uiverse.io by adamgiebl, MIT — tagged "Candidate —
     default card" in `UI_CHROME_COLLECTION.md`): a shadow layer, a border/rim layer, a face layer,
     and a thin inset-color lip along the face's bottom edge — all retinted through five new
     `UiStyle.DockPanel*` tokens (warm dark family, not the demo's cool grey) rather than forking a
     second token system. `ProgramHud.BuildDockRegionPanel` wraps it with fixed 8px/4px margins so
     the shadow/border layers stay inside their own zone and can't bleed into a neighboring column —
     `ControlsColumn`, `ActionColumn`, the hand's own panel, and the queue-log strip (previously a
     bare `PanelSunken` rect) all get one now.
  2. **The hand's fan is clipped to its own panel** — `GearHandView.Build` adds a `FanClip` child
     (`RectMask2D`) one level below `Root`; card cells parent under `FanClip` instead of directly
     under `Root`, so the fan's overlap-widen math can never visually cross into ControlsColumn/
     ActionColumn regardless of rotation/droop. The brief flagged the obvious conflict with the
     drag-to-play gesture (a clipped card can't be dragged out) — resolved by having
     `CardDragController.OnBeginDrag` reparent the picked-up cell from `FanClip` up to `Root`
     (unmasked, coincides with `FanClip`'s exact rect so no anchoredPosition/visual jump) and
     `OnEndDrag` reparent it back before restoring position/rotation/sibling-index, whether the drag
     committed or cancelled. Resting cards stay clipped; the one card actually being dragged never is.
  3. **Card face stack deepened** the same normal-card.css way: a second, softer, farther-thrown
     `CardShadowFar` layer added behind the existing `CardShadow` contact shadow (new
     `UiStyle.ModalShadowFar` token), plus a `CardInsetLip` strip along each card's bottom edge (new
     `UiStyle.ModalCardInsetLip` token, inset a few px from the card's sides so it stays inside the
     rounded-corner footprint). Card face/border/ink colors themselves are unchanged — the added
     panel backing + deeper shadow stack is what gives them contrast now, not a palette change.
  - **Not done — flagged as a human call, not blocking:** actually reskinning `HudDock`'s own
    `PanelDark` backdrop, or porting `wallet-card-holder`/`hands-deck-comic-swatches`' hover-forward
    motion — the brief's ask was specifically panel boundaries + fan clipping + card pop; broader
    reskin stayed out of scope to keep this a chrome-only pass, not a second interaction/motion
    rework riding along with it.
  - **Tests:** no test edits needed — `GearHandViewTests`/`ProgramHudLayoutTests` assert structure/
    behavior (button names, drag-play semantics, the `ControlsColumnContentHeight` budget), not pixel
    colors or exact panel geometry, and none of those contracts changed. The 8px/4px panel margins
    were sized against `ControlsColumnContentHeight`'s existing headroom (~24 UI units of slack at
    2560×1080 ultrawide, the tightest case) to stay inside it rather than risk overflow.
  Batchmode: EditMode **173/173**, PlayMode **53/53** (this worktree; confirmed no other `Unity.exe`
  held this worktree's path before running — the one other running instance was batchmode PlayMode
  tests on the separate `logiCard-map` worktree).
  - **Human Play-mode pass is still the sign-off this brief itself calls for** — "does this look
    mature enough to ship" wasn't verifiable through batchmode (no Game View compositing in
    `-runTests`, same limitation noted under Hand Deck Drag Play below). Reporting back for that now
    rather than polishing further unverified.

- **Hand Deck Drag Play, Ready for Integrator/human review** (`HAND_DECK_DRAG_PLAY_AGENT_BRIEF.md`, human
  playtest ask 2026-08-15: "cut the unused space at the bottom, give the cards a separate space... hand
  deck style like hearthstone... play the card means tap→hold→drag it out of the handdeck area→release").
  Two changes, both scoped to `GearHandView.cs`/`ProgramHud.cs`:
  1. **Dock layout rework.** `BuildProgramControls`'s three dock columns changed from
     `ControlsColumn 0–38% / QueueColumn 38–70% / ActionColumn 70–100%` to
     `ControlsColumn 0–26% / HandColumn 26–74% / ActionColumn 74–100%` — the hand's column nearly doubled
     (32%→48% of the dock's width). Inside that column, `GearHandAreaMinY` flipped from `0.60` (hand got
     the top 40%, queue log got the bottom 60% — a mostly-empty box once you'd queued 1-2 nodes, the
     human's literal complaint) to `0.20` (hand now gets the top 80%, queue log a slim 20% strip with a
     `RectMask2D` so overflow clips instead of bleeding past the dock's bottom edge). `ControlsColumn`'s
     row budget (`ControlsColumnContentHeight`) is unchanged — it's a stacked-height sum, not
     width-dependent, so `ProgramHudLayoutTests` needed no edits (verified: still 173/173 green).
  2. **Hearthstone-style fan.** `GearHandView.Build` widens each of the 5 card slots to ~1/5×1.18 of the
     hand's width (evenly stepped so the first card's left edge sits at 0 and the last's right edge at 1,
     regardless of count) instead of an even 1/5 split with a 2% gutter — neighbors now overlap ~18%
     instead of sitting in separate boxes. Each card also gets a small per-step rotation (±5°×step from
     center) and vertical droop (9px×|step| from center) — a simplified center-pivot fan rather than a
     bottom-pivot rig, cosmetic only and doesn't touch drag math (thresholds/bounds are measured against
     `Root`, not the rotated cell). Cards are also taller now (5% vertical inset vs. the old 6%/94% split
     inside a much shorter column before).
  3. **Drag-to-play replaces click-to-arm for Bandage/Storm only** (Interact/Flashbang/Adrenaline
     untouched — still the original click-to-arm scaffold; Interact/Flashbang stay permanently blocked,
     Adrenaline's real trigger stays the separate `AdrenalineButton`). Bandage/Storm no longer register an
     `onClick` listener at all — instead each gets a `GearHandView.CardDragController` (nested public
     class implementing `IPointerDownHandler`/`IBeginDragHandler`/`IDragHandler`/`IEndDragHandler`, living
     on the card's cell so drag resolution finds it once `Button`/`Selectable` declines — it never
     implements drag interfaces). Drag math: `OnDrag` converts the screen-space pointer delta into the
     hand's local space via `RectTransformUtility.ScreenPointToLocalPointInRectangle` (correct under the
     CanvasScaler, not a raw 1:1 pixel-to-unit assumption); `OnEndDrag` checks
     `Vector2.Distance(pressPosition, position) >= GearHandView.DragThresholdPixels` (60px) AND
     `!RectTransformUtility.RectangleContainsScreenPoint(Root, position, camera)` — both must hold to
     raise the new `CardPlayRequested` event; either condition failing snaps the card back with no event
     at all (a plain click below threshold never even reaches `OnBeginDrag`, since Unity's own drag
     threshold gates it first). `ProgramHud.OnGearCardDragPlay` is the single subscriber — it calls
     `_input.TryQueueBandageAt`/`TryQueueStormAt(_clock.CurrentSeconds, out _)`, the exact same Sim-adjacent
     entry points the old scrubber-tap path called, just triggered by the drop instead of a slider drag.
     Success/failure feedback reuses existing plumbing unchanged: a queued node fires `QueueChanged` →
     `RefreshGearHandLegality` → `SetSpent`, which greys the card (no new "played" animation — noted as a
     nice-to-have, not done); a rejection fires the existing `BoardInputController.ActionRejected` → HUD
     outcome banner. The card's position/rotation snap back to rest unconditionally in `OnEndDrag` before
     either outcome runs, so a rejected play never leaves a card stranded off-hand.
  4. **Removed as dead once Mode is never set to Bandage/Storm from the HUD again:** `OnGearCardArmed`/
     `OnGearArmCleared` (ProgramHud), `_bandageModeControls`/`_stormModeControls` GameObjects + labels +
     their `RefreshBandageModeControls`/`RefreshStormModeControls` methods, and `OnScrubberMoved`'s
     Bandage/Storm placement branch (the brief explicitly called this one out — two ways to trigger the
     same queue call in different states). `_input.Mode` now only ever takes Move/Shoot/Door from the HUD;
     `ActionVerb.Bandage`/`.Storm` still exist and are still read (node-verb tagging, `HasNodeQueued`),
     just never written by `ProgramHud` anymore.
  - **Flagged for Integrator, not fixed (out of this seat's file scope):**
    `BoardInputController.TryTapPoint`'s Bandage-mode board-tap branch (`ResolveBandageExecuteTime`) is now
    unreachable from the HUD in practice, since nothing sets `Mode = ActionVerb.Bandage` anymore. Left
    alone per the brief's boundary (Sim/Net-adjacent, frozen contract, call-don't-change) — flagging in
    case Integrator wants to prune the now-dead branch in a later pass.
  - **Tests updated:** `GearHandViewTests.cs` — `ProgramPhaseArmsBandageAndIgnoresAdrenaline` /
    `ArmedCardUsesModalPrimaryFace` re-based on `CardId.Interact` (Bandage no longer arms via click);
    added `ClickingBandageOrStormDoesNothing`, `BandageAndStormCarryADragControllerOthersDoNot`,
    `DragPastThresholdAndReleaseOutsideHandRequestsPlay`, `ShortDragCancelsWithNoPlayRequest`,
    `PastThresholdDragReleasedInsideHandCancels`, `DragOnSpentCardIsANoOp`,
    `BeginDragHighlightsTheCardLikeAnArmedOne` — drive `CardDragController.OnBeginDrag`/`OnDrag`/
    `OnEndDrag` directly with hand-built `PointerEventData`, per the brief's suggested approach (no real
    OS input, no live `EventSystem` needed since we call the interface methods directly).
    `ProgramHudPlayModeTests.cs` — `GearBandageArmsThenBoardTapPlacesABandageNode` /
    `GearStormArmsThenScrubberPlacesAStormNode` rewritten as `GearBandageDragOutOfHandPlacesABandageNode`
    / `GearStormDragOutOfHandPlacesAStormNode` (drag through the real built HUD/`BoardInputController`
    instead of click+board-tap/direct `TryQueueStormAt`); added
    `ShortDragOnBandageDoesNotQueueAndCardStaysInteractable` and `DragOnAlreadyQueuedStormCardIsANoOp` for
    the brief's minimum-coverage list (a/b/c: commit, cancel, blocked-card no-op).
  - **Bug found + fixed during batchmode verification:** `CardDragController` originally cached its
    `RectTransform` in `Awake()` — `NullReferenceException` in the 4 new EditMode drag tests, because
    `AddComponent` doesn't reliably run `Awake` before a directly-invoked interface method in EditMode
    (no live player loop tick between `AddComponent` and the test calling `OnBeginDrag`). Fixed with a
    lazily-resolved `Rect` property instead of an `Awake`-cached field.
  - **Screenshot capture attempted, not usable:** wrote a temporary PlayMode test calling
    `ScreenCapture.CaptureScreenshot` at rest and mid-drag; the test passed but produced no PNG — batchmode
    `-runTests` doesn't actually composite a Game View window to capture (confirmed a D3D12 device exists
    in the log; the missing piece is the window surface, not the GPU). Deleted the temp test rather than
    leave a screenshot-less stub in the suite. **Human Play-mode pass is the only way to sign off on the
    fan/spacing/hover feel** — this was already flagged as unavoidable in the brief itself.
  Batchmode: EditMode **173/173**, PlayMode **53/53** (this worktree, Editor closed on it during the run —
  confirmed the only two `Unity.exe` processes present belonged to the main `D:\projects\Game\logiCard`
  checkout, not this worktree, before running).

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

- **Hand Deck Drag Play (2026-08-15): `BoardInputController.TryTapPoint`'s Bandage-mode board-tap branch
  is now unreachable from the HUD** — drag-to-play never sets `Mode = ActionVerb.Bandage` (see Done above),
  so the `ResolveBandageExecuteTime` board-tap path in that Sim/Net-adjacent file has no caller left in
  production. Left alone (out of this seat's file scope, frozen contract) — flagging in case Integrator
  wants to prune it in a later pass rather than carry dead-in-practice code indefinitely.
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
