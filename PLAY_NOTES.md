# Play Notes — Character Select Carousel

**The cards and Prev/Next buttons now use real art, not flat-color rectangles** — Kenney "UI Pack -
Adventure" (CC0) 9-slice sprites: Scout's card is a cream/parchment panel, Juggernaut's is a darker
solid-brown variant, nav buttons are the matching wood-bordered button sprite. This replaces both the
earlier flat-color cards *and* a same-session UI Toolkit rebuild that got reverted after visual
feedback (`docs/UI_TOOLKIT_MIGRATION_PROPOSAL.md` has that history if it's ever relevant again — it's
not live code anymore, this screen is back on plain uGUI). The main thing worth checking this pass:
**does the Kenney wood/parchment look actually read well** against the ghost headline, the warm
per-archetype background tint, and the halo glow — or does it clash/feel pasted-in. It's a deliberate
compromise pick (fantasy-adventure styled art on a SWAT-tactics game) chosen for material/warmth fit
over genre fit — see `Assets/_Project/Art/UI/THIRD_PARTY.md` for the other packs considered and why
this one won.

How to see it: Boot the game (Play), let it flow through to **Character Select**, then use
**Next / Prev** or click the dimmed flank card. Only Scout and Juggernaut are in the roster.

What "good" looks like:

- One character is always **center** — large, fully opaque, front. The other is the **flank** —
  smaller and dimmed, off to the side. Both cards are wood-bordered cardstock/parchment panels
  (Kenney art), not flat colored rectangles — Scout reads lighter/cream, Juggernaut darker/solid-brown.
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
