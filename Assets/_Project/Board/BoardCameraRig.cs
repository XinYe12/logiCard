using System;
using System.Collections.Generic;
using UnityEngine;

namespace LogiCard.Board
{
    /// <summary>
    /// Owns the diorama camera's yaw around the board's vertical axis, and (2026-08-11) its zoom —
    /// <c>orthographicSize</c>. Pitch and distance stay exactly as <c>GameBootstrap.ConfigureCamera</c>
    /// set them; <c>orthographicSize</c> is set once there too (default owned by ConfigureCamera —
    /// historically the analytically-calibrated 5.0 baseline, see DRAFT_HANDOFF's "orthographicSize
    /// resolved analytically" entry; later waves may lower it toward fill) but ownership of
    /// runtime changes to it now belongs here, same as yaw already did.
    ///
    /// Smooth, continuous yaw via right-mouse-drag (2026-08-10 — supersedes an earlier discrete
    /// 8-step-button version, direct feedback: "needs to be smoothly rotated, not with button to
    /// rotate at a few fixed angle"). Pitch is deliberately never touched — this is the mechanism that
    /// keeps the camera "on top of the map," never able to rotate underneath it, without needing a
    /// separate clamp: yaw around the vertical axis cannot change how high the camera sits.
    ///
    /// Zoom (2026-08-11) is mouse-scroll-wheel only, no pinch/touch path — `docs/SCOPE.md` C48 has this
    /// project as landscape-desktop-first for Steam, with Android/portrait explicitly deferred
    /// (`PRODUCT_MEMORY.md` C30), so there's no touch input surface to wire a pinch gesture into yet.
    /// Revisit if/when a touch build actually lands.
    ///
    /// <c>MinOrthographicSize</c>/<c>MaxOrthographicSize</c> bounds are derived the same way
    /// DRAFT_HANDOFF's "orthographicSize resolved analytically" entry calibrated the 5.0 baseline: for
    /// this orthographic camera at fixed pitch 52°, a world displacement's on-screen extent is its dot
    /// product with the camera's local right/up axes (`DistanceFromCenter` never factors in). See the
    /// constants' own doc comments for the worked numbers across all three maps.
    ///
    /// This still breaks <c>docs/UI_BOARD_ANCHORED_COMPONENTS.md</c>'s documented assumption that
    /// "the board and camera are both static — recompute [board-anchored UI] projection only when the
    /// underlying selection changes, never every frame" — now potentially every frame *while actively
    /// dragging or scrolling*, not just once per discrete step. See <see cref="Rotated"/>, which fires
    /// for zoom changes too (not just yaw) so every existing subscriber gets the same invalidation
    /// treatment without a second event or any change to how <c>GameBootstrap</c> wires it up.
    ///
    /// (2026-08-15) Two additive features, both gated on <see cref="CameraMode"/>:
    /// <list type="bullet">
    /// <item><b>Free pan</b> (<see cref="PanBy"/>, WASD/arrows/edge-of-screen, League of Legends
    /// style) — translates the orbit pivot (<c>_boardCenter</c>) across the board's ground plane,
    /// clamped to <see cref="SetPanBounds"/>. A third independent axis alongside yaw/zoom, same
    /// pivot-mutation shape <c>Apply()</c> already had; not a rewrite.</item>
    /// <item><b>TPS lock</b> (<see cref="EnterTpsLock"/>/<see cref="ExitTpsLock"/>/<see cref="CycleTpsLock"/>,
    /// <see cref="TpsCycleKey"/>) — a real mode switch to perspective, positioned behind/above one
    /// pawn, Resident-Evil style. Reuses the one live <c>Camera.main</c> (toggles
    /// <c>orthographic</c>/position/rotation) rather than a second Camera. <c>RotateBy</c>/<c>ZoomBy</c>/
    /// <c>PanBy</c> are no-ops while locked — orbit state (yaw/zoom/pivot) is frozen, not reset, so
    /// exiting TPS lock returns to exactly where Overview was left.</item>
    /// </list>
    /// Program-phase board input (path drafting) is intentionally scoped out while TPS-locked rather
    /// than silently kept — see <c>BoardInputController.TryClickAtScreenPosition</c>'s camera-mode
    /// gate — because a close over-the-shoulder view can't sanely support "tap the board to draft a
    /// path." TPS lock
    /// itself is legal in any phase (it's an observation mode, not a phase gate); only the live-mouse
    /// click-to-draft path is suppressed. Deferred for v1: pan momentum/easing, camera collision
    /// against props/walls, and any blend/transition between Overview and TPS lock (both cuts are an
    /// instant snap today).
    /// </summary>
    public sealed class BoardCameraRig : MonoBehaviour
    {
        /// <summary>
        /// Overview = today's orthographic diorama orbit (yaw/zoom/pan). TpsLock = perspective,
        /// positioned behind/above one pawn (2026-08-15 free-pan + TPS-lock wave). A real mode
        /// switch, not a zoom level — orbit's yaw-drag/zoom-scroll/pan primitives are no-ops while
        /// locked (see the mode guards in <see cref="RotateBy"/>/<see cref="ZoomBy"/>/<see cref="PanBy"/>),
        /// and <see cref="Update"/> drives TPS follow instead of orbit input while locked.
        /// </summary>
        public enum CameraMode
        {
            Overview,
            TpsLock,
        }

        public const float PitchDegrees = 52f;
        public const float DistanceFromCenter = 14f;

        /// <summary>World units of pan per second of full keyboard/edge-pan input (League of Legends
        /// style, 2026-08-15). Board footprints run roughly 8x9 to 8x13 world units (three maps,
        /// <c>MapDefinitions</c>), so this crosses a board in ~1.5-2s — fast enough to feel responsive,
        /// slow enough not to overshoot the clamp every frame.</summary>
        public const float PanUnitsPerSecond = 6f;

        /// <summary>Screen-space pixels from a viewport edge that trigger edge-pan, LoL-style. Measured
        /// against full-screen <c>Input.mousePosition</c>/<c>Screen.width/height</c>, not the camera's
        /// carved-out <c>rect</c> — the mouse can edge-pan from the strip/dock margins too.</summary>
        public const float EdgePanMarginPixels = 18f;

        /// <summary>Hotkey that cycles Overview -> Pawn 1 TPS lock -> Pawn 2 TPS lock -> ... -> Overview,
        /// in the order <see cref="SetTpsTargets"/> was given. Unused elsewhere in this project (only
        /// other bound key is <c>PhotoModeController.ToggleKey</c>, F9).</summary>
        public const KeyCode TpsCycleKey = KeyCode.T;

        /// <summary>Perspective FOV while TPS-locked. Orthographic (Overview) has no FOV concept;
        /// this only applies once <see cref="EnterTpsLock"/> flips <c>camera.orthographic</c> off.</summary>
        public const float TpsFieldOfViewDegrees = 55f;

        /// <summary>World units behind the locked pawn's root the camera eye sits (Resident-Evil-style
        /// close over-the-shoulder, not a wide follow cam) — tuned against <c>PawnView.TargetVisualHeight</c>
        /// (1.0, pawn root is at ground/feet level), not this rig's orbit <see cref="DistanceFromCenter"/>.</summary>
        public const float TpsFollowDistance = 2.1f;

        /// <summary>World units above the locked pawn's root the camera eye sits.</summary>
        public const float TpsFollowHeight = 1.6f;

        /// <summary>World units above the locked pawn's root the camera looks at (roughly chest/head
        /// height on a ~1.0-unit-tall pawn) — the eye sits higher still (<see cref="TpsFollowHeight"/>),
        /// so this pitches the view down slightly onto the pawn, matching an over-the-shoulder read.</summary>
        public const float TpsLookHeightOffset = 0.9f;

        /// <summary>Minimum ground-plane movement (world units) between frames before TPS facing
        /// re-derives from it. <see cref="PawnView"/> never stores a facing/rotation of its own (it
        /// only translates), so TPS lock derives "which way is forward" from recent motion instead —
        /// below this threshold (near-stationary pawn) the last derived facing is kept rather than
        /// snapping to a near-zero, noisy direction.</summary>
        public const float TpsFacingUpdateThreshold = 0.01f;

        /// <summary>Degrees of yaw per pixel of horizontal mouse drag.</summary>
        public const float DegreesPerPixel = 0.25f;

        /// <summary>
        /// Zoom-in floor. Re-derived 2026-08-16 for the Match Shell Layout wave (five-band HUD,
        /// `docs/ui/MATCH_SHELL_LAYOUT.md`), which shrank <c>ProgramHud.MapViewport</c> from ~58% to
        /// ~48% of window height while <c>cam.rect</c> stays full window *width* — see
        /// <c>GameBootstrap.ConfigureCamera</c>'s comment for why that specific change (not window
        /// aspect ratio, which this formula was already independent of) forces a re-derivation:
        /// shrinking only the rect's height fraction widens the camera's auto-computed <c>aspect</c>
        /// (<c>= windowAspect / rectHeightFraction</c>), which widens the *horizontal* world extent
        /// shown for a fixed <c>orthographicSize</c> — the board's width fills a smaller fraction of
        /// frame than before even though vertical coverage (below) is untouched by this. Scaling every
        /// zoom constant by the same ratio the rect height shrank
        /// (<c>0.48 / 0.58 ≈ 0.828</c>) exactly restores the pre-wave horizontal fill fraction while
        /// proportionally tightening vertical coverage too (more floor-edge crop, the accepted
        /// trade-off — see <see cref="MaxOrthographicSize"/>'s doc for the worked coverage numbers).
        ///
        /// Was 2.6 (pre-wave floor, itself derived from the yaw-0 full-width-fit case
        /// <c>width / (2 * aspect) = 8 / 2 = 4.0</c> at aspect 1, `docs/SCOPE.md` C48). New floor
        /// <c>2.6 * 0.828 ≈ 2.15</c> — the same full-width-fit target now needs a smaller
        /// <c>orthographicSize</c> to reach, since the wider post-wave <c>aspect</c> already shows more
        /// width per unit of <c>orthographicSize</c>.
        ///
        /// Hard constraint vs ConfigureCamera: <see cref="Init"/> clamps the camera's starting
        /// <c>orthographicSize</c> into <c>[Min, Max]</c>. ConfigureCamera's default (now 2.8, see its
        /// own comment) must be ≥ this floor, or Init silently forces it back up.
        ///
        /// Board *depth* still varies by map (FreightYard 10, VaultComplex 9, RailPlatform 13); the
        /// exact-fit <c>orthographicSize</c> for each (<c>depth * sin(52°) / 2</c> — 3.94 / 3.55 / 5.12,
        /// unaffected by the rect-height change since it's a purely vertical, rect-fraction-independent
        /// quantity) all exceed this new floor too, so max zoom-in still runs the far/near edge past the
        /// top/bottom of frame — accepted, same as C61. No per-map zoom floors.
        /// </summary>
        public const float MinOrthographicSize = 2.15f;

        /// <summary>
        /// Zoom-out ceiling. Re-derived 2026-08-16 alongside <see cref="MinOrthographicSize"/> for the
        /// same Match Shell Layout MapViewport shrink (~58% → ~48% window height) — scaled by the same
        /// <c>0.48 / 0.58 ≈ 0.828</c> ratio for consistency (the horizontal-fill argument on
        /// <see cref="MinOrthographicSize"/> applies here too: an unscaled ceiling would let the player
        /// zoom out to a relatively narrower-looking board than before the wave).
        ///
        /// Vertical board coverage is <c>depth * sin(52°) / (2 * orthographicSize)</c>
        /// (aspect-independent — a function of <c>orthographicSize</c> and depth only, not of the
        /// rect's height fraction, per the DRAFT_HANDOFF derivation). Was 8.0, giving VaultComplex
        /// (depth 9, the smallest board and binding case) ~44.3% vertical coverage. New ceiling
        /// <c>8.0 * 0.828 ≈ 6.6</c> gives VaultComplex <c>9 * sin(52°) / (2 * 6.6) ≈ 53.6%</c> — *more*
        /// generous than before (a smaller absolute viewport has less room to spare, so the max-zoom-out
        /// speck risk is tightened, not just carried over). FreightYard / RailPlatform read larger still
        /// at this same bound (~59.5% / ~77.4% vertical coverage respectively).
        /// </summary>
        public const float MaxOrthographicSize = 6.6f;

        /// <summary>orthographicSize units removed per positive notch of <c>Input.mouseScrollDelta.y</c>
        /// (scroll up/away = zoom in = smaller size, hence the sign flip in <see cref="Update"/>).
        /// Full travel <c>(Max - Min) / 0.45 ≈ 4.45 / 0.45 ≈ 10</c> notches post Match Shell Layout
        /// re-derivation (was 12 notches over the pre-wave 5.4 span) — still a usable magnification
        /// range without feeling sluggish or twitchy; left unscaled since the "how many notches feel
        /// right" judgment isn't a function of the Min/Max re-derivation itself.</summary>
        public const float SizePerScrollNotch = 0.45f;

        /// <summary>orthographicSize units per second the camera eases toward its scroll-set target —
        /// same "apply a small delta every frame via the public primitive" shape yaw already uses
        /// (<see cref="Update"/> feeds <see cref="RotateBy"/> a per-frame pixel delta during a drag);
        /// zoom reuses that exact pattern instead of introducing a new lerp/damp convention — this
        /// project has none elsewhere to match. A totally instant jump-to-target read as harsh in
        /// review, so <see cref="Update"/> spreads the approach to the scroll-set target across frames
        /// via <c>Mathf.MoveTowards</c> at this rate rather than snapping <see cref="ZoomBy"/> straight
        /// to it.</summary>
        public const float ZoomUnitsPerSecond = 10f;

        private Camera _camera;
        private Vector3 _boardCenter;
        private float _yawDegrees;
        private bool _dragging;
        private float _lastMouseX;
        private float _orthographicSize;
        private float _targetOrthographicSize;

        private bool _panBoundsSet;
        private Vector3 _panMinWorld;
        private Vector3 _panMaxWorld;

        private CameraMode _mode = CameraMode.Overview;
        private IReadOnlyList<Transform> _tpsTargets = Array.Empty<Transform>();
        private int _tpsTargetIndex = -1;
        private Transform _tpsPawnTransform;
        private Vector3 _tpsFacing = Vector3.forward;
        private Vector3 _tpsLastPawnPosition;

        public float YawDegrees => _yawDegrees;

        public bool IsDragging => _dragging;

        public CameraMode Mode => _mode;

        /// <summary>Which TPS target index is locked (0-based into <see cref="SetTpsTargets"/>'s
        /// list), or -1 while <see cref="Mode"/> is <see cref="CameraMode.Overview"/>.</summary>
        public int TpsTargetIndex => _tpsTargetIndex;

        /// <summary>Current <c>orthographicSize</c> (mirrors the camera's live value once <see cref="Init"/> has run).</summary>
        public float OrthographicSize => _orthographicSize;

        /// <summary>Scroll-set goal <see cref="OrthographicSize"/> is easing toward this frame; equal to
        /// <see cref="OrthographicSize"/> once the ease finishes.</summary>
        public float TargetOrthographicSize => _targetOrthographicSize;

        /// <summary>
        /// Raised after yaw or zoom changes and the camera has already been updated. Any board-anchored
        /// UI (the door prompt, docs/UI_BOARD_ANCHORED_COMPONENTS.md) must re-run its world-to-screen
        /// projection here — its cached position is now stale, not just on the next selection change.
        /// Deliberately one event for both yaw and zoom (rather than a second <c>Zoomed</c> event) —
        /// every current subscriber wants "the view changed, re-project" regardless of which changed,
        /// and reusing it means zoom needs zero changes to how <c>GameBootstrap</c> wires this up.
        /// </summary>
        public event Action Rotated;

        public void Init(Camera camera, Vector3 boardCenter, float startingYawDegrees = 0f)
        {
            _camera = camera;
            _boardCenter = boardCenter;
            _yawDegrees = startingYawDegrees;
            _orthographicSize = Mathf.Clamp(camera.orthographicSize, MinOrthographicSize, MaxOrthographicSize);
            _targetOrthographicSize = _orthographicSize;
            camera.orthographicSize = _orthographicSize;
            camera.orthographic = true;
            _mode = CameraMode.Overview;
            _tpsTargetIndex = -1;
            _tpsPawnTransform = null;
            Apply();
        }

        /// <summary>
        /// Clamps <see cref="PanBy"/> to the board's playable footprint on the ground plane — two
        /// opposite world-space corners (order doesn't matter, min/max is taken per-axis here) so
        /// callers can pass <c>BoardView.WorldFromPlanar</c> of the <c>ArenaBoard</c>'s own
        /// Min/Max X/Y directly without pre-sorting. Per-map (three maps, different footprints,
        /// <c>MapDefinitions</c>) — call again after rebuilding the board for a different map.
        /// </summary>
        public void SetPanBounds(Vector3 worldCornerA, Vector3 worldCornerB)
        {
            _panMinWorld = Vector3.Min(worldCornerA, worldCornerB);
            _panMaxWorld = Vector3.Max(worldCornerA, worldCornerB);
            _panBoundsSet = true;
        }

        /// <summary>
        /// Pawns the TPS lock can cycle through, in cycle order — see <see cref="CycleTpsLock"/>.
        /// GameBootstrap passes the match's two pawn Transforms (attacker, defender); this rig doesn't
        /// know or care about pawn identity/roster beyond "a Transform to look from behind."
        /// </summary>
        public void SetTpsTargets(IReadOnlyList<Transform> targets)
        {
            _tpsTargets = targets ?? Array.Empty<Transform>();
        }

        /// <summary>
        /// Core rotation primitive — any delta, applied immediately, no snapping. <see cref="Update"/>
        /// is the only caller in normal play (right-mouse-drag); exposed publicly so tests (and any
        /// future non-mouse input, e.g. a keyboard hold) can drive it directly without simulating input.
        /// </summary>
        public void RotateBy(float deltaDegrees)
        {
            if (_camera == null || deltaDegrees == 0f || _mode != CameraMode.Overview)
            {
                return;
            }

            _yawDegrees = Mathf.Repeat(_yawDegrees + deltaDegrees, 360f);
            Apply();
            Rotated?.Invoke();
        }

        /// <summary>
        /// Translates the orbit pivot across the board's ground plane (LoL-style free pan,
        /// 2026-08-15) — a third independent axis alongside yaw/zoom, not a replacement for either.
        /// <paramref name="worldDelta"/>'s Y is ignored (ground-plane only; height stays the fixed
        /// function of pitch/distance <see cref="Apply"/> always used). No-op while TPS-locked, same
        /// as <see cref="RotateBy"/>/<see cref="ZoomBy"/> — panning an orbit pivot the TPS view isn't
        /// using would do nothing visible but would silently move Overview out from under the player
        /// for whenever they exit TPS lock.
        /// </summary>
        public void PanBy(Vector3 worldDelta)
        {
            if (_camera == null || _mode != CameraMode.Overview)
            {
                return;
            }

            worldDelta.y = 0f;
            if (worldDelta == Vector3.zero)
            {
                return;
            }

            Vector3 target = _boardCenter + worldDelta;
            if (_panBoundsSet)
            {
                target.x = Mathf.Clamp(target.x, _panMinWorld.x, _panMaxWorld.x);
                target.z = Mathf.Clamp(target.z, _panMinWorld.z, _panMaxWorld.z);
            }

            if (target == _boardCenter)
            {
                return;
            }

            _boardCenter = target;
            Apply();
            Rotated?.Invoke();
        }

        /// <summary>
        /// Core zoom primitive — any delta, applied immediately and clamped, no snapping. Mirrors
        /// <see cref="RotateBy"/>'s shape exactly. <see cref="Update"/> is the only caller in normal
        /// play, feeding it small per-frame deltas as it eases <see cref="OrthographicSize"/> toward
        /// <see cref="TargetOrthographicSize"/> (the same "repeated small delta = smooth motion"
        /// pattern <see cref="RotateBy"/> already gets for free from continuous mouse-drag deltas) —
        /// exposed publicly so tests can drive it directly without simulating scroll input or waiting
        /// on the ease.
        /// </summary>
        public void ZoomBy(float deltaSize)
        {
            if (_camera == null || deltaSize == 0f || _mode != CameraMode.Overview)
            {
                return;
            }

            float clamped = Mathf.Clamp(_orthographicSize + deltaSize, MinOrthographicSize, MaxOrthographicSize);
            if (clamped == _orthographicSize)
            {
                return;
            }

            _orthographicSize = clamped;
            _camera.orthographicSize = _orthographicSize;
            Rotated?.Invoke();
        }

        /// <summary>
        /// Enters perspective TPS lock behind/above <paramref name="pawnTransform"/> (Resident-Evil
        /// style, 2026-08-15) — flips the shared <c>Camera.main</c> from orthographic Overview to
        /// perspective (see class doc: reuses the one live camera, no second <c>Camera</c>). Facing
        /// resets to world +Z (this project's maps spawn the attacker on the low-Y edge moving toward
        /// increasing Y, GameBootstrap.BuildPawns) until the pawn actually moves — see
        /// <see cref="TpsFacingUpdateThreshold"/>.
        /// </summary>
        public void EnterTpsLock(Transform pawnTransform)
        {
            if (_camera == null || pawnTransform == null)
            {
                return;
            }

            _tpsPawnTransform = pawnTransform;
            _tpsLastPawnPosition = pawnTransform.position;
            _tpsFacing = Vector3.forward;
            _mode = CameraMode.TpsLock;
            _camera.orthographic = false;
            _camera.fieldOfView = TpsFieldOfViewDegrees;
            ApplyTpsLock();
            Rotated?.Invoke();
        }

        /// <summary>Returns to Overview at the same yaw/zoom/pivot it had before TPS lock — orbit
        /// state is untouched while locked (<see cref="RotateBy"/>/<see cref="ZoomBy"/>/<see cref="PanBy"/>
        /// are no-ops during <see cref="CameraMode.TpsLock"/>), so this is just re-running
        /// <see cref="Apply"/> against whatever that state already is.</summary>
        public void ExitTpsLock()
        {
            if (_mode != CameraMode.TpsLock)
            {
                return;
            }

            _tpsPawnTransform = null;
            _mode = CameraMode.Overview;
            _camera.orthographic = true;
            _camera.orthographicSize = _orthographicSize;
            Apply();
            Rotated?.Invoke();
        }

        /// <summary>
        /// Overview -> target[0] TPS lock -> target[1] TPS lock -> ... -> Overview. The whole "pick a
        /// pawn to view through" surface for v1 (brief: keep it simple) — cycling a hotkey rather than
        /// a HUD pawn picker; <see cref="TpsCycleKey"/> is the sole binding. No-op if
        /// <see cref="SetTpsTargets"/> was never called or given an empty list.
        /// </summary>
        public void CycleTpsLock()
        {
            if (_tpsTargets.Count == 0)
            {
                return;
            }

            if (_mode == CameraMode.Overview)
            {
                _tpsTargetIndex = 0;
            }
            else
            {
                _tpsTargetIndex++;
                if (_tpsTargetIndex >= _tpsTargets.Count)
                {
                    _tpsTargetIndex = -1;
                    ExitTpsLock();
                    return;
                }
            }

            EnterTpsLock(_tpsTargets[_tpsTargetIndex]);
        }

        private void Update()
        {
            if (_camera == null)
            {
                return;
            }

            if (Input.GetKeyDown(TpsCycleKey))
            {
                CycleTpsLock();
            }

            if (_mode == CameraMode.TpsLock)
            {
                // Orbit input (drag/scroll/pan) is a diorama-overview concept only — see the mode
                // guards on RotateBy/ZoomBy/PanBy. Follow the locked pawn instead.
                ApplyTpsLock();
                return;
            }

            HandleYawDrag();
            HandleZoomScroll();
            HandlePan();
        }

        private void HandleYawDrag()
        {
            if (Input.GetMouseButtonDown(1))
            {
                _dragging = true;
                _lastMouseX = Input.mousePosition.x;
                return;
            }

            if (Input.GetMouseButtonUp(1))
            {
                _dragging = false;
                return;
            }

            if (!_dragging || !Input.GetMouseButton(1))
            {
                _dragging = false;
                return;
            }

            float mouseX = Input.mousePosition.x;
            float deltaPixels = mouseX - _lastMouseX;
            _lastMouseX = mouseX;

            if (deltaPixels != 0f)
            {
                RotateBy(deltaPixels * DegreesPerPixel);
            }
        }

        private void HandleZoomScroll()
        {
            // Positive scroll (wheel away from the player) is the conventional "zoom in" gesture, so it
            // subtracts from orthographicSize (smaller size = magnified view) — hence the sign flip.
            float scrollNotches = Input.mouseScrollDelta.y;
            if (scrollNotches != 0f)
            {
                _targetOrthographicSize = Mathf.Clamp(
                    _targetOrthographicSize - (scrollNotches * SizePerScrollNotch),
                    MinOrthographicSize,
                    MaxOrthographicSize);
            }

            if (!Mathf.Approximately(_orthographicSize, _targetOrthographicSize))
            {
                float eased = Mathf.MoveTowards(_orthographicSize, _targetOrthographicSize, ZoomUnitsPerSecond * Time.deltaTime);
                ZoomBy(eased - _orthographicSize);
            }
        }

        /// <summary>
        /// WASD/arrow keys plus edge-of-screen pointer panning, League of Legends style — camera-relative
        /// (projected onto the ground plane), not fixed world axes, so "up" still pans away from the
        /// player at any yaw. Suppressed during an active right-drag (<see cref="_dragging"/>): panning
        /// while also orbiting would fight the drag's own screen-space intent. Deferred for v1: no
        /// ease/momentum on release (matches yaw drag's already-instant feel), no camera collision.
        /// </summary>
        private void HandlePan()
        {
            if (_dragging)
            {
                return;
            }

            Vector2 dir = Vector2.zero;
            if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow))
            {
                dir.y += 1f;
            }

            if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow))
            {
                dir.y -= 1f;
            }

            if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow))
            {
                dir.x += 1f;
            }

            if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow))
            {
                dir.x -= 1f;
            }

            dir += EdgePanDirection();
            if (dir.sqrMagnitude < 0.0001f)
            {
                return;
            }

            if (dir.sqrMagnitude > 1f)
            {
                dir.Normalize();
            }

            Vector3 forward = _camera.transform.forward;
            forward.y = 0f;
            forward = forward.sqrMagnitude > 0.0001f ? forward.normalized : Vector3.forward;

            Vector3 right = _camera.transform.right;
            right.y = 0f;
            right = right.sqrMagnitude > 0.0001f ? right.normalized : Vector3.right;

            Vector3 worldDir = (right * dir.x) + (forward * dir.y);
            PanBy(worldDir * PanUnitsPerSecond * Time.deltaTime);
        }

        /// <summary>Pointer-near-edge contribution to <see cref="HandlePan"/>, measured against the
        /// full screen (not the camera's carved-out viewport <c>rect</c>) since the pointer can sit in
        /// the top-strip/HUD-dock margins and still mean "pan this way." Returns zero off-screen (e.g.
        /// after an OS-level focus loss) rather than reading a stale edge position.</summary>
        private static Vector2 EdgePanDirection()
        {
            Vector3 mouse = Input.mousePosition;
            if (mouse.x < 0f || mouse.x > Screen.width || mouse.y < 0f || mouse.y > Screen.height)
            {
                return Vector2.zero;
            }

            Vector2 dir = Vector2.zero;
            if (mouse.x <= EdgePanMarginPixels)
            {
                dir.x -= 1f;
            }
            else if (mouse.x >= Screen.width - EdgePanMarginPixels)
            {
                dir.x += 1f;
            }

            if (mouse.y <= EdgePanMarginPixels)
            {
                dir.y -= 1f;
            }
            else if (mouse.y >= Screen.height - EdgePanMarginPixels)
            {
                dir.y += 1f;
            }

            return dir;
        }

        private void Apply()
        {
            var rotation = Quaternion.Euler(PitchDegrees, _yawDegrees, 0f);
            _camera.transform.rotation = rotation;
            _camera.transform.position = _boardCenter - (rotation * Vector3.forward * DistanceFromCenter);
        }

        /// <summary>
        /// Continuous TPS follow: reads <c>_tpsPawnTransform</c>'s current position each frame and
        /// repositions behind/above it — a pure function of that transform's already-updated
        /// position, same "no restart, just re-derive" shape PLAYBACK_CONTRACT.md requires of Execute
        /// presenters (PawnView.ApplyTime already drives that transform from the scrubber; this never
        /// starts its own timer/tween). Facing comes from recent ground-plane movement since
        /// <see cref="PawnView"/> never stores a rotation of its own.
        /// </summary>
        private void ApplyTpsLock()
        {
            if (_tpsPawnTransform == null)
            {
                ExitTpsLock();
                return;
            }

            Vector3 pawnPos = _tpsPawnTransform.position;
            Vector3 delta = pawnPos - _tpsLastPawnPosition;
            delta.y = 0f;
            if (delta.sqrMagnitude > TpsFacingUpdateThreshold * TpsFacingUpdateThreshold)
            {
                _tpsFacing = delta.normalized;
            }

            _tpsLastPawnPosition = pawnPos;

            Vector3 eyePoint = pawnPos - (_tpsFacing * TpsFollowDistance) + (Vector3.up * TpsFollowHeight);
            Vector3 lookPoint = pawnPos + (Vector3.up * TpsLookHeightOffset);
            Vector3 lookDir = lookPoint - eyePoint;

            bool moved = _camera.transform.position != eyePoint;
            _camera.transform.position = eyePoint;
            if (lookDir.sqrMagnitude > 0.0001f)
            {
                _camera.transform.rotation = Quaternion.LookRotation(lookDir.normalized, Vector3.up);
            }

            if (moved)
            {
                Rotated?.Invoke();
            }
        }
    }
}
