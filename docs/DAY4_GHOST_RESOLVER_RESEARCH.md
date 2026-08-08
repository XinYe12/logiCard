# Day 4 / M1 Slice 1 — Ghost Resolve + Playback Research Note

**Status:** Research only, 2026-07-30. Not a confirmed design change — see [PRODUCT_MEMORY.md](PRODUCT_MEMORY.md) (C26). **Historical note (2026-08-08):** written against the pre-pivot grid model (`GridCoordinate` types throughout) — superseded by the shipped continuous-space model, see [CONTINUOUS_PIVOT_PLAN.md](CONTINUOUS_PIVOT_PLAN.md). The assembly-boundary reasoning in §D still applies verbatim to the continuous version; the grid-specific code sketches do not.
**Scope:** Day 4 of [SCHEDULE.md](SCHEDULE.md) — local Host-style ghost resolve + playback of moves/shoots, Wound stub text on hit.

Sources read: [VERTICAL_SLICE.md](VERTICAL_SLICE.md) (D7), [TDD.md](TDD.md) (D6 §2–§6), [GDD.md](GDD.md) (D4 §3A, §5, §6), [CORE_LOOP.md](CORE_LOOP.md) (D3), [SCHEDULE.md](SCHEDULE.md) (D8 Day 4), [PRODUCT_MEMORY.md](PRODUCT_MEMORY.md) (C23/C24/C27/C28), [TABLETOP_RULES.md](TABLETOP_RULES.md) (D5 §IV), [UI_FLOW.md](UI_FLOW.md) §7. Also read current code: `Sim/*`, `Net/*`, `Timeline/PawnProgram.cs`, `Board/*`, `UI/ProgramHud.cs`, `Boot/GameBootstrap.cs`, and all four `.asmdef` files.

---

## A. What Day 4 / M1 must prove

Day 4 DoD in `SCHEDULE.md`: "local Host-style ghost resolve (even offline) + playback moves/shoots; Wound stub text on hit", exit = the D7 Slice 1 checklist minus networking.

Observable behaviors that must be true at the end of Day 4:

1. Lock In compiles the local `TimelinePayload`(s) and hands them to a resolver **before** playback starts (no live simulation during Execute). This is the C23 shape: Payload → Host ghost → ReplayTape → playback.
2. Pressing Play / scrubbing during Execute drives pawns from resolver output only. The pawn views must not read `PawnProgram` directly anymore.
3. A pawn visibly arrives at its scheduled destination at the correct **Time Resource second** on the scrubber (Playback Duration may compress — currently 60s TR in 8s real-world via `TimeResourceClockDriver.PlaybackSecondsForFullBudget`).
4. A shoot is visibly distinct from a move (D7 "Move vs. shoot are visually distinct actions") — a tracer line / muzzle flash primitive is enough.
5. If the target is in LoS at the shoot instant, a **"Wounded"** stub text appears (D7 explicitly allows stub text for wound/win state).
6. Cold-observer test: someone who did not watch you build it can point at the scrubber and say which scheduled action caused which effect.

Not in scope for Day 4: stance bands (hardcode Walk), Hold Angle, real path drawing, doors, wound surcharge re-timing, Bandage, networking.

---

## B. TimelinePayload → outcomes pipeline

Current payload reality (`Net/ActionNode.cs`, `Timeline/PawnProgram.cs`):

- `ActionNode { ActionVerb Verb, float ExecuteTime, GridCoordinate GridPosition, StanceType Stance, CardData Modifier }`.
- `PawnProgram` appends nodes in schedule order, and sets `ExecuteTime = UsedSeconds` **after** reserving cost. So `ExecuteTime` is the **completion** second, not the start: a Move node's `ExecuteTime` is the arrival second; a Shoot node's `ExecuteTime` is the end of its 2s Snap window.
- `GridPosition` is overloaded: destination tile for Move, **target tile** for Shoot. The resolver must branch on `Verb`.
- The payload carries no pawn identity and no start coordinate (the class comment says identity travels separately at Day 11). The resolver therefore needs a small input wrapper.

Pipeline, step by step:

**1. Compile ghost tracks (per pawn).** Walk the pawn's nodes in ascending `ExecuteTime`, starting from the pawn's spawn coordinate at t=0.

- Move node: the pawn occupies its previous tile until `ExecuteTime - segmentCost`, then interpolates to `GridPosition`, arriving exactly at `ExecuteTime`. Since `PawnProgram` bookkeeps a single running clock, the simplest faithful reconstruction is: `segmentStart = previousNodeExecuteTime` (or 0 for the first node), `segmentEnd = node.ExecuteTime`. That automatically produces the correct dwell for Shoot nodes, because a Shoot node consumes 2s of the running clock.
- Shoot node: no positional change; the pawn holds position for the shoot window `[previousExecuteTime, ExecuteTime]`.
- Output per pawn is a `ScheduledPath` (waypoints + arrival seconds). This type already exists and already does clamped interpolation via `Evaluate(seconds)`, and `PawnView.ApplyTime` already samples it — so a `ScheduledPath` is the natural "ghost position over Time Resource" carrier for both the sim and the tape.
- **Do not reuse `PawnProgram.BuildMovePreviewPath` for resolve**: its own doc comment says it ignores time consumed by interleaved Shoot nodes, so its arrival seconds are wrong once a Shoot sits between two Moves. Day 4 needs a resolve-side compile that respects shoot dwell.

**2. Collect shoot events.** Every Shoot node becomes a candidate event at `t = node.ExecuteTime` with `shooterId`, `shooterStance`, and the aimed direction / target tile.

**3. Evaluate LoS at each shoot instant.** For each shoot event at time `t`:

- Shooter ghost tile = round/snap of `track[shooter].Evaluate(t)`. Victim ghost tile = same for every other pawn.
- Same floor required (GDD §5). Bresenham (TDD §4) from shooter tile to victim tile; walk the intermediate tiles and reject if any is impassable. With no doors in Slice 1 the only blockers are board bounds, so the line is effectively always clear — but implement the Bresenham walk now so Day 9 doors drop in without touching the resolver.
- GDD §5 also constrains LoS to orthogonal lines (and `PawnProgram.TryQueueShoot` already rejects non-row/column targets), so in practice Slice 1's Bresenham degenerates to a straight row or column scan.

**4. Apply wound outcomes.** Snap Shot wounds on hit, and misses a target that is in Sprint stance at that instant (GDD §3A/§5). Group events that fall within a small epsilon of the same second and resolve them against a **snapshot** of health taken before the group, so a mutual exchange produces two wounds rather than one shooter dying first (paper D5 §IV "simultaneous tie → both take a Wound"). Wounded is a stub state for Slice 1: emit the event, print text, and do **not** re-time the remaining schedule (GDD's +1 tick surcharge and the paper's +1s base-speed recalc both belong to Day 8 / Slice 3, and both would require a second resolve pass).

**5. Emit the ReplayTape.** Immutable, sorted by Time Resource second, per TDD §4 Phase 4 ("Tick 140: P1 moves to X · Tick 145: P2 fires · Tick 145: P1 wounded"). It should carry two things:

- **Continuous tracks**: pawnId → `ScheduledPath`, sampled every frame by `PawnView.ApplyTime` for smooth motion.
- **Discrete events**: `MoveArrive`, `ShootFire`, `Wounded`, (later `Killed`, `Invalid`), each with a second, a pawn, and an optional target — these drive the tracer VFX and the "Wounded" text.

**6. Playback.** `TimeResourceClockDriver.TimeChanged` already fires with the current TR second, and `GameBootstrap.ApplyTimeToPawns` already fans it out. Day 4 replaces the hardcoded paths with tape tracks and adds an event cursor: advance the cursor while `events[i].Seconds <= now` and fire them; if the user scrubs backwards (the HUD slider allows it), reset the cursor and pawn state to zero and re-apply forward. Re-applying from scratch is cheap at this scale and keeps scrubbing consistent.

---

## C. Combat rules that matter for Slice 1

Only these; everything else is Slice 2+.

| Rule | Source | Slice 1 treatment |
|---|---|---|
| LoS is same-floor, orthogonal, blocked by closed doors | GDD §5 | Bresenham grid walk; no doors exist yet, so it always clears |
| Authority LoS must not use physics raycasts | TDD §4 | Pure integer grid math, unit-testable |
| Snap Shot wounds on hit | GDD §3A/§6 | The one outcome Day 4 needs |
| Snap Shot misses a Sprinting target | GDD §3A/§5 | Implement the check, but it can't trigger — Slice 1 hardcodes one stance |
| Cannot fire while Sprinting | GDD §3.2 | Already enforced at program time in `PawnProgram.TryQueueShoot` |
| Simultaneous exchange | D5 §IV (mutual wound) vs GDD §5 (mutual *lethal* = Draw) | Snap-only Slice 1 → mutual **wound**, both sides. The Draw rule needs Hold Angle, which is Day 6 |
| Hold Angle lethal / hits Sprint | GDD §3A | Out of scope; `ShootCost.HoldAngleSeconds` already reserved |
| Wounded = +1 tick surcharge, Bandage deadline | GDD §5 | Out of scope; stub text only per D7 |
| Otherwise Invalid → Stop | GDD §4, D3 §6 | Out of scope for Day 4, but leaving an `Invalid` event type in the tape enum costs nothing and Day 8 needs it |

---

## D. Minimal GhostResolver sketch

**Assembly-definition constraint (important, and not in any doc).** `LogiCard.Sim.asmdef` has `references: []` and `noEngineReferences: true`; `LogiCard.Net.asmdef` references Sim and Cards. So a resolver that takes a `TimelinePayload` **cannot** live in `Sim/` without creating a circular assembly reference. TDD §6 wants the ghost sim in `Sim/` and the ReplayTape in `Net/`, and the clean split that satisfies both the doc and the asmdefs is:

- `Sim/GridLineOfSight.cs` — Bresenham + blocker walk, pure, no Net types.
- `Sim/CombatRules.cs` (optional) — "does this shot wound this stance" as a static predicate.
- `Net/ReplayTape.cs`, `Net/TapeEvent.cs`, `Net/GhostResolver.cs` — the payload-driven orchestrator, which is also where TDD §6 already puts ReplayTape. `Net` is engine-referencing but the resolver itself can stay plain C#, so the existing EditMode test assembly (which already references Sim, Timeline, and Net) can unit-test it without a scene.

```csharp
namespace LogiCard.Net
{
    public enum TapeEventType { MoveArrive, ShootFire, Wounded, Killed, Invalid }

    public readonly struct TapeEvent
    {
        public float Seconds { get; }
        public int PawnId { get; }
        public TapeEventType Type { get; }
        public GridCoordinate Coordinate { get; }   // arrival tile, or shot target tile
        public int TargetPawnId { get; }            // -1 when not applicable
        public string Text { get; }                 // stub label, e.g. "Wounded"
    }

    public readonly struct GhostInput
    {
        public int PawnId { get; }
        public GridCoordinate Start { get; }
        public float BaseSecondsPerTile { get; }
        public TimelinePayload Payload { get; }
    }

    public sealed class ReplayTape
    {
        public IReadOnlyDictionary<int, ScheduledPath> Tracks { get; } // continuous ghost position
        public IReadOnlyList<TapeEvent> Events { get; }               // sorted by Seconds
        public float EndSeconds { get; }
    }

    /// Offline stand-in for the Fusion Host black box (TDD §3 Phase 3, C23).
    /// Day 11 swaps only who calls Resolve and how the tape is transported.
    public sealed class GhostResolver
    {
        public GhostResolver(GridBoard board, float simultaneityEpsilon = 0.01f);

        public ReplayTape Resolve(IReadOnlyList<GhostInput> inputs);

        // --- internals ---
        // ScheduledPath CompileTrack(GhostInput input);
        //   nodes ascending by ExecuteTime; Move -> segment [prevTime, ExecuteTime];
        //   Shoot -> dwell at current tile for its window.
        // List<ShotIntent> CollectShots(IReadOnlyList<GhostInput> inputs);
        // void ResolveShotGroup(float t, IReadOnlyList<ShotIntent> simultaneous,
        //                       IReadOnlyDictionary<int, ScheduledPath> tracks,
        //                       PawnHealth[] snapshot, List<TapeEvent> outEvents);
    }
}
```

```csharp
namespace LogiCard.Sim
{
    public static class GridLineOfSight
    {
        /// Integer Bresenham walk; false if any tile between from and to is impassable
        /// or the two coordinates are on different floors (TDD §4, GDD §5).
        public static bool HasLineOfSight(GridBoard board, GridCoordinate from, GridCoordinate to);
    }
}
```

Determinism notes for the sketch: sort events by `(Seconds, PawnId)` so ties are stable; group simultaneity with an explicit epsilon rather than float equality; take the health snapshot before applying a group so mutual exchanges are symmetric. Resolve should be a pure function of `(board, inputs)` with no `UnityEngine.Time` or `Random` — that is what makes the Day 11 transport swap a no-op for outcomes (TDD §3 Phase 5, "desync of outcomes is impossible").

Wiring on Day 4: `GameBootstrap` (or a small `RoundResolveController`) builds `GhostInput`s from the two `PawnProgram`s on Lock In, calls `Resolve`, hands `tape.Tracks[pawnId]` to each `PawnView`, and subscribes an event cursor to `TimeResourceClockDriver.TimeChanged`.

---

## E. Open questions / risks

1. **`ExecuteTime` semantics are undocumented.** `PawnProgram` writes the *completion* second; TDD §2's example (`14.5s`) reads like a trigger instant. The resolver's track compile depends entirely on which it is. Worth a one-line comment lock before writing the resolver, because Day 5's path/stance work and Day 11's revalidation both inherit the choice.
2. **When exactly does a Snap Shot resolve?** GDD §3A says "requires LoS at resolve time" but Snap has a 2s duration. Resolving at the window's end (i.e. at `ExecuteTime`) is the simplest and matches the compiled data; resolving at the start, or requiring LoS across the whole window, are both defensible and would change outcomes. Hold Angle (Day 6) explicitly *is* a window, so the distinction becomes load-bearing then, not now.
3. **Does a pawn block LoS?** GDD §5 lists only closed doors as blockers, and there are only two pawns in the demo, so it does not matter for Slice 1 — but it will once the rail car (C31, "bulletproof") arrives.
4. **Mutual-outcome rule is split across docs.** Paper D5 §IV = mutual wound; GDD §5 = mutual *lethal* → Draw. They are reconcilable (Draw applies to lethal Hold Angle), and Slice 1 is Snap-only, so mutual wound is the safe read. Flag it for the Day 6 playtest note.
5. **Wound has no Slice-1 mechanical effect, by design.** Applying GDD's +1 tick surcharge mid-resolve would require re-timing the victim's remaining nodes and re-running the shot pass. D7 says stub text is fine, so keep the resolver single-pass on Day 4 and budget the re-timing work for Day 8. Be aware the single-pass shape is what Flashbang (TDD §4, "+3.0s shifting subsequent ActionNode times") will eventually break — the resolver should be structured as a loop over a time-ordered event queue so re-timing can be added, not as two hardcoded passes.
6. **Payload lacks identity and origin.** `TimelinePayload` has no pawn id and no start tile, so the resolver needs the `GhostInput` wrapper above (or the payload gains those fields). Deciding now avoids a Day 11 refactor when the RPC carries the sender separately.
7. **Assembly boundary**, as described in section D — `Sim` cannot reference `Net`, so decide the resolver's home before creating the file.
8. **Scrubbing backwards during Execute** is reachable today via the HUD slider, so the event cursor needs a reset path or wounds will fire twice / not at all on a rewind.
9. **Stance is fixed at construction.** `PawnProgram.CurrentStance` is get-only and defaults to Walk, so the Sprint-evades-Snap branch is untestable in-game on Day 4; cover it with an EditMode unit test instead of trying to exercise it through the HUD.
