# Character Select Motion — Agent Brief

**Worktree:** `D:\projects\Game\logiCard-char-select-motion`  
**Branch:** `feat/char-select-motion` @ `498d463`  
**Why:** Phase 5 UI polish — Character Select is still a flat 2-up grid. Research (`docs/UI_TOOLS_RESEARCH.md` stub 3) + motion ref (`docs/UI_CHARACTER_SELECT_ANIMATION_REF.md` Reference 1) are ready. Human is busy on atmosphere elsewhere; Integrator owns main dirty tree.

## Defaults (open questions decided by Integrator)

- **Zero-dependency** internal `UiMotion` — do **not** import DOTween.
- Legacy `Text` stays — no TMP migration in this slice.
- No Kenney chrome download.
- Map Select stays a plain `SelectionGrid` (**C59**).

## The job

1. Read `docs/UI_CHARACTER_SELECT_ANIMATION_REF.md` (Reference 1 only — feel, not React port) and `docs/UI_TOOLS_RESEARCH.md` §5 stub 3.
2. Add `Assets/_Project/UI/UiMotion.cs` — tiny helper: animate float/Color/Vector2 over duration with ease `cubic-bezier(0.4,0,0.2,1)` approx (Unity `EaseInOutCubic` / SmoothStep is fine). Drive from a MonoBehaviour coroutine host or static + runner on the canvas. No third-party packages.
3. Replace Character Select’s flat grid with a **2-item center/flank carousel**:
   - Roster stays Scout + Juggernaut only.
   - Roles: center (large, opaque, front) + one flank (smaller, slightly dimmed). With 2 items there is no populated “back”.
   - Prev/Next buttons **and** clicking the flank card rotates roles over **~650ms** coordinated scale/opacity/anchor crossfade. Lock input while animating.
   - Giant ghost headline behind figures (e.g. archetype name) — mood only, not a hit target.
   - Background panel tint may crossfade toward a per-archetype warm accent (desk-lamp palette — stay in `UiStyle` family; no pastel megakit).
4. Wire from `AppFlowController.BuildCharacterSelect` — prefer extracting `CharacterSelectView.cs` if it keeps `AppFlowController` readable. Confirm still goes to Map Select; `SelectedArchetype` still updates.
5. **Preserve PlayMode names:** `Pick_Scout`, `Pick_Juggernaut`, `ConfirmCharacter`. Tests click those buttons — keep them findable (can be the card hit targets).
6. `SelectionGrid` remains for Map Select — do not break it; Character Select may stop using it.
7. Add or extend a PlayMode test that clicks through Character Select → Map Select still works (existing `BootThroughLobbyLocalPlayReachesMatchHud` must stay green). Optional: assert Prev/Next or flank click changes `SelectedArchetype` after animation settles (`yield return` ~0.7s).

## Tests

Unity **6000.5.5f1**. Prefer batchmode on **this** worktree path only (main Editor may lock `D:\projects\Game\logiCard`).

```
-runTests -testPlatform EditMode
-runTests -testPlatform PlayMode
```

Do **not** pass `-quit` with `-runTests`. Use `-acceptSoftwareTermsForThisRunOnly` if required. Report pass counts.

## Boundary — do not touch

| Path | Why |
|------|-----|
| `BoardWeatherPocket.cs`, `WeatherPackImportTool.cs`, `Resources/Weather/**` | Atmosphere worktree owns until merge |
| `GameBootstrap.cs`, `RoundPlayback.cs`, `MatchClock.cs`, `BoardSurfaceMaterials.cs`, `BoardReflectionProbes.cs`, `BoardView.cs` | Integrator dirty on main |
| `ProgramHud.cs`, `HudDockHeight` / camera rect | Dock↔camera contract |
| `ModalDialog.cs` | Sibling worktree `logiCard-modal-restyle` |
| `docs/DRAFT_HANDOFF.md`, `SCHEDULE.md`, `PRODUCT_MEMORY.md` | Integrator-only |
| Map Select carousel treatment | C59 — leave flat |

No push, no merge to master, no force-push. Commit on `feat/char-select-motion` only.

## Why safe

Separate directory + file scope. No overlap with atmosphere weather or Integrator rematch/floors/lighting. Modal sibling owns only `ModalDialog` (+ optional additive `UiStyle` tokens — if you need new `UiStyle` colors, **prefix names** like `CharSelectBgScout` so modal branch can add `ModalCardShadow` without clobbering the same lines).

## Report back

- Commit hash(es) on branch
- Files touched
- Test results (or why skipped — Editor lock)
- Deviations from brief
- Screenshot note if you Played (optional)

---

## Worker status

**Done** — Character Select center/flank carousel + `UiMotion` landed on `feat/char-select-motion`. Map Select left as flat `SelectionGrid`. See commit message / parent agent report for hash + test counts.
