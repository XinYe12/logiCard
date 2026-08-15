# MATCH_SHELL_LAYOUT — Character seat brief (docs)

**From:** Integrator  
**To:** Character (`D:\projects\Game\logiCard-char-select-motion`)  
**Wave:** Match Shell Layout — **docs / InfoBar content sheet only**  
**Read:** `docs/ui/MATCH_SHELL_LAYOUT.md` (master), screenshot refs on main tree.

## Goal

Define what the top **InfoBar** shows for **Characters** in-match — replacing the mock’s “LORD VEXAR /
22 HP / mana orbs” with logiCard truth (wounds, side, Character identity, TR).

## Deliverable

Append a short **InfoBar field sheet** to an existing Character doc (roster / select brief — extend,
don’t create a root orphan). For Attacker + Defender rows (or single combined bar — recommend one):

| Field | Source | Notes |
|-------|--------|-------|
| Character name / archetype | roster | Scout / Juggernaut for demo |
| Side label | Attacker / Defender | C18 |
| Wounds | `WoundsOf` | Healthy / Wounded / Dead — not fantasy HP bars unless you explicitly recommend mapping wounds→pips |
| Time Resource | match pool / round N | display only |
| (optional) | signature / deck size later | mark OPEN if post-demo |

Call out anything that needs a Sim reader that does not exist yet (flag for Integrator — do not invent).

## Do not

- Implement Character Select motion in this wave (prior briefs stay parked unless human reopens).
- Edit HUD C#.
- Merge / push.

## Done when

STATUS + Integrator summary with doc path.
