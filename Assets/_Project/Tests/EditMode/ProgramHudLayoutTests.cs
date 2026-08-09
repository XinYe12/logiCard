using LogiCard.UI;
using NUnit.Framework;

namespace LogiCard.Tests.EditMode
{
    /// <summary>
    /// Landscape geometry constants for Integrator camera-rect wiring (C48 / Phase 1).
    /// </summary>
    [TestFixture]
    public sealed class ProgramHudLayoutTests
    {
        [Test]
        public void HudDockIsRightEdgeNotBottomThumbBand()
        {
            Assert.That(ProgramHud.HudDockWidth, Is.EqualTo(0.30f));
            Assert.That(ProgramHud.HudDockHeight, Is.EqualTo(0f));
            Assert.That(ProgramHud.TopStripHeight, Is.EqualTo(0.08f));
            Assert.That(ProgramHud.ThumbZoneHeight, Is.EqualTo(0f),
                "ThumbZoneHeight is a compile-compat alias and must equal HudDockHeight while the dock is on the right.");
        }
    }
}
