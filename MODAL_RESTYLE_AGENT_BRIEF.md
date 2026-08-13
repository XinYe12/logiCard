# ModalDialog Cardstock Restyle — Agent Brief

**Worktree:** `D:\projects\Game\logiCard-modal-restyle`  
**Branch:** `feat/modal-restyle` @ `498d463`  
**Why:** `docs/UI_TOOLS_RESEARCH.md` stub 5 — Quit/confirm modal still reads flat grey Unity default. ART_DIRECTION wants cardstock-on-dimmer. Human iterating atmosphere elsewhere; Integrator dirty on rematch/floors.

## The job

1. Read `docs/ART_DIRECTION.md` (Desk-Lamp / cardstock language) and `docs/UI_TOOLS_RESEARCH.md` §5 stub 5.
2. Restyle `Assets/_Project/UI/ModalDialog.cs` so the dialog reads as **warm cardstock on a deep dimmer**, not flat dark grey:
   - Stronger dimmer (deeper / slightly warmer black).
   - Card: warmer paper/ink tone via `UiStyle` (add tokens if needed — e.g. `ModalCard`, `ModalCardBorder`, `ModalShadow`). Prefer additive new fields; avoid renaming existing `Card` if Character Select sibling might rely on it — or keep `Card` and only change modal call sites to new tokens.
   - Soft shadow under the card (second slightly larger darker panel behind, or offset duplicate) — procedural only, **no new asset packs / Kenney download**.
   - Keep rounded 9-slice via `UiStyle.RoundSprite`.
   - Divider / primary / secondary remain readable; primary stays high-contrast confirm.
3. **Preserve hit-target names for tests:** root `ModalDialog`, buttons `ModalPrimary`, `ModalSecondary`. Layout anchors may tighten but clicks must still work. Do not change `Show(...)` signature unless unavoidable (prefer keep signature).
4. Do **not** restyle Character Select, Map Select, or `ProgramHud`.
5. Existing PlayMode `MatchEndQuitOpensConfirmDialogBeforeLeaving` must stay green. Optional EditMode smoke if you add pure helpers.

## Tests

Unity **6000.5.5f1**. Batchmode on **this** worktree only:

```
-runTests -testPlatform EditMode
-runTests -testPlatform PlayMode
```

No `-quit` with `-runTests`. `-acceptSoftwareTermsForThisRunOnly` if needed.

## Boundary — do not touch

| Path | Why |
|------|-----|
| Weather / `BoardWeatherPocket` / `Resources/Weather/**` | Atmosphere worktree |
| `GameBootstrap`, `RoundPlayback`, `MatchClock`, board surface/probe/view | Integrator dirty |
| `AppFlowController.BuildCharacterSelect`, new `CharacterSelectView`, `UiMotion` | Sibling `logiCard-char-select-motion` |
| `ProgramHud`, dock height, camera | Dock contract |
| `docs/DRAFT_HANDOFF.md` etc. | Integrator-only |

If you add `UiStyle` tokens, use **modal-prefixed** names (`ModalCard`, `ModalDimmer` already exists — retune carefully) so char-select can add `CharSelect*` tokens without merge pain.

No push / merge / force-push. Commit on `feat/modal-restyle` only.

## Why safe

Separate worktree; file ownership is `ModalDialog.cs` + additive `UiStyle` modal tokens only.

## Report back

- Commit hash(es)
- Files touched
- Test results
- Deviations
