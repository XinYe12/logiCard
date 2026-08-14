# Map — STATUS

**Wave / Day:** Docs/decision-prep session, 2026-08-14. **Still not registered in `docs/departments/INDEX.md`**
on this worktree's copy — see Blocked (this is not self-registerable; flagged again below, with a new finding).
**Branch / worktree:** `logiCard-map` / `dept/map` @ `d632d3b` (tip after Phase 1 doc reorg into `docs/map/`).
**Last cross-reviewed:** 2026-08-14 — self-review only; no peer department has reviewed this seat's output yet.

## Scope (per seat brief)

- **Owns:** map/room/floor **presentation** construction standard and (Phase 2) rebuild — materials, prop
  dressing, per-`MapSurfaceRole` visual language. `BoardView.cs`, `BoardSurfaceMaterials.cs` presentation
  logic.
- **Explicitly does not own (read-only):** `MapDefinitions.cs`/room-rectangle authorship, `GameBootstrap`'s
  `BuildXxxGeometry()`/wall/door Sim authoring, `ArenaBoard`, pathfinding, Door API. Those stay Sim-layer,
  locked by **C57/C35/C39/C41**, and are not this department's to rewrite per the seat brief.
- **Adjacent, not owned:** weather/cloud style (Atmosphere dept lane — `BoardWeatherPocket`), character pawn
  materials (Character dept lane), HUD/board-anchored prompts (UI dept lane).

## Done

- Phase 1 (prior session): `docs/map/MAP_PRESENTATION_STANDARD.md` written and, this session, confirmed still
  at Phase 1 status after the docs/ department reorg (`d632d3b`).
- This session (docs/decision-prep only, per brief — no Phase 2 code):
  - Re-read in order: `departments/INDEX.md` → `departments/map/STATUS.md` →
    `docs/map/MAP_PRESENTATION_STANDARD.md` → `docs/map/MAP_AUTHORING.md` → `docs/map/ART_PACK_RESEARCH.md`.
  - Wrote `docs/map/C53_SURFACE_MATERIAL_DECISION.md` — one-page human decision sheet for §4: amend C53 for
    board *surface materials* only (flat/toon over photographic-PBR), geometry density and Atmosphere
    untouched. Recommended default = **yes**, with the one-sentence risk if **no** (a third grading pass on
    material families that already showed twice — C58/C60 — that grading doesn't fix the complaint).
  - Confirmed the Phase 2 ordered checklist below matches `MAP_PRESENTATION_STANDARD.md` §5 exactly (no
    rewrite needed, just surfaced here as the ready checklist for whoever opens the contract).
  - Checked the main Integrator tree (`D:\projects\Game\logiCard`, read-only `git status`) to verify the
    conflict note below is still accurate, not stale — it is, and has a new detail: `docs/departments/INDEX.md`
    is *also* dirty in the main tree, which explains why this worktree's copy still doesn't show Map
    registered (Integrator's registration edit, if any, is sitting uncommitted there, not merged).

- **Human answered §4, 2026-08-14: YES.** Confirmed via `docs/map/C53_SURFACE_MATERIAL_DECISION.md` — amend
  C53 for board surface materials toward flat/toon; geometry density and Atmosphere stay untouched. On the
  baseline-conflict question (below), human chose **wait for Integrator to commit/reclaim** rather than start
  Phase 2 against the current dirty tree. So: §4 is resolved, but Phase 2 code still does not start this
  session — two Integrator-owned items remain outstanding (next bullet).

## In progress

- None — this was a docs/decision-prep-only session per the brief. No Phase 2 code started. §4 is answered but
  Phase 2 is still gated on Integrator (see below), not on any further Map-side work.

## Blocked

- **This seat is still not listed in `docs/departments/INDEX.md`'s live-folders table or ownership matrix**,
  confirmed again this session by reading the file at this worktree's tip. Per `PARALLEL_OPS.md`, only
  Integrator edits that file — not self-registering.
- **Main Integrator tree has `docs/departments/INDEX.md` itself dirty/uncommitted**, alongside `BoardView.cs`,
  `BoardSurfaceMaterials.cs`, `BoardReflectionProbes.cs`, `GameBootstrap.cs`, `RoundPlayback.cs`,
  `MatchClock.cs`, and two Interior materials (`(Mat)Floor_URP.mat`, `(Mat)Glass_URP.mat`), plus
  `docs/DRAFT_HANDOFF.md` and `docs/contracts/CURRENT.md`. This is Integrator's own in-progress
  work-in-flight, not this department's to touch or assume a baseline from — likely why INDEX registration
  hasn't reached this worktree yet.
- **Phase 2 code start now depends on two Integrator actions, not further Map decisions:**
  1. Commit/reclaim the dirty `BoardView.cs`/`BoardSurfaceMaterials.cs`/`BoardReflectionProbes.cs` (human chose
     this path explicitly over starting Map's Phase 2 against an unknown baseline — see above).
  2. Write the `PRODUCT_MEMORY.md` C-row recording the §4 amendment (Integrator-only per the ownership
     matrix — Map does not write PRODUCT_MEMORY rows) and open the Phase 2 contract.
  Map has nothing further to do here until an Integrator session picks these up.

## Conflict note — Phase 2 must not assume current main-tree baseline

The main Integrator tree (`D:\projects\Game\logiCard`, `master`) currently has **uncommitted, dirty** changes
to `BoardView.cs`, `BoardSurfaceMaterials.cs`, and `BoardReflectionProbes.cs` — exactly the three files Phase 2
of this doc's plan needs to edit. Those files are invisible to this worktree (worktrees only see committed
history, per `PARALLEL_OPS.md`) and their eventual shape is unknown from here. **Phase 2 must not start against
an assumed baseline for those three files** — wait for Integrator to either commit/reclaim that dirty work or
explicitly hand off a known-good tip before Map opens a Phase 2 branch, or Phase 2's diff will be built against
a baseline that's already stale the moment Integrator's dirty work lands.

## Offers

- Phase 2 (once §4 is confirmed, a PRODUCT_MEMORY C-row exists, **and** the main-tree conflict above is
  cleared): rebuild the three existing maps' floors/walls/door materials and make `PlaceRoomDressing`
  map-aware, per the ordered checklist below (= `MAP_PRESENTATION_STANDARD.md` §5, restated here so a contract
  can be opened straight off this file without re-deriving it):

  1. Add a `Solid()`/gradient-based floor+wall material set to `BoardSurfaceMaterials`, keyed by the same four
     `MapSurfaceRole`s, replacing `BuildWetSurface()` as the default path. Keep `BuildWetSurface` code (don't
     delete working code speculatively) in case a future non-board surface wants a wet-photo look.
  2. Re-skin nappin door/prop materials via the pack's own `(Mat)Gradient*` variants or a flattened duplicate,
     through the existing `InteriorPackImportTool` duplicate-and-convert pattern.
  3. Make `PlaceRoomDressing` map-aware so Rail Platform / Vault Complex get real in-room dressing instead of
     Freight-Yard-shaped coordinates or nothing.
  4. Re-run `BuildDioramaVolume`/`BuildLighting` grading *after* the material swap, not before — grade a
     saturated base, don't re-grade the same muted one a third time.
  5. Human screenshot check against the Link's Awakening reference (`ART_DIRECTION.md` Moodboard) before
     calling it done — batchmode green is not a look check (`docs/DIRECTING_AGENTS.md`).
