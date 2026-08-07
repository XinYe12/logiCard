# Cross-Dept Contracts — Current Wave

**Wave:** 1 + 2 — Day 10 finish, Day 11 audio stub, and Wave 2 wiring are all done as of this update.  
**Updated:** 2026-08-07 by Integrator  
**Rule:** Only Integrator edits this file after a merge. Workers implement against the frozen signatures below.

## VFX views (Presentation → Core)

Match the existing `ShotTracerView` pattern (`Assets/_Project/Board/ShotTracerView.cs`): `Init` builds once; caller drives place/aim and visibility.

**Merged into `master` (`fc32a2d`) and wired (`a57d095`, 2026-08-07).** Both classes exist at `Assets/_Project/Board/*.cs` matching the signatures below exactly, and `RoundPlayback` drives them per the tape-event loop below. No `GameBootstrap` changes were needed — both views are created dynamically inside `RoundPlayback`, same as `ShotTracerView`.

### `MuzzleFlashView` (`LogiCard.Board`)

- `void Init()` — jagged physical muzzle mesh (yellow/orange clay); no particles/bloom; strip colliders; start hidden  
- `void Place(Vector3 worldOrigin, Vector3 worldAim)` — flash at shooter origin, oriented toward aim  
- `void SetVisible(bool visible)` — no self-timer; Core owns show/hide window (~2 frames of playback)

ART_DIRECTION §3: hard-edged physical object, not glow.

### `WoundSplatView` (`LogiCard.Board`)

- `void Init()` — irregular wet red clay splat; slightly higher smoothness than default clay; strip colliders; start hidden  
- `void Place(Vector3 worldPosition)` — at victim hit position, near floor  
- `void SetVisible(bool visible)` — persistent once shown; Core decides when

**Wire sites — done (`a57d095`):** `RoundPlayback` tape loop — `ShootFire` → muzzle flash briefly (`MuzzleFlashVisibleSeconds = 0.15f` TR-seconds from the event's completion instant); `Wounded`/`Killed` → wound splat (persistent from the event's instant, hidden again on rewind). Same ownership pattern as `BuildTracers` / `UpdateTracers` → `ShotTracerView`.

## FoleyPlayer (Audio → Core / UI)

New assembly under `Assets/_Project/Audio/` (Wave 1 stub — **no Boot/UI calls until Wave 2**).

```csharp
namespace LogiCard.Audio
{
    public enum FoleyId
    {
        Footstep,
        Shot,
        TimeCard,
        LockIn,
    }

    public interface IFoleyPlayer
    {
        void Play(FoleyId id);
    }
}
```

- Wave 1 Audio dept: implement stub + placeholder clips; dead code until Integrator wires.  
- **Delivered**: `feat/day11-audio-stub` @ `764a42e` (+ small follow-up `5c402db`), merged to `master` @ `ef6e3f5` / `7e08aba` (2026-08-07). Contract matched verbatim, no deviations.
- **Wire sites — done (`04f9191`, 2026-08-07):** `RoundPlayback.Report` — `MoveArrive` → `Play(Footstep)` once per completed move leg, `ShootFire` → `Play(Shot)` at the shot's completion instant (same forward-only, once-per-crossing hook the WOUNDED/DOWN banner already uses — no replay on rewind). `ProgramHud.OnLockInPressed` → `Play(LockIn)`; `ConfirmTimeCard` → `Play(TimeCard)`. `GameBootstrap` builds one `FoleyPlayer` and threads it into both `RoundPlayback.Init`/`ProgramHud.Init` as a new optional trailing `IFoleyPlayer` parameter, and now also adds an `AudioListener` to the scripted camera (previously missing — `Play()` would have been silently inaudible). `LogiCard.UI.asmdef`/`LogiCard.Boot.asmdef` gained an explicit `LogiCard.Audio` reference.  

ART_DIRECTION audio floor: clay-on-board footsteps; cap-gun / heavy-stapler shot; paper Time Card; Lock In switch snap. Placeholders that still sound distinct are acceptable for Day 11 DoD if human ear-check passes.

## Ownership reminders this wave

| File / area | Owner |
|-------------|--------|
| `PawnView.cs`, `RoundPlayback.cs`, `GameBootstrap.cs` | Core / Integrator |
| `MuzzleFlashView.cs`, `WoundSplatView.cs` (new) | Presentation (`feat/day10-hit-vfx`) |
| `Assets/_Project/Audio/**` (new) | Audio (`feat/day11-audio-stub`) |
| `docs/SHIP_README_DRAFT.md`, `docs/CAPTURE_CHECKLIST.md` | Ship (Wave 2+) |
| `docs/DRAFT_HANDOFF.md`, SCHEDULE ticks, this file | Integrator only |
