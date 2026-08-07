# Draft Handoff — 2026-08-07

**Schedule:** M2.5 continuous pivot + Day 8 URP done; Phase 6 human gate cleared. **Day 9 board/UI identity is committed on `master`** (`57fc1cd`), verified, but not yet visually eyeballed by a human — that's the one thing left before ticking SCHEDULE Day 9. Match-over HUD fix is done on its own branch (`0d1a7ac`), not yet merged into `master`.

**Heads up for whoever reads this next:** two different sessions worked on Day 9 concurrently today — one directly on `master`, one (this session) in the `logiCard-day9-yarn` worktree. The `master` version landed first and is more complete, so the worktree's duplicate edit was discarded (never committed) and that worktree is now superseded. If you see contradictory notes below from an earlier version of this file, trust the state described here — it was written after reconciling both.

## Parallel worktrees

| Worktree | Branch | Status |
|----------|--------|--------|
| `logiCard-match-over-hud` | `feat/match-over-hud` @ `0d1a7ac` | **Done.** Fixed the stale “R3 · ATTACKER PICKS” header and duplicate “MATCH OVER” button text; PlayMode regression test added. Ready for user to merge into `master` whenever they want it in. |
| `logiCard-day9-yarn` | `feat/day9-yarn-ui` @ `54b051a` | **Superseded** — Day 9 landed on `master` instead (`57fc1cd`). Nothing of value in this worktree; safe to remove. |
| `logiCard-verify-playtest` | `verify/playtest-door-scrub` @ `54b051a` | Parked from an earlier verify run; optional remove. |

## Implemented

**Committed on `master`, newest first:**

- `57fc1cd` — **Day 9 presentation pass** (ART_DIRECTION §4 Demo art floor):
  - `PathPreviewView.cs`: yarn `LineRenderer` strand + pin beads replace the Day 5 bead-only placeholder (draft lighter/unsettled, booked settled); `Init`/`Show`/`Clear` API unchanged.
  - `BoardView.cs`: `PlacePaintedGrid` — etched unit-grid strokes on the board face.
  - `GameBootstrap.cs`: ground tint shifted toward plywood/cardstock.
  - `ProgramHud.cs`: Allot/Time Card panel gets a cardstock paper face + soft shadow; Time Resource scrubber restyled to a cool cyan/white AR look, contrasting with the clay board. Door prompt, Lock In, and Aftermath/match-label methods untouched — those are `feat/match-over-hud`'s and main's territory respectively.
- `0b4f147` — SCHEDULE.md Days 4–8 ticked.
- `6f7acf9` — Tracer truncation fix: `Segment.TryGetIntersection` → `ArenaBoard.TryGetNearestBlockPoint` → `GhostResolver` stamps `ShootFire.Position` with the tracer's real visual endpoint instead of the raw aim point, so a blocked shot's beam now stops at the wall/closed door instead of drawing through it.
- `ef05061` / `54b051a` — earlier playtest packs (door scrub, snap LoS, UNDO row, Lock In draft-drop; wall render, Hold Angle timing, door lifecycle).

**Leave uncommitted / do not ship:** `ProjectSettings.asset` (Sentis analytics churn), `unity-first-open.log`, and the parallel-development skill edits under `.claude/skills/parallel-development/` + `.cursor/skills/` — a different concurrent session touched those; not this workstream's to commit.

## Verification

- Day 9 (`57fc1cd`): disposable worktree `logiCard-verify-day9` (created and removed same session) — **EditMode 102/102**, **PlayMode 28/28**.
- Match-over HUD (`0d1a7ac`): verified in its own worktree — **EditMode 102/102**, **PlayMode 29/29** (28 + 1 new).
- Tracer truncation (`6f7acf9`): **EditMode 102/102** (99 + 3 new), **PlayMode 28/28**.
- Playtest pack (`ef05061`): **EditMode 99/99**, **PlayMode 28/28**.
- **Phase 6 human call (2026-08-07): good enough as-is** — user declined further radius tuning.
- **Manual Bootstrap smoke (2026-08-07): confirmed good by human** — full Time Card → Program → Lock In → playback → next round.
- **Wall/door "wound behind the wall" playtest finding: investigated, closed as not-a-bug.** Traced via the Console's per-round event log — a pawn opened the door, then shot through the gap it created. The wall itself held (an earlier shot from the same pawn truncated correctly at the wall face). No code change needed.
- **Not yet done:** a human eyeballing Day 9's actual look in the Editor (yarn path, painted grid, cardstock Allot, AR scrubber) — automated tests don't check visuals. This is the one gate left before ticking SCHEDULE Day 9.

## Still unfinished

1. **Human: eyeball Day 9 in the Editor** — does the yarn/chalk path, painted grid, cardstock Time Card, and AR scrubber actually read as intended? Tick SCHEDULE Day 9 if so.
2. **Merge `feat/match-over-hud` into `master`** (user call, whenever convenient) — expect a clean merge on `ProgramHud.cs` since it and Day 9 touch disjoint methods (Allot/scrubber vs. Aftermath/match-label).
3. Optional cleanup: remove superseded `logiCard-day9-yarn` and parked `logiCard-verify-playtest` worktrees/branches.

## Known issues (deferred, cosmetic — not a gate)

- Pawn model visually pokes through wall/closed-door geometry when its logical position sits at/near the wall plane. Cause: the sim tracks pawns as a point with no collider (deliberate — no Physics/Physics2D anywhere in resolve), so nothing pushes the render mesh out of the wall mesh. Doesn't affect hit resolution, pathing, or LoS. User call: defer to a later pawn-model/art pass.

## Tomorrow / next agent

1. Get the human's visual sign-off on Day 9, then tick SCHEDULE Day 9.
2. Merge `feat/match-over-hud` when the user's ready.
3. Start **Day 10** — clay motion + physical VFX (stepped playback, muzzle flash, wound splat) — once Day 9 is accepted.
4. Don't reopen tracer truncation / `HitRadius` / `LaneHalfWidth` / `InteractRadius` tuning unless a new playtest finding says so — that's settled for now.

## Blockers / notes

- Unity **6000.5.5f1**; main project `/Users/xuxinye/Documents/projects/Game/LogiCard`. Editor is typically open there (user playtests live) — batchmode needs a different worktree path. Spin up a disposable one (`git worktree add`) and remove it after, rather than reusing `logiCard-verify-playtest` (parked) or the other agents' worktrees.
- Do **not** pass `-quit` with `-runTests` (exits before the suite runs); use `-acceptSoftwareTermsForThisRunOnly`.
- Hub "Add project": select parent `…/Game`, not `LogiCard` itself.
- `master` tracks `origin/master`; everything above is local-only, not pushed.
- If you notice another session's edits mid-task (uncommitted changes you didn't make, or this file changing under you), stop and reconcile before continuing — don't just build on top blind. That's what happened with Day 9 today.
