# Core / Integrator — STATUS

**Wave / Day:** Wave 1 + Wave 2 — Day 10 and Day 11 both fully wired and committed  
**Branch / worktree:** `master` @ `04f9191` — `/Users/xuxinye/Documents/projects/Game/LogiCard`  
**Last cross-reviewed:** 2026-08-07 — presentation/STATUS (report-back `f2256f6` reviewed, merged, wired), audio/STATUS (report-back `764a42e`+`5c402db` reviewed, merged in two passes, wired), ship/STATUS (report-back `fc58db3` reviewed, three owned files landed)

## Owned files (this wave)

- `Assets/_Project/Board/PawnView.cs` — stepped 8–12fps playback quantization (done)  
- `Assets/_Project/Boot/RoundPlayback.cs` — VFX wire (done) + Foley hooks (done)  
- `Assets/_Project/Boot/GameBootstrap.cs` — not needed for VFX wiring (views spawn dynamically inside `RoundPlayback`, same as `ShotTracerView`); needed for Foley (builds the one `FoleyPlayer` instance, adds the missing `AudioListener`)  
- `docs/DRAFT_HANDOFF.md`, `docs/SCHEDULE.md` ticks, `docs/contracts/CURRENT.md`, `docs/departments/INDEX.md`

## Done

- Day 9 accepted; SCHEDULE Day 9 ticked  
- Parallel ops docs landed (`PARALLEL_OPS.md`, departments, contracts)  
- Day 10 VFX worktree already spun (`logiCard-day10-vfx`)
- **Stepped 8–12fps motion on `PawnView`** (ART_DIRECTION §2), uncommitted on `master`:
  - `PawnView.ApplyTime` now holds its rendered pose for ~1/10 real second (`Time.unscaledTime`-gated) once a path is already armed, instead of writing `transform.position` every engine frame — "pose snaps, not blends." A fresh `SetPath` (new draft preview, newly armed tape, `Disarm`'s carry-point reset) always forces its very next `ApplyTime` through immediately and exactly, and path start/end (`timeResourceSeconds <= 0` or `>= Path.EndSeconds`) always snap exactly too, so interactive draft preview and key poses never lag.
  - Deliberately **not** wired through `RoundPlayback.cs`/`GameBootstrap.cs` — self-contained in `PawnView.cs` per this wave's scope; those two stay untouched pending VFX wiring.
  - Two pre-existing PlayMode tests encoded the opposite contract (exact continuous position at *any* scrub instant, including sub-second mid-segment points on an already-armed tape) and would fail under real stepped motion by construction, not by a bug — `RoundPlaybackPlayModeTests.DefenderStaysHomeUntilTheTapeArms` and `ProgramHudPlayModeTests.PlaybackPlacesThePawnOnItsScheduledPointAtTheArrivalSecond`. Converted both to `[UnityTest]` and inserted a real-time wait (`SliceSceneFixture.WaitForPawnStepRelease`, 0.15s `WaitForSecondsRealtime`) past the new hold before the follow-up scrub, instead of weakening their tolerances — they still assert exact positions, just after the hold has legitimately cleared.
  - Verified in a disposable worktree (`logiCard-verify-day10`, removed after): **EditMode 102/102, PlayMode 29/29**, no exceptions in the PlayMode log.
  - Committed as `d60f01d`.
- **Presentation VFX merged** (2026-08-07): `feat/day10-hit-vfx` (`f2256f6` — `MuzzleFlashView.cs` + `WoundSplatView.cs`, matching the frozen `Init`/`Place(...)`/`SetVisible(bool)` contract in `contracts/CURRENT.md`) reviewed and merged into `master` (`fc32a2d`, `--no-ff`, no conflicts). Re-verified in a disposable worktree with the stepped-motion change alongside it before merging: **EditMode 102/102, PlayMode 29/29**, no exceptions.
- **VFX wired into `RoundPlayback`** (`a57d095`): `BuildHitVfx`/`UpdateHitVfx`/`ClearHitVfx` follow the existing `BuildTracers`/`UpdateTracers`/`ClearTracers` pattern — muzzle flash on `ShootFire` (shooter's position at the shot's completion instant, lit ~0.15 TR-seconds), wound splat on `Wounded`/`Killed` (victim's position, persistent, hidden on rewind). No `GameBootstrap` changes needed. Re-verified against `master` at `ef6e3f5` (post-Audio-merge) in a disposable worktree: **EditMode 102/102, PlayMode 29/29**, no exceptions. No automated test covers the flash/splat visuals themselves (no existing test asserted on tracer visuals either) — still needs a human Editor look before the Day 10 SCHEDULE tick.
- **Audio Wave 1 follow-up merged** (`7e08aba`): small cleanup commit from Audio (`5c402db` — dropped unused `using`, STATUS update with their own batchmode confirmation). No functional change; safe fast merge.
- **Ship docs landed** (`950ff63`): pulled only the three files Ship actually owns (`SHIP_README_DRAFT.md`, `CAPTURE_CHECKLIST.md`, `departments/ship/STATUS.md`) out of `fc58db3` rather than merging the branch — that commit also carried a stale pre-Day-10 snapshot of the shared docs from when their worktree forked at `a5c276a`, which would have clobbered today's current versions under a plain merge.
- **Wave 2: `FoleyPlayer` wired into `RoundPlayback`/`ProgramHud`** (`04f9191`): `GameBootstrap` builds one `FoleyPlayer` and threads it into both `Init` calls as a new optional trailing `IFoleyPlayer` param. `RoundPlayback.Report` (same forward-only, once-per-crossing hook the WOUNDED/DOWN banner already uses) — `MoveArrive` → `Play(Footstep)`, `ShootFire` → `Play(Shot)`. `ProgramHud.OnLockInPressed` → `Play(LockIn)`, `ConfirmTimeCard` → `Play(TimeCard)`. Also fixed a real gap found while wiring: the scripted camera never got an `AudioListener` (only auto-added via the Editor's "GameObject > Camera" menu, which this project's code-built camera never goes through) — without it, every `Play()` call would have been a silent no-op past a console warning. `LogiCard.UI.asmdef`/`LogiCard.Boot.asmdef` gained an explicit `LogiCard.Audio` reference. Verified against `master` at `7e08aba` in a disposable worktree: **EditMode 102/102, PlayMode 29/29**, no exceptions, no "no audio listener" warnings in the log.

## In progress

- Nothing. All four dept slices for Wave 1 + Wave 2 are delivered, merged, and wired.

## Blocked

- Day 10 SCHEDULE tick until human Editor look confirms flash/splat read correctly  
- Day 11 SCHEDULE tick until human ear-check confirms the four Foley placeholders read as distinct and land at the right moments

## Depends on

- Nothing outstanding. Next Core work is Wave 3 (Days 12–14: Windows candidate + playtest hotfixes) once the human sign-offs above land, or any playtest finding that reopens something.

## Offers

- Merge authority for worker branches (human still approves)
