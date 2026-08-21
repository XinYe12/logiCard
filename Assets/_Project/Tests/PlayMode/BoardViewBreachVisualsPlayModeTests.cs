using System.Collections;
using LogiCard.Net;
using LogiCard.Sim;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace LogiCard.Tests.PlayMode
{
    /// <summary>
    /// C36 breach-point presentation (<c>BoardView.RefreshBreachVisuals</c>) — the visual half of the
    /// geometry-breach primitive whose Sim + <c>RoundPlayback</c> layers already shipped. Intact and
    /// Damaged draw the ordinary wall fence, Breached hides it and shows the blown-open dressing, and
    /// an attached bomb shows a charge marker.
    ///
    /// PLAYBACK_CONTRACT §2 rules 2/4: the visual is a pure function of
    /// <see cref="ArenaBoard.GetBreachState"/> / <see cref="ArenaBoard.HasAttachedBomb"/> re-derived
    /// every frame, never a one-shot fired when a tape event is crossed — so these tests scrub
    /// backwards as well as forwards and expect the wall to come back, the same way the door presenter
    /// and the "Healed presenter" bug class in that doc demand.
    ///
    /// No map has an authored <see cref="BreachPoint"/> yet (deliberately deferred — a human picks the
    /// wall), so these register a scratch one on the live <see cref="LogiCard.Board.BoardView.Model"/>,
    /// exactly like <c>RoundPlaybackPlayModeTests</c>'s bomb cases and <c>GhostResolverBombTests</c>.
    /// </summary>
    [TestFixture]
    public sealed class BoardViewBreachVisualsPlayModeTests : SliceSceneFixture
    {
        /// <summary>First registered breach point's scene names — see <c>BoardView.PlaceBreachMesh</c>.</summary>
        private const string BreachRootName = "Breach_0";
        private const string BreachWallName = "Breach_0_Wall";

        private BreachPoint RegisterScratchBreachPoint()
        {
            var point = new BreachPoint(
                new Segment(new PlanarPosition(Home.X - 1f, Home.Y), new PlanarPosition(Home.X + 1f, Home.Y)),
                BreachState.Intact,
                "Test Breach");
            BoardVisual.Model.RegisterBreachPoint(point);
            return point;
        }

        private GameObject Wall()
        {
            Transform wall = BoardVisual.transform.Find(BreachWallName);
            Assert.That(wall, Is.Not.Null, "BoardView drew no wall body for the registered breach point.");
            return wall.gameObject;
        }

        private GameObject Child(string childName)
        {
            Transform root = BoardVisual.transform.Find(BreachRootName);
            Assert.That(root, Is.Not.Null, "BoardView built no breach visual root.");
            Transform child = root.Find(childName);
            Assert.That(child, Is.Not.Null, $"Breach visual has no {childName}.");
            return child.gameObject;
        }

        private GameObject Opening() => Child("BreachedDressing");

        private GameObject BombMarker() => Child("BombMarker");

        private void AssertRendersAsWall(string because)
        {
            Assert.That(Wall().activeInHierarchy, Is.True, because);
            Assert.That(Opening().activeInHierarchy, Is.False, because);
        }

        private void AssertRendersAsOpening(string because)
        {
            Assert.That(Wall().activeInHierarchy, Is.False, because);
            Assert.That(Opening().activeInHierarchy, Is.True, because);
        }

        /// <summary>
        /// A breach point is not in <see cref="ArenaBoard.Walls"/>, so before this presenter existed a
        /// registered point blocked Move/Shoot while rendering as thin air. It must draw the same wall
        /// body any other segment gets — and it must appear even though it was registered after
        /// <c>BoardView.Build</c> already ran (the only way one can exist until a map authors one).
        /// </summary>
        [UnityTest]
        public IEnumerator AnIntactBreachPointRendersAsAnOrdinaryWall()
        {
            Clock.Pause();
            RegisterScratchBreachPoint();
            yield return null;

            AssertRendersAsWall("An Intact breach point must be indistinguishable from a wall.");
            Assert.That(BombMarker().activeInHierarchy, Is.False, "Nothing has attached a bomb yet.");
        }

        /// <summary>
        /// Direct model drive, independent of any tape: Breached opens, and putting the model back to
        /// Intact (what rewinding past a Detonate does) closes it again. Damaged is reserved and
        /// unexercised by the wall-only v1 verb, so it deliberately renders exactly like Intact rather
        /// than inventing a look for a state nothing can currently reach.
        /// </summary>
        [UnityTest]
        public IEnumerator BreachedRendersAnOpeningAndGoingBackToIntactRestoresTheWall()
        {
            Clock.Pause();
            BreachPoint point = RegisterScratchBreachPoint();
            yield return null;
            AssertRendersAsWall("Setup: should start Intact.");

            BoardVisual.Model.SetBreachState(point, BreachState.Damaged);
            yield return null;
            AssertRendersAsWall("Damaged is reserved by C36 and must render as the existing wall.");

            BoardVisual.Model.SetBreachState(point, BreachState.Breached);
            yield return null;
            AssertRendersAsOpening("Breached must render as an actual opening, not a solid wall.");

            BoardVisual.Model.SetBreachState(point, BreachState.Intact);
            yield return null;
            AssertRendersAsWall(
                "The visual must be re-derived from the model every tick, so going back to Intact " +
                "restores the wall — the Healed-presenter bug class if it did not.");
        }

        /// <summary>An attached, undetonated bomb needs some visual of its own — the flag is live on the
        /// board at any scrubber second and was previously invisible.</summary>
        [UnityTest]
        public IEnumerator AnAttachedBombShowsAMarkerThatClearsWhenTheFlagDoes()
        {
            Clock.Pause();
            BreachPoint point = RegisterScratchBreachPoint();
            yield return null;
            Assert.That(BombMarker().activeInHierarchy, Is.False);

            BoardVisual.Model.SetAttachedBomb(point, true);
            yield return null;
            Assert.That(BombMarker().activeInHierarchy, Is.True,
                "HasAttachedBomb must show a marker.");
            AssertRendersAsWall("Attaching a bomb must not change geometry — only Detonate does.");

            BoardVisual.Model.SetAttachedBomb(point, false);
            yield return null;
            Assert.That(BombMarker().activeInHierarchy, Is.False,
                "Clearing the flag (rewinding past the Attach, or a Detonate consuming it) must clear the marker.");
        }

        /// <summary>
        /// The whole loop through the real presenter: Program a Attach + Detonate, arm, and scrub. The
        /// wall must still be drawn one tick before the Detonate's second, be an opening at it, and be
        /// drawn again after rewinding to zero — i.e. the visual tracks
        /// <c>RoundPlayback.SyncBreachToSeconds</c>'s model writes in both directions.
        /// </summary>
        [UnityTest]
        public IEnumerator AttachThenDetonateSwapsTheVisualAtTheDetonateSecondAndRewindRestoresIt()
        {
            BreachPoint point = RegisterScratchBreachPoint();

            Assert.That(AttackerInput.Program.TryQueueBombAttach(point, out string attachReason), Is.True, attachReason);
            Assert.That(AttackerInput.Program.TryQueueBombDetonate(point, out string detonateReason), Is.True, detonateReason);
            AttackerInput.CommitToPlayback();
            Playback.ResolveAndArm();
            Clock.Pause();

            TapeEvent? attached = null;
            TapeEvent? breached = null;
            foreach (TapeEvent tapeEvent in Playback.Tape.Events)
            {
                if (tapeEvent.Type == TapeEventType.BombAttached && !attached.HasValue)
                {
                    attached = tapeEvent;
                }

                if (tapeEvent.Type == TapeEventType.GeometryBreached && !breached.HasValue)
                {
                    breached = tapeEvent;
                }
            }

            Assert.That(attached.HasValue, Is.True, "Queued BombAttach emitted no BombAttached event.");
            Assert.That(breached.HasValue, Is.True, "Queued BombDetonate emitted no GeometryBreached event.");

            Clock.SetSeconds(attached.Value.Seconds);
            yield return null;
            Assert.That(BombMarker().activeInHierarchy, Is.True, "Marker must be up from the Attach's own second.");
            AssertRendersAsWall("Attach alone must not open the wall.");

            Clock.SetSeconds(Mathf.Max(0f, breached.Value.Seconds - 0.05f));
            yield return null;
            AssertRendersAsWall("The wall must still be drawn right up to the Detonate's own second.");

            Clock.SetSeconds(breached.Value.Seconds);
            yield return null;
            AssertRendersAsOpening("The wall must be an opening at the Detonate's own second.");
            Assert.That(BombMarker().activeInHierarchy, Is.False,
                "Detonation consumes the bomb, so the marker must clear with it.");

            Clock.SetSeconds(0f);
            yield return null;
            AssertRendersAsWall("Rewinding to round start must put the wall back.");

            Clock.SetSeconds(breached.Value.Seconds);
            yield return null;
            AssertRendersAsOpening("Scrubbing forward again must re-open it.");
        }
    }
}
