# Atmosphere — STATUS

**Wave / Day:** Permanent department seat (GDD §11 / PARALLEL_OPS) — **In progress** 2026-08-13
**Branch / worktree:** `feat/atmosphere-stylized` @ `D:\projects\Game\logiCard-atmosphere-stylized`
**Last cross-reviewed:** 2026-08-13 — human Play, fourth `image copy 15` re-take: "clouds need to be more glued together, higher, still needs to be higher... blur the model so it doesn't look like a big white ball" (screenshot cleaned up after review, per this lane's pattern)

## Owned files (this seat)

- `BoardWeatherPocket.cs`, `Resources/Weather/**`, `WeatherPackImportTool`, `Tools/gen_soft_cloud_atlas.py`, weather PlayMode smoke, this STATUS

## Verdict (prior pass — mixed)

7-mass spread and denser haze read fine in the screenshot, but exposed the real problem underneath: every one of the 6 hand-authored lobe patterns (Raft/Stack/Comma/Crown/Anvil/Drift, all the way back to the original clay pivot) had one dominant lobe (RadiusNorm up to 0.42) against several much smaller ones — so each mass, however small or numerous, still read as "a big sphere with pimples," not a cloud. Human named this directly this round. Also: masses weren't overlapping enough to look glued, and height (raised once already, `755fb21`) is still not enough.

## In progress / just landed (this pass, unverified — see Blocked)

1. **No more "ball" lobes** — replaced all 6 hand-authored patterns with `GenerateCloudCluster`: a sunflower/golden-angle disk fill, 9-12 lobes per mass, radius band narrowed to 0.19-0.25 (was up to 0.42) so no single lobe dominates. Dome-biased Y keeps a rounded-on-top silhouette without a flat disk of same-height balls. Called fresh (`Random`) per mass per `Build()`, so the old fixed pattern pool is gone — variety now comes from randomness + each mass's own aspect ratio, not named shapes.
2. **More glued together** — tightened `CloudMasses` X spacing so adjacent masses' `ScaleXFactor` half-widths deliberately overlap (was leaving the NW-Main pair with a bare gap) instead of just touching.
3. **Higher** — `HeightUnits` raised ~45% again (was 3.75-4.3, now 5.4-6.1) after `755fb21`'s smaller bump wasn't enough. Rain emit height bumped to match (`2.85` → `4.6`, still "just under the shelf").

## Blocked

- Human Re-Play needed — untested since edit (Editor has this worktree open per `Assets/_Recovery` crash-snapshot, so no batchmode this pass either; this is a pure look call same as always). No screenshot yet.

## Offers

- If height is *still* not enough after this: the height fix has been incremental twice now and missed both times — worth asking whether clouds should scale with `BoardCameraRig.OrthographicSize` (the zoom level) instead of a fixed world height, since a human playing more zoomed-in would see any fixed-height cloud cover more of the frame. That's a bigger change (weather has never referenced the camera) — flag before implementing blind a third time.
- If lobes still read spherical: narrow `GenerateCloudCluster`'s radius band further (currently 0.19-0.25) or raise lobe count above 12.
