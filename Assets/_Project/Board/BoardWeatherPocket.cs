using System.Collections;
using LogiCard.Sim;
using UnityEngine;
using UnityEngine.Rendering;

namespace LogiCard.Board
{
    /// <summary>
    /// Contained stormy sky pocket above the floating board chunk (C53 / ART_DIRECTION Moodboard).
    ///
    /// Not a skybox and not an infinite horizon — camera clear flags stay SolidColor (dark void).
    /// Clouds, rain, rim mist, and lightning are real scene geometry sized to the board footprint so
    /// weather reads as sitting on the diorama, matching the locked reference's "sky pocket over the chunk."
    ///
    /// 2026-08-12 atmosphere stylized pass: cloud bank restored to CloudAtlas Kenney billboards
    /// (<c>c0c4f39</c> <see cref="PlaceCloudPuff"/> / <see cref="CloudMaterial"/>) — pack
    /// <c>PF_CloudLayer</c> demoted (open-world scale, wrong toy read). Soft atmosphere is rim-only
    /// Kenney mist + optional pack <c>PF_Fog_Main</c>/<c>PF_Fog_Distant</c> at the board edge so the
    /// playable center stays readable (no full-board white-out, no CreatePrimitive spheres).
    /// Rain stays pack <c>PF_RainSystem</c>; lightning stays Zap <c>VFX_Zap_White</c>.
    /// Prefab copies land via <see cref="LogiCard.Art.Editor.WeatherPackImportTool"/>.
    /// </summary>
    public sealed class BoardWeatherPocket : MonoBehaviour
    {
        private static Material _cloudMaterial;
        private static Material _mistMaterial;
        private static GameObject _rainSystemPrefab;
        private static GameObject _fogMainPrefab;
        private static GameObject _fogDistantPrefab;
        private static GameObject _lightningPrefab;

        private const string RainSystemResourcePath = "Weather/PF_RainSystem";
        private const string FogMainResourcePath = "Weather/PF_Fog_Main";
        private const string FogDistantResourcePath = "Weather/PF_Fog_Distant";
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
        /// Cloud scale/height correction, tuned 2026-08-09 and carried forward through the
        /// billboard-particle rework (2026-08-10) and pack rain/Zap wiring. Shrunk ~45% and pushed
        /// further back/up after the HUD dock move (orthographicSize 9.0 -> 5.0). Orthogonal to the
        /// rendering technique — puff positions/footprints below consume these factors so the
        /// "contained pocket, not looming over the board" framing stays correct.
        /// </summary>
        private const float InterimCloudScale = 0.65f;
        private const float InterimCloudHeightBoost = 2.1f;

        /// <summary>
        /// Cloud sprite atlas: 4x2 grid of Kenney "Smoke Particles" (CC0) puff silhouettes, composed
        /// into one texture so <see cref="ParticleSystem.TextureSheetAnimationModule"/> can hand each
        /// particle a random frame — see <c>Assets/_Project/Art/Environment/THIRD_PARTY.md</c>.
        /// </summary>
        private const int CloudAtlasColumns = 4;
        private const int CloudAtlasRows = 2;

        /// <summary>Particle count scales with each puff's footprint (world-unit width * depth), clamped
        /// so tiny fringe puffs still read as a cluster and the wide ceiling shelf doesn't spawn a huge
        /// batch of overlapping billboards.</summary>
        private const float CloudParticleDensity = 1.0f;
        private const int CloudParticlesMin = 8;
        private const int CloudParticlesMax = 22;

        /// <summary>Not a real "infinite" lifetime (Unity has none) — long enough that a burst-spawned,
        /// non-moving puff cluster never visibly expires during a play session.</summary>
        private const float CloudParticleLifetimeSeconds = 9999f;

        private void PlaceCloudBank(float width, float depth)
        {
            var root = new GameObject("CloudBank");
            root.transform.SetParent(transform, false);

            // Dense ceiling layer — the solid cloud shelf the reference sits right above the chunk.
            PlaceCloudPuff(root.transform, "Ceiling", new Vector3(0f, 3.6f * InterimCloudHeightBoost, 0f),
                new Vector3(width * 1.15f, 0.85f, depth * 1.1f) * InterimCloudScale,
                new Color(0.22f, 0.24f, 0.30f, 1f));

            // Mid puffs — break the silhouette so it reads as volume, not one flat slab.
            PlaceCloudPuff(root.transform, "Puff_NW", new Vector3(-width * 0.28f, 3.15f * InterimCloudHeightBoost, depth * 0.22f),
                new Vector3(width * 0.55f, 0.7f, depth * 0.42f) * InterimCloudScale,
                new Color(0.28f, 0.30f, 0.36f, 1f));
            PlaceCloudPuff(root.transform, "Puff_SE", new Vector3(width * 0.30f, 3.25f * InterimCloudHeightBoost, -depth * 0.20f),
                new Vector3(width * 0.50f, 0.65f, depth * 0.45f) * InterimCloudScale,
                new Color(0.26f, 0.28f, 0.34f, 1f));
            PlaceCloudPuff(root.transform, "Puff_NE", new Vector3(width * 0.22f, 3.55f * InterimCloudHeightBoost, depth * 0.28f),
                new Vector3(width * 0.48f, 0.55f, depth * 0.38f) * InterimCloudScale,
                new Color(0.20f, 0.22f, 0.28f, 1f));
            PlaceCloudPuff(root.transform, "Puff_SW", new Vector3(-width * 0.18f, 3.05f * InterimCloudHeightBoost, -depth * 0.26f),
                new Vector3(width * 0.42f, 0.5f, depth * 0.36f) * InterimCloudScale,
                new Color(0.30f, 0.32f, 0.38f, 1f));
            PlaceCloudPuff(root.transform, "Puff_Center", new Vector3(0.1f * width, 3.4f * InterimCloudHeightBoost, 0.05f * depth),
                new Vector3(width * 0.62f, 0.75f, depth * 0.55f) * InterimCloudScale,
                new Color(0.24f, 0.26f, 0.32f, 1f));

            // Lower fringe — slightly warmer/lighter so the pocket catches a hint of ground bounce
            // (the reference's under-lit cloud bellies) without needing streetlamps yet.
            PlaceCloudPuff(root.transform, "Fringe_A", new Vector3(-width * 0.35f, 2.7f * InterimCloudHeightBoost, 0.05f * depth),
                new Vector3(width * 0.40f, 0.35f, depth * 0.30f) * InterimCloudScale,
                new Color(0.34f, 0.32f, 0.30f, 1f));
            PlaceCloudPuff(root.transform, "Fringe_B", new Vector3(width * 0.32f, 2.75f * InterimCloudHeightBoost, depth * 0.10f),
                new Vector3(width * 0.38f, 0.32f, depth * 0.28f) * InterimCloudScale,
                new Color(0.32f, 0.30f, 0.28f, 1f));
        }

        /// <summary>
        /// Builds one "puff" as a burst-spawned, non-moving cluster of textured billboard particles
        /// (CloudAtlas Kenney frames). Restored from <c>c0c4f39</c> — replaces pack
        /// <c>PF_CloudLayer</c> for the stylized toy read.
        /// </summary>
        private static void PlaceCloudPuff(
            Transform parent,
            string name,
            Vector3 localPosition,
            Vector3 localScale,
            Color tint)
        {
            var puff = new GameObject(name);
            puff.transform.SetParent(parent, false);
            puff.transform.localPosition = localPosition;

            var ps = puff.AddComponent<ParticleSystem>();
            ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

            float footprint = Mathf.Max(0.01f, localScale.x * localScale.z);
            int count = Mathf.Clamp(
                Mathf.RoundToInt(footprint * CloudParticleDensity),
                CloudParticlesMin,
                CloudParticlesMax);
            // Overlap factor > 1 so neighboring billboards blend into one mass rather than sitting as
            // visibly separate discs across the puff's footprint.
            float baseSize = Mathf.Sqrt(footprint / count) * 1.35f;

            var main = ps.main;
            main.loop = false;
            main.playOnAwake = true;
            main.duration = 1f;
            main.startLifetime = new ParticleSystem.MinMaxCurve(CloudParticleLifetimeSeconds);
            main.startSpeed = 0f;
            main.startSize = new ParticleSystem.MinMaxCurve(baseSize * 0.75f, baseSize * 1.35f);
            main.startRotation = new ParticleSystem.MinMaxCurve(0f, 360f * Mathf.Deg2Rad);
            main.startColor = new ParticleSystem.MinMaxGradient(
                new Color(tint.r * 0.85f, tint.g * 0.85f, tint.b * 0.85f, 0.85f),
                new Color(Mathf.Min(1f, tint.r * 1.15f), Mathf.Min(1f, tint.g * 1.15f), Mathf.Min(1f, tint.b * 1.15f), 0.98f));
            main.maxParticles = Mathf.Max(count, 4);
            main.simulationSpace = ParticleSystemSimulationSpace.Local;
            main.gravityModifier = 0f;
            main.scalingMode = ParticleSystemScalingMode.Local;

            var emission = ps.emission;
            emission.enabled = true;
            emission.rateOverTime = 0f;
            emission.SetBursts(new[] { new ParticleSystem.Burst(0f, (short)count) });

            var shape = ps.shape;
            shape.enabled = true;
            shape.shapeType = ParticleSystemShapeType.Box;
            // Explicit full-volume fill (default box "emit from" can otherwise be shell-only) — puffs
            // should scatter particles throughout the bounding box, not just its outer surface.
            shape.boxThickness = Vector3.one;
            shape.scale = new Vector3(
                Mathf.Max(localScale.x, 0.05f),
                Mathf.Max(localScale.y, 0.05f),
                Mathf.Max(localScale.z, 0.05f));
            shape.position = Vector3.zero;
            shape.rotation = Vector3.zero;

            // Each particle keeps a fixed random frame from the cloud atlas for its whole lifetime —
            // frameOverTime stays flat 0 so it never flipbooks; variety is per-particle at spawn.
            var tsa = ps.textureSheetAnimation;
            tsa.enabled = true;
            tsa.mode = ParticleSystemAnimationMode.Grid;
            tsa.numTilesX = CloudAtlasColumns;
            tsa.numTilesY = CloudAtlasRows;
            tsa.animation = ParticleSystemAnimationType.WholeSheet;
            tsa.frameOverTime = new ParticleSystem.MinMaxCurve(0f);
            tsa.startFrame = new ParticleSystem.MinMaxCurve(0f, CloudAtlasColumns * CloudAtlasRows - 1);
            tsa.cycleCount = 1;

            var renderer = puff.GetComponent<ParticleSystemRenderer>();
            renderer.renderMode = ParticleSystemRenderMode.Billboard;
            renderer.sortMode = ParticleSystemSortMode.Distance;
            renderer.sharedMaterial = CloudMaterial();
            renderer.shadowCastingMode = ShadowCastingMode.Off;
            renderer.receiveShadows = false;

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
            // this file's rain has used since the 2026-08-09/10 framing fix.
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
            // Pack authored a Cone (type 5, angle 25, emits local +Z). Force Box so emission is along
            // shape +Y, then flip the shape so that axis is world -Y (down).
            shape.shapeType = ParticleSystemShapeType.Box;
            shape.scale = new Vector3(width * 0.95f, 0.15f, depth * 0.95f);
            shape.position = Vector3.zero;
            shape.rotation = new Vector3(180f, 0f, 0f);

            var emission = ps.emission;
            emission.rateOverTime = 700f;

            var main = ps.main;
            main.prewarm = true;
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            main.maxParticles = 2500;
            main.startSpeed = new ParticleSystem.MinMaxCurve(5.5f, 7.5f);
            main.startLifetime = new ParticleSystem.MinMaxCurve(0.55f, 0.85f);
            main.startSize3D = true;
            main.startSizeX = new ParticleSystem.MinMaxCurve(0.012f, 0.02f);
            main.startSizeY = new ParticleSystem.MinMaxCurve(0.18f, 0.32f);
            main.startSizeZ = new ParticleSystem.MinMaxCurve(0.012f, 0.02f);

            var psRenderer = instance.GetComponent<ParticleSystemRenderer>();
            if (psRenderer != null)
            {
                psRenderer.renderMode = ParticleSystemRenderMode.Stretch;
                psRenderer.lengthScale = 1.8f;
                psRenderer.velocityScale = 0.06f;
                psRenderer.sharedMaterial = SoftRainMaterial(psRenderer.sharedMaterial);
            }

            main.startColor = new Color(0.72f, 0.78f, 0.88f, 0.42f);
            main.gravityModifier = 0.35f;

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
        /// depth fade, and soften the bright blue base tint. Never mutates the shared pack/Resources asset.
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
        /// Rim-only stylized mist — Kenney CloudAtlas billboards drift along the board edge, plus
        /// lightly-tuned pack <c>PF_Fog_Main</c>/<c>PF_Fog_Distant</c> at the apron. Deliberately
        /// does <b>not</b> cover the playable center (human rejected full-board fog white-out).
        /// </summary>
        private void PlaceFogMist(float width, float depth)
        {
            var root = new GameObject("RimMist");
            root.transform.SetParent(transform, false);

            float rimX = width * 0.48f;
            float rimZ = depth * 0.48f;
            float edgeAlongX = width * 0.72f;
            float edgeAlongZ = depth * 0.72f;
            float rimThickness = Mathf.Max(width, depth) * 0.12f;

            // Four edge midpoints — thin boxes that hug the apron, never the board center.
            PlaceRimMistPuff(root.transform, "Mist_N",
                new Vector3(0f, 0.55f, rimZ),
                new Vector3(edgeAlongX, 0.55f, rimThickness),
                new Color(0.78f, 0.74f, 0.62f, 0.38f),
                driftX: new ParticleSystem.MinMaxCurve(-0.12f, 0.12f),
                driftZ: new ParticleSystem.MinMaxCurve(-0.04f, 0.02f));

            PlaceRimMistPuff(root.transform, "Mist_S",
                new Vector3(0f, 0.48f, -rimZ),
                new Vector3(edgeAlongX, 0.5f, rimThickness),
                new Color(0.72f, 0.78f, 0.86f, 0.34f),
                driftX: new ParticleSystem.MinMaxCurve(-0.12f, 0.12f),
                driftZ: new ParticleSystem.MinMaxCurve(-0.02f, 0.04f));

            PlaceRimMistPuff(root.transform, "Mist_E",
                new Vector3(rimX, 0.52f, 0f),
                new Vector3(rimThickness, 0.55f, edgeAlongZ),
                new Color(0.76f, 0.72f, 0.64f, 0.36f),
                driftX: new ParticleSystem.MinMaxCurve(-0.04f, 0.02f),
                driftZ: new ParticleSystem.MinMaxCurve(-0.12f, 0.12f));

            PlaceRimMistPuff(root.transform, "Mist_W",
                new Vector3(-rimX, 0.5f, 0f),
                new Vector3(rimThickness, 0.5f, edgeAlongZ),
                new Color(0.70f, 0.76f, 0.84f, 0.32f),
                driftX: new ParticleSystem.MinMaxCurve(-0.02f, 0.04f),
                driftZ: new ParticleSystem.MinMaxCurve(-0.12f, 0.12f));

            // Corner accents — slightly higher/warmer so the rim reads as volume, still off-center.
            PlaceRimMistPuff(root.transform, "Mist_NW",
                new Vector3(-rimX * 0.92f, 0.72f, rimZ * 0.92f),
                new Vector3(rimThickness * 1.4f, 0.65f, rimThickness * 1.4f),
                new Color(0.80f, 0.74f, 0.58f, 0.40f),
                driftX: new ParticleSystem.MinMaxCurve(0.02f, 0.10f),
                driftZ: new ParticleSystem.MinMaxCurve(-0.10f, -0.02f));

            PlaceRimMistPuff(root.transform, "Mist_SE",
                new Vector3(rimX * 0.92f, 0.68f, -rimZ * 0.92f),
                new Vector3(rimThickness * 1.4f, 0.6f, rimThickness * 1.4f),
                new Color(0.68f, 0.74f, 0.82f, 0.36f),
                driftX: new ParticleSystem.MinMaxCurve(-0.10f, -0.02f),
                driftZ: new ParticleSystem.MinMaxCurve(0.02f, 0.10f));

            // Pack distant/main fog — rim apron only, low rate, never a full-board volume.
            PlaceRimPackFog(
                ref _fogDistantPrefab,
                FogDistantResourcePath,
                "FogDistant_N",
                new Vector3(0f, 1.1f, depth * 0.58f),
                width * 0.85f,
                rimThickness * 1.6f,
                rateOverTime: 8f,
                startSize: new ParticleSystem.MinMaxCurve(1.8f, 3.2f),
                startColor: new Color(0.70f, 0.74f, 0.82f, 0.22f));

            PlaceRimPackFog(
                ref _fogDistantPrefab,
                FogDistantResourcePath,
                "FogDistant_S",
                new Vector3(0f, 1.0f, -depth * 0.58f),
                width * 0.85f,
                rimThickness * 1.6f,
                rateOverTime: 8f,
                startSize: new ParticleSystem.MinMaxCurve(1.8f, 3.2f),
                startColor: new Color(0.72f, 0.70f, 0.62f, 0.20f));

            PlaceRimPackFog(
                ref _fogMainPrefab,
                FogMainResourcePath,
                "FogMain_E",
                new Vector3(width * 0.58f, 0.85f, 0f),
                rimThickness * 1.5f,
                depth * 0.7f,
                rateOverTime: 6f,
                startSize: new ParticleSystem.MinMaxCurve(1.4f, 2.4f),
                startColor: new Color(0.74f, 0.70f, 0.58f, 0.18f));

            PlaceRimPackFog(
                ref _fogMainPrefab,
                FogMainResourcePath,
                "FogMain_W",
                new Vector3(-width * 0.58f, 0.85f, 0f),
                rimThickness * 1.5f,
                depth * 0.7f,
                rateOverTime: 6f,
                startSize: new ParticleSystem.MinMaxCurve(1.4f, 2.4f),
                startColor: new Color(0.68f, 0.74f, 0.80f, 0.18f));
        }

        /// <summary>
        /// Animated Kenney-atlas mist pocket. Continuous low emission + slow drift + alpha pulse so
        /// atmosphere reads alive without covering the board center.
        /// </summary>
        private static void PlaceRimMistPuff(
            Transform parent,
            string name,
            Vector3 localPosition,
            Vector3 localScale,
            Color tint,
            ParticleSystem.MinMaxCurve driftX,
            ParticleSystem.MinMaxCurve driftZ)
        {
            var puff = new GameObject(name);
            puff.transform.SetParent(parent, false);
            puff.transform.localPosition = localPosition;

            var ps = puff.AddComponent<ParticleSystem>();
            ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

            float footprint = Mathf.Max(0.01f, localScale.x * localScale.z);
            int maxCount = Mathf.Clamp(Mathf.RoundToInt(footprint * 0.55f), 6, 14);
            float baseSize = Mathf.Sqrt(footprint / maxCount) * 1.55f;

            var main = ps.main;
            main.loop = true;
            main.playOnAwake = true;
            main.prewarm = true;
            main.duration = 6f;
            main.startLifetime = new ParticleSystem.MinMaxCurve(4.5f, 7.5f);
            main.startSpeed = 0f;
            main.startSize = new ParticleSystem.MinMaxCurve(baseSize * 0.85f, baseSize * 1.45f);
            main.startRotation = new ParticleSystem.MinMaxCurve(0f, 360f * Mathf.Deg2Rad);
            main.startColor = new ParticleSystem.MinMaxGradient(
                new Color(tint.r * 0.9f, tint.g * 0.9f, tint.b * 0.9f, tint.a * 0.75f),
                new Color(Mathf.Min(1f, tint.r * 1.1f), Mathf.Min(1f, tint.g * 1.1f), Mathf.Min(1f, tint.b * 1.1f), tint.a));
            main.maxParticles = maxCount;
            main.simulationSpace = ParticleSystemSimulationSpace.Local;
            main.gravityModifier = 0f;
            main.scalingMode = ParticleSystemScalingMode.Local;

            var emission = ps.emission;
            emission.enabled = true;
            // Low continuous rate — clearly animated, not a static fog slab.
            emission.rateOverTime = Mathf.Clamp(maxCount / 5f, 1.2f, 2.8f);

            var shape = ps.shape;
            shape.enabled = true;
            shape.shapeType = ParticleSystemShapeType.Box;
            shape.boxThickness = Vector3.one;
            shape.scale = new Vector3(
                Mathf.Max(localScale.x, 0.05f),
                Mathf.Max(localScale.y, 0.05f),
                Mathf.Max(localScale.z, 0.05f));
            shape.position = Vector3.zero;
            shape.rotation = Vector3.zero;

            var velocity = ps.velocityOverLifetime;
            velocity.enabled = true;
            velocity.space = ParticleSystemSimulationSpace.Local;
            velocity.x = driftX;
            velocity.y = new ParticleSystem.MinMaxCurve(0.02f, 0.08f);
            velocity.z = driftZ;

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
                    new GradientAlphaKey(1f, 0.2f),
                    new GradientAlphaKey(0.85f, 0.7f),
                    new GradientAlphaKey(0f, 1f),
                });
            colorOverLifetime.color = gradient;

            var rotation = ps.rotationOverLifetime;
            rotation.enabled = true;
            rotation.z = new ParticleSystem.MinMaxCurve(-0.15f, 0.15f);

            var tsa = ps.textureSheetAnimation;
            tsa.enabled = true;
            tsa.mode = ParticleSystemAnimationMode.Grid;
            tsa.numTilesX = CloudAtlasColumns;
            tsa.numTilesY = CloudAtlasRows;
            tsa.animation = ParticleSystemAnimationType.WholeSheet;
            tsa.frameOverTime = new ParticleSystem.MinMaxCurve(0f);
            tsa.startFrame = new ParticleSystem.MinMaxCurve(0f, CloudAtlasColumns * CloudAtlasRows - 1);
            tsa.cycleCount = 1;

            var renderer = puff.GetComponent<ParticleSystemRenderer>();
            renderer.renderMode = ParticleSystemRenderMode.Billboard;
            renderer.sortMode = ParticleSystemSortMode.Distance;
            renderer.sharedMaterial = MistMaterial();
            renderer.shadowCastingMode = ShadowCastingMode.Off;
            renderer.receiveShadows = false;

            ps.Play(true);
        }

        private void PlaceRimPackFog(
            ref GameObject prefabCache,
            string resourcePath,
            string name,
            Vector3 localPosition,
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
            instance.transform.localPosition = localPosition;
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
                    shape.scale = new Vector3(targetWidth, 0.55f, targetDepth);
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
                main.startLifetime = new ParticleSystem.MinMaxCurve(5f, 8f);
                main.maxParticles = Mathf.Min(main.maxParticles, 60);

                var velocity = ps.velocityOverLifetime;
                velocity.enabled = true;
                velocity.space = ParticleSystemSimulationSpace.Local;
                velocity.x = new ParticleSystem.MinMaxCurve(-0.08f, 0.08f);
                velocity.y = new ParticleSystem.MinMaxCurve(0.01f, 0.06f);
                velocity.z = new ParticleSystem.MinMaxCurve(-0.08f, 0.08f);

                var psRenderer = ps.GetComponent<ParticleSystemRenderer>();
                if (psRenderer != null && psRenderer.sharedMaterial != null)
                {
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
        /// strobing.</summary>
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
            // Ground strike — playtest 2026-08-11: anchor at board floor with slight off-center XZ.
            instance.transform.localPosition = new Vector3(width * 0.18f, 0.05f, -depth * 0.12f);
            instance.transform.localRotation = Quaternion.identity;
            instance.transform.localScale = Vector3.one;

            var ps = instance.GetComponent<ParticleSystem>();
            if (ps == null)
            {
                return;
            }

            ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

            StartCoroutine(LightningLoop(ps));
        }

        /// <summary>
        /// Re-triggers the (one-shot, non-looping) Zap rig on a randomized interval.
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

        /// <summary>
        /// Shared CloudAtlas material for ceiling cloud puffs — URP Particles/Unlit forced transparent
        /// so atlas alpha reads (opaque default = white squares).
        /// </summary>
        private static Material CloudMaterial()
        {
            if (_cloudMaterial != null)
            {
                return _cloudMaterial;
            }

            _cloudMaterial = CreateAtlasParticleMaterial(Color.white);
            return _cloudMaterial;
        }

        /// <summary>
        /// Shared CloudAtlas material for rim mist — same atlas, slightly softer base tint.
        /// </summary>
        private static Material MistMaterial()
        {
            if (_mistMaterial != null)
            {
                return _mistMaterial;
            }

            _mistMaterial = CreateAtlasParticleMaterial(new Color(1f, 1f, 1f, 0.85f));
            return _mistMaterial;
        }

        private static Material CreateAtlasParticleMaterial(Color baseTint)
        {
            var particles = Shader.Find("Universal Render Pipeline/Particles/Unlit")
                ?? Shader.Find("Particles/Standard Unlit")
                ?? Shader.Find("Sprites/Default");
            var mat = new Material(particles);

            Texture2D atlas = Resources.Load<Texture2D>("Weather/CloudAtlas");
            if (atlas != null)
            {
                mat.mainTexture = atlas;
                if (mat.HasProperty("_BaseMap"))
                {
                    mat.SetTexture("_BaseMap", atlas);
                }
            }
            else
            {
                Debug.LogWarning(
                    "BoardWeatherPocket: missing Resources/Weather/CloudAtlas.png — cloud/mist " +
                    "billboards will read as solid quads.");
            }

            mat.color = baseTint;
            if (mat.HasProperty("_BaseColor"))
            {
                mat.SetColor("_BaseColor", baseTint);
            }

            // new Material(URP Particles/Unlit) defaults Opaque — force transparent or atlas alpha is lost.
            if (mat.HasProperty("_Surface"))
            {
                mat.SetFloat("_Surface", 1f);
            }

            if (mat.HasProperty("_Blend"))
            {
                mat.SetFloat("_Blend", 0f); // Alpha
            }

            if (mat.HasProperty("_ZWrite"))
            {
                mat.SetFloat("_ZWrite", 0f);
            }

            if (mat.HasProperty("_SrcBlend"))
            {
                mat.SetFloat("_SrcBlend", (float)UnityEngine.Rendering.BlendMode.SrcAlpha);
            }

            if (mat.HasProperty("_DstBlend"))
            {
                mat.SetFloat("_DstBlend", (float)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            }

            mat.DisableKeyword("_ALPHATEST_ON");
            mat.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
            mat.renderQueue = (int)RenderQueue.Transparent;

            return mat;
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
