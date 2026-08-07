using System.Collections.Generic;
using LogiCard.Sim;
using UnityEngine;

namespace LogiCard.Board
{
    /// <summary>
    /// Scene view for <see cref="ArenaBoard"/> (C35/C39 Phase 4): one ground plane plus thin boxes
    /// for wall/door segments. Sim owns geometry truth; this only draws and converts coordinates.
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
