---
name: parallel-development
description: Split off a decoupled slice of the current or upcoming work into a separate git worktree for a second agent to run concurrently, with a self-contained brief and an exact handoff command. Trigger phrase — "PARALLEL DEVELOPMENT".
---

# Parallel Development

Invoked when the user says **"PARALLEL DEVELOPMENT"** (or clearly asks to split current/upcoming work with another agent working at the same time). Produces: a new git worktree on its own branch, a self-contained brief file written into it, and the exact command the user pastes to a second agent — without the user having to re-explain the pattern or you re-deriving it from scratch each time.

This project has used this pattern twice already: `VERIFY_AGENT_BRIEF.md` (splitting Day 5/6 test verification from Day 7 door implementation) and `PHASE1_AGENT_BRIEF.md` (splitting continuous-geometry primitives from the sequential resolver/authoring retarget). Read either as a concrete worked example if unsure what "self-contained" and "no file overlap" mean in practice — this skill generalizes both.

## Step 1 — Identify a genuinely decoupled slice

Not everything splits cleanly. Before creating anything, find a slice of work that satisfies **both**:

- **No file overlap.** The slice's files must not be files you (the main agent) are about to edit, or files another in-flight slice already owns. If the slice's work only becomes safe to start once some other piece lands (a type doesn't exist yet, an API isn't frozen), it is not yet splittable — either wait, or split something else that genuinely has zero dependency right now.
- **A crisp, stateable contract.** The other agent needs to be able to do the work correctly from a written brief alone, with zero conversational back-and-forth — it starts cold, no shared context. If the slice can't be described in a page or two (signatures, scenarios, boundary), it's not ready to split; do more design work first (or launch a Plan agent to produce one) rather than handing over an underspecified task.

Good candidates seen in this project: a self-contained new-file-only slice with a frozen public API (geometry primitives with no MonoBehaviour dependency), or a verification/test pass over already-committed code that the main agent isn't touching. Bad candidates: anything requiring types/files the main agent's current work will also modify, or "go figure out how X should work" with no concrete spec.

If nothing currently qualifies, say so plainly instead of forcing a split — a bad split (merge conflicts, contradictory assumptions) costs more than not splitting.

## Step 2 — Checkpoint the current worktree

A worktree only sees **committed** history — uncommitted changes in the current working directory are invisible to a new worktree. Before creating one:

1. `git status --short` to see what's uncommitted.
2. If there's uncommitted work the new worktree's baseline should include, **ask the user before committing it** (never commit without being asked — see the project's standing git-safety rules). Frame it plainly: "I need to commit X as a checkpoint so the new worktree can fork from it — proceed?"
3. Once the working tree is in the state you want the split to fork from, note the commit hash.

## Step 3 — Create the worktree

```
git worktree add ../<repo-name>-<short-slice-name> -b <branch-name> <base-ref>
```

Pick `<short-slice-name>` and `<branch-name>` to describe the slice, not just "agent2" — future-you and the user need to tell worktrees apart later (this project has `logiCard-verify` and `logiCard-continuous-phase1`, not `logiCard-2`/`logiCard-3`). Base off the checkpoint commit from Step 2, not an arbitrary older ref.

## Step 4 — Write the brief file into the new worktree

Create `<SLICE_NAME>_AGENT_BRIEF.md` at the root of the new worktree (not in the main one — the second agent needs to find it immediately on opening that directory). Include, in this order:

1. **Where they are and why.** Branch name, base commit, one sentence on what the overall project/pivot is, and that another agent is working concurrently in a different directory on different files.
2. **The job.** Concrete, numbered — file paths to create/edit, method signatures if you have them, scenarios to cover. Reuse existing patterns/types from the codebase by name and file path wherever possible instead of inventing new ones; tell the agent to read the existing analog first (e.g. "read `X.cs` before writing `Y.cs` — don't reinvent what's already there").
3. **Tests.** What to write, what existing test scenarios to port/reuse, and the exact command to run them (this project's Unity batch-mode invocation, with the correct Editor version — check `ProjectSettings/ProjectVersion.txt` in case it's drifted since this skill file was last used).
4. **Boundary — what NOT to touch, and why.** The single most important section. Name every file/directory the other in-flight work (yours, or a different split) is using, and say explicitly: only create new files in `<scope>`, never edit `<files the main work owns>`, never push/merge/force-push, never touch other worktrees. Explain *why* each restriction exists (usually: "that file is what Phase N in the main worktree is about to retarget, and touching it now creates a conflict") — a rule with a reason is followed more reliably than a bare instruction.
5. **Why the split is safe.** One paragraph: separate directories/working files/Unity `Library/` avoid Editor project-lock contention (this project's Unity Editor can only have one instance per project open); the file-scope boundary means even coincidental file overlap shouldn't mean line overlap.
6. **How to report back.** Test results, deviations from the brief, anything the brief got wrong once real code was involved. Commit locally to their branch; never merge — the user reconciles branches by hand.

## Step 5 — Give the user the exact handoff command

Output a short, copy-paste-ready block the user can hand directly to a second agent (a new Claude Code window/session, Cursor, etc.) — typically just: *"Open `<worktree-path>` and read `<BRIEF_FILENAME>` first, then do what it says."* Don't make the user re-explain scope or context themselves — the brief file already carries all of it. State plainly which worktree/branch you (the main agent) keep working in, so there's no ambiguity about who owns what.

## After both sides finish

Reconciling the two branches into `master` is **the user's call, not automatic** — ask how they want to do it (review the other branch's diff first, or merge directly) rather than merging unprompted. This mirrors the project's standing rule that commits/merges need explicit sign-off.
