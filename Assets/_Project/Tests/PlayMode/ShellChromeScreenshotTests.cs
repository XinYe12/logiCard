using System.Collections;
using System.IO;
using LogiCard.UI;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UI;

namespace LogiCard.Tests.PlayMode
{
    /// <summary>
    /// Visual-verification harness for the non-HUD shell screens (docs/ui/UI_SHELL_CHROME.md).
    ///
    /// This project's standing rule is that batchmode tests prove code correctness, not visual
    /// correctness — a chrome/layout change is not "done" until someone has actually looked at it. On a
    /// headless seat "looked at it" has to mean a rendered PNG, so this walks Boot → Character Select →
    /// Map Select → Lobby → Round Result → Match End and writes one 1920×1080 capture per screen.
    ///
    /// Opt-in: does nothing unless the <c>LOGICARD_SHOT_DIR</c> environment variable names an output
    /// directory, and it must be run WITHOUT <c>-nographics</c> (there is no renderer otherwise):
    ///
    ///   LOGICARD_SHOT_DIR=/tmp/shots Unity -batchmode -projectPath . -runTests -testPlatform PlayMode \
    ///     -testResults out.xml -logFile out.txt
    ///
    /// The Canvas is temporarily flipped from ScreenSpaceOverlay to ScreenSpaceCamera against a
    /// 1920×1080 RenderTexture — an Overlay canvas cannot be read back into a texture at all, and going
    /// through a RenderTexture also pins the capture to the CanvasScaler's reference resolution instead
    /// of whatever window size batchmode happens to pick.
    /// </summary>
    [TestFixture]
    public sealed class ShellChromeScreenshotTests : SliceSceneFixture
    {
        private const string OutputDirVariable = "LOGICARD_SHOT_DIR";
        private const int ShotWidth = 1920;
        private const int ShotHeight = 1080;

        [UnityTest]
        public IEnumerator CaptureShellScreens()
        {
            string outputDir = System.Environment.GetEnvironmentVariable(OutputDirVariable);
            if (string.IsNullOrEmpty(outputDir))
            {
                // Not a screenshot run — stay a no-op so the normal PlayMode suite is unaffected.
                yield break;
            }

            Directory.CreateDirectory(outputDir);

            AppFlowController flow = Hud.AppFlow;
            Assert.That(flow, Is.Not.Null);

            Canvas canvas = FindByName<Canvas>("HUD");
            Assert.That(canvas, Is.Not.Null, "Screenshot harness could not find the HUD canvas.");

            var rt = new RenderTexture(ShotWidth, ShotHeight, 24, RenderTextureFormat.ARGB32);
            var camGo = new GameObject("ShellShotCamera", typeof(Camera));
            // Parked far from the arena so only the canvas plane is ever in frame.
            camGo.transform.position = new Vector3(0f, 10000f, 0f);
            var cam = camGo.GetComponent<Camera>();
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = Color.black;
            cam.targetTexture = rt;
            cam.orthographic = false;
            cam.nearClipPlane = 0.1f;
            cam.farClipPlane = 100f;

            RenderMode previousMode = canvas.renderMode;
            Camera previousCamera = canvas.worldCamera;
            canvas.renderMode = RenderMode.ScreenSpaceCamera;
            canvas.worldCamera = cam;
            canvas.planeDistance = 10f;

            yield return null;
            yield return null;

            flow.ShowBoot();
            yield return Capture(cam, rt, outputDir, "01-boot");

            Button play = FindByName<Button>("TitlePlayButton");
            play.onClick.Invoke();
            yield return Capture(cam, rt, outputDir, "02-character-select-scout");

            Button next = FindByName<Button>("CharSelectNext");
            next.onClick.Invoke();
            yield return new WaitForSecondsRealtime(0.8f);
            yield return Capture(cam, rt, outputDir, "03-character-select-juggernaut");

            FindByName<Button>("ConfirmCharacter").onClick.Invoke();
            yield return Capture(cam, rt, outputDir, "04-map-select");

            FindByName<Button>("Pick_VaultComplex").onClick.Invoke();
            yield return Capture(cam, rt, outputDir, "05-map-select-vault");

            FindByName<Button>("ConfirmMap").onClick.Invoke();
            yield return Capture(cam, rt, outputDir, "06-lobby");

            flow.ShowRoundResult("ROUND 3 — OBJECTIVE HELD", matchOver: false);
            yield return Capture(cam, rt, outputDir, "07-round-result");

            flow.ShowMatchEnd("ATTACKER WINS");
            yield return Capture(cam, rt, outputDir, "08-match-end");

            FindByName<Button>("QuitToTitleButton").onClick.Invoke();
            yield return null;
            yield return Capture(cam, rt, outputDir, "09-quit-modal");

            canvas.renderMode = previousMode;
            canvas.worldCamera = previousCamera;
            cam.targetTexture = null;
            Object.Destroy(camGo);
            Object.Destroy(rt);
            yield return null;
        }

        private static IEnumerator Capture(Camera cam, RenderTexture rt, string dir, string name)
        {
            // Two frames so freshly-activated screens have laid out and any Shadow/effect meshes rebuilt.
            // Deliberately NOT WaitForEndOfFrame: that yield instruction never resumes under
            // -batchmode, so a harness built on it hangs forever instead of capturing anything.
            yield return null;
            Canvas.ForceUpdateCanvases();
            yield return null;

            cam.Render();

            RenderTexture previous = RenderTexture.active;
            RenderTexture.active = rt;
            var tex = new Texture2D(rt.width, rt.height, TextureFormat.RGB24, false);
            tex.ReadPixels(new Rect(0f, 0f, rt.width, rt.height), 0, 0);
            tex.Apply();
            RenderTexture.active = previous;

            File.WriteAllBytes(Path.Combine(dir, name + ".png"), tex.EncodeToPNG());
            Object.Destroy(tex);
        }
    }
}
