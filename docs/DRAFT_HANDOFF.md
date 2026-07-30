# Draft Handoff — 2026-07-30

**Schedule position:** Day 3 of 14 (`docs/SCHEDULE.md`), Day 3 checkbox ticked. Days 1-2 committed (`435398d`, `e325ad0`); Day 3 work below is implemented but **not yet committed**.

## Implemented

Day 3 focus — "Program UI: schedule Move (click destination) + Shoot (pick direction/tile); Lock; build `TimelinePayload` locally":

- `Assets/_Project/Net/` (new folder, new `LogiCard.Net.asmdef`): `ActionVerb` (Move/Shoot), `ActionNode` (ExecuteTime, GridPosition as `GridCoordinate`, Stance, nullable `CardData` Modifier), `TimelinePayload` wrapping an ordered node list.
- `Assets/_Project/Cards/LogiCard.Cards.asmdef` (new) — wraps existing `CardData`/`CardId` so `Net` can reference it; also had to add `LogiCard.Cards` directly to `Timeline` and `Tests.EditMode` asmdefs (Unity asmdef references are not transitive — this was a compile-error fix mid-implementation).
- `Assets/_Project/Sim/ShootCost.cs` (new) — `SnapShotSeconds = 2f`, `HoldAngleSeconds = 3f` (reserved, unused until Day 6).
- `Assets/_Project/Timeline/PawnProgram.cs` (new, pure C#) — per-pawn queue: `TryQueueMove`/`TryQueueShoot` accumulate `ExecuteTime` against a budget (queue-time rejection, not just at Lock), `Build()` returns a `TimelinePayload`, `BuildMovePreviewPath` reconstructs a `ScheduledPath` for on-screen preview.
- `Assets/_Project/Board/TileMarker.cs` (new) + `BoardView.cs` edit — tiles expose their `GridCoordinate` for raycast picking.
- `Assets/_Project/Board/BoardInputController.cs` (new) — click-to-queue Move/Shoot for the attacker pawn, phase-gated to Program, `EventSystem.IsPointerOverGameObject()` guard so HUD taps don't also hit the board; `CommitToPlayback()` hands the committed path to the pawn at Lock-In.
- `PawnView.cs` — added `SetPath(ScheduledPath)`.
- `ProgramHud.cs` — Move/Shoot toggle buttons, live queued-action + budget readout text, `OnLockInPressed` now builds the `TimelinePayload`, logs every `ActionNode` (Verb/ExecuteTime/GridPosition/Stance/Modifier — the Day 3 DoD), then commits to playback before the existing Reveal→Execute coroutine.
- `GameBootstrap.cs` — attacker pawn now starts stationary and is player-programmed via `BoardInputController`; defender stays hardcoded (no second local input source until Day 11 networking).
- `Assets/_Project/Tests/EditMode/PawnProgramTests.cs` (new, 6 tests).

## Verification

- Ran the real Unity EditMode suite in batch mode (`D:\unity\Editor\6000.5.4f1`): **33/33 pass** (27 prior + 6 new), clean compile, no exceptions.
- **Not done:** manual in-Editor playtest (actually clicking tiles, watching the pawn glide, pressing Lock In and reading the Console). This needs a windowed Editor session, which wasn't run this pass — do this first thing before trusting the UI wiring blind.

## Still unfinished

- Manual click/Lock-In playtest (see above) — do this before starting Day 4, since it's the only unverified part of Day 3.
- Everything past Day 3 per `docs/SCHEDULE.md`: Day 4 (M1 Slice 1 — local Host-style ghost resolve + playback of moves/shoots, Wound stub text) is next and completely unstarted in code.
- `docs/DAY4_GHOST_RESOLVER_RESEARCH.md` already exists in the working tree (research-only, dated today) with a concrete pipeline proposal for Day 4 written against the Day 3 code above — read this before designing Day 4, it may shortcut the design pass.

## Tomorrow

1. Manual playtest of Day 3 (Program phase click Move/Shoot, budget rejection, Lock In console log, Reveal/Execute playback) — confirm before moving on.
2. Read `docs/DAY4_GHOST_RESOLVER_RESEARCH.md` as a starting point for Day 4.
3. Implement Day 4: local ghost resolve step that consumes `TimelinePayload` (not `PawnProgram` directly — the research note flags that pawn views must stop reading `PawnProgram` once a resolver exists), produces a playback-ready result, and a Wound stub text on hit.
4. Re-run EditMode tests + manual playtest; tick Day 4 in `SCHEDULE.md` only once both pass.

## Blockers / notes

- Working tree has the full Day 3 diff staged as unstaged changes + new files — **nothing has been committed**. Decide whether to commit before starting Day 4 (recommended, matches the Day 1-2 precedent of one commit per day).
- `.cursor/skills/save-draft/` and `docs/DAY4_GHOST_RESOLVER_RESEARCH.md` were found untracked in git status but were not created by this session's main work — left untouched.
- No product decisions changed; only the Day 3 schedule checkbox was ticked, done earlier in this session against the test evidence above, not as part of running this save-draft pass.
