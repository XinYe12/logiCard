# Parallel Ops — Multi-Agent Constitution

**Status:** Active 2026-08-07. Permanent operating system for concurrent agents on logiCard.  
**Product truth stays elsewhere:** `PRODUCT_MEMORY.md`, `ART_DIRECTION.md`, `SCHEDULE.md`, `DRAFT_HANDOFF.md`. This file and `docs/departments/` track **who owns what and how to stay consistent**, not design decisions.

## Locked defaults

- **Capacity:** 3 agents max on a normal day — 1 **Integrator** (main tree) + 2 **Workers** (separate worktrees). Do not run 4+ without a fourth human-only merge seat.
- **Hard rule:** never two agents on the same working tree. Worktrees only see **committed** history — dirty main-tree work is invisible to peers.
- **Merge authority:** human approves; agents never merge to `master` unprompted (see `.claude/skills/parallel-development/SKILL.md`).
- **Scope knife:** human (C34). Integrator may propose cuts; only human freezes.

## Departments

| Dept | Role | Owns (write) | Must not touch | Worktree habit |
|------|------|--------------|----------------|----------------|
| **Core / Integrator** | Merge authority, wiring, bugfixes, schedule ticks | `Boot/`, `Net/`, `Timeline/`, `Sim/` when fixing resolve, `docs/DRAFT_HANDOFF.md`, `docs/SCHEDULE.md` ticks, `docs/contracts/CURRENT.md` after merge | Worker presentation/audio new files until merge | Main: `/Users/xuxinye/Documents/projects/Game/LogiCard` |
| **Presentation** | Motion, VFX, board look, playback readability | `Board/*View.cs` (new + assigned), materials/VFX under `Art/` as briefed | `GhostResolver`, `PawnProgram`, HUD allot logic | `logiCard-<slice>` worktree |
| **Audio** | Foley + UI feedback sounds | New `Assets/_Project/Audio/**`, sound hooks only where contract lists | Sim/Net resolve math; art materials | Own worktree |
| **Ship** | Windows candidate notes, README, capture checklist | `docs/` ship pages (`SHIP_README_DRAFT.md`, `CAPTURE_CHECKLIST.md`), build/player settings **only when briefed**, `screenshots/` | Gameplay code unless Integrator assigns a hotfix | Own worktree; docs-only slices need no Unity Editor |
| **Verify** (ephemeral) | Batchmode green checks | None (read-only except local TestResults/logs) | Never commit from verify trees | Disposable `logiCard-verify-*`; delete after |

Live snapshot of who is active: [`departments/INDEX.md`](departments/INDEX.md).  
Per-dept progress: `departments/<dept>/STATUS.md`.  
Frozen cross-dept APIs this wave: [`contracts/CURRENT.md`](contracts/CURRENT.md).

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
| Before merge request | Worker | Own STATUS complete; Integrator STATUS not conflicting | Report-back in brief + STATUS |
| After merge to master | Integrator | — | Update DRAFT_HANDOFF, INDEX, tick SCHEDULE only when DoD met, refresh `contracts/CURRENT.md` |
| End of session | Every agent | Peer STATUS for drift | Own STATUS “Last cross-reviewed” |

**Conflict rule:** If two STATUS docs disagree on an API or art decision, **Integrator + human** win; workers amend to match. Product decisions still require human confirm → C# row in PRODUCT_MEMORY (save-file rule).

## Doc write ownership

- Only **Integrator** edits `DRAFT_HANDOFF.md` and SCHEDULE checkboxes  
- Only **Integrator** edits `contracts/CURRENT.md` after a merge  
- Workers edit **only their** `departments/<dept>/STATUS.md` plus files listed in their AGENT_BRIEF  
- Terminology / ART_DIRECTION / PRODUCT_MEMORY changes: Integrator-only unless the brief explicitly assigns a one-line sync  

## Operating loop (per wave)

1. **Integrator** picks ≤2 safe slices (no file overlap), checkpoints commit if workers need dirty work, runs PARALLEL DEVELOPMENT → worktrees + briefs.  
2. **Workers** open only their worktree path; follow brief; update STATUS; never push/merge.  
3. **Verify** (or Integrator) runs suite on disposable worktree — do **not** pass `-quit` with `-runTests`; use `-acceptSoftwareTermsForThisRunOnly`.  
4. **Human** reviews look/feel when art/audio; Integrator merges.  
5. Integrator wires contracts, updates glue docs, starts next wave.

## What NOT to parallelize

- Two agents editing `GameBootstrap` / `RoundPlayback` / `ProgramHud` in the same wave  
- Two agents editing `DRAFT_HANDOFF` or ART_DIRECTION terminology  
- Verify on the same path as an open Editor  
- Unprompted board/path art restarts after Day 9 human sign-off (schedule > polish unless human reopens)

## Days 10–14 wave map (summary)

- **Wave 1 (Day 10):** Integrator = stepped motion + VFX wire; Presentation = muzzle/wound views; Audio = `FoleyPlayer` stub (new files only).  
- **Wave 2 (Day 11):** Integrator = wire audio into playback/HUD; Ship = README draft + capture checklist; Verify = full suite.  
- **Wave 3 (Days 12–14):** Integrator = Windows candidate + playtest hotfixes; Ship = finalize README/media; Presentation only on assigned visual blockers.

Detail and merge order live in department STATUS + `contracts/CURRENT.md`.
