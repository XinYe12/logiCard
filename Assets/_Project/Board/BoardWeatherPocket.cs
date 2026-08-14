using System.Collections;
using System.Collections.Generic;
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
    /// Cloud bank (2026-08-12 <c>image copy 13</c>): Link's Awakening-style <b>clay lobes</b> —
    /// opaque URP Unlit meshes glued into masses (true 3D volume under ortho/rotate). Billboard
    /// CloudAtlas discs were rejected as too 2D/cheap with dark alpha 边缘 <b>as the primary cloud
    /// read</b>. Soft atlas is still used, now for two things: subtle rim mist at the board's far
    /// corners, and a thin billboard haze fringe hugging each formation's envelope so the opaque
    /// mesh's hard silhouette blurs into the void instead of cutting sharp. Each formation
    /// (2026-08-13) is built two-layer: <see cref="SpawnCloudPuff"/> makes one small irregular puff,
    /// <see cref="PlaceClayMass"/> assembles many puffs with a triangular dense-middle/loose-edge
    /// profile into a rounded mound — spherical but not entirely. Lobes are ellipsoid-squashed (not
    /// mesh-kneaded — that pass shattered shade UVs). Shading is a posterized crown/belly map.
    /// Pack <c>PF_CloudLayer</c> demoted. Rain = <c>PF_RainSystem</c>; Zap =
    /// White + Yellow only (<c>VFX_Zap_White</c> / <c>VFX_Zap_Yellow</c>). Prefabs via
    /// <see cref="LogiCard.Art.Editor.WeatherPackImportTool"/>.
    /// Weather is <b>modular</b>: <see cref="Build"/> binds the host to the board; cards call
    /// <see cref="ApplyWeather"/> to spawn Fair/Storm (or <see cref="BoardWeatherMood.Clear"/>)
    /// as a replaceable child module over the sky pocket. Storm lightning glues itself to the
    /// CloudBank's own measured world bounds (position and reach) rather than to independent
    /// hand-tuned numbers, so wherever a future card places the Storm module, the strikes follow —
    /// see <see cref="PlaceStormLightning"/>.
    /// </summary>
    public enum BoardWeatherMood
    {
        Clear,
        Fair,
        Storm,
    }

    public sealed class BoardWeatherPocket : MonoBehaviour
    {
        private static Material _clayCloudMaterial;
        private static Material _mistMaterial;
        private static Mesh _unitSphereMesh;
        private static GameObject _rainSystemPrefab;
        private static GameObject _fogMainPrefab;
        private static GameObject _fogDistantPrefab;
        private static GameObject _rainMistPrefab;
        private static GameObject _lightningWhitePrefab;
        private static GameObject _lightningYellowPrefab;

        private const string RainSystemResourcePath = "Weather/PF_RainSystem";
        private const string FogMainResourcePath = "Weather/PF_Fog_Main";
        private const string FogDistantResourcePath = "Weather/PF_Fog_Distant";
        private const string RainMistResourcePath = "Weather/PF_RainMist";
        private const string LightningWhiteResourcePath = "Weather/VFX_Zap_White";
        private const string LightningYellowResourcePath = "Weather/VFX_Zap_Yellow";

        private BoardWeatherMood _mood = BoardWeatherMood.Clear;
        private float _boardWidth;
        private float _boardDepth;
        private bool _boardBound;
        private Transform _moduleRoot;
        private Color _savedAmbient;
        private bool _lightingDimmed;
        private Light[] _dimmedLights;
        private float[] _savedLightIntensities;
        private Color[] _savedLightColors;
        private Bounds _cloudBankWorldBounds;
        private bool _hasCloudBankBounds;
        private readonly List<Bounds> _cloudMassWorldBounds = new List<Bounds>();

        /// <summary>Active weather module (Clear = no sky module).</summary>
        public BoardWeatherMood ActiveMood => _mood;

        /// <summary>
        /// Bind the weather host to <paramref name="board"/> (position + footprint). Does <b>not</b>
        /// spawn weather — cards / bootstrap call <see cref="ApplyWeather"/> afterward.
        /// </summary>
        public void Build(BoardView board)
        {
            ClearWeather();

            if (board == null || board.Model == null)
            {
                _boardBound = false;
                return;
            }

            ArenaBoard model = board.Model;
            _boardWidth = (model.MaxX - model.MinX) * board.WorldScale;
            _boardDepth = (model.MaxY - model.MinY) * board.WorldScale;
            transform.position = board.CenterWorld;
            _boardBound = true;
        }

        /// <summary>
        /// Convenience: bind board then spawn <paramref name="mood"/> (used by bootstrap / tests).
        /// </summary>
        public void Build(BoardView board, BoardWeatherMood mood)
        {
            Build(board);
            ApplyWeather(mood);
        }

        /// <summary>
        /// Card / program entry — replace the active weather module. <see cref="BoardWeatherMood.Clear"/>
        /// removes sky weather and restores pre-storm lighting. Fair/Storm spawn a self-contained
        /// child (<c>Weather_Fair</c> / <c>Weather_Storm</c>) so a future Storm card only mounts that module.
        /// </summary>
        public void ApplyWeather(BoardWeatherMood mood)
        {
            if (!_boardBound && mood != BoardWeatherMood.Clear)
            {
                Debug.LogWarning("BoardWeatherPocket.ApplyWeather: host not bound — call Build(board) first.");
                return;
            }

            ClearWeather();
            _mood = mood;
            if (mood == BoardWeatherMood.Clear)
            {
                return;
            }

            var module = new GameObject(mood == BoardWeatherMood.Storm ? "Weather_Storm" : "Weather_Fair");
            _moduleRoot = module.transform;
            _moduleRoot.SetParent(transform, false);

            float width = _boardWidth;
            float depth = _boardDepth;
            PlaceCloudBank(width, depth);
            PlaceRain(width, depth);
            PlaceFogMist(width, depth);
            if (mood == BoardWeatherMood.Storm)
            {
                PlaceStormVolumeFog(width, depth);
                ApplyStormLightingDim();
            }

            PlaceLightning(width, depth);
        }

        /// <summary>Tear down the active module and stop Zap loops. Safe if already clear.</summary>
        public void ClearWeather()
        {
            StopAllCoroutines();
            RestoreLightingIfDimmed();

            for (int i = transform.childCount - 1; i >= 0; i--)
            {
                DestroyImmediate(transform.GetChild(i).gameObject);
            }

            _moduleRoot = null;
            _mood = BoardWeatherMood.Clear;
            _hasCloudBankBounds = false;
            _cloudMassWorldBounds.Clear();
        }

        private Transform ModuleParent => _moduleRoot != null ? _moduleRoot : transform;

        /// <summary>
        /// Cloud height — contained shelf over the chunk. 1.7 kept after human height notes
        /// (<c>image copy 10</c>–<c>12</c>).
        /// </summary>
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

        /// <summary>
        /// Layer 1 — spawn function: one small, irregular cloud puff as a handful of glued lobes
        /// (golden-angle sunflower disk fill so the puff's own silhouette is never a single dominant
        /// sphere — same fix as the 2026-08-13 "big white ball" pass, just at puff scale now). Called
        /// repeatedly by <see cref="PlaceClayMass"/> (Layer 2) to build up one formation. Random per
        /// call, so every puff — and every formation — is a fresh arbitrary shape.
        /// </summary>
        private static ClayLobe[] SpawnCloudPuff(int lobeCount, float minRadiusNorm, float maxRadiusNorm)
        {
            const float goldenAngle = 137.50776f * Mathf.Deg2Rad;
            const float maxDiskRadius = 0.40f;
            lobeCount = Mathf.Max(1, lobeCount);
            var lobes = new ClayLobe[lobeCount];

            for (int i = 0; i < lobeCount; i++)
            {
                float t = (i + 0.5f) / lobeCount;
                float diskRadius = Mathf.Sqrt(t) * maxDiskRadius;
                float theta = i * goldenAngle;
                float x = diskRadius * Mathf.Cos(theta);
                float z = diskRadius * Mathf.Sin(theta);

                // Dome bias: center (small diskRadius) sits high/crown, rim sits lower/belly.
                float domeT = diskRadius / maxDiskRadius;
                float y = Mathf.Lerp(0.14f, -0.08f, domeT) + Random.Range(-0.03f, 0.03f);

                float radiusNorm = Random.Range(minRadiusNorm, maxRadiusNorm);
                lobes[i] = new ClayLobe(x, y, z, radiusNorm, belly: y < -0.01f);
            }

            return lobes;
        }

        /// <summary>Symmetric triangular distribution on [-1, 1], peaked at 0 (inverse-CDF sampling).
        /// Drives Layer 2's "dense/thick middle, loose/thin sides" placement.</summary>
        private static float TriangularSample()
        {
            float u = Random.value;
            return u < 0.5f ? Mathf.Sqrt(2f * u) - 1f : 1f - Mathf.Sqrt(2f * (1f - u));
        }

        /// <summary>One cloud mass's placement/size/tint — factors are relative to board width/depth
        /// (X/Z) or absolute world units scaled by <see cref="InterimCloudHeightBoost"/> (height).</summary>
        private readonly struct CloudMassSpec
        {
            public readonly string Name;
            public readonly float PosXFactor;
            public readonly float PosZFactor;
            public readonly float HeightUnits;
            public readonly float ScaleXFactor;
            public readonly float ScaleYUnits;
            public readonly float ScaleZFactor;
            public readonly Color TopTint;
            public readonly Color BellyTint;

            public CloudMassSpec(
                string name, float posX, float posZ, float heightUnits,
                float scaleX, float scaleY, float scaleZ, Color topTint, Color bellyTint)
            {
                Name = name;
                PosXFactor = posX;
                PosZFactor = posZ;
                HeightUnits = heightUnits;
                ScaleXFactor = scaleX;
                ScaleYUnits = scaleY;
                ScaleZFactor = scaleZ;
                TopTint = topTint;
                BellyTint = bellyTint;
            }
        }

        /// <summary>
        /// Centered cloud shelf (2026-08-13 Play: prior ±0.68–0.78 X span read as four corner piles
        /// with a hole over the board). Masses now sit in a tight central cluster (~±0.26 board width)
        /// so Fair and Storm both read as one overhead bank a card can "spawn above the sky."
        /// </summary>
        private static readonly CloudMassSpec[] CloudMasses =
        {
            new CloudMassSpec("Mass_W", -0.22f, 0.06f, 5.55f, 0.36f, 0.95f, 0.32f,
                new Color(0.99f, 0.98f, 0.97f), new Color(0.90f, 0.90f, 0.92f)),
            new CloudMassSpec("Mass_NW", -0.10f, 0.14f, 5.45f, 0.34f, 0.9f, 0.30f,
                new Color(0.99f, 0.99f, 1f), new Color(0.93f, 0.95f, 1f)),
            new CloudMassSpec("Mass_Main", 0f, 0.02f, 5.7f, 0.42f, 1.05f, 0.36f,
                new Color(1f, 1f, 1f), new Color(0.94f, 0.96f, 1f)),
            new CloudMassSpec("Mass_NE", 0.12f, 0.12f, 5.5f, 0.34f, 0.9f, 0.30f,
                new Color(0.98f, 0.99f, 1f), new Color(0.92f, 0.94f, 0.99f)),
            new CloudMassSpec("Mass_E", 0.24f, 0.04f, 5.6f, 0.36f, 0.95f, 0.32f,
                new Color(0.99f, 0.97f, 0.95f), new Color(0.89f, 0.88f, 0.90f)),
            new CloudMassSpec("Mass_S", 0.06f, -0.10f, 5.4f, 0.32f, 0.85f, 0.28f,
                new Color(1f, 0.99f, 0.98f), new Color(0.96f, 0.95f, 0.93f)),
            new CloudMassSpec("Mass_High", -0.02f, -0.02f, 6.05f, 0.28f, 0.75f, 0.24f,
                new Color(1f, 1f, 1f), new Color(0.95f, 0.96f, 1f)),
        };

        private void PlaceCloudBank(float width, float depth)
        {
            var root = new GameObject("CloudBank");
            root.transform.SetParent(ModuleParent, false);
            _cloudMassWorldBounds.Clear();

            // Opaque clay spheres — no alpha 边缘 rings. Desk-lamp Lit shading supplies the 3D read
            // billboards never could (human Play image copy 13). Soft CloudAtlas haze fringes each
            // mass's envelope so the mesh's hard silhouette blurs into the void (human ask 2026-08-13).
            for (int i = 0; i < CloudMasses.Length; i++)
            {
                CloudMassSpec spec = CloudMasses[i];
                Color topTint = _mood == BoardWeatherMood.Storm ? StormTopTint(spec.TopTint) : spec.TopTint;
                Color bellyTint = _mood == BoardWeatherMood.Storm ? StormBellyTint(spec.BellyTint) : spec.BellyTint;
                Vector3 pos = new Vector3(
                    spec.PosXFactor * width,
                    spec.HeightUnits * InterimCloudHeightBoost,
                    spec.PosZFactor * depth);
                Vector3 scale = new Vector3(spec.ScaleXFactor * width, spec.ScaleYUnits, spec.ScaleZFactor * depth);

                // 7-10 puffs per formation, each its own small Layer-1 puff — see PlaceClayMass for
                // the triangular dense-middle/loose-edges assembly (human ask 2026-08-13).
                PlaceClayMass(root.transform, spec.Name, pos, scale, Random.Range(7, 11), RandomMassYaw(), topTint, bellyTint);
                PlaceCloudEdgeHaze(root.transform, "Haze_" + spec.Name, pos, scale, topTint);

                // Per-mass bounds, not just the aggregate — masses sit at different heights (Mass_High
                // is a full HeightUnits taller than the rest), so a strike's X/Z and its target tip
                // height must both come from the SAME mass, never "position under mass A, height from
                // the whole bank's center" (that mismatch was still poking through — image copy 17).
                Transform massTransform = root.transform.Find(spec.Name);
                if (massTransform != null)
                {
                    _cloudMassWorldBounds.Add(ComputeWorldBounds(massTransform));
                }
            }

            // Aggregate kept too — used only as a last-resort fallback if per-mass placement ever fails.
            _cloudBankWorldBounds = ComputeWorldBounds(root.transform);
            _hasCloudBankBounds = _cloudBankWorldBounds.size.sqrMagnitude > 0f;
        }

        /// <summary>World-space bounds of every opaque clay mesh under <paramref name="root"/>,
        /// encapsulated. Cheap and exact — these are static <see cref="MeshRenderer"/>s, not particles,
        /// so bounds are valid the instant they're created (no simulate-forward needed).</summary>
        private static Bounds ComputeWorldBounds(Transform root)
        {
            var renderers = root.GetComponentsInChildren<MeshRenderer>(true);
            if (renderers.Length == 0)
            {
                return new Bounds(root.position, Vector3.zero);
            }

            Bounds bounds = renderers[0].bounds;
            for (int i = 1; i < renderers.Length; i++)
            {
                bounds.Encapsulate(renderers[i].bounds);
            }

            return bounds;
        }

        /// <summary>Storm clay — same mound shapes as fair, pulled toward slate grey so the bank
        /// reads as weather, not bright fair-weather pillows (human 2026-08-13).</summary>
        private static Color StormTopTint(Color fair) =>
            Color.Lerp(fair, new Color(0.52f, 0.56f, 0.64f, 1f), 0.78f);

        private static Color StormBellyTint(Color fair) =>
            Color.Lerp(fair, new Color(0.30f, 0.33f, 0.40f, 1f), 0.85f);

        /// <summary>Mild random spin per mass so the same cluster doesn't always face the same way.</summary>
        private static float RandomMassYaw() => Random.Range(0f, 360f);

        /// <summary>
        /// Layer 2 — assembly: places <paramref name="puffCount"/> Layer-1 puffs (<see
        /// cref="SpawnCloudPuff"/>) inside one formation using a <b>triangular density/size profile</b>
        /// along the formation's local X — puffs land denser and bigger near the center
        /// (<see cref="TriangularSample"/> peaks at 0) and sparser/smaller toward the fringe, with fringe
        /// puffs drifting slightly upward. Reads as a rounded-triangle/pyramid cloud built from many
        /// small irregular pieces, not a uniform ball of same-size lobes — human 2026-08-13, pointing at
        /// a reference: "dense, thick in the middle, loose and thin at the sides... almost triangular."
        /// No cast shadows (image copy 14: shadow ate the board). No Lit terminator darkening —
        /// Unlit + posterized shade map keeps crowns bright and bellies pale without a glossy-render
        /// gradient (image copy 16 close-up: "needs to be more stylized").
        /// Individual lobes are Y-squashed ellipsoids (yaw only — never pitch/roll). Full random
        /// spin rotated the crown/belly shade map off-axis so every pillow self-lit (image copy 15
        /// "lightings wrongly applied"). Volume read comes from mass-height tint + a near-flat
        /// shade wash, not from each lobe's own highlight.
        /// </summary>
        private static void PlaceClayMass(
            Transform parent,
            string name,
            Vector3 localPosition,
            Vector3 massScale,
            int puffCount,
            float yawDegrees,
            Color topTint,
            Color bellyTint)
        {
            var mass = new GameObject(name);
            mass.transform.SetParent(parent, false);
            mass.transform.localPosition = localPosition;
            mass.transform.localRotation = Quaternion.Euler(0f, yawDegrees, 0f);

            Mesh sphere = UnitSphereMesh();
            Material clay = ClayCloudMaterial();
            float footprint = Mathf.Max(massScale.x, massScale.z);

            for (int p = 0; p < puffCount; p++)
            {
                float u = TriangularSample(); // -1..1, peaked at 0 — formation's dense/thick core
                float depthJitter = Random.Range(-0.5f, 0.5f);
                float edgeT = Mathf.Abs(u); // 0 at core .. 1 at fringe

                float puffScale = Mathf.Lerp(1f, 0.4f, edgeT); // core puffs big, fringe puffs small
                int puffLobes = Mathf.RoundToInt(Mathf.Lerp(5f, 2f, edgeT)); // fringe puffs thinner too
                float puffMinR = Mathf.Lerp(0.22f, 0.15f, edgeT);
                float puffMaxR = Mathf.Lerp(0.30f, 0.21f, edgeT);

                var puffGo = new GameObject($"Puff_{p}");
                puffGo.transform.SetParent(mass.transform, false);
                // Pull depth toward the core so the assembled mass reads as one rounded mound
                // (spherical-but-not-entirely), not a flat raft of equal bubbles.
                float depthSpread = massScale.z * (0.55f - 0.25f * edgeT);
                float puffY = Mathf.Lerp(0.06f, 0.20f, edgeT) * massScale.y;
                puffGo.transform.localPosition = new Vector3(
                    u * massScale.x * 0.42f,
                    puffY,
                    depthJitter * depthSpread);

                ClayLobe[] puff = SpawnCloudPuff(puffLobes, puffMinR, puffMaxR);
                float puffFootprintXZ = footprint * puffScale * 0.55f;
                float puffThicknessY = massScale.y * puffScale;

                for (int i = 0; i < puff.Length; i++)
                {
                    ClayLobe lobe = puff[i];
                    var go = new GameObject($"Lobe_{i}");
                    go.transform.SetParent(puffGo.transform, false);

                    Vector3 lobeLocal = new Vector3(
                        lobe.UnitOffset.x * puffFootprintXZ,
                        lobe.UnitOffset.y * puffThicknessY,
                        lobe.UnitOffset.z * puffFootprintXZ);
                    go.transform.localPosition = lobeLocal;

                    // Glue oversize + Y-squash ellipsoid. Yaw only — shade map crown stays world-up.
                    float diameter = lobe.RadiusNorm * puffFootprintXZ * 2.35f;
                    go.transform.localScale = new Vector3(
                        diameter * Random.Range(0.88f, 1.18f),
                        diameter * Random.Range(0.62f, 0.92f),
                        diameter * Random.Range(0.88f, 1.18f));
                    go.transform.localRotation = Quaternion.Euler(0f, Random.Range(0f, 360f), 0f);

                    var filter = go.AddComponent<MeshFilter>();
                    filter.sharedMesh = sphere;

                    var renderer = go.AddComponent<MeshRenderer>();
                    renderer.sharedMaterial = clay;
                    // Shadows off — cast shadow blacked out the board (image copy 14); self-receive
                    // carved harsh crevices between lobes.
                    renderer.shadowCastingMode = ShadowCastingMode.Off;
                    renderer.receiveShadows = false;

                    // Mass-volume tint: brighter toward the mound crown, cooler toward belly/fringe.
                    // Replaces per-lobe shade-map "each ball has its own sun" lighting.
                    float height01 = Mathf.InverseLerp(
                        -0.35f * massScale.y,
                        0.55f * massScale.y,
                        puffY + lobeLocal.y);
                    Color tint = Color.Lerp(bellyTint, topTint, Mathf.Clamp01(height01));
                    tint = Color.Lerp(tint, bellyTint, edgeT * 0.35f);
                    var block = new MaterialPropertyBlock();
                    block.SetColor("_BaseColor", tint);
                    block.SetColor("_Color", tint);
                    renderer.SetPropertyBlock(block);
                }
            }
        }

        /// <summary>
        /// Soft CloudAtlas billboard fringe riding the outer envelope of one clay mass. The opaque
        /// Unlit spheres have a hard mesh-edge silhouette with no built-in softness (no Fresnel/rim
        /// alpha available without a custom shader); this reuses the already-proven rim-mist billboard
        /// technique (<see cref="PlaceRimMistPuff"/>) — camera-facing puffs with baked alpha falloff —
        /// so the silhouette blurs into the void instead of cutting hard (human ask 2026-08-13).
        /// Depth-tested against the opaque core (material/queue unchanged from <see cref="MistMaterial"/>),
        /// so haze never shows through a mass onto the board.
        /// </summary>
        private static void PlaceCloudEdgeHaze(
            Transform parent,
            string name,
            Vector3 localPosition,
            Vector3 massScale,
            Color tint)
        {
            var puff = new GameObject(name);
            puff.transform.SetParent(parent, false);
            puff.transform.localPosition = localPosition;

            var ps = puff.AddComponent<ParticleSystem>();
            ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

            // Thin silhouette softener only — heavy haze read as more floating glass bubbles on top
            // of the clay (image copy 15). Keep fringe sparse/low-alpha so the two-layer clay mass owns the shape.
            Vector3 envelope = massScale * 1.08f;
            float footprint = Mathf.Max(0.01f, envelope.x * envelope.z);
            int maxCount = Mathf.Clamp(Mathf.RoundToInt(footprint * 0.4f), 6, 14);
            float baseSize = Mathf.Sqrt(footprint / maxCount) * 1.55f;

            var main = ps.main;
            main.loop = true;
            main.playOnAwake = true;
            main.prewarm = true;
            main.duration = 8f;
            main.startLifetime = new ParticleSystem.MinMaxCurve(6f, 9.5f);
            main.startSpeed = 0f;
            main.startSize = new ParticleSystem.MinMaxCurve(baseSize * 0.85f, baseSize * 1.35f);
            main.startRotation = new ParticleSystem.MinMaxCurve(0f, 360f * Mathf.Deg2Rad);
            main.startColor = new ParticleSystem.MinMaxGradient(
                new Color(tint.r, tint.g, tint.b, 0.16f),
                new Color(tint.r, tint.g, tint.b, 0.30f));
            main.maxParticles = maxCount;
            main.simulationSpace = ParticleSystemSimulationSpace.Local;
            main.gravityModifier = 0f;
            main.scalingMode = ParticleSystemScalingMode.Local;

            var emission = ps.emission;
            emission.enabled = true;
            emission.rateOverTime = Mathf.Clamp(maxCount / 6f, 1.5f, 3.5f);

            var shape = ps.shape;
            shape.enabled = true;
            shape.shapeType = ParticleSystemShapeType.Box;
            shape.scale = envelope;
            // Shell only — haze rides the mass envelope surface, not its interior (would hide
            // behind the opaque core and waste particles).
            shape.boxThickness = Vector3.zero;
            shape.position = Vector3.zero;
            shape.rotation = Vector3.zero;

            var velocity = ps.velocityOverLifetime;
            velocity.enabled = true;
            velocity.space = ParticleSystemSimulationSpace.Local;
            velocity.x = new ParticleSystem.MinMaxCurve(-0.03f, 0.03f);
            velocity.y = new ParticleSystem.MinMaxCurve(-0.02f, 0.02f);
            velocity.z = new ParticleSystem.MinMaxCurve(-0.03f, 0.03f);

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
                    new GradientAlphaKey(1f, 0.25f),
                    new GradientAlphaKey(0.8f, 0.75f),
                    new GradientAlphaKey(0f, 1f),
                });
            colorOverLifetime.color = gradient;

            var rotation = ps.rotationOverLifetime;
            rotation.enabled = true;
            rotation.z = new ParticleSystem.MinMaxCurve(-0.08f, 0.08f);

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

            var instance = Instantiate(prefab, ModuleParent);
            instance.name = "Rain";
            // Emit just under the cloud shelf so streaks read as falling out of the pocket. Bumped
            // from 2.85 with the 2026-08-13 cloud-height raise (masses now sit at 5.4-6.1 units,
            // was 3.75-4.3) so rain still starts close under the clouds instead of from a bare gap.
            instance.transform.localPosition = new Vector3(0f, 4.6f * InterimCloudHeightBoost, 0f);
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

            bool storm = _mood == BoardWeatherMood.Storm;
            var emission = ps.emission;
            emission.rateOverTime = storm ? 1600f : 700f;

            var main = ps.main;
            main.prewarm = true;
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            main.maxParticles = storm ? 4500 : 2500;
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

            main.startColor = storm
                ? new Color(0.55f, 0.60f, 0.70f, 0.55f)
                : new Color(0.72f, 0.78f, 0.88f, 0.42f);
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
            root.transform.SetParent(ModuleParent, false);

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

            var instance = Instantiate(prefab, ModuleParent);
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

        /// <summary>Fair weather — occasional single strike.</summary>
        private const float FairLightningIntervalMinSeconds = 12f;
        private const float FairLightningIntervalMaxSeconds = 22f;

        /// <summary>Storm — many more pack Zap plays (human 2026-08-13).</summary>
        private const float StormLightningIntervalMinSeconds = 0.9f;
        private const float StormLightningIntervalMaxSeconds = 2.8f;
        private const int StormLightningRigCount = 6;

        private void PlaceLightning(float width, float depth)
        {
            if (_mood == BoardWeatherMood.Storm)
            {
                PlaceStormLightning(width, depth);
                return;
            }

            GameObject prefab = LoadPrefab(ref _lightningWhitePrefab, LightningWhiteResourcePath);
            if (prefab == null)
            {
                return;
            }

            ParticleSystem ps = SpawnZapRig(
                prefab,
                "Lightning",
                new Vector3(width * 0.18f, 0.05f, -depth * 0.12f),
                targetTipWorldY: null);
            if (ps != null)
            {
                StartCoroutine(LightningLoop(ps, FairLightningIntervalMinSeconds, FairLightningIntervalMaxSeconds, doubleStrikeChance: 0f));
            }
        }

        /// <summary>
        /// Storm Zap field — <b>Yellow only</b>. Bolt height is driven by the placed cloud mass
        /// height (<see cref="StrikeTipWorldY"/> ← measured clay bounds from
        /// <c>HeightUnits × InterimCloudHeightBoost</c>). Ground spawn, upright; ConeVolume
        /// <c>shape.length</c> = rise from ground to that cloud Y so the tip tracks the shelf.
        /// </summary>
        private void PlaceStormLightning(float width, float depth)
        {
            var root = new GameObject("LightningStorm");
            root.transform.SetParent(ModuleParent, false);

            GameObject prefab = LoadPrefab(ref _lightningYellowPrefab, LightningYellowResourcePath);
            if (prefab == null)
            {
                Debug.LogWarning(
                    "BoardWeatherPocket: missing Resources/Weather/VFX_Zap_Yellow — run " +
                    "Tools > LogiCard > Import Weather Pack Prefabs.");
                return;
            }

            for (int i = 0; i < StormLightningRigCount; i++)
            {
                Bounds strikeMass = PickStrikeMassBounds();
                ParticleSystem ps = SpawnStormZap(
                    prefab,
                    $"Zap_{i}",
                    StrikeLocalPositionWithinMass(strikeMass, width, depth),
                    StrikeTipWorldY(strikeMass));
                if (ps == null)
                {
                    continue;
                }

                ps.transform.SetParent(root.transform, true);
                StartCoroutine(LightningLoop(
                    ps,
                    StormLightningIntervalMinSeconds,
                    StormLightningIntervalMaxSeconds,
                    doubleStrikeChance: 0.55f));
            }
        }

        /// <summary>Picks one placed cloud mass's own measured bounds. Falls back to the CloudBank
        /// aggregate, then to a degenerate placeholder, only if per-mass placement didn't happen.</summary>
        private Bounds PickStrikeMassBounds()
        {
            if (_cloudMassWorldBounds.Count > 0)
            {
                return _cloudMassWorldBounds[Random.Range(0, _cloudMassWorldBounds.Count)];
            }

            if (_hasCloudBankBounds)
            {
                return _cloudBankWorldBounds;
            }

            return new Bounds(ModuleParent.position, Vector3.zero);
        }

        /// <summary>Strike X/Z sampled from one mass's real measured footprint, inset so strikes stay
        /// under its silhouette rather than its bounding-box edge. Falls back to the old board-fraction
        /// guess only if no bounds are available at all (e.g. CloudBank failed to place).</summary>
        private Vector3 StrikeLocalPositionWithinMass(Bounds massBounds, float width, float depth)
        {
            if (massBounds.size.sqrMagnitude > 0f)
            {
                const float footprintInset = 0.55f;
                float x = massBounds.center.x + Random.Range(-0.5f, 0.5f) * massBounds.size.x * footprintInset;
                float z = massBounds.center.z + Random.Range(-0.5f, 0.5f) * massBounds.size.z * footprintInset;
                Vector3 groundWorld = new Vector3(x, ModuleParent.position.y + 0.05f, z);
                return ModuleParent.InverseTransformPoint(groundWorld);
            }

            return new Vector3(Random.Range(-0.22f, 0.22f) * width, 0.05f, Random.Range(-0.18f, 0.18f) * depth);
        }

        /// <summary>
        /// Cloud-height reference for the bolt tip — the mass's measured world-Y center (from the
        /// fixed shelf placement <c>HeightUnits × InterimCloudHeightBoost</c>). Same mass as the
        /// strike X/Z so tip height matches the clay actually above that point.
        /// </summary>
        private static float? StrikeTipWorldY(Bounds massBounds)
        {
            return massBounds.size.sqrMagnitude > 0f ? massBounds.center.y : (float?)null;
        }

        /// <summary>
        /// Storm Zap: ground spawn, stock upright look. Bolt vertical span is set from
        /// <paramref name="cloudTipWorldY"/> (cloud shelf height) via
        /// <see cref="FitZapHeightToCloudRise"/>.
        /// </summary>
        private ParticleSystem SpawnStormZap(
            GameObject prefab,
            string name,
            Vector3 groundLocalPosition,
            float? cloudTipWorldY)
        {
            var instance = Instantiate(prefab, ModuleParent);
            instance.name = name;
            instance.transform.localPosition = groundLocalPosition;
            instance.transform.localRotation = Quaternion.identity;
            instance.transform.localScale = Vector3.one;

            if (cloudTipWorldY.HasValue)
            {
                float cloudRise = Mathf.Max(0.5f, cloudTipWorldY.Value - instance.transform.position.y);
                FitZapHeightToCloudRise(instance, cloudRise);
            }

            var systems = instance.GetComponentsInChildren<ParticleSystem>(true);
            for (int i = 0; i < systems.Length; i++)
            {
                systems[i].Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            }

            return instance.GetComponent<ParticleSystem>() ?? (systems.Length > 0 ? systems[0] : null);
        }

        /// <summary>
        /// Map cloud rise (ground → cloud shelf Y) onto the Zap bolt layers. Prefab uses Cone
        /// (base-only) with a fixed <c>length: 5</c>; switching those layers to
        /// <see cref="ParticleSystemShapeType.ConeVolume"/> and setting <c>shape.length = cloudRise</c>
        /// makes the particle span track the clay height. Stretch <c>lengthScale</c> is kept modest
        /// so the ribbon doesn't poke past the shelf; <c>startSize</c> is left alone (thickness).
        /// Ground Sphere layers (flare / scorch / floor) are untouched.
        /// </summary>
        private static void FitZapHeightToCloudRise(GameObject zapRoot, float cloudRise)
        {
            float rise = Mathf.Max(0.5f, cloudRise);

            var systems = zapRoot.GetComponentsInChildren<ParticleSystem>(true);
            for (int i = 0; i < systems.Length; i++)
            {
                ParticleSystem ps = systems[i];
                ParticleSystem.ShapeModule shape = ps.shape;

                if (shape.enabled &&
                    (shape.shapeType == ParticleSystemShapeType.Sphere ||
                     shape.shapeType == ParticleSystemShapeType.Hemisphere))
                {
                    continue;
                }

                bool isCone =
                    shape.enabled &&
                    (shape.shapeType == ParticleSystemShapeType.Cone ||
                     shape.shapeType == ParticleSystemShapeType.ConeVolume ||
                     shape.shapeType == ParticleSystemShapeType.ConeShell ||
                     shape.shapeType == ParticleSystemShapeType.ConeVolumeShell);

                if (isCone)
                {
                    // Volume along +Y (prefab already tilts the shape 90°) — length = cloud shelf rise.
                    shape.shapeType = ParticleSystemShapeType.ConeVolume;
                    shape.angle = 0f;
                    shape.radius = 0.01f;
                    shape.length = rise;
                }

                var renderer = ps.GetComponent<ParticleSystemRenderer>();
                if (renderer != null && renderer.renderMode == ParticleSystemRenderMode.Stretch)
                {
                    // Prefab lengthScale 2 on size~2 added ~4u past the cone tip and cleared the clay.
                    // Keep a short ribbon so segments connect without overshooting the shelf.
                    renderer.velocityScale = 0f;
                    renderer.lengthScale = 0.75f;
                }
            }
        }

        private ParticleSystem SpawnZapRig(
            GameObject prefab,
            string name,
            Vector3 localPosition,
            float? targetTipWorldY)
        {
            var instance = Instantiate(prefab, ModuleParent);
            instance.name = name;
            instance.transform.localPosition = localPosition;
            instance.transform.localRotation = Quaternion.identity;
            instance.transform.localScale = Vector3.one;

            // Fair — stock upward Zap, untouched.
            _ = targetTipWorldY;

            var systems = instance.GetComponentsInChildren<ParticleSystem>(true);
            for (int i = 0; i < systems.Length; i++)
            {
                systems[i].Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            }

            return instance.GetComponent<ParticleSystem>() ?? (systems.Length > 0 ? systems[0] : null);
        }

        /// <summary>
        /// Re-triggers the (one-shot, non-looping) Zap rig on a randomized interval. Storm passes a
        /// high <paramref name="doubleStrikeChance"/> so flashes cluster like real lightning.
        /// </summary>
        private static IEnumerator LightningLoop(
            ParticleSystem rig,
            float intervalMin,
            float intervalMax,
            float doubleStrikeChance)
        {
            // Stagger first strike so six storm rigs don't sync on frame 0.
            yield return new WaitForSeconds(Random.Range(0.05f, intervalMax));

            while (rig != null)
            {
                rig.Play(true);
                if (doubleStrikeChance > 0f && Random.value < doubleStrikeChance)
                {
                    yield return new WaitForSeconds(Random.Range(0.06f, 0.18f));
                    if (rig == null)
                    {
                        yield break;
                    }

                    rig.Play(true);
                }

                yield return new WaitForSeconds(Random.Range(intervalMin, intervalMax));
            }
        }

        /// <summary>
        /// Pack fog volumes for storm only — denser apron without a full-board fog slab. Fair weather
        /// keeps rim-only mist (PlayMode smoke asserts no FogGround/RainMist there).
        /// </summary>
        private void PlaceStormVolumeFog(float width, float depth)
        {
            PlaceRimPackFog(
                ref _fogMainPrefab,
                FogMainResourcePath,
                "FogMain_Storm",
                new Vector3(0f, 1.4f, depth * 0.35f),
                width * 0.85f,
                depth * 0.35f,
                rateOverTime: 10f,
                startSize: new ParticleSystem.MinMaxCurve(2.0f, 3.4f),
                startColor: new Color(0.45f, 0.48f, 0.55f, 0.22f));

            PlaceRimPackFog(
                ref _rainMistPrefab,
                RainMistResourcePath,
                "RainMist_Storm",
                new Vector3(0f, 0.85f, 0f),
                width * 0.9f,
                depth * 0.9f,
                rateOverTime: 8f,
                startSize: new ParticleSystem.MinMaxCurve(1.2f, 2.2f),
                startColor: new Color(0.50f, 0.54f, 0.60f, 0.16f));
        }

        /// <summary>
        /// Cool + dim diorama lights under a storm module. Snapshots intensities so
        /// <see cref="ClearWeather"/> / Fair can restore (card swap must not permanently crush the key).
        /// </summary>
        private void ApplyStormLightingDim()
        {
            if (_lightingDimmed)
            {
                return;
            }

            _savedAmbient = RenderSettings.ambientLight;
            RenderSettings.ambientLight = new Color(0.16f, 0.18f, 0.22f);

            Light[] lights = Object.FindObjectsByType<Light>(FindObjectsSortMode.None);
            _dimmedLights = lights;
            _savedLightIntensities = new float[lights.Length];
            _savedLightColors = new Color[lights.Length];
            for (int i = 0; i < lights.Length; i++)
            {
                Light light = lights[i];
                if (light == null)
                {
                    continue;
                }

                _savedLightIntensities[i] = light.intensity;
                _savedLightColors[i] = light.color;
                if (!light.enabled)
                {
                    continue;
                }

                if (light.type == LightType.Directional)
                {
                    light.intensity *= 0.62f;
                    light.color = Color.Lerp(light.color, new Color(0.70f, 0.76f, 0.88f), 0.55f);
                }
                else if (light.type == LightType.Point)
                {
                    light.intensity *= 0.85f;
                }
            }

            _lightingDimmed = true;
        }

        private void RestoreLightingIfDimmed()
        {
            if (!_lightingDimmed)
            {
                return;
            }

            RenderSettings.ambientLight = _savedAmbient;
            if (_dimmedLights != null)
            {
                for (int i = 0; i < _dimmedLights.Length; i++)
                {
                    Light light = _dimmedLights[i];
                    if (light == null)
                    {
                        continue;
                    }

                    light.intensity = _savedLightIntensities[i];
                    light.color = _savedLightColors[i];
                }
            }

            _dimmedLights = null;
            _savedLightIntensities = null;
            _savedLightColors = null;
            _lightingDimmed = false;
        }

        /// <summary>
        /// Soft Unlit clay for sphere lobes. Lit diffuse was darkening sphere limbs into mid-grey
        /// "边缘" against the void (image copy 14). Shade map is a <b>near-flat</b> vertical wash —
        /// strong per-lobe crown contrast read as individually lit bubbles under the top-down camera
        /// (image copy 15). Mass-volume tint is applied in <see cref="PlaceClayMass"/> instead.
        /// No cast shadows so the board stays readable.
        /// </summary>
        private static Material ClayCloudMaterial()
        {
            if (_clayCloudMaterial != null)
            {
                return _clayCloudMaterial;
            }

            var unlit = Shader.Find("Universal Render Pipeline/Unlit")
                ?? Shader.Find("Unlit/Color")
                ?? Shader.Find("Sprites/Default");
            _clayCloudMaterial = new Material(unlit);

            Texture2D shade = Resources.Load<Texture2D>("Weather/ClaySphereShade");
            if (shade != null)
            {
                _clayCloudMaterial.mainTexture = shade;
                if (_clayCloudMaterial.HasProperty("_BaseMap"))
                {
                    _clayCloudMaterial.SetTexture("_BaseMap", shade);
                }
            }

            var cream = new Color(1f, 1f, 1f, 1f);
            _clayCloudMaterial.color = cream;
            if (_clayCloudMaterial.HasProperty("_BaseColor"))
            {
                _clayCloudMaterial.SetColor("_BaseColor", cream);
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
