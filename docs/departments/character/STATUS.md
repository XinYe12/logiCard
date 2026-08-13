# Character — STATUS

**Wave / Day:** Permanent seat — live `logiCard-char-select-motion`
**Branch / worktree:** `feat/char-select-motion` @ `D:\projects\Game\logiCard-char-select-motion`
**Brief:** `CHAR_SELECT_MOTION_AGENT_BRIEF.md` (worktree root)
**Last cross-reviewed:** 2026-08-18 — rebased `feat/char-select-motion` onto current master
(`f45e986`, past Match Shell Layout / Map / Camera / Storm counter / camera control-hint merges).
Prior note: 2026-08-16 Integrator pulled `CHARACTER_FANTASY.md` §4.1 InfoBar field sheet into
master as part of the Match Shell Layout merge (docs only) — the carousel deliverable
(`CharacterSelectView.cs`/`UiMotion.cs`) remained a separate, still-unmerged workstream at that
time and stays unmerged now, just rebased forward. (2026-08-13: a same-day UI Toolkit pilot rebuild
of Character Select was tried and reverted back to uGUI after a human Play pass called it "still
bad," and the carousel was then skinned with Kenney "UI Pack - Adventure" CC0 art — see
`docs/UI_TOOLKIT_MIGRATION_PROPOSAL.md`; the deliverable stayed uGUI throughout.)

## Owned files (this seat)

- `Assets/_Project/UI/CharacterSelectView.cs` (+ `.meta`) — 2-item center/flank carousel view, uGUI
  (see below — a same-session UI Toolkit pilot was tried and reverted)
- `Assets/_Project/UI/UiMotion.cs` (+ `.meta`) — zero-dependency float/Color/Vector2 tween helper
- `Assets/_Project/UI/UiStyle.cs` — extended with `CharSelect*`-prefixed tokens only
- `Assets/_Project/UI/AppFlowController.cs` — `BuildCharacterSelect` path rewired to the new view
- `Assets/_Project/Tests/PlayMode/AppFlowPlayModeTests.cs` — carousel coverage
- `Assets/_Project/Art/UI/` — new: Kenney "UI Pack - Adventure" (CC0) sprites +
  `UiKenneyImportTool.cs` + `THIRD_PARTY.md` — see Done below
- `docs/UI_TOOLKIT_MIGRATION_PROPOSAL.md` — kept as documentation of the reverted pilot's findings
  (see below); not a live implementation anymore
- `PLAY_NOTES.md`, `CHAR_SELECT_MOTION_AGENT_BRIEF.md`, this STATUS

## Done

- Center/flank carousel replacing the flat 2-up `SelectionGrid` for Character Select (Scout + Juggernaut).
- Prev/Next + flank-click role rotation, ~650ms coordinated scale/opacity/anchor crossfade via `UiMotion`, input locked during animation.
- Ghost archetype headline behind figures; background tint crossfade toward per-archetype accent, staying in the `UiStyle` family.
- `Pick_Scout` / `Pick_Juggernaut` / `ConfirmCharacter` button names preserved for existing PlayMode tests.
- Map Select untouched — still the plain `SelectionGrid` (C59).
- Deliverable `b5d7c77` — EditMode 137 / PlayMode 48, both green (pre-rebase baseline) — still on worktree, not yet merged to master.
- Cleanup pass — reverted unrelated `ProjectSettings/ProjectSettings.asset` scripting-define churn and 6 accidental `ithappy`/`nappin` `.meta` deletions (none were part of this slice's scope); deleted local batchmode run noise — none committed. Landed at `25244d7`.
- Selected-card highlight/glow polish (2026-08-13) — two soft rounded-rect halo rings (`GlowRing`, `CharacterSelectView.cs`) sit behind the carousel, always tracking whichever card is currently center, alpha riding the same eased role lerp the card itself uses. New tokens `CharSelectGlowScout` / `CharSelectGlowJuggernaut` in `UiStyle.cs`.
- **UI Toolkit pilot — tried, then reverted (2026-08-13).** At the human's request for "an already mature game UI system," rebuilt `CharacterSelectView.cs` on `UIDocument`/`VisualElement`, scoped to a pilot + proposal doc rather than a unilateral full-game migration (that's cross-department territory — `ModalDialog.cs`, `ProgramHud.cs`, Map Select, `GameBootstrap.cs` aren't this worktree's to touch). Got it fully working functionally (fixed three real `UIDocument` lifecycle bugs along the way — see `docs/UI_TOOLKIT_MIGRATION_PROPOSAL.md`'s Findings section, kept intact as real cost data for whoever revisits this), but it never had a visual Play pass (no Editor was open on this worktree to drive one) and shipped with a `No Theme Style Sheet` warning of unconfirmed visual impact. Human feedback after finally seeing it: **"it is still bad."** Rather than keep iterating blind on an unverified rendering path, reverted `CharacterSelectView.cs`, the two asmdefs, and the PlayMode test helpers back to the pre-pilot uGUI implementation (git history at `2c99a08` preserves the Toolkit code and the fixes, if that path gets revisited later) — the migration proposal doc stays as-is; it documents real findings independent of whether the pilot code is currently live.
- **Kenney "UI Pack - Adventure" (CC0) skin (2026-08-13).** Human's actual ask, once UI Toolkit was off the table: stop hand-drawing flat-color chrome, use existing art that fits the theme. Compared Kenney's plain "UI Pack" (flat cold-blue, risked reading as "default Unity" per `ART_DIRECTION.md` §7's own warning), "Fantasy UI Borders" (monochrome, no material read), and "Boardgame Pack" (great piece/token iconography, wrong asset type for panel chrome) against "UI Pack - Adventure" (warm wood-bordered, cream-parchment 9-slice panels/buttons) — picked the last one as the closest CC0-library match to this project's cardstock/desk-lamp-warm palette. Wired: Scout's card face is `panel_brown` (cream), Juggernaut's is `panel_brown_dark` (solid brown) — real material variation instead of a color-multiply hack; Prev/Next nav buttons use `button_brown`. Sprites live under `Assets/_Project/Art/UI/Resources/CharSelect/` (a `Resources/` folder — loaded via `Resources.Load<Sprite>`, works in an actual build, not just in-Editor) with 9-slice borders set by `Assets/_Project/Art/UI/Editor/UiKenneyImportTool.cs` (batchmode `-executeMethod LogiCard.Art.Editor.UiKenneyImportTool.Run`, border pixels hand-measured since Kenney doesn't ship per-file border metadata for individually-cropped PNGs). Removed the now-dead `CharSelectCardScout`/`CharSelectCardJuggernaut` flat-color tokens from `UiStyle.cs` since nothing references them anymore. Provenance in `Assets/_Project/Art/UI/THIRD_PARTY.md`. EditMode 137/137, PlayMode 49/49, both green (pre-rebase baseline).
- **Match Shell Layout (2026-08-16, merged into master separately):** `CHARACTER_FANTASY.md` §4.1
  InfoBar field sheet — one combined bar (Attacker | Defender columns + shared phase/round/pool
  strip), wound ladder as Healthy/Wounded/Dead (not HP), signature/deck size marked OPEN pending
  C64. Integrator flag carried forward: needs a per-side `ArchetypeOf(pawnId)` reader before both
  InfoBar columns can be truthful (`SelectedArchetype` is local-only today) — **still not wired**;
  see "Rebase (2026-08-18)" below for this pass's disposition on that flag.
- **Rebase (2026-08-18):** rebased onto master `f45e986` (110 commits: Match Shell Layout, Map,
  Camera, Storm counter, Healed presenter, camera control-hint UI chrome). Conflicts were docs-only
  (`PLAY_NOTES.md`, this STATUS file, plus add/add conflicts on `Assets/_Project/Art/UI/` files that
  both branches happened to create — resolved by keeping this branch's Kenney-skin content) —
  resolved by keeping both features' notes/history side by side, no semantic changes.
  `CharacterSelectView.cs` / `UiMotion.cs` / `UiStyle.cs` CharSelect* tokens /
  `AppFlowController.BuildCharacterSelect` applied clean, no code conflicts with Match Shell Layout
  or camera control-hint touches to `ProgramHud.cs` / `GearHandView.cs` (this branch never touched
  those files). The `ArchetypeOf(pawnId)` reader was **not** wired as part of this rebase — it's
  InfoBar/Match-Shell scope (Integrator/UI owned), out of scope for a carousel rebase, and touching
  `ProgramHud`/InfoBar here risks conflicting with work in flight on those files outside this
  worktree. Flagging forward rather than silently dropping it.

## In progress

- Session pickup 2026-08-13: tip still `a915bb7` (Kenney skin + Toolkit revert). Branch is **5 ahead / 7 behind** `master` (master has modal restyle + Bandage/C63 merges). Untracked `screenshots/image copy 13.png` present — looks like a flat orange/yellow mock, not the Kenney parchment build; need human to confirm whether that's Play feedback / a new target, or unrelated. Otherwise still waiting on Play sign-off per `PLAY_NOTES.md`.

## Blocked

- Merge is gated on a human Play pass (see `PLAY_NOTES.md`) — visual/feel sign-off can't come from batchmode, and this specifically replaces hand-drawn chrome with sourced art, so the thing most worth checking is whether the Kenney wood/parchment look actually reads well against the rest of the screen (ghost headline, warm bg tint, glow) rather than clashing.

## Depends on

- Integrator to `git merge --no-ff feat/char-select-motion` from the main tree once the human is satisfied. No push, no merge performed by this worker.
- Separately, `docs/UI_TOOLKIT_MIGRATION_PROPOSAL.md` remains open for whenever the Integrator/human wants to revisit UI Toolkit project-wide — not blocking this branch's merge, just parked.

## Offers

- Idle-ready. Will only touch this worktree again to fix a merge/regression bug or pick up a fresh queued polish item — no new carousel/TMP/asset-pack scope without a fresh brief.
