# Clay cloud motion — plan (Atmosphere)

**Goal:** Storm `CloudBank` clay masses feel **alive / 动起来** without looking like billboard particles or breaking the shade map (crown stays world-up).

**Constraint:** Lobes are Unlit + yaw-only rotation. Do **not** pitch/roll masses or lobes — that reintroduces the “each ball has its own sun” bug (image copy 15).

---

## Phase A — mass drift (landed)

Per Layer-2 mass (`Mass_*`): slow independent **bob (Y) + lateral drift (X/Z) + yaw rock**.

- Driver: `ClayCloudDrift` on each mass after `PlaceClayMass`
- Randomized phase / amplitude / speed so the bank doesn’t breathe in lockstep
- Cheap (7 masses × LateUpdate)

**Human check:** Storm bank should slowly heave and sway; still one clay shelf, not jittery.

---

## Phase B — puff breathe (next)

Per `Puff_*` under a mass: very small **scale pulse** (XZ + slight Y) on a slower second frequency.

- Keeps the mound’s silhouette soft and “doughy”
- Amplitude tiny (≈2–4%) so it doesn’t read as scaling bugs
- Skip if Phase A already sells the look

---

## Phase C — haze / energize sync (optional)

- Edge-haze billboards: slow opacity or local drift with the parent mass (they already parent under the bank)
- Cloud energize: already pulses; optionally bias spawn groups toward masses that are currently rising (nice-to-have, not required)

---

## Out of scope (for now)

- Soft-body / mesh deformation / knead shaders
- Navmesh / physics wind
- Replacing clay with pack `PF_CloudLayer` particles as the primary read

---

## Toggle note

Look-pass **Weather: Sunny / Storm** button lives on `BoardWeatherPocket` (`WeatherToggleUi`) so Atmosphere can A/B moods without editing `ProgramHud`.

## Match Shell note

When HUD chrome covers the bottom ~45%, weather must stay MapViewport-local — see `WEATHER_MAP_VIEWPORT.md` (do not treat camera clear as full-screen sky).
