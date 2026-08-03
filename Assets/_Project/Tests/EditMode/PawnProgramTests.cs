using LogiCard.Net;
using LogiCard.Sim;
using LogiCard.Timeline;
using NUnit.Framework;

namespace LogiCard.Tests.EditMode
{
    [TestFixture]
    public sealed class PawnProgramTests
    {
        [Test]
        public void QueueMove_ExpandsIntoPerTileWaypointsAtWalkCost()
        {
            var program = new PawnProgram(new GridCoordinate(0, 0), baseSecondsPerTile: 1f, budgetSeconds: 60f);

            bool ok = program.TryQueueMove(new GridCoordinate(2, 0), out string reason);

            float expected = TimeResourceMath.SegmentSeconds(2f, 1f, StanceType.Walk);
            Assert.That(ok, Is.True, reason);
            Assert.That(program.Nodes.Count, Is.EqualTo(2), "Two-tile path should book one Move node per tile.");
            Assert.That(program.UsedSeconds, Is.EqualTo(expected));
            Assert.That(program.Nodes[0].GridPosition, Is.EqualTo(new GridCoordinate(1, 0)));
            Assert.That(program.Nodes[0].ExecuteTime, Is.EqualTo(2f));
            Assert.That(program.Nodes[1].GridPosition, Is.EqualTo(new GridCoordinate(2, 0)));
            Assert.That(program.Nodes[1].ExecuteTime, Is.EqualTo(expected));
            Assert.That(program.CurrentPosition, Is.EqualTo(new GridCoordinate(2, 0)));
        }

        [Test]
        public void QueueMove_ExceedingBudget_IsRejectedAndStateUnchanged()
        {
            var program = new PawnProgram(new GridCoordinate(0, 0), baseSecondsPerTile: 1f, budgetSeconds: 2f);

            bool ok = program.TryQueueMove(new GridCoordinate(4, 0), out string reason);

            Assert.That(ok, Is.False);
            Assert.That(reason, Is.Not.Null.And.Not.Empty);
            Assert.That(program.UsedSeconds, Is.EqualTo(0f));
            Assert.That(program.CurrentPosition, Is.EqualTo(new GridCoordinate(0, 0)));
            Assert.That(program.Nodes, Is.Empty);
            Assert.That(program.HasDraft, Is.False);
        }

        [Test]
        public void DraftThenAllotSprint_CommitsFasterThanWalk()
        {
            var program = new PawnProgram(new GridCoordinate(0, 0), baseSecondsPerTile: 1f, budgetSeconds: 60f);

            Assert.That(program.TryDraftPath(new GridCoordinate(0, 2), out _), Is.True);
            Assert.That(program.DraftTileCount, Is.EqualTo(2));
            Assert.That(program.TrySetDraftStance(StanceType.Sprint, out _), Is.True);
            Assert.That(program.TryCommitDraft(out _), Is.True);

            Assert.That(program.UsedSeconds, Is.EqualTo(2f));
            Assert.That(program.Nodes[0].Stance, Is.EqualTo(StanceType.Sprint));
            Assert.That(program.CurrentStance, Is.EqualTo(StanceType.Sprint));
        }

        [Test]
        public void DraftThenSetStanceCrawl_UsesCrawlCost()
        {
            var program = new PawnProgram(new GridCoordinate(0, 0), baseSecondsPerTile: 1f, budgetSeconds: 60f);
            program.TryDraftPath(new GridCoordinate(3, 0), out _);

            float crawlCost = StanceAllotment.CostForTiles(3f, 1f, StanceType.Crawl);
            Assert.That(program.TrySetDraftStance(StanceType.Crawl, out _), Is.True);
            Assert.That(program.DraftStance, Is.EqualTo(StanceType.Crawl));
            Assert.That(program.DraftAllottedSeconds, Is.EqualTo(crawlCost));
        }

        [Test]
        public void AddWaypoint_AdjacentTile_AppendsOneStep()
        {
            var program = new PawnProgram(new GridCoordinate(0, 0), baseSecondsPerTile: 1f, budgetSeconds: 60f);

            Assert.That(program.TryAddWaypoint(new GridCoordinate(1, 0), out _), Is.True);
            Assert.That(program.TryAddWaypoint(new GridCoordinate(1, 1), out _), Is.True);
            Assert.That(program.DraftTileCount, Is.EqualTo(2));
            Assert.That(program.DraftWaypoints[1], Is.EqualTo(new GridCoordinate(1, 1)));
        }

        /// <summary>
        /// The actual C21 behavior change: a non-adjacent tap appends the shortest leg from the
        /// draft's tip, rather than replacing the whole draft with one system-computed path.
        /// </summary>
        [Test]
        public void AddWaypoint_NonAdjacentTile_AppendsShortestLegFromTip()
        {
            var program = new PawnProgram(new GridCoordinate(0, 0), baseSecondsPerTile: 1f, budgetSeconds: 60f);

            Assert.That(program.TryAddWaypoint(new GridCoordinate(0, 2), out _), Is.True);
            Assert.That(program.DraftTileCount, Is.EqualTo(2), "First waypoint: shortest leg from (0,0).");

            Assert.That(program.TryAddWaypoint(new GridCoordinate(2, 2), out _), Is.True);
            Assert.That(program.DraftTileCount, Is.EqualTo(4), "Second waypoint appends its own leg from (0,2), not a fresh path from (0,0).");
            Assert.That(program.DraftWaypoints[2], Is.EqualTo(new GridCoordinate(1, 2)));
            Assert.That(program.DraftWaypoints[3], Is.EqualTo(new GridCoordinate(2, 2)));
        }

        [Test]
        public void AddWaypoint_RetapPreviousWaypoint_Backtracks()
        {
            var program = new PawnProgram(new GridCoordinate(0, 0), baseSecondsPerTile: 1f, budgetSeconds: 60f);
            program.TryAddWaypoint(new GridCoordinate(1, 0), out _);
            program.TryAddWaypoint(new GridCoordinate(2, 0), out _);

            Assert.That(program.TryAddWaypoint(new GridCoordinate(1, 0), out _), Is.True);
            Assert.That(program.DraftTileCount, Is.EqualTo(1));
            Assert.That(program.DraftWaypoints[0], Is.EqualTo(new GridCoordinate(1, 0)));
        }

        [Test]
        public void QueueShoot_HoldAngle_CostsThreeSecondsAndTagsMode()
        {
            var program = new PawnProgram(new GridCoordinate(0, 0), baseSecondsPerTile: 1f, budgetSeconds: 60f);
            program.SetShootMode(ShootMode.HoldAngle);

            bool ok = program.TryQueueShoot(new GridCoordinate(3, 0), out string reason);

            Assert.That(ok, Is.True, reason);
            Assert.That(program.UsedSeconds, Is.EqualTo(ShootCost.HoldAngleSeconds));
            Assert.That(program.Nodes[0].ShootMode, Is.EqualTo(ShootMode.HoldAngle));
        }

        [Test]
        public void QueueShoot_AddsFixedSnapShotCostWithoutMovingPosition()
        {
            var program = new PawnProgram(new GridCoordinate(0, 0), baseSecondsPerTile: 1f, budgetSeconds: 60f);

            bool ok = program.TryQueueShoot(new GridCoordinate(3, 0), out string reason);

            Assert.That(ok, Is.True, reason);
            Assert.That(program.UsedSeconds, Is.EqualTo(ShootCost.SnapShotSeconds));
            Assert.That(program.CurrentPosition, Is.EqualTo(new GridCoordinate(0, 0)));
            Assert.That(program.Nodes[0].Verb, Is.EqualTo(ActionVerb.Shoot));
        }

        [Test]
        public void QueueShoot_OffRowAndColumn_IsRejected()
        {
            var program = new PawnProgram(new GridCoordinate(0, 0), baseSecondsPerTile: 1f, budgetSeconds: 60f);

            bool ok = program.TryQueueShoot(new GridCoordinate(2, 3), out string reason);

            Assert.That(ok, Is.False);
            Assert.That(reason, Is.Not.Null.And.Not.Empty);
            Assert.That(program.Nodes, Is.Empty);
        }

        [Test]
        public void QueueShoot_WhileSprinting_IsRejected()
        {
            var program = new PawnProgram(new GridCoordinate(0, 0), baseSecondsPerTile: 1f, budgetSeconds: 60f, startingStance: StanceType.Sprint);

            bool ok = program.TryQueueShoot(new GridCoordinate(3, 0), out string reason);

            Assert.That(ok, Is.False);
            Assert.That(reason, Is.Not.Null.And.Not.Empty);
        }

        [Test]
        public void QueueShoot_CommitsPendingDraftFirst()
        {
            var program = new PawnProgram(new GridCoordinate(0, 0), baseSecondsPerTile: 1f, budgetSeconds: 60f);
            program.TryDraftPath(new GridCoordinate(1, 0), out _);
            program.TrySetDraftStance(StanceType.Walk, out _);

            Assert.That(program.TryQueueShoot(new GridCoordinate(1, 3), out string reason), Is.True, reason);
            Assert.That(program.HasDraft, Is.False);
            Assert.That(program.Nodes[0].Verb, Is.EqualTo(ActionVerb.Move));
            Assert.That(program.Nodes[1].Verb, Is.EqualTo(ActionVerb.Shoot));
            Assert.That(program.CurrentPosition, Is.EqualTo(new GridCoordinate(1, 0)));
        }

        [Test]
        public void Build_ReturnsNodesInAscendingTimeWithCorrectVerbsAndNullModifiers()
        {
            var program = new PawnProgram(new GridCoordinate(0, 0), baseSecondsPerTile: 1f, budgetSeconds: 60f);
            program.TryQueueMove(new GridCoordinate(2, 0), out _);
            program.TryQueueShoot(new GridCoordinate(2, 4), out _);
            program.TryQueueMove(new GridCoordinate(2, 2), out _);

            TimelinePayload payload = program.Build();

            Assert.That(payload.Nodes.Count, Is.EqualTo(5), "Two 2-tile moves + one shoot.");
            Assert.That(payload.Nodes[0].Verb, Is.EqualTo(ActionVerb.Move));
            Assert.That(payload.Nodes[2].Verb, Is.EqualTo(ActionVerb.Shoot));
            Assert.That(payload.Nodes[0].ExecuteTime, Is.LessThan(payload.Nodes[2].ExecuteTime));
            Assert.That(payload.Nodes[2].ExecuteTime, Is.LessThan(payload.Nodes[4].ExecuteTime));
            foreach (ActionNode node in payload.Nodes)
            {
                Assert.That(node.Modifier, Is.Null);
            }
        }

        [Test]
        public void SprintPath_IsVisibleOnPreviewClockTiming()
        {
            var program = new PawnProgram(new GridCoordinate(0, 0), baseSecondsPerTile: 1f, budgetSeconds: 60f);
            program.TryQueueMove(new GridCoordinate(0, 3), out _, StanceType.Sprint);

            ScheduledPath preview = program.BuildMovePreviewPath(new GridCoordinate(0, 0));
            Assert.That(preview.EndSeconds, Is.EqualTo(3f));
            Assert.That(preview.Nodes.Count, Is.EqualTo(4), "Origin + 3 tile arrivals.");
        }
    }
}
