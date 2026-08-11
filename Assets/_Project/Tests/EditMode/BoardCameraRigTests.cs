using LogiCard.Board;
using NUnit.Framework;
using UnityEngine;

namespace LogiCard.Tests.EditMode
{
    /// <summary>
    /// Pure yaw-rotation logic — no scene/bootstrap needed, just a GameObject + Camera the rig can
    /// point at a board center.
    /// </summary>
    [TestFixture]
    public sealed class BoardCameraRigTests
    {
        private GameObject _cameraGo;
        private BoardCameraRig _rig;

        [SetUp]
        public void SetUp()
        {
            _cameraGo = new GameObject("TestCamera", typeof(Camera));
            _rig = _cameraGo.AddComponent<BoardCameraRig>();
            _rig.Init(_cameraGo.GetComponent<Camera>(), Vector3.zero);
        }

        [TearDown]
        public void TearDown()
        {
            if (_cameraGo != null)
            {
                Object.DestroyImmediate(_cameraGo);
            }
        }

        [Test]
        public void Init_StartsAtZeroYaw()
        {
            Assert.That(_rig.YawDegrees, Is.EqualTo(0f));
        }

        [Test]
        public void RotateBy_AddsDeltaToYaw()
        {
            _rig.RotateBy(30f);
            Assert.That(_rig.YawDegrees, Is.EqualTo(30f).Within(0.001f));
        }

        [Test]
        public void RotateBy_AcceptsAnyContinuousDelta_NotJustFixedSteps()
        {
            _rig.RotateBy(12.5f);
            Assert.That(_rig.YawDegrees, Is.EqualTo(12.5f).Within(0.001f));
        }

        [Test]
        public void RotateBy_Negative_SubtractsFromYaw()
        {
            _rig.RotateBy(-30f);
            Assert.That(_rig.YawDegrees, Is.EqualTo(330f).Within(0.001f));
        }

        [Test]
        public void RotateBy_WrapsAt360()
        {
            _rig.RotateBy(350f);
            _rig.RotateBy(20f);
            Assert.That(_rig.YawDegrees, Is.EqualTo(10f).Within(0.001f));
        }

        [Test]
        public void RotateBy_ZeroDelta_DoesNotRaiseRotated()
        {
            int fired = 0;
            _rig.Rotated += () => fired++;

            _rig.RotateBy(0f);

            Assert.That(fired, Is.EqualTo(0));
        }

        [Test]
        public void RotateBy_NonZeroDelta_RaisesRotatedOnce()
        {
            int fired = 0;
            _rig.Rotated += () => fired++;

            _rig.RotateBy(15f);

            Assert.That(fired, Is.EqualTo(1));
        }

        [Test]
        public void RotateBy_KeepsPitchFixed()
        {
            _rig.RotateBy(77f);
            Assert.That(_cameraGo.transform.rotation.eulerAngles.x, Is.EqualTo(BoardCameraRig.PitchDegrees).Within(0.01f));
        }

        [Test]
        public void RotateBy_KeepsDistanceFromBoardCenterFixed()
        {
            var boardCenter = new Vector3(4f, 0f, 5f);
            _rig.Init(_cameraGo.GetComponent<Camera>(), boardCenter);

            _rig.RotateBy(93f);

            float distance = Vector3.Distance(_cameraGo.transform.position, boardCenter);
            Assert.That(distance, Is.EqualTo(BoardCameraRig.DistanceFromCenter).Within(0.01f));
        }

        [Test]
        public void RotateBy_NeverMovesCameraBelowBoardHeight()
        {
            // "Cannot rotate to the bottom of the map, it has to be on top of the map" - since pitch
            // never changes, the camera's height above the board center is a fixed function of pitch
            // and distance regardless of yaw. Sweep a full rotation and confirm it holds everywhere,
            // not just at the angles spot-checked above.
            var boardCenter = new Vector3(0f, 0f, 0f);
            _rig.Init(_cameraGo.GetComponent<Camera>(), boardCenter);
            float expectedHeight = Mathf.Sin(BoardCameraRig.PitchDegrees * Mathf.Deg2Rad) * BoardCameraRig.DistanceFromCenter;

            for (int i = 0; i < 24; i++)
            {
                _rig.RotateBy(15f);
                Assert.That(_cameraGo.transform.position.y, Is.EqualTo(expectedHeight).Within(0.01f),
                    $"Camera dropped below expected height at yaw {_rig.YawDegrees}.");
            }
        }

        [Test]
        public void Init_StartsAtBaselineOrthographicSize()
        {
            // GameBootstrap.ConfigureCamera sets 5.0 before Init runs; Init must not silently change
            // an in-bounds starting value.
            Assert.That(_rig.OrthographicSize, Is.EqualTo(5.0f).Within(0.001f));
        }

        [Test]
        public void ZoomBy_AddsDeltaToOrthographicSize()
        {
            _rig.ZoomBy(1.5f);
            Assert.That(_rig.OrthographicSize, Is.EqualTo(6.5f).Within(0.001f));
        }

        [Test]
        public void ZoomBy_AppliesImmediatelyToTheCamera()
        {
            _rig.ZoomBy(-1f);
            Assert.That(_cameraGo.GetComponent<Camera>().orthographicSize, Is.EqualTo(_rig.OrthographicSize).Within(0.001f));
        }

        [Test]
        public void ZoomBy_ClampsToMinOrthographicSize()
        {
            _rig.ZoomBy(-100f);
            Assert.That(_rig.OrthographicSize, Is.EqualTo(BoardCameraRig.MinOrthographicSize).Within(0.001f));
        }

        [Test]
        public void ZoomBy_ClampsToMaxOrthographicSize()
        {
            _rig.ZoomBy(100f);
            Assert.That(_rig.OrthographicSize, Is.EqualTo(BoardCameraRig.MaxOrthographicSize).Within(0.001f));
        }

        [Test]
        public void ZoomBy_ZeroDelta_DoesNotRaiseRotated()
        {
            int fired = 0;
            _rig.Rotated += () => fired++;

            _rig.ZoomBy(0f);

            Assert.That(fired, Is.EqualTo(0));
        }

        [Test]
        public void ZoomBy_NonZeroDelta_RaisesRotatedOnce()
        {
            int fired = 0;
            _rig.Rotated += () => fired++;

            _rig.ZoomBy(0.5f);

            Assert.That(fired, Is.EqualTo(1));
        }

        [Test]
        public void ZoomBy_AlreadyAtBound_DoesNotRaiseRotated()
        {
            _rig.ZoomBy(-100f); // drive to the min bound
            int fired = 0;
            _rig.Rotated += () => fired++;

            _rig.ZoomBy(-5f); // still clamped to the same min bound - no actual change

            Assert.That(fired, Is.EqualTo(0));
        }

        [Test]
        public void ZoomBy_KeepsYawUnchanged()
        {
            _rig.RotateBy(40f);
            _rig.ZoomBy(2f);
            Assert.That(_rig.YawDegrees, Is.EqualTo(40f).Within(0.001f));
        }
    }
}
