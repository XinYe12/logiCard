# Draft Handoff — 2026-08-07

## 2026-08-10 (continued 5) — stepped pawn animation dropped for smooth interpolation (C55)

**Human noticed something real from a playtest screenshot** (`screenshots/image copy 2.png` — reflection
progress visible, flagged as still needing more work): character movement stuttered noticeably while
rain animated smoothly, and asked whether that's a game problem or their computer.

**Diagnosed, not guessed:** `PawnView.ApplyTime` (`Assets/_Project/Board/PawnView.cs:44`, pre-fix) had a
`StepIntervalSeconds = 1f/10f` real-time throttle on pose updates, added Day 10 (`d60f01d`) to implement
`ART_DIRECTION.md` §2's "Stop-Motion Feel" pillar (`C29`) — required-for-ship stepped 8–12fps character
motion, explicitly not a bug. Rain is an unthrottled Shuriken particle system, so the contrast was
expected, not a sign of dropped frames. Confirmed this has nothing to do with the human's hardware — the
throttle is deterministic and keyed off `Time.unscaledTime`, same on any machine.

**Given the choice (keep / raise the rate / drop it), the human chose to drop it entirely.** Reasoning
surfaced to them before they decided: the stop-motion pillar was authored against flat-shaded clay
primitives and the toy-chibi character framing `C53` already superseded — now that materials/lighting/
doors have moved toward photorealism, the same held-pose technique reads as a framerate bug rather than a
deliberate style choice. Recorded as `C55` in `PRODUCT_MEMORY.md`.

**Implemented directly in the main tree** (small, well-understood change, Integrator-owned `Board/`
territory is otherwise closed out this pass): removed the throttle fields/logic from `PawnView.ApplyTime`
— it now applies every call, matching the render framerate of everything else in the scene. `C23`'s "no
root motion" rule is unaffected (still Host/ReplayTape-driven transforms, not an Animator). Removed the
now-dead `WaitForPawnStepRelease()` PlayMode test helper and its 3 call sites; two tests that existed only
to wait out the throttle no longer need to be coroutines (`[UnityTest] IEnumerator` → `[Test] void`).

**Actually verified, not just self-reviewed** — Editor was still open/live on the main tree, so spun a
disposable detached worktree (`logiCard-verify-c55`, created and removed same session) and ran real
batchmode: **EditMode 124/124, PlayMode 37/37**, both green. Removed the worktree after.

`ART_DIRECTION.md` §2 rewritten (old pillar kept in a collapsed `<details>` block, marked superseded, not
deleted — this project's established convention); `PRODUCT_MEMORY.md` C29 cross-referenced to the new C55
row; `TDD.md` §5 and `PAWN_ART_REWORK_PLAN.md`'s stale "stepped 8-12fps" mentions corrected.

## 2026-08-10 (continued 4) — both worker slots landed: real URP post-processing, real doors

**`feat/urp-post-processing` merged** (`f2d9ca9`, worker commit `17db2bb`). Fixes every gap the URP
audit found: `LogiCardVolumeProfile.asset` wired into `LogiCardURP.asset`'s `m_VolumeProfile` (ACES
tonemap, cool-filter color grading, restrained bloom on warm glass/practicals, vignette), SSAO renderer
feature added (`m_RendererFeatures` was `[]`), MSAA 1→4, `m_AdditionalLightShadowsSupported` 0→1 (so the
checkpoint-2 point-light practicals actually cast the soft shadows they were already configured for),
`m_SoftShadowsSupported` 0→1, `m_ShadowCascadeCount` 1→2. Also shipped the C54 photo-mode stretch goal:
`PhotoModeController` (new `Assets/_Project/Rendering/`) swaps the scene's Diorama Volume between the
live readability grade and a stronger hero-shot profile (`LogiCardVolumeProfile_Photo.asset`) on `F9`.
SSR investigated and found infeasible in this URP version (17.5.0, no SSR renderer feature
package-side) — wet-street reflections still need probes or a planar reflection later, not done here.
Scoped `GameBootstrap.cs` touch (2 lines: one `using`, one `AddComponent` call) matched the brief exactly.

**`feat/env-checkpoint3-doors` merged** (`0acdbef`, 2 worker commits). Imports the Quaternius Ultimate
House Interior Pack (CC0, C54's chosen pack) via a new `InteriorPackImportTool.cs` (same FBX→URP/Lit
pattern as `PawnImportTool`), replaces the tinted-box door placeholders with real fitted door meshes —
hinge swings ~95° open, and the standing green/red state tint is preserved, still driven by the
authoritative `Door` model state via `ApplyDoorVisualState`, not inferred — and swaps Yard/Hall/Vault's
primitive cube dressing for real cabinets, shelves, tables, chairs, window frames, and ceiling lights.
Falls back to the old tinted-box/cube behavior if the imported prefabs aren't found (keeps EditMode tests
green without needing `Resources.Load` to succeed on a synthetic board). Camera/`orthographicSize`
untouched per C54 — worker flagged it as possibly needing a look once doors are actually seen, not
retuned blind. `THIRD_PARTY.md` updated: Quaternius selected and imported, KayKit rejected.

**Both merges were clean, no conflicts** — despite both branches independently touching
`Assets/_Project/Boot/GameBootstrap.cs` (URP: 2-line scoped addition; doors: none in the end) and one
touching `BoardView.cs` on top of the other's fork point, git's 3-way merge resolved everything
correctly. Each worker's own doc edits (stale-base branch-divergence artifacts, same pattern as the
prior two merges) resolved back to master's current docs with zero diff — confirmed before committing,
not assumed.

**Batchmode not independently re-run on `master` post-merge** — Editor still open/live, same recurring
constraint. Both workers reported clean runs on their own branches (URP: EditMode 124/124, PlayMode
37/37; doors: EditMode 124/124, PlayMode 37/37) via their own batchmode jobs (not self-reported without
evidence — actual `Finished Run ... batchmode` tool results). Self-reviewed both diffs directly instead
of re-running: URP asset diff matches the audit finding line-for-line, `PhotoModeController.cs` is
defensive (null-checks missing resources, logs rather than throws), door mesh code preserves the
door-state-drives-tint contract and strips colliders consistent with C40. Checked `Logs/Editor.log` for
compile errors — none present, though the Editor hasn't regained focus/recompiled since these files
landed on disk, so this isn't a positive confirmation, just an absence of a negative one.

**Still needed from the human:** a Play + screenshot of Yard→Hall→Vault (checkpoint 2's ask) and of
Door #1 closed/open (checkpoint 3's ask) — both workers explicitly said they have no screenshot-capture
capability themselves. `orthographicSize` (5.0) may need a look once real doors are visible at that
framing. Character models (Quaternius chibi pawns) still haven't been reviewed in-Editor since the C53
pivot — the one open item from the original look-and-feel punch list that nothing has touched yet.

## 2026-08-10 (continued 3) — env checkpoint 2 merged; two decisions now waiting on the human

**`feat/env-lookfeel-overhaul` checkpoint 2 merged** (`3feccca`, 3 worker commits). Real, visible change:
room-zoned wet-dusk board surfaces via real CC0 Poly Haven PBR textures (asphalt Yard, concrete Hall,
polished Vault, brick walls, wood-framed terrain edge) through a new `BoardSurfaceMaterials.cs`, plus
warm point-light practicals at Hall/Vault/Yard window dressing. Merged clean, no conflicts — the
`BoardCameraRig.cs`/tests branch-divergence artifact flagged earlier resolved correctly (3-way merge
recognized it as "added only on master," kept it, camera rotation is untouched). Worker-reported
EditMode 113/113, PlayMode 34/34 on their branch; not independently re-run on master post-merge (Editor
still open/live at merge time) — self-reviewed `BoardSurfaceMaterials.cs`, the `GameBootstrap.cs` merge
result, and confirmed the `LogiCard.Board` asmdef reference survived instead.

**Doors are still tinted boxes** — checkpoint 3 (door/prop meshes) is blocked on a human pack choice
between two proposed CC0 candidates (documented in `Assets/_Project/Art/Environment/THIRD_PARTY.md`):
Quaternius "Ultimate House Interior Pack" (worker's recommendation — same author pipeline as the
existing pawn import tooling) vs. KayKit "Dungeon Remastered" (stronger modular wall kit, weaker genre
match — fantasy-dungeon read vs. this project's SWAT-facility brief).

**Second open question from the worker, also needs a human call, not a unilateral one:** should the
live in-match camera match the reference image's hero-shot fidelity/mood exactly, or a
readability-preserving version tuned back for actual play legibility? Worker built toward richer
wet-dusk detail without resolving this trade-off themselves — asked the human directly instead of
guessing, correctly per this project's escalation norms.

**`orthographicSize` still at 5.0**, untouched since the dock move — worker flagged it as needing a
framing check once the human sees a real screenshot, not retuned blind.

## 2026-08-10 (continued 2) — Honest progress check; ui-dock-polish merged; URP audit

**Human asked directly whether the project is actually on track aesthetically** ("i literally did not
see too much changes happened... I think the problem right now is with the rendering pipeline and
models"). Answered honestly rather than reassuringly. Two real findings:

1. **Env worktree checkpoint 2 had already landed real work I hadn't reported.** I'd previously told
   the human the env worktree had "zero new commits" — wrong, it had 2 (`6e01892`, `537633f`): Poly
   Haven CC0 PBR textures (asphalt/brick/concrete/wood, real photo-scanned material, not procedural)
   wired into board surfaces via a new `BoardSurfaceMaterials.cs`. Corrected the record with the human.
   **Not yet merged to master** — this is real progress the human hasn't been able to see yet, a
   process gap (should surface/merge checkpoints faster) not a "nothing is happening" problem.
2. **Render pipeline audit** (`Assets/_Project/Art/URP/LogiCardURP.asset` +
   `LogiCardURP_Renderer.asset`, both read this session): essentially stock/default, barely configured.
   `m_MSAA: 1` (off), `m_VolumeProfile: {fileID: 0}` (**no global post-processing volume exists at
   all** — no bloom, no color grading, no tonemapping, no vignette), `m_RendererFeatures: []` (no
   SSAO, no screen-space reflections), single shadow cascade, hard shadows only, only the main light
   casts shadows. **The human's hypothesis is partly right but for a different reason than expected:**
   URP itself can get much closer to the reference image, this is unconfigured setup work that hasn't
   been done, not a ceiling the pipeline imposes.

**Combined verdict given to the human:** two roughly-equal-weight gaps, both still open — (a) the
render pipeline has no post-processing/AO/reflections/soft-shadows configured, and (b) most on-screen
models (doors, characters, clouds) are still primitive placeholders; only board-surface textures have
gone real so far. Distance to goal is genuinely "not close yet," and that's an honest read of where
things stand, not a discouraging one — the two gaps are independently fixable and neither is blocked.

**`feat/ui-dock-polish` merged** (`d2624c2`, worker commit `a0d823b`). Real bug: `CanvasMatchWidthOrHeight
= 0.4` means the dock's UI-unit height shrinks on wide windows, and the old row-height stack (Verb 56 /
Stance 50 / Action 64, Pad 20, RowGap 12 ≈ 326 UI units) overflowed past the dock's actual budget at
2560×1080 (~306 UI units), clipping the stance row and SET PATH. Retuned to ~282 UI units with an
explicit `ControlsColumnContentHeight` vs. `DockHeightInUiUnits()` invariant an EditMode test now locks,
so this class of overflow can't silently regress again. Also fixed Adrenaline's disabled tint (was
`AccentDim`, read as a broken Lock In; now `PanelMid` + faded ink for gated/spent), widened Character
Select's grid/detail gap, and re-centered the Quit modal so it doesn't stretch into a band on ultrawide.
Worker reported EditMode 124/124, PlayMode 37/37 clean.

**Merge note — worker touched Integrator-owned docs despite the brief saying not to.** The worktree's
single commit was based on the stale `3d92e03` (before camera rotation and several doc updates landed
on master) and rewrote `DRAFT_HANDOFF.md`/`contracts/CURRENT.md`/`departments/INDEX.md` against that old
state. Git's 3-way merge correctly resolved all three files back to master's current content with zero
diff — nothing was lost — but flagging the pattern: a worktree that runs long without syncing will
increasingly diverge on anything it touches outside its lane, doc or code.

**Batchmode not independently re-run this merge** — Editor was open (live session) at merge time, same
recurring constraint this whole session. Self-reviewed the `ProgramHud.cs`/`AppFlowController.cs`/
`ModalDialog.cs`/`SelectionGrid.cs`/`UiStyle.cs` diff directly instead; changes are constants/layout-math
only, no logic restructuring, consistent with what the worker's report described. Still wants both a
batchmode confirmation and a human Editor look once the Editor is free.

**Next up:** merge env checkpoint 2 (Poly Haven materials) with care around the `BoardCameraRig.cs`
branch-divergence artifact (shows as deleted in their diff — fork-timing artifact, not intentional);
stand up a real post-processing Volume Profile + renderer features (bloom, color grade, SSAO, MSAA) as
a self-contained next slice; get the human's mesh-pack decision (Quaternius Ultimate House Interior vs.
KayKit Dungeon Remastered) unstuck; get the Chadderbox cloud pack or pick a fallback; get an actual
human/Editor look at current character models (never reviewed since the C53 pivot).

## 2026-08-10 (continued) — Compiler error fixed; smooth camera rotation; second worker spun

**Compiler error found and fixed without batchmode.** User reported "still compiler error" after the first
camera-rotation commit. Editor was locked (open on the main tree) so batchmode wasn't available — read
`Logs/Editor.log` directly instead (grepped for `error CS`) and found the real cause in seconds: the new
`BoardCameraRigTests.cs` (EditMode) used `LogiCard.Board` types, but `LogiCard.Tests.EditMode.asmdef` never
referenced that assembly — no prior EditMode test needed it. Added the reference, confirmed via the same log
grep that it was the only error (appeared 3x from repeated recompile attempts, single root cause). **Worth
remembering for next time an Editor is locked and something won't compile:** `Logs/Editor.log` in the project
root has the real compiler output, no batchmode or session interruption needed.

**Camera rotation rebuilt — smooth, not discrete.** Direct feedback: "camera rotation needs to be smoothly
rotated, not with button to rotate at a few fixed angle. User cannot rotate the camera to the bottom of the
map, it has to be on top of the map." `BoardCameraRig.Step(int)` (8 fixed 45° presets) replaced with
`RotateBy(float)` (any delta, no snapping), driven by right-mouse-drag (`Input.GetMouseButton(1)`, matching
the legacy Input Manager this project already uses elsewhere — not the new Input System package). Right
button specifically chosen to not collide with left-click board interactions. Pitch stays exactly fixed at
52° — this is the actual mechanism satisfying "cannot rotate to the bottom": since yaw only rotates around the
vertical axis, the camera's height above the board is a fixed function of pitch and distance regardless of
yaw, so it can never end up underneath. Added a test sweeping a full 360° rotation in 15° increments asserting
the camera's world-space height never changes, not just spot-checking a couple of angles. The old "ROTATE
VIEW" button is gone (right-drag replaces its function); a small non-interactive top-strip hint
("RIGHT-DRAG TO ROTATE VIEW") replaces it since the gesture isn't self-discoverable.

**Second worker spun via `/parallel-development`** (user explicitly asked: "make use of the other
worktrees"). `feat/ui-dock-polish` — a readability/polish pass on the new bottom-dock 3-column layout, the
Adrenaline primary-slot swap, Character Select grid, and Quit confirm dialog, none of which have had an actual
look since landing. No overlap with the active `feat/env-lookfeel-overhaul` worktree (Board/Art territory) or
the Integrator's own main-tree work (`GameBootstrap.cs`/`BoardCameraRig.cs`). Both worker slots now in use.

**Recurring pattern worth fixing at the source:** `image.png` has now been overwritten by a playtest
screenshot and restored from `screenshots/image.png` three separate times this session. Each time preserved
correctly (nothing lost), but flagging again in case there's a workflow change that would avoid it — e.g.
saving screenshots under any other filename before pasting.

## 2026-08-10 — Dock moved to bottom band; environment checkpoint 2 started

**Direct playtest feedback, two points:** (1) "cannot stand" the right-edge dock, wanted it at the bottom for
general vertical alignment; (2) "look-and-feel still bad, continue with the implementation to make it good."

**Dock moved right-edge → bottom band**, done directly on `master` (not delegated — small enough to execute
with confidence given deep familiarity with `ProgramHud.cs` from two prior review passes this session, and
the change was mechanically well-specified even though it required more than an anchor flip). Real content
re-flow, not just a coordinate change: `BuildProgramControls` now splits the dock into three columns
(controls/queue/actions) instead of one tall vertical stack; `BuildActionRow` rebuilt as a vertical stack
inside the narrow action column (was a horizontal row with fixed-width transport buttons); `BuildQueuePanel`
simplified now that it owns a full column; `BuildOutcomeBanner` repositioned above the dock instead of left of
it. `HudDockWidth` is gone, replaced by `HudDockHeight = 0.34f`. `GameBootstrap.cs`'s camera rect (the one
Integrator-owned coupling) rewired to match — board region is now full width, top-of-dock to bottom-of-strip.
Two tests that directly asserted the old right-edge geometry updated to match the new shape.

**Could not batchmode-verify before committing** — the Editor was open on the main tree (live playtest) both
times this was attempted. Self-reviewed the diff carefully in its place (signature matches, no orphaned
constant references, consistent structure) given the compiler wasn't available to catch mistakes immediately.
Should be re-verified once the Editor is free, though since it's already open live, a direct look is just as
good.

**Flagged, not resolved:** the board's visible aspect ratio changed meaningfully with this move (previously
narrower-than-tall since the right dock ate width; now wider-than-tall since the bottom dock eats height
instead, on an already-wide 16:9 screen). `orthographicSize` (5.0, last tuned against the old shape) likely
needs another pass — noted in the env worktree's brief as something that worker has permission to retune if
the framing looks off during checkpoint 2.

**Environment checkpoint 1 merged, checkpoint 2 started.** Checkpoint 1 (weather pocket + wet-dusk lighting)
was already reviewed and verified in an earlier session turn — merging it now made sense specifically because
the human is asking for visible progress; leaving it sitting unmerged achieves nothing. `feat/env-lookfeel-
overhaul` fast-forwarded onto current `master` (picks up the dock move + UI factory), then briefed to proceed
straight to checkpoint 2 (environment/prop asset pack sourcing, door models, character rework) — the original
brief's "stop and wait for a screenshot" gate on checkpoint 2 is explicitly superseded by "continue the
implementation." No hero-shot-vs-readability answer was given this round; proceeding on the inference that
"still bad" means more richness is wanted, not less — stated as an inference in the brief, not treated as
confirmed, and the question is still being surfaced for a real answer.

**Also fixed:** a new playtest screenshot had overwritten `image.png`, which `ART_DIRECTION.md` points to as
the locked moodboard reference — restored it from `screenshots/image.png` (kept in sync) and saved the
playtest screenshot separately at `screenshots/playtest-2026-08-10-allot-dock-complaint.png` so nothing was
lost.

## 2026-08-09 (continued) — UI worktree merged; environment worktree waiting on a human look

**`feat/ui-component-system` shipped and merged** (worker commit `7bd252f` → merge commit on `master`).
Reviewed in depth before merging, not just the report: spot-checked that the camera-rect coupling still
resolves correctly (`GameBootstrap.cs`'s `cam.rect` reads `ProgramHud.HudDockWidth`/`TopStripHeight`
symbolically, so widening the dock to 0.34 needed **no `GameBootstrap.cs` rewrite** — confirmed by reading the
line, not taken on trust), confirmed the "broken 5/4-column stance row" fix is real (Phase 1's original code
genuinely mixed `PlaceSplitCell(..., 4)` and `(..., 5)` calls in the same row — now a clean 3-way split + full-
width SET PATH), and confirmed `ModalDialog`/`SelectionGrid` are properly generic rather than one-off. Boundary
confirmed clean via the worker's own commit diff (`git show <sha> --stat`, not the branch-vs-master diff,
which included unrelated doc drift from `master` moving ahead while the branch was out): only
`Assets/_Project/UI/**` + its tests touched. Independently re-verified in the worker's own worktree before
merging: EditMode 113/113, PlayMode 34/34 — matched the report exactly. **Batchmode on `master` itself is
still pending** — the main tree's Editor was locked (live playtest session) when I tried; the merge was clean
with no conflicts and the pre-merge worktree verification already covers the actual code, so this is a
formality, not an open risk, but it should still be run once the Editor is free.

**`feat/env-lookfeel-overhaul` reviewed too, not merged — by design.** Checkpoint 1 (contained sky/cloud/rain
pocket above the board + a lighting/volume retune toward the reference's wet-dusk mood) is done, boundary-
clean (`cam.rect` untouched, confirmed via diff — everything else touched was within the explicitly granted
`ConfigureCamera`/`BuildLighting`/`BuildDioramaVolume` carve-out), and independently re-verified: EditMode
110/110, PlayMode 32/32. The clouds/rain are real geometry sized to the actual board bounds (not hardcoded),
implemented as tinted primitive spheres + a Shuriken particle system (URP has no orthographic-camera-
compatible volumetric cloud support, confirmed by the worker — this is a deliberate substitute, not a
shortcut). This branch **stays unmerged** — the worker's brief explicitly built in a stop-here checkpoint
(this project has already had one blind art attempt rejected on sight, `377029f`) and it's holding to that.
**Still needed from the human:** a Play + screenshot of this worktree, and an answer to whether the live
in-match camera should match the reference's hero-shot mood as closely as possible or stay a lighter,
readability-preserving version.

`docs/contracts/CURRENT.md` / `departments/INDEX.md` / `SCHEDULE.md` updated: UI's camera-rect contract closed
out, one worker slot open again, Phase 5's status line reflects both branches' real state.

## 2026-08-09 (continued) — Core gameplay paused; look-and-feel + UI wave kicked off

**Human call, read this before touching `Sim/`/`Net/`/`Timeline/`/anything gameplay:** a live playtest today
found real problems (button labels bleeding into each other — fixed; a badly mis-tuned camera showing large
black voids — recalibrated, still an estimate; general lighting/art quality not close to the bar this product
needs). The user's decision: **stop advancing core gameplay/networking (Phase 2 and beyond) and put full
focus on look-and-feel and UI** until both are in a good place. Phase 2's already-landed work stays merged,
just not extended further right now.

**Art direction broadened (`PRODUCT_MEMORY.md` C53).** The user supplied a locked visual reference
(`image.png`, repo root + `screenshots/image.png` — now kept in sync, they'd drifted apart) — a richly
detailed floating city-block diorama chunk (real architecture, cars, pedestrians, wet streets) on a
natural terrain-edge base, in a dark void, with a contained stormy sky + clouds + rain hovering directly above
the chunk (not an infinite horizon). This supersedes the "Digital Claymation / Link's Awakening toy-chibi"
framing (`VISION.md`, `ART_DIRECTION.md`, both updated with supersession notes per this project's established
convention — history kept, not deleted). Usefully, the board's existing structural shape (bounded chunk,
physical edge, dark void, solid-color camera clear) already matches the reference — this is a fidelity/detail/
weather upgrade, not a rebuild. Explicitly left OPEN: whether the *live in-match* camera should match the
reference's hero-shot fidelity exactly, or a readability-preserving version of it (`ART_DIRECTION.md` already
flags a standing tension between richness and tactical readability) — first checkpoint of the env worktree's
work resolves this.

**Two worktrees spun, both worker slots in use:**
- `feat/env-lookfeel-overhaul` (`logiCard-env-lookfeel`): sky/weather + lighting mood pass first (**hard
  checkpoint — waits for a human screenshot before continuing**, this project has already had one blind art
  attempt rejected on sight, `377029f`), then environment detail (likely needs sourced/imported CC0 assets —
  hand-rolled primitives can't hit this fidelity), door models, and a Scout/Juggernaut character rework (the
  current chibi Quaternius import was picked for the now-superseded toy look, likely needs replacing not
  polishing). Brief: `ENV_LOOKFEEL_AGENT_BRIEF.md` at the worktree root.
- `feat/ui-component-system` (`logiCard-ui-components`): extracts a shared UI factory (found real debt —
  `ProgramHud.cs`/`AppFlowController.cs` each duplicate their own `CreateButton`/`CreateText`/`CreatePanel`
  and have already started silently diverging), a real layout/readability pass (today's wrap/camera fixes were
  first aid, not a redesign), a new dialog/modal component, a generalized selection component, and the missing
  `UI_FLOW.md`-spec'd Adrenaline button. Brief: `UI_COMPONENT_SYSTEM_AGENT_BRIEF.md` at the worktree root.

One live coupling carried over from Phase 1: `GameBootstrap.ConfigureCamera()`'s camera viewport rect depends
on `ProgramHud`'s dock-width constants (UI worker's territory) while its `orthographicSize`/lighting live in
the same method (env worker's territory) — neither worker edits `GameBootstrap.cs` directly, Integrator wires
both at merge time. Frozen in `docs/contracts/CURRENT.md`.

`SCHEDULE.md`'s Phase 2 row marked **paused** (not abandoned); Phase 5 marked **active, top priority**. The UI
effort doesn't get its own phase number — tracked as a wave like any other, per the Cadence section.

Also fixed this session, uncommitted-verification caveat: the camera-zoom and button-wrap fixes from the live
playtest landed on `master` but **batchmode hasn't re-verified them yet** — the Editor was open on this exact
path during the live playtest, so batchmode would have collided. Verify once the Editor is free.

## 2026-08-09 (continued) — RelayMatchResolver wired into Find Match

`RelayMatchResolver` is no longer dormant — `Assets/_Project/Boot/GameBootstrap.cs` now subscribes to
`AppFlowController.EnteredMatch` (which gained a `bool viaRelay` param: true for Find Match, false for Local
Play) and calls `RoundPlayback`'s new `SetMatchResolver(...)` to pick `RelayMatchResolver()` (defaults to
`127.0.0.1:7777`, matching `Relay/`'s `RelayProtocol.DefaultPort`) or `LocalMatchResolver` accordingly.
`AppFlowController.BypassToMatch()` (the path every existing test/`SliceSceneFixture` uses) passes
`viaRelay: false`, so `LocalMatchResolver` stays the default everywhere nothing changed.

Real matchmaking/session assignment is still OPEN (`NETWORKING_DESIGN.md`) — Find Match still shows a short
stub "searching" beat rather than a real queue, and the actual network connection only happens naturally at
the match's first Lock In (that's structurally where `IMatchResolver.ResolveAsync` gets called). Added
failure handling there: a connection failure (e.g. no relay running, the realistic common case during
testing) now reports through the existing `OutcomeReported` HUD banner instead of throwing unhandled and
silently freezing the round.

**Manual two-Unity smoke test is now simpler** — no per-session `GameBootstrap` edit needed, both instances
default to the same port automatically:
1. `dotnet run --project Relay/LogiCard.Relay -- --port 7777 --board demo`
2. Open two Unity instances (Editor + Editor, or Editor + a build) on this same `master` checkout.
3. On each: Boot → Character Select → Lobby → **Find Match** (not Local Play).
4. Both Lock In one round → both should play back the identical tape. Relay exits after one resolve (restart
   for another round — session persistence is still OPEN).

Verified: EditMode 110/110, PlayMode 32/32 (`AppFlowPlayModeTests.FindMatchStubEntersMatchAfterDelay` already
exercises the `viaRelay: true` path through `GameBootstrap`'s new wiring end-to-end and passed clean). The
actual live two-process network round-trip through a real relay hasn't been run by a human yet — same
Editor-only gap as the rest of this session's pending items.

## 2026-08-09 (continued) — Phase 2 transport decision locked (C52); first slice in flight

**`C52` (`PRODUCT_MEMORY.md`): custom lightweight resolve-relay backend, server-authoritative host-integrity.**
Presented four bundled options (Fusion 2 server-hosted / NGO+Relay / custom resolve-relay / Steam P2P+
replay-audit) to the user with a recommendation and reasoning grounded in this project's actual shape — the
Program→Lock→Resolve→Playback loop is episodic, not continuous real-time state, so Fusion/NGO/Mirror's
tick-sync machinery is the wrong tool for this game, and `GhostResolver` is already pure engine-free C#
(verified zero `UnityEngine` references anywhere in `Sim/` or `Net/GhostResolver.cs`), making a small trusted
relay cheap to run. User confirmed the recommendation. `NETWORKING_DESIGN.md` and `RISKS.md` (R1/R6) updated
to reflect the lock; `NETWORKING_DESIGN.md`'s OPEN summary now shows items 1-2 (transport, host-integrity)
resolved, items 2b/3-6 (wire protocol, hosting target, ranked/casual, reconnect policy, cost estimate,
anti-cheat depth) still open and out of scope for the first slice.

**Traced the exact seam and landed it myself** (Boot/-owned, high-risk — RISKS.md's R1/R6 are the two
highest-scored risks in the project, didn't want to hand this in blind to a cold-start worker):
`RoundPlayback.ResolveAndArm()` (`Assets/_Project/Boot/RoundPlayback.cs:112-140`) built `GhostInput` from every
locally-registered pawn and resolved synchronously in-process — confirmed this is exactly the "same-process
dual-GhostInput stand-in" the doc already flagged. Introduced `IMatchResolver` (coroutine-shaped, matching this
project's existing coroutine idiom rather than `Task`-based async — nothing in the codebase used `Task`
anywhere, checked first) and `LocalMatchResolver` (wraps today's exact behavior, stays the default via `Init`'s
new optional `matchResolver` param). Hit and fixed a real gotcha along the way: a bare `yield return` on a
nested `IEnumerator` does **not** drain synchronously in Unity even when the inner enumerator never yields —
first attempt broke 8 `RoundPlaybackPlayModeTests` cases expecting synchronous `Tape` population; fixed by
manually pumping the inner enumerator (`while (resolve.MoveNext()) yield return resolve.Current;`), which
restored full synchronicity for the local case with zero test changes needed. Documented the gotcha in
`NETWORKING_DESIGN.md` so nobody rediscovers it. Verified: EditMode 108/108, PlayMode 32/32.

**Phase 2 first slice: shipped and merged (`47f4534`, `685f542` → merge commit on `master`, 2026-08-09).**
Worker built a minimal standalone relay process (`Relay/LogiCard.Relay`, net8.0 console app, sibling to
`Assets/`) pairing exactly two TCP connections and running `GhostResolver` once as authority, plus
`RelayMatchResolver.cs` — the client-side `IMatchResolver` implementation, background-thread socket I/O polled
from a coroutine so Unity's main thread never blocks. Wire protocol: raw TCP, 4-byte length-prefixed JSON
envelopes (`RelayProtocol.cs`) — chosen over WebSocket to skip handshake complexity for a first slice.

Reviewed in depth before merging, not just the report — this is the two highest-scored risks in the project
(RISKS.md R1/R6), worth the extra scrutiny:
- **`ActionNode.Modifier` is a Unity `ScriptableObject` (`CardData`)** — missed by my earlier "zero
  `UnityEngine` references" check, which only covered `Sim/` and `GhostResolver.cs` itself, not `ActionNode.cs`.
  Worker found it and added a non-Unity stub (`Relay/.../Shims/CardData.cs`) so the *real* `ActionNode.cs`
  compiles unmodified in the relay; the wire protocol always sends `Modifier: null`. **Verified myself**: every
  live call site that builds a real `ActionNode` (`PawnProgram.cs:336/415/453` — Move/Shoot/Door, the only
  three verbs actually wired to player input) already always passes `null` — gear cards are genuinely unbuilt
  (`C34`), so this is an honestly-flagged limitation, not a live bug.
- **The relay combines both clients' inputs in connection order, not pawn identity** — checked whether this
  could make match outcomes depend on who connects first. It can't: `GhostResolver.Resolve` (`GhostResolver.cs:99`,
  `order.Sort()`) explicitly re-sorts by `PawnId` before doing anything else, independent of input-list order.
- Boundary confirmed clean: only new files (`Relay/**`, `RelayMatchResolver.cs`, `RelayProtocol.cs`, tests) —
  no `GameBootstrap.cs`/`RoundPlayback.cs`/`IMatchResolver.cs`/`LocalMatchResolver.cs`/`GhostResolver.cs`/`Sim/**`
  touched.
- Tests are real, not rubber-stamped: the standalone integration test (`Relay/LogiCard.Relay.Tests`) asserts
  byte-level tape equality between two networked clients *and* against a local in-process resolve of the same
  inputs — an actual determinism proof, not just "didn't crash."

**Re-verified independently post-merge** (all three suites, not just trusting the worker's numbers): Unity
EditMode 110/110, PlayMode 32/32, standalone xUnit (`dotnet test Relay/LogiCard.Relay.sln`) 2/2.

**Deliberately left dormant:** `RelayMatchResolver` is landed but nothing in the live game picks it yet —
`LocalMatchResolver` stays the default everywhere, `AppFlowController`'s Find Match button is still just a
timer stub. Wiring the relay into that flow is a separate next step, not attempted here. `SCHEDULE.md`'s
Phase 2 row marked "in progress," not done — wire hosting/deploy target, ranked/casual split, reconnect policy,
matchmaking cost estimate, and anti-cheat audit depth are all still OPEN (`NETWORKING_DESIGN.md`'s OPEN
summary).

## 2026-08-09 — Board merge confirmed green; worktree cleanup; Phase 1 shipped and merged

**Board merge (`d81ffeb`) is now fully verified.** Ran EditMode + PlayMode batchmode directly on `master`
(Editor happened to be closed on this exact path) — **EditMode 107/107, PlayMode 29/29, no failures.** This
was the one explicit open item carried over from the 2026-08-08 save note. Closing it out.

**Worktree cleanup:** `logiCard-board-edge-dressing`, `logiCard-playmode-board-rewrite`, and
`logiCard-verify-board-merge` were fully de-registered from git (`git worktree remove` / `prune` — branches
`feat/board-edge-dressing` and `feat/playmode-board-rewrite` stay intact, both merged). `-verify-board-merge`'s
directory deleted cleanly; the other two directories on disk **would not delete** ("used by another process" —
OneDrive.Sync.Service was running; consistent with this project's known pattern of a background sync/index
process locking `Library`/worktree files, see the Baidu NetDisk note from an earlier session). Git no longer
tracks them as worktrees either way, so this is inert disk cruft, not a risk — safe to delete by hand later
(close OneDrive first) or leave. **Also found four older orphaned worktree directories on disk** — not in
`git worktree list`, so already fully de-registered from a past session's cleanup that hit the same lock issue:
`logiCard-pawn-art-step8`, `logiCard-pawn-docs`, `logiCard-pivot-gameplay-art-ui`, `logiCard-pivot-new-docs`.
Not touched this session (out of today's scope, not verified empty of anything valuable) — flagging for
whoever next has a free moment to inspect and clear them.

`docs/departments/INDEX.md`'s Capacity note was stale (still called the two board branches "queued but not
started" after they'd already merged) — corrected.

**Phase 1 (Landscape Desktop UI): shipped and merged (`771db57` → merge commit on `master`, 2026-08-09).**
Worker reworked `Assets/_Project/UI/ProgramHud.cs` off its old portrait/mobile scaffold (`referenceResolution
= (1080,1920)`, bottom "thumb zone") to `UI_FLOW.md`'s landscape layout: `1920x1080` canvas, board dominant,
new **right-edge** HUD dock (`HudDockWidth = 0.30f`) — the doc allowed side-or-bottom, worker chose side
since the existing vertical control stack fits a 16:9 side margin better than a short bottom band. Added
`AppFlowController.cs`: a functional (not polished — Phase 5's job) click-through Boot → Character Select →
Lobby → Waiting/Reveal → Round Result → Match End shell, wired so Lock In now plays a real Waiting→Reveal
beat instead of a flat 0.8s wait. `SliceSceneFixture` calls the new `Hud.BypassAppFlowForTests()` so every
pre-existing match test still starts straight in Program phase, undisturbed.

Reviewed against the brief before merging: boundary respected (only `Assets/_Project/UI/**` + tests touched,
no `Sim/`/`Net/`/`Board/*View.cs`/`GhostResolver`/`GameBootstrap.cs`/docs). One harmless nit found and not
worth sending back — `ProgramHud.LockInRoutine()`'s dead `_appFlow == null` branch double-calls
`SwitchPhase(RoundPhase.Reveal)`; `_appFlow` is always non-null after `Init()` so this never actually fires.

**Integrator wired the one deliberate coupling** the brief called out:
`GameBootstrap.ConfigureCamera()` (`Assets/_Project/Boot/GameBootstrap.cs:~298`) now reads
`cam.rect = new Rect(0f, 0f, 1f - ProgramHud.HudDockWidth, 1f - ProgramHud.TopStripHeight)` instead of the old
bottom-band formula. `ProgramHud.ThumbZoneHeight` stays as a compile-compat alias (`= HudDockHeight = 0`),
still asserted on by `ProgramHudLayoutTests`/`AppFlowPlayModeTests` — harmless dead weight now that the real
wiring is in, safe to delete in a later pass if anyone wants the tidy-up, not blocking anything.

**Post-merge batchmode on `master`: EditMode 108/108, PlayMode 32/32.** Contract closed out in
`docs/contracts/CURRENT.md`; `docs/SCHEDULE.md`'s Phase 1 row marked "mechanical bar met." **Still wants a
human Editor look** (dock overlap/readability at real window size, does the click-through flow feel right) —
same visual-confirmation gate this project has used for every prior presentation change; not done yet, not
blocking anything else.

Not started: Phase 2 (networking) — blocked on the user locking a transport choice + host-integrity approach
per `NETWORKING_DESIGN.md`, not picked unilaterally.

**Still pending, human-only, not blocking anything:** an Editor look at three things, none reviewed in-Editor
yet — the board-edge-dressing visual tuning (lip thickness, apron margin, clutter scale), the newly-committed
Scout/Juggernaut pawn art (see [`DRAFT_HANDOFF_ARCHIVE.md`](DRAFT_HANDOFF_ARCHIVE.md)'s 2026-08-08 entries for
detail on both), and now the new landscape HUD/app-flow shell from this session.

## Older history (pre-2026-08-09) — archived

Everything before the 2026-08-09 entries above (the full scope pivot narrative, C45 board rework, pawn-art
rework, Day 9–12 ship history, and the 2026-08-06 door/block fixes) was moved to
[`DRAFT_HANDOFF_ARCHIVE.md`](DRAFT_HANDOFF_ARCHIVE.md) on 2026-08-10 to keep this file's "read first" cost
down — it had grown to 680 lines. Nothing was deleted; git history and the archive file both preserve it
verbatim. `PRODUCT_MEMORY.md`'s binding `C46`–`C54` rows are the current authoritative summary of the
decisions made in that older material.
