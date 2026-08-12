# Modal Restyle — Play Notes

**Repro:** Play a match to completion → Match Over → **Quit** → the confirm dialog opens.

**What changed:** the dialog used to read as flat dark-grey Unity chrome. It should now read as
**warm cardstock sitting on a deep dimmer**:

- Background behind the card is a deeper, slightly warm-black void (not neutral grey).
- The card face itself is a warm paper tone, matching the Time Card cardstock language, with a
  thin warm rim and a soft procedural drop-shadow offset down-right (no new sprites).
- Title/body read as dark ink on the paper; the hairline divider is a warm hairline, not the old
  cool accent line.
- Primary (confirm/Quit) button is a dark-ink fill with pale text — stays the obvious
  high-contrast confirm. Secondary (cancel) is a lighter paper chip with dark ink label.

**Sign-off ask:** does this read as handmade Desk-Lamp cardstock, not default Unity grey? Click
both buttons to confirm hit targets still land (rounded corners are slightly tighter than the old
layout).

# Play Notes — Character Select Carousel

How to see it: Boot the game (Play), let it flow through to **Character Select**, then use
**Next / Prev** or click the dimmed flank card. Only Scout and Juggernaut are in the roster.

What "good" looks like:

- One character is always **center** — large, fully opaque, front. The other is the **flank** —
  smaller and dimmed, off to the side.
- Clicking the flank card (or Next/Prev) swaps their roles over ~650ms: the flank scales up,
  brightens, and slides to center while the old center shrinks/dims/slides out — scale, opacity,
  and anchor position all move together, not staggered.
- Input is locked for that ~650ms — mashing Next/Prev or clicking mid-swap should not desync or
  double-trigger the animation.
- A large, low-contrast archetype-name headline sits behind the figures (mood text, not
  clickable).
- The background panel tint crossfades toward a warm per-archetype accent as the center role
  changes — stays in the desk-lamp `UiStyle` palette, not a bright/pastel shift.
- Confirming (`ConfirmCharacter`) still advances to **Map Select**, and Map Select is unchanged —
  still the flat grid.

If it looks like a hard cut instead of a crossfade, or Next/Prev double-fires during the
animation, that's a regression worth reporting back on `feat/char-select-motion`.
