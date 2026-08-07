# Draft Handoff — 2026-08-07

**Schedule:** M2.5 continuous pivot + Day 8 URP done; Phase 6 human gate cleared. Day 9 board/UI identity landed on `master`, went through one real round of human feedback ("too plain and dull"), got a lighting/post-processing fix, and then the path visual got redesigned entirely (yarn → FragPunk-style ink line, human decision). **All of it — lighting fix and ink-line path rewrite — is committed and test-verified.** `master` is at `d4eb6ca`. The match-over HUD fix is merged in too. **Still open: human visual sign-off in the Editor** — everything below is verified by automated tests, none of it has been looked at yet.

**Heads up — read this if you're a fresh session:** at least one other concurrent session (Cursor-based, working from a "Art UI Decisions" plan) was actively editing this exact `master` working tree today, at the same time as this session, more than once. Both collisions were caught and reconciled cleanly (see below), but don't assume you're the only one touching this repo. Check `git status` and `git log` before building on anything, and if a file changes under you mid-task, stop and reconcile before continuing.

## Parallel worktrees

| Worktree | Branch | Status |
|----------|--------|--------|
| `logiCard-match-over-hud` | — | **Merged and removed.** Fixed the stale “R3 · ATTACKER PICKS” header and duplicate “MATCH OVER” button text. Now part of `master` (`0ad1991`). |
| `logiCard-day9-yarn` | — | **Deleted** (superseded — Day 9 landed on `master` directly instead). |
| `logiCard-verify-playtest` | `verify/playtest-door-scrub` @ `54b051a` | Still parked from an earlier verify run; optional remove. |

Only `logiCard-verify-playtest` remains as a leftover worktree. Every disposable `logiCard-verify-*` worktree from this session was created and removed within the same session — none should exist right now; if you see one, it's stale and safe to remove.

## Implemented

**Committed on `master`, newest first:**

- `d4eb6ca` — finished propagating the path-art decision's terminology across the whole doc corpus (`SCHEDULE.md`, `GDD.md`, `PRODUCT_MEMORY.md`, `RISKS.md`, `SCOPE.md`, `UI_FLOW.md`, `VERTICAL_SLICE.md`, `Art/README.md`, `URP_AGENT_BRIEF.md`, `.cursor/rules/logicard-product-memory.mdc`) — a concurrent session started this, this session finished the remaining "yarn/chalk" references.
- `950b0ac` — **Path visual pivot, code + docs**: human decision (2026-08-07, via a Cursor plan) dropped Day 9's 3D yarn entirely in favor of a FragPunk/界外狂潮-style **线稿涂鸦** (thin, slightly wobbly hand-drawn ink line on the board surface — not fat spray, not a glitchy HUD line, not neon).
  - `PathPreviewView.cs` rewritten: `LineRenderer` stroke lying close to the board surface, subdivided per leg with a deterministic Perlin-seeded sideways wobble (stable across calls — a draft path being actively dragged doesn't jitter). Draft reads like **pencil** (lighter gray, thinner, rougher/more wobble); booked reads like **settled ink** (dark charcoal, bolder, steadier) — a sketch-to-ink metaphor, not a port of yarn's old opacity logic. Waypoint dots restyled to the same ink language instead of the old warm terracotta pins. `Init`/`Show`/`Clear` API unchanged — no caller needed to change.
  - `ART_DIRECTION.md`: path pillar (Demo art floor table), §4 UI/UX bullet, cut-order line, Do/Don't table all updated to describe the ink line and explicitly mark yarn/chalk as superseded.
- `5ea34aa` / `c3a3832` — asmdef fixes: `LogiCard.Boot.asmdef` needed references to `Unity.RenderPipelines.Universal.Runtime` and `Unity.RenderPipelines.Core.Runtime` for the lighting code below to compile.
- `12f8a02` — **Lighting/post-processing fix**, in response to human feedback that Day 9 read as "too plain and dull" despite the path/cardstock/grid work landing:
  - Root cause wasn't color choice — the scene had one shadowless light, no fill, no ambient tuning, and **post-processing was off at both the URP Renderer asset level and the per-camera level**, so a Volume would have done nothing even before any of this.
  - `Assets/_Project/Art/URP/LogiCardURP_Renderer.asset`: wired in URP's built-in default `PostProcessData` asset (was `fileID: 0`).
  - `GameBootstrap.ConfigureCamera`: `renderPostProcessing = true` on the camera (URP defaults this to **off** per-camera even when the pipeline supports it).
  - `GameBootstrap.BuildLighting`: key light now casts soft shadows; added a dim cool fill light; flat ambient tuned warm.
  - `GameBootstrap.BuildDioramaVolume` (new): global Volume — warm/saturated color grade, restrained bloom, vignette (the "lit stage in a dark room" read ART_DIRECTION asks for).
  - `PrimitiveMaterialFactory`: materials now get a faint tiled procedural-noise grain (ART_DIRECTION's "subtle procedural noise," generated at runtime, no texture asset needed) and smoothness bumped 0.05 → 0.18 (dead-flat matte was reading as "unlit cardboard"). This same factory is what the new ink-line stroke/dots use too.
- `0ad1991` — merge of `feat/match-over-hud`.
- `57fc1cd` — Day 9 presentation pass: painted board grid (`BoardView.PlacePaintedGrid`), plywood ground tint, cardstock Time Card + AR scrubber styling (`ProgramHud`). (Its yarn path piece was superseded by `950b0ac` above.)
- `0b4f147` — SCHEDULE.md Days 4–8 ticked.
- `6f7acf9` — Tracer truncation fix: a blocked shot's tracer now stops at the wall/closed door instead of drawing through it.
- `ef05061` / `54b051a` — earlier playtest packs (door scrub, snap LoS, UNDO row, Lock In draft-drop; wall render, Hold Angle timing, door lifecycle).

**Leave uncommitted / do not ship:** `ProjectSettings.asset` (Sentis analytics churn), `unity-first-open.log`, and the parallel-development skill edits under `.claude/skills/parallel-development/` + `.cursor/skills/` — a different concurrent session touched those; not this workstream's to commit.

## Verification

- Path visual pivot (`950b0ac`): disposable worktree, created and removed same session — **EditMode 102/102**, **PlayMode 29/29**.
- Lighting/post-processing fix (`5ea34aa`): **EditMode 102/102**, **PlayMode 29/29**, no exceptions in the PlayMode log. First attempt hit two compile errors (missing asmdef references), both fixed and reverified before this passed.
- Post-merge state (`0ad1991`, match-over-hud + Day 9 together): **EditMode 102/102**, **PlayMode 29/29**.
- Day 9 presentation (`57fc1cd`) standalone: **EditMode 102/102**, **PlayMode 28/28**.
- Match-over HUD (on its branch before merge): **EditMode 102/102**, **PlayMode 29/29**.
- Tracer truncation (`6f7acf9`): **EditMode 102/102**, **PlayMode 28/28**.
- Playtest pack (`ef05061`): **EditMode 99/99**, **PlayMode 28/28**.
- **Phase 6 human call: good enough as-is** — user declined further radius tuning.
- **Manual Bootstrap smoke: confirmed good by human** — full Time Card → Program → Lock In → playback → next round.
- **Wall/door "wound behind the wall" playtest finding: investigated, closed as not-a-bug.** A pawn opened the door, then shot through the gap it created. The wall itself held.
- **Not yet done, at all:** a human looking at either the lighting pass or the new ink-line path in the Editor. Everything above is test-verified only — tests don't check what a render looks like.

## Still unfinished

1. **Human: look at both the lighting pass and the ink-line path in the Editor.** Do they read as intended? Both are easy to retune from feedback — lighting numbers live in `GameBootstrap.BuildLighting`/`BuildDioramaVolume` and `PrimitiveMaterialFactory`; path stroke numbers (width, wobble amount, colors) live at the top of `PathPreviewView.cs`.
2. Tick SCHEDULE Day 9 once both are accepted.
3. Optional cleanup: remove parked `logiCard-verify-playtest`.

## Known issues (deferred, cosmetic — not a gate)

- Pawn model visually pokes through wall/closed-door geometry when its logical position sits at/near the wall plane. Cause: the sim tracks pawns as a point with no collider (deliberate — no Physics/Physics2D anywhere in resolve). Doesn't affect hit resolution, pathing, or LoS. User call: defer to a later pawn-model/art pass.

## Tomorrow / next agent

1. Get the human's visual sign-off on lighting + ink-line path, iterate on numbers if needed, then tick SCHEDULE Day 9.
2. Start **Day 10** — clay motion + physical VFX (stepped playback, muzzle flash, wound splat) — once Day 9 is accepted.
3. Don't reopen tracer truncation / `HitRadius` / `LaneHalfWidth` / `InteractRadius` tuning unless a new playtest finding says so — that's settled.

## Blockers / notes

- Unity **6000.5.5f1**; main project `/Users/xuxinye/Documents/projects/Game/LogiCard`. Editor is typically open there (user playtests live) — batchmode needs a different worktree path. Spin up a disposable one (`git worktree add`) and remove it after.
- If you edit a `.asset`/pipeline file on disk while the Editor has the project open (as this session did for the URP Renderer asset), Unity should pick it up via its file watcher on focus regain — if visuals don't update, try `Assets > Refresh` in the Editor.
- Do **not** pass `-quit` with `-runTests` (exits before the suite runs); use `-acceptSoftwareTermsForThisRunOnly`.
- Hub "Add project": select parent `…/Game`, not `LogiCard` itself.
- `master` tracks `origin/master`; everything above is local-only, not pushed.
- Two separate concurrent-session collisions happened this session (a duplicate Day 9 implementation, and a live doc-editing overlap during the path-art pivot). Both were caught by checking `git status`/`git log` before committing and reconciled without losing work, but it cost real time. If you're picking this up next: check for concurrent activity *before* you start building, not after.
