using System.Collections.Generic;
using LogiCard.Cards;
using LogiCard.UI;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.UI;

namespace LogiCard.Tests.EditMode
{
    /// <summary>
    /// Presentation smoke for the C62 gear-hand scaffold. No Sim / resolve coverage — costs stay
    /// placeholders until OPEN #16 locks.
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

        [Test]
        public void ProgramPhaseArmsBandageAndIgnoresAdrenaline()
        {
            GearHandView hand = GearHandView.Build(_ui, _canvasRoot, Vector2.zero, Vector2.one);
            CardId? armed = null;
            hand.CardArmed += id => armed = id;

            Assert.That(hand.IsInteractable(CardId.Bandage), Is.True);
            Assert.That(hand.IsInteractable(CardId.Adrenaline), Is.False);

            hand.GetButton(CardId.Bandage).onClick.Invoke();
            Assert.That(armed, Is.EqualTo(CardId.Bandage));
            Assert.That(hand.ArmedId, Is.EqualTo(CardId.Bandage));

            hand.GetButton(CardId.Adrenaline).onClick.Invoke();
            Assert.That(hand.ArmedId, Is.EqualTo(CardId.Bandage),
                "Adrenaline must stay inert during Program presentation.");
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
            Button bandage = hand.GetButton(CardId.Bandage);
            bandage.onClick.Invoke();

            Assert.That(bandage.GetComponent<Image>().color, Is.EqualTo(UiStyle.ModalPrimaryButton),
                "Armed cardstock should reuse Modal* primary tokens, not invent a HUD palette.");
        }
    }
}
