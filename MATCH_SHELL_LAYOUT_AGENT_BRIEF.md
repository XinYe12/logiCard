# MATCH_SHELL_LAYOUT — Atmosphere seat brief (docs + local check)

**From:** Integrator  
**To:** Atmosphere (`D:\projects\Game\logiCard-atmosphere-stylized`)  
**Wave:** Match Shell Layout — **weather read confined to MapViewport**  
**Read:** `docs/ui/MATCH_SHELL_LAYOUT.md` (master).

## Goal

Once HUD chrome covers the bottom ~45% of the screen, sky/clouds/rain must still feel attached to the
**diorama map**, not wash under the hand/toolbar as full-screen wallpaper.

## Deliverable

1. Docs note (extend existing atmosphere/weather STATUS or weather doc): recommended framing —
   pocket stays board-local; avoid depending on full-screen clear.
2. Optional local visual check only — **no merge** of Sunny-mood work; do not reopen Fair-lightning.
3. List any `BoardWeatherPocket` assumptions that break if the camera letterboxes to MapViewport.

## Do not

- Edit UI shell / ProgramHud.
- Land Sunny mood in this wave.
- Merge / push.

## Done when

STATUS + Integrator summary (blockers listed).
