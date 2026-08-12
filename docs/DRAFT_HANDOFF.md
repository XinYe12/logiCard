# Draft Handoff — 2026-08-12

**Milestone:** Phase 5 Commercial Art Bar (active, top priority). Phase 2 Net paused.  
**Tip:** `5b2ee7c` on `master` (ahead of origin) — **atmosphere merged** — + **dirty Integrator tree** (rematch/floors/lighting, not committed). Worker slots **0/2** coding. UI branches still awaiting Play+merge.  
**Read first next session:** this file → `docs/PLAYBACK_CONTRACT.md` → `docs/PARALLEL_OPS.md` → `docs/departments/INDEX.md`.

## Implemented

**Committed tip (`5b2ee7c` merge of `feat/atmosphere-stylized` @ `0acd909`):**
- LA-style CloudAtlas bank + rim mist; FogGround/RainMist out of live pocket.
- Soft bulbous atlas (white tops / blue-grey undersides); height boost; large masses; Alpha blend.
- PlayMode weather smoke tests; `Tools/gen_soft_cloud_atlas.py`; THIRD_PARTY notes.
- Human Play `image copy 12` cleared merge gate.

**Earlier same day (still on master history):**
- Docs research: `MAP_AUTHORING.md` + `UI_TOOLS_RESEARCH.md`.
- Playback contract + door hinge/swing/look; PH BoardSurfaces 1K→2K.

**Dirty on main (uncommitted):**
- **Rematch / fresh match:** death+wounds clear on Rematch → Local Play (`MatchClock.Reset`, `RoundPlayback.ResetForNewMatch`, `GameBootstrap.BeginFreshMatch`).
- **Urban floors** + brighter lighting + dark void; DoF aperture 2.6; GDD §8 / ART_DIRECTION framing.
- Scratch cotton fog was rejected earlier; pack wiring restored before atmosphere worker landed the LA bank (now merged).

**UI workers (Done — awaiting human Play + merge):**
- `logiCard-char-select-motion` / `feat/char-select-motion` @ `b5d7c77` — EditMode 137 / PlayMode 48.
- `logiCard-modal-restyle` / `feat/modal-restyle` @ `492b8fe` — EditMode 137 / PlayMode 47.

**Docs cards collection (research — not coding slot):**
- `logiCard-cards-collection` / `feat/cards-collection-docs` @ `d00acfc` — `docs/CARD_COLLECTION.md`.
- Awaiting human answers in doc §8 before PRODUCT_MEMORY row.

## Verification

- Atmosphere branch pre-merge: structural EditMode/PlayMode green on worker tree; look cleared by human `image copy 12`.
- Rematch / floor / lighting on main: **not** batchmode-green this session.
- Char-select / modal: green on their worktrees; not yet merged.
- Art look: batchmode never clears.

## Still unfinished

- Post-atmosphere polish (non-blocking): more atlas variation, stronger 3D lobe shading, lighter edge tones — see presentation STATUS.
- Unmonitored human passes below (door timeline; rematch; floors/lighting; UI branches).
- Commit Integrator dirty tree when human asks (rematch + floors + lighting + GDD).
- Merge UI workers when human clears Play.
- Adrenaline effect resolve — UI stub only.
- `DAY13_PLAYTEST_FINDINGS.md` empty.
- Phase 2 Net paused.

## ⚠️ Awaiting human review — unmonitored

1. Door Open on tape second (fix `3018d83`) — Lock In → Execute
2. Rematch after kill — pawns healthy at spawn (dirty fix)
3. Urban floors + brighter lighting + dark void (dirty; not sky-blue)
4. ~~Atmosphere~~ — **merged** (`5b2ee7c`); optional follow-up shading/variation
5. Char-select carousel @ `b5d7c77` — Play `logiCard-char-select-motion`
6. Modal cardstock @ `492b8fe` — Play Quit confirm on `logiCard-modal-restyle`
7. Older polish: south-edge Move-click, zoom-fill, soft-rain, reflections/glass/Scout outfit, diorama arc

## Tomorrow

1. Human Plays UI worktrees → Integrator merges `feat/char-select-motion` then `feat/modal-restyle` (watch `UiStyle` additive tokens).
2. Human clears Unmonitored #1–3; Integrator commits dirty rematch/floors/lighting when asked.
3. Optional atmosphere follow-up worktree (variation / 3D / edge tones).
4. Card collection §8 answers → promote C# when ready.

## Blockers / notes

- Main Editor on `D:/projects/Game/logiCard` — batchmode only in disposable worktrees.
- Dirty/untracked noise: mats; `Assets/ExplosiveLLC/` (human keep/delete); screenshot churn; root archive deleted (lives under `docs/drafts/`); `_1k_backup_2026-08-12` gitignored.
- Do not buy screen-space god-ray packs for ortho.
- Intra-match wound carry across rounds remains correct (C33); only **new match** clears death.
- Atmosphere worktree may be removed after human confirms no leftover edits.
