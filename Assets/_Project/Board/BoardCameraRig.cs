using System;
using UnityEngine;

namespace LogiCard.Board
{
    /// <summary>
    /// Owns the diorama camera's yaw around the board's vertical axis. Pitch, distance, and
    /// orthographic size stay exactly as <c>GameBootstrap.ConfigureCamera</c> set them — this only
    /// steps the view around the board so the player can see it from a different side (e.g. the C53
    /// reference's diagonal corner framing, vs. the default straight-down-the-long-axis view).
    ///
    /// Discrete 45° steps, not free-drag orbit: a continuously draggable camera would fight both this
    /// tactics game's readability needs and <c>docs/UI_BOARD_ANCHORED_COMPONENTS.md</c>'s documented
    /// assumption that "the board and camera are both static — recompute [board-anchored UI]
    /// projection only when the underlying selection changes, never every frame." Rotation still
    /// breaks that assumption once — see <see cref="Rotated"/>.
    /// </summary>
    public sealed class BoardCameraRig : MonoBehaviour
    {
        public const float PitchDegrees = 52f;
        public const float DistanceFromCenter = 14f;
        public const float StepDegrees = 45f;

        private Camera _camera;
        private Vector3 _boardCenter;
        private float _yawDegrees;

        public float YawDegrees => _yawDegrees;

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

        /// <summary>Steps yaw by one <see cref="StepDegrees"/> increment, positive = clockwise looking down.</summary>
        public void Step(int direction)
        {
            if (_camera == null)
            {
                return;
            }

            float delta = direction >= 0 ? StepDegrees : -StepDegrees;
            _yawDegrees = Mathf.Repeat(_yawDegrees + delta, 360f);
            Apply();
            Rotated?.Invoke();
        }

        private void Apply()
        {
            var rotation = Quaternion.Euler(PitchDegrees, _yawDegrees, 0f);
            _camera.transform.rotation = rotation;
            _camera.transform.position = _boardCenter - (rotation * Vector3.forward * DistanceFromCenter);
        }
    }
}
