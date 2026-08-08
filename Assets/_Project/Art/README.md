# Art foundation (Day 8 early — URP)

Desk-Lamp Diorama render foundation, isolated from the continuous-space pivot.

## Open the look-dev scene

1. Open this worktree in Unity **6000.5.5f1**.
2. Open `Assets/_Project/Scenes/LightingLab.unity`.
3. Enter Game view — expect warm spot key + soft fill over a plywood base in a dark void, with clay-tint placeholders and a **线稿涂鸦** / ink-path sample (path direction amended 2026-08-07; sample mat may still be named `Mat_PathYarn`).

## Pipeline assets

| Asset | Path |
|-------|------|
| URP Asset | `Assets/_Project/Art/URP/LogiCardURP.asset` |
| Forward Renderer | `Assets/_Project/Art/URP/LogiCardURP_Renderer.asset` |

Assigned as the project default render pipeline (Graphics + Quality).

Package: `com.unity.render-pipelines.universal` **17.5.0** (Unity 6000.5.5f1 builtin).

## Material palette

Under `Assets/_Project/Art/Materials/`:

- `Mat_BoardPlywood` — warm wood-ish board
- `Mat_ClayWarm` / `Mat_ClayCool` — matte polymer pawns
- `Mat_PathYarn` — legacy asset name; use as a **matte ink / 线稿涂鸦** sample (not neon). Path direction amended 2026-08-07 (ART_DIRECTION) — prefer thin sketchy ink on board over yarn cloth.
- `Mat_VoidBlack` — unlit void

## Gotchas

- **`Bootstrap.unity` is intentionally untouched.** It may look incomplete until Phase 4 rewires `BoardView` onto continuous geometry and authored mats. Runtime primitives should still tint via `PrimitiveMaterialFactory` (URP Lit).
- Re-run the one-shot bootstrap if assets are missing:
  ```
  Unity.exe -batchmode -nographics -projectPath <this-worktree> -executeMethod LogiCard.Art.Editor.UrpFoundationBootstrap.Run -logFile urp-bootstrap.log
  ```
- Do not merge this branch into `master` until the continuous pivot reaches a commit checkpoint — the user reconciles by hand.
