# Camera — vertical right-drag pans, horizontal right-drag still rotates

**Status:** New ask, human-directed (2026-08-16), on top of the just-landed control-hint fix (`212f402`).
**Worktree:** `logiCard-camera-control` / `feat/camera-freecam-tps`.
**Owns:** `Assets/_Project/Board/BoardCameraRig.cs` only — same lane as your prior work this wave.

## The ask

Right-click-drag currently only reads the **horizontal** mouse delta (`HandleYawDrag`, calls
`RotateBy`). Human wants the **vertical** component of that same right-drag gesture to **pan** the
camera (forward/back along the ground plane) — same motion `HandlePan`'s W/S keys already produce,
just driven by drag delta instead of a fixed per-second speed. Horizontal drag keeps rotating exactly
as it does today. This is one combined two-axis drag, not a new mode/mouse-button.

## Why this shape

- `HandlePan` already computes a camera-relative forward/back/left/right direction on the ground
  plane (see its own doc comment — League-of-Legends-style, projected so "forward" tracks yaw). Reuse
  that direction logic for the vertical-drag case rather than inventing a second movement model.
- Don't reuse `PanBy`'s per-second speed constant as-is for drag — dragging should feel like pushing
  the world directly (1:1-ish with pixels, like `RotateBy`'s `DegreesPerPixel` does for rotation), not
  like holding a key down. Add a `WorldUnitsPerPixel`-style constant (name it to match this file's
  existing `DegreesPerPixel` convention) and scale by it, the same shape `HandleYawDrag` already uses
  for its axis.
- `HandlePan` explicitly early-returns while `_dragging` is true today (comment: "panning while also
  orbiting would fight the drag's own screen-space intent"). That early-return logic needs to change
  now that dragging itself does the panning — re-read that comment and method before touching it, the
  reasoning may still partly apply (e.g. don't also fire WASD pan during a drag) but the "no panning
  during drag" conclusion no longer holds.
- Keep pitch untouched, same as every other control in this file — vertical drag moves the camera
  along the ground plane (dolly), it does not tilt the view.
- Respect existing pan bounds (`_panBoundsSet`/`_panMinWorld`/`_panMaxWorld`, `SetPanBounds`) — drag-pan
  must clamp the same way keyboard-pan already does, not bypass it.
- No-op during `CameraMode.TpsLock`, same as every other orbit/pan/zoom control (`RotateBy`/`ZoomBy`/
  `PanBy` all already guard on `_mode != CameraMode.Overview` — match that pattern).

## Update the control hint

`OnGUI`'s legend string (just added, `212f402`) currently says `"Right-drag: Rotate"` — update it to
reflect the new two-axis behavior, e.g. `"Right-drag: Rotate / Pan"` or similar — keep it short, it's a
corner legend, not a manual.

## Tests

Extend/add EditMode or PlayMode coverage the same way this file's existing yaw-drag and zoom tests are
structured — find them first (likely `BoardCameraRigTests`-equivalent) and match their pattern rather
than inventing a new test shape.

## Verification

Batchmode EditMode + PlayMode in this worktree (Editor closed on this exact path first — check for a
stale `Temp/UnityLockfile` the way you had to last time). Report exact pass/fail counts. Commit on
`feat/camera-freecam-tps`, don't merge, don't push.

## Out of scope

Zoom, yaw-drag's own math, TPS lock, pan bounds definition itself (`SetPanBounds`'s callers) — none of
that changes here.
