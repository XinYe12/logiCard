using System;
using UnityEngine;

namespace LogiCard.Board
{
    /// <summary>
    /// Owns the diorama camera's yaw around the board's vertical axis. Pitch, distance, and
    /// orthographic size stay exactly as <c>GameBootstrap.ConfigureCamera</c> set them — this only
    /// rotates the view around the board so the player can see it from a different side (e.g. the C53
    /// reference's diagonal corner framing, vs. the default straight-down-the-long-axis view).
    ///
    /// Smooth, continuous yaw via right-mouse-drag (2026-08-10 — supersedes an earlier discrete
    /// 8-step-button version, direct feedback: "needs to be smoothly rotated, not with button to
    /// rotate at a few fixed angle"). Pitch is deliberately never touched — this is the mechanism that
    /// keeps the camera "on top of the map," never able to rotate underneath it, without needing a
    /// separate clamp: yaw around the vertical axis cannot change how high the camera sits.
    ///
    /// This still breaks <c>docs/UI_BOARD_ANCHORED_COMPONENTS.md</c>'s documented assumption that
    /// "the board and camera are both static — recompute [board-anchored UI] projection only when the
    /// underlying selection changes, never every frame" — now potentially every frame *while actively
    /// dragging*, not just once per discrete step. See <see cref="Rotated"/>.
    /// </summary>
    public sealed class BoardCameraRig : MonoBehaviour
    {
        public const float PitchDegrees = 52f;
        public const float DistanceFromCenter = 14f;

        /// <summary>Degrees of yaw per pixel of horizontal mouse drag.</summary>
        public const float DegreesPerPixel = 0.25f;

        private Camera _camera;
        private Vector3 _boardCenter;
        private float _yawDegrees;
        private bool _dragging;
        private float _lastMouseX;

        public float YawDegrees => _yawDegrees;

        public bool IsDragging => _dragging;

        /// <summary>
        /// Raised after yaw changes and the camera transform has already been updated. Any
        /// board-anchored UI (the door prompt, docs/UI_BOARD_ANCHORED_COMPONENTS.md) must re-run its
        /// world-to-screen projection here — its cached position is now stale, not just on the next
        /// selection change.
        /// </summary>
        public event Action Rotated;

        public void Init(Camera camera, Vector3 boardCenter, float startingYawDegrees = 0f)
        {
            _camera = camera;
            _boardCenter = boardCenter;
            _yawDegrees = startingYawDegrees;
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

        private void Update()
        {
            if (_camera == null)
            {
                return;
            }

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

        private void Apply()
        {
            var rotation = Quaternion.Euler(PitchDegrees, _yawDegrees, 0f);
            _camera.transform.rotation = rotation;
            _camera.transform.position = _boardCenter - (rotation * Vector3.forward * DistanceFromCenter);
        }
    }
}
