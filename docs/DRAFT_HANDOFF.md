# Draft Handoff — 2026-08-07

## 2026-08-10 (continued 14) — three-map roster landed and wired; both worker slots closed

**Both new maps merged and the shared dispatch wired** — the three-map roster from "continued 13" below
is now complete and selectable (not just present as dead code):

- `feat/map-vault-complex` merged clean (`6c384d2`); a missing `MapDefinitions.cs.meta` (my own
  oversight when that file was first created, caught by the Vault Complex worker as an aside) fixed
  separately (`d291db8`).
- `feat/map-rail-platform` merged with real textual conflicts against the already-merged Vault Complex
  branch (`59b9a4e`) — both branches added new methods immediately after the same insertion points in
  `GameBootstrap.cs` and `MapDefinitions.cs`. Resolved by keeping every method from both branches intact
  (no logical conflict, just proximity); same commit also carries the dispatch wiring below.
- **`GameBootstrap.BuildBoard(MapId)`'s switch, `MapDefinitions.ForId`'s switch, and `BuildPawns()`** —
  the shared dispatch points deliberately reserved for the Integrator, per-map-collision-avoidance —
  now route all three `MapId` values to their real geometry/layout/spawn points instead of throwing
  `NotImplementedException`. `BuildPawns()` picks attacker/defender spawn points and the correct scripted
  defender-payload method per map (Rail Platform: attacker `(4,0)`, defender `(4,11)` →
  `BuildRailPlatformDefenderPayload`; Vault Complex: attacker `(2,0)`, defender `(6,8)` →
  `BuildVaultComplexDefenderPayload`) — numerics chosen to match each map's own defender-AI approach
  vectors, same offset-from-door pattern Freight Yard already used.
- `ActiveMap` stays a constant, still defaulted to `FreightYard` — no map-select UI exists, explicit
  follow-up per the approved plan, not attempted here.
- Recorded as `PRODUCT_MEMORY.md` **C57**.
- Both map-worker slots now closed (`docs/departments/INDEX.md`: 0 of 2 in use). The two worktrees
  (`logiCard-env-lookfeel`, `logiCard-ui-dock-polish`) still exist on disk with stray untracked brief
  files/`TestResults/` from each worker's session — pending cleanup, not yet removed.
- Batchmode verification of the fully-wired combined state via a disposable `logiCard-verify-maps`
  worktree at merge commit `59b9a4e`: **EditMode 124/124, PlayMode 37/37, all green.** (First attempt's
  `-runTests` was combined with an explicit `-quit` flag and exited 0 with no results file at all —
  `-quit` races Unity's own post-test-run quit and can cut off the test runner before it starts; dropped
  `-quit` and reran clean. Worth remembering for next time this pattern comes up.) Worktree removed after.
  Both now-finished map-worker worktrees (`logiCard-env-lookfeel`, `logiCard-ui-dock-polish`) deregistered
  from `git worktree` cleanly; their on-disk directories are down to empty shells but wouldn't `rmdir` —
  "Device or resource busy," likely OneDrive sync or Windows Search Indexer holding a transient handle
  (same class of issue as the documented Baidu NetDisk/`Library/Bee` lock, different process). Not a git
  or project-state problem — harmless empty leftovers, safe to delete by hand whenever the lock clears.

**Cloud fix correction** — the "continued 13" entry below and the "Awaiting human review" section
originally pointed at `af5d2b1` as the cloud-transparency fix. That commit used the wrong shader keyword
(`_ALPHABLEND_ON`, which doesn't exist on `Universal Render Pipeline/Particles/Unlit`) — caught by a
follow-up human screenshot ("the cloud and weather definitely have not changed") showing clouds still
rendering as broken jagged black shapes. Corrected in `7944b61`, this time verified directly against the
shader source in `Library/PackageCache` before committing (`_SURFACE_TYPE_TRANSPARENT`, same keyword the
glass fix uses). Batchmode-verified (EditMode 124/124, PlayMode 37/37) but — like everything in the
"Awaiting human review" section below — not yet visually confirmed. Item 2 in that section now reflects
this correction.

## 2026-08-10 (continued 13) — three-map roster kicked off: Vent/Breach primitives, Freight Yard retrofit, two new maps in flight

**Human decision, recorded in full in `PRODUCT_MEMORY.md` (next C-row once landed):** logiCard grows
from one hardcoded map to a roster of three. "Complexity" clarified explicitly — not a difficulty ramp
between maps, but each map having genuine interactive terrain (vents and similar) both sides can use.
Design **and** implement now (lifts the core-gameplay-paused rule specifically for map/terrain work,
not networking/other Sim work); maps stay hand-authored, no shared data-driven format.

**Two new `Door` kinds, zero new Sim types** (`f95b19a`): `DoorKind.Vent` (repeatable narrow shortcut,
same interact/resolve/UI pipeline as any door, just a distinct base tint) and `DoorKind.Breach`
(one-way permanent shortcut — starts Closed, UI never offers Close again once Open). Both reuse
`PawnProgram`/`GhostResolver`/`RoundPlayback`/`ProgramHud` entirely unchanged.

**`MapId`/`MapLayout` groundwork** (`8038748`): new `MapDefinitions.cs` centralizes room-rectangle data
that was previously triplicated (and already flagged as a manual-sync risk) across `GameBootstrap`'s
walls, `BoardView.PlaceRoomFloors`, and `BoardReflectionProbes`' own constants. `GameBootstrap.BuildBoard`
now dispatches on `MapId` (still defaults to `FreightYard`, no map-select UI yet — explicit follow-up,
not attempted). As a side effect, the two flank corridors now get their own reflection probe too (never
had one before — a real improvement, not just a refactor).

**Freight Yard retrofitted** (`3d1c0e2`): one Vent (west wall, bypasses the frontal Door #1 chokepoint)
and one Breach (east wall, permanent flank route once paid for). Checked carefully that both stay well
clear of every position the scripted defender AI or existing tests use to resolve "the nearest door" —
confirmed via real batchmode (not just reasoning) that `RoundPlaybackPlayModeTests` and the door-
disambiguation `ProgramHudPlayModeTests` all still pass: EditMode 124/124, PlayMode 37/37.

**Two new maps now building in parallel** (`feat/map-rail-platform`, `feat/map-vault-complex`): Rail
Platform (long sightlines, two platforms + a corridor, rewards Hold Angle/Juggernaut) and Vault Complex
(dense maze of small rooms, rewards Snap Shot/Scout agility). Both workers deliver self-contained new
methods only (`BuildXxxGeometry()`, `MapDefinitions.Xxx()`, `BuildXxxDefenderPayload()`) — explicitly
told not to touch the shared `MapId`/dispatch switches, which the Integrator wires once both land, to
avoid a two-worker collision on the same dispatch point.

**Nothing about the new maps is visually confirmed** — same standing caveat as everything else in this
session's presentation/level work, now doubly true since this is geometry nobody (human or agent) has
ever seen rendered. Batchmode passing confirms compile + no regression, not "plays right."

## ⚠️ Awaiting human review — parked, not forgotten

Five visual changes landed during the 2026-08-10 autonomous push and have **never been confirmed by
anyone with eyes on the running game** — code is correct by direct inspection of serialized state,
batchmode is green, but "runs correctly" and "looks right" are different claims, and this session has
already had three cases where they diverged. Per the human's explicit instruction, these stay
**unmonitored** until they come back to review — do not assume any of them are visually done, do not
build further on top of the assumption that they look right, and do not re-touch them speculatively.

1. **Reflection probes** (`80049df`) — does the wet floor (Yard/Vault) show a visible sheen/highlight
   now, or does the `clearFlags` fix still read as unchanged?
2. **Clouds** (`be119b8`, corrected transparency fix `7944b61` — supersedes the first, wrong attempt
   `af5d2b1`) — do they read as soft cloud puffs, or is there a leftover artifact (hard edges, seams
   between billboards, wrong tint)?
3. **Window glass** (`5720d31`) — is it actually see-through, and does the warm glow behind it
   (Hall/Vault windows) read as a lit-window effect, or is it too subtle/too strong/off?
4. **Scout's re-outfit** (`d5ee45e`, Adventurer→Worker) — does the vest/high-vis read as "facility
   worker," or does it look odd in a way code review couldn't catch?
5. **The whole checkpoint 2/3 arc** — real board materials, real door meshes, room dressing — never
   confirmed as "looks right" by anyone this entire arc, only ever seen through screenshots.

**Reminder mechanism:** this section stays at the top of this file until the human explicitly clears
it. `docs/SCHEDULE.md`'s Phase 5 row points here too.

## 2026-08-10 (continued 12) — decision: not pushing character material fidelity blind

**Considered and deliberately declined**, logging the reasoning per the human's "notify me of decisions"
instruction rather than just silently skipping it. The character research pass (earlier today) left the
flat-material/low-poly fidelity gap explicitly open, framed as something to close "if the human wants to"
— not an auto-continue item the way C56's outfit swap was. Two real considerations against pushing it now:

1. **The source pack ships no textures at all** (confirmed in the research findings) — a real fidelity
   improvement isn't a wiring fix, it's either sourcing new assets (Mixamo or a base-character pack,
   options B/C — both explicitly flagged as needing a human art-direction call, not just an asset swap)
   or inventing new procedural detail from scratch, either way a real design decision.
2. **This session has already shipped two visually-broken changes that passed every batchmode check**
   (clouds rendering as solid black rectangles, window glass staying opaque) — both looked fine by every
   automated signal and were both real, embarrassing misses caught only by an actual human look. Tweaking
   pawn materials blind, with zero visual feedback available for the next two hours, risks a third instance
   of exactly that failure mode — and unlike the reflection/cloud/glass fixes (which had objective,
   inspectable ground truth: a serialized property either says Opaque or Transparent), material "fidelity"
   is a subjective aesthetic judgment with no code-level ground truth to verify against.

**Decision: hold this open, don't guess.** Redirecting the remaining autonomous window to verifiable work
(bug-hunting via code/asset inspection, doc/schedule hygiene) instead of speculative visual tuning that
can't be checked until a human looks at it.

## 2026-08-10 (continued 11) — orthographicSize resolved analytically, no change needed

**Standing open item since checkpoint 3** (`orthographicSize = 5.0`, last human-verified 2026-08-09
against the pre-door-mesh board): worked this out by projection math instead of waiting for a screenshot,
since none is available for 2 hours.

**Math:** `BoardCameraRig`'s orthographic camera has fixed pitch `52°` (`Quaternion.Euler(52,0,0)`), so its
local "up" vector is `(0, cos52°, sin52°) ≈ (0, 0.616, 0.788)` and "right" stays exactly `(1,0,0)` (pure
X-axis rotation never touches the right vector). The board (`ArenaBoard(0,0,8,10)`) lies flat in the world
XZ plane. For an orthographic camera, a world displacement's on-screen extent is its dot product with the
camera's right/up axes — no perspective falloff, so `DistanceFromCenter` (14 units) doesn't factor in at
all, only `orthographicSize` and the viewport's aspect ratio do.

- **Vertical (board depth, 10 units along world Z → camera's "up"):** `10 × 0.788 ≈ 7.88` world units of
  screen-vertical extent, against a visible vertical range of `2 × orthographicSize = 10`. That's **≈79%
  vertical coverage** — matching the `orthographicSize = 5.0` recalibration's own documented target
  (75–80%, set 2026-08-09 from a real screenshot) almost exactly. Confirms the vertical tuning is correct,
  and this part of the calculation is **aspect-ratio-independent**, so it holds regardless of window shape.
- **Horizontal (board width, 8 units along world X → camera's "right," zero cross-talk with Z):** screen
  fraction = `8 / (2 × orthographicSize × aspect) = 0.8 / aspect`. This only *shrinks* as the viewport gets
  wider — on any landscape or square window (`aspect ≥ 1`, which `C48`'s landscape-desktop mandate
  guarantees), the board's width can occupy **at most 80%** of the viewport and less on any wider window,
  so it cannot be cut off. (A near-square capture in one of today's screenshots made the board look tight
  against the frame edges — that's consistent with a narrow capture/crop, not evidence of an actual
  framing bug, given the math above.)

**Conclusion: `orthographicSize = 5.0` is correctly calibrated — no change made.** This resolves the
standing "needs another look" flag with actual math rather than another guess; still worth a human glance
next time the Editor's open, but there's no numeric reason to touch it.

## 2026-08-10 (continued 10) — human stepped away for 2h, Integrator pushing autonomously

**Human explicit instruction:** "keep pushing the schedule till i come back notify me of decisions, but
i will not be check in editor." No human visual sign-off is available for this window — proceeding on
Integrator judgment for calls that would normally wait for a screenshot or a human pick, logging every
decision here instead of pausing. Push notifications are unavailable this session (mobile push disabled
in the human's config), so decisions are logged here and surfaced at the top of this file instead.

**Two more real bugs found and fixed while auditing the cloud fix, both same root-cause class as the
reflection-probe bug (silent misconfiguration batchmode can't catch):**

1. **Clouds rendered as solid black rectangles**, not soft puffs — the human caught this immediately on
   the very next screenshot after the cloud rework merged. Root cause: `BoardWeatherPocket.CloudMaterial()`
   created a URP particle material via `new Material(shader)` without ever configuring transparency — URP
   materials default to **Opaque**, they don't infer blending from a texture's alpha channel. Every
   billboard rendered as a hard-edged opaque quad; the atlas's transparent padding read as solid black.
   Fixed (`af5d2b1`): added `ConfigureAlphaBlend()`, setting `_Surface`/`_Blend`/blend-factor/`_ZWrite`
   properties and the `_ALPHABLEND_ON` keyword (correct for the particle shader family) plus Transparent
   render queue. Verified via disposable worktree batchmode: EditMode 124/124, PlayMode 37/37.

2. **Window glass material was also opaque** (`Glass.mat`, used by `WindowSmall`/`WindowLarge`), found by
   proactively auditing other runtime-material creation sites for the same pattern rather than waiting for
   another screenshot to catch it. This one silently defeated an already-shipped feature — `BoardView.cs`
   places a warm emissive glow pane essentially at the same position as each window's own glass mesh
   (checkpoint 3's "lit window" dressing), so an opaque glass pane in front of it would fully block the
   glow. **First fix attempt was itself wrong** — used `_ALPHABLEND_ON` (the particle-shader keyword) on a
   `Universal Render Pipeline/Lit` material, which doesn't recognize that keyword; caught by inspecting the
   regenerated `.mat` file directly (`m_InvalidKeywords` contained it, `m_ValidKeywords` was empty,
   `stringTagMap` still said `Opaque`) rather than assuming the first attempt worked. Corrected to the
   right URP Lit keyword, `_SURFACE_TYPE_TRANSPARENT` (`c45dafb`, `5720d31`). New tool:
   `InteriorPackImportTool.RunGlassFix()` (re-runnable, same bootstrap-tool pattern as the rest of this
   session's fixes) — ran via batchmode in a disposable worktree, verified the resulting asset's serialized
   properties directly before copying it back to `master`, not assumed correct from a clean log.

**Final combined batchmode pass** (disposable worktree `logiCard-verify-final`, created and removed same
session) on `master` @ `5720d31` with all three fixes together: **EditMode 124/124, PlayMode 37/37.**

**None of this is visually confirmed** — same caveat as everything else in this session's visual work, now
more pointed since there's no human available to check for 2 hours. Treat all three fixes (reflection
clear-flags, cloud alpha-blend, glass transparency) as "should be correct by direct inspection of the
serialized asset state," not "confirmed to look right."

## 2026-08-10 (continued 9) — real clouds merged; ready for a fresh human look

**`feat/real-cloud-models` merged** (`be119b8`, worker commit `c0c4f39`). Replaces every flat tinted
primitive-sphere cloud puff with a burst-spawned, non-moving `ParticleSystem` cluster of billboarded,
randomly-framed cloud sprites — a real CC0 texture atlas (Kenney "Smoke Particles," 8 "White puff" frames
composed into one 4x2 grid), not geometry, so the alpha silhouette reads as soft mass instead of a hard
sphere edge. Reuses the exact original puff bounding boxes/positions and the `InterimCloudScale`/
`InterimCloudHeightBoost` framing correction unchanged, so the earlier "clouds loom over the board" bug
(fixed 2026-08-09) can't regress. Rain untouched, as asked. `Assets/_Project/Art/Environment/THIRD_PARTY.md`
updated with full provenance, including why the atlas is a derived composite rather than a raw asset copy.

**Reviewed before merging, not taken on the report:** read the full `BoardWeatherPocket.cs` diff —
confirmed the bounding-box/scale reuse claim by inspection, confirmed particle tinting happens per-particle
via `startColor` (reusing the sphere code's exact tint values) rather than a lossy shared-material
approach, confirmed `PlaceRain` has zero diff.

**Re-verified via a disposable detached worktree** (`logiCard-verify-clouds`, created and removed same
session), Editor still locked on the main tree: EditMode 124/124, PlayMode 37/37.

**Where this leaves Phase 5's open items:** reflection clear-flags fix (`80049df`) and the cloud rework
(`be119b8`) both landed today in response to direct playtest feedback that neither reflections nor clouds
were reading as improvements. **Neither has had an actual human look yet** — both agents/the Integrator
worked without Editor/screenshot access. This is the natural next checkpoint: get a fresh screenshot before
doing anything else in this area, same "don't chain blind changes" discipline this session has used
throughout.

## 2026-08-10 (continued 8) — reflection root-cause found and fixed; cloud rework in flight

**Human caught the reflection retune not actually working** from a real screenshot ("i dont think the
reflections has any changes... i can't see this is getting better with my own eyes") and asked directly
whether it's good enough to move on — correctly not accepting a green test run as proof.

**Root-caused directly on the main tree, not delegated** (`80049df`): `BoardReflectionProbes.cs`'s
`ReflectionProbe` components never had `clearFlags`/`backgroundColor` set. Unity's default is
`CameraClearFlags.Skybox`, and this project deliberately has no skybox configured (`ART_DIRECTION.md`
wants a bounded dark void, not an open horizon) — so every probe was rendering a mismatched/undefined
environment instead of the actual dark void the main camera shows (`cam.backgroundColor =
(0.035, 0.04, 0.055)`, set in `GameBootstrap.ConfigureCamera`). Fixed by setting `clearFlags =
SolidColor` and matching that exact background color on each probe. Batchmode could never have caught
this — it only proves the probe builds and runs without throwing, not what it actually captures.
Deliberately did **not** also re-touch `wetSmoothnessBoost` in the same commit — one variable at a time,
so the next screenshot can actually tell us whether this was the real fix instead of stacking another
blind guess on top. **Re-verified via a disposable detached worktree** (`logiCard-verify-refprobe`,
created and removed same session): EditMode 124/124, PlayMode 37/37.

**Second, parallel track: real cloud/weather models.** The human separately flagged the placeholder tinted
sphere clouds as the more obviously fake element in the same screenshot — `BoardWeatherPocket.cs`'s
`PlaceCloudBank` has been explicitly marked `TEMPORARY interim` since Day 8/checkpoint 1 and never
replaced. Worker spun (`feat/real-cloud-models`) to source real CC0 cloud textures/assets (billboard/
particle-based, since URP has no orthographic-compatible volumetric cloud support) and replace the sphere
puffs, leaving rain untouched (human confirmed rain already reads fine).

## 2026-08-10 (continued 7) — reflection probes + Scout re-outfit both merged

**`feat/wet-surface-reflections` merged** (`a531e90`, real subagent this time — see the archive's
"continued 6" entry for the earlier process error and its correction). SSR confirmed infeasible in this
URP version (17.5.0), so the wet Yard/Hall/Vault floors get their reflection source from real Reflection
Probes instead: new `BoardReflectionProbes.cs` places one probe per room (mirrors `BoardWeatherPocket`'s
`Build(BoardView)` pattern), using `ReflectionProbeMode.Realtime` + `RefreshMode.OnAwake` as a
baked-equivalent substitute since the board is built procedurally at runtime with no persistent editor
scene to classic-bake against. `LogiCardURP.asset`'s `m_ReflectionProbeBlending`/`m_ReflectionProbeBoxProjection`
flipped on via a new re-runnable `ReflectionProbeBootstrap.cs` (same pattern as the earlier URP
post-processing tool). `BoardSurfaceMaterials`' `wetSmoothnessBoost` retuned now that there's a real
reflection source (Yard 0.55→0.42, Hall 0.28→0.34, Vault 0.62→0.46, Flank 0.35→0.30) — the old values were
tuned blind against zero reflection input. `GameBootstrap.cs` got one small, scoped addition
(`BuildReflectionProbes()`, mirrors `BuildWeatherPocket()`'s call shape exactly).

**`feat/scout-reoutfit` merged** (`d5ee45e`), resolving **C56**. Scout's source FBX swapped from
`Adventurer` to `Worker` within the same already-owned CC0 Quaternius pack — one-line change in
`PawnImportTool.ImportScoutBatch`, existing pipeline untouched. `Worker`'s extra vest/high-vis parts read
as utility/work attire rather than fantasy-adventurer or off-duty leisurewear (the other two candidates in
the pack, `Casual_2`/`Casual_Hoodie`, were rejected for the latter reason). Team-color tint hook still
works unchanged (`Worker_Body` matches the same `"Body"` substring `Adventurer_Body` did).

**Both merges reviewed in depth before merging, not just taken on the reports:** read the actual diffs,
confirmed `GameBootstrap.cs`'s addition in the reflection PR was as narrow as claimed (one method call +
one new method, mirroring existing code shape), confirmed the Scout re-outfit's `PawnImportTool.cs` change
was exactly the one line claimed. Both merged clean, no conflicts, no overlap between the two branches.

**Re-verified via a disposable detached worktree** (`logiCard-verify-merge`, created and removed same
session) since the Editor was still locked on the main tree — real batchmode on the combined merge, not
self-review alone: **EditMode 124/124, PlayMode 37/37, both green.**

**Still needs a human sighted pass** — neither agent had Editor/screenshot access, both said so plainly.
Worth checking together: does the reflection read right (or too-obviously-low-res), does Worker's outfit
actually look like a plainclothes operator at play distance, and the standing `orthographicSize` question
from checkpoint 3.

## Older history — archived

Everything before the entries above was moved to [`DRAFT_HANDOFF_ARCHIVE.md`](DRAFT_HANDOFF_ARCHIVE.md) —
two archival passes, both noted at that file's top. Nothing was deleted; git history and the archive both
preserve it verbatim. `PRODUCT_MEMORY.md`'s binding `C46`–`C56` rows and `SCHEDULE.md`'s current phase
status are the authoritative summary of what's landed vs. still open — read those, not this file's history,
if that's what you're after.
