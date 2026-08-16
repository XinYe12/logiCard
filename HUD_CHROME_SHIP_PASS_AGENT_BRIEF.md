# Agent Brief — Ship a real chrome pass on the HUD dock (human: "current UI is unplayable")

**From:** Integrator **To:** UI seat (`feat/modal-restyle`, this worktree)
**Priority:** Urgent — human playtest feedback 2026-08-15, on top of the just-merged hand-deck/
drag-to-play work (`164012f`/`dece429`). This is a **visual chrome pass**, not another interaction
rework — drag-to-play, the fan positions, the column layout fractions all stay as they are unless
they're actively fighting the chrome you're adding.
**Scope:** `Assets/_Project/UI/GearHandView.cs`, `Assets/_Project/UI/ProgramHud.cs`,
`Assets/_Project/UI/UiStyle.cs` / `UiFactory.cs` (extend, don't fork a second token system).

## The ask (human's words)

> the shadow needs to be fixed, the resolution of the shadow too low. talk to the ui depart to fix
> this ui quickly. give me a mature solution that we can ship with respect to UI. the current UI is
> unplayable. i already gave UI department some successful vibe codes. tell them to start using
> those uiverse.io code to start with. the cards and the bottom timeline and things needs to be
> separated.

(The "shadow resolution" line is about the **3D board's real-time shadows**, not UI drop-shadows —
Integrator already fixed that directly, URP main-light shadowmap 2048→4096 and shadow distance
50→20 to match the board's actual scale. Not your concern. Everything else below is yours.)

## Why "unplayable" — diagnosis from the human's screenshot

The hand-deck feature (`164012f`) is functionally correct but has **no real panel chrome** — every
region is a transparent `RectTransform` column abutting its neighbor with nothing to visually
separate them:

- The fanned hand's leftmost/widest cards visually **overlap into the ControlsColumn**, covering
  part of the MOVE/SHOOT/DOOR verb row and the stance buttons underneath — there's no panel
  boundary or z-order/clip telling the player "this is the hand, that's the verb controls."
  (`GearHandAreaMinY`/fan overlap math in `GearHandView.Build` widens cards past the hand column's
  own bounds without anything clipping or backing them.)
- The compact queue-log strip sits as a bare dark rectangle with no border/framing, reading as a
  UI glitch rather than an intentional readout panel.
- Cards themselves (`CardShadow`/`CardBorder`/`ModalCard` face) are flat and low-contrast against
  the dark dock background — nothing pops, nothing reads as "this is a card I can pick up."

**Fix the boundary problem first** — every dock region (ControlsColumn, the hand, the queue-log
strip, ActionColumn) needs its own visually distinct panel: real background fill, a border/rim, a
drop shadow that sits *behind* its own region rather than bleeding across into a neighbor's. Clip
the fan (`RectMask2D` on the hand's container, same technique `BuildQueuePanel` already added for
the log strip) so overlap-fan is contained to the hand's own space instead of spilling into
Controls.

## Use the collected code — don't restart from scratch

The human has been feeding `docs/UI_CHROME_COLLECTION.md` real Uiverse.io (MIT) reference code for
months specifically for this. Stop treating it as "collected, not yet greenlit" — the human is
explicitly greenlighting implementation now. Concrete starting points already cataloged there,
already tagged as the best hits for exactly this job:

- **`docs/ui-collection/normal-card.css`** — tagged *"Candidate — default card"*: layered
  `box-shadow` = lift shadow + contact shadow + inset bottom lip. This is your **default panel/card
  face** technique — port the layering (a soft offset shadow behind, a tighter contact shadow
  close under, a subtle inset-color strip along one edge) into `UiStyle`'s existing
  `ModalShadow`/`ModalCardBorder`/`ModalCard` stack (`GearHandView.Build` already does a shadow +
  border + face stack per card — deepen it, don't replace it with something incompatible).
- **`docs/ui-collection/wallet-card-holder.{html,css}`** — tagged *"Best collection hit so far for
  gear-hand/deck feel"*: stacked cards in a pocket, hover fans + reveals, per-card hover brings one
  forward. Reference for the hand's **resting stack chrome** (the "pocket" it sits in) and hover
  behavior, on top of the drag gesture you already built.
- **`docs/ui-collection/hands-deck-comic-swatches.{html,css}`** — tagged *"Best gear-hand strip
  motion hit alongside wallet stack"*: overlap strip + hover lift + neighbor fan-scale + hard comic
  shadow. Reference for the **card-to-card overlap/shadow relationship** in the resting fan.

**Retint, don't copy colors literally.** All three demos use cool/neutral web palettes (greys,
blues, purples) — this project's locked direction is warm cream/cardstock toy-diorama
(`UiStyle.ModalCard`/`ModalInk`/etc. already encode it; see `docs/core/ART_DIRECTION.md` §4). Port
the *technique* (shadow layering, border rim, hover-forward z-order, pocket framing) through
`UiStyle`'s existing warm tokens, not the demo's own hex values. This keeps one consistent visual
system instead of a second style bolted on next to the first.

**License note** (already recorded in `UI_CHROME_COLLECTION.md`'s own license section): Uiverse
code is MIT — free to use/modify/ship, keep the MIT notice in a third-party attributions file if
you ship substantial copied structure. `wallet-card-holder`'s demo markup has Stripe/Wise/PayPal
brand marks in it — if you reference its HTML structure, strip those, they're not MIT-cleared.

## Separate the regions — concretely

1. Give **ControlsColumn**, the **hand's own container**, the **queue-log strip**, and
   **ActionColumn** each a real backing panel (face + border + shadow, per the ported
   `normal-card` layering) so their extents are visually obvious even with nothing selected.
2. Clip the hand's fan to its own column (`RectMask2D`) so cards never visually cross into
   ControlsColumn or ActionColumn regardless of overlap/rotation math.
3. Card face contrast: the cards need to read clearly against the dock background — check contrast
   once retinted, this is exactly what looked "muddy" in the screenshot.

## What NOT to change

Drag-to-play logic (`CardDragController`), the `CardPlayRequested` event contract, `TryQueueBandageAt`/
`TryQueueStormAt` call sites, and the dock's column width fractions (26/48/26) are out of scope
here — this is chrome on top of working interaction, not a second interaction rework. If a chrome
choice genuinely fights the drag gesture (e.g. clipping would hide a card mid-drag), solve it
(e.g. don't clip the actively-dragged card, only resting ones) rather than reopening the gesture
itself.

## Testing

Existing `GearHandViewTests.cs`/`ProgramHudPlayModeTests.cs`/`ProgramHudLayoutTests.cs` assert
structure/behavior, not pixel colors — a chrome-only pass should mostly leave them green. If any
assert on a `RectTransform` size/anchor you change while adding backing panels, update them
honestly rather than loosening an assertion to make it pass. Batchmode both suites in this
worktree before reporting back. Visual feel/"does this look mature enough to ship" is still a human
call — this brief exists because the human is asking urgently, so get this in front of them for a
Play-test as fast as you can rather than polishing indefinitely; report back with what you built
and how confident you are that it reads as "shippable," and let Integrator/human make the final
call.

## Report back

Commit on `feat/modal-restyle` (current tip `dece429`, already merged to master as `164012f` — you
are ahead of what's on master by nothing new yet; this commit lands on top). Update
`docs/departments/ui/STATUS.md`. Do not merge or push — report back to Integrator.
