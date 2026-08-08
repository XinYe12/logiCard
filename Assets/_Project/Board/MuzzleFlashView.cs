using UnityEngine;

namespace LogiCard.Board
{
    /// <summary>
    /// Jagged yellow/orange resin burst for a shot's muzzle flash (ART_DIRECTION §3: "2 frames,
    /// then gone"). Hard-edged static mesh only — no particles, bloom, or self-timed fade;
    /// visibility is driven by whoever owns playback timing.
    /// </summary>
    public sealed class MuzzleFlashView : MonoBehaviour
    {
        private static readonly Color FlashTint = new Color(1f, 0.72f, 0.12f);

        private const float ChestHeight = 0.45f;
        private const float MuzzleOffset = 0.18f;
        private const int ShardCount = 7;

        // Fixed irregular radii / lengths so the silhouette is asymmetric but deterministic.
        private static readonly float[] ShardRadii =
        {
            0.14f, 0.22f, 0.11f, 0.19f, 0.16f, 0.24f, 0.13f,
        };

        private static readonly float[] ShardLengths =
        {
            0.28f, 0.18f, 0.32f, 0.21f, 0.26f, 0.15f, 0.30f,
        };

        private static readonly float[] ShardThicknesses =
        {
            0.035f, 0.028f, 0.042f, 0.030f, 0.038f, 0.025f, 0.040f,
        };

        // Angular jitter (degrees) off equal spacing — keeps the fan from reading as a regular star.
        private static readonly float[] ShardAngleJitter =
        {
            -12f, 8f, -5f, 15f, -18f, 6f, -9f,
        };

        private Transform _root;

        public void Init()
        {
            var root = new GameObject("Flash");
            root.transform.SetParent(transform, false);
            _root = root.transform;

            Material material = PrimitiveMaterialFactory.Tinted(FlashTint);

            for (int i = 0; i < ShardCount; i++)
            {
                var shard = GameObject.CreatePrimitive(PrimitiveType.Cube);
                shard.name = "Shard" + i;
                shard.transform.SetParent(_root, false);

                Collider collider = shard.GetComponent<Collider>();
                if (collider != null)
                {
                    Destroy(collider);
                }

                shard.GetComponent<MeshRenderer>().sharedMaterial = material;

                float angle = (360f / ShardCount) * i + ShardAngleJitter[i];
                Quaternion yaw = Quaternion.Euler(0f, angle, 0f);
                float halfLength = ShardLengths[i] * 0.5f;
                shard.transform.localPosition = yaw * new Vector3(0f, 0f, halfLength + ShardRadii[i] * 0.15f);
                shard.transform.localRotation = yaw * Quaternion.Euler(0f, 0f, (i % 2 == 0) ? 18f : -22f);
                shard.transform.localScale = new Vector3(
                    ShardThicknesses[i],
                    ShardThicknesses[i] * (0.7f + (i % 3) * 0.15f),
                    ShardLengths[i]);
            }

            SetVisible(false);
        }

        public void Place(Vector3 worldOrigin, Vector3 worldAim)
        {
            if (_root == null)
            {
                return;
            }

            Vector3 lift = Vector3.up * ChestHeight;
            Vector3 from = worldOrigin + lift;
            Vector3 to = worldAim + lift;
            Vector3 delta = to - from;
            float length = delta.magnitude;

            Quaternion facing = length > 0.0001f
                ? Quaternion.LookRotation(delta)
                : Quaternion.identity;

            _root.position = from + facing * (Vector3.forward * MuzzleOffset);
            _root.rotation = facing;
        }

        public void SetVisible(bool visible)
        {
            if (_root != null)
            {
                _root.gameObject.SetActive(visible);
            }
        }
    }
}
