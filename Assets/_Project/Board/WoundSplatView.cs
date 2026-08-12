using UnityEngine;

namespace LogiCard.Board
{
    /// <summary>
    /// Wet red clay wound splat that sticks to the floor on hit (ART_DIRECTION §3: "Required on
    /// hit"). Persistent once shown — no auto-hide; visibility is driven by whoever owns playback.
    /// </summary>
    public sealed class WoundSplatView : MonoBehaviour
    {
        private static readonly Color SplatTint = new Color(0.55f, 0.05f, 0.04f);

        private const float FloorClearance = 0.02f;
        private const float WetSmoothness = 0.45f;

        private Transform _root;

        public void Init()
        {
            var root = new GameObject("Splat");
            root.transform.SetParent(transform, false);
            _root = root.transform;

            Material material = PrimitiveMaterialFactory.Tinted(SplatTint);
            if (material.HasProperty("_Smoothness"))
            {
                material.SetFloat("_Smoothness", WetSmoothness);
            }

            if (material.HasProperty("_Glossiness"))
            {
                material.SetFloat("_Glossiness", WetSmoothness);
            }

            // Overlapping squashed spheres — irregular blob, not a perfect disc.
            AddLobe(material, "LobeA", new Vector3(0f, 0f, 0f), new Vector3(0.22f, 0.04f, 0.18f));
            AddLobe(material, "LobeB", new Vector3(0.08f, 0f, 0.05f), new Vector3(0.14f, 0.035f, 0.16f));
            AddLobe(material, "LobeC", new Vector3(-0.07f, 0f, -0.04f), new Vector3(0.12f, 0.03f, 0.10f));
            AddLobe(material, "LobeD", new Vector3(0.02f, 0f, -0.09f), new Vector3(0.10f, 0.028f, 0.13f));

            SetVisible(false);
        }

        public void Place(Vector3 worldPosition)
        {
            if (_root == null)
            {
                return;
            }

            _root.position = worldPosition + Vector3.up * FloorClearance;
            // Slight yaw variety so stacked hits don't look stamped from the same cookie cutter.
            float yaw = (worldPosition.x * 37.1f + worldPosition.z * 19.7f) % 360f;
            _root.rotation = Quaternion.Euler(0f, yaw, 0f);
        }

        public void SetVisible(bool visible)
        {
            if (_root != null)
            {
                _root.gameObject.SetActive(visible);
            }
        }

        /// <summary>Scrubber-derived visibility (PLAYBACK_CONTRACT continuous presenter).</summary>
        public bool IsVisible => _root != null && _root.gameObject.activeSelf;

        private void AddLobe(Material material, string name, Vector3 localPosition, Vector3 localScale)
        {
            var lobe = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            lobe.name = name;
            lobe.transform.SetParent(_root, false);

            Collider collider = lobe.GetComponent<Collider>();
            if (collider != null)
            {
                Destroy(collider);
            }

            lobe.GetComponent<MeshRenderer>().sharedMaterial = material;
            lobe.transform.localPosition = localPosition;
            lobe.transform.localScale = localScale;
        }
    }
}
