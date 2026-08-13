# Character — STATUS

**Wave / Day:** Permanent seat — live `logiCard-char-select-motion`
**Branch / worktree:** `feat/char-select-motion` @ `D:\projects\Game\logiCard-char-select-motion`
**Brief:** `CHAR_SELECT_MOTION_AGENT_BRIEF.md` (worktree root, historical — see mandate shift below)
**Last cross-reviewed:** 2026-08-18 — rebased `feat/char-select-motion` onto current master
(`f45e986`). **Mandate shift (2026-08-13, unchanged by this rebase):** Character owns
behavior/abilities (GDD attrs + long-term unique-verb operators); Character Select UI/motion was
handed to **UI** dept partway through this branch's history — the carousel commits earlier in this
branch (carousel, glow polish, UI Toolkit pilot + revert, Kenney skin) remain mergeable history but
are no longer this seat's active scope. See "Rebase (2026-08-18)" note under Done for what the
rebase itself touched.

## Mandate (2026-08-13)

| Owns now | Does **not** own going forward |
|----------|--------------------------------|
| Scout/Juggernaut **attribute behavior** (Speed/Agility/Strength) | `CharacterSelectView.cs`, `CharSelect*` `UiStyle` tokens, Kenney char-select chrome, select motion/feel |
| Long-term unique-verb operators (**C42–C44**) — briefs first; Sim only after Integrator carve-out | Map Select carousel (**C59** stays UI/Integrator) |
| Character ability implementation briefs → future resolve tests against frozen contracts | Live `GhostResolver` edits while core Sim remains paused (except Integrator-granted carve-outs) |

**Integrator ask:** sync `docs/PARALLEL_OPS.md`, `docs/departments/INDEX.md`, and `GDD.md` §11 Character/UI
rows to this split (Character worker does not edit those Integrator-owned org docs).

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

## Owned files (this seat — behavior/docs)

- `docs/CHARACTER_UNIQUE_VERB_OPERATORS_BRIEF.md` — **C42**
- `docs/CHARACTER_BOMBER_AGENT_BRIEF.md` — **C43**
- `docs/CHARACTER_TIME_PLAYER_AGENT_BRIEF.md` — **C44**
- `docs/CHARACTER_ATTRS_SCOUT_JUGGERNAUT_BRIEF.md` — live cast attrs audit + wiring recommendation
- `docs/CHARACTER_ROSTER_LONGTERM.md` — design source (cross-links to briefs)
- `Assets/_Project/Characters/**` (`CharacterData`, Scout/Juggernaut assets) — attrs data authority
- This STATUS

## Handed to UI (do not touch from this seat)

- `Assets/_Project/UI/CharacterSelectView.cs` (+ `.meta`)
- `Assets/_Project/UI/UiMotion.cs` (if still only used by char-select; UI may reclaim or share)
- `UiStyle` `CharSelect*` tokens
- `Assets/_Project/Art/UI/**` Kenney CharSelect sprites / `UiKenneyImportTool` / `THIRD_PARTY.md` (char-select skin)
- `PLAY_NOTES.md`, `CHAR_SELECT_MOTION_AGENT_BRIEF.md` — historical Play gate for the carousel branch tip
- Prior carousel commits on this branch remain mergeable history; UI/Integrator decide merge vs re-home

## Done (this session)

- Committed dirty pickup STATUS note (`a707d9f`).
- Left untracked `screenshots/image copy 13.png` (human keep/delete).
- Wrote four implementation briefs (Bandage-shaped: locked / repo reality / open questions /
  recommended resolve+HUD / contract split / non-goals). **No Sim/resolver code.**
- Key finding in attrs brief: **C25 Agility penalties exist on `CharacterData` assets but are never
  read by `PawnProgram`** — Speed/Door knobs partially wired; Agility unwired.

## In progress

- Nothing coding-hot. Awaiting human answers on brief §3 questions (especially Time Player
  fast-forward leak, Bomber walls-vs-floor, GDD §2 vs §6 speed framing) and Integrator org-doc sync +
  any future Sim carve-out.

## Blocked

- Unique-verb Sim work blocked on: (1) long-term status / phase promotion, (2) C36 geometry-breach
  primitives, (3) Bomber floor-drop → per-floor infra, (4) Time Player epistemic answer, (5) explicit
  Sim pause carve-out.
- Attrs Agility wiring blocked on Integrator carve-out (Sim/Timeline-adjacent) even though design is
  already confirmed in **C25**.

## Depends on

- Integrator: update PARALLEL_OPS / INDEX / GDD §11 ownership; optionally merge or re-home
  `feat/char-select-motion` UI commits under UI seat after human Play.
- Human: greenlight open questions in the four briefs before any Character Sim contract opens.

## Offers

- Idle on code. Next useful Character slice when asked: deepen a single brief from human answers, or
  (only with Sim carve-out) wire Scout/Juggernaut Agility per attrs brief recommendation.
