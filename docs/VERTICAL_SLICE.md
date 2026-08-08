# Vertical Slice Spec

**Doc ID:** D7  
**Status:** SHIPPED — 2026-08-08. This bring-up slice is complete; the core loop it proved out (Time Card,
path/stance Move, Snap/Hold Shoot, wounds/death, doors, playback) is implemented, playable, and is now the
shipping product's core loop (**C46**). Kept as historical reference for how the pipeline was proven, not a
forward-looking target — see `docs/SCHEDULE.md`'s phase table for what's actually next. Prior: 2026-07-30 C34
Polished Core Demo (superseded).  
**Depends on:** [CORE_LOOP.md](CORE_LOOP.md), [GDD.md](GDD.md), [SCOPE.md](SCOPE.md), [ART_DIRECTION.md](ART_DIRECTION.md)

**Purpose (historical):** Cut the original 14-day demo build to a **pipeline proof**, then a **tight combat core**, then a **protected diorama art pass** — not an ever-growing GDD feature pile. All three stages are done.

---

## The minimal proof (A → B → C) — Slice 1

**A.** Player schedules a **Move** on their personal timeline (even a single-tile path counts — path-drawing polish is not required yet). Base verb, not a card.  
**B.** Player schedules a **Shoot** (aim + time → Snap Shot mode is enough) on the timeline, aimed down LoS. Base verb, not a card.  
**C.** Time Card Allot → Lock → Reveal → Host resolves continuous **Time Resource** → **Playback** (ReplayTape; duration may be compressed):
- Character **visibly moves** to the destination tile on the board.
- Character **visibly performs the shoot action**, and if the target is in LoS at that Time Resource instant, the hit/miss + Wound rule from GDD applies.
- Player can start a **second round** via Aftermath → Time Card (**C33**).

**Pass/fail bar:** a first-time observer watching the **timeline scrubber** (Time Resource), with no narration, can say *"that scheduled Move made them move"* and *"that scheduled Shoot made them shoot"* — even if Playback Duration is shorter than Time Resource.

---

## Stub vs. real for Slice 1

| Piece | Stub OK for Slice 1 | Must be real for Slice 1 |
|---|---|---|
| Path drawing UI | Simplified (click-to-set one destination tile) | Path executes at the correct Time Resource second |
| Stance bands (Sprint/Walk/Crawl) | Hardcode a single stance | — (added Core Combat) |
| Shoot modes (Snap/Hold) | Hardcode Snap Shot only | Snap Wound-on-hit fires correctly |
| Map | Single 5×5, no attic/vent/monitor | Grid movement + collision |
| Networking | Local / scripted opponent | Time Resource ordering deterministic |
| Art | Temporary primitives OK **until art pass** | Move vs Shoot visually distinct |
| Wound/Win state | Stub text ("Wounded"/"Dead") is fine | — |
| Playback Duration | May be compressed vs Time Resource | Observer still reads correct **order** on scrubber |
| Match loop | — | Time Card Allot + Aftermath → next round |

---

## Milestone ordering inside the 14-day demo (C34)

1. **Slice 1 — Pipeline proof:** Time Card + scheduled Move + Snap Shoot → Playback → second round. Local/scripted OK.  
2. **Core Combat:** waypoint path + stance bands; Snap vs Hold Angle; wounds/death readability; **one door** that changes move/LoS. Local match playable end-to-end.  
3. **Diorama Art Pass:** URP/lighting, board dressing, **线稿涂鸦** paths, cardstock Time Card, clay pawns, stepped motion, physical VFX, tactile audio — required ship floor (**ART_DIRECTION**).  
4. **Ship:** Windows candidate, playtest, capture video, README. Optional Android smoke only if time remains.

Win/Android dual polish and Fusion are **not** required for this demo’s ship bar (**C34**).

---

## Pass/fail checklist (Slice 1)

- [ ] Player can play a Time Card and enter Program (**C33**).  
- [ ] Player can schedule a path (minimal is fine) during Program — base Move verb.  
- [ ] Player can schedule a Shoot action (aim + time, Snap mode) — base verb, no card.  
- [ ] Lock / Reveal / Execute / Aftermath transitions work; second round is possible.  
- [ ] Timeline scrubber advances through Time Resource and fires ops at the correct scheduled **seconds**.  
- [ ] Character moves to the correct destination at the correct Time Resource instant.  
- [ ] Character performs a visible shoot action; LoS check applies Wound on hit.  
- [ ] A cold observer can identify which scheduled timeline action caused which effect (Move vs Shoot).

---

## Explicitly not in Slice 1 / Core Combat ship

Path-draw 线稿涂鸦 polish (art pass), full gear card deck, Bandage/Otherwise, attic/vent/monitor, Flashbang/Adrenaline, Fusion networking, Android polish, final SSS. Covered by SCOPE Later / post-demo under **C34**.
