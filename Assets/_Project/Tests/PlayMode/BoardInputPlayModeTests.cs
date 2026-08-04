using System.Collections;
using LogiCard.Board;
using LogiCard.Net;
using LogiCard.Sim;
using LogiCard.Timeline;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace LogiCard.Tests.PlayMode
{
    /// <summary>
    /// Continuous board wiring (C35/C39 Phase 4/5): ground-plane taps reach <see cref="PawnProgram"/>,
    /// and the on-screen preview follows.
    /// </summary>
    [TestFixture]
    public sealed class BoardInputPlayModeTests : SliceSceneFixture
    {
        [UnityTest]
        public IEnumerator GroundPlaneRaycastResolvesToPlanarPoint()
        {
            yield return null;

            var probes = new[]
            {
                new PlanarPosition(1f, 1f),
                new PlanarPosition(3f, 0.5f),
                new PlanarPosition(0.5f, 3.5f),
                new PlanarPosition(3.5f, 3.5f),
            };

            foreach (PlanarPosition point in probes)
            {
                Vector3 above = BoardVisual.WorldFromPlanar(point) + (Vector3.up * 5f);

                Assert.That(Physics.Raycast(above, Vector3.down, out RaycastHit hit, 20f), Is.True,
                    $"No collider under {point}.");

                PlanarPosition resolved = BoardVisual.PlanarFromWorld(hit.point);
                Assert.That(resolved.DistanceTo(point), Is.LessThan(0.15f),
                    $"Raycast under {point} resolved to {resolved}.");
            }
        }

        [Test]
        public void MoveTapQueuesTheNodeAndExtendsThePawnPreviewPath()
        {
            PlanarPosition destination = new PlanarPosition(Home.X, Home.Y + 1f);
            float expected = MoveSeconds(Home, destination);

            AttackerInput.Mode = ActionVerb.Move;
            Assert.That(AttackerInput.TryTapPoint(destination), Is.True);
            Assert.That(AttackerInput.Program.HasDraft, Is.True, "Move taps draft a path before SET PATH.");
            Assert.That(AttackerInput.TryCommitDraftPath(), Is.True);

            PawnProgram program = AttackerInput.Program;
            Assert.That(program.Nodes.Count, Is.EqualTo(1), "Clear straight leg books one Move node.");
            Assert.That(program.Nodes[0].Verb, Is.EqualTo(ActionVerb.Move));
            Assert.That(program.Nodes[0].Position.DistanceTo(destination), Is.LessThan(0.0001f));
            Assert.That(program.Nodes[0].ExecuteTime, Is.EqualTo(expected).Within(0.0001f));

            Assert.That(AttackerPawn.Path.Nodes[AttackerPawn.Path.Nodes.Count - 1].DistanceTo(destination),
                Is.LessThan(0.0001f));
            Assert.That(AttackerPawn.Path.EndSeconds, Is.EqualTo(expected).Within(0.0001f));
        }

        [Test]
        public void MoveTap_SprintAllotment_BooksFasterThanWalk()
        {
            PlanarPosition destination = new PlanarPosition(Home.X + 2f, Home.Y);
            AttackerInput.Mode = ActionVerb.Move;
            Assert.That(AttackerInput.TryTapPoint(destination), Is.True);
            Assert.That(AttackerInput.TrySetDraftStance(StanceType.Sprint), Is.True);
            Assert.That(AttackerInput.TryCommitDraftPath(), Is.True);

            float sprintCost = StanceAllotment.CostForTiles(2f, AttackerInput.Program.BaseSecondsPerTile, StanceType.Sprint);
            Assert.That(AttackerInput.Program.UsedSeconds, Is.EqualTo(sprintCost).Within(0.0001f));
            Assert.That(AttackerInput.Program.Nodes[0].Stance, Is.EqualTo(StanceType.Sprint));
            Assert.That(AttackerPawn.Path.EndSeconds, Is.EqualTo(sprintCost).Within(0.0001f));
        }

        [Test]
        public void ShootTapOutOfBoundsIsRejectedAndLeavesTheBudgetUntouched()
        {
            AttackerInput.Mode = ActionVerb.Shoot;

            Assert.That(AttackerInput.TryTapPoint(new PlanarPosition(9f, 9f)), Is.False);
            Assert.That(AttackerInput.Program.Nodes, Is.Empty);
            Assert.That(AttackerInput.Program.UsedSeconds, Is.EqualTo(0f));
        }

        [Test]
        public void ShootTapQueuesSnapShotAndLeavesThePawnInPlace()
        {
            Vector3 before = AttackerPawn.transform.position;
            AttackerInput.Mode = ActionVerb.Shoot;

            Assert.That(AttackerInput.TryTapPoint(new PlanarPosition(Home.X, Home.Y + 3f)), Is.True);

            PawnProgram program = AttackerInput.Program;
            Assert.That(program.Nodes[0].Verb, Is.EqualTo(ActionVerb.Shoot));
            Assert.That(program.UsedSeconds, Is.EqualTo(ShootCost.SnapShotSeconds).Within(0.0001f));
            Assert.That(program.CurrentPosition.DistanceTo(Home), Is.LessThan(0.0001f));
            Assert.That(Vector3.Distance(AttackerPawn.transform.position, before), Is.LessThan(0.0001f));
        }

        [Test]
        public void ReturningToProgramPhaseClearsTheQueueAndResetsThePawn()
        {
            AttackerInput.Mode = ActionVerb.Move;
            AttackerInput.TryTapPoint(new PlanarPosition(Home.X + 1f, Home.Y));
            AttackerInput.TryCommitDraftPath();
            Assert.That(AttackerInput.Program.Nodes, Is.Not.Empty);

            Phase.GoTo(RoundPhase.Reveal);
            Phase.GoTo(RoundPhase.Program);

            Assert.That(AttackerInput.Program.Nodes, Is.Empty);
            Assert.That(AttackerInput.Program.UsedSeconds, Is.EqualTo(0f));
            Assert.That(AttackerInput.Program.CurrentPosition.DistanceTo(Home), Is.LessThan(0.0001f));
            Assert.That(Vector3.Distance(AttackerPawn.transform.position, BoardVisual.WorldFromPlanar(Home)),
                Is.LessThan(0.0001f));
        }

        [Test]
        public void TapsAfterCommitAreIgnored()
        {
            AttackerInput.Mode = ActionVerb.Move;
            AttackerInput.TryTapPoint(new PlanarPosition(Home.X + 1f, Home.Y));
            AttackerInput.TryCommitDraftPath();
            AttackerInput.CommitToPlayback();

            int queuedBeforeExtraTap = AttackerInput.Program.Nodes.Count;

            Assert.That(AttackerInput.TryTapPoint(new PlanarPosition(Home.X + 2f, Home.Y)), Is.False);
            Assert.That(AttackerInput.Program.Nodes.Count, Is.EqualTo(queuedBeforeExtraTap));
        }
    }
}
