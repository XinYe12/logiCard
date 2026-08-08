# Capture checklist — 60–90s portfolio video

**Goal:** A cold observer understands Time Card → Program → Playback without narration overload.  
**Target length:** 60–90 seconds.  
**Aspect:** Portrait (C30) or letterboxed portrait in a landscape frame — match the shipped Windows window.  
**Operator:** One practiced take beats three imperfect ones. Mute mic unless a single short title card is planned.

---

## Before rolling

- [ ] Day 12+ **Windows** candidate (Editor Play Mode only if build is not ready — note which in the take log)  
- [ ] Board lighting / Volume on — not the dull pre-diorama state  
- [ ] Ink path readable on clay; Time Card + Lock In visible in bottom thumb zone  
- [ ] Audio up (stubs OK) — footstep / shot / Time Card / Lock In distinguishable  
- [ ] Rehearsed match script: **one door** opens or closes and clearly changes move **or** LoS once  
- [ ] HUD does not cover the only readable action on the board  
- [ ] Game view / build locked to portrait aspect  

**Cold-observer test (watch once with sound, no commentary):** can they name Time Card, path authoring, Lock In, and Move-vs-Shoot in Playback?

---

## Shot list (suggested order)

| # | Seconds | Shot | Must read | Operator note |
|---|---------|------|-----------|---------------|
| 1 | 0–8 | Wide diorama under lamp; dark void outside board | “Handmade miniature,” not default Unity | Hold still; no HUD zoom |
| 2 | 8–18 | Time Card allot in thumb zone → confirm | Cardstock UI; **N** committed from pool | Show presets or slider once; don’t linger |
| 3 | 18–35 | Program: tap path (ink grows) + stance if visible | Player authoring, not a cutscene | 3–5 waypoints enough; keep thumb zone in frame |
| 4 | 35–45 | Aim Snap or Hold; door OPEN/CLOSE once | Shoot + door as verbs | Door beat must be visible on board, not only in UI text |
| 5 | 45–55 | Lock In | Physical-switch moment | One clear press; pause half a beat |
| 6 | 55–75 | Playback scrub or autoplay | Move stepped; shot tracer/muzzle; hit splat; Move ≠ Shoot | If scrubbing, keep playhead readable; don’t skip the hit |
| 7 | 75–90 | Aftermath → next Time Card **or** match-over | Loop continues / stakes land | Prefer wound carry into round 2 if time allows |

**Trim rule:** If over 90s, cut lingering on Allot or Aftermath first — never cut the door beat or Move-vs-Shoot contrast.

---

## Audio bed

- Prefer diegetic foley over music.  
- If music is added later, duck under Lock In + shot.  
- Silence between beats is fine; do not wall-to-wall talk over the scrubber.

---

## Stills (for README)

Capture from the same build as the video when possible.

- [ ] Hero: full board + HUD portrait frame  
- [ ] Path ink close-up on clay board  
- [ ] Shoot moment (tracer or muzzle)  
- [ ] Door interaction — open gap or closed block readable  
- [ ] Time Card + Lock In thumb zone  

Drop files under `screenshots/` (or note paths) and link them from `SHIP_README_DRAFT.md` before promoting to root README.

---

## Export

- [ ] Master: 1080×1920 (or 1080p letterbox with portrait game view)  
- [ ] H.264 (or platform upload spec)  
- [ ] Filename: `logicard-demo-60s.<ext>`  
- [ ] Take log: date, build/commit, Windows vs Editor  
- [ ] Paste video URL + still paths into `SHIP_README_DRAFT.md`

---

## Fail criteria (reshoot)

- Observer cannot tell Move from Shoot  
- Board reads as unlit gray default scene  
- Door never matters  
- HUD occludes the only readable action  
- Playback looks like a random cutscene (no scrubber / no authored path visible beforehand)
