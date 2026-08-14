# Draft Handoff — 2026-08-14

**Milestone:** Phase 5 Commercial Art Bar (active). Phase 2 Net paused (Bandage carve-out via C63).  
**Tip:** `master` @ **`4a355dd`** — rematch reset + sunny relight + C65 (surface-material amendment) + Cards' C64 catalog/C66 deckbuilder-sizing merge, all committed (was dirty). Prior combined batchmode green @ `7213d98` (EditMode 149/149, PlayMode 48/48) — **not re-run since**; nothing since is independently batchmode-verified.  
**Ops:** Atmosphere / Cards / Character / UI / **Map** + Integrator (`PARALLEL_OPS.md`). **Coding-hot today: UI (Bandage HUD), Map (Phase 2, just unblocked).** Prefer ≤2 coding-hot.  
**Read first next session:** this file → `PARALLEL_OPS.md` → `departments/INDEX.md` → `contracts/CURRENT.md` → Bandage HUD + Map Phase 2 contracts → `PLAYBACK_CONTRACT.md` if touching Execute.

## Live folders

| Seat | Folder | Tip / state |
|------|--------|-------------|
| Integrator | `logiCard` | `master` @ `4a355dd` — clean |
| Atmosphere | `logiCard-atmosphere-stylized` | `feat/atmosphere-stylized` @ **`fac245a`** + dirty weather polish — await human Play |
| Cards | `logiCard-cards-collection` | `feat/cards-collection-docs` @ **`8b5e86d`** — **merged to master (`4a355dd`)**; idle, retire OK unless restaffed |
| Character | `logiCard-char-select-motion` | `feat/char-select-motion` @ **`3d1d799`** — 4 briefs; needs human answers |
| UI | `logiCard-modal-restyle` | `feat/modal-restyle` @ **`cc13209`** — **Bandage HUD in progress** (`BANDAGE_HUD_AGENT_BRIEF.md`) |
| Map | `logiCard-map` | `dept/map` @ `d632d3b` — **Phase 2 contract open**, unblocked (Board* reclaim done, C65 written) |
| (retire OK) | `logiCard-gear-bandage-sim` | merged @ `0b11031` |

## Implemented

- **C62** first-wave gear rules; **C63** Bandage numerics (3s TR, 1×/Character/match, HUD-gated not-mid-Sprint).
- **Bandage Sim-side** merged `4e6bb66` — `ActionVerb.Bandage`, `Healed`, `BandageCharge` carry.
- **UI modal + `GearHandView` scaffold** merged `7213d98` — **HUD dock/wire is today’s UI job**.
- **C64** (`dcffe23`): hybrid long-term card system; transitional full-hand for shipping staples.
- **Docs reorg** `e07be61` — department folders; cross-links fixed.
- **Rematch reset + sunny relight** `a419ad4` — `GameBootstrap.RequestFreshMatch`/`RoundPlayback.ResetForNewMatch`/`MatchClock.Reset` clear wound/death/door/gear carry on a fresh match; board lighting/grade moved off the wet-dusk storm key toward the bright midday Zelda-reference look; floor tints follow (Yard/Flank cool asphalt, Hall wood, Vault marble-concrete).
- **C65**: C53 amended — board *surface materials* (floors/walls/door-tint/prop-tint) move flat/toon, human-confirmed YES (`docs/map/C53_SURFACE_MATERIAL_DECISION.md`). Geometry density and weather/atmosphere stay C53-as-written. Unblocks Map Phase 2 (contract opened below).
- **C66**: deckbuilder sizing + hand/Reveal rules, closes C64's parked OPENs — 5–8 deck, ≤2 copies, always-have hand, signature extra/always-on/costs TR, Reveal at flip. `feat/cards-collection-docs` merged `4a355dd` (Q1–Q8 defaults, `CARD_COLLECTION.md`/`CARD_SYSTEM_OPENS.md` sync); Flashbang brief stays paused.
- Atmosphere: fair clay weather look advanced on branch (not Integrator-merged).

## Verification

- Combined master @ `7213d98`: EditMode 149/149, PlayMode 48/48.
- Everything since (`dcffe23` → `4a355dd`, incl. rematch reset + relight, C65/C66 docs merges): **not** re-batchmoded — `a419ad4` adds a new PlayMode test (`FreshMatchClearsCarriedDeathAndReturnsPawnsToSpawn`) and a new EditMode test (`MatchClock.Reset`) that have not been run in batchmode yet; run before trusting green.
- Bandage HUD: **in flight on UI worktree** — no merge yet.
- Map Phase 2: not started — contract just opened.

## Still unfinished

- **Batchmode run on `4a355dd`** — the rematch/relight commit's new tests haven't been verified in batchmode; Editor must be closed on this exact path first.
- **Bandage HUD-side** (open contract, UI staffed): dock `GearHandView` → `ProgramHud`, timeline place, 3 legality gates. Brief: UI worktree `BANDAGE_HUD_AGENT_BRIEF.md`.
- **Map Phase 2** (open contract, Map staffed): `BoardSurfaceMaterials`/`BoardView` flat/toon material swap per C65 + `MAP_PRESENTATION_STANDARD.md` §5. Human screenshot check before calling done.
- **Healed presenter** (Integrator after HUD merge): `TapeEventType.Healed` in `RoundPlayback` + `PLAYBACK_CONTRACT` §3.
- **Atmosphere** merge after human look (`fac245a`+ dirty).
- **Character** Sim contracts blocked on brief answers + carve-out.
- Interact needs station; Adrenaline real effect needs PLAYBACK redesign; Phase 2 Net paused.
- Older unmonitored: door tape Open second; south-edge Move-click; zoom-fill/soft-rain/reflections.

## Today / next

1. **UI** codes Bandage HUD per contract + brief (not Integrator on main).
2. **Map** starts Phase 2 per contract (`docs/contracts/CURRENT.md`) — material swap only, no geometry/Sim touch.
3. Integrator: run batchmode on `4a355dd`; monitor UI + Map; merge HUD/Map Phase 2 when Ready + green.
4. Atmosphere: human Play → merge when cleared.
5. Cards: idle, merged — restaff only for deckbuilder systems brief or Flashbang re-derive. Character: idle until human answers briefs.

## Blockers / notes

- Main Editor lock → batchmode on other worktrees; avoid multiple Unity instances.
- Capacity: UI + Map hot; do not also hot-code Atmosphere without a look gate.
- C64 does **not** unlock deckbuilder coding — OPENs parked on C64 row.
- Untracked junk: `Assets/ExplosiveLLC/`, screenshot copies — human keep/delete. `ProjectSettings.asset` also has an uncommitted scripting-define change (`UNITY_POST_PROCESSING_STACK_V2` across all platforms) that looks like a side effect of the untracked `ExplosiveLLC` import, not part of today's rematch/lighting work — left uncommitted pending that decision.
- No push unless asked.
