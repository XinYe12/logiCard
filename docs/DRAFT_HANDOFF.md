# Draft Handoff — 2026-08-12

## Implemented

Phase 5 art / presentation still top priority (`SCHEDULE.md`); core Net paused.

**Human play (screenshot `image copy 2.png`):**
1. Door mesh animation — **good**
2. Fog/mist — **not seen** (density bumped this session; re-check)
3. Yard 2K grain — **yep**

**Door swing timeline bug (this session):**
- Symptom: during reveal/playback, all door opens finished at the end instead of on their tape seconds.
- Cause: `ApplyDoorVisualState` restarted the hinge coroutine on every `ApplyTime` tick whenever `SwingRoutine != null`, so the 0.38s arc never completed until the scrubber stopped.
- Fix: same-state refresh leaves an in-flight swing alone. PlayMode regression `DoorSwingKeepsProgressingAcrossPlaybackTicks`.
- Fog denser/warmer pass included (response to “not seen”).

**Earlier today on `master`:** hinge fit, mesh casing/tint/thickness, fog wire, PH 2K BoardSurfaces.

## Verification

- Disposable `logiCard-verify-doorswing`: **EditMode 136/136, PlayMode 45/45** (incl. new door-swing test)
- Door swing timeline: batchmode green — **needs human Play re-check**
- Fog denser pass: not yet sighted

## Still unfinished

- Fog/mist may still read weak after density bump — human look.
- Optional PH 4K; buy list still human call if atmosphere still flat.
- Clouds deferred. Day 13 findings empty. Phase 2 Net paused.

## ⚠️ Awaiting human review — unmonitored

1. **Door swing on timeline** (this fix — Open should start at the Door event second, not pile up at reveal end)
2. Fog/mist denser pass (was invisible)
3. South-edge Move-click, zoom-fill, C60, soft-rain, reflections/glass/Scout/checkpoint arc

## Tomorrow / next

1. Human: Lock In → watch Playback — doors should swing when their Open hits, not all at the end.
2. Glance fog/mist; clear or reject.
3. Day 13 findings when ready for worker waves.

## Blockers / notes

- Main Editor often open — verify in disposable worktree.
- `Assets/ExplosiveLLC/` unexplained.
- Local 1K BoardSurfaces backup gitignored under `Textures/_1k_backup_*/`.
