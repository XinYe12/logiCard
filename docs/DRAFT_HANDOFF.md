# Draft Handoff — 2026-08-07

**Schedule:** M2.5 continuous pivot + Day 8 URP done; Phase 6 human gate cleared. **Day 9 is DONE and accepted** (2026-08-07) — board/UI identity landed on `master`, went through a full feedback loop (too plain/dull → lighting fix → path redesigned to an ink line → red-tint bug found and fixed by the first real in-Editor look), got human sign-off, SCHEDULE ticked. `master` is at `08112e1`. **Day 10 (clay motion + physical VFX) is now in progress**, split across two workers: main (this session, on `master`) is doing stepped 8–12fps playback motion plus wiring hit-VFX into the playback loop; a parallel agent in `logiCard-day10-vfx` is building the two new VFX view components (muzzle flash, wound splat) main will wire in. Neither side has landed anything for Day 10 yet as of this draft.

**Day 9 sign-off, read this before touching board/path art again:** human accepted the current board/path look **with reservations** — not fully satisfied with the board's visual quality, but explicit that schedule takes priority over further art polish right now ("if that is what we can do in this schedule I am fine with it," said about both the board and the path). This is a **conscious tradeoff, not an oversight** — don't restart art iteration on the board or path without the human raising it again. If Day 10+ work touches rendering and there's slack, revisiting board quality is fair game, but it is not blocking and was not asked for.

**Heads up — read this if you're a fresh session:** at least one other concurrent session (Cursor-based, working from a "Art UI Decisions" plan) was actively editing this exact `master` working tree today, at the same time as this session, more than once. Both collisions were caught and reconciled cleanly (see below), but don't assume you're the only one touching this repo. Check `git status` and `git log` before building on anything, and if a file changes under you mid-task, stop and reconcile before continuing.

## Parallel worktrees

| Worktree | Branch | Status |
|----------|--------|--------|
| `logiCard-day10-vfx` | `feat/day10-hit-vfx` @ `08112e1` | **New, in progress.** Brief at `DAY10_HIT_VFX_AGENT_BRIEF.md` in that worktree. Building `Assets/_Project/Board/MuzzleFlashView.cs` + `WoundSplatView.cs` — two new, self-contained view components (`Init`/`Place`/`SetVisible`, matching the existing `ShotTracerView.cs` pattern). Does **not** touch `PawnView.cs`/`RoundPlayback.cs`/`GameBootstrap.cs` — main wires those in afterward. Not merged; check `git log --oneline master..feat/day10-hit-vfx` in the main tree to see if it's reported back yet. |
| `logiCard-match-over-hud` | — | **Merged and removed.** Fixed the stale “R3 · ATTACKER PICKS” header and duplicate “MATCH OVER” button text. Now part of `master` (`0ad1991`). |
| `logiCard-day9-yarn` | — | **Deleted** (superseded — Day 9 landed on `master` directly instead). |
| `logiCard-verify-playtest` | `verify/playtest-door-scrub` @ `54b051a` | Still parked from an earlier verify run; optional remove. |

Every disposable `logiCard-verify-*` worktree from earlier today was created and removed within the same session — none should exist right now; if you see one, it's stale and safe to remove. `logiCard-day10-vfx` is the one active in-flight worktree.

## Implemented

**Committed on `master`, newest first:**

- `727ebd4` — **Fixed a real bug the human's first in-Editor look caught**: board/walls/pawns all rendered nearly pure red, while the untouched camera background stayed correct. Root cause: `PrimitiveMaterialFactory`'s clay-grain texture (added in `12f8a02`) used `TextureFormat.R8` — a single-channel format. `SetPixels32` was fed matching R=G=B pixel values, but R8 only physically stores red; green/blue silently drop on `Apply()`. Sampled as `_BaseMap`, that comes back `(r, 0, 0, 1)`, so `BaseColor * BaseMap` crushed every tinted material's green/blue toward zero regardless of its intended tint — hence everything reading red-ish. Fix: `RGB24` instead of `R8`. This is exactly the kind of bug automated tests can't catch (nothing asserts on rendered color) — first reason this session needed an actual human look, not just green test runs.
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

- Red-tint fix (`727ebd4`): **EditMode 102/102**, **PlayMode 29/29**. Caught by the human's first real look in the Editor — the whole board/walls/pawns had gone red. Not yet re-confirmed visually after the fix (tests pass, but tests don't check color).
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
- **Human visual sign-off (2026-08-07): DONE.** Colors confirmed back to normal after the red-tint fix. Board accepted with reservations (not fully satisfied with visual quality, but schedule > further polish — see the sign-off note at the top of this doc). Path accepted on the same terms. **SCHEDULE Day 9 ticked.**

## Still unfinished

1. **Day 10, main's half:** stepped 8–12fps playback motion (`PawnView.cs` — quantize the sampled Time Resource second so poses snap instead of interpolate smoothly, per ART_DIRECTION §2). Not started yet as of this draft.
2. **Day 10, parallel half:** waiting on `logiCard-day10-vfx` to report back (`MuzzleFlashView.cs` + `WoundSplatView.cs`). Once it does: review, verify (disposable worktree, same pattern as every other verify this session), merge into `master` (your call, not an agent's), then wire both components into `RoundPlayback`'s tape-event loop (`ShootFire` → muzzle flash briefly; `Wounded`/`Killed` → wound splat persists) the same way `BuildTracers`/`UpdateTracers` already drive `ShotTracerView`.
3. Optional cleanup: remove parked `logiCard-verify-playtest`.
4. Optional, not requested: board visual polish beyond the current floor, if slack appears later — see the Day 9 sign-off note above before touching this unprompted.

## Known issues (deferred, cosmetic — not a gate)

- Pawn model visually pokes through wall/closed-door geometry when its logical position sits at/near the wall plane. Cause: the sim tracks pawns as a point with no collider (deliberate — no Physics/Physics2D anywhere in resolve). Doesn't affect hit resolution, pathing, or LoS. User call: defer to a later pawn-model/art pass.

## Tomorrow / next agent

1. Check `logiCard-day10-vfx` for a report-back, and implement stepped-motion playback on `master` (see "Still unfinished" above for both halves).
2. Once both land: merge the VFX branch, wire it in, verify the full suite, then tick SCHEDULE Day 10 with a human look (screenshots or live Editor) — same pattern as Day 9: tests passing is not the same as it looking right.
3. Don't reopen tracer truncation / `HitRadius` / `LaneHalfWidth` / `InteractRadius` tuning unless a new playtest finding says so — that's settled.
4. Don't restart board/path art polish unprompted — see the Day 9 sign-off note at the top of this doc.

## Blockers / notes

- Unity **6000.5.5f1**; main project `/Users/xuxinye/Documents/projects/Game/LogiCard`. Editor is typically open there (user playtests live) — batchmode needs a different worktree path. Spin up a disposable one (`git worktree add`) and remove it after.
- If you edit a `.asset`/pipeline file on disk while the Editor has the project open (as this session did for the URP Renderer asset), Unity should pick it up via its file watcher on focus regain — if visuals don't update, try `Assets > Refresh` in the Editor.
- Do **not** pass `-quit` with `-runTests` (exits before the suite runs); use `-acceptSoftwareTermsForThisRunOnly`.
- Hub "Add project": select parent `…/Game`, not `LogiCard` itself.
- `master` tracks `origin/master`; everything above is local-only, not pushed.
- Two separate concurrent-session collisions happened this session (a duplicate Day 9 implementation, and a live doc-editing overlap during the path-art pivot). Both were caught by checking `git status`/`git log` before committing and reconciled without losing work, but it cost real time. If you're picking this up next: check for concurrent activity *before* you start building, not after.
