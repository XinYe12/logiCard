using LogiCard.Board;
using NUnit.Framework;
using UnityEngine;

namespace LogiCard.Tests.EditMode
{
    /// <summary>
    /// Pure yaw-stepping logic — no scene/bootstrap needed, just a GameObject + Camera the rig can
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
        public void Step_Positive_AdvancesByStepDegrees()
        {
            _rig.Step(1);
            Assert.That(_rig.YawDegrees, Is.EqualTo(BoardCameraRig.StepDegrees));
        }

        [Test]
        public void Step_Negative_RetreatsByStepDegrees()
        {
            _rig.Step(-1);
            Assert.That(_rig.YawDegrees, Is.EqualTo(360f - BoardCameraRig.StepDegrees).Within(0.001f));
        }

        [Test]
        public void Step_EightTimes_WrapsBackToZero()
        {
            int steps = Mathf.RoundToInt(360f / BoardCameraRig.StepDegrees);
            for (int i = 0; i < steps; i++)
            {
                _rig.Step(1);
            }

            Assert.That(_rig.YawDegrees, Is.EqualTo(0f).Within(0.001f));
        }

        [Test]
        public void Step_RaisesRotatedEvent()
        {
            int fired = 0;
            _rig.Rotated += () => fired++;

            _rig.Step(1);

            Assert.That(fired, Is.EqualTo(1));
        }

        [Test]
        public void Step_KeepsPitchFixed()
        {
            _rig.Step(1);
            Assert.That(_cameraGo.transform.rotation.eulerAngles.x, Is.EqualTo(BoardCameraRig.PitchDegrees).Within(0.01f));
        }

        [Test]
        public void Step_KeepsDistanceFromBoardCenterFixed()
        {
            var boardCenter = new Vector3(4f, 0f, 5f);
            _rig.Init(_cameraGo.GetComponent<Camera>(), boardCenter);

            _rig.Step(1);

            float distance = Vector3.Distance(_cameraGo.transform.position, boardCenter);
            Assert.That(distance, Is.EqualTo(BoardCameraRig.DistanceFromCenter).Within(0.01f));
        }
    }
}
