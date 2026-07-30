# logiCard — Product Memory (D9)

**Think of this as the game’s save file.**  
If a rule or feature is not listed under **CONFIRMED**, it is not locked — do not build it as if it were.

**Last updated:** 2026-07-30 (**C34 Polished Core Demo** — art + tight gameplay supersedes feature breadth for the 14-day artifact)

**Rule:** Only **CONFIRMED** is binding. Sync `.cursor/rules/logicard-product-memory.mdc` when CONFIRMED changes.

---

## Pre-implementation doc checklist

| ID | Doc | Status |
|----|-----|--------|
| D1–D9 | Vision → Product memory | Done |
| D10 | Art direction (`ART_DIRECTION.md`) | **Done** |
| D11 | Risk register (`RISKS.md`) | **Done** |
| D12 | UI / UX flow (`UI_FLOW.md`) | **Done** |
| Gate | `confirm: pre-implementation gate passed` | **PASSED 2026-07-29** |

Full paths: see [`PRE_IMPLEMENTATION.md`](PRE_IMPLEMENTATION.md).

---

## Glossary (binding — C27)

| Term | Meaning | Example |
|------|---------|---------|
| **Time Resource** | Player’s **game resource** — operation budget measured in **seconds of planned action**. Planned with real-world-like physics math (distance × speed × stance), but it is **not** required to equal on-screen animation length. | “This Sprint path costs **8s** of Time Resource.” |
| **Playback Duration** | How long the **cinema / ReplayTape** takes to **show** an event on the player’s screen. Designer-tunable; may compress or stretch vs Time Resource. | “That 8s Time Resource Move plays back in **1.2s** Playback Duration.” |
| **Real-world** | Keyword reserved for **actual physical wall-clock time** in our world only. | “Program phase lasts **30 real-world seconds**.” |

**Do not** call Playback Duration “real-world time.”  
**Do not** confuse Time Resource seconds with real-world seconds unless a UI timer is explicitly real-world (e.g. Program countdown).

---

## CONFIRMED (source of truth)

### Plain-language summary

- **Loop:** Allot (Time Card) → Blind Program → Reveal → Host resolves → cinematic **Playback** (ReplayTape) → Aftermath → next Allot (or Match Over).
- **Time:** Digital uses **continuous Time Resource** (seconds), same family of math as paper (`BaseSpeed × StanceMultiplier`). **No 12-tick discrete Master Clock** in digital. Match holds one shared pool; each round commits **N** seconds via a Time Card (**C33**).
- **UI clock:** A continuous **timeline / scrubber** showing Time Resource ordering (“grenade at 2.0s before sniper at 4.0s”), while Playback Duration controls how fast that cinema feels.
- **14-day ship (C34):** polished local **Windows** vertical slice — Desk-Lamp Diorama visual floor + tight core (Time Card, path/stance Move, Snap/Hold Shoot, wounds/death, one door). Fusion, full Android polish, gear cards, Bandage/Otherwise/vent/monitor/Flashbang/Adrenaline are **post-demo**.
- **Long-term (unchanged architecture):** Fusion Host (**C5**), Win+Android (**C6**) remain product targets after the 14-day artifact.
- **Shape:** **portrait, one-handed** (**C30**) — phone upright, single thumb; board on top, all controls in the bottom thumb zone.

### Binding ID table

| ID | Decision |
|----|----------|
| C1 | Engine: Unity Personal |
| C2 | Players: 2–8 vision; **demo 1v1** |
| C3 | Secret plan, simultaneous resolve |
| C4 | Allot → Program → Lock → Reveal → Resolve → Aftermath (→ Allot or Match Over) |
| C5 | Photon Fusion 2 Host Mode (**long-term**; deferred from 14-day ship by **C34**) |
| C6 | Ship Windows + Android (**long-term**; 14-day polished ship = **Windows**; Android smoke-only if time — **C34**) |
| C7 | Bots (vision); demo nice-to-have (**C19**) |
| C8 | Attack vs Defend PvP, fixed map ground+attic |
| C9 | $0 demo path |
| C10 | Repo logiCard ↔ GitHub XinYe12/logiCard |
| C11 | ~14-day impl after gate |
| C12 | Portfolio README + video + architecture |
| C13 | Lightweight 2.5D + 2D timeline UI |
| C14 | **Phase:** Pre-impl docs **complete**; gate **PASSED 2026-07-29**. Implementation active per [SCHEDULE.md](SCHEDULE.md). Design changes still require CONFIRMED updates (**C26**). |
| C15 | Move + Shoot = base verbs; cards = gear; Character attrs |
| C16 | Attack vs Defend PvP |
| C17 | **14-day content (C34):** 1v1 local; **5×5 ground** (attic deferred); **one door**; Scout/Juggernaut attrs; path+stance; Shoot Snap/Hold; Healthy/Wounded/Dead; FoW Out. **Post-demo:** attic/vent/monitor; Bandage/Interact-card/Flashbang/Adrenaline; Otherwise Stop |
| C18 | Labels + spawns; same gear deck |
| C19 | Demo bots nice-to-have |
| C20 | **Demo numerics (continuous):** Match pool **900s** via Time Cards (**C33**); Program phase **30 real-world seconds**; Move/Shoot costs in Time Resource seconds (placeholders OK); stance combat; Snap/Hold; wounds → Dead (Bandage deferred — **C34**); mutual lethal = draw. **No 12-tick clock.** |
| C21 | Path + time → Stance |
| C22 | Paper D5 done |
| C23 | Payload → Host ghost → ReplayTape → playback |
| C24 | Slice 1: schedule Move + Shoot in **Time Resource**; Lock/Reveal; playback shows move + shoot; observer reads cause/effect on the **timeline scrubber** (not tick 1–12) |
| C25 | Shoot = base verb (Snap/Hold modes), same pattern as Move/Stance. Agility scaling: **confirmed** — mirrors the movement-stance penalty. Juggernaut pays **+1 tick** once when switching Snap ↔ Hold Angle (Scout pays 0), same shape as their stance-change penalty. Placeholder magnitude; tune together during playtesting. |
| C26 | D9 save-file rule |
| C27 | **Glossary:** Time Resource (game budget seconds) ≠ Playback Duration (cinema length) ≠ **Real-world** (physical wall-clock only) |
| C28 | **Digital time model:** Continuous Time Resource (real-world-*like* planning math). Playback Duration is separate and tunable so long Time Resource actions are not forced to play as long wall-clock animations. |
| C29 | **Art direction (D10):** Desk-Lamp Diorama — digital claymation, tilt-shift miniature scale, stepped 8–12fps character motion, physicalized VFX, yarn/chalk paths, AR-like Time Resource UI vs clay board; audio = tactile miniature foley. Moodboard: `image.png`. **C34** elevates the demo art floor from “primitives OK” to a **required presentation floor** (see ART_DIRECTION § Demo art floor); full SSS/thumbprint maps/bespoke rigs remain optional. |
| C30 | **Orientation (2026-07-29):** **Portrait only, one-handed.** Phone is held upright; the whole demo must be playable with a single thumb. Portrait lock on Android (no autorotate); Windows runs the **same portrait-aspect layout** in a tall window. Every primary action (Lock In, Adrenaline, card placement, path taps) sits in the bottom thumb-reach zone; the board occupies the upper screen and is never required for a precision drag out of thumb reach. **Supersedes all earlier "landscape" notes** in D8/D10/D11/D12. |
| C32 | **Snap Shot targeting (2026-07-30):** A Snap Shot resolves at its scheduled **completion** second and wounds only a pawn standing on the **aimed tile** at that instant with clear line of sight. It does **not** sweep the line of fire — covering a lane over time is Hold Angle's job (Day 6). This makes Snap a prediction bet, consistent with blind programming. Line of sight is integer **Bresenham over grid tiles**, same floor only, blocked by impassable tiles between the endpoints (endpoints excluded) — **never** a physics raycast, so Host resolve is bit-identical everywhere. |
| C31 | **High-speed rail / 高铁 (2026-07-29):** Side-of-map rail line on the diorama. A character may **board and ride** it as transportation. Rider **may Shoot while riding**. The **rail car is bulletproof** (shots do not wound through the car / do not break the ride). **Once per match** — after that ride is spent, the rail cannot be used again that game. **Not a Slice 1–3 Must-Have**; implement only after Move/Shoot/Clock/doors are green (treat like optional map gadget — cut before Clock/Move/Shoot). Numerics (board Time Resource cost, ride speed/route, once = shared vs per-player) stay in OPEN until playtest lock. |
| C33 | **Match Time Resource pool / Time Card (2026-07-30):** One **shared** match pool (demo placeholder **900s / 15 min**). Each round one side plays a **Time Card** committing **N** seconds from the remaining pool (`N` clamped to `[MinRoundSeconds, Remaining]`). Round 1 chooser = **Attacker**; chooser **alternates** each round. Both sides still Program **simultaneously** inside that same `N` (resolve is not turn-based — only the allotment choice alternates). **N is spent in full** when played; seconds left unspent inside the round window are burned. Board state **carries** between rounds (positions + wounds). Match ends on Dead or when the pool cannot fund another round. Time Card is a **round-allotment commit**, not a gear card. |
| C34 | **Polished Core Demo (2026-07-30):** The 14-day portfolio artifact prioritizes a **cohesive, aesthetically pleasing local Windows vertical slice** over feature breadth. **In:** Time Card match loop (**C33**); path + stance Move; Snap/Hold Shoot; wounds/death; **one** contextual door (blocks move + LoS); Desk-Lamp Diorama **required visual/audio floor** (**C29** / ART_DIRECTION). **Out of 14-day ship:** Fusion online (**C5** deferred); full Android polish (**C6** → Windows ship + optional Android smoke); Bandage; Otherwise; vent; monitor; Flashbang; Adrenaline; attic; loot; 高铁 (**C31**). Cut order if behind: Android smoke → door reopen nuance → Crawl AV nuance → optional DoF/SSS — **never** cut Time Card loop, Move/Shoot readability, warm diorama composition, yarn path, physical shot feedback, or Windows build stability. |

---

## OPEN / parking lot

1. ~~**Match Time Resource budget**~~ — **resolved by C33** (shared 900s pool + per-round Time Card).  
2. **Default Playback Duration policy:** global scale / per-event caps (demo uses a per-TR-second rate).  
3. **Card economy:** full hand vs draw each Program — **deferred** with gear cards under **C34**.  
4. **高铁 (C31) numerics:** Board cost in Time Resource seconds; ride path (fixed side track vs waypoints); ride speed; floor (ground only vs both); **once** = one ride total for the match vs once per player; does bulletproof fully shield the rider or only block LoS through car body (windows / open doors)?

Pre-impl docs **D1–D12 done; gate PASSED 2026-07-29.** Build per Schedule Day 1+.

---

## How to update

1. Confirm in chat → edit **C#** row.  
2. Sync Cursor rule.  
3. Tick [`PRE_IMPLEMENTATION.md`](PRE_IMPLEMENTATION.md).
