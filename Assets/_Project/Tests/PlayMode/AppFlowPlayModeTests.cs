using System.Collections;
using LogiCard.UI;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UIElements;
using Button = UnityEngine.UI.Button;

namespace LogiCard.Tests.PlayMode
{
    /// <summary>
    /// Cold-path click-through for UI_FLOW.md's pre-match shell (Phase 1 — functional, not polish).
    /// </summary>
    [TestFixture]
    public sealed class AppFlowPlayModeTests : SliceSceneFixture
    {
        [UnityTest]
        public IEnumerator BootThroughLobbyLocalPlayReachesMatchHud()
        {
            // Fixture already bypassed into match — rebuild the shell path from Boot.
            AppFlowController flow = Hud.AppFlow;
            Assert.That(flow, Is.Not.Null);

            flow.ShowBoot();
            Assert.That(flow.Current, Is.EqualTo(AppFlowController.Screen.Boot));
            Assert.That(flow.IsInMatch, Is.False);

            Button play = FindByName<Button>("TitlePlayButton");
            Assert.That(play, Is.Not.Null, "Boot screen has no TitlePlayButton.");
            play.onClick.Invoke();
            Assert.That(flow.Current, Is.EqualTo(AppFlowController.Screen.CharacterSelect));

            // Pick_Juggernaut is now a UI Toolkit VisualElement (Character Select's UI Toolkit pilot).
            VisualElement juggernaut = FindVisualElement<VisualElement>("Pick_Juggernaut");
            Assert.That(juggernaut, Is.Not.Null);
            ClickVisualElement(juggernaut);
            Assert.That(flow.SelectedArchetype, Is.EqualTo("Juggernaut"));

            Button confirm = FindByName<Button>("ConfirmCharacter");
            confirm.onClick.Invoke();
            Assert.That(flow.Current, Is.EqualTo(AppFlowController.Screen.MapSelect));

            Button confirmMap = FindByName<Button>("ConfirmMap");
            Assert.That(confirmMap, Is.Not.Null, "Map Select screen has no ConfirmMap button.");
            confirmMap.onClick.Invoke();
            Assert.That(flow.Current, Is.EqualTo(AppFlowController.Screen.Lobby));

            Button local = FindByName<Button>("LocalPlayButton");
            Assert.That(local, Is.Not.Null);
            local.onClick.Invoke();
            Assert.That(flow.Current, Is.EqualTo(AppFlowController.Screen.None));
            Assert.That(flow.IsInMatch, Is.True);

            Assert.That(FindByName<Button>("LockInButton"), Is.Not.Null);
            Assert.That(FindByName<Transform>("HudDock"), Is.Not.Null);
            yield return null;
        }

        [UnityTest]
        public IEnumerator CharacterSelectNextRotatesArchetypeAfterCrossfade()
        {
            AppFlowController flow = Hud.AppFlow;
            flow.ShowBoot();

            Button play = FindByName<Button>("TitlePlayButton");
            play.onClick.Invoke();
            Assert.That(flow.Current, Is.EqualTo(AppFlowController.Screen.CharacterSelect));
            Assert.That(flow.SelectedArchetype, Is.EqualTo("Scout"));

            VisualElement next = FindVisualElement<VisualElement>("CharSelectNext");
            Assert.That(next, Is.Not.Null, "Character Select carousel needs CharSelectNext.");
            ClickVisualElement(next);
            Assert.That(flow.SelectedArchetype, Is.EqualTo("Juggernaut"),
                "Selection should update immediately on navigate.");

            yield return new WaitForSecondsRealtime(0.7f);

            Assert.That(flow.SelectedArchetype, Is.EqualTo("Juggernaut"));
            Assert.That(flow.Current, Is.EqualTo(AppFlowController.Screen.CharacterSelect));

            Button confirm = FindByName<Button>("ConfirmCharacter");
            confirm.onClick.Invoke();
            Assert.That(flow.Current, Is.EqualTo(AppFlowController.Screen.MapSelect));
        }

        [UnityTest]
        public IEnumerator FindMatchStubEntersMatchAfterDelay()
        {
            AppFlowController flow = Hud.AppFlow;
            flow.ShowLobby();

            Button find = FindByName<Button>("FindMatchButton");
            Assert.That(find, Is.Not.Null);
            find.onClick.Invoke();

            float deadline = Time.realtimeSinceStartup + 5f;
            while (!flow.IsInMatch && Time.realtimeSinceStartup < deadline)
            {
                yield return null;
            }

            Assert.That(flow.IsInMatch, Is.True, "Find Match stub never entered the match.");
            Assert.That(flow.Current, Is.EqualTo(AppFlowController.Screen.None));
        }

        [Test]
        public void LandscapeDockConstantsExposeBottomBandGeometry()
        {
            Assert.That(ProgramHud.HudDockHeight, Is.EqualTo(0.34f),
                "Dock moved to a bottom band (2026-08-10) — this is the real dock extent now.");
            Assert.That(ProgramHud.TopStripHeight, Is.GreaterThan(0f));
            Assert.That(ProgramHud.HudDockHeight + ProgramHud.TopStripHeight, Is.LessThan(1f),
                "Board region must remain the majority of the frame.");
        }

        [UnityTest]
        public IEnumerator MatchEndQuitOpensConfirmDialogBeforeLeaving()
        {
            AppFlowController flow = Hud.AppFlow;
            flow.ShowMatchEnd("MATCH OVER");
            Assert.That(flow.Current, Is.EqualTo(AppFlowController.Screen.MatchEnd));

            Button quit = FindByName<Button>("QuitToTitleButton");
            Assert.That(quit, Is.Not.Null);
            quit.onClick.Invoke();
            yield return null;

            Assert.That(FindByName<Transform>("ModalDialog"), Is.Not.Null, "Quit should open ModalDialog.");
            Assert.That(flow.Current, Is.EqualTo(AppFlowController.Screen.MatchEnd),
                "Quit must not leave Match End until the dialog is confirmed.");

            Button confirm = FindByName<Button>("ModalPrimary");
            Assert.That(confirm, Is.Not.Null);
            confirm.onClick.Invoke();
            yield return null;

            Assert.That(flow.Current, Is.EqualTo(AppFlowController.Screen.Boot));
            Assert.That(FindByName<Transform>("ModalDialog"), Is.Null);
        }
    }
}
