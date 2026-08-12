# Character — STATUS

**Wave / Day:** Permanent seat — live `logiCard-char-select-motion`
**Branch / worktree:** `feat/char-select-motion` @ `D:\projects\Game\logiCard-char-select-motion`
**Brief:** `CHAR_SELECT_MOTION_AGENT_BRIEF.md` (worktree root)
**Last cross-reviewed:** 2026-08-13 — center-card highlight/glow polish pass

## Owned files (this seat)

- `Assets/_Project/UI/CharacterSelectView.cs` (+ `.meta`) — 2-item center/flank carousel view
- `Assets/_Project/UI/UiMotion.cs` (+ `.meta`) — zero-dependency float/Color/Vector2 tween helper
- `Assets/_Project/UI/UiStyle.cs` — extended with `CharSelect*`-prefixed tokens only
- `Assets/_Project/UI/AppFlowController.cs` — `BuildCharacterSelect` path rewired to the new view
- `Assets/Tests/PlayMode/AppFlowPlayModeTests.cs` — carousel coverage
- `PLAY_NOTES.md`, `CHAR_SELECT_MOTION_AGENT_BRIEF.md`, this STATUS

## Done

- Center/flank carousel replacing the flat 2-up `SelectionGrid` for Character Select (Scout + Juggernaut).
- Prev/Next + flank-click role rotation, ~650ms coordinated scale/opacity/anchor crossfade via `UiMotion`, input locked during animation.
- Ghost archetype headline behind figures; background tint crossfade toward per-archetype accent, staying in the `UiStyle` family.
- `Pick_Scout` / `Pick_Juggernaut` / `ConfirmCharacter` button names preserved for existing PlayMode tests.
- Map Select untouched — still the plain `SelectionGrid` (C59).
- Landed at `b5d7c77` — EditMode 137 / PlayMode 48, both green.
- Cleanup pass — reverted unrelated `ProjectSettings/ProjectSettings.asset` scripting-define churn and 6 accidental `ithappy`/`nappin` `.meta` deletions (none were part of this slice's scope); deleted local batchmode run noise — none committed. Landed at `25244d7`.
- Selected-card highlight/glow polish (this session) — see below.

## In progress

- Nothing. Awaiting the next human Play-mode pass before Integrator merges.

## Blocked

- Merge is gated on a human Play pass (see `PLAY_NOTES.md` at worktree root for how to see it and what "good" looks like) — visual/feel sign-off can't come from batchmode.

## Depends on

- Integrator to `git merge --no-ff feat/char-select-motion` from the main tree once the human is satisfied. No push, no merge performed by this worker.

## Offers

- Idle-ready. Will only touch this worktree again to fix a merge/regression bug or pick up a fresh queued polish item — no new carousel/TMP/Kenney scope without a fresh brief.
