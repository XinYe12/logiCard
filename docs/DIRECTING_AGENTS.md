# Directing Agents — a playbook for the human on this project

**Audience:** you (the human/PM), not the agents. `PARALLEL_OPS.md` is the constitution agents follow; this
is the companion doc for how to get good results out of that system — what to check, what to ask, and what to
be suspicious of. Every example below is a real incident from this project's history, not a hypothetical.

## The mental model

One **Integrator** (main tree, merge authority, owns `Boot/`/`Net/`/`Timeline/`/`Sim/` and the docs that
track state) plus up to **2 Workers** (separate worktrees, file-scoped briefs, never merge themselves). You
are the actual gate — agents propose, you approve. The system only works if you're doing real review at the
gate, not rubber-stamping "looks done."

## Your actual session routine

**Starting a session:**
1. Open a terminal in `D:\projects\Game\logiCard` (the main tree — never point two sessions at this exact
   path at once).
2. Run `git status` and `git worktree list` before asking for anything. Read the output. A worktree you
   don't remember creating is a "what is this?" question, not something to ignore.
3. Skim the top of `docs/DRAFT_HANDOFF.md` — it's the running log and should reflect where the last session
   left off.
4. State what you want. If it's big or ambiguous, let the agent use Plan mode rather than pushing it to code
   immediately — reviewing a written plan costs one read; reviewing a wrong implementation costs a redo.

**Spinning up a second worker**, once the Integrator hands you a worktree path + brief filename:
1. Open a **new, separate terminal window** — not a tab in your main session.
2. `cd` into the exact path you were given, e.g.:
   ```
   cd "D:\projects\Game\logiCard-<slice-name>"
   ```
3. Start a Claude session in that window and paste exactly:
   ```
   Read <BRIEF_FILENAME> and do what it says.
   ```
4. That session only sees that folder. It can't touch your main session or any other worktree by accident.

**When a worker reports back:**
1. Have the Integrator diff the worker's changes against its brief's file boundary before merging — a
   worker that touched something outside its brief is a real flag, not paperwork.
2. If it's visual/feel work, look at it yourself before approving — an agent's description of its own visual
   work is not the same as you looking at it (see "What tests can and can't tell you" below).
3. Say "merge it" explicitly. Nothing merges to `master` on its own.

**Ending a session:**
1. Ask "what's uncommitted right now, and why" — expect a real file-by-file answer, not "just some stuff."
2. Ask whether `DRAFT_HANDOFF.md` actually reflects current state. If it has a queue/summary section, this is
   the moment to refresh it — not something to leave for the next session to discover is stale.
3. Decide on pushing to `origin` now vs. explicitly deferring it. Either is fine; letting it become invisible
   debt nobody tracks is not.

## Before you trust a "done" report

An agent's summary describes what it *intended* to do, not necessarily what happened. Three things worth
checking, cheaply, before accepting a report at face value:

- **Did anyone else touch this repo since the last session?** `git worktree list` and `git status` at the
  start of anything. This project has repeatedly had orphaned worktrees with real uncommitted state that
  nobody flagged — a docs branch that had already solved a question the current session was still guessing
  at, a stale verify-worktree with drift nobody cleaned up. Both were only caught because someone happened to
  run `git worktree list` and asked "what's this?" instead of assuming a clean tree.
- **Is a factual claim actually verified, or just repeated back?** Mid-session, an agent asked "where did
  this asset come from?" and got a quick verbal answer ("Kenney") that turned out wrong on inspection — the
  real source was a different pack entirely (Quaternius), confirmed only because a separate concurrent
  session had actually verified it against the source with you directly. If something's about to get written
  down as fact in a doc (a license, a provenance claim, a decision rationale) and the only backing is "I
  think" or "someone said," that's a flag to verify against the actual source before it ships, not after.
- **Does this touch something already locked?** This project tracks binding decisions as numbered rows in
  `PRODUCT_MEMORY.md` (`C17`, `C39`, etc.) — some explicitly marked "demo-binding." A board redesign this
  session quietly contradicted a locked footprint decision (`C39` item 7) that nobody had flagged; it only
  surfaced because the agent happened to grep `PRODUCT_MEMORY.md`/`GDD.md` before implementing instead of
  just building what was asked. If you're asking for something that feels like it might be bigger than a
  tweak, it's worth asking directly: "does this reopen a locked decision?" — don't assume the agent checked.

## When to actually delegate to a second worker

Real parallelism needs a slice that's **file-disjoint** and either **new-file/additive** or buildable against
a **frozen contract** (exact numbers/API already decided, just not yet merged). This project's own successful
examples: Presentation building `MuzzleFlashView`/`WoundSplatView` against a frozen `Init`/`Place`/`SetVisible`
signature before Core's wiring landed; a test-rewrite worker building against a fully-specified new board
layout before the Integrator's own commit existed.

**Bad delegation** (produces rework, not speed): "go figure out X" with no frozen spec, or assigning a file
the Integrator is still mid-edit on. If you're not sure a slice is real, ask the Integrator "what would the
other agent actually need to know to start right now, with nothing else landed yet?" — if the honest answer
is "wait for me to finish," it's not parallel, it's just later.

**Capacity is 1 Integrator + 2 Workers.** Watch for an Integrator quietly doing everything solo and going
idle at blockers instead of spinning up a worker — that's a process failure, not a capacity limit, and worth
calling out directly if you see it (as happened once this session).

## What tests can and can't tell you

Batchmode EditMode/PlayMode green means "doesn't crash" and "existing contracts still hold." It proves
**nothing** about whether art looks right, a level feels right, or a UI reads well — this project has no
automated visual-quality checks anywhere. For anything in that category (pawn art, board dressing, lighting,
camera framing, level feel), the loop is: agent implements → **you** Play and look/feel it → paste a
screenshot or describe what's off → agent iterates. There's no shortcut here; budget it as real time, not
something that happens automatically alongside code review. Batchmode verification is a regression safety
net that runs *in addition to*, never *instead of*, your look.

## What "OK to merge?" should actually mean

Before approving a merge, a quick real check beats a fast "yes":

1. Did the worker stay inside its briefed file boundary? (Diff `git diff --stat` against the brief's "owns"
   list — a worker touching a file outside its brief is a real flag, not paperwork.)
2. Is there an actual batchmode result, or just a claim of one? Ask to see the pass/fail counts, not just
   "tests pass."
3. For anything visual/feel-based: did *you* actually look, or are you approving on the agent's description
   of its own work?

## Reading order (so your asks land coherently)

`CLAUDE.md` → `docs/DRAFT_HANDOFF.md` (always, it's the daily rollup) → if multiple agents are or might be in
flight, `docs/PARALLEL_OPS.md` + `docs/departments/INDEX.md`. If you're about to ask for something that
sounds like it might already be scoped somewhere (a plan doc, a findings file), a quick "check if this is
already covered in docs/ before starting" costs one turn and has already saved real rework this project.

## Quick reference

- Who owns what file right now: `docs/departments/INDEX.md`'s ownership matrix.
- Frozen cross-agent contracts this wave: `docs/contracts/CURRENT.md`.
- Locked product decisions: `docs/PRODUCT_MEMORY.md`'s numbered rows.
- The actual agent-side rules this doc assumes you understand the *consequences* of: `docs/PARALLEL_OPS.md`.
