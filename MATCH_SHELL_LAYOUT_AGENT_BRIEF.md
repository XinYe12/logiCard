# MATCH_SHELL_LAYOUT — Camera slice note

**From:** Integrator  
**To:** Camera (`D:\projects\Game\logiCard-camera-control`, `feat/camera-freecam-tps`)  

## Instruction

**Pause new freecam/TPS feature work** until UI freezes `MapViewport` rect on `feat/modal-restyle`.

When UI reports Ready with a public MapViewport `RectTransform`:

1. Free-pan / orbit / zoom must feel scoped to the **map hole**, not the whole Game View under the HUD.
2. TPS lock still targets pawns on the board; do not fight the new shell stacking.
3. Report back; Integrator merges Camera after UI shell lands (or same train if no file conflict).

If you have uncommitted freecam WIP, checkpoint on your branch and idle — do not touch `ProgramHud`.
