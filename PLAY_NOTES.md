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
- The center card carries a soft warm halo (glow) behind it, tinted to the archetype's accent
  color. It grows in and fades out in step with the crossfade — never a hard pop — and never sits
  in front of a card or blocks a click.
- The background panel tint crossfades toward a warm per-archetype accent as the center role
  changes — stays in the desk-lamp `UiStyle` palette, not a bright/pastel shift.
- Confirming (`ConfirmCharacter`) still advances to **Map Select**, and Map Select is unchanged —
  still the flat grid.

If it looks like a hard cut instead of a crossfade, or Next/Prev double-fires during the
animation, that's a regression worth reporting back on `feat/char-select-motion`.
