using UnityEngine;

namespace LogiCard.Board
{
    /// <summary>
    /// Wet-dusk surface materials for the C53 environment detail pass. Prefers Poly Haven CC0
    /// textures from <c>Resources/BoardSurfaces/</c> (see Art/Environment/THIRD_PARTY.md); falls
    /// back to procedural tints if those assets are missing so EditMode tests still compile/run.
    /// </summary>
    internal static class BoardSurfaceMaterials
    {
        private static Material _yard;
        private static Material _hall;
        private static Material _vault;
        private static Material _flank;
        private static Material _wall;
        private static Material _woodEdge;
        private static Material _strataRock;
        private static Material _strataDirt;
        private static Material _strataGrass;
        private static Material _propCrate;
        private static Material _propMetal;
        private static Material _warmGlass;

        public static Material YardFloor => _yard ??= BuildWetSurface(
            "asphalt_diff", "asphalt_nor", "asphalt_rough",
            fallback: new Color(0.18f, 0.19f, 0.22f),
            wetSmoothnessBoost: 0.55f,
            tint: new Color(0.55f, 0.60f, 0.68f),
            tile: 2.2f);

        public static Material HallFloor => _hall ??= BuildWetSurface(
            "concrete_diff", "concrete_nor", "concrete_rough",
            fallback: new Color(0.42f, 0.40f, 0.38f),
            wetSmoothnessBoost: 0.28f,
            tint: new Color(0.78f, 0.74f, 0.68f),
            tile: 1.8f);

        public static Material VaultFloor => _vault ??= BuildWetSurface(
            "concrete_diff", "concrete_nor", "concrete_rough",
            fallback: new Color(0.28f, 0.30f, 0.34f),
            wetSmoothnessBoost: 0.62f,
            tint: new Color(0.45f, 0.50f, 0.58f),
            tile: 2.4f);

        public static Material FlankFloor => _flank ??= BuildWetSurface(
            "asphalt_diff", "asphalt_nor", "asphalt_rough",
            fallback: new Color(0.22f, 0.20f, 0.18f),
            wetSmoothnessBoost: 0.35f,
            tint: new Color(0.48f, 0.44f, 0.40f),
            tile: 2.0f);

        public static Material BrickWall => _wall ??= BuildWetSurface(
            "brick_diff", "brick_nor", "brick_rough",
            fallback: new Color(0.42f, 0.34f, 0.30f),
            wetSmoothnessBoost: 0.12f,
            tint: new Color(0.85f, 0.78f, 0.72f),
            tile: 1.4f);

        public static Material WoodEdge => _woodEdge ??= BuildWetSurface(
            "wood_diff", "wood_nor", "wood_rough",
            fallback: new Color(0.55f, 0.42f, 0.28f),
            wetSmoothnessBoost: 0.18f,
            tint: Color.white,
            tile: 1.1f);

        public static Material StrataRock => _strataRock ??= Solid(new Color(0.38f, 0.36f, 0.34f), 0.12f);

        public static Material StrataDirt => _strataDirt ??= Solid(new Color(0.28f, 0.20f, 0.12f), 0.08f);

        public static Material StrataGrass => _strataGrass ??= Solid(new Color(0.22f, 0.32f, 0.16f), 0.1f);

        public static Material PropCrate => _propCrate ??= BuildWetSurface(
            "wood_diff", "wood_nor", "wood_rough",
            fallback: new Color(0.48f, 0.36f, 0.22f),
            wetSmoothnessBoost: 0.15f,
            tint: new Color(0.9f, 0.82f, 0.7f),
            tile: 0.8f);

        public static Material PropMetal => _propMetal ??= Solid(new Color(0.35f, 0.38f, 0.42f), 0.45f, metallic: 0.55f);

        public static Material WarmGlass => _warmGlass ??= Emissive(new Color(1f, 0.72f, 0.38f), 2.4f);

        private static Material BuildWetSurface(
            string diffName,
            string norName,
            string roughName,
            Color fallback,
            float wetSmoothnessBoost,
            Color tint,
            float tile)
        {
            Texture2D diff = Load(diffName);
            Texture2D nor = Load(norName);
            Texture2D rough = Load(roughName);

            Material mat = NewLit();
            if (diff != null)
            {
                ApplyColor(mat, tint);
                mat.SetTexture("_BaseMap", diff);
                mat.SetTextureScale("_BaseMap", new Vector2(tile, tile));
            }
            else
            {
                ApplyColor(mat, fallback);
            }

            if (nor != null && mat.HasProperty("_BumpMap"))
            {
                mat.SetTexture("_BumpMap", nor);
                mat.SetTextureScale("_BumpMap", new Vector2(tile, tile));
                mat.EnableKeyword("_NORMALMAP");
                if (mat.HasProperty("_BumpScale"))
                {
                    mat.SetFloat("_BumpScale", 1.1f);
                }
            }

            // URP Lit: roughness map isn't a first-class slot on every version — bake wetness into
            // Smoothness and darken slightly so rain reads on the Yard/Vault without a custom shader.
            float baseSmooth = rough != null ? 0.22f : 0.15f;
            SetSmoothness(mat, Mathf.Clamp01(baseSmooth + wetSmoothnessBoost));
            SetMetallic(mat, 0f);
            return mat;
        }

        private static Material Solid(Color color, float smoothness, float metallic = 0f)
        {
            Material mat = NewLit();
            ApplyColor(mat, color);
            SetSmoothness(mat, smoothness);
            SetMetallic(mat, metallic);
            return mat;
        }

        private static Material Emissive(Color color, float intensity)
        {
            Material mat = NewLit();
            ApplyColor(mat, color);
            SetSmoothness(mat, 0.65f);
            SetMetallic(mat, 0f);
            if (mat.HasProperty("_EmissionColor"))
            {
                mat.EnableKeyword("_EMISSION");
                mat.SetColor("_EmissionColor", color * intensity);
                mat.globalIlluminationFlags = MaterialGlobalIlluminationFlags.RealtimeEmissive;
            }

            return mat;
        }

        private static Material NewLit()
        {
            var lit = Shader.Find("Universal Render Pipeline/Lit");
            if (lit != null)
            {
                return new Material(lit);
            }

            var unlit = Shader.Find("Universal Render Pipeline/Unlit");
            if (unlit != null)
            {
                return new Material(unlit);
            }

            return new Material(Shader.Find("Standard") ?? Shader.Find("Sprites/Default"));
        }

        private static Texture2D Load(string resourceName)
        {
            return Resources.Load<Texture2D>("BoardSurfaces/" + resourceName);
        }

        private static void ApplyColor(Material material, Color color)
        {
            material.color = color;
            if (material.HasProperty("_BaseColor"))
            {
                material.SetColor("_BaseColor", color);
            }
        }

        private static void SetSmoothness(Material material, float smoothness)
        {
            if (material.HasProperty("_Smoothness"))
            {
                material.SetFloat("_Smoothness", smoothness);
            }

            if (material.HasProperty("_Glossiness"))
            {
                material.SetFloat("_Glossiness", smoothness);
            }
        }

        private static void SetMetallic(Material material, float metallic)
        {
            if (material.HasProperty("_Metallic"))
            {
                material.SetFloat("_Metallic", metallic);
            }
        }
    }
}
