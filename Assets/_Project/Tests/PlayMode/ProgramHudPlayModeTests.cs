using System.Collections;
using LogiCard.Cards;
using LogiCard.Net;
using LogiCard.Sim;
using LogiCard.Timeline;
using LogiCard.UI;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.EventSystems;
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
        /// South of Door #1 (door segment sits on y=4 at x∈[3.75,4.25]). Destinations on
        /// that segment become unreachable once Door #1 starts Closed.
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
        /// 2026-08-18 — the camera control-hint moved from an IMGUI stopgap on <c>BoardCameraRig</c>
        /// itself to real UI-owned chrome (<c>ProgramHud.RegisterCameraRig</c> +
        /// <c>BoardCameraRig.ControlHintText</c>, refreshed in <c>ProgramHud.Update</c>). Proves it is
        /// live (tracks the rig's actual mode, not a static string) and never blocks a board tap.
        /// </summary>
        [UnityTest]
        public IEnumerator CameraControlHintTracksLiveCameraMode()
        {
            Text hint = FindByName<Text>("CameraControlHint");
            Assert.That(hint, Is.Not.Null, "HUD has no CameraControlHint label.");
            Assert.That(hint.raycastTarget, Is.False, "Hint must never block a board tap underneath it.");
            Assert.That(Bootstrap.CameraRig, Is.Not.Null, "Fixture must have a live BoardCameraRig.");

            yield return null; // let ProgramHud.Update() run at least once

            Assert.That(hint.text, Is.EqualTo(Bootstrap.CameraRig.ControlHintText));
            Assert.That(hint.text, Does.Contain("Right-drag"));

            Bootstrap.CameraRig.CycleTpsLock();
            yield return null;

            Assert.That(hint.text, Is.EqualTo(Bootstrap.CameraRig.ControlHintText));
            Assert.That(hint.text, Does.Contain("T: Cycle"));

            Bootstrap.CameraRig.ExitTpsLock();
            yield return null;

            Assert.That(hint.text, Does.Contain("Right-drag"), "Exiting TPS lock must restore the free-camera hint.");
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

            Assert.That(AttackerInput.TryTapPoint(new PlanarPosition(4f, 4f)), Is.True);
            Assert.That(AttackerInput.PendingDoor, Is.Not.Null, "Tapping near the door should select it.");
            Assert.That(AttackerInput.PendingDoor.DisplayName, Is.EqualTo("Door #1"),
                "Near-door tap must resolve Door #1 (frontal), not Door #2.");
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
            PlanarPosition besideDoor = new PlanarPosition(4f, 3.85f);
            AttackerInput.PrepareRound(besideDoor, 60f);
            Phase.GoTo(RoundPhase.Reveal);
            Phase.GoTo(RoundPhase.Program);

            Button doorMode = FindByName<Button>("Mode_Door");
            Button open = FindByName<Button>("Door_Open");
            doorMode.onClick.Invoke();

            Assert.That(AttackerInput.TryTapPoint(besideDoor), Is.True);
            Door selected = AttackerInput.PendingDoor;
            Assert.That(selected, Is.Not.Null);
            Assert.That(selected.DisplayName, Is.EqualTo("Door #1"),
                "Beside-door start must resolve Door #1 (frontal), not Door #2.");

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
        /// UI_FLOW §7 gap closed this wave: Adrenaline is a Playback-only primary, 1/match, gated to
        /// an active booked segment. Resolve effect is deferred — this only covers the control.
        /// </summary>
        [UnityTest]
        public IEnumerator AdrenalineAppearsInPlaybackAndSpendsOncePerMatch()
        {
            AttackerInput.Mode = ActionVerb.Move;
            AttackerInput.TryTapPoint(SafeMoveDestination);
            AttackerInput.TryCommitDraftPath();

            Button lockIn = FindByName<Button>("LockInButton");
            Assert.That(lockIn, Is.Not.Null);
            Assert.That(lockIn.gameObject.activeInHierarchy, Is.True, "Lock In should show during Program.");

            Button adrenalineHidden = FindByName<Button>("AdrenalineButton");
            Assert.That(adrenalineHidden, Is.Not.Null, "HUD must build an AdrenalineButton.");
            Assert.That(adrenalineHidden.gameObject.activeInHierarchy, Is.False,
                "Adrenaline must stay hidden outside Playback.");

            lockIn.onClick.Invoke();
            float deadline = Time.realtimeSinceStartup + 5f;
            while (Phase.Phase != RoundPhase.Execute && Time.realtimeSinceStartup < deadline)
            {
                yield return null;
            }

            Assert.That(Phase.Phase, Is.EqualTo(RoundPhase.Execute));
            Button adrenaline = FindByName<Button>("AdrenalineButton");
            Assert.That(adrenaline.gameObject.activeInHierarchy, Is.True);
            Assert.That(lockIn.gameObject.activeInHierarchy, Is.False,
                "Lock In yields the primary slot to Adrenaline during Playback.");

            Clock.SetSeconds(0f);
            yield return null;
            Assert.That(adrenaline.interactable, Is.True,
                "At t=0 inside a booked Move segment, Adrenaline should be spendable.");

            bool raised = false;
            Hud.AdrenalineUsed += () => raised = true;
            adrenaline.onClick.Invoke();
            Assert.That(raised, Is.True);
            Assert.That(adrenaline.interactable, Is.False, "1/match — second press must not re-arm.");

            Text label = adrenaline.GetComponentInChildren<Text>();
            Assert.That(label.text, Does.Contain("USED"));
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

        /// <summary>Screen point guaranteed outside the built <c>GearHand</c> root's bounds, for drag-release tests.</summary>
        private static Vector2 OutsideGearHandScreenPoint()
        {
            RectTransform handRoot = FindByName<RectTransform>(GearHandView.RootName);
            Assert.That(handRoot, Is.Not.Null, "HUD has no GearHand root.");
            var corners = new Vector3[4];
            handRoot.GetWorldCorners(corners);
            return new Vector2(corners[0].x, corners[0].y) - new Vector2(600f, 600f);
        }

        /// <summary>Drives one full press→drag-out→release gesture directly on a card's drag controller.</summary>
        private static void DragCardOut(GearHandView.CardDragController drag, Vector2 releasePoint)
        {
            var data = new PointerEventData(EventSystem.current) { pressPosition = Vector2.zero, position = Vector2.zero };
            drag.OnBeginDrag(data);
            data.position = releasePoint;
            data.delta = releasePoint;
            drag.OnDrag(data);
            drag.OnEndDrag(data);
        }

        /// <summary>
        /// Bandage HUD-side contract (C63), rewired for drag-to-play (Hand Deck Drag Play brief,
        /// 2026-08-15): dragging Gear_Bandage out of the hand and releasing queues at the scrubber's
        /// current seconds (0s here — nothing has moved it), same as the old click-arm-then-scrubber
        /// path did, just via the drop. Wound/charge gates come from
        /// <see cref="ProgramHud.RegisterMatchState"/> — injected here as synthetic delegates rather
        /// than driving a real wound through Sim resolve, which is out of this HUD contract's scope.
        /// </summary>
        [Test]
        public void GearBandageDragOutOfHandPlacesABandageNode()
        {
            Hud.RegisterMatchState(() => 1, () => 0, () => 0);

            Button bandage = FindByName<Button>(GearHandView.ButtonName(CardId.Bandage));
            Assert.That(bandage, Is.Not.Null, "HUD has no Gear_Bandage button.");
            Assert.That(bandage.interactable, Is.True, "Bandage should be draggable once wounded with an unused charge.");

            GearHandView.CardDragController drag = FindByName<GearHandView.CardDragController>("Slot_Bandage");
            Assert.That(drag, Is.Not.Null, "Bandage card must carry a drag controller.");

            DragCardOut(drag, OutsideGearHandScreenPoint());

            Assert.That(AttackerInput.Program.Nodes.Count, Is.EqualTo(1));
            Assert.That(AttackerInput.Program.Nodes[0].Verb, Is.EqualTo(ActionVerb.Bandage));
            Assert.That(AttackerInput.Program.UsedSeconds, Is.EqualTo(PawnProgram.BandageSeconds).Within(0.0001f));
            Assert.That(bandage.interactable, Is.False, "Charge is spent — a Bandage node is already queued this Program.");
        }

        /// <summary>
        /// Storm contract (C67), same rewire as Bandage's test above. Storm has no board-tap path at
        /// all (self-targeting, nothing to aim at) and no scrubber-drag placement anymore either — the
        /// drag-out-of-hand gesture is now its only placement path in production, and this exercises
        /// exactly that instead of calling <see cref="BoardInputController.TryQueueStormAt"/> directly.
        /// </summary>
        [Test]
        public void GearStormDragOutOfHandPlacesAStormNode()
        {
            Button storm = FindByName<Button>(GearHandView.ButtonName(CardId.Storm));
            Assert.That(storm, Is.Not.Null, "HUD has no Gear_Storm button.");
            Assert.That(storm.interactable, Is.True, "Storm should be draggable with none queued yet this Program.");

            GearHandView.CardDragController drag = FindByName<GearHandView.CardDragController>("Slot_Storm");
            Assert.That(drag, Is.Not.Null, "Storm card must carry a drag controller.");

            DragCardOut(drag, OutsideGearHandScreenPoint());

            Assert.That(AttackerInput.Program.Nodes.Count, Is.EqualTo(1));
            Assert.That(AttackerInput.Program.Nodes[0].Verb, Is.EqualTo(ActionVerb.Storm));
            Assert.That(storm.interactable, Is.False, "A Storm node is already queued this Program.");
        }

        /// <summary>
        /// C69 — the real cross-round counter (<c>RoundPlayback.StormCastCountOf</c>) must keep Storm
        /// blocked in round 2 even though round 2 starts with a fresh, empty Program (which used to be
        /// enough to make the old per-round "already queued this Program" dedup call it legal again —
        /// the actual bug this counter fixes).
        /// </summary>
        [Test]
        public void StormStaysBlockedInASecondRoundAfterBeingCastInTheFirst()
        {
            Button storm = FindByName<Button>(GearHandView.ButtonName(CardId.Storm));
            Assert.That(AttackerInput.TryQueueStormAt(0f, out string reason), Is.True, reason);
            Assert.That(storm.interactable, Is.False, "A Storm node is already queued this Program.");

            AttackerInput.CommitToPlayback();
            Playback.ResolveAndArm();
            Clock.Pause();
            Clock.SetSeconds(Playback.Tape.EndSeconds);

            Phase.GoTo(RoundPhase.Aftermath);
            Bootstrap.RequestNextRound();
            Assert.That(Bootstrap.BeginRound(60f), Is.True);

            Assert.That(AttackerInput.Program.Nodes, Is.Empty, "Round 2 must start with a fresh, empty Program.");
            Assert.That(storm.interactable, Is.False,
                "Storm must stay blocked across rounds — it is 1x/Character/match, not 1x/Program.");
        }

        /// <summary>A drag that never clears the threshold must not queue anything and must leave the card armable.</summary>
        [Test]
        public void ShortDragOnBandageDoesNotQueueAndCardStaysInteractable()
        {
            Hud.RegisterMatchState(() => 1, () => 0, () => 0);

            Button bandage = FindByName<Button>(GearHandView.ButtonName(CardId.Bandage));
            GearHandView.CardDragController drag = FindByName<GearHandView.CardDragController>("Slot_Bandage");
            Assert.That(drag, Is.Not.Null);

            var data = new PointerEventData(EventSystem.current) { pressPosition = Vector2.zero, position = Vector2.zero };
            drag.OnBeginDrag(data);
            data.position = new Vector2(5f, 0f);
            data.delta = data.position;
            drag.OnDrag(data);
            drag.OnEndDrag(data);

            Assert.That(AttackerInput.Program.Nodes, Is.Empty, "A short drag must not queue a Bandage node.");
            Assert.That(bandage.interactable, Is.True, "Card must stay armable after a cancelled drag.");
        }

        /// <summary>Dragging an already-blocked card (SetSpent via legality gate) out of the hand must be a no-op.</summary>
        [Test]
        public void DragOnAlreadyQueuedStormCardIsANoOp()
        {
            Assert.That(AttackerInput.TryQueueStormAt(AttackerInput.Program.UsedSeconds, out string reason), Is.True, reason);

            Button storm = FindByName<Button>(GearHandView.ButtonName(CardId.Storm));
            Assert.That(storm.interactable, Is.False, "Storm should already be blocked — one is queued this Program.");

            GearHandView.CardDragController drag = FindByName<GearHandView.CardDragController>("Slot_Storm");
            DragCardOut(drag, OutsideGearHandScreenPoint());

            Assert.That(AttackerInput.Program.Nodes.Count, Is.EqualTo(1),
                "Dragging an already-blocked Storm card must not add a second node.");
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
                "Scrubbing to an exact instant must show that instant's precise point.");

            Clock.SetSeconds(arrival);
            Assert.That(Vector3.Distance(AttackerPawn.transform.position, BoardVisual.WorldFromPlanar(destination)),
                Is.LessThan(0.0001f), "Pawn is not on its scheduled point at the arrival second.");
        }
    }
}
