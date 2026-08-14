# Core / Integrator — STATUS

**Wave / Day:** Phase 5 — Integrator 2026-08-14
**Branch / worktree:** `master` — clean, Storm card Sim-side landed directly
**Last cross-reviewed:** 2026-08-14 — committed dirty rematch/floors/lighting; wrote C65; opened Map Phase 2
contract; merged Cards' `feat/cards-collection-docs`, Atmosphere's `feat/atmosphere-stylized`, and Map's
`dept/map` Phase 2 (all human-approved); human directly asked for a Storm gear card (chat) — wrote **C67**,
landed the Sim-side myself, opened a 3-way Cards+UI+Atmosphere contract

## Done

- Opened **Bandage HUD-side** in `docs/contracts/CURRENT.md`
- Wrote `BANDAGE_HUD_AGENT_BRIEF.md` into UI worktree (`logiCard-modal-restyle`)
- Committed dirty rematch/floors/lighting (`a419ad4`) — human asked; reclaims `Board*` for Map
- Wrote **C65** to `docs/core/PRODUCT_MEMORY.md` (C53 surface-material amendment, human YES via
  `docs/map/C53_SURFACE_MATERIAL_DECISION.md`)
- Opened **Map Phase 2** in `docs/contracts/CURRENT.md`; refreshed INDEX/DRAFT_HANDOFF/this file
- Merged `feat/cards-collection-docs` → `master` (`4a355dd`), human-approved: C64 `CARD_COLLECTION.md`
  catalog sync + Cards' draft C65 row, renumbered **C66** (collided with the surface-material C65 already
  landed) — deckbuilder sizing (5–8 deck, ≤2 copies), always-have hand, signature extra/always-on/costs
  TR, Reveal at flip. Renumbered consistently across `PRODUCT_MEMORY.md`, the cursor rule, `CARD_COLLECTION.md`,
  `CARD_SYSTEM_OPENS.md`, Cards' STATUS. Dropped 3 stale pre-reorg doc duplicates the branch recreated at
  `docs/` root (docs/cards/ versions are current).
- Merged `feat/atmosphere-stylized` → `master` (`668b162`), human Play-signed ("this is good"): modular
  `BoardWeatherPocket` mood host (bootstrap mounts Storm), Zap lightning tip glued to cloud shelf height,
  storm cloud energize (Yellow Zap rim clusters, random-group pulse). Left the branch's unrelated dirty
  (Floor/Glass mats, ProjectSettings, orphan pack `.meta` deletes, `_Recovery/`, debug screenshots) out of
  the merge per Atmosphere's own STATUS note — still sitting in that worktree, human keep/delete call.
- Merged `dept/map` → `master` (`a76f006`), human Play-signed: `BoardSurfaceMaterials` room
  floors/walls/door-tint/prop-tint moved from photographic-PBR (`BuildWetSurface`) to `Solid()`/nappin-
  Gradient flat family (`BuildWetSurface` kept, just no longer the board-surface default);
  `BoardView.PlaceRoomDressing` now map-aware (Freight Yard/Rail Platform/Vault Complex each get real
  dressing); walls draw as toy fences instead of a brick slab, still presentation-only (no collider/Sim
  change). New `BoardSurfaceMaterialsTests`. Left the branch's `ProjectSettings` define noise and orphan
  pack `.meta` deletes out of the merge per Map's own STATUS note.
- **Storm card Sim-side (C67)**, landed directly (mirrors C63's Bandage Sim-side carve-out — no worker
  needed, small bounded change): `ActionVerb.Storm`, `TapeEventType.StormCast`,
  permissive `GhostResolver.CompileTrack` emission, `RoundPlayback.SyncWeatherToSeconds`/
  `SnapshotWeatherAtArm`/`SetWeatherPocket` (continuous presenter mirroring door sync, double-guarded
  against re-triggering `BoardWeatherPocket.ApplyWeather`'s expensive rebuild on repeated ticks),
  `ResetForNewMatch` reverting weather to Fair on rematch, `GameBootstrap.BuildWeatherPocket()` boot
  mood flipped Storm→Fair so the card is a visible change. `GhostResolverStormTests` (EditMode) added.
  Opened the cross-dept Storm contract (Cards + UI + Atmosphere) in `contracts/CURRENT.md`.

## In progress

- Monitor Cards + UI + Atmosphere Storm-card report-backs, and UI's Bandage HUD report-back
- Batchmode re-verify current tip (new PlayMode/EditMode tests from rematch/relight, Atmosphere's
  `CloudEnergize`, Map's `BoardSurfaceMaterialsTests`, and today's `GhostResolverStormTests` not yet run
  in batchmode — Editor must be closed)
- After Bandage HUD merge: Healed presenter (`PLAYBACK_CONTRACT` §3)
- After Storm HUD lands: PlayMode arm→scrub→rewind coverage for the weather presenter (blocked on UI's
  `TryQueueStorm` existing to arm through)
- Optional, not blocking: `GameBootstrap` lighting/`BuildDioramaVolume` re-grade against Map's new
  saturated flat materials — human already likes the current Play look

## Offers

- Merge Storm card (Cards + UI + Atmosphere) when all three report Ready + green
- Merge UI Bandage HUD when Ready + green
- Map idle — restaff for a prop/dressing follow-up if wanted
