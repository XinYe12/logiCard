using System.Collections;
using LogiCard.Board;
using LogiCard.Boot;
using LogiCard.Net;
using LogiCard.Sim;
using LogiCard.Timeline;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UI;

namespace LogiCard.Tests.PlayMode
{
    /// <summary>
    /// Day 4 end to end (continuous Phase 4/5): Lock In resolves both payloads into a tape, the
    /// Time Resource scrubber plays that tape, and a hit announces itself.
    /// </summary>
    [TestFixture]
    public sealed class RoundPlaybackPlayModeTests : SliceSceneFixture
    {
        /// <summary>
        /// Defender's scripted Snap aims at (2,1); attacker standing there (within HitRadius) gets hit.
        /// Door starts Closed — the scripted defender opens it before the Snap so LoS through the choke
        /// is legal (see <see cref="GameBootstrap"/> BuildDefenderPayload).
        /// </summary>
        private static readonly PlanarPosition AmbushPoint = new PlanarPosition(2f, 1f);

        private void ArmWithAttackerMoveTo(PlanarPosition destination)
        {
            AttackerInput.Mode = ActionVerb.Move;
            Assert.That(AttackerInput.TryTapPoint(destination), Is.True);
            Assert.That(AttackerInput.TryCommitDraftPath(), Is.True);
            AttackerInput.CommitToPlayback();
            Playback.ResolveAndArm();
            Clock.Pause();
        }

        private static TapeEvent? FirstEventOfType(ReplayTape tape, TapeEventType type)
        {
            foreach (TapeEvent tapeEvent in tape.Events)
            {
                if (tapeEvent.Type == type)
                {
                    return tapeEvent;
                }
            }

            return null;
        }

        [Test]
        public void NoTapeExistsBeforeLockIn()
        {
            Assert.That(Playback.Tape, Is.Null);
        }

        [Test]
        public void ResolveArmsATrackForEveryPawn()
        {
            ArmWithAttackerMoveTo(AmbushPoint);

            Assert.That(Playback.Tape, Is.Not.Null);
            Assert.That(Playback.Tape.Tracks.ContainsKey(GameBootstrap.AttackerPawnId), Is.True);
            Assert.That(Playback.Tape.Tracks.ContainsKey(GameBootstrap.DefenderPawnId), Is.True);
        }

        [UnityTest]
        public IEnumerator DefenderStaysHomeUntilTheTapeArms()
        {
            Clock.Pause();
            Clock.SetSeconds(20f);
            Vector3 home = BoardVisual.WorldFromPlanar(new PlanarPosition(2f, 4f));
            Assert.That(Vector3.Distance(DefenderPawn.transform.position, home), Is.LessThan(0.0001f));

            ArmWithAttackerMoveTo(AmbushPoint);

            // Newly armed tape forces one exact apply at t=0; wait past PawnView's stepped-playback
            // hold (ART_DIRECTION §2) before the next scrub so it isn't read back mid-throttle.
            yield return WaitForPawnStepRelease();

            // Defender Walk base 2s/unit × Walk×2: 1.4 units to (2, 2.6) ⇒ 5.6s.
            Clock.SetSeconds(5.6f);
            Vector3 approaching = BoardVisual.WorldFromPlanar(new PlanarPosition(2f, 2.6f));
            Assert.That(Vector3.Distance(DefenderPawn.transform.position, approaching), Is.LessThan(0.05f));
        }

        [Test]
        public void ScriptedShotWoundsAnAttackerStandingOnTheAimedPoint()
        {
            ArmWithAttackerMoveTo(AmbushPoint);

            TapeEvent? wound = FirstEventOfType(Playback.Tape, TapeEventType.Wounded);
            Assert.That(wound.HasValue, Is.True, "Defender's scripted Snap Shot did not wound the attacker.");
            Assert.That(wound.Value.PawnId, Is.EqualTo(GameBootstrap.AttackerPawnId));
            Assert.That(wound.Value.TargetPawnId, Is.EqualTo(GameBootstrap.DefenderPawnId));
        }

        [Test]
        public void CrossingTheWoundSecondShowsStubTextAndRewindClearsIt()
        {
            Text banner = FindByName<Text>("OutcomeBanner");
            Assert.That(banner, Is.Not.Null, "HUD has no OutcomeBanner.");

            ArmWithAttackerMoveTo(AmbushPoint);
            TapeEvent wound = FirstEventOfType(Playback.Tape, TapeEventType.Wounded).Value;

            Clock.SetSeconds(wound.Seconds - 0.5f);
            Assert.That(banner.text, Is.Empty, "Wound announced before it happens.");

            Clock.SetSeconds(wound.Seconds);
            Assert.That(banner.text, Does.Contain("WOUNDED"));

            Clock.SetSeconds(0f);
            Assert.That(banner.text, Is.Empty, "Rewinding did not clear the outcome.");

            Clock.SetSeconds(wound.Seconds);
            Assert.That(banner.text, Does.Contain("WOUNDED"), "Scrubbing forward again did not re-announce.");
        }

        [Test]
        public void ShootingProducesATracerSeparateFromMovement()
        {
            ArmWithAttackerMoveTo(AmbushPoint);

            Assert.That(Object.FindObjectsByType<ShotTracerView>(FindObjectsSortMode.None), Is.Not.Empty,
                "A Shoot must read differently from a Move on the board.");
        }

        [Test]
        public void ReturningToAllotDropsTheTapeAndCarriesPawnPositions()
        {
            ArmWithAttackerMoveTo(AmbushPoint);
            Clock.SetSeconds(12f);

            PlanarPosition attackerEnd = Playback.Tape.Tracks[GameBootstrap.AttackerPawnId]
                .Evaluate(Playback.Tape.Tracks[GameBootstrap.AttackerPawnId].EndSeconds);
            PlanarPosition defenderEnd = Playback.Tape.Tracks[GameBootstrap.DefenderPawnId]
                .Evaluate(Playback.Tape.Tracks[GameBootstrap.DefenderPawnId].EndSeconds);

            Phase.GoTo(RoundPhase.Aftermath);
            Phase.GoTo(RoundPhase.Allot);

            Assert.That(Playback.Tape, Is.Null);
            Assert.That(Playback.PositionOf(GameBootstrap.AttackerPawnId).DistanceTo(attackerEnd), Is.LessThan(0.0001f));
            Assert.That(Playback.PositionOf(GameBootstrap.DefenderPawnId).DistanceTo(defenderEnd), Is.LessThan(0.0001f));
            Assert.That(Vector3.Distance(AttackerPawn.transform.position, BoardVisual.WorldFromPlanar(attackerEnd)),
                Is.LessThan(0.0001f));
            Assert.That(Vector3.Distance(DefenderPawn.transform.position, BoardVisual.WorldFromPlanar(defenderEnd)),
                Is.LessThan(0.0001f));
        }

        /// <summary>
        /// Scripted defender opens the Closed door mid-tape. Tint/model must follow the scrubber,
        /// then persist Open onto the shared board after Aftermath so the next round can path through.
        /// </summary>
        [Test]
        public void DoorOpenedOnTapeSyncsWhileScrubbingAndCarriesAfterAftermath()
        {
            ArmWithAttackerMoveTo(AmbushPoint);

            TapeEvent? opened = FirstEventOfType(Playback.Tape, TapeEventType.DoorOpened);
            Assert.That(opened.HasValue, Is.True, "Defender script must open the door before Snap.");

            Assert.That(BoardVisual.Model.TryGetDoor(opened.Value.Position, out Door door), Is.True);
            Clock.SetSeconds(0f);
            Assert.That(BoardVisual.Model.GetDoorState(door), Is.EqualTo(DoorState.Closed),
                "At t=0 the shared board must still show the round-start Closed state.");

            Clock.SetSeconds(opened.Value.Seconds);
            Assert.That(BoardVisual.Model.GetDoorState(door), Is.EqualTo(DoorState.Open),
                "Scrubbing past DoorOpened must update the shared board (and tint) immediately.");

            Clock.SetSeconds(0f);
            Assert.That(BoardVisual.Model.GetDoorState(door), Is.EqualTo(DoorState.Closed),
                "Rewinding must restore round-start door state.");

            Phase.GoTo(RoundPhase.Aftermath);
            Assert.That(BoardVisual.Model.GetDoorState(door), Is.EqualTo(DoorState.Open),
                "Aftermath must leave the door Open for the next round.");

            Bootstrap.RequestNextRound();
            Assert.That(Bootstrap.BeginRound(60f), Is.True);
            Assert.That(BoardVisual.Model.GetDoorState(door), Is.EqualTo(DoorState.Open),
                "Carried Open must survive into the next Program.");
        }

        [Test]
        public void SecondRoundAcceptsBoardInputFromCarriedPoints()
        {
            ArmWithAttackerMoveTo(AmbushPoint);
            Clock.SetSeconds(Playback.Tape.EndSeconds);

            Phase.GoTo(RoundPhase.Aftermath);
            Bootstrap.RequestNextRound();
            Assert.That(Phase.Phase, Is.EqualTo(RoundPhase.Allot));
            Assert.That(MatchClock.CurrentChooser, Is.EqualTo(MatchSide.Defender));

            PlanarPosition carried = Playback.PositionOf(GameBootstrap.AttackerPawnId);
            Assert.That(Bootstrap.BeginRound(60f), Is.True);
            Assert.That(Phase.Phase, Is.EqualTo(RoundPhase.Program));
            Assert.That(AttackerInput.Program.CurrentPosition.DistanceTo(carried), Is.LessThan(0.0001f));
            Assert.That(AttackerInput.Program.BudgetSeconds, Is.EqualTo(60f).Within(0.0001f));

            PlanarPosition next = new PlanarPosition(carried.X + 1f, carried.Y);
            AttackerInput.Mode = ActionVerb.Move;
            Assert.That(AttackerInput.TryTapPoint(next), Is.True);
            Assert.That(AttackerInput.TryCommitDraftPath(), Is.True);
            Assert.That(AttackerInput.Program.Nodes.Count, Is.EqualTo(1));
            Assert.That(AttackerInput.Program.Nodes[0].Position.DistanceTo(next), Is.LessThan(0.0001f));
        }

        [UnityTest]
        public IEnumerator LockInButtonResolvesBeforePlaybackStarts()
        {
            AttackerInput.Mode = ActionVerb.Move;
            AttackerInput.TryTapPoint(AmbushPoint);

            Button lockIn = FindByName<Button>("LockInButton");
            Assert.That(lockIn, Is.Not.Null, "HUD has no LockInButton.");
            lockIn.onClick.Invoke();

            Assert.That(Playback.Tape, Is.Not.Null, "Lock In must resolve a tape before Execute.");

            float deadline = Time.realtimeSinceStartup + 5f;
            while (Phase.Phase != RoundPhase.Execute && Time.realtimeSinceStartup < deadline)
            {
                yield return null;
            }

            Assert.That(Phase.Phase, Is.EqualTo(RoundPhase.Execute), "Lock In never reached Execute.");
        }
    }
}
