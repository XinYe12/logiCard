# D5: Paper Prototype Rulebook — "The Time Track" (v2.0)

**Doc ID:** D5  
**Status:** Stakeholder v2.0 — 2026-07-28  
**Also referenced as:** `GDD_v2.0_Tabletop_Prototype`  
**Companion digital rules:** [GDD.md](GDD.md) (2-week Unity demo — tick-based simplification)

This tabletop translation tests the **continuous time budget**, **character speed attributes**, and **path-drawing** mechanics **without a computer**.

To keep math human-friendly on cardboard, this prototype uses a single **60-second Time Resource slice** (1 minute of planned action). Digital uses the same continuous Time Resource model (**C28**); **Playback Duration** on screen is separate.

---

## I. Materials Needed

* **The Map:** A **10×10** square grid.
* **The Paper Clock (Time Track):** Numbered track **1 to 60** (1 minute of action). Use a generic pawn as the **Timekeeper**.
* **Player Trackers:** 1 colored cube per player on the Time Track.
* **Path Tokens:** 10 small arrows per player for the movement route.
* **Stance Tokens:** Colored chips on Path Tokens — **Red = Sprint**, **Yellow = Walk**, **Green = Crawl**.
* **Tactic Cards:** Index cards for *Flashbang*, *Hold Angle*, *Bandage*, and *Breach*.
* **Character Cards:** Two loadouts:
  * **The Scout:** Speed Base = **1 second per tile**.
  * **The Heavy:** Speed Base = **2 seconds per tile**.

---

## II. Character & Stance Math

Time to move 1 tile:

\[
\text{Tile Cost (Seconds)} = \text{Character Base Speed} \times \text{Stance Multiplier}
\]

**Stance Multipliers:**

| Stance | Multiplier | Notes |
|--------|------------|--------|
| **Sprint** | ×1 | Fastest; makes noise; **cannot shoot while moving** |
| **Walk** | ×2 | Medium; **can shoot** at targets of opportunity |
| **Crawl** | ×4 | Slowest; silent; invisible to sensors |

**Examples:**
- Heavy (Base 2s) × Crawl (×4) = **8 seconds** per tile  
- Scout (Base 1s) × Sprint (×1) = **1 second** per tile  

---

## III. The Round Structure

### Phase 1: Path Drawing (Secret Planning)

Players sit on opposite sides of a **divider** so they cannot see each other's board.

1. **Place Path Tokens:** Lay arrows from start tile to destination.
2. **Assign Stances:** Stance Token on each arrow. Stance may change mid-route (e.g. Sprint 3 tiles, then Walk 2).
3. **Drop Tactics:** Place Tactic Cards on specific tiles along the path where they should trigger.

### Phase 2: The Math (Timeline Booking)

Remove the divider. Calculate *when* actions occur on the **1–60** Paper Clock.

1. Both players place their colored cube on **Tick 0** of the Time Track.
2. Cost the first Path Token (e.g. Scout Sprint = 1s). Write cumulative time on a sticky note next to that tile.
3. Sum cumulative time for every tile and action on the route.
   * **Tactic Cost example:** *Flashbang* adds **+3 seconds** at the tile it is thrown.
4. When both routes are booked onto the clock, begin Phase 3.

### Phase 3: Execution (The Cinema)

Move the Timekeeper pawn up the **1–60** track **one second at a time**.  
When the Timekeeper hits a number matching a player's sticky note, that player moves their pawn to that tile and/or resolves their Tactic Card.

---

## IV. Combat & Collisions (Testing the Rules)

Guns are tied to **stance** (not separate Shoot cards on paper):

* **Line of Sight Intersects:** If at a given second (e.g. Tick 14) both have unobstructed LoS to each other, combat initiates.
* **Stance Superiority:**
  * **Walking** (gun up) vs **Sprinting** (gun down) → Walker wins; Sprinter takes a **Wound**.
  * If the other player has **Hold Angle** on that tile → Hold Angle wins regardless of stance.
* **Simultaneous Tie:** Both Walking and spot each other on the same second → both take a **Wound** (paper: mutual wound; digital GDD may treat mutual lethal as Draw — reconcile in playtest notes).
* **Wound Penalty:** On Wound, instantly recalculate the rest of the unplayed route: Character Base Speed **permanently +1 second** for the rest of this slice.

---

## V. Testing Goals for the Tabletop

Play **2–3** times with a friend. Answer:

1. Does the Scout feel too fast? (Sprint across 10×10 before Heavy clears a door?)
2. Is Crawl (×4) too punishing to ever use?
3. How does it feel to predict the enemy at exactly **Tick 22**?

Log answers in a short playtest note (date, who, 3 findings).

---

## VI. Relationship to Digital GDD (D4)

| Paper (D5) | Digital (D4 / C28) |
|------------|-------------------|
| 60s continuous Time Resource track | Continuous Time Resource (demo round window TBD; 60s placeholder) |
| 10×10 map | see `GDD.md` §1 for current digital footprint |
| Scout / Heavy base seconds/tile | Scout / Juggernaut tiles × stance math |
| Manual Timekeeper pawn | Host ghost sim + ReplayTape; **Playback Duration** may compress cinema |
| Wound → +1s base speed rest of slice | Wound → surcharge + Bandage deadline |

Paper is for **feel-testing Distance × Speed × Stance**. Digital may stay tick-quantized for net determinism; learnings from D5 feed GDD tuning.
