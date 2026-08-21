# Board-Anchored Interactive UI Components

**Status:** Drafted 2026-08-06, first playtest cycle of the continuous-space slice.
**Depends on:** [UI_FLOW.md](UI_FLOW.md) (high-level UX flow this implements), `ProgramHud.cs` / `BoardInputController.cs` / `BoardView.cs`.
**Scope:** any future UI element that must appear tied to a *world/board position* — not a fixed row in the HUD dock — so the player interacts with it where the thing it controls actually is. The door OPEN/CLOSE button is the first and reference example.

## Why this exists

Playtest feedback (2026-08-06) on the door interaction flow went through three iterations before landing:
1. A fixed OPEN/CLOSE row in the HUD dock, gated on selecting the door first — functionally correct, but disconnected from *where* the door was, and read as "ambiguous."
2. A floating panel meant to appear at the door — shipped with an anchor/pivot bug that put it near the bottom of the screen instead. See the pitfall below; don't repeat it.
3. A small button cluster correctly anchored beside the door — the shape this doc describes.

Any new context-specific control (a pickup prompt, a hazard warning, a future "vault here" button, etc.) should follow this pattern rather than reinventing it or falling back to a HUD-dock row that's disconnected from the board.

## When to use a board-anchored component vs. a HUD-dock row

| Use a board-anchored component when... | Use a HUD-dock row when... |
|---|---|
| The action targets one specific, currently-visible board object (a door, a pickup, an interactable) | The action is a mode/verb the player picks in the abstract (Move, Shoot, stance) before touching the board |
| The player just selected that object | The control needs to always be reachable regardless of what's currently selected |
| Losing the visual link to *what* you're acting on would be confusing | Screen space in the board viewport is too tight for the control (board is the majority of a landscape frame; HUD docks occupy the margins — see `UI_FLOW.md`) |

Doors are the first case. Move/Shoot mode buttons, the stance band, and Lock In stay in the HUD dock — they're verb pickers, not object-targeted actions.

## Content contract — every interaction prompt needs three things

This is the part that's easy to skip when you're focused on getting a button to render in the right place, so it's called out on its own, separate from the positioning mechanics below. **Any UI that lets the player change the state of a board object — a door, a power station, a future terminal or valve — must show all three, every time, with no exceptions:**

1. **Identity — what you're acting on.**
   The object's player-facing name, e.g. `"Door #1"`, `"Power Station #3"`. Needed the moment there's more than one of a kind on the board, and cheap to carry from day one even when the demo only ever spawns one — retrofitting identity after code/UI has assumed "the door" (singular) is more expensive than an unused field now. Source it from the Sim-layer model object itself (e.g. `Door.DisplayName`), never invent it at the UI layer, so it can't drift from what the resolver/tape actually reasons about.

2. **Current state — authoritative for what the player is looking at, never inferred from a button preference.**
   Show the object's state every time the prompt refreshes. **Do not remember "the player last pressed OPEN"** as state — that exact class of bug shipped once already (2026-08-06: a HUD-preferred action silently diverged from reality and a confirm could book the opposite of what was displayed).

   For doors during **Program**, the authoritative read is **scheduled state**: round-start live `ArenaBoard.GetDoorState`, then each Door toggle already booked on this pawn's `PawnProgram` for that door, in order (`PawnProgram.ScheduledDoorState`). That is what OPEN/CLOSE just changed, and what the prompt must show. Move drafting pathfinds on a **clone** with those scheduled door states so "open then walk through" works in the same draft; the shared live board stays at round-start until Aftermath (Lock In arm / resolve still need the true start state). Raw live-only `GetDoorState` in the prompt made OPEN look like a no-op (playtest 2026-08-07).

   After playback/Aftermath, live board and scheduled state agree again.

3. **Options — the concrete actions available, each an explicit confirm.**
   Every option is its own labeled, separately-tappable control (`OPEN`, `CLOSE`, ...) with its cost/consequence visible on the control itself (e.g. `"OPEN 4s"`), and pressing it must be the *only* thing that commits the action. No control may silently substitute a different action than the one it's labeled as (the auto-flip-to-opposite bug, same date). No control may auto-fire from a state change alone (selecting the object must never itself book an action) — the player's tap on a specific, labeled option is the one and only trigger.

**Worked example — the door:**

| Contract leg | Door's answer | Source |
|---|---|---|
| Identity | `"Door #1"` | `Door.DisplayName` |
| State | `"CLOSED"` / `"OPEN"` | `PawnProgram.ScheduledDoorState(door)` during Program (live ⊕ booked toggles); live board after Aftermath |
| Options | `OPEN 4s`, `CLOSE 4s` | `PawnProgram.DoorInteractSeconds` cost, `BoardInputController.TryConfirmPendingDoor(DoorAction, ...)` per option |

**Second worked example — the Bomber breach point (C36/C71, added 2026-08-21):**

| Contract leg | Breach point's answer | Source |
|---|---|---|
| Identity | `"Breach Point #1"` | `BreachPoint.DisplayName` |
| State | `"INTACT"` / `"BREACHED"` (+ `"BOMB SET"`) | `PawnProgram.ScheduledBreachState` / `ScheduledHasAttachedBomb` during Program; live board after Aftermath |
| Options | `ATTACH 3s`, `DETONATE 1s` | `PawnProgram.BombAttachSeconds`/`BombDetonateSeconds`, `BoardInputController.TryConfirmPendingBombAttach`/`TryConfirmPendingBombDetonate` |

Two things this second case adds to the pattern, both worth copying:

- **Scheduled state, not live state, is what the prompt shows — for *every* interactable, not just
  doors.** Attach/Detonate had the same latent trap Open/Close hit in 2026-08-07: right after booking
  ATTACH, the live `ArenaBoard.HasAttachedBomb` still reads false (the resolver, not the draft, is what
  mutates the board), so a live-only read keeps offering ATTACH and lets the player book — and pay
  for — the same action twice. `PawnProgram.ScheduledBreachState`/`ScheduledHasAttachedBomb` mirror
  `ScheduledDoorState` exactly. If you add a new interactable, add its scheduled projection at the same
  time as its verb, not after a playtest finds the prompt looking dead.
- **The options are mutually exclusive, so exactly one is ever shown.** ATTACH only while unarmed,
  DETONATE only while armed, neither once `Breached` — the same one-way-permanent idea `DoorKind.Breach`
  uses (`hideClose`), and stronger, since a blown wall can never be un-blown at all. "Highlight the
  action that would change state" is trivially satisfied when only that action is on screen.

**Sizing gotcha — cost text on a small button truncates silently.** `UiFactory.CreateButton` uses
`UiTextOverflow.Button` (wrap, then *truncate vertically*). In a 42px-tall prompt button, a two-line
`"ATTACH\n3s"` renders as `ATTACH` alone: the cost leg of the contract is in the string, passes a
`Does.Contain("3s")` assertion, and never reaches the player's eyes. Put the cost on the same line
(`"ATTACH  3s"`) and size the cluster to fit it, or make the button tall enough for two lines. Caught
only by rendering a capture and looking at it — no batchmode assertion would have found it.

**Minimum shape in code**, if you're modeling a new interactable — doesn't have to be this exact struct, but every new board-anchored prompt's data should be expressible as it:

```csharp
readonly struct InteractionPromptContent
{
    string TargetName;                       // "Door #1" — leg 1
    string StateLabel;                       // "CLOSED" — leg 2
    IReadOnlyList<(string Label, float Cost)> Options; // [("OPEN", 4f), ("CLOSE", 4f)] — leg 3
}
```

## The conversion pipeline

```
BoardView.WorldFromPlanar(planarPosition)   // Sim-space point -> Unity world space
  -> Camera.main.WorldToScreenPoint(worldPoint)   // world space -> full-screen pixels
  -> RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, screenPoint, null, out local)
       // screen pixels -> local space of the canvas RectTransform you're parented under
```

Notes specific to this project's setup:
- The game camera (`GameBootstrap.ConfigureCamera`) uses a **restricted viewport rect**, not the full screen (it leaves room for the top bar and HUD dock margins). `Camera.WorldToScreenPoint` already accounts for this and returns coordinates usable directly as full-screen pixels — no manual adjustment needed.
- The HUD canvas is `RenderMode.ScreenSpaceOverlay` (`ProgramHud.BuildCanvas`). For overlay canvases, the `camera` argument to `ScreenPointToLocalPointInRectangle` must be `null` — passing `Camera.main` there is for Screen Space - Camera or World Space canvases and will silently give the wrong answer for this project's canvas.
- The board and camera are both **static** — recompute the projection only when the underlying selection changes (e.g. a new door gets picked), never every frame. `Update()`-driven tracking is unnecessary cost and unnecessary risk here.

## The pitfall that shipped once — anchor vs. pivot

`ScreenPointToLocalPointInRectangle`'s output (`local`) is measured **relative to the target RectTransform's own pivot**, not any particular corner. For a normal full-stretch canvas rect (pivot `(0.5, 0.5)`), `local = (0, 0)` means *screen center*.

Your floating element has two *separate* settings that are easy to conflate:
- **`anchorMin`/`anchorMax`** — where on the **parent** the element's anchor reference point sits.
- **`pivot`** — where **within the element's own rect** that anchor reference point sits.

The bug: the first version anchored the element to the parent's *bottom-center* (`anchorMin = anchorMax = (0.5, 0)`) while feeding it `local`, which is measured from the parent's *center*. Two different origins, silently added together — the element rendered near the bottom of the screen instead of at the door.

**The fix, and the rule going forward:**
- Set `anchorMin = anchorMax = (0.5, 0.5)` (the parent's center reference point) so the coordinate space matches what `ScreenPointToLocalPointInRectangle` returns. Then `anchoredPosition = local` (plus a small fixed offset if you want a gap) is correct with no extra math.
- Use the element's own **`pivot`** to control *which part of the element* sits at that point, independent of the anchor. E.g. `pivot = (0, 0.5)` (left-center) makes the element extend to the right of the anchor point — i.e. render "beside" the target rather than centered on top of it or floating above it.

## Sizing and placement

- These are small, single-purpose controls — a button or a tight cluster of 2–3 buttons, not a panel with prose. The board viewport is the majority of a landscape screen (`UI_FLOW.md`); anything bigger still crowds out the thing it's supposed to be next to.
- Prefer **beside** the target (a small horizontal offset, e.g. ~15–20px gap before the element starts) over **above** it — above tends to run off the top of the board viewport or overlap other board geometry; beside stays within the board's own horizontal footprint on this project's layouts.
- Still respect standard desktop click-target sizing from `UI_FLOW.md`'s interaction rules even though the element is compact.

## Lifecycle — enumerate every hide case explicitly

A board-anchored element does **not** automatically inherit visibility from the HUD-dock panel it's conceptually related to, because it's parented directly under the canvas root, not under that panel. The first version of the door prompt stayed stuck on screen after switching modes and after the round moved past Program, because only one of its hide paths was wired. Hide it explicitly on **all** of:

1. **Selection cleared** — the thing it targets is no longer selected (refresh call already passes `null`/no-target and the refresh function hides it).
2. **Mode/verb switched away** from the one that owns this element (e.g. leaving Door mode) — don't rely on the generic per-mode panel refresh catching this; add an explicit hide alongside it.
3. **Phase leaves the state it's valid in** (e.g. leaving `RoundPhase.Program`) — a control left visible during Reveal/Execute/Aftermath will float over playback with stale, now-locked actions behind it.

## Naming and testability

Keep the same GameObject/Button names across a relocation if PlayMode tests already look them up by name (`FindByName<Button>("Door_Open")` etc.). Tests that only care a control exists and is clickable via `.onClick.Invoke()` don't care *where* it's parented or how it's positioned — moving a control's location shouldn't require rewriting the tests that exercise its behavior.

## Reference implementation

`ProgramHud.BuildDoorPrompt` (construction) and `ProgramHud.RefreshDoorPrompt` (projection + show/hide) are the worked example — copy their shape for the next board-anchored control rather than re-deriving the pipeline above from scratch. `ProgramHud.BuildBombPrompt`/`RefreshBombPrompt` (2026-08-21) is that copy, done once already: if you want to see what "mirror the door prompt" actually looks like as a diff, read those two against their door counterparts.

`BomberPromptScreenshotTests` is the rendered-capture harness for this class of control (opt-in via `LOGICARD_SHOT_DIR`, must run **without** `-nographics`). It is how the truncation gotcha above was found; run it, or something like it, before calling a new prompt done. One caveat it documents in its own comments: an Overlay canvas can't be read back into a texture, so the harness flips the canvas to ScreenSpaceCamera, and under that mode the shipped `null`-camera projection lands the cluster off-frame — the harness re-anchors it arithmetically for the capture only. That correction is a property of the capture rig, not a bug in the prompt.

## Checklist for the next board-anchored interaction control

Copy this into the PR/change description (or just walk it before calling the work done) for any new prompt of this kind:

- [ ] **Identity** shown, sourced from the Sim-layer model object (not invented at the UI layer).
- [ ] **State** shown, read live from the authoritative model every refresh — never inferred/remembered from player input.
- [ ] **Every option** is its own labeled control with cost/consequence visible, and pressing it is the only way it fires (no auto-trigger on selection, no silent substitution).
- [ ] Anchored `(0.5, 0.5)` on its parent, positioned via `anchoredPosition = local (+ offset)`, pivoted to sit **beside**, not on top of or floating disconnected from, the target.
- [ ] Hidden on: selection cleared, mode/verb switched away, phase leaves the valid state — all three, explicitly.
- [ ] Compact — a button or small cluster, not a panel with prose.
- [ ] Existing GameObject/Button names preserved if a test already depends on them.

## Enforcement — how this doc stays followed, not just written

A doc nobody reads before writing code doesn't do anything. This project's `CLAUDE.md` (repo root) points every agent session at this file specifically for board-anchored/interaction-state UI work, so it loads as project context before that kind of change starts, not after a review catches it. If `CLAUDE.md` ever stops mentioning this doc, or a new interaction control ships without the checklist above being checked, that's the process failing — fix the pointer, don't just fix the one instance.
