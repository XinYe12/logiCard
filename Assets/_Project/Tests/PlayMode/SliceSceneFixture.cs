using LogiCard.Board;
using LogiCard.Boot;
using LogiCard.Sim;
using LogiCard.Timeline;
using LogiCard.UI;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UIElements;

namespace LogiCard.Tests.PlayMode
{
    /// <summary>
    /// Stands up the real <see cref="GameBootstrap"/> composition root in play mode, so tests read the
    /// same board, HUD, clock and input wiring the player gets instead of a hand-built stand-in.
    /// Plays a default Time Card so tests begin in Program with a funded round (C33).
    /// </summary>
    public abstract class SliceSceneFixture
    {
        protected const float DefaultRoundSeconds = 60f;

        protected GameBootstrap Bootstrap { get; private set; }

        protected BoardView BoardVisual { get; private set; }

        protected PawnView AttackerPawn { get; private set; }

        protected BoardInputController AttackerInput { get; private set; }

        protected TimeResourceClockDriver Clock { get; private set; }

        protected RoundPhaseController Phase { get; private set; }

        protected ProgramHud Hud { get; private set; }

        protected RoundPlayback Playback { get; private set; }

        protected PawnView DefenderPawn { get; private set; }

        protected MatchClock MatchClock { get; private set; }

        /// <summary>The attacker's start point for this round, read back from the program.</summary>
        protected PlanarPosition Home { get; private set; }

        [SetUp]
        public void BuildScene()
        {
            Bootstrap = new GameObject("TestBootstrap").AddComponent<GameBootstrap>();

            Hud = Object.FindFirstObjectByType<ProgramHud>();
            Clock = Bootstrap.GetComponent<TimeResourceClockDriver>();
            Phase = Bootstrap.GetComponent<RoundPhaseController>();
            MatchClock = Bootstrap.MatchClock;

            Assert.That(Hud, Is.Not.Null, "Bootstrap built no ProgramHud.");
            Assert.That(Clock, Is.Not.Null, "Bootstrap built no TimeResourceClockDriver.");
            Assert.That(Phase, Is.Not.Null, "Bootstrap built no RoundPhaseController.");
            Assert.That(MatchClock, Is.Not.Null, "Bootstrap built no MatchClock.");

            // Phase 1 landscape shell starts on Boot/Title; match HUD tests skip straight to Program.
            // The map-select addition (2026-08-10) defers board/pawn/playback building until AppFlow
            // reaches the match — bypass BEFORE looking those up, or they don't exist yet.
            Hud.BypassAppFlowForTests();

            BoardVisual = Object.FindFirstObjectByType<BoardView>();
            AttackerInput = Object.FindFirstObjectByType<BoardInputController>();
            AttackerPawn = AttackerInput != null ? AttackerInput.GetComponent<PawnView>() : null;
            Playback = Bootstrap.GetComponent<RoundPlayback>();
            DefenderPawn = FindByName<PawnView>("Pawn_Defender");

            Assert.That(BoardVisual, Is.Not.Null, "Bootstrap built no BoardView.");
            Assert.That(AttackerInput, Is.Not.Null, "Bootstrap wired no BoardInputController.");
            Assert.That(AttackerPawn, Is.Not.Null, "BoardInputController is not on a pawn.");
            Assert.That(Playback, Is.Not.Null, "Bootstrap built no RoundPlayback.");
            Assert.That(DefenderPawn, Is.Not.Null, "Bootstrap built no defender pawn.");

            Assert.That(Bootstrap.BeginRound(DefaultRoundSeconds), Is.True, "Default Time Card failed.");
            Assert.That(Phase.Phase, Is.EqualTo(RoundPhase.Program));

            Home = AttackerInput.Program.CurrentPosition;
        }

        [TearDown]
        public void TearDownScene()
        {
            if (Bootstrap != null)
            {
                Object.DestroyImmediate(Bootstrap.gameObject);
            }

            foreach (Camera camera in Object.FindObjectsByType<Camera>(FindObjectsSortMode.None))
            {
                Object.DestroyImmediate(camera.gameObject);
            }

            foreach (EventSystem eventSystem in Object.FindObjectsByType<EventSystem>(FindObjectsSortMode.None))
            {
                Object.DestroyImmediate(eventSystem.gameObject);
            }
        }

        /// <summary>Time Resource cost of moving between two points at the attacker's own base speed.</summary>
        protected float MoveSeconds(PlanarPosition from, PlanarPosition to)
        {
            return TimeResourceMath.SegmentSeconds(from, to, AttackerInput.Program.BaseSecondsPerTile, StanceType.Walk);
        }

        protected static T FindByName<T>(string name) where T : Component
        {
            foreach (T candidate in Object.FindObjectsByType<T>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            {
                if (candidate.gameObject.name == name)
                {
                    return candidate;
                }
            }

            return null;
        }

        /// <summary>
        /// UI Toolkit analog of <see cref="FindByName{T}"/> — VisualElements aren't Components, so
        /// they're queried through whichever <see cref="UIDocument"/>'s tree contains them.
        /// </summary>
        protected static T FindVisualElement<T>(string name) where T : VisualElement
        {
            foreach (UIDocument document in Object.FindObjectsByType<UIDocument>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            {
                VisualElement root = document.rootVisualElement;
                if (root == null)
                {
                    continue;
                }

                T found = root.Q<T>(name);
                if (found != null)
                {
                    return found;
                }
            }

            return null;
        }

        /// <summary>
        /// Synthesizes a click on a UI Toolkit element — the closest analog to uGUI's
        /// <c>Button.onClick.Invoke()</c> tests use elsewhere. Dispatched through the real event
        /// pipeline (<see cref="VisualElement.SendEvent"/>) so any <c>RegisterCallback&lt;ClickEvent&gt;</c>
        /// handler on the element fires, same as a real pointer click would.
        /// </summary>
        protected static void ClickVisualElement(VisualElement element)
        {
            Assert.That(element, Is.Not.Null, "ClickVisualElement called with a null element.");
            using (ClickEvent evt = ClickEvent.GetPooled())
            {
                evt.target = element;
                element.SendEvent(evt);
            }
        }
    }
}
