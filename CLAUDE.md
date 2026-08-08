# logiCard — Agent Instructions

Continuous-space tactics prototype (Unity 6000.5.5f1). Turn-based programmed-movement combat: players draft a path/shoot/door program each round against a Time Resource budget, then it resolves and plays back deterministically. See `docs/GDD.md` and `docs/CORE_LOOP.md` for the design; `docs/DRAFT_HANDOFF.md` for the current session-to-session state (read it first in any new session — it's the running log of what's actually landed vs. still open, and is usually more current than any doc below it).

## Multi-agent / parallel work

When more than one agent is in flight, also read `docs/PARALLEL_OPS.md` and `docs/departments/INDEX.md` at session start (then peer `STATUS.md` + `docs/contracts/CURRENT.md`). Never share a working tree with another agent; workers update only their own department STATUS; Integrator alone edits DRAFT_HANDOFF, SCHEDULE ticks, and contracts.

## Before touching UI that lets the player change a board object's state

**Read `docs/UI_BOARD_ANCHORED_COMPONENTS.md` before writing or modifying any interaction control tied to a board object** — a door, a future power station/terminal/pickup, anything the player selects and then changes the state of. This covers two things, both mandatory, not optional style guidance:

1. **The content contract** — every such control must show identity (what you're acting on), live state (read from the authoritative model, never inferred from player input), and options (each its own labeled, explicit-confirm control). This project has already shipped bugs from skipping each leg of this once.
2. **The positioning mechanics** — the world→screen→canvas-local conversion pipeline, and a specific anchor/pivot pitfall that silently mispositions a board-anchored element if you get it wrong (it did, once). Don't re-derive this from scratch; copy the reference implementation the doc points to.

If you're building this kind of control and haven't read that doc in this session, read it now, before writing code.

## Docs are the source of truth for *why*, not just *what*

This project's `docs/` folder carries decisions and their reasoning (`CONTINUOUS_PIVOT_PLAN.md`, `PRODUCT_MEMORY.md`, `CORE_LOOP.md`, etc.) — when a change touches an area with a doc, check it before assuming the code alone tells the whole story. When you land a decision or convention future sessions need to follow (not just a one-off fix), write it into a doc under `docs/` and, if it's something every session should know before starting related work, add a pointer here. A doc nobody's told to read doesn't do anything — the pointer is not optional.

## Testing

Unity batchmode EditMode + PlayMode tests are the bar. Batchmode requires the Editor **closed** on this exact project path (`D:\projects\Game\logiCard`) — other worktree paths don't share that lock. Don't claim green without an actual run; `docs/DRAFT_HANDOFF.md` tracks verification status explicitly and should say so honestly.
