using LogiCard.Board;
using UnityEngine;
using UnityEngine.Rendering;

namespace LogiCard.UI
{
    /// <summary>
    /// A live 3D preview of one archetype's <em>actual match model</em>, rendered off-screen into a
    /// <see cref="RenderTexture"/> so a uGUI <see cref="UnityEngine.UI.RawImage"/> can show it inside a
    /// Screen-Space card face (a mesh cannot be parented under a <see cref="RectTransform"/>).
    ///
    /// The figure comes from <see cref="PawnView.TryInstantiateArchetypeVisual"/> — the same
    /// <c>Resources</c> prefab, the same height normalisation, the same team tint a match spawns. That is
    /// deliberate and load-bearing: Character Select must show the player the thing they will actually
    /// get on the board, the same "live state, never a stand-in inferred from the UI" discipline
    /// <c>docs/ui/UI_BOARD_ANCHORED_COMPONENTS.md</c> holds board-anchored controls to. If this ever
    /// drifts to a bespoke preview asset, the screen starts lying.
    ///
    /// Isolation is belt-and-braces, because this rig is alive in the same scene as a real match board:
    /// 1. it lives at <see cref="RigOrigin"/>, thousands of units from the arena, one island per rig;
    /// 2. everything it owns sits on the <see cref="LayerName"/> layer, which the preview camera is the
    ///    only camera to include (<c>GameBootstrap.ConfigureCamera</c> masks it out of the main camera);
    /// 3. the camera's far clip is a couple of metres, so even a mis-set mask cannot reach the board.
    ///
    /// Cost: one small camera per rig, rendering live while Character Select is open. That is fine for a
    /// menu — and <see cref="SetShowing"/> switches the whole rig off the moment the screen hides, which
    /// also keeps its private lights out of <c>GameBootstrap.BuildLighting</c>'s "scene already lit?"
    /// probe (that probe ignores inactive objects, and ignores this layer besides).
    /// </summary>
    public sealed class CharacterPreviewRig : MonoBehaviour
    {
        /// <summary>Layer (ProjectSettings/TagManager.asset, index 8) only the preview camera renders.</summary>
        public const string LayerName = "CharacterPreview";

        // Rendered at roughly 2x the on-screen size of the card's emblem well and downsampled by the
        // RawImage's bilinear filter — supersampling instead of RenderTexture MSAA, which URP will
        // silently renegotiate against the pipeline asset's own MSAA setting.
        private const int TextureWidth = 768;
        private const int TextureHeight = 896;

        /// <summary>Far from the arena, and far from anything a stray camera frustum reaches.</summary>
        private static readonly Vector3 RigOrigin = new Vector3(0f, -4000f, 0f);

        /// <summary>Spacing between per-archetype islands. Well beyond <see cref="FarClip"/>.</summary>
        private const float RigSpacing = 40f;

        /// <summary>
        /// Islands are handed out round-robin rather than keyed off the archetype, so a rig built while
        /// a previous (already-destroyed-this-frame) one still exists can never share a spot and render
        /// two overlapping figures. Wrapped so a long session's counter can't drift into float-precision
        /// territory.
        /// </summary>
        private const int RigSlots = 64;

        private const float FieldOfView = 28f;
        private const float NearClip = 0.05f;
        private const float FarClip = 12f;

        /// <summary>
        /// Vertical world extent the camera frames — <see cref="PawnView.TargetVisualHeight"/> (1.0)
        /// plus head/foot air. Tuned by looking: 1.24 left the figure reading small and adrift inside
        /// the card well, 1.14 fills it without cropping the feet during the idle sway.
        /// </summary>
        private const float FramedHeight = 1.14f;

        /// <summary>Aim point up the figure. Slightly above the waist reads as a portrait, not a tabletop shot.</summary>
        private const float AimHeight = 0.54f;

        /// <summary>Downward tilt, degrees. Small — a steep angle foreshortens the figure into a blob.</summary>
        private const float CameraPitch = 6f;

        /// <summary>Base three-quarter turn away from dead-on, degrees.</summary>
        private const float BaseYaw = -22f;

        /// <summary>Slow idle sway around <see cref="BaseYaw"/> — proves the preview is live 3D.</summary>
        private const float SwayDegrees = 13f;
        private const float SwaySpeed = 0.55f;

        private static int _rigCount;

        private Transform _model;
        private Camera _camera;
        private RenderTexture _texture;
        private float _phase;

        /// <summary>The live texture to hand a <c>RawImage</c>. Null when the model failed to load.</summary>
        public RenderTexture Texture => _texture;

        /// <summary>False when this archetype has no imported mesh yet — callers keep their fallback art.</summary>
        public bool HasModel => _model != null;

        /// <summary>Layer bit mask for <see cref="Camera.cullingMask"/> arithmetic; 0 if the layer is missing.</summary>
        public static int LayerBit
        {
            get
            {
                int layer = LayerMask.NameToLayer(LayerName);
                return layer < 0 ? 0 : 1 << layer;
            }
        }

        /// <summary>
        /// Builds an isolated preview island for <paramref name="build"/>, starting switched off.
        /// Returns null if the archetype's mesh isn't in <c>Resources</c>, or if there is no graphics
        /// device to render into — never a half-built rig, so a caller's "did this work?" check is a
        /// single null test and its fallback art covers both cases.
        /// </summary>
        public static CharacterPreviewRig Create(PawnBuild build, Color teamTint)
        {
            // No graphics device (`-batchmode -nographics`, the project's standard test invocation):
            // RenderTexture.Create fails, and URP then logs a burst of "Unable to find surface for
            // attachment 0" errors that the test framework counts as unhandled failures. There is
            // nothing to look at on a headless seat anyway — decline to build, and Character Select
            // falls back to its monogram exactly as it did before this rig existed. The screenshot
            // harness deliberately runs WITHOUT -nographics, so it still exercises the real thing.
            if (SystemInfo.graphicsDeviceType == GraphicsDeviceType.Null)
            {
                return null;
            }

            var go = new GameObject($"CharacterPreview_{build}");
            var rig = go.AddComponent<CharacterPreviewRig>();
            if (!rig.Build(build, teamTint))
            {
                DestroyNow(go);
                return null;
            }

            go.SetActive(false);
            return rig;
        }

        private bool Build(PawnBuild build, Color teamTint)
        {
            transform.position = RigOrigin + new Vector3((_rigCount++ % RigSlots) * RigSpacing, 0f, 0f);

            GameObject model = PawnView.TryInstantiateArchetypeVisual(transform, build, teamTint);
            if (model == null)
            {
                return false;
            }

            _model = model.transform;
            _model.localPosition = Vector3.zero;
            _model.localRotation = Quaternion.Euler(0f, BaseYaw, 0f);

            // An Animator with no controller leaves the mesh in its bind pose, and — critically — a
            // SkinnedMeshRenderer whose bones never move can end up frustum-culled against stale bounds.
            // The rig is stationary and tiny, so just render it unconditionally.
            foreach (SkinnedMeshRenderer skinned in model.GetComponentsInChildren<SkinnedMeshRenderer>())
            {
                skinned.updateWhenOffscreen = true;
            }

            ApplyLayer(gameObject);

            BuildCamera();
            BuildLights();
            return true;
        }

        private void BuildCamera()
        {
            _texture = new RenderTexture(TextureWidth, TextureHeight, 24, RenderTextureFormat.ARGB32)
            {
                name = $"{name}_RT",
                antiAliasing = 1,
                filterMode = FilterMode.Bilinear,
            };

            var camGo = new GameObject("PreviewCamera");
            camGo.transform.SetParent(transform, false);

            Vector3 aim = transform.position + (Vector3.up * AimHeight);
            // Frame FramedHeight of world height at FieldOfView; the texture is portrait, so height is
            // always the binding dimension for a standing figure.
            float distance = (FramedHeight * 0.5f) / Mathf.Tan(FieldOfView * 0.5f * Mathf.Deg2Rad);
            Quaternion look = Quaternion.Euler(CameraPitch, 180f, 0f);
            camGo.transform.SetPositionAndRotation(aim - (look * Vector3.forward * distance), look);

            _camera = camGo.AddComponent<Camera>();
            _camera.fieldOfView = FieldOfView;
            _camera.nearClipPlane = NearClip;
            _camera.farClipPlane = FarClip;
            _camera.cullingMask = LayerBit != 0 ? LayerBit : _camera.cullingMask;
            // Transparent clear so the card's own emblem well (a soft radial pool) stays the backing —
            // an opaque clear would paste a hard rectangle onto the cardstock.
            _camera.clearFlags = CameraClearFlags.SolidColor;
            _camera.backgroundColor = new Color(0f, 0f, 0f, 0f);
            _camera.allowHDR = false;
            _camera.allowMSAA = false;
            _camera.useOcclusionCulling = false;
            _camera.targetTexture = _texture;
            // A menu preview must never steal the match's listener or its depth ordering.
            _camera.depth = -50f;
            ApplyLayer(camGo);
        }

        /// <summary>
        /// Private three-point-ish lighting. The rig can be on screen before <c>GameBootstrap</c> has
        /// built any match lights at all (Character Select happens before the board exists), so the
        /// preview cannot borrow scene lighting — without these the figure renders as a black cutout.
        /// Warm key from the camera's right, cool-ish fill from its left, dim rim from behind to keep the
        /// silhouette off the dark card well.
        /// </summary>
        private void BuildLights()
        {
            AddLight("PreviewKey", Quaternion.Euler(28f, 200f, 0f), new Color(1f, 0.95f, 0.86f), 1.5f);
            AddLight("PreviewFill", Quaternion.Euler(12f, 145f, 0f), new Color(0.78f, 0.84f, 1f), 0.75f);
            AddLight("PreviewRim", Quaternion.Euler(-16f, 15f, 0f), new Color(1f, 0.82f, 0.6f), 0.65f);
        }

        private void AddLight(string lightName, Quaternion rotation, Color color, float intensity)
        {
            var go = new GameObject(lightName);
            go.transform.SetParent(transform, false);
            go.transform.rotation = rotation;
            var light = go.AddComponent<Light>();
            light.type = LightType.Directional;
            light.color = color;
            light.intensity = intensity;
            light.shadows = LightShadows.None;
            // URP honours this inconsistently across versions; the rig being switched off whenever
            // Character Select is hidden is the guarantee that actually holds.
            light.cullingMask = LayerBit != 0 ? LayerBit : light.cullingMask;
            ApplyLayer(go);
        }

        private static void ApplyLayer(GameObject root)
        {
            int layer = LayerMask.NameToLayer(LayerName);
            if (layer < 0)
            {
                // Layer missing from TagManager: the rig still renders (it is parked thousands of units
                // from the arena, inside a 12-unit far clip), it just loses camera-level isolation.
                Debug.LogWarning($"[logiCard] Layer '{LayerName}' is not defined — character preview isolation is degraded.");
                return;
            }

            foreach (Transform child in root.GetComponentsInChildren<Transform>(true))
            {
                child.gameObject.layer = layer;
            }
        }

        /// <summary>Screen lifecycle hook: live only while Character Select is on screen.</summary>
        public void SetShowing(bool showing)
        {
            if (this == null)
            {
                return;
            }

            gameObject.SetActive(showing);
        }

        private void Update()
        {
            if (_model == null)
            {
                return;
            }

            _phase += Time.unscaledDeltaTime * SwaySpeed;
            _model.localRotation = Quaternion.Euler(0f, BaseYaw + (Mathf.Sin(_phase) * SwayDegrees), 0f);
        }

        private void OnDestroy()
        {
            if (_camera != null)
            {
                _camera.targetTexture = null;
            }

            if (_texture != null)
            {
                _texture.Release();
                DestroyNow(_texture);
                _texture = null;
            }
        }

        /// <summary>Destroy that also works from edit-mode/teardown paths where <c>Destroy</c> is illegal.</summary>
        private static void DestroyNow(Object target)
        {
            if (target == null)
            {
                return;
            }

            if (Application.isPlaying)
            {
                Destroy(target);
            }
            else
            {
                DestroyImmediate(target);
            }
        }

        /// <summary>Tears the rig's GameObject down; safe to call on a null/destroyed reference.</summary>
        public static void Dispose(CharacterPreviewRig rig)
        {
            if (rig == null)
            {
                return;
            }

            DestroyNow(rig.gameObject);
        }
    }
}
