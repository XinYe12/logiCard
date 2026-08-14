# Parallel Ops — Multi-Agent Constitution

**Status:** Active 2026-08-12 — permanent **Atmosphere / Cards / Character / UI** department worktrees under Integrator authority.  
**Product truth stays elsewhere:** `PRODUCT_MEMORY.md`, `ART_DIRECTION.md`, `GDD.md` §11, `SCHEDULE.md`, `DRAFT_HANDOFF.md`. This file and `docs/departments/` track **who owns what and how to stay consistent**, not design decisions.  
**This doc is the agent-side rulebook.** For the human's side — how to review reports, when a delegation is real vs. rework, what to check before approving a merge — see `docs/DIRECTING_AGENTS.md`.

## Locked defaults

- **Permanent departments (4):** **Atmosphere**, **Cards**, **Character**, **UI** — each **permanently owns** a sibling worktree folder and may continually develop inside it. Canonical paths (Windows):
  - `D:\projects\Game\logiCard-atmosphere`
  - `D:\projects\Game\logiCard-cards`
  - `D:\projects\Game\logiCard-character`
  - `D:\projects\Game\logiCard-ui`
- **Integrator = ultimate boss:** sits on main `D:\projects\Game\logiCard` (`master`). Monitors every department worktree (STATUS, contracts, diffs, batchmode). Departments never outrank Integrator. Integrator may pause a lane, reclaim files, reject a merge, or reassign scope.
- **Merge authority:** human approves; only Integrator merges to `master` (agents never merge unprompted — see `.claude/skills/parallel-development/SKILL.md`).
- **Hard rule:** never two agents on the same working tree. Worktrees only see **committed** history — dirty main-tree work is invisible to peers.
- **Concurrent coding capacity:** prefer ≤ **1 Integrator + 2 active coding departments** in a normal day so contracts stay reviewable. The other permanent worktrees may idle or do docs-only. Do **not** run all four coding hot at once without Integrator explicitly opening contracts for each and a human merge seat.
- **Ephemeral slices:** disposable `logiCard-<slice>` / `logiCard-verify-*` worktrees remain allowed for one-off fixes; fold lasting work back into the matching permanent department when done.
- **Scope knife:** human. Integrator may propose cuts; only human freezes → `PRODUCT_MEMORY` C# when design locks.

## Departments

### Permanent product departments (continual build)

| Dept | Role | Owns (write, typical) | Must not touch | Worktree |
|------|------|----------------------|----------------|----------|
| **Atmosphere** | Sky / clouds / mist / weather pocket; **mood lighting** while a weather module is active (storm dim, sunny brighten); diorama air above the board | `BoardWeatherPocket.cs`, `Resources/Weather/**`, weather import tools, related PlayMode smoke, atmosphere STATUS | `GhostResolver`, gear resolve, HUD allot math, Character Select | `logiCard-atmosphere` |
| **Cards** | Gear catalog, hand/economy, collection binder, Time Card *presentation* as cardstock | `docs/CARD_COLLECTION.md` (until promoted), future gear UI/data as contracted, cards STATUS | Host resolve tape mutation without PLAYBACK_CONTRACT redesign; Character unique verbs | `logiCard-cards` |
| **Character** | Character Cards, roster, pawn look, Character Select motion/feel | Character Select views, pawn art/outfit pipelines as briefed, roster docs, character STATUS | Map Select carousel (**C59**), modal chrome owned by UI, weather | `logiCard-character` |
| **UI** | Shell + HUD chrome, modals, docks, selection grids, board-anchored prompts | `Assets/_Project/UI/**` (except Character Select when Character dept owns that slice), `UiStyle` tokens prefixed per contract, ui STATUS | Sim/Net resolve; weather; pawn meshes | `logiCard-ui` |

### Authority + support seats

| Dept | Role | Owns (write) | Must not touch | Worktree habit |
|------|------|--------------|----------------|----------------|
| **Core / Integrator** | **Ultimate boss** — monitors all permanent worktrees; merge; wiring; bugfixes; schedule ticks | `Boot/`, `Net/`, `Timeline/`, `Sim/` when fixing resolve; `docs/DRAFT_HANDOFF.md`, `docs/SCHEDULE.md` ticks, `docs/contracts/CURRENT.md` after merge; may reclaim any dept path | Blind edits inside a live dept lane without reclaiming ownership in INDEX first | Main: `D:\projects\Game\logiCard` |
| **Audio** | Foley + UI feedback sounds (support; not one of the four permanent product seats) | New `Assets/_Project/Audio/**`, sound hooks only where contract lists | Sim/Net resolve math; art materials | Ephemeral or shared when briefed |
| **Ship** | Windows candidate notes, README, capture checklist | Ship docs, build/player settings **only when briefed**, `screenshots/` when assigned | Gameplay code unless Integrator assigns a hotfix | Own worktree; docs-only OK without Unity |
| **Verify** (ephemeral) | Batchmode green checks | None (read-only except local TestResults/logs) | Never commit from verify trees | Disposable `logiCard-verify-*`; delete after |

Live snapshot of who is active: [`departments/INDEX.md`](departments/INDEX.md).  
Per-dept progress: `departments/<dept>/STATUS.md` (`atmosphere`, `cards`, `character`, `ui`, plus `core`).  
Frozen cross-dept APIs this wave: [`contracts/CURRENT.md`](contracts/CURRENT.md).  
Product framing of the four pillars: [`GDD.md`](core/GDD.md) §11.

## Integrator monitoring duties

Every Integrator session:

1. Read each permanent dept `STATUS.md` + INDEX ownership matrix.  
2. Confirm no file overlap across active coding depts; open/refresh contracts before parallel hot work.  
3. Review report-backs; batchmode-verify when code lands; merge only after human sign-off.  
4. Update `DRAFT_HANDOFF.md`, INDEX, and `contracts/CURRENT.md` after merges.  
5. Reclaim a dept’s files into main (and note in INDEX) when that lane is idle/merged and Integrator must edit them.

Departments escalate blockers to Integrator; Integrator escalates product decisions to human (save-file rule).

## Session start (every agent)

Read in order:

1. [`DRAFT_HANDOFF.md`](DRAFT_HANDOFF.md) — human-facing daily rollup  
2. [`departments/INDEX.md`](departments/INDEX.md) — who owns what right now  
3. Peer `departments/*/STATUS.md` for any in-flight dept  
4. [`contracts/CURRENT.md`](contracts/CURRENT.md) — APIs you must honor  

Then update **your** STATUS to In progress. If a file you need is owned by another in-flight dept, **stop** and escalate to Integrator.

## Cross-review protocol

| When | Who | Reads | Writes |
|------|-----|-------|--------|
| Session start | Every agent | DRAFT_HANDOFF → INDEX → peer STATUS → contracts/CURRENT | Own STATUS “In progress” |
| Before claiming a file | Every agent | INDEX ownership | Stop if contested; escalate to Integrator |
| Before merge request | Dept worker | Own STATUS complete; Integrator STATUS not conflicting | Report-back in brief + STATUS |
| After merge to master | Integrator | — | Update DRAFT_HANDOFF, INDEX, tick SCHEDULE only when DoD met, refresh `contracts/CURRENT.md` |
| End of session | Every agent | Peer STATUS for drift | Own STATUS “Last cross-reviewed” |

**Conflict rule:** If two STATUS docs disagree on an API or art decision, **Integrator + human** win; departments amend to match. Product decisions still require human confirm → C# row in PRODUCT_MEMORY (save-file rule).

## Doc write ownership

- Only **Integrator** edits `DRAFT_HANDOFF.md` and SCHEDULE checkboxes  
- Only **Integrator** edits `contracts/CURRENT.md` after a merge  
- Departments edit **only their** `departments/<dept>/STATUS.md` plus files listed in INDEX / brief  
- Terminology / ART_DIRECTION / PRODUCT_MEMORY / GDD binding rules: Integrator-only unless the brief explicitly assigns a one-line sync  

## Operating loop

1. **Integrator** keeps the four permanent worktrees healthy (branch tips, ownership, contracts). Opens ≤2 coding-hot departments unless human expands capacity.  
2. **Departments** work only in their permanent folder; follow contracts; update STATUS; never push/merge to `master`.  
3. **Verify** (or Integrator) runs suite on disposable worktree — do **not** pass `-quit` with `-runTests`; use `-acceptSoftwareTermsForThisRunOnly`.  
4. **Human** reviews look/feel when art/UI; Integrator merges.  
5. Integrator wires contracts, updates glue docs, continues monitoring.

## What NOT to parallelize

- Two agents editing `GameBootstrap` / `RoundPlayback` / `ProgramHud` in the same wave without an Integrator-owned split  
- Two agents editing `DRAFT_HANDOFF` or ART_DIRECTION terminology  
- Atmosphere + board-surface lighting fights without a contract (Integrator mediates). **Exception (2026-08-14):** Atmosphere owns **mood lighting** while `BoardWeatherPocket` has an active module (Sunny brighten / Storm dim); bootstrap Fair/Clear baseline lights stay Integrator/`GameBootstrap`.  
- Character Select owned by Character **and** UI at once — pick one owner in INDEX  
- Verify on the same path as an open Editor  
- Unprompted board/path art restarts after human sign-off (schedule > polish unless human reopens)

## Starting / refreshing a permanent department worktree

```bash
# From main tree D:/projects/Game/logiCard — create once; keep forever (or recreate if removed).
git worktree add -b dept/atmosphere ../logiCard-atmosphere master
git worktree add -b dept/cards ../logiCard-cards master
git worktree add -b dept/character ../logiCard-character master
git worktree add -b dept/ui ../logiCard-ui master
```

Each department keeps a living brief or STATUS at `docs/departments/<dept>/STATUS.md` on **its** branch/worktree. Slice-specific `*_AGENT_BRIEF.md` files may still live at the worktree root for a focused job inside that permanent lane.

When a department reports back (commit on its branch only): Integrator reviews diff + boundary, verifies, merges with human approval, updates contracts / DRAFT_HANDOFF / INDEX.

Ephemeral verify:

```bash
git worktree add -d ../logiCard-verify-<x> master
# run EditMode + PlayMode, then remove
```

Do **not** delete a permanent department worktree after a merge unless the human is retiring that seat — permanent folders are meant to continue development.

## Historical wave notes (Days 10–14)

Days 10–11 waves are **done**. Wave 3 / Phase 5 work now runs through the **permanent four** + Integrator rather than ad-hoc Day-named worktrees. Playtest findings still gate triage via `docs/DAY13_PLAYTEST_FINDINGS.md` when filled. Detail and merge order live in department STATUS + `contracts/CURRENT.md`.
