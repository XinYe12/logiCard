using System;
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
    /// </summary>
    public sealed class BoardCameraRig : MonoBehaviour
    {
        public const float PitchDegrees = 52f;
        public const float DistanceFromCenter = 14f;

        /// <summary>Degrees of yaw per pixel of horizontal mouse drag.</summary>
        public const float DegreesPerPixel = 0.25f;

        /// <summary>
        /// Zoom-in floor. Deliberately below the yaw-0 full-width fit
        /// (<c>width / (2 * aspect) = 8 / 2 = 4.0</c> at the narrowest landscape aspect this project
        /// supports — `aspect == 1`, `docs/SCOPE.md` C48) so the player can fill the board in frame.
        /// At 2.6, board width covers <c>8 / (2 * 2.6) ≈ 1.54×</c> the frame horizontally at yaw 0 /
        /// aspect 1 — edges clip; that is the point of a zoom-in-to-fill control (human feedback
        /// 2026-08-11: prior floor 4.2 left almost no zoom-in headroom against a ~5.0 default).
        ///
        /// Hard constraint vs ConfigureCamera: <see cref="Init"/> clamps the camera's starting
        /// <c>orthographicSize</c> into <c>[Min, Max]</c>. Whatever default ConfigureCamera ships
        /// (historically 5.0; later waves may lower toward fill around ~3.4) must be ≥ this floor,
        /// or Init silently forces it back up. 2.6 leaves clear headroom under a 3.4-class default.
        ///
        /// Board *depth* still varies by map (FreightYard 10, VaultComplex 9, RailPlatform 13);
        /// vertical fit needed is <c>depth * sin(52°) / 2</c> — 3.94 / 3.55 / 5.12 respectively.
        /// All three already exceed this floor (and RailPlatform's 5.12 already exceeded the old
        /// 5.0 baseline), so max zoom-in can run the far/near edge past the top/bottom of frame —
        /// accepted, same as C61. No per-map zoom floors.
        ///
        /// Yaw makes this worse still: at 45°-ish diagonal yaw the board's on-screen footprint is its
        /// full bounding-box diagonal — for RailPlatform,
        /// <c>sqrt((width/2)² + (depth/2)²) = sqrt(4² + 6.5²) ≈ 7.63</c>, well above this floor.
        /// Guaranteeing zero clipping at every yaw would require raising the zoom-in floor above the
        /// zoom-out ceiling below, which defeats the control — accepted trade-off, documented since C61.
        /// </summary>
        public const float MinOrthographicSize = 2.6f;

        /// <summary>
        /// Zoom-out ceiling. Vertical board coverage is <c>depth * sin(52°) / (2 * orthographicSize)</c>
        /// (aspect-independent, per the DRAFT_HANDOFF derivation). The smallest board, VaultComplex
        /// (depth 9), is the binding case — smaller boards read as proportionally smaller still as you
        /// zoom out. At <c>orthographicSize = 8</c> VaultComplex covers
        /// <c>9 * sin(52°) / 16 ≈ 9 * 0.788 / 16 ≈ 44.3%</c> of the frame vertically — still readable
        /// as a tactical board, not a speck, while tighter than the prior 10.0 ceiling now that the
        /// ConfigureCamera default is moving closer to the board (a 10× speck from a ~3.4 default
        /// would feel worse than the same ceiling did against a 5.0 baseline). FreightYard /
        /// RailPlatform read larger still at this same bound since their depth is bigger
        /// (~49% / ~64% vertical coverage respectively).
        /// </summary>
        public const float MaxOrthographicSize = 8.0f;

        /// <summary>orthographicSize units removed per positive notch of <c>Input.mouseScrollDelta.y</c>
        /// (scroll up/away = zoom in = smaller size, hence the sign flip in <see cref="Update"/>).
        /// Full travel <c>(Max - Min) / 0.45 = 5.4 / 0.45 = 12</c> notches — usable across the wider
        /// magnification span without feeling sluggish or twitchy.</summary>
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

        public float YawDegrees => _yawDegrees;

        public bool IsDragging => _dragging;

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
            Apply();
        }

        /// <summary>
        /// Core rotation primitive — any delta, applied immediately, no snapping. <see cref="Update"/>
        /// is the only caller in normal play (right-mouse-drag); exposed publicly so tests (and any
        /// future non-mouse input, e.g. a keyboard hold) can drive it directly without simulating input.
        /// </summary>
        public void RotateBy(float deltaDegrees)
        {
            if (_camera == null || deltaDegrees == 0f)
            {
                return;
            }

            _yawDegrees = Mathf.Repeat(_yawDegrees + deltaDegrees, 360f);
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
            if (_camera == null || deltaSize == 0f)
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

        private void Update()
        {
            if (_camera == null)
            {
                return;
            }

            HandleYawDrag();
            HandleZoomScroll();
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

        private void Apply()
        {
            var rotation = Quaternion.Euler(PitchDegrees, _yawDegrees, 0f);
            _camera.transform.rotation = rotation;
            _camera.transform.position = _boardCenter - (rotation * Vector3.forward * DistanceFromCenter);
        }
    }
}
