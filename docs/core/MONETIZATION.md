# Monetization — Cosmetic-Only F2P

**Doc ID:** D13  
**Status:** Drafted 2026-08-08 (**C47**)  
**Depends on:** [VISION.md](VISION.md), [SCOPE.md](SCOPE.md), [PRODUCT_MEMORY.md](PRODUCT_MEMORY.md), [ART_DIRECTION.md](ART_DIRECTION.md), [GDD.md](GDD.md), [SCHEDULE.md](SCHEDULE.md)  
**Authority:** Confirmed decision is **free-to-play, cosmetic-only IAP, no pay-to-win** (**C47**). Numerics in this doc are OPEN until a human design call.

---

## What's monetized

Only **presentation slots** that change how the match looks, never how it resolves. Concrete categories:

| Slot | What the player buys / unlocks | Why it's safe |
|------|--------------------------------|---------------|
| **Character skins** | Alternate clay/toy materials, paint jobs, silhouette-preserving costume variants on Scout / Juggernaut (and later roster characters) | Same attrs, same verbs, same wound ladder |
| **Board / diorama theme skins** | Alternate base, lamp tint, room material packs on the fixed map footprint | Same geometry, doors, radii, LoS blockers |
| **Path-ink stroke style** | Alternate **线稿涂鸦** stroke looks (ink weight, wobble, color family) for booked Move paths | Pure presentation of an already-booked route |
| **Time Card back design** | Alternate cardstock backs / edge treatments on the allotment UI card | Does not change pool math, presets, or `MinRoundSeconds` |
| **Victory VFX** | Alternate physicalized win sting (clay splat / confetti / lamp flicker variants) | Post-match only; never during resolve |

Nothing outside these slots is for sale in the first ship. New cosmetic slots need an explicit design review against the exclusions below before they ship.

---

## Explicitly excluded from sale (with reasoning)

### Gameplay-legible numbers (`GDD.md`)

Anything that changes how Move, Shoot, or Door resolve is **not** a cosmetic:

- `HitRadius` / `LaneHalfWidth` (Snap / Hold contact geometry — **C39**)
- `InteractRadius` (door reach)
- Movement speed / `BaseSecondsPerTile` / stance multipliers
- Action costs (Snap / Hold / Door Time Resource costs, Agility switch penalties — **C25**)
- Wound thresholds, match pool size, `MinRoundSeconds`, Program-phase wall-clock

These are the numbers both players read on the scrubber and bet against. Selling a tighter `HitRadius` or a cheaper Sprint is pay-to-win by definition, even if the store labels it a "skin perk."

### Competitive visibility / contrast floor

Because Move vs. Shoot must stay **visually distinct and readable to both players** (see [CORE_LOOP.md](CORE_LOOP.md) success criteria and [ART_DIRECTION.md](ART_DIRECTION.md)'s readable-silhouette / high-contrast HUD floor), a low-contrast "cosmetic" skin is a **genuine competitive advantage disguised as flavor**, not a cosmetic in the safe sense.

**Rule:** no purchasable or earnable presentation may reduce a pawn's board visibility or contrast below the floor `ART_DIRECTION.md` sets for commercial ship. Dark-on-dark clay, camouflage that blends into a diorama theme, transparent bodies, or path-ink that disappears against the board are all out — even if they look cool in isolation. Skin + board-theme combinations that fail the floor together are also out; the store must not sell a pair that only cheats when equipped together.

---

## Free-earn track

There must be **at least one** way to earn cosmetics without paying. Exact mechanism is **OPEN** — needs a human design call. Candidates (not chosen):

- Battle-pass-style seasonal track
- Straight unlock-by-play (match XP / match count)
- Challenge / achievement unlocks

Do not invent drop rates, track length, or premium-pass gating here. Whatever ships, paid IAP must never be the only path to a cosmetic that changes competitive readability — and given the contrast rule above, *no* cosmetic may change competitive readability at all.

---

## Cross-reference: unique-verb roster (`CHARACTER_ROSTER_LONGTERM.md`)

Future unique-verb characters (**Bomber**, **Time Player**, and any later operators in that doc) are **gameplay content**, not cosmetics.

If/when either is promoted out of long-term-vision status into shippable roster:

- It must ship **free**, or be gated only by skill / grind that every player can complete without paying.
- Putting a unique verb behind a paywall breaks the **C47** no-pay-to-win guarantee, even if the store calls the unlock a "character skin pack."

Do not edit `CHARACTER_ROSTER_LONGTERM.md` from this doc's workstream — Integrator owns that file. This cross-reference is the monetization constraint those promotions must satisfy.

---

## Steam-specific integration

Steamworks wallet / IAP / inventory integration is a **Phase 4 / Phase 6** concern per [SCHEDULE.md](SCHEDULE.md)'s phase table (Monetization Foundation → Steam Certification & Ship). This doc defines *what may be sold and what must never be*; it does not design Steam inventory schemas, entitlement sync, or refund handling.

---

## Numerics — OPEN

Do **not** invent prices. Open questions for a human design call:

| # | Question |
|---|----------|
| 1 | Pricing bands (individual skins vs. bundles vs. season pass)? |
| 2 | Currency model — Steam wallet direct USD only, or a soft currency + hard currency split? |
| 3 | Exact free-earn mechanism (see Free-earn track above)? |
| 4 | Drop / unlock rates if any random grant exists at all (default recommendation: prefer deterministic unlocks over loot-box RNG)? |
| 5 | Whether board-theme × character-skin combos need a pre-purchase contrast preview in the store UI? |

Until these land, engineering may wire **skeleton** IAP (one earnable + one purchasable cosmetic through Steam sandbox — Phase 4 exit criteria) without locking live prices.
