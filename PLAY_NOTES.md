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
