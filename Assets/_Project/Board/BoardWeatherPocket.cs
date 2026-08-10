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
        /// TEMPORARY interim sizing (2026-08-10) — these primitive-sphere puffs are being replaced by
        /// real cloud mesh assets (see ENV_LOOKFEEL_AGENT_BRIEF.md); not worth precision-tuning
        /// geometry that's on its way out. The original sizing was tuned before the HUD dock's
        /// right-edge -> bottom-band move changed the camera's effective zoom/aspect (orthographicSize
        /// 9.0 -> 5.0 plus a much wider board-region aspect); a ~9-10 unit-wide sphere sitting only
        /// ~3 units above a 10-unit-deep board, viewed through a camera that's now effectively much
        /// closer, loomed over the entire board (confirmed via a human screenshot — playtest
        /// 2026-08-10). Shrunk ~45% and pushed further back/up as an immediate unblock, not a final
        /// pass — proper sizing happens once real assets land.
        /// </summary>
        private const float InterimCloudScale = 0.55f;
        private const float InterimCloudHeightBoost = 2.1f;

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

        private static void PlaceCloudPuff(
            Transform parent,
            string name,
            Vector3 localPosition,
            Vector3 localScale,
            Color tint)
        {
            var puff = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            puff.name = name;
            puff.transform.SetParent(parent, false);
            puff.transform.localPosition = localPosition;
            puff.transform.localScale = localScale;
            puff.GetComponent<MeshRenderer>().sharedMaterial = CloudMaterial(tint);
            StripCollider(puff);
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

        private static Material CloudMaterial(Color tint)
        {
            // Shared shader template; each puff gets its own tinted instance so under-lit fringes
            // can run warmer than the storm ceiling without fighting a single shared color.
            Material mat = new Material(CloudTemplate)
            {
                color = tint,
            };
            if (mat.HasProperty("_BaseColor"))
            {
                mat.SetColor("_BaseColor", tint);
            }

            if (mat.HasProperty("_Smoothness"))
            {
                mat.SetFloat("_Smoothness", 0.08f);
            }

            if (mat.HasProperty("_Metallic"))
            {
                mat.SetFloat("_Metallic", 0f);
            }

            return mat;
        }

        private static Material CloudTemplate
        {
            get
            {
                if (_cloudMaterial != null)
                {
                    return _cloudMaterial;
                }

                var lit = Shader.Find("Universal Render Pipeline/Lit");
                _cloudMaterial = lit != null
                    ? new Material(lit)
                    : new Material(Shader.Find("Sprites/Default") ?? Shader.Find("Standard"));
                return _cloudMaterial;
            }
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

        private static void StripCollider(GameObject go)
        {
            Collider col = go.GetComponent<Collider>();
            if (col != null)
            {
                Destroy(col);
            }
        }
    }
}
