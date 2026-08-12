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
    /// Cloud bank (2026-08-12 <c>image copy 13</c>): Link's Awakening-style <b>clay sphere lobes</b> —
    /// opaque URP Lit meshes glued into masses (true 3D volume under ortho/rotate). Billboard
    /// CloudAtlas discs were rejected as too 2D/cheap with dark alpha 边缘. Soft atlas retained for
    /// subtle rim mist only. Pack <c>PF_CloudLayer</c> demoted. Rain = <c>PF_RainSystem</c>; Zap =
    /// <c>VFX_Zap_White</c>. Prefabs via <see cref="LogiCard.Art.Editor.WeatherPackImportTool"/>.
    /// </summary>
    public sealed class BoardWeatherPocket : MonoBehaviour
    {
        private static Material _clayCloudMaterial;
        private static Material _mistMaterial;
        private static Mesh _unitSphereMesh;
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
        /// Cloud scale/height — contained shelf over the chunk. 1.7 kept after human height notes
        /// (<c>image copy 10</c>–<c>12</c>).
        /// </summary>
        private const float InterimCloudScale = 0.9f;
        private const float InterimCloudHeightBoost = 1.7f;

        /// <summary>One clay lobe inside a mass — unit offsets in [-0.5,0.5] scaled by mass footprint.</summary>
        private readonly struct ClayLobe
        {
            public readonly Vector3 UnitOffset;
            public readonly float RadiusNorm;
            public readonly bool Belly;

            public ClayLobe(float x, float y, float z, float radiusNorm, bool belly = false)
            {
                UnitOffset = new Vector3(x, y, z);
                RadiusNorm = radiusNorm;
                Belly = belly;
            }
        }

        // Distinct glued silhouettes (LA Evil Eagle sky) — not one repeated billboard stamp.
        private static readonly ClayLobe[] PatternRaft =
        {
            new ClayLobe(0f, 0.05f, 0f, 0.42f),
            new ClayLobe(-0.28f, 0f, 0.06f, 0.30f),
            new ClayLobe(0.30f, 0.02f, -0.04f, 0.31f),
            new ClayLobe(-0.08f, 0.18f, -0.12f, 0.24f),
            new ClayLobe(0.14f, 0.16f, 0.14f, 0.22f),
            new ClayLobe(0.02f, -0.16f, 0.02f, 0.28f, belly: true),
            new ClayLobe(-0.18f, -0.12f, -0.10f, 0.20f, belly: true),
        };

        private static readonly ClayLobe[] PatternStack =
        {
            new ClayLobe(0f, -0.08f, 0f, 0.34f, belly: true),
            new ClayLobe(0f, 0.10f, 0.04f, 0.36f),
            new ClayLobe(-0.16f, 0.22f, -0.02f, 0.26f),
            new ClayLobe(0.18f, 0.20f, 0.06f, 0.25f),
            new ClayLobe(0.02f, 0.34f, 0f, 0.20f),
            new ClayLobe(-0.22f, 0.02f, 0.12f, 0.22f),
        };

        private static readonly ClayLobe[] PatternComma =
        {
            new ClayLobe(-0.12f, 0.02f, 0.04f, 0.36f),
            new ClayLobe(0.16f, 0.08f, -0.06f, 0.30f),
            new ClayLobe(0.28f, 0.20f, -0.14f, 0.20f),
            new ClayLobe(-0.26f, 0.14f, 0.10f, 0.22f),
            new ClayLobe(0.02f, -0.14f, 0.02f, 0.26f, belly: true),
            new ClayLobe(0.10f, -0.06f, 0.16f, 0.18f, belly: true),
        };

        private static readonly ClayLobe[] PatternCrown =
        {
            new ClayLobe(0f, 0.06f, 0f, 0.32f),
            new ClayLobe(-0.18f, 0.16f, 0.08f, 0.24f),
            new ClayLobe(0.18f, 0.16f, -0.06f, 0.24f),
            new ClayLobe(0f, 0.28f, 0.02f, 0.22f),
            new ClayLobe(-0.10f, -0.10f, -0.08f, 0.20f, belly: true),
            new ClayLobe(0.12f, -0.10f, 0.10f, 0.20f, belly: true),
        };

        private void PlaceCloudBank(float width, float depth)
        {
            var root = new GameObject("CloudBank");
            root.transform.SetParent(transform, false);

            // Opaque clay spheres — no alpha 边缘 rings. Desk-lamp Lit shading supplies the 3D read
            // billboards never could (human Play image copy 13).
            PlaceClayMass(root.transform, "Mass_Main",
                new Vector3(0f, 3.9f * InterimCloudHeightBoost, 0.02f * depth),
                new Vector3(width * 1.05f, 1.35f, depth * 0.85f) * InterimCloudScale,
                PatternRaft,
                topTint: new Color(0.97f, 0.98f, 1f),
                bellyTint: new Color(0.86f, 0.90f, 0.96f));

            PlaceClayMass(root.transform, "Mass_NW",
                new Vector3(-width * 0.26f, 3.75f * InterimCloudHeightBoost, depth * 0.18f),
                new Vector3(width * 0.72f, 1.25f, depth * 0.58f) * InterimCloudScale,
                PatternStack,
                topTint: new Color(0.96f, 0.97f, 1f),
                bellyTint: new Color(0.84f, 0.88f, 0.95f));

            PlaceClayMass(root.transform, "Mass_SE",
                new Vector3(width * 0.28f, 3.8f * InterimCloudHeightBoost, -depth * 0.16f),
                new Vector3(width * 0.7f, 1.2f, depth * 0.55f) * InterimCloudScale,
                PatternComma,
                topTint: new Color(1f, 0.99f, 0.97f),
                bellyTint: new Color(0.90f, 0.88f, 0.84f));

            PlaceClayMass(root.transform, "Mass_High",
                new Vector3(-width * 0.05f, 4.2f * InterimCloudHeightBoost, -depth * 0.06f),
                new Vector3(width * 0.45f, 0.95f, depth * 0.38f) * InterimCloudScale,
                PatternCrown,
                topTint: new Color(1f, 1f, 1f),
                bellyTint: new Color(0.88f, 0.91f, 0.97f));
        }

        /// <summary>
        /// One LA-style cloud mass: overlapping opaque sphere meshes (clay pillows), not camera-facing
        /// sprites. Mesh stolen once from a disposable Sphere primitive — never left in the scene as fog.
        /// </summary>
        private static void PlaceClayMass(
            Transform parent,
            string name,
            Vector3 localPosition,
            Vector3 massScale,
            ClayLobe[] pattern,
            Color topTint,
            Color bellyTint)
        {
            var mass = new GameObject(name);
            mass.transform.SetParent(parent, false);
            mass.transform.localPosition = localPosition;

            Mesh sphere = UnitSphereMesh();
            Material clay = ClayCloudMaterial();
            float footprint = Mathf.Max(massScale.x, massScale.z);

            for (int i = 0; i < pattern.Length; i++)
            {
                ClayLobe lobe = pattern[i];
                var go = new GameObject($"Lobe_{i}");
                go.transform.SetParent(mass.transform, false);

                Vector3 pos = new Vector3(
                    lobe.UnitOffset.x * massScale.x,
                    lobe.UnitOffset.y * massScale.y,
                    lobe.UnitOffset.z * massScale.z);
                go.transform.localPosition = pos;

                float diameter = lobe.RadiusNorm * footprint * 2f;
                go.transform.localScale = Vector3.one * diameter;

                var filter = go.AddComponent<MeshFilter>();
                filter.sharedMesh = sphere;

                var renderer = go.AddComponent<MeshRenderer>();
                renderer.sharedMaterial = clay;
                renderer.shadowCastingMode = ShadowCastingMode.On;
                renderer.receiveShadows = true;

                var block = new MaterialPropertyBlock();
                Color tint = lobe.Belly ? bellyTint : topTint;
                block.SetColor("_BaseColor", tint);
                block.SetColor("_Color", tint);
                renderer.SetPropertyBlock(block);
            }
        }

        private static Mesh UnitSphereMesh()
        {
            if (_unitSphereMesh != null)
            {
                return _unitSphereMesh;
            }

            // Borrow Unity's unit sphere mesh only — destroy the temp GO immediately (not scene fog).
            var tmp = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            _unitSphereMesh = tmp.GetComponent<MeshFilter>().sharedMesh;
            if (Application.isPlaying)
            {
                Object.Destroy(tmp);
            }
            else
            {
                Object.DestroyImmediate(tmp);
            }

            return _unitSphereMesh;
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
        /// Soft CloudAtlas grid for rim mist billboards only (cloud bank is clay meshes now).
        /// </summary>
        private const int CloudAtlasColumns = 4;
        private const int CloudAtlasRows = 2;

        /// <summary>
        /// Rim-only soft mist — very subtle. Human Play <c>image copy 13</c>: dense low billboards
        /// read as cheap 2D clouds over the board; keep apron haze sparse so clay bank owns the look.
        /// </summary>
        private void PlaceFogMist(float width, float depth)
        {
            var root = new GameObject("RimMist");
            root.transform.SetParent(transform, false);

            float rimX = width * 0.52f;
            float rimZ = depth * 0.52f;
            float rimThickness = Mathf.Max(width, depth) * 0.10f;

            // Two far corners only — not four full edges (was skimming the playable face).
            PlaceRimMistPuff(root.transform, "Mist_NW",
                new Vector3(-rimX * 0.95f, 0.55f, rimZ * 0.95f),
                new Vector3(rimThickness * 1.2f, 0.45f, rimThickness * 1.2f),
                new Color(0.92f, 0.90f, 0.86f, 0.18f),
                driftX: new ParticleSystem.MinMaxCurve(0.02f, 0.08f),
                driftZ: new ParticleSystem.MinMaxCurve(-0.08f, -0.02f));

            PlaceRimMistPuff(root.transform, "Mist_SE",
                new Vector3(rimX * 0.95f, 0.5f, -rimZ * 0.95f),
                new Vector3(rimThickness * 1.2f, 0.4f, rimThickness * 1.2f),
                new Color(0.88f, 0.90f, 0.94f, 0.16f),
                driftX: new ParticleSystem.MinMaxCurve(-0.08f, -0.02f),
                driftZ: new ParticleSystem.MinMaxCurve(0.02f, 0.08f));

            PlaceRimPackFog(
                ref _fogDistantPrefab,
                FogDistantResourcePath,
                "FogDistant_N",
                new Vector3(0f, 1.15f, depth * 0.62f),
                width * 0.7f,
                rimThickness * 1.4f,
                rateOverTime: 4f,
                startSize: new ParticleSystem.MinMaxCurve(1.5f, 2.6f),
                startColor: new Color(0.82f, 0.84f, 0.90f, 0.12f));

            PlaceRimPackFog(
                ref _fogDistantPrefab,
                FogDistantResourcePath,
                "FogDistant_S",
                new Vector3(0f, 1.05f, -depth * 0.62f),
                width * 0.7f,
                rimThickness * 1.4f,
                rateOverTime: 4f,
                startSize: new ParticleSystem.MinMaxCurve(1.5f, 2.6f),
                startColor: new Color(0.86f, 0.84f, 0.78f, 0.10f));
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
            shape.boxThickness = Vector3.zero;
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
        /// Shared opaque URP Lit clay for sphere lobes — desk-lamp shading gives volume; no alpha
        /// fringe, so no dark 边缘 rings (billboard failure mode on image copy 13).
        /// </summary>
        private static Material ClayCloudMaterial()
        {
            if (_clayCloudMaterial != null)
            {
                return _clayCloudMaterial;
            }

            var lit = Shader.Find("Universal Render Pipeline/Lit")
                ?? Shader.Find("Universal Render Pipeline/Simple Lit")
                ?? Shader.Find("Standard");
            _clayCloudMaterial = new Material(lit);
            var cream = new Color(0.97f, 0.98f, 1f, 1f);
            _clayCloudMaterial.color = cream;
            if (_clayCloudMaterial.HasProperty("_BaseColor"))
            {
                _clayCloudMaterial.SetColor("_BaseColor", cream);
            }

            if (_clayCloudMaterial.HasProperty("_Metallic"))
            {
                _clayCloudMaterial.SetFloat("_Metallic", 0f);
            }

            if (_clayCloudMaterial.HasProperty("_Smoothness"))
            {
                // Soft clay sheen — enough to read sphere volume under the desk lamp.
                _clayCloudMaterial.SetFloat("_Smoothness", 0.42f);
            }

            return _clayCloudMaterial;
        }

        /// <summary>
        /// Shared CloudAtlas material for rim mist — Alpha blend (additive would wash the board apron).
        /// </summary>
        private static Material MistMaterial()
        {
            if (_mistMaterial != null)
            {
                return _mistMaterial;
            }

            _mistMaterial = CreateAtlasParticleMaterial(new Color(1f, 1f, 1f, 0.75f), additive: false);
            return _mistMaterial;
        }

        private static Material CreateAtlasParticleMaterial(Color baseTint, bool additive)
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

            mat.SetOverrideTag("RenderType", "Transparent");
            if (mat.HasProperty("_Surface"))
            {
                mat.SetFloat("_Surface", 1f);
            }

            if (mat.HasProperty("_ZWrite"))
            {
                mat.SetFloat("_ZWrite", 0f);
            }

            if (mat.HasProperty("_AlphaClip"))
            {
                mat.SetFloat("_AlphaClip", 0f);
            }

            mat.DisableKeyword("_ALPHATEST_ON");
            mat.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");

            if (additive)
            {
                // URP Particles/Unlit BlendMode Additive = 2 — soft edges add light into the void
                // instead of alpha-blending toward black (which reads as a grey/black outline).
                if (mat.HasProperty("_Blend"))
                {
                    mat.SetFloat("_Blend", 2f);
                }

                if (mat.HasProperty("_SrcBlend"))
                {
                    mat.SetFloat("_SrcBlend", (float)UnityEngine.Rendering.BlendMode.SrcAlpha);
                }

                if (mat.HasProperty("_DstBlend"))
                {
                    mat.SetFloat("_DstBlend", (float)UnityEngine.Rendering.BlendMode.One);
                }

                mat.DisableKeyword("_ALPHAPREMULTIPLY_ON");
            }
            else
            {
                if (mat.HasProperty("_Blend"))
                {
                    mat.SetFloat("_Blend", 0f); // Alpha
                }

                if (mat.HasProperty("_SrcBlend"))
                {
                    mat.SetFloat("_SrcBlend", (float)UnityEngine.Rendering.BlendMode.SrcAlpha);
                }

                if (mat.HasProperty("_DstBlend"))
                {
                    mat.SetFloat("_DstBlend", (float)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                }

                mat.DisableKeyword("_ALPHAPREMULTIPLY_ON");
            }

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
