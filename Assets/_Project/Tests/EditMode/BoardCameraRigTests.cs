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
            // Unity Camera default / historical ConfigureCamera baseline is 5.0; Init must not
            // silently change an in-bounds starting value.
            Assert.That(_rig.OrthographicSize, Is.EqualTo(5.0f).Within(0.001f));
        }

        [Test]
        public void Init_PreservesConfigureCameraDefaultNearFill()
        {
            // Integrator may lower ConfigureCamera's default toward fill (~3.4). Min must stay ≤ that
            // value or Init clamps the new default back up and undoes the framing change.
            var cam = _cameraGo.GetComponent<Camera>();
            cam.orthographicSize = 3.4f;
            _rig.Init(cam, Vector3.zero);

            Assert.That(_rig.OrthographicSize, Is.EqualTo(3.4f).Within(0.001f));
            Assert.That(cam.orthographicSize, Is.EqualTo(3.4f).Within(0.001f));
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

        [Test]
        public void PanBy_MovesCameraAlongDelta_KeepingDistanceFromNewCenter()
        {
            var boardCenter = new Vector3(4f, 0f, 5f);
            _rig.Init(_cameraGo.GetComponent<Camera>(), boardCenter);

            _rig.PanBy(new Vector3(1.5f, 0f, -0.5f));

            Vector3 expectedCenter = boardCenter + new Vector3(1.5f, 0f, -0.5f);
            Assert.That(Vector3.Distance(_cameraGo.transform.position, expectedCenter),
                Is.EqualTo(BoardCameraRig.DistanceFromCenter).Within(0.01f));
        }

        [Test]
        public void PanBy_IgnoresYComponent()
        {
            _rig.PanBy(new Vector3(0f, 5f, 0f));

            Assert.That(_cameraGo.transform.position.y,
                Is.EqualTo(Mathf.Sin(BoardCameraRig.PitchDegrees * Mathf.Deg2Rad) * BoardCameraRig.DistanceFromCenter).Within(0.01f));
        }

        [Test]
        public void PanBy_ZeroDelta_DoesNotRaiseRotated()
        {
            int fired = 0;
            _rig.Rotated += () => fired++;

            _rig.PanBy(Vector3.zero);

            Assert.That(fired, Is.EqualTo(0));
        }

        [Test]
        public void PanBy_NonZeroDelta_RaisesRotatedOnce()
        {
            int fired = 0;
            _rig.Rotated += () => fired++;

            _rig.PanBy(new Vector3(1f, 0f, 0f));

            Assert.That(fired, Is.EqualTo(1));
        }

        [Test]
        public void PanBy_ClampsToPanBounds()
        {
            _rig.SetPanBounds(new Vector3(-2f, 0f, -2f), new Vector3(2f, 0f, 2f));

            _rig.PanBy(new Vector3(100f, 0f, 100f));

            Vector3 expectedCenter = new Vector3(2f, 0f, 2f);
            Assert.That(Vector3.Distance(_cameraGo.transform.position, expectedCenter),
                Is.EqualTo(BoardCameraRig.DistanceFromCenter).Within(0.01f));
        }

        [Test]
        public void PanBy_AtBoundClamp_DoesNotRaiseRotated()
        {
            _rig.SetPanBounds(new Vector3(-2f, 0f, -2f), new Vector3(2f, 0f, 2f));
            _rig.PanBy(new Vector3(100f, 0f, 100f)); // drive to the corner
            int fired = 0;
            _rig.Rotated += () => fired++;

            _rig.PanBy(new Vector3(100f, 0f, 100f)); // still clamped to the same corner

            Assert.That(fired, Is.EqualTo(0));
        }

        [Test]
        public void EnterTpsLock_SwitchesToPerspective()
        {
            var target = new GameObject("TpsTarget").transform;
            target.position = new Vector3(3f, 0f, 3f);

            _rig.EnterTpsLock(target);

            Assert.That(_rig.Mode, Is.EqualTo(BoardCameraRig.CameraMode.TpsLock));
            Assert.That(_cameraGo.GetComponent<Camera>().orthographic, Is.False);

            Object.DestroyImmediate(target.gameObject);
        }

        [Test]
        public void EnterTpsLock_PositionsCameraBehindAndAboveTarget()
        {
            var target = new GameObject("TpsTarget").transform;
            target.position = new Vector3(3f, 0f, 3f);

            _rig.EnterTpsLock(target);

            Vector3 camPos = _cameraGo.transform.position;
            // Default facing is world +Z until the target moves — camera sits behind (lower Z) and
            // above (higher Y) the target.
            Assert.That(camPos.z, Is.LessThan(target.position.z));
            Assert.That(camPos.y, Is.GreaterThan(target.position.y));

            Object.DestroyImmediate(target.gameObject);
        }

        [Test]
        public void ExitTpsLock_RestoresOrthographicAndPriorOrbitFraming()
        {
            var boardCenter = new Vector3(4f, 0f, 5f);
            _rig.Init(_cameraGo.GetComponent<Camera>(), boardCenter);
            _rig.RotateBy(30f);
            var target = new GameObject("TpsTarget").transform;

            _rig.EnterTpsLock(target);
            _rig.ExitTpsLock();

            Assert.That(_rig.Mode, Is.EqualTo(BoardCameraRig.CameraMode.Overview));
            Assert.That(_cameraGo.GetComponent<Camera>().orthographic, Is.True);
            Assert.That(_rig.YawDegrees, Is.EqualTo(30f).Within(0.001f));
            float distance = Vector3.Distance(_cameraGo.transform.position, boardCenter);
            Assert.That(distance, Is.EqualTo(BoardCameraRig.DistanceFromCenter).Within(0.01f));

            Object.DestroyImmediate(target.gameObject);
        }

        [Test]
        public void RotateBy_NoOpWhileTpsLocked()
        {
            var target = new GameObject("TpsTarget").transform;
            _rig.EnterTpsLock(target);
            float yawBefore = _rig.YawDegrees;

            _rig.RotateBy(45f);

            Assert.That(_rig.YawDegrees, Is.EqualTo(yawBefore).Within(0.001f));
            Object.DestroyImmediate(target.gameObject);
        }

        [Test]
        public void ZoomBy_NoOpWhileTpsLocked()
        {
            var target = new GameObject("TpsTarget").transform;
            _rig.EnterTpsLock(target);
            float sizeBefore = _rig.OrthographicSize;

            _rig.ZoomBy(1f);

            Assert.That(_rig.OrthographicSize, Is.EqualTo(sizeBefore).Within(0.001f));
            Object.DestroyImmediate(target.gameObject);
        }

        [Test]
        public void PanBy_NoOpWhileTpsLocked()
        {
            var boardCenter = new Vector3(4f, 0f, 5f);
            _rig.Init(_cameraGo.GetComponent<Camera>(), boardCenter);
            var target = new GameObject("TpsTarget").transform;
            _rig.EnterTpsLock(target);
            int fired = 0;
            _rig.Rotated += () => fired++;

            _rig.PanBy(new Vector3(5f, 0f, 0f));

            Assert.That(fired, Is.EqualTo(0));
            Object.DestroyImmediate(target.gameObject);
        }

        [Test]
        public void CycleTpsLock_WithNoTargets_IsNoOp()
        {
            _rig.CycleTpsLock();

            Assert.That(_rig.Mode, Is.EqualTo(BoardCameraRig.CameraMode.Overview));
        }

        [Test]
        public void CycleTpsLock_StepsThroughTargetsThenBackToOverview()
        {
            var targetA = new GameObject("TpsTargetA").transform;
            var targetB = new GameObject("TpsTargetB").transform;
            _rig.SetTpsTargets(new[] { targetA, targetB });

            _rig.CycleTpsLock();
            Assert.That(_rig.Mode, Is.EqualTo(BoardCameraRig.CameraMode.TpsLock));
            Assert.That(_rig.TpsTargetIndex, Is.EqualTo(0));

            _rig.CycleTpsLock();
            Assert.That(_rig.Mode, Is.EqualTo(BoardCameraRig.CameraMode.TpsLock));
            Assert.That(_rig.TpsTargetIndex, Is.EqualTo(1));

            _rig.CycleTpsLock();
            Assert.That(_rig.Mode, Is.EqualTo(BoardCameraRig.CameraMode.Overview));
            Assert.That(_rig.TpsTargetIndex, Is.EqualTo(-1));

            Object.DestroyImmediate(targetA.gameObject);
            Object.DestroyImmediate(targetB.gameObject);
        }
    }
}
