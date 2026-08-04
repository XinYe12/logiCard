using LogiCard.Sim;
using NUnit.Framework;

namespace LogiCard.Tests.EditMode
{
    // OrthogonalPathfinderTests deleted here (C35/C39 pivot, Phase 3) — PawnProgram no longer uses
    // OrthogonalPathfinder, and continuous-space pathfinding coverage lives in
    // ContinuousPathfinderTests.cs instead (see docs/CONTINUOUS_PIVOT_PLAN.md §D).

    [TestFixture]
    public sealed class StanceAllotmentTests
    {
        [Test]
        public void CostForTiles_MatchesTimeResourceMathMultipliers()
        {
            Assert.That(StanceAllotment.CostForTiles(3f, 1f, StanceType.Sprint), Is.EqualTo(3f));
            Assert.That(StanceAllotment.CostForTiles(3f, 1f, StanceType.Walk), Is.EqualTo(6f));
            Assert.That(StanceAllotment.CostForTiles(3f, 1f, StanceType.Crawl), Is.EqualTo(12f));
        }
    }
}
