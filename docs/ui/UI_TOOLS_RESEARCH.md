# UI Tools Research — next polish wave

**Status:** Drafted 2026-08-12 — docs-only research on `feat/ui-tools-research`  
**Audience:** Future implementer choosing tools / packs / stack moves without rediscovering the codebase.  
**Not this slice:** No `Assets/_Project/UI/**` rewrite. Atmosphere / weather and map-authoring docs are owned elsewhere.  
**Depends on:** [`UI_FLOW.md`](UI_FLOW.md), [`UI_BOARD_ANCHORED_COMPONENTS.md`](UI_BOARD_ANCHORED_COMPONENTS.md), [`UI_CHARACTER_SELECT_ANIMATION_REF.md`](UI_CHARACTER_SELECT_ANIMATION_REF.md), [`ART_DIRECTION.md`](../core/ART_DIRECTION.md) §4, `PRODUCT_MEMORY` **C48** / **C59**, [`ART_PACK_RESEARCH.md`](../map/ART_PACK_RESEARCH.md) (domain boundary note).

---

## 1. Current stack (inventory)

### Runtime system

| Fact | Evidence |
|------|----------|
| **uGUI only** at runtime | All six UI types live under `Assets/_Project/UI/` and `using UnityEngine.UI` — `UiFactory`, `UiStyle`, `ProgramHud`, `AppFlowController`, `SelectionGrid`, `ModalDialog`. |
| **No UI Toolkit (UXML/USS) screens** | No `.uxml` / runtime `UIDocument` usage in project UI. `Packages/manifest.json` has `com.unity.ugui` **2.5.0** + module `com.unity.modules.uielements` (engine default); no authored Toolkit UI. |
| **No TextMeshPro usage** | Zero `TMPro` / `TMP_` references. Labels are legacy `UnityEngine.UI.Text`. TMP is available via modern `com.unity.ugui` but **not adopted**. |
| **Code-driven construction, not prefabs** | Screens and HUD are built in `Awake`/`Init` via `new GameObject` + `UiFactory.CreatePanel` / `CreateText` / `CreateButton` / `CreateSlider`. No UI prefab library under `_Project/UI`. |
| **URP project** | `com.unity.render-pipelines.universal` **17.5.0** (Unity **6000.x** line). UI itself is Screen Space Overlay — RP-agnostic sprites/colors. |

### Shared chrome layer

- **`UiFactory`** (`Assets/_Project/UI/UiFactory.cs`) — shared constructor for panels, `Text`, `Button`, `Slider`; dock row helpers `PlaceRow` / `PlaceSplitCell` / `PlaceActionCell`; `ConfigureLandscapeScaler`.
- **`UiStyle`** (`Assets/_Project/UI/UiStyle.cs`) — ink/panel/accent palette; `Pad`/`Gap`/`RowGap`; landscape `ReferenceResolution = (1920, 1080)`; `CanvasMatchWidthOrHeight = 0.4f`; procedural **9-sliced** `RoundSprite` (runtime `Texture2D` — deliberately not Editor-only builtin skin, so batchmode/Player stay green).
- **`UiTextOverflow`** — `Body` / `Button` / `SingleLine` policies applied in `UiFactory.ApplyOverflow`.

### Fonts

- Default: `Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf")` in `ProgramHud` / `UiFactory` ctor fallback.
- No custom TTF/OTF, no TMP font assets, no display/body font pair (Character Select animation refs want Anton/Inter-class contrast — not present).

### Screen / HUD ownership

| Surface | Type | Role |
|---------|------|------|
| Pre-match shell | `AppFlowController` | Boot → Character Select → Map Select → Lobby → Waiting/Reveal stubs → Round Result → Match End (`UI_FLOW.md` map; **C48** / **C59**). |
| Selection lists | `SelectionGrid` | Character 2-up + Map 3-up; button names `Pick_{Id}` for PlayMode lookups. |
| Modals | `ModalDialog` | Dimmer + sliced card (~40%×40%), primary/secondary actions — Quit confirm pattern (**C59** restyle). |
| In-match HUD | `ProgramHud` | Top strip + bottom `HudDock` + board-anchored door prompt + scrubber markers. |

### Docking constants (coupled to camera)

From `ProgramHud`:

- `TopStripHeight = 0.08f` (full-width status).
- `HudDockHeight = 0.34f` (bottom band; moved from right-edge margin 2026-08-10).
- Compact row budget (`VerbRowHeight` 48, `StanceRowHeight` 44, `ActionRowHeight` 56, …) must fit `DockHeightInUiUnits` at ultrawide — EditMode `ProgramHudLayoutTests` locks this.
- **Camera coupling:** `GameBootstrap.ConfigureCamera` sets `cam.rect` from these fractions (`y = HudDockHeight`, height `1 - HudDockHeight - TopStripHeight`). Changing dock geometry **requires** camera rect rewire (commented on `ProgramHud.HudDockHeight`).

Canvas: `RenderMode.ScreenSpaceOverlay` + landscape scaler (`UiFactory.ConfigureLandscapeScaler`).

### Board-anchored pattern

Contract: [`UI_BOARD_ANCHORED_COMPONENTS.md`](UI_BOARD_ANCHORED_COMPONENTS.md).  
Reference implementation: `ProgramHud.BuildDoorPrompt` / `RefreshDoorPrompt` — world→screen→canvas-local (`anchorMin/Max = (0.5,0.5)`, `pivot` for beside-placement, camera arg `null` for Overlay). Identity / scheduled state / labeled options must stay intact for any future tool choice.

### Motion / animation today

- No DOTween / LeanTween package in `manifest.json`.
- Character Select is a flat `SelectionGrid` (tint selected cell) — **not** the TOONHUB / DepthCarousel feel documented in [`UI_CHARACTER_SELECT_ANIMATION_REF.md`](UI_CHARACTER_SELECT_ANIMATION_REF.md) (reference only; **C59** explicitly kept Map Select simple).

---

## 2. Pain / gaps for the next polish wave

Evidence-backed gaps vs docs + current code:

| Gap | Why it hurts | Evidence |
|-----|--------------|----------|
| **Legacy `Text` readability** | Long labels (`JUGGERNAUT`, map names) need `resizeTextForBestFit` hacks; no SDF crispness at 1080p/ultrawide; overflow policies are hand-tuned. | `SelectionGrid` best-fit; `UiTextOverflow`; dock compaction comments in `ProgramHud`. |
| **Motion language missing** | Character Select refs need coordinated scale/blur/opacity/position crossfade (~650ms) or a single `focusPosition` tween — hand-rolled uGUI has no shared tween helper. | `UI_CHARACTER_SELECT_ANIMATION_REF.md`; current `BuildCharacterSelect` is static grid. |
| **9-slice chrome is one procedural sprite** | Cardstock Time Card / soft shadow / paper edge (`ART_DIRECTION` §4) want authored slices, not only `UiStyle.RoundSprite` + flat `Image.color`. | `UiStyle.BuildRoundSprite`; `ProgramHud` cardstock color constants without paper textures. |
| **Dock density under C48 landscape** | Bottom dock is 34% of frame; ultrawide shrinks UI units; rows already compacted once (2026-08-10). Adding gear cards / richer Time Card art will fight the budget again. | `HudDockHeight`, `ControlsColumnContentHeight`, layout tests. |
| **Camera↔dock hard coupling** | Any visual redesign of margins must stay in sync with `ConfigureCamera` rect — packs that assume full-bleed HUD or mobile safe-areas fight this. | `ProgramHud` + `GameBootstrap.ConfigureCamera` comments. |
| **Board-anchored UI stays RectTransform math** | Toolkit migration would split Overlay GameObject UI vs panel UI unless door prompts stay uGUI — hybrid cost. | Board-anchored doc + `RefreshDoorPrompt`. |
| **Modal / shell still “default Unity flat”** | `ModalDialog` restyle (**C59**) improved proportions; still solid colors + round sprite, not glass/cardstock physicality called out in art notes. | `ModalDialog.Show`; `ART_DIRECTION` §4; `ART_PACK_RESEARCH` “UI glass → UI work, not an asset pack.” |
| **Portrait note vs C48** | Historical **C30** portrait/thumb-zone is superseded for Steam by **C48** landscape desktop. Do not buy/adapt mobile-first portrait kits as the default path; treat portrait as a future separate port. | `PRODUCT_MEMORY` C30 vs C48; `UI_FLOW.md` platforms line. |
| **No authored font pairing** | Animation refs and brand title want display + body fonts; project ships builtin LegacyRuntime only. | Font load sites above; Character Select ref font links. |

---

## 3. Tool / pack candidates (researched)

Constraints honored for every option: landscape desktop-first (**C48**), Steam mouse+keyboard; keep `UiFactory` / dock↔camera contract valid; board-anchored checklist remains law; Character Select carousel = **feel** only (do not port React/GSAP); **do not** buy screen-space god-ray packs (wrong domain — [`ART_PACK_RESEARCH.md`](../map/ART_PACK_RESEARCH.md) Explicit do-not-buy / orthographic note).

### A. Stay on hand-rolled uGUI + light helpers *(recommended default)*

| | |
|--|--|
| **What** | Keep `UiFactory` / `UiStyle` / code-built screens. Add thin helpers only: optional **DOTween** (free Asset Store / demigiant) or a tiny internal `UiMotion` for Character Select; optional authored 9-slice sprites under `Resources/UI/`; optional font assets still via legacy Text **or** TMP (see E). |
| **Fit** | Matches how every screen is built today; preserves PlayMode name lookups (`Pick_*`, `LockInButton`, `Door_Open`, …); board-anchored pipeline unchanged. |
| **Cost / risk** | Low. Motion and chrome are custom work, but scoped. No stack rewrite. |
| **Unlocks** | Character Select role-crossfade; ModalDialog paper/glass polish; dock iconography without fighting a full kit theme. |
| **Breaks** | Nothing structural if helpers stay behind `UiFactory`. |

### B. Unity UI Toolkit (runtime) migration / hybrid

| | |
|--|--|
| **What** | New screens in UXML/USS (`UIDocument`); optionally keep Program HUD + door prompt on uGUI. Unity 6 still documents **uGUI as a valid runtime choice**; Toolkit is stronger for data-heavy / stylesheet UI ([Unity Manual — Comparison of UI systems](https://docs.unity3d.com/6000.0/Documentation/Manual/UI-system-compare.html)). |
| **Fit** | Pre-match shell *could* benefit from stylesheets; **in-match HUD + board-anchored prompts** are GameObject/RectTransform-centric and already coupled to camera rect + PlayMode button names. |
| **Cost / risk** | **High** for full migrate; **Medium** for hybrid (two input/focus stacks, two layout languages, duplicated theme tokens). Character Select motion is *harder* in Toolkit than uGUI+tween (CSS-like transitions vs DOTween/`CanvasGroup`). |
| **Unlocks** | Long-term theming, less imperative layout code for static menus. |
| **Breaks** | Immediate rewrite of `AppFlowController` / tests; risk to door prompt if forced into Toolkit panels; slows polish wave. |

**Verdict:** Do **not** migrate Program HUD this wave. Hybrid only if a future shell-only experiment is briefed — not the default.

### C. Unity UI Extensions (uGUI control library)

| | |
|--|--|
| **Identity** | [Unity UI Extensions](https://assetstore.unity.com/packages/2d/gui/ui-extensions-175295) — Asset Store package **175295**; free; **BSD-3**; Store lists **Unity 6000.0.x Compatible** with **URP**. Also UPM/git distribution (v3.0 relaunch). |
| **What** | ~100 production uGUI controls / effects / layouts (gradients, soft masks, nicer scrollers, etc.) — **helpers**, not a Desk-Lamp art theme. |
| **Fit** | Stays on uGUI; can bolt onto existing hierarchy. Useful if scrubber/markers or modal need effects we refuse to hand-roll. |
| **Cost / risk** | Low–medium dependency surface; pick *few* controls, avoid importing the whole demo zoo into `_Project`. |
| **Unlocks** | Soft-mask / gradient / fancy layout primitives under Overlay canvas. |
| **Breaks** | Nothing if unused types stay out of critical path; still does not replace cardstock art direction. |

### D. Sprite UI kits (art chrome) — two researched

#### D1. Kenney UI Pack (CC0) — preferred art-kit candidate if buying/downloading chrome

| | |
|--|--|
| **Identity** | [Kenney UI Pack](https://kenney.nl/assets/ui-pack) (also mirrored on OpenGameArt); **CC0** public domain. PNG/spritesheet UI chrome — pipeline-agnostic (works with URP + uGUI `Image` 9-slice). |
| **Fit** | Landscape-neutral flat buttons/panels/sliders; easy to recolor toward warm cardstock / ink (`UiStyle` palette) without fighting a pastel “casual mobile” skin. |
| **Cost / risk** | Free. Integration work = import sprites, wire through `UiFactory` optional sprite args (API already accepts `Sprite` + `Image.Type.Sliced`). Risk: looks generic if used raw — must retint to Desk-Lamp. |
| **Unlocks** | Real 9-slice chrome for ModalDialog / SelectionGrid / Time Card frame without procedural-only corners. |
| **Breaks** | None if factory keeps color overlays; do not replace board-anchored logic with kit prefabs. |

#### D2. Free Casual GUI (Unco Games) — researched, **weak fit**

| | |
|--|--|
| **Identity** | [Free Casual GUI](https://assetstore.unity.com/packages/2d/gui/free-casual-gui-332804) (Asset Store **332804**); free under Store EULA; URP Compatible (listed 2021.3+). Also [itch.io](https://uncogames.itch.io/free-casual-gui). |
| **Fit** | Explicit **boba / cream-pastel casual** aesthetic — fights Desk-Lamp diorama + AR scrubber contrast (`ART_DIRECTION` §4). Portrait/mobile sample pages more than Steam 16:9 commander HUD. |
| **Cost / risk** | Free but high **art-direction debt** (recolor until it stops reading as another genre). |
| **Unlocks** | Lots of PNG chrome quickly. |
| **Breaks** | Visual brand coherence if adopted as default skin. |

**Paid alternative noted (not required):** [Flat Minimalist GUI / UI pack 2.0](https://assetstore.unity.com/packages/2d/gui/flat-minimalist-gui-ui-pack-2-0-over-700-png-194676) (~$34.99, Store EULA, URP Compatible) — cleaner desktop flat look than Casual GUI, still generic; only if Kenney CC0 proves too thin.

### E. TextMeshPro migration only *(optional, orthogonal)*

| | |
|--|--|
| **What** | Replace `UnityEngine.UI.Text` with TMP under `com.unity.ugui` 2.x (TMP ships with modern uGUI). Keep all layout code. |
| **Fit** | Directly attacks overflow/crispness pain; fonts for Character Select ghost titles become tractable. |
| **Cost / risk** | Medium touch-surface: every `CreateText`, PlayMode text asserts, best-fit hacks. Do **not** mix TMP and legacy Text on the same tight dock row without a plan. |
| **Unlocks** | SDF text, font assets, better truncation. |
| **Breaks** | Any test that assumes `Text` component type; factory signature churn. |

Can combine with **A** (and Kenney sprites) without choosing Toolkit.

### Explicit rejects / wrong domain

| Reject | Why |
|--------|-----|
| Screen-space god-ray / sun-shaft packs (Super Rays, LSPP, etc.) | Lighting VFX domain; ortho-fragile — [`ART_PACK_RESEARCH.md`](../map/ART_PACK_RESEARCH.md). Not UI tools. |
| Full UI Toolkit rewrite of `ProgramHud` this wave | High risk to dock↔camera + board-anchored + PlayMode names for little ship value. |
| Porting React/Tailwind/GSAP Character Select literally | Forbidden by animation ref doc; feel only. |
| Mobile-portrait UI kits as Steam default | **C48** supersedes **C30** for this product surface. |
| Free Casual GUI as default skin | Aesthetic clash (above). |

---

## 4. Recommendation matrix

| Rank | Path | When |
|------|------|------|
| **1 — Default** | **A: hand-rolled uGUI + light helpers**, plus **Kenney CC0 sprites** as optional chrome, plus **DOTween (or tiny UiMotion)** for Character Select feel | Next UI polish / Character Select / Modal restyle briefs |
| **2 — Parallel optional** | **E: TMP-only** once text overflow becomes a playtest blocker | Can land before or with Character Select motion |
| **3 — Fallback** | **C: Unity UI Extensions** — import *specific* controls only if soft-mask/gradient needs appear | Only after hand-roll estimate exceeds ~0.5 day per control |
| **4 — Do not yet** | **B: Toolkit migration**; **D2 Casual GUI** as theme; god-ray packs; React ports | Revisit Toolkit only for a greenfield shell if uGUI chrome hits a hard ceiling |

**Default path (one line):** Stay on code-driven uGUI (`UiFactory`/`UiStyle`), add motion + 9-slice/font polish in-place; do not migrate Toolkit or adopt a pastel UI megakit.

**Fallback:** If Character Select motion or soft-mask effects stall, add DOTween + UI Extensions selectively — still no Toolkit rewrite.

**Do not do X yet**

- Do not rewrite `Assets/_Project/UI/**` onto UI Toolkit.
- Do not buy screen-space volumetric UI/lighting packs for HUD polish.
- Do not implement DepthCarousel 3D pedestal look as primary Character Select (ref doc: Reference 1 flat roles primary).
- Do not change `HudDockHeight` / `TopStripHeight` without an Integrator camera-rect pass.
- Do not theme Map Select with the Character Select carousel treatment (**C59**).

---

## 5. Implementation brief stubs

Ready for Integrator to spin later — **docs/research only in this slice; no code here**.

1. **TMP factory swap**  
   - **Touch:** `UiFactory.cs`, `UiStyle.cs` (font assets), callers (`ProgramHud`, `AppFlowController`, `SelectionGrid`, `ModalDialog`), PlayMode tests that `GetComponent<Text>()`.  
   - **DoD:** All HUD/shell labels are TMP; ultrawide dock + Character/Map names readable without best-fit hacks; EditMode+PlayMode green.

2. **Kenney (or equivalent CC0) chrome pass**  
   - **Touch:** `Resources/UI/` sprites; `UiFactory.CreateButton`/`CreatePanel` default sprites; `ModalDialog`, `SelectionGrid`, Time Card panel colors in `ProgramHud`.  
   - **DoD:** Modal + selection cards use sliced paper/ink chrome retinted to `UiStyle`; still matches board-anchored door prompt sizing; no dependency on Casual GUI pastel.

3. **Character Select motion (feel port)**  
   - **Touch:** `AppFlowController.BuildCharacterSelect` (or new `CharacterSelectView.cs`); optional DOTween package; art placeholders for Scout/Juggernaut; **read** `UI_CHARACTER_SELECT_ANIMATION_REF.md` (Reference 1 primary).  
   - **DoD:** Prev/next or click rotates center/flank roles with ~650ms coordinated fade/scale; Confirm still sets archetype; Map Select unchanged; PlayMode `Pick_Scout` / `Pick_Juggernaut` names preserved or tests updated deliberately.

4. **Dock density / gear-ready layout audit**  
   - **Touch:** `ProgramHud` row constants; possibly `HudDockHeight` **with** `GameBootstrap.ConfigureCamera` rect; `ProgramHudLayoutTests`.  
   - **DoD:** Documented row budget for future gear strip; ultrawide content height still ≤ `DockHeightInUiUnits`; camera rect test updated if fractions change.

5. **ModalDialog physical restyle**  
   - **Touch:** `ModalDialog.cs`, `UiStyle` shadow/cardstock tokens; optional sprite from stub 2.  
   - **DoD:** Quit/confirm modal reads as cardstock-on-dimmer (ART_DIRECTION), not flat grey Unity default; primary/secondary hit targets unchanged for tests.

---

## Open questions for human

1. Prefer **DOTween** (common, free) vs a **zero-dependency** internal `UiMotion` for Character Select?  
2. Approve **Kenney CC0** as the first chrome download, or wait for bespoke paper UI art?  
3. Should **TMP migration** gate Character Select polish, or can motion ship on legacy Text first?  
4. Any appetite for a **shell-only UI Toolkit experiment** later, or lock “uGUI until post-Steam-vertical-slice”?

---

## See also

- [`UI_FLOW.md`](UI_FLOW.md) — screen map / landscape HUD regions  
- [`UI_BOARD_ANCHORED_COMPONENTS.md`](UI_BOARD_ANCHORED_COMPONENTS.md) — door-prompt contract  
- [`UI_CHARACTER_SELECT_ANIMATION_REF.md`](UI_CHARACTER_SELECT_ANIMATION_REF.md) — motion feel only  
- [`ART_PACK_RESEARCH.md`](../map/ART_PACK_RESEARCH.md) — do not conflate lighting VFX packs with UI tools  
