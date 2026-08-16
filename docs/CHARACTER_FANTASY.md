# Character Fantasy — Cast Concepts

**Status:** Concept draft 2026-08-13 — not binding numerics, not a Sim brief.  
**Purpose:** Say who each Character *is* in the heist before we argue costs or code.  
**Depends on:** [`GDD.md`](GDD.md) §2; [`VISION.md`](VISION.md); [`CHARACTER_ROSTER_LONGTERM.md`](CHARACTER_ROSTER_LONGTERM.md);
[`CHARACTER_PLAN.md`](CHARACTER_PLAN.md).  
**Sibling:** ability wiring stays in implementation briefs; gear fantasy stays in `CARD_COLLECTION.md`.

If two Characters feel identical after you strip the UI chrome, this doc failed.

---

## 1. Pillars (shared by the whole cast)

1. **Timeline operators, not loadout pack mules.** Fantasy comes from how you spend Time Resource on
   the board — path, aim, interact — not from a private gear binder (**C18** / **C62**).
2. **Blind programming stays sacred.** No Character fantasy may require seeing the opponent's locked
   plan or a peek at resolve outcomes during Program (`VISION.md` Success Metric).
3. **Readable after Playback.** A cold observer should guess *which* archetype acted from the tape
   (speed of reposition, door tempo, unique verb spectacle) without opening a stats panel.
4. **Desk-lamp heist, not class MMO.** Names can be punchy; powers stay schedulable, Host-deterministic,
   miniature-board-scaled — no physics sandbox identities.

---

## 2. Live cast — attribute twins

Scout and Juggernaut share **Move / Shoot / Door**. They are not "DPS" and "Tank." They are two answers
to the same job: **commit N seconds of heist choreography**.

### Scout — the tempo thief

| Lens | Fantasy |
|------|---------|
| **Job** | Steal seconds. Be where the plan needs a body *before* the defender's Hold matures. |
| **Board read** | Prefers long flanks, vents, and re-routes; spends Time Resource on **distance**, not on handling. |
| **Combat tell** | Snaps and stance changes are "free" in the tempo sense — the scrubber doesn't punish fiddling. |
| **Door tell** | Doors are expensive relative to Juggernaut — Scout opens when the route *must* exist, not casually. |
| **Failure mode** | Arrives early into a prepared lane; speed without information. |
| **Not this** | Not a stealth class with a unique hide verb. Crawl is a shared stance, not a Scout-only power. |

**One-line pitch:** *Fast feet, light hands — you buy space with the clock.*

### Juggernaut — the committed hinge

| Lens | Fantasy |
|------|---------|
| **Job** | Own a hinge: a door, a lane, a short kill-box. Make every stance and aim-mode change a **decision**. |
| **Board read** | Shorter paths still hurt; flanks are expensive — central chokepoints and door control pay rent. |
| **Combat tell** | Snap↔Hold and Sprint exits cost Time Resource — the scrubber shows commitment. |
| **Door tell** | Doors are cheap — Juggernaut is the one who can afford to cycle a hinge inside a tight N. |
| **Failure mode** | Over-commits a Hold on empty air; slow to abandon a bad read. |
| **Not this** | Not a damage sponge. Wounds ladder is shared. No exclusive armor gear. |

**One-line pitch:** *Slow mass, hard hinge — you buy control with commitment.*

### Live-cast contrast checklist (design smell test)

- [ ] Same gear list still feels fair (Scout doesn't "need" Flashbang more as a *rule*).
- [ ] Maps with a short guarded center vs long flank (C45 / C57) actually *use* the speed asymmetry.
- [ ] Playback: Scout paths look skittery/long; Juggernaut paths look deliberate/short.
- [ ] If Agility stayed unwired forever, Scout/Jug would feel half-finished — fantasy assumes **C25** is real.

---

## 3. Long-term unique-verb operators

These break the "same verbs" model on purpose (**C42**). Fantasy must stay **verb-shaped**, not
"Scout-only Bandage."

### Bomber — the vertical editor

| Lens | Fantasy |
|------|---------|
| **Job** | Edit the *board's* future topology: attach risk now, detonate on a booked second. |
| **Spectacle** | Wall/floor becomes a new route — or a hole someone falls through. |
| **Mind game** | Opponent programs against today's geometry; Bomber scheduled yesterday's attach. |
| **Hard fantasy dependency** | Multi-floor heist space (attic/above) — without verticality, "Bomber" collapses toward "map Breach-door with extra steps." |
| **Not this** | Not a grenade aimed at a pawn id. Drop is a geometry consequence. Not Detonator martyr (**C38**). |

**One-line pitch:** *You don't outshoot the hinge — you delete the floor it stands on.*

**Open fantasy (not decided):** Is v1 allowed to be wall-only "route cutter" while verticality is unbuilt,
or does the name require the fall?

### Time Player — the object archivist

| Lens | Fantasy |
|------|---------|
| **Job** | Refuse the board's recent history: put an object back (or, if allowed, push it forward) along
  *its* state machine. |
| **Spectacle** | A breached hole knits; a ruined hinge remembers being whole. |
| **Mind game** | Opponent spends N to break something; you spend N to un-spend that break — tempo war on props. |
| **Sacred constraint** | Must not become "I preview the future of the round" — that stabs blind programming. |
| **Not this** | Not rewind-a-pawn / undo wounds (that's revive/martyr territory, **C38**). Not a global time reverse. |

**One-line pitch:** *You don't move faster — you make the room forget.*

**Open fantasy (not decided):** Rewind-only archivist vs true bidirectional "time" branding.

### Detonator (martyr note — **C38**, not C42 roster)

Locked as a **martyr archetype** on Downed→Dead, not as a Program verb like Bomber. Keep the fantasy
file aware so Bomber and Detonator never merge in pitch decks:

| | Bomber | Detonator |
|---|--------|-----------|
| When | Booked attach/detonate | On finishing blow (Downed→Dead) |
| Who chooses timing | Player program | Opponent (by finishing you) + Host append |
| Board vs body | Board geometry | Body-centered AoE |

Full Detonator concept doc is still backlog (`CHARACTER_PLAN.md` §3 item 6).

---

## 4. How fantasy shows up in product surfaces (owner hints)

| Surface | What fantasy needs | Owner |
|---------|-------------------|--------|
| Character Select blurb | One-line pitch + attr tell | **UI** chrome; Character owns copy truth |
| In-match **InfoBar** | Side + archetype + wound ladder + TR (see §4.1) | **Character** field meaning; **UI** chrome |
| In-match scrubber | Costs that match the tell (Agility/Strength real) | Character rules → UI present |
| Unique verb prompt | Identity / live state / confirm | UI board-anchored; Character legality |
| Collection / store | Cosmetics only; verbs free | Cards + Monetization — Character enforces "not P2W kit" |
| Pawn silhouette | Scout light/fast vs Jug heavy/planted (art) | Art / Phase 5 — fantasy can request, not mandate packs |

### 4.1 In-match InfoBar field sheet (Match Shell Layout — 2026-08-15)

**Owner:** Character (content meaning) · **UI** builds the band · **Integrator** wires readers.  
**Layout target:** [`docs/ui/MATCH_SHELL_LAYOUT.md`](ui/MATCH_SHELL_LAYOUT.md) region **InfoBar** (~6–8% height).  
**Reject:** mock “LORD VEXAR / 22 HP / mana orbs.” No fantasy HP bars, no mana.

**Bar shape (recommend):** **one combined InfoBar** — two side columns (Attacker | Defender) inside a single band, plus a shared match strip (phase / round / pool). Do **not** stack two full-height bars; the region budget is too thin.

| Field | Attacker | Defender | Source (today) | Notes |
|-------|----------|----------|----------------|-------|
| **Side label** | `ATTACKER` | `DEFENDER` | `MatchSide` (**C18**) | Spawn / allotment labels only — not kit locks. |
| **Character name / archetype** | e.g. Scout | e.g. Juggernaut | Roster + `CharacterData.displayName`; local pick via `AppFlowController.SelectedArchetype` | Demo cast = Scout / Juggernaut. Show **display name**, not asset filename. |
| **Wounds** | Healthy / Wounded / Dead | same | `RoundPlayback.WoundsOf(pawnId)` + `GhostResolver.WoundsUntilDead` (= 2) | Map `0 → Healthy`, `1 → Wounded`, `≥2 → Dead`. **Discrete ladder labels or 0–2 pips — not an HP bar.** |
| **Time Resource (shared)** | Match pool remaining + round index (+ chooser during Allot) | *(same strip — not per-side)* | `MatchClock.RemainingSeconds`, `RoundIndex`, `CurrentChooser` | Display only. Round budget / scrubber used-seconds stay on TimelineSchedule / ToolBar — InfoBar carries **match-pool** truth (**C33**). |
| **Phase** | Shared | Shared | `RoundPhaseController` / existing HUD phase string | ALLOT / PROGRAM / REVEAL / EXECUTE / … — match identity, not Character fantasy. |

**Optional / OPEN (post-demo — do not block shell):**

| Field | Status | Notes |
|-------|--------|-------|
| Signature card / deck size | **OPEN** | **C64** hybrid signatures + personal decks — no in-match reader yet; omit until Cards + Integrator freeze a display contract. |
| Attr tell (Speed/Agility/Strength numbers) | **Out of InfoBar** | Belongs on Character Select / a deepen panel, not the always-on status strip. |
| Unique-verb charge / bomb-armed icon | **Out of v1 InfoBar** | Bomber / Time Player not demo shell scope; revisit when those contracts open. |

**Integrator flags — Sim / Boot readers that are thin or missing:**

1. **Per-side archetype mid-match** — `SelectedArchetype` is the local player's Char Select string only. Defender (and any second local / net pawn) archetype is **not** exposed as a clean dual reader today; Boot often hardcodes speeds/art (`CHARACTER_ATTRS` D4 wiring gap). InfoBar needs an Integrator-owned `ArchetypeOf(pawnId)` (or side→`CharacterData`) before both columns can be truthful.
2. **Wound ladder enum** — only raw int via `WoundsOf`; UI (or a one-line helper) maps to Healthy/Wounded/Dead. No separate Sim API required if UI owns the map against `WoundsUntilDead`.
3. **No InfoBar widget yet** — content sheet only; UI seats the band. Character does not invent chrome.

**Copy tone:** desk-lamp heist status (side · name · wound word · pool clock), not MOBA portrait chrome.

---

## 5. Roster growth rule-of-thumb (concept)

Before naming a new Character, classify the fantasy:

| If the fantasy is… | It is probably… | Next doc |
|--------------------|-----------------|----------|
| Same verbs, different tempo/cost | Attr variant | Extend `CharacterData` + GDD §6 |
| A capability every cast member could hold | Gear card | Cards / `CARD_COLLECTION.md` |
| A capability only this pick has, schedulable | Unique verb | C42 brief + roster long-term |
| A map-only interaction | Door/Vent/Breach/station | Map / C57 — not a Character |
| A death conversion | Martyr (C38 family) | Separate from Program verbs |

If it fails Host-deterministic event-stream, it is **not** a Character ability yet — it is an
architecture proposal.

---

## 6. Open concept questions for the human (fantasy layer only)

1. Do Scout / Juggernaut pitches above match the game you want people to describe after one Playback?
2. Bomber: is wall-only v1 an acceptable *name*, or must fall-through-floor ship in the first reveal?
3. Time Player: is "archivist / rewind" enough branding, or does the name require fast-forward?
4. Any third unique-verb fantasy you want parked beside Bomber/Time Player before Detonator depth?

Answers here feed PRODUCT_MEMORY later; this doc stays a concept scratch until you confirm.

---

## See also

- [`CHARACTER_PLAN.md`](CHARACTER_PLAN.md) — pre-code roadmap
- [`CHARACTER_CARDS_BOUNDARY.md`](CHARACTER_CARDS_BOUNDARY.md) — gear vs verb vs attrs
- [`CHARACTER_ATTRS_SCOUT_JUGGERNAUT_BRIEF.md`](CHARACTER_ATTRS_SCOUT_JUGGERNAUT_BRIEF.md) — wiring reality
- [`CHARACTER_ROSTER_LONGTERM.md`](CHARACTER_ROSTER_LONGTERM.md) — Bomber / Time Player design source
- [`docs/ui/MATCH_SHELL_LAYOUT.md`](ui/MATCH_SHELL_LAYOUT.md) — InfoBar region geometry (UI); §4.1 is Character content for that band
