# D6: Technical Design Document (TDD)

**Doc ID:** D6  
**Status:** Stakeholder v1.0 — 2026-07-29; networking §1 + continuous resolve §2/§4 amended 2026-08-08 (**C51** / **C35/C39**)  
**Focus:** Host-Authoritative Timeline Resolution (Unity)  
**Depends on:** [GDD.md](GDD.md), [TABLETOP_RULES.md](TABLETOP_RULES.md), [CORE_LOOP.md](CORE_LOOP.md), [NETWORKING_DESIGN.md](NETWORKING_DESIGN.md)

---

## 1. Network Topology

Topology, transport choice, matchmaking, host-integrity, and reconnect design live in
**[NETWORKING_DESIGN.md](NETWORKING_DESIGN.md)** (**C51**) — that doc is the source of truth.
Today's build still runs both programs in-process through `GhostResolver` (no real transport yet);
the reusable shape remains Host-authoritative ghost sim → `ReplayTape` → client playback.

---

## 2. Data Structures (The Timeline Payload)

When a player draws a path, sets stances, and drops cards on the UI timeline, the engine compiles this into a serialized struct: **`TimelinePayload`**.

The client sends this payload to the Host via **RPC** (or the networked successor described in `NETWORKING_DESIGN.md`).

**Payload = array of `ActionNode`:**

| Field | Type | Example |
|-------|------|---------|
| `ExecuteTime` | `float` | `14.5` seconds (Time Resource completion second) |
| `Position` | `PlanarPosition` (continuous `X`/`Y` + `Floor`) | `(4.0, 3.2, Ground)` — see `Assets/_Project/Sim/PlanarPosition.cs`; Move waypoint, Shoot aim point, or Door interaction point depending on verb |
| `Stance` | `StanceType` enum | Sprint, Walk, Crawl |
| `Modifier` | `CardData` nullable | Flashbang, Hold Angle, Bandage, Breach/Interact |

Client-side pre-check: path must fit Character Speed × Stance math and time budget before Lock is allowed.

---

## 3. The Core Network Loop (Phase Transition)

### Phase 1: Local Programming (Clients)
Players draw paths and place cards in UI. **No network traffic** yet. Client verifies math vs Character attributes and budget.

### Phase 2: The Handshake (Client → Host RPC)
On timer zero or both **Lock In**: clients RPC `TimelinePayload` to Host. Input freezes; UI shows **"Simulating..."**.

### Phase 3: Host Resolution — Black Box (Host only)
Host receives both payloads. Runs **Ghost Simulation** with no requirement to render: steps timeline (second-by-second or fixed sim dt), updates ghost grid positions from Stance, grid LoS, wounds, card effects.

### Phase 4: The Playback Tape (Host → Clients)
Host compiles immutable **`ReplayTape`** (e.g. `Tick 140: P1 moves to X` · `Tick 145: P2 fires` · `Tick 145: P1 wounded`). Tape synced to all clients.

### Phase 5: Cinematic Execution (Clients)
Clients scrub UI and drive transforms/Animators from the same tape. **Desync of outcomes is impossible** — clients are playback, not simulators.

```mermaid
flowchart LR
  prog[Local_Program] --> lock[RPC_TimelinePayload]
  lock --> ghost[Host_GhostSim]
  ghost --> tape[ReplayTape_Sync]
  tape --> play[Client_Playback]
```

---

## 4. Resolving Mechanics (Host Logic)

### Pathing & Stances
* **No NavMesh** for resolve authority — **visibility graph + Dijkstra** over continuous space (`Assets/_Project/Sim/ContinuousPathfinder.cs`, **C39**). Pure C# float math for determinism; never engine navigation.
* Segment time ≈ `Distance * CharacterBaseSpeed * StanceModifier` (Euclidean distance — **C35/C39**; align constants with GDD / paper learnings).
* Host updates ghost `PlanarPosition` each sim step along booked nodes.

### Combat & LoS
* LoS via **continuous segment-vs-obstacle intersection** (`Assets/_Project/Sim/ContinuousLineOfSight.cs`) — **not** Unity physics raycasts for authority. **Never a physics raycast** — Host resolve must stay bit-identical everywhere (**C32** discipline, unchanged under real networking).
* **Snap Shot:** wounds pawns within `HitRadius` of the free-aim point at completion, clear LoS required.
* **Hold Angle:** analytic segment-vs-lane sweep within `LaneHalfWidth` across the aim window (not discrete instant-probing).
* Same-frame LoS both ways: compare **Stance** (e.g. Walk > Sprint). Equal stance → mutual wounds (paper) / lethal draw rules per GDD.

### Card Effects
* Event triggers at `ExecuteTime`. Example: Flashbang at `12.0s` stuns victims in radius → **+3.0s** delay, shifting subsequent `ActionNode` times backward on that ghost’s schedule.

### Demo quantization note
Digital build uses **continuous `ExecuteTime` (Time Resource seconds)** in `TimelinePayload`. **Playback Duration** on clients is separate and tunable (**C27/C28**). Do not quantize to a 12-tick clock. Authoritative Host (local stand-in today; real transport per `NETWORKING_DESIGN.md`) still owns the Black Box.

---

## 5. Art & Animation Integration

* Clients read timestamps only.
* **No root motion** — script moves transforms; clips play in place.
* Smooth per-frame interpolation (**C55**, 2026-08-10) — supersedes the earlier stepped ~12fps stop-motion
  approach; `PawnView` now samples its path every frame instead of throttling pose updates.

---

## 6. Suggested project folders (implementation)

```
Assets/_Project/
  Net/          # Fusion callbacks, RPCs, TimelinePayload, ReplayTape
  Sim/          # Host ghost sim, Bresenham LoS, stance math (pure C# preferred)
  Timeline/     # Payload compile from UI
  UI/           # Program / Lock / Simulating / Scrubber playback
  Board/        # Grid, doors, vent, monitor views
  Characters/   # Scout/Heavy attrs ScriptableObjects
  Cards/        # CardData ScriptableObjects
```

Pure C# under `Sim/` must be unit-testable without a running scene.

---

## 7. Security / anti-cheat baseline

* Client may suggest path; Host **revalidates** Speed × Stance × budget before accepting payload.
* Illegal nodes / teleports rejected; Host may substitute empty Wait or force Otherwise Stop.
* Never apply damage/wounds from client RPCs — only from Host tape events.
* This baseline is **necessary but incomplete** once Host can be a real, potentially-adversarial player under monetized PvP — full host-integrity / anti-cheat answer lives in [NETWORKING_DESIGN.md](NETWORKING_DESIGN.md).
