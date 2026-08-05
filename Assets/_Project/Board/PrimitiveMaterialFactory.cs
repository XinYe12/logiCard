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

        public static Material Tinted(Color color)
        {
            var material = new Material(Template);
            ApplyColor(material, color);
            ApplyMatte(material);
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
            if (material.HasProperty("_Smoothness"))
            {
                material.SetFloat("_Smoothness", 0.05f);
            }

            if (material.HasProperty("_Glossiness"))
            {
                material.SetFloat("_Glossiness", 0.05f);
            }

            if (material.HasProperty("_Metallic"))
            {
                material.SetFloat("_Metallic", 0f);
            }
        }
    }
}
