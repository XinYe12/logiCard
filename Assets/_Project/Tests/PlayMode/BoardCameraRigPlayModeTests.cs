using LogiCard.Board;
using LogiCard.Sim;
using NUnit.Framework;
using UnityEngine;

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
            // Zoom out (+size), not in — a large negative delta from the ConfigureCamera default can
            // hit MinOrthographicSize and clamp, which is covered by ZoomingIsClampedWithinAnalyticBounds
            // instead. Keep the in-bounds +delta pattern so this asserts real motion, not a clamp.
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

        [Test]
        public void PanningIsClampedToTheRealMapsBounds()
        {
            // ConfigureCamera wires SetPanBounds from the actual built ArenaBoard (2026-08-15) — this
            // exercises that wiring against the real map, not a synthetic bounds rect like the EditMode
            // coverage does.
            BoardCameraRig rig = Bootstrap.CameraRig;
            Vector3 maxCorner = BoardVisual.WorldFromPlanar(
                new PlanarPosition(BoardVisual.Model.MaxX, BoardVisual.Model.MaxY));

            rig.PanBy(new Vector3(10_000f, 0f, 10_000f));

            Assert.That(rig.transform.position.x, Is.LessThanOrEqualTo(maxCorner.x + BoardCameraRig.DistanceFromCenter));
            Assert.That(rig.transform.position.z, Is.LessThanOrEqualTo(maxCorner.z + BoardCameraRig.DistanceFromCenter));
        }

        [Test]
        public void CyclingTpsLockStepsThroughAttackerThenDefenderThenOverview()
        {
            // GameBootstrap wires SetTpsTargets to [attacker, defender] (2026-08-15) — this is end-to-end
            // wiring coverage, not the pure cycle-order logic already covered in EditMode.
            BoardCameraRig rig = Bootstrap.CameraRig;

            rig.CycleTpsLock();
            Assert.That(rig.Mode, Is.EqualTo(BoardCameraRig.CameraMode.TpsLock));
            Assert.That(rig.TpsTargetIndex, Is.EqualTo(0));

            rig.CycleTpsLock();
            Assert.That(rig.Mode, Is.EqualTo(BoardCameraRig.CameraMode.TpsLock));
            Assert.That(rig.TpsTargetIndex, Is.EqualTo(1));

            rig.CycleTpsLock();
            Assert.That(rig.Mode, Is.EqualTo(BoardCameraRig.CameraMode.Overview));
            Assert.That(rig.TpsTargetIndex, Is.EqualTo(-1));
            Assert.That(Camera.main.orthographic, Is.True);
        }

        [Test]
        public void TpsLock_SuppressesProgramPhaseBoardClicks_OverviewDoesNot()
        {
            // Same south-Yard screen-click shape BoardInputPlayModeTests already exercises for the
            // ortho camera — here the point is the camera-mode gate (2026-08-15), not the raycast math.
            BoardCameraRig rig = Bootstrap.CameraRig;
            var yardPoint = new PlanarPosition(4f, 2f);
            Vector3 screen = Camera.main.WorldToScreenPoint(BoardVisual.WorldFromPlanar(yardPoint));

            Assert.That(AttackerInput.TryClickAtScreenPosition(screen), Is.True,
                "Overview mode must still accept board clicks (unchanged behavior).");

            rig.CycleTpsLock();
            Assert.That(rig.Mode, Is.EqualTo(BoardCameraRig.CameraMode.TpsLock));

            Assert.That(AttackerInput.TryClickAtScreenPosition(screen), Is.False,
                "Program-phase board clicks must be scoped out while TPS-locked.");
        }
    }
}
