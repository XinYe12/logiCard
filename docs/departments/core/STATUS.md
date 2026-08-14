# Core / Integrator — STATUS

**Wave / Day:** Phase 5 — Integrator 2026-08-14
**Branch / worktree:** `master` — clean, all Storm-card work merged
**Last cross-reviewed:** 2026-08-14 — reviewed and merged Cards, Atmosphere, and UI's Storm-contract
report-backs (checked actual diffs, not just reports — caught and fixed real issues in two of the three,
see Done)

## Done

- **Storm card (C67)** — human directly asked for a playable card that spawns the already-shipped storm
  weather over the board. Wrote C67, landed the Sim-side myself (`ActionVerb.Storm`, `TapeEventType.
  StormCast`, permissive `GhostResolver` emission, `RoundPlayback.SyncWeatherToSeconds` continuous
  presenter, `GameBootstrap` boot mood flipped Storm→Fair so the card is a visible change), pre-landed
  `CardId.Storm = 4` to avoid a Cards/UI cross-worktree race, opened a 3-way Cards+UI+Atmosphere contract.
- **Cards' Storm DoD** — merged (`a925fd5`): `CARD_COLLECTION.md` entry + `GEAR_STORM_AGENT_BRIEF.md`
  (TR —, 1×/Character/match recommended, numerics OPEN). No `CardData.cs` edit. Also brought in **C68**
  (Character 8-card play-deck packaging — separate human-directed work on the same branch); verified
  Cards correctly renumbered their own draft-C67 to C68 after seeing master had already claimed C67.
- **Atmosphere's Storm DoD — real complication found and resolved.** Their branch mixed the actual DoD
  fix (same-mood `ApplyWeather` early-out + `ApplyStormLightingDim` clean-baseline-on-restore) with an
  unrelated, uncoordinated "Sunny weather mood" feature (new `BoardWeatherMood` value, boot default
  changed Fair→Sunny, renamed lighting fields) that wasn't in their report. Asked the human rather than
  guessing; confirmed Sunny mode should NOT merge. Ported just the two real fixes onto master's actual
  (non-Sunny-refactored) code by hand (`c051731`), plus two portable PlayMode tests that had no Sunny
  dependency. Sunny-mode work stays uncommitted in that worktree pending a separate decision.
- **UI's Bandage HUD-side (C63) + Storm HUD-side (C67) — both closed, merged together** (`be8ac46`):
  `PawnProgram.TryQueueStorm` correctly mirrors the real `TryQueueDoor`/`TryQueueShoot` guard-first shape
  (not a hypothetical); `BoardInputController.TryQueueStormAt`; `GearHandView` 5th slot; `ProgramHud`
  arm/place/legality wiring generalized for both cards; the `GearHandViewTests` roster test I'd flagged
  was fixed exactly as predicted. Honestly flagged one real deviation (Storm's once-per-match gate is
  per-round only, no cross-round counter) rather than claiming full compliance — accepted as fine for
  this wave (TR cost is 0, recast is a harmless no-op). UI reported a real batchmode run on their own
  worktree: Bandage 153/153 EditMode / 49/49 PlayMode; combined with Storm, 166/166 / 51/51.
- Prior same-day work: dirty rematch/floors/lighting committed; **C65** (C53 surface-material amendment)
  written; Map Phase 2 contract opened and merged (`a76f006`, human Play-signed); Atmosphere's earlier
  storm-look merge (`668b162`).

## In progress

- Batchmode re-verify the combined tip on `master` — UI ran batchmode on their own worktree and reported
  green, but nothing has been independently re-run on the merged combination. Editor must be closed on
  this exact path first.
- Healed presenter (`TapeEventType.Healed` in `RoundPlayback` + `PLAYBACK_CONTRACT` §3) — not started.

## Offers

- Run/coordinate a combined batchmode verification pass.
- Healed presenter follow-up.
- Atmosphere's Sunny-mode work needs a separate decision (merge as its own feature, or drop) — not
  blocking anything, just sitting uncommitted.
- Map idle — restaff for a prop/dressing follow-up if wanted. Character idle until human answers briefs.
