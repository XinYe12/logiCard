using System.Collections;
using LogiCard.Sim;
using UnityEngine;

namespace LogiCard.Board
{
    /// <summary>
    /// Contained stormy sky pocket above the floating board chunk (C53 / ART_DIRECTION Moodboard).
    ///
    /// Not a skybox and not an infinite horizon — camera clear flags stay SolidColor (dark void).
    /// Clouds, rain, and lightning are real scene geometry sized to the board footprint so weather
    /// reads as sitting on the diorama, matching the locked reference's "sky pocket over the chunk."
    ///
    /// 2026-08-11 re-source: clouds and rain now instantiate/configure prefabs from the free
    /// "Cinematic Weather VFX Bundle" (<c>Assets/RainSnowCloudEffect</c>) and lightning uses "Zap VFX –
    /// URP" (<c>Assets/Vefects</c>), replacing the earlier hand-rolled procedural
    /// <see cref="ParticleSystem"/> construction (a hand-authored cloud sprite atlas assembled into
    /// billboards) — see git history on this file pre-2026-08-11 if that construction is ever needed
    /// again. <see cref="LogiCard.Art.Editor.WeatherPackImportTool"/> copies the three source prefabs
    /// this class needs into <c>Resources/Weather</c> so they can be <see cref="Resources.Load"/>ed
    /// here; the source pack folders themselves are read-only reference and are never mutated (see
    /// <c>docs/ART_PACK_RESEARCH.md</c>).
    ///
    /// Materials on the new pack's own prefabs (<c>M_CloudLayer_URP.mat</c>, <c>M_Rain_URP.mat</c>)
    /// were checked directly against their serialized properties (not assumed) before this rewrite —
    /// both already ship correct URP transparency (<c>_Surface: 1</c> / Transparent,
    /// <c>_SURFACE_TYPE_TRANSPARENT</c> valid, correct src/dst blend, <c>_ZWrite: 0</c>) so no material
    /// patch-up step was needed, unlike <c>InteriorPackImportTool.FixGlassTransparency</c>. If a
    /// transparent particle material is ever hand-authored again in this project, see this file's
    /// pre-2026-08-11 history for two real, human-caught bugs that cost real time: a
    /// <c>new Material(shader)</c> defaults to Opaque, and <c>_ALPHABLEND_ON</c> is not the correct
    /// transparency keyword for URP particle shaders — <c>_SURFACE_TYPE_TRANSPARENT</c> is.
    /// </summary>
    public sealed class BoardWeatherPocket : MonoBehaviour
    {
        private static GameObject _cloudLayerPrefab;
        private static GameObject _rainSystemPrefab;
        private static GameObject _fogGroundPrefab;
        private static GameObject _rainMistPrefab;
        private static GameObject _lightningPrefab;

        private const string CloudLayerResourcePath = "Weather/PF_CloudLayer";
        private const string RainSystemResourcePath = "Weather/PF_RainSystem";
        private const string FogGroundResourcePath = "Weather/PF_Fog_Ground";
        private const string RainMistResourcePath = "Weather/PF_RainMist";
        private const string LightningResourcePath = "Weather/VFX_Zap_White";

        /// <summary>
        /// Build (or rebuild) the weather pocket over <paramref name="board"/>. Safe to call once from
        /// bootstrap after the board exists; destroys prior children first.
        /// </summary>
        public void Build(BoardView board)
        {
            for (int i = transform.childCount - 1; i >= 0; i--)
            {
                DestroyImmediate(transform.GetChild(i).gameObject);
            }

            if (board == null || board.Model == null)
            {
                return;
            }

            ArenaBoard model = board.Model;
            float width = (model.MaxX - model.MinX) * board.WorldScale;
            float depth = (model.MaxY - model.MinY) * board.WorldScale;
            Vector3 center = board.CenterWorld;

            transform.position = center;

            PlaceCloudBank(width, depth);
            PlaceRain(width, depth);
            PlaceFogMist(width, depth);
            PlaceLightning(width, depth);
        }

        /// <summary>
        /// Cloud scale/height correction, tuned 2026-08-09 and carried forward unchanged through the
        /// billboard-particle rework (2026-08-10) and this prefab re-source (2026-08-11). The sizing
        /// was tuned after the HUD dock's right-edge -> bottom-band move changed the camera's effective
        /// zoom/aspect (orthographicSize 9.0 -> 5.0 plus a much wider board-region aspect); a ~9-10
        /// unit-wide puff sitting only ~3 units above a 10-unit-deep board, viewed through a camera
        /// that's now effectively much closer, loomed over the entire board (confirmed via a human
        /// screenshot — playtest 2026-08-10). Shrunk ~45% and pushed further back/up. This correction
        /// is orthogonal to the rendering technique (primitive spheres, then textured particle
        /// billboards, now instantiated pack prefabs) — all three consume the same puff positions/
        /// footprints below, so keeping these factors is what keeps the "contained pocket, not looming
        /// over the board" framing correct (`docs/ART_DIRECTION.md` Moodboard) regardless of what draws
        /// inside each puff volume.
        /// </summary>
        private const float InterimCloudScale = 0.65f;
        private const float InterimCloudHeightBoost = 2.1f;

        /// <summary>
        /// <c>PF_CloudLayer.prefab</c>'s own authored <c>ShapeModule</c> box is 180x8x180 and its
        /// <c>InitialModule.startSize</c> spans 25-55 — sized for a large open-world layer, not an
        /// 8x9-13 unit board (this project's actual <c>ArenaBoard</c> dimensions, see
        /// <c>GameBootstrap.cs</c>). Every instantiated copy below rescales both the shape and the
        /// start size by the same board-fit ratio so particles stay proportioned to the board
        /// regardless of the pack's absolute authored scale — the exact failure mode
        /// <see cref="InterimCloudScale"/>'s doc comment already describes once for the old puffs.
        /// </summary>
        private const float CloudLayerAuthoredWidth = 180f;
        private const float CloudLayerAuthoredHeight = 8f;
        private const float CloudLayerAuthoredSizeMin = 25f;
        private const float CloudLayerAuthoredSizeMax = 55f;

        private void PlaceCloudBank(float width, float depth)
        {
            GameObject prefab = LoadPrefab(ref _cloudLayerPrefab, CloudLayerResourcePath);
            if (prefab == null)
            {
                return;
            }

            var root = new GameObject("CloudBank");
            root.transform.SetParent(transform, false);

            // Wide, thin ceiling shelf directly over the board — the dominant cloud mass the reference
            // sits right under. Reuses the exact placement the old sphere/billboard "Ceiling" puff used.
            PlaceCloudLayerInstance(root.transform, prefab, "Ceiling",
                new Vector3(0f, 3.6f * InterimCloudHeightBoost, 0f),
                width * 1.15f * InterimCloudScale, depth * 1.1f * InterimCloudScale);

            // Two offset puffs break the flat shelf into readable volume — a narrower version of the
            // old 7-puff arrangement (NW/SE spread only) since a single prefab type, reused at a couple
            // of positions/sizes, already reads as a volumetric mass rather than one flat slab; see
            // report for why 7 wasn't replicated 1:1.
            PlaceCloudLayerInstance(root.transform, prefab, "Puff_NW",
                new Vector3(-width * 0.28f, 3.15f * InterimCloudHeightBoost, depth * 0.22f),
                width * 0.62f * InterimCloudScale, depth * 0.5f * InterimCloudScale);

            PlaceCloudLayerInstance(root.transform, prefab, "Puff_SE",
                new Vector3(width * 0.30f, 3.3f * InterimCloudHeightBoost, -depth * 0.2f),
                width * 0.55f * InterimCloudScale, depth * 0.48f * InterimCloudScale);
        }

        private static void PlaceCloudLayerInstance(
            Transform parent,
            GameObject prefab,
            string name,
            Vector3 localPosition,
            float targetWidth,
            float targetDepth)
        {
            var instance = Instantiate(prefab, parent);
            instance.name = name;
            instance.transform.localPosition = localPosition;
            instance.transform.localRotation = Quaternion.identity;
            instance.transform.localScale = Vector3.one;

            var ps = instance.GetComponent<ParticleSystem>();
            if (ps == null)
            {
                return;
            }

            ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

            float sizeScale = targetWidth / CloudLayerAuthoredWidth;

            var shape = ps.shape;
            shape.scale = new Vector3(
                targetWidth,
                CloudLayerAuthoredHeight * sizeScale,
                targetDepth);

            var main = ps.main;
            // Fully populated at scene start rather than building up from empty over the authored
            // ~30s duration.
            main.prewarm = true;
            main.startSize = new ParticleSystem.MinMaxCurve(
                CloudLayerAuthoredSizeMin * sizeScale,
                CloudLayerAuthoredSizeMax * sizeScale);

            ps.Play(true);
        }

        private void PlaceRain(float width, float depth)
        {
            GameObject prefab = LoadPrefab(ref _rainSystemPrefab, RainSystemResourcePath);
            if (prefab == null)
            {
                return;
            }

            var instance = Instantiate(prefab, transform);
            instance.name = "Rain";
            // Emit just under the cloud shelf so streaks read as falling out of the pocket. Same height
            // this file's rain has used since the 2026-08-09/10 framing fix (see
            // InterimCloudHeightBoost above) — unrelated to the pack's own authored spawn height
            // (PF_RainSystem.prefab spawns at y=15, tuned for its much larger open-world box).
            instance.transform.localPosition = new Vector3(0f, 2.85f * InterimCloudHeightBoost, 0f);
            instance.transform.localRotation = Quaternion.identity;
            instance.transform.localScale = Vector3.one;

            var ps = instance.GetComponent<ParticleSystem>();
            if (ps == null)
            {
                return;
            }

            ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

            var shape = ps.shape;
            // Pack authored a Cone (type 5, angle 25, emits local +Z). Prior sessions only overrode
            // .scale + velocityOverLifetime, which left startSpeed still firing mostly sideways —
            // Stretch then drew short bright horizontal dashes that read as blue spark/dots from the
            // board camera (screenshot 13). Force the old procedural rain's Box so emission is along
            // shape +Y, then flip the shape so that axis is world -Y (down).
            shape.shapeType = ParticleSystemShapeType.Box;
            shape.scale = new Vector3(width * 0.95f, 0.15f, depth * 0.95f);
            shape.position = Vector3.zero;
            shape.rotation = new Vector3(180f, 0f, 0f);

            var emission = ps.emission;
            // Fixed, not board-scaled: this exact rate is what the *previous* procedural rain used, and
            // the human already confirmed that density "already reads fine" — keep that bar rather than
            // re-deriving a rate from the pack's own (much larger, open-world) authored density.
            emission.rateOverTime = 700f;

            var main = ps.main;
            main.prewarm = true;
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            main.maxParticles = 2500;
            // The pack's own authored startSpeed (25-40) and startLifetime (1.2-2s) were left at their
            // open-world values in the first pass of this re-source — at those numbers a particle travels
            // 30-80 world units before dying, several times the board's own 8-13 unit footprint, so every
            // streak (Stretch render mode) rendered far longer than the visible frame and read as harsh
            // "laser" lines crossing the whole scene (caught via a human screenshot, 2026-08-11). Same
            // proven-good numbers the old procedural rain used before this re-source, not a fresh guess.
            // startSpeed now feeds Stretch length *and* falls down (Box + 180° rotation above).
            main.startSpeed = new ParticleSystem.MinMaxCurve(5.5f, 7.5f);
            main.startLifetime = new ParticleSystem.MinMaxCurve(0.55f, 0.85f);
            // Pack left size3D on with a near-dot width (0.03) — Stretch needs the old thin-but-readable
            // drop proportions or streaks collapse to spark points once direction is fixed.
            main.startSize3D = true;
            main.startSizeX = new ParticleSystem.MinMaxCurve(0.012f, 0.02f);
            main.startSizeY = new ParticleSystem.MinMaxCurve(0.18f, 0.32f);
            main.startSizeZ = new ParticleSystem.MinMaxCurve(0.012f, 0.02f);

            // Stretch-billboard renderer length is size*lengthScale + speed*velocityScale, independent
            // of startSpeed/startLifetime above — the pack's authored lengthScale (3.5) still rendered
            // streaks ~4 world units long even after the speed/lifetime fix (confirmed via a second human
            // screenshot, 2026-08-11: still reading as long harsh lines, not soft rain). Same proven-good
            // renderer values the old procedural rain used.
            var psRenderer = instance.GetComponent<ParticleSystemRenderer>();
            if (psRenderer != null)
            {
                psRenderer.renderMode = ParticleSystemRenderMode.Stretch;
                psRenderer.lengthScale = 1.8f;
                psRenderer.velocityScale = 0.06f;
                psRenderer.sharedMaterial = SoftRainMaterial(psRenderer.sharedMaterial);
            }

            // Pack's authored startColor (alpha 0.7-0.9) reads far more opaque/saturated than the old
            // procedural rain's tint (alpha 0.42) — on top of the long-streak issue, this was the other
            // half of why it read as harsh "laser" lines rather than soft rain. Same tint the old
            // procedural rain used.
            main.startColor = new Color(0.72f, 0.78f, 0.88f, 0.42f);
            main.gravityModifier = 0.35f;

            // Soft edge fade — pack has none; without it opaque birth/death reads as popping sparks.
            var colorOverLifetime = ps.colorOverLifetime;
            colorOverLifetime.enabled = true;
            var gradient = new Gradient();
            gradient.SetKeys(
                new[]
                {
                    new GradientColorKey(Color.white, 0f),
                    new GradientColorKey(Color.white, 1f),
                },
                new[]
                {
                    new GradientAlphaKey(0f, 0f),
                    new GradientAlphaKey(0.55f, 0.12f),
                    new GradientAlphaKey(0.45f, 0.75f),
                    new GradientAlphaKey(0f, 1f),
                });
            colorOverLifetime.color = gradient;

            // Extra world-space downward drift + light wind, same as the old procedural rain. With the
            // Box/downward startSpeed above this reinforces fall rather than fighting a Cone +Z spray.
            // Every axis must share the same MinMaxCurve mode (constant vs curve) — Unity errors
            // otherwise ("Particle Velocity curves must all be in the same mode") and PlayMode tests
            // treat that Error log as a failure.
            var velocity = ps.velocityOverLifetime;
            velocity.enabled = true;
            velocity.space = ParticleSystemSimulationSpace.World;
            velocity.x = new ParticleSystem.MinMaxCurve(-0.6f, -0.2f);
            velocity.y = new ParticleSystem.MinMaxCurve(-2.5f, -1.5f);
            velocity.z = new ParticleSystem.MinMaxCurve(0f, 0f);

            ps.Play(true);
        }

        private static Material _softRainMaterial;

        /// <summary>
        /// Runtime instance of the pack rain material: keep the streak texture, drop soft-particle
        /// depth fade (authored FarFadeDistance 1 eats drops near the board → sparse surviving
        /// sparks), and soften the bright blue base tint. Never mutates the shared pack/Resources asset.
        /// </summary>
        private static Material SoftRainMaterial(Material source)
        {
            if (_softRainMaterial != null)
            {
                return _softRainMaterial;
            }

            if (source != null)
            {
                _softRainMaterial = new Material(source);
            }
            else
            {
                var particles = Shader.Find("Universal Render Pipeline/Particles/Unlit")
                    ?? Shader.Find("Particles/Standard Unlit")
                    ?? Shader.Find("Sprites/Default");
                _softRainMaterial = new Material(particles);
            }

            var rainTint = new Color(0.75f, 0.82f, 0.92f, 0.55f);
            _softRainMaterial.color = rainTint;
            if (_softRainMaterial.HasProperty("_BaseColor"))
            {
                _softRainMaterial.SetColor("_BaseColor", rainTint);
            }

            if (_softRainMaterial.HasProperty("_SoftParticlesEnabled"))
            {
                _softRainMaterial.SetFloat("_SoftParticlesEnabled", 0f);
            }

            if (_softRainMaterial.HasProperty("_CameraFadingEnabled"))
            {
                _softRainMaterial.SetFloat("_CameraFadingEnabled", 0f);
            }

            _softRainMaterial.DisableKeyword("_SOFTPARTICLES_ON");
            _softRainMaterial.DisableKeyword("_FADING_ON");

            return _softRainMaterial;
        }

        /// <summary>
        /// Soft ground haze + rain mist — ART_PACK_RESEARCH Lighting+Ground use-now #2. Owned weather
        /// pack prefabs that were never wired (only rain/clouds/Zap landed). Sized to the board so
        /// they read as desk-lamp atmosphere, not an open-world fog wall.
        /// </summary>
        private void PlaceFogMist(float width, float depth)
        {
            // Playtest 2026-08-12: first wire was invisible at play distance — denser/warmer/larger.
            PlaceBoardScaledFog(
                ref _fogGroundPrefab,
                FogGroundResourcePath,
                "FogGround",
                localY: 0.45f,
                width * 1.15f,
                depth * 1.15f,
                rateOverTime: 55f,
                startSize: new ParticleSystem.MinMaxCurve(2.8f, 4.5f),
                startColor: new Color(0.72f, 0.66f, 0.55f, 0.42f));

            PlaceBoardScaledFog(
                ref _rainMistPrefab,
                RainMistResourcePath,
                "RainMist",
                localY: 0.85f,
                width * 1.05f,
                depth * 1.05f,
                rateOverTime: 70f,
                startSize: new ParticleSystem.MinMaxCurve(1.6f, 2.8f),
                startColor: new Color(0.78f, 0.82f, 0.88f, 0.35f));
        }

        private void PlaceBoardScaledFog(
            ref GameObject prefabCache,
            string resourcePath,
            string name,
            float localY,
            float targetWidth,
            float targetDepth,
            float rateOverTime,
            ParticleSystem.MinMaxCurve startSize,
            Color startColor)
        {
            GameObject prefab = LoadPrefab(ref prefabCache, resourcePath);
            if (prefab == null)
            {
                return;
            }

            var instance = Instantiate(prefab, transform);
            instance.name = name;
            instance.transform.localPosition = new Vector3(0f, localY, 0f);
            instance.transform.localRotation = Quaternion.identity;
            instance.transform.localScale = Vector3.one;

            var systems = instance.GetComponentsInChildren<ParticleSystem>(true);
            for (int i = 0; i < systems.Length; i++)
            {
                ParticleSystem ps = systems[i];
                ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

                var shape = ps.shape;
                if (shape.enabled)
                {
                    shape.shapeType = ParticleSystemShapeType.Box;
                    shape.scale = new Vector3(targetWidth, 0.4f, targetDepth);
                    shape.position = Vector3.zero;
                    shape.rotation = Vector3.zero;
                }

                var emission = ps.emission;
                if (emission.enabled)
                {
                    emission.rateOverTime = rateOverTime;
                }

                var main = ps.main;
                main.prewarm = true;
                main.simulationSpace = ParticleSystemSimulationSpace.Local;
                main.startSize = startSize;
                main.startColor = startColor;
                main.startLifetime = new ParticleSystem.MinMaxCurve(4.5f, 7.5f);
                main.maxParticles = Mathf.Min(main.maxParticles, 400);

                var psRenderer = ps.GetComponent<ParticleSystemRenderer>();
                if (psRenderer != null && psRenderer.sharedMaterial != null)
                {
                    // Soften shared pack mats without mutating Resources — clone once per instance.
                    psRenderer.material = new Material(psRenderer.sharedMaterial);
                    Color tint = startColor;
                    psRenderer.material.color = tint;
                    if (psRenderer.material.HasProperty("_BaseColor"))
                    {
                        psRenderer.material.SetColor("_BaseColor", tint);
                    }
                }

                ps.Play(true);
            }
        }

        /// <summary>Randomized interval between flashes — modest and occasional, not constant storm
        /// strobing, matching this project's "don't be tedious" presentation bar (see
        /// `docs/DRAFT_HANDOFF.md`).</summary>
        private const float LightningIntervalMinSeconds = 12f;
        private const float LightningIntervalMaxSeconds = 22f;

        private void PlaceLightning(float width, float depth)
        {
            GameObject prefab = LoadPrefab(ref _lightningPrefab, LightningResourcePath);
            if (prefab == null)
            {
                return;
            }

            var instance = Instantiate(prefab, transform);
            instance.name = "Lightning";
            // Ground strike — playtest 2026-08-11 (screenshot 14): prior y = 3.2 * cloud boost (~6.7)
            // floated the Zap impact mid-air. Anchor at board floor with a slight off-center XZ so
            // flashes don't metronome dead-center; bolt reads as hitting dirt, not hanging in void.
            instance.transform.localPosition = new Vector3(width * 0.18f, 0.05f, -depth * 0.12f);
            instance.transform.localRotation = Quaternion.identity;
            instance.transform.localScale = Vector3.one;

            var ps = instance.GetComponent<ParticleSystem>();
            if (ps == null)
            {
                return;
            }

            // Cancel the prefab's own playOnAwake burst — the whole point is a *scheduled*, occasional
            // flash, not one immediately when the board loads.
            ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

            StartCoroutine(LightningLoop(ps));
        }

        /// <summary>
        /// Re-triggers the (one-shot, non-looping) Zap rig on a randomized interval. Calling
        /// <see cref="ParticleSystem.Play"/> with children replays its whole authored hierarchy —
        /// including the point light its own "Light Spawn" sub-system enables for the flash (starts
        /// disabled in the prefab; the pack's own particle wiring turns it on/off) — so this loop never
        /// needs to hand-manage that light itself.
        /// </summary>
        private static IEnumerator LightningLoop(ParticleSystem rig)
        {
            while (rig != null)
            {
                yield return new WaitForSeconds(Random.Range(LightningIntervalMinSeconds, LightningIntervalMaxSeconds));
                if (rig == null)
                {
                    yield break;
                }

                rig.Play(true);
            }
        }

        private static GameObject LoadPrefab(ref GameObject cache, string resourcePath)
        {
            if (cache != null)
            {
                return cache;
            }

            cache = Resources.Load<GameObject>(resourcePath);
            if (cache == null)
            {
                Debug.LogWarning(
                    $"BoardWeatherPocket: missing Resources/{resourcePath}.prefab — run " +
                    "Tools > LogiCard > Import Weather Pack Prefabs.");
            }

            return cache;
        }
    }
}
