using LogiCard.Board;
using NUnit.Framework;
using UnityEngine.UI;

namespace LogiCard.Tests.PlayMode
{
    /// <summary>
    /// End-to-end wiring: HUD button -> GameBootstrap -> BoardCameraRig -> door-prompt refresh hook.
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
        public void RotateButtonExistsAndStepsTheRig()
        {
            BoardCameraRig rig = Bootstrap.CameraRig;
            float before = rig.YawDegrees;

            Button rotateButton = FindByName<Button>("CameraRotateButton");
            Assert.That(rotateButton, Is.Not.Null, "Top strip has no CameraRotateButton.");

            rotateButton.onClick.Invoke();

            Assert.That(rig.YawDegrees, Is.EqualTo(before + BoardCameraRig.StepDegrees).Within(0.001f));
        }

        [Test]
        public void RotatingDoesNotThrowWithNoDoorSelected()
        {
            // RefreshBoardAnchoredUI runs on every rotation regardless of mode/selection - the common
            // case (not currently in Door mode with a pending door) must be a safe no-op, not a
            // null-reference off the missing selection.
            Button rotateButton = FindByName<Button>("CameraRotateButton");
            Assert.That(() => rotateButton.onClick.Invoke(), Throws.Nothing);
        }
    }
}
