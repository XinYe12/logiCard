# Match Shell Layout — vertical stack (2026-08-15)

**Status:** Human-directed layout target. **Layout / region geometry only** this wave — do not restyle
individual card faces, buttons, or icons yet.  
**Refs:** `screenshots/image copy 18.png` (expanded schedule timeline), `screenshots/image copy 19.png`
(collapsed timeline strip). Use these for **region order + timeline-as-schedule feel**, not for
Hearthstone minion lanes.  
**Amends in practice:** `UI_FLOW.md` §4 “HUD dock in margins” stacking — pending PRODUCT_MEMORY C-row
(provisional until human confirms → Integrator writes C#). **C48 landscape canvas stays** unless human
reopens portrait; the stack is full-width horizontal bands inside the existing desktop window.

## Locked vertical order (top → bottom)

| # | Region id | Role | logiCard content (not mock fiction) |
|---|-----------|------|-------------------------------------|
| 1 | **InfoBar** | Read-only match identity / status | Phase label, round, wounds, Time Resource remaining / used, Attacker–Defender labels. **Not** fantasy HP/mana from the mock. |
| 2 | **MapViewport** | Our diorama board + pawns | Existing continuous board (`BoardView` / camera). **Reject** mock “battleground” minion rows. Board clicks (waypoints, aim, door) stay here. |
| 3 | **HandBand** | Gear / play hand | Existing `GearHandView` fan + drag-to-play. |
| 4 | **ToolBar** | Verb / stance / primary actions | Move / Shoot / Door, stance, Snap/Hold, Lock In, transport (Play/Rewind/Undo), Adrenaline when Execute. Replaces the old 3-column dock’s controls + action columns. |
| 5 | **TimelineSchedule** | Time Resource cinema as a **schedule** | Playhead = scrubber second. Rows = schedule tracks (see below). Replaces the old dock scrubber + queue-log strip. |

```
┌─────────────────────────────────────────┐
│ InfoBar                                 │  ~6–8% height
├─────────────────────────────────────────┤
│                                         │
│ MapViewport  (diorama — not HS lanes)   │  ~48–55% height
│                                         │
├─────────────────────────────────────────┤
│ HandBand          (fan)                 │  ~14–18%
├─────────────────────────────────────────┤
│ ToolBar           (verbs / Lock In)     │  ~10–12%
├─────────────────────────────────────────┤
│ TimelineSchedule  (playful schedule)    │  ~14–18%
└─────────────────────────────────────────┘
```

Fractions are **strawman** — UI seat may tune ±4% as long as MapViewport stays the single largest region
and TimelineSchedule never collapses under ~12% when expanded.

## Timeline — schedule UI, make it playful

** mechanize first, toy later:** wire real data into a readable multi-row track; chrome can stay stub.

| Track | Maps to |
|-------|---------|
| **YOU** (own side) | Own `PawnProgram` nodes / tape events for the local pawn (Move legs, Shoot, Door, Bandage, Storm, …) as colored blocks on the Time Resource axis |
| **ENEMY** | Opponent program once revealed / during Playback (ghost tape) — Program phase may show empty or “locked” until Reveal |
| **EFFECTS** | Cross-cutting: wounds, weather (`StormCast`), door state flips, future channel — not unit minions |

**Playful (target feel, not this-wave polish):** ticket / toy-block chips (collection: `normal-card`,
`resource-bank-card-flip`, comic hand swatches), soft playhead diamond (mock gold line), expand/collapse
like image 18 ↔ 19 (expanded grid vs “▼ TIMELINE ▼” strip). **This wave:** expanded schedule shell +
playhead scrubbing working; collapse toggle optional nice-to-have.

**Out of scope this wave:** inventing a 12-tick clock (C28 continuous TR); Hearthstone mana orbs; mock
card names.

## Explicit rejects

- Hearthstone **minion battlefield** rows replacing the 3D map.
- Moving the map below the hand.
- Putting Timeline above the hand.
- Restyling every chrome component before the shell regions exist.

## Ownership (this wave)

| Concern | Owner |
|---------|--------|
| Shell bands + move existing HUD widgets into regions | **UI** (coding) |
| What InfoBar fields mean for Characters | **Character** (docs recommend) |
| Hand ↔ schedule card-block language | **Cards** (docs recommend) |
| Map framing inside MapViewport | **Map** (docs) + **Camera** slice / Integrator (code when rect frozen) |
| Weather stays inside map read, not full-bleed HUD | **Atmosphere** (docs + light check) |
| Contract + merge + Boot/Playback playhead binding | **Integrator** |

## Relation to HUD Chrome Ship Pass (`3f77b6c`)

Chrome dock panels were built for the **old** 3-column bottom dock. **Layout wave supersedes merge
priority** — do not merge chrome alone. UI continues from `feat/modal-restyle` @ `3f77b6c` and
rebuilds panels into the new bands; reuse `UiFactory.CreateBackingPanel` / `DockPanel*` tokens where
they still fit.
