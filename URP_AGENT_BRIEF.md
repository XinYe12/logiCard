# Brief for the URP / diorama-lighting agent — read this first

You are in an **isolated git worktree** of the logiCard Unity project, on branch `art/urp-foundation`, forked from `master` at commit `7d56765`. The game is mid **continuous-space pivot** (C35/C39 — see `docs/CONTINUOUS_PIVOT_PLAN.md`). The critical-path coding (Phase 2–3 Sim/Net/Timeline retarget) is happening in a **different directory** on dirty `master`. Your job is the **Day 8 art-foundation slice** pulled forward so M3 can start while that pivot continues — SCHEDULE already says “Start Day 8 early” for URP risk.

**Read first:** `docs/ART_DIRECTION.md` (Desk-Lamp Diorama floor), `docs/SCHEDULE.md` Day 8 + M3 cut order, this brief. You do **not** need to implement continuous geometry or touch gameplay code.

## Where everyone else is (do not collide)

| Worktree | Branch | Owns |
|----------|--------|------|
| `D:/projects/Game/logiCard` | `master` (dirty, uncommitted Phase 2+) | `Assets/_Project/Sim/`, `Net/`, `Timeline/PawnProgram.cs`, their EditMode tests |
| `D:/projects/Game/logiCard-continuous-phase1` | `continuous/phase1-geometry` | New Phase 1 geometry primitives + their new EditMode tests |
| `D:/projects/Game/logiCard-verify` | `verify/day5-6-tests` | **Parked** — do not merge or touch |
| **You:** `D:/projects/Game/logiCard-urp-foundation` | `art/urp-foundation` | URP + lighting lab + clay material palette only |

Unity can only lock **one Editor instance per project path**. Opening this worktree is fine while another agent has a *different* path open. Do not open `logiCard` (main) or `logiCard-continuous-phase1` from this session.

## Your job — Day 8 foundation only

Project is currently **Built-in RP** (`Packages/manifest.json` has no URP; `ProjectSettings/GraphicsSettings.asset` has `m_CustomRenderPipeline: {fileID: 0}`). Migrate to **URP** and prove the Desk-Lamp look in a **throwaway lab scene** — do **not** rebuild the playable Bootstrap loop.

### 1. Add Universal Render Pipeline (Unity 6 / Editor `6000.5.5f1`)

- Add `com.unity.render-pipelines.universal` via Package Manager (version compatible with Editor 6000.5.5f1 — let Unity resolve; do not pin a random old URP).
- Create pipeline assets under **`Assets/_Project/Art/URP/`** (new folder), e.g.:
  - `LogiCardURP.asset` (Universal Render Pipeline Asset)
  - `LogiCardURP_Renderer.asset` (Forward Renderer)
- Assign the pipeline in **Graphics** and **Quality** project settings so the project actually *uses* URP (not just package-installed).
- Keep settings modest: Windows demo first; no chasing high-end shadows/DoF/SSS. Optional DoF/SSS stay **out of scope** (C34 cut order).

### 2. Lighting lab scene (proof of Day 8 DoD)

Create **`Assets/_Project/Scenes/LightingLab.unity`** (new — do **not** edit `Bootstrap.unity`).

Minimum contents:

- A simple **diorama base** (flat board-ish plane or box) sized roughly to the demo footprint feel (~`[0,4]×[0,4]` world units is fine — same magnitudes as GDD / pivot Decision 7).
- **Dark void** outside the board (clear camera background / surrounding unlit dark — not an infinite lit horizon).
- **Warm desk-lamp key** (point or spot, warm color temperature / orange-amber) + **soft fill** so silhouettes stay readable.
- A few placeholder clay-tint cubes/spheres on the board so materials + lighting read together.
- Portrait-friendly camera framing (tall framing / high angle over the board — **C30** portrait language; no need to wire Android lock here).

Exit criterion for this step: a cold look at the Game view says “warm miniature under a lamp,” not “default Unity grey skybox.”

### 3. Clay-tint material palette

Under **`Assets/_Project/Art/Materials/`**, author a small set of **matte / polymer-clay** URP materials (URP Lit or Simple Lit — prefer matte, low smoothness, no chrome PBR). Suggested slots (names flexible):

- Board / plywood base
- Warm clay (pawn-ish)
- Cool clay (second silhouette)
- Path yarn / chalk (muted fiber or chalk — **not** neon)
- Void / unlit black (if useful)

Subtle procedural noise on albedo is nice if cheap; **true SSS and thumbprint normals are optional — skip them** unless free.

### 4. Minimal runtime material compatibility (optional but preferred)

`Board/PrimitiveMaterialFactory.cs` clones the Built-in primitive default material. Under URP that often pinks out. You **may** edit **only** that one Board file so `Tinted(Color)` uses a URP Lit/Simple Lit (or Unlit) shader with low smoothness — same public API, no callers change.

**Do not** edit `BoardView`, `BoardInputController`, `PathPreviewView`, `PawnView`, `TileMarker`, `ShotTracerView`, or anything under `Boot/` / `UI/`. Wiring the palette into the real match scene is **Phase 4 / later Day 8–9** after continuous views land.

### 5. Short note in-tree

Add **`Assets/_Project/Art/README.md`**: what you created, how to open LightingLab, pipeline asset paths, and any gotchas (e.g. “Bootstrap may still look wrong until Phase 4 rewires BoardView mats”). Do **not** edit `docs/DRAFT_HANDOFF.md`, `docs/PRODUCT_MEMORY.md`, or `docs/GDD.md` — those are live in the main worktree.

## Tests / verification

There is no EditMode suite for this slice. Verify by:

1. Open **this** project path in Editor `6000.5.5f1` (first import will rebuild `Library/` — expect a long wait).
2. Confirm Graphics/Quality point at your URP asset; no pink pipeline errors in Console on empty/lab scene.
3. Open `LightingLab.unity` — lighting + clay mats match ART_DIRECTION Day 8 floor (“Board reads as desk diorama under lamp light”).
4. Optional sanity: batchmode project open should not fail:
   ```
   "D:\Unity\Editor\6000.5.5f1\Editor\Unity.exe" -batchmode -nographics -projectPath "D:\projects\Game\logiCard-urp-foundation" -quit -logFile "D:\projects\Game\logiCard-urp-foundation\urp-open.log"
   ```
   (Path may differ on this machine — check `ProjectSettings/ProjectVersion.txt` = `6000.5.5f1` and locate that Editor.)

Do **not** run or rewrite EditMode/PlayMode gameplay tests — those belong to the pivot agents, and the main tree’s tests are mid-retarget.

## Boundary — what NOT to touch, and why

**In scope (create/edit):**

- `Packages/manifest.json` / packages-lock (URP only)
- `ProjectSettings/` only as needed for URP assignment (Graphics, Quality, maybe URP global settings asset under `Assets/_Project/Art/URP/`)
- **New** `Assets/_Project/Art/**`
- **New** `Assets/_Project/Scenes/LightingLab.unity` (+ `.meta`)
- Optionally **only** `Assets/_Project/Board/PrimitiveMaterialFactory.cs`

**Do not:**

- Edit `Assets/_Project/Sim/**`, `Net/**`, `Timeline/**`, `Boot/**`, `UI/**`, `Characters/**`, `Cards/**`
- Edit any other `Board/**` file besides the optional `PrimitiveMaterialFactory.cs`
- Edit `Assets/_Project/Scenes/Bootstrap.unity` (just fixed elsewhere; Phase 4 rebuilds composition root)
- Edit or add under `Assets/_Project/Tests/**`
- Edit `docs/DRAFT_HANDOFF.md`, `docs/PRODUCT_MEMORY.md`, `docs/GDD.md`, `.cursor/rules/**`
- Push, force-push, or merge into `master`
- Touch other worktrees at all
- Start yarn-path mesh work, Time Card cardstock UI, clay pawn meshes, muzzle VFX, or audio — those are Day 9–11 and need the continuous Board/UI surface

**Why:** Phase 2–3 in the main tree are rewriting Sim/Net/Timeline and their EditMode tests. Phase 1 owns new Sim geometry files in another worktree. Phase 4 will retarget `BoardView` / `GameBootstrap` / `RoundPlayback` onto `ArenaBoard` — if you rewrite those now against the old grid API, your work is deleted or conflicted. URP + a lab scene is deliberately **ahead of** that surface.

## Why this split is safe

Separate worktree ⇒ separate working tree and separate Unity `Library/` (gitignored; first open reimports). Your file set does not overlap Phase 1’s new Sim files, the main agent’s Sim/Net/Timeline edits, or the parked verify branch. The only intentional shared surface is optional `PrimitiveMaterialFactory.cs`, which the continuous pivot lists as a **zero-change** file for coordinate retarget — a URP shader swap there is orthogonal and merges cleanly.

## When you're done

Report:

1. URP package version resolved, pipeline asset paths, Graphics/Quality assignment confirmed.
2. LightingLab contents + whether the warm-lamp / void / clay read landed.
3. Whether you touched `PrimitiveMaterialFactory.cs` (diff summary).
4. Any pink materials, shader errors, or Unity 6 URP surprises.
5. Commit locally on `art/urp-foundation` with clear messages. **Do not merge** — the user reconciles branches by hand after the pivot reaches a commit checkpoint.
