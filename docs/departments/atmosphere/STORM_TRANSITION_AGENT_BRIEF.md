# Agent Brief — "Storm rolling in" transition (was optional DoD #3)

**From:** Integrator **To:** Atmosphere seat
**Priority:** Optional polish. Not blocking anything, not blocked on anything. Pick up whenever.
**Scope:** `BoardWeatherPocket.cs` weather-swap only. Do not touch lighting-restore internals
(`CaptureLightingBaseline`/`RestoreLightingIfOverridden`), Sunny/Storm mood definitions, or anything
outside the module-swap path this brief describes.

## Start from a fresh worktree, not the old one

`logiCard-atmosphere-stylized` (`feat/atmosphere-stylized`) is **116 commits behind `master`** as of
2026-08-20 — its own copy of `BoardWeatherPocket.cs` is the pre-merge version, superseded by the Sunny
mode merge (`0857b80`) and everything since. Don't try to reconcile that old worktree; it'll just
recreate the same merge pain this session already worked through twice. Instead:

```
git worktree add ../logiCard-storm-transition -b feat/storm-transition master
```

Work from current `master`'s `BoardWeatherPocket.cs`, not the old branch.

## What's wrong

Every weather mood change (`Fair`↔`Storm`↔`Sunny`) is an instant cut — `ApplyWeather` calls
`ClearWeather()` (destroys the whole active module: clouds, rain, mist, lightning, all of it in one
frame) and immediately builds the new module from scratch. No crossfade, no build-up — one frame it's
Fair, the next it's a fully-formed Storm. This was always flagged as a stub, not a final look
(`docs/departments/atmosphere/STATUS.md`'s "Deferred (optional DoD #3)").

## What "done" looks like

A short transition when weather changes — clouds should visibly roll/gather in rather than pop into
existence, roughly matching the "storm rolling in" framing from the original DoD note. Exact shape
(duration, whether it's a fade/scale-in on the new module, a brief overlap where both modules coexist,
or something else) is your call — this is presentation-only, not a mechanic, so there's no numeric to
get greenlit first the way Flashbang's effect shape needs one.

**Hard constraint — do not violate this:** `docs/core/PLAYBACK_CONTRACT.md` rule 4 (door-bug class):
**no per-tick restart.** `RoundPlayback.SyncWeatherToSeconds` derives mood as a pure function of
`(arm-time snapshot + any StormCast ≤ scrubber second)` and calls `ApplyWeather` only when the derived
mood actually changes — never per-tick. Whatever transition you build must not restart on repeated
`ApplyTime` calls at the same derived mood, and must remain scrub-safe (rewinding past the mood change
and scrubbing forward again should replay the transition consistently, not desync or double-fire). If
you're not sure a transition design satisfies this, that's the thing to flag back, not guess past.

## Test

`Assets/_Project/Tests/PlayMode/BoardWeatherPocketPlayModeTests.cs` already has
`ApplyWeatherSameMoodKeepsCloudBankInstance` and `FairStormLightingRoundTripsAcrossRepeatedCycles` —
both must keep passing unchanged (they're the existing scrub-safety/idempotency guarantees). Add a new
PlayMode test for whatever transition behavior you build — at minimum, that scrubbing back past a mood
change and forward again doesn't double-fire or leave stale transition state.

## Report back

Batchmode green (Editor closed on your worktree's own path) before reporting: EditMode + PlayMode
counts. Update `docs/departments/atmosphere/STATUS.md` yourself with the commit hash and what you built.
Do **not** merge or push — commit on your branch and report back for Integrator review, same as every
other department this session.
