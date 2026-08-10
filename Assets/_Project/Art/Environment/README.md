# Environment art (C53)

Wet-dusk board surfaces and (pending human pack choice) interior prop meshes.

- **Provenance:** [`THIRD_PARTY.md`](THIRD_PARTY.md)
- **Runtime textures:** `Resources/BoardSurfaces/` (loaded by `BoardSurfaceMaterials`)
- **Source copies:** `Textures/<polyhaven_asset>/` (original filenames)

Configure import settings after a fresh clone (normal maps, linear rough):

```
Unity.exe -batchmode -nographics -projectPath <this-worktree> -executeMethod LogiCard.Art.Editor.EnvironmentSurfacesBootstrap.Run -logFile env-surfaces-bootstrap.log
```

Or menu: **Tools → LogiCard → Configure Environment Surfaces**.
