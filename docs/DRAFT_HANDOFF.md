# Draft Handoff — 2026-08-06

**Schedule:** M2.5 continuous pivot (Days 7b–7g) code-complete and verified green. Day 8 URP art foundation merged. Phase 6 (tune + cold-observer) is the only thing standing between here and Day 9 — and it's human-only (playtest feel + door Closed-at-start fixture rework), not agent-delegable.

## Implemented

Continuous-space pivot **Phases 1–5** on `master` (C35/C39/C40/C41):

- **Sim geometry:** `PlanarPosition`, `Segment`, `ArenaBoard`, `ContinuousLineOfSight`, `ContinuousPathfinder` (+ EditMode suites). Grid board/LoS/pathfinder/coordinate + their tests **deleted**.
- **Sim/Net/Timeline:** `ScheduledPath`, `Door`, `TimeResourceMath`, `ActionNode`, `TapeEvent`, `GhostResolver` (analytic Hold sweep), `PawnProgram` (Euclidean draft cost, free-aim Shoot, radius Door, revisit legal).
- **Board/Boot/UI:** ground-plane + segment walls/doors; `PlanarFromWorld` taps; `TryGetNearestDoor`; RoundPlayback continuous carry (no tile snap); HUD `DraftDistance` / free-aim wording. `TileMarker` deleted.
- **Bootstrap:** DAY7 wall-with-gap + door starts **Open** (Closed-at-start deferred to Phase 6). Spawns `(2,0)` / `(2,4)`.
- **PlayMode** fixtures retargeted (`SliceSceneFixture`, BoardInput / RoundPlayback / ProgramHud).
- **Docs/memory:** C40 no pawn-vs-pawn collision; C41 Phase-1 merge locks (Door-typed API, inclusive `Segment.Intersects`). Draft note: `docs/drafts/pawn-collision-tradeoff.md`.

## Verification

- **Green as of `d2f9171`** (2026-08-05): 95/95 EditMode + 23/23 PlayMode, after the undo-scope + path-preview-bead fixes. `73ea58e`'s Closed-door probe re-ran PlayMode at 23/23 green (on the *reverted* Open state).
- URP merge (`4cfe8ea`) + its Editor-generated `ProjectSettings` churn (`6466f4a`) landed after that verify pass but touched no gameplay code — treat green as still current, but a fresh EditMode+PlayMode pass after opening the Editor post-merge hasn't been explicitly re-recorded.
- Manual Bootstrap smoke (continuous tap Move, free-aim Snap/Hold, wall LoS, Lock In → playback → round carry) is still **not recorded** — human-only, not agent-delegable.

## Still unfinished

1. **Phase 6 tuning** — `HitRadius` / `LaneHalfWidth` / `InteractRadius` (still ~`0.45f`) untouched against real play. **Human playtest — not agent-delegable.**
2. **Manual Bootstrap smoke** — see Verification above. **Human — not agent-delegable.**
3. **SCHEDULE.md checkboxes** — Day 7–7g and Day 8 boxes are still unticked pending the human cold-observer playtest call; don't tick them from code inspection alone.
4. **Full batchmode re-verify** — needed after the door-Closed-at-start rework (see below) plus this session's 4 fixes land together. Appears to already be in flight in worktree `logiCard-verify-day8` (branch `verify/post-urp-day8`, `VERIFY_POST_URP_AGENT_BRIEF.md`) — check there before starting a second one.

## Door Closed-at-start — now done (uncommitted, in this working tree)

Previously deferred (`73ea58e`, see git history) because flipping the flag broke `RoundPlaybackPlayModeTests`' AmbushPoint scenario and a few HUD test destinations that sat on the door segment. As of this session that rework landed directly in `D:\projects\Game\logiCard` (concurrent with this session's playtest-bugfix work, not done by this thread):

- `GameBootstrap.cs`: door now starts **Closed**; `BuildDefenderPayload` steps into range and explicitly opens it before the scripted Snap, so AmbushPoint LoS still holds.
- `RoundPlaybackPlayModeTests.cs` / `ProgramHudPlayModeTests.cs`: destinations/comments updated to stay legal against a Closed-by-default door.
- `GhostResolverTests.cs`: gained `HoldAngleShootFireCarriesWindowStartSoPlaybackCanLitTracerBeforeContact`, covering this session's `TapeEvent.WindowStartSeconds` fix (see below) — the two changes are compatible.

Not yet compiled/run as a whole — needs the same fresh EditMode+PlayMode pass as this session's fixes (see #4 above).

## Tomorrow (2026-08-06)

1. Phase 6 cold-observer playtest pass (human): tune the three radii, decide Closed-vs-Open door start for real, then have an agent do the matching fixture rework in one pass.
2. Manual Bootstrap smoke, same checklist as above (retest — 4 bugs fixed today, see below).
3. Once both land: tick SCHEDULE.md Day 7–8 boxes, consider starting Day 9 (board/UI identity).

## 2026-08-06 playtest fixes (not yet re-verified by a batchmode run)

First human playtest pass of the continuous slice surfaced 4 issues, all fixed in-tree (Editor was open during this session, so no batchmode re-run happened — do that before trusting these):

1. **Wall/door boxes rendered rotated 90° from their real `Segment`** (`BoardView.PlaceSegmentBox`) — `yaw = atan2(dx,dy)` was wrong for a box whose long axis is local +X; needed `atan2(-dy,dx)`. This is why a wall spanning X looked like a bar spanning Y, and made it look like walls didn't block movement (they did — only the render was wrong) and made path-preview stop-points look misplaced.
2. **Hold Angle looked like it never fired** — it *was* resolving correctly (contact math checked out against the demo layout), but the firing tracer only lit up starting at the shot's completion instant, while a Hold's contact (and the wound it causes) can land anywhere earlier in the hold window — so the wound banner could appear with no beam yet on screen. `TapeEvent` now carries `WindowStartSeconds`; `RoundPlayback`'s tracer is lit for the whole `[WindowStart, Complete+TracerVisibleSeconds]` span. Same tracer mechanism Snap already used — no new animation needed once the timing was fixed.
3. **Door Open/Close was ambiguous** — tapping the board used to book an Open/Close immediately against a HUD-preselected action, silently flipped to its *opposite* whenever it already matched the door's live state, so the HUD could show "selected: OPEN" while the tap actually booked a Close. Reworked to a two-step confirm: tapping near a door only selects it (`BoardInputController.PendingDoor`); the OPEN/CLOSE buttons are the explicit confirm (`TryConfirmPendingDoor`) and are disabled until a door is selected. `PreferredDoorAction` API removed.
4. Branches `continuous/phase1-geometry` and `feature/hud-door-verb` deleted (content already on `master`).

`ProgramHudPlayModeTests.DoorModeButtonSwitchesTheInputVerbAndAction` rewritten as `DoorModeSelectsADoorThenRequiresExplicitConfirm` to match the new select-then-confirm flow. No other test files touched — everything else should still hold, but **run EditMode + PlayMode fresh** (Editor closed) before trusting that.

## 2026-08-06 second playtest pass — the actually-critical door bug

Retesting the fixes above (against the door-Closed-at-start rework, see above) surfaced the real bug hiding behind "door interaction is ambiguous":

1. **Door state never persisted, at all, ever** (`RoundPlayback`) — `GhostResolver.Resolve` only ever mutated its own resolve-local scratch clone of the board (intentional: keeps Resolve a pure function of board+inputs, per its doc comment). Nothing ever copied that resolved state back onto the shared `ArenaBoard` that the *next* round's pathfinding and the door's rendered tint both read from. A player who booked and resolved an Open watched it silently reset to Closed the instant the round ended — the door could never actually be gotten through, no matter how many times you "opened" it. Fixed: `RoundPlayback.CommitRoundState` now walks the tape's `DoorOpened`/`DoorClosed` events in chronological order and applies each to the real board (`ApplyDoorStateFromTape`), same lifecycle point where wounds/positions already carry over. **This means opening a door only takes effect starting the *next* round** (consistent with every other queued action resolving, not executing live) — you cannot open-and-walk-through in the same Program draft.
2. **Rejected taps were silent** — a Move blocked by a closed door (no route at all, since the door starts Closed and there's no way around) just logged to the Debug console; nothing on screen told the player why nothing happened. `BoardInputController` now has an `ActionRejected` event; the HUD shows the reason in the existing outcome banner ("Can't do that — …") until the next successful action clears it. This is *not* a new modal/dialogue — it reuses the banner already used for wound/kill text. A fuller "detected you're blocked by a door, want to switch to opening it" flow is still open if this isn't enough after retesting.
3. **Snap/Hold shots may pass through a closed door** — user-flagged as minor/deferred to a later phase, not investigated this pass. `ContinuousLineOfSight`/`ArenaBoard.IsBlocking` *should* already account for closed doors in the shot's line-of-sight check, so if it recurs after retesting, that mismatch (not "doors don't block LoS at all") is where to start looking.

None of this has been re-verified by a test run — same caveat as above, retest live and/or run batchmode once free.

## 2026-08-06 third playtest pass

1. **`InteractRadius` too small** (`PawnProgram.cs`) — a pawn visibly adjacent to the door (0.58 world units from the segment) still read "out of interaction range" against the old 0.45 placeholder. Raised to `0.7f`.
2. **The rejection banner from the pass above didn't cover every rejection path** — it fired correctly for tap-based rejections (Move/Shoot/Door-select/confirm — confirmed working in a screenshot: "Can't do that — Door is out of interaction range.") but `BoardInputController.TryCommitDraftPath` (the SET PATH / Lock In commit path) never raised `ActionRejected` at all. Now it does.
3. **Root cause of "character freezes at Lock In"** — this is the bug #2 was hiding. `ProgramHud.OnLockInPressed`'s budget guard only checked `Program.UsedSeconds` (already-*committed* cost), never the pending draft's cost. A drafted-but-not-yet-"SET PATH"ed move that didn't fit the round's Time Resource budget sailed past that guard; `BoardInputController.CommitToPlayback` then called `TryCommitDraftPath`, which correctly rejected it for being over budget — but `CommitToPlayback` locked the round in anyway, discarding the draft. Net effect: Lock In "succeeded," the pawn had zero Move nodes, and playback showed it standing frozen the whole round with no explanation. Fixed: `CommitToPlayback` now returns `false` (and leaves the round unlocked) when a pending draft exists and fails to commit; `OnLockInPressed` checks that return value and aborts instead of proceeding, and the rejection reason now shows in the outcome banner via #2's fix (e.g. "Can't do that — Would exceed Time Resource budget (30.6s of 30.0s).").

Still not re-verified by a test run.

## 2026-08-06 fourth pass — the door "dialogue" request, clarified

User clarified: the ask was never about error messages — they wanted an actual interactive UI element to open/close the door, not a fixed row buried in the thumb zone. Built it:

- **`ProgramHud.BuildDoorPrompt`/`RefreshDoorPrompt`** — a small floating panel (label + OPEN/CLOSE buttons, same `Door_Open`/`Door_Close` names the existing test looks up) that spawns anchored *at the selected door's own screen position* — projected each time the selection changes via `BoardInputController.BoardView.WorldFromPlanar` -> `Camera.main.WorldToScreenPoint` -> `RectTransformUtility.ScreenPointToLocalPointInRectangle` into the HUD's screen-space-overlay canvas. Hidden whenever no door is selected, whenever the player leaves Door mode, and whenever the phase isn't Program (so it can't float over playback).
- The old static OPEN/CLOSE buttons that lived in the thumb zone's Door row are gone; that row is now just an orienting label ("DOOR — tap near a door to select it").
- Added `BoardInputController.BoardView` (read-only) so the HUD can do the world-to-screen projection — previously it only exposed the Sim-level `ArenaBoard`, not the view.

Not yet visually verified in the Editor (this was built from code reading — the projection math is standard Unity technique but hasn't been eyeballed against the real camera rig). First thing to check on retest: does the prompt actually land visually on/near the door, not offset or off-screen.

## 2026-08-06 fifth pass — the prompt landed in the wrong place, plus a standing convention doc

Screenshot confirmed the "not yet visually verified" caveat above was warranted: the prompt rendered near the bottom of the screen, nowhere near the door.

- **Root cause:** anchor/pivot mismatch. `RectTransformUtility.ScreenPointToLocalPointInRectangle` returns a point measured relative to the target rect's own *pivot* (canvas root's pivot is its center, so `(0,0)` = screen center) — but the prompt was anchored to the parent's *bottom-center* (`anchorMin=anchorMax=(0.5,0)`) and then given that center-relative point as its `anchoredPosition` directly. Two different coordinate origins added together, offsetting it by roughly half the screen height.
- **Fix + redesign:** `anchorMin=anchorMax=(0.5,0.5)` (matches the coordinate space `local` is measured in) with the prompt's own `pivot=(0,0.5)` (left-center, so it extends rightward — "beside" — from the anchor point instead of centering on top of it or floating above it). Also shrunk it from a labelled panel to a compact two-button cluster (`96×84`), per the user's actual ask: "an interaction button that appears beside the door," not a dialogue panel.
- **New doc: `docs/UI_BOARD_ANCHORED_COMPONENTS.md`** — the user asked for this to be written down so the pattern (and this exact pitfall) doesn't have to be rediscovered for the next board-anchored control (a pickup prompt, a hazard warning, etc.). Covers the conversion pipeline, the anchor-vs-pivot pitfall above, sizing/placement rules, and the lifecycle hide-cases (mode switch, phase change, selection cleared) the first version also missed one of.

Still not visually re-verified — same caveat as the pass above, now doubly warranted.

## 2026-08-06 sixth pass — formalized the interaction-prompt contract + process enforcement

User asked to generalize past just the door: any UI that changes a board object's state needs (1) the object's identity, (2) its live current state, (3) the explicit options available — and asked how to make sure future agents actually follow that, not just this one instance.

- **`docs/UI_BOARD_ANCHORED_COMPONENTS.md`** gained a "Content contract" section (identity/state/options, each with the failure mode that happens if you skip it — sourced from bugs already found this session) plus a copy-paste checklist for the next such control.
- **`Door.cs`** gained an optional `DisplayName` (defaults `null`, so none of the dozen+ existing `new Door(...)` call sites in tests needed touching). `GameBootstrap`'s demo door is now `"Door #1"`.
- **`ProgramHud`** door prompt now shows identity + state on the cluster itself (`"Door #1 · CLOSED"`) above the OPEN/CLOSE buttons, not just in the thumb-zone label.
- **`CLAUDE.md`** created at repo root (didn't exist before) — this is the actual enforcement mechanism: it's loaded into every agent session automatically and explicitly points at `UI_BOARD_ANCHORED_COMPONENTS.md` before any board-object-state UI work starts, plus a standing instruction that decisions worth a future session knowing get written to `docs/` with a pointer added here, not left to be rediscovered.

Not yet visually re-verified.

## Blockers / notes

- Unity **6000.5.5f1**; project path `D:\projects\Game\logiCard`.
- Batchmode needs Editor **closed** on this project.
- `master` is ahead of `origin/master` by 16 commits as of `6466f4a`; nothing pushed yet.
- Do not start M3 art absorption until M2.5 verify + Phase 6 bar is honest — Phase 6 itself is still open (see above), so that gate has not been cleared yet even though verify has.
