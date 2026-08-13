# Scout / Juggernaut Attributes — Character Implementation Brief

**Status:** Draft 2026-08-13 — **current product cast** (GDD §2 / **C17**), not a unique-verb operator.
Docs-only this pass; **no** new resolver code (Sim pause still applies except existing carve-outs).
**Scope:** Speed / Agility / Strength behavior for the live Scout + Juggernaut Character Cards — what is
locked, what the repo already does, what is silently unwired, and a recommended wiring shape for when
Integrator opens a Character-attrs Sim/HUD contract.
**Depends on:** [`GDD.md`](GDD.md) §2 / §6; [`PRODUCT_MEMORY.md`](PRODUCT_MEMORY.md) **C15**, **C17**,
**C25**; [`CHARACTER_UNIQUE_VERB_OPERATORS_BRIEF.md`](CHARACTER_UNIQUE_VERB_OPERATORS_BRIEF.md) (contrast:
attrs ≠ unique verbs).
**Does not touch:** Bomber / Time Player, Character Select chrome (handed to UI 2026-08-13), gear
attr-scaling except the Interact-as-card Strength carve-out already reserved by **C62**.

---

## 1. What's already locked (do not re-litigate)

From **GDD** §2 / §6 and **C25**:

| Attribute | Meaning | Scout | Juggernaut |
|-----------|---------|-------|------------|
| **Speed** | Time Resource seconds per continuous distance unit at Walk baseline | **1.0 s/unit** (GDD §6 table; §2 also cites 1.5 u/s — see §3) | **2.0 s/unit** (§6; §2 cites 0.75 u/s) |
| **Agility** | Extra seconds when changing stance **or** switching Snap ↔ Hold | **0s** | **+1s** once per switch (**C25**) |
| **Strength** | Door open/close Time Resource cost | **4s** | **2s** |

- Same verbs for both Characters: Move / Shoot / Door — attrs only (**C15** / **C17**).
- Magnitudes are **placeholders** — tune in playtest (**C20** / **C25**).
- Unique-verb Characters (C42–C44) are a different model; attrs briefs must not invent Scout-only verbs.

**Still OPEN — this brief exists to make these concrete enough to decide, not to decide them:**
reconcile GDD §2 vs §6 speed framing; whether Agility penalties are wired; whether Character Select
choice drives both attacker and defender programs; Strength vs Vent/Breach door kinds.

---

## 2. What already exists in the repo (read this before assuming a blank slate)

| File / area | What it says | Cross-check |
|---|---|---|
| `CharacterData.cs` | `baseSecondsPerTile`, `stanceChangePenaltySeconds`, `shootModeChangePenaltySeconds`, `doorInteractBaseSeconds` | Correct schema for GDD attrs. |
| `Scout.asset` / `Juggernaut.asset` | 1s / 0 / 0 / 4s vs 2s / 1 / 1 / 2s | Matches GDD §6 table. |
| `PawnProgram` ctor | Takes `baseSecondsPerTile` + `doorInteractBaseSeconds` | **No parameters** for stance or shoot-mode Agility penalties. |
| `PawnProgram` stance / shoot queue paths | Cost Move via `StanceAllotment` / `TimeResourceMath`; Shoot uses fixed mode costs | **Agility penalty fields are never read** anywhere under `Assets/_Project` except the ScriptableObject assets themselves. **C25 is design-locked but code-unwired.** |
| `GameBootstrap` pawn spawn | Hardcodes attacker/defender `baseSecondsPerTile` and `PawnBuild` (Scout/Juggernaut art) | Comment claims speeds "already match CharacterData presets" — does **not** load `Scout.asset` / `Juggernaut.asset` by reference for program construction. Door interact seconds similarly default to ctor 4f unless callers pass otherwise. |
| `AppFlowController.SelectedArchetype` | Player picks Scout/Juggernaut pre-match | Need to verify (Integrator/contract time) whether that string actually selects `CharacterData` into both `GhostInput` programs or only labels UI — attrs brief flags this as a wiring audit item. |
| GDD §2 vs §6 | §2: Scout 1.5 u/s, Juggernaut 0.75 u/s; §6: 1.0 / 2.0 s per unit Walk | Reciprocals of each other if Walk baseline is the rate — but §6's "1.0 s/unit" ≡ 1.0 u/s, **not** 1.5. Doc inconsistency; assets follow §6. |

**Bottom line:** Speed + Door Strength partially exist as ctor knobs; Agility is **documented and asset-authored but not enforced**. CharacterData assets are not clearly the live authority for match programs.

---

## 3. Open questions blocking a frozen contract

1. **Doc numeric authority.** Greenlight GDD §6 + assets (1s / 2s per unit) and amend §2's 1.5/0.75
   framing, or the reverse? Recommendation: **§6 + assets win**; Integrator amends §2 on confirm.
2. **Wire Agility now or later?** C25 is confirmed design. Is unwired Agility a bug fix (narrow Sim
   carve-out) or parked until a Character-attrs contract opens?
3. **When does the penalty apply?** Leaving Sprint only vs any stance change; Snap↔Hold every switch
   vs first switch per round — C25 says "once when switching" / "same shape as stance-change penalty."
   Confirm: charge **each** switch event that matches the rule, not once per match.
4. **Character Select → live attrs.** Does `SelectedArchetype` drive `PawnProgram` construction for the
   local player? For a local 2nd pawn / scripted defender? For future net opponents?
5. **Vent / Breach doors and Strength.** Do Vent/Breach use the same `doorInteractBaseSeconds`, or
   kind-specific costs? (Map feature, but Strength is Character-owned.)
6. **Interact-as-card Strength carve-out (**C62**).** Out of this brief's implement scope, but attrs
   contract should not rename/remove `doorInteractBaseSeconds` in a way that blocks that later gear hook.

---

## 4. Proposed Sim / Program shape (recommendation, not locked)

**Recommended:** make `CharacterData` the single authority for match Program construction:

- Boot / flow resolves `SelectedArchetype` → `CharacterData` asset.
- `PawnProgram` gains Agility fields (or receives a small `CharacterCombatAttrs` struct) and applies
  `stanceChangePenaltySeconds` when Draft stance commits across a change that C25 taxes, and
  `shootModeChangePenaltySeconds` when queuing a Shoot whose mode differs from the previous Shoot mode
  (or from a defined default — exact trigger in §3 Q3).
- Door queue already uses `DoorInteractSeconds` — ensure it is always copied from
  `CharacterData.doorInteractBaseSeconds`, not left at the 4f default for Juggernaut.
- EditMode tests: Scout queues stance change at +0; Juggernaut at +1; Snap↔Hold mirrors; door costs 4 vs 2.

**Not recommended:** keeping hardcoded `attackerSecondsPerTile` / `DefenderSecondsPerTile` constants in
`GameBootstrap` as the live source of truth while assets drift.

**Flag:** this is still **recommendation-not-contract**. Do not land wiring under the general Sim pause
without an explicit Integrator carve-out (attrs wiring is Sim/Timeline-adjacent).

---

## 5. Proposed HUD shape (recommendation, not locked)

- Program HUD cost previews must include Agility surcharges once wired (scrubber used/budget already
  shows running total — surcharges should appear the instant the taxed action is scheduled, same as
  Move/Shoot today).
- Character Select copy may keep high-level Speed/Agility/Strength blurbs — **UI owns that chrome**
  after the 2026-08-13 handoff; Character owns the numeric truth behind the blurbs.
- No board-anchored prompt required for attrs themselves.

---

## 6. Suggested contract split, once questions are greenlit

| Work | Seat |
|------|------|
| Amend GDD §2 vs §6 if needed | Integrator (PRODUCT_MEMORY / GDD) |
| Explicit Sim carve-out for attrs wiring | Integrator |
| `PawnProgram` + tests reading `CharacterData` Agility/Strength/Speed | Character |
| HUD cost preview reflecting surcharges | UI |
| Character Select presentation | UI (not Character) |

---

## 7. Explicit non-goals of this brief

- No unique verbs.
- No Character Select motion / Kenney / Toolkit work.
- No live code in this docs pass.
- No gear implementation.

---

## See also

- [`GDD.md`](GDD.md) §2 / §6
- [`PRODUCT_MEMORY.md`](PRODUCT_MEMORY.md) — **C25**
- [`CHARACTER_UNIQUE_VERB_OPERATORS_BRIEF.md`](CHARACTER_UNIQUE_VERB_OPERATORS_BRIEF.md)
- Cards worktree `docs/GEAR_BANDAGE_AGENT_BRIEF.md` — doc shape reference
- `Assets/_Project/Characters/CharacterData.cs`, `Scout.asset`, `Juggernaut.asset`
