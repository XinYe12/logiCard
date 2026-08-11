# Nappin Interior Re-Sourcing — Agent Brief

**Worktree:** `logiCard-nappin-interior-wiring` (branch `feat/nappin-interior-wiring`, based on `master` @ `971985c`)
**Scope:** Real code + asset work. Editor batchmode required to verify. This worktree has its own copy
of the project — Unity batchmode here does **not** conflict with the main tree's Editor lock.

## Known project-wide blocker — read this first

`Assets/ExplosiveLLC/SuperCharacterController/` is **not present in this worktree** (deliberately
excluded from the checkpoint commit this worktree forked from) because it has real C# compile errors
(missing `TerrainCollider`/Terrain Physics module reference, invalid `SerializeField` usage on a
non-field) that abort Unity batchmode entirely. You should not encounter this here — if you somehow do
(e.g. a stray file), stop and report rather than trying to fix it; it's out of scope for this brief.

## Why this task exists

Read `docs/ART_PACK_RESEARCH.md` in full — it's the running research/decision log this brief is scoped
from. Short version: this project pivoted its interior art direction to nappin's free "soft clay /
curvy-minimal" pack family. `Assets/nappin/OfficeEssentialsPack/` (Office Essentials, ~54 prefabs) is
already the locked style anchor and already in the project (Built-in Standard shader — an URP upgrade
package exists but its import status is unconfirmed, don't assume it's landed, check for yourself: any
`.mat` file under `Assets/nappin/OfficeEssentialsPack/Materials/` whose `m_Shader` still points at
`{fileID: 46, guid: 0000000000000000f000000000000000, type: 0}` is still Built-in Standard, not URP).
Two new nappin packs just landed in this same checkpoint: `Assets/nappin/HouseInteriorPack/` and
`Assets/nappin/WeaponStylizedPack/` (the latter is irrelevant to this brief — skip it, it's for
Shoot-readability prop dressing, a separate future task).

**This project already did the exact re-sourcing pattern you need once before, on a different source
pack.** `Assets/_Project/Art/Editor/InteriorPackImportTool.cs` currently sources from the original
**Quaternius CC0 FBX pack** (`Assets/_Project/Art/Environment/Interior/Source/*.fbx`) and bakes 14 named
prefabs into `Assets/_Project/Art/Environment/Resources/Interior/`. Earlier this session it was
temporarily re-pointed at a different pack (Synty PolygonOffice) using a **prefab-instantiation** pattern
instead of the FBX-import pattern, then reverted only because that source pack turned out to be
unlicensed — not because the pattern was wrong. **Look at that reverted code for the shape to follow**:
`git show ee95328:Assets/_Project/Art/Editor/InteriorPackImportTool.cs` (from the main repo, not this
worktree) shows a `Catalog` of `(sourcePrefabPaths[], outputName, normalizeDoor)` tuples, a
`GetOrCreateConvertedMaterial` step that **duplicates a source material into `Resources/Interior/Materials/`
before URP-converting it rather than mutating the original pack's asset** (nappin's materials are shared
by other things that might use this pack later — same reasoning applies here), and a
`FixGlassTransparency`-style step for anything transparent. Reuse this shape; don't reinvent it.

## The existing pipeline's hard contract — don't break this

- Output prefabs must land at `Assets/_Project/Art/Environment/Resources/Interior/<name>.prefab` for
  these exact 14 names: `Door`, `DoorAlt`, `DoorDouble`, `WindowSmall`, `WindowLarge`, `LightCeiling`,
  `LightCeilingAlt`, `LightDesk`, `ShelfLarge`, `Shelf`, `Bookshelf`, `Cabinet`, `Table`, `Chair`.
- `BoardView.cs` loads these by name (`Resources.Load<GameObject>("Interior/" + resourceName)`,
  `Resources.Load<GameObject>("Interior/Door")` for the wall-fitted door leaf specifically) — **if you
  keep this exact output contract, `BoardView.cs` needs zero changes.**
- Door prefabs need pivot at bottom-center, unit-scaled (width≈1, height≈1) — reuse
  `NormalizeDoorPivotAndScale`/`NormalizePropPivot`/`CalculateLocalBounds` from the current file
  unchanged, they're pack-agnostic geometry math, not Quaternius-specific.

## What's actually in the nappin packs — confirmed by direct inspection, not guessed

`Assets/nappin/OfficeEssentialsPack/Prefabs/` has confirmed matches for some of the 14: `(Prb)Door.prefab`,
`(Prb)SeparatorDoor.prefab` (a second door style — good `DoorAlt` candidate), `(Prb)Window.prefab`,
`(Prb)Shelf1.prefab`. **No confirmed match for `DoorDouble`, `LightCeiling`/`LightCeilingAlt`/`LightDesk`,
`ShelfLarge`, `Bookshelf`, `Cabinet`, `Table`, or `Chair`** in a first pass — do a real targeted search
through the full `Prefabs/` folder (it has ~50+ items) before concluding something's missing; don't stop
at the first grep.

**`Assets/nappin/HouseInteriorPack/` is unusual — check this before assuming it adds new meshes.** It
contains only a single demo scene (`HouseInteriorPack.unity`, ~177KB) and a readme — **no separate
Models/Materials/Prefabs folders of its own.** That strongly suggests it's a room-layout demo scene built
entirely from prefabs that already live in `OfficeEssentialsPack` (same publisher, shared asset library),
not a source of genuinely new meshes. **Open the scene (or read its serialized `.unity` YAML directly)
and check what prefabs it actually references** — if they all resolve back into `OfficeEssentialsPack/`,
this pack doesn't expand your catalog at all and you should say so plainly rather than assume it does.

**Gaps found in a prior research pass, likely still real:** no dedicated cabinet, only one door style, one
window. If a name has no reasonable match anywhere in nappin's actual content, it's fine to leave that
single catalog entry pointed at its old Quaternius source rather than force a bad substitute — note which
ones (if any) and why, same as the interior-wiring precedent set.

## The job

1. Confirm nappin's material shader situation for yourself (see "hard contract" section above) — this
   determines whether your material-conversion step needs to run the full Standard→URP/Lit swap (like
   the Quaternius/reverted-PolygonOffice tools did) or just needs light verification if URP import already landed.
2. Search `OfficeEssentialsPack/Prefabs/` (and check what `HouseInteriorPack.unity` actually references)
   for real matches to each of the 14 names.
3. Adapt `InteriorPackImportTool.cs` (or write a sibling tool, your call) following the `ee95328` shape:
   catalog of source paths → output name, duplicate-then-convert materials (never mutate nappin's
   originals in place), reuse the existing pivot/scale normalization, bake to the fixed output paths.
4. If any prefab has glass/transparent parts, verify the resulting material's serialized properties
   directly after conversion (`_Surface`, `_SrcBlend`/`_DstBlend`, `_ZWrite`, keyword list) — don't trust
   a clean batchmode log alone. This project has hit the wrong-transparency-keyword bug twice already on
   two different shaders; check the actual `.mat` file this time before calling it done.
5. Run full batchmode EditMode + PlayMode in **this worktree**, confirm no regression against the last
   known-good baseline (EditMode 124/124, PlayMode 37/37). Check `docs/` for this project's batchmode
   command pattern first — known gotcha: never pass `-quit` together with `-runTests`.

## Deliverables

Commit on `feat/nappin-interior-wiring`. **Do not merge or push.** Report back: which of the 14 names got
re-sourced from nappin vs. left on Quaternius (and why for any left behind), whether `HouseInteriorPack`
turned out to reference new content or just reuse `OfficeEssentialsPack`, confirmation of transparent-part
material correctness if any exist, and batchmode pass/fail counts.

## Boundary

- Don't touch `docs/DRAFT_HANDOFF.md`, `docs/SCHEDULE.md`, `docs/contracts/CURRENT.md`,
  `docs/departments/INDEX.md`, `docs/ART_PACK_RESEARCH.md` — Integrator-only.
- Don't touch `Assets/RainSnowCloudEffect/`, `Assets/Vefects/`, `Assets/_Project/Board/BoardWeatherPocket.cs`
  — a sibling slice this wave (weather/lightning wiring), different worktree, different worker.
- Don't touch `Assets/ithappy/**`, `Assets/_Project/Editor/PawnImportTool.cs`,
  `Assets/_Project/Board/PawnView.cs`, `Assets/_Project/Art/Characters/**` — characters wiring is
  separately scoped, not this brief.
- Don't touch `Assets/nappin/OfficeEssentialsPack/**` or `Assets/nappin/HouseInteriorPack/**` source
  content itself — read-only reference, same reasoning as the PolygonOffice precedent (shared by
  anything else that might use this pack later). Only create/modify files under
  `Assets/_Project/Art/Editor/` and `Assets/_Project/Art/Environment/Resources/Interior/`.
- Don't touch `GameBootstrap.cs`'s map dispatch switches, `Sim/`, `Net/`, `Timeline/`.
- `BoardView.cs` should need **zero** changes if you preserve the output contract — if you find yourself
  needing to change it, stop and explain why in your report rather than guessing at a wider change.
