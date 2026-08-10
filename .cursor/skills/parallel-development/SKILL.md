---
name: parallel-development
description: >-
  Keep two agents busy without file conflicts. Triggered by "PARALLEL DEVELOPMENT"
  or /btw + parallel development. Identifies a safe concurrent slice for a second
  agent, emits a copy-paste handoff (and optionally a worktree + brief), while the
  main agent continues its own claimed work and does not go idle waiting.
---

# Parallel Development

**Goal:** neither agent sits idle. While you (the main agent) keep doing your claimed work, the user gets an exact paste for a second agent whose scope does **not** collide with yours.

**The trigger means "execute Steps 1–5 now," not "explain this skill."** When the user says "PARALLEL DEVELOPMENT" or "/btw … parallel development," they want the output of the pattern (claimed lane + slice + paste-ready handoff), not a description of what the pattern is or how it works. If you catch yourself writing sentences like "this skill does X" or "it works by…" in response to the trigger, stop — that's the wrong output. Only describe the mechanism if the user explicitly asks a question *about* the skill itself (e.g. "what does parallel-development do?", "explain how /btw works") rather than invoking it.

Triggered when the user says **"PARALLEL DEVELOPMENT"**, **"/btw … parallel development"**, or clearly asks what another agent can do at the same time.

This project has used the pattern before: `VERIFY_AGENT_BRIEF.md`, `PHASE1_AGENT_BRIEF.md`, `HUD_DOOR_AGENT_BRIEF.md`, `URP_AGENT_BRIEF.md`. Read one if unsure what "self-contained" and "no file overlap" mean.

---

## Two modes (pick by how the user invoked you)

| Mode | When | What you produce | What you do next |
|------|------|------------------|------------------|
| **Handoff-first (`/btw`)** | User used `/btw`, or asked what to tell the other agent without asking you to stop | Ranked safe slice(s) + **copy-paste handoff block** immediately. Create worktree/brief only if the slice needs a separate directory and you can do it quickly without abandoning main work. | **Resume your main task in the same turn** (or say exactly what you’re continuing). Never end the turn with only analysis and no handoff *or* no progress on the main job. |
| **Full split** | User wants a real second worktree set up now | Checkpoint → `git worktree add` → write `*_AGENT_BRIEF.md` into that worktree → paste handoff | Keep working in the **main** worktree on the files you claimed; the other agent owns the new path |

`/btw` is a side channel: answer with the handoff **without derailing** the primary agent’s in-flight work. Prefer mode **Handoff-first** unless they explicitly ask you to create the worktree.

---

## Step 1 — Claim your lane, then find theirs

Before proposing anything for agent 2:

1. **State what you (main) own right now** — files/directories you are editing or about to edit. That set is **off-limits** to the other agent.
2. **Scan for already-done or parked work** so you don’t duplicate:
   - Open worktrees / branches (`git worktree list`, recent `*_AGENT_BRIEF.md`)
   - Whether a candidate is already on `master` (e.g. feature merged, brief obsolete)
   - Stale worktrees that only need cleanup (merge check → close), not a second implementation
3. Pick a slice for agent 2 that satisfies **both**:
   - **No file overlap** with your claimed lane (and no dependency on APIs you haven’t frozen yet).
   - **A crisp contract** — doable from a written brief alone, cold start, ≤ ~1–2 pages.

**Good slices:** new-file-only with frozen API; tests over committed code you aren’t touching; docs-only; worktree cleanup after merge; read-only diagnosis (locks, ancestry) that reports back.

**Bad slices:** same files you’re mid-edit on; “figure out design X”; human playtest / Phase-6 feel tuning; re-implementing something already on `master` (produces a duplicate to reconcile, not free parallelism).

If **nothing** is safely parallel right now, say so in one sentence and list what would unblock a split (e.g. “after you commit Phase 2 API”). Do **not** invent busywork that will conflict.

Rank when several options exist: **additive code with frozen contract** > **verify/tests on frozen code** > **docs / worktree cleanup** > **read-only investigation**. Prefer the highest-rank item that is actually free.

---

## Step 2 — Checkpoint (full-split mode only)

A worktree only sees **committed** history.

1. `git status --short`
2. If uncommitted work must be in the fork baseline, **ask before committing** (standing git-safety rules).  
   Example: “I need to commit X as a checkpoint so the new worktree can fork from it — proceed?”
3. Note the base commit hash.

For **Handoff-first / `/btw`**: if the other agent can work in an existing worktree or on a branch tip already committed, skip creating anything; just point them at path + instructions. If they need a new worktree and the user didn’t ask for setup, give the handoff **and** the exact `git worktree add …` line the user (or you in a follow-up) can run.

---

## Step 3 — Create the worktree (full-split mode)

```
git worktree add ../<repo-name>-<short-slice-name> -b <branch-name> <base-ref>
```

Name the slice after the **job** (`logiCard-hud-door`, `logiCard-continuous-phase1`), not `agent2`. Base off the Step 2 commit.

Unity lock is **per project path**: separate worktrees ⇒ separate `Library/` ⇒ two Editors/batchmodes can run. Never point both at `D:/projects/Game/logiCard`.

---

## Step 4 — Brief file (full-split, or when the job is non-trivial)

Write `<SLICE_NAME>_AGENT_BRIEF.md` at the **root of the other agent’s worktree** (not only in chat). Include, in order:

1. **Where / why** — branch, base commit, one sentence of project context, that main agent works elsewhere on different files.
2. **The job** — numbered; paths; signatures; “read `X` before writing `Y`.”
3. **Tests** — what to run; Unity batchmode with Editor version from `ProjectSettings/ProjectVersion.txt`.
4. **Boundary** — every path main owns and must not be touched, with **why**. No push/merge/force-push; no other worktrees.
5. **Why safe** — separate directories + file-scope boundary.
6. **Report back** — results, deviations; commit on their branch only; user merges.

For a **tiny** `/btw` slice (e.g. “close this merged worktree”), the paste block alone may be enough — still list boundaries.

---

## Step 5 — Always emit the paste-ready handoff (this is the product)

Put this block near the **top** of your reply when in `/btw` / handoff-first mode so the user can start agent 2 immediately:

```markdown
### Paste to the other agent
> Open `<absolute-worktree-or-repo-path>` and read `<BRIEF_FILENAME>` first, then do what it says.
>
> (If no brief file: paste the full job + boundary inline below this line.)

**Other agent owns:** <files/dirs or “worktree cleanup only”>  
**You (main) keep:** <path> on <branch> — <files you continue editing>  
**Do not overlap:** <explicit conflict list>
```

Then either:

- continue main work in the same response, or  
- one line: “Continuing on \<main task\> in \<main worktree\>.”

Do **not** make the user re-explain context to agent 2. Do **not** go idle after emitting the handoff.

---

## Anti-patterns (from real sessions)

- Assigning “implement feature X” when X is **already on master** → duplicate branch, manual reconciliation, wasted agent.
- Splitting Phase 6 tuning / Bootstrap smoke → needs a human, not a second coder.
- Giving agent 2 files you still have dirty / are about to edit.
- Emitting a long plan for parallelism and then **stopping** main work until the user pastes — that leaves **you** idle; the skill failed.
- Merging the other branch unprompted — user reconciles.

---

## After both sides finish

Ask how they want to reconcile (review diff vs merge). Never merge/push unless they explicitly ask.
