# Draft Handoff — 2026-08-14

**Milestone:** Phase 5 Commercial Art Bar (active). Phase 2 Net paused (Bandage carve-out via C63, extended to C67).  
**Tip:** `master` @ **`24bf725`** — Storm gear (C67) fully landed; Bandage HUD-side (C63) landed same UI merge; **Map Phase 2** merged (`a76f006` ← `dept/map` `565583f`). Prior combined batchmode green @ `7213d98` — **not independently re-run** on current tip (UI reported 166/166 EditMode, 51/51 PlayMode on their worktree only).  
**Ops:** Atmosphere / Cards / Character / UI / **Map** + Integrator. **No department coding-hot** — Storm wave + Map Phase 2 closed.  
**Read first next session:** this file → `PARALLEL_OPS.md` → `departments/INDEX.md` → `contracts/CURRENT.md` → `PLAYBACK_CONTRACT.md` if touching Execute / Healed.

## Live folders

| Seat | Folder | Tip / state |
|------|--------|-------------|
| Integrator | `logiCard` | `master` @ `24bf725` — clean enough; leftover ProjectSettings / ExplosiveLLC noise uncommitted |
| Atmosphere | `logiCard-atmosphere-stylized` | Storm DoD merged. **Uncommitted Sunny weather mood** held back — human decision pending |
| Cards | `logiCard-cards-collection` | @ `3e77925` — merged, idle; Storm numerics + C68 packaging await human lock |
| Character | `logiCard-char-select-motion` | 4 briefs; needs human answers |
| UI | `logiCard-modal-restyle` | @ `31c55e8` — Bandage + Storm HUD merged, idle; icon-catalog dirty uncommitted |
| Map | `logiCard-map` | `dept/map` @ **`565583f`** — Phase 2 **merged** (`a76f006`), idle |
| (retire OK) | `logiCard-gear-bandage-sim` | merged @ `0b11031` |

## Implemented

- **C62/C63** Bandage (Sim + HUD). **C64/C66** hybrid cards + deckbuilder sizing. **C68** Character 8-card packaging (docs; lock pending where noted).
- **Rematch reset + sunny relight**; **C65** + **Map Phase 2** — flat/toon floors, Gradient prop/door tint, map-aware dressing, toy fence walls; human look `image copy 15.png`.
- **Atmosphere** storm Zap tip + cloud energize (human Play-signed).
- **C67 Storm gear — fully landed:** Sim (`ActionVerb.Storm`, `StormCast`, Fair boot + SyncWeather presenter), Cards catalog/brief, UI arm/place with Bandage, Atmosphere ApplyWeather idempotency. Storm once-per-match is HUD-only this wave; transition polish deferred.

## Verification

- Historical combined green: `7213d98` EditMode 149/149, PlayMode 48/48.
- UI own-worktree after Bandage+Storm: EditMode 166/166, PlayMode 51/51 — **not** Integrator re-run on master tip.
- Map Phase 2: human Play signed; batchmode not claimed on Map branch.

## Still unfinished

- **Independent batchmode** on current `master` tip (Editor closed on `D:\projects\Game\logiCard`).
- **Healed presenter** — `TapeEventType.Healed` in `RoundPlayback` + `PLAYBACK_CONTRACT` §3 (unblocked now that Bandage HUD merged).
- **Atmosphere Sunny mood** — uncommitted in atmosphere worktree; land vs drop (boot mood must stay Fair-reconciled).
- **Storm numerics lock** — Cards recommends real TR cost + 1×/match; needs human → C-row; optional charge-carry like Bandage.
- Optional lighting/`BuildDioramaVolume` re-grade vs Map materials (human already likes look).
- Character briefs unanswered; Interact station; Adrenaline PLAYBACK; Phase 2 Net paused.
- Older unmonitored: door tape Open second; south-edge Move-click; zoom-fill/soft-rain/reflections.

## Tomorrow

1. Integrator: batchmode on tip; Healed presenter; ask human about Atmosphere Sunny + Storm numerics.
2. Departments idle unless restaffed — UI may resume icon-catalog; Character blocked on answers.

## Blockers / notes

- Map worktree still has local dirty noise (ProjectSettings UNITY_POST_PROCESSING_STACK_V2, orphan pack `.meta` deletes, `image copy 13` pre-sync screenshot) — **do not** fold into feature merges.
- Main: `ExplosiveLLC/`, same ProjectSettings define noise — human keep/delete.
- No push unless asked.
