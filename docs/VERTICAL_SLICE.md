# Vertical Slice Spec

**Doc ID:** D7
**Status:** Drafted 2026-07-29
**Depends on:** [CORE_LOOP.md](CORE_LOOP.md), [GDD.md](GDD.md), [SCOPE.md](SCOPE.md)

**Purpose:** The 2-week demo scope in `SCOPE.md`/`GDD.md` is still a lot of surface (path-draw UI, 3 stance bands, doors/vent/monitor, wound system, 6 tactic cards). This doc cuts that down to the **smallest deliverable that proves the core pipeline works at all** — build this first, then layer the rest on top per the slice ordering below.

---

## The minimal proof (A → B → C)

**A.** Player schedules a **Move** on their personal timeline (even a single-tile path counts — path-drawing polish is not required yet). Base verb, not a card.
**B.** Player schedules a **Shoot** (aim + time → Snap Shot mode is enough) on the timeline, aimed down LoS. Base verb, not a card — same attribute-driven pattern as Move, no card is drawn or held.
**C.** Lock → Reveal → Host resolves continuous **Time Resource** → **Playback** (ReplayTape; duration may be compressed):
- Character **visibly moves** to the destination tile on the board.
- Character **visibly performs the shoot action**, and if the target is in LoS at that Time Resource instant, the hit/miss + Wound rule from GDD applies.

**Pass/fail bar:** a first-time observer watching the **timeline scrubber** (Time Resource), with no narration, can say *"that scheduled Move made them move"* and *"that scheduled Shoot made them shoot"* — even if Playback Duration is shorter than Time Resource. This is [VISION.md](VISION.md)'s success metric, scaled to the smallest testable unit.

---

## Stub vs. real for this slice

| Piece | Stub OK for Slice 1 | Must be real for Slice 1 |
|---|---|---|
| Path drawing UI | Simplified (e.g. click-to-set one destination tile) | Path executes at the correct tick, at the tile Speed dictates |
| Stance bands (Sprint/Walk/Crawl) | Hardcode a single stance | — (added Slice 2) |
| Shoot modes (Snap/Hold) | Hardcode Snap Shot only, no aim UI polish | Snap Shot's Wound-on-hit rule fires correctly; confirms Shoot is scheduled like Move, not drawn from a hand |
| Map | Single 5×5, no attic/vent/monitor/doors | Grid movement + collision |
| Networking | Local/hotseat or single scripted client acceptable | Time Resource ordering is deterministic on Host |
| Art | Primitives (capsule + line-renderer path) | Move vs. shoot are visually distinct actions |
| Wound/Win state | Stub text ("Wounded"/"Dead") is fine | — |
| Playback Duration | May be compressed vs Time Resource | Observer still reads correct **order** on scrubber |

---

## Milestone ordering inside the 14-day demo

1. **Slice 1 (this doc):** scheduled action → Time Resource timeline → Playback → visible Move + visible Shoot. Proves the pipeline; both sides can be scripted/local.
2. **Slice 2:** real path-drawing UI + stance bands (Sprint/Tactical Walk/Stealth Crawl, GDD 3.2); Snap Shot vs. Hold Angle RPS distinction.
3. **Slice 3:** Wound/Bandage/Otherwise→Stop, second tactic card, doors.
4. **Slice 4:** 1v1 Photon networking, vent + monitor, Flashbang/Adrenaline, full GDD Section 9 acceptance criteria.

Win/Android build target (**C6**) is attempted once Slice 2 or 3 is stable, not required for Slice 1.

---

## Pass/fail checklist (Slice 1)

- [ ] Player can schedule a path (minimal is fine) during Program phase — base Move verb.
- [ ] Player can schedule a Shoot action (aim + time, Snap mode) on the timeline — base verb, no card involved.
- [ ] Lock / Reveal transitions work.
- [ ] Master / timeline scrubber advances through Time Resource and fires ops at the correct scheduled **seconds**.
- [ ] Character animates/moves to the correct destination at the correct Time Resource instant (Playback Duration may differ).
- [ ] Character performs a visible shoot action; LoS check applies Wound on hit.
- [ ] A cold observer can correctly identify which scheduled timeline action caused which effect (Move vs Shoot).

---

## Explicitly not in this slice

Path-draw polish, all 3 stance bands, doors/vent/monitor, wound/bandage deadline pressure, Flashbang/Adrenaline, networking, Android build, art direction. All covered by GDD/SCOPE and picked up in Slices 2–4.
