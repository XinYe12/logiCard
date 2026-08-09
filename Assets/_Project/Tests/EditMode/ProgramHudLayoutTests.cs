using LogiCard.UI;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.UI;

namespace LogiCard.Tests.EditMode
{
    /// <summary>
    /// Landscape geometry constants for Integrator camera-rect wiring (C48 / Phase 1), plus the
    /// shared UI factory overflow defaults this wave locked.
    /// </summary>
    [TestFixture]
    public sealed class ProgramHudLayoutTests
    {
        [Test]
        public void HudDockIsRightEdgeNotBottomThumbBand()
        {
            Assert.That(ProgramHud.HudDockWidth, Is.EqualTo(0.34f));
            Assert.That(ProgramHud.HudDockHeight, Is.EqualTo(0f));
            Assert.That(ProgramHud.TopStripHeight, Is.EqualTo(0.08f));
            Assert.That(ProgramHud.ThumbZoneHeight, Is.EqualTo(0f),
                "ThumbZoneHeight is a compile-compat alias and must equal HudDockHeight while the dock is on the right.");
        }

        [Test]
        public void CanvasScalerDefaultsBiasHeightForDockReadability()
        {
            Assert.That(UiStyle.ReferenceResolution, Is.EqualTo(new Vector2(1920f, 1080f)));
            Assert.That(UiStyle.CanvasMatchWidthOrHeight, Is.EqualTo(0.4f));
        }

        [Test]
        public void UiFactoryButtonOverflowWrapsInsteadOfBleeding()
        {
            var go = new GameObject("OverflowProbe", typeof(Text));
            var text = go.GetComponent<Text>();
            UiFactory.ApplyOverflow(text, UiTextOverflow.Button);
            Assert.That(text.horizontalOverflow, Is.EqualTo(HorizontalWrapMode.Wrap));
            Assert.That(text.verticalOverflow, Is.EqualTo(VerticalWrapMode.Truncate));
            Object.DestroyImmediate(go);
        }

        [Test]
        public void UiFactoryBodyOverflowWrapsHorizontally()
        {
            var go = new GameObject("BodyOverflowProbe", typeof(Text));
            var text = go.GetComponent<Text>();
            UiFactory.ApplyOverflow(text, UiTextOverflow.Body);
            Assert.That(text.horizontalOverflow, Is.EqualTo(HorizontalWrapMode.Wrap));
            Assert.That(text.verticalOverflow, Is.EqualTo(VerticalWrapMode.Overflow));
            Object.DestroyImmediate(go);
        }
    }
}
