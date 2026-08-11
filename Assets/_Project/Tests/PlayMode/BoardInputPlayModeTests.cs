using System.Collections;
using LogiCard.Board;
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
                new PlanarPosition(6.5f, 2f),
                new PlanarPosition(1f, 8.5f),
                new PlanarPosition(7f, 9f),
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

        /// <summary>
        /// BUG FOUND 2026-08-06: Lock In used to discard an over-budget pending draft and lock in
        /// anyway with no explanation. It must not silently keep a draft that blocks forever either
        /// (playtest 2026-08-07): when committed actions fit, Lock In drops the pending draft and
        /// proceeds.
        /// </summary>
        [Test]
        public void OverBudgetPendingDraftIsDroppedSoCommitToPlaybackCanProceed()
        {
            PlanarPosition start = AttackerInput.Program.CurrentPosition;
            AttackerInput.PrepareRound(start, 1f);
            Phase.GoTo(RoundPhase.Reveal);
            Phase.GoTo(RoundPhase.Program);

            PlanarPosition destination = new PlanarPosition(start.X, start.Y + 1f);
            float draftCost = MoveSeconds(start, destination);
            Assert.That(draftCost, Is.GreaterThan(1f), "Fixture assumption: 1m Walk costs more than 1s.");
            Assert.That(AttackerInput.Program.BudgetSeconds, Is.EqualTo(1f).Within(0.0001f));

            AttackerInput.Mode = ActionVerb.Move;
            Assert.That(AttackerInput.TryTapPoint(destination), Is.True);
            Assert.That(AttackerInput.Program.HasDraft, Is.True);

            Assert.That(AttackerInput.CommitToPlayback(), Is.True,
                "Over-budget pending draft must be dropped so Lock In can proceed with committed actions.");
            Assert.That(AttackerInput.Program.HasDraft, Is.False);
            Assert.That(AttackerInput.Program.Nodes, Is.Empty);
        }

        /// <summary>
        /// Playtest 2026-08-07: after Lock In rejects an over-budget draft, UNDO must clear that
        /// draft so a later Lock In can succeed (UNDO is the scheduled fix path, not Rewind).
        /// </summary>
        [Test]
        public void UndoClearsOverBudgetDraftSoCommitToPlaybackCanSucceed()
        {
            PlanarPosition start = AttackerInput.Program.CurrentPosition;
            AttackerInput.PrepareRound(start, 1f);
            Phase.GoTo(RoundPhase.Reveal);
            Phase.GoTo(RoundPhase.Program);

            PlanarPosition destination = new PlanarPosition(start.X, start.Y + 1f);
            Assert.That(MoveSeconds(start, destination), Is.GreaterThan(1f));

            AttackerInput.Mode = ActionVerb.Move;
            Assert.That(AttackerInput.TryTapPoint(destination), Is.True);
            Assert.That(AttackerInput.TryCommitDraftPath(), Is.False);
            Assert.That(AttackerInput.Program.HasDraft, Is.True);

            Assert.That(AttackerInput.TryUndoLastStep(), Is.True);
            Assert.That(AttackerInput.Program.HasDraft, Is.False);
            Assert.That(AttackerInput.Program.Nodes, Is.Empty);

            Assert.That(AttackerInput.CommitToPlayback(), Is.True,
                "After undoing the over-budget draft, Lock In must succeed.");
        }

        /// <summary>
        /// Committed queue under budget + pending over-budget draft: Lock In keeps the committed
        /// nodes and drops only the draft.
        /// </summary>
        [Test]
        public void CommitToPlaybackDropsOverBudgetDraftButKeepsCommittedNodes()
        {
            PlanarPosition start = AttackerInput.Program.CurrentPosition;
            AttackerInput.PrepareRound(start, 3f);
            Phase.GoTo(RoundPhase.Reveal);
            Phase.GoTo(RoundPhase.Program);

            // Stay south of Closed Door #1 (y=4): 1m Walk = 2s committed.
            PlanarPosition first = new PlanarPosition(start.X, start.Y + 1f);
            AttackerInput.Mode = ActionVerb.Move;
            Assert.That(AttackerInput.TryTapPoint(first), Is.True);
            Assert.That(AttackerInput.TryCommitDraftPath(), Is.True);
            Assert.That(AttackerInput.Program.Nodes.Count, Is.EqualTo(1));
            float committed = AttackerInput.Program.UsedSeconds;
            Assert.That(committed, Is.EqualTo(2f).Within(0.01f));

            // Another 1m Walk draft would be +2s → 4s total > 3s budget.
            PlanarPosition second = new PlanarPosition(start.X + 1f, start.Y + 1f);
            Assert.That(AttackerInput.TryTapPoint(second), Is.True);
            Assert.That(AttackerInput.Program.HasDraft, Is.True);
            Assert.That(committed + AttackerInput.Program.DraftAllottedSeconds, Is.GreaterThan(3f));

            Assert.That(AttackerInput.CommitToPlayback(), Is.True);
            Assert.That(AttackerInput.Program.HasDraft, Is.False);
            Assert.That(AttackerInput.Program.Nodes.Count, Is.EqualTo(1));
            Assert.That(AttackerInput.Program.UsedSeconds, Is.EqualTo(committed).Within(0.0001f));
        }

        /// <summary>
        /// BUG FOUND 2026-08-11: OutcomeBanner sat in the board region with Unity's default
        /// Text.raycastTarget=true. At ConfigureCamera's fill zoom (ortho 3.4) that band covers
        /// most of Freight Yard's soil floor — Move clicks there were absorbed by UI and never
        /// reached the ground plane (matches the earlier undiagnosed "Yard soil click" report).
        /// </summary>
        [Test]
        public void OutcomeBannerDoesNotRaycastBlockBoardClicks()
        {
            Text banner = FindByName<Text>("OutcomeBanner");
            Assert.That(banner, Is.Not.Null, "Match chrome should include OutcomeBanner.");
            Assert.That(banner.raycastTarget, Is.False,
                "OutcomeBanner is display-only and must not absorb board clicks.");
        }

        /// <summary>
        /// South-Yard screen taps (the band just above the bottom dock) must reach Move scheduling
        /// under the live camera rect + HUD, not die as UI-absorbed no-ops.
        /// </summary>
        [UnityTest]
        public IEnumerator SouthYardScreenClickQueuesMoveThroughFullClickPipeline()
        {
            yield return null;

            Camera cam = Camera.main;
            Assert.That(cam, Is.Not.Null);
            Assert.That(cam.orthographicSize, Is.EqualTo(3.4f).Within(0.05f),
                "Fixture must keep ConfigureCamera's fill zoom — that is what exposes the bug.");

            // Heart of the Yard soil floor: on-screen this sits inside the OutcomeBanner band at ortho 3.4.
            PlanarPosition yardPoint = new PlanarPosition(4f, 2f);
            Assert.That(BoardVisual.Model.InBounds(yardPoint), Is.True);
            Assert.That(yardPoint.DistanceTo(Home), Is.GreaterThan(0.5f),
                "Probe must be away from the spawn so TryAddWaypoint is a real Move.");

            Vector3 screen = cam.WorldToScreenPoint(BoardVisual.WorldFromPlanar(yardPoint));
            Assert.That(screen.z, Is.GreaterThan(0f), "Yard probe should be in front of the camera.");

            // Pre-fix: EventSystem would hit OutcomeBanner here. Post-fix: no UI hit (or only
            // non-blocking graphics), then the click pipeline queues a Move draft.
            var pointer = new PointerEventData(EventSystem.current) { position = screen };
            var uiHits = new System.Collections.Generic.List<RaycastResult>();
            EventSystem.current.RaycastAll(pointer, uiHits);
            foreach (RaycastResult hit in uiHits)
            {
                Assert.That(hit.gameObject.name, Is.Not.EqualTo("OutcomeBanner"),
                    "OutcomeBanner must not be in the UI raycast stack over the Yard.");
            }

            AttackerInput.Mode = ActionVerb.Move;
            Assert.That(AttackerInput.TryClickAtScreenPosition(screen), Is.True,
                $"South-Yard screen click at {screen} should queue a Move via the full pipeline.");
            Assert.That(AttackerInput.Program.HasDraft, Is.True);
            Assert.That(AttackerInput.Program.DraftWaypoints[AttackerInput.Program.DraftWaypoints.Count - 1]
                .DistanceTo(yardPoint), Is.LessThan(0.35f),
                "Resolved planar point should land near the Yard probe.");
        }

        /// <summary>
        /// Clicks inside the bottom HUD dock must still be absorbed (controls stay clickable) and
        /// must not schedule board Moves.
        /// </summary>
        [Test]
        public void HudDockScreenClickIsAbsorbedAndDoesNotQueueMove()
        {
            // Middle of the bottom dock band in screen pixels.
            float x = Screen.width * 0.5f;
            float y = Screen.height * (ProgramHud.HudDockHeight * 0.5f);
            Vector3 dockScreen = new Vector3(x, y, 0f);

            AttackerInput.Mode = ActionVerb.Move;
            int before = AttackerInput.Program.HasDraft
                ? AttackerInput.Program.DraftWaypoints.Count
                : 0;

            Assert.That(AttackerInput.TryClickAtScreenPosition(dockScreen), Is.False,
                "Dock clicks must not fall through to the board.");
            int after = AttackerInput.Program.HasDraft
                ? AttackerInput.Program.DraftWaypoints.Count
                : 0;
            Assert.That(after, Is.EqualTo(before));
        }

        /// <summary>
        /// Near-south in-bounds taps still resolve when the physics underlay is briefly disabled -
        /// ground-plane fallback keeps the bottom edge clickable under fill zoom.
        /// </summary>
        [UnityTest]
        public IEnumerator GroundPlaneFallbackResolvesSouthEdgeWhenUnderlayDisabled()
        {
            yield return null;

            Transform underlay = BoardVisual.transform.Find("RoomFloors/GroundUnderlay");
            Assert.That(underlay, Is.Not.Null, "BoardView should still expose GroundUnderlay.");
            Collider col = underlay.GetComponent<Collider>();
            Assert.That(col, Is.Not.Null);
            col.enabled = false;

            try
            {
                Camera cam = Camera.main;
                PlanarPosition south = new PlanarPosition(4f, 1.2f);
                Vector3 screen = cam.WorldToScreenPoint(BoardVisual.WorldFromPlanar(south));

                AttackerInput.Mode = ActionVerb.Move;
                Assert.That(AttackerInput.TryClickAtScreenPosition(screen), Is.True,
                    "Plane fallback must resolve an in-bounds south Yard tap without the underlay collider.");
                Assert.That(AttackerInput.Program.HasDraft, Is.True);
            }
            finally
            {
                col.enabled = true;
            }
        }

    }
}
