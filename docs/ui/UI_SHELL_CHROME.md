# Shell chrome — Boot / Character Select / Map Select / Lobby / Match End

**Status:** Landed 2026-08-18 on the shell-chrome worktree; Character Select's live 3D card portrait
landed 2026-08-20 on the same worktree. Awaiting human/Integrator review.
**Scope fence:** *visual chrome only.* Same discipline as the Match Shell Layout wave ("layout / region
geometry only"). No screen was added or removed, no button's behaviour changed, no new gameplay.

**Not covered here:** the in-match HUD (`ProgramHud.cs`, `GearHandView.cs`, the five-band
InfoBar / MapViewport / HandBand / ToolBar / TimelineSchedule stack). That is already built and
human-signed-off; it keeps its own `CreateBackingPanel` + `CreateButton` chrome. Do **not** swap shell
helpers onto it without a fresh brief.

The 2026-08-20 card-portrait pass is the one place this reaches outside `Assets/_Project/UI/`: it makes
`PawnView`'s archetype-mesh load/normalise/tint the shared entry point both the board and the preview call,
and adds two isolation lines to `GameBootstrap` (culling mask, lighting probe). No pawn, board or match
behaviour changed — see that section for why the sharing is the point rather than a shortcut.

## Why this exists

The human's verdict on the previous shell was "totally unacceptable — reads as placeholder programmer
art." Three concrete causes, all now fixed:

| Symptom | Cause | Fix |
|---|---|---|
| Flat muddy brown page | `UiStyle.CharSelectBgScout` (0.42, 0.28, 0.14) literally filled the whole screen, and every other screen filled it with `PanelDark` | Screens are a **lit backdrop**, not a colour fill — `UiFactory.CreateShellBackdrop` |
| Plain rectangular boxes with no depth | Buttons/panels were single `Image`s with one flat colour | Layered chrome: contact shadow + riser + lit face (`CreateShellButton`), parchment plates (`CreateShellPlate`) |
| Generic orange rectangle CTA | `UiStyle.Accent` fill, default `ColorTint` transition | One saturated red accent from the clay-icon style lock, plus a physical press (`ShellButton`) |

## The five ideas

1. **A screen is a lit ground, not a colour.** `UiFactory.CreateShellBackdrop(parent, glowTint)` stacks
   four raycast-transparent layers at the head of a screen: `ShellVoid` (deep warm near-black), a
   stretched radial light pool tinted per screen, a tiled paper mottle, and an edge vignette. Nothing in
   this game should ever be a single flat full-screen `Image` again.
2. **Objects sit on the ground and cast shadows.** Every card, plate and button has a shadow layer
   beneath it. That is what makes the toy/diorama read; a rounded rectangle with no shadow is still a
   rectangle.
3. **Warm cream + exactly one red.** Straight from the locked clay-icon style in
   `docs/UI_CHROME_COLLECTION.md` (`icon_bandage` is the lock). Parchment (`ModalCard` family, already
   shipped and approved via `ModalDialog`) for anything you read; `ShellAccent` red for the one thing the
   screen wants you to do. No second accent hue.
4. **Display face for character, body face for reading.** Iomanoid (CC0) carries headlines, the ghost
   headline, the brand mark and the card monograms. Everything smaller — button labels, card name
   plates, body copy — stays on `LegacyRuntime.ttf`, bold where it needs weight. Iomanoid is an outlined
   art-deco face: gorgeous at 50pt+, thin and low-contrast at 22–36pt. This split was made *after*
   looking at a render where every button label was Iomanoid and hard to read.
5. **Buttons are physical.** `ShellButton` gives a face on a visible riser over a contact shadow; press
   drops the face into the shadow, hover lifts it. Ported from
   `docs/ui-collection/button-gradient-pill.css` (Uiverse.io by Codecite, MIT), which does the same thing
   with `translateY` + `box-shadow: none`.

## The API (all on `UiFactory`, all shell-only)

| Helper | Use |
|---|---|
| `CreateShellBackdrop(parent, glowTint)` | Paint a screen's ground. Returns the light-pool `Image` so a screen can re-tint its mood at runtime. |
| `CreateShellButton(parent, name, label, tone, size, onClick, riser)` | Every shell button. Tones: `Primary` (red clay), `Secondary` (parchment), `Quiet` (dark slate). |
| `CreateHeadline(parent, name, text, size, ink, overflow, shadowDistance)` | Display-face headline with a uGUI `Shadow`. |
| `CreateRule(parent, name, min, max)` | Short accent rule that sits a headline on something. |
| `CreateShellPlate(parent, name, min, max)` | Parchment card for copy. Returns the face — parent content under it. |
| `CreateShellSlate(parent, name, min, max)` | Warm dark counterpart for panels that must not be paper. |

Procedural sprites live on `UiStyle`: `RoundSprite` (existing), plus `PillSprite`, `RadialSprite`,
`VignetteSprite`, `GrainSprite`. All generated in code for the same reason `RoundSprite` already was —
Unity's builtin extra resources silently return null in batchmode and in a real Player build. `GrainSprite`
is built at **1 pixel-per-unit** so one `Image.Type.Tiled` tile is 128 UI units; at the default 100 ppu it
would tile ~15,000 times across a 1920-wide canvas.

## The Character Select card portrait is a live 3D render (2026-08-20)

The human looked at the restyle above and asked for one thing by name: *"I'd like to see the actual model
of the character be placed inside the character card."* The monogram in the emblem well is gone (kept only
as a fallback); each card now shows a **live 3D render of that archetype's real match prefab**.

**The rule this is built on:** the preview loads the *same* `Resources` prefab, at the *same* normalised
height, with the *same* team tint that a match spawns — via `PawnView.TryInstantiateArchetypeVisual`,
which is now the single public entry point for instantiating an archetype mesh (`PawnView.Init` and the
preview rig are both callers; `PawnView.ResourcePathFor` owns the path, `PawnView.AttackerTint` /
`DefenderTint` own the colours `GameBootstrap.BuildPawns` uses). **Do not give Character Select its own
preview asset, its own scale, or its own tint.** The moment a preview shows something other than what the
board will show, the screen is lying to the player about their own pick — the same failure mode
`UI_BOARD_ANCHORED_COMPONENTS.md` bans for door controls ("live state, read from the authoritative model,
never inferred").

**How it works** (`Assets/_Project/UI/CharacterPreviewRig.cs`). A mesh cannot be parented under a
`RectTransform`, so this is the standard render-texture route:

| Piece | Choice made | Why |
|---|---|---|
| Where the model lives | An "island" at `y = -4000`, one per rig, 40 units apart | Nowhere near the arena, and nothing else is within reach of the preview camera's 12-unit far clip |
| Isolation | Layer **`CharacterPreview`** (TagManager index 8); preview camera's `cullingMask` is only that layer, and `GameBootstrap.ConfigureCamera` masks it *out* of the board camera | Three independent guards (distance, mask, far clip) because this rig is alive in the same scene as a real match |
| Lighting | The rig owns three private directional lights (warm key, cool fill, back rim) | Character Select happens **before** `GameBootstrap.BuildLighting` has lit anything — a rig that borrows scene lighting renders a black cutout on the very screen it's for |
| Texture | 768×896 `ARGB32`, `antiAliasing = 1`, downsampled by the `RawImage` | Supersampling instead of RT MSAA, which URP renegotiates against the pipeline asset |
| Background | `SolidColor` with **alpha 0** | The card's own emblem well stays the backing; an opaque clear pastes a hard rectangle onto the cardstock |
| Framing | fov 28, 6° down-tilt, 1.14 world units of height framed, aimed at y 0.54 | Tuned by looking at renders, not derived — 1.24 read small and adrift in the well |
| Motion | Model yaw sways ±13° around a −22° three-quarter base | Reads as live 3D rather than a baked image, without spinning the face away |

**Carousel swapping needs no code at all.** Both rigs stay alive and each card is permanently bound to its
own archetype's `RenderTexture` — the Scout card shows the Scout because it *is* the Scout card, centre or
flank. A destroy/instantiate swap on the centre card was the alternative and was rejected: it adds state
that can desync from `_activeIndex` mid-crossfade, to save one small camera on a menu screen.

**Lifecycle:** `CharacterSelectView`'s own `OnEnable`/`OnDisable` *is* the screen lifecycle, because
`AppFlowController.Show` shows and hides screens by toggling their root GameObject. The rigs (cameras,
lights, models) switch off with the screen; `OnDestroy` releases the RenderTextures.

**A third trap, for the list below:** under `-batchmode -nographics` there is no graphics device, so
`RenderTexture.Create` fails and URP logs a burst of `Unable to find surface for attachment 0` errors —
which the test framework counts as unhandled log errors and **fails an unrelated Character Select test**
(`AppFlowPlayModeTests.CharacterSelectNextRotatesArchetypeAfterCrossfade`, which merely activates the
screen). `CharacterPreviewRig.Create` therefore declines to build when
`SystemInfo.graphicsDeviceType == GraphicsDeviceType.Null`, and the card falls back to its monogram. Any
future off-screen render rig needs the same guard. Note the shape of this: the headless suite went red for
a *rendering* reason, which is the mirror image of trap 1 below — headless green never proved the pixels,
and here headless red didn't mean the pixels were wrong either. Only the screenshot run settles it.

**A trap this created, already defused:** `GameBootstrap.BuildLighting` used to bail out on
`FindFirstObjectByType<Light>() != null`. The preview rig's private lights would have tripped that and
left a match board completely unlit. It now uses `SceneIsAlreadyLit()`, which skips lights on the
`CharacterPreview` layer. If you add another off-screen rig with its own lights, put it on that layer or
teach that method about it.

**What the previews immediately exposed** (pre-existing board bugs, *not* preview bugs — do not "fix" them
in the preview, fix the pawn art):
- `PawnView.TintedPartNameMarker` is `"Body"`, which on these imported meshes is the **skin**, not the
  torso — so the Scout has a bright orange face and hands, and always has had on the board.
- The Juggernaut prefab ships with its **bunny-ears hat mesh enabled**, so the "breacher" wears rabbit
  ears. Invisible at top-down board scale; unmissable at card scale.

## Two traps that already bit once

1. **`SetAsFirstSibling` after a backdrop buries you.** The backdrop occupies the first
   `UiFactory.ShellBackdropLayerCount` (4) sibling slots. Character Select's ghost headline and card
   stage used `SetAsFirstSibling()` / `SetSiblingIndex(1)` from the pre-backdrop era, and the entire
   carousel rendered *behind* the void layer — the screen came up empty. Offset by
   `ShellBackdropLayerCount` instead. This was caught only because a screenshot was taken; every
   batchmode test still passed, because the objects existed and were clickable, just invisible.
2. **`WaitForEndOfFrame` never resumes under `-batchmode`.** The screenshot harness below hung forever on
   it. Use two plain `yield return null`s plus `Canvas.ForceUpdateCanvases()`.

## Verifying visually (required, not optional)

Batchmode tests prove code correctness, never visual correctness — trap 1 above is the proof. There is a
harness for this: `Assets/_Project/Tests/PlayMode/ShellChromeScreenshotTests.cs`. It is a no-op unless
`LOGICARD_SHOT_DIR` is set, and must run **without** `-nographics`:

```
LOGICARD_SHOT_DIR=/tmp/shots "$UNITY" -batchmode -projectPath <path> \
  -runTests -testPlatform PlayMode -testFilter ShellChromeScreenshotTests \
  -testResults /tmp/shot.xml -logFile /tmp/shot.txt
```

It walks Boot → Character Select (Scout, Juggernaut, then back to Scout) → Map Select → Lobby → Round
Result → Match End → Quit modal and writes ten 1920×1080 PNGs. The extra swap-back shot exists because the
two card portraits are separate live rigs: it is the frame that would catch a card showing the wrong
archetype's model, or a preview that only renders for whichever archetype happened to be centred first. It temporarily flips the Canvas from `ScreenSpaceOverlay` to
`ScreenSpaceCamera` against a RenderTexture — an Overlay canvas cannot be read back at all, and the
RenderTexture also pins captures to the CanvasScaler's reference resolution rather than whatever window
size batchmode picks.

**Extend the harness when you add a shell screen.** A screen with no capture has not been looked at.

## Element names are load-bearing

PlayMode tests find shell controls by GameObject name (`SliceSceneFixture.FindByName<T>`):
`TitlePlayButton`, `Pick_Scout`, `Pick_Juggernaut`, `Pick_<MapId>`, `ConfirmCharacter`, `ConfirmMap`,
`FindMatchButton`, `LocalPlayButton`, `CharSelectPrev`, `CharSelectNext`, `RematchButton`,
`QuitToTitleButton`, `ContinueButton`, `ModalDialog`, `ModalPrimary`. `CreateShellButton` deliberately
puts the `Button` on the outer object carrying the placement rect so those lookups and existing
`Stretch(button.GetComponent<RectTransform>(), …)` call sites keep working. Rename only with a real
reason, and update the tests in the same commit.

## Assets used, and what was deliberately not

- **Iomanoid** (CC0, Raymond Larabie) → `Assets/_Project/Art/UI/Resources/Fonts/Iomanoid.otf`. Provenance
  in `Assets/_Project/Art/UI/THIRD_PARTY.md`. Only the base face; the Front/Back/Shine layered variants
  stay in the collection — depth here comes from a uGUI `Shadow` component, which stays in sync when a
  headline's text is reassigned at runtime (Match End and Round Result both do this).
- **`normal-card.css`** (Uiverse.io by adamgiebl, MIT) — the lift + contact + inset-lip shadow stack. Was
  already ported into `Modal*`/`Dock*` tokens by the HUD chrome pass; the shell plates reuse that port
  rather than re-deriving it.
- **`button-gradient-pill.css`** (Uiverse.io by Codecite, MIT) — press motion, as described above. The
  glass-pill and bubbles-fill buttons stay parked; glass is a modern-web look that fights the toy read,
  and bubbles-fill needs a hover flood-fill that would be a lot of machinery for the same result.
- **Kenney "UI Pack - Adventure"** card faces stay on the Character Select cards — real 9-slice texture in
  the right warm family. `panel_brown` is a cool grey-tan, so it is tinted warm per-card
  (`Card.BaseTint`); the flank dim multiplies that tint instead of white. `button_brown` is now unused —
  Prev/Next moved onto `CreateShellButton` so every shell button is one family.
- **The clay icons** (`docs/ui-collection/icons/`) are used as a *palette and material reference only*.
  They currently ship as JPEGs on flat white with no alpha, so dropping one on a card pastes a white
  square. Character Select uses a monogram in an emblem well as the placeholder instead. When those get
  real alpha, the emblem well is the slot they belong in.
- **No off-the-shelf asset pack** was adopted as the direction, per the standing rejection in
  `docs/UI_CHROME_COLLECTION.md`.

## Open / next

- ~~Archetype art: the emblem well wants a real portrait or clay figure, not a monogram.~~ Done
  2026-08-20 — the well now holds a live 3D render of the archetype's real match model (section above).
- Pawn art, *surfaced* by that preview and still open: the `"Body"`-named renderer that
  `PawnView` team-tints is the skin (orange-faced Scout), and the Juggernaut prefab's bunny-ears hat mesh
  is enabled. Both are board-side art bugs; fix them on the pawn, not in Character Select.
- The imported meshes have an `Animator` with no controller, so both the board and the card show the
  bind pose (arms out). A one-clip idle would lift the card a lot; it belongs on the pawn art track so
  the board gets it too.
- The collection's bucket 8 (lobby/shell layout refs) is still thin — this pass built from the shared
  material language rather than from reference screens. Worth a human look specifically at layout, not
  just colour.
- If the shell chrome is approved, `docs/UI_CHROME_COLLECTION.md` can promote `normal-card` +
  `button-gradient-pill` from "Candidate/Held" to the locked default family, and this becomes a
  `PRODUCT_MEMORY` row.
