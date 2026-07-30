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
    /// Day 4 end to end: Lock In resolves both payloads into a tape, the Time Resource scrubber
    /// plays that tape, and a hit announces itself.
    /// </summary>
    [TestFixture]
    public sealed class RoundPlaybackPlayModeTests : SliceSceneFixture
    {
        /// <summary>The defender's scripted Snap Shot lands here; standing on it gets the player hit.</summary>
        private static readonly GridCoordinate AmbushTile = new GridCoordinate(0, 2);

        private void ArmWithAttackerMoveTo(GridCoordinate destination)
        {
            AttackerInput.Mode = ActionVerb.Move;
            Assert.That(AttackerInput.TryTapTile(destination), Is.True);
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
            ArmWithAttackerMoveTo(AmbushTile);

            Assert.That(Playback.Tape, Is.Not.Null);
            Assert.That(Playback.Tape.Tracks.ContainsKey(GameBootstrap.AttackerPawnId), Is.True);
            Assert.That(Playback.Tape.Tracks.ContainsKey(GameBootstrap.DefenderPawnId), Is.True);
        }

        /// <summary>
        /// The defender has no visible plan during Program; its movement may only come from the tape.
        /// </summary>
        [Test]
        public void DefenderStaysHomeUntilTheTapeArms()
        {
            Clock.Pause();
            Clock.SetSeconds(20f);
            Vector3 home = BoardVisual.WorldFromCoord(new GridCoordinate(4, 4));
            Assert.That(Vector3.Distance(DefenderPawn.transform.position, home), Is.LessThan(0.0001f));

            ArmWithAttackerMoveTo(AmbushTile);

            // Juggernaut walks 2s/tile at the x2 Walk multiplier, so two tiles land at 8s.
            Clock.SetSeconds(8f);
            Vector3 crossing = BoardVisual.WorldFromCoord(new GridCoordinate(4, 2));
            Assert.That(Vector3.Distance(DefenderPawn.transform.position, crossing), Is.LessThan(0.001f));
        }

        [Test]
        public void ScriptedShotWoundsAnAttackerStandingOnTheAimedTile()
        {
            ArmWithAttackerMoveTo(AmbushTile);

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

            ArmWithAttackerMoveTo(AmbushTile);
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
            ArmWithAttackerMoveTo(AmbushTile);

            Assert.That(Object.FindObjectsByType<ShotTracerView>(FindObjectsSortMode.None), Is.Not.Empty,
                "A Shoot must read differently from a Move on the board.");
        }

        [Test]
        public void ReturningToAllotDropsTheTapeAndCarriesPawnPositions()
        {
            ArmWithAttackerMoveTo(AmbushTile);
            Clock.SetSeconds(12f);

            GridCoordinate attackerEnd = Playback.Tape.Tracks[GameBootstrap.AttackerPawnId]
                .Evaluate(Playback.Tape.Tracks[GameBootstrap.AttackerPawnId].EndSeconds)
                .ToNearestCoordinate();
            GridCoordinate defenderEnd = Playback.Tape.Tracks[GameBootstrap.DefenderPawnId]
                .Evaluate(Playback.Tape.Tracks[GameBootstrap.DefenderPawnId].EndSeconds)
                .ToNearestCoordinate();

            Phase.GoTo(RoundPhase.Aftermath);
            Phase.GoTo(RoundPhase.Allot);

            Assert.That(Playback.Tape, Is.Null);
            Assert.That(Playback.PositionOf(GameBootstrap.AttackerPawnId), Is.EqualTo(attackerEnd));
            Assert.That(Playback.PositionOf(GameBootstrap.DefenderPawnId), Is.EqualTo(defenderEnd));
            Assert.That(Vector3.Distance(AttackerPawn.transform.position, BoardVisual.WorldFromCoord(attackerEnd)),
                Is.LessThan(0.0001f));
            Assert.That(Vector3.Distance(DefenderPawn.transform.position, BoardVisual.WorldFromCoord(defenderEnd)),
                Is.LessThan(0.0001f));
        }

        [Test]
        public void SecondRoundAcceptsBoardInputFromCarriedTiles()
        {
            ArmWithAttackerMoveTo(AmbushTile);
            Clock.SetSeconds(Playback.Tape.EndSeconds);

            Phase.GoTo(RoundPhase.Aftermath);
            Bootstrap.RequestNextRound();
            Assert.That(Phase.Phase, Is.EqualTo(RoundPhase.Allot));
            Assert.That(MatchClock.CurrentChooser, Is.EqualTo(MatchSide.Defender));

            GridCoordinate carried = Playback.PositionOf(GameBootstrap.AttackerPawnId);
            Assert.That(Bootstrap.BeginRound(60f), Is.True);
            Assert.That(Phase.Phase, Is.EqualTo(RoundPhase.Program));
            Assert.That(AttackerInput.Program.CurrentPosition, Is.EqualTo(carried));
            Assert.That(AttackerInput.Program.BudgetSeconds, Is.EqualTo(60f).Within(0.0001f));

            // Move one tile along +X from the carried tile — proves programming works again.
            GridCoordinate next = carried.Offset(1, 0);
            AttackerInput.Mode = ActionVerb.Move;
            Assert.That(AttackerInput.TryTapTile(next), Is.True);
            Assert.That(AttackerInput.Program.Nodes.Count, Is.EqualTo(1));
            Assert.That(AttackerInput.Program.Nodes[0].GridPosition, Is.EqualTo(next));
        }

        [UnityTest]
        public IEnumerator LockInButtonResolvesBeforePlaybackStarts()
        {
            AttackerInput.Mode = ActionVerb.Move;
            AttackerInput.TryTapTile(AmbushTile);

            Button lockIn = FindByName<Button>("LockInButton");
            lockIn.onClick.Invoke();

            Assert.That(Playback.Tape, Is.Not.Null, "Resolve must happen at Lock In, not once Execute starts.");

            float deadline = Time.realtimeSinceStartup + 5f;
            while (Phase.Phase != RoundPhase.Execute && Time.realtimeSinceStartup < deadline)
            {
                yield return null;
            }

            Assert.That(Phase.Phase, Is.EqualTo(RoundPhase.Execute));
        }
    }
}
