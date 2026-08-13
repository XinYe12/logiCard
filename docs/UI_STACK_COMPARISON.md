# UI stack comparison — Toolkit vs uGUI vs third-party

**Status:** Research deliverable (docs only — no code). Written 2026-08-13 on `feat/modal-restyle` after the UI seat’s mandate widened to all surfaces (lobby, Character Select, Map Select, in-game HUD/dock, modals).  
**Audience:** Human + Integrator — pick a default stack before the next UI build wave.  
**Not a PRODUCT_MEMORY row** until human confirms.

**Sources (read, not re-litigated blind):**

- [`UI_TOOLKIT_MIGRATION_PROPOSAL.md`](UI_TOOLKIT_MIGRATION_PROPOSAL.md) — Character seat’s Toolkit pilot findings (live on `feat/char-select-motion`; pilot **reverted** after human Play: “it is still bad”). Copy that file onto `master` when Character merges, or read it from `D:\projects\Game\logiCard-char-select-motion\docs\`.
- [`Assets/_Project/Art/UI/THIRD_PARTY.md`](../Assets/_Project/Art/UI/THIRD_PARTY.md) — same branch: **Kenney “UI Pack - Adventure” (CC0) already selected** for Character Select chrome.
- [`UI_TOOLS_RESEARCH.md`](UI_TOOLS_RESEARCH.md) — pre-pilot survey (2026-08-12); still correct on HUD coupling and rejects.
- Character STATUS @ `a707d9f` / tip `feat/char-select-motion`: uGUI carousel + `UiMotion` + Kenney 9-slice skin; awaiting human Play on the Kenney look; **5 ahead / 7 behind** `master` (includes modal restyle + C63).

---

## Sync note — Character Select before inherit

| Fact | Implication for UI seat |
|------|-------------------------|
| Toolkit Character Select **reverted** (`a915bb7`); history at `2c99a08` | Do not restart a Toolkit rewrite of that screen without a new human ask + Theme Style Sheet plan. |
| Live path is **uGUI** `CharacterSelectView` + `UiMotion` + Kenney Adventure panels/buttons | Inherit that surface as-is; polish chrome/motion on uGUI. |
| Map Select deliberately plain (`SelectionGrid`, **C59**) | Do not apply carousel treatment there. |
| Kenney Adventure is a **known fantasy-adventure compromise** on a SWAT-tactics game | Expand the same pack carefully; retint/crop; do not pretend it is bespoke cardstock art. |
| Modal restyle human-signed on this worktree | Keep `Modal*` cardstock tokens; optional later Kenney slice on modals is chrome, not a stack change. |

---

## Options

### A. Unity UI Toolkit (runtime)

| | |
|--|--|
| **What** | `UIDocument` / `VisualElement` (+ optional UXML/USS). Unity’s actively developed UI system. |
| **Evidence** | Character Select pilot: shell **can** move; layout math ports mechanically; native `border-radius`. Also hit real costs: `UIDocument` teardown on `OnEnable`/`OnDisable`, parent `SetActive` cascade, orphaned hosts across PlayMode cycles, different click-test pattern, missing Theme Style Sheet (`No Theme Style Sheet set to PanelSettings`), asmdef + sort-order hybrids. Human visual verdict after Play: **still bad** → revert. |
| **Fit** | Pre-match shell *in theory*. In-match `ProgramHud` / board-anchored door prompts **unproven** and still coupled to camera rect + Overlay raycast (`UI_BOARD_ANCHORED_COMPONENTS.md`). |
| **Cost** | High for full migrate; Medium for hybrid shell — two input/focus stacks, duplicated tokens, Theme asset decision required before a second screen. |
| **Verdict** | **Park.** Findings remain valid cost data; they do **not** justify a project-wide migration after a failed visual pilot. |

### B. Continued uGUI (current stack)

| | |
|--|--|
| **What** | Code-built `UiFactory` / `UiStyle` / Overlay Canvas: `ProgramHud`, `AppFlowController`, `SelectionGrid`, `ModalDialog`, `GearHandView`, `CharacterSelectView`, `UiMotion`. Legacy `Text` today; TMP orthogonal later. |
| **Evidence** | Entire ship UI already here. Modal cardstock human-signed. Character Select motion + Kenney skin landed on uGUI after Toolkit revert. PlayMode name lookups (`Pick_*`, `LockInButton`, `ModalPrimary`, …) and board-anchored math stay one technology. |
| **Fit** | Landscape desktop (**C48**), Steam mouse+keyboard, dock↔camera contract, door prompts. |
| **Cost** | Low incremental — polish in place. Pain points are chrome/text/motion, not the framework. |
| **Verdict** | **Default backbone.** |

### C. Third-party asset / library (not a full UI framework)

Treat as **layers on top of a stack**, not a third stack:

| Sub-option | Role | Status |
|------------|------|--------|
| **Kenney “UI Pack - Adventure” (CC0)** | 9-slice panel/button chrome | **Already chosen** for Character Select (`THIRD_PARTY.md`). Closest warm parchment/wood match in the on-hand CC0 library; fantasy tone is an accepted compromise. |
| **Unity UI Extensions** | Optional soft-mask / gradient / fancy controls | Research-only fallback if hand-roll &gt; ~0.5 day per control (`UI_TOOLS_RESEARCH.md`). |
| **DOTween** | Tween package | **Not needed** — `UiMotion` already covers Character Select; keep zero-dep unless motion stalls. |
| **Full paid GUI megakits / Casual pastel kits** | Theme replacement | **Reject** — fights Desk-Lamp / cardstock (`ART_DIRECTION`); Casual GUI already rejected. |
| **“Mature” third-party UI frameworks** (NGUI, etc.) | Replace uGUI | **Reject** — adds vendor lock for no ship gain over uGUI+Kenney. |

| | |
|--|--|
| **Verdict** | **Adopt chrome (Kenney Adventure) aggressively on uGUI; do not adopt a third-party UI runtime.** |

---

## Comparison (one screen)

| Criterion | Toolkit | uGUI | Third-party chrome/libs |
|-----------|---------|------|-------------------------|
| Ship risk to HUD / door prompts | High if folded in | Low (already live) | N/A if sprites only |
| Shell polish speed | Medium (after Theme + lifecycle pattern) | Fast with Kenney + `UiMotion` | Fast (sprites already importing) |
| Human Play signal | Failed pilot (“still bad”) | Modal signed; Char Select Kenney awaiting Play | Kenney is the response to “hand-drawn still bad” |
| Test / batchmode | Extra helpers; Theme gap | Existing `FindByName` / `onClick.Invoke` | Import tool already batchmode-safe |
| Art direction | Doesn’t supply parchment look | Flat until skinned | Adventure pack = warm paper-ish chrome |

---

## Recommendation

**Stay on uGUI as the only runtime UI stack for the vertical-slice polish wave.**  
**Skin with Kenney “UI Pack - Adventure” (already selected)** across shell surfaces where wood/parchment reads as cardstock-adjacent; retint via `UiStyle` / Image color; do not theme Map Select like Character Select (**C59**).  
**Keep `UiMotion`** for shell motion; leave `ProgramHud` / board-anchored prompts on uGUI.  
**Park UI Toolkit** until/unless human explicitly reopens it with: (1) checked-in default Theme Style Sheet, (2) a shell-only second pilot, (3) human Play before any further screens. Do not migrate the dock.  
**Do not** buy or adopt a third-party UI framework; optional UI Extensions only for a named control gap.

**One-liner for Integrator:** *uGUI + Kenney Adventure chrome + existing `UiMotion`; Toolkit parked post-revert; no new UI runtime.*

---

## Suggested next builds (after human confirms this doc)

1. Inherit / merge Character Select (Kenney skin Play sign-off) onto the unified UI seat without stack thrash.  
2. Extend Kenney Adventure (or matching parchment slices) to Modal / Map / Lobby / Time Card frames — chrome pass, not Toolkit.  
3. Dock density + `GearHandView` parent into `ProgramHud` when OPEN #16 / Integrator brief allows.  
4. TMP factory swap when overflow becomes a playtest blocker (orthogonal to A/B/C).

---

## See also

- `docs/UI_TOOLKIT_MIGRATION_PROPOSAL.md` (Character branch — real Toolkit costs)  
- `Assets/_Project/Art/UI/THIRD_PARTY.md` (Character branch — Kenney Adventure provenance)  
- `docs/UI_TOOLS_RESEARCH.md` · `docs/UI_FLOW.md` · `docs/UI_BOARD_ANCHORED_COMPONENTS.md` · `docs/ART_DIRECTION.md` §4
