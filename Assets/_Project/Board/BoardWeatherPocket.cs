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
    /// Cloud bank (2026-08-12 <c>image copy 13</c>): Link's Awakening-style <b>clay sphere lobes</b> —
    /// opaque URP Unlit meshes glued into masses (true 3D volume under ortho/rotate). Billboard
    /// CloudAtlas discs were rejected as too 2D/cheap with dark alpha 边缘 <b>as the primary cloud
    /// read</b>. Soft atlas is still used, now for two things: subtle rim mist at the board's far
    /// corners, and a thin billboard haze fringe hugging each formation's envelope so the opaque
    /// mesh's hard silhouette blurs into the void instead of cutting sharp. Each formation
    /// (2026-08-13) is built two-layer: <see cref="SpawnCloudPuff"/> makes one small irregular puff,
    /// <see cref="PlaceClayMass"/> assembles many puffs with a triangular dense-middle/loose-edge
    /// profile — a real cloud shape built from small pieces, not a scaled-up single sphere. Shading is
    /// a posterized (not smooth) crown/belly map so close range reads as painted, not a glossy render.
    /// Pack <c>PF_CloudLayer</c> demoted. Rain = <c>PF_RainSystem</c>; Zap =
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
                GameObject child = transform.GetChild(i).gameObject;
                DestroyKneadedLobeMeshes(child);
                DestroyImmediate(child);
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

        /// <summary>Each clay lobe now gets its own kneaded (non-shared) mesh instance — unlike the
        /// cached <see cref="_unitSphereMesh"/>, these must be explicitly destroyed on rebuild or they
        /// leak (Unity meshes aren't GC'd just because their GameObject is destroyed).</summary>
        private static void DestroyKneadedLobeMeshes(GameObject root)
        {
            var filters = root.GetComponentsInChildren<MeshFilter>(true);
            for (int i = 0; i < filters.Length; i++)
            {
                Mesh mesh = filters[i].sharedMesh;
                if (mesh != null && mesh != _unitSphereMesh)
                {
                    DestroyImmediate(mesh);
                }
            }
        }

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
        /// Masses spread across the full board width (2026-08-13: "single cloud size is too big" — a
        /// few large blobs read as separate objects; replaced with seven smaller ones so the bank reads
        /// as one continuous cloud layer instead of discrete chunks). X spacing between adjacent masses
        /// is tighter than <c>ScaleXFactor</c> half-widths sum to (deliberate overlap margin) so
        /// neighboring masses' silhouettes visually touch instead of leaving a void gap between them
        /// (human ask 2026-08-13: "clouds need to be more glued together"). Height bumped ~45% from the
        /// prior pass (still not high enough per human Play — "still needs to be higher").
        /// </summary>
        private static readonly CloudMassSpec[] CloudMasses =
        {
            new CloudMassSpec("Mass_W2", -0.68f, 0.10f, 5.6f, 0.34f, 0.85f, 0.30f,
                new Color(0.99f, 0.98f, 0.97f), new Color(0.90f, 0.90f, 0.92f)),
            new CloudMassSpec("Mass_NW", -0.38f, 0.20f, 5.4f, 0.40f, 0.95f, 0.34f,
                new Color(0.99f, 0.99f, 1f), new Color(0.93f, 0.95f, 1f)),
            new CloudMassSpec("Mass_Main", 0f, 0.04f, 5.7f, 0.46f, 1.05f, 0.36f,
                new Color(1f, 1f, 1f), new Color(0.94f, 0.96f, 1f)),
            new CloudMassSpec("Mass_NE", 0.32f, 0.14f, 5.5f, 0.36f, 0.9f, 0.30f,
                new Color(0.98f, 0.99f, 1f), new Color(0.92f, 0.94f, 0.99f)),
            new CloudMassSpec("Mass_SE", 0.56f, -0.12f, 5.45f, 0.38f, 0.9f, 0.32f,
                new Color(1f, 0.99f, 0.98f), new Color(0.96f, 0.95f, 0.93f)),
            new CloudMassSpec("Mass_E2", 0.78f, 0.06f, 5.75f, 0.30f, 0.8f, 0.26f,
                new Color(0.99f, 0.97f, 0.95f), new Color(0.89f, 0.88f, 0.90f)),
            new CloudMassSpec("Mass_High", -0.10f, -0.10f, 6.1f, 0.24f, 0.7f, 0.22f,
                new Color(1f, 1f, 1f), new Color(0.95f, 0.96f, 1f)),
        };

        private void PlaceCloudBank(float width, float depth)
        {
            var root = new GameObject("CloudBank");
            root.transform.SetParent(transform, false);

            // Opaque clay spheres — no alpha 边缘 rings. Desk-lamp Lit shading supplies the 3D read
            // billboards never could (human Play image copy 13). Soft CloudAtlas haze fringes each
            // mass's envelope so the mesh's hard silhouette blurs into the void (human ask 2026-08-13).
            for (int i = 0; i < CloudMasses.Length; i++)
            {
                CloudMassSpec spec = CloudMasses[i];
                Vector3 pos = new Vector3(
                    spec.PosXFactor * width,
                    spec.HeightUnits * InterimCloudHeightBoost,
                    spec.PosZFactor * depth);
                Vector3 scale = new Vector3(spec.ScaleXFactor * width, spec.ScaleYUnits, spec.ScaleZFactor * depth);

                // 7-10 puffs per formation, each its own small Layer-1 puff — see PlaceClayMass for
                // the triangular dense-middle/loose-edges assembly (human ask 2026-08-13).
                PlaceClayMass(root.transform, spec.Name, pos, scale, Random.Range(7, 11), RandomMassYaw(), spec.TopTint, spec.BellyTint);
                PlaceCloudEdgeHaze(root.transform, "Haze_" + spec.Name, pos, scale, spec.TopTint);
            }
        }

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
                puffGo.transform.localPosition = new Vector3(
                    u * massScale.x * 0.5f,
                    Mathf.Lerp(0f, 0.18f, edgeT) * massScale.y, // fringe wisps drift up and out
                    depthJitter * massScale.z * (1f - 0.3f * edgeT));

                ClayLobe[] puff = SpawnCloudPuff(puffLobes, puffMinR, puffMaxR);
                float puffFootprintXZ = footprint * puffScale * 0.55f;
                float puffThicknessY = massScale.y * puffScale;

                for (int i = 0; i < puff.Length; i++)
                {
                    ClayLobe lobe = puff[i];
                    var go = new GameObject($"Lobe_{i}");
                    go.transform.SetParent(puffGo.transform, false);

                    go.transform.localPosition = new Vector3(
                        lobe.UnitOffset.x * puffFootprintXZ,
                        lobe.UnitOffset.y * puffThicknessY,
                        lobe.UnitOffset.z * puffFootprintXZ);

                    // Slightly oversized so lobes swallow each other (glued clay, not separate balls).
                    float diameter = lobe.RadiusNorm * puffFootprintXZ * 2.0f;
                    go.transform.localScale = Vector3.one * diameter;

                    var filter = go.AddComponent<MeshFilter>();
                    // Kneaded per-lobe (2026-08-13) — no two lobes share a mesh anymore, each is its
                    // own dough-deformed copy of the base sphere. See KneadClayLobeMesh.
                    filter.sharedMesh = KneadClayLobeMesh(sphere, intensity01: 0.15f, roundIterations: 4);

                    var renderer = go.AddComponent<MeshRenderer>();
                    renderer.sharedMaterial = clay;
                    // Shadows off — cast shadow blacked out the board (image copy 14); self-receive
                    // carved harsh crevices between lobes.
                    renderer.shadowCastingMode = ShadowCastingMode.Off;
                    renderer.receiveShadows = false;

                    var block = new MaterialPropertyBlock();
                    Color tint = lobe.Belly ? bellyTint : topTint;
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

            // Just outside the opaque lobe cluster, not inside it — tight enough that puffs read as
            // a blurred extension of the mass rather than separate floating dots.
            Vector3 envelope = massScale * 1.05f;
            float footprint = Mathf.Max(0.01f, envelope.x * envelope.z);
            int maxCount = Mathf.Clamp(Mathf.RoundToInt(footprint * 0.9f), 16, 30);
            float baseSize = Mathf.Sqrt(footprint / maxCount) * 2.2f;

            var main = ps.main;
            main.loop = true;
            main.playOnAwake = true;
            main.prewarm = true;
            main.duration = 8f;
            main.startLifetime = new ParticleSystem.MinMaxCurve(6f, 9.5f);
            main.startSpeed = 0f;
            main.startSize = new ParticleSystem.MinMaxCurve(baseSize * 0.75f, baseSize * 1.25f);
            main.startRotation = new ParticleSystem.MinMaxCurve(0f, 360f * Mathf.Deg2Rad);
            // Denser than the first pass (0.14-0.30) — human 2026-08-13: haze read too see-through.
            main.startColor = new ParticleSystem.MinMaxGradient(
                new Color(tint.r, tint.g, tint.b, 0.34f),
                new Color(tint.r, tint.g, tint.b, 0.58f));
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

        /// <summary>Vertex adjacency (shared triangle topology, same for every kneaded lobe since they
        /// all start from <see cref="UnitSphereMesh"/>) — built once, reused by every <see
        /// cref="KneadClayLobeMesh"/> call for the rounding pass.</summary>
        private static int[][] _sphereAdjacency;

        /// <summary>
        /// "揉面团" (kneading dough) — human 2026-08-13: the puffs were still reading as spheres even
        /// after the triangular-formation pass, because every lobe literally *was* an unmodified sphere
        /// primitive. This deforms a private copy of the base sphere per lobe: squeeze (pinch two
        /// opposite sides — bulges the waist between them), press (one broad soft dent), push (one
        /// broad soft bulge), pound (one small sharp dent — the "pointy edges"), all the same radial
        /// "dent" primitive at different center-counts/radii/strengths. Then a rounding pass (partial
        /// Laplacian relax over the shared topology) softens those sharp transitions into curves —
        /// "round the edges... like a sandbag" — and a final uniform rescale corrects the size drift
        /// relaxation causes, so kneading never desyncs a lobe from its tuned on-screen <c>RadiusNorm</c>.
        /// One unique mesh per lobe (not shared) — see <see cref="DestroyKneadedLobeMeshes"/> for cleanup.
        ///
        /// First pass (<c>image copy 15</c>, intensity 0.24) read as "shattered glass," not dough. Two
        /// causes, both fixed here: (1) UV stayed pinned to each vertex's pre-deform latitude while the
        /// dents moved it well away from the height that latitude implied, so the posterized crown/belly
        /// bands landed as scattered light/dark patches instead of a coherent gradient — UV.y is now
        /// re-derived from each vertex's actual post-knead height. (2) Dents were too strong/sharp for
        /// this mesh's resolution, creasing visibly — intensity dropped (0.24→0.15), falloffs widened,
        /// and the round pass strengthened (3→4 iterations, 0.45→0.55 blend).
        /// </summary>
        private static Mesh KneadClayLobeMesh(Mesh baseMesh, float intensity01, int roundIterations)
        {
            Vector3[] baseVerts = baseMesh.vertices;
            Vector2[] baseUV = baseMesh.uv;
            if (_sphereAdjacency == null)
            {
                _sphereAdjacency = BuildVertexAdjacency(baseMesh);
                _baseVMin = float.MaxValue;
                _baseVMax = float.MinValue;
                for (int i = 0; i < baseUV.Length; i++)
                {
                    _baseVMin = Mathf.Min(_baseVMin, baseUV[i].y);
                    _baseVMax = Mathf.Max(_baseVMax, baseUV[i].y);
                }
            }

            float targetAvgRadius = AverageRadius(baseVerts);
            float strength = targetAvgRadius * intensity01;
            Vector3[] verts = (Vector3[])baseVerts.Clone();

            // Squeeze — pinch two opposite sides inward; bulges the waist between them.
            Vector3 squeezeAxis = Random.onUnitSphere;
            Dent(verts, baseVerts, squeezeAxis, -strength * Random.Range(0.4f, 0.7f), RandomFalloffAngle(55f, 85f));
            Dent(verts, baseVerts, -squeezeAxis, -strength * Random.Range(0.4f, 0.7f), RandomFalloffAngle(55f, 85f));

            // Press — one broad soft dent.
            Dent(verts, baseVerts, Random.onUnitSphere, -strength * Random.Range(0.5f, 0.8f), RandomFalloffAngle(50f, 75f));

            // Push — one broad soft bulge.
            Dent(verts, baseVerts, Random.onUnitSphere, strength * Random.Range(0.5f, 0.8f), RandomFalloffAngle(50f, 75f));

            // Pound — one small, sharp dent (rounding below softens this into the "pointy edge").
            Dent(verts, baseVerts, Random.onUnitSphere, -strength * Random.Range(0.7f, 1.0f), RandomFalloffAngle(30f, 45f));

            // Round — partial relax so lumps survive; full Laplacian would erase them. More iterations
            // / stronger blend than the first pass (image copy 15 read as jagged shattered glass, not
            // soft dough) to smooth the coarse-mesh creasing the dents leave behind.
            for (int s = 0; s < roundIterations; s++)
            {
                var relaxed = new Vector3[verts.Length];
                for (int i = 0; i < verts.Length; i++)
                {
                    int[] neighbors = _sphereAdjacency[i];
                    if (neighbors.Length == 0)
                    {
                        relaxed[i] = verts[i];
                        continue;
                    }

                    Vector3 avg = Vector3.zero;
                    for (int n = 0; n < neighbors.Length; n++)
                    {
                        avg += verts[neighbors[n]];
                    }

                    avg /= neighbors.Length;
                    relaxed[i] = Vector3.Lerp(verts[i], avg, 0.55f);
                }

                verts = relaxed;
            }

            // Relaxing shrinks a mesh toward its centroid — rescale back to the original average
            // radius so the lobe's tuned RadiusNorm/diameter still lands on-screen as intended.
            float currentAvgRadius = AverageRadius(verts);
            float correction = currentAvgRadius > 0.0001f ? targetAvgRadius / currentAvgRadius : 1f;
            for (int i = 0; i < verts.Length; i++)
            {
                verts[i] *= correction;
            }

            // Re-derive V (crown/belly band) from each vertex's ACTUAL post-knead height, not its
            // pre-deform latitude. image copy 15's shattered-glass look was this: UV stayed pinned to
            // the original sphere's latitude while kneading moved vertices well away from the height
            // that latitude implied, so the posterized bands landed as scattered light/dark patches
            // instead of a coherent crown-to-belly gradient over the new shape. U is left alone (the
            // shade map only varies a little by U, for the edge vignette).
            float minY = float.MaxValue;
            float maxY = float.MinValue;
            for (int i = 0; i < verts.Length; i++)
            {
                minY = Mathf.Min(minY, verts[i].y);
                maxY = Mathf.Max(maxY, verts[i].y);
            }

            var uv = new Vector2[verts.Length];
            for (int i = 0; i < verts.Length; i++)
            {
                float t = maxY > minY ? Mathf.InverseLerp(minY, maxY, verts[i].y) : 0.5f;
                uv[i] = new Vector2(baseUV[i].x, Mathf.Lerp(_baseVMin, _baseVMax, t));
            }

            var mesh = new Mesh { name = "ClayLobeDough" };
            mesh.vertices = verts;
            mesh.triangles = baseMesh.triangles;
            mesh.uv = uv;
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
            return mesh;
        }

        private static float _baseVMin;
        private static float _baseVMax;

        /// <summary>One radial displacement: vertices within <paramref name="falloffAngle"/> of
        /// <paramref name="center"/> (angular distance on the base sphere) move along their own base
        /// direction by <paramref name="strength"/>, smoothstep-falling to zero at the edge of influence.
        /// Negative strength dents inward (squeeze/press/pound), positive bulges outward (push).</summary>
        private static void Dent(Vector3[] verts, Vector3[] baseVerts, Vector3 center, float strength, float falloffAngle)
        {
            Vector3 axis = center.normalized;
            for (int i = 0; i < verts.Length; i++)
            {
                Vector3 dir = baseVerts[i].normalized;
                float angle = Mathf.Acos(Mathf.Clamp(Vector3.Dot(dir, axis), -1f, 1f));
                if (angle >= falloffAngle)
                {
                    continue;
                }

                float t = 1f - (angle / falloffAngle);
                float falloff = t * t * (3f - 2f * t);
                verts[i] += dir * strength * falloff;
            }
        }

        private static float RandomFalloffAngle(float minDegrees, float maxDegrees) =>
            Random.Range(minDegrees, maxDegrees) * Mathf.Deg2Rad;

        private static float AverageRadius(Vector3[] verts)
        {
            float sum = 0f;
            for (int i = 0; i < verts.Length; i++)
            {
                sum += verts[i].magnitude;
            }

            return verts.Length > 0 ? sum / verts.Length : 1f;
        }

        private static int[][] BuildVertexAdjacency(Mesh mesh)
        {
            Vector3[] verts = mesh.vertices;
            int[] tris = mesh.triangles;
            var neighborSets = new HashSet<int>[verts.Length];
            for (int i = 0; i < verts.Length; i++)
            {
                neighborSets[i] = new HashSet<int>();
            }

            for (int t = 0; t < tris.Length; t += 3)
            {
                int a = tris[t];
                int b = tris[t + 1];
                int c = tris[t + 2];
                neighborSets[a].Add(b);
                neighborSets[a].Add(c);
                neighborSets[b].Add(a);
                neighborSets[b].Add(c);
                neighborSets[c].Add(a);
                neighborSets[c].Add(b);
            }

            var adjacency = new int[verts.Length][];
            for (int i = 0; i < verts.Length; i++)
            {
                adjacency[i] = new int[neighborSets[i].Count];
                neighborSets[i].CopyTo(adjacency[i]);
            }

            return adjacency;
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
        /// Soft Unlit clay for sphere lobes. Lit diffuse was darkening sphere limbs into mid-grey
        /// "边缘" against the void (image copy 14). Painted vertical shade map = bright crown /
        /// pale belly without a harsh terminator; no cast shadows so the board stays readable.
        /// The first Unlit pass (image copy 15) baked too weak a gradient (~231-254 of 255) — read
        /// as one flat blown-out white mass with no per-lobe volume. Shade map redrawn with real
        /// crown/belly contrast (~152-255) so overlapping lobes read as separate glued pillows again.
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
