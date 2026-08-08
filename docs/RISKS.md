# D11: Risk Register — Steam F2P PvP Ship

**Doc ID:** D11  
**Status:** Rewritten 2026-08-08 — **C46 full scope pivot** (see `PRODUCT_MEMORY.md` C46–C51). Prior: Drafted 2026-07-29.  
**Depends on:** [PRODUCT_MEMORY.md](PRODUCT_MEMORY.md), [SCHEDULE.md](SCHEDULE.md), [TDD.md](TDD.md), [SCOPE.md](SCOPE.md), [NETWORKING_DESIGN.md](NETWORKING_DESIGN.md), [MONETIZATION.md](MONETIZATION.md)

Living register. Re-score at each phase gate (see `SCHEDULE.md`), not a fixed calendar day.

**Legend:** L = likelihood · I = impact (1–5). **Score** = L × I. Owner default: you + AI pair.

---

## Top risks

| ID | Risk | L | I | Score | Mitigation | Trigger to cut / pivot |
|----|------|---|---|-------|------------|------------------------|
| R1 | **Real networking has no fallback anymore.** "Fusion Host Mode" is currently a label only — no package installed, zero transport/session code, today's resolve is a same-process local stand-in. | 5 | 5 | 25 | `NETWORKING_DESIGN.md` locks the transport choice and builds a real two-process tape-synced match as Phase 2's exit criteria | If Phase 2 stalls, escalate a transport re-evaluation — do **not** silently keep shipping the same-process stand-in as if it were done; there is no "ship local/hotseat instead" escape hatch for a PvP product |
| R6 | **Determinism/desync under real network conditions**, plus a new **host-integrity gap**: under Fusion Host Mode, one real player literally *is* the host computing the authoritative resolve — fine for local hotseat, a real cheating vector once PvP is real and monetized | 4 | 5 | 20 | Deterministic float-math resolve discipline carries forward unchanged (**C35**); host-integrity answer (dedicated host / relay / replay-audit) required by `NETWORKING_DESIGN.md` before real matches ship | Any client-side hit detection, or ship without an explicit host-integrity answer → do not ship |
| R2 | **Commercial scope is large and open-ended** without a calendar forcing function (monetization + netcode + art bar + AI + landscape UI, all real work) | 4 | 4 | 16 | Phase exit gates in `SCHEDULE.md`; freeze at last completed phase rather than half-starting the next, same discipline the old day-count cut order protected | Never cut Move+Shoot visibility or the core loop |
| R7 | **Cheat/invalid payloads** — "Host revalidates Speed×Stance×budget" was sufficient when both sides trust each other by construction (local demo); not a full answer once Host can be an adversarial real player | 4 | 4 | 16 | Input revalidation carries forward; full answer pending `NETWORKING_DESIGN.md`'s host-integrity design | Reject + Otherwise Stop substitute; do not treat input revalidation alone as sufficient for ranked/monetized play |
| R13 | **Monetization/conversion risk** — F2P cosmetic-only revenue may not sustain ongoing costs; no economy model validated yet | 4 | 4 | 16 | `MONETIZATION.md` needs a benchmarked economy model before committing store-infra engineering time | No committed store backend spend before Phase 4's skeleton economy is validated |
| R14 | **Pay-to-win perception risk (game-specific)** — even nominally cosmetic IAP can be a real competitive advantage here, since the core loop's fairness depends on visual legibility of Move vs Shoot on the board; a low-contrast "cosmetic" skin is a real edge, not just flavor | 3 | 5 | 15 | `MONETIZATION.md`'s silhouette/visibility-neutrality guardrail enforced in code (Phase 4), not just stated in docs | Any purchasable item that measurably changes readability or hit geometry → pulled immediately |
| R16 | **Matchmaking infrastructure/server cost risk** — F2P PvP needs a persistent backend (at minimum a matchmaking queue), genuinely new ongoing-cost infrastructure the local-hotseat demo never needed | 3 | 4 | 12 | `NETWORKING_DESIGN.md` must pick a concrete matchmaking approach and estimate ongoing cost before Phase 2 exits | No live matchmaking commitment without a cost estimate |
| R15 | **Steam storefront/certification risk** — Steamworks SDK integration, store page, review timelines, regional pricing/refunds interacting with F2P+IAP, currently unscoped | 3 | 4 | 12 | Research integration path during Phase 4–5, not left to Phase 6 | Store page drafted before Phase 6 starts, not during it |
| R18 | **Fallback-bot quality/detectability risk** — must be good enough to not feel obviously fake (breaks trust) without becoming a de facto marketed PvE mode (violates `VISION.md`'s non-goal) | 3 | 4 | 12 | `AI_FALLBACK_BOT.md`'s explicit difficulty/disclosure bounds; reference the existing scripted-defender AI as a starting point, not a from-scratch project | Bot detectable as fake in playtesting → tune before Phase 3 exits, don't ship |
| R3 | **Time Resource vs Playback Duration** confuses players / designers | 4 | 4 | 16 | UI labels Time Resource scrubber; cinema uses separate Playback clock; tooltips once | If playtests misread order, slow Playback before changing math |
| R9 | **Continuous Time Resource window too large** (15-min fantasy in one Program) | 4 | 3 | 12 | Per-round **60s TR** placeholder until confirmed against real target session length (now a real retention-adjacent concern, not just a demo placeholder) | Playtests too long → shorter TR window |
| R12 | **GDD still shifting mid-impl** | 3 | 5 | 15 | D9 save-file rule (**C26**); no new CONFIRMED without chat confirm + C# row | Park ideas in OPEN |
| R17 | **Live-ops/post-launch risk** — no marketed single-player content plus elimination-only 1v1 means population/matchmaking health is the single point of failure for player experience; cosmetic-store content cadence is unbudgeted | 3 | 4 | 12 | Fallback bot (R18) is the main empty-queue mitigation; content cadence is an open planning question | Population health tracked from Phase 2 onward, not deferred to post-launch |
| R10 | **Art bar under-investment** — the risk has flipped: the old concern was "don't over-invest beyond the demo floor," the new concern is under-investing relative to a paid commercial product's bar, and the imported Quaternius meshes are a likely-inadequate long-term asset for a distinctly-branded paid product | 3 | 4 | 12 | Budget real character art before wide marketing (Phase 5); no longer "optional if time allows" | Phase 5 doesn't exit on placeholder meshes |

---

## Parked (platform not in current scope — Android/portrait, C48)

These remain real risks for a future mobile port, not active during Phase 0–6:

| ID | Risk | Note |
|----|------|------|
| R4 | Android heat/FPS from DoF/SSS/clay look | Revisit only if/when a mobile port is scoped |
| R11 | Dual-build tax (Win + Android) | Same |
| R8 | Phone-as-Host instability | Netcode-topology-specific to Android hosting; superseded by `NETWORKING_DESIGN.md`'s real topology work regardless |

## Closed

| ID | Risk | Why closed |
|----|------|------------|
| R5 | Path-draw UI overrun | Feature shipped and works; no longer a live risk |

---

## Risk responses by phase

| Phase | Primary watch |
|-------|----------------|
| Phase 0 (Pivot Lock) | R2, R12 — scope/doc consistency before code starts |
| Phase 1 (Landscape UI) | R3 — readability under the new layout |
| Phase 2 (Networking Foundation) | R1, R6, R7 — the biggest gap, highest scores |
| Phase 3 (Fallback Bot) | R18, R17 |
| Phase 4 (Monetization Foundation) | R13, R14, R16 |
| Phase 5 (Commercial Art Bar) | R10 |
| Phase 6 (Steam Cert & Ship) | R15 |

---

## Accepted risks (conscious)

- No FoW in the current build (visibility simplifies sync testing) — unaffected by the pivot.
- Full 15-minute single Program not required — unaffected by the pivot.

*(Two previously-accepted risks are retired by the pivot, not still true: "bots nice-to-have only" — bots now
have a required role, R18; "clay final fidelity not required for ship bar" — art bar is now required, R10.)*

---

## Log (append playtest notes)

| Date | Finding | Action |
|------|---------|--------|
| 2026-07-29 | Register created | — |
| 2026-08-08 | Rewritten for the F2P Steam PvP pivot (**C46**) | R1/R6/R7/R10 reframed; R4/R8/R11 parked; R5 closed; R13–R18 added |
