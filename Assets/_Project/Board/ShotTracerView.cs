using UnityEngine;

namespace LogiCard.Board
{
    /// <summary>
    /// Stretched primitive standing in for a shot, so a Shoot reads differently from a Move on the
    /// board (D7 Slice 1). Visibility is driven by the Time Resource scrubber rather than wall-clock
    /// time, so scrubbing back across a shot shows it again.
    /// </summary>
    public sealed class ShotTracerView : MonoBehaviour
    {
        private const float Thickness = 0.07f;
        private const float ChestHeight = 0.45f;

        private Transform _beam;

        public void Init(Color color)
        {
            var beam = GameObject.CreatePrimitive(PrimitiveType.Cube);
            beam.name = "Beam";
            beam.transform.SetParent(transform, false);

            // A tracer must never intercept the tile picking raycast.
            Collider collider = beam.GetComponent<Collider>();
            if (collider != null)
            {
                Destroy(collider);
            }

            beam.GetComponent<MeshRenderer>().sharedMaterial = PrimitiveMaterialFactory.Tinted(color);
            _beam = beam.transform;
            SetVisible(false);
        }

        public void Aim(Vector3 from, Vector3 to)
        {
            if (_beam == null)
            {
                return;
            }

            Vector3 lift = Vector3.up * ChestHeight;
            Vector3 start = from + lift;
            Vector3 end = to + lift;
            Vector3 delta = end - start;
            float length = delta.magnitude;

            _beam.position = (start + end) * 0.5f;
            _beam.rotation = length > 0.0001f ? Quaternion.LookRotation(delta) : Quaternion.identity;
            _beam.localScale = new Vector3(Thickness, Thickness, Mathf.Max(length, 0.1f));
        }

        public void SetVisible(bool visible)
        {
            if (_beam != null)
            {
                _beam.gameObject.SetActive(visible);
            }
        }

        /// <summary>Scrubber-derived visibility (PLAYBACK_CONTRACT continuous presenter).</summary>
        public bool IsVisible => _beam != null && _beam.gameObject.activeSelf;
    }
}
