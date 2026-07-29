# D10: Art Direction & Audio Bible

**Doc ID:** D10  
**Status:** Drafted 2026-07-29  
**Depends on:** [PRODUCT_MEMORY.md](PRODUCT_MEMORY.md) (D9 locked — mechanics/time glossary do not shift under this doc)  
**Canonical path:** `docs/ART_DIRECTION.md` (also referenced as `D10_Art_Direction.md`)

**Core Theme:** "The Desk-Lamp Diorama"

The visual goal is to make the player feel like they are standing over a meticulously crafted **physical tabletop miniature set**. Tactical SWAT-raid seriousness is contrasted with the tactile, handmade charm of **digital clay**.

**Demo vs target:** The 2-week build may ship **primitives + lighting/camera cheats** that *evoke* this bible. Full SSS, thumbprint maps, and final clay shaders are the **target look** (polish / Later) unless time allows. Do not contradict Scope Out (no final Link’s Awakening–level shaders required for Slice 1).

---

## Moodboard — tilt-shift miniature scale

Hero reference (local): [`../image.png`](../image.png)

High-angle tilt-shift townscape: shallow DoF band, toy-like buildings and cars, “commander over a model” read. This is the **scale and camera language** for the diorama board — not a literal European city art requirement.

---

## 1. Visual Pillars (Digital Claymation)

Achieve “physical” feel via lighting, shaders, and camera more than ultra-high-poly meshes.

* **Material (Subsurface Scattering):** Characters and dynamic props use shaders with SSS so light bleeds through thin forms (ears, gun edges) like polymer clay.  
  *Demo floor:* cheap SSS or soft lit clay-tint materials.
* **Texture (Thumbprints):** Nothing perfectly smooth — subtle thumbprints, dust, smudges in normals.  
  *Demo floor:* flat or lightly noisy albedo OK.
* **Environment:** Map sits on a physical **base** (plywood / plastic board). Grid lines painted or etched into the floor. Outside the board: dark void, optional faint messy workbench silhouette.
* **Camera (Tilt-Shift):** Strong DoF — blur near and far so pawns read ~**2-inch** miniatures.  
  *Demo floor:* moderate DoF; kill DoF on mid Android if heat/FPS suffer (**C13** smooth-first).

---

## 2. Animation (Stop-Motion Feel)

Smooth 60fps character motion kills the illusion.

* **Stepped interpolation:** Character clips baked ~**8–12 fps** (on twos/threes). Pose snaps, not blends.
* **No root motion:** Host / ReplayTape moves transforms mathematically; animation plays in place (**C23**).
* **Exaggerated anticipation:** Door kicks, shotgun blasts need big readable anticipation from top-down.

**Playback Duration (C27):** Cinema length is tunable and may compress **Time Resource**; stepped animation still reads if anticipation is clear.

---

## 3. VFX & Particles (Physicalized)

Ban standard glow-game VFX (no bloom lasers, no magical fading smoke).

* **Muzzle flashes:** Jagged static mesh (yellow cotton / orange resin) — **2 frames**, then gone.
* **Smoke (Flashbang):** Cotton-wool puffs that scale up jaggedly — not smooth particle clouds.
* **Blood / wounds:** Wet red clay splats that stick to floor or models.

---

## 4. UI / UX Aesthetic

UI bridges “Player as Commander” and “Physical Board.”

* **Cards:** Thick cardstock; cast a soft shadow on the diorama when dragged.
* **Timeline (Time Resource scrubber):** Clean high-tech / AR-like overlay — deliberate contrast with messy clay board.
* **Path drawing:** Not a generic neon line — **colored yarn** pinned to the board, or **chalk** on floor tiles.
* **Layout / touch:** **Portrait, one-handed** (**C30**) — board framed as a tall diorama (both floors stacked), controls in the bottom thumb zone; large Lock In; follow D12. Program timer is **real-world** seconds (**C27**).

---

## 5. Audio & Foley

Soundscape sells miniature tactile scale.

* **Footsteps:** Dull heavy thuds — solid clay on plywood/cardboard.
* **Gunplay:** Punchy but muffled — cap gun / heavy stapler scale, not cinematic battlefield.
* **UI:** Paper shuffle for cards; plastic dial for timeline scrubber; physical switch snap on **Lock In**.

---

## 6. Color & lighting notes

* Desk-lamp / practical miniature lighting — warm key, soft fill, readable silhouettes.
* Saturated “painted miniature” colors (moodboard cue); avoid flat PBR chrome and sci-fi neon.
* Keep UI AR timeline legible on Android (contrast over bloom).

---

## 7. Do / Don’t

| Do | Don’t |
|----|--------|
| Diorama base + void outside board | Infinite open-world horizon |
| Stepped clay motion | Buttery mocap loops with root motion |
| Physicalized VFX (mesh pops, cotton) | Glow trails, soft particle fog |
| Yarn/chalk paths | Generic cyber grid lines |
| Compress Playback Duration when needed | Force long Time Resource = long wall-clock walk |

---

## Acceptance (art for demo ship)

1. Board reads as a **desk diorama** even with primitive pawns.  
2. Move vs Shoot are **visually distinct** in Playback.  
3. Timeline overlay contrasts with clay board.  
4. Mid Android stays smooth — drop DoF/SSS before dropping readability (**C13**).
