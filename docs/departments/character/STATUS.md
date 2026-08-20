# Character — STATUS

**Wave / Day:** Permanent seat — live `logiCard-char-select-motion`
**Branch / worktree:** `feat/char-select-motion` @ `D:\projects\Game\logiCard-char-select-motion`
**Last cross-reviewed:** 2026-08-18 — rebased `feat/char-select-motion` onto current master
(`f45e986`); otherwise (1) 2026-08-16 **decision sheet fully answered** — human accepted every
Agent recommendation across Part A–D in one pass, promoted to `PRODUCT_MEMORY.md` **C70–C73**,
(2) Match Shell Layout **InfoBar field sheet** landed in `CHARACTER_FANTASY.md` §4.1 (merged to
master 2026-08-16, separately from this branch). Char Select UI/motion carousel work earlier in
this branch's history was handed to **UI** dept partway through (see Mandate below) and remains
mergeable history but is no longer this seat's active scope. See the Rebase entry under Done for
what this pass touched.

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
- **Rebase (2026-08-18):** rebased `feat/char-select-motion` onto master, ending at tip `7530f3d`
  (past 110+ commits: Match Shell Layout, Map, Camera, Storm counter, Healed presenter, camera
  control-hint UI chrome, and an Integrator Cards-docs rebase that landed mid-session). Conflicts
  were docs-only (`PLAY_NOTES.md`, this STATUS file across several commits in the replay,
  `docs/DRAFT_HANDOFF.md`, an add/add filename collision on root `MATCH_SHELL_LAYOUT_AGENT_BRIEF.md`
  — this branch's own Character-facing brief kept, master's differently-scoped UI-facing brief of the
  same name dropped, flagged below — plus add/add conflicts on `Assets/_Project/Art/UI/` files both
  branches happened to create independently) — resolved by keeping both sides' notes/history, no
  semantic changes. Code (`CharacterSelectView.cs` / `UiMotion.cs` / `UiStyle.cs` CharSelect*
  tokens / `AppFlowController.BuildCharacterSelect` / Kenney art) applied clean, no conflicts with
  Match Shell Layout or camera control-hint touches to `ProgramHud.cs` / `GearHandView.cs` (this
  branch never touched those files — no architectural collision found). Batchmode verified on the
  rebased tip: **EditMode 190/190, PlayMode 63/63**, both green (master baseline is 190/190 and
  62/62 — the +1 PlayMode test is this branch's own carousel coverage,
  `CharacterSelectNextRotatesArchetypeAfterCrossfade`). The `ArchetypeOf(pawnId)` reader was **not**
  wired as part of this rebase — under the mandate shift below it's UI/Integrator scope regardless,
  and touching `ProgramHud`/InfoBar here would risk conflicting with work in flight on those files
  outside this worktree. Flagging forward rather than silently dropping it. **Also flagging for
  Integrator:** root `MATCH_SHELL_LAYOUT_AGENT_BRIEF.md` now exists identically-named but
  differently-scoped on master (UI-seat brief, unrelated to this branch's Character-seat brief of
  the same name) — a real naming collision this rebase had to pick a winner for, worth moving one or
  both into `docs/departments/<seat>/` to avoid recurring.

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

- Nothing. ~~Next real step is Integrator opening the actual C36 geometry-breach contract~~ — **done
  2026-08-20** ("character, GO"): C36 primitive + Bomber wall-only verb Sim layer landed on master, see
  `docs/contracts/CURRENT.md`'s open C36/Bomber section. Still not a Character task — RoundPlayback
  presenter, map authoring, and HUD are the next pieces, owned by Integrator/UI respectively; Bomber's
  own Sim resolve body (attach/detonate legality, Character-gating) is Character's slot once a Sim
  carve-out exists for it specifically (the primitive landing doesn't auto-open that). Time Player still
  depends on this same C36 primitive per C70/A1's build order and is otherwise untouched.

## Backlog — pawn art bugs found 2026-08-20 (human: log and move on, not urgent)

Exposed by the new Character Select card preview (`docs/ui/UI_SHELL_CHROME.md`'s render-texture rig —
close-up card scale shows what top-down board scale always hid). Real board bugs, not UI bugs:

1. **Scout's face/hands render bright orange-red.** `PawnView`'s team-tint targets a mesh part named
   `"Body"` (`TintedPartNameMarker`), but on this model `"Body"` is the *skin*, not the jacket/torso —
   the tint always hits Scout's skin on the actual board too, just unnoticed at board scale.
2. **Juggernaut's prefab has a rabbit-ears hat mesh enabled.** The "Breacher" archetype visibly wears
   bunny ears. Same story — invisible top-down, obvious close-up.
3. Also noted in passing: both prefabs carry an `Animator` with no controller, so board and card both
   show the bind/T-pose. Not logged as a bug (no idle animation was ever promised), but a one-clip idle
   would read a lot better in the card and belongs on this same pawn-art track since the board would
   benefit too.

Human said "log and move on" (2026-08-20) — not urgent, pick up whenever this seat is next restaffed
for pawn art.

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
