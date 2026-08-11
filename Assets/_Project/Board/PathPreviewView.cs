using System.Collections.Generic;
using LogiCard.Sim;
using UnityEngine;

namespace LogiCard.Board
{
    /// <summary>
    /// FragPunk/界外狂潮-style "线稿涂鸦" (sketchy ink line): a thin, slightly wobbly hand-drawn
    /// stroke on the board surface. Design pivot 2026-08-07 — replaces Day 9's 3D yarn/pin-bead
    /// look (see docs/ART_DIRECTION.md path pillar); not fat spray, not a glitchy HUD line, not
    /// neon. Draft reads like pencil (lighter, rougher, unsettled); booked reads like settled ink
    /// (darker, bolder, steadier) — a sketch-to-ink metaphor, not a port of yarn's old logic.
    /// </summary>
    public sealed class PathPreviewView : MonoBehaviour
    {
        private static readonly Color DraftInk = new Color(0.62f, 0.58f, 0.54f, 1f);
        private static readonly Color BookedInk = new Color(0.14f, 0.12f, 0.11f, 1f);

        private const float StrokeHeight = 0.03f;
        private const float DotHeight = 0.032f;
        private const float DraftWidth = 0.028f;
        private const float BookedWidth = 0.036f;
        private const float DraftDotRadius = 0.045f;
        private const float BookedDotRadius = 0.055f;

        // How far a point can wander perpendicular to its leg, in world units — draft wobbles more
        // (rough sketch), booked settles down (confident inked line) while staying hand-drawn.
        private const float DraftWobble = 0.06f;
        private const float BookedWobble = 0.02f;
        private const int SubdivisionsPerLeg = 8;

        private LineRenderer _stroke;
        private readonly List<Transform> _dots = new List<Transform>();
        private BoardView _board;

        public void Init(BoardView board)
        {
            _board = board;
        }

        /// <param name="skipFirstDot">
        /// When true, the first waypoint is the pawn's standing origin — draw the stroke from it, but
        /// do not place an ink bead there (the pawn already marks that point). Playtest 2026-08-11:
        /// omitting the origin from the polyline entirely left the first move leg undrawn.
        /// </param>
        public void Show(IReadOnlyList<PlanarPosition> waypoints, bool isDraft, bool skipFirstDot = false)
        {
            Clear();
            if (_board == null || waypoints == null || waypoints.Count == 0)
            {
                return;
            }

            if (waypoints.Count > 1)
            {
                BuildStroke(waypoints, isDraft);
            }

            Color dotColor = isDraft ? DraftInk : BookedInk;
            float dotRadius = isDraft ? DraftDotRadius : BookedDotRadius;
            int dotStart = skipFirstDot ? 1 : 0;
            for (int i = dotStart; i < waypoints.Count; i++)
            {
                BuildDot(waypoints[i], dotColor, dotRadius);
            }
        }

        public void Clear()
        {
            if (_stroke != null)
            {
                Object.Destroy(_stroke.gameObject);
                _stroke = null;
            }

            for (int i = 0; i < _dots.Count; i++)
            {
                if (_dots[i] != null)
                {
                    Object.Destroy(_dots[i].gameObject);
                }
            }

            _dots.Clear();
        }

        private void BuildStroke(IReadOnlyList<PlanarPosition> waypoints, bool isDraft)
        {
            var go = new GameObject("PathInk");
            go.transform.SetParent(transform, false);

            var line = go.AddComponent<LineRenderer>();
            line.useWorldSpace = true;
            line.alignment = LineAlignment.View;
            line.numCapVertices = 3;
            line.numCornerVertices = 2;
            line.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            line.receiveShadows = false;

            float width = isDraft ? DraftWidth : BookedWidth;
            line.startWidth = width;
            line.endWidth = width;

            Color color = isDraft ? DraftInk : BookedInk;
            line.sharedMaterial = PrimitiveMaterialFactory.Tinted(color);
            line.startColor = color;
            line.endColor = color;

            List<Vector3> points = BuildWobblyPoints(waypoints, isDraft);
            line.positionCount = points.Count;
            for (int i = 0; i < points.Count; i++)
            {
                line.SetPosition(i, points[i]);
            }

            _stroke = line;
        }

        /// <summary>
        /// Subdivides each leg and nudges interior points sideways by a small, deterministic
        /// (Perlin-seeded) amount — a stable hand-drawn wobble that looks the same every time for the
        /// same waypoints, unlike true per-frame randomness (which would visibly jitter while a
        /// player is still dragging a draft). Wobble tapers to zero at each waypoint so legs still
        /// meet exactly — no gaps or kinks at turns.
        /// </summary>
        private List<Vector3> BuildWobblyPoints(IReadOnlyList<PlanarPosition> waypoints, bool isDraft)
        {
            float wobble = isDraft ? DraftWobble : BookedWobble;
            var points = new List<Vector3>();

            for (int leg = 0; leg < waypoints.Count - 1; leg++)
            {
                Vector3 a = _board.WorldFromPlanar(waypoints[leg]) + (Vector3.up * StrokeHeight);
                Vector3 b = _board.WorldFromPlanar(waypoints[leg + 1]) + (Vector3.up * StrokeHeight);
                Vector3 delta = b - a;
                Vector3 perp = new Vector3(-delta.z, 0f, delta.x).normalized;
                float seed = leg * 17.13f;

                int steps = Mathf.Max(1, SubdivisionsPerLeg);
                int startIndex = leg == 0 ? 0 : 1;
                for (int s = startIndex; s <= steps; s++)
                {
                    float t = s / (float)steps;
                    Vector3 point = Vector3.Lerp(a, b, t);

                    float taper = Mathf.Sin(t * Mathf.PI);
                    float noise = (Mathf.PerlinNoise(seed, t * 6f) - 0.5f) * 2f;
                    point += perp * (noise * wobble * taper);

                    points.Add(point);
                }
            }

            return points;
        }

        private void BuildDot(PlanarPosition point, Color color, float radius)
        {
            var dot = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            dot.name = "PathInkDot";
            dot.transform.SetParent(transform, false);
            dot.transform.position = _board.WorldFromPlanar(point) + (Vector3.up * DotHeight);
            dot.transform.localScale = new Vector3(radius, 0.01f, radius);
            dot.GetComponent<MeshRenderer>().sharedMaterial = PrimitiveMaterialFactory.Tinted(color);

            Collider col = dot.GetComponent<Collider>();
            if (col != null)
            {
                Object.Destroy(col);
            }

            _dots.Add(dot.transform);
        }
    }
}
