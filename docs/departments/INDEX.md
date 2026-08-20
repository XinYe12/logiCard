# Departments — Active Index

**Updated:** 2026-08-19 — **Dispatch round closed, all seats idle.** Since the 2026-08-17 Match
Shell/Map/Camera merge: the full 2026-08-17 backlog (Healed presenter, Storm per-match counter, dead
Bandage/Storm board-tap prune, IMGUI control-hint → real UI chrome) landed 2026-08-18 and was human
Play-approved; a restaffing pass the same day closed Cards' docs rebase and merged Character's Select
carousel (now owned by UI dept). See merge gates 6-10 below for detail. Only Atmosphere's Sunny mode
remains open, blocked on a human decision. Plan: [`../ui/MATCH_SHELL_LAYOUT.md`](../ui/MATCH_SHELL_LAYOUT.md).

**Ops constitution:** [`../PARALLEL_OPS.md`](../PARALLEL_OPS.md) · human-side playbook: [`../DIRECTING_AGENTS.md`](../DIRECTING_AGENTS.md)
**Contracts:** [`../contracts/CURRENT.md`](../contracts/CURRENT.md) · Playback: [`../core/PLAYBACK_CONTRACT.md`](../core/PLAYBACK_CONTRACT.md)
**Human rollup:** [`../DRAFT_HANDOFF.md`](../DRAFT_HANDOFF.md)

## Capacity

**Permanent seats:** 5 + Integrator. **Coding-hot:** none — dispatch round closed, all seats idle unless restaffed.

## Live folders ↔ seats

| Seat | Canonical | **Live folder now** | Tip / state | STATUS |
|------|-----------|---------------------|-------------|--------|
| **Integrator** | `logiCard` | `D:\projects\Game\logiCard` | `master` @ `bb6fcdf` — everything through the 2026-08-18 restaffing pass (Cards rebase + Character carousel) merged | [`core/STATUS.md`](core/STATUS.md) |
| **Atmosphere** | `logiCard-atmosphere` | `logiCard-atmosphere-stylized` | **Sunny weather mood merged to master** (`0857b80`, 2026-08-20); worktree idle | [`atmosphere/STATUS.md`](atmosphere/STATUS.md) |
| **UI (restyle)** | — | — | **Merged to master** (`546ba31`, 2026-08-20), human-approved as-is; worktree removed | [`ui/STATUS.md`](ui/STATUS.md) |
| **Cards** | `logiCard-cards` | `logiCard-cards-collection` | Rebased + fully reconciled onto master (`47baf50`); idle | [`cards/STATUS.md`](cards/STATUS.md) |
| **Character** | `logiCard-character` | `logiCard-char-select-motion` | Carousel feature rebased + merged to master (`9472783`); **UI dept now owns this code going forward**, not Character; idle | [`character/STATUS.md`](character/STATUS.md) |
| **UI** | `logiCard-ui` | `logiCard-modal-restyle` | Fully merged to master (`e1c80fb`); idle | [`ui/STATUS.md`](ui/STATUS.md) |
| **Map** | `logiCard-map` | `D:\projects\Game\logiCard-map` | **Fully merged to master** (`07501d7`); idle | [`map/STATUS.md`](map/STATUS.md) |
| **Camera** | — | `logiCard-camera-control` | `2e2d022` — **fully merged to master** (`e594c51`); idle | — |

## Ownership matrix (write locks)

| Path / concern | Owner now |
|----------------|-----------|
| `docs/ui/MATCH_SHELL_LAYOUT.md`, `docs/contracts/CURRENT.md`, `DRAFT_HANDOFF`, INDEX | **Integrator** |
| `ProgramHud` / `GearHandView` / match shell bands / `UiStyle` dock tokens | **Integrator** (merged; UI idle unless restaffed) |
| `Assets/_Project/Board/BoardSurfaceMaterials.cs`, `BoardView.cs` fence code | **Integrator** (Map merged; idle unless restaffed) |
| `BoardCameraRig` / `GameBootstrap.ConfigureCamera` | **Integrator** (Camera merged; idle unless restaffed) |
| Sim/Net/Timeline resolve | **Integrator** — frozen; UI calls only |

## Integrator merge gates

1. ~~Storm / Bandage HUD / hand-deck~~ — merged.
2. ~~HUD Chrome alone~~ — absorbed by Match Shell.
3. ~~Match Shell Layout (UI)~~ — **merged `c9925b1`**, human Play-signed, docs peers folded in `a21b29c`, batchmode green (174/174, 56/56).
4. ~~Map fence-shadow + material tweaks~~ — **merged `07501d7`**, human-approved, batchmode green (174/174, 56/56).
5. ~~Camera~~ — **merged `e594c51`** (`--no-ff`), human-tested and hands-on iterated live during re-test (combined pan+rotate, then pitch-tilt right-drag), batchmode green on master post-merge (188/188, 59/59).
6. ~~Healed presenter~~ — **landed on `master` directly (2026-08-18, no dispatch needed)** — one-shot banner only (`RoundPlayback.Report`); no board-splat leg, since `Healed` can only ever clear a wound carried in from a prior round, and this round's own wound splats are built only from this round's own Wounded/Killed events (see `PLAYBACK_CONTRACT.md` §3). Batchmode green: EditMode 188/188, PlayMode 60/60.
7. ~~Storm per-match counter~~ — **landed on `master` directly (2026-08-18, no dispatch needed)** — `RoundPlayback.StormCastCountOf` + `GhostResolver`-side enforcement, mirroring Bandage's `BandageChargeOf`/charge-gate shape exactly; `RegisterMatchState` grew a third delegate. Closes the deviation flagged in C69/`contracts/CURRENT.md`'s Storm contract.
8. ~~Dead Bandage/Storm board-tap paths~~ — **pruned on `master` directly (2026-08-18)** — `BoardInputController.TryTapPoint`'s two unreachable branches + the `ResolveBandageExecuteTime` helper they alone called, removed. Batchmode counts unchanged (190/190, 61/61), confirming it was dead code.
9. ~~IMGUI control-hint → real UI chrome~~ — **landed on `master` directly (2026-08-18, last backlog item)** — `BoardCameraRig.OnGUI()` removed; `ProgramHud` now owns a live `CameraControlHint` label (consolidated with a static duplicate label that already existed) driven by new `BoardCameraRig.ControlHintText` via `ProgramHud.RegisterCameraRig`. Batchmode green: EditMode 190/190, PlayMode 62/62. **Human Play-approved 2026-08-18** — closed, no further verification owed.
10. ~~Cards docs rebase + Character Select carousel~~ — **landed on `master` directly (2026-08-18 restaffing pass)** — Cards' branch reconciled with zero real-content diff (`47baf50`); Character's 2-item carousel (`CharacterSelectView.cs`/`UiMotion.cs`, Kenney chrome) merged as-is (`9472783`) — **UI dept now owns this code**, not Character. Batchmode green: EditMode 190/190, PlayMode 63/63.
11. ~~Atmosphere Sunny decision~~ — **merged to master directly (2026-08-20, `0857b80`)** — human called it "ok to merge" after a Play look; boot default deliberately stays Fair (C67), Sunny reachable via `ApplyWeather`/`ToggleSunnyStorm`. Two real git-merge-corruption bugs found and fixed by hand-verifying against both branches' clean versions rather than trusting the automatic merge — see `DRAFT_HANDOFF.md`'s 2026-08-20 note. Batchmode: EditMode 190/190, PlayMode 65/65.
12. ~~UI shell-chrome restyle~~ — **merged to master (`546ba31`, 2026-08-20)**, human-approved as-is. Includes a follow-up live 3D character-model preview in Character Select cards. Batchmode: EditMode 190/190, PlayMode 66/66. Exposed two board-art bugs (Scout skin mistint, Juggernaut bunny ears) logged in `character/STATUS.md`, not fixed — human said log and move on.
13. `ArchetypeOf(pawnId)` InfoBar reader flagged 2026-08-19, still unwired — investigated and found entangled with C73's larger, not-yet-authorized "Character Select → live attrs wiring" contract (`GameBootstrap` hardcodes both pawns' archetype today, ignoring `SelectedArchetype` entirely) and with an unconfirmed InfoBar two-column layout recommendation (Character STATUS's own "Waiting on human" item 5) — not started, see `DRAFT_HANDOFF.md`'s matching note for the full reasoning.
14. **C36 geometry-breach + Bomber wall-only verb — Sim layer landed on master (2026-08-20), human-directed ("character, GO")**, per C71's already-locked scope. `BreachPoint`/`BreachState`, `ActionVerb.BombAttach`/`BombDetonate`, `GhostResolver` chronological integration — all tested, `Resolve()` verified to stay pure. Deliberately deferred: RoundPlayback presenter, BoardView visuals, map-authored breach points, HUD, Character-gating — see `docs/contracts/CURRENT.md`'s open C36/Bomber section for the full frozen-signature list and why. Batchmode: EditMode 196/196, PlayMode 66/66.
15. ~~Atmosphere storm-rolling-in transition~~ — **merged to master (`ecf0093`, 2026-08-20)**, picked up from `STORM_TRANSITION_AGENT_BRIEF.md` in a correctly-fresh worktree off current master. Fair/Storm modules now slide in over 1.1s instead of popping instantly. Batchmode: EditMode 196/196, PlayMode 67/67. This dispatch round is closed — no coding-hot seats remain.
