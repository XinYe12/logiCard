# Draft Handoff — 2026-08-12

## Implemented

Phase 5 art / presentation still top priority (`SCHEDULE.md`); core Net paused.

**Playback / Execution contract (this session):**
- New `docs/PLAYBACK_CONTRACT.md` — Reveal ≠ Execute; tape scrub rules; mid-Playback interaction rules; extension checklist for new `TapeEventType`s / verbs.
- Pointer in `CLAUDE.md`.
- Audited `RoundPlayback` presenters: tracers / muzzle / wound already scrubber-derived `SetVisible` (no door-bug twins). Door same-state swing fix kept (`3018d83`).
- Tests: `TapeEventPlaybackCoverageTests` (enum coverage), `ShootFireVfxFollowScrubberSeconds`, `WoundSplatFollowsScrubberSeconds`; door-swing regression retained.
- Adrenaline remains ship mid-Playback control with **stub effect** — roadmap verbs (Bandage/Flashbang/tools/vent-as-card) must follow the contract when they land.

**Human play earlier (`image copy 2.png`):**
1. Door mesh — **good**
2. Fog/mist — **not seen** (density bumped; re-check)
3. Yard 2K grain — **yep**
4. Door swing all at end of reveal — **fixed** (`3018d83`); needs human re-check on timeline

## Verification

- Disposable `logiCard-verify-playback-contract`: **EditMode 137/137, PlayMode 47/47**
  (enum coverage + ShootFire/WoundSplat scrubber timing + door-swing regression).

## Still unfinished

- Fog/mist sighted pass. Optional PH 4K. Clouds deferred.
- Day 13 findings empty. Phase 2 Net paused.
- Adrenaline **effect** resolve still deferred (UI gate only).

## ⚠️ Awaiting human review — unmonitored

1. Door swing on timeline (Open at event second)
2. Fog/mist denser pass
3. South-edge Move-click, zoom-fill, C60, soft-rain, reflections/glass/Scout/checkpoint arc

## Tomorrow / next

1. Human: Lock In → Execute — doors/shoot VFX on tape seconds; glance fog.
2. Any new verb → follow `PLAYBACK_CONTRACT.md` checklist.
3. Day 13 findings when ready for worker waves.

## Blockers / notes

- Main Editor often open — verify in disposable worktree.
- `Assets/ExplosiveLLC/` unexplained.
- Local 1K BoardSurfaces backup gitignored under `Textures/_1k_backup_*/`.
