using System.Collections;
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
    /// Day 3 HUD wiring plus the playback hand-off: thumb-zone controls must drive the program,
    /// and Lock In must put the committed path on screen through the Time Resource clock.
    /// </summary>
    [TestFixture]
    public sealed class ProgramHudPlayModeTests : SliceSceneFixture
    {
        [Test]
        public void ModeButtonsSwitchTheInputVerb()
        {
            Button move = FindByName<Button>("Mode_Move");
            Button shoot = FindByName<Button>("Mode_Shoot");
            Assert.That(move, Is.Not.Null, "HUD has no Mode_Move button.");
            Assert.That(shoot, Is.Not.Null, "HUD has no Mode_Shoot button.");

            shoot.onClick.Invoke();
            Assert.That(AttackerInput.Mode, Is.EqualTo(ActionVerb.Shoot));

            move.onClick.Invoke();
            Assert.That(AttackerInput.Mode, Is.EqualTo(ActionVerb.Move));
        }

        [Test]
        public void QueueReadoutShowsUsedBudgetAndEachQueuedAction()
        {
            Text readout = FindByName<Text>("QueueReadout");
            Assert.That(readout, Is.Not.Null, "HUD has no QueueReadout text.");

            GridCoordinate destination = Home.Offset(0, 2);
            float expected = MoveSeconds(Home, destination);

            AttackerInput.Mode = ActionVerb.Move;
            AttackerInput.TryTapTile(destination);

            Assert.That(readout.text, Does.Contain($"Used {expected:0.0}"));
            Assert.That(readout.text, Does.Contain($"/ {AttackerInput.Program.BudgetSeconds:0.0}s"));
            Assert.That(readout.text, Does.Contain("1: Move"));
        }

        [UnityTest]
        public IEnumerator LockInButtonBuildsThePayloadAndReachesExecutePhase()
        {
            AttackerInput.Mode = ActionVerb.Move;
            AttackerInput.TryTapTile(Home.Offset(0, 2));
            AttackerInput.TryTapTile(Home.Offset(2, 2));

            TimelinePayload payload = AttackerInput.Program.Build();
            Assert.That(payload.Nodes.Count, Is.EqualTo(2));

            Button lockIn = FindByName<Button>("LockInButton");
            Assert.That(lockIn, Is.Not.Null, "HUD has no LockInButton.");
            lockIn.onClick.Invoke();

            // Reveal holds for 0.8 real-world seconds before Execute (ProgramHud.LockInRoutine).
            float deadline = Time.realtimeSinceStartup + 5f;
            while (Phase.Phase != RoundPhase.Execute && Time.realtimeSinceStartup < deadline)
            {
                yield return null;
            }

            Assert.That(Phase.Phase, Is.EqualTo(RoundPhase.Execute), "Lock In never reached Execute.");
        }

        [Test]
        public void PlaybackPlacesThePawnOnItsScheduledTileAtTheArrivalSecond()
        {
            GridCoordinate destination = Home.Offset(0, 2);
            float arrival = MoveSeconds(Home, destination);

            AttackerInput.Mode = ActionVerb.Move;
            AttackerInput.TryTapTile(destination);
            AttackerInput.CommitToPlayback();

            Clock.Pause();

            Clock.SetSeconds(0f);
            Assert.That(Vector3.Distance(AttackerPawn.transform.position, BoardVisual.WorldFromCoord(Home)),
                Is.LessThan(0.0001f), "Pawn does not start on its home tile.");

            Clock.SetSeconds(arrival * 0.5f);
            Vector3 midpoint = Vector3.Lerp(BoardVisual.WorldFromCoord(Home), BoardVisual.WorldFromCoord(destination), 0.5f);
            Assert.That(Vector3.Distance(AttackerPawn.transform.position, midpoint), Is.LessThan(0.001f),
                "Playback snaps instead of interpolating across the Time Resource segment.");

            Clock.SetSeconds(arrival);
            Assert.That(Vector3.Distance(AttackerPawn.transform.position, BoardVisual.WorldFromCoord(destination)),
                Is.LessThan(0.0001f), "Pawn is not on its scheduled tile at the arrival second.");
        }
    }
}
