# Departments — Active Index

**Updated:** 2026-08-14 — **Storm card wave closed.** Cards (catalog + numerics brief), UI (HUD dock/
arm/place, same pass as Bandage HUD-side, also closed), and Atmosphere (idempotency + lighting round-trip
fix, ported directly onto master rather than merging their branch) all merged on human approval. An
unrelated "Sunny weather mood" feature on Atmosphere's branch was explicitly held back per human decision
— stays uncommitted in that worktree. Map Phase 2 and the earlier rematch/floors/lighting work are also
merged. **No department is coding-hot right now.**

**Ops constitution:** [`../PARALLEL_OPS.md`](../PARALLEL_OPS.md) · human-side playbook: [`../DIRECTING_AGENTS.md`](../DIRECTING_AGENTS.md)
**Contracts:** [`../contracts/CURRENT.md`](../contracts/CURRENT.md) · Playback: [`../core/PLAYBACK_CONTRACT.md`](../core/PLAYBACK_CONTRACT.md)
**Human rollup:** [`../DRAFT_HANDOFF.md`](../DRAFT_HANDOFF.md) · pillars: [`../core/GDD.md`](../core/GDD.md) §11 · map: [`../map/MAP_AUTHORING.md`](../map/MAP_AUTHORING.md) · [`../map/MAP_PRESENTATION_STANDARD.md`](../map/MAP_PRESENTATION_STANDARD.md)

## Capacity

**Permanent seats:** 5 (Atm/Cards/Character/UI/**Map**) + Integrator. **Coding-hot preference:** ≤2. All
seats idle right now.

## Live folders ↔ seats

| Seat | Canonical | **Live folder now** | Tip / state | STATUS |
|------|-----------|---------------------|-------------|--------|
| **Integrator** | `logiCard` | `D:\projects\Game\logiCard` | `master` — clean, all Storm-card merges landed | [`core/STATUS.md`](core/STATUS.md) |
| **Atmosphere** | `logiCard-atmosphere` | `logiCard-atmosphere-stylized` | Storm DoD 1–2 merged (ported); idle. Uncommitted "Sunny mood" work still sitting in the worktree, held back pending a separate decision | [`atmosphere/STATUS.md`](atmosphere/STATUS.md) |
| **Cards** | `logiCard-cards` | `logiCard-cards-collection` | `feat/cards-collection-docs` @ `3e77925` — **merged**, idle. Storm numerics + C68 packaging await human lock | [`cards/STATUS.md`](cards/STATUS.md) |
| **Character** | `logiCard-character` | `logiCard-char-select-motion` | `feat/char-select-motion` — 4 briefs; idle until human answers | [`character/STATUS.md`](character/STATUS.md) |
| **UI** | `logiCard-ui` | `logiCard-modal-restyle` | `feat/modal-restyle` @ `31c55e8` — **Bandage + Storm HUD-side merged**, idle. Chrome-collection icon work resumes, uncommitted | [`ui/STATUS.md`](ui/STATUS.md) |
| **Map** | `logiCard-map` | `D:\projects\Game\logiCard-map` | `dept/map` @ `565583f` — Phase 2 merged, idle | [`map/STATUS.md`](map/STATUS.md) |

## Ownership matrix (write locks)

| Path / concern | Owner now |
|----------------|-----------|
| `docs/cards/CARD_COLLECTION.md`, `docs/cards/CARD_SYSTEM_MODEL_COMPARISON.md`, `docs/cards/CARD_SYSTEM_OPENS.md`, `docs/cards/DECKBUILDER_SYSTEMS_BRIEF.md`, `docs/cards/GEAR_STORM_AGENT_BRIEF.md` | **Cards** — merged, idle |
| `Assets/_Project/Cards/CardData.cs` | **Integrator** — `CardId.Storm` pre-landed; Cards reads, does not edit |
| Weather / `BoardWeatherPocket` / `Resources/Weather/**` | **Integrator** — Storm DoD fixes merged, clean on main; Atmosphere idle (Sunny-mode work held back in that worktree) |
| `ProgramHud`, `GearHandView`, `PawnProgram.TryQueueBandage`/`TryQueueStorm`, `BoardInputController` Bandage/Storm place, related tests | **Integrator** — merged, clean on main; UI idle |
| `RoundPlayback.BandageChargeOf` | **Integrator** — merged, clean on main |
| `Assets/_Project/Board/BoardSurfaceMaterials.cs`, `BoardView.cs` material/dressing call sites | **Integrator** — Phase 2 merged, clean on main; Map idle |
| `MapDefinitions` / `GameBootstrap.BuildXxxGeometry` / Sim door walls | **Integrator** (C57) — Map reads only |
| `GameBootstrap` (rematch, lighting grade, probes, camera, weather boot mood) | **Integrator** — clean on main; no other dept touches it |
| `Net/ActionVerb.cs`, `Net/TapeEvent.cs`, `Net/GhostResolver.cs`, `Boot/RoundPlayback.cs` (Storm Sim-side) | **Integrator** — closed, reference only |
| `CharacterSelectView` / char-select art / `UiMotion` | **UI** (mandate; not this slice) |
| `Assets/_Project/Characters/**`, ability briefs | **Character** |
| `PRODUCT_MEMORY`, `DRAFT_HANDOFF`, contracts, INDEX | **Integrator** |

## Integrator merge gates

1. ~~**Storm card**~~ — **merged** (Cards `a925fd5`, UI `be8ac46`, Atmosphere fix `c051731`).
2. ~~**UI Bandage HUD**~~ — **merged** (`be8ac46`, same commit as Storm HUD-side).
3. **Healed presenter** (`TapeEventType.Healed` in `RoundPlayback` + `PLAYBACK_CONTRACT` §3) — not started.
4. Character ability Sim — blocked on brief answers + carve-out.
5. Batchmode re-verify current tip — UI reported a real batchmode run for their own worktree (166/166
   EditMode, 51/51 PlayMode); nothing has been independently re-run by Integrator on the combined tip.
6. Optional: `GameBootstrap` lighting/`BuildDioramaVolume` re-grade against Map's new saturated flat
   materials — Map flagged it, not required; human already likes the Play look.
7. Separate decision needed: Atmosphere's uncommitted "Sunny weather mood" work (new mood, boot-default
   change, toggle) — currently held back, not part of any merge.
