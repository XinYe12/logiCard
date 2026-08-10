using System.Collections.Generic;
using LogiCard.Sim;
using UnityEngine;

namespace LogiCard.Board
{
    /// <summary>
    /// Scene view for <see cref="ArenaBoard"/> (C35/C39 Phase 4 + C53 detail pass): room-zoned
    /// floors, brick walls, terrain-edge strata, and prop dressing. Sim owns geometry truth; this
    /// only draws and converts coordinates. Wall/door <em>positions</em> stay gameplay-owned.
    /// </summary>
    public sealed class BoardView : MonoBehaviour
    {
        public float WorldScale = 1f;
        public float FloorSpacing = 2.5f;
        public float WallHeight = 0.85f;
        public float SegmentThickness = 0.14f;

        // Keep these far from wall brick — playtest 2026-08-07: Closed used a near-wall rust and
        // read as "door never changes color." Checkpoint 3 replaces these boxes with real door meshes.
        private static readonly Color DoorOpenColor = new Color(0.35f, 0.82f, 0.42f);
        private static readonly Color DoorClosedColor = new Color(0.82f, 0.22f, 0.18f);

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

        public void Build(ArenaBoard model)
        {
            _model = model;
            _doorVisuals.Clear();

            for (int i = transform.childCount - 1; i >= 0; i--)
            {
                DestroyImmediate(transform.GetChild(i).gameObject);
            }

            PlaceRoomFloors(model);
            PlaceTerrainEdge(model);
            PlaceVoidDressing(model);
            PlacePaintedGrid(model);
            PlaceRoomDressing(model);

            for (int i = 0; i < model.Walls.Count; i++)
            {
                PlaceSegmentBox($"Wall_{i}", model.Walls[i], BoardSurfaceMaterials.BrickWall, WallHeight);
            }

            for (int i = 0; i < model.Doors.Count; i++)
            {
                Door door = model.Doors[i];
                Color color = model.GetDoorState(door) == DoorState.Open ? DoorOpenColor : DoorClosedColor;
                GameObject box = PlaceSegmentBox(
                    $"Door_{i}",
                    door.Segment,
                    PrimitiveMaterialFactory.Tinted(color),
                    WallHeight * 0.92f);
                _doorVisuals.Add(new DoorVisual(door, box.GetComponent<MeshRenderer>()));
            }
        }

        /// <summary>Back-compat overload — colors ignored; C53 surfaces own the palette.</summary>
        public void Build(ArenaBoard model, Color groundColor, Color wallColor)
        {
            Build(model);
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
        /// C45 room zones as distinct wet-dusk surfaces (Yard asphalt / Hall concrete / Vault polish /
        /// flank approach lanes). Bounds match GameBootstrap.BuildBoard — presentation only.
        /// </summary>
        private void PlaceRoomFloors(ArenaBoard model)
        {
            var floors = new GameObject("RoomFloors");
            floors.transform.SetParent(transform, false);

            // Yard — open south approach (y below 4).
            PlaceFloorSlab(floors.transform, "YardFloor", 0f, 0f, 8f, 4f, BoardSurfaceMaterials.YardFloor);
            // Hall — walled kill-box (x in [2,6], y in [4,7]).
            PlaceFloorSlab(floors.transform, "HallFloor", 2f, 4f, 6f, 7f, BoardSurfaceMaterials.HallFloor);
            // Vault — north objective room (y above 7).
            PlaceFloorSlab(floors.transform, "VaultFloor", 0f, 7f, 8f, 10f, BoardSurfaceMaterials.VaultFloor);
            // Unguarded flanks beside Hall.
            PlaceFloorSlab(floors.transform, "FlankWest", 0f, 4f, 2f, 7f, BoardSurfaceMaterials.FlankFloor);
            PlaceFloorSlab(floors.transform, "FlankEast", 6f, 4f, 8f, 7f, BoardSurfaceMaterials.FlankFloor);

            // Keep a thin structural slab under everything so raycasts always hit even if a zone
            // seam has a hairline gap (BoardInputController taps the ground collider).
            float width = model.MaxX - model.MinX;
            float depth = model.MaxY - model.MinY;
            float centerX = (model.MinX + model.MaxX) * 0.5f;
            float centerY = (model.MinY + model.MaxY) * 0.5f;
            var underlay = GameObject.CreatePrimitive(PrimitiveType.Cube);
            underlay.name = "GroundUnderlay";
            underlay.transform.SetParent(floors.transform, false);
            underlay.transform.localPosition = LocalFromPlanar(new PlanarPosition(centerX, centerY))
                                               + new Vector3(0f, -0.06f, 0f);
            underlay.transform.localScale = new Vector3(width * WorldScale, 0.08f, depth * WorldScale);
            underlay.GetComponent<MeshRenderer>().sharedMaterial = BoardSurfaceMaterials.StrataDirt;
        }

        private void PlaceFloorSlab(
            Transform parent,
            string name,
            float minX,
            float minY,
            float maxX,
            float maxY,
            Material material)
        {
            float width = maxX - minX;
            float depth = maxY - minY;
            float centerX = (minX + maxX) * 0.5f;
            float centerY = (minY + maxY) * 0.5f;

            var slab = GameObject.CreatePrimitive(PrimitiveType.Cube);
            slab.name = name;
            slab.transform.SetParent(parent, false);
            slab.transform.localPosition = LocalFromPlanar(new PlanarPosition(centerX, centerY))
                                           + new Vector3(0f, -0.05f, 0f);
            slab.transform.localScale = new Vector3(width * WorldScale, 0.1f, depth * WorldScale);
            slab.GetComponent<MeshRenderer>().sharedMaterial = material;
            // Zone slabs are visual only — underlay keeps a single clickable collider.
            StripCollider(slab);
        }

        /// <summary>
        /// Reference-style chunk edge: wood frame + dirt/rock/grass strata cross-section, replacing
        /// the old flat plywood lip. Still a bounded floating chunk — not an infinite terrain.
        /// </summary>
        private void PlaceTerrainEdge(ArenaBoard model)
        {
            var edgeRoot = new GameObject("TerrainEdge");
            edgeRoot.transform.SetParent(transform, false);

            float width = model.MaxX - model.MinX;
            float depth = model.MaxY - model.MinY;
            float centerX = (model.MinX + model.MaxX) * 0.5f;
            float centerY = (model.MinY + model.MaxY) * 0.5f;

            const float frameThickness = 0.18f;
            const float frameHeight = 0.55f;
            // Frame sits mostly below the playable face so the strata read from the side.
            const float frameCenterY = -0.22f;

            PlaceEdgeBand(
                edgeRoot.transform, "Frame_MinY",
                new PlanarPosition(centerX, model.MinY),
                new Vector3((width + frameThickness) * WorldScale, frameHeight, frameThickness * WorldScale),
                frameCenterY, BoardSurfaceMaterials.WoodEdge);
            PlaceEdgeBand(
                edgeRoot.transform, "Frame_MaxY",
                new PlanarPosition(centerX, model.MaxY),
                new Vector3((width + frameThickness) * WorldScale, frameHeight, frameThickness * WorldScale),
                frameCenterY, BoardSurfaceMaterials.WoodEdge);
            PlaceEdgeBand(
                edgeRoot.transform, "Frame_MinX",
                new PlanarPosition(model.MinX, centerY),
                new Vector3(frameThickness * WorldScale, frameHeight, depth * WorldScale),
                frameCenterY, BoardSurfaceMaterials.WoodEdge);
            PlaceEdgeBand(
                edgeRoot.transform, "Frame_MaxX",
                new PlanarPosition(model.MaxX, centerY),
                new Vector3(frameThickness * WorldScale, frameHeight, depth * WorldScale),
                frameCenterY, BoardSurfaceMaterials.WoodEdge);

            // Inset strata rings — grass near the top lip, rock mid, dirt deep — matching the
            // reference's natural terrain-edge cross-section inside the wood strip.
            PlaceStrataRing(edgeRoot.transform, model, "Grass", 0.02f, 0.08f, 0.12f, BoardSurfaceMaterials.StrataGrass);
            PlaceStrataRing(edgeRoot.transform, model, "Rock", -0.12f, 0.18f, 0.10f, BoardSurfaceMaterials.StrataRock);
            PlaceStrataRing(edgeRoot.transform, model, "Dirt", -0.30f, 0.22f, 0.08f, BoardSurfaceMaterials.StrataDirt);
        }

        private void PlaceStrataRing(
            Transform parent,
            ArenaBoard model,
            string label,
            float centerY,
            float height,
            float thickness,
            Material material)
        {
            float width = model.MaxX - model.MinX;
            float depth = model.MaxY - model.MinY;
            float centerX = (model.MinX + model.MaxX) * 0.5f;
            float centerYPlanar = (model.MinY + model.MaxY) * 0.5f;

            PlaceEdgeBand(
                parent, $"Strata_{label}_MinY",
                new PlanarPosition(centerX, model.MinY),
                new Vector3(width * WorldScale, height, thickness * WorldScale),
                centerY, material);
            PlaceEdgeBand(
                parent, $"Strata_{label}_MaxY",
                new PlanarPosition(centerX, model.MaxY),
                new Vector3(width * WorldScale, height, thickness * WorldScale),
                centerY, material);
            PlaceEdgeBand(
                parent, $"Strata_{label}_MinX",
                new PlanarPosition(model.MinX, centerYPlanar),
                new Vector3(thickness * WorldScale, height, depth * WorldScale),
                centerY, material);
            PlaceEdgeBand(
                parent, $"Strata_{label}_MaxX",
                new PlanarPosition(model.MaxX, centerYPlanar),
                new Vector3(thickness * WorldScale, height, depth * WorldScale),
                centerY, material);
        }

        private void PlaceEdgeBand(
            Transform parent,
            string name,
            PlanarPosition planarCenter,
            Vector3 localScale,
            float localY,
            Material material)
        {
            var band = GameObject.CreatePrimitive(PrimitiveType.Cube);
            band.name = name;
            band.transform.SetParent(parent, false);
            band.transform.localPosition = LocalFromPlanar(planarCenter) + new Vector3(0f, localY, 0f);
            band.transform.localScale = localScale;
            band.GetComponent<MeshRenderer>().sharedMaterial = material;
            StripCollider(band);
        }

        /// <summary>
        /// Presentation-only room props + warm window panes (emissive). Placed clear of door
        /// corridors and spawn lanes so they never read as blockers (C40 — no pawn collision anyway;
        /// these also strip colliders so taps pass through to the ground underlay).
        /// </summary>
        private void PlaceRoomDressing(ArenaBoard model)
        {
            var root = new GameObject("RoomDressing");
            root.transform.SetParent(transform, false);

            // Yard — wet approach clutter at the edges, leave the (4,*) spine clear for attacker.
            PlaceProp(root.transform, "Yard_CrateStack_W", new PlanarPosition(0.7f, 1.2f),
                new Vector3(0.55f, 0.45f, 0.55f), 0.22f, 12f, BoardSurfaceMaterials.PropCrate);
            PlaceProp(root.transform, "Yard_CrateStack_E", new PlanarPosition(7.3f, 1.4f),
                new Vector3(0.6f, 0.5f, 0.5f), 0.25f, -18f, BoardSurfaceMaterials.PropCrate);
            PlaceProp(root.transform, "Yard_Barrel_W", new PlanarPosition(1.1f, 2.8f),
                new Vector3(0.35f, 0.55f, 0.35f), 0.28f, 0f, BoardSurfaceMaterials.PropMetal);
            PlaceProp(root.transform, "Yard_Barrel_E", new PlanarPosition(6.9f, 2.6f),
                new Vector3(0.35f, 0.55f, 0.35f), 0.28f, 0f, BoardSurfaceMaterials.PropMetal);

            // Hall — cover + warm practical window panes on the side walls (reference's lit windows).
            PlaceProp(root.transform, "Hall_Cover_SW", new PlanarPosition(2.55f, 4.55f),
                new Vector3(0.7f, 0.35f, 0.45f), 0.18f, 8f, BoardSurfaceMaterials.PropCrate);
            PlaceProp(root.transform, "Hall_Cover_NE", new PlanarPosition(5.45f, 6.4f),
                new Vector3(0.65f, 0.38f, 0.5f), 0.19f, -22f, BoardSurfaceMaterials.PropCrate);
            PlaceWindowPane(root.transform, "Hall_Window_W", new PlanarPosition(2.05f, 5.5f),
                new Vector3(0.04f, 0.45f, 0.7f), 0.55f);
            PlaceWindowPane(root.transform, "Hall_Window_E", new PlanarPosition(5.95f, 5.5f),
                new Vector3(0.04f, 0.45f, 0.7f), 0.55f);

            // Vault — shelving / crate depth dressing north of Door #2.
            PlaceProp(root.transform, "Vault_Shelf_W", new PlanarPosition(1.2f, 8.8f),
                new Vector3(1.1f, 0.7f, 0.35f), 0.35f, 0f, BoardSurfaceMaterials.PropCrate);
            PlaceProp(root.transform, "Vault_Shelf_E", new PlanarPosition(6.8f, 8.8f),
                new Vector3(1.1f, 0.7f, 0.35f), 0.35f, 0f, BoardSurfaceMaterials.PropCrate);
            PlaceProp(root.transform, "Vault_Crate", new PlanarPosition(4.0f, 9.3f),
                new Vector3(0.7f, 0.4f, 0.55f), 0.2f, 15f, BoardSurfaceMaterials.PropMetal);
            PlaceWindowPane(root.transform, "Vault_Window_N", new PlanarPosition(4f, 9.85f),
                new Vector3(1.2f, 0.4f, 0.04f), 0.55f);

            // Silence unused-parameter warning if layout constants ever diverge from model bounds —
            // dressing is authored to the C45 Yard/Hall/Vault numbers, not derived from Min/Max.
            _ = model;
        }

        private void PlaceProp(
            Transform parent,
            string name,
            PlanarPosition planar,
            Vector3 scale,
            float localY,
            float yawDegrees,
            Material material)
        {
            var prop = GameObject.CreatePrimitive(PrimitiveType.Cube);
            prop.name = name;
            prop.transform.SetParent(parent, false);
            prop.transform.localPosition = LocalFromPlanar(planar) + new Vector3(0f, localY, 0f);
            prop.transform.localScale = scale * WorldScale;
            prop.transform.localRotation = Quaternion.Euler(0f, yawDegrees, 0f);
            prop.GetComponent<MeshRenderer>().sharedMaterial = material;
            StripCollider(prop);
        }

        private void PlaceWindowPane(Transform parent, string name, PlanarPosition planar, Vector3 scale, float localY)
        {
            var pane = GameObject.CreatePrimitive(PrimitiveType.Cube);
            pane.name = name;
            pane.transform.SetParent(parent, false);
            pane.transform.localPosition = LocalFromPlanar(planar) + new Vector3(0f, localY, 0f);
            pane.transform.localScale = scale * WorldScale;
            pane.GetComponent<MeshRenderer>().sharedMaterial = BoardSurfaceMaterials.WarmGlass;
            StripCollider(pane);
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

            const float apronMargin = 2.75f;
            var apron = GameObject.CreatePrimitive(PrimitiveType.Cube);
            apron.name = "VoidApron";
            apron.transform.SetParent(voidRoot.transform, false);
            apron.transform.localPosition = LocalFromPlanar(new PlanarPosition(centerX, centerY))
                                            + new Vector3(0f, -0.55f, 0f);
            apron.transform.localScale = new Vector3(
                (width + (apronMargin * 2f)) * WorldScale,
                0.06f,
                (depth + (apronMargin * 2f)) * WorldScale);
            apron.GetComponent<MeshRenderer>().sharedMaterial = PrimitiveMaterialFactory.Tinted(VoidApronColor);
            StripCollider(apron);

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

            // Cooler etched lines so they sit into wet asphalt/concrete rather than reading as
            // warm clay paint on plywood.
            Color lineColor = new Color(0.12f, 0.14f, 0.18f, 0.7f);
            const float lineY = 0.02f;
            const float thickness = 0.03f;

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

        private GameObject PlaceSegmentBox(string name, Segment segment, Material material, float height)
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
            float yaw = Mathf.Atan2(-dy, dx) * Mathf.Rad2Deg;
            box.transform.localRotation = Quaternion.Euler(0f, yaw, 0f);
            box.GetComponent<MeshRenderer>().sharedMaterial = material;

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
