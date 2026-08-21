using System.Collections;
using System.IO;
using LogiCard.Net;
using LogiCard.Sim;
using LogiCard.UI;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UI;

namespace LogiCard.Tests.PlayMode
{
    /// <summary>
    /// Visual-verification harness for the board-anchored Bomber prompt (C36/C71), same opt-in shape and
    /// same reasoning as <see cref="ShellChromeScreenshotTests"/>: batchmode proves the prompt's logic,
    /// not that it renders beside the wall at a sane size — and on a headless seat "someone looked at
    /// it" has to mean a rendered PNG.
    ///
    /// Opt-in via <c>LOGICARD_SHOT_DIR</c> and must run WITHOUT <c>-nographics</c>; a normal PlayMode run
    /// no-ops out on the first line so the suite is unaffected:
    ///
    ///   LOGICARD_SHOT_DIR=/tmp/shots Unity -batchmode -projectPath . -runTests -testPlatform PlayMode \
    ///     -testResults out.xml -logFile out.txt
    ///
    /// Unlike the shell harness this renders through the *game* camera rather than a parked canvas-only
    /// one, because the whole point of a board-anchored control is where it lands relative to the board
    /// geometry it describes — a capture of the canvas alone could not show that at all.
    /// </summary>
    [TestFixture]
    public sealed class BomberPromptScreenshotTests : SliceSceneFixture
    {
        private const string OutputDirVariable = "LOGICARD_SHOT_DIR";
        private const int ShotWidth = 1920;
        private const int ShotHeight = 1080;

        [UnityTest]
        public IEnumerator CaptureBomberPromptStates()
        {
            string outputDir = System.Environment.GetEnvironmentVariable(OutputDirVariable);
            if (string.IsNullOrEmpty(outputDir))
            {
                yield break;
            }

            Directory.CreateDirectory(outputDir);

            var point = new BreachPoint(
                new Segment(
                    new PlanarPosition(Home.X - 1f, Home.Y + 0.4f),
                    new PlanarPosition(Home.X + 1f, Home.Y + 0.4f)),
                BreachState.Intact,
                "Breach Point #1");
            BoardVisual.Model.RegisterBreachPoint(point);

            Canvas canvas = FindByName<Canvas>("HUD");
            Camera cam = Camera.main;
            Assert.That(canvas, Is.Not.Null, "Screenshot harness could not find the HUD canvas.");
            Assert.That(cam, Is.Not.Null, "Screenshot harness needs the live game camera.");

            var rt = new RenderTexture(ShotWidth, ShotHeight, 24, RenderTextureFormat.ARGB32);
            RenderTexture previousTarget = cam.targetTexture;
            Rect previousRect = cam.rect;
            RenderMode previousMode = canvas.renderMode;
            Camera previousCanvasCamera = canvas.worldCamera;

            cam.targetTexture = rt;
            // The live camera is letterboxed to the MapViewport hole; a ScreenSpaceCamera canvas
            // renders inside that same viewport rect, which would squash the whole HUD into the top
            // band of the capture. Full-frame for the shot instead — the board is framed wider than in
            // play, but every UI element lands at its real relative position, which is what this is for.
            cam.rect = new Rect(0f, 0f, 1f, 1f);
            canvas.renderMode = RenderMode.ScreenSpaceCamera;
            canvas.worldCamera = cam;
            // Well inside the near plane of the (orthographic) board camera so HUD chrome always
            // composites in front of the board rather than being clipped by arena geometry.
            canvas.planeDistance = 1f;

            // Let the canvas re-lay-out against the RenderTexture's dimensions BEFORE anything projects
            // a board point into it — the prompt's placement math reads the canvas rect, so refreshing
            // it against the pre-flip rect would park it off-frame. The long warm-up is for the camera,
            // not the canvas: BoardCameraRig eases toward its framing over many frames, and capturing
            // mid-ease put the pawn (and therefore the prompt) outside the frame entirely.
            for (int i = 0; i < 150; i++)
            {
                yield return null;
            }

            Canvas.ForceUpdateCanvases();
            yield return null;

            FindByName<Button>("Mode_Bomber").onClick.Invoke();
            Assert.That(AttackerInput.TryTapPoint(Home), Is.True, "Screenshot setup failed to select the breach point.");
            yield return Capture(cam, rt, outputDir, "bomber-01-intact-attach-offered");

            FindByName<Button>("Bomb_Attach").onClick.Invoke();
            yield return Capture(cam, rt, outputDir, "bomber-02-bomb-set-detonate-offered");

            FindByName<Button>("Bomb_Detonate").onClick.Invoke();
            yield return Capture(cam, rt, outputDir, "bomber-03-breached-no-options");

            Assert.That(AttackerInput.Program.Nodes.Count, Is.EqualTo(2),
                "Screenshot run should have booked exactly Attach + Detonate.");
            Assert.That(AttackerInput.Program.Nodes[1].Verb, Is.EqualTo(ActionVerb.BombDetonate));

            canvas.renderMode = previousMode;
            canvas.worldCamera = previousCanvasCamera;
            cam.targetTexture = previousTarget;
            cam.rect = previousRect;
            Object.Destroy(rt);
            yield return null;
        }

        /// <summary>
        /// Harness-only placement correction, and the one thing in this file that does not exercise
        /// shipped code. <c>ProgramHud.RefreshBombPrompt</c> passes <c>null</c> as the camera argument to
        /// <see cref="RectTransformUtility.ScreenPointToLocalPointInRectangle"/> — correct, and required,
        /// for the ScreenSpaceOverlay canvas the game actually runs (docs/ui/UI_BOARD_ANCHORED_COMPONENTS.md).
        /// This harness has to flip that canvas to ScreenSpaceCamera to read it back into a texture at
        /// all, and under that mode the same call needs the canvas camera, so the shipped math lands the
        /// cluster off-frame *in the capture only*. Re-running the identical projection with the camera
        /// argument puts it where the real Overlay path already puts it in play. The prompt's actual
        /// show/hide/label behaviour is covered by <c>ProgramHudPlayModeTests</c>, not by this.
        /// </summary>
        private void ReanchorPromptForCameraSpaceCapture(Camera cam)
        {
            RectTransform promptRect = FindByName<RectTransform>("BombPrompt");
            BreachPoint pending = AttackerInput.PendingBreachPoint;
            // The prompt's own parent (MatchChrome) is stretched exactly like the canvas root the
            // shipped code projects against, so it is the same coordinate space — and unlike a
            // find-by-name lookup it cannot pick up some other object called "Root".
            var canvasRoot = promptRect != null ? promptRect.parent as RectTransform : null;
            if (promptRect == null || canvasRoot == null || pending == null || !promptRect.gameObject.activeInHierarchy)
            {
                return;
            }

            // Screen pixels → canvas-local by hand rather than through
            // ScreenPointToLocalPointInRectangle: against a RenderTexture-backed ScreenSpaceCamera
            // canvas that helper needs the canvas camera, and even given it, an orthographic rig one
            // unit from the canvas plane makes its ray-plane math blow up (measured: ~2e5 units off).
            // The parent is a full-stretch, centre-pivot rect, so the mapping is exact arithmetic.
            PlanarPosition mid = PlanarPosition.Lerp(pending.Segment.A, pending.Segment.B, 0.5f);
            Vector3 screenPoint = cam.WorldToScreenPoint(BoardVisual.WorldFromPlanar(mid));
            var local = new Vector2(
                (screenPoint.x / cam.pixelWidth - 0.5f) * canvasRoot.rect.width,
                (screenPoint.y / cam.pixelHeight - 0.5f) * canvasRoot.rect.height);
            promptRect.anchoredPosition = local + new Vector2(18f, 0f);
        }

        private IEnumerator Capture(Camera cam, RenderTexture rt, string dir, string name)
        {
            // Two frames + an explicit canvas update, and deliberately NOT WaitForEndOfFrame — that
            // yield instruction never resumes under -batchmode (see ShellChromeScreenshotTests).
            yield return null;
            Canvas.ForceUpdateCanvases();
            yield return null;

            // Re-project the board-anchored prompt against the settled canvas rect (the same entry
            // point GameBootstrap uses after a camera rotation), then lay out once more.
            Hud.RefreshBoardAnchoredUI();
            ReanchorPromptForCameraSpaceCapture(cam);
            Canvas.ForceUpdateCanvases();

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
