# Draft Handoff — 2026-08-07

**Schedule:** M2.5 continuous pivot (Days 7b–7g) code-complete on `master` through `ef05061`. Day 8 URP merged. 2026-08-07 playtest follow-up pack is **committed**. Phase 6 cold-observer + radius tuning still human-only; Day 9 (board/UI identity) waits on that gate.

## Implemented

**Already on `master` (`54b051a` and earlier):** continuous Phases 1–5 (C35/C39/C40/C41); 2026-08-06 playtest pack (wall yaw, Hold tracer window, door Closed-at-start + Aftermath carry, select-then-confirm door, board-anchored OPEN/CLOSE, `InteractRadius` 0.7, rejection banner, Lock In respects pending draft).

**Committed `ef05061` (2026-08-07 playtest follow-ups):**

| Area | What landed |
|------|-------------|
| Snap LoS | `GhostResolver` requires LoS to victim, not only aim (closed-door HitRadius edge case) |
| Door scrub | `RoundPlayback.SyncDoorsToSeconds` — tint/model follow scrubber + Aftermath carry |
| Door status UX | Prompt uses `PawnProgram.ScheduledDoorState`; keep selection after OPEN/CLOSE; strong red/green door colors |
| Open→walk same Program | Move drafting pathfinds on a **scheduled-door clone**; shared board stays round-start until Aftermath |
| UNDO | Always on bottom action row (Play / Rewind / UNDO / Lock In) |
| Lock In + draft | Drops over-budget **pending** draft when committed queue fits; queue shows draft → total when draft exists |
| Docs | `UI_BOARD_ANCHORED_COMPONENTS.md` scheduled-state + pathfinding note |

Human playtest confirmed: door open/close status + path-through after Open work. Lock In sticky-fail was the pending-draft issue (Editor log `34.1s of 30.0s` while Used looked fine).

**Still not committed (leave as-is):** `ProjectSettings.asset` (`SENTIS_ANALYTICS_ENABLED`), `unity-first-open.log`.

## Verification

- Worktree `logiCard-verify-playtest` (Editor open on main `LogiCard` path): full suite after final Lock In draft-drop sync — **EditMode 99/99**, **PlayMode 28/28**.
- Manual Bootstrap smoke checklist still **not** formally recorded.

## Still unfinished

1. **Phase 6** — tune `HitRadius` / `LaneHalfWidth` / `InteractRadius`; cold-observer “door changes a fight once.” Human.
2. **Manual Bootstrap smoke** — full Time Card → Program → Lock In → playback → next round. Human.
3. **SCHEDULE.md** Day 7–8 boxes — tick only after the human Phase 6 call.
4. Optional cleanup: remove verify worktree/branch `verify/playtest-door-scrub` / `logiCard-verify-playtest`.

## Tomorrow / next agent

1. Human: Phase 6 cold-observer + Bootstrap smoke (items 1–2 above) — this is the gate.
2. Agent: do not start Day 9 board/UI identity until the human confirms the Phase 6 bar is honest.

## Blockers / notes

- Unity **6000.5.5f1**; project `/Users/xuxinye/Documents/projects/Game/LogiCard`.
- Batchmode: Editor closed on the **same** path, or a **different worktree path** in parallel. Do **not** pass `-quit` with `-runTests` (exits before the suite runs); use `-acceptSoftwareTermsForThisRunOnly` like prior verify runs.
- Hub “Add project”: select parent `…/Game`, not `LogiCard` itself.
- `master` tracks `origin/master`; today’s work is uncommitted locally only.
- Editor is typically open on the main path — use `logiCard-verify-playtest` for batchmode.
