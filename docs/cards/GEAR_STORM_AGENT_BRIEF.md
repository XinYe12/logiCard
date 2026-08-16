# Storm — Gear Numerics Recommendation

**Status:** **Locked 2026-08-16 — C69.** Human confirmed the recommendations below as-is: Time Resource cost free (0s), charges 1×/Character/match. HUD-side true per-match enforcement (a real `StormCastCountOf` counter, mirroring `BandageChargeOf`) remains unstarted follow-up — see C69's implementation note.  
**Depends on:** [`PRODUCT_MEMORY.md`](../core/PRODUCT_MEMORY.md) **C67**; [`CARD_COLLECTION.md`](CARD_COLLECTION.md) §4.2 / §6A; [`../contracts/CURRENT.md`](../contracts/CURRENT.md) Storm contract.  
**Does not touch:** `CardData.cs` (`CardId.Storm = 4` already landed); combat/mechanical effects; UI/Atmosphere seats.

---

## 1. Locked product facts (do not re-litigate)

| Fact | Source |
|------|--------|
| Storm is a **gear / library** card, not a signature | **C67** |
| **Program**-phase only; self-targeting; no board position / LoS / target pawn | **C67** + Storm contract |
| Effect this wave = **presentation only** — switches board weather mood to **Storm** for the rest of the match | **C67** |
| No combat effect (visibility, blind, damage) without a new C# + PLAYBACK redesign | Storm contract out-of-scope |
| Sim-side closed: `ActionVerb.Storm`, `TapeEventType.StormCast`, permissive resolve | **C67** |
| Boot weather = **Fair** so casting Storm is visible | **C67** |
| Charge / TR cost — **locked** | **C69** |

---

## 2. Locked defaults (C69, 2026-08-16)

| Field | Recommendation | Why |
|-------|----------------|-----|
| **Time Resource cost** | **Free (0s)** | Storm is a presentation-only mood switch, not a Program-budget action. |
| **Charges** | **1× per Character per match** | C67: once cast, Storm persists for the **remainder of the match**. Unlimited casts would be no-ops after the first Fair→Storm switch. Mirrors Bandage's per-Character/match charge shape for HUD gating. |
| **Phase** | **Program** (locked) | Arm → place on scrubber Time Resource second only. |
| **`effectSummary` (one line)** | `Switch board weather to Storm for the rest of the match. Presentation only — no combat effect.` | Catalog / `CardData` copy; not a mechanical claim. |

**HUD gate (UI seat):** enforce the recommended once-per-match rule client-side; resolver stays permissive (same split as Bandage / Storm Sim notes).

---

## 3. Explicit non-goals

- Do not invent a TR seconds number.  
- Do not add combat, vision, or LoS effects.  
- Do not edit `CardData.cs` / assets from this brief alone (UI may wire a Storm `.asset` later).  
- Do not open Flashbang or deckbuilder work from this file.

---

## See also

- [`CARD_COLLECTION.md`](CARD_COLLECTION.md) — §4.2 / §6A Storm rows  
- [`../contracts/CURRENT.md`](../contracts/CURRENT.md) — Storm cross-dept contract  
- [`GEAR_BANDAGE_AGENT_BRIEF.md`](GEAR_BANDAGE_AGENT_BRIEF.md) — pattern reference  
