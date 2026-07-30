using System.Collections;
using System.Collections.Generic;
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
    /// Day 3 board wiring: tiles must be pickable, a tap must reach <see cref="PawnProgram"/>,
    /// and the on-screen preview must follow. None of this is reachable from an EditMode test.
    /// </summary>
    [TestFixture]
    public sealed class BoardInputPlayModeTests : SliceSceneFixture
    {
        [Test]
        public void EveryBoardCoordinateGetsExactlyOneTileMarker()
        {
            var marked = new HashSet<GridCoordinate>();
            foreach (TileMarker marker in Object.FindObjectsByType<TileMarker>(FindObjectsSortMode.None))
            {
                Assert.That(marked.Add(marker.Coordinate), Is.True, $"Duplicate TileMarker for {marker.Coordinate}.");
            }

            var expected = new HashSet<GridCoordinate>(BoardVisual.Model.GetAllCoordinates());
            Assert.That(marked, Is.EquivalentTo(expected));
        }

        /// <summary>
        /// Covers the one step <see cref="BoardInputController.TryTapTile"/> skips: that a physics hit
        /// on a tile actually resolves back to the coordinate drawn at that world position.
        /// </summary>
        [UnityTest]
        public IEnumerator TileColliderRaycastResolvesToTheCoordinateDrawnThere()
        {
            // Tile colliders are created during Awake; they only enter the physics scene once a
            // frame has elapsed, so queries before that hit nothing.
            yield return null;

            var probes = new[]
            {
                new GridCoordinate(1, 1),
                new GridCoordinate(3, 2),
                new GridCoordinate(0, 4),
                new GridCoordinate(4, 0),
            };

            foreach (GridCoordinate coordinate in probes)
            {
                Vector3 above = BoardVisual.WorldFromCoord(coordinate) + (Vector3.up * 5f);

                Assert.That(Physics.Raycast(above, Vector3.down, out RaycastHit hit, 20f), Is.True,
                    $"No collider under {coordinate}.");

                var marker = hit.collider.GetComponent<TileMarker>();
                Assert.That(marker, Is.Not.Null, $"Collider at {coordinate} carries no TileMarker.");
                Assert.That(marker.Coordinate, Is.EqualTo(coordinate));
            }
        }

        [Test]
        public void MoveTapQueuesTheNodeAndExtendsThePawnPreviewPath()
        {
            GridCoordinate destination = Home.Offset(0, 2);
            float expected = MoveSeconds(Home, destination);

            AttackerInput.Mode = ActionVerb.Move;
            Assert.That(AttackerInput.TryTapTile(destination), Is.True);

            PawnProgram program = AttackerInput.Program;
            Assert.That(program.Nodes.Count, Is.EqualTo(1));
            Assert.That(program.Nodes[0].Verb, Is.EqualTo(ActionVerb.Move));
            Assert.That(program.Nodes[0].GridPosition, Is.EqualTo(destination));
            Assert.That(program.Nodes[0].ExecuteTime, Is.EqualTo(expected).Within(0.0001f));

            Assert.That(AttackerPawn.Path.Nodes[AttackerPawn.Path.Nodes.Count - 1], Is.EqualTo(destination));
            Assert.That(AttackerPawn.Path.EndSeconds, Is.EqualTo(expected).Within(0.0001f));
        }

        [Test]
        public void ShootTapOffRowAndColumnIsRejectedAndLeavesTheBudgetUntouched()
        {
            AttackerInput.Mode = ActionVerb.Shoot;

            Assert.That(AttackerInput.TryTapTile(Home.Offset(1, 2)), Is.False);
            Assert.That(AttackerInput.Program.Nodes, Is.Empty);
            Assert.That(AttackerInput.Program.UsedSeconds, Is.EqualTo(0f));
        }

        [Test]
        public void ShootTapAlongColumnQueuesSnapShotAndLeavesThePawnInPlace()
        {
            Vector3 before = AttackerPawn.transform.position;
            AttackerInput.Mode = ActionVerb.Shoot;

            Assert.That(AttackerInput.TryTapTile(Home.Offset(0, 3)), Is.True);

            PawnProgram program = AttackerInput.Program;
            Assert.That(program.Nodes[0].Verb, Is.EqualTo(ActionVerb.Shoot));
            Assert.That(program.UsedSeconds, Is.EqualTo(ShootCost.SnapShotSeconds).Within(0.0001f));
            Assert.That(program.CurrentPosition, Is.EqualTo(Home));
            Assert.That(Vector3.Distance(AttackerPawn.transform.position, before), Is.LessThan(0.0001f));
        }

        [Test]
        public void ReturningToProgramPhaseClearsTheQueueAndResetsThePawn()
        {
            AttackerInput.Mode = ActionVerb.Move;
            AttackerInput.TryTapTile(Home.Offset(2, 0));
            Assert.That(AttackerInput.Program.Nodes, Is.Not.Empty);

            Phase.GoTo(RoundPhase.Reveal);
            Phase.GoTo(RoundPhase.Program);

            Assert.That(AttackerInput.Program.Nodes, Is.Empty);
            Assert.That(AttackerInput.Program.UsedSeconds, Is.EqualTo(0f));
            Assert.That(AttackerInput.Program.CurrentPosition, Is.EqualTo(Home));
            Assert.That(Vector3.Distance(AttackerPawn.transform.position, BoardVisual.WorldFromCoord(Home)),
                Is.LessThan(0.0001f));
        }

        [Test]
        public void TapsAfterCommitAreIgnored()
        {
            AttackerInput.Mode = ActionVerb.Move;
            AttackerInput.TryTapTile(Home.Offset(1, 0));
            AttackerInput.CommitToPlayback();

            int queuedBeforeExtraTap = AttackerInput.Program.Nodes.Count;

            Assert.That(AttackerInput.TryTapTile(Home.Offset(2, 0)), Is.False);
            Assert.That(AttackerInput.Program.Nodes.Count, Is.EqualTo(queuedBeforeExtraTap));
        }
    }
}
