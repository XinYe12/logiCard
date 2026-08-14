using UnityEngine;

namespace LogiCard.Board
{
    /// <summary>
    /// Board surface materials (C65 / Map Phase 2). Room floors, walls, door-tint base, and prop tint
    /// use the flat/gradient toon family (<see cref="Solid"/> + nappin <c>(Mat)Gradient*_URP</c>).
    /// Photographic <see cref="BuildWetSurface"/> stays in this file for non-board callers but is no
    /// longer the default path for <see cref="MapSurfaceRole"/> surfaces.
    /// </summary>
    internal static class BoardSurfaceMaterials
    {
        private static Material _yard;
        private static Material _hall;
        private static Material _vault;
        private static Material _flank;
        private static Material _wall;
        private static Material _fenceRail;
        private static Material _fencePost;
        private static Material _woodEdge;
        private static Material _strataRock;
        private static Material _strataDirt;
        private static Material _strataGrass;
        private static Material _propCrate;
        private static Material _propMetal;
        private static Material _warmGlass;
        private static Material _doorLeaf;
        private static Material _propBody;
        private static Material _propAccent;

        // C65: saturated toy-diorama albedos (Link's Awakening / soft clay), low smoothness — color at
        // the material, not via a third grading pass on photographic PBR (see MAP_PRESENTATION_STANDARD §1).
        public static Material YardFloor => _yard ??= Solid(new Color(0.94f, 0.78f, 0.48f), 0.12f);

        public static Material HallFloor => _hall ??= Solid(new Color(0.90f, 0.68f, 0.42f), 0.14f);

        public static Material VaultFloor => _vault ??= Solid(new Color(0.78f, 0.88f, 0.96f), 0.22f);

        public static Material FlankFloor => _flank ??= Solid(new Color(0.62f, 0.78f, 0.48f), 0.10f);

        /// <summary>Legacy name — cream plaster panel fill between fence posts (was coral brick slab).</summary>
        public static Material BrickWall => _wall ??= Solid(new Color(0.96f, 0.90f, 0.78f), 0.10f);

        /// <summary>Horizontal fence rails — warm honey wood matching sandy Yard floors.</summary>
        public static Material FenceRail => _fenceRail ??= Solid(new Color(0.86f, 0.62f, 0.34f), 0.14f);

        /// <summary>Vertical fence posts — slightly darker wood so posts read against rails.</summary>
        public static Material FencePost => _fencePost ??= Solid(new Color(0.62f, 0.40f, 0.22f), 0.12f);

        public static Material WoodEdge => _woodEdge ??= Solid(new Color(0.72f, 0.48f, 0.28f), 0.16f);

        public static Material StrataRock => _strataRock ??= Solid(new Color(0.38f, 0.36f, 0.34f), 0.12f);

        public static Material StrataDirt => _strataDirt ??= Solid(new Color(0.34f, 0.35f, 0.38f), 0.08f);

        public static Material StrataGrass => _strataGrass ??= Solid(new Color(0.22f, 0.32f, 0.16f), 0.1f);

        public static Material PropCrate => _propCrate ??= Solid(new Color(0.86f, 0.62f, 0.36f), 0.14f);

        public static Material PropMetal => _propMetal ??= Solid(new Color(0.42f, 0.48f, 0.58f), 0.35f, metallic: 0.35f);

        public static Material WarmGlass => _warmGlass ??= Emissive(new Color(1f, 0.72f, 0.38f), 2.4f);

        /// <summary>Door leaf base before open/closed state tint — nappin GradientBrown when present.</summary>
        public static Material DoorLeaf => _doorLeaf ??= LoadInteriorGradient("GradientBrown")
            ?? Solid(new Color(0.78f, 0.55f, 0.32f), 0.18f);

        /// <summary>Default opaque prop body (cabinets/shelves/tables).</summary>
        public static Material PropBody => _propBody ??= LoadInteriorGradient("GradientBeige")
            ?? PropCrate;

        /// <summary>Secondary prop accent (chairs / small furniture).</summary>
        public static Material PropAccent => _propAccent ??= LoadInteriorGradient("GradientBlue")
            ?? Solid(new Color(0.45f, 0.62f, 0.88f), 0.16f);

        /// <summary>Role → flat/toon floor material (C65 default path).</summary>
        public static Material ForRole(MapSurfaceRole role)
        {
            switch (role)
            {
                case MapSurfaceRole.Yard:
                    return YardFloor;
                case MapSurfaceRole.Hall:
                    return HallFloor;
                case MapSurfaceRole.Vault:
                    return VaultFloor;
                default:
                    return FlankFloor;
            }
        }

        /// <summary>
        /// True when the material has no photographic diffuse map — the C65 flat/toon family signature.
        /// Gradient*_URP assets keep a soft ramp texture (still non-photo); those count as flat family
        /// via <paramref name="allowGradientRamp"/>.
        /// </summary>
        public static bool IsFlatFamily(Material material, bool allowGradientRamp = true)
        {
            if (material == null)
            {
                return false;
            }

            if (!material.HasProperty("_BaseMap"))
            {
                return true;
            }

            Texture map = material.GetTexture("_BaseMap");
            if (map == null)
            {
                return true;
            }

            if (!allowGradientRamp)
            {
                return false;
            }

            string name = material.name ?? string.Empty;
            return name.IndexOf("Gradient", System.StringComparison.OrdinalIgnoreCase) >= 0;
        }

        /// <summary>
        /// Kept for non-board / revert paths. Board room surfaces no longer call this (C65).
        /// </summary>
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

        private static Material LoadInteriorGradient(string gradientName)
        {
            // Already duplicated+URP-converted by InteriorPackImportTool into Resources/Interior/Materials.
            Material source = Resources.Load<Material>($"Interior/Materials/(Mat){gradientName}_URP");
            if (source == null)
            {
                return null;
            }

            // Instance so we can drop sheen without mutating the shared import asset.
            Material mat = new Material(source);
            SetSmoothness(mat, 0.16f);
            SetMetallic(mat, 0f);
            if (mat.HasProperty("_BumpMap"))
            {
                mat.SetTexture("_BumpMap", null);
                mat.DisableKeyword("_NORMALMAP");
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
