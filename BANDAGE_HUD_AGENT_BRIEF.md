# Bandage HUD — Agent Brief (UI seat)

**Seat:** UI  
**Worktree:** `D:\projects\Game\logiCard-modal-restyle`  
**Branch:** `feat/modal-restyle` (or a short-lived `feat/bandage-hud` off current tip — either OK; commit here only)  
**Base tip at brief write:** rebase/merge `master` first if you are behind Integrator’s `e07be61`+ (docs reorg).  
**Contract:** `docs/contracts/CURRENT.md` → **Bandage HUD-side (open 2026-08-14)**  
**Integrator** works on main `D:\projects\Game\logiCard` — dirty rematch/floors/lighting; **does not** implement this HUD.

Read first: this brief → `docs/contracts/CURRENT.md` (Bandage HUD) → `docs/ui/UI_FLOW.md` §4 item 3 → `docs/cards/GEAR_BANDAGE_AGENT_BRIEF.md` §5 → C63 in `docs/core/PRODUCT_MEMORY.md`.  
If touching Execute/Playback presenters: stop — Healed presenter is Integrator **after** this merges.

---

## Job

Ship Bandage Program HUD against the frozen contract. Sim resolve (`ActionVerb.Bandage`, `Healed`, `BandageCharge` carry) is already on master.

1. **`PawnProgram.TryQueueBandage(float executeTime, out string rejectionReason)`**  
   - Cost = `PawnProgram.BandageSeconds` (= **3f**, C63).  
   - Reserve TR like Door/Shoot; book `ActionNode(ActionVerb.Bandage, executeTime, position, stance)`.  
   - Reject if `IsMidSprintAt(executeTime)` (C63 — Walk OK; exclusive mid-segment).  
   - Commit draft first if any. Undo history must include Bandage steps.

2. **`RoundPlayback.BandageChargeOf(int pawnId)`** — mirror `WoundsOf`.  
   **Caution:** main Integrator tree has **uncommitted** rematch edits in this file. Add only the reader; do not rewrite `ResetForNewMatch` / rematch logic.

3. **`BoardInputController.TryQueueBandageAt` + `Mode == Bandage`**  
   - Board tap: nearest booked Move node → that `ExecuteTime`; else place at `UsedSeconds`.  
   - Wire through existing `QueueChanged` / reject events.

4. **`ProgramHud` — dock + arm/place**  
   - `GearHandView.Build` into **queue column** (do **not** grow `ControlsColumnContentHeight` / break `ProgramHudLayoutTests`).  
   - Arm Bandage → `Mode = ActionVerb.Bandage`.  
   - Scrubber **click** while armed → place at scrubber Time Resource seconds.  
   - **Gates:** Wounded (`WoundsOf > 0`); charge (`BandageChargeOf == 0` and no Bandage node in this Program); not mid-Sprint (in `TryQueueBandage`).  
   - Bandage cost label `"3s"`; Interact/Flashbang stay blocked / `"TR —"`; Adrenaline Execute-only.  
   - After place: clear arm, Mode → Move, refresh spent/blocked.  
   - Add `RegisterMatchState` (or equivalent) so wounds/charge come from `RoundPlayback` — if you need a one-line `GameBootstrap` hook, put it in STATUS as “Integrator wire on merge”; **prefer not** editing `GameBootstrap` (Integrator dirty).

5. **Tests**  
   - EditMode: `PawnProgram` Bandage success / budget / mid-Sprint reject.  
   - Update `GearHandViewTests`: Bandage may show `"3s"`; other cards stay `"TR —"`.  
   - PlayMode: arm → place smoke (Gear_Bandage hit target already exists).

6. **STATUS** — set In progress → Ready; report commit hash + batchmode counts.

---

## Boundary — do not touch

| Path | Why |
|------|-----|
| `GameBootstrap.cs` (unless one-line Register unavoidable) | Integrator dirty (rematch/lighting) |
| Rematch / `MatchClock.Reset` / floors / materials / probes | Integrator dirty hold |
| `GhostResolver` / `TapeEvent` Healed **presenter** / `PLAYBACK_CONTRACT` §3 row | Integrator after HUD merge |
| Atmosphere / Character / Cards docs redesign | Other seats |
| `docs/DRAFT_HANDOFF.md`, `docs/contracts/CURRENT.md`, `docs/departments/INDEX.md` | Integrator only |
| Push / merge to `master` | Human + Integrator |

---

## Why safe

Separate worktree; Integrator stays on dirty rematch/floors + INDEX/handoff. Your scope is gear HUD + `TryQueueBandage` (C63 carve-out). No Atmosphere/Character overlap.

---

## Report back

- Commit(s) on your branch only.  
- EditMode / PlayMode counts (batchmode; Editor must not lock **this** worktree path).  
- Deviations from contract (especially GameBootstrap touch or scrubber UX).  
- Ready for Integrator merge when DoD met — do not merge yourself.
