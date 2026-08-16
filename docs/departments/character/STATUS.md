# Character — STATUS

**Wave / Day:** Permanent seat — live `logiCard-char-select-motion`
**Branch / worktree:** `feat/char-select-motion` @ `D:\projects\Game\logiCard-char-select-motion`
**Last cross-reviewed:** 2026-08-16 — Integrator pulled `CHARACTER_FANTASY.md` §4.1 InfoBar field
sheet into master as part of the Match Shell Layout merge (docs only). Rest of the worktree's
decision-sheet walk (Part B onward) and `CharacterSelectView.cs`/`UiMotion.cs` deliverable remain
their own separate, still-unmerged workstream — not pulled in by this pass.

## Owned files (this seat)

- `CharacterSelectView.cs`, `UiMotion.cs`, `CharSelect*` UiStyle tokens, `AppFlowController.BuildCharacterSelect`, related PlayMode tests
- `PLAY_NOTES.md`, `CHAR_SELECT_MOTION_AGENT_BRIEF.md`, this STATUS

## Done

- Deliverable `b5d7c77` (EditMode 137 / PlayMode 48) — still on worktree, not yet merged to master
- Docs/cleanup `25244d7` — worktree clean, idle-ready
- **Match Shell Layout (2026-08-16, merged):** `CHARACTER_FANTASY.md` §4.1 InfoBar field sheet — one
  combined bar (Attacker | Defender columns + shared phase/round/pool strip), wound ladder as
  Healthy/Wounded/Dead (not HP), signature/deck size marked OPEN pending C64. Integrator flag carried
  forward: needs a per-side `ArchetypeOf(pawnId)` reader before both InfoBar columns can be truthful
  (`SelectedArchetype` is local-only today) — not wired by this merge.

## In progress

- Nothing — standing by for merge/regression fixes only

## Blocked

- Human Play + Integrator merge

## Offers

- Help Integrator on `UiStyle` combine if needed after merge
