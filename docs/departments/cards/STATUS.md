# Cards — STATUS

**Wave / Day:** Permanent seat — Match Shell Layout schedule-language recommendation (2026-08-15)
**Branch / worktree:** `logiCard-cards-collection` / `feat/cards-collection-docs`
**Last cross-reviewed:** 2026-08-18 — rebased branch onto current `master` (`f45e986`, picking up
  C69–C73, Storm counter, Healed presenter, camera control-hint chrome, Match Shell Layout merges).
  New branch tip `9751af5` — docs-only, no `Assets/_Project/**`/`CardData.cs` touched. Conflicts:
  `MATCH_SHELL_LAYOUT_AGENT_BRIEF.md` (add/add — kept this branch's Cards-specific dispatch version,
  identical goal to master's generic UI brief but scoped to this seat) and `docs/DRAFT_HANDOFF.md`
  (content — took master's version wholesale, it's Integrator-owned and this seat's stale copy was
  many sessions behind). `docs/cards/CARD_COLLECTION.md` and `PRODUCT_MEMORY.md` auto-merged clean —
  their C68 content was already folded into master (per DRAFT_HANDOFF's "Docs peers folded in" note),
  so no renumbering was needed on this pass. Batchmode-verified fresh on the rebased tip, Editor
  closed: EditMode 190/190, PlayMode 62/62 — matches master's current baseline exactly. Branch is
  current against master; **not merged, not pushed** (Integrator's call). Prior: 2026-08-14 — merged
  master (Storm Sim / **C67**); renumbered packaging → **C68**; Storm `CARD_COLLECTION` +
  `GEAR_STORM_AGENT_BRIEF` written (Cards Storm DoD). No `CardData.cs` edit.

## Owned files (this seat)

- `docs/cards/CARD_COLLECTION.md`
- `docs/cards/CARD_SYSTEM_MODEL_COMPARISON.md`
- `docs/cards/CARD_SYSTEM_OPENS.md`
- `docs/cards/DECKBUILDER_SYSTEMS_BRIEF.md`
- `docs/cards/GEAR_STORM_AGENT_BRIEF.md` (**new** — numerics recommendation)
- `docs/core/PRODUCT_MEMORY.md` — **C68** (+ merge kept master's **C67** Storm)
- Gear briefs (Flashbang **paused**; Storm recommendation open for human lock)
- This STATUS

## Done

- Match Shell Layout §13 (`CARD_COLLECTION.md`): 5-card TimelineSchedule track/chip/Program-vs-Playback
  visibility table, HandBand vs schedule framing, ticket-stub/rubber-stamp/sky-wash presentation notes
- **C68** packaging (8/Character; Character-in-deck) on branch (was draft-C67; renumbered after master claimed C67 for Storm)
- Storm Cards DoD: catalog entry + `TR —` / **1× match** recommendation + one-line effectSummary

## In progress

- Waiting on Integrator **merge**; human lock of Storm numerics when ready

## Blocked

- Storm HUD / Atmosphere — **UI** / **Atmosphere** seats (contract)
- Deckbuilder UI/Sim — needs separate contract (**C68** does not greenlight)
- Bandage HUD — **UI seat**
- TimelineSchedule/HandBand shell build — **UI seat** (coding), per `MATCH_SHELL_LAYOUT.md` ownership table

## Offers / Integrator handoff

- Review + merge `feat/cards-collection-docs` (Match Shell Layout §13 + C68 packaging + Storm catalog/brief; C67 Storm already on master)
- After human confirms Storm TR/charges → follow-up C# amend (C62→C63 shape)
- Match Shell Layout §13 ready for UI seat to consume when building TimelineSchedule/HandBand regions
