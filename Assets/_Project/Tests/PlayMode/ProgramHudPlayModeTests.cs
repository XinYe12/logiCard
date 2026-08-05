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
    /// HUD wiring plus the playback hand-off on the continuous board (Phase 4/5).
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
        public void DoorModeButtonSwitchesTheInputVerbAndAction()
        {
            Button door = FindByName<Button>("Mode_Door");
            Button open = FindByName<Button>("Door_Open");
            Button close = FindByName<Button>("Door_Close");
            Assert.That(door, Is.Not.Null, "HUD has no Mode_Door button.");
            Assert.That(open, Is.Not.Null, "HUD has no Door_Open button.");
            Assert.That(close, Is.Not.Null, "HUD has no Door_Close button.");

            door.onClick.Invoke();
            Assert.That(AttackerInput.Mode, Is.EqualTo(ActionVerb.Door));
            Assert.That(AttackerInput.PreferredDoorAction, Is.EqualTo(DoorAction.Open));

            close.onClick.Invoke();
            Assert.That(AttackerInput.PreferredDoorAction, Is.EqualTo(DoorAction.Close));

            open.onClick.Invoke();
            Assert.That(AttackerInput.PreferredDoorAction, Is.EqualTo(DoorAction.Open));
        }

        [Test]
        public void ShootModeButtonsSelectSnapAndHold()
        {
            Button shoot = FindByName<Button>("Mode_Shoot");
            Button snap = FindByName<Button>("Shoot_Snap");
            Button hold = FindByName<Button>("Shoot_Hold");
            Assert.That(snap, Is.Not.Null, "HUD has no Shoot_Snap button.");
            Assert.That(hold, Is.Not.Null, "HUD has no Shoot_Hold button.");

            shoot.onClick.Invoke();
            hold.onClick.Invoke();
            Assert.That(AttackerInput.PreferredShootMode, Is.EqualTo(ShootMode.HoldAngle));

            Assert.That(AttackerInput.TryTapPoint(new PlanarPosition(Home.X, Home.Y + 3f)), Is.True);
            Assert.That(AttackerInput.Program.Nodes[0].ShootMode, Is.EqualTo(ShootMode.HoldAngle));
            Assert.That(AttackerInput.Program.UsedSeconds, Is.EqualTo(ShootCost.HoldAngleSeconds).Within(0.0001f));

            snap.onClick.Invoke();
            Assert.That(AttackerInput.PreferredShootMode, Is.EqualTo(ShootMode.SnapShot));
        }

        [Test]
        public void StanceButtonsAndSetPathBookAMoveWithChosenBand()
        {
            Button sprint = FindByName<Button>("Stance_Sprint");
            Button setPath = FindByName<Button>("SetPathButton");
            Assert.That(sprint, Is.Not.Null, "HUD has no Stance_Sprint button.");
            Assert.That(setPath, Is.Not.Null, "HUD has no SetPathButton.");

            AttackerInput.Mode = ActionVerb.Move;
            Assert.That(AttackerInput.TryTapPoint(new PlanarPosition(Home.X, Home.Y + 2f)), Is.True);

            sprint.onClick.Invoke();
            Assert.That(AttackerInput.Program.DraftStance, Is.EqualTo(StanceType.Sprint));

            setPath.onClick.Invoke();
            Assert.That(AttackerInput.Program.HasDraft, Is.False);
            Assert.That(AttackerInput.Program.UsedSeconds, Is.EqualTo(2f).Within(0.0001f));
            Assert.That(AttackerInput.Program.Nodes[0].Stance, Is.EqualTo(StanceType.Sprint));
        }

        [Test]
        public void QueueReadoutShowsDraftThenCommittedActions()
        {
            Text readout = FindByName<Text>("QueueReadout");
            Assert.That(readout, Is.Not.Null, "HUD has no QueueReadout text.");

            PlanarPosition destination = new PlanarPosition(Home.X, Home.Y + 2f);
            float expected = MoveSeconds(Home, destination);

            AttackerInput.Mode = ActionVerb.Move;
            AttackerInput.TryTapPoint(destination);

            Assert.That(readout.text, Does.Contain("DRAFT"));
            Assert.That(readout.text, Does.Contain("Walk"));

            AttackerInput.TryCommitDraftPath();

            Assert.That(readout.text, Does.Contain($"Used {expected:0.0}"));
            Assert.That(readout.text, Does.Contain($"/ {AttackerInput.Program.BudgetSeconds:0.0}s"));
            Assert.That(readout.text, Does.Contain("Move"));
        }

        [UnityTest]
        public IEnumerator LockInButtonCommitsDraftAndReachesExecutePhase()
        {
            AttackerInput.Mode = ActionVerb.Move;
            AttackerInput.TryTapPoint(new PlanarPosition(Home.X, Home.Y + 2f));
            Assert.That(AttackerInput.Program.HasDraft, Is.True);

            Button lockIn = FindByName<Button>("LockInButton");
            Assert.That(lockIn, Is.Not.Null, "HUD has no LockInButton.");
            lockIn.onClick.Invoke();

            Assert.That(AttackerInput.Program.HasDraft, Is.False, "Lock In must commit the draft path.");
            Assert.That(AttackerInput.Program.Nodes, Is.Not.Empty);

            float deadline = Time.realtimeSinceStartup + 5f;
            while (Phase.Phase != RoundPhase.Execute && Time.realtimeSinceStartup < deadline)
            {
                yield return null;
            }

            Assert.That(Phase.Phase, Is.EqualTo(RoundPhase.Execute), "Lock In never reached Execute.");
        }

        [Test]
        public void PlaybackPlacesThePawnOnItsScheduledPointAtTheArrivalSecond()
        {
            PlanarPosition destination = new PlanarPosition(Home.X, Home.Y + 2f);
            float arrival = MoveSeconds(Home, destination);

            AttackerInput.Mode = ActionVerb.Move;
            AttackerInput.TryTapPoint(destination);
            AttackerInput.CommitToPlayback();

            Clock.Pause();

            Clock.SetSeconds(0f);
            Assert.That(Vector3.Distance(AttackerPawn.transform.position, BoardVisual.WorldFromPlanar(Home)),
                Is.LessThan(0.0001f), "Pawn does not start on its home point.");

            Clock.SetSeconds(arrival * 0.5f);
            Vector3 midpoint = Vector3.Lerp(
                BoardVisual.WorldFromPlanar(Home), BoardVisual.WorldFromPlanar(destination), 0.5f);
            Assert.That(Vector3.Distance(AttackerPawn.transform.position, midpoint), Is.LessThan(0.001f),
                "Playback snaps instead of interpolating across the Time Resource segment.");

            Clock.SetSeconds(arrival);
            Assert.That(Vector3.Distance(AttackerPawn.transform.position, BoardVisual.WorldFromPlanar(destination)),
                Is.LessThan(0.0001f), "Pawn is not on its scheduled point at the arrival second.");
        }
    }
}
