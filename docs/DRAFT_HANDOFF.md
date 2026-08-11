# Draft Handoff — 2026-08-07

## 2026-08-11 (continued) — asset-pack audit merged, interior props re-sourcing in flight

Human asked to use a worktree to (1) audit `D:\XinyeData\projects\assets` for anything licensing-safe to
use, and (2) research more Asset-Store-buyable packs. Delegated to a worker in `feat/asset-pack-audit`,
reviewed the diff directly before merging, merged clean (docs-only, no `Assets/`/code touched, no test
run needed).

**Confirms, doesn't newly discover, the existing Synty licensing flag** — the reseller bundle in that
folder (`80套…U3D素材`) contains the exact three already-imported packs (Office/Heist/City), each wrapped
with an explicit "not for commercial use, buy the genuine version" disclaimer + Taobao shop link. No
folder anywhere in the desk has a real Asset Store receipt for those three. New: confirmed live prices to
license them if the human keeps the Synty look (Heist $29.99 + Office $49.99 + City $20.00 ≈ $99.98
total) — see `docs/ART_PACK_RESEARCH.md`'s new top section.

**Safe-to-use-now subset identified** (`docs/ASSET_PACK_AUDIT.md`, new): `Kenney_Extracted` and
`Tem_0230_Kenney…All-in-1` (genuine Kenney CC0, verified by license-file content not just labeling), and
Quaternius "Ultimate Modular Men" (genuine CC0) — the latter's file naming (`Worker_Body.fbx`,
`Swat_Body.fbx`) matches `PawnImportTool.cs`'s existing source paths almost exactly, so it's very likely
the actual origin of the current Scout/Juggernaut placeholders. Flagged unsafe: `0095城市`, `B840 Anime
City Pack`, `G108、卡通城镇` (worse — raw scraped forum files with a second, different Taobao shop link,
not even Unity-packaged).

**New Characters section in `ART_PACK_RESEARCH.md`** — nappin has no character pack (confirmed against
their full catalog). Closest clay-adjacent candidate found: Toony Tiny Citizens Megapack ($30), flagged
as needing real prep work to fit `PawnImportTool.cs`'s single-FBX/no-texture/`"Body"`-renderer shape.
Honest fallback stays keeping Quaternius (free, already fits the pipeline exactly).

**`feat/interior-props-wiring` merged.** All 14 `Resources/Interior/*.prefab` names re-sourced from
`Assets/PolygonOffice/` instead of Quaternius (Door/DoorAlt/DoorDouble/WindowSmall/WindowLarge/
LightCeiling/LightCeilingAlt/LightDesk/ShelfLarge/Shelf/Bookshelf/Cabinet/Table/Chair) — closes item 2 of
the "what's actually left to wire" list two sections below. `InteriorPackImportTool.cs` rewritten to
instantiate PolygonOffice's ready-made prefabs directly (that pack ships prefabs with external materials
already, unlike Quaternius's raw-FBX source) and, critically, **duplicates every source material into
`Resources/Interior/Materials/` before URP-converting it rather than mutating the shared
PolygonOffice-owned original** — those materials are referenced by content well beyond this catalog.
`BoardView.cs` needed zero changes; the `Resources/Interior/<name>.prefab` load-by-name contract and the
door pivot/unit-scale contract are both fully preserved (verified directly in the serialized prefabs, not
just taken on the report). Window glass re-verified transparent post-rebuild — same
`_SURFACE_TYPE_TRANSPARENT`/`_SrcBlend`/`_DstBlend`/`_ZWrite` fix shape as the earlier Quaternius glass
regression, this time confirmed by the Integrator reading the actual serialized `.mat` before merging,
not assumed from a green log. One real Unity quirk hit and worked around: a material's blend-state
keywords/properties can get silently re-derived back to shader defaults the *first* time a newly-shader-
converted asset is ever saved — the tool now runs its whole catalog+fix pass twice per invocation, which
converges correctly. Batchmode (worker-reported, matches the Integrator's independent artifact checks):
EditMode 124/124, PlayMode 37/37 — Integrator could not re-run batchmode itself this pass (didn't locate
the Editor executable quickly) but did independently verify the boundary (`git diff` against the true
merge-base: zero touches under `Assets/PolygonOffice/**`, zero touches to `BoardView.cs`) and the glass
material's serialized properties directly. Merged `e2892ef` on `master`; worktree removed.

**A third worktree was set up for the human to run themselves** (their own separate agent tooling,
same pattern as the 2026-08-10 vibrancy/map-continuation waves) rather than the Integrator spawning a
third background agent — see whatever slice/brief that session's own summary describes for what it
covers; check `git worktree list` / `docs/departments/INDEX.md` for its current name and status if this
entry is stale by the time you're reading it.

## 2026-08-11 — save draft: Synty POLYGON packs landed (raw import), integration scoped for next session

Session pausing here per the human — everything below is committed on `master` locally (not pushed this
time; push on request next session, same as always).

**Three Synty POLYGON packs imported and merged.** Human pointed at a local folder
(`D:\XinyeData\projects\assets`) containing a large collection of pre-downloaded Unity asset packs and
asked to use them directly. Found and imported (batchmode `-importPackage`, verified EditMode 124/124
before merge): `Assets/PolygonHeist/`, `Assets/PolygonOffice/`, `Assets/PolygonCity/` — exactly the three
packs `docs/ART_PACK_RESEARCH.md` recommended. **This is a raw import only — none of it is wired into the
game yet.** `BoardView`/`BoardSurfaceMaterials`/`PawnImportTool`/`Resources/Interior/`/`BoardWeatherPocket`
still reference the old Quaternius/Poly Haven/Kenney assets.

**Licensing flag — read before doing anything else with these files.** The source folder's packaging
(sequentially numbered `.rar` archives bundled as an "80-pack collection," Taobao reseller link inside
one archive) strongly indicates these are not individually-licensed Asset Store purchases. Flagged
directly to the human, who made an informed, explicit call: **use now to prototype/iterate, buy real
licenses (Unity Asset Store, same publisher account as the `OfficeEssentialsPack` already purchased
legitimately) before any public release or Steam upload.** This is a real ship-blocking TODO — do not
let it get lost by the time shipping is actually being discussed. Whoever picks up ship-readiness work
should check this section first.

**What's actually left to wire (scoped, not started) — read this before diving in tomorrow:**

1. **Characters (Scout/Juggernaut) — more involved than a simple FBX swap.** `PawnImportTool.cs`
   currently takes one FBX with per-part materials and no textures, builds a static prefab at
   `Resources/Scout/Scout.prefab` or `Resources/Juggernaut/Juggernaut.prefab`, and `PawnView.cs`'s
   `TryBuildImported` auto-scales it to `TargetVisualHeight` and tints whichever renderer's name contains
   `"Body"` with the team color via a `MaterialPropertyBlock`. **Heist's character prefabs
   (`Assets/PolygonHeist/Prefab/Characters/Character_Male_SWAT_01.prefab` for Juggernaut,
   `Character_Male_Overall_01.prefab` for Scout) don't fit this model directly** — inspected the prefab
   hierarchy and it's a shared modular rig: one skeleton with *every* outfit/character variant nested as
   child renderers in the same file (all confirmed present: SWAT, FBI, Overall, Shirt, SuitVest, in both
   Male/Female, all inside `Character_Male_SWAT_01.prefab`'s own hierarchy). No renderer is literally
   named `"Body"`. Two real options for next session: (a) write a small Editor script that isolates just
   the wanted outfit's renderers (disable/strip the rest) before saving as the archetype prefab, or
   (b) extend `PawnView`'s tint-marker logic to match a broader/different naming convention for this
   pack. Also confirmed the materials are still on Unity's built-in **Standard** shader (not URP/Lit) —
   `PawnImportTool`'s existing URP-conversion logic (read `_Color`, swap shader, set `_BaseColor`) is
   reusable, just needs to run against the prefab's material dependencies instead of an FBX path.
2. **Interior props (`Resources/Interior/*.prefab`, 14 resource names: Cabinet/Chair/Table/Shelf/
   ShelfLarge/Bookshelf/LightCeiling/LightCeilingAlt/LightDesk/WindowSmall/WindowLarge/Door/DoorAlt/
   DoorDouble) — not started, catalog is large.** `PolygonOffice` alone is ~4800 files across deeply
   nested category folders (`Prefabs/Props/Desk Props/`, `Kitchen Props/`, `Misc/`, `Roof Props/`, etc.)
   — didn't find obvious single-match prefabs for basic furniture (chair/table/cabinet/shelf) in a first
   pass; needs a proper targeted search next session, not a rushed guess. Doors specifically carry extra
   risk — this session's earlier glass-transparency bug hunt (the `_SURFACE_TYPE_TRANSPARENT` vs
   `_ALPHABLEND_ON` saga) was about exactly this kind of prop, so budget real verification time for
   whatever replaces `WindowSmall`/`WindowLarge`/glass materials, don't assume it'll just work.
3. **Board floor materials (`BoardSurfaceMaterials.cs`)** and **weather/clouds (`BoardWeatherPocket.cs`)**
   — not started at all this session; `PolygonCity` has its own cloud/rain FX prefabs
   (`Assets/PolygonCity/FX/FX_Rain.prefab`, materials under `Materials/FX_Materials/Cloud_Mat.mat` etc.)
   worth checking as a real cartoon-cloud-style replacement for the Kenney smoke atlas, per the still-open
   "cartoon/clay-art clouds" gap from C58.

**Move-click bug** — still unresolved from before, see the "continued 15" entry below for the full
investigation. Unchanged since then; still needs the human to reproduce with console open.

## 2026-08-10 (continued 16) — save draft: click-bug investigation, art pack research, first real asset landed

Session pausing here — human is switching to another machine. Everything below is committed and pushed
to `origin/master` as of this entry.

**Move-click-fails-on-soil-ground bug — investigated, not yet root-caused.** Human reported move-clicks
failing specifically on the Yard (soil-textured) floor, working fine elsewhere. Chased this hard via
direct PlayMode diagnostics run in a disposable worktree (not committed, thrown away after): (1)
raycast-to-planar conversion is correct at every tested Yard/Hall point; (2) re-tested across a full
camera-rotation sweep (0°-270° plus negative yaw) — still correct everywhere, rotation isn't it; (3)
direct Yard-interior-to-Yard-interior pathfinding via `TryAddWaypoint` also checked — perfectly direct
routes, `ratio=1.00` against straight-line distance, no circuitous routing bug. All three of my strongest
hypotheses are empirically ruled out. Root cause **not found** — most likely candidate left is something
about the human's actual live window (DPI scaling, real aspect ratio) that batchmode's fixed small test
resolution can't reproduce. Landed instead (`ec4a141`): `BoardInputController`'s click handling had two
totally silent failure paths (raycast hits nothing; raycast hits something outside the board) — both now
log and show a visible "can't do that" toast, and UI-absorbed clicks near the dock log too (debug-only,
not a toast — that path fires on every normal HUD click). Next occurrence should be actually diagnosable
from the console instead of a mystery. **Still open, needs the human to reproduce with the console visible
next time it happens.**

**Art pack research (`docs/ART_PACK_RESEARCH.md`, `PRODUCT_MEMORY.md` pending a decision row once
purchases land) — merged.** Human said the look is "still a big disappointment" even after C58's recolor
pass and asked to stop hand-building/tinting primitives, buy real asset packs instead, reference stays
Link's Awakening. Delegated to a worktree (human ran it themselves via their own separate agent tooling —
not spawned by me, per their explicit preference this session). Verdict: buy Synty POLYGON Heist + Office
+ City (~$75-100 on sale) + free SIMPLE Sky for clouds — full shopping list, license/commercial-ship
terms, alternatives considered and rejected (ithappy Cartoon City, Kenney/KayKit, Mixamo), integration
effort sketch per system. No purchase made by that research pass — human's call.

**First real asset pack received and committed.** Human bought and imported an office prop pack —
lands as `Assets/nappin/OfficeEssentialsPack/` (287 files, ~32MB: materials, models, prefabs, textures,
a demo scene). Publisher is "nappin" (nappin.dev), not literally the Synty POLYGON Office Pack the
research doc recommended — human's own purchasing call, same category (office interior dressing).
**Received and committed, not yet integrated** — `BoardView`'s `Resources.Load("Interior/…")` calls
still point at the old Quaternius set; wiring this pack into the actual board (desks/chairs/props
replacing current `Assets/_Project/Art/Environment/Interior/` dressing) is unstarted follow-up work.
Also picked up incidental Editor-driven changes from opening/inspecting the new pack: `QualitySettings`
antialiasing `0→4`, a new Sentis scripting define symbol, and `Glass.mat` shader-keyword/blend-property
normalization (harmless, Editor auto-correction, not manually touched).

**Next session should start by:** (1) getting the human to reproduce the click bug with console open,
(2) confirming whether more of the recommended pack list (Heist/City/SIMPLE Sky) also landed or is still
pending, (3) starting real integration once the human confirms which packs are in hand.

## 2026-08-10 (continued 15) — vibrancy recolor pass + map-select UI merged (C58/C59)

Human pushed back hard on the look ("big changes... Link's Awakening... vibrant... do not be so
tedious") and asked for two parallel jobs, explicitly delegated to worker worktrees since they were
running low on usage themselves. **Note on process:** I initially launched both as Agent tool calls
myself before realizing the human specifically wanted paste-ready commands to run in *their own*
separate agent sessions instead — stopped both immediately, gave them the handoff blocks, and they ran
the work themselves. What follows is my review/verify/merge of what came back.

**`feat/vibrancy-pass` (C58)** — post-processing grade warmed (`saturation -4→18`, cool `colorFilter`
→ warm-neutral, `postExposure -0.10→0.08`), board surface tints retinted warmer/more saturated,
clouds denser (`1.0→1.4x` density, `8-22→12-30` particles) and warm-tinted. Clean — no fixes needed.
**Real gap, flagged not hidden:** the "cartoon/clay-art" cloud *style* ask only partially landed —
density/warmth are real, but it's still the same realistic Kenney smoke-photo texture, not a style
swap. That needs new art or a shader treatment, logged as a follow-up. Batchmode: EditMode 124/124,
PlayMode 37/37. Merged clean, `5b73960`.

**`feat/map-continuation` (C59)** — floor grid-line lattice deleted (`BoardView.PlacePaintedGrid`,
cosmetic clutter), a real map-select screen added (Character Select → Map Select → Lobby, reusing
`SelectionGrid`, plain not elaborate per the human's "don't be tedious" ask), local-only (no network
sync — Net stays paused). Also restyled `ModalDialog` (rounded card, divider, pill buttons) toward a
human-supplied reference screenshot. **This one needed real Integrator fixes before merge** — the
worker's own batchmode claims weren't run/reported, and disposable-worktree verification caught:
1. **Compile error** — `BuildPawns()`'s switch still referenced the old `ActiveMap` constant name
   after it was renamed to a field; one-line fix.
2. **37/37 PlayMode failures** — `GameBootstrap.Awake()` now defers board/pawn build until the app
   flow reaches the match (correct, needed for real map choice), but `SliceSceneFixture`'s test setup
   asserted those objects existed *before* calling the bypass that actually builds them. Reordered the
   fixture. A second test (`BootThroughLobbyLocalPlayReachesMatchHud`) still expected Character Select
   to lead straight to Lobby — updated for the new Map Select step in between.
3. **A real bug, not just a test artifact** — `UiStyle.RoundSprite` used
   `Resources.GetBuiltinResource<Sprite>("UI/Skin/UISprite.psd")`. That path is an Editor-only extra
   resource; the runtime API silently fails on it (logs an assert, returns null) in batchmode tests
   **and in an actual Player build** — this would have shipped broken. Replaced with a small
   procedurally-generated 9-sliced rounded sprite, cached after first build, works identically
   everywhere. Also wired the primary/secondary dialog buttons to the new rounded-sprite/color tokens
   — `UiStyle.PrimaryButton`/`SecondaryButton` had been declared but never actually used by
   `ModalDialog.Show()`, so the buttons stayed square/amber despite the card getting rounded.

   Batchmode after fixes: EditMode 124/124, PlayMode 37/37. Merged, `cdb16cf` (fixes) + `5e9b148`
   (merge commit). Final combined pass with C58 also merged: EditMode 124/124, PlayMode 37/37 —
   `logiCard-verify-final` disposable worktree, removed after.

**Two more empty worktree-directory leftovers** (`logiCard-vibrancy-pass`, `logiCard-map-continuation`)
join the same pending-cleanup backlog as the two from the map-roster wave — deregistered from git
cleanly, on-disk directories wouldn't delete (`Device or resource busy`, same transient OneDrive/Search
Indexer lock class as before). Harmless, safe to delete by hand whenever the lock clears.

**Character-movement vibrancy is explicitly deferred, not forgotten** — the human's own framing
("let's focus on color for now") scoped it out of this round. Next natural ask if/when they come back
to this.

**Standing caveat, unchanged:** both jobs are presentation/UI — batchmode green confirms "wired and not
regressing," not "looks right" or "looks vibrant enough." Neither has been visually confirmed by a
human yet.

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
