# Cards — STATUS

**Wave / Day:** Card collection design research (docs-only) — 2026-08-12
**Branch / worktree:** `feat/cards-collection-docs` @ `D:\projects\Game\logiCard-cards-collection`
**Brief:** Paste brief (this session) — expand `docs/CARD_COLLECTION.md`, add this STATUS file
**Last cross-reviewed:** 2026-08-12 — session start (DRAFT_HANDOFF / PARALLEL_OPS / PRODUCT_MEMORY C15,C18,OPEN#3 / MONETIZATION / PLAYBACK_CONTRACT read)

**In progress.**

## Owned files (this wave)

- `docs/CARD_COLLECTION.md` — expanded, not new
- `docs/departments/cards/STATUS.md` (this file)

## Done

- Expanded `docs/CARD_COLLECTION.md` with:
  - §3A player-facing glossary (one-page, plain-language, distinct from §3's dev vocabulary)
  - §6A strawman charge table for Bandage / Flashbang / Adrenaline / Interact-as-card, numerics marked OPEN
  - §5A explicit "same gear deck (C18)" vs unique-verb Characters (C42–C44) boundary + test
  - §11 recommended first-ship build sequence (proposal only, resolve-risk ordered)
- No new card names invented beyond the four named + Otherwise family; nothing added outside "proposal only" framing.
- Did not touch `PRODUCT_MEMORY.md`, `DRAFT_HANDOFF.md`, `Boot/`, UI code, or weather — docs-only, no Unity gameplay code, no HUD strip, no Sim verbs.

## In progress

- Nothing further in this slice.

## Blocked

All work beyond this research draft is blocked on human answers to `CARD_COLLECTION.md` §8:

1. **Catalog scope** — four named only, four + minimal Otherwise, or a larger named list?
2. **Same deck?** — keep C18 as-is, amend for per-Character exclusives, or shared cards + attr-scaled cost (§5 option B)?
3. **In-match economy** (OPEN #3) — full hand + charges, draw-per-Program, pre-match loadout ≤K, or hybrid?
4. **Meta collection** — none this milestone, cosmetic binder only, or unlock-to-use gear (⚠️ P2W risk flagged against C47)?
5. **Interact-as-card vs contextual Door/Vent** — keep Door/Vent/Breach as map actions and reserve Interact-card for future stations, migrate some interacts to the hand, or defer entirely?
6. **Adrenaline** — keep Execute-only stub, or design a real effect now (must state whether the tape may branch, per `PLAYBACK_CONTRACT` §2 rule 5)?

Do **not** ask Integrator for a C# row until these land — per brief, this wave stops at research/proposal.

## Depends on

- Human answers to the six §8 questions above.
- After that: Integrator promotes confirmed answers into `PRODUCT_MEMORY.md` (save-file rule) before any C# row or Sim/HUD work starts.

## Offers

- Ready to split `CARD_COLLECTION.md` into `GEAR_CATALOG.md` + `CARD_ECONOMY.md` once §8 is answered (doc's own §9 ownership table already proposes this).
