# Brief: Bomber HUD — Board-Anchored Prompt + Mode (C36)

**Where / why:** Work in `D:\projects\Game\logiCard-modal-restyle` (branch `feat/modal-restyle`, idle at
`e1c80fb`, merged/reusable) or a fresh worktree off current `master` — your call, but base off current
`master` tip, not a stale branch. Integrator stays on `master` doing Sim/Net/Playback work only; UI owns
`ProgramHud.cs` per `docs/departments/INDEX.md`'s ownership matrix. No file overlap.

**Mandatory reading before writing any code:** `docs/ui/UI_BOARD_ANCHORED_COMPONENTS.md` — every control
tied to a board object (this is exactly that: a Bomber prompt tied to a `BreachPoint`) must show identity,
live state (read from `ArenaBoard`, never inferred from player input), and options as separate labeled
explicit-confirm controls. This project has shipped bugs from skipping each leg of this once already.
Also skim `docs/core/PLAYBACK_CONTRACT.md` if your prompt reads any tape-derived state during Playback
(it shouldn't need to — Program-time drafting only, same as the Door prompt — but confirm before assuming).

**Reference implementation — copy this shape, don't re-derive it:** `Assets/_Project/UI/ProgramHud.cs`'s
`RefreshDoorPrompt` (~line 1243) is the exact pattern to mirror: world→screen→canvas-local projection via
`_input.BoardView.WorldFromPlanar` → `Camera.main.WorldToScreenPoint` →
`RectTransformUtility.ScreenPointToLocalPointInRectangle`, a small offset so the cluster doesn't sit on
top of the pawn/geometry, buttons that highlight the action that *would change* state (not the one
matching current state — a past playtest bug), and a `DoorKind.Breach`-style one-way-permanent pattern
you should also apply here (once Detonated/Breached, don't offer Attach/Detonate again on that point —
same reasoning as `hideClose` at line 1300, `pending.Kind == DoorKind.Breach && !closed`).

**The job:**

1. A `BOMBER` mode button alongside the existing Move/Shoot/Door mode buttons (find where those are
   wired — likely near wherever `SetMode`/`ActionVerb` mode switching lives in `ProgramHud.cs`).
2. A board-anchored ATTACH/DETONATE prompt cluster for the selected `BreachPoint`, mirroring
   `RefreshDoorPrompt`: shows target name, live `BreachState`/`HasAttachedBomb` (read from `ArenaBoard`,
   not cached), ATTACH offered while `!HasAttachedBomb`, DETONATE offered once attached and not yet
   Breached, both hidden once `BreachState == Breached`.
3. Costs: `BombAttachSeconds = 3f`, `BombDetonateSeconds = 1f` (C71 strawman numerics, already frozen in
   `docs/contracts/CURRENT.md`'s C36 section — same shape as the Door OPEN/CLOSE cost labels).
4. Scrubber markers: check how Door open/close events show up on the `TimelineSchedule` band (if at all)
   and mirror that for `BombAttached`/`GeometryBreached` — if Door doesn't have scrubber markers either,
   skip this rather than inventing new chrome unprompted; note what you found either way.
5. **Character-gating is explicitly NOT yours this brief** — per `docs/contracts/CURRENT.md`, no archetype
   gate exists yet for who can queue Bomber actions; that's a separate, not-yet-scoped item. Build the
   prompt to work for any pawn for now, same as the Sim layer currently allows.

No map has an authored `BreachPoint` yet — build/test your prompt against a scratch `BreachPoint`
registered directly on a test board (same pattern the PlayMode tests in `RoundPlaybackPlayModeTests.cs`
use), not a real map. Don't add `RegisterBreachPoint` calls to `GameBootstrap`.

**Tests:** Unity batchmode EditMode + PlayMode, Editor **closed** on your worktree's own path. A new
PlayMode test exercising prompt visibility/labels against a live `BreachPoint` selection is expected, same
class as existing Door-prompt PlayMode coverage if any exists (check).

**Boundary — do not touch:** `Assets/_Project/Boot/RoundPlayback.cs`, `Assets/_Project/Net/*`,
`Assets/_Project/Sim/*`, `Assets/_Project/Board/BoardView.cs` (visuals are a separate brief,
`docs/map/BREACH_VISUALS_AGENT_BRIEF.md`), `GameBootstrap.cs`/`MapDefinitions.cs` (map authoring parked).

**Report back:** files touched, batchmode results including your new test(s) confirmed passed by name,
a description or screenshot of the prompt if you can render one, and anything that looks like it needs a
human design call rather than a guess. Commit on your branch only — no push, no merge.
