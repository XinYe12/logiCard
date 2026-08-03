using System.Collections.Generic;
using LogiCard.Sim;
using UnityEngine;

namespace LogiCard.Board
{
    /// <summary>
    /// Day 5 placeholder for path readability. Yarn/chalk materials land on Day 9; until then
    /// tile beads along the booked route are enough to show the orthogonal path.
    /// </summary>
    public sealed class PathPreviewView : MonoBehaviour
    {
        private static readonly Color DraftColor = new Color(0.95f, 0.78f, 0.35f, 1f);
        private static readonly Color BookedColor = new Color(0.75f, 0.55f, 0.28f, 1f);

        private readonly List<Transform> _beads = new List<Transform>();
        private BoardView _board;

        public void Init(BoardView board)
        {
            _board = board;
        }

        public void Show(IReadOnlyList<GridCoordinate> waypoints, bool isDraft)
        {
            Clear();
            if (_board == null || waypoints == null || waypoints.Count == 0)
            {
                return;
            }

            Color color = isDraft ? DraftColor : BookedColor;
            for (int i = 0; i < waypoints.Count; i++)
            {
                var bead = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                bead.name = $"PathBead_{i}";
                bead.transform.SetParent(transform, false);
                bead.transform.position = _board.WorldFromCoord(waypoints[i]) + (Vector3.up * 0.22f);
                bead.transform.localScale = Vector3.one * (isDraft ? 0.22f : 0.16f);
                bead.GetComponent<MeshRenderer>().sharedMaterial = PrimitiveMaterialFactory.Tinted(color);

                Collider col = bead.GetComponent<Collider>();
                if (col != null)
                {
                    Object.Destroy(col);
                }

                _beads.Add(bead.transform);
            }
        }

        public void Clear()
        {
            for (int i = 0; i < _beads.Count; i++)
            {
                if (_beads[i] != null)
                {
                    Object.Destroy(_beads[i].gameObject);
                }
            }

            _beads.Clear();
        }
    }
}
