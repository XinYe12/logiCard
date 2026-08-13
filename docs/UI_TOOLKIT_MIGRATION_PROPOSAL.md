# UI Toolkit migration — proposal (fed by the Character Select pilot)

**Status:** Proposal, not a decision. Written 2026-08-13 on `feat/char-select-motion` after rebuilding
Character Select on Unity UI Toolkit (`UIDocument`/`VisualElement`) as a working pilot, at the human's
request for "an already mature game UI system we can apply to the entire game UI." This doc is the
Integrator-facing writeup that request needs before it can be scheduled — see `docs/PARALLEL_OPS.md`
("Departments escalate blockers to Integrator; Integrator escalates product decisions to human").
`docs/UI_TOOLS_RESEARCH.md` (2026-08-12) evaluated this same option ahead of time and recommended
against it for that wave ("do not rewrite `Assets/_Project/UI/**` onto UI Toolkit"); this doc supersedes
that verdict with real, not speculative, evidence — the pilot exists, compiles, and passes its PlayMode
coverage (`Assets/_Project/UI/CharacterSelectView.cs` on `feat/char-select-motion`).

## Bottom line

UI Toolkit is a legitimate "mature" choice — it's Unity's own actively-developed UI system, not a
third-party bolt-on, and it works for a hand-styled, code-built, no-UXML-asset screen exactly the way
`UiFactory`/`UiStyle` already work for uGUI. The pilot proves the *shell* (pre-match menus) can move.
It does **not** prove the *in-match HUD* should — `UI_TOOLS_RESEARCH.md`'s original caution about
`ProgramHud`'s camera-rect/board-anchored coupling stands, and nothing in this pilot touched that
surface. Recommended shape: **hybrid, shell-first**, not a rip-and-replace.

## What the pilot actually built

`CharacterSelectView.cs` was rebuilt end-to-end on `UIDocument`/`VisualElement`, replacing the uGUI
`RectTransform`/`Image`/`Text`/`Button`/`CanvasGroup` tree with an equivalent `VisualElement` tree,
hand-styled inline (no `.uxml`/`.uss` asset files — same "no external asset" posture `UiStyle.RoundSprite`
already uses, for the same batchmode-safety reason). Same behavior contract as before:

- 2-item center/flank carousel, ~650ms coordinated crossfade, driven by the *same*, completely
  unmodified `UiMotion.cs` coroutine helper — the tween layer is UI-technology-agnostic and needs no
  changes to work with either uGUI or Toolkit.
- `Pick_Scout` / `Pick_Juggernaut` hit targets, ghost archetype headline, per-archetype background
  tint, the halo glow polish landed earlier this session.
- `ConfirmCharacter` stays a uGUI `Button`, built by `AppFlowController.BuildCharacterSelect` exactly
  as before — this screen is deliberately **hybrid** (Toolkit carousel + uGUI sibling button), because
  that's the realistic rollout shape, not a green-field rebuild.

Verified via batchmode only (see Verification below) — **no interactive/visual Play pass has happened
on this pilot yet**, because this session had no open Editor on this worktree to drive one. That gap
matters more here than usual: batchmode assertions confirm the carousel is wired correctly, not that it
*renders* correctly (see the missing-theme risk below). Treat this as functionally proven, visually
unverified.

## Real findings (not speculation — hit these bugs, fixed them, kept the fix)

These are the concrete costs a full rollout needs to budget for, discovered by actually doing it:

1. **`UIDocument.rootVisualElement` is torn down and rebuilt empty on every `OnEnable`/`OnDisable`.**
   Confirmed Unity behavior, not a bug — Unity's own guidance is to either rebuild on every re-enable or
   avoid disabling the GameObject at all. `AppFlowController` hides every pre-match screen by
   `GameObject.SetActive(false)` (`Show()`'s `SetActive` calls) — the naive port (`UIDocument` directly
   on the screen's own GameObject) silently loses its whole tree the first time the player leaves and
   returns to a screen. **Cost for a full rollout:** every migrated screen needs the same workaround
   this pilot landed on (below), not a one-off.
2. **Parent cascades hit the same bug one level up.** The first fix (host the `UIDocument` on a sibling
   GameObject instead of the screen itself) still broke, because that sibling was parented under
   `AppFlowController`'s shared `_root` panel — which itself gets `SetActive(false)` whenever the whole
   pre-match shell hides (`Show(Screen.None)`, i.e. entering a match, and `BypassAppFlowForTests` in the
   test fixture). Unity's active-state cascades to children regardless of the child's own `activeSelf`.
   **Fix:** the `UIDocument` host must be parented under something a full rollout is *certain* never
   gets disabled — in this pilot, that's "nothing" (scene root) — which raises finding 3.
3. **A scene-root, nothing-ever-disables-it host also means nothing ever destroys it.** Orphaned
   `UIDocument` hosts from a previous `GameBootstrap` accumulate (two were live simultaneously across
   PlayMode `[SetUp]`/`[TearDown]` cycles before this was caught), and a name-based `VisualElement`
   query can silently match the *stale* one instead of the live one — the click lands, but on a dead
   instance, so nothing observable changes and the failure reads as "the click didn't do anything,"
   not an obvious ordering bug. **Fix:** the owning `MonoBehaviour` must explicitly `DestroyImmediate`
   its `UIDocument` host in `OnDestroy` — ownership can't be inferred from Transform parenting the way
   uGUI's Canvas-child screens get it for free.
4. **Cross-technology click simulation needs a different pattern than uGUI's tests use.**
   `Button.onClick.Invoke()` (uGUI, a public `UnityEvent`) has no Toolkit equivalent — `Button.clicked`
   is a C# event, not externally invocable, and `Clickable`'s internal gesture manipulator won't respond
   to a synthetic `ClickEvent` sent from outside. The pattern that works: register `ClickEvent` callbacks
   directly on plain `VisualElement`s (skip the built-in `Button` control) and dispatch synthetic events
   via `element.SendEvent(ClickEvent.GetPooled())` — this goes through the real event dispatcher, so it
   reaches any `RegisterCallback<ClickEvent>` handler. Landed as `SliceSceneFixture.ClickVisualElement` +
   `FindVisualElement<T>`, additive to the shared test fixture (didn't touch the existing uGUI
   `FindByName<T>`/`onClick.Invoke()` path other suites use).
5. **Missing theme stylesheet — a real, currently-unresolved gap.** Every batchmode run logs
   `No Theme Style Sheet set to PanelSettings , UI will not render properly`. The `PanelSettings` here is
   created at runtime (`ScriptableObject.CreateInstance`, no `.asset` file) to keep with this project's
   "no external asset, batchmode-safe" posture — but that means no default runtime theme is attached
   either. Functionally this didn't block anything (PlayMode assertions on state/wiring don't need
   pixels), but it's an open question whether text/controls render with correct fallback styling without
   one. **This is exactly the gap the "no visual Play pass yet" caveat above is about** — a full rollout
   needs either a checked-in default runtime theme asset or a confirmed-safe runtime-only substitute,
   verified by an actual human Play pass, before trusting this warning is cosmetic.
6. **Assembly references.** `UnityEngine.UIElementsModule` had to be added to both
   `Assets/_Project/UI/LogiCard.UI.asmdef` and `Assets/_Project/Tests/PlayMode/LogiCard.Tests.PlayMode.asmdef`
   — not automatic just because the built-in module is present in `Packages/manifest.json`
   (`com.unity.modules.ui`). Every asmdef touching Toolkit code in a full rollout needs this.
7. **Cross-technology draw order is a real tuning knob, not automatic.** `PanelSettings.sortingOrder`
   and `Canvas.sortingOrder` interleave by Unity design, but a migrated screen with a uGUI sibling still
   on the same visual layer (this pilot's `ConfirmCharacter` button) needs that order set deliberately
   (`sortingOrder = 10f` here) — worth a second look in the human Play pass, since it was chosen by
   reasoning about the layout, not verified by eye.
8. **Layout math ports mechanically, which is a genuine upside.** uGUI's `anchorMin`/`anchorMax`
   stretch-rect convention (`UiFactory.Stretch`) maps 1:1 onto USS `left`/`right`/`top`/`bottom`
   percentages (flip Y, since uGUI is Y-up and USS is Y-down) — `CharacterSelectView.StretchVe` is a
   direct port of `UiFactory.Stretch`'s two call sites, same arguments. Point-anchored, pivoted,
   scaled elements (the carousel cards, the glow rings) port via `left`/`bottom` percent + pixel
   `width`/`height` + a fixed pixel `marginLeft` (half-width, for horizontal centering) + `scale`/
   `transformOrigin` for the pivot-anchored grow/shrink — every layout number in the Toolkit version is
   copied verbatim from the uGUI version's constants, not re-tuned. **Net positive for a rollout:** this
   is mechanical, not a redesign, for any screen built the same "hand-anchored, no auto-layout" way
   `UiFactory` already builds things.
9. **Native rounded corners, no baked sprite.** `UiStyle.RoundSprite` exists specifically to work around
   uGUI having no native rounded-rect primitive (see that class's own doc comment on why it isn't
   `AssetDatabase.GetBuiltinExtraResource`). USS `border-*-radius` is native and needs no baked texture —
   a small but real simplification a rollout gets "for free" once a screen is on Toolkit.

## Recommended sequencing

Not a decision — a proposed order for the Integrator/human to confirm or amend:

1. **Verify this pilot visually first.** Human Play pass on `feat/char-select-motion` specifically
   checking: does text render (theme warning above), does `ConfirmCharacter` composite correctly over
   the carousel (sort-order tuning above), does the crossfade/glow still read the same as the uGUI
   version did. This is the cheapest possible next step and directly resolves the biggest open unknown.
2. **If the pilot holds up visually, migrate one more shell screen** (Map Select or Lobby — both
   simpler than Character Select, no crossfade) to confirm the workaround pattern (findings 1–4) holds
   for a second, independently-owned screen and isn't something that only happened to work here.
3. **Leave `ProgramHud` / in-match HUD / board-anchored prompts on uGUI.** Nothing in this pilot
   addressed the coupling `UI_TOOLS_RESEARCH.md` already flagged (camera-rect contract, door prompt
   raycasting, `HudDockHeight`/`TopStripHeight`). That's a separate, harder proposal if it's ever
   revisited — don't fold it into "the UI Toolkit migration" as if it's the same-sized problem.
4. **`ModalDialog` needs its own sign-off**, not a unilateral touch from whichever screen gets to it
   first — it's owned by the sibling `logiCard-modal-restyle` worktree per
   `CHAR_SELECT_MOTION_AGENT_BRIEF.md`'s boundary table. A Toolkit modal is a reasonable idea (dialogs
   are exactly the kind of self-contained shell UI this pilot suggests migrates cleanly) but it's that
   worktree's call, coordinated through the Integrator.
5. **Resolve the theme stylesheet question once, centrally**, before a second screen migrates — better
   to decide "checked-in default runtime theme asset" vs. "confirmed-safe to leave unset" a single time
   than rediscover finding 5 per screen.

## Verification

- EditMode 137/137, PlayMode 49/49, both green on `feat/char-select-motion` (batchmode, this worktree).
- `BootThroughLobbyLocalPlayReachesMatchHud` (the full Boot→Lobby→Match click-through, unchanged in
  scope) and `CharacterSelectNextRotatesArchetypeAfterCrossfade` both cover the Toolkit carousel via the
  new `FindVisualElement`/`ClickVisualElement` helpers — same coverage shape as before, adapted to the
  new element type.
- **Not done:** an actual human Play pass. See finding 5 and sequencing step 1 — this is the load-bearing
  gap in "is this actually shippable," not the batchmode-green result.
