# MATCH_SHELL_LAYOUT — Cards seat brief (docs)

**From:** Integrator  
**To:** Cards (`D:\projects\Game\logiCard-cards-collection`)  
**Wave:** Match Shell Layout — **docs / recommendations only** (no Unity HUD code)  
**Read:** `docs/ui/MATCH_SHELL_LAYOUT.md` (on master after Integrator lands it; or ask human to copy),
refs `screenshots/image copy 18.png` / `19.png` on main.

## Goal

Recommend how **gear / play cards** show up in the new **TimelineSchedule** (YOU / ENEMY / EFFECTS)
and what the **HandBand** should communicate vs the schedule — without inventing new Sim verbs.

## Deliverable

One short section appended to an existing Cards doc (prefer `docs/cards/CARD_COLLECTION.md` or
`DECKBUILDER_SYSTEMS_BRIEF.md` — do not invent a new root doc). Cover:

1. Per first-wave card (Bandage, Interact, Flashbang, Adrenaline, Storm): which schedule **track**
   (YOU / ENEMY / EFFECTS), chip label, and whether Program vs Playback visibility differs.
2. Hand vs schedule: hand = “what I can still play”; schedule = “what I already booked this round.”
3. Playful presentation notes (ticket stub / toy block) that fit Desk-Lamp — no code.

## Do not

- Edit `Assets/_Project/UI/**`, `ProgramHud`, `GearHandView`.
- Change `CardId` / Sim costs.
- Merge / push.

## Done when

STATUS updated + Integrator pinged with doc path + 5–10 line summary.
