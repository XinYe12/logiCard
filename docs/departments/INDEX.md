# Departments — Active Index

**Updated:** 2026-08-07  
**Ops constitution:** [`../PARALLEL_OPS.md`](../PARALLEL_OPS.md)  
**Contracts:** [`../contracts/CURRENT.md`](../contracts/CURRENT.md)  
**Human rollup:** [`../DRAFT_HANDOFF.md`](../DRAFT_HANDOFF.md)

## Capacity

Integrator + up to **2** coding workers. Current coding fill: **0/2** — Presentation and Audio have both delivered, merged, and been wired in; nothing in flight right now. Ship was docs-only and has also delivered.

## Active agents / worktrees

All four dept slices for this wave are delivered and merged into `master`. Nothing is currently in-flight.

| Dept | Branch | Worktree path | Status file | Notes |
|------|--------|---------------|-------------|-------|
| **Core / Integrator** | `master` @ `04f9191` | `/Users/xuxinye/Documents/projects/Game/LogiCard` | [core/STATUS.md](core/STATUS.md) | Stepped motion, VFX (merged+wired), Audio (merged+wired), Ship docs — all committed. Nothing queued; waiting on human Editor look + ear-check before ticking Day 10/11 on SCHEDULE |
| **Presentation** | `feat/day10-hit-vfx` @ `f2256f6` — **merged + wired into master** | `/Users/xuxinye/Documents/projects/Game/logiCard-day10-vfx` | [presentation/STATUS.md](presentation/STATUS.md) | `MuzzleFlashView` + `WoundSplatView` landed on master (`fc32a2d`) and are driven by RoundPlayback's tape-event loop (`a57d095`); worktree safe to remove |
| **Audio** | `feat/day11-audio-stub` @ `5c402db` — **merged + wired into master** | `/Users/xuxinye/Documents/projects/Game/logiCard-day11-audio` | [audio/STATUS.md](audio/STATUS.md) | `FoleyPlayer`/`IFoleyPlayer` landed (`ef6e3f5`, `7e08aba`) and `Play()` now fires from RoundPlayback/ProgramHud (`04f9191`); worktree safe to remove |
| **Ship** | `feat/ship-docs` @ `fc58db3` — **landed on master** | `/Users/xuxinye/Documents/projects/Game/logiCard-ship-docs` | [ship/STATUS.md](ship/STATUS.md) | README case study + capture checklist landed (`950ff63`); still DRAFT pending human capture + Windows candidate; worktree safe to remove |
| **Verify** | parked | `/Users/xuxinye/Documents/projects/Game/logiCard-verify-playtest` | — | Optional remove; do not commit from here |

## Ownership matrix (write locks)

| Path / concern | Owner now |
|----------------|-----------|
| `Boot/`, `Net/`, `Timeline/`, `Sim/` (fixes) | Core |
| `Board/PawnView.cs`, `Board` wiring via Boot | Core |
| `Board/MuzzleFlashView.cs`, `Board/WoundSplatView.cs` | Presentation |
| `Assets/_Project/Audio/**` | Audio |
| `UI/ProgramHud.cs` (allot/Lock In logic) | Core (Audio may get **sound call** sites only in Wave 2 via Integrator) |
| `docs/DRAFT_HANDOFF.md`, `docs/SCHEDULE.md` ticks, `docs/contracts/CURRENT.md` | Core / Integrator |
| `docs/departments/<dept>/STATUS.md` | That dept only |
| `docs/SHIP_README_DRAFT.md`, `docs/CAPTURE_CHECKLIST.md` | Ship |

## Cross-review checklist (session start)

- [ ] Read DRAFT_HANDOFF  
- [ ] Read this INDEX  
- [ ] Read peer STATUS for every **In progress** row above  
- [ ] Read contracts/CURRENT  
- [ ] Confirm no file overlap before editing  
