# Draft Handoff — 2026-08-13

**Milestone:** Phase 5 Commercial Art Bar (active). Phase 2 Net paused (Bandage carve-out via C63).  
**Tip:** `master` @ **`dcffe23`** (C64 hybrid card-system promoted + Cards detail docs on master). Prior combined batchmode green was at `7213d98` (EditMode 149/149, PlayMode 48/48) — **not re-run after `dcffe23`** (docs-only). Plus **dirty Integrator tree** (rematch/floors/lighting — uncommitted).  
**Ops:** Atmosphere / Cards / Character / UI + Integrator (`PARALLEL_OPS.md`). Prefer ≤2 coding-hot.  
**Read first next session:** this file → `PARALLEL_OPS.md` → `departments/INDEX.md` → `contracts/CURRENT.md` → `CARD_SYSTEM_MODEL_COMPARISON.md` / C64 → `PLAYBACK_CONTRACT.md` if touching Execute.

## Live folders

| Seat | Folder | Tip / state |
|------|--------|-------------|
| Integrator | `logiCard` | `master` @ `dcffe23` + dirty rematch/floors/lighting |
| Atmosphere | `logiCard-atmosphere-stylized` | `feat/atmosphere-stylized` @ **`af7b2e5`** (fair clay cloud bank locked) + **dirty** polish tree |
| Cards | `logiCard-cards-collection` | `feat/cards-collection-docs` @ **`dc631ce`** — C64 done; Flashbang paused; clean except stray screenshot |
| Character | `logiCard-char-select-motion` | `feat/char-select-motion` @ `dec54e7` — 4 ability briefs; needs human answers |
| UI | `logiCard-modal-restyle` | `feat/modal-restyle` @ **`8f4b406`** (font/UI catalog) + dirty STATUS / ui-collection refs — Bandage HUD **not** started |
| (retire OK) | `logiCard-gear-bandage-sim` | merged @ `0b11031` |

## Implemented

- **C62** first-wave gear rules; **C63** Bandage numerics (3s TR, 1×/Character/match, HUD-gated not-mid-Sprint).
- **Bandage Sim-side** merged `4e6bb66` — `ActionVerb.Bandage`, `Healed`, `BandageCharge` carry; contract in `contracts/CURRENT.md`.
- **UI modal + `GearHandView` scaffold** merged `7213d98` — **not wired into `ProgramHud`**.
- **C64** (`dcffe23`): long-term card system = **hybrid** (signature cards + shared deckbuilding library). Amends C18/C62. Hands/decks hidden; library+signatures free (C47). Shipping staples stay on **transitional full-hand**. Detail: `docs/CARD_SYSTEM_MODEL_COMPARISON.md`. Cards branch conversation → `dc631ce`.
- **Character mandate shift:** owns abilities/attrs (not Char Select UI). Four docs-only briefs at `dec54e7`. Finding: **C25 Agility penalties authored but unread by `PawnProgram`**.
- **UI mandate:** owns all screen presentation; research mandate noted — human later preferred **Bandage HUD coding first** over a long research essay.
- Atmosphere: fair clay weather look locked @ `af7b2e5` (not Integrator-reviewed for merge).

## Verification

- Combined master @ `7213d98`: EditMode 149/149, PlayMode 48/48 (ephemeral verify worktree; removed).
- `dcffe23` C64/docs: **not** re-batchmoded (docs-only).
- Rematch/floors dirty: **not** batchmode-green.
- Atmosphere / UI tips moved past INDEX text — INDEX tips stale vs `af7b2e5` / `8f4b406` / `dc631ce` / `dcffe23`.

## Still unfinished

- **Bandage HUD-side** (open contract): dock `GearHandView` → `ProgramHud`, timeline place, 3 legality gates. Natural UI job; human-directed **priority coding slice**.
- **Healed presenter** (Integrator after HUD): `TapeEventType.Healed` in `RoundPlayback` + `PLAYBACK_CONTRACT` §3.
- **Dirty rematch/floors/lighting** on main — commit when asked; includes `BandageCharge` reset fix.
- **Atmosphere** merge after human look (`af7b2e5`+ dirty).
- **Character** Sim contracts blocked on brief answers + explicit carve-out per ability.
- **Cards:** `CARD_COLLECTION.md` rewritten for C64 dual-horizon (branch, not yet merged to master). Live menu: `CARD_SYSTEM_OPENS.md` (deck size / draw / signature / Reveal). Flashbang brief still paused.
- Interact needs station; Adrenaline real effect needs PLAYBACK redesign; Phase 2 Net paused.
- Older unmonitored: door tape Open second; south-edge Move-click; zoom-fill/soft-rain/reflections.

## Tomorrow

1. **UI codes Bandage HUD** against `contracts/CURRENT.md` (skip broad UI-framework essay unless blocked).
2. Integrator: refresh INDEX tips; merge HUD when Ready → then Healed presenter; commit rematch/floors when asked.
3. Atmosphere: human Play @ latest tip → merge when cleared (clean pack `.meta` noise first).
4. Cards (docs-only if staffed): C64 catalog rewrite **or** short OPEN decision menu — not Flashbang architecture, not deckbuilder code.
5. Character: idle until human answers the four briefs’ open questions.

## Blockers / notes

- Main Editor lock → batchmode on other worktrees; avoid multiple Unity instances (Package Manager contention).
- Capacity ≤2 coding-hot (suggested: UI + Atmosphere if looking art).
- C64 does **not** unlock deckbuilder coding yet — OPENs parked on C64 row.
- Intra-match wound/charge carry (C33/C63); only **new match** clears death + BandageCharge (dirty rematch).
- Untracked junk: `Assets/ExplosiveLLC/`, `docs/image.png`, screenshot copies — human keep/delete.
- No push unless asked.
