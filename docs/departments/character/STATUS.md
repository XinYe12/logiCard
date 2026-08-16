# Character — STATUS

**Wave / Day:** Permanent seat — live `logiCard-char-select-motion`
**Branch / worktree:** `feat/char-select-motion` @ `D:\projects\Game\logiCard-char-select-motion`
**Last cross-reviewed:** 2026-08-16 — (1) **Decision sheet fully answered** — human accepted every Agent recommendation across Part A–D in one pass; promoted to `PRODUCT_MEMORY.md` **C70–C73**. (2) Match Shell Layout **InfoBar field sheet** landed in `CHARACTER_FANTASY.md` §4.1 (merged to master 2026-08-16).

## Mandate

| Owns | Does not own |
|------|----------------|
| Character fantasy, attrs meaning, unique-verb concepts + impl briefs | Char Select UI → **UI** |
| Joint boundary with Cards | Gear catalog → **Cards** |
| Future resolve only after Integrator carve-out | Live Sim under general pause |
| InfoBar **field meaning** (Match Shell) | InfoBar / shell chrome → **UI** |

**Mode:** Pre-code. See [`CHARACTER_PLAN.md`](../../CHARACTER_PLAN.md).

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

- Nothing — decision sheet is done. Next real step is Integrator opening the actual C36 geometry-breach
  contract (both Bomber and Time Player depend on it per C70/A1's build order), not a Character task.

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
