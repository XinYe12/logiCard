# MATCH_SHELL_LAYOUT — Map seat brief (docs)

**From:** Integrator  
**To:** Map (`D:\projects\Game\logiCard-map`, `dept/map`)  
**Wave:** Match Shell Layout — **docs / framing recommendations**  
**Read:** `docs/ui/MATCH_SHELL_LAYOUT.md` (master).

## Goal

The match UI’s **MapViewport** is a mid-screen rectangle (not full-window). Recommend how the
authored maps / board presentation should **read inside that hole** — still our continuous diorama,
never a Hearthstone lane board.

## Deliverable

Short addendum on an existing map doc (`MAP_AUTHORING.md` or `MAP_PRESENTATION_STANDARD.md`):

1. Preferred camera framing for a ~50% height center viewport (what must stay readable: doors, flanks, pawns).
2. Whether any map dressing assumes full-bleed screen (flags for Atmosphere/Camera).
3. Explicit: no second “card battlefield” layer on the map.

## Do not

- Edit `ProgramHud` / UI shell.
- Edit `BoardCameraRig` while Camera slice owns it — recommend only; Integrator/Camera implement.
- Merge / push unless Integrator asks.

## Done when

STATUS + Integrator summary.
