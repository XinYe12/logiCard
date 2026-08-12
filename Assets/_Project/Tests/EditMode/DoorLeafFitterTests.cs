using LogiCard.Board;
using NUnit.Framework;
using UnityEngine;

namespace LogiCard.Tests.EditMode
{
    /// <summary>
    /// Presentation fit for nappin-style door leaves (width on Z) under a Y-hinge.
    /// </summary>
    [TestFixture]
    public sealed class DoorLeafFitterTests
    {
        private GameObject _hingeGo;

        [SetUp]
        public void SetUp()
        {
            _hingeGo = new GameObject("Hinge");
        }

        [TearDown]
        public void TearDown()
        {
            if (_hingeGo != null)
            {
                Object.DestroyImmediate(_hingeGo);
            }
        }

        [Test]
        public void FitUnderHinge_ReorientsWidthOnZ_SoLeafSwingsFromHingeEdge()
        {
            // Simulate nappin (Prb)Door: thin on X, tall on Y, wide on Z.
            GameObject leafGo = GameObject.CreatePrimitive(PrimitiveType.Cube);
            leafGo.name = "Leaf";
            leafGo.transform.SetParent(_hingeGo.transform, false);
            leafGo.transform.localScale = new Vector3(0.15f, 2f, 1.5f);

            const float width = 1.2f;
            const float height = 0.8f;
            const float thickness = 0.14f;
            Transform mount = DoorLeafFitter.FitUnderHinge(
                _hingeGo.transform, leafGo.transform, width, height, thickness);

            Assert.That(mount.parent, Is.EqualTo(_hingeGo.transform));

            Bounds fitted = DoorLeafFitter.CalculateBoundsInAncestor(_hingeGo.transform, leafGo);
            Assert.That(fitted.size.x, Is.EqualTo(width).Within(0.02f), "width on +X");
            Assert.That(fitted.size.y, Is.EqualTo(height).Within(0.02f), "height on +Y");
            Assert.That(fitted.size.z, Is.EqualTo(thickness).Within(0.02f), "thickness on Z");
            Assert.That(fitted.min.x, Is.EqualTo(0f).Within(0.02f), "hinge edge at x=0");
            Assert.That(fitted.min.y, Is.EqualTo(0f).Within(0.02f), "floor at y=0");
            Assert.That(fitted.center.z, Is.EqualTo(0f).Within(0.02f), "centered in wall thickness");
        }

        [Test]
        public void FitUnderHinge_LeavesWidthOnX_Unrotated()
        {
            // SeparatorDoor-shaped: already width on X.
            GameObject leafGo = GameObject.CreatePrimitive(PrimitiveType.Cube);
            leafGo.name = "Leaf";
            leafGo.transform.SetParent(_hingeGo.transform, false);
            leafGo.transform.localScale = new Vector3(1.4f, 2.5f, 0.12f);

            DoorLeafFitter.FitUnderHinge(_hingeGo.transform, leafGo.transform, 1f, 1f, 0.14f);

            Assert.That(leafGo.transform.localRotation.eulerAngles.y, Is.EqualTo(0f).Within(0.1f));
        }
    }
}
