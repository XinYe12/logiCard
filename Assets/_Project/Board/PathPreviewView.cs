using System.Collections.Generic;
using LogiCard.Sim;
using UnityEngine;

namespace LogiCard.Board
{
    /// <summary>
    /// Day 9 path readability: muted yarn strand + pin beads on the board (ART_DIRECTION Demo
    /// art floor — not neon cyber lines). Draft yarn reads lighter/unsettled; booked is settled.
    /// </summary>
    public sealed class PathPreviewView : MonoBehaviour
    {
        // Match Assets/_Project/Art/Materials/Mat_PathYarn (_BaseColor ≈ terracotta yarn).
        private static readonly Color DraftYarn = new Color(0.92f, 0.55f, 0.42f, 1f);
        private static readonly Color BookedYarn = new Color(0.78f, 0.28f, 0.22f, 1f);
        private static readonly Color DraftPin = new Color(0.95f, 0.82f, 0.55f, 1f);
        private static readonly Color BookedPin = new Color(0.55f, 0.42f, 0.28f, 1f);

        private const float YarnHeight = 0.18f;
        private const float PinHeight = 0.22f;
        private const float YarnWidthDraft = 0.07f;
        private const float YarnWidthBooked = 0.055f;

        private readonly List<Transform> _pins = new List<Transform>();
        private LineRenderer _yarn;
        private BoardView _board;

        public void Init(BoardView board)
        {
            _board = board;
        }

        public void Show(IReadOnlyList<PlanarPosition> waypoints, bool isDraft)
        {
            Clear();
            if (_board == null || waypoints == null || waypoints.Count == 0)
            {
                return;
            }

            Color yarnColor = isDraft ? DraftYarn : BookedYarn;
            Color pinColor = isDraft ? DraftPin : BookedPin;
            float yarnWidth = isDraft ? YarnWidthDraft : YarnWidthBooked;

            EnsureYarn();
            _yarn.positionCount = waypoints.Count;
            _yarn.startWidth = yarnWidth;
            _yarn.endWidth = yarnWidth;
            _yarn.sharedMaterial = PrimitiveMaterialFactory.Tinted(yarnColor);
            _yarn.startColor = yarnColor;
            _yarn.endColor = yarnColor;

            for (int i = 0; i < waypoints.Count; i++)
            {
                Vector3 world = _board.WorldFromPlanar(waypoints[i]);
                _yarn.SetPosition(i, world + (Vector3.up * YarnHeight));

                // Yarn pins — small matte beads at each waypoint (ART_DIRECTION optional pin beads).
                var pin = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                pin.name = $"PathPin_{i}";
                pin.transform.SetParent(transform, false);
                pin.transform.position = world + (Vector3.up * PinHeight);
                pin.transform.localScale = Vector3.one * (isDraft ? 0.14f : 0.11f);
                pin.GetComponent<MeshRenderer>().sharedMaterial = PrimitiveMaterialFactory.Tinted(pinColor);

                Collider col = pin.GetComponent<Collider>();
                if (col != null)
                {
                    Object.Destroy(col);
                }

                _pins.Add(pin.transform);
            }
        }

        public void Clear()
        {
            for (int i = 0; i < _pins.Count; i++)
            {
                if (_pins[i] != null)
                {
                    Object.Destroy(_pins[i].gameObject);
                }
            }

            _pins.Clear();

            if (_yarn != null)
            {
                _yarn.positionCount = 0;
            }
        }

        private void EnsureYarn()
        {
            if (_yarn != null)
            {
                return;
            }

            var yarnGo = new GameObject("YarnStrand");
            yarnGo.transform.SetParent(transform, false);
            _yarn = yarnGo.AddComponent<LineRenderer>();
            _yarn.useWorldSpace = true;
            _yarn.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            _yarn.receiveShadows = false;
            _yarn.numCapVertices = 4;
            _yarn.numCornerVertices = 4;
            _yarn.alignment = LineAlignment.View;
            _yarn.textureMode = LineTextureMode.Stretch;
        }
    }
}
