# Environment art (C53)

Wet-dusk board surfaces + Quaternius interior door/prop meshes (checkpoint 3).

- **Provenance:** [`THIRD_PARTY.md`](THIRD_PARTY.md)
- **Runtime textures:** `Resources/BoardSurfaces/` (loaded by `BoardSurfaceMaterials`)
- **Source texture copies:** `Textures/<polyhaven_asset>/` (original filenames)
- **Interior FBX sources:** `Interior/Source/` (curated Quaternius subset)
- **Interior prefabs:** `Resources/Interior/` (URP Lit, loaded by `BoardView`)

Configure Poly Haven import settings after a fresh clone (normal maps, linear rough):

```
Unity.exe -batchmode -nographics -projectPath <this-worktree> -executeMethod LogiCard.Art.Editor.EnvironmentSurfacesBootstrap.Run -logFile env-surfaces-bootstrap.log
```

Bake interior prefabs from FBX (after adding/changing Source meshes):

```
Unity.exe -batchmode -nographics -projectPath <this-worktree> -executeMethod LogiCard.Art.Editor.InteriorPackImportTool.Run -logFile interior-import.log
```

Menus: **Tools → LogiCard → Configure Environment Surfaces** /
**Import Interior Pack Prefabs**.
