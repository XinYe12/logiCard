# Cross-Dept Contracts — Current Wave

**Wave:** Look-and-feel + UI, started 2026-08-09. Core-gameplay/networking work explicitly paused by the
human until this wave lands — see `SCHEDULE.md`'s Cadence section. **UI side landed and merged three times**
(`feat/ui-component-system`, a same-session dock move right-edge → bottom band, then `feat/ui-dock-polish`'s
ultrawide-overflow fix + dialog tightening) — `Assets/_Project/UI/**` is closed out again, no worker
assigned. **Environment side: all three checkpoints merged** — checkpoint 1 (2026-08-09, weather/lighting),
checkpoint 2 (2026-08-10, real Poly Haven PBR board-surface textures), checkpoint 3 (2026-08-10, Quaternius
door/prop meshes replacing tinted boxes, per C54). **Rendering: URP post-processing gap closed** — real
Volume Profile, SSAO, MSAA, soft shadows all landed 2026-08-10, plus a C54 photo-mode stretch goal
(`PhotoModeController`, `F9`). **Integrator also landed, in the main tree directly:** camera rotation
(`BoardCameraRig`, smooth right-drag), an interim cloud-size fix, and dropping stepped stop-motion pawn
animation for smooth interpolation (**C55**, human-confirmed as an improvement). **Character-model
research** landed real findings (genre-clash outfit, flat materials); human picked the minimal fix
(**C56**), landed as **Scout re-outfitted Adventurer→Worker** within the existing CC0 pack. **Wet-surface
reflections**: first pass (Reflection Probes + retune) shipped but a human screenshot showed no visible
change — root-caused directly (probe `clearFlags` never set, rendering the wrong environment) and fixed.
**Clouds**: replaced primitive spheres with real textured particle clouds (Kenney CC0 sprite atlas). Both
worker slots closed again after C60/C61 (2026-08-11). **Still open:** human sighted pass on C60 vibrancy
(runtime grade actually warm now) + C61 scroll zoom feel + earlier reflection/clouds/Scout outfit items.
**Updated:** 2026-08-14 by Integrator — dirty rematch/floors/lighting committed (`master`), clearing the
`Board*` conflict; **Map Phase 2 contract opened** (C65, human-confirmed YES on the C53 surface-material
amendment). Bandage HUD-side contract still open (C63). Gear pause carve-out (C63) and Map's C57 carve-out
(map/terrain) still apply; Net / other Sim stay paused.
**Rule:** Only Integrator edits this file after a merge. Workers implement against the frozen signatures
below.

## Frozen contracts this wave

### Map Phase 2 — board surface material swap (open 2026-08-14 — **Map seat** on `logiCard-map`)

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

### Bandage HUD-side (open 2026-08-14 — **UI seat** on `logiCard-modal-restyle`; brief `BANDAGE_HUD_AGENT_BRIEF.md`)

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
