using UnityEngine;

namespace LogiCard.Board
{
    /// <summary>
    /// Makes tinted materials for the primitive placeholder art.
    ///
    /// Deliberately avoids Shader.Find("Standard"): nothing in the project references that
    /// shader as an asset, so it is stripped from player builds and Shader.Find returns null
    /// at runtime. Primitives always come with the default material, so we clone that instead.
    /// </summary>
    internal static class PrimitiveMaterialFactory
    {
        private static Material _template;

        public static Material Tinted(Color color)
        {
            var material = new Material(Template) { color = color };
            if (material.HasProperty("_Glossiness"))
            {
                material.SetFloat("_Glossiness", 0.05f);
            }

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

                var probe = GameObject.CreatePrimitive(PrimitiveType.Cube);
                probe.hideFlags = HideFlags.HideAndDontSave;
                _template = probe.GetComponent<MeshRenderer>().sharedMaterial;
                Object.DestroyImmediate(probe);
                return _template;
            }
        }
    }
}
