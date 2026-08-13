# Capture checklist — reusable phase-gate template

**Status:** Updated 2026-08-08 (**C46**) — reframed from a one-time Day-14 portfolio artifact into a reusable
template, run at any phase gate that needs proof (`docs/SCHEDULE.md`'s Phase 1 landscape-UI proof, Phase 5 art
bar proof, Phase 6 store trailer). Same shot discipline each time; the "before rolling" checklist and shot
list below are the reusable core.

**Goal:** A cold observer understands Time Card → Program → Playback without narration overload.
**Target length:** 60–90 seconds for a quick capture; longer for a Phase 6 store trailer (note the intended
use at the top of each take log).
**Aspect:** **Landscape, 16:9** (**C48**) — match the shipped Windows window. (Portrait was the pre-pivot
aspect; if a future mobile port ever needs a portrait cut, that's a separate capture pass, not this one.)
**Operator:** One practiced take beats three imperfect ones. Mute mic unless a single short title card is
planned. The match may be captured against a real opponent or the matchmaking-fallback bot
(`AI_FALLBACK_BOT.md`) depending on what's available at capture time — either is fine, the bot is invisible by
design.

---

## Before rolling

- [ ] Build or Editor Play Mode candidate for whatever phase this capture is proving (note which in the take log)
- [ ] Board lighting / Volume on — not the dull pre-diorama state
- [ ] Ink path readable on clay; Time Card + Lock In visible in the HUD dock
- [ ] Audio up (stubs OK) — footstep / shot / Time Card / Lock In distinguishable
- [ ] Rehearsed match script: **at least one of the two doors** (`C45`) opens or closes and clearly changes move **or** LoS once
- [ ] HUD does not cover the only readable action on the board
- [ ] Game view / build locked to landscape 16:9

**Cold-observer test (watch once with sound, no commentary):** can they name Time Card, path authoring, Lock
In, and Move-vs-Shoot in Playback?

---

## Shot list (suggested order)

| # | Seconds | Shot | Must read | Operator note |
|---|---------|------|-----------|---------------|
| 1 | 0–8 | Wide diorama under lamp; dark void outside board | "Handmade miniature," not default Unity | Hold still; no HUD zoom |
| 2 | 8–18 | Time Card allot in the HUD dock → confirm | Cardstock UI; **N** committed from pool | Show presets or slider once; don't linger |
| 3 | 18–35 | Program: click path (ink grows) + stance if visible | Player authoring, not a cutscene | 3–5 waypoints enough; keep the HUD dock in frame |
| 4 | 35–45 | Aim Snap or Hold; a door OPEN/CLOSE once | Shoot + door as verbs | Door beat must be visible on board, not only in UI text |
| 5 | 45–55 | Lock In | Physical-switch moment | One clear click; pause half a beat |
| 6 | 55–75 | Playback scrub or autoplay | Move stepped; shot tracer/muzzle; hit splat; Move ≠ Shoot | If scrubbing, keep playhead readable; don't skip the hit |
| 7 | 75–90 | Aftermath → next Time Card **or** match-over | Loop continues / stakes land | Prefer wound carry into round 2 if time allows |

**Trim rule:** If over the target length, cut lingering on Allot or Aftermath first — never cut the door beat
or Move-vs-Shoot contrast.

---

## Audio bed

- Prefer diegetic foley over music.
- If music is added later, duck under Lock In + shot.
- Silence between beats is fine; do not wall-to-wall talk over the scrubber.

---

## Stills (for README / store page)

Capture from the same build as the video when possible.

- [ ] Hero: full board + HUD landscape frame
- [ ] Path ink close-up on clay board
- [ ] Shoot moment (tracer or muzzle)
- [ ] Door interaction — open gap or closed block readable
- [ ] Time Card + Lock In HUD dock

Drop files under `screenshots/` (or note paths) and link them from `SHIP_README_DRAFT.md`. Phase 6 additionally
needs store-page-ready stills — same list, higher resolution, no debug UI visible.

---

## Export

- [ ] Master: **1920×1080** (or 1080p at native landscape aspect — was `1080×1920` portrait pre-pivot)
- [ ] H.264 (or platform upload spec)
- [ ] Filename: `logicard-<phase-or-purpose>-<length>s.<ext>` (e.g. `logicard-phase1-landscape-60s.mp4`)
- [ ] Take log: date, build/commit, phase this capture is proving, Windows vs Editor
- [ ] Paste video URL + still paths into `SHIP_README_DRAFT.md`

---

## Fail criteria (reshoot)

- Observer cannot tell Move from Shoot
- Board reads as unlit gray default scene
- Neither door ever matters
- HUD occludes the only readable action
- Playback looks like a random cutscene (no scrubber / no authored path visible beforehand)
