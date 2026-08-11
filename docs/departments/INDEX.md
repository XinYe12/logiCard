# Departments — Active Index

**Updated:** 2026-08-11 — image-14 wave: Integrator game bugs + lighting/ground research + map-bottom-click.
**Ops constitution:** [`../PARALLEL_OPS.md`](../PARALLEL_OPS.md) · human-side playbook: [`../DIRECTING_AGENTS.md`](../DIRECTING_AGENTS.md)
**Contracts:** [`../contracts/CURRENT.md`](../contracts/CURRENT.md)
**Human rollup:** [`../DRAFT_HANDOFF.md`](../DRAFT_HANDOFF.md)

## Capacity

Integrator + up to **2** coding workers — **2 of 2 in use** (one docs-only research slot + one code slot).
(`logiCard-art-pack-research` / stale zoom-fill / rain worktrees are leftovers — not counted.)

## Active agents / worktrees

Human feedback on `screenshots/image copy 14.png`: path first-step missing, bad door anim,
lightning mid-air, clouds deferred, **round state resets to spawn**, lighting/ground still bad,
bottom-of-map move-click fails.

- **Integrator (main `logiCard`, `master`)** — round state carry, path preview first segment, door
  hinge animation, lightning ground strike. Owns `RoundPlayback`, related Boot/Sim carry paths,
  `PathPreviewView` / preview wiring in `BoardInputController` only as needed for path draw,
  door view animation, `BoardWeatherPocket` lightning placement.
- **`logiCard-lighting-ground-assets`** (`feat/lighting-ground-assets`) — docs research for lighting
  VFX + ground packs. Brief: `LIGHTING_GROUND_ASSETS_AGENT_BRIEF.md`.
- **`logiCard-map-bottom-click`** (`feat/map-bottom-click`) — diagnose/fix bottom-edge move-click.
  Brief: `MAP_BOTTOM_CLICK_AGENT_BRIEF.md`.

## Ownership matrix (write locks)

| Path / concern | Owner now |
|----------------|-----------|
| `RoundPlayback.cs`, `GameBootstrap` next-round/TimeCard/pawn carry, Sim resolve commit | Integrator |
| Door mesh / open-close animation presentation | Integrator |
| `BoardWeatherPocket` lightning strike placement | Integrator |
| `PathPreviewView` (+ minimal `BoardInputController` path-preview wiring if required) | Integrator |
| `docs/ART_PACK_RESEARCH.md`, `docs/ASSET_PACK_AUDIT.md` (research updates) | `logiCard-lighting-ground-assets` |
| `BoardInputController` click→planar / bottom-edge fix + related tests | `logiCard-map-bottom-click` |
| `docs/DRAFT_HANDOFF.md`, `PRODUCT_MEMORY.md`, `contracts/CURRENT.md`, this INDEX | Integrator |
| `docs/departments/<dept>/STATUS.md` | That dept only |

## Cross-review checklist (session start)

- [ ] Read DRAFT_HANDOFF
- [ ] Read this INDEX
- [ ] Read peer STATUS for every **In progress** row above
- [ ] Read contracts/CURRENT
- [ ] Confirm no file overlap before editing
