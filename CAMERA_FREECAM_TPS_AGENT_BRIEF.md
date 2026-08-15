# Agent Brief — Free-pan camera + per-character TPS lock view

**From:** Integrator **To:** whoever picks up this worktree (`feat/camera-freecam-tps`, branch
already created off master @ `164012f`)
**Priority:** Feature, human-requested 2026-08-15.
**Scope:** `Assets/_Project/Board/BoardCameraRig.cs`, `Assets/_Project/Boot/GameBootstrap.cs`
(`ConfigureCamera` only), new files as needed. This isn't one of the four permanent departments
(Atmosphere/Cards/Character/UI) or Map — camera has always been Integrator-owned
(`GameBootstrap`/`BoardCameraRig`), and this worktree is a dedicated ephemeral slice for it, same
shape as the earlier `logiCard-camera-zoom-fill` slice mentioned in `docs/drafts/2026-08-07.md`.
Report back to Integrator; do not merge or push yourself.

## The ask (human's words)

> now we have the rotation, i also want the user to be able to move their camera, like players in
> League of Legends. There will also be options to lock in the viewpoint/perspective of a
> particular character, so that player can see the game in a TPS kind of viewpoint, like Resident
> Evil.

Two distinct features:

### 1. Free camera pan (League of Legends style)

Today's camera (`BoardCameraRig`) only orbits a **fixed pivot** (`_boardCenter`, set once in
`Init` from `GameBootstrap.ConfigureCamera`) via right-drag yaw + scroll zoom — pitch (52°) and
distance (14 units) never change, and the pivot itself never moves. Add the ability to **translate
that pivot** across the board's ground plane — the LoL reference is: keyboard (WASD or arrow keys)
and/or edge-of-screen pointer panning slides the camera's look-at point across the map, independent
of rotation/zoom, clamped so the pivot can't leave the board's playable footprint (see
`MapDefinitions`/`ArenaBoard` bounds — reuse whatever this project already has for board extents
per map; there are three maps with different footprints, Freight Yard/Rail Platform/Vault Complex,
so don't hardcode one board's bounds).

- Keep yaw + zoom exactly as they are — this is additive, a third independent axis of control
  (pivot position), not a replacement.
- `Apply()`'s math (`_boardCenter - (rotation * Vector3.forward * DistanceFromCenter)`) already
  treats `_boardCenter` as the pivot — panning is "make `_boardCenter` mutable and clamp it,"
  not a rewrite of the orbit math.
- `Rotated` event already fires on any camera change for board-anchored UI re-projection
  (`docs/ui/UI_BOARD_ANCHORED_COMPONENTS.md`'s "recompute only when camera changes" contract) —
  panning must fire it too, same as yaw/zoom do, or door prompts etc. will drift stale.

### 2. Per-character TPS lock (Resident Evil style)

A selectable mode where the camera instead locks onto a specific pawn and gives a close,
over-the-shoulder/behind-the-character perspective view — a real mode switch, not a zoom level:
today's camera is **orthographic** (diorama/toy-board read); a TPS lock view needs to become
**perspective** (FOV-based) positioned behind and above the locked pawn, looking along its facing.

- This is a mode toggle: "diorama/overview" (today's orbit cam) ↔ "TPS lock on Pawn X." Design the
  transition and the UI affordance for switching (which pawn, on/off) — there's no existing
  control for "pick a pawn to view through," so you're inventing that surface. Keep it simple for
  v1 (e.g. a hotkey or a HUD toggle cycling through the match's pawns) rather than over-building.
  Reference the same board-anchored-control content contract for any new UI you add if it's a
  persistent on-screen control (`docs/ui/UI_BOARD_ANCHORED_COMPONENTS.md`) — but a
  camera-mode toggle isn't itself "changing a board object's state," so use judgment on whether
  that doc's contract even applies before over-applying it.
- `Assets/_Project/Board/PawnView.cs` is the existing pawn presentation component — find pawn
  Transforms through whatever this project already uses to enumerate pawns (grep for existing
  "find all pawns" patterns before inventing a new one; `RoundPlayback`/`GameBootstrap` likely
  already track the roster).
- Only one `Camera.main` exists today (`GameBootstrap.ConfigureCamera`), and its `rect` is carved
  out to dodge the top strip / bottom HUD dock (`ProgramHud.HudDockHeight`/`TopStripHeight`) —
  reuse the same camera (toggle `orthographic`/position/rotation) rather than introducing a second
  live `Camera` unless you have a good reason to; a second camera means camera-switching logic,
  render-texture or enable/disable juggling, and doubles what board-anchored UI has to reason
  about.
- Decide and document: does Program-phase input (path drafting, board taps) still work while
  TPS-locked, or does entering TPS lock implicitly return control to the overview cam for anything
  that needs a board click? A first-person/TPS view makes "tap the board to draft a path" awkward
  at best — don't silently break Program-phase input; either keep TPS as an Execute/observation-only
  mode, or explicitly scope Program-phase interaction out of it and say so in your report.
- Playback/Execute presentation: check `docs/core/PLAYBACK_CONTRACT.md` before touching anything
  that runs during tape playback — "Continuous presenters must be a pure function of scrubber
  seconds; do not restart timed FX every `ApplyTime` tick" applies to camera-follow too if TPS lock
  stays active while a round plays back (the camera position while locked should track the pawn's
  *current scrubber-time position*, not restart/snap oddly on scrub).

## What "done" looks like for v1

You don't have to gold-plate this — a working free-pan (clamped to board bounds, all three maps)
and a working TPS lock toggle (perspective, positioned behind/above one pawn, a simple way to pick
which pawn and turn it off) that don't break the existing orbit/zoom or board-anchored UI is a
complete first pass. Note anything you deliberately deferred (smoothing/easing on pan, camera
collision against props, transition blend between overview↔TPS) rather than silently shipping it
rough — human sign-off on feel is expected regardless, same as any camera work.

## Testing

`BoardCameraRig` has existing PlayMode/EditMode coverage (grep for it) — extend rather than
replace. Batchmode works fine in this worktree (separate path from main, no Editor-lock conflict).
Camera feel itself is a human Play-test call, same as the recent hand-deck work — get it compiling
and behaviorally tested, then flag that a Play pass is needed for final sign-off.

## Report back

Commit on `feat/camera-freecam-tps` (already created, currently at master's tip `164012f` — no
prior commits on it yet). Do **not** merge or push. Report back: what you built, how pan/TPS-lock
are triggered (final key/control bindings), commit hash(es), and test results.
