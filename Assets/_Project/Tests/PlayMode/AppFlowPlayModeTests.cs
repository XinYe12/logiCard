using System.Collections;
using LogiCard.UI;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UI;

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

            Button juggernaut = FindByName<Button>("Pick_Juggernaut");
            Assert.That(juggernaut, Is.Not.Null);
            juggernaut.onClick.Invoke();
            Assert.That(flow.SelectedArchetype, Is.EqualTo("Juggernaut"));

            Button confirm = FindByName<Button>("ConfirmCharacter");
            confirm.onClick.Invoke();
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
        public void LandscapeDockConstantsExposeRightEdgeGeometry()
        {
            Assert.That(ProgramHud.HudDockWidth, Is.GreaterThan(0f));
            Assert.That(ProgramHud.HudDockHeight, Is.EqualTo(0f),
                "Dock is a right margin; bottom-band height must stay 0 for Integrator camera wiring.");
            Assert.That(ProgramHud.ThumbZoneHeight, Is.EqualTo(ProgramHud.HudDockHeight));
            Assert.That(ProgramHud.TopStripHeight, Is.GreaterThan(0f));
            Assert.That(ProgramHud.HudDockWidth + ProgramHud.TopStripHeight, Is.LessThan(1f),
                "Board region must remain the majority of the frame.");
        }

    }
}
