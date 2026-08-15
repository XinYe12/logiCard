# Character — STATUS

**Wave / Day:** Permanent seat — live `logiCard-char-select-motion`
**Branch / worktree:** `feat/char-select-motion` @ `D:\projects\Game\logiCard-char-select-motion`
**Last cross-reviewed:** 2026-08-18 — rebased `feat/char-select-motion` onto current master
(`f45e986`); otherwise (1) 2026-08-15 decision-sheet Part A closed, walk still paused at **B1**,
(2) Match Shell Layout **InfoBar field sheet** landed in `CHARACTER_FANTASY.md` §4.1. Char Select
UI/motion carousel work earlier in this branch's history was handed to **UI** dept partway through
(see Mandate below) and remains mergeable history but is no longer this seat's active scope. See
the Rebase entry under Done for what this pass touched.

## Mandate

| Owns | Does not own |
|------|----------------|
| Character fantasy, attrs meaning, unique-verb concepts + impl briefs | Char Select UI → **UI** |
| Joint boundary with Cards | Gear catalog → **Cards** |
| Future resolve only after Integrator carve-out | Live Sim under general pause |
| InfoBar **field meaning** (Match Shell) | InfoBar / shell chrome → **UI** |

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
| `CHARACTER_FANTASY.md` | Cast fantasy + roster growth rules + **§4.1 InfoBar field sheet** |
| `CHARACTER_CARDS_BOUNDARY.md` | Character↔Cards seam (Cards review ask) |
| `CHARACTER_C36_DEPENDENCY.md` | What Character needs from Core/C36 |
| `CHARACTER_TIME_PLAYER_EPISTEMICS.md` | C44 FF / blind-programming gate |
| `CHARACTER_DETONATOR_VS_BOMBER.md` | C38 vs C43 split |
| Four impl briefs (C42 / C43 / C44 / attrs) | Recommendation-not-contract |
| `CHARACTER_ROSTER_LONGTERM.md` | Design source + links |
| [`CHARACTER_DECISION_SHEET.md`](CHARACTER_DECISION_SHEET.md) | Human pass across all OPEN questions |

## In progress

- Decision-sheet walk with human — **paused at B1** (Part A fully closed: A1–A5). B1 not yet answered.
- 2026-08-15: agent legwork pass added an **Agent recommendation** column across every still-open
  row (B1–B9, C2–C7, D1–D6) so the human's resume pass is pick-one-or-override instead of a
  cold read. Nothing promoted; Decision column untouched by the agent for open rows.

## Done this session (Match Shell Layout — docs)

- **InfoBar field sheet** appended to [`CHARACTER_FANTASY.md`](../../CHARACTER_FANTASY.md) **§4.1**.
  - Recommend **one combined bar** (Attacker | Defender columns + shared phase/round/pool strip).
  - Fields: side label (C18), archetype display name, wounds as Healthy/Wounded/Dead (not HP), shared match TR pool + round + phase.
  - Signature/deck size marked **OPEN** (C64, post-demo).
  - Integrator flag: need per-side `ArchetypeOf(pawnId)` — `SelectedArchetype` is local-only today.

## Waiting on human (when ready)

1. Resume [`CHARACTER_DECISION_SHEET.md`](CHARACTER_DECISION_SHEET.md) at **B1** (Part A is done), then continue through the rest of Part B, Part C, Part D — agent recommendations are there to accept or override, not a substitute for the Decision column.
2. `CHARACTER_FANTASY.md` §6 — Scout/Jug pitches OK? Bomber wall-only name? Time Player rewind branding?
3. `CHARACTER_TIME_PLAYER_EPISTEMICS.md` §4 — **C1 answered as option C** on the sheet; still needs narrative-framing sync / PRODUCT_MEMORY promotion with Integrator.
4. Cards peer review of `CHARACTER_CARDS_BOUNDARY.md` (optional)
5. Confirm or override InfoBar §4.1 recommendations (combined bar / wound ladder / no mana).

## Offers

- Resume “walk the sheet” with simple-language prompts (still parked at B1).
- No code until sheet done + Integrator contracts a slice.
- Commit dirty docs (+ STATUS) when you ask.

## Integrator ask

1. *(unchanged until decision sheet done)* Sync PARALLEL_OPS / INDEX / GDD §11 for Char Select → UI handoff; after answers land, promote to PRODUCT_MEMORY and sequence C36 before Bomber Sim.
2. **Match Shell InfoBar:** UI may bind against §4.1; Integrator should plan a per-side archetype reader before both columns go live (see §4.1 flags).
