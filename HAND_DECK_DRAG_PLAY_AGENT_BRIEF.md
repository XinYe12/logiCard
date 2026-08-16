# Agent Brief — Hearthstone-style hand deck + drag-to-play

**From:** Integrator **To:** UI seat (`feat/modal-restyle`, this worktree)
**Priority:** Feature, human-requested 2026-08-15 while playtesting.
**Scope:** `Assets/_Project/UI/GearHandView.cs`, `Assets/_Project/UI/ProgramHud.cs` (HUD dock layout
+ gear-hand wiring), `Assets/_Project/Tests/EditMode/ProgramHudLayoutTests.cs`. Stay inside these —
don't touch Sim/Net/`PawnProgram`/`BoardInputController` public surface (see Contract below, they're
consumed, not changed).

## The ask (human's words)

> cut the unused space at the bottom, give the cards a separate space so that i can have the hand
> deck style like hearthstone. play the card means, user tap-> hold -> drag it out of the handdeck
> area -> release, to play, not like right now it is fixed as a button. play the card to switch to
> storm weather

Clarified with the human: **both Storm and Bandage** get the drag-to-play gesture (not Storm only).
Release = queue at whatever the Time scrubber currently reads — no second step, no re-arming, no
tap-the-scrubber follow-up. This replaces the current click-to-arm-then-scrubber-tap two-step
entirely for these two cards.

## Current state (what you're replacing)

`Assets/_Project/UI/ProgramHud.cs`:
- `BuildProgramControls` splits the bottom `HudDock` (bottom `HudDockHeight` = 34% of screen) into
  three columns: `ControlsColumn` (0–38%, scrubber/verb/stance), `QueueColumn` (38–70%), `ActionColumn`
  (70–100%, Lock In / transport).
- `BuildGearHand(queueCol)` (line ~499) docks `GearHandView` into the **top 40%** of `QueueColumn`
  (`GearHandAreaMinY = 0.60f`, `Build(..., new Vector2(0f, GearHandAreaMinY), Vector2.one)`), leaving
  the **bottom 60%** of that same column to `BuildQueuePanel` — a text readout (`_queueText`,
  "Used X.Xs / Y.Ys" + one line per booked node) that's mostly empty dead space once you've only
  queued 1–2 nodes. **This is the "unused space at the bottom" the human means** — it's fixed-height
  and doesn't shrink to fit content.
- `GearHandView.Build` (in `GearHandView.cs`) lays out 5 card slots as equal-width `Button`s in a
  single horizontal strip inside whatever rect it's given (currently ~32% of screen width, squeezed).
  `OnCardClicked` (line 253) is the entire play trigger today: click arms (`_armedId = id`,
  `CardArmed?.Invoke(id)`), click again cancels. Arming Bandage/Storm calls
  `ProgramHud.OnGearCardArmed` → `SetMode(ActionVerb.Bandage/.Storm)`, which shows a
  `_bandageModeControls`/`_stormModeControls` hint panel and switches `BoardInputController.Mode`.
  Placement currently happens on a **later, separate** interaction: `ProgramHud.OnScrubberMoved`
  (line ~1560) checks `_gearHand.ArmedId` and calls `_input.TryQueueBandageAt(_clock.CurrentSeconds, ...)`
  / `_input.TryQueueStormAt(_clock.CurrentSeconds, ...)` when the player drags the **scrubber widget**
  — a different control than the card itself. `OnGearArmCleared` (line 895) resets `Mode` back to
  `Move` once the arm clears (place succeeds, manual re-click, or phase swap).

## What to build

### 1. Reclaim the dead space, give cards their own band

Don't keep splitting `QueueColumn` 40/60 with the hand. Give the gear hand a **dedicated horizontal
band** with real room to fan cards Hearthstone-style (bigger cards, slight overlap/fan, hover-lift is
a nice-to-have but not required for v1). Shrink the queue log to fit its actual content instead of a
fixed 60%-of-column block — a compact strip that grows with entries up to a cap, not a mostly-empty
box. Where exactly the reclaimed space goes (widen the hand to span more of the dock's width vs. just
taller within `QueueColumn`) is your call as the design owner here — optimize for "hand reads like a
hand," not for preserving the current column boundaries. `ProgramHudLayoutTests.cs` pins
`ControlsColumnContentHeight` / `DockHeightInUiUnits` invariants — read it before you touch dock
geometry, and update it (don't just make it pass by accident) if your new layout changes what it's
asserting about.

### 2. Press → hold → drag-out → release to play

Replace `OnCardClicked`'s arm/click behavior for **Bandage and Storm specifically** (Interact/
Flashbang/Adrenaline aren't wired this wave — leave their `SetSpent(..., true)` blocked state alone)
with a real drag gesture on the card's `Button`/`RectTransform`:

- Implement `IPointerDownHandler`, `IBeginDragHandler`, `IDragHandler`, `IEndDragHandler` (or
  `IPointerUpHandler`) on a small helper component attached per-card, or a shared drag controller
  GearHandView owns — your call, but keep `GearHandView`'s existing public surface
  (`ArmedId`/`CardArmed`/`ArmCleared`/`SetSpent`/`IsInteractable`) intact or cleanly adapted, since
  `ProgramHud` depends on all of it (see Contract below).
- **Drag threshold + released outside the hand's bounds** → play: call the same underlying queue
  function that `OnScrubberMoved` calls today (`_input.TryQueueBandageAt(_clock.CurrentSeconds, out
  reason)` / `_input.TryQueueStormAt(_clock.CurrentSeconds, out reason)`), at the moment of release.
  On success, the card should animate back to the hand or fade — whatever reads as "played," it's
  gone from the hand slot until `SetSpent` clears it again (once-per-match) or state allows re-arming.
  On failure (`queued == false`, e.g. illegal), surface `reason` the same way `TryTapPoint` does today
  (`ActionRejected` on `BoardInputController`, or an equivalent) and snap the card back to its hand
  position — don't leave it stranded off-hand.
- **Released back inside the hand area, or drag distance below threshold** → cancel, card snaps back,
  no queue call, no rejection toast (this is just "I picked it up and put it back," not a failed play
  attempt).
- You no longer need the old two-step arm-then-wait-for-scrubber state for these two cards. Decide
  whether `SetMode(ActionVerb.Bandage/.Storm)` / the `_bandageModeControls`/`_stormModeControls` hint
  panels still serve a purpose (e.g. showing "STORM — drag to play" context text while a drag is in
  progress) or should go away with the old flow — your call, but don't leave dead UI state that never
  gets entered or exited.
- `OnScrubberMoved`'s Bandage/Storm placement branch (line ~1581) becomes redundant once drag-to-play
  is the real path — remove it rather than leaving two ways to trigger the same queue call in
  different states.

### 3. Contract — don't change these call signatures

`BoardInputController.TryQueueBandageAt(float, out string)` / `TryQueueStormAt(float, out string)`,
`PawnProgram.TryQueueBandage`/`TryQueueStorm`, `RefreshGearHandLegality`'s `SetSpent` gating, and
`BoardInputController.ActionRejected` are Integrator-owned Sim/Net-adjacent wiring (frozen Storm/
Bandage contracts, C63/C67) — call them, don't change their shape. If the drag gesture needs
something none of them currently expose, stop and flag it back to Integrator rather than reaching
into `PawnProgram`/`RoundPlayback` yourself.

## Testing

Unity lets you drive `IPointerDownHandler`/`IDragHandler`/`IEndDragHandler` directly by calling their
interface methods with a hand-built `PointerEventData` — no real OS input needed. Add PlayMode
coverage (mirror the existing `BoardWeatherPocketPlayModeTests.cs` / gear PlayMode smoke style in
this repo) for at minimum: (a) drag-past-threshold + release outside hand → queue call fires and the
card leaves the hand; (b) short drag / release inside hand → no queue call, card stays; (c) drag on a
`SetSpent`-blocked card → no-op, matching today's `IsInteractable` gate. Update
`ProgramHudLayoutTests.cs` for whatever geometry constants you change.

**Visual feel is a human call, not something batchmode can verify** — get it compiling and
behaviorally tested, then note in your STATUS that the fan/spacing/hover feel needs a human Play-test
pass before this is truly done. If you can drive a Play-mode session and grab screenshots
(`Application.CaptureScreenshot` or the Editor's Game view) of the new hand layout mid-drag and at
rest, attach/reference them in your report — that's the fastest way for the human to sign off without
re-deriving your layout choices from code.

## Report back

Commit on `feat/modal-restyle` (this worktree already has one unmerged commit ahead of master,
`7f7e19b` icon collection — that's fine, layer on top of it, don't rebase it away). Update your own
`docs/departments/ui/STATUS.md`. Do **not** merge or push — report the commit(s) + a plain-language
summary of what the new layout looks like and how the gesture feels, back to Integrator for review
and human sign-off.
