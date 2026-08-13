# D10: Art Direction & Audio Bible

**Doc ID:** D10  
**Status:** Updated 2026-08-08 — commercial ship art bar (**C46**); path pillar = FragPunk/界外狂潮-style **线稿涂鸦** (supersedes yarn/chalk)  
**Depends on:** [PRODUCT_MEMORY.md](PRODUCT_MEMORY.md) (D9), [SCOPE.md](SCOPE.md), [GDD.md](GDD.md)  
**Canonical path:** `docs/ART_DIRECTION.md` (also referenced as `D10_Art_Direction.md`)

**Core Theme:** "The Desk-Lamp Diorama" *(broadened 2026-08-09 — see the Moodboard section below and
`PRODUCT_MEMORY.md` C53. The desk-lamp-in-a-dark-room lighting mood and toy-chibi fidelity are superseded; the
bounded-floating-chunk-in-a-void structure is not.)*

The visual goal is to make the player feel like they are standing over a meticulously crafted **physical tabletop miniature set**, now pushed toward real detail and weather rather than a toy/chibi read (**C53**). Tactical SWAT-raid seriousness is contrasted with the tactile, handmade charm of **digital clay** — this contrast is being reworked toward more grounded materials; see the Moodboard section for the current target.

**Commercial ship bar:** The bar below is the **required commercial-ship floor**, not a demo floor with optional stretch goals. Full SSS, thumbprint maps, bespoke character rigs, and cinematic DoF move from “optional if time allows” into **in scope for Phase 5 (Commercial Art Bar, see `docs/SCHEDULE.md`)** — not cut-first items.

---

## Moodboard — floating diorama chunk with weather (updated 2026-08-09, C53)

Hero reference (local): [`../image.png`](../image.png) · also `screenshots/image.png` (both kept in sync — see
below if they ever diverge again)

**Replaces the prior toy-townscape reference at this same path** (superseded, not separately archived — the
old reference was a simpler high-angle tilt-shift toy townscape; this one sets a materially higher bar). The
current reference: a richly detailed floating city-block chunk — real architectural detail (lit windows,
signage, awnings), cars/buses/pedestrians with umbrellas, a river with a small bridge and rail line, dense
vegetation — sitting on a natural terrain-edge base (grass → rock → dirt cross-section, framed by a wood
strip) that floats in a dark void below. A dramatic stormy sky with heavy cloud cover and visible rain sits
**directly above the chunk, contained to it** — not an infinite horizon.

**What this sets as the target vs. what stays unchanged (`PRODUCT_MEMORY.md` C53):** the board's structural
shape — a bounded chunk, physical edge, floating in dark void, camera solid-color clear flags — already
matches this reference; that part is validated, not changed. What moves is fidelity (toy-chibi → detailed/
grounded) and the addition of a real sky/cloud/weather system contained above the board. This is the
**scale and camera language plus the weather/detail bar** for the diorama board going forward — translate its
mood and material richness onto this game's actual indoor Yard/Hall/Vault layout, not a literal outdoor city
requirement.

---

## Commercial ship art bar

Ship fails presentation acceptance if any of these are missing:

| Pillar | Required floor | Phase 5 target (in scope — not optional nice-to-have) |
|--------|----------------|------------------------------------------------------|
| **Board** | Physical plywood/plastic **base**; painted or etched grid; dark **void** outside the board | Messy workbench silhouette in the void |
| **Lighting** | Warm desk-lamp **key** + soft fill; readable painted-miniature silhouettes | Strong tilt-shift DoF |
| **Materials** | Clay-tint / matte polymer look with **subtle** procedural noise (no stock shiny PBR chrome) | True SSS, thumbprint normals |
| **Paths** | Thin, slightly wobbly hand-drawn **ink line** on the board surface — FragPunk/界外狂潮-style "线稿涂鸦" (decision 2026-08-07, supersedes the earlier yarn/chalk direction); not fat spray, not a glitchy HUD line, not neon | Waypoint ink dots in the same stroke language |
| **Time Card / HUD** | Cardstock Time Card in the HUD dock; Lock In feels like a physical switch; **AR scrubber** stays clean/high-contrast vs clay board | Soft card shadow on the diorama |
| **Characters** | Distinct silhouettes — Scout vs Juggernaut readable via imported CC0 Quaternius meshes; **smooth per-frame interpolation** in Playback (**C55**, supersedes the earlier stepped 8–12fps pillar). *Note:* current Quaternius imports (see `docs/PAWN_ART_REWORK_PLAN.md` — do not edit that in-progress plan here) are fine for the current build but likely too generic/undifferentiated for a paid, distinctly-branded product; replacement or heavy rework is a Phase 5 candidate, not immediate work. | Bespoke modeled clay characters, facial detail |
| **VFX** | **Physical** muzzle-flash mesh (~2 frames); persistent **clay wound splat** on hit | Cotton Flashbang smoke (when that card is in scope) |
| **Audio** | Tactile foley: clay-on-board footsteps; cap-gun / heavy-stapler shot; paper Time Card; Lock In switch snap | Full mix / music bed |

**Render note:** Prefer **URP** early in the art pass for lighting/material control; keep an Android-safe fallback profile if a smoke build is attempted.

**Phase prioritization (art):** if capacity is tight, sequence Phase 5 stretch items after the required floor — DoF/SSS → Crawl-specific AV nuance → door reopen nuance. **Never** drop: warm diorama composition, sketchy ink-line path, physical shot feedback, Time Card/Lock In readability, Windows stability.

---

## 1. Visual Pillars (Digital Claymation) *(superseded in direction by C53, kept as historical baseline — see Moodboard above)*

Achieve “physical” feel via lighting, shaders, and camera more than ultra-high-poly meshes. **Characters are an approved exception** to the clay/SSS framing below — they use imported CC0 meshes (Quaternius) with URP/Lit + pushed smoothness for a glossy-toy read (*Link's Awakening* 2019), not clay-tint/SSS. Board materials and dynamic props keep the clay language unchanged; see `PAWN_ART_REWORK_PLAN.md`.

* **Material (Subsurface Scattering):** Dynamic props use shaders with SSS so light bleeds through thin forms (ears, gun edges) like polymer clay. Characters: superseded — URP/Lit + glossy smoothness on imported meshes, not clay-tint/SSS.  
  *Ship floor:* clay-tint materials + soft lighting for props/board; true SSS optional. Characters: glossy toy sheen on imported meshes.
* **Texture (Thumbprints):** Nothing perfectly smooth — subtle thumbprints, dust, smudges in normals.  
  *Ship floor:* light procedural noise / imperfect albedo OK.
* **Environment:** Map sits on a physical **base** (plywood / plastic board). Grid lines painted or etched into the floor. Outside the board: dark void, optional faint messy workbench silhouette.
* **Camera (Tilt-Shift):** Strong DoF — blur near and far so pawns read ~**2-inch** miniatures.  
  *Ship floor:* moderate DoF on Windows; kill DoF before readability on any Android smoke (**C13**).

---

## 2. Animation

* **Smooth interpolation (2026-08-10, C55 — supersedes the stepped 8–12fps pillar below):** Character
  movement now samples its `ScheduledPath` every frame and blends normally, matching the render
  framerate of everything else in the scene (camera, rain, lighting). The stepped/stop-motion look
  worked against flat-shaded clay primitives, but once C53/C54 pushed materials, lighting, and door
  models toward photorealism, the contrast made per-frame pose holds read as a framerate bug rather
  than a deliberate stylistic choice — a direct human call after noticing it in a playtest screenshot,
  not a performance fix. `PawnView.ApplyTime` no longer throttles; see `Assets/_Project/Board/PawnView.cs`.
* **No root motion (unchanged):** Host / ReplayTape moves transforms mathematically; animation plays in
  place (**C23**).
* **Exaggerated anticipation (unchanged):** Door kicks, shotgun blasts need big readable anticipation
  from top-down.

**Playback Duration (C27):** Cinema length is tunable and may compress **Time Resource**; unchanged by
the animation-smoothness decision above.

<details>
<summary>Superseded: original "Stop-Motion Feel" pillar (2026-08-07–2026-08-10)</summary>

Smooth 60fps character motion kills the illusion.

* **Stepped interpolation:** Character clips baked ~**8–12 fps** (on twos/threes). Pose snaps, not blends.
* **No root motion:** Host / ReplayTape moves transforms mathematically; animation plays in place (**C23**).
* **Exaggerated anticipation:** Door kicks, shotgun blasts need big readable anticipation from top-down.

This pillar was written when the board was flat-shaded clay primitives and paired with the toy-chibi
character framing C53 later superseded. It is no longer the current direction — kept here for history,
not as guidance.
</details>

---

## 3. VFX & Particles (Physicalized)

Ban standard glow-game VFX (no bloom lasers, no magical fading smoke).

* **Muzzle flashes:** Jagged static mesh (yellow cotton / orange resin) — **2 frames**, then gone. **Required.**
* **Smoke (Flashbang):** Cotton-wool puffs — future roadmap (card deferred, **C46** amends **C34**).
* **Blood / wounds:** Wet red clay splats that stick to floor or models. **Required on hit.**

---

## 4. UI / UX Aesthetic

UI bridges “Player as Commander” and “Physical Board.”

* **Time Card:** Thick cardstock; soft shadow when confirmed; lives in the HUD dock (**C48** / **C33**).
* **Timeline (Time Resource scrubber):** Clean high-tech / AR-like overlay — deliberate contrast with messy clay board.
* **Path drawing:** Not a generic neon line, fat spray, or glitchy HUD line — a thin, slightly wobbly hand-drawn **ink line** on the board surface (FragPunk/界外狂潮-style "线稿涂鸦," decision 2026-08-07; supersedes the earlier yarn/chalk direction). Draft reads like pencil (lighter, rougher); booked reads like settled ink (darker, bolder).
* **Layout:** landscape desktop-first (**C48**) — see `UI_FLOW.md`. Program timer is **real-world** seconds (**C27**).

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
| Smooth per-frame interpolation (C55), transforms driven mathematically | Root motion / engine-driven animation transforms (**C23**, still forbidden) |
| Physicalized VFX (mesh pops, clay splats) | Glow trails, soft particle fog |
| Sketchy ink-line paths (线稿涂鸦) | Generic cyber grid lines, fat spray, glitchy HUD lines |
| Compress Playback Duration when needed | Force long Time Resource = long wall-clock walk |
| Ship the **Commercial ship art bar** | Treat presentation as “Later polish” after core loop |

---

## Acceptance (art for ship)

1. Board reads as a **desk diorama** even without final character models.  
2. Move vs Shoot are **visually distinct** in Playback.  
3. Timeline overlay contrasts with clay board.  
4. Time Card, Lock In, hit, and footsteps each have **distinct** audio (or clearly stubbed placeholders that still sell tactility).  
5. A cold observer does **not** describe the build as “default Unity.”  
6. If an Android smoke build exists, drop DoF/SSS before dropping readability (**C13**).
