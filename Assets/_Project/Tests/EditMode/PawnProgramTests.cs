using LogiCard.Net;
using LogiCard.Sim;
using LogiCard.Timeline;
using NUnit.Framework;

namespace LogiCard.Tests.EditMode
{
    /// <summary>
    /// Retargeted onto continuous space (C35/C39 pivot, Phase 3) — <see cref="PlanarPosition"/>/
    /// <see cref="ArenaBoard"/> replace <c>GridCoordinate</c>/<c>GridBoard</c>. On a clear board,
    /// <see cref="ContinuousPathfinder"/> books a single straight-line waypoint per leg (no more
    /// one-node-per-tile expansion), so node counts are lower than the old grid tests' — the cost
    /// math itself is unchanged (Euclidean distance replaces tile-count, but the numbers coincide
    /// for these axis-aligned distances).
    /// </summary>
    [TestFixture]
    public sealed class PawnProgramTests
    {
        [Test]
        public void QueueMove_StraightLineIsOneMoveNodeAtWalkCost()
        {
            var program = new PawnProgram(new PlanarPosition(0, 0), baseSecondsPerTile: 1f, budgetSeconds: 60f);

            bool ok = program.TryQueueMove(new PlanarPosition(2, 0), out string reason);

            float expected = TimeResourceMath.SegmentSeconds(2f, 1f, StanceType.Walk);
            Assert.That(ok, Is.True, reason);
            Assert.That(program.Nodes.Count, Is.EqualTo(1), "A clear straight line books a single Move node to the destination.");
            Assert.That(program.UsedSeconds, Is.EqualTo(expected));
            Assert.That(program.Nodes[0].Position, Is.EqualTo(new PlanarPosition(2, 0)));
            Assert.That(program.Nodes[0].ExecuteTime, Is.EqualTo(expected));
            Assert.That(program.CurrentPosition, Is.EqualTo(new PlanarPosition(2, 0)));
        }

        [Test]
        public void QueueMove_ExceedingBudget_IsRejectedAndStateUnchanged()
        {
            var program = new PawnProgram(new PlanarPosition(0, 0), baseSecondsPerTile: 1f, budgetSeconds: 2f);

            bool ok = program.TryQueueMove(new PlanarPosition(4, 0), out string reason);

            Assert.That(ok, Is.False);
            Assert.That(reason, Is.Not.Null.And.Not.Empty);
            Assert.That(program.UsedSeconds, Is.EqualTo(0f));
            Assert.That(program.CurrentPosition, Is.EqualTo(new PlanarPosition(0, 0)));
            Assert.That(program.Nodes, Is.Empty);
            Assert.That(program.HasDraft, Is.False);
        }

        [Test]
        public void DraftThenAllotSprint_CommitsFasterThanWalk()
        {
            var program = new PawnProgram(new PlanarPosition(0, 0), baseSecondsPerTile: 1f, budgetSeconds: 60f);

            Assert.That(program.TryDraftPath(new PlanarPosition(0, 2), out _), Is.True);
            Assert.That(program.DraftDistance, Is.EqualTo(2f));
            Assert.That(program.TrySetDraftStance(StanceType.Sprint, out _), Is.True);
            Assert.That(program.TryCommitDraft(out _), Is.True);

            Assert.That(program.UsedSeconds, Is.EqualTo(2f));
            Assert.That(program.Nodes[0].Stance, Is.EqualTo(StanceType.Sprint));
            Assert.That(program.CurrentStance, Is.EqualTo(StanceType.Sprint));
        }

        [Test]
        public void DraftThenSetStanceCrawl_UsesCrawlCost()
        {
            var program = new PawnProgram(new PlanarPosition(0, 0), baseSecondsPerTile: 1f, budgetSeconds: 60f);
            program.TryDraftPath(new PlanarPosition(3, 0), out _);

            float crawlCost = StanceAllotment.CostForTiles(3f, 1f, StanceType.Crawl);
            Assert.That(program.TrySetDraftStance(StanceType.Crawl, out _), Is.True);
            Assert.That(program.DraftStance, Is.EqualTo(StanceType.Crawl));
            Assert.That(program.DraftAllottedSeconds, Is.EqualTo(crawlCost));
        }

        [Test]
        public void AddWaypoint_TwoStraightLegs_AppendsOneWaypointEach()
        {
            var program = new PawnProgram(new PlanarPosition(0, 0), baseSecondsPerTile: 1f, budgetSeconds: 60f);

            Assert.That(program.TryAddWaypoint(new PlanarPosition(1, 0), out _), Is.True);
            Assert.That(program.TryAddWaypoint(new PlanarPosition(1, 1), out _), Is.True);
            Assert.That(program.DraftWaypointCount, Is.EqualTo(2));
            Assert.That(program.DraftWaypoints[1], Is.EqualTo(new PlanarPosition(1, 1)));
        }

        /// <summary>
        /// The actual C21 behavior change: a second tap appends the shortest leg from the draft's
        /// tip, rather than replacing the whole draft with one system-computed path from the origin.
        /// On a clear board the origin-to-destination route would also collapse to a single waypoint,
        /// so the append-vs-replace distinction shows up as waypoint *count* (2, preserving the first
        /// leg's stop) rather than a different route shape.
        /// </summary>
        [Test]
        public void AddWaypoint_SecondLeg_AppendsFromTipNotFromOrigin()
        {
            var program = new PawnProgram(new PlanarPosition(0, 0), baseSecondsPerTile: 1f, budgetSeconds: 60f);

            Assert.That(program.TryAddWaypoint(new PlanarPosition(0, 2), out _), Is.True);
            Assert.That(program.DraftWaypointCount, Is.EqualTo(1), "First waypoint: shortest leg from (0,0).");

            Assert.That(program.TryAddWaypoint(new PlanarPosition(2, 2), out _), Is.True);
            Assert.That(program.DraftWaypointCount, Is.EqualTo(2),
                "Second waypoint appends its own leg from (0,2); a fresh path from (0,0) would collapse to a single waypoint on a clear board.");
            Assert.That(program.DraftWaypoints[0], Is.EqualTo(new PlanarPosition(0, 2)));
            Assert.That(program.DraftWaypoints[1], Is.EqualTo(new PlanarPosition(2, 2)));
        }

        [Test]
        public void AddWaypoint_RetapPreviousWaypoint_Backtracks()
        {
            var program = new PawnProgram(new PlanarPosition(0, 0), baseSecondsPerTile: 1f, budgetSeconds: 60f);
            program.TryAddWaypoint(new PlanarPosition(1, 0), out _);
            program.TryAddWaypoint(new PlanarPosition(2, 0), out _);

            Assert.That(program.TryAddWaypoint(new PlanarPosition(1, 0), out _), Is.True);
            Assert.That(program.DraftWaypointCount, Is.EqualTo(1));
            Assert.That(program.DraftWaypoints[0], Is.EqualTo(new PlanarPosition(1, 0)));
        }

        [Test]
        public void TryUndoLastDraftStep_RepeatedCalls_WalksBackOneStepAtATime()
        {
            var program = new PawnProgram(new PlanarPosition(0, 0), baseSecondsPerTile: 1f, budgetSeconds: 60f);
            program.TryAddWaypoint(new PlanarPosition(1, 0), out _);
            program.TryAddWaypoint(new PlanarPosition(2, 0), out _);
            program.TryAddWaypoint(new PlanarPosition(3, 0), out _);
            Assert.That(program.DraftWaypointCount, Is.EqualTo(3));

            Assert.That(program.TryUndoLastDraftStep(out _), Is.True);
            Assert.That(program.DraftWaypointCount, Is.EqualTo(2));
            Assert.That(program.DraftWaypoints[1], Is.EqualTo(new PlanarPosition(2, 0)));

            Assert.That(program.TryUndoLastDraftStep(out _), Is.True);
            Assert.That(program.DraftWaypointCount, Is.EqualTo(1));
            Assert.That(program.DraftWaypoints[0], Is.EqualTo(new PlanarPosition(1, 0)));

            Assert.That(program.TryUndoLastDraftStep(out _), Is.True);
            Assert.That(program.HasDraft, Is.False);

            Assert.That(program.TryUndoLastDraftStep(out string reason), Is.False);
            Assert.That(reason, Is.Not.Null.And.Not.Empty);
        }

        [Test]
        public void TryUndoLastStep_WalksBackThroughCommittedMoveAndShoot()
        {
            // BUG FOUND 2026-08-05: undo used to require an active draft, so a program like
            // Move -> Move -> Shoot -> (drafting a third Move) could only ever undo back to right
            // after the Shoot — the two committed Moves and the Shoot itself were unreachable.
            var program = new PawnProgram(new PlanarPosition(0, 0), baseSecondsPerTile: 1f, budgetSeconds: 60f);

            Assert.That(program.TryQueueMove(new PlanarPosition(1, 0), out _), Is.True);
            float usedAfterFirstMove = program.UsedSeconds;

            Assert.That(program.TryQueueMove(new PlanarPosition(2, 0), out _), Is.True);
            float usedAfterSecondMove = program.UsedSeconds;
            Assert.That(program.CurrentPosition, Is.EqualTo(new PlanarPosition(2, 0)));

            Assert.That(program.TryQueueShoot(new PlanarPosition(2, 3), out _), Is.True);
            Assert.That(program.Nodes.Count, Is.EqualTo(3));

            Assert.That(program.CanUndoLastStep, Is.True);
            Assert.That(program.TryUndoLastStep(out _), Is.True);
            Assert.That(program.Nodes.Count, Is.EqualTo(2), "First undo should remove only the Shoot.");
            Assert.That(program.UsedSeconds, Is.EqualTo(usedAfterSecondMove).Within(0.0001f));

            Assert.That(program.TryUndoLastStep(out _), Is.True);
            Assert.That(program.Nodes.Count, Is.EqualTo(1), "Second undo should remove the second committed Move.");
            Assert.That(program.CurrentPosition, Is.EqualTo(new PlanarPosition(1, 0)));
            Assert.That(program.UsedSeconds, Is.EqualTo(usedAfterFirstMove).Within(0.0001f));

            Assert.That(program.TryUndoLastStep(out _), Is.True);
            Assert.That(program.Nodes, Is.Empty, "Third undo should remove the first committed Move too.");
            Assert.That(program.CurrentPosition, Is.EqualTo(new PlanarPosition(0, 0)));
            Assert.That(program.UsedSeconds, Is.EqualTo(0f).Within(0.0001f));

            Assert.That(program.CanUndoLastStep, Is.False);
            Assert.That(program.TryUndoLastStep(out string reason), Is.False);
            Assert.That(reason, Is.Not.Null.And.Not.Empty);
        }

        [Test]
        public void QueueShoot_HoldAngle_CostsThreeSecondsAndTagsMode()
        {
            var program = new PawnProgram(new PlanarPosition(0, 0), baseSecondsPerTile: 1f, budgetSeconds: 60f);
            program.SetShootMode(ShootMode.HoldAngle);

            bool ok = program.TryQueueShoot(new PlanarPosition(3, 0), out string reason);

            Assert.That(ok, Is.True, reason);
            Assert.That(program.UsedSeconds, Is.EqualTo(ShootCost.HoldAngleSeconds));
            Assert.That(program.Nodes[0].ShootMode, Is.EqualTo(ShootMode.HoldAngle));
        }

        [Test]
        public void QueueShoot_AddsFixedSnapShotCostWithoutMovingPosition()
        {
            var program = new PawnProgram(new PlanarPosition(0, 0), baseSecondsPerTile: 1f, budgetSeconds: 60f);

            bool ok = program.TryQueueShoot(new PlanarPosition(3, 0), out string reason);

            Assert.That(ok, Is.True, reason);
            Assert.That(program.UsedSeconds, Is.EqualTo(ShootCost.SnapShotSeconds));
            Assert.That(program.CurrentPosition, Is.EqualTo(new PlanarPosition(0, 0)));
            Assert.That(program.Nodes[0].Verb, Is.EqualTo(ActionVerb.Shoot));
        }

        [Test]
        public void QueueShoot_OutOfBounds_IsRejected()
        {
            var program = new PawnProgram(new PlanarPosition(0, 0), baseSecondsPerTile: 1f, budgetSeconds: 60f);

            bool ok = program.TryQueueShoot(new PlanarPosition(9, 9), out string reason);

            Assert.That(ok, Is.False);
            Assert.That(reason, Is.Not.Null.And.Not.Empty);
            Assert.That(program.Nodes, Is.Empty);
        }

        [Test]
        public void QueueShoot_WhileSprinting_IsRejected()
        {
            var program = new PawnProgram(new PlanarPosition(0, 0), baseSecondsPerTile: 1f, budgetSeconds: 60f, startingStance: StanceType.Sprint);

            bool ok = program.TryQueueShoot(new PlanarPosition(3, 0), out string reason);

            Assert.That(ok, Is.False);
            Assert.That(reason, Is.Not.Null.And.Not.Empty);
        }

        [Test]
        public void QueueShoot_CommitsPendingDraftFirst()
        {
            var program = new PawnProgram(new PlanarPosition(0, 0), baseSecondsPerTile: 1f, budgetSeconds: 60f);
            program.TryDraftPath(new PlanarPosition(1, 0), out _);
            program.TrySetDraftStance(StanceType.Walk, out _);

            Assert.That(program.TryQueueShoot(new PlanarPosition(1, 3), out string reason), Is.True, reason);
            Assert.That(program.HasDraft, Is.False);
            Assert.That(program.Nodes[0].Verb, Is.EqualTo(ActionVerb.Move));
            Assert.That(program.Nodes[1].Verb, Is.EqualTo(ActionVerb.Shoot));
            Assert.That(program.CurrentPosition, Is.EqualTo(new PlanarPosition(1, 0)));
        }

        [Test]
        public void Build_ReturnsNodesInAscendingTimeWithCorrectVerbsAndNullModifiers()
        {
            var program = new PawnProgram(new PlanarPosition(0, 0), baseSecondsPerTile: 1f, budgetSeconds: 60f);
            program.TryQueueMove(new PlanarPosition(2, 0), out _);
            program.TryQueueShoot(new PlanarPosition(2, 4), out _);
            program.TryQueueMove(new PlanarPosition(2, 2), out _);

            TimelinePayload payload = program.Build();

            Assert.That(payload.Nodes.Count, Is.EqualTo(3), "One move + one shoot + one move, each a single leg on a clear board.");
            Assert.That(payload.Nodes[0].Verb, Is.EqualTo(ActionVerb.Move));
            Assert.That(payload.Nodes[1].Verb, Is.EqualTo(ActionVerb.Shoot));
            Assert.That(payload.Nodes[0].ExecuteTime, Is.LessThan(payload.Nodes[1].ExecuteTime));
            Assert.That(payload.Nodes[1].ExecuteTime, Is.LessThan(payload.Nodes[2].ExecuteTime));
            foreach (ActionNode node in payload.Nodes)
            {
                Assert.That(node.Modifier, Is.Null);
            }
        }

        [Test]
        public void SprintPath_IsVisibleOnPreviewClockTiming()
        {
            var program = new PawnProgram(new PlanarPosition(0, 0), baseSecondsPerTile: 1f, budgetSeconds: 60f);
            program.TryQueueMove(new PlanarPosition(0, 3), out _, StanceType.Sprint);

            ScheduledPath preview = program.BuildMovePreviewPath(new PlanarPosition(0, 0));
            Assert.That(preview.EndSeconds, Is.EqualTo(3f));
            Assert.That(preview.Nodes.Count, Is.EqualTo(2), "Origin + one straight-line arrival on a clear board.");
        }
    }
}
