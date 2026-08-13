# Character — STATUS

**Wave / Day:** Permanent seat — live `logiCard-char-select-motion`
**Branch / worktree:** `feat/char-select-motion` @ `D:\projects\Game\logiCard-char-select-motion`
**Last cross-reviewed:** 2026-08-13 — **mandate shift:** Character owns behavior/abilities (GDD attrs +
long-term unique-verb operators); Character Select UI/motion handed to **UI** dept.

## Mandate (2026-08-13)

| Owns now | Does **not** own going forward |
|----------|--------------------------------|
| Scout/Juggernaut **attribute behavior** (Speed/Agility/Strength) | `CharacterSelectView.cs`, `CharSelect*` `UiStyle` tokens, Kenney char-select chrome, select motion/feel |
| Long-term unique-verb operators (**C42–C44**) — briefs first; Sim only after Integrator carve-out | Map Select carousel (**C59** stays UI/Integrator) |
| Character ability implementation briefs → future resolve tests against frozen contracts | Live `GhostResolver` edits while core Sim remains paused (except Integrator-granted carve-outs) |

**Integrator ask:** sync `docs/PARALLEL_OPS.md`, `docs/departments/INDEX.md`, and `GDD.md` §11 Character/UI
rows to this split (Character worker does not edit those Integrator-owned org docs).

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
