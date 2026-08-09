# Phase 2, first slice: RelayMatchResolver + minimal resolve-relay — Agent Brief

## Where / why

- **Branch:** `feat/phase2-relay-slice`, based on `master` @ `ee8fbd3`.
- **Worktree:** you're already in it (`logiCard-phase2-relay-slice`). Work only here — the Integrator is
  working in the primary tree, not touching any file you own.
- **Project context:** logiCard just shipped Phase 1 (landscape UI). It's now on Phase 2 of a post-pivot
  phase-based schedule (`docs/SCHEDULE.md`, `docs/PRODUCT_MEMORY.md` C46-C51): real networking, currently a
  same-process local stand-in. **Read `docs/NETWORKING_DESIGN.md` in full before writing any code** — it's the
  frozen design doc, including the exact architecture decision (`C52`, 2026-08-09) and a "Phase 2, first
  slice" section written specifically for this brief. Do not skip it; this summary is not a substitute.

## The decision already locked (do not re-litigate)

`C52`: a **custom lightweight resolve-relay backend**, not Photon Fusion / NGO / Mirror. Host-integrity =
**server-authoritative relay**. Rationale (already decided, not yours to revisit): this game's Program → Lock
→ Resolve → Playback loop is episodic, not continuous real-time state, so tick-sync netcode packages are the
wrong tool; `GhostResolver` is already pure, engine-free C# (zero `UnityEngine` references anywhere in `Sim/`
or `Net/GhostResolver.cs`, verified by grep), so running it as a small trusted relay is cheap and doesn't need
a full game-server deploy.

## The frozen contract you build against

`Assets/_Project/Net/IMatchResolver.cs` (landed, do not modify):

```csharp
public interface IMatchResolver
{
    IEnumerator ResolveAsync(IReadOnlyList<GhostInput> inputs, Action<ReplayTape> onResolved);
}
```

`RoundPlayback.ResolveAndArm()` already calls through this interface via `Init`'s new
`IMatchResolver matchResolver = null` parameter (defaults to `LocalMatchResolver`, today's exact same-process
behavior — unchanged, still the default everywhere). **You do not need to touch `RoundPlayback.cs` or
`GameBootstrap.cs` at all** — the Integrator wires your new resolver into `GameBootstrap` once you report
back, same pattern as Phase 1's camera-rect coupling.

**Coroutine gotcha, already hit and fixed — read before writing `ResolveAsync`:** a bare `yield return
someInnerEnumerator` inside a Unity coroutine does **not** drain it synchronously, even when the inner
enumerator never itself yields — Unity defers to the next scheduler pump regardless. This matters for you in
reverse: your `RelayMatchResolver.ResolveAsync` is *expected* to actually yield real waits while network I/O
is in flight (e.g. `yield return null` in a poll loop, or `yield return new WaitUntil(() => _responseReceived)`)
— that's correct, not a bug. Full detail in `NETWORKING_DESIGN.md`'s Implementation Notes if you want the
`RoundPlayback` side of the story.

## The job

1. **A minimal standalone relay process**, outside Unity's asset database (put it at `Relay/LogiCard.Relay/`
   at the repo root, a sibling to `Assets/` — not inside `Assets/`, so Unity's asset importer/compiler never
   touches it). A plain .NET console app (pick a reasonable modern LTS target framework, document your choice
   and why in your report-back).
   - **Must reference the actual shared source**, not a reimplementation or copy: `Assets/_Project/Sim/*.cs`
     and `Assets/_Project/Net/{GhostResolver,TimelinePayload,ActionNode,ActionVerb,ReplayTape,TapeEvent}.cs`
     (confirmed zero `UnityEngine` dependency — they should compile as-is in a plain console app). Link them
     into the new `.csproj` via `<Compile Include="...">` globs pointing at the real files, so resolve logic
     can never drift between what a client previews and what the relay authoritatively computes. Do **not**
     copy-paste or fork these files.
   - **First-slice session model, deliberately minimal:** accepts exactly two connections, pairs them as one
     match, waits for both sides' `GhostInput`, runs `GhostResolver.Resolve` once, sends the identical
     `ReplayTape` back to both. No real matchmaking queue, no persistence, no production hosting concerns —
     those are separate, still-OPEN items in `NETWORKING_DESIGN.md`'s OPEN summary table, explicitly out of
     scope here.
   - **Wire protocol is your call** (per `NETWORKING_DESIGN.md`: "an implementation detail, not a
     design-review item"). Simplest thing that works is fine — e.g. raw TCP with length-prefixed JSON messages
     avoids WebSocket handshake complexity entirely for a first slice and still satisfies "real transport, real
     two-process." Check whether `TimelinePayload`/`GhostInput`/`ReplayTape`/`ActionNode`/`TapeEvent` serialize
     cleanly with `System.Text.Json` as-is (structs, enums, `Dictionary<>` fields might need attributes or a
     custom converter) — don't assume, verify.

2. **`Assets/_Project/Net/RelayMatchResolver.cs`** — new file, implements `IMatchResolver`. Connects to the
   relay (host/port should be easily configurable — constructor params are fine, no need for a settings UI),
   sends this side's own `GhostInput`, and yields (real Unity waits, not a blocking call) until the relay's
   combined `ReplayTape` arrives, then calls `onResolved`. Socket I/O should not block Unity's main thread —
   use a background thread/async socket API polled from the coroutine, or `System.Net.Sockets`' async methods
   awaited via a small poll loop.

3. **Do not wire this into `GameBootstrap.cs`.** That's the Integrator's job at merge time, same boundary
   reason as Phase 1's camera-rect coupling — `Boot/` stays Core-owned.

## Tests / verification

This is the hard part — plan for it explicitly, don't bolt it on at the end:

1. **A standalone integration test, outside Unity**, alongside the relay project (xUnit/NUnit console test,
   your choice): start the relay, connect two plain .NET test clients directly (no Unity needed — just the
   shared `Sim`/`Net` types and your relay-client code), send two different `GhostInput` payloads, assert both
   clients receive an identical `ReplayTape`, and assert it matches what calling `GhostResolver.Resolve`
   locally with the same inputs would produce (determinism check, not just "didn't crash"). This is the real
   proof of Phase 2's exit criterion and doesn't depend on Unity batchmode at all.
2. **Unity-side tests for `RelayMatchResolver`** (EditMode or PlayMode, your call): spin a lightweight
   loopback/stub relay server in-test (doesn't need to be the full relay project — a minimal fake responder is
   fine) and verify `RelayMatchResolver.ResolveAsync`'s coroutine correctly yields while waiting and correctly
   resolves once a response arrives. Do not require two full Unity Editor processes for this — that's not
   automatable from a test.
3. Unity batchmode for the existing suite, to prove zero regression (nothing here should touch anything that
   affects it, but confirm): `D:\unity\Editor\6000.5.5f1\Editor\Unity.exe -batchmode -projectPath
   "D:\projects\Game\logiCard-phase2-relay-slice" -runTests -testPlatform EditMode -testResults <path>
   -acceptSoftwareTermsForThisRunOnly` then the same with `-testPlatform PlayMode`. Never pass `-quit` with
   `-runTests`.
4. In your report-back, include exact instructions for a human to manually smoke-test the real "two processes"
   case (run the relay, run two Unity instances pointed at localhost, complete one round) — you don't need to
   perform this yourself, but the Integrator or the human will want the recipe.

## Boundary — do not touch, and why

- **`Assets/_Project/Boot/GameBootstrap.cs`, `Assets/_Project/Boot/RoundPlayback.cs`** — Core/Integrator-owned;
  the seam is already wired, you build against it, not into it.
- **`Assets/_Project/Net/IMatchResolver.cs`, `LocalMatchResolver.cs`, `GhostResolver.cs`** — frozen contract,
  landed. Don't modify; only add `RelayMatchResolver.cs` alongside them.
- **`Assets/_Project/Sim/**`** — don't modify the actual gameplay/resolve logic, only reference it (via
  `<Compile Include>`) from the new relay project.
- **Ranked/casual queue split, reconnect/disconnect policy, matchmaking cost estimate, anti-cheat audit
  depth** — all separately OPEN in `NETWORKING_DESIGN.md`, explicitly not this slice's job. Don't invent
  answers for these; a two-connection-only relay with no persistence is the correct scope here.
- **Docs** (`DRAFT_HANDOFF.md`, `SCHEDULE.md`, `contracts/CURRENT.md`, `PRODUCT_MEMORY.md`) — Integrator-only.
  Put design deviations/decisions you had to make in your report-back instead.
- No push, no merge to `master`, no other worktrees. Commit on `feat/phase2-relay-slice` only.

## Why this split is safe

Separate worktree (own `Library/`, own Unity batchmode lock) + a clean boundary: everything you touch is
either brand new (the `Relay/` project) or one new file (`RelayMatchResolver.cs`) behind an already-frozen
interface. The Integrator isn't touching `Net/` or `Boot/` right now and won't until you report back.

## Report back

Commit on your branch (never push/merge). Include:

- The relay project's target framework, wire protocol choice, and serialization approach, with your
  reasoning — these were deliberately left to your judgment, explain the call.
- Confirmation the relay references the actual shared `.cs` files (not copies) — show the `<Compile Include>`
  paths.
- Integration test results (the standalone one proving two-process determinism, and the Unity-side
  `RelayMatchResolver` tests).
- Batchmode EditMode + PlayMode results from your own worktree (proving zero regression to the existing
  suite).
- The exact manual two-Unity-instance smoke-test recipe for a human to run.
- Anything from `NETWORKING_DESIGN.md`'s OPEN items you deliberately left untouched, so it's not mistaken for
  "forgot this."
