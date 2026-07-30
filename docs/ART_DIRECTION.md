# D10: Art Direction & Audio Bible

**Doc ID:** D10  
**Status:** Updated 2026-07-30 — **C34** elevates the demo art floor from optional polish to ship requirement  
**Depends on:** [PRODUCT_MEMORY.md](PRODUCT_MEMORY.md) (D9), [SCOPE.md](SCOPE.md), [GDD.md](GDD.md)  
**Canonical path:** `docs/ART_DIRECTION.md` (also referenced as `D10_Art_Direction.md`)

**Core Theme:** "The Desk-Lamp Diorama"

The visual goal is to make the player feel like they are standing over a meticulously crafted **physical tabletop miniature set**. Tactical SWAT-raid seriousness is contrasted with the tactile, handmade charm of **digital clay**.

**Demo vs target (C34):** The 14-day Windows ship **must** hit the **Demo art floor** below so a cold observer reads “handmade miniature game,” not “default Unity scene.” Full SSS, thumbprint maps, bespoke character rigs, and cinematic DoF remain **optional** if time allows — never cut the required floor to chase them.

---

## Moodboard — tilt-shift miniature scale

Hero reference (local): [`../image.png`](../image.png) · also `screenshots/image.png`

High-angle tilt-shift townscape: shallow DoF band, toy-like buildings and cars, “commander over a model” read. This is the **scale and camera language** for the diorama board — not a literal European city art requirement.

---

## Demo art floor (required for 14-day ship — C34)

Ship fails presentation acceptance if any of these are missing:

| Pillar | Required floor | Optional (nice) |
|--------|----------------|-----------------|
| **Board** | Physical plywood/plastic **base**; painted or etched grid; dark **void** outside the board | Messy workbench silhouette in the void |
| **Lighting** | Warm desk-lamp **key** + soft fill; readable painted-miniature silhouettes | Strong tilt-shift DoF |
| **Materials** | Clay-tint / matte polymer look with **subtle** procedural noise (no stock shiny PBR chrome) | True SSS, thumbprint normals |
| **Paths** | **Yarn** or **chalk** path visualization (not neon cyber lines) | Cloth sim / pin beads |
| **Time Card / HUD** | Cardstock Time Card in thumb zone; Lock In feels like a physical switch; **AR scrubber** stays clean/high-contrast vs clay board | Soft card shadow on the diorama |
| **Characters** | Distinct clay-like pawn silhouettes (Scout vs Juggernaut readable); **stepped 8–12 fps** motion in Playback | Bespoke modeled clay characters, facial detail |
| **VFX** | **Physical** muzzle-flash mesh (~2 frames); persistent **clay wound splat** on hit | Cotton Flashbang smoke (post-demo card) |
| **Audio** | Tactile foley: clay-on-board footsteps; cap-gun / heavy-stapler shot; paper Time Card; Lock In switch snap | Full mix / music bed |

**Render note:** Prefer **URP** early in the art pass for lighting/material control; keep an Android-safe fallback profile if a smoke build is attempted.

**Cut order inside art (if behind):** optional DoF/SSS → Crawl-specific AV nuance → door reopen nuance. **Never** cut: warm diorama composition, yarn/chalk path, physical shot feedback, Time Card/Lock In readability, Windows stability.

---

## 1. Visual Pillars (Digital Claymation)

Achieve “physical” feel via lighting, shaders, and camera more than ultra-high-poly meshes.

* **Material (Subsurface Scattering):** Characters and dynamic props use shaders with SSS so light bleeds through thin forms (ears, gun edges) like polymer clay.  
  *Demo floor:* clay-tint materials + soft lighting; true SSS optional.
* **Texture (Thumbprints):** Nothing perfectly smooth — subtle thumbprints, dust, smudges in normals.  
  *Demo floor:* light procedural noise / imperfect albedo OK.
* **Environment:** Map sits on a physical **base** (plywood / plastic board). Grid lines painted or etched into the floor. Outside the board: dark void, optional faint messy workbench silhouette.
* **Camera (Tilt-Shift):** Strong DoF — blur near and far so pawns read ~**2-inch** miniatures.  
  *Demo floor:* moderate DoF on Windows; kill DoF before readability on any Android smoke (**C13**).

---

## 2. Animation (Stop-Motion Feel)

Smooth 60fps character motion kills the illusion.

* **Stepped interpolation:** Character clips baked ~**8–12 fps** (on twos/threes). Pose snaps, not blends. **Required for ship.**
* **No root motion:** Host / ReplayTape moves transforms mathematically; animation plays in place (**C23**).
* **Exaggerated anticipation:** Door kicks, shotgun blasts need big readable anticipation from top-down.

**Playback Duration (C27):** Cinema length is tunable and may compress **Time Resource**; stepped animation still reads if anticipation is clear.

---

## 3. VFX & Particles (Physicalized)

Ban standard glow-game VFX (no bloom lasers, no magical fading smoke).

* **Muzzle flashes:** Jagged static mesh (yellow cotton / orange resin) — **2 frames**, then gone. **Required.**
* **Smoke (Flashbang):** Cotton-wool puffs — post-demo (card deferred under **C34**).
* **Blood / wounds:** Wet red clay splats that stick to floor or models. **Required on hit.**

---

## 4. UI / UX Aesthetic

UI bridges “Player as Commander” and “Physical Board.”

* **Time Card:** Thick cardstock; soft shadow when confirmed; lives in the thumb zone (**C30** / **C33**).
* **Timeline (Time Resource scrubber):** Clean high-tech / AR-like overlay — deliberate contrast with messy clay board.
* **Path drawing:** Not a generic neon line — **colored yarn** pinned to the board, or **chalk** on floor tiles.
* **Layout / touch:** **Portrait, one-handed** (**C30**) — board framed as a tall diorama, controls in the bottom thumb zone; large Lock In; follow D12. Program timer is **real-world** seconds (**C27**).

---

## 5. Audio & Foley

Soundscape sells miniature tactile scale.

* **Footsteps:** Dull heavy thuds — solid clay on plywood/cardboard.  
* **Gunplay:** Punchy but muffled — cap gun / heavy stapler scale, not cinematic battlefield.  
* **UI:** Paper shuffle for Time Card; plastic dial for timeline scrubber; physical switch snap on **Lock In**.

---

## 6. Color & lighting notes

* Desk-lamp / practical miniature lighting — warm key, soft fill, readable silhouettes.
* Saturated “painted miniature” colors (moodboard cue); avoid flat PBR chrome and sci-fi neon.
* Keep UI AR timeline legible (contrast over bloom).

---

## 7. Do / Don’t

| Do | Don’t |
|----|--------|
| Diorama base + void outside board | Infinite open-world horizon |
| Stepped clay motion | Buttery mocap loops with root motion |
| Physicalized VFX (mesh pops, clay splats) | Glow trails, soft particle fog |
| Yarn/chalk paths | Generic cyber grid lines |
| Compress Playback Duration when needed | Force long Time Resource = long wall-clock walk |
| Ship the **Demo art floor** | Treat presentation as “Later polish” after C34 |

---

## Acceptance (art for demo ship)

1. Board reads as a **desk diorama** even without final character models.  
2. Move vs Shoot are **visually distinct** in Playback.  
3. Timeline overlay contrasts with clay board.  
4. Time Card, Lock In, hit, and footsteps each have **distinct** audio (or clearly stubbed placeholders that still sell tactility).  
5. A cold observer does **not** describe the build as “default Unity.”  
6. If an Android smoke build exists, drop DoF/SSS before dropping readability (**C13**).
