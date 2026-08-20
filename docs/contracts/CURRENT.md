# Cross-Dept Contracts — Current Wave

**Updated:** 2026-08-20 by Integrator — **C36 geometry-breach + Bomber wall-only verb (Sim layer only)
landed directly on master**, human-directed ("character, GO") per C71's already-locked scope. See the
section below. Match Shell Layout, Map, and Camera waves remain CLOSED per the 2026-08-17 note preserved
below. **No seat is coding-hot right now.**
**Rule:** Only Integrator edits this file after a merge. Workers implement against the frozen signatures
below.

## Open — C36 geometry-breach + Bomber wall-only verb, Sim layer (opened + landed 2026-08-20)

**Human ask:** "character, GO" — resume C36/Bomber work, explicitly paused since the Phase 5 art
priority began. Scope confirmed against **C71** (already locked 2026-08-16, human accepted every
recommendation — re-confirmed via a redundant AskUserQuestion pass that matched C71 exactly): wall-only
first (no floor-drop/per-floor-occupancy this wave), designed breach points only (not freeform), Attach
+ Detonate as two scheduled nodes mirroring Door Open/Close.

**Landed (Sim layer, fully tested):**

```csharp
// Sim
public enum BreachState { Intact, Damaged, Breached }  // Damaged reserved, unexercised by wall-only v1
public sealed class BreachPoint { Segment Segment; BreachState InitialState; string DisplayName; }

// ArenaBoard — mirrors Door's API exactly
void RegisterBreachPoint(BreachPoint point);
bool TryGetBreachPoint(PlanarPosition point, out BreachPoint breachPoint);       // exact-match, mirrors TryGetDoor
bool TryGetNearestBreachPoint(PlanarPosition point, float maxDistance, out BreachPoint breachPoint);
BreachState GetBreachState(BreachPoint point);
void SetBreachState(BreachPoint point, BreachState state);
bool HasAttachedBomb(BreachPoint point);
void SetAttachedBomb(BreachPoint point, bool attached);
// IsBlocking / TryGetNearestBlockPoint / Clone extended: an Intact or Damaged breach point blocks
// Move/Shoot exactly like a wall; only Breached opens it.

// Net
ActionVerb.BombAttach      // Position = target BreachPoint's segment midpoint (mirrors Door)
ActionVerb.BombDetonate    // same targeting shape
TapeEventType.BombAttached     // one-shot; no geometry effect
TapeEventType.GeometryBreached // continuous, mirrors DoorOpened/Closed — Breached from this Seconds on

// Timeline (PawnProgram) — same InteractRadius/board-tap shape as TryQueueDoor
bool TryQueueBombAttach(BreachPoint point, out string rejectionReason);   // BombAttachSeconds = 3f (C71 strawman)
bool TryQueueBombDetonate(BreachPoint point, out string rejectionReason); // BombDetonateSeconds = 1f (C71 strawman)
```

**GhostResolver:** `ResolveShots` gained a third interleaved chronological stream (alongside shots and
door toggles) — Attach/Detonate toggles on the same `BreachPoint` apply in strict time order against
the round's scratch board, same precedence-over-same-instant-shots rule doors already use. A same-round
Attach is visible to a later-same-round Detonate on the same point. **Resolve() never mutates its own
`ArenaBoard` input** (verified by test — `GhostResolverBombTests`'s two "stays a pure function" cases) —
same discipline Door already holds to; persistence to the real board across rounds is a presenter's job,
not the resolver's.

**Deviations, documented not hidden:**
1. **Detonate requires the same `InteractRadius` proximity as Attach** (mirrors Door exactly) — C71
   never actually settled whether detonation should instead be remote/no-proximity (a real detonator
   wouldn't require walking back). Flagged in `PawnProgram.TryQueueBombDetonate`'s doc comment as a
   one-line change if remote detonation is wanted later, not an architecture decision.
2. **No same-round dynamic movement re-routing through a newly-breached wall** — a Move leg drafted
   before a detonation cannot benefit from geometry that opens later in the same round (unlike Door,
   which the resolver does dynamically re-check mid-leg). Deliberate simplification: nothing at draft
   time would have told the player this wall could open, so the draft-time pathfinder never offers that
   route anyway — this loses no real capability, just the Door-equivalent machinery for it. Shoot LoS
   *is* fully dynamic (verified) since that's evaluated at resolve time, not draft time.
3. **Attach has no charge/count limit** — C71 locks costs (3s/1s strawman) but never states a
   per-match Bomber charge count the way Bandage/Storm have one. Left genuinely unlimited rather than
   inventing a number; revisit if the human wants one.

**Explicitly NOT built this wave (Sim-layer-only slice, by design — see `DRAFT_HANDOFF.md`'s 2026-08-20
note for the full reasoning on why this was scoped down rather than attempted whole):**

- **RoundPlayback presenter** — `BombAttached`/`GeometryBreached` are `ReservedNoPresenterYet` in
  `TapeEventPlaybackCoverageTests`. `GeometryBreached` should mirror `DoorOpened`/`DoorClosed` exactly
  (continuous, `SyncBreachToSeconds`); `BombAttached` likely a one-shot banner or board marker.
- **BoardView visuals** — no breach-point rendering (wall material/mesh change on Breach) exists yet.
- **Map authoring** — no map has an actual designed `BreachPoint` registered. `GhostResolverBombTests`
  builds its own scratch `ArenaBoard`; nothing in `GameBootstrap`/the three shipped maps calls
  `RegisterBreachPoint` yet. This needs a real per-map content decision (which wall, which map first —
  Freight Yard is the obvious candidate as the primary map), not just code.
- **HUD** — no board-anchored prompt (`UI_BOARD_ANCHORED_COMPONENTS.md` applies once built), no mode
  button, no scrubber markers. Per `CHARACTER_BOMBER_AGENT_BRIEF.md` §6, this is UI seat's slot once the
  Sim contract is frozen — it now is.
- **Bomber Character grant** — nothing gates `BombAttach`/`BombDetonate` to a specific archetype yet
  (any pawn can currently queue them); Character-gating is a HUD/legality concern per the brief, same
  split as everything else.

**Batchmode:** EditMode 196/196 (188 baseline + 6 new `GhostResolverBombTests`), PlayMode 66/66
(unaffected — no PlayMode/scene coverage this wave, by design, see above). Editor closed on `master`'s
own path for every run.

**Next real step, when picked up again:** map authoring (pick a wall on Freight Yard, register a
`BreachPoint`), then the RoundPlayback presenter + a real PlayMode test exercising the whole
Program→Resolve→Playback loop, then UI's HUD slot. Not blocked on anything further design-wise — C71
already answered every open question that mattered for this slice.

## Closed — Match Shell Layout (opened 2026-08-15, closed 2026-08-16)

**Human ask:** layout only (not per-component chrome). Refs `screenshots/image copy 18.png` /
`19.png` for region order + playful schedule timeline — **reject** Hearthstone minion battlefield;
keep our diorama map. **Landed exactly as scoped** — no card-lane overlay, diorama map kept.

| Seat | Mode | Result |
|------|------|--------|
| **UI** | Coding | Landed — five-band `ProgramHud`, merged `c9925b1` |
| **Cards** | Docs | Landed — `CARD_COLLECTION.md` §13, merged `a21b29c` |
| **Character** | Docs | Landed — `CHARACTER_FANTASY.md` §4.1, merged `a21b29c` |
| **Map** | Docs | Landed — `MAP_PRESENTATION_STANDARD.md` §6, merged `a21b29c` |
| **Atmosphere** | Docs | Landed — `WEATHER_MAP_VIEWPORT.md`/`CLOUD_MOTION.md`, merged `a21b29c` |
| **Camera** | Was paused | **Now unblocked** — MapViewport API exists; resume freecam reconcile |

**Landed signatures**

1. Region order locked as planned: InfoBar → MapViewport → HandBand → ToolBar → TimelineSchedule.
2. `ProgramHud.MapViewport` — public rect Camera/Integrator letterbox against. Supersedes the old
   bottom-dock-only camera math (`HudDockHeight`/`TopStripHeight`, closed Phase-1 contract below) —
   those constants stayed numerically equal to the new rect, so `GameBootstrap.ConfigureCamera` needed
   zero edits to keep rendering correctly *at the old geometry*; retuning for MapViewport's shorter
   height (Map §6: doors > flank sightline > floor edge priority; Rail Platform is the tall-map risk
   case) is the still-open Camera work.
3. Timeline tracks: **YOU / ENEMY / EFFECTS** — playhead = existing Time Resource scrubber seconds
   (C28 continuous — not a 12-tick clock).
4. Drag-to-play + verb Sim entry points unchanged (`TryQueueBandageAt` / `TryQueueStormAt` / Move/Shoot/Door).
5. `GameBootstrap.RegisterMatchState(() => playback.WoundsOf(id), () => playback.BandageChargeOf(id))`
   — the one-line Integrator hook flagged as still-open at UI Ready — wired at merge time; InfoBar
   wounds/charge reads are live, not stubbed.
6. HUD Chrome Ship Pass (`3f77b6c`) did not merge alone — absorbed into the Match Shell merge.

**Deviations, documented not hidden:** master had independently grown its own `GearHandView.cs`/
`ProgramHud.cs` hand-deck-drag-play implementation (`164012f`) in parallel with the UI worktree's own
version of the same feature (built on before HUD chrome + Match Shell). The merge took the UI
worktree's version wholesale for both files (the fully tested superset the human actually reviewed),
not a line-by-line reconciliation of the two independent implementations — functionally equivalent,
confirmed by the full green batchmode re-run above, but worth knowing if a future diff between the two
looks unfamiliar.

**Character and Atmosphere docs peers each have separate, larger unmerged work sitting in their
worktrees that was explicitly NOT pulled by this merge** (Character: an older Char Select carousel
feature, 12 commits; Atmosphere: the rejected Sunny weather mood + a stray recovery scene) — only the
Match Shell-scoped doc file(s) from each landed.

## Frozen contracts this wave (prior)

### Storm card — cross-dept (closed 2026-08-14 — **Cards + UI + Atmosphere**, human-directed)

**Landed:** Cards' `docs/cards/CARD_COLLECTION.md` entry + `docs/cards/GEAR_STORM_AGENT_BRIEF.md`
(numerics recommendation: `TR —`, 1×/Character/match, effect summary — still OPEN pending human lock).
UI's `PawnProgram.TryQueueStorm`/`BoardInputController.TryQueueStormAt`/`GearHandView` dock/`ProgramHud`
arm-place wiring, merged alongside Bandage HUD-side below. Atmosphere's `ApplyWeather` same-mood
early-out + `ApplyStormLightingDim` clean-baseline-on-restore fix (ported onto master directly rather
than merging their branch — see note below), with two new PlayMode tests.
**Deviations, documented not hidden:** (1) Storm's once-per-match gate is HUD-side "not already queued
this Program" only — per-round, not a true cross-round counter (no `StormCastCountOf` the way Bandage
has `BandageChargeOf`); harmless for now since TR cost is 0 and recasting an already-active mood is a
no-op at the presenter level — revisit if numerics lock to a real cost or strict enforcement is wanted.
**Closed 2026-08-18** — `RoundPlayback.StormCastCountOf` landed (mirrors `BandageChargeOf`), and
`GhostResolver` now enforces the cast itself the same way it already enforced Bandage's charge; see the
frozen-signatures block below, updated to match, and `docs/departments/core/STATUS.md`.
(2) Atmosphere's DoD item 3 ("storm rolling in" transition) was explicitly skipped, still an instant
module swap. **Not merged:** Atmosphere's branch also carried an unrelated, uncoordinated "Sunny weather
mood" feature (new `BoardWeatherMood` value, boot-mode changed Fair→Sunny, renamed lighting fields) —
human confirmed this should NOT land with this contract; only the two DoD fixes were ported (translated
onto master's actual, non-Sunny-refactored code), and that work stays uncommitted in the Atmosphere
worktree pending a separate decision.

**Depends on:** **C67**; Sim-side closed below (already on `master`); `GearHandView`/`ProgramHud` dock
pattern (same files the still-open Bandage HUD-side contract targets — build both in the same UI pass,
your call whether sequential or together, since it's the same worktree); `PLAYBACK_CONTRACT.md`.

**Frozen signatures (Sim-side — landed, `master`)**

```csharp
// Net
ActionVerb.Storm            // self-targeting, no board position, no wound/charge effect
TapeEventType.StormCast     // continuous presenter (mirrors DoorOpened/Closed), not one-shot

// Cards — CardId.Storm's value is landed here too (not left to the Cards seat) so Cards and UI
// can build in parallel from separate worktrees with no cross-branch ordering dependency —
// Bandage never hit this because all four first-wave CardId values were defined together upfront.
CardId.Storm = 4

// Boot — weather binding; GameBootstrap already calls this in BuildWeatherPocket()
void RoundPlayback.SetWeatherPocket(BoardWeatherPocket pocket);
```

`GhostResolver` emits `StormCast` at the node's `ExecuteTime`. **Updated 2026-08-18:** no longer fully
permissive — enforces the 1×/match cast itself (`GhostInput.StartingStormCastCount` →
`ReplayTape.StormCastCountFor`), the same shape as Bandage's charge gate. The HUD's "already queued
this Program" dedup stays as a same-round belt-and-suspenders check on top. `RoundPlayback.SyncWeatherToSeconds` derives
the active mood as a pure function of (arm-time snapshot + any `StormCast` ≤ scrubber second) and only
calls `BoardWeatherPocket.ApplyWeather` when the derived mood actually changes — **do not call
`ApplyWeather` from anywhere else per-tick**; it tears down and rebuilds the whole cloud/rain/lightning
module (`ClearWeather` → `DestroyImmediate` on every child), so an unguarded per-tick call is the "door bug
class" PLAYBACK_CONTRACT §2 rule 4 warns about, just far more expensive. `GameBootstrap.BuildWeatherPocket()`
now boots the board on **Fair**, not Storm (was Storm from the earlier Atmosphere merge this session) —
otherwise casting the card would be a no-op.

**DoD, split by seat:**

**Cards** (`docs/cards/CARD_COLLECTION.md`; `CardId.Storm` is already landed, no `CardData.cs` edit needed):
1. Write a `GEAR_STORM_AGENT_BRIEF.md`-style short numerics recommendation (Time Resource cost
   placeholder — do **not** invent a number, use `"TR —"` like every other un-locked first-wave card per
   C62's convention; once-per-match vs. unlimited casts; a one-line `effectSummary` describing the
   presentation-only weather trigger, not a combat effect).
2. `docs/cards/CARD_COLLECTION.md` catalog entry: Storm, gear card, self-targeting, Program-phase only,
   no LoS/target-pawn needed (unlike Bandage's board-tap-near-node option — Storm only ever needs a
   Time Resource second, nothing else).
3. Numerics stay **OPEN** until human confirms — this DoD item proposes defaults, Integrator/human locks
   them into a follow-up C-row amendment, same two-step shape C62→C63 used for Bandage.

**UI** (`Assets/_Project/UI/GearHandView.cs`, `ProgramHud.cs`, `BoardInputController.cs`,
`Assets/_Project/Timeline/PawnProgram.cs`):
1. Add a `PawnProgram.TryQueueStorm(float executeTime, out string rejectionReason)`. `TryQueueBandage`
   doesn't exist yet either (still-open contract, same worktree) — mirror the **real** existing
   precedent instead: `TryQueueDoor`/`TryQueueShoot` (`PawnProgram.cs` ~line 381/426) both start
   `if (HasDraft && !TryCommitDraft(out rejectionReason)) return false;` before their own checks.
   Storm needs no further legality beyond Program-phase + whatever once-per-match rule Cards'
   brief recommends — no board position, no Sprint gate.
2. `BoardInputController`: arming Storm sets `Mode = ActionVerb.Storm`; scrubber click places at the
   scrubber's current Time Resource second (Storm has no board-tap placement — it isn't targeted).
3. Dock a `Storm` slot into `GearHandView.FirstWave` (or its successor once Bandage's dock pattern
   lands) using the same cardstock visual language as the existing four.
4. **`Assets/_Project/Tests/EditMode/GearHandViewTests.cs` line 46–53,
   `FirstWaveRosterIsBandageInteractFlashbangAdrenaline`, hard-asserts `FirstWave.Length == 4` and the
   exact roster — adding Storm breaks it immediately.** Update that one test (extend the roster +
   rename); the file's other tests iterate `FirstWave` generically and don't need changes.
5. Tests: EditMode `PawnProgram` Storm case; PlayMode HUD arm→place smoke, mirroring whatever pattern
   the Bandage HUD-side contract's own DoD item 7 establishes — build this once, reuse for both cards.

**Atmosphere** (`Assets/_Project/Board/BoardWeatherPocket.cs`):
1. Confirm `ApplyWeather`/`ClearWeather` are safe to call **mid-match, repeatedly, in any order**
   (Fair→Storm, Storm→Fair on rewind, Storm→Storm no-op) — this API used to only run once at boot;
   it is now driven by a Playback-time presenter that can, in principle, call it many times across a
   scrub session (guarded on the `RoundPlayback` side, see above, but add your own same-mood early-out
   inside `ApplyWeather` too as defense in depth — belt and suspenders, not a design change). **No
   existing test exercises repeated `ApplyWeather` calls** (`BoardWeatherPocketPlayModeTests.cs` only
   checks a single mount) — add one: call `ApplyWeather` with the same mood twice and assert a child
   (e.g. `CloudBank`) is the *same* `GameObject` instance both times, not destroyed/recreated.
2. Double-check `ApplyStormLightingDim`/`RestoreLightingIfDimmed` (`BoardWeatherPocket.cs` lines
   ~1531/1574) round-trip cleanly across repeated Fair↔Storm cycles within one Play session (dimmed-light
   state must not drift or double-apply).
3. **Creative, optional:** a short "storm rolling in" transition-in beat (few-hundred-ms build, not an
   instant pop) sells the card-cast moment better than the current instant module swap — nice-to-have,
   not required for DoD.
4. Out of scope: no new weather VFX assets — this reuses the already-shipped Storm module verbatim.
   Note: `BoardWeatherPocketPlayModeTests.WeatherPocketBuildsCloudBankAndRimMistWithoutThrow` already
   had to be fixed by Integrator (it asserted boot-time `ActiveMood == Storm`, no longer true after the
   Fair boot-mood change) — it now calls `ApplyWeather(Storm)` explicitly before its Storm-structure
   assertions; nothing further needed there unless your idempotency work touches the same file region.

**Out of scope (all seats):** any combat/mechanical effect for Storm (visibility, blind, damage) — this
wave is presentation-only, mirrors Adrenaline's stub precedent; a mechanical effect needs its own
PRODUCT_MEMORY row and PLAYBACK_CONTRACT redesign, not bundled here.

### Storm Sim-side (closed 2026-08-14 — reference)

- `ActionVerb.Storm`, `TapeEventType.StormCast`, `CardId.Storm = 4`, permissive
  `GhostResolver.CompileTrack` emission, `RoundPlayback.SyncWeatherToSeconds`/`SnapshotWeatherAtArm`/
  `SetWeatherPocket`, `ResetForNewMatch` reverting weather to Fair on rematch,
  `GameBootstrap.BuildWeatherPocket()` boot-mood flip to Fair.
  `GhostResolverStormTests` (EditMode) covers the resolver. **Not yet covered:** a PlayMode arm→scrub→
  rewind test — blocked on UI's `TryQueueStorm`/HUD wiring above; add it alongside that work, mirroring
  the pattern `RoundPlaybackPlayModeTests` already uses for door/wound scrubbing. **Not yet
  batchmode-verified** — same standing caveat as everything landed today.

### Map Phase 2 — board surface material swap (closed 2026-08-14 — **Map seat** on `logiCard-map`, merged `a76f006`)

**Depends on:** **C65** (human YES, `docs/map/C53_SURFACE_MATERIAL_DECISION.md`); standard doc
`docs/map/MAP_PRESENTATION_STANDARD.md` §2 (material-family table) and §5 (Phase 2 preview, this contract
follows it); `Board*` dirty tree reclaimed — `master` @ `a419ad4` now carries the rematch/floors/lighting
commit Map was blocked behind, so `BoardSurfaceMaterials.cs`/`BoardView.cs`/`BoardReflectionProbes.cs` are
clean to branch from.

**Scope — presentation-layer only, matches MAP_PRESENTATION_STANDARD.md §0:**

- `Assets/_Project/Board/BoardSurfaceMaterials.cs` — floor/wall/door-tint/interior-prop-tint material
  builders only.
- `Assets/_Project/Board/BoardView.cs` — `SurfaceMaterialFor(MapSurfaceRole)` call sites, `PlaceRoomFloors`,
  `PlaceRoomDressing`/`PlaceDoorMesh` material (not mesh) wiring, map-aware dressing per §3.
- Untouched (out of scope, do not edit): `MapDefinitions`, `GameBootstrap.BuildXxxGeometry()`/
  `BuildXxxDefenderPayload()`, pathfinding, `Door` API, `Sim/`/`Net/`/`Timeline/`, weather/
  `BoardWeatherPocket` (Atmosphere's lane), and `GameBootstrap.BuildLighting()`/`BuildDioramaVolume()` —
  the 2026-08-14 rematch commit already retuned the lighting rig against the *old* material family;
  re-grading after the material swap is step 4 below, still Integrator's call on `GameBootstrap`, not Map's
  to edit directly (flag the need, Integrator makes the pass — `GameBootstrap` stays Integrator-owned per
  `departments/INDEX.md`).

**DoD (mirrors MAP_PRESENTATION_STANDARD.md §5):**

1. `BoardSurfaceMaterials` gains a `Solid()`/gradient-based floor+wall material set keyed by the same four
   `MapSurfaceRole`s (Yard/Hall/Vault/Flank), replacing `BuildWetSurface()` as the default path for those
   roles. Keep `BuildWetSurface` code itself (don't delete a working, still-referenced helper) — just stop
   calling it for board surfaces.
2. Re-skin nappin door/prop materials via the pack's own `(Mat)Gradient*` variants or a flattened duplicate,
   through the existing `InteriorPackImportTool` duplicate-and-convert pattern. Door/prop **meshes** stay;
   only the material changes.
3. Make `PlaceRoomDressing` map-aware (`MapId` param or per-map methods, C57's "one bespoke method per map"
   discipline) so Rail Platform / Vault Complex get real in-room dressing instead of Freight-Yard-shaped
   coordinates or nothing.
4. Flag the lighting/grade re-pass for Integrator once the material swap lands — don't self-edit
   `BuildLighting`/`BuildDioramaVolume`.
5. Tests: EditMode coverage that `SurfaceMaterialFor` returns the new flat family per role; existing
   `BoardView`/room-floor PlayMode smoke stays green.
6. Human screenshot check against the Link's Awakening reference (`ART_DIRECTION.md` Moodboard) before
   calling it done — batchmode green is not a look check.

**Out of scope:** geometry density, vents/breaches, room structure (C57), weather/atmosphere (Atmosphere's
lane), pathfinding/Door API/Sim (C35/C39/C41).

### Bandage HUD-side (closed 2026-08-14 — **UI seat** on `logiCard-modal-restyle`; brief `BANDAGE_HUD_AGENT_BRIEF.md`)

**Landed** exactly per the frozen signatures below. Batchmode reported by UI (Editor closed on that
worktree): EditMode 153/153, PlayMode 49/49 (Bandage alone); 166/166 / 51/51 combined with Storm. Not
independently re-run by Integrator. **Follow-up still open:** Integrator's Healed presenter
(`TapeEventType.Healed` in `RoundPlayback` + `PLAYBACK_CONTRACT` §3) — not started.

**Depends on:** C63; Sim-side closed below; `GearHandView` scaffold on master (`7213d98`);
`UI_FLOW.md` §4 item 3; `PLAYBACK_CONTRACT.md` (Healed presenter is a **separate** Integrator follow-up
after this HUD lands — do not invent FX here).

**Frozen signatures**

```csharp
// Timeline — Program booking (gear carve-out). Cost locked by C63.
public const float BandageSeconds = 3f; // on PawnProgram
bool PawnProgram.TryQueueBandage(float executeTime, out string rejectionReason);
bool PawnProgram.IsMidSprintAt(float seconds); // helper; used by TryQueueBandage

// Boot — match-carry read for HUD gates (mirrors WoundsOf)
int RoundPlayback.BandageChargeOf(int pawnId);

// Board — place while Mode == ActionVerb.Bandage
bool BoardInputController.TryQueueBandageAt(float executeTime, out string reason);
```

**HUD behavior (DoD)**

1. Dock `GearHandView.Build(...)` into the Program HUD (queue column — do **not** grow
   `ControlsColumnContentHeight` / break `ProgramHudLayoutTests`).
2. **Arm:** Program-phase only. Bandage arms → `BoardInputController.Mode = ActionVerb.Bandage`.
   Interact / Flashbang stay blocked (no contract). Adrenaline stays Execute-only via existing
   `GearHandPhase` rules (dock may show it greyed in Program).
3. **Place (timeline):** while Bandage is armed / Mode is Bandage —
   - scrubber **click** places at the scrubber's current Time Resource seconds, or
   - board tap near a booked Move node places at that node's `ExecuteTime`, else places at
     `PawnProgram.UsedSeconds` (append / stationary at schedule tip).
4. **Three legality gates** (Program-time; resolver stays permissive per Sim contract):
   - **Wounded** — `RoundPlayback.WoundsOf(localPawn) > 0` (Healthy → cannot arm)
   - **Charge** — match `BandageChargeOf == 0` and no Bandage node already in this Program
   - **Not mid-Sprint** — `!IsMidSprintAt(executeTime)` (Walk micro-moves OK; C63)
5. Cost label for Bandage = `"3s"` (C63). Other first-wave cards keep `"TR —"` placeholders.
6. After a successful place: clear gear arm, return Mode to Move, refresh spent/blocked presentation.
7. Tests: EditMode `PawnProgram` Bandage cases; PlayMode HUD arm→place smoke; update
   `GearHandViewTests` so only Bandage may show a locked cost.

**Out of scope:** `TapeEventType.Healed` presenter; Interact/Flashbang/Adrenaline resolve; deckbuilder
(C64).

### Bandage Sim-side (closed 2026-08-13 — reference)

- `ActionVerb.Bandage`, `TapeEventType.Healed`, per-match `BandageCharge` through
  `GhostInput` → `GhostResolver` → `ReplayTape` → `RoundPlayback`.
- Resolver permissive (no Sprint / Healthy re-check). Merged `4e6bb66`.

## Closed contracts (reference)

### `HudDockHeight` ↔ column layout, ultrawide overflow fix (closed 2026-08-10)

- Worker kept `HudDockHeight = 0.34f` unchanged — didn't need to touch the camera-rect coupling, just the
  controls-column row budget inside it. Added `ProgramHud.ControlsColumnContentHeight` and
  `DockHeightInUiUnits(width, height)` as an explicit, EditMode-tested invariant so a future retune can't
  silently overflow the dock again the same way.
- `BoardCameraRig` unaffected — worker's brief explicitly excluded `GameBootstrap.cs`/`BoardCameraRig.cs`.
- Merged `d2624c2` (worker commit `a0d823b`). Worker-reported EditMode 124/124, PlayMode 37/37; not yet
  independently re-run by Integrator (Editor was live at merge time) — see `DRAFT_HANDOFF.md`.

### Camera viewport-rect, bottom-band shape (closed 2026-08-10)

- Dock moved from a right-edge margin (`HudDockWidth`) to a bottom band (`HudDockHeight = 0.34f`) — direct
  playtest feedback ("put the control at the bottom... vertical alignment generally"), not a worker decision.
- `GameBootstrap.cs`: `cam.rect = new Rect(0f, ProgramHud.HudDockHeight, 1f, 1f - ProgramHud.HudDockHeight -
  ProgramHud.TopStripHeight)` — board region is full width now, from the top of the dock to the bottom of the
  top strip.
- **Flagged, not yet resolved:** the board's visible aspect ratio changed meaningfully with this move (was
  narrower-than-tall, now wider-than-tall) — `orthographicSize` (5.0) was tuned against the old shape and
  likely needs retuning. Env worker's checkpoint 2 brief has permission to retune it if the framing looks off.
- Verification pending — Editor was open on the main tree (live playtest) both times batchmode was attempted
  after this change; self-reviewed carefully in its place, not yet independently confirmed.

## Ownership reminders this wave

- `Assets/_Project/Board/BoardView.cs`, `DoorLeafFitter.cs`, door import normalize: **Integrator** (door fit).
- `Assets/_Project/UI/**`: closed out — no worker assigned.
- `Assets/_Project/Boot/GameBootstrap.cs`: Integrator-only unless a brief assigns a line.
- `Sim/`, `Net/`, `Timeline/`, `GhostResolver`: core gameplay/networking paused (map/terrain exception already shipped).

## Closed contracts (reference)

### Camera viewport-rect ↔ `ProgramHud`'s dock-width constants, UI side (closed 2026-08-09)

- `feat/ui-component-system` widened `HudDockWidth` 0.30 → 0.34 for cell readability; `TopStripHeight`
  unchanged at 0.08.
- **No `GameBootstrap.cs` rewrite needed** — `cam.rect = new Rect(0f, 0f, 1f - ProgramHud.HudDockWidth, 1f -
  ProgramHud.TopStripHeight)` reads the constant symbolically, confirmed by Integrator before merge, not just
  taken on the worker's word.
- The env worker's `orthographicSize`/`BuildLighting`/`BuildDioramaVolume` changes (checkpoint 1,
  `feat/env-lookfeel-overhaul`) land independently — different lines of the same method, no overlap.

### `IMatchResolver` + `RelayMatchResolver` (Phase 2 first slice, closed 2026-08-09)

- `Assets/_Project/Net/IMatchResolver.cs` / `LocalMatchResolver.cs`: landed (Integrator), frozen, default
  everywhere. `RoundPlayback.ResolveAndArm()` calls through it via `Init`'s `matchResolver` param.
- `Assets/_Project/Net/RelayMatchResolver.cs` + `Relay/LogiCard.Relay/**` (new net8.0 console project, repo
  root, sibling to `Assets/`): landed (worker, `feat/phase2-relay-slice`). Networked `IMatchResolver` over raw
  TCP (4-byte length-prefixed JSON envelopes, `RelayProtocol.cs`), talking to a minimal standalone relay that
  pairs exactly two connections and runs the real shared `GhostResolver` once as authority. Reviewed in depth
  before merge — see `DRAFT_HANDOFF.md` for the two specific correctness checks done (CardData/Modifier
  dead-path verification, connection-order independence via `GhostResolver`'s internal `PawnId` sort).
- Re-verified independently post-merge on `master`: EditMode 110/110, PlayMode 32/32, standalone xUnit 2/2
  (`dotnet test Relay/LogiCard.Relay.sln`).

### `AppFlowController.EnteredMatch` gains `bool viaRelay` + `RoundPlayback.SetMatchResolver` (closed 2026-08-09)

- `AppFlowController.EnteredMatch` is now `Action<bool>` — Find Match fires `true`, Local Play and every
  test/`SliceSceneFixture`'s `BypassToMatch()` fire `false`.
- `RoundPlayback.SetMatchResolver(IMatchResolver)`: swaps the resolver used by the next `ResolveAndArm()`.
  Safe any time before the match's first Lock In.
- `GameBootstrap` subscribes and picks `RelayMatchResolver()` (defaults `127.0.0.1:7777`) for Find Match,
  `LocalMatchResolver` for Local Play — landed live, `RelayMatchResolver` is no longer dormant.
- `RoundPlayback.ResolveAndArmRoutine()`'s resolver pump now catches a failed `MoveNext()` and reports it via
  the existing `OutcomeReported` banner instead of an unhandled exception.
- Verified: EditMode 110/110, PlayMode 32/32. Real two-process network round-trip through a live relay not
  yet human-smoke-tested.

### `ProgramHud`'s HUD-dock layout constants ↔ `GameBootstrap.ConfigureCamera()`'s camera viewport rect (Phase 1, closed 2026-08-09)

- `ProgramHud` landed `HudDockWidth = 0.30f` (right-edge dock), `HudDockHeight = 0f` (not a bottom band),
  `TopStripHeight = 0.08f`; kept `ThumbZoneHeight` as a compile-compat alias (`= HudDockHeight`), locked by
  `ProgramHudLayoutTests`/`AppFlowPlayModeTests`.
- Integrator rewired `GameBootstrap.cs:298-301`:
  `cam.rect = new Rect(0f, 0f, 1f - ProgramHud.HudDockWidth, 1f - ProgramHud.TopStripHeight)`.
- Re-verified post-merge on `master`: EditMode 108/108, PlayMode 32/32.
