# Departments — Active Index

**Updated:** 2026-08-11 — post–image-13 pass: closer default framing + softer lighting/floor
(Integrator) parallel with zoom-fill + rain-VFX workers. Capacity **2 of 2**.
**Ops constitution:** [`../PARALLEL_OPS.md`](../PARALLEL_OPS.md) · human-side playbook: [`../DIRECTING_AGENTS.md`](../DIRECTING_AGENTS.md)
**Contracts:** [`../contracts/CURRENT.md`](../contracts/CURRENT.md)
**Human rollup:** [`../DRAFT_HANDOFF.md`](../DRAFT_HANDOFF.md)

## Capacity

Integrator + up to **2** coding workers — **2 of 2 in use.** Check `git worktree list` and
`DRAFT_HANDOFF.md`'s top section before assuming this table is current. (`logiCard-art-pack-research`
below runs in the human's own separate session and isn't counted against this cap.)

## Active agents / worktrees

Human feedback on `screenshots/image copy 13.png`: better than C60, but floor/zoom-in need to be
bigger; lighting + VFX still bad; **no new pack purchase** this wave.

- **Integrator (main `logiCard`, `master`)** — owns `GameBootstrap.ConfigureCamera` (default
  `orthographicSize` closer), `BuildLighting` / `BuildDioramaVolume` (soften harsh shadows / lift
  crushed blacks), `BoardSurfaceMaterials.cs` + baked `(Mat)Floor_URP.mat` (brighter floor), docs.
- **`logiCard-camera-zoom-fill`** (`feat/camera-zoom-fill`) — lower `BoardCameraRig` min zoom +
  retune max/scroll feel + tests. Brief: `CAMERA_ZOOM_FILL_AGENT_BRIEF.md`.
- **`logiCard-rain-vfx-tune`** (`feat/rain-vfx-tune`) — soft-rain retune in `BoardWeatherPocket.cs`
  using already-owned weather packs. Brief: `RAIN_VFX_TUNE_AGENT_BRIEF.md`.
- **`logiCard-art-pack-research`** — human-run; not counted against the 2-worker cap.

## Ownership matrix (write locks)

| Path / concern | Owner now |
|----------------|-----------|
| `GameBootstrap.cs` (`ConfigureCamera` / `BuildLighting` / `BuildDioramaVolume` / practicals) | Integrator |
| `BoardSurfaceMaterials.cs`, `Resources/Interior/Materials/(Mat)Floor_URP.mat`, `BoardView` void colors, `BoardReflectionProbes` clear color sync | Integrator |
| `Assets/_Project/Board/BoardCameraRig.cs` + camera EditMode/PlayMode tests | `logiCard-camera-zoom-fill` |
| `Assets/_Project/Board/BoardWeatherPocket.cs` | `logiCard-rain-vfx-tune` |
| `docs/DRAFT_HANDOFF.md`, `PRODUCT_MEMORY.md`, `contracts/CURRENT.md`, this INDEX | Integrator |
| `docs/departments/<dept>/STATUS.md` | That dept only |

## Cross-review checklist (session start)

- [ ] Read DRAFT_HANDOFF
- [ ] Read this INDEX
- [ ] Read peer STATUS for every **In progress** row above
- [ ] Read contracts/CURRENT
- [ ] Confirm no file overlap before editing
