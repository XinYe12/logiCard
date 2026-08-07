# Draft Handoff — 2026-08-07

**Schedule:** M2.5 continuous pivot + Day 8 URP done; Phase 6 human gate cleared. Day 9 board/UI identity landed on `master`, went through one real round of human feedback ("too plain and dull"), and got a follow-up lighting/post-processing fix. `master` is at `5ea34aa`. The match-over HUD fix is merged in too. **Still open: human visual sign-off on the new lighting pass** — that's the one gate left before ticking SCHEDULE Day 9.

**Heads up:** a different concurrent session also worked on this repo today (edited `master` directly, including a Day 9 pass that landed before a duplicate attempt in a worktree). That collision is fully resolved — see `Parallel worktrees` below — but if you're a fresh session picking this up, don't assume you're the only one touching `master` right now. Check `git status`/`git log` before building on anything, the way this session had to.

## Parallel worktrees

| Worktree | Branch | Status |
|----------|--------|--------|
| `logiCard-match-over-hud` | — | **Merged and removed.** Fixed the stale “R3 · ATTACKER PICKS” header and duplicate “MATCH OVER” button text. Now part of `master` (`0ad1991`). |
| `logiCard-day9-yarn` | — | **Deleted** (superseded — Day 9 landed on `master` directly instead). |
| `logiCard-verify-playtest` | `verify/playtest-door-scrub` @ `54b051a` | Still parked from an earlier verify run; optional remove. |

Only `logiCard-verify-playtest` remains as a leftover worktree. All disposable `logiCard-verify-*` worktrees this session were created and removed in the same session — none should exist right now; if you see one, it's stale and safe to remove.

## Implemented

**Committed on `master`, newest first:**

- `5ea34aa` / `c3a3832` — asmdef fixes: `LogiCard.Boot.asmdef` needed references to `Unity.RenderPipelines.Universal.Runtime` and `Unity.RenderPipelines.Core.Runtime` for the lighting code below to compile. If you add more `UnityEngine.Rendering(.Universal)` usage anywhere in `LogiCard.Boot`, this is already covered.
- `12f8a02` — **Lighting/post-processing fix**, in response to human feedback that Day 9 read as "too plain and dull" despite the yarn/cardstock/grid work landing:
  - Root cause wasn't color choice — the scene had one shadowless light, no fill, no ambient tuning, and **post-processing was off at both the URP Renderer asset level and the per-camera level**, so a Volume would have done nothing even before any of this.
  - `Assets/_Project/Art/URP/LogiCardURP_Renderer.asset`: wired in URP's built-in default `PostProcessData` asset (was `fileID: 0`).
  - `GameBootstrap.ConfigureCamera`: `renderPostProcessing = true` on the camera (URP defaults this to **off** per-camera even when the pipeline supports it).
  - `GameBootstrap.BuildLighting`: key light now casts soft shadows; added a dim cool fill light; flat ambient tuned warm.
  - `GameBootstrap.BuildDioramaVolume` (new): global Volume — warm/saturated color grade, restrained bloom, vignette (the "lit stage in a dark room" read ART_DIRECTION asks for).
  - `PrimitiveMaterialFactory`: materials now get a faint tiled procedural-noise grain (ART_DIRECTION's "subtle procedural noise," generated at runtime, no texture asset needed) and smoothness bumped 0.05 → 0.18 (dead-flat matte was reading as "unlit cardboard").
  - **Not yet visually confirmed** — this was built and verified by automated tests only (tests don't check what a render looks like). Needs a human look in the Editor.
- `0ad1991` — merge of `feat/match-over-hud`.
- `57fc1cd` — Day 9 presentation pass: yarn `LineRenderer` path (`PathPreviewView`), painted board grid (`BoardView.PlacePaintedGrid`), plywood ground tint, cardstock Time Card + AR scrubber styling (`ProgramHud`).
- `0b4f147` — SCHEDULE.md Days 4–8 ticked.
- `6f7acf9` — Tracer truncation fix: a blocked shot's tracer now stops at the wall/closed door instead of drawing through it (`Segment.TryGetIntersection` → `ArenaBoard.TryGetNearestBlockPoint` → `GhostResolver` stamps `ShootFire.Position` with the real visual endpoint).
- `ef05061` / `54b051a` — earlier playtest packs (door scrub, snap LoS, UNDO row, Lock In draft-drop; wall render, Hold Angle timing, door lifecycle).

**Leave uncommitted / do not ship:** `ProjectSettings.asset` (Sentis analytics churn), `unity-first-open.log`, and the parallel-development skill edits under `.claude/skills/parallel-development/` + `.cursor/skills/` — a different concurrent session touched those; not this workstream's to commit.

## Verification

- Lighting/post-processing fix (`5ea34aa`): disposable worktree, created and removed same session — **EditMode 102/102**, **PlayMode 29/29**, no exceptions in the PlayMode log from the new `GameBootstrap` code path. First attempt hit two compile errors (missing asmdef references), both fixed and reverified before this passed.
- Post-merge state (`0ad1991`, match-over-hud + Day 9 together): **EditMode 102/102**, **PlayMode 29/29**.
- Day 9 presentation (`57fc1cd`) standalone: **EditMode 102/102**, **PlayMode 28/28**.
- Match-over HUD (on its branch before merge): **EditMode 102/102**, **PlayMode 29/29**.
- Tracer truncation (`6f7acf9`): **EditMode 102/102**, **PlayMode 28/28**.
- Playtest pack (`ef05061`): **EditMode 99/99**, **PlayMode 28/28**.
- **Phase 6 human call: good enough as-is** — user declined further radius tuning.
- **Manual Bootstrap smoke: confirmed good by human** — full Time Card → Program → Lock In → playback → next round.
- **Wall/door "wound behind the wall" playtest finding: investigated, closed as not-a-bug.** A pawn opened the door, then shot through the gap it created. The wall itself held.
- **Not yet done:** human eyeballing the new lighting/material pass in the Editor. All of the above is test-verified, not look-verified.

## Still unfinished

1. **Human: look at the new lighting pass in the Editor.** Does it actually read as "desk-lamp diorama / digital clay" now, or does it need more iteration (light angles/intensities, grade strength, grain visibility are all easy to retune once someone's eyes are on it)?
2. Optional cleanup: remove parked `logiCard-verify-playtest`.

## Known issues (deferred, cosmetic — not a gate)

- Pawn model visually pokes through wall/closed-door geometry when its logical position sits at/near the wall plane. Cause: the sim tracks pawns as a point with no collider (deliberate — no Physics/Physics2D anywhere in resolve). Doesn't affect hit resolution, pathing, or LoS. User call: defer to a later pawn-model/art pass.

## Tomorrow / next agent

1. Get the human's visual sign-off on the lighting pass, iterate on numbers if needed (all in `GameBootstrap.BuildLighting`/`BuildDioramaVolume` and `PrimitiveMaterialFactory` — no structural changes needed, just tuning), then tick SCHEDULE Day 9.
2. Start **Day 10** — clay motion + physical VFX (stepped playback, muzzle flash, wound splat) — once Day 9 is accepted.
3. Don't reopen tracer truncation / `HitRadius` / `LaneHalfWidth` / `InteractRadius` tuning unless a new playtest finding says so — that's settled.

## Blockers / notes

- Unity **6000.5.5f1**; main project `/Users/xuxinye/Documents/projects/Game/LogiCard`. Editor is typically open there (user playtests live) — batchmode needs a different worktree path. Spin up a disposable one (`git worktree add`) and remove it after.
- If you edit a `.asset`/pipeline file on disk while the Editor has the project open (as this session did for the URP Renderer asset), Unity should pick it up via its file watcher on focus regain — if visuals don't update, try `Assets > Refresh` in the Editor.
- Do **not** pass `-quit` with `-runTests` (exits before the suite runs); use `-acceptSoftwareTermsForThisRunOnly`.
- Hub "Add project": select parent `…/Game`, not `LogiCard` itself.
- `master` tracks `origin/master`; everything above is local-only, not pushed.
- If you notice another session's edits mid-task (uncommitted changes you didn't make, or this file changing under you), stop and reconcile before continuing — don't just build on top blind. That's what happened with Day 9 today, and it cost real time to untangle.
