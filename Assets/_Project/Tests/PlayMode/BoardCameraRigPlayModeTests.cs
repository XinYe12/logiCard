using LogiCard.Board;
using NUnit.Framework;

namespace LogiCard.Tests.PlayMode
{
    /// <summary>
    /// End-to-end wiring: GameBootstrap builds a real BoardCameraRig on the scripted camera and
    /// connects its Rotated event to the door-prompt refresh hook. Actual mouse-drag input isn't
    /// simulated here (that's BoardCameraRig.Update, not testable without an input-simulation layer
    /// this project doesn't have) - these tests drive the same RotateBy primitive Update calls.
    /// </summary>
    [TestFixture]
    public sealed class BoardCameraRigPlayModeTests : SliceSceneFixture
    {
        [Test]
        public void BootstrapBuildsACameraRig()
        {
            Assert.That(Bootstrap.CameraRig, Is.Not.Null);
        }

        [Test]
        public void RotatingChangesYaw()
        {
            BoardCameraRig rig = Bootstrap.CameraRig;
            float before = rig.YawDegrees;

            rig.RotateBy(22f);

            Assert.That(rig.YawDegrees, Is.EqualTo(before + 22f).Within(0.001f));
        }

        [Test]
        public void RotatingDoesNotThrowWithNoDoorSelected()
        {
            // Rotated fires RefreshBoardAnchoredUI on every call - the common case (not currently in
            // Door mode with a pending door) must be a safe no-op, not a null-reference off the
            // missing selection.
            BoardCameraRig rig = Bootstrap.CameraRig;
            Assert.That(() => rig.RotateBy(10f), Throws.Nothing);
        }

        [Test]
        public void ZoomingChangesOrthographicSize()
        {
            // Zoom out (+size), not in — baseline 5.0 minus 1.0 would hit MinOrthographicSize (4.2)
            // and clamp, which is covered by ZoomingIsClampedWithinAnalyticBounds instead.
            BoardCameraRig rig = Bootstrap.CameraRig;
            float before = rig.OrthographicSize;

            rig.ZoomBy(1f);

            Assert.That(rig.OrthographicSize, Is.EqualTo(before + 1f).Within(0.001f));
        }

        [Test]
        public void ZoomingDoesNotThrowWithNoDoorSelected()
        {
            // Zoom reuses the same Rotated event as yaw (see BoardCameraRig's class doc) specifically
            // so it gets the same GameBootstrap-wired RefreshBoardAnchoredUI invalidation - confirm
            // that path is exercised and safe here too, not just for RotateBy.
            BoardCameraRig rig = Bootstrap.CameraRig;
            Assert.That(() => rig.ZoomBy(1f), Throws.Nothing);
        }

        [Test]
        public void ZoomingIsClampedWithinAnalyticBounds()
        {
            BoardCameraRig rig = Bootstrap.CameraRig;

            rig.ZoomBy(-100f);
            Assert.That(rig.OrthographicSize, Is.EqualTo(BoardCameraRig.MinOrthographicSize).Within(0.001f));

            rig.ZoomBy(100f);
            Assert.That(rig.OrthographicSize, Is.EqualTo(BoardCameraRig.MaxOrthographicSize).Within(0.001f));
        }
    }
}
