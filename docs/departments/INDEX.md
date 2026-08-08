# Departments — Active Index

**Updated:** 2026-08-08 — reset for the **C46** scope pivot (see `docs/PRODUCT_MEMORY.md` C46–C51,
`docs/SCHEDULE.md`'s new phase table). Wave 1+2 (Day 10 VFX / Day 11 Audio / Ship docs) is fully shipped and
merged — see git history for the old rows if needed.
**Ops constitution:** [`../PARALLEL_OPS.md`](../PARALLEL_OPS.md) · human-side playbook: [`../DIRECTING_AGENTS.md`](../DIRECTING_AGENTS.md)
**Contracts:** [`../contracts/CURRENT.md`](../contracts/CURRENT.md)
**Human rollup:** [`../DRAFT_HANDOFF.md`](../DRAFT_HANDOFF.md)

## Capacity

Integrator + up to **2** coding workers. Current coding fill: depends on what's in flight — check
`git worktree list` and `DRAFT_HANDOFF.md`'s top section before assuming this table is current; a docs-pivot
wave (two workers, `docs/pivot-new-design-docs` + `docs/pivot-gameplay-art-ui`) landed and merged
2026-08-08, and two older board-rework worker slots (`feat/board-edge-dressing`, `feat/playmode-board-rewrite`)
are still queued but not yet started as of this reset — see `DRAFT_HANDOFF.md` for the live capacity picture.

## Active agents / worktrees

Blank pending the next wave. Populate this table when a new wave's workers spin up — same shape as before
(Dept | Branch | Status file | Notes), one row per active worker.

| Dept | Branch | Status file | Notes |
|------|--------|--------------|-------|
| — | — | — | — |

## Ownership matrix (write locks)

Blank pending the next wave's actual work breakdown — the old rows (`MuzzleFlashView.cs`, `Audio/**`, etc.)
were specific to the shipped Wave 1+2 split and don't map onto whatever this pivot's phases (`SCHEDULE.md`)
actually need. Repopulate per-wave. These rows are **evergreen, not wave-specific** — keep them:

| Path / concern | Owner now |
|----------------|-----------|
| `Boot/`, `Net/`, `Timeline/`, `Sim/` (fixes) | Core |
| `docs/DRAFT_HANDOFF.md`, `docs/SCHEDULE.md` ticks, `docs/contracts/CURRENT.md`, `docs/PRODUCT_MEMORY.md` | Core / Integrator |
| `docs/departments/<dept>/STATUS.md` | That dept only |

## Cross-review checklist (session start)

- [ ] Read DRAFT_HANDOFF
- [ ] Read this INDEX
- [ ] Read peer STATUS for every **In progress** row above
- [ ] Read contracts/CURRENT
- [ ] Confirm no file overlap before editing
