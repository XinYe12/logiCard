# Shell chrome — Boot / Character Select / Map Select / Lobby / Match End

**Status:** Landed 2026-08-18 on the shell-chrome worktree. Awaiting human/Integrator review.
**Scope fence:** *visual chrome only.* Same discipline as the Match Shell Layout wave ("layout / region
geometry only"). No screen was added or removed, no button's behaviour changed, no new gameplay.

**Not covered here:** the in-match HUD (`ProgramHud.cs`, `GearHandView.cs`, the five-band
InfoBar / MapViewport / HandBand / ToolBar / TimelineSchedule stack). That is already built and
human-signed-off; it keeps its own `CreateBackingPanel` + `CreateButton` chrome. Do **not** swap shell
helpers onto it without a fresh brief.

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

It walks Boot → Character Select (both archetypes) → Map Select → Lobby → Round Result → Match End →
Quit modal and writes nine 1920×1080 PNGs. It temporarily flips the Canvas from `ScreenSpaceOverlay` to
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

- Archetype art: the emblem well wants a real portrait or clay figure, not a monogram.
- The collection's bucket 8 (lobby/shell layout refs) is still thin — this pass built from the shared
  material language rather than from reference screens. Worth a human look specifically at layout, not
  just colour.
- If the shell chrome is approved, `docs/UI_CHROME_COLLECTION.md` can promote `normal-card` +
  `button-gradient-pill` from "Candidate/Held" to the locked default family, and this becomes a
  `PRODUCT_MEMORY` row.
