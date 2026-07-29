# logiCard — Product Memory (D9)

**Think of this as the game’s save file.**  
If a rule or feature is not listed under **CONFIRMED**, it is not locked — do not build it as if it were.

**Last updated:** 2026-07-29 (**Pre-implementation gate PASSED** — Unity unlocked; **C30 portrait**; **C31 高铁** confirmed design)

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

- **Loop:** Blind Program → Reveal → Host resolves → cinematic **Playback** (ReplayTape).
- **Time:** Digital uses **continuous Time Resource** (seconds), same family of math as paper (`BaseSpeed × StanceMultiplier`). **No 12-tick discrete Master Clock** in digital.
- **UI clock:** A continuous **timeline / scrubber** showing Time Resource ordering (“grenade at 2.0s before sniper at 4.0s”), while Playback Duration controls how fast that cinema feels.
- **Verbs / cards / net / platforms:** unchanged (Move + Shoot base verbs; gear cards; Fusion Host; Win+Android; Slice 1 = visible Move + Shoot on the timeline).
- **Shape:** **portrait, one-handed** (**C30**) — phone upright, single thumb; board on top, all controls in the bottom thumb zone.

### Binding ID table

| ID | Decision |
|----|----------|
| C1 | Engine: Unity Personal |
| C2 | Players: 2–8 vision; **demo 1v1** |
| C3 | Secret plan, simultaneous resolve |
| C4 | Program → Lock → Reveal → Resolve → Aftermath |
| C5 | Photon Fusion 2 Host Mode |
| C6 | Ship Windows + Android |
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
| C17 | Demo content: 1v1; 5×5+5×5; doors/vent/monitor; Scout/Juggernaut; path+stance; Shoot Snap/Hold; Bandage/Interact/Flashbang/Adrenaline; Otherwise Stop; Healthy/Wounded/Dead; FoW Out |
| C18 | Labels + spawns; same gear deck |
| C19 | Demo bots nice-to-have |
| C20 | **Demo numerics (continuous):** Time Resource costs in **seconds** (Scout/Heavy bases + stance mults per GDD/paper); Program phase **30 real-world seconds**; max 3 gear cards/round; Snap/Hold/Bandage/Interact/Flashbang/Adrenaline costs in Time Resource seconds (placeholders OK); Wound/Bandage rules; stance combat; mutual lethal = draw. **No 12-tick clock.** |
| C21 | Path + time → Stance |
| C22 | Paper D5 done |
| C23 | Payload → Host ghost → ReplayTape → playback |
| C24 | Slice 1: schedule Move + Shoot in **Time Resource**; Lock/Reveal; playback shows move + shoot; observer reads cause/effect on the **timeline scrubber** (not tick 1–12) |
| C25 | Shoot = base verb (Snap/Hold modes), same pattern as Move/Stance. Agility scaling: **confirmed** — mirrors the movement-stance penalty. Juggernaut pays **+1 tick** once when switching Snap ↔ Hold Angle (Scout pays 0), same shape as their stance-change penalty. Placeholder magnitude; tune together during playtesting. |
| C26 | D9 save-file rule |
| C27 | **Glossary:** Time Resource (game budget seconds) ≠ Playback Duration (cinema length) ≠ **Real-world** (physical wall-clock only) |
| C28 | **Digital time model:** Continuous Time Resource (real-world-*like* planning math). Playback Duration is separate and tunable so long Time Resource actions are not forced to play as long wall-clock animations. |
| C29 | **Art direction (D10):** Desk-Lamp Diorama — digital claymation, tilt-shift miniature scale, stepped 8–12fps character motion, physicalized VFX, yarn/chalk paths, AR-like Time Resource UI vs clay board; audio = tactile miniature foley. Demo may use primitives that evoke this; full SSS/thumbprints = target polish. Moodboard: `image.png`. |
| C30 | **Orientation (2026-07-29):** **Portrait only, one-handed.** Phone is held upright; the whole demo must be playable with a single thumb. Portrait lock on Android (no autorotate); Windows runs the **same portrait-aspect layout** in a tall window. Every primary action (Lock In, Adrenaline, card placement, path taps) sits in the bottom thumb-reach zone; the board occupies the upper screen and is never required for a precision drag out of thumb reach. **Supersedes all earlier "landscape" notes** in D8/D10/D11/D12. |
| C31 | **High-speed rail / 高铁 (2026-07-29):** Side-of-map rail line on the diorama. A character may **board and ride** it as transportation. Rider **may Shoot while riding**. The **rail car is bulletproof** (shots do not wound through the car / do not break the ride). **Once per match** — after that ride is spent, the rail cannot be used again that game. **Not a Slice 1–3 Must-Have**; implement only after Move/Shoot/Clock/doors are green (treat like optional map gadget — cut before Clock/Move/Shoot). Numerics (board Time Resource cost, ride speed/route, once = shared vs per-player) stay in OPEN until playtest lock. |

---

## OPEN / parking lot

1. **Match Time Resource budget:** Full ~15 minutes vs repeating short slices — demo placeholder **60s** TR/round until confirmed.  
2. **Default Playback Duration policy:** global scale / per-event caps.  
3. **Card economy:** full hand vs draw each Program.  
4. **高铁 (C31) numerics:** Board cost in Time Resource seconds; ride path (fixed side track vs waypoints); ride speed; floor (ground only vs both); **once** = one ride total for the match vs once per player; does bulletproof fully shield the rider or only block LoS through car body (windows / open doors)?

Pre-impl docs **D1–D12 done; gate PASSED 2026-07-29.** Build per Schedule Day 1+.

---

## How to update

1. Confirm in chat → edit **C#** row.  
2. Sync Cursor rule.  
3. Tick [`PRE_IMPLEMENTATION.md`](PRE_IMPLEMENTATION.md).
