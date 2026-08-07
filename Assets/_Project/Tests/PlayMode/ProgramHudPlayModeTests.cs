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
        /// <summary>
        /// South of the door choke (door segment sits on y=2 at x∈[1.75,2.25]). Destinations on
        /// that segment become unreachable once the demo door starts Closed.
        /// </summary>
        private PlanarPosition SafeMoveDestination => new PlanarPosition(Home.X, Home.Y + 1f);
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

        /// <summary>
        /// BUG FOUND 2026-08-06 (playtest): a board tap used to book an Open/Close immediately
        /// against a HUD-preselected action, silently flipped to its opposite whenever it matched
        /// the door's live state — confusing/"ambiguous" since what got booked didn't always match
        /// what the HUD showed as selected. Tapping now only selects a door (PendingDoor); OPEN/
        /// CLOSE is the explicit confirm, and confirming with nothing selected — or nothing at all
        /// — books nothing.
        /// </summary>
        [Test]
        public void DoorModeSelectsADoorThenRequiresExplicitConfirm()
        {
            Button door = FindByName<Button>("Mode_Door");
            Button open = FindByName<Button>("Door_Open");
            Button close = FindByName<Button>("Door_Close");
            Assert.That(door, Is.Not.Null, "HUD has no Mode_Door button.");
            Assert.That(open, Is.Not.Null, "HUD has no Door_Open button.");
            Assert.That(close, Is.Not.Null, "HUD has no Door_Close button.");

            door.onClick.Invoke();
            Assert.That(AttackerInput.Mode, Is.EqualTo(ActionVerb.Door));
            Assert.That(AttackerInput.PendingDoor, Is.Null, "Nothing should be selected before a tap.");

            close.onClick.Invoke();
            Assert.That(AttackerInput.Program.Nodes, Is.Empty, "Confirming with nothing selected must not book anything.");

            Assert.That(AttackerInput.TryTapPoint(new PlanarPosition(2f, 2f)), Is.True);
            Assert.That(AttackerInput.PendingDoor, Is.Not.Null, "Tapping near the door should select it.");
            Assert.That(AttackerInput.Program.Nodes, Is.Empty, "Selecting a door must not book anything by itself.");

            Assert.That(AttackerInput.TryTapPoint(new PlanarPosition(0.2f, 0.2f)), Is.True);
            Assert.That(AttackerInput.PendingDoor, Is.Null, "A tap away from any door should cancel the pending selection.");
        }

        /// <summary>
        /// Playtest 2026-08-07: OPEN must change the prompt's status. Live board stays Closed until
        /// Aftermath; ScheduledDoorState + keeping PendingDoor after confirm are what make the
        /// label actually flip.
        /// </summary>
        [Test]
        public void DoorOpenConfirmUpdatesScheduledStatusAndKeepsSelection()
        {
            // Must already be within InteractRadius — same constraint as a real Open tap after a Move.
            PlanarPosition besideDoor = new PlanarPosition(2f, 1.85f);
            AttackerInput.PrepareRound(besideDoor, 60f);
            Phase.GoTo(RoundPhase.Reveal);
            Phase.GoTo(RoundPhase.Program);

            Button doorMode = FindByName<Button>("Mode_Door");
            Button open = FindByName<Button>("Door_Open");
            doorMode.onClick.Invoke();

            Assert.That(AttackerInput.TryTapPoint(besideDoor), Is.True);
            Door selected = AttackerInput.PendingDoor;
            Assert.That(selected, Is.Not.Null);

            Assert.That(AttackerInput.Board.GetDoorState(selected), Is.EqualTo(DoorState.Closed));
            Assert.That(AttackerInput.Program.ScheduledDoorState(selected), Is.EqualTo(DoorState.Closed));

            open.onClick.Invoke();

            Assert.That(AttackerInput.Program.Nodes.Count, Is.EqualTo(1),
                "Open confirm should book one Door node (pawn starts beside door).");
            Assert.That(AttackerInput.Program.Nodes[0].Door, Is.EqualTo(DoorAction.Open));
            Assert.That(AttackerInput.PendingDoor, Is.SameAs(selected),
                "Selection must stay so the prompt can show the new status.");
            Assert.That(AttackerInput.Program.ScheduledDoorState(selected), Is.EqualTo(DoorState.Open));
            Assert.That(AttackerInput.Board.GetDoorState(selected), Is.EqualTo(DoorState.Closed),
                "Live passability still waits for Aftermath.");
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
            Assert.That(AttackerInput.TryTapPoint(SafeMoveDestination), Is.True);

            sprint.onClick.Invoke();
            Assert.That(AttackerInput.Program.DraftStance, Is.EqualTo(StanceType.Sprint));

            setPath.onClick.Invoke();
            Assert.That(AttackerInput.Program.HasDraft, Is.False);
            Assert.That(AttackerInput.Program.UsedSeconds, Is.EqualTo(1f).Within(0.0001f));
            Assert.That(AttackerInput.Program.Nodes[0].Stance, Is.EqualTo(StanceType.Sprint));
        }

        [Test]
        public void QueueReadoutShowsDraftThenCommittedActions()
        {
            Text readout = FindByName<Text>("QueueReadout");
            Assert.That(readout, Is.Not.Null, "HUD has no QueueReadout text.");

            PlanarPosition destination = SafeMoveDestination;
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
            AttackerInput.TryTapPoint(SafeMoveDestination);
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

        /// <summary>
        /// BUG FOUND 2026-08-07 (playtest): the end screen showed a stale "Rn · SIDE PICKS" top
        /// strip and the word "MATCH OVER" twice (headline + dead button) once the match actually
        /// ended.
        /// </summary>
        [Test]
        public void MatchOverClearsTheStaleHeaderAndDoesNotRepeatItsHeadlineOnTheButton()
        {
            Text matchLabel = FindByName<Text>("MatchLabel");
            Text aftermathLabel = FindByName<Text>("AftermathLabel");
            Button nextRoundButton = FindByName<Button>("NextRoundButton");
            Assert.That(matchLabel, Is.Not.Null, "HUD has no MatchLabel text.");
            Assert.That(aftermathLabel, Is.Not.Null, "HUD has no AftermathLabel text.");
            Assert.That(nextRoundButton, Is.Not.Null, "HUD has no NextRoundButton.");
            Text nextRoundButtonLabel = nextRoundButton.GetComponentInChildren<Text>();

            Phase.GoTo(RoundPhase.MatchOver);

            Assert.That(matchLabel.text, Does.Not.Contain("PICKS"),
                "Top strip should not still frame the match as mid-round once it is over.");
            Assert.That(aftermathLabel.text, Does.Contain("MATCH OVER"));
            Assert.That(nextRoundButtonLabel.text, Is.Not.EqualTo(aftermathLabel.text),
                "Button must not repeat the exact same headline text.");
            Assert.That(nextRoundButton.interactable, Is.False);
        }

        [Test]
        public void PlaybackPlacesThePawnOnItsScheduledPointAtTheArrivalSecond()
        {
            PlanarPosition destination = SafeMoveDestination;
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
