# Brief: BoardView Breach Visuals (Bomber/C36)

**Where / why:** Work in `D:\projects\Game\logiCard-map` (branch `dept/map`, currently idle at `1065116`,
merged/reusable). Integrator owns `master` directly and is not touching `BoardView.cs` this wave — no
file overlap. Base your branch off current `master` tip (`b6da502` at time of writing), not the stale
`dept/map` tip, the same "fork off current master, not a stale branch" mistake this project has already
corrected once (Atmosphere's storm-transition brief).

**Context:** C36's Bomber Sim layer (`BreachPoint`/`BreachState`/`ActionVerb.BombAttach`/`BombDetonate`)
and its `RoundPlayback` presenter (`SyncBreachToSeconds`, mirrors `SyncDoorsToSeconds`) are both landed
and tested on `master` (`docs/contracts/CURRENT.md`'s C36 section, commit `b6da502`). `ArenaBoard` already
exposes `GetBreachState(BreachPoint)` (Intact/Damaged/Breached) and `HasAttachedBomb(BreachPoint)` live,
read correctly every `RoundPlayback.ApplyTime` tick. **No visual exists yet** — a Breached wall currently
opens for Move/Shoot legality but still renders as solid.

**The job:**

1. Read `docs/core/PLAYBACK_CONTRACT.md` first (mandatory per `CLAUDE.md` for any Playback/Execution/
   ReplayTape-adjacent work) — your visual must be a pure function of `ArenaBoard.GetBreachState`, read
   fresh every relevant tick, not a one-shot animation triggered on event-crossing (that's the Healed-
   presenter class of bug this doc exists to prevent).
2. Find how `BoardView.cs` currently renders `Door`/`DoorState` (wall segment material/mesh swap on
   Open/Closed) — mirror that exact mechanism for `BreachPoint`/`BreachState`: Intact/Damaged render as
   the existing wall, Breached renders as an opening (removed/swapped wall mesh — Damaged is reserved,
   unexercised by wall-only v1 per the C36 contract, so it can render identically to Intact for now).
3. Also give `HasAttachedBomb == true` *some* visual (a simple marker/tint is enough — this is Sim-layer
   correctness work, not an art pass; don't over-invest in fidelity here).
4. No map has an authored `BreachPoint` yet (deliberately deferred, human decision pending) — write your
   test(s) by registering a scratch `BreachPoint` directly on a test board/`BoardView.Model`, the same
   pattern `GhostResolverBombTests` (EditMode) and the new `RoundPlaybackPlayModeTests` (PlayMode) already
   use. Do not add a real `RegisterBreachPoint` call to `GameBootstrap`/`MapDefinitions` — that's explicitly
   out of scope until the human picks a wall/map.

**Tests:** Unity batchmode EditMode + PlayMode, Editor **closed** on your worktree's own path
(`D:\projects\Game\logiCard-map`, separate `Library/` from `master`'s so this doesn't collide with any
other batchmode run in flight). Report real pass/fail counts, not a claim.

**Boundary — do not touch:**
- `Assets/_Project/Boot/RoundPlayback.cs`, `Assets/_Project/Net/*`, `Assets/_Project/Sim/*` — frozen
  Sim/Net contract, Integrator-owned, already tested. Read-only for you.
- `Assets/_Project/Boot/GameBootstrap.cs`, `Assets/_Project/Board/MapDefinitions.cs` — map authoring is
  parked pending a human decision; do not register a real `BreachPoint` anywhere.
- `Assets/_Project/UI/ProgramHud.cs` — HUD prompt is a separate brief (UI dept), not yours.

**Report back:** files touched, batchmode results, screenshots/description of the visual if you can render
one. Commit on your branch only — no push, no merge.
