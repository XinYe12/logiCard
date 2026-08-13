# Map — STATUS

**Wave / Day:** New permanent seat, opened 2026-08-13 — live `logiCard-map`
**Branch / worktree:** `dept/map` @ `D:\projects\Game\logiCard-map`
**Last cross-reviewed:** 2026-08-13 — seat opened by Integrator, mandate below

## Mandate (2026-08-13)

The human's own framing: *"the current architecture and floor and texture is a mess."* Job in two
phases, doc first:

1. **Standardization doc** — how map/room/floor construction should actually work to fit the game's
   philosophy (GDD) and visual target, before touching more assets ad hoc.
2. **First real implementation job** — rebuild the map(s) using existing imported asset packs to hit a
   **vibrant, cute, Link's Awakening feel** — explicitly *not* the current dark/grey/desaturated look.

## Owned files (this seat)

- New standardization doc (name TBD by this seat, e.g. `docs/MAP_STANDARDS.md`)
- `Assets/_Project/Board/BoardView.cs` (mesh/prop/material assembly), `BoardSurfaceMaterials.cs`,
  `BoardReflectionProbes.cs`
- Floor/prop/material choices within `Assets/_Project/Art/Environment/**`
- This STATUS

## Must not touch (read, don't rewrite)

- `MapDefinitions.cs` / `MapLayout.cs` / `MapId` — Sim-layer room-bound source of truth (**C57**),
  Integrator-owned
- Door/Vent/Breach placement logic, `GhostResolver`, any `Sim/`/`Net/`/`Timeline/` code — standing
  pause applies, no carve-out for Map yet
- `BoardWeatherPocket.cs` / `Resources/Weather/**` — Atmosphere's lane
- `Assets/_Project/UI/**` — UI's lane

## Known conflict — read before starting

Main tree currently has **uncommitted, unverified** "urban floor" work touching these same files
(`BoardSurfaceMaterials.cs`, `BoardReflectionProbes.cs`, `BoardView.cs`) sitting dirty on
`D:\projects\Game\logiCard`. It is not merged, not batchmode-green, and per the human's own assessment
is part of "the mess." Do not assume it represents a direction to build on — start from `master` as
committed and treat that dirty work as informational at most, not a baseline.

## Done

- Nothing yet — seat just opened.

## In progress

- Nothing yet.

## Blocked

- Nothing yet.

## Offers

- N/A — new seat.
