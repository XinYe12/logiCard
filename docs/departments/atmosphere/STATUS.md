# Atmosphere — STATUS

**Wave / Day:** Permanent department seat (GDD §11 / PARALLEL_OPS) — **In progress** 2026-08-13
**Branch / worktree:** `feat/atmosphere-stylized` @ `D:\projects\Game\logiCard-atmosphere-stylized`
**Last cross-reviewed:** 2026-08-13 — human Play, seventh round: kneaded lobes read as "a nightmare" — jagged, faceted, shattered-glass — not soft dough (screenshot cleaned up after review, per this lane's pattern)

## Owned files (this seat)

- `BoardWeatherPocket.cs`, `Resources/Weather/**`, `WeatherPackImportTool`, `Tools/gen_soft_cloud_atlas.py`, weather PlayMode smoke, this STATUS

## Verdict (prior pass — kneading shipped broken)

First `KneadClayLobeMesh` pass (`756ad39`, intensity 0.24) was a real miss, not a fine-tuning gap. Diagnosed two compounding bugs from the screenshot rather than guessing at knobs blind:

1. **UV/geometry mismatch (primary cause of the "shattered glass" look).** The posterized `ClaySphereShade` shading is sampled by UV, and UV was left untouched by kneading — still pinned to each vertex's *pre-deform* latitude. But the dents moved vertices well away from the height that latitude implied, so the crown/bright and belly/dark posterized bands landed as scattered light/dark patches across the deformed surface instead of a coherent gradient. A smooth (non-posterized) gradient would have hidden this mismatch as a subtle blur; the flat bands from two passes ago made it glaring.
2. **Dents too strong/sharp for the mesh's resolution**, visibly creasing rather than curving.

## In progress / just landed (this pass, unverified — see Blocked)

1. **UV re-derived post-knead** — `KneadClayLobeMesh` now recomputes each vertex's V (crown/belly band) from its *actual* final height after all dents + rounding, instead of reusing the pre-deform UV. U is left alone (only feeds the shade map's minor edge vignette). This is the structural fix — should hold even if knead strength gets tuned up or down later.
2. **Gentler kneading** — intensity `0.24 → 0.15`, all dent falloff angles widened (less locally sharp), round pass strengthened (3→4 iterations, blend 0.45→0.55) so the mesh comes out smoother/rounder for its resolution.

## Blocked

- Human Re-Play needed — untested since edit (Editor has this worktree open per `Assets/_Recovery` crash-snapshot, so no batchmode this pass either; this is a pure look call same as always). No screenshot yet.

## Offers

- If it's *still* jagged: the UV fix should have removed the dominant cause, so a remaining jagged look would point at real geometric creasing — lower `intensity01` further (currently 0.15) or raise `roundIterations`/blend before suspecting anything else.
- If lobes now read as barely-deformed spheres (over-corrected): raise `intensity01` back up gradually — the UV fix means it's now safe to push this without the shattered-glass risk re-triggering, since that bug is independent of strength.
- If dough character is right but shading looks too flat/uniform on the new curved surface: the posterized band widths in `ClaySphereShade.png`'s generation script may need revisiting now that UV tracks the real geometry.
