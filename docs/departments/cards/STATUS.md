# Cards — STATUS

**Wave / Day:** Permanent seat — research wave **merged** + **C62** promoted 2026-08-12; Bandage
implementation brief drafted 2026-08-13
**Branch / worktree:** `logiCard-cards-collection` / `feat/cards-collection-docs`, rebased onto master
(`77831cf`) — no divergence, docs continue on this branch until next merge
**Last cross-reviewed:** 2026-08-13 — wrote `GEAR_BANDAGE_AGENT_BRIEF.md`, cross-checked existing
`Assets/_Project/Cards/*.asset` scaffolding against C62/§6A

## Owned files (this seat)

- `docs/CARD_COLLECTION.md` (on master; Cards may continue edits when INDEX assigns)
- `docs/GEAR_BANDAGE_AGENT_BRIEF.md` (new — awaiting Integrator review)
- This STATUS
- Future gear UI/data only when contracted

## Done

- Expanded `CARD_COLLECTION.md` (glossary, charge strawman, C18 vs unique-verb boundary, §11 sequence)
- Human §8 answers recorded (`68c48bb`)
- Integrator promoted **C62**; OPEN #3 resolved; docs branch merged to master
- Wrote `GEAR_BANDAGE_AGENT_BRIEF.md`: focused implementation brief for Bandage (first C62 first-wave
  card). Docs-only, no Sim/resolve code. Surfaces that the existing `Assets/_Project/Cards/*.asset`
  scaffolding is stale/pre-C62 (Interact.asset still describes migrating Door/Vent onto the hand, which
  §8 Q5 explicitly forbids; Flashbang.asset's numbers diverge from §6A's strawman; `CardData.oncePerMatch`
  is a bool that can't encode multi-charge cards) and that Bandage's own asset encodes an undocumented
  "must be stationary" constraint that needs a real design answer, not an assumption.

## In progress

- Brief awaiting Integrator review — five open questions in the brief's §3 (cost/charges, "stationary"
  legality rule, per-match vs per-round charge persistence, already-Healthy legality, board-anchored-UI
  applicability) need human/Integrator answers before a Sim/HUD contract can be frozen. Also needs the
  standing core-gameplay/Sim pause (`SCHEDULE.md` Phase 2) explicitly carved out for gear work, same way
  **C57** carved it out for map/terrain — not assumed lifted by this brief.

## Blocked

- Gear Sim/HUD contract blocked on: (1) human answers to the brief's §3 open questions, (2) an explicit
  pause carve-out for gear work. Interact-as-card additionally blocked on a station target existing;
  Adrenaline real effect blocked on the PLAYBACK_CONTRACT tape-branch redesign (later, per C62/§11).

## Offers

- Split `CARD_COLLECTION.md` → `GEAR_CATALOG.md` + `CARD_ECONOMY.md` if Integrator wants
- Write the equivalent brief for Interact-as-card, Flashbang, or Adrenaline next, in §11's suggested order,
  once Bandage's brief has been reviewed
- Reconcile the stale `Assets/_Project/Cards/*.asset` scaffolding (Interact/Flashbang numbers, `oncePerMatch`
  schema) if Integrator wants that done ahead of any card's actual contract
