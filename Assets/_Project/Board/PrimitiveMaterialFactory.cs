using UnityEngine;

namespace LogiCard.Board
{
    /// <summary>
    /// Makes tinted materials for the primitive placeholder art.
    ///
    /// Prefers URP Lit (or Unlit) so player builds stay valid after the Day 8 pipeline
    /// migration. Falls back to cloning a CreatePrimitive default material if URP
    /// shaders are unavailable (Built-in RP or missing package).
    /// </summary>
    internal static class PrimitiveMaterialFactory
    {
        private static Material _template;
        private static Texture2D _clayGrain;

        public static Material Tinted(Color color)
        {
            var material = new Material(Template);
            ApplyColor(material, color);
            ApplyMatte(material);
            ApplyClayGrain(material);
            return material;
        }

        private static Material Template
        {
            get
            {
                if (_template != null)
                {
                    return _template;
                }

                var urpLit = Shader.Find("Universal Render Pipeline/Lit");
                if (urpLit != null)
                {
                    _template = new Material(urpLit);
                    ApplyMatte(_template);
                    return _template;
                }

                var urpUnlit = Shader.Find("Universal Render Pipeline/Unlit");
                if (urpUnlit != null)
                {
                    _template = new Material(urpUnlit);
                    return _template;
                }

                var probe = GameObject.CreatePrimitive(PrimitiveType.Cube);
                probe.hideFlags = HideFlags.HideAndDontSave;
                _template = probe.GetComponent<MeshRenderer>().sharedMaterial;
                Object.DestroyImmediate(probe);
                return _template;
            }
        }

        private static void ApplyColor(Material material, Color color)
        {
            material.color = color;
            if (material.HasProperty("_BaseColor"))
            {
                material.SetColor("_BaseColor", color);
            }
        }

        private static void ApplyMatte(Material material)
        {
            // A little more than dead-flat (was 0.05): clay/polymer has a faint soft sheen under a
            // key light, not zero specular response — pure 0.05 read as "unlit cardboard" (playtest
            // 2026-08-07: "too plain and dull").
            if (material.HasProperty("_Smoothness"))
            {
                material.SetFloat("_Smoothness", 0.18f);
            }

            if (material.HasProperty("_Glossiness"))
            {
                material.SetFloat("_Glossiness", 0.18f);
            }

            if (material.HasProperty("_Metallic"))
            {
                material.SetFloat("_Metallic", 0f);
            }
        }

        /// <summary>
        /// ART_DIRECTION Demo art floor: "clay-tint / matte polymer look with subtle procedural
        /// noise." A flat tint alone reads as plastic/default-Unity; a faint tiled grain sells
        /// hand-worked imperfection without needing an authored texture asset.
        /// </summary>
        private static void ApplyClayGrain(Material material)
        {
            if (!material.HasProperty("_BaseMap"))
            {
                return;
            }

            material.SetTexture("_BaseMap", ClayGrain);
            material.SetTextureScale("_BaseMap", new Vector2(3f, 3f));
        }

        private static Texture2D ClayGrain
        {
            get
            {
                if (_clayGrain != null)
                {
                    return _clayGrain;
                }

                const int size = 64;
                var tex = new Texture2D(size, size, TextureFormat.R8, false, true)
                {
                    name = "ClayGrain",
                    wrapMode = TextureWrapMode.Repeat,
                    filterMode = FilterMode.Bilinear,
                };

                var pixels = new Color32[size * size];
                for (int y = 0; y < size; y++)
                {
                    for (int x = 0; x < size; x++)
                    {
                        float n = Mathf.PerlinNoise(x * 0.15f, y * 0.15f);
                        // Stay close to white — this multiplies against _BaseColor, so it should read
                        // as faint thumbprint/imperfection, not a visible pattern.
                        byte v = (byte)Mathf.RoundToInt(Mathf.Lerp(198f, 255f, n));
                        pixels[(y * size) + x] = new Color32(v, v, v, 255);
                    }
                }

                tex.SetPixels32(pixels);
                tex.Apply(false, true);
                _clayGrain = tex;
                return _clayGrain;
            }
        }
    }
}
