# Weather read ↔ Match Shell MapViewport (2026-08-15)

**Wave:** Match Shell Layout — Atmosphere docs deliverable.  
**Master:** `docs/ui/MATCH_SHELL_LAYOUT.md` (MapViewport ~48–55% height; chrome below must not get sky wallpaper).  
**Code:** `BoardWeatherPocket` — **read-only for this wave** (no Sunny land, no Fair-lightning reopen, no ProgramHud edit).

## Recommended framing

1. **Keep the weather pocket board-local.** Clouds / rain / mist / Zap / SunnySun stay scene geometry (or mood lights) parented to the pocket at `board.CenterWorld`, sized to the board footprint. That is the C53 “sky pocket over the chunk,” not a skybox.
2. **Do not depend on full-screen camera clear as the sky.** SolidColor void (or Sunny pale clear) must only paint pixels inside **MapViewport**. Once HandBand + ToolBar + TimelineSchedule cover the bottom ~45%, any full-bleed clear under translucent chrome reads as wallpaper washing the HUD.
3. **Camera letterbox = MapViewport.** Integrator / Camera slice should keep `Camera.main.rect` (or a dedicated map camera) locked to the MapViewport band when UI freezes region geometry — same coupling pattern as today’s `ProgramHud.HudDockHeight` / `TopStripHeight` → `GameBootstrap.ConfigureCamera`, retargeted to the five-band stack.
4. **Mood lighting stays diorama-scoped in intent.** Storm dim / Sunny brighten may still use ambient + directionals + Diorama Volume, but the **visual sky read** (clouds, clear color, rain volume) must remain attached to the map chunk. Do not add a full-screen skybox to “fix” a shorter MapViewport.

## What already matches

| Mechanism | Why it’s MapViewport-safe |
|-----------|---------------------------|
| Pocket `Build(board)` at board center + footprint scale | Weather sits on the diorama; cropping the camera rect still frames the same chunk |
| No skybox / infinite horizon (class contract) | Void + pocket geometry scale with the board, not the window |
| Existing `cam.rect` letterbox in `ConfigureCamera` | Board already does not draw into the HUD dock; shell wave only retunes the fractions |

## `BoardWeatherPocket` assumptions that break / degrade if MapViewport letterboxes (or chrome grows)

| Assumption | Risk when MapViewport shrinks / rect changes | Severity |
|------------|-----------------------------------------------|----------|
| **Sunny sets `Camera.main.backgroundColor`** to pale sky blue | If MainCamera still covers full screen (or a second HUD camera composites wrong), blue clear leaks under Hand/Tool/Timeline. If MainCamera is *not* the MapViewport camera, Sunny clear never reaches the visible map. | **High** — stop treating clear color as full-window sky; bind clear to the map camera only |
| **`Camera.main` / first Camera for capture** | Shell may introduce overlay cameras, UI cameras, or retag MainCamera. Mood restore writes the wrong `backgroundColor`. | **High** when multi-cam |
| **Global `RenderSettings.ambientLight` + all scene `Light`s** | Still OK for the diorama stage; translucent HUD panels can pick up the grade. Opaque chrome hides it. | Medium — prefer opaque Hand/Tool/Timeline backings |
| **URP Volume `ColorAdjustments` on “Diorama Volume”** | If the map camera is the only one with post-processing, fine. If a full-screen camera also runs the volume, HUD chrome gets Sunny punch / Storm crush. | Medium — keep post on map cam only |
| **`WeatherToggleUi` = ScreenSpaceOverlay**, top-right anchored to full portrait | Button floats over InfoBar / chrome, not inside MapViewport; unrelated to letterbox math but clashes with the new shell ownership. | Low for weather read; remove or hand to UI later |
| **Ortho size / BoardCameraRig framed for old dock (~34% + 8% strip)** | MapViewport ~48–55% (vs today’s ~58% middle band) needs a one-time ortho/framing retune so the pocket + board still fill the band — otherwise more void, weaker cloud shelf. | Medium — Camera / Integrator when rect frozen |
| **Rain / haze materials with camera-relative fading** | Smaller pixel rect + different ortho can change soft-rain fade / particle density read. | Low — visual check after rect lands |
| **Lightning / energize world bounds from CloudBank** | Still board-local; breaks only if camera no longer sees the bank (over-zoom / bad rect). | Low if framing retuned |

## Out of scope this wave

- Editing `ProgramHud` / shell bands (UI seat).
- Landing or merging further Sunny-mood code.
- Reopening Fair lightning.
- Implementing MapViewport `cam.rect` (Integrator / Camera when UI freezes geometry).

## Local check (optional)

Code-reviewed against `MATCH_SHELL_LAYOUT.md` + current `ConfigureCamera` / `BoardWeatherPocket` lighting paths. **No PlayMode visual this pass** — wait until UI publishes frozen band fractions, then one Editor glance: Storm bank + rain must sit over the chunk inside MapViewport only; Sunny must not blue the HandBand.
