using System.Collections.Generic;
using LogiCard.Cards;
using LogiCard.UI;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace LogiCard.Tests.EditMode
{
    /// <summary>
    /// Presentation smoke for the C62 gear-hand scaffold, plus the Bandage/Storm drag-to-play
    /// gesture added by the Hand Deck Drag Play brief (2026-08-15). No Sim / resolve coverage —
    /// costs stay placeholders until OPEN #16 locks; the actual TryQueueBandageAt/TryQueueStormAt
    /// call is exercised end-to-end in ProgramHudPlayModeTests instead.
    /// </summary>
    [TestFixture]
    public sealed class GearHandViewTests
    {
        private GameObject _canvasGo;
        private RectTransform _canvasRoot;
        private UiFactory _ui;
        private readonly List<Object> _owned = new List<Object>();

        [SetUp]
        public void SetUp()
        {
            _canvasGo = new GameObject("GearHandTestCanvas", typeof(Canvas));
            _owned.Add(_canvasGo);
            _canvasRoot = _canvasGo.GetComponent<RectTransform>();
            // Explicit, generous size — the drag tests below place points at fixed pixel offsets
            // from the hand's measured corners and need real headroom, not whatever a bare
            // RequireComponent(RectTransform) default happens to be.
            _canvasRoot.sizeDelta = new Vector2(1200f, 400f);
            _ui = new UiFactory(null);
        }

        [TearDown]
        public void TearDown()
        {
            for (int i = _owned.Count - 1; i >= 0; i--)
            {
                if (_owned[i] != null)
                {
                    Object.DestroyImmediate(_owned[i]);
                }
            }

            _owned.Clear();
        }

        [Test]
        public void FirstWaveRosterIsBandageInteractFlashbangAdrenalineStorm()
        {
            Assert.That(GearHandView.FirstWave.Length, Is.EqualTo(5));
            Assert.That(GearHandView.FirstWave[0].Id, Is.EqualTo(CardId.Bandage));
            Assert.That(GearHandView.FirstWave[1].Id, Is.EqualTo(CardId.Interact));
            Assert.That(GearHandView.FirstWave[2].Id, Is.EqualTo(CardId.Flashbang));
            Assert.That(GearHandView.FirstWave[3].Id, Is.EqualTo(CardId.Adrenaline));
            Assert.That(GearHandView.FirstWave[4].Id, Is.EqualTo(CardId.Storm));
        }

        [Test]
        public void CostLabelsStayPlaceholdersUntilNumericsLockExceptBandage()
        {
            foreach (GearHandCardInfo info in GearHandView.FirstWave)
            {
                if (info.Id == CardId.Bandage)
                {
                    Assert.That(info.CostLabel, Is.EqualTo("3s"), "Bandage's cost locked via C63.");
                    continue;
                }

                Assert.That(info.CostLabel, Is.EqualTo("TR —"),
                    $"{info.Id} must not invent a Time Resource cost while OPEN #16 is open.");
            }
        }

        [Test]
        public void BuildCreatesStableHitTargetsForEachCard()
        {
            GearHandView hand = GearHandView.Build(_ui, _canvasRoot, Vector2.zero, Vector2.one);
            Assert.That(hand.Root.name, Is.EqualTo(GearHandView.RootName));

            foreach (GearHandCardInfo info in GearHandView.FirstWave)
            {
                Button button = hand.GetButton(info.Id);
                Assert.That(button, Is.Not.Null, $"Missing button for {info.Id}");
                Assert.That(button.name, Is.EqualTo(GearHandView.ButtonName(info.Id)));
            }
        }

        /// <summary>
        /// Interact stands in for "any click-to-arm card" here — Bandage moved to drag-to-play (see
        /// the CardDrag* tests below) and no longer arms via click at all.
        /// </summary>
        [Test]
        public void ProgramPhaseArmsInteractAndIgnoresAdrenaline()
        {
            GearHandView hand = GearHandView.Build(_ui, _canvasRoot, Vector2.zero, Vector2.one);
            CardId? armed = null;
            hand.CardArmed += id => armed = id;

            Assert.That(hand.IsInteractable(CardId.Interact), Is.True);
            Assert.That(hand.IsInteractable(CardId.Adrenaline), Is.False);

            hand.GetButton(CardId.Interact).onClick.Invoke();
            Assert.That(armed, Is.EqualTo(CardId.Interact));
            Assert.That(hand.ArmedId, Is.EqualTo(CardId.Interact));

            hand.GetButton(CardId.Adrenaline).onClick.Invoke();
            Assert.That(hand.ArmedId, Is.EqualTo(CardId.Interact),
                "Adrenaline must stay inert during Program presentation.");
        }

        /// <summary>
        /// Bandage/Storm no longer carry an onClick listener at all (Hand Deck Drag Play brief) — a
        /// plain click is a no-op, not an arm. <see cref="GearHandView.CardArmed"/> must never fire
        /// for them.
        /// </summary>
        [Test]
        public void ClickingBandageOrStormDoesNothing()
        {
            GearHandView hand = GearHandView.Build(_ui, _canvasRoot, Vector2.zero, Vector2.one);
            CardId? armed = null;
            hand.CardArmed += id => armed = id;

            hand.GetButton(CardId.Bandage).onClick.Invoke();
            hand.GetButton(CardId.Storm).onClick.Invoke();

            Assert.That(armed, Is.Null, "A plain click on Bandage/Storm must never arm — only a completed drag plays them.");
            Assert.That(hand.ArmedId, Is.Null);
        }

        [Test]
        public void BandageAndStormCarryADragControllerOthersDoNot()
        {
            GearHandView hand = GearHandView.Build(_ui, _canvasRoot, Vector2.zero, Vector2.one);

            Assert.That(hand.GetDragController(CardId.Bandage), Is.Not.Null);
            Assert.That(hand.GetDragController(CardId.Storm), Is.Not.Null);
            Assert.That(hand.GetDragController(CardId.Interact), Is.Null);
            Assert.That(hand.GetDragController(CardId.Flashbang), Is.Null);
            Assert.That(hand.GetDragController(CardId.Adrenaline), Is.Null);
        }

        /// <summary>
        /// Drag-past-threshold + release outside the hand plays the card: fires
        /// <see cref="GearHandView.CardPlayRequested"/> (the signal <see cref="ProgramHud"/> turns
        /// into <c>TryQueueBandageAt</c>/<c>TryQueueStormAt</c>) exactly once.
        /// </summary>
        [Test]
        public void DragPastThresholdAndReleaseOutsideHandRequestsPlay()
        {
            GearHandView hand = GearHandView.Build(_ui, _canvasRoot, Vector2.zero, Vector2.one);
            GearHandView.CardDragController drag = hand.GetDragController(CardId.Bandage);
            Assert.That(drag, Is.Not.Null);

            CardId? requested = null;
            hand.CardPlayRequested += id => requested = id;

            Vector2 outside = OutsideHandScreenPoint(hand);
            var data = new PointerEventData(null) { pressPosition = Vector2.zero, position = Vector2.zero };
            drag.OnBeginDrag(data);

            data.position = outside;
            data.delta = outside;
            drag.OnDrag(data);
            drag.OnEndDrag(data);

            Assert.That(requested, Is.EqualTo(CardId.Bandage));
        }

        /// <summary>Short drag (below <see cref="GearHandView.DragThresholdPixels"/>) cancels — no play attempt.</summary>
        [Test]
        public void ShortDragCancelsWithNoPlayRequest()
        {
            GearHandView hand = GearHandView.Build(_ui, _canvasRoot, Vector2.zero, Vector2.one);
            GearHandView.CardDragController drag = hand.GetDragController(CardId.Storm);
            Assert.That(drag, Is.Not.Null);

            bool requested = false;
            hand.CardPlayRequested += _ => requested = true;

            var data = new PointerEventData(null) { pressPosition = Vector2.zero, position = Vector2.zero };
            drag.OnBeginDrag(data);
            data.position = new Vector2(5f, 0f);
            data.delta = data.position;
            drag.OnDrag(data);
            drag.OnEndDrag(data);

            Assert.That(requested, Is.False, "A 5px drag is well under DragThresholdPixels — must cancel, not play.");
        }

        /// <summary>Past-threshold drag that releases back inside the hand cancels too — not just short drags.</summary>
        [Test]
        public void PastThresholdDragReleasedInsideHandCancels()
        {
            GearHandView hand = GearHandView.Build(_ui, _canvasRoot, Vector2.zero, Vector2.one);
            GearHandView.CardDragController drag = hand.GetDragController(CardId.Bandage);

            bool requested = false;
            hand.CardPlayRequested += _ => requested = true;

            Vector2 insideFarPoint = InsideHandScreenPoint(hand) + new Vector2(GearHandView.DragThresholdPixels + 20f, 0f);
            var data = new PointerEventData(null) { pressPosition = InsideHandScreenPoint(hand), position = InsideHandScreenPoint(hand) };
            drag.OnBeginDrag(data);
            data.position = insideFarPoint;
            data.delta = insideFarPoint - data.pressPosition;
            drag.OnDrag(data);
            drag.OnEndDrag(data);

            Assert.That(requested, Is.False, "Releasing back inside the hand must cancel even past the drag threshold.");
        }

        /// <summary>A SetSpent-blocked card never starts a drag at all — matches today's IsInteractable gate.</summary>
        [Test]
        public void DragOnSpentCardIsANoOp()
        {
            GearHandView hand = GearHandView.Build(_ui, _canvasRoot, Vector2.zero, Vector2.one);
            hand.SetSpent(CardId.Bandage, true);
            GearHandView.CardDragController drag = hand.GetDragController(CardId.Bandage);

            bool requested = false;
            hand.CardPlayRequested += _ => requested = true;

            Vector2 outside = OutsideHandScreenPoint(hand);
            var data = new PointerEventData(null) { pressPosition = Vector2.zero, position = Vector2.zero };
            drag.OnBeginDrag(data);
            data.position = outside;
            data.delta = outside;
            drag.OnDrag(data);
            drag.OnEndDrag(data);

            Assert.That(requested, Is.False, "A blocked card must not play no matter how the drag would otherwise resolve.");
            Assert.That(hand.GetButton(CardId.Bandage).GetComponent<Image>().color, Is.Not.EqualTo(UiStyle.ModalPrimaryButton),
                "A drag that never started must not leave the highlighted/dragging face behind.");
        }

        [Test]
        public void BeginDragHighlightsTheCardLikeAnArmedOne()
        {
            GearHandView hand = GearHandView.Build(_ui, _canvasRoot, Vector2.zero, Vector2.one);
            Button bandage = hand.GetButton(CardId.Bandage);
            GearHandView.CardDragController drag = hand.GetDragController(CardId.Bandage);

            var data = new PointerEventData(null) { pressPosition = Vector2.zero, position = Vector2.zero };
            drag.OnBeginDrag(data);

            Assert.That(bandage.GetComponent<Image>().color, Is.EqualTo(UiStyle.ModalPrimaryButton),
                "Mid-drag cardstock should reuse the same Modal* primary tokens as an armed card.");

            drag.OnEndDrag(data);
            Assert.That(bandage.GetComponent<Image>().color, Is.Not.EqualTo(UiStyle.ModalPrimaryButton),
                "Cancelling the drag must drop the highlight along with snapping the card back.");
        }

        private static Vector2 OutsideHandScreenPoint(GearHandView hand)
        {
            var corners = new Vector3[4];
            hand.Root.GetWorldCorners(corners);
            return new Vector2(corners[0].x, corners[0].y) - new Vector2(500f, 500f);
        }

        private static Vector2 InsideHandScreenPoint(GearHandView hand)
        {
            var corners = new Vector3[4];
            hand.Root.GetWorldCorners(corners);
            return (corners[0] + corners[2]) * 0.5f;
        }

        [Test]
        public void ExecutePhaseSurfacesAdrenalineOnly()
        {
            GearHandView hand = GearHandView.Build(_ui, _canvasRoot, Vector2.zero, Vector2.one);
            hand.SetPhase(GearHandPhase.Execute);

            Assert.That(hand.IsInteractable(CardId.Bandage), Is.False);
            Assert.That(hand.IsInteractable(CardId.Interact), Is.False);
            Assert.That(hand.IsInteractable(CardId.Flashbang), Is.False);
            Assert.That(hand.IsInteractable(CardId.Adrenaline), Is.True);

            hand.GetButton(CardId.Adrenaline).onClick.Invoke();
            Assert.That(hand.ArmedId, Is.EqualTo(CardId.Adrenaline));
        }

        [Test]
        public void ReclickArmedCardClearsArm()
        {
            GearHandView hand = GearHandView.Build(_ui, _canvasRoot, Vector2.zero, Vector2.one);
            bool cleared = false;
            hand.ArmCleared += () => cleared = true;

            hand.GetButton(CardId.Flashbang).onClick.Invoke();
            Assert.That(hand.ArmedId, Is.EqualTo(CardId.Flashbang));

            hand.GetButton(CardId.Flashbang).onClick.Invoke();
            Assert.That(hand.ArmedId, Is.Null);
            Assert.That(cleared, Is.True);
        }

        [Test]
        public void SpentOncePerMatchCardStopsArming()
        {
            GearHandView hand = GearHandView.Build(_ui, _canvasRoot, Vector2.zero, Vector2.one);
            hand.SetPhase(GearHandPhase.Execute);
            hand.GetButton(CardId.Adrenaline).onClick.Invoke();
            Assert.That(hand.ArmedId, Is.EqualTo(CardId.Adrenaline));

            hand.SetSpent(CardId.Adrenaline);
            Assert.That(hand.IsSpent(CardId.Adrenaline), Is.True);
            Assert.That(hand.ArmedId, Is.Null);
            Assert.That(hand.IsInteractable(CardId.Adrenaline), Is.False);
        }

        [Test]
        public void ArmedCardUsesModalPrimaryFace()
        {
            GearHandView hand = GearHandView.Build(_ui, _canvasRoot, Vector2.zero, Vector2.one);
            Button interact = hand.GetButton(CardId.Interact);
            interact.onClick.Invoke();

            Assert.That(interact.GetComponent<Image>().color, Is.EqualTo(UiStyle.ModalPrimaryButton),
                "Armed cardstock should reuse Modal* primary tokens, not invent a HUD palette.");
        }
    }
}
