# Draft Handoff — 2026-07-31

**Schedule position:** Day 5 (path + stance) implemented in working tree, not committed. Days 4 and 5 still unchecked in `SCHEDULE.md`. Stick with plan — core combat through Day 7, art Day 8+ (user confirmed; no art-first pivot). HEAD still `4d3d227` on `origin/master`.

## Implemented

**Carryover already on master (`4d3d227`):** Day 3b MatchClock / Time Card loop (C33); Day 4 ghost resolve + ReplayTape playback + Wound stub; C34 Polished Core Demo doc rescope.

**Day 5 — path + stance (uncommitted)**
- `Sim/OrthogonalPathfinder.cs` — BFS orthogonal routes on `GridBoard`.
- `Sim/StanceAllotment.cs` — tile count × base → Sprint/Walk/Crawl band costs (C21).
- `Timeline/PawnProgram.cs` — draft path → allot seconds/stance → `TryCommitDraft`; multi-tile Moves expand one node per tile; Shoot/Lock auto-commit pending draft.
- `Board/BoardInputController.cs` — Move taps draft/extend; stance APIs; path bead preview via `Board/PathPreviewView.cs`.
- `UI/ProgramHud.cs` — stance slider + SPRINT/WALK/CRAWL + SET PATH; thumb zone raised to 0.44; queue readout shows draft.
- Tests: `PathStanceTests.cs`; updated `PawnProgramTests` + PlayMode board/HUD/playback fixtures for draft→commit.

**Doc-consistency sync (still uncommitted, from 07-30):** GDD §6 Time Card presets; PRODUCT_MEMORY C33 numerics; schedule note that Day 3b is absorbed (not a 15th day).

## Verification

- Day 5 exercised in-Editor enough to hit the SET PATH UX (player feedback received). Not a formal Day 4 cold-observer writeup; Day 5 DoD (“Sprint/Walk/Crawl change Move timing on Clock”) not signed off.
- Batch EditMode/PlayMode suites **not re-run today** — open Editor holds the project lock; prior green baseline was EditMode 61/61, PlayMode 20/20 at `4d3d227` (pre–Day 5).
- Day 5 unit/PlayMode tests written but unverified in this environment.

## Still unfinished

- Day 4 cold-observer DoD + SCHEDULE checkbox (code on master; formal pass still open).
- Day 5 DoD confirm on scrubber (Sprint faster / Crawl slower than Walk) + SCHEDULE checkbox; working tree uncommitted.
- Re-save `Bootstrap.unity` if stale pre-MatchClock serialized fields remain.
- Day 6: Hold Angle vs Snap (`ShootCost.HoldAngleSeconds` unused).
- Day 7: one door + local E2E (M2).
- Art pass stays Days 8–11 — primitives until then by plan.

## Tomorrow

1. Commit today’s Day 5 + doc-sync when asked (or first thing if continuing clean).
2. Confirm Day 5 on scrubber; tick Day 5 only if timing reads. Fold a quick Day 4 cold-observer check into the same play session if still open.
3. Day 6 — Hold Angle vs Snap Shot; mutual same-second rule; RPS readable in one playtest.
4. Do not start URP/art, Fusion, gear cards, or attic early.

## Blockers / notes

- Working tree is large and uncommitted: Day 5 code + tests + PathPreview + leftover 07-30 doc sync. Nothing pushed beyond `4d3d227`.
- Batch Unity tests need an isolated project copy while the Editor has `logiCard` open.
- SET PATH = explicit “book draft into budget”; Lock In / Shoot also commit the draft.
