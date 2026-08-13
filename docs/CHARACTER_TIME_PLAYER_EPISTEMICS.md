# Time Player — Epistemic Rules (Object Timeline)

**Status:** Concept draft 2026-08-13 — design language for the **C44** buildability gate.  
**Purpose:** State what Time Player may know and show during Program vs Resolve/Playback so the verb
cannot stab blind simultaneous programming. Numerics stay in the impl brief.  
**Depends on:** [`VISION.md`](VISION.md) Success Metric; [`PRODUCT_MEMORY.md`](PRODUCT_MEMORY.md) **C44**,
OPEN #10; [`CHARACTER_TIME_PLAYER_AGENT_BRIEF.md`](CHARACTER_TIME_PLAYER_AGENT_BRIEF.md);
[`CHARACTER_FANTASY.md`](CHARACTER_FANTASY.md).  
**Binding note:** Until a human answer is written into PRODUCT_MEMORY, **no Time Player Sim contract**.

---

## 1. The pillar (plain language)

Both players lock a program against the **current** public board. Neither may see the other's locked
plan. Resolve then tells the truth once. Playback teaches cause and effect
(`VISION.md` — friend understands "my 2s thing beat your 4s thing").

Time Player's fantasy ("make the room forget") is safe **only if** it never becomes "I already know
how the room turns out this round."

---

## 2. Information classes

| Class | Examples | Program-time OK? |
|-------|----------|------------------|
| **Public now** | Door open/closed; breach Intact/Damaged/Breached; pawn last-revealed wounds/positions from prior Aftermath | Yes — everyone sees the board |
| **Private plan** | Opponent's queued Move/Shoot/Door/Bomb/Rewind nodes | **Never** |
| **Resolve future** | What state an object will have at t=40 after both plans resolve | **Not as a preview of the joint outcome** |
| **Own bookings** | "I queued Rewind on BreachPoint_A at t=12" | Yes — it's your scrubber |

Time Player may read **Public now** and write **Own bookings**. It must not expose **Private plan**
or a **Resolve future** that depends on hidden opponent nodes.

---

## 3. Why "fast-forward" is the dangerous word

**Rewind** (Breached → Damaged → Intact) applied as a **resolve-time event** is structurally like Door
Close: you book it blind; you learn if it mattered in Playback.

**Fast-forward** tempts three leaky readings:

| Leaky reading | Why it fails the pillar |
|---------------|-------------------------|
| Program UI shows "at t=40 this wall will be Breached" using a ghost of **both** plans | Direct private-plan leak |
| Program UI simulates **only your** future nodes on the object | Soft leak / false confidence — teaches a lie if opponent also touches it |
| Fast-forward means "skip to the object's end state for this round" as a free peek | Same as resolve spoiler |

Even a "honest" FF that only steps +1 on the enum at ExecuteTime is fine **as a verb**; the danger is
**preview UX** and **branding that promises foresight**.

---

## 4. Policy options (pick one for PRODUCT_MEMORY)

### Option A — Rewind only (recommended default for v1)

- Ship `ObjectRewind` only. No Fast-forward verb, no FF button, no "time" preview chrome.
- Branding: archivist / undo-the-break (`CHARACTER_FANTASY.md`).
- **Passes epistemic gate** with ordinary Door-like opacity.

### Option B — Resolve-time FF, no preview

- `ObjectFastForward` exists as a booked node (±0 fantasy: +1 state at ExecuteTime).
- HUD shows only: identity, **current** state, cost, confirm — same as Door.
- **Forbidden:** any overlay that draws the object's state at a future scrubber time before Reveal.
- **Passes** if UI discipline holds; fails if "helpful" future ghosts get added later.

### Option C — FF only on object-local deterministic public rules

- FF may advance states that would change from **already-public** ongoing effects only (rare in this
  game — we have almost no public ticking object timers today).
- **Do not** choose C unless such public timers exist; otherwise it collapses to B with extra confusion.

### Option D — Cut Time Player until epistemic redesign of the whole Program phase

- Nuclear; only if A/B feel wrong for the fantasy name.

**Character recommendation:** **A** for first reveal; revisit B only after rewind feels good in play.

---

## 5. Rules that apply even to Rewind-only

1. **Target = object, not pawn.** No rewind of Healthy/Wounded/Downed/Dead (**C38** territory).
2. **v1 object set = geometry breach points** (after C36 exists). Doors/bombs later, explicit scope-in.
3. **Step size = one state** along the defined machine, not a free scrub of match history.
4. **Same-second conflicts** — if Bomber detonates and Time Player rewinds the same point in one round,
   Host ordering must be defined in a future contract (out of scope here; flag only).
5. **Playback teaches,** Program does not spoil — rewind's success/failure readability lives on the tape.

---

## 6. UI / copy blacklist (for UI dept later)

Do not ship:

- "Preview breach at selected time"
- Ghost walls that show post-round geometry during Program
- Tooltips that incorporate opponent intent
- A disabled Fast-forward control that implies foresight is coming "soon" unless Option B is locked

Do ship (when built):

- Current state from authoritative model
- Explicit confirm to book Rewind at time T
- Scrubber marker for your node only

---

## 7. What "human answered the gate" looks like

A PRODUCT_MEMORY note (or C44 amendment) that says one of:

- "Time Player v1 = Rewind only (Option A)."
- "Time Player includes Fast-forward as resolve-time event only; Program preview of future object state
  forbidden (Option B)."

Until that sentence exists, Integrator should refuse a Time Player Sim carve-out even if C36 is ready.

---

## 8. Explicit non-goals

- Does not lock Time Resource cost (OPEN #10).
- Does not implement Adrenaline / mid-Playback branches (different problem class).
- Does not authorize pawn-state rewind.

---

## See also

- [`CHARACTER_TIME_PLAYER_AGENT_BRIEF.md`](CHARACTER_TIME_PLAYER_AGENT_BRIEF.md) — §3 Q1
- [`CHARACTER_C36_DEPENDENCY.md`](CHARACTER_C36_DEPENDENCY.md) — breach primitive prerequisite
- [`VISION.md`](VISION.md) — Success Metric
- [`PRODUCT_MEMORY.md`](PRODUCT_MEMORY.md) — **C44**
