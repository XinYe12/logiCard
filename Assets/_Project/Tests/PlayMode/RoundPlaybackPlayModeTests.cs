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
        /// Defender's scripted Snap aims at (4,3); attacker standing there (within HitRadius) gets hit.
        /// Door #1 (frontal Hall entrance at y=4) starts Closed — the scripted defender opens it before
        /// the Snap so LoS through Door #1 is legal (see <see cref="GameBootstrap"/> BuildDefenderPayload).
        /// </summary>
        private static readonly PlanarPosition AmbushPoint = new PlanarPosition(4f, 3f);

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

        [Test]
        public void DefenderStaysHomeUntilTheTapeArms()
        {
            Clock.Pause();
            Clock.SetSeconds(20f);
            Vector3 home = BoardVisual.WorldFromPlanar(new PlanarPosition(4f, 6f));
            Assert.That(Vector3.Distance(DefenderPawn.transform.position, home), Is.LessThan(0.0001f));

            ArmWithAttackerMoveTo(AmbushPoint);

            // Defender Walk base 2s/unit × Walk×2: 1.4 units to (4, 4.6) ⇒ 5.6s.
            Clock.SetSeconds(5.6f);
            Vector3 approaching = BoardVisual.WorldFromPlanar(new PlanarPosition(4f, 4.6f));
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

        /// <summary>
        /// PLAYBACK_CONTRACT §3's Healed row: one-shot banner only (Bandage can only ever clear a
        /// wound carried in from a prior round — GhostResolver.CompileTrack resolves it from
        /// GhostInput.StartingWounds before this round's own ResolveShots pass runs — so there is
        /// never a same-round wound splat to hide/restore, unlike Wounded/Killed above).
        /// </summary>
        [Test]
        public void CrossingTheHealedSecondShowsStubTextAndRewindClearsIt()
        {
            Text banner = FindByName<Text>("OutcomeBanner");
            Assert.That(banner, Is.Not.Null, "HUD has no OutcomeBanner.");

            // Round 1: attacker walks into the defender's scripted Snap and gets wounded, then that
            // wound is committed into round 2's carried state (C33).
            ArmWithAttackerMoveTo(AmbushPoint);
            Clock.SetSeconds(Playback.Tape.EndSeconds);

            Phase.GoTo(RoundPhase.Aftermath);
            Assert.That(Playback.WoundsOf(GameBootstrap.AttackerPawnId), Is.GreaterThan(0),
                "Setup requires the attacker to carry a wound into round 2 (committed on Aftermath entry).");

            Bootstrap.RequestNextRound();
            Assert.That(Bootstrap.BeginRound(60f), Is.True);

            // Round 2: queue only a Bandage node — no Move — at an explicit early instant. Must land
            // strictly before the defender's own re-scripted Snap wounds the attacker again this
            // round (attacker's carried position is still the prior round's AmbushPoint), so this
            // test's "before/at/rewind" scrub points only ever cross the one event under test.
            Assert.That(AttackerInput.TryQueueBandageAt(1f, out string reason), Is.True, reason);
            AttackerInput.CommitToPlayback();
            Playback.ResolveAndArm();
            Clock.Pause();

            TapeEvent? healed = FirstEventOfType(Playback.Tape, TapeEventType.Healed);
            Assert.That(healed.HasValue, Is.True, "Bandage must emit a Healed event for the wound carried in from round 1.");

            Clock.SetSeconds(healed.Value.Seconds - 0.5f);
            Assert.That(banner.text, Is.Empty, "Heal announced before it happens.");

            Clock.SetSeconds(healed.Value.Seconds);
            Assert.That(banner.text, Does.Contain("HEALED"));

            Clock.SetSeconds(0f);
            Assert.That(banner.text, Is.Empty, "Rewinding did not clear the outcome.");

            Clock.SetSeconds(healed.Value.Seconds);
            Assert.That(banner.text, Does.Contain("HEALED"), "Scrubbing forward again did not re-announce.");
        }

        [Test]
        public void ShootingProducesATracerSeparateFromMovement()
        {
            ArmWithAttackerMoveTo(AmbushPoint);

            Assert.That(Object.FindObjectsByType<ShotTracerView>(FindObjectsSortMode.None), Is.Not.Empty,
                "A Shoot must read differently from a Move on the board.");
        }

        /// <summary>
        /// PLAYBACK_CONTRACT: ShootFire continuous VFX are scrubber-faithful — absent just before
        /// the aim window / fire second, present at the event, cleared on rewind.
        /// </summary>
        [Test]
        public void ShootFireVfxFollowScrubberSeconds()
        {
            ArmWithAttackerMoveTo(AmbushPoint);

            TapeEvent? fire = FirstEventOfType(Playback.Tape, TapeEventType.ShootFire);
            Assert.That(fire.HasValue, Is.True, "Defender script must fire a shot.");

            float beforeWindow = Mathf.Max(0f, fire.Value.WindowStartSeconds - 0.05f);
            Clock.SetSeconds(beforeWindow);
            Assert.That(AnyVisible(Object.FindObjectsByType<ShotTracerView>(FindObjectsSortMode.None)), Is.False,
                "Tracer must not light before the shooter's aim/hold window.");
            Assert.That(AnyVisible(Object.FindObjectsByType<MuzzleFlashView>(FindObjectsSortMode.None)), Is.False,
                "Muzzle must not light before the fire second.");

            Clock.SetSeconds(fire.Value.WindowStartSeconds);
            Assert.That(AnyVisible(Object.FindObjectsByType<ShotTracerView>(FindObjectsSortMode.None)), Is.True,
                "Tracer must light at WindowStartSeconds.");

            Clock.SetSeconds(fire.Value.Seconds);
            Assert.That(AnyVisible(Object.FindObjectsByType<MuzzleFlashView>(FindObjectsSortMode.None)), Is.True,
                "Muzzle must light at the ShootFire completion second.");

            Clock.SetSeconds(0f);
            Assert.That(AnyVisible(Object.FindObjectsByType<ShotTracerView>(FindObjectsSortMode.None)), Is.False,
                "Rewind must hide tracers.");
            Assert.That(AnyVisible(Object.FindObjectsByType<MuzzleFlashView>(FindObjectsSortMode.None)), Is.False,
                "Rewind must hide muzzle flashes.");
        }

        /// <summary>
        /// PLAYBACK_CONTRACT: wound splat is continuous scrubber state (banner is one-shot separately).
        /// </summary>
        [Test]
        public void WoundSplatFollowsScrubberSeconds()
        {
            ArmWithAttackerMoveTo(AmbushPoint);

            TapeEvent? wound = FirstEventOfType(Playback.Tape, TapeEventType.Wounded);
            Assert.That(wound.HasValue, Is.True);

            Clock.SetSeconds(Mathf.Max(0f, wound.Value.Seconds - 0.05f));
            Assert.That(AnyVisible(Object.FindObjectsByType<WoundSplatView>(FindObjectsSortMode.None)), Is.False);

            Clock.SetSeconds(wound.Value.Seconds);
            Assert.That(AnyVisible(Object.FindObjectsByType<WoundSplatView>(FindObjectsSortMode.None)), Is.True);

            Clock.SetSeconds(0f);
            Assert.That(AnyVisible(Object.FindObjectsByType<WoundSplatView>(FindObjectsSortMode.None)), Is.False);
        }

        /// <summary>
        /// C36/Bomber — the real Program→Resolve→Playback loop the contract's "next real step" note
        /// asked for: no map has an authored <see cref="BreachPoint"/> yet (deliberately deferred, a
        /// human content decision), so this test registers its own directly on the live
        /// <see cref="BoardView.Model"/>, the same way <c>GhostResolverBombTests</c> builds its own
        /// scratch board for the Sim layer. Attach then Detonate in one round: geometry stays Intact
        /// until the Detonate's own second, flips to Breached exactly there, and rewinding past it
        /// restores Intact (PLAYBACK_CONTRACT rule 2/4 — pure function of scrubber seconds, no restart).
        /// </summary>
        [Test]
        public void AttachThenDetonateBreachesTheWallAtItsSecondAndRewindRestoresIt()
        {
            var point = new BreachPoint(
                new Segment(new PlanarPosition(Home.X - 1f, Home.Y), new PlanarPosition(Home.X + 1f, Home.Y)),
                BreachState.Intact,
                "Test Breach");
            BoardVisual.Model.RegisterBreachPoint(point);

            Assert.That(AttackerInput.Program.TryQueueBombAttach(point, out string attachReason), Is.True, attachReason);
            Assert.That(AttackerInput.Program.TryQueueBombDetonate(point, out string detonateReason), Is.True, detonateReason);
            AttackerInput.CommitToPlayback();
            Playback.ResolveAndArm();
            Clock.Pause();

            TapeEvent? attached = FirstEventOfType(Playback.Tape, TapeEventType.BombAttached);
            TapeEvent? breached = FirstEventOfType(Playback.Tape, TapeEventType.GeometryBreached);
            Assert.That(attached.HasValue, Is.True, "Queued BombAttach must emit a BombAttached event.");
            Assert.That(breached.HasValue, Is.True, "Queued BombDetonate on an attached point must emit GeometryBreached.");

            Clock.SetSeconds(Mathf.Max(0f, breached.Value.Seconds - 0.05f));
            Assert.That(BoardVisual.Model.GetBreachState(point), Is.EqualTo(BreachState.Intact),
                "Wall must stay Intact before the Detonate's own second.");

            Clock.SetSeconds(breached.Value.Seconds);
            Assert.That(BoardVisual.Model.GetBreachState(point), Is.EqualTo(BreachState.Breached),
                "Wall must be Breached at the Detonate's own second.");

            Clock.SetSeconds(0f);
            Assert.That(BoardVisual.Model.GetBreachState(point), Is.EqualTo(BreachState.Intact),
                "Rewind to round-start must restore Intact.");

            Clock.SetSeconds(breached.Value.Seconds);
            Assert.That(BoardVisual.Model.GetBreachState(point), Is.EqualTo(BreachState.Breached),
                "Scrubbing forward again must re-apply the breach.");
        }

        /// <summary>
        /// C36/Bomber — an Attach committed in round 1 must persist (the real bug class door persistence
        /// already fixed, C33's carry-across-rounds discipline) so round 2's Detonate on the same point,
        /// with no new Attach queued, still succeeds — proves <c>SyncBreachToSeconds</c>'s final
        /// authoritative apply in <c>CommitRoundState</c> actually writes the attached-bomb flag back
        /// onto the shared <see cref="ArenaBoard"/>, not just a scrubber-local view.
        /// </summary>
        [Test]
        public void BombAttachedInRoundOnePersistsSoRoundTwoCanDetonateWithoutReattaching()
        {
            var point = new BreachPoint(
                new Segment(new PlanarPosition(Home.X - 1f, Home.Y), new PlanarPosition(Home.X + 1f, Home.Y)),
                BreachState.Intact,
                "Test Breach");
            BoardVisual.Model.RegisterBreachPoint(point);

            Assert.That(AttackerInput.Program.TryQueueBombAttach(point, out string attachReason), Is.True, attachReason);
            AttackerInput.CommitToPlayback();
            Playback.ResolveAndArm();
            Clock.SetSeconds(Playback.Tape.EndSeconds);

            Phase.GoTo(RoundPhase.Aftermath);
            Assert.That(BoardVisual.Model.HasAttachedBomb(point), Is.True,
                "Attach must be committed onto the real board at Aftermath, same as doors/wounds.");

            Bootstrap.RequestNextRound();
            Assert.That(Bootstrap.BeginRound(DefaultRoundSeconds), Is.True);

            Assert.That(AttackerInput.Program.TryQueueBombDetonate(point, out string detonateReason), Is.True, detonateReason);
            AttackerInput.CommitToPlayback();
            Playback.ResolveAndArm();
            Clock.Pause();

            TapeEvent? breached = FirstEventOfType(Playback.Tape, TapeEventType.GeometryBreached);
            Assert.That(breached.HasValue, Is.True,
                "Round 2's Detonate must succeed against the bomb attached in round 1 — resolver reads HasAttachedBomb from the real board.");

            Clock.SetSeconds(breached.Value.Seconds);
            Assert.That(BoardVisual.Model.GetBreachState(point), Is.EqualTo(BreachState.Breached));
            Assert.That(BoardVisual.Model.HasAttachedBomb(point), Is.False,
                "Detonate must consume the attached bomb, same as GhostResolver.ApplyBombToggleGroup — " +
                "otherwise a spent bomb would read as still attached and a third round could Detonate again for free.");

            Phase.GoTo(RoundPhase.Aftermath);
            Assert.That(BoardVisual.Model.HasAttachedBomb(point), Is.False,
                "The consumed flag must also persist through CommitRoundState's authoritative apply, not just the live scrub.");
        }

        private static bool AnyVisible(ShotTracerView[] views)
        {
            for (int i = 0; i < views.Length; i++)
            {
                if (views[i] != null && views[i].IsVisible)
                {
                    return true;
                }
            }

            return false;
        }

        private static bool AnyVisible(MuzzleFlashView[] views)
        {
            for (int i = 0; i < views.Length; i++)
            {
                if (views[i] != null && views[i].IsVisible)
                {
                    return true;
                }
            }

            return false;
        }

        private static bool AnyVisible(WoundSplatView[] views)
        {
            for (int i = 0; i < views.Length; i++)
            {
                if (views[i] != null && views[i].IsVisible)
                {
                    return true;
                }
            }

            return false;
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
            Assert.That(door.DisplayName, Is.EqualTo("Door #1"),
                "Scripted defender must open Door #1 (frontal), not Door #2 (Hall→Vault).");
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

        /// <summary>
        /// Playtest 2026-08-12: door opens all finished at end of reveal because every ApplyTime
        /// restarted the hinge swing. Spam scrubber ticks after DoorOpened — arc must keep progressing.
        /// </summary>
        [UnityTest]
        public IEnumerator DoorSwingKeepsProgressingAcrossPlaybackTicks()
        {
            ArmWithAttackerMoveTo(AmbushPoint);

            TapeEvent? opened = FirstEventOfType(Playback.Tape, TapeEventType.DoorOpened);
            Assert.That(opened.HasValue, Is.True, "Defender script must open a door.");

            Transform hinge = FindDoorHingeNear(opened.Value.Position);
            Assert.That(hinge, Is.Not.Null, "No Door_*/Hinge near the DoorOpened event.");

            Clock.SetSeconds(0f);
            yield return null;
            Assert.That(AbsYawDegrees(hinge.localEulerAngles.y), Is.LessThan(5f), "Closed at t=0.");

            Clock.SetSeconds(opened.Value.Seconds);
            yield return null;

            for (int i = 0; i < 12; i++)
            {
                Clock.SetSeconds(opened.Value.Seconds + (0.02f * i));
                yield return null;
            }

            yield return new WaitForSecondsRealtime(0.28f);

            Assert.That(
                AbsYawDegrees(hinge.localEulerAngles.y),
                Is.GreaterThan(25f),
                "Hinge yaw must advance under scrubber spam; restarting the swing each tick left doors closed until playback ended.");
        }

        private static float AbsYawDegrees(float eulerY)
        {
            float y = eulerY;
            if (y > 180f)
            {
                y -= 360f;
            }

            return Mathf.Abs(y);
        }

        private static Transform FindDoorHingeNear(PlanarPosition doorMid)
        {
            Vector3 world = new Vector3(doorMid.X, 0f, doorMid.Y);
            Transform best = null;
            float bestDist = float.MaxValue;

            Transform[] hinges = Object.FindObjectsByType<Transform>(FindObjectsSortMode.None);
            for (int i = 0; i < hinges.Length; i++)
            {
                Transform t = hinges[i];
                if (t == null || t.name != "Hinge" || t.parent == null || !t.parent.name.StartsWith("Door_"))
                {
                    continue;
                }

                float dist = Vector3.Distance(
                    new Vector3(t.parent.position.x, 0f, t.parent.position.z),
                    world);
                if (dist < bestDist)
                {
                    bestDist = dist;
                    best = t;
                }
            }

            return bestDist < 1.5f ? best : null;
        }


        [Test]
        public void FreshMatchClearsCarriedDeathAndReturnsPawnsToSpawn()
        {
            // Ambush wounds the attacker; Aftermath commits that carry (C33). Rematch must not
            // reopen Program with a corpse still on the board (playtest 2026-08-12 image 4).
            ArmWithAttackerMoveTo(AmbushPoint);
            Assert.That(FirstEventOfType(Playback.Tape, TapeEventType.Wounded).HasValue
                    || FirstEventOfType(Playback.Tape, TapeEventType.Killed).HasValue,
                Is.True,
                "Ambush arm should wound/kill so carry would otherwise stick.");

            Phase.GoTo(RoundPhase.Aftermath);
            Bootstrap.RequestNextRound();

            PlanarPosition attackerHome = new PlanarPosition(4f, 0f);
            PlanarPosition defenderHome = new PlanarPosition(4f, 6f);

            Bootstrap.RequestFreshMatch();

            Assert.That(Phase.Phase, Is.EqualTo(RoundPhase.Allot));
            Assert.That(MatchClock.RoundIndex, Is.EqualTo(1));
            Assert.That(MatchClock.RemainingSeconds, Is.EqualTo(Bootstrap.matchPoolSeconds).Within(0.0001f));
            Assert.That(Playback.WoundsOf(GameBootstrap.AttackerPawnId), Is.EqualTo(0));
            Assert.That(Playback.WoundsOf(GameBootstrap.DefenderPawnId), Is.EqualTo(0));
            Assert.That(Playback.AnyoneDead, Is.False);
            Assert.That(Playback.Tape, Is.Null);
            Assert.That(Playback.PositionOf(GameBootstrap.AttackerPawnId).DistanceTo(attackerHome), Is.LessThan(0.0001f));
            Assert.That(Playback.PositionOf(GameBootstrap.DefenderPawnId).DistanceTo(defenderHome), Is.LessThan(0.0001f));
        }

    }
}
