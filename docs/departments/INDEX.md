# Departments — Active Index

**Updated:** 2026-08-09 — Phase 1 shipped and merged; Phase 2 first slice (relay resolver) now in flight.
Wave 1+2 (Day 10 VFX / Day 11 Audio / Ship docs) is fully shipped and merged — see git history for the old
rows if needed.
**Ops constitution:** [`../PARALLEL_OPS.md`](../PARALLEL_OPS.md) · human-side playbook: [`../DIRECTING_AGENTS.md`](../DIRECTING_AGENTS.md)
**Contracts:** [`../contracts/CURRENT.md`](../contracts/CURRENT.md)
**Human rollup:** [`../DRAFT_HANDOFF.md`](../DRAFT_HANDOFF.md)

## Capacity

Integrator + up to **2** coding workers. `feat/phase1-landscape-ui` merged 2026-08-09. `feat/phase2-relay-slice`
spun 2026-08-09 (1 of 2 worker slots in use) — 1 slot still open. Check `git worktree list` and
`DRAFT_HANDOFF.md`'s top section before assuming this table is current.

## Active agents / worktrees

| Dept | Branch | Status file | Notes |
|------|--------|--------------|-------|
| Core (networking) | `feat/phase2-relay-slice` | `logiCard-phase2-relay-slice` worktree (no dept STATUS.md — same pattern as Phase 1) | Building `RelayMatchResolver` + a minimal standalone resolve-relay process (new `Relay/` project outside `Assets/`), behind the frozen `IMatchResolver` contract landed on `master`. Brief: `PHASE2_RELAY_SLICE_AGENT_BRIEF.md` at the worktree root. |

## Ownership matrix (write locks)

Wave-specific row added on top of the evergreen ones below:

| Path / concern | Owner now |
|----------------|-----------|
| `Assets/_Project/Net/RelayMatchResolver.cs` (new) + `Relay/**` (new, repo-root sibling to `Assets/`) | Core (`feat/phase2-relay-slice`) |
| `Assets/_Project/Boot/GameBootstrap.cs`, `Assets/_Project/Boot/RoundPlayback.cs` | Core/Integrator — the `IMatchResolver` seam is already landed there; the relay-slice worker builds against it, doesn't edit it |
| `Assets/_Project/Net/IMatchResolver.cs`, `LocalMatchResolver.cs`, `GhostResolver.cs` | Frozen, landed — nobody edits these this wave |
| `Boot/`, `Net/`, `Timeline/`, `Sim/` (fixes) | Core |
| `docs/DRAFT_HANDOFF.md`, `docs/SCHEDULE.md` ticks, `docs/contracts/CURRENT.md`, `docs/PRODUCT_MEMORY.md` | Core / Integrator |
| `docs/departments/<dept>/STATUS.md` | That dept only |

## Cross-review checklist (session start)

- [ ] Read DRAFT_HANDOFF
- [ ] Read this INDEX
- [ ] Read peer STATUS for every **In progress** row above
- [ ] Read contracts/CURRENT
- [ ] Confirm no file overlap before editing
