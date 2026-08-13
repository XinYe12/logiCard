# Draft Handoff — 2026-08-13

**Milestone:** Phase 5 Commercial Art Bar (active). Phase 2 Net paused (Bandage-only carve-out, see C63).  
**Tip:** `master` @ `7213d98` — batchmode-verified combined state: **EditMode 149/149, PlayMode 48/48.** Plus a **dirty Integrator tree** (rematch/floors/lighting — uncommitted, see below).  
**Ops:** Permanent depts Atmosphere / Cards / Character / UI (GDD §11); Integrator = ultimate boss (`PARALLEL_OPS.md`). Prefer ≤2 coding-hot.  
**Mandate shift today (human-directed):** Character now owns **behavior/abilities**, not Character Select presentation. UI now owns **all screen presentation** (lobby, Character Select, Map Select, in-game HUD) and must research a mature UI approach before more building. Cards designed the long-term **card-system model** with the human — hybrid signature-cards + shared deckbuilding library, promoted as **C64** (amends C18/C62).  
**Live folders:** `logiCard` · `logiCard-atmosphere-stylized` · `logiCard-cards-collection` · `logiCard-char-select-motion` · `logiCard-gear-bandage-sim` (merged, can retire) · `logiCard-modal-restyle` (merged, new mandate pending).  
**Read first next session:** this file → `PARALLEL_OPS.md` → `departments/INDEX.md` → `contracts/CURRENT.md` → `PLAYBACK_CONTRACT.md` if touching Execute.

## Implemented (merged to master this session)

- **C62 / Bandage gear rules confirmed** (`77831cf`) — see `docs/CARD_COLLECTION.md`.
- **C63 — Bandage numerics + gear-pause carve-out**: 3s TR, 1 charge per Character per **match**, legality "not mid-Sprint" is HUD-gated (resolver stays permissive).
- **Bandage Sim-side, merged `4e6bb66`**: `ActionVerb.Bandage`, `TapeEventType.Healed`, `BandageCharge` threaded through `GhostInput → GhostResolver → ReplayTape → RoundPlayback` mirroring `Wounds`; `RelayProtocol` wire DTOs kept in sync; `Bandage.asset` fixed. Full contract spec in `docs/contracts/CURRENT.md`.
- **UI modal restyle + C62 gear-hand scaffold, merged `7213d98`**: `ModalDialog` warm-cardstock restyle (human Play signed off), plus additive `GearHandView.cs`/`GearHandViewTests.cs` — **not yet wired into `ProgramHud`**. One merge conflict (stale `docs/departments/ui/STATUS.md`) resolved by taking the branch version.
- Merging Bandage exposed a real gap in the dirty rematch fix: `ResetForNewMatch()` reset `Wounds` but not the new `BandageCharge` — fixed inline, still part of the uncommitted bundle below.
- **C64 — long-term card system = hybrid** (signature cards + shared deckbuilding library), promoted from `docs/CARD_SYSTEM_MODEL_COMPARISON.md` §6D after a live design conversation with the human. Amends C18 ("same gear deck" retired) and C62 ("unique verbs stay verbs" → verb *armed by* an exclusive card). Free forever, skins-only monetization, unchanged. **Does not disrupt shipping work** — Bandage/gear-hand stay on transitional full-hand access.
- **Character mandate shift landed** (`dec54e7`): four ability implementation briefs written (`CHARACTER_UNIQUE_VERB_OPERATORS_BRIEF.md`/C42, `CHARACTER_BOMBER_AGENT_BRIEF.md`/C43, `CHARACTER_TIME_PLAYER_AGENT_BRIEF.md`/C44, `CHARACTER_ATTRS_SCOUT_JUGGERNAUT_BRIEF.md`), docs-only, no Sim code. Real finding: **C25 Agility penalties are asset-authored but never read by `PawnProgram`** — Juggernaut's documented +1s stance/shoot-mode-switch penalty currently does nothing in Sim. Character Select ownership (`CharacterSelectView.cs`, `CharSelect*` tokens, char-select art) handed to UI.
- `GDD.md` §11 and `PARALLEL_OPS.md`'s department table synced to the new Character/UI split.

## Dirty on main (uncommitted, do not lose)

- Rematch/fresh match: `MatchClock.Reset`, `RoundPlayback.ResetForNewMatch` (now also resets `BandageCharge`), `GameBootstrap.BeginFreshMatch` + tests.
- Urban floors (`BoardSurfaceMaterials`), lighting/probes, dark void, `ART_DIRECTION.md` notes.
- Screenshot churn; untracked `Assets/ExplosiveLLC/`, `docs/image.png`, `_1k_backup` meta — human keep/delete call.

## Verification

- **Master `7213d98` combined state: batchmode-confirmed green** (EditMode 149/149, PlayMode 48/48) via ephemeral `logiCard-verify-master`, removed after.
- Rematch/floors dirty bundle: **still not batchmode-green this session** (Editor-lock on main, needs a worktree run).
- Character's 4 briefs and Cards' `CARD_SYSTEM_MODEL_COMPARISON.md`: docs-only, boundary-checked against actual diffs (not just claimed) — no Sim/resolver code in either.
- Atmosphere worktree has moved past its last-reviewed tip — not yet re-verified. A live interactive Editor was seen open on it mid-session (someone actively iterating) — check before touching that worktree.

## Not yet staffed / open work

- **Bandage HUD-side slot** (spec in `docs/contracts/CURRENT.md`): dock `GearHandView` into `ProgramHud`, timeline placement, 3 client-side legality gates.
- **Healed presenter** (Integrator's own follow-up): wire `TapeEventType.Healed` into `RoundPlayback.ApplyTime`/`BuildHitVfx`, add the row to `PLAYBACK_CONTRACT.md` §3.
- **UI research**: mature UI approach comparison (read `UI_TOOLKIT_MIGRATION_PROPOSAL.md` first — already piloted + reverted once by Character; real lifecycle bugs documented) before UI takes on full-screen ownership.
- **Character ability Sim contracts**: blocked on human answers to the 4 briefs' open questions **and** an explicit Sim-pause carve-out per ability (mirror C57/C63) — not assumed by this mandate shift.
- **Cards**: Flashbang brief stays paused until re-derived against C64 (is it a shared-library staple or deck tech?).
- **Atmosphere**: still merge-gated on human look sign-off; worktree has moved, dirty tree, not yet reviewed.
- Interact-as-card needs a station target before it's buildable; Adrenaline real effect needs a PLAYBACK_CONTRACT tape-branch redesign.
- Unmonitored older items: door tape Open second; south-edge Move-click; zoom-fill/soft-rain/reflections; `DAY13_PLAYTEST_FINDINGS.md` empty.

## Tomorrow

1. UI: research deliverable (UI Toolkit vs uGUI vs third-party), then open against the Bandage HUD-side contract and/or full-UI ownership.
2. Human answers Character's 4 briefs' open questions; Integrator considers Sim carve-outs per ability.
3. Review Atmosphere's latest work; Play-test and merge when ready.
4. Commit dirty rematch/floors/lighting bundle when human asks; batchmode-verify it on a worktree.
5. Cards re-derives Flashbang against C64 once ready.

## Blockers / notes

- Main Editor on `D:\projects\Game\logiCard` → batchmode only on other worktrees.
- Running too many batchmode/Editor instances at once causes real failures (Package Manager lock contention, not code bugs) — seen this session. Prefer one Unity process at a time when possible.
- Capacity: ≤2 coding-hot; Atmosphere is the hot art lane.
- Intra-match wound carry (C33) stays; only **new match** clears death and gear charges (dirty rematch fix).
- Do not buy ortho god-ray packs. No push unless asked.
