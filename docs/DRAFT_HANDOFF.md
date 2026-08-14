# Draft Handoff — 2026-08-14

**Milestone:** Phase 5 Commercial Art Bar (active). Phase 2 Net paused (Bandage carve-out via C63).  
**Tip:** Map worktree `dept/map` — **Phase 2 Ready for merge** (human look signed). Main `master` tip moves with Integrator merges (Atmosphere/Cards already on this worktree via prior merges). Prior combined batchmode green @ `7213d98` — **not re-run** for rematch/relight/C65/Atmosphere/Map Phase 2.  
**Ops:** Atmosphere / Cards / Character / UI / **Map** + Integrator. Prefer ≤2 coding-hot.  
**Read first next session:** this file → `PARALLEL_OPS.md` → `departments/INDEX.md` → `contracts/CURRENT.md` → Bandage HUD contract; Map Phase 2 is Ready (merge).

## Live folders

| Seat | Folder | Tip / state |
|------|--------|-------------|
| Integrator | `logiCard` | `master` — merge Map Phase 2 from `logiCard-map` / `dept/map` when ready |
| Atmosphere | `logiCard-atmosphere-stylized` | storm Zap + cloud energize **merged**; worktree may still hold unrelated dirty |
| Cards | `logiCard-cards-collection` | C64/C66 docs **merged**; idle unless restaffed |
| Character | `logiCard-char-select-motion` | 4 briefs; needs human answers |
| UI | `logiCard-modal-restyle` | Bandage HUD in progress (`BANDAGE_HUD_AGENT_BRIEF.md`) |
| Map | `logiCard-map` | `dept/map` — **Phase 2 Ready**; human signed `screenshots/image copy 15.png` |
| (retire OK) | `logiCard-gear-bandage-sim` | merged @ `0b11031` |

## Implemented

- **C62/C63** Bandage Sim-side merged; UI HUD dock still open on UI seat.
- **C64/C66** hybrid card system + deckbuilder sizing docs on master.
- **Rematch reset + sunny relight** (`a419ad4` lineage) on master.
- **C65** surface-material amendment (flat/toon for board surfaces).
- **Atmosphere** storm weather + cloud energize — human Play-signed, merged.
- **Map Phase 2 (this worktree, Ready):**
  - `BoardSurfaceMaterials` — Solid floors/walls; Gradient*_URP door/prop tint; wet-PBR kept unused for board roles.
  - `BoardView` — map-aware dressing; door/prop reskin; **toy fence walls** (posts + rails + cream panel).
  - EditMode `BoardSurfaceMaterialsTests`.
  - Human look OK: `screenshots/image copy 15.png` (“nice floor” → fences edited → “good!”).

## Verification

- Combined master @ `7213d98`: EditMode 149/149, PlayMode 48/48 (historical).
- Map Phase 2: **human Play signed**; **batchmode not run** — do not claim green.
- Atmosphere storm: human Play signed earlier; batchmode still open for current tip.

## Still unfinished

- **Integrator merge Map Phase 2** from `dept/map` (this handoff’s Ready item).
- **Batchmode** on tip after Map (+ prior untested rematch/Atmosphere) lands — Editor closed on path.
- **Bandage HUD-side** (UI seat) — open contract.
- **Healed presenter** after HUD merge.
- Optional: Integrator lighting/`BuildDioramaVolume` re-grade vs new materials (Map flagged; human already likes current Play).
- Character briefs unanswered; Interact station; Adrenaline PLAYBACK; Phase 2 Net paused.
- Leftover dirty elsewhere: `ProjectSettings` UNITY_POST_PROCESSING_STACK_V2 define noise, orphan pack `.meta` deletes, `ExplosiveLLC/` — keep out of feature merges.

## Tomorrow

1. Integrator merges Map Phase 2; run batchmode; optional lighting polish.
2. UI finishes Bandage HUD → merge when Ready.
3. Character idle until brief answers.

## Blockers / notes

- Map did **not** commit `ProjectSettings.asset` or orphan `.unitypackage.meta` deletes (side-effect noise).
- No push unless asked.
- Capacity ≤2 coding-hot (UI + Integrator merge/verify).
