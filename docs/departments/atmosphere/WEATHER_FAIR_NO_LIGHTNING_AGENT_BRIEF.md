# Agent Brief — Fair weather must not spawn lightning

**From:** Integrator **To:** Atmosphere seat (`feat/atmosphere-stylized`, this worktree)
**Priority:** Bug — human found via Play, 2026-08-15.
**Scope:** single file, narrow diff. Do not touch anything else in this worktree.

## Bug

Normal/Fair weather (white cloud) currently spawns lightning. It should not — thunder/lightning
is a Storm-only signal. Human report: "the normal cloud (white) should not have thunder."

## Root cause

`Assets/_Project/Board/BoardWeatherPocket.cs`, method `PlaceLightning` (around line 1255–1278):

```csharp
private void PlaceLightning(float width, float depth)
{
    if (_mood == BoardWeatherMood.Storm)
    {
        PlaceStormLightning(width, depth);
        return;
    }

    GameObject prefab = LoadPrefab(ref _lightningWhitePrefab, LightningWhiteResourcePath);
    ...
    StartCoroutine(LightningLoop(ps, FairLightningIntervalMinSeconds, FairLightningIntervalMaxSeconds, doubleStrikeChance: 0f));
}
```

For any non-Storm mood (Fair, and now your uncommitted Sunny), this falls through and spawns a
white `VFX_Zap_White` rig on a slow 12–22s loop (`FairLightningIntervalMinSeconds/MaxSeconds`,
lines ~1246–1248). That's the bug — it should do nothing for Fair (and Sunny).

## Fix

Make `PlaceLightning` a Storm-only spawn: early-return (no-op) for every mood except Storm,
instead of falling through to the white-lightning branch. Remove (or stop calling) the now-dead
`LightningWhiteResourcePath` / `_lightningWhitePrefab` / `FairLightningIntervalMinSeconds/MaxSeconds`
path if nothing else references it after the change — check first, don't leave unused dead code.

## Contract note

**Your working copy of this file already has uncommitted changes** (Sunny mood, same-mood
early-out, lighting-restore guards — the held-back work from 2026-08-14, per `DRAFT_HANDOFF.md`).
None of that touches `PlaceLightning` or the Fair-lightning constants, so there should be no
overlap. Keep your diff for this bug scoped to `PlaceLightning`/the dead constants only:

- Do **not** revert, rebase, or otherwise disturb your existing uncommitted Sunny-mode hunks.
- Do **not** touch the other unrelated dirty files in this worktree (Floor/Glass mats,
  ProjectSettings, orphan `.meta` deletes, `_Recovery/`, screenshots) — those stay out per your
  own STATUS.md note.
- Commit **only** this bug fix by pathspec (`git add Assets/_Project/Board/BoardWeatherPocket.cs`
  plus the test file if you add one — not `git add -A`), as its own commit on
  `feat/atmosphere-stylized`. Leave the Sunny-mode work uncommitted exactly as it is; that's a
  separate pending human decision, not part of this fix.

## Test

`Assets/_Project/Tests/PlayMode/BoardWeatherPocketPlayModeTests.cs` has no existing assertion that
Fair spawns a "Lightning" object (only Storm's "LightningStorm" is checked, ~line 52). Add a
PlayMode smoke asserting `ApplyWeather(Fair)` produces **no** lightning child under the Fair
module, so this doesn't regress silently again.

## Report back

Update your own `docs/departments/atmosphere/STATUS.md` (Committed section) with the commit hash
and a one-line note. Do **not** merge or push — report the commit back to Integrator for review
and merge.
