# Draft Handoff — 2026-08-14

**Milestone:** Phase 5 Commercial Art Bar (active). Phase 2 Net paused (Bandage carve-out via C63).  
**Tip:** `master` — Map Phase 2 (flat/toon floors + toy fence walls, C65) merged in, on top of the rematch/relight, C65/C66 docs, and Atmosphere storm merges from earlier today. Prior combined batchmode green @ `7213d98` (EditMode 149/149, PlayMode 48/48) — **not re-run since**; nothing landed today is independently batchmode-verified.  
**Ops:** Atmosphere / Cards / Character / UI / **Map** + Integrator (`PARALLEL_OPS.md`). **Coding-hot today: UI (Bandage HUD).** Prefer ≤2 coding-hot.  
**Read first next session:** this file → `PARALLEL_OPS.md` → `departments/INDEX.md` → `contracts/CURRENT.md` → Bandage HUD contract → `PLAYBACK_CONTRACT.md` if touching Execute.

## Live folders

| Seat | Folder | Tip / state |
|------|--------|-------------|
| Integrator | `logiCard` | `master` — clean (Map Phase 2 just merged) |
| Atmosphere | `logiCard-atmosphere-stylized` | **merged** (storm Zap tip + cloud energize); worktree still has unrelated dirty (mats/ProjectSettings/`_Recovery`) left out on purpose |
| Cards | `logiCard-cards-collection` | **merged** (C64 catalog sync + C66 deckbuilder sizing); idle, retire OK unless restaffed |
| Character | `logiCard-char-select-motion` | `feat/char-select-motion` — 4 briefs; needs human answers |
| UI | `logiCard-modal-restyle` | `feat/modal-restyle` — **Bandage HUD in progress** (`BANDAGE_HUD_AGENT_BRIEF.md`) |
| Map | `logiCard-map` | `dept/map` — **Phase 2 merged**; human signed the look (`screenshots/image copy 15.png`); idle unless restaffed |
| (retire OK) | `logiCard-gear-bandage-sim` | merged @ `0b11031` |

## Implemented

- **C62** first-wave gear rules; **C63** Bandage numerics (3s TR, 1×/Character/match, HUD-gated not-mid-Sprint).
- **Bandage Sim-side** merged `4e6bb66` — `ActionVerb.Bandage`, `Healed`, `BandageCharge` carry.
- **UI modal + `GearHandView` scaffold** merged `7213d98` — **HUD dock/wire is today’s UI job**.
- **C64**: hybrid long-term card system; transitional full-hand for shipping staples.
- **Rematch reset + sunny relight** — `GameBootstrap.RequestFreshMatch`/`RoundPlayback.ResetForNewMatch`/`MatchClock.Reset` clear wound/death/door/gear carry on a fresh match; board lighting/grade moved off the wet-dusk storm key toward a bright midday look.
- **C65**: C53 amended — board *surface materials* move flat/toon, human-confirmed YES (`docs/map/C53_SURFACE_MATERIAL_DECISION.md`). Geometry density and weather/atmosphere stay C53-as-written.
- **C66**: deckbuilder sizing + hand/Reveal rules, closes C64's parked OPENs — 5–8 deck, ≤2 copies, always-have hand, signature extra/always-on/costs TR, Reveal at flip.
- **Atmosphere storm weather**: modular `BoardWeatherPocket` mood host (Storm mounted at boot); Zap lightning tip glued to the cloud shelf height; storm cloud energize (Yellow Zap rim clusters, random-group pulse). Human Play-signed.
- **Map Phase 2 (C65 implementation)**: `BoardSurfaceMaterials` moved room floors/walls/door-tint/prop-tint from `BuildWetSurface()` photographic-PBR to the `Solid()`/nappin-Gradient flat family (`BuildWetSurface` kept, just no longer the board-surface default); `BoardView.PlaceRoomDressing` is now map-aware (Freight Yard / Rail Platform / Vault Complex each get real in-room dressing); walls draw as toy fences (posts + rails + cream panel) instead of a coral brick slab, still presentation-only (Sim wall segments/colliders unchanged — no physics collider, same as before). EditMode `BoardSurfaceMaterialsTests` added. Human Play-signed (`screenshots/image copy 15.png`).

## Verification

- Combined master @ `7213d98`: EditMode 149/149, PlayMode 48/48.
- Everything landed since (rematch/relight, C65/C66 docs, Atmosphere storm, Map Phase 2): **not** re-batchmoded. New coverage added today and unrun in batchmode: `MatchClockTests.Reset`, `RoundPlaybackPlayModeTests.FreshMatchClearsCarriedDeathAndReturnsPawnsToSpawn`, `BoardWeatherPocketPlayModeTests` `CloudEnergize` cases, `BoardSurfaceMaterialsTests`.
- Atmosphere storm look + Map Phase 2 look: both **human Play-signed**, neither batchmode-verified.
- Bandage HUD: **in flight on UI worktree** — no merge yet.

## Still unfinished

- **Batchmode run on current tip** — nothing landed today has been verified in batchmode; Editor must be closed on this exact path first.
- **Bandage HUD-side** (open contract, UI staffed): dock `GearHandView` → `ProgramHud`, timeline place, 3 legality gates. Brief: UI worktree `BANDAGE_HUD_AGENT_BRIEF.md`.
- **Healed presenter** (Integrator after HUD merge): `TapeEventType.Healed` in `RoundPlayback` + `PLAYBACK_CONTRACT` §3.
- **Optional**: `GameBootstrap.BuildLighting`/`BuildDioramaVolume` re-grade against Map's new saturated flat materials — Map flagged it as optional polish, not required; human already likes the current Play look, so treat as a "if wanted" follow-up, not a blocker.
- **Character** Sim contracts blocked on brief answers + carve-out.
- Interact needs station; Adrenaline real effect needs PLAYBACK redesign; Phase 2 Net paused.
- Older unmonitored: door tape Open second; south-edge Move-click; zoom-fill/soft-rain/reflections.
- Atmosphere worktree still carries unrelated dirty (Floor/Glass mats, ProjectSettings, orphan pack `.meta` deletes, `_Recovery/`, debug screenshots) — left out of the merge on purpose; human keep/delete call.
- Map did not commit `ProjectSettings.asset` scripting-define noise or orphan `.unitypackage.meta` deletes either — same bucket as the main tree's `ExplosiveLLC`/`ProjectSettings` leftovers below.

## Today / next

1. **UI** codes Bandage HUD per contract + brief (not Integrator on main).
2. Integrator: run batchmode on current tip; merge HUD when Ready + green; optional lighting re-pass if wanted.
3. Character: idle until human answers briefs. Cards/Atmosphere/Map: idle, merged — restaff only if a follow-up is wanted.

## Blockers / notes

- Main Editor lock → batchmode on other worktrees; avoid multiple Unity instances.
- Capacity: UI hot; do not also hot-code another department without a look/contract gate.
- C64 does **not** unlock deckbuilder coding — OPENs parked on C64 row.
- Untracked junk: `Assets/ExplosiveLLC/`, screenshot copies — human keep/delete. `ProjectSettings.asset` also has an uncommitted scripting-define change (`UNITY_POST_PROCESSING_STACK_V2` across all platforms) that looks like a side effect of the untracked `ExplosiveLLC` import — left uncommitted pending that decision. Same noise showed up independently in the Atmosphere and Map worktrees; nobody has committed it anywhere.
- No push unless asked.
