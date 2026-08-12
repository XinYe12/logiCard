# Draft Handoff — 2026-08-12

## Implemented

Phase 5 art / presentation still top priority (`SCHEDULE.md`); core Net paused.

**Integrator session (2026-08-12) — door leaf hinge fit (uncommitted on `master`):**
- Root cause of image-14 "cabinet teleport" door: nappin `(Prb)Door` leaf is **width-on-Z / thickness-on-X** (body collider ~0.17×3.4×1.92). Import normalize + `BoardView.PlaceDoorMesh` assumed width-on-X, so the Y-swing spun the thick face.
- New `DoorLeafFitter` reorients Z-wide leaves, scales to segment gap, parks hinge edge at x=0 / floor at y=0.
- `BoardView` uses the fitter for real door prefabs; fallback box unchanged.
- `InteriorPackImportTool.NormalizeDoorPivotAndScale` also yaws width-Z → width-X on reimport (runtime fit is what Play uses today).
- EditMode tests: `DoorLeafFitterTests` (2).

**Ops hygiene:**
- Removed merged worktrees: `art-pack-research`, `camera-zoom-fill`, `rain-vfx-tune` (pre-remove stashes kept).
- `lighting-ground-assets` + `map-bottom-click`: git worktrees pruned; **dirs still locked on disk** (another process) — delete when free.
- Worker slots still free.

**Image-14 wave already on `master` (tip was `6abbde4` before this session's dirty tree):**
- Path ink strokes from round origin (`skipFirstDot`) — **human-confirmed good**
- Round carry hardened — **human-confirmed good**
- Lightning Zap floor-anchored — **human-confirmed pass**
- Door hinge smoothstep (~0.38s) — landed earlier; **axis/fit fix is this session** (needs sighted re-check)
- South-edge Move click — merged; not re-confirmed this pass
- Lighting/ground shopping notes in `ART_PACK_RESEARCH.md` — research only; not wired

## Verification

- Main-tree batchmode (Editor was closed): **EditMode 136/136, PlayMode 44/44**
- DoorLeafFitter filter run: **2/2** before full suite
- Door open presentation: **batchmode green only — needs human Play / screenshot**

## Still unfinished

- **Door open presentation** — code fix in working tree; human must confirm it reads as a hinged door, not a cabinet.
- **Lighting + ground look** — research written; nothing wired. Flat/muddy board complaint still open until use-now / buy list is integrated (**human pick required**).
- **Clouds** — deferred (human); better assets later.
- **Move-click on Yard/south** — code merged + batchmode green; not re-confirmed in a sighted pass.
- Days 10–14 schedule ticks still open; Phase 2 Net paused.
- `DAY13_PLAYTEST_FINDINGS.md` still empty — Wave 3 worker spawn stays gated on written findings (`PARALLEL_OPS.md`).

## ⚠️ Awaiting human review — unmonitored

Landed and batchmode-green (or docs-only); **do not assume they look/feel right**, do not chain more blind polish on them until a sighted pass clears or rejects each:

1. **Door leaf hinge fit** (this session — highest priority sighted check: Open a door in Playback)
2. South-edge / Yard Move-click fix (OutcomeBanner + plane fallback)
3. Zoom-fill (min 2.6 / max 8 / scroll 0.45) + Integrator default ortho 3.4
4. Soft-rain retune (Box emit, size3D streaks, soft-particle off)
5. C60 vibrancy / floor lift / softer lighting from image-13 Integrator pass
6. Soft rain + zoom from image-13 workers (pre–image-14)
7. Earlier parked visuals still never cleared: reflection probes, window glass, Scout Worker outfit, checkpoint 2/3 diorama arc (see prior draft `docs/drafts/2026-08-07.md`)
8. Lighting/ground research recommendations (not wired — clearing means “buy/wire decided,” not “looks fine”)

## Tomorrow / next

1. Human: Play Bootstrap, Open a door during Playback — confirm hinged leaf (not cabinet). Clear or reject Unmonitored #1.
2. Optional: wire use-now lighting/ground from `ART_PACK_RESEARCH.md` — **only after human picks buy vs use-now**.
3. Sighted pass over the rest of the Unmonitored list when ready.
4. Fill `DAY13_PLAYTEST_FINDINGS.md` before any Day 10/11/13 worker wave.

## Blockers / notes

- No safe coding worker slice right now: door is Integrator-owned and needs sighted sign-off; lighting is human-gated; Day 13 findings empty.
- Orphan dirs: `D:\projects\Game\logiCard-lighting-ground-assets`, `D:\projects\Game\logiCard-map-bottom-click` — close whatever holds them, then delete.
- `Assets/ExplosiveLLC/` still untracked; purpose unexplained — keep or delete is a human call.
- Screenshots `image copy 10–14.png` untracked under `screenshots/`.
- Prior long-form session log: `docs/drafts/2026-08-07.md`.
- Do not buy screen-space god-ray packs for ortho; see research “Do not buy” list.
- Door fix is **dirty on `master`** — commit when you want it locked in.
