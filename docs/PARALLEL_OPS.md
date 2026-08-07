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

## Starting a new worktree — quick reference

The full mechanics live in `.claude/skills/parallel-development/SKILL.md` (invoke with `/parallel-development` or
by saying "parallel development" — an agent will pick a slice, set this up, and hand you a paste-ready command
for the other agent). This is the condensed version, for doing it by hand or when no agent is available to run
the skill:

```bash
# 1. From the main tree (/Users/xuxinye/Documents/projects/Game/LogiCard), create the worktree.
#    Name the branch/directory after the SLICE, not "agent2" — e.g. a Day 13 wall-clip fix:
git worktree add -b feat/day13-wall-clip-fix ../logiCard-day13-wall-clip-fix master

# 2. Write a self-contained brief at the worktree ROOT (not the main tree):
#    <SLICE_NAME>_AGENT_BRIEF.md — where/why, the job (concrete paths/signatures), tests to run,
#    boundary (files NOT to touch and why), why the split is safe, how to report back.
#    See DAY10_HIT_VFX_AGENT_BRIEF.md / DAY11_AUDIO_STUB_AGENT_BRIEF.md / SHIP_DOCS_AGENT_BRIEF.md
#    in git history (git show <branch>:<BRIEF_FILE>) as worked examples if those worktrees are gone.

# 3. Hand this to the other agent/session, unchanged:
cd /Users/xuxinye/Documents/projects/Game/logiCard-day13-wall-clip-fix
# open this path in a second Cursor/Claude session, then:
# Read <SLICE_NAME>_AGENT_BRIEF.md first, then do what it says.
```

When the worker reports back (commit on their branch, never pushed/merged by them): review the diff and
boundary, batchmode-verify in a **disposable** worktree if it touches code (`git worktree add -d
../logiCard-verify-<x> master`, copy the changed files in since worktrees only see committed history, run
EditMode + PlayMode, remove the worktree after), then `git merge --no-ff <branch>` from the main tree once you
and the human are satisfied — never merge unprompted. Update `contracts/CURRENT.md` / `DRAFT_HANDOFF.md` /
`departments/INDEX.md` after, per the doc-ownership rules above.

To remove a worktree once its branch is fully merged: `git worktree remove ../logiCard-<slice-name>` (add
`--force` only if you've confirmed there's nothing valuable left uncommitted in it — `git stash` first if unsure,
stashes survive worktree removal and are recoverable with `git stash list` / `git stash pop` from any worktree of
the repo).

## Days 10–14 wave map (summary)

- ~~**Wave 1 (Day 10):** Integrator = stepped motion + VFX wire; Presentation = muzzle/wound views; Audio = `FoleyPlayer` stub (new files only).~~ **Done** (2026-08-07) — `d60f01d`, `fc32a2d`, `a57d095`.
- ~~**Wave 2 (Day 11):** Integrator = wire audio into playback/HUD; Ship = README draft + capture checklist; Verify = full suite.~~ **Done** (2026-08-07) — `ef6e3f5`/`7e08aba`, `04f9191`, `950ff63`.
- **Wave 3 (Days 12–14):** in progress. Day 12's Windows candidate is being built natively on the human's own Windows machine — **not an agent task**, that build doesn't happen through this repo's worktree workflow. Everything else in Wave 3 is gated on the human sign-off below; there is nothing safe to delegate to a fresh worker until it's done.

### Wave 3 kickoff — read this before spawning anything for Days 12–14

1. **Human sign-off first, blocks everything else.** Play a round in the Editor (or the Windows build once it exists) and fill in `docs/DAY13_PLAYTEST_FINDINGS.md` — Day 10 visuals (stepped motion, muzzle flash, wound splat) and Day 11 audio (the four Foley sounds). That file has the exact repro steps and a triage key. Nothing below this point should start before real findings exist there — spawning a "fix it" worker off a vague verbal note instead of a written finding is exactly the kind of bad split the `/parallel-development` skill warns against.
2. **Integrator triages each finding** using the key in that file: ship-as-is (note it, move on), quick fix (same-session, no worktree), real fix (`/parallel-development` a worker slice — same pattern as the Day 10/11 VFX/Audio splits: new worktree, frozen contract in `contracts/CURRENT.md`, brief at the worktree root), or defer (log under Known Issues in `DRAFT_HANDOFF.md`). Respect the capacity cap — 1 Integrator + 2 Workers max, no two workers on `RoundPlayback`/`GameBootstrap`/`ProgramHud` in the same wave (same rule that governed Wave 1/2).
3. **Tick Day 10 and Day 11 on `SCHEDULE.md`** once their findings are triaged (fixes landed or explicitly deferred) — don't wait for Day 12+ to close those out.
4. **Day 12 (Windows candidate):** human builds on their own Windows machine directly. Once it exists, Integrator ticks Day 12 and notes the build location/version in `DRAFT_HANDOFF.md`. (A Mac-side batchmode build was attempted 2026-08-07 and abandoned — see that date's entry in `DRAFT_HANDOFF.md` for why, and what to check first — Sentis dependency — if a Mac build is ever needed again.)
5. **Day 13 (playtest / presentation bugfix):** same findings file, same triage loop as step 2 — this is the "three written findings" playtest `SCHEDULE.md`'s cadence rule asks for.
6. **Day 14 (Ship):** once the Windows build and capture footage/screenshots exist (`docs/CAPTURE_CHECKLIST.md`), spin a fresh `feat/ship-docs`-style worktree with a brief to embed the video link + stills into `SHIP_README_DRAFT.md` and promote it to root `README.md`. Not before — the draft is intentionally incomplete without real media.

Detail and merge order live in department STATUS + `contracts/CURRENT.md`.
