using LogiCard.UI;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.UI;

namespace LogiCard.Tests.EditMode
{
    /// <summary>
    /// Match Shell Layout geometry (docs/ui/MATCH_SHELL_LAYOUT.md, 2026-08-15): the five
    /// InfoBar/MapViewport/HandBand/ToolBar/TimelineSchedule band fractions, the camera-rect
    /// constants they still feed (<c>GameBootstrap.ConfigureCamera</c> reads
    /// <see cref="ProgramHud.HudDockHeight"/>/<see cref="ProgramHud.TopStripHeight"/> unchanged), plus
    /// the shared UI factory overflow defaults.
    /// </summary>
    [TestFixture]
    public sealed class ProgramHudLayoutTests
    {
        [Test]
        public void MatchShellBandsStackInLockedOrderAndSumToFullHeight()
        {
            float sum = ProgramHud.InfoBarHeight + ProgramHud.MapViewportHeight + ProgramHud.HandBandHeight
                + ProgramHud.ToolBarHeight + ProgramHud.TimelineHeight;
            Assert.That(sum, Is.EqualTo(1f).Within(0.0001f),
                "InfoBar + MapViewport + HandBand + ToolBar + TimelineSchedule must cover the full frame.");

            Assert.That(ProgramHud.MapViewportHeight, Is.GreaterThan(ProgramHud.InfoBarHeight),
                "MapViewport must stay the single largest region (MATCH_SHELL_LAYOUT.md).");
            Assert.That(ProgramHud.MapViewportHeight, Is.GreaterThan(ProgramHud.HandBandHeight));
            Assert.That(ProgramHud.MapViewportHeight, Is.GreaterThan(ProgramHud.ToolBarHeight));
            Assert.That(ProgramHud.MapViewportHeight, Is.GreaterThan(ProgramHud.TimelineHeight));

            Assert.That(ProgramHud.TimelineHeight, Is.GreaterThanOrEqualTo(0.12f),
                "TimelineSchedule must not collapse under ~12% when expanded (MATCH_SHELL_LAYOUT.md).");
        }

        [Test]
        public void CameraRectConstantsStillMatchMapViewportRect()
        {
            // GameBootstrap.ConfigureCamera computes cam.rect from these two constants alone (it is
            // not this UI seat's file to touch this wave) — they must stay numerically equivalent to
            // the MapViewport band so the camera hole and the UI hole line up without a Boot change.
            Assert.That(ProgramHud.TopStripHeight, Is.EqualTo(ProgramHud.InfoBarHeight));
            Assert.That(ProgramHud.HudDockHeight,
                Is.EqualTo(ProgramHud.HandBandHeight + ProgramHud.ToolBarHeight + ProgramHud.TimelineHeight).Within(0.0001f));
            Assert.That(1f - ProgramHud.HudDockHeight - ProgramHud.TopStripHeight,
                Is.EqualTo(ProgramHud.MapViewportHeight).Within(0.0001f));
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
        public void ToolBarAndTimelineContentFitTheirBandsAtReferenceAndUltrawide()
        {
            AssertFits(ProgramHud.ToolBarControlsContentHeight, ProgramHud.ToolBarHeight, "ToolBar controls zone");
            AssertFits(ProgramHud.ToolBarActionsContentHeight, ProgramHud.ToolBarHeight, "ToolBar actions zone");
            AssertFits(ProgramHud.TimelineScheduleContentHeight, ProgramHud.TimelineHeight, "TimelineSchedule column");
        }

        private static void AssertFits(float content, float bandHeightFraction, string label)
        {
            Assert.That(content, Is.GreaterThan(0f), $"{label} budget should reflect real stacked content.");

            float refBand = ProgramHud.BandHeightInUiUnits(bandHeightFraction, 1920f, 1080f);
            float hdBand = ProgramHud.BandHeightInUiUnits(bandHeightFraction, 1280f, 720f);
            float ultraBand = ProgramHud.BandHeightInUiUnits(bandHeightFraction, 2560f, 1080f);

            Assert.That(content, Is.LessThanOrEqualTo(refBand),
                $"{label} content {content:0.#} must fit 1920×1080 band {refBand:0.#} UI units.");
            Assert.That(content, Is.LessThanOrEqualTo(hdBand),
                $"{label} content {content:0.#} must fit 1280×720 band {hdBand:0.#} UI units.");
            Assert.That(content, Is.LessThanOrEqualTo(ultraBand),
                $"{label} content {content:0.#} must fit 2560×1080 band {ultraBand:0.#} UI units — the tightest case.");
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
