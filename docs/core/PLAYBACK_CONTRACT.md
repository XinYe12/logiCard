# Playback / Execution Contract

**Status:** Active 2026-08-12. Integrator-owned.  
**Product truth:** `GDD.md`, `CORE_LOOP.md`, `UI_FLOW.md`, `PRODUCT_MEMORY.md` (C23/C27/C28/C33).  
**Code owner:** `RoundPlayback` (`Assets/_Project/Boot/RoundPlayback.cs`) + presenters it drives.

This file locks how the locked round is **shown** and which **stage interactions** are legal during that cinema. Read it before adding a Program verb, a `TapeEventType`, or a mid-Playback control.

---

## 1. Reveal ≠ Execution (names)

| Phase (GDD) | Code today | What it is | Board interaction |
|-------------|------------|------------|-------------------|
| **Reveal** | Short flash via `AppFlowController` around Lock In; phase button `RoundPhase.Reveal` | Paths/schedule face-up, &lt;~2 real-world seconds (`UI_FLOW` §6) | None |
| **Execution / Playback** | `RoundPhase.Execute` | Cinema of the immutable `ReplayTape` on the Time Resource scrubber; Playback Duration may compress (`C27`) | **Ship:** Adrenaline 1/match while an active segment plays (`UI_FLOW` §7). **Roadmap:** more mid-cinema tools later |
| **Aftermath** | `RoundPhase.Aftermath` | Carry positions / doors / wounds; next Allot or Match Over | Continue / Next Round |

Colloquial “reveal stage” in playtest talk usually means **Execution/Playback**. Prefer GDD names in docs and commits.

---

## 2. Architecture that must stay true

```
Program payloads → Lock In → GhostResolver / IMatchResolver
  → ReplayTape (tracks + events, Time Resource seconds)
  → TimeResourceClockDriver.TimeChanged
  → RoundPlayback.ApplyTime(seconds)
       ├─ continuous: PawnView, doors, tracers, muzzle, wound splats
       ├─ one-shot forward: foley + outcome banners (_eventCursor)
       └─ stage gate: Adrenaline (Execute + active segment only)
```

Rules:

1. **Resolve once.** After arm, the tape for the round is immutable. Presentation never invents outcomes the resolver did not emit.
2. **Scrubber second is truth.** Continuous presentation is a pure function of `(tape, armSnapshot, seconds)` — rewind-safe.
3. **One-shots are forward-only.** Foley / banners advance with `_eventCursor`; rewind clears the banner and does not re-fire until crossed again forward.
4. **No per-tick restart.** If a presenter uses a timed animation (door hinge, future FX), same tape-derived state must **not** restart that animation every `ApplyTime` tick. Transition only when the derived state changes. (Playtest 2026-08-12 door bug class.)
5. **Ship mid-Playback interaction ≠ re-resolve.** Adrenaline today is UI gate + stub effect. Any future interaction that changes outcomes mid-cinema needs an **explicit redesign** (e.g. tape branch / second resolve) — do not silently mutate the armed tape.

---

## 3. Ship verbs — presentation matrix

| Source | Must show at TR second | Rewind | Presenter |
|--------|------------------------|--------|-----------|
| `ScheduledPath` | Pawn pose | Yes | `PawnView.ApplyTime` |
| `DoorOpened` / `DoorClosed` | Board door state + hinge start | Yes | `SyncDoorsToSeconds` → `BoardView.RefreshDoorVisuals` |
| `ShootFire` | Tracer (window) + muzzle + Shot foley | VFX yes / foley one-shot | `UpdateTracers` / `UpdateHitVfx` / `Report` |
| `Wounded` / `Killed` | Wound splat + banner | Splat yes / banner one-shot | `UpdateHitVfx` / `Report` |
| `MoveArrive` | Footstep foley | One-shot forward | `Report` |
| `Healed` | Banner only | One-shot forward | `Report` |
| `Invalid` | Reserved (Otherwise Stop — post-demo) | — | None yet |

Vent/Breach are `DoorKind`s on the same Door tape path — not a separate Playback system.

**`Healed` has no splat leg by design, not oversight (2026-08-18):** `GhostResolver.CompileTrack`
resolves a Bandage node's heal from `GhostInput.StartingWounds` (carried in from a prior round) before
this round's own `ResolveShots` pass ever applies a hit — a `Healed` event can therefore only ever clear
a wound that predates this round's own `BuildHitVfx`, which only ever splats *this* round's own
`Wounded`/`Killed` events. There is never a same-round splat for it to hide/restore. If wound splats
are ever made to persist across rounds, revisit this row.

---

## 4. Mid-Playback interactions

**Ship (UI only today):**

- **Adrenaline** — `ProgramHud`, Execute only, once per match, only while scrubber is inside an active booked segment. Effect resolve is **stub** (`AdrenalineUsed` event). Do not expand sim here unless `SCHEDULE.md` / PRODUCT_MEMORY say so.

**Roadmap (same contract when they land):**

- Bandage, Flashbang, Interact-as-card, Adrenaline resolve, dedicated vent tooling, 高铁, etc.
- Program-time verbs → resolver emits tape event(s) → `ApplyTime` presents at `event.Seconds`.
- Cinema-time verbs → gate on Execute + segment (or redesigned authority); never desync the armed tape by accident.

---

## 5. Extension checklist (new verb or TapeEventType)

Before merging:

1. [ ] GDD / CORE_LOOP / PRODUCT_MEMORY row exists (or explicitly deferred).
2. [ ] `GhostResolver` (or match resolver) emits the tape event(s) with correct TR `Seconds` (and `WindowStartSeconds` if a hold window).
3. [ ] `RoundPlayback.ApplyTime` path presents it:
   - continuous state → derive from `seconds` (build-once at arm + `SetVisible` / sync), **or**
   - one-shot → `Report` on forward cursor only.
4. [ ] No coroutine/FX restart on same-state refresh (door-bug class).
5. [ ] PlayMode: scrub to `event.Seconds - ε` (absent) and `event.Seconds` (present); rewind clears continuous state.
6. [ ] If `TapeEventType` gains a value, update the presenter matrix above and the enum coverage test in `RoundPlaybackPlayModeTests`.
7. [ ] Mid-Playback control? Document gate + whether it is stub or authority-changing.

---

## 6. Schedule guardrail

Phase 5 (art) remains top priority per `SCHEDULE.md`. This contract is **playback correctness** for ship verbs — not Phase 2 Net expansion, not inventing roadmap cards. Point future Playback work here; product cuts still go through human → PRODUCT_MEMORY.
