using LogiCard.UI;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.UI;

namespace LogiCard.Tests.EditMode
{
    /// <summary>
    /// Landscape geometry constants for Integrator camera-rect wiring (C48 / Phase 1), plus the
    /// shared UI factory overflow defaults and the bottom-dock vertical budget this polish pass locked.
    /// </summary>
    [TestFixture]
    public sealed class ProgramHudLayoutTests
    {
        [Test]
        public void HudDockIsBottomBandNotRightEdge()
        {
            Assert.That(ProgramHud.HudDockHeight, Is.EqualTo(0.34f));
            Assert.That(ProgramHud.TopStripHeight, Is.EqualTo(0.08f));
            Assert.That(ProgramHud.HudDockHeight + ProgramHud.TopStripHeight, Is.LessThan(1f),
                "Board region must remain the majority of the frame.");
        }

        [Test]
        public void CanvasScalerDefaultsBiasWidthSlightlyForDockType()
        {
            Assert.That(UiStyle.ReferenceResolution, Is.EqualTo(new Vector2(1920f, 1080f)));
            Assert.That(UiStyle.CanvasMatchWidthOrHeight, Is.EqualTo(0.4f));
            Assert.That(UiStyle.Pad, Is.EqualTo(16f));
            Assert.That(UiStyle.Gap, Is.EqualTo(8f));
            Assert.That(UiStyle.RowGap, Is.EqualTo(8f));
        }

        [Test]
        public void ControlsColumnFitsDockAtReferenceAndUltrawide()
        {
            float content = ProgramHud.ControlsColumnContentHeight;
            Assert.That(content, Is.GreaterThan(200f), "Budget should reflect a real stacked column.");

            float refDock = ProgramHud.DockHeightInUiUnits(1920f, 1080f);
            float hdDock = ProgramHud.DockHeightInUiUnits(1280f, 720f);
            float ultraDock = ProgramHud.DockHeightInUiUnits(2560f, 1080f);

            Assert.That(content, Is.LessThanOrEqualTo(refDock),
                $"Controls column {content:0.#} must fit 1920×1080 dock {refDock:0.#} UI units.");
            Assert.That(content, Is.LessThanOrEqualTo(hdDock),
                $"Controls column {content:0.#} must fit 1280×720 dock {hdDock:0.#} UI units.");
            Assert.That(content, Is.LessThanOrEqualTo(ultraDock),
                $"Controls column {content:0.#} must fit 2560×1080 dock {ultraDock:0.#} UI units — this is the overflow that shipped with the dock move.");
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
