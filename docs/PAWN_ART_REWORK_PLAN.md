# Pawn rework: Link's Awakening-style toy figurines via a CC0 asset pack

**Status:** Plan approved by human (2026-08-08), **not yet implemented**. Continue from here on the next
session/machine. Supersedes the primitive-assembly approach in `377029f` — that commit was rejected by
human review before it was even run (wrong shapes, wrong material) and should be reworked, not extended.

## Context

The previous pawn-silhouette attempt (`377029f`) assembled `GameObject.CreatePrimitive` capsules/cubes/spheres
with a matte, grain-textured material. The user rejected it on sight — cube parts + matte grain reads as
"default Unity primitives glued together," the exact look `ART_DIRECTION.md` says to avoid, not
handmade/toy character art.

Confirmed with the user:
- **Target aesthetic**: *The Legend of Zelda: Link's Awakening* (2019 Switch remake) diorama/toy style —
  chunky, rounded, **glossy** toy/felt figures, saturated warm color, chibi-ish proportions. Not matte, not
  grainy, not blocky.
- **Source**: pivot to a free, CC0-licensed, ready-made low-poly character pack instead of hand-tuning
  primitives or writing custom procedural mesh code. This breaks from the project's existing "everything
  procedural, nothing imported" convention (board materials, path line, and Foley audio are all
  code-generated on purpose) — a deliberate, approved exception for character models only. Note this
  explicitly in docs so a future session doesn't treat it as an oversight.
- **Verification**: there is no way to render/screenshot this project from an agent session — no capture
  tooling exists in the repo, and this project's batchmode test runs use `-nographics`, which cannot render
  a frame. The loop is: change code → human hits Play in the Editor and pastes a screenshot into chat →
  agent looks at it and iterates. Do **not** declare this done from passing EditMode/PlayMode tests alone —
  those only prove "doesn't crash," never visual correctness. That gap is exactly what caused this redo.

Researched candidates (both free, CC0/public-domain, Unity-URP-ready, commercially safe for a portfolio ship):
- **Kenney.nl "Blocky Characters"** — one rigged base body, 18 skins/26 animations. Good for color/material
  variety, but a single base mesh means skins alone likely won't give a genuinely different *silhouette* —
  same trap as tinting one shape two colors.
- **Quaternius "Ultimate Modular Men Pack"** — CC0, character split into swappable body-part models
  (head/torso/arms/legs), Unity-optimized FBX/OBJ/glTF. Better structural fit: lets Scout and Juggernaut be
  built from genuinely different proportions/parts out of the same kit, not just different paint.

Neither has been visually inspected yet against the Link's Awakening reference — that's step 1 below, not
something to assume.

## Implementation steps

1. **Source & vet (do this before writing any code).** Fetch preview imagery for the shortlisted packs
   (Kenney Blocky Characters, Quaternius Ultimate Modular Men, and any close alternative that turns up) and
   actually look at them against the Link's Awakening reference — rounded vs. blocky, glossy vs. flat-shaded,
   chibi vs. realistic proportions. Pick final pack(s). Confirm CC0/license text at the source.

2. **Import minimally.** Bring only the specific meshes actually needed (not the whole pack) into a new
   `Assets/_Project/Art/Characters/` folder, mirroring the existing `Assets/_Project/Art/URP/...` layout
   convention. Add a short `Assets/_Project/Art/Characters/THIRD_PARTY.md` recording pack name, source URL,
   license, and date — CC0 needs no attribution, but this repo is a portfolio piece and provenance should
   still be traceable.

3. **Rework `Assets/_Project/Board/PawnView.cs`.** Replace the primitive-assembly code
   (`BuildScout`/`BuildJuggernaut`/`AddPrimitive` from `377029f`) with `Instantiate(prefab, _visual)` for the
   archetype's imported model, still parented under the existing `_visual` root transform — keeps
   `SetHighlighted`'s uniform-scale behavior working unchanged. Keep the `PawnBuild` enum and the same
   `Init(board, color, path, build)` signature/call shape from `GameBootstrap` — only the internals of what
   gets built change.

4. **Team-color tinting.** Apply the attacker/defender team color onto the imported mesh's renderer(s) via
   `MaterialPropertyBlock.SetColor("_BaseColor", ...)` per-renderer (don't mutate/duplicate the imported
   material asset). Push `_Smoothness` up toward a glossy-plastic value (rough starting guess 0.5–0.7, vs.
   the old primitive pawns' 0.18) to chase the Link's Awakening sheen — tune this for real in the screenshot
   loop, don't lock the number in blind.

5. **No animation wiring.** Leave any imported rig/animation clips unused. `RoundPlayback`/`PawnView.ApplyTime`
   already moves the pawn's root transform directly on the stepped 8–12fps cadence — `ART_DIRECTION.md`
   explicitly requires "no root motion." Use the mesh as a static (or single fixed pose) visual; don't wire
   an `Animator` unless asked later.

6. **`GameBootstrap.cs`**: no changes expected — `SpawnPawn(..., PawnBuild build)` call sites for
   Scout/Juggernaut stay as-is; only `PawnView`'s internals change.

7. **Shader safety**: imported FBX materials often default to Built-in RP's Standard shader, which renders
   magenta/pink under URP. Re-hook imported materials onto `Universal Render Pipeline/Lit` (mirror the
   "prefer URP Lit, fall back" logic already established in `PrimitiveMaterialFactory.cs`) rather than
   assuming the import lands URP-ready.

8. **Docs** (same pattern this project already used for the yarn→ink-line pivot, `950b0ac`):
   - `docs/ART_DIRECTION.md`: update the Characters row of the Demo art floor table and the "Digital
     Claymation" pillar language to name Link's Awakening (2019) as the concrete reference, and explicitly
     mark the old "clay-like primitive silhouette" framing as superseded for characters (the board/materials
     clay language elsewhere is untouched — out of scope).
   - `docs/DRAFT_HANDOFF.md`: log the rework, correct/replace the `377029f` entry's framing now that it's
     being redone, and note the new "imported CC0 asset for characters only" exception to the project's
     procedural-only convention.
   - `docs/DAY13_PLAYTEST_FINDINGS.md`: reword the existing Scout/Juggernaut silhouette check item if the
     described technique no longer matches (it currently describes "lean capsule + small head" / "wide
     capsule + blocky head + shoulder pads," which won't exist anymore).

## Verification loop (explicit, staged)

- **Round 1**: get one archetype (Scout) importing and rendering at all. Ask for a screenshot at the actual
  in-game top-down diorama camera angle/distance (not a close-up) — scale and readability at real play
  distance is what matters, not an isolated asset-preview shot.
- **Round 2**: add Juggernaut; confirm the two read as distinct silhouettes at that same distance, not just
  distinct colors.
- **Round 3+**: tune material glossiness, saturation, and proportions against what the screenshots actually
  show, until the human confirms it reads like the reference — not "generic low-poly asset pack."
- Only after explicit human sign-off: run the existing disposable-worktree batchmode EditMode+PlayMode
  pattern (see `docs/DRAFT_HANDOFF.md` Verification section for the exact recipe already used repeatedly this
  project) as a **regression safety net only** — proof nothing broke, not proof it looks right — then commit.

## Risks / open items to carry forward, not resolve now

- Silhouette differentiation depends on what the chosen pack's actual parts look like once inspected in
  step 1 — may need different part combinations than guessed here.
- Keep the imported footprint small (only used FBX + minimal textures) to avoid repo bloat in a
  portfolio-ship repo.
