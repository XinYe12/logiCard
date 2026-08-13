# Character — STATUS

**Wave / Day:** Permanent seat — live `logiCard-char-select-motion`
**Branch / worktree:** `feat/char-select-motion` @ `D:\projects\Game\logiCard-char-select-motion`
**Brief:** `CHAR_SELECT_MOTION_AGENT_BRIEF.md` (worktree root)
**Last cross-reviewed:** 2026-08-18 — rebased `feat/char-select-motion` onto current master
(`f45e986`, past Match Shell Layout / Map / Camera / Storm counter / camera control-hint merges).
Prior note: 2026-08-16 Integrator pulled `CHARACTER_FANTASY.md` §4.1 InfoBar field sheet into
master as part of the Match Shell Layout merge (docs only) — the carousel deliverable
(`CharacterSelectView.cs`/`UiMotion.cs`) remained a separate, still-unmerged workstream at that
time and stays unmerged now, just rebased forward.

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
- Deliverable `b5d7c77` — EditMode 137 / PlayMode 48, both green (pre-rebase baseline) — still on worktree, not yet merged to master.
- Cleanup pass — reverted unrelated `ProjectSettings/ProjectSettings.asset` scripting-define churn and 6 accidental `ithappy`/`nappin` `.meta` deletions (none were part of this slice's scope); deleted local batchmode run noise — none committed. Landed at `25244d7`.
- Selected-card highlight/glow polish (2026-08-13 session) — two soft rounded-rect halo rings (`GlowRing`, `CharacterSelectView.cs`) sit behind the carousel, always tracking whichever card is currently center. Ring alpha rides the same eased role lerp the card itself uses (`Mathf.InverseLerp` on `Role.Scale` between flank/center extremes, times the card's own crossfade alpha), so the halo grows in as a card scales up to center and fades as it demotes to flank — no separate timer, no snap. New tokens `CharSelectGlowScout` / `CharSelectGlowJuggernaut` in `UiStyle.cs` (RGB only; alpha is computed per-frame from ring padding/max-alpha constants local to the view, not baked into the token). Rings are `raycastTarget = false` and parented before the cards in `CarouselStage`, so they never intercept clicks and never draw on top. Rebased onto `master` @ `77831cf` at the time (docs-only conflict in this STATUS file, resolved keeping the "Permanent seat" framing). EditMode 137/137, PlayMode 49/49, both green (batchmode noise reverted before commit, per the standing cleanup pattern above).
- **Match Shell Layout (2026-08-16, merged into master separately):** `CHARACTER_FANTASY.md` §4.1
  InfoBar field sheet — one combined bar (Attacker | Defender columns + shared phase/round/pool
  strip), wound ladder as Healthy/Wounded/Dead (not HP), signature/deck size marked OPEN pending
  C64. Integrator flag carried forward: needs a per-side `ArchetypeOf(pawnId)` reader before both
  InfoBar columns can be truthful (`SelectedArchetype` is local-only today) — **still not wired**;
  see "Rebase (2026-08-18)" below for this pass's disposition on that flag.
- **Rebase (2026-08-18):** rebased onto master `f45e986` (110 commits: Match Shell Layout, Map,
  Camera, Storm counter, Healed presenter, camera control-hint UI chrome). Conflicts were docs-only
  (`PLAY_NOTES.md`, this STATUS file) — resolved by keeping both features' notes/history side by
  side, no semantic changes. `CharacterSelectView.cs` / `UiMotion.cs` / `UiStyle.cs` CharSelect*
  tokens / `AppFlowController.BuildCharacterSelect` applied clean, no code conflicts with
  Match Shell Layout or camera control-hint touches to `ProgramHud.cs` / `GearHandView.cs` (this
  branch never touched those files). The `ArchetypeOf(pawnId)` reader was **not** wired as part of
  this rebase — it's InfoBar/Match-Shell scope (Integrator/UI owned), out of scope for a carousel
  rebase, and touching `ProgramHud`/InfoBar here risks conflicting with work in flight on those
  files outside this worktree. Flagging forward rather than silently dropping it.

## In progress

- Nothing. Awaiting the next human Play-mode pass before Integrator merges.

## Blocked

- Merge is gated on a human Play pass (see `PLAY_NOTES.md` at worktree root for how to see it and what "good" looks like, updated this session for the glow) — visual/feel sign-off can't come from batchmode.

## Depends on

- Integrator to `git merge --no-ff feat/char-select-motion` from the main tree once the human is satisfied. No push, no merge performed by this worker.

## Offers

- Idle-ready. Will only touch this worktree again to fix a merge/regression bug or pick up a fresh queued polish item — no new carousel/TMP/Kenney scope without a fresh brief.
