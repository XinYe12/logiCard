using System.Collections.Generic;
using LogiCard.Sim;
using UnityEngine;

namespace LogiCard.Board
{
    /// <summary>
    /// Scene view for <see cref="ArenaBoard"/> (C35/C39 Phase 4): one ground plane plus thin boxes
    /// for wall/door segments, plus physical board-edge lip and dark void dressing
    /// (ART_DIRECTION Board pillar). Sim owns geometry truth; this only draws and converts coordinates.
    /// </summary>
    public sealed class BoardView : MonoBehaviour
    {
        public float WorldScale = 1f;
        public float FloorSpacing = 2.5f;
        public float WallHeight = 0.85f;
        public float SegmentThickness = 0.12f;

        // Keep these far from wall brown (GameBootstrap ~0.42/0.38/0.34) — playtest 2026-08-07:
        // Closed used a near-wall rust and read as "door never changes color."
        private static readonly Color DoorOpenColor = new Color(0.35f, 0.82f, 0.42f);
        private static readonly Color DoorClosedColor = new Color(0.82f, 0.22f, 0.18f);

        // Void apron + clutter — near camera clear-color (GameBootstrap ~0.07/0.07/0.09) so the
        // board reads as a lit stage in a dark room, not a plane floating in empty black.
        private static readonly Color VoidApronColor = new Color(0.05f, 0.045f, 0.055f);
        private static readonly Color VoidClutterColor = new Color(0.11f, 0.09f, 0.08f);

        private ArenaBoard _model;
        private readonly List<DoorVisual> _doorVisuals = new List<DoorVisual>();

        public ArenaBoard Model => _model;

        public Vector3 CenterWorld => _model == null
            ? transform.position
            : WorldFromPlanar(new PlanarPosition(
                (_model.MinX + _model.MaxX) * 0.5f,
                (_model.MinY + _model.MaxY) * 0.5f));

        public void Build(ArenaBoard model, Color groundColor, Color wallColor)
        {
            _model = model;
            _doorVisuals.Clear();

            for (int i = transform.childCount - 1; i >= 0; i--)
            {
                DestroyImmediate(transform.GetChild(i).gameObject);
            }

            float width = model.MaxX - model.MinX;
            float depth = model.MaxY - model.MinY;
            float centerX = (model.MinX + model.MaxX) * 0.5f;
            float centerY = (model.MinY + model.MaxY) * 0.5f;

            var ground = GameObject.CreatePrimitive(PrimitiveType.Cube);
            ground.name = "GroundPlane";
            ground.transform.SetParent(transform, false);
            ground.transform.localPosition = LocalFromPlanar(new PlanarPosition(centerX, centerY))
                                            + new Vector3(0f, -0.05f, 0f);
            ground.transform.localScale = new Vector3(width * WorldScale, 0.1f, depth * WorldScale);
            ground.GetComponent<MeshRenderer>().sharedMaterial = PrimitiveMaterialFactory.Tinted(groundColor);

            // ART_DIRECTION Board pillar: physical edge + dark void beyond the playable face.
            PlaceBoardEdge(model, groundColor);
            PlaceVoidDressing(model);

            // Day 9: painted/etched unit grid on the board face (ART_DIRECTION Demo art floor).
            PlacePaintedGrid(model);

            for (int i = 0; i < model.Walls.Count; i++)
            {
                PlaceSegmentBox($"Wall_{i}", model.Walls[i], wallColor, WallHeight);
            }

            for (int i = 0; i < model.Doors.Count; i++)
            {
                Door door = model.Doors[i];
                Color color = model.GetDoorState(door) == DoorState.Open ? DoorOpenColor : DoorClosedColor;
                GameObject box = PlaceSegmentBox($"Door_{i}", door.Segment, color, WallHeight * 0.92f);
                _doorVisuals.Add(new DoorVisual(door, box.GetComponent<MeshRenderer>()));
            }
        }

        /// <summary>
        /// Refresh door box colors. Pass <paramref name="overrideStates"/> during Program to preview
        /// scheduled Open/Close without mutating the live board (pathfinding still uses model state
        /// until Aftermath carry — playtest 2026-08-07).
        /// </summary>
        public void RefreshDoorVisuals(IReadOnlyDictionary<Door, DoorState> overrideStates = null)
        {
            if (_model == null)
            {
                return;
            }

            for (int i = 0; i < _doorVisuals.Count; i++)
            {
                DoorVisual visual = _doorVisuals[i];
                if (visual.Renderer == null)
                {
                    continue;
                }

                DoorState state = overrideStates != null && overrideStates.TryGetValue(visual.Door, out DoorState preview)
                    ? preview
                    : _model.GetDoorState(visual.Door);
                Color color = state == DoorState.Open ? DoorOpenColor : DoorClosedColor;
                visual.Renderer.sharedMaterial = PrimitiveMaterialFactory.Tinted(color);
            }
        }

        public Vector3 LocalFromPlanar(PlanarPosition p)
        {
            float floorHeight = p.Floor == Floor.Attic ? FloorSpacing : 0f;
            return new Vector3(p.X * WorldScale, floorHeight, p.Y * WorldScale);
        }

        public Vector3 WorldFromPlanar(PlanarPosition p)
        {
            return transform.TransformPoint(LocalFromPlanar(p));
        }

        /// <summary>Inverse of <see cref="WorldFromPlanar"/> for ground-plane raycasts (Decision 4 input).</summary>
        public PlanarPosition PlanarFromWorld(Vector3 world)
        {
            Vector3 local = transform.InverseTransformPoint(world);
            float scale = WorldScale <= 0f ? 1f : WorldScale;
            return new PlanarPosition(local.x / scale, local.z / scale, Floor.Ground);
        }

        /// <summary>
        /// Raised lip around the ground plane perimeter so the board reads as a physical plywood/
        /// plastic base rather than a plane that stops in space. Bounds come from the model —
        /// never a hardcoded board size.
        /// </summary>
        private void PlaceBoardEdge(ArenaBoard model, Color groundColor)
        {
            var edgeRoot = new GameObject("BoardEdge");
            edgeRoot.transform.SetParent(transform, false);

            // Darker than the playable face — plywood edge / bevel strip.
            Color lipColor = new Color(
                groundColor.r * 0.55f,
                groundColor.g * 0.48f,
                groundColor.b * 0.42f,
                1f);

            const float lipThickness = 0.14f;
            // Ground top is at local y=0; lip rises slightly above so the edge catches key light.
            const float lipHeight = 0.16f;
            const float lipCenterY = 0.02f;

            float width = model.MaxX - model.MinX;
            float depth = model.MaxY - model.MinY;
            float centerX = (model.MinX + model.MaxX) * 0.5f;
            float centerY = (model.MinY + model.MaxY) * 0.5f;

            // Long edges span full width (corners covered); short edges span depth only.
            PlaceEdgeLip(
                edgeRoot.transform,
                "Edge_MinY",
                new PlanarPosition(centerX, model.MinY),
                new Vector3((width + lipThickness) * WorldScale, lipHeight, lipThickness * WorldScale),
                lipCenterY,
                lipColor);
            PlaceEdgeLip(
                edgeRoot.transform,
                "Edge_MaxY",
                new PlanarPosition(centerX, model.MaxY),
                new Vector3((width + lipThickness) * WorldScale, lipHeight, lipThickness * WorldScale),
                lipCenterY,
                lipColor);
            PlaceEdgeLip(
                edgeRoot.transform,
                "Edge_MinX",
                new PlanarPosition(model.MinX, centerY),
                new Vector3(lipThickness * WorldScale, lipHeight, depth * WorldScale),
                lipCenterY,
                lipColor);
            PlaceEdgeLip(
                edgeRoot.transform,
                "Edge_MaxX",
                new PlanarPosition(model.MaxX, centerY),
                new Vector3(lipThickness * WorldScale, lipHeight, depth * WorldScale),
                lipCenterY,
                lipColor);
        }

        private void PlaceEdgeLip(
            Transform parent,
            string name,
            PlanarPosition planarCenter,
            Vector3 localScale,
            float localY,
            Color color)
        {
            var lip = GameObject.CreatePrimitive(PrimitiveType.Cube);
            lip.name = name;
            lip.transform.SetParent(parent, false);
            lip.transform.localPosition = LocalFromPlanar(planarCenter) + new Vector3(0f, localY, 0f);
            lip.transform.localScale = localScale;
            lip.GetComponent<MeshRenderer>().sharedMaterial = PrimitiveMaterialFactory.Tinted(color);
            StripCollider(lip);
        }

        /// <summary>
        /// Dark recessed apron beyond the board plus a few dim primitive silhouettes (messy
        /// workbench cue). Tilt-shift DoF blurs them further — they only need to read as
        /// "something in the void," not detailed props.
        /// </summary>
        private void PlaceVoidDressing(ArenaBoard model)
        {
            var voidRoot = new GameObject("VoidDressing");
            voidRoot.transform.SetParent(transform, false);

            float width = model.MaxX - model.MinX;
            float depth = model.MaxY - model.MinY;
            float centerX = (model.MinX + model.MaxX) * 0.5f;
            float centerY = (model.MinY + model.MaxY) * 0.5f;

            // Apron extends past every edge; sits below the ground slab so only the outer ring shows.
            const float apronMargin = 2.75f;
            var apron = GameObject.CreatePrimitive(PrimitiveType.Cube);
            apron.name = "VoidApron";
            apron.transform.SetParent(voidRoot.transform, false);
            apron.transform.localPosition = LocalFromPlanar(new PlanarPosition(centerX, centerY))
                                            + new Vector3(0f, -0.22f, 0f);
            apron.transform.localScale = new Vector3(
                (width + (apronMargin * 2f)) * WorldScale,
                0.06f,
                (depth + (apronMargin * 2f)) * WorldScale);
            apron.GetComponent<MeshRenderer>().sharedMaterial = PrimitiveMaterialFactory.Tinted(VoidApronColor);
            StripCollider(apron);

            // Sparse workbench clutter — offsets from model bounds, not a fixed board size.
            PlaceVoidClutter(
                voidRoot.transform,
                "Clutter_Block",
                new PlanarPosition(model.MinX - 1.35f, model.MinY + (depth * 0.28f)),
                new Vector3(0.9f * WorldScale, 0.35f, 0.55f * WorldScale),
                -0.12f,
                18f);
            PlaceVoidClutter(
                voidRoot.transform,
                "Clutter_Slab",
                new PlanarPosition(model.MaxX + 1.45f, model.MaxY - (depth * 0.22f)),
                new Vector3(1.2f * WorldScale, 0.18f, 0.7f * WorldScale),
                -0.14f,
                -25f);
            PlaceVoidClutter(
                voidRoot.transform,
                "Clutter_Can",
                new PlanarPosition(model.MinX + (width * 0.18f), model.MaxY + 1.55f),
                new Vector3(0.35f * WorldScale, 0.55f, 0.35f * WorldScale),
                -0.08f,
                40f);
            PlaceVoidClutter(
                voidRoot.transform,
                "Clutter_Tray",
                new PlanarPosition(model.MaxX - (width * 0.25f), model.MinY - 1.4f),
                new Vector3(0.85f * WorldScale, 0.12f, 0.45f * WorldScale),
                -0.16f,
                8f);
            PlaceVoidClutter(
                voidRoot.transform,
                "Clutter_Peg",
                new PlanarPosition(model.MinX - 1.6f, model.MaxY - (depth * 0.35f)),
                new Vector3(0.22f * WorldScale, 0.7f, 0.22f * WorldScale),
                -0.05f,
                -12f);
        }

        private void PlaceVoidClutter(
            Transform parent,
            string name,
            PlanarPosition planarCenter,
            Vector3 localScale,
            float localY,
            float yawDegrees)
        {
            var clutter = GameObject.CreatePrimitive(PrimitiveType.Cube);
            clutter.name = name;
            clutter.transform.SetParent(parent, false);
            clutter.transform.localPosition = LocalFromPlanar(planarCenter) + new Vector3(0f, localY, 0f);
            clutter.transform.localScale = localScale;
            clutter.transform.localRotation = Quaternion.Euler(0f, yawDegrees, 0f);
            clutter.GetComponent<MeshRenderer>().sharedMaterial = PrimitiveMaterialFactory.Tinted(VoidClutterColor);
            StripCollider(clutter);
        }

        private static void StripCollider(GameObject go)
        {
            Collider col = go.GetComponent<Collider>();
            if (col != null)
            {
                Object.Destroy(col);
            }
        }

        private void PlacePaintedGrid(ArenaBoard model)
        {
            var gridRoot = new GameObject("PaintedGrid");
            gridRoot.transform.SetParent(transform, false);

            Color lineColor = new Color(0.42f, 0.32f, 0.22f, 0.85f);
            const float lineY = 0.02f;
            const float thickness = 0.035f;

            int x0 = Mathf.CeilToInt(model.MinX);
            int x1 = Mathf.FloorToInt(model.MaxX);
            for (int x = x0; x <= x1; x++)
            {
                PlaceGridStroke(
                    gridRoot.transform,
                    $"GridX_{x}",
                    new PlanarPosition(x, model.MinY),
                    new PlanarPosition(x, model.MaxY),
                    lineColor,
                    lineY,
                    thickness);
            }

            int y0 = Mathf.CeilToInt(model.MinY);
            int y1 = Mathf.FloorToInt(model.MaxY);
            for (int y = y0; y <= y1; y++)
            {
                PlaceGridStroke(
                    gridRoot.transform,
                    $"GridY_{y}",
                    new PlanarPosition(model.MinX, y),
                    new PlanarPosition(model.MaxX, y),
                    lineColor,
                    lineY,
                    thickness);
            }
        }

        private void PlaceGridStroke(
            Transform parent,
            string name,
            PlanarPosition a,
            PlanarPosition b,
            Color color,
            float height,
            float thickness)
        {
            float dx = b.X - a.X;
            float dy = b.Y - a.Y;
            float length = Mathf.Sqrt((dx * dx) + (dy * dy));
            if (length < 1e-4f)
            {
                return;
            }

            PlanarPosition mid = PlanarPosition.Lerp(a, b, 0.5f);
            var stroke = GameObject.CreatePrimitive(PrimitiveType.Cube);
            stroke.name = name;
            stroke.transform.SetParent(parent, false);
            stroke.transform.localPosition = LocalFromPlanar(mid) + new Vector3(0f, height, 0f);
            stroke.transform.localScale = new Vector3(length * WorldScale, 0.02f, thickness * WorldScale);
            float yaw = Mathf.Atan2(-dy, dx) * Mathf.Rad2Deg;
            stroke.transform.localRotation = Quaternion.Euler(0f, yaw, 0f);
            stroke.GetComponent<MeshRenderer>().sharedMaterial = PrimitiveMaterialFactory.Tinted(color);

            Collider col = stroke.GetComponent<Collider>();
            if (col != null)
            {
                Object.Destroy(col);
            }
        }

        private GameObject PlaceSegmentBox(string name, Segment segment, Color color, float height)
        {
            float dx = segment.B.X - segment.A.X;
            float dy = segment.B.Y - segment.A.Y;
            float length = Mathf.Sqrt((dx * dx) + (dy * dy));
            if (length < 1e-4f)
            {
                length = SegmentThickness;
            }

            PlanarPosition mid = PlanarPosition.Lerp(segment.A, segment.B, 0.5f);
            var box = GameObject.CreatePrimitive(PrimitiveType.Cube);
            box.name = name;
            box.transform.SetParent(transform, false);
            box.transform.localPosition = LocalFromPlanar(mid) + new Vector3(0f, height * 0.5f, 0f);
            box.transform.localScale = new Vector3(length * WorldScale, height, SegmentThickness * WorldScale);
            // Box's long axis (scale.x) is local +X, which Quaternion.Euler(0,yaw,0) sends to
            // (cos(yaw), 0, -sin(yaw)) in local space — so aligning it to (dx, dy) needs
            // atan2(-dy, dx), not atan2(dx, dy) (that formula is for aligning local +Z instead).
            // BUG FOUND 2026-08-06 (playtest): every wall/door box rendered rotated 90° from its
            // real Segment position, e.g. a wall spanning x in [0, 1.75] at fixed y rendered as a
            // bar spanning y instead of x.
            float yaw = Mathf.Atan2(-dy, dx) * Mathf.Rad2Deg;
            box.transform.localRotation = Quaternion.Euler(0f, yaw, 0f);
            box.GetComponent<MeshRenderer>().sharedMaterial = PrimitiveMaterialFactory.Tinted(color);

            // Walls/doors should not steal board taps — only the ground plane is clickable.
            Collider col = box.GetComponent<Collider>();
            if (col != null)
            {
                Object.Destroy(col);
            }

            return box;
        }

        private readonly struct DoorVisual
        {
            public Door Door { get; }

            public MeshRenderer Renderer { get; }

            public DoorVisual(Door door, MeshRenderer renderer)
            {
                Door = door;
                Renderer = renderer;
            }
        }
    }
}
