# Draft Handoff — 2026-08-14

**Milestone:** Phase 5 Commercial Art Bar (active). Phase 2 Net paused (Bandage carve-out via C63).  
**Tip:** `master` — Storm gear card Sim-side (C67) landed on top of Map Phase 2, rematch/relight, C65/C66 docs, and the Atmosphere storm merge from earlier today. Prior combined batchmode green @ `7213d98` (EditMode 149/149, PlayMode 48/48) — **not re-run since**; nothing landed today is independently batchmode-verified.  
**Ops:** Atmosphere / Cards / Character / UI / **Map** + Integrator (`PARALLEL_OPS.md`). **Coding-hot today: Cards + UI + Atmosphere together on the Storm card** (deliberate 3-way exception to the ≤2 default, human-directed — see `contracts/CURRENT.md`).  
**Read first next session:** this file → `PARALLEL_OPS.md` → `departments/INDEX.md` → `contracts/CURRENT.md` → **Storm card contract** + Bandage HUD contract → `PLAYBACK_CONTRACT.md` if touching Execute.

## Live folders

| Seat | Folder | Tip / state |
|------|--------|-------------|
| Integrator | `logiCard` | `master` — clean (Storm Sim-side just landed) |
| Atmosphere | `logiCard-atmosphere-stylized` | **Storm card**: confirm `ApplyWeather`/`ClearWeather` idempotency + lighting-dim round-trip under repeated Playback scrubbing |
| Cards | `logiCard-cards-collection` | **Storm card**: `CardId.Storm`, catalog entry, numerics recommendation (TR placeholder, once-per-match?) |
| Character | `logiCard-char-select-motion` | `feat/char-select-motion` — 4 briefs; needs human answers |
| UI | `logiCard-modal-restyle` | `feat/modal-restyle` — **Bandage HUD + Storm card HUD wiring, same pass** (`BANDAGE_HUD_AGENT_BRIEF.md` + Storm contract) |
| Map | `logiCard-map` | `dept/map` — Phase 2 merged; human signed the look (`screenshots/image copy 15.png`); idle unless restaffed |
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
- **C67 / Storm card, Sim-side (Integrator, mirrors C63's Bandage carve-out)**: `ActionVerb.Storm`, `TapeEventType.StormCast`, permissive `GhostResolver` emission, `RoundPlayback.SyncWeatherToSeconds` — a continuous presenter (same shape as door sync) that drives `BoardWeatherPocket.ApplyWeather`, guarded so it never restarts the expensive cloud/rain/lightning rebuild on a repeated scrubber tick. `GameBootstrap` now boots the board on **Fair**, not Storm (was Storm from the earlier Atmosphere merge) so casting the card is a visible change. Rematch resets weather to Fair. `GhostResolverStormTests` (EditMode) added. Cross-dept contract opened for **Cards + UI + Atmosphere** — see `contracts/CURRENT.md`.

## Verification

- Combined master @ `7213d98`: EditMode 149/149, PlayMode 48/48.
- Everything landed since (rematch/relight, C65/C66 docs, Atmosphere storm, Map Phase 2, Storm Sim-side): **not** re-batchmoded. New coverage added today and unrun in batchmode: `MatchClockTests.Reset`, `RoundPlaybackPlayModeTests.FreshMatchClearsCarriedDeathAndReturnsPawnsToSpawn`, `BoardWeatherPocketPlayModeTests` `CloudEnergize` cases, `BoardSurfaceMaterialsTests`, `GhostResolverStormTests`.
- Atmosphere storm look + Map Phase 2 look: both **human Play-signed**, neither batchmode-verified.
- Bandage HUD: **in flight on UI worktree** — no merge yet.
- Storm card HUD wiring: **not started** — contract just opened.

## Still unfinished

- **Batchmode run on current tip** — nothing landed today has been verified in batchmode; Editor must be closed on this exact path first.
- **Storm card** (open contract, Cards + UI + Atmosphere staffed): Cards adds `CardId.Storm` + catalog + numerics brief; UI adds `PawnProgram.TryQueueStorm` + HUD dock + arm/place; Atmosphere confirms `ApplyWeather` is safe under repeated Playback calls. Detail: `contracts/CURRENT.md`.
- **Bandage HUD-side** (open contract, UI staffed): dock `GearHandView` → `ProgramHud`, timeline place, 3 legality gates. Brief: UI worktree `BANDAGE_HUD_AGENT_BRIEF.md`.
- **Healed presenter** (Integrator after HUD merge): `TapeEventType.Healed` in `RoundPlayback` + `PLAYBACK_CONTRACT` §3.
- **Optional**: `GameBootstrap.BuildLighting`/`BuildDioramaVolume` re-grade against Map's new saturated flat materials — Map flagged it as optional polish, not required; human already likes the current Play look, so treat as a "if wanted" follow-up, not a blocker.
- **Character** Sim contracts blocked on brief answers + carve-out.
- Interact needs station; Adrenaline real effect needs PLAYBACK redesign; Phase 2 Net paused.
- Older unmonitored: door tape Open second; south-edge Move-click; zoom-fill/soft-rain/reflections.
- Atmosphere worktree still carries unrelated dirty (Floor/Glass mats, ProjectSettings, orphan pack `.meta` deletes, `_Recovery/`, debug screenshots) — left out of the merge on purpose; human keep/delete call.
- Map did not commit `ProjectSettings.asset` scripting-define noise or orphan `.unitypackage.meta` deletes either — same bucket as the main tree's `ExplosiveLLC`/`ProjectSettings` leftovers below.

## Today / next

1. **Cards / UI / Atmosphere** work the Storm card contract in parallel (`contracts/CURRENT.md`) — no file overlap between the three slices.
2. **UI** also codes Bandage HUD per its own contract + brief, same worktree/pass.
3. Integrator: run batchmode on current tip; merge Storm card + Bandage HUD together when all three report Ready + green.
4. Character: idle until human answers briefs. Map: idle, merged — restaff only if a follow-up is wanted.

## Blockers / notes

- Main Editor lock → batchmode on other worktrees; avoid multiple Unity instances.
- Capacity: Cards + UI + Atmosphere hot together this wave (Storm card, deliberate exception); Character/Map stay idle meanwhile.
- C64 does **not** unlock deckbuilder coding — OPENs parked on C64 row.
- Untracked junk: `Assets/ExplosiveLLC/`, screenshot copies — human keep/delete. `ProjectSettings.asset` also has an uncommitted scripting-define change (`UNITY_POST_PROCESSING_STACK_V2` across all platforms) that looks like a side effect of the untracked `ExplosiveLLC` import — left uncommitted pending that decision. Same noise showed up independently in the Atmosphere and Map worktrees; nobody has committed it anywhere.
- No push unless asked.
