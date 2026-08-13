# Detonator vs Bomber — Concept Stub

**Status:** Concept draft 2026-08-13 — keep two "explosion" fantasies from merging.  
**Purpose:** Separate **Detonator** (martyr, **C38**) from **Bomber** (Program unique verb, **C43**)
so pitch decks, Cards catalog, and future briefs never treat them as the same Character.  
**Depends on:** [`PRODUCT_MEMORY.md`](PRODUCT_MEMORY.md) **C38**, **C43**; [`CHARACTER_FANTASY.md`](CHARACTER_FANTASY.md);
[`CHARACTER_BOMBER_AGENT_BRIEF.md`](CHARACTER_BOMBER_AGENT_BRIEF.md); [`CHARACTER_ROSTER_LONGTERM.md`](CHARACTER_ROSTER_LONGTERM.md).  
**Non-goal:** Full Downed/revive system design, martyr roster beyond Detonator, or Sim code.

---

## 1. One-sentence each

| | Pitch |
|---|--------|
| **Bomber** | You **schedule** a bomb on the **board**; geometry changes (and maybe someone falls) on a booked second. |
| **Detonator** | When someone **finishes** you (Downed → Dead), the Host appends a **body-centered** blast — a martyr tax on the kill. |

If a design doc says "the bomb Character" without this split, send it back here.

---

## 2. Comparison matrix

| Axis | Bomber (**C43**) | Detonator (**C38**) |
|------|------------------|---------------------|
| **Roster model** | Unique **Program verb** (**C42**) | Martyr **archetype** on wound ladder |
| **When it fires** | Player-booked attach / detonate | Downed → **Dead** transition only (finishing blow, not first knockdown) |
| **Who chooses timing** | Bomber player (blind program) | Killer's finish timing + Host append |
| **Target fantasy** | Surface / breach point / floor footprint | AoE around the dying body |
| **Board vs body** | Edits **geometry** (C36) | Wounds/knockback like a Shoot hit |
| **Suicide-from-scratch** | N/A (normal Program costs) | **Closed** — must already be Downed; finish triggers it |
| **Prerequisite systems** | C36 (+ per-floor if fall) | Downed state + revive tile targeting (C38 ladder) |
| **Gear?** | No — verb, not card (**C62**) | No — death conversion, not hand gear |
| **Demo wound ladder** | Unaffected today | **Long-term only** — demo stays Healthy→Wounded→Dead |

---

## 3. Why the split matters

1. **Epistemics:** Bomber is a blind tempo bet on geometry. Detonator is a *consequence of being finished* — opponent can play around it by not finishing, or accept the tax.
2. **Systems:** Bomber pulls C36/verticality. Detonator pulls Downed/revive. Building either does not deliver the other.
3. **Monetization:** Both are gameplay power — free / skill-gated, never a paid "explosion pack" that mixes them.
4. **Naming in HUD:** "Bomb" mode on Bomber ≠ "martyr aura" on Detonator. Never one `CardId.Explode`.

---

## 4. Detonator concept stub (only what's locked + open)

### Locked (C38)

- Wound ladder grows: Healthy → Wounded → **Downed** → Dead (long-term).
- Revive targets a **tile/point**, not a unit ref (Snap-like miss if empty).
- Detonator: Host appends AoE wound/knockback on **Downed→Dead** only.
- Wider martyr roster beyond Detonator stays unlocked.
- Numerics OPEN (OPEN #8).

### Open (parking lot — not answered here)

- Blast radius / knockback distance / whether it can friendly-fire.
- Does Detonator still have Scout-like attrs, or a unique attr row?
- Can Detonator also be Bomber later (two fantasies on one body)? **Default recommendation: no** —
  one spectacular identity each; combining creates "must pick this" P2W-shaped pressure even if free.
- Presentation: tape event for martyr blast vs ordinary Killed.

### Fantasy line

**Detonator:** *Kill me messy — the room charges you for it.*

---

## 5. Bomber reminder (pointer only)

Full verb design lives in [`CHARACTER_BOMBER_AGENT_BRIEF.md`](CHARACTER_BOMBER_AGENT_BRIEF.md) and
[`CHARACTER_C36_DEPENDENCY.md`](CHARACTER_C36_DEPENDENCY.md). Fantasy: *delete the floor the hinge
stands on* — scheduled, geometry-first, not a death rattle.

---

## 6. Cards / catalog hygiene

| Allowed | Forbidden |
|---------|-----------|
| Separate Character entries when either ships | A gear card named "Detonate" that either Character uses |
| Glossary rows that link to this stub | Merging both under "Explosive" class without the matrix |
| Cosmetics (skins) sold for either | Paywall on martyr blast or bomb verb |

---

## 7. Backlog after this stub

- Full Detonator implementation brief (Bandage-shaped) — **only** when C38 approaches a phase gate.
- Human call: mutual exclusivity of Bomber ∩ Detonator on one roster pick (§4 open).
- Integrator: keep C38 and C43 as separate contract waves.

---

## See also

- [`PRODUCT_MEMORY.md`](PRODUCT_MEMORY.md) — **C38**, **C43**, OPEN #8 / #9
- [`CHARACTER_FANTASY.md`](CHARACTER_FANTASY.md) §3
- [`CHARACTER_PLAN.md`](CHARACTER_PLAN.md)
