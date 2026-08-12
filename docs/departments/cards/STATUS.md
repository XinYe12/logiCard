# Cards — STATUS

**Wave / Day:** Card collection design research (docs-only) — 2026-08-12
**Branch / worktree:** `feat/cards-collection-docs` @ `D:\projects\Game\logiCard-cards-collection`
**Brief:** Paste brief (this session) — expand `docs/CARD_COLLECTION.md`, add this STATUS file
**Last cross-reviewed:** 2026-08-12 — human answered §8 in chat; doc updated same session

**In progress — §8 answered, awaiting Integrator promotion to PRODUCT_MEMORY.**

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
- **Human answered §8 in chat, 2026-08-12** — all six questions resolved, accepting Cards dept's recommendation as-is:
  1. Catalog = four named only, no Otherwise this wave.
  2. Same deck (C18) kept, with one scoped carve-out: Interact-as-card cost may scale by Strength (Door precedent); Bandage/Flashbang stay flat.
  3. Economy = full hand + per-card charges (model 1); OPEN #3 resolved.
  4. Meta collection = none this milestone; unlock-to-use gear rejected outright, not just deferred.
  5. Interact-as-card reserved for future stations only; Door/Vent/Breach untouched.
  6. Adrenaline stays Execute-only stub; real effect is a separate later design pass.
  Recorded inline in `CARD_COLLECTION.md` §8 and §10.

## In progress

- Nothing further in this slice — recording done; next action belongs to Integrator (see Depends on).

## Blocked

- Not blocked on more human input. Blocked on **Integrator** writing the `PRODUCT_MEMORY.md` C# row that promotes these §8 answers per the save-file rule (`PRODUCT_MEMORY.md` §How to update: "Confirm in chat → edit C# row") — Cards dept does not edit `PRODUCT_MEMORY.md` per brief boundary.
- No Sim/HUD/C# code work starts until that row lands.

## Depends on

- Integrator: promote the six §8 answers into a `PRODUCT_MEMORY.md` C# row, then this wave is ready for a real implementation brief (Sim verb / resolver work, HUD gear strip) — none of that is Cards dept's file scope either; those go to whichever dept/worker Integrator assigns per `PARALLEL_OPS.md`.

## Offers

- Ready to split `CARD_COLLECTION.md` into `GEAR_CATALOG.md` + `CARD_ECONOMY.md` now that §8 is answered (doc's own §9 ownership table already proposes this) — say the word if Integrator wants that split done now vs. deferred to whoever picks up the C# row.
