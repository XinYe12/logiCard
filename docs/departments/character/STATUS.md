# Character — STATUS

**Wave / Day:** Permanent seat — live `logiCard-char-select-motion`
**Branch / worktree:** `feat/char-select-motion` @ `D:\projects\Game\logiCard-char-select-motion`
**Last cross-reviewed:** 2026-08-18 — rebased `feat/char-select-motion` onto current master
(`f45e986`); docs/concepts only otherwise (option C), concept backlog §3 items 1–8 filled
(2026-08-13). Char Select UI/motion carousel work earlier in this branch's history was handed to
**UI** dept partway through (see Mandate below) and remains mergeable history but is no longer this
seat's active scope. See the Rebase entry under Done for what this pass touched.

## Mandate

| Owns | Does not own |
|------|----------------|
| Character fantasy, attrs meaning, unique-verb concepts + impl briefs | Char Select UI → **UI** |
| Joint boundary with Cards | Gear catalog → **Cards** |
| Future resolve only after Integrator carve-out | Live Sim under general pause |

**Mode:** Pre-code. See [`CHARACTER_PLAN.md`](../../CHARACTER_PLAN.md).

## Done

- Carousel history (pre-mandate-shift, kept for reference — no longer this seat's active scope):
  center/flank carousel replacing the flat 2-up `SelectionGrid`, `~650ms` coordinated crossfade via
  `UiMotion`, ghost archetype headline, halo-glow polish, a same-day UI Toolkit pilot that was tried
  and reverted after human feedback ("it is still bad"), then a Kenney "UI Pack - Adventure" (CC0)
  skin — all landed and green at the time (EditMode 137/137, PlayMode 49/49). Full detail preserved
  in git history on this branch; UI dept owns whether/how to carry it forward.
- **Match Shell Layout (2026-08-16, merged into master separately):** `CHARACTER_FANTASY.md` §4.1
  InfoBar field sheet — one combined bar (Attacker | Defender columns + shared phase/round/pool
  strip), wound ladder as Healthy/Wounded/Dead (not HP), signature/deck size marked OPEN pending
  C64. Integrator flag carried forward: needs a per-side `ArchetypeOf(pawnId)` reader before both
  InfoBar columns can be truthful (`SelectedArchetype` is local-only today) — **still not wired**;
  see "Rebase (2026-08-18)" below for this pass's disposition on that flag.
- **Rebase (2026-08-18):** rebased `feat/char-select-motion` onto master `f45e986` (110 commits:
  Match Shell Layout, Map, Camera, Storm counter, Healed presenter, camera control-hint UI chrome).
  Conflicts were docs-only (`PLAY_NOTES.md`, this STATUS file across several commits in the replay,
  plus add/add conflicts on `Assets/_Project/Art/UI/` files both branches happened to create) —
  resolved by keeping both sides' notes/history, no semantic changes. Code
  (`CharacterSelectView.cs` / `UiMotion.cs` / `UiStyle.cs` CharSelect* tokens /
  `AppFlowController.BuildCharacterSelect` / Kenney art) applied clean, no conflicts with Match
  Shell Layout or camera control-hint touches to `ProgramHud.cs` / `GearHandView.cs` (this branch
  never touched those files — no architectural collision found). The `ArchetypeOf(pawnId)` reader
  was **not** wired as part of this rebase — under the mandate shift below it's UI/Integrator scope
  regardless, and touching `ProgramHud`/InfoBar here would risk conflicting with work in flight on
  those files outside this worktree. Flagging forward rather than silently dropping it.

## Concept pack (complete for this wave)

| Doc | Role |
|-----|------|
| `CHARACTER_PLAN.md` | Roadmap + readiness checklist |
| `CHARACTER_FANTASY.md` | Cast fantasy + roster growth rules |
| `CHARACTER_CARDS_BOUNDARY.md` | Character↔Cards seam (Cards review ask) |
| `CHARACTER_C36_DEPENDENCY.md` | What Character needs from Core/C36 |
| `CHARACTER_TIME_PLAYER_EPISTEMICS.md` | C44 FF / blind-programming gate |
| `CHARACTER_DETONATOR_VS_BOMBER.md` | C38 vs C43 split |
| Four impl briefs (C42 / C43 / C44 / attrs) | Recommendation-not-contract |
| `CHARACTER_ROSTER_LONGTERM.md` | Design source + links |

## In progress

- Nothing drafting. Waiting on **human design answers** and Integrator org sync — not more stub volume.

## Waiting on human (when ready)

1. `CHARACTER_FANTASY.md` §6 — Scout/Jug pitches OK? Bomber wall-only name? Time Player rewind branding?
2. `CHARACTER_TIME_PLAYER_EPISTEMICS.md` §4 — Option **A** (rewind only) vs **B** (FF, no preview) → PRODUCT_MEMORY
3. Cards peer review of `CHARACTER_CARDS_BOUNDARY.md` (optional)

## Offers

- Idle on new stubs unless you name a gap.
- No code until you leave option C + Integrator contracts a slice.
- Commit the uncommitted docs pack when you ask.

## Integrator ask (unchanged)

Sync PARALLEL_OPS / INDEX / GDD §11 for Char Select → UI handoff; sequence C36 before unique-verb Sim.
