# UI — STATUS

**Wave / Day:** C62 gear-hand scaffold (UI-only) — **Done on branch, awaiting Integrator wire + human modal Play**
**Branch / worktree:** `feat/modal-restyle` @ (see latest commit) (`D:\projects\Game\logiCard-modal-restyle`)
**Brief:** Queued while modal restyle awaits human Play (Match Over → Quit → confirm). Scope: layout/presentation only; numerics OPEN #16.
**Last cross-reviewed:** 2026-08-13 — C62 gear-hand scaffold landed on branch

## Owned files (this wave)

- `Assets/_Project/UI/ModalDialog.cs` (prior wave; untouched this slice)
- `Assets/_Project/UI/UiStyle.cs` (prior `Modal*` tokens reused — no new parallel palette)
- `Assets/_Project/UI/GearHandView.cs` (new — C62 scaffold)
- `Assets/_Project/Tests/EditMode/GearHandViewTests.cs` (new — presentation smoke)

## Done

- `492b8fe` Restyle ModalDialog as warm cardstock on a deep dimmer — deeper/warmer `ModalDimmer`,
  new additive `ModalCard` / `ModalCardBorder` / `ModalShadow` / `ModalInk` / `ModalDivider` /
  `ModalPrimaryButton(Text)` / `ModalSecondaryButton` tokens, procedural shadow + border panels
  behind the card (no new asset packs), rounded 9-slice kept via `UiStyle.RoundSprite`.
  Hit-target names preserved (`ModalDialog`, `ModalPrimary`, `ModalSecondary`); `Show(...)`
  signature unchanged. Existing `Card` / `Ink` / `PrimaryButton` tokens untouched — Character
  Select sibling can add `CharSelect*` tokens without collision.
- Batchmode verified on this worktree (prior wave): EditMode 137/137, PlayMode 47/47 (incl.
  `MatchEndQuitOpensConfirmDialogBeforeLeaving`).
- **C62 gear-hand scaffold:** new `GearHandView` builds a 4-slot horizontal cardstock strip
  (Bandage / Interact / Flashbang / Adrenaline) using `Modal*` paper tokens. Program vs Execute
  gating (Adrenaline Playback-only), arm/clear highlight, spent grey-out. Cost labels are
  explicit `TR —` placeholders (OPEN #16). Stable hit targets `Gear_{CardId}`. EditMode
  `GearHandViewTests` cover roster, placeholders, phase gating, arm/clear, spent, Modal*
  armed face. **Not wired into `ProgramHud`** — Integrator docks later. No Sim/resolve.
- Unity Editor binary not present on this machine path for a fresh batchmode run this session;
  tests are compile-ready for Integrator/verify worktree.

## In progress

- Nothing further in this worktree's UI lane.

## Blocked

- Prior wave merge: still waiting on human to Play → Match Over → Quit → confirm dialog and sign
  off that it reads warm paper on a deep dimmer. See `PLAY_NOTES.md` (worktree root).
- C62 dock integration + TR numerics: Integrator / OPEN #16 — out of this worker's scope.

## Offers

- Ready to help Integrator resolve `UiStyle.cs` merge conflicts if Character Select merges first
  — this wave only added `Modal*`-prefixed tokens, so a clean textual merge is expected as long as
  Character Select uses `CharSelect*` prefixes as agreed in the brief.
- After modal sign-off, Integrator can parent `GearHandView.Build(...)` into the HUD dock without
  touching ModalDialog; worker can take a follow-up if a dock-slot brief is cut.
