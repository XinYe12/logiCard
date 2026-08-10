using LogiCard.Sim;
using UnityEngine;
using UnityEngine.Rendering;

namespace LogiCard.Board
{
    /// <summary>
    /// Contained stormy sky pocket above the floating board chunk (C53 / ART_DIRECTION Moodboard).
    ///
    /// Not a skybox and not an infinite horizon — camera clear flags stay SolidColor (dark void).
    /// Clouds and rain are real scene geometry sized to the board footprint so weather reads as
    /// sitting on the diorama, matching the locked reference's "sky pocket over the chunk."
    /// </summary>
    public sealed class BoardWeatherPocket : MonoBehaviour
    {
        private static Material _cloudMaterial;
        private static Material _rainMaterial;

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
        }

        /// <summary>
        /// Cloud scale/height correction, tuned 2026-08-09 and carried forward unchanged into the
        /// real cloud-particle rework (2026-08-10, see <see cref="PlaceCloudPuff"/>). The sizing was
        /// tuned after the HUD dock's right-edge -> bottom-band move changed the camera's effective
        /// zoom/aspect (orthographicSize 9.0 -> 5.0 plus a much wider board-region aspect); a ~9-10
        /// unit-wide puff sitting only ~3 units above a 10-unit-deep board, viewed through a camera
        /// that's now effectively much closer, loomed over the entire board (confirmed via a human
        /// screenshot — playtest 2026-08-10). Shrunk ~45% and pushed further back/up. This correction
        /// is orthogonal to the rendering technique (primitive spheres before, textured particle
        /// billboards now) — both consume the same puff bounding boxes below, so keeping these
        /// factors is what keeps the "contained pocket, not looming over the board" framing correct
        /// (`docs/ART_DIRECTION.md` Moodboard) regardless of what draws inside each puff volume.
        /// </summary>
        private const float InterimCloudScale = 0.55f;
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
        /// instead of a single tinted sphere primitive. Several depth-sorted, randomly-framed cloud
        /// sprites overlapping within the puff's bounding box read as a soft volumetric mass — the
        /// wispy alpha silhouette from the source texture (see <c>THIRD_PARTY.md</c>) does the work a
        /// hard-edged sphere outline never could. <paramref name="localPosition"/>/<paramref
        /// name="localScale"/> reuse the exact placement/sizing the sphere puffs used, so the overall
        /// cloud-bank layout and the <see cref="InterimCloudScale"/>/<see
        /// cref="InterimCloudHeightBoost"/> framing correction are unaffected by this swap.
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

            // Each particle keeps a fixed random frame from the cloud atlas (see
            // Resources/Weather/CloudAtlas.png) for its whole lifetime — frameOverTime stays at a flat
            // 0 so it never animates/flipbooks, it's purely per-particle shape variety at spawn.
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
            var rainGo = new GameObject("Rain");
            rainGo.transform.SetParent(transform, false);
            // Emit just under the cloud shelf so streaks read as falling out of the pocket. Boosted
            // by the same interim factor as the clouds so the gap between them stays proportional.
            rainGo.transform.localPosition = new Vector3(0f, 2.85f * InterimCloudHeightBoost, 0f);

            var ps = rainGo.AddComponent<ParticleSystem>();
            ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

            var main = ps.main;
            main.loop = true;
            main.playOnAwake = true;
            main.duration = 5f;
            main.startLifetime = new ParticleSystem.MinMaxCurve(0.55f, 0.85f);
            main.startSpeed = new ParticleSystem.MinMaxCurve(5.5f, 7.5f);
            main.startSize3D = true;
            main.startSizeX = new ParticleSystem.MinMaxCurve(0.012f, 0.02f);
            main.startSizeY = new ParticleSystem.MinMaxCurve(0.18f, 0.32f);
            main.startSizeZ = new ParticleSystem.MinMaxCurve(0.012f, 0.02f);
            main.startColor = new Color(0.72f, 0.78f, 0.88f, 0.42f);
            main.maxParticles = 2500;
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            main.gravityModifier = 0.35f;
            main.scalingMode = ParticleSystemScalingMode.Hierarchy;

            var emission = ps.emission;
            emission.rateOverTime = 700f;

            var shape = ps.shape;
            shape.enabled = true;
            shape.shapeType = ParticleSystemShapeType.Box;
            // Slightly inset so rain stays over the playable face, not the void apron.
            shape.scale = new Vector3(width * 0.95f, 0.15f, depth * 0.95f);
            shape.position = Vector3.zero;
            shape.rotation = Vector3.zero;

            var velocity = ps.velocityOverLifetime;
            velocity.enabled = true;
            velocity.space = ParticleSystemSimulationSpace.World;
            // All three axes must share the same MinMaxCurve mode — Unity errors otherwise
            // ("Particle Velocity curves must all be in the same mode") and PlayMode tests treat
            // that Error log as a failure.
            velocity.x = new ParticleSystem.MinMaxCurve(-0.6f, -0.2f);
            velocity.y = new ParticleSystem.MinMaxCurve(-2.5f, -1.5f);
            velocity.z = new ParticleSystem.MinMaxCurve(0f, 0f);

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

            var renderer = rainGo.GetComponent<ParticleSystemRenderer>();
            renderer.renderMode = ParticleSystemRenderMode.Stretch;
            renderer.lengthScale = 1.8f;
            renderer.velocityScale = 0.06f;
            renderer.sharedMaterial = RainMaterial();
            renderer.shadowCastingMode = ShadowCastingMode.Off;
            renderer.receiveShadows = false;

            ps.Play(true);
        }

        /// <summary>
        /// Single shared material for every cloud puff's particle billboards — per-puff tinting now
        /// happens via each particle's start color (see <see cref="PlaceCloudPuff"/>), not a separate
        /// material instance per puff, since the URP particle-unlit shader multiplies the atlas texture
        /// by vertex/particle color by default. Same fallback shader chain as <see cref="RainMaterial"/>
        /// so both stay renderable if URP shader variants aren't stripped-in the same way.
        /// </summary>
        private static Material CloudMaterial()
        {
            if (_cloudMaterial != null)
            {
                return _cloudMaterial;
            }

            var particles = Shader.Find("Universal Render Pipeline/Particles/Unlit")
                ?? Shader.Find("Particles/Standard Unlit")
                ?? Shader.Find("Sprites/Default");
            _cloudMaterial = new Material(particles);

            Texture2D atlas = Resources.Load<Texture2D>("Weather/CloudAtlas");
            if (atlas != null)
            {
                _cloudMaterial.mainTexture = atlas;
                if (_cloudMaterial.HasProperty("_BaseMap"))
                {
                    _cloudMaterial.SetTexture("_BaseMap", atlas);
                }
            }

            ConfigureAlphaBlend(_cloudMaterial);
            return _cloudMaterial;
        }

        /// <summary>
        /// A URP Lit/Unlit/Particle material created via <c>new Material(shader)</c> defaults to
        /// **Opaque** surface type — it does not infer transparency from the texture's alpha channel
        /// on its own. Without this, <see cref="CloudMaterial"/>'s atlas (real alpha-cutout cloud
        /// puffs on transparent padding) rendered every particle as a hard-edged, fully opaque
        /// rectangle — the transparent padding read as solid black instead of disappearing (confirmed
        /// via a human screenshot, 2026-08-10: "black rectangular blocks," not soft clouds). Mirrors
        /// what URP's ShaderGUI sets when a human picks "Transparent" + "Alpha" in the Inspector.
        ///
        /// SECOND BUG, found from a follow-up screenshot after the first attempt still looked broken:
        /// this originally set <c>_ALPHABLEND_ON</c>, which does not exist as a keyword on
        /// "Universal Render Pipeline/Particles/Unlit" — confirmed directly against the shader source
        /// in Library/PackageCache (com.unity.render-pipelines.universal@17.5.0's
        /// Shaders/Particles/ParticlesUnlit.shader): its surface-type keyword is
        /// <c>_SURFACE_TYPE_TRANSPARENT</c>, exactly the same one URP Lit uses (see
        /// InteriorPackImportTool.FixGlassTransparency, which hit and fixed the identical mistake on a
        /// *different* shader first). The numeric Blend/ZWrite properties were already correct and
        /// drove some real GPU state change, which is presumably why this read as "still broken" rather
        /// than "identical to before," but the fragment shader's own surface-type branch never flipped.
        /// </summary>
        private static void ConfigureAlphaBlend(Material material)
        {
            material.SetOverrideTag("RenderType", "Transparent");
            if (material.HasProperty("_Surface"))
            {
                material.SetFloat("_Surface", 1f); // 1 = Transparent
            }

            if (material.HasProperty("_Blend"))
            {
                material.SetFloat("_Blend", 0f); // 0 = Alpha
            }

            if (material.HasProperty("_SrcBlend"))
            {
                material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            }

            if (material.HasProperty("_DstBlend"))
            {
                material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            }

            if (material.HasProperty("_ZWrite"))
            {
                material.SetInt("_ZWrite", 0);
            }

            material.DisableKeyword("_ALPHATEST_ON");
            material.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
            material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
            material.DisableKeyword("_ALPHAMODULATE_ON");
            material.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;
        }

        private static Material RainMaterial()
        {
            if (_rainMaterial != null)
            {
                return _rainMaterial;
            }

            var particles = Shader.Find("Universal Render Pipeline/Particles/Unlit");
            if (particles == null)
            {
                particles = Shader.Find("Particles/Standard Unlit");
            }

            if (particles == null)
            {
                particles = Shader.Find("Sprites/Default");
            }

            _rainMaterial = new Material(particles);
            var rainTint = new Color(0.75f, 0.82f, 0.92f, 0.55f);
            _rainMaterial.color = rainTint;
            if (_rainMaterial.HasProperty("_BaseColor"))
            {
                _rainMaterial.SetColor("_BaseColor", rainTint);
            }

            return _rainMaterial;
        }
    }
}
