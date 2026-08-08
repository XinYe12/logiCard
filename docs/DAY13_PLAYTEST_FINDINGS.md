# Day 10/11 sign-off + Day 13 presentation playtest — findings template

**Who fills this in:** the human, in the Editor (or the Windows candidate once it exists). Not an agent — this is
specifically the judgment call automated tests can't make (nothing asserts on rendered color, VFX shape, or audio
content).

**Why this file exists:** `SCHEDULE.md`'s own cadence rule is "three written findings each" playtest. This gives
tomorrow's Integrator session a crisp, triage-able list instead of a verbal "yeah it's fine" — each finding below
maps directly to either a same-session Integrator fix or a `/parallel-development` worker slice, per
`PARALLEL_OPS.md`'s Wave 3 plan.

## How to trigger everything in one pass

1. Play Bootstrap.unity, play a Time Card.
2. Move the attacker to roughly `(2, 1)` — that's the scripted defender's ambush aim point.
3. Lock In, watch Playback (both a slow forward scrub and the auto-Play button are worth trying).

That one round should surface: stepped pawn motion, a muzzle flash, a wound splat, and all four Foley sounds
(Footstep, Shot, TimeCard, LockIn).

## Day 10 — visuals (`a57d095` stepped motion + VFX wiring)

- [ ] Attacker (Scout: lean capsule + small head) and defender (Juggernaut: wide capsule + blocky head + shoulder pads) read as visually distinct silhouettes, not "two capsules, different color" (`377029f`, 2026-08-08 — this was a gap in the original Day 10 landing, fixed same day).
- [ ] Stepped motion reads as "pose snaps, not blends" during Playback, not a smooth 60fps glide.
- [ ] Muzzle flash appears at the shooter, oriented toward the aim point, briefly, when a shot fires.
- [ ] Wound splat appears at the victim's position on Wounded/Killed, persists, disappears again on rewind.
- [ ] Nothing looks obviously broken (flash/splat facing wrong way, wrong position, never appearing, never
      disappearing).

**Findings (fill in, even short ones — "the flash is too subtle to notice" is a valid finding):**

1.
2.
3.

## Day 11 — audio (`04f9191` Foley wiring)

- [ ] Footstep, Shot, TimeCard, and LockIn are all audible (check Editor/system volume + Game view speaker icon
      isn't muted).
- [ ] The four sounds read as distinct from each other, not four variations of the same beep.
- [ ] Sounds land at the right moments (Footstep per move leg, Shot on fire, TimeCard on confirm, LockIn on Lock
      In) — not early, not late, not missing.

**Findings:**

1.
2.
3.

## Triage key (for whoever picks this up next — see `PARALLEL_OPS.md` Wave 3 plan)

- **Ship it as-is** — note it, move on. This is allowed; Day 9's board/path were accepted "with reservations" on
  the same logic (schedule > polish once the floor bar is met).
- **Quick fix** (a constant, a color, a timing number) — Integrator does it directly on `master`, no worktree
  needed.
- **Real fix** (new geometry, a different approach, more than a few lines) — Integrator runs `/parallel-development`
  to spin a worker slice, same as the Day 10/11 VFX/Audio splits this session.
- **Not this ship** — log it under Known Issues in `DRAFT_HANDOFF.md` and move on, same as the existing
  wall-clipping cosmetic issue already there.

Once this file has real findings and the ship/fix/defer calls are made, tick Day 10 and Day 11 on `SCHEDULE.md`
and update `DRAFT_HANDOFF.md`'s Verification section to point at this file.
