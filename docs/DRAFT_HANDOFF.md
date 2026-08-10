# Draft Handoff — 2026-08-07

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
Scout/Juggernaut pawn art (see the 2026-08-08 entries below for detail on both), and now the new landscape
HUD/app-flow shell from this session.

## 2026-08-08 — Full scope pivot: 14-day portfolio demo → F2P PvP Steam ship (C46–C51)

**Read this section first — it supersedes the framing (not the facts) of everything below it.** The user
dropped the 14-day-sprint/portfolio-demo scope entirely. New target: a monetizing PvP game shipping to Steam —
free-to-play with cosmetic-only IAP (no pay-to-win), commercial-grade landscape-desktop UI and art bar, AI
used only as an invisible matchmaking-fallback bot (never a marketed mode), phase-based schedule instead of a
calendar. **The core gameplay loop is explicitly unchanged** — Move/Shoot/Door mechanics, Time Card/Time
Resource numbers, Scout/Juggernaut attributes, wound ladder, win condition, and the `C45` multi-room board
(see the section right below this one) all carry forward untouched. This is a scope/business reframe, not a
mechanics redesign.

Recorded as `PRODUCT_MEMORY.md` decisions `C46` (the pivot itself) through `C51` (real networking promoted to
core scope) — read those six rows first if you're picking this up fresh, they're the authoritative summary.
Four decisions were confirmed with the human before any doc got rewritten: F2P + cosmetic IAP, landscape
desktop redesign, AI as invisible fallback only, phase/milestone schedule with no fixed date.

**Committed to `master` (`cf4cd32`):** `PRODUCT_MEMORY.md` (`C46`–`C51` + amendment-pointer clauses on every
decision the pivot touches), `VISION.md` (new Business Model section, Non-Goals carve-out for the fallback
bot), `SCOPE.md` (full IN/OUT/LATER restructure), `SCHEDULE.md` (Day-1–14 calendar → 7-phase table with exit
criteria, old build history kept as a historical appendix, not deleted), `RISKS.md` (reframed/parked/closed
existing risks, six new risks — notably a **host-integrity gap**: under Fusion Host Mode a real player *is*
the host computing the authoritative resolve, a real cheating vector once PvP is real money-adjacent, not
solved yet).

**Key technical finding driving a lot of this:** the host-authoritative deterministic resolve model (Host
computes on plain float math, clients play back, Host revalidates payloads) is real and code-verified in
`Assets/_Project/Net/` — genuinely reusable. But "Photon Fusion" is currently a *label only* — no package
installed, zero transport/session/matchmaking code, today's `GhostResolver` runs both players' programs in the
same Unity process. Real networking is the single biggest gap between demo and shippable PvP product.

**DONE as of `4f403cf` — the full docs rewrite landed.** Both parallel workers reported back, were reviewed
against their briefs (clean, no boundary violations, correctly left OPEN numerics OPEN instead of inventing
them), and merged:
- `docs/pivot-new-design-docs` → merged `d74aab8`: `MONETIZATION.md`, `NETWORKING_DESIGN.md`,
  `AI_FALLBACK_BOT.md` (all new, D13–D15), plus `TDD.md`'s targeted retarget (§1 now points to
  `NETWORKING_DESIGN.md`, §2/§4's stale grid-era language fixed).
- `docs/pivot-gameplay-art-ui` → merged `afa5c0b`: `GDD.md`/`CORE_LOOP.md`/`TABLETOP_RULES.md` cross-ref
  fixes (including the pre-existing `C45` staleness bugs), `ART_DIRECTION.md` reframed to "commercial ship art
  bar," `UI_FLOW.md` rewritten portrait→landscape, `UI_BOARD_ANCHORED_COMPONENTS.md` light touch.

Then Integrator follow-up closed out everything both workers correctly flagged as outside their tight brief
scope rather than silently over-fixing: remaining "14-day"/"demo" framing in `GDD.md`/`CORE_LOOP.md`/
`ART_DIRECTION.md` (`858e66c`); `CHARACTER_ROSTER_LONGTERM.md` reframe + monetization guardrail,
`VERTICAL_SLICE.md` SHIPPED banner, `CAPTURE_CHECKLIST.md` + `SHIP_README_DRAFT.md` full rewrites (`b8f5591`);
`contracts/CURRENT.md` + `departments/INDEX.md` + all four `STATUS.md` reset for the next wave (`c6b3ee0`);
hygiene pass — deleted the two dead pointer stubs + `D10_Art_Direction.md`, banner-marked
`CONTINUOUS_PIVOT_PLAN.md` SHIPPED and the Day 4/Day 7 research notes historical (`4f403cf`).

A final grep swept the whole `docs/` corpus for stray "14-day"/"demo" framing — nothing left outside
intentional historical references or amendment-clause pointers.

**DONE (2026-08-08).** Both board workers reported back, were reviewed against their briefs (clean, in
boundary, matched their own reports), and merged:
- `feat/board-edge-dressing` → merged `4a9a992` (worker commit `325e83e`): physical board-edge lip (four cubes
  along `model` bounds) + void apron/workbench clutter dressing in `BoardView.cs`, all bounds-driven, no
  hardcoded board size. EditMode 107/107 in the worker's own worktree; PlayMode correctly deferred (was
  expected-red pending the sibling merge). **Still needs a human Editor look** — the worker flagged lip
  thickness (0.14/0.16), apron margin (2.75), and clutter scale as possibly too strong/faint at actual
  diorama camera distance (ortho 9.0 + Bokeh DoF) — not verified visually yet.
- `feat/playmode-board-rewrite` → merged `d81ffeb` (worker commit `07711b6`): mechanical coordinate retarget
  of the 3 PlayMode files to the `C45` board, plus disclosed `DisplayName == "Door #1"` hardening asserts in
  the door-selection tests (two doors now exist; substituted coords already resolved unambiguously, this just
  locks it in). EditMode 107/107, **PlayMode 29/29 — all three suites full green** in the worker's own
  worktree, verified against the real post-`C45` `GameBootstrap.cs`. **This clears the red PlayMode suite**
  that's existed since `C45` landed.

**Post-merge verification:** ran EditMode batchmode on the merged `master` (`d81ffeb`) in a disposable
worktree (`logiCard-verify-board-merge`) — **107/107 passed.** PlayMode was not re-run on the combined merge
before the session had to stop for a machine switch — risk is low (the two branches are file-disjoint,
`BoardView.cs` vs. 3 test files, and each already passed PlayMode individually pre-merge) but **whoever picks
this up next should run the full PlayMode suite on `master` once, to be sure**, before treating this as fully
closed. The disposable verify worktree (`D:\projects\Game\logiCard-verify-board-merge`) is still sitting on
this machine's disk if useful; otherwise safe to ignore/remove, it's not tracked by git.

**Also committed this session, previously-uncommitted pawn-art-rework work** (paused mid-flight when focus
shifted to the board/scope work, now captured so it isn't stranded across the machine switch): `PawnView.cs`
per-part team-color tinting, `Editor/PawnImportTool.cs`, and the full `Art/Characters/**` tree — Scout
(`Adventurer.fbx` + full material set) and **Juggernaut** (`Swat.fbx` + `Juggernaut.prefab` + materials) both
now have real generated prefabs/materials, not just the raw source Scout had before. Round 2 (Juggernaut) may
be further along than last known — **nobody has looked at it in-Editor yet**, that's the next pawn-art step
whenever that thread picks back up.

**Deliberately left uncommitted, not mine to touch:** `.claude/skills/parallel-development/SKILL.md` +
`.cursor/skills/parallel-development/` (a different concurrent session's edits, flagged as such since early
in this project's history), `ProjectSettings/ProjectSettings.asset` (recurring Sentis-analytics churn, never
shipped), `image.png` (unrelated pre-existing drift).

**What's next, in order:** (1) a human/fresh-session PlayMode run on `master` to confirm the board merge is
fully green, (2) a human Editor look at the board dressing's visual tuning, (3) whenever convenient, pick a
Phase 1/2/5 slice from `docs/SCHEDULE.md`'s phase table and start real implementation on the scope pivot —
still nothing but docs has landed for that yet.

**Once both land:** pick a Phase 1/2/5 slice from `docs/SCHEDULE.md`'s phase table and start real
implementation — the pivot itself was docs-only, no code changed yet. `contracts/CURRENT.md` and
`departments/INDEX.md` are reset and empty, ready to populate once that wave starts.

**Everything below this point (the `C45` board rework, the pawn-art rework) is still accurate and still real
work-in-progress — the pivot doesn't invalidate any of it, it just wasn't reframed as "the top priority" until
this note.**

---

**Board rework: multi-room layout in progress (2026-08-08), C45.** User redirected focus from the pawn rework
to "scene setting, map building, and lighting" — the board/AI/camera side is implemented in this main tree as
of this note; two parallel workers are in flight for the rest. Tilt-shift DoF (`GameBootstrap.BuildDioramaVolume`,
Bokeh mode) landed first and is done, not part of this item.

New board: `ArenaBoard(0,0,8,10)` — Yard (open, attacker spawn `(4,0)`) → Hall (walled kill-box, Door #1
frontal + Door #2 rear, defender spawns inside at `(4,6)`) → Vault (open), with unguarded flank corridors on
either side of Hall. Reverses the previously locked `[0,4]×[0,4]` footprint (`PRODUCT_MEMORY.md` `C39` item
7 / `C17`) via a new superseding decision, `C45` — see `PRODUCT_MEMORY.md` and `GDD.md` (both updated). New
`AmbushPoint = (4,3)`; scripted defender AI rewritten with the same relative choreography as before, just
recentered (`GameBootstrap.BuildDefenderPayload`). Camera `orthographicSize` scaled `3.6f → 9.0f`
(proportional estimate, **not yet eyeballed in the Editor — needs a human look** to confirm the whole board
frames without HUD cropping).

**Expected-red until the parallel workers land — not a regression:** `RoundPlaybackPlayModeTests.cs`,
`ProgramHudPlayModeTests.cs`, and `BoardInputPlayModeTests.cs` all hardcode positions from the *old* single-room
board and will fail against this new geometry until `feat/playmode-board-rewrite` (Worker B, briefed with the
frozen coordinate spec, building in parallel — see `PLAYMODE_BOARD_REWRITE_AGENT_BRIEF.md` in that worktree)
lands and merges. All EditMode tests are confirmed independent (synthetic boards, no `GameBootstrap`
dependency) and should stay green throughout.

**Also in flight, parallel, zero overlap:** `feat/board-edge-dressing` (Worker A) — board perimeter/void
dressing in `BoardView.cs`, addressing the Day 9 "not fully satisfied" visual note, developed against the
current small board (it reads bounds generically, so it carries over unmodified once this merges).

**Still needed before this is done:** human Editor smoke-test of a full round on the new board (defender
choreography lands the wound, flank corridors walkable and LoS-safe from Hall, camera frames the whole
board), then merge both workers, batchmode-verify in a disposable worktree, tick `SCHEDULE.md`.

**Pawn art rework: moved from planned to in progress (2026-08-08).** Step 1 (source & vet candidate packs)
is done: Kenney "Blocky Characters" was downloaded, previewed, and rejected (blocky, single rigid mesh, wrong
silhouette trap); Quaternius "Ultimate Modular Men" was downloaded, previewed, and selected as the geometry
base (modular parts give Scout/Juggernaut a real silhouette difference, though it's not a visual match for
the Link's Awakening target out of the box — that gap gets compensated downstream via shader/lighting work).
Full findings + licenses: `Assets/_Project/Art/Characters/THIRD_PARTY.md`. Plan status: `docs/PAWN_ART_REWORK_PLAN.md`.
The shader/import/`PawnView.cs` work this unblocks is still underway in the main worktree as of this note —
**not finished or verified, do not treat as done.**

**Pawn art rework in progress, read `docs/PAWN_ART_REWORK_PLAN.md` before touching `PawnView.cs` (2026-08-08):**
`377029f`'s primitive-assembled Scout/Juggernaut silhouettes were rejected by human review on sight (before
even being run) — cube parts + matte grain read as "default Unity primitives glued together," not handmade
toy art. Human confirmed the actual target: **The Legend of Zelda: Link's Awakening (2019 Switch remake)**
diorama/toy style — rounded, glossy, chibi — achieved by importing a free CC0 low-poly character pack
(Kenney "Blocky Characters" or Quaternius "Ultimate Modular Men Pack" are the researched candidates), not by
hand-tuning more primitives. This is a deliberate, approved exception to the project's usual
"everything procedural, nothing imported" convention, for character models only. Full implementation plan,
verification-loop protocol (no agent-side screenshot capture exists — human must Play + paste screenshots),
and doc-update checklist are in `docs/PAWN_ART_REWORK_PLAN.md`. **Not yet implemented** — pick up from step 1
(source & vet candidate packs) next session.

**Schedule:** M2.5 continuous pivot + Day 8 URP done; Phase 6 human gate cleared. **Day 9 is DONE and accepted** (2026-08-07). **Day 10 (clay motion + physical VFX) is fully wired and committed** (`a57d095`) — stepped playback, the muzzle-flash/wound-splat views, and their wiring into `RoundPlayback`'s tape-event loop are all on `master`; still needs a human Editor look before ticking SCHEDULE. **Day 11 audio is fully wired too** (`04f9191`, 2026-08-07) — `FoleyPlayer.Play()` now fires on Footstep/Shot (`RoundPlayback`) and Time Card/Lock In (`ProgramHud`); still needs a human ear-check before ticking SCHEDULE. **Day 14 ship case-study draft + capture checklist landed** (`950ff63`). **Multi-agent Parallel Ops is live** — see `docs/PARALLEL_OPS.md` + `docs/departments/INDEX.md` before starting any concurrent session. **Wave 3 (Days 12–14) plan is written up in `PARALLEL_OPS.md`'s "Wave 3 kickoff" section and `docs/DAY13_PLAYTEST_FINDINGS.md`** — read those before spawning agents for what's next.

**Day 12 Windows candidate — attempted, cancelled in favor of building on Windows directly (2026-08-07):** tried a batchmode `-buildTarget Win64` build from this Mac (Unity has no native Windows machine here, so this meant installing the Windows Build Support (Mono) module via Unity Hub CLI first, then building from a disposable worktree). Compiled clean, but the actual build ran 40+ minutes without finishing — likely a cold `Library/` cache in the fresh worktree stacked with a first-time platform switch, plus the project's unrelated Sentis package dependency (538 log mentions, not used by any gameplay code — same "Sentis analytics churn" already flagged on `ProjectSettings.asset` elsewhere in this doc) adding real overhead, plus dozens of failed license/telemetry network calls retrying. User (**has a real Windows machine**) called it and will build there directly instead — much simpler than fighting a cross-compile from Mac. Build stopped, disposable worktree removed, the one-off `BuildScript.cs` batchmode entry point deleted (unneeded once building natively). **If Day 12 comes up again on a Mac-only setup:** the Windows Build Support module *is* now installed on this machine's Unity (`6000.5.5f1`) if that helps, but consider first checking whether Sentis is actually needed in this project — it wasn't traced to any gameplay code and may be safe to remove, which would likely fix build time on its own.

**Concurrent-session note (2026-08-07, resolved):** while Core was mid-edit on `RoundPlayback.cs` for VFX wiring, a second Integrator-role session reviewed and merged the Audio stub (`ef6e3f5`) directly into this same `master` working tree, updating `DRAFT_HANDOFF.md`/`contracts/CURRENT.md`/`departments/INDEX.md` concurrently. No file overlap occurred (Audio touched only `Assets/_Project/Audio/**` + its own STATUS.md) and both sessions' doc edits were reconciled by hand. The human confirmed and cancelled that other session afterward — as of this note, this is the only session on `master`. Third instance of the same "two agents, one working tree" risk this doc has now logged; if you're a fresh session, confirm who else might be pointed at this exact path before editing, every time.

**Day 9 sign-off, read this before touching board/path art again:** human accepted the current board/path look **with reservations** — not fully satisfied with the board's visual quality, but explicit that schedule takes priority over further art polish right now ("if that is what we can do in this schedule I am fine with it," said about both the board and the path). This is a **conscious tradeoff, not an oversight** — don't restart art iteration on the board or path without the human raising it again. If Day 10+ work touches rendering and there's slack, revisiting board quality is fair game, but it is not blocking and was not asked for.

**Heads up — read this if you're a fresh session:** at least one other concurrent session (Cursor-based, working from a "Art UI Decisions" plan) was actively editing this exact `master` working tree today, at the same time as this session, more than once. Both collisions were caught and reconciled cleanly (see below), but don't assume you're the only one touching this repo. Check `git status`, `git log`, and **`docs/departments/INDEX.md`** before building on anything, and if a file changes under you mid-task, stop and reconcile before continuing. **Never two agents on the same working tree.**

## Parallel worktrees

**All worktrees removed as of this save (2026-08-07)** — every dept slice for this wave delivered and merged; nothing left in flight. Branches still exist locally (nothing force-deleted), just no working-tree checkout:

| Branch | Last commit | Status |
|--------|-------------|--------|
| `feat/day10-hit-vfx` | `f2256f6` | **Merged into `master` (`fc32a2d`) and wired (`a57d095`).** `MuzzleFlashView` + `WoundSplatView`, re-verified (EditMode 102/102, PlayMode 29/29), then wired into `RoundPlayback`'s tape-event loop. STATUS: `docs/departments/presentation/STATUS.md`. |
| `feat/day11-audio-stub` | `5c402db` | **Merged in two passes** (`ef6e3f5`, then `7e08aba`). **Wired** (`04f9191`) — `Play()` fires from `RoundPlayback` (Footstep/Shot) and `ProgramHud` (TimeCard/LockIn). STATUS: `docs/departments/audio/STATUS.md`. |
| `feat/ship-docs` | `fc58db3` | **Landed on `master` (`950ff63`)** — only the three files Ship owns, not the stale doc snapshot their commit also carried. STATUS: `docs/departments/ship/STATUS.md`. |
| `verify/playtest-door-scrub` | `54b051a` | Had uncommitted drift on top (door-sync logic that appears superseded by what's already on `master` — matched verbatim). **Stashed, not discarded**, before removing the worktree: `git stash list` from the main tree, recoverable via `git stash pop` if ever needed. Never committed here; nothing lost either way. |
| `logiCard-match-over-hud` / `logiCard-day9-yarn` | — | Already removed in an earlier session (merged or superseded). |

Ops constitution: `docs/PARALLEL_OPS.md`. Contracts this wave: `docs/contracts/CURRENT.md`.

## Implemented

**Committed on `master`, newest first:**

- `377029f` — **Day 10 gap fix: distinct Scout/Juggernaut pawn silhouettes** — `ART_DIRECTION.md`'s Demo art floor requires "Distinct clay-like pawn silhouettes (Scout vs Juggernaut readable)," but this was never actually built: it fell through the cracks when Day 10 was split into stepped-motion (Core) and VFX (Presentation) worktrees, and neither slice covered it — `DRAFT_HANDOFF`'s own "Still unfinished" list never flagged it either. `PawnView.Init` previously built the same plain capsule for every pawn, tinted only by color. Added a `PawnBuild` enum (`Scout`/`Juggernaut`) driving a per-archetype primitive assembly: Scout is a lean narrow capsule + small head; Juggernaut is a wide squat capsule + blocky head + shoulder pads — all still primitives/clay-tint material (ART_DIRECTION explicitly allows this: "physical feel via lighting, shaders, camera more than ultra-high-poly meshes"; bespoke modeled characters are optional, not floor). `GameBootstrap.BuildPawns` passes Scout for the attacker and Juggernaut for the defender, matching the `CharacterData` Scout/Juggernaut speed presets (1s vs 2s per tile) already hardcoded there — note `CharacterData`'s `Scout.asset`/`Juggernaut.asset` ScriptableObjects still aren't actually loaded/wired anywhere; `GameBootstrap` continues hardcoding the matching numbers instead (pre-existing, out of scope here). Verified in a disposable worktree (Editor was open on this path) — **EditMode 102/102, PlayMode 29/29**, no exceptions. Still needs the same human Editor look as the rest of Day 10's visuals (silhouette shape has no automated test coverage, same gap as flash/splat/motion).
- `04f9191` — **Wave 2: wire `FoleyPlayer` into `RoundPlayback`/`ProgramHud`** — `GameBootstrap` builds one `FoleyPlayer` and threads it into both `Init` calls as a new optional trailing `IFoleyPlayer` parameter. Call sites: `RoundPlayback.Report` (the same forward-only, once-per-crossing hook already driving the WOUNDED/DOWN banner — rewinding past an event doesn't replay its sound) — `MoveArrive` → `Play(Footstep)` once per completed move leg, `ShootFire` → `Play(Shot)` at the shot's completion instant; `ProgramHud.OnLockInPressed` → `Play(LockIn)`, `ConfirmTimeCard` → `Play(TimeCard)`. Also added an `AudioListener` to the scripted camera in `GameBootstrap.ConfigureCamera` — Unity only auto-adds one via the Editor's "GameObject > Camera" menu, which this project's fully-scripted camera never goes through; without it `Play()` would have produced no audible sound at all, silently, and the human ear-check would have had nothing to listen to. `LogiCard.UI.asmdef`/`LogiCard.Boot.asmdef` gained an explicit `LogiCard.Audio` reference (asmdef references don't propagate automatically between named assemblies). Verified: **EditMode 102/102, PlayMode 29/29**, no exceptions, no "no audio listener" warnings in the log.
- `7e08aba` — merge of a small Day 11 Audio follow-up (`feat/day11-audio-stub` @ `5c402db`): dropped an unused `using System;` from `FoleyPlayer.cs`, updated the dept's own STATUS.md with their own batchmode confirmation (102/102 EditMode). No functional change.
- `950ff63` — **Land Day 14 ship case-study draft + capture checklist** — pulled `docs/SHIP_README_DRAFT.md`, `docs/CAPTURE_CHECKLIST.md`, `docs/departments/ship/STATUS.md` directly from `feat/ship-docs` @ `fc58db3` rather than merging the branch, since that commit also carried a stale pre-Day-10 snapshot of `PARALLEL_OPS.md`/`contracts/CURRENT.md`/`departments/INDEX.md` from when their worktree forked at `a5c276a` — a plain merge would have tried to check those out over today's current uncommitted versions of the same paths. README expanded into a full case study (hook, constraint, core loop, readable-combat bar, presentation bets, post-demo next steps); checklist got a timed 60–90s shot list with operator notes and fail criteria. Both still marked DRAFT pending human capture + Windows candidate. Docs-only, no gameplay code touched.
- `a57d095` — **Wire `MuzzleFlashView`/`WoundSplatView` into `RoundPlayback`** — same build-once-at-arm / per-scrub-visibility pattern as the existing `ShotTracerView` tape loop (`BuildTracers`/`UpdateTracers`). `BuildHitVfx` spawns a flash per `ShootFire` event and a splat per `Wounded`/`Killed` event when the tape arms; `UpdateHitVfx` toggles visibility each `ApplyTime` call; `ClearHitVfx` tears both down on `Disarm`. Flash: shooter's position at the shot's *completion* instant (not the tracer's aim-in/hold window start — the flash is the muzzle igniting, not the beam), lit for a short fixed Time Resource window (`MuzzleFlashVisibleSeconds = 0.15f`; ART_DIRECTION §3's "2 frames, then gone" translated to TR-seconds since RoundPlayback never deals in real/engine frames). Splat: victim's position (`TapeEvent.Position` for a hit), persistent once the scrubber has passed it, hidden again on rewind — same rule the tracer/banner logic already uses. No `GameBootstrap` changes needed — like `ShotTracerView`, both views are created dynamically inside `RoundPlayback`, never spawned from the bootstrap.
- `ef6e3f5` — **Merge Day 11 Audio Wave 1**: `IFoleyPlayer`/`FoleyId` (from `feat/day11-audio-stub` @ `764a42e`), verbatim to the frozen contract in `contracts/CURRENT.md`; `FoleyPlayer` (`MonoBehaviour`) lazily synthesizes and caches one runtime `AudioClip` per `FoleyId` (tone/noise/envelope recipe distinct per Footstep/Shot/TimeCard/LockIn) and plays via `AudioSource.PlayOneShot` — no binary clip assets, no Boot/UI/Board/Sim references (grep-confirmed), no auto-play. New assembly `LogiCard.Audio` (`autoReferenced: true`, no extra references). Reviewed + batchmode-verified before merge (see Verification below); only new files under `Assets/_Project/Audio/**` plus the dept's own `STATUS.md` — nothing else touched. Dead code — Core wires `Play()` calls into `RoundPlayback`/`ProgramHud` in Wave 2.
- `fc32a2d` — **Merge Day 10 Presentation**: `MuzzleFlashView.cs` + `WoundSplatView.cs` (from `feat/day10-hit-vfx` @ `f2256f6`), matching the frozen `Init`/`Place(...)`/`SetVisible(bool)` contract in `contracts/CURRENT.md`. Jagged 7-shard yellow/orange resin burst for the muzzle flash (no particles/bloom, colliders stripped, starts hidden); wet dark-red 4-lobe clay blob for the wound splat (bumped smoothness for "wet," persistent once shown). Reviewed against contract + boundary before merging — only the two new files + `.meta`, nothing else touched. Standalone; not wired into `RoundPlayback`/`GameBootstrap` yet.
- `d60f01d` — **Day 10 Core: stepped 8–12fps playback on `PawnView`** (ART_DIRECTION §2) — `Assets/_Project/Board/PawnView.cs` holds its rendered pose ~1/10 real second once a path is already armed, instead of updating every engine frame; a fresh `SetPath` and path start/end still always snap exactly (draft preview and key poses stay precise). Self-contained — `RoundPlayback.cs`/`GameBootstrap.cs` untouched, reserved for VFX wiring next. Two PlayMode tests encoded the old "exact position at any scrub instant" contract and needed updating to match the new deliberate behavior (see Verification below), not a workaround — they still assert exact positions, just after the hold legitimately clears.
- *(uncommitted on main working tree)* **Parallel Ops system** — `docs/PARALLEL_OPS.md`, `docs/departments/**`, `docs/contracts/CURRENT.md`, `docs/SHIP_README_DRAFT.md`, `docs/CAPTURE_CHECKLIST.md`; pointers in `CLAUDE.md` + parallel-development skill. Audio + Ship worktrees spun. Commit when asked.
- `a5c276a` — Save draft: Day 9 accepted, Day 10 split in progress.
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

- Foley wiring (`04f9191`): disposable worktree, created and removed same session — **EditMode 102/102**, **PlayMode 29/29**, no exceptions, no "no audio listener" warnings (confirming `Play()` actually fires during the Lock In / Time Card / Move / Shoot paths the PlayMode suite already exercises, not just that it compiles). No automated test asserts on *which* clip played or its content — same gap as the VFX visuals; still needs a human ear-check.
- Ship docs (`950ff63`): docs-only, no batchmode run needed (no code touched).
- VFX wiring (`a57d095`): re-verified against `master` at `ef6e3f5` (which already carries both the VFX merge and the Audio merge) in a disposable worktree, created and removed same session — **EditMode 102/102**, **PlayMode 29/29**, no exceptions. No existing test touches `MuzzleFlashView`/`WoundSplatView` counts directly, so this confirms no regression, not new coverage of the flash/splat behavior itself — still needs a human Editor look.
- Audio Wave 1 stub (`ef6e3f5`): batchmode EditMode run directly on the `logiCard-day11-audio` worktree (separate project path, so the main Editor staying open didn't block it) — **EditMode 118/118**, **0 compile errors**. Merge itself was clean (`--no-ff`, no conflicts — new-files-only slice). The department's own batchmode run wasn't possible in their session (no Unity install there); this was Integrator's pre-merge check.
- Day 10 merge (`fc32a2d`): re-verified with stepped motion + the two VFX views together in a disposable worktree (`logiCard-verify-day10-merge`, created and removed same session) before merging — **EditMode 102/102**, **PlayMode 29/29**, no failures, no exceptions. Merge itself was clean (`--no-ff`, no conflicts — the VFX branch only adds two brand-new files).
- Stepped playback (`PawnView.cs`, `d60f01d`): disposable worktree (`logiCard-verify-day10`, created and removed same session, copied the 4 changed files in since worktrees only see committed history) — **EditMode 102/102**, **PlayMode 29/29**, no exceptions in the PlayMode log.
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

1. ~~**Day 10, main's half:** stepped 8–12fps playback motion (`PawnView.cs`).~~ **Done, committed** (`d60f01d`, 2026-08-07).
2. ~~**Day 10, parallel half:** VFX report-back, verify, merge, wire.~~ **Done** — merged `fc32a2d`, wired `a57d095` (2026-08-07). ~~**Day 10 gap:** pawn silhouettes were never actually distinct (plain capsule × 2).~~ **Fixed** `377029f` (2026-08-08) — see Implemented above. **Remaining:** human Editor look before ticking Day 10 on SCHEDULE (silhouette shape, flash/splat behavior, and stepped motion all have no automated test coverage — see Verification and `DAY13_PLAYTEST_FINDINGS.md`, which now includes a silhouette check).
3. ~~**Day 11 audio stub:** report-back, review, merge, wire.~~ **Done** — merged `ef6e3f5`/`7e08aba`, wired `04f9191` (2026-08-07). **Remaining:** human ear-check before ticking Day 11 on SCHEDULE.
4. ~~**Ship docs:** drafts seeded on `feat/ship-docs`.~~ **Done, landed** (`950ff63`, 2026-08-07). Still DRAFT pending human capture + Windows candidate (expected — not a blocker for anything else).
5. ~~Optional cleanup: remove parked/delivered worktrees.~~ **Done** (2026-08-07) — all four removed; branches intact. `logiCard-verify-playtest`'s uncommitted drift stashed first, not discarded (see Parallel worktrees table).
6. **Day 12 Windows candidate:** attempted from this Mac, cancelled — user is building natively on their own Windows machine instead. See the note near the top of this doc for why the Mac attempt was slow.
7. Optional, not requested: board visual polish — see Day 9 sign-off before touching unprompted.

## Known issues (deferred, cosmetic — not a gate)

- Pawn model visually pokes through wall/closed-door geometry when its logical position sits at/near the wall plane. Cause: the sim tracks pawns as a point with no collider (deliberate — no Physics/Physics2D anywhere in resolve). Doesn't affect hit resolution, pathing, or LoS. User call: defer to a later pawn-model/art pass.

## Tomorrow / next agent — Wave 3 kickoff

**Read `docs/PARALLEL_OPS.md`'s "Wave 3 kickoff" section (bottom of file) before spawning anything** — it's the
step-by-step for Days 12–14 and the full reasoning, this is just the short version:

1. Read `docs/departments/INDEX.md` + `docs/contracts/CURRENT.md`. Stepped motion, VFX (wired), Audio (wired), and Ship docs are all on `master` (`842ce27` is the tip as of this save). No Core-side implementation queued.
2. **Gate: human fills in `docs/DAY13_PLAYTEST_FINDINGS.md`** (Editor look + ear-check, repro steps included in that file) before anything else happens. Nothing is safe to hand a fresh worker off a vague verbal note — this project has already eaten real time from that kind of shortcut.
3. Integrator triages each written finding per that file's key (ship-as-is / quick fix / `/parallel-development` a real fix / defer), then ticks Day 10 + Day 11 on `SCHEDULE.md`.
4. Windows candidate (Day 12) is happening natively on the user's own Windows machine — **not an agent task**. Once it exists, tick Day 12 and note the build location here.
5. Day 13 is the same findings-file loop, framed as the "presentation playtest" `SCHEDULE.md`'s cadence rule asks for.
6. Day 14: once the Windows build + capture footage/screenshots exist, spin a fresh Ship worktree to embed them into `SHIP_README_DRAFT.md` and promote it to root `README.md`.
7. Don't reopen tracer / radius tuning unless a new playtest finding says so.
8. Don't restart board/path art polish unprompted.

## 2026-08-06 seventh pass — same-round door open+cross, and the GDD-mandated block re-check that was never built

Traced "I opened the door but still can't get through" to a real design gap, cross-checked against the docs (not guessed): `GDD.md`:59 / `CORE_LOOP.md`:90 already specify "blocked path / closed door → stop before the block," but the continuous-space retarget never implemented the resolve-time half of that. Two coordinated fixes, both with new EditMode coverage:

1. **`PawnProgram` draft-time self-consistency** — a pawn's own already-committed Door nodes this round are now applied to a local clone of the board (`BuildLocalBoard()`, new) before `TryDraftPath`/`TryAddWaypoint` run pathfinding, so "walk near the door → open it → walk through" is plannable in one round. Deliberately still fully blind to the *opponent's* plan — only this pawn's own committed actions count. New tests: `DoorTests.TryDraftPath_WithoutOwnQueuedDoorOpen_DetoursAroundTheStillClosedDoor` / `TryDraftPath_AfterOwnQueuedDoorOpen_CrossesTheGapInOneRound`.
2. **`GhostResolver` "stop before the block"** — movement used to be pre-baked per-pawn with zero re-validation against the board's actual evolving door state, so a pawn could visibly glide through a door that turned out closed by the time they got there (contention with the opponent, or any other mismatch between the draft-time snapshot and reality). `Resolve()` now extracts every pawn's Door toggles up front (`BuildDoorTransitions`, independent of and not touching the existing, already-tested `ApplyDoorGroup`/`ResolveShots` shot-and-event sweep, to keep this change's blast radius small), and `CompileTrack` checks each Move leg against it (`TryFindEarliestDoorBlock`, using new `Segment.TryIntersectionParams`) — a leg that crosses a door that's actually Closed at that instant gets truncated there, and everything still queued after it for that pawn is dropped (mirrors the existing "death freezes remaining queue" precedent, C37, applied to "blocked" instead of "dead"). New tests: `DoorTests.Move_CrossingADoorThatClosesBeforehand_StopsAtTheDoor` / `Move_CrossingADoorThatOpensBeforehand_CompletesNormally` / `Move_BlockedByADoor_CancelsThatPawnsLaterQueuedActions`.

Contention itself (who opens/closes a shared door and when) needed no new design — `ApplyDoorGroup`'s existing chronological-order + simultaneity-epsilon + "Close wins" tie-break already governs it, same mechanism combat already uses. Confirmed no regression risk by inspection: `GhostResolverTests.cs`'s shared `NewBoard()` registers zero doors (so `TryFindEarliestDoorBlock` is a no-op there), and every existing Move-vs-door scenario in `RoundPlaybackPlayModeTests`/`GameBootstrap`'s scripted defender stays on one side of the wall line rather than crossing it — but this has **not been run through an actual test pass yet**, only reasoned through by reading every affected call site. Verify before trusting.

## 2026-08-06 eighth pass — playtest #1 (crossing) confirmed working; found why state looked stuck

User confirmed the persistence/crossing/block-at-door fixes above work correctly. Two remaining symptoms — door tint never changing, the board-anchored prompt always reading "OPEN" — turned out to be **one bug, and not in any of this session's new code**:

- **Root cause: `GameBootstrap.BuildDefenderPayload`'s scripted defender queued `Door(Open)` unconditionally, every single round**, regardless of the door's actual live state. This was harmless before door state persisted across rounds at all (nothing carried over for it to undo) — but once persistence started working (this session, pass 2), the defender silently re-opened the door near the end of *every* round's timeline, undoing any Close the player had just booked. Both the tint and the prompt were reading the real, correctly-updated state the whole time — the state itself just kept getting stomped back to Open by the AI a few seconds after the player closed it.
- **Fix:** `TryScriptDoor` now checks `_board.Model.GetDoorState(door)` first and no-ops if it already matches the action's implied state — the defender only opens it when it's actually closed. Round 1 is unaffected (door always starts Closed, so the defender's Open still fires exactly as before, preserving `RoundPlaybackPlayModeTests`' AmbushPoint scenario, which needs that open for its Snap Shot's LoS).

Not yet re-verified by the user or a test run — should be a quick recheck given the fix is a one-line conditional in already-scripted AI, not new resolve logic.

## Blockers / notes

- Unity **6000.5.5f1**; main project `/Users/xuxinye/Documents/projects/Game/LogiCard`. Editor is typically open there (user playtests live) — batchmode needs a different worktree path. Spin up a disposable one (`git worktree add`) and remove it after.
- If you edit a `.asset`/pipeline file on disk while the Editor has the project open (as this session did for the URP Renderer asset), Unity should pick it up via its file watcher on focus regain — if visuals don't update, try `Assets > Refresh` in the Editor.
- Do **not** pass `-quit` with `-runTests` (exits before the suite runs); use `-acceptSoftwareTermsForThisRunOnly`.
- Hub "Add project": select parent `…/Game`, not `LogiCard` itself.
- `master` tracks `origin/master`; everything above is local-only, not pushed.
- Two separate concurrent-session collisions happened this session (a duplicate Day 9 implementation, and a live doc-editing overlap during the path-art pivot). Both were caught by checking `git status`/`git log` before committing and reconciled without losing work, but it cost real time. If you're picking this up next: check for concurrent activity *before* you start building, not after.
