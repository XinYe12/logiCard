# Worktree / lock audit — 2026-08-05

Read-only pass for parallel cleanup. No production C# edits.

## URP (`art/urp-foundation` → `logiCard-urp-foundation`)

- Branch tip: `23747e8`
- Master includes merge: `4cfe8ea merge art/urp-foundation` (then `0c6f2ae` Door cost HUD).
- Assets present on master: `Assets/_Project/Art/URP/LogiCardURP.asset`, `LightingLab.unity`, URP in `Packages/manifest.json`.
- **Verdict:** fully absorbed. Safe to close.

```powershell
cd D:\projects\Game\logiCard
# Close Unity if it has this worktree path open first
git worktree remove D:\projects\Game\logiCard-urp-foundation
git branch -d art/urp-foundation
```

## Verify (`verify/day5-6-tests` → `logiCard-verify`)

- Branch tip: `87146a7`; brief findings: EditMode **77/77**, PlayMode **23/23**; test-only fixes; manual cold-observer **not** done; SCHEDULE Day 4–6 left unticked on purpose.
- Those test fixes targeted pre-pivot grid fixtures (`GridLineOfSightTests`, etc.) now deleted on master. Continuous pivot rewrote PlayMode fixtures separately.
- **Verdict:** do **not** merge into master (conflict / obsolete). Safe to close; keep the findings note in git history if desired (`VERIFY_AGENT_BRIEF.md` on that branch).

```powershell
git worktree remove D:\projects\Game\logiCard-verify
git branch -d verify/day5-6-tests
```

## HUD Door (`feature/hud-door-verb`)

- Master already has Door verb + `DoorInteractSeconds` on OPEN/CLOSE (`15a335b`, `0c6f2ae`).
- Branch was a duplicate; earlier reconciliation: no remaining must-cherry-picks (optional empty-queue Door hint only).
- Worktree not in `.git/worktrees` anymore. If branch still exists:

```powershell
git branch -d feature/hud-door-verb
```

## Unity “another instance has this project open”

- Lock is **per project path**, not per repo. Batchmode on `D:\projects\Game\logiCard` conflicts only with an Editor (or another batchmode) on **that same path**.
- Other worktrees (`…-verify`, `…-urp-foundation`, …) use different `Library/` + lock files and do **not** block main — unless something incorrectly pointed `-projectPath` at main.
- No `Temp\UnityLockfile` present under main when audited (Editor likely closed, or Temp cleaned). If lock returns:
  1. `Get-Process Unity*` — kill leftover Editors.
  2. If no process but lock file remains under `Temp\UnityLockfile`, delete that file only after confirming no Unity PID.
  3. Never run batchmode while the Editor has the same folder open.

## Not agent-delegable

Phase 6 radius tuning + Bootstrap smoke / cold-observer playtests — human judgment only.
