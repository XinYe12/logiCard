# Draft Handoff — 2026-08-14

**Milestone:** Phase 5 Commercial Art Bar (active). Phase 2 Net paused (Bandage carve-out via C63).  
**Tip:** `master` @ **`e07be61`** (docs reorg) atop C64 `dcffe23`. Prior combined batchmode green @ `7213d98` (EditMode 149/149, PlayMode 48/48) — not re-run after docs-only. Plus **dirty Integrator tree** (rematch/floors/lighting — uncommitted).  
**Ops:** Atmosphere / Cards / Character / UI + Integrator (`PARALLEL_OPS.md`). **Coding-hot today: UI (Bandage HUD).** Prefer ≤2 coding-hot.  
**Read first next session:** this file → `PARALLEL_OPS.md` → `departments/INDEX.md` → `contracts/CURRENT.md` → Bandage HUD contract → `PLAYBACK_CONTRACT.md` if touching Execute.

## Live folders

| Seat | Folder | Tip / state |
|------|--------|-------------|
| Integrator | `logiCard` | `master` @ `e07be61` + dirty rematch/floors/lighting |
| Atmosphere | `logiCard-atmosphere-stylized` | `feat/atmosphere-stylized` @ **`fac245a`** + dirty weather polish — await human Play |
| Cards | `logiCard-cards-collection` | `feat/cards-collection-docs` @ **`11b18c2`** — C64 catalog done; Flashbang paused |
| Character | `logiCard-char-select-motion` | `feat/char-select-motion` @ **`3d1d799`** — 4 briefs; needs human answers |
| UI | `logiCard-modal-restyle` | `feat/modal-restyle` @ **`cc13209`** — **Bandage HUD in progress** (`BANDAGE_HUD_AGENT_BRIEF.md`) |
| (retire OK) | `logiCard-gear-bandage-sim` | merged @ `0b11031` |

## Implemented

- **C62** first-wave gear rules; **C63** Bandage numerics (3s TR, 1×/Character/match, HUD-gated not-mid-Sprint).
- **Bandage Sim-side** merged `4e6bb66` — `ActionVerb.Bandage`, `Healed`, `BandageCharge` carry.
- **UI modal + `GearHandView` scaffold** merged `7213d98` — **HUD dock/wire is today’s UI job**.
- **C64** (`dcffe23`): hybrid long-term card system; transitional full-hand for shipping staples.
- **Docs reorg** `e07be61` — department folders; cross-links fixed.
- Atmosphere: fair clay weather look advanced on branch (not Integrator-merged).

## Verification

- Combined master @ `7213d98`: EditMode 149/149, PlayMode 48/48.
- `dcffe23` / `e07be61`: docs — not re-batchmoded.
- Rematch/floors dirty: **not** batchmode-green.
- Bandage HUD: **in flight on UI worktree** — no merge yet.

## Still unfinished

- **Bandage HUD-side** (open contract, UI staffed): dock `GearHandView` → `ProgramHud`, timeline place, 3 legality gates. Brief: UI worktree `BANDAGE_HUD_AGENT_BRIEF.md`.
- **Healed presenter** (Integrator after HUD merge): `TapeEventType.Healed` in `RoundPlayback` + `PLAYBACK_CONTRACT` §3.
- **Dirty rematch/floors/lighting** on main — commit when asked; includes `BandageCharge` reset on fresh match.
- **Atmosphere** merge after human look (`fac245a`+ dirty).
- **Character** Sim contracts blocked on brief answers + carve-out.
- Interact needs station; Adrenaline real effect needs PLAYBACK redesign; Phase 2 Net paused.
- Older unmonitored: door tape Open second; south-edge Move-click; zoom-fill/soft-rain/reflections.

## Today / next

1. **UI** codes Bandage HUD per contract + brief (not Integrator on main).
2. Integrator: monitor UI; refresh tips done; merge HUD when Ready → then Healed presenter; commit rematch/floors when asked.
3. Atmosphere: human Play → merge when cleared.
4. Cards/Character: idle unless staffed for docs-only.

## Blockers / notes

- Main Editor lock → batchmode on other worktrees; avoid multiple Unity instances.
- Capacity: UI hot; do not also hot-code Atmosphere without a look gate.
- C64 does **not** unlock deckbuilder coding — OPENs parked on C64 row.
- Untracked junk: `Assets/ExplosiveLLC/`, screenshot copies — human keep/delete.
- No push unless asked.
