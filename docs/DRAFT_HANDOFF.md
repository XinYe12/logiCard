# Draft Handoff — 2026-08-07

**Schedule:** M2.5 continuous pivot (Days 7b–7g) code-complete on `master` through `6f7acf9`. Day 8 URP merged. 2026-08-07 playtest follow-up pack and the tracer-truncation fix are **both committed**. **Phase 6 gate is cleared** (human call, 2026-08-07 — see below); Day 9 is unblocked, but see the parallel-worktree table for who already owns which surface.

**Parallel work in flight (separate worktrees, not yet merged — user reconciles by hand):**

| Worktree | Branch | Owns | Status as of this handoff |
|----------|--------|------|----------------------------|
| `logiCard-day9-yarn` | `feat/day9-yarn-ui` | Yarn/chalk `PathPreviewView` + Time Card/scrubber presentation | Forked at `54b051a` (stale — predates both commits below); brief at `DAY9_YARN_AGENT_BRIEF.md` |
| `logiCard-match-over-hud` | `feat/match-over-hud` | `ProgramHud.RefreshMatchLabel` / `RefreshAftermathPanel` — stale “R3 · ATTACKER PICKS” header on MatchOver + duplicate “MATCH OVER” button label (playtest 2026-08-07) | Forked at `0b4feec`; brief at `MATCH_OVER_HUD_AGENT_BRIEF.md` |
| `logiCard-verify-playtest` | `verify/playtest-door-scrub` | Parked verify from the earlier 99/99·28/28 run | Do not merge/retarget |

Do not edit `ProgramHud.cs` Aftermath/match-label methods or `PathPreviewView.cs` from `master` — those are claimed by the worktrees above.

## Implemented

**Already on `master` (`54b051a` and earlier):** continuous Phases 1–5 (C35/C39/C40/C41); 2026-08-06 playtest pack (wall yaw, Hold tracer window, door Closed-at-start + Aftermath carry, select-then-confirm door, board-anchored OPEN/CLOSE, `InteractRadius` 0.7, rejection banner, Lock In respects pending draft).

**Committed `6f7acf9` (2026-08-07, playtest follow-up #2 — bullets visually passing through walls/closed doors):**

| Area | What landed |
|------|-------------|
| Tracer truncation | `Segment.TryGetIntersection` (exact crossing point) + `ArenaBoard.TryGetNearestBlockPoint`; `GhostResolver.ResolveShot` now stamps `ShootFire`'s `Position` with the tracer's real visual endpoint (blocked point, or aim point if clear) instead of always the raw aim point |

Hit resolution itself was already correct (LoS-gated) — this was purely the tracer drawing past the obstacle it was blocked by. `TapeEvent.Position` doc comment updated to reflect the new semantics; no other consumer existed besides `RoundPlayback.BuildTracers`.

**Committed `ef05061` (2026-08-07 playtest follow-ups #1):**

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

- `ef05061` (playtest follow-ups #1): worktree `logiCard-verify-playtest` — **EditMode 99/99**, **PlayMode 28/28**.
- `6f7acf9` (tracer truncation): disposable worktree `logiCard-verify-tracer` (created and removed same session — main path's Editor was open, so batchmode couldn't run there) — **EditMode 102/102** (99 + 3 new tracer-truncation tests), **PlayMode 28/28**.
- **Phase 6 human call (2026-08-07): good enough as-is.** User explicitly declined to tune `HitRadius`/`LaneHalfWidth`/`InteractRadius` further — "they look alright for now."
- **Wall/door tracer playtest finding, investigated and closed as not-a-bug:** user saw a pawn wounded while apparently "behind the wall" in the Aftermath end-state screenshot. Traced via the Console's per-round `[logiCard] Ghost resolve` event log (Round 3): `6.20s P2 DoorOpened (2,2)` → `8.20s P2 ShootFire (2,1)` → wounds P1. The wall itself held (P1's two shots at 5.59s/7.59s both truncated at the wall face, x=1.4/1.69, short of the door gap at x∈[1.75,2.25]) — P2 simply opened the door first, then shot through the gap it created. Aftermath's final positions don't match shot-time positions since pawns keep moving after being hit, which is what made it look wrong at a glance. No code change needed; the door mechanic worked as designed.
- **Manual Bootstrap smoke (2026-08-07): confirmed good by human** — full Time Card → Program → Lock In → playback → next round, done live in-Editor across the session above (the match-over/aftermath screenshots came from this same run).

## Still unfinished

1. **SCHEDULE.md** Day 7–8 boxes — tick now that the Phase 6 human call landed (see Verification above).
2. Merge/reconcile `feat/day9-yarn-ui` and `feat/match-over-hud` once those agents report back (see parallel work table above) — user reconciles by hand, not an agent merge.
3. Optional cleanup: remove verify worktree/branch `verify/playtest-door-scrub` / `logiCard-verify-playtest`.

## Known issues (deferred, cosmetic — not a gate)

- **Pawn model visually pokes through wall/closed-door geometry** when its logical position sits at/near the wall plane (e.g. standing right at a doorway). Cause: the sim tracks pawns as a point (no collider/physical volume by design — C32, no Physics/Physics2D involved in resolve), so nothing pushes the render mesh out of the wall mesh. Does not affect hit resolution, pathing, or LoS — those are all point/segment math and already correct. User call (2026-08-07): defer to a later pawn-model/art pass rather than fixing now.

## Tomorrow / next agent

1. Phase 6 gate is cleared — Day 9 board/UI identity work is unblocked, but it's already claimed by `logiCard-day9-yarn` (stale, forked at `54b051a` — needs rebasing onto `6f7acf9` before it can land) and `logiCard-match-over-hud`. Don't duplicate that work on `master`; check whether either has reported back before starting anything new there.
2. Confirm with the user before merging either worktree back into `master`.

## Blockers / notes

- Unity **6000.5.5f1**; project `/Users/xuxinye/Documents/projects/Game/LogiCard`.
- Batchmode: Editor closed on the **same** path, or a **different worktree path** in parallel. Do **not** pass `-quit` with `-runTests` (exits before the suite runs); use `-acceptSoftwareTermsForThisRunOnly` like prior verify runs.
- Hub “Add project”: select parent `…/Game`, not `LogiCard` itself.
- `master` tracks `origin/master`; all of today's work (both commits) is local-only, not pushed.
- Editor is typically open on the main path (user playtests there live) — use a separate worktree for batchmode; spin up a disposable one (`git worktree add`) and remove it after if none of the existing ones fit, rather than reusing `logiCard-verify-playtest` (parked) or the Day 9/match-over-hud worktrees (owned by other agents).
