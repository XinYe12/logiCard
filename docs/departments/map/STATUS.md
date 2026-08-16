# Map — STATUS

**Wave / Day:** Map Phase 2 (C65) — **Merged to master** (`a76f006`), 2026-08-14.
**Branch / worktree:** `logiCard-map` / `dept/map` @ `565583f` (merged tip).
**Last cross-reviewed:** human Play look signed (`screenshots/image copy 15.png`).

## Scope (per seat brief)

- **Owns:** map/room/floor **presentation** — materials, prop dressing, per-`MapSurfaceRole` visual language.
  `BoardView.cs`, `BoardSurfaceMaterials.cs`.
- **Does not own:** `MapDefinitions` room authorship, `GameBootstrap` geometry/lighting, Sim/Door API,
  weather (Atmosphere), HUD (UI).

## Done

- Phase 1 docs + §4 YES → **C65**; Phase 2 contract implemented and merged.
- Flat/`Solid()` floors; Gradient*_URP / Solid door+prop tint; map-aware dressing (3 maps); toy fence walls.
- EditMode `BoardSurfaceMaterialsTests`; human look OK.
- **Self-directed look-check pass (2026-08-15)** — Phase 2 checklist item 5 groundwork. Diagnostic
  capture produced `screenshots/lookcheck/{FreightYard,RailPlatform,VaultComplex}.png`; throwaway
  `MapLookCheckCapture.cs` + scratch logs removed. Judged against `MAP_PRESENTATION_STANDARD.md` §2
  (not `ART_DIRECTION.md` Moodboard — that photo is stale C53 photoreal and contradicts C65; mismatch
  flagged in §5 of the standard). **Not a final human sign-off** — candidate tweaks below await human.

### Look-check verdict vs §2 (candidate; human decides)

| §2 row | Verdict | Notes |
|---|---|---|
| Room floors | **Pass** | Solid flat albedos read as Yard sand / Hall warm wood / Vault blue / Flank green — no photo PBR diffuse on stage. |
| Walls | **Pass (family)** | Cream panel + honey rail/post fence is flat/Solid family; matches toy-floor palette. |
| Door / prop tint | **Pass** | Blue chairs, brown desks/cabinets, Gradient/Solid body+accent — no muted HDRP floor bake on stage. |
| Void dressing | **Pass (reference)** | Cartoon_City lamp + bin still the right flat family at the edge. |
| Wet reflections as goal | **Pass** | Floors no longer tuned around wetness; soft lamp pools are lighting, not material wetness. |

**Overall:** material-family swap landed as C65 intended. Remaining issues are presentation bugs / chroma
tuning / lighting grade — not a revert-to-photo-PBR problem.

### Candidate tweaks (for human review — do not auto-apply)

1. ~~**Fence soft-shadow “black burst” (Map-owned, high priority).**~~ **Fixed 2026-08-16** — see
   "Done — fence shadow-acne fix" below. Still awaiting human visual sign-off.
2. **Yard vs Hall chroma separation (Map-owned, low).** Yard `(0.94, 0.78, 0.48)` and Hall
   `(0.90, 0.68, 0.42)` are too close under warm lamps — rooms don’t read as distinct roles at a glance.
   **Candidate:** push Hall cooler/darker or more orange; leave Yard sandy; keep Vault blue + Flank green
   (those already separate well).
3. **Vault floor smoothness (Map-owned, low).** Vault `0.22` is the shiniest Solid floor — under point
   lamps it pools more than Yard/Flank. **Candidate:** drop toward `0.12–0.14` to match siblings.
4. **Warm lamp radial wash (Integrator-owned, already flagged).** Soft point-light gradients on flat
   floors fight the “painted miniature” read. Map materials are correct; this is still the optional
   `BuildLighting` / `BuildDioramaVolume` re-pass after C65 — not a Map material regress.
5. **ART_DIRECTION Moodboard vs C65 (doc, Integrator/core).** §5 of `MAP_PRESENTATION_STANDARD.md`
   now says: judge surfaces against §2, not the C53 photoreal moodboard image. Someone should amend
   `ART_DIRECTION.md` Moodboard so board *surfaces* cite C65 flat/toon (geometry/weather bar can stay).

## Done — fence shadow-acne fix (2026-08-16)

Candidate tweak #1 above, green-lit and landed. `PlaceFencePart` (`Assets/_Project/Board/BoardView.cs`)
gained an optional `castShadows` parameter (default `true`, so all other/future callers are unaffected).
`PlaceWallFence` now passes `castShadows: false` for the `Panel` and both `Rail` parts (`Rail_Top`,
`Rail_Mid`) — these are the thin, overlapping fence cubes whose self/cross-shadowing produced the
jet-black "burst" acne at dense wall junctions seen in all three 2026-08-15 look-check captures. Posts
(`Post_A`, `Post_B`, `Post_M*`) were left at the default (still cast shadows) — they're the thick
members, weren't visually implicated in the acne, and the brief called for leaving them alone absent
evidence otherwise. No material colors, smoothness values, or other renderer flags were touched
(candidate #2 chroma / #3 smoothness stay untouched and still await a separate human art-direction call).

Fresh look-check screenshots were **not** recaptured. The 2026-08-15 diagnostic tool
(`MapLookCheckCapture.cs`) was throwaway and, per `git log` on this branch, was never actually
committed to `dept/map` — it exists only as a description in this STATUS doc, not as a reachable
commit, so there's no exact pattern left to reuse. Reconstructing it from scratch was judged more
effort than this one-line fix warrants, so it was skipped rather than inventing a fragile substitute.
**A human should eyeball this in-Editor or in-Play before the acne is called fixed** — shadow artifacts
are inherently a visual call that batchmode green cannot validate.

Verified: independent batchmode run on this branch post-fix — EditMode 158/158 passed, PlayMode 49/49
passed, 0 failed, 0 inconclusive, 0 skipped.

## In progress

- None — seat idle pending human visual sign-off on the shadow-acne fix above, and pending human
  decision on the remaining candidate tweaks (#2 Yard/Hall chroma, #3 Vault smoothness — untouched,
  still awaiting a separate human art-direction call).

## Done — Match Shell Layout docs recommendation (2026-08-15)

Per `MATCH_SHELL_LAYOUT_AGENT_BRIEF.md` (Integrator, Match Shell Layout wave): added
`MAP_PRESENTATION_STANDARD.md` §6 — camera-framing recommendation for MapViewport as a ~48–55%-height
center rect (not full window). Docs only, no code touched.

- Recommend re-deriving `orthographicSize` default + `BoardCameraRig` min/max zoom against the new,
  shorter `cam.rect` height (current 3.4 / [2.6, 8.0] were calibrated for a taller region); priority
  order when trading zoom for fit: doors > flank sightline > floor edge. Rail Platform (depth 13) is
  the tall-map case to human-check first.
- Audited Map-owned dressing (`BoardView`, `BoardSurfaceMaterials`) — all board-local world space, no
  full-bleed assumption found. Flagged two crop risks for Camera/Atmosphere (not Map fixes): storm
  cloud pocket (`BoardWeatherPocket`) and void-edge props can get clipped by the shorter rect at
  default zoom, since `cam.rect` crops rather than rescales.
- Restated the "no card-battlefield layer on the map" reject from `MATCH_SHELL_LAYOUT.md` in map-doc
  terms.

Not implemented: no edits to `ProgramHud`, `BoardCameraRig`, or `GameBootstrap.ConfigureCamera` — per
brief, Camera slice / Integrator own that code.

## Blocked / follow-ups (not Map-owned)

- Optional Integrator lighting/`BuildDioramaVolume` re-pass (human already likes Play look).
- ART_DIRECTION Moodboard text still cites C53 photoreal for materials — contradicts C65 (flagged).

## Verification

- **Independent batchmode run on this branch (`dept/map` @ `565583f`), 2026-08-15:** EditMode 158/158 passed, PlayMode 49/49 passed, 0 failed, no compile errors. Confirmed green — first independent run on this branch.
- **Independent batchmode run post fence-shadow fix, 2026-08-16:** EditMode 158/158 passed, PlayMode
  49/49 passed, 0 failed, 0 inconclusive, 0 skipped. Confirmed green.
- Look-check PNGs kept at `screenshots/lookcheck/` for human review; diagnostic test deleted after use.
  Not refreshed for the shadow fix — see "Done — fence shadow-acne fix" above for why.

## Offers

- Fence shadow-acne fix (#1) is landed on this branch, pending human visual sign-off. If human
  green-lights #2–#3 (Yard/Hall chroma, Vault smoothness), Map can land those in a small follow-up on
  this worktree. Otherwise park this seat.
