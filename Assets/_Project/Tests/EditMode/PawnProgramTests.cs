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
        public void QueueMove_AccumulatesExecuteTimeLikeTimeResourceMath()
        {
            var program = new PawnProgram(new GridCoordinate(0, 0), baseSecondsPerTile: 1f, budgetSeconds: 60f);

            bool ok = program.TryQueueMove(new GridCoordinate(2, 0), out string reason);

            float expected = TimeResourceMath.SegmentSeconds(new GridCoordinate(0, 0), new GridCoordinate(2, 0), 1f, StanceType.Walk);
            Assert.That(ok, Is.True, reason);
            Assert.That(program.UsedSeconds, Is.EqualTo(expected));
            Assert.That(program.Nodes[0].ExecuteTime, Is.EqualTo(expected));
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
        public void Build_ReturnsNodesInAscendingTimeWithCorrectVerbsAndNullModifiers()
        {
            var program = new PawnProgram(new GridCoordinate(0, 0), baseSecondsPerTile: 1f, budgetSeconds: 60f);
            program.TryQueueMove(new GridCoordinate(2, 0), out _);
            program.TryQueueShoot(new GridCoordinate(2, 4), out _);
            program.TryQueueMove(new GridCoordinate(2, 2), out _);

            TimelinePayload payload = program.Build();

            Assert.That(payload.Nodes.Count, Is.EqualTo(3));
            Assert.That(payload.Nodes[0].Verb, Is.EqualTo(ActionVerb.Move));
            Assert.That(payload.Nodes[1].Verb, Is.EqualTo(ActionVerb.Shoot));
            Assert.That(payload.Nodes[2].Verb, Is.EqualTo(ActionVerb.Move));
            Assert.That(payload.Nodes[0].ExecuteTime, Is.LessThan(payload.Nodes[1].ExecuteTime));
            Assert.That(payload.Nodes[1].ExecuteTime, Is.LessThan(payload.Nodes[2].ExecuteTime));
            foreach (ActionNode node in payload.Nodes)
            {
                Assert.That(node.Modifier, Is.Null);
            }
        }
    }
}
