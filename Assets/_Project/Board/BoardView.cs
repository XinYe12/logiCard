using System.Collections;
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

        // Soft state cue only — playtest 2026-08-12: 0.72 lerp painted the whole leaf into a solid
        // red/green pillar and washed nappin wood to a flat plane. Keep far from brick so Closed still
        // reads, but leave most of the authored albedo alone.
        private static readonly Color DoorOpenColor = new Color(0.35f, 0.82f, 0.42f);
        private static readonly Color DoorClosedColor = new Color(0.82f, 0.22f, 0.18f);
        private const float DoorStateTintBlend = 0.22f;

        /// <summary>Leaf depth in world units — thicker than wall SegmentThickness so the open door
        /// reads as a slab, not a paper plane (screenshot 2026-08-12).</summary>
        private const float DoorLeafThickness = 0.28f;

        /// <summary>Open swing around the hinge Y axis (degrees). Negative = swing the leaf clear of
        /// the wall gap in the leaf's local space.</summary>
        private const float DoorOpenYawDegrees = -95f;

        /// <summary>Real-time seconds for the hinge arc — playtest 2026-08-11: instant snap read as a
        /// cabinet teleporting out of the wall circle, not a door swinging.</summary>
        private const float DoorSwingSeconds = 0.38f;

        // C60 vibrancy pass: both were near-black, contributing to the void reading as flat dark rather
        // than a rich contrast frame around a warm board. Lightened and warmed while staying dark enough
        // to read as void, not floor.
        private static readonly Color VoidApronColor = new Color(0.13f, 0.11f, 0.10f); // was (0.05, 0.045, 0.055)
        private static readonly Color VoidClutterColor = new Color(0.20f, 0.16f, 0.12f); // was (0.11, 0.09, 0.08)

        private ArenaBoard _model;
        private readonly List<DoorVisual> _doorVisuals = new List<DoorVisual>();

        public ArenaBoard Model => _model;

        /// <summary>Current map's room-rectangle data — see <see cref="MapLayout"/>. Set by
        /// <see cref="Build(ArenaBoard, MapLayout)"/>; other room-bounds-dependent consumers
        /// (<see cref="BoardReflectionProbes"/>) read this instead of restating their own literals.</summary>
        public MapLayout Layout { get; private set; }

        public Vector3 CenterWorld => _model == null
            ? transform.position
            : WorldFromPlanar(new PlanarPosition(
                (_model.MinX + _model.MaxX) * 0.5f,
                (_model.MinY + _model.MaxY) * 0.5f));

        /// <summary>Back-compat overload for every existing test/call site that doesn't care about
        /// multi-map layout data — defaults to the original map's room rectangles.</summary>
        public void Build(ArenaBoard model)
        {
            Build(model, MapDefinitions.FreightYard());
        }

        public void Build(ArenaBoard model, MapLayout layout)
        {
            _model = model;
            Layout = layout;
            _doorVisuals.Clear();

            for (int i = transform.childCount - 1; i >= 0; i--)
            {
                DestroyImmediate(transform.GetChild(i).gameObject);
            }

            PlaceRoomFloors(model);
            PlaceTerrainEdge(model);
            PlaceVoidDressing(model);
            PlaceRoomDressing(model);

            for (int i = 0; i < model.Walls.Count; i++)
            {
                PlaceSegmentBox($"Wall_{i}", model.Walls[i], BoardSurfaceMaterials.BrickWall, WallHeight);
            }

            for (int i = 0; i < model.Doors.Count; i++)
            {
                Door door = model.Doors[i];
                DoorVisual visual = PlaceDoorMesh($"Door_{i}", door);
                ApplyDoorVisualState(visual, model.GetDoorState(door), animate: false);
                _doorVisuals.Add(visual);
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
                if (visual.Root == null)
                {
                    continue;
                }

                DoorState state = overrideStates != null && overrideStates.TryGetValue(visual.Door, out DoorState preview)
                    ? preview
                    : _model.GetDoorState(visual.Door);
                ApplyDoorVisualState(visual, state, animate: true);
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
        /// C45 room zones as distinct urban surfaces (Yard asphalt / Hall wood / Vault marble-concrete /
        /// flank approach lanes). Bounds match GameBootstrap.BuildBoard — presentation only.
        /// </summary>
        private void PlaceRoomFloors(ArenaBoard model)
        {
            var floors = new GameObject("RoomFloors");
            floors.transform.SetParent(transform, false);

            // Room rectangles come from Layout (MapDefinitions) now, not restated per-map here —
            // this used to be the first of three independent copies of the same room bounds
            // (BoardReflectionProbes and GameBootstrap's wall placement being the other two).
            IReadOnlyList<MapRoom> rooms = Layout.Rooms;
            if (rooms != null)
            {
                for (int i = 0; i < rooms.Count; i++)
                {
                    MapRoom room = rooms[i].ClampTo(model);
                    if (!room.IsValid)
                    {
                        continue;
                    }

                    PlaceFloorSlab(
                        floors.transform,
                        room.Name + "Floor",
                        room.MinX,
                        room.MinY,
                        room.MaxX,
                        room.MaxY,
                        SurfaceMaterialFor(room.SurfaceRole));
                }
            }

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

        private static Material SurfaceMaterialFor(MapSurfaceRole role)
        {
            switch (role)
            {
                case MapSurfaceRole.Yard:
                    return BoardSurfaceMaterials.YardFloor;
                case MapSurfaceRole.Hall:
                    return BoardSurfaceMaterials.HallFloor;
                case MapSurfaceRole.Vault:
                    return BoardSurfaceMaterials.VaultFloor;
                default:
                    return BoardSurfaceMaterials.FlankFloor;
            }
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
        /// Presentation-only Quaternius interior dressing + warm glass panes. Placed clear of door
        /// corridors and spawn lanes so they never read as blockers (C40 — no pawn collision anyway;
        /// these also strip colliders so taps pass through to the ground underlay).
        /// </summary>
        private void PlaceRoomDressing(ArenaBoard model)
        {
            var root = new GameObject("RoomDressing");
            root.transform.SetParent(transform, false);

            // Yard — approach clutter at the edges, leave the (4,*) spine clear for attacker.
            PlaceInteriorProp(root.transform, "Yard_Cabinet_W", "Cabinet", new PlanarPosition(0.7f, 1.2f),
                0.55f, 12f);
            PlaceInteriorProp(root.transform, "Yard_Cabinet_E", "Cabinet", new PlanarPosition(7.3f, 1.4f),
                0.55f, -18f);
            PlaceInteriorProp(root.transform, "Yard_Chair_W", "Chair", new PlanarPosition(1.1f, 2.8f),
                0.45f, 40f);
            PlaceInteriorProp(root.transform, "Yard_Table_E", "Table", new PlanarPosition(6.9f, 2.6f),
                0.5f, 0f);

            // Hall — cover furniture + window frames on the side walls.
            PlaceInteriorProp(root.transform, "Hall_Cover_SW", "Cabinet", new PlanarPosition(2.55f, 4.55f),
                0.5f, 8f);
            PlaceInteriorProp(root.transform, "Hall_Cover_NE", "Shelf", new PlanarPosition(5.45f, 6.4f),
                0.55f, -22f);
            PlaceInteriorProp(root.transform, "Hall_Window_W", "WindowSmall", new PlanarPosition(2.08f, 5.5f),
                0.7f, 90f, yLift: 0.35f);
            PlaceInteriorProp(root.transform, "Hall_Window_E", "WindowSmall", new PlanarPosition(5.92f, 5.5f),
                0.7f, 90f, yLift: 0.35f);
            PlaceInteriorProp(root.transform, "Hall_Light", "LightCeiling", new PlanarPosition(4f, 5.5f),
                0.45f, 0f, yLift: WallHeight * 0.92f);
            // Keep warm emissive panes behind the window frames for the lit-window read.
            PlaceWindowPane(root.transform, "Hall_Glow_W", new PlanarPosition(2.05f, 5.5f),
                new Vector3(0.03f, 0.4f, 0.55f), 0.55f);
            PlaceWindowPane(root.transform, "Hall_Glow_E", new PlanarPosition(5.95f, 5.5f),
                new Vector3(0.03f, 0.4f, 0.55f), 0.55f);

            // Vault — shelving / depth dressing north of Door #2.
            PlaceInteriorProp(root.transform, "Vault_Bookshelf_W", "Bookshelf", new PlanarPosition(1.2f, 8.8f),
                0.85f, 0f);
            PlaceInteriorProp(root.transform, "Vault_Shelf_E", "ShelfLarge", new PlanarPosition(6.8f, 8.8f),
                0.85f, 180f);
            PlaceInteriorProp(root.transform, "Vault_Cabinet", "Cabinet", new PlanarPosition(4.0f, 9.3f),
                0.55f, 15f);
            PlaceInteriorProp(root.transform, "Vault_Window_N", "WindowLarge", new PlanarPosition(4f, 9.88f),
                0.9f, 0f, yLift: 0.35f);
            PlaceInteriorProp(root.transform, "Vault_Light", "LightCeilingAlt", new PlanarPosition(4f, 8.5f),
                0.45f, 0f, yLift: WallHeight * 0.92f);
            PlaceWindowPane(root.transform, "Vault_Glow_N", new PlanarPosition(4f, 9.85f),
                new Vector3(1.0f, 0.35f, 0.03f), 0.55f);

            _ = model;
        }

        private void PlaceInteriorProp(
            Transform parent,
            string name,
            string resourceName,
            PlanarPosition planar,
            float uniformScale,
            float yawDegrees,
            float yLift = 0f)
        {
            GameObject prefab = Resources.Load<GameObject>("Interior/" + resourceName);
            if (prefab == null)
            {
                // Prefabs not imported yet — keep a muted cube so the scene still builds in tests.
                PlaceProp(parent, name, planar, new Vector3(uniformScale, uniformScale, uniformScale),
                    Mathf.Max(0.15f, yLift), yawDegrees, BoardSurfaceMaterials.PropCrate);
                return;
            }

            GameObject prop = Object.Instantiate(prefab, parent, false);
            prop.name = name;
            prop.transform.localPosition = LocalFromPlanar(planar) + new Vector3(0f, yLift, 0f);
            prop.transform.localScale = Vector3.one * (uniformScale * WorldScale);
            prop.transform.localRotation = Quaternion.Euler(0f, yawDegrees, 0f);
            StripCollidersRecursive(prop);
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
        /// Door leaf fitted to the wall-segment gap. Open/closed still uses the standing green/red
        /// state tint (playtest 2026-08-07) plus a Y-hinge swing. Mesh fit goes through
        /// <see cref="DoorLeafFitter"/> so nappin width-on-Z leaves hinge correctly.
        /// </summary>
        private DoorVisual PlaceDoorMesh(string name, Door door)
        {
            Segment segment = door.Segment;
            float dx = segment.B.X - segment.A.X;
            float dy = segment.B.Y - segment.A.Y;
            float length = Mathf.Sqrt((dx * dx) + (dy * dy));
            if (length < 1e-4f)
            {
                length = SegmentThickness;
            }

            float height = WallHeight * 0.92f;
            PlanarPosition mid = PlanarPosition.Lerp(segment.A, segment.B, 0.5f);
            float yaw = Mathf.Atan2(-dy, dx) * Mathf.Rad2Deg;

            var root = new GameObject(name);
            root.transform.SetParent(transform, false);
            root.transform.localPosition = LocalFromPlanar(mid);
            root.transform.localRotation = Quaternion.Euler(0f, yaw, 0f);

            // Hinge at the segment's A-end (−X in root space after yaw alignment).
            var hinge = new GameObject("Hinge");
            hinge.transform.SetParent(root.transform, false);
            hinge.transform.localPosition = new Vector3(-length * 0.5f * WorldScale, 0f, 0f);

            float leafWidth = length * WorldScale;
            float leafThickness = DoorLeafThickness * WorldScale;

            // Static wood casing (jambs + lintel) on the root — import strips nappin's door_frame so
            // the leaf alone read as a free pillar in the wall gap (screenshot 2026-08-12).
            PlaceDoorCasing(root.transform, leafWidth, height);

            GameObject prefab = Resources.Load<GameObject>("Interior/Door");
            Transform leaf;
            MeshRenderer[] renderers;
            Color[][] originalColors;
            if (prefab != null)
            {
                GameObject leafGo = Object.Instantiate(prefab, hinge.transform, false);
                leafGo.name = "Leaf";
                // Runtime bounds fit — nappin (Prb)Door is width-on-Z; import normalize alone used to
                // treat thickness as width and the leaf spun like a cabinet (image-14 playtest).
                DoorLeafFitter.FitUnderHinge(
                    hinge.transform, leafGo.transform, leafWidth, height, leafThickness);
                leaf = leafGo.transform;
                renderers = leafGo.GetComponentsInChildren<MeshRenderer>();
                originalColors = CaptureRendererColors(renderers);
                StripCollidersRecursive(leafGo);
            }
            else
            {
                // Fallback tinted box if import hasn't run — same footprint as the old placeholder.
                GameObject box = GameObject.CreatePrimitive(PrimitiveType.Cube);
                box.name = "LeafFallback";
                box.transform.SetParent(hinge.transform, false);
                box.transform.localPosition = new Vector3(length * 0.5f * WorldScale, height * 0.5f, 0f);
                box.transform.localScale = new Vector3(leafWidth, height, leafThickness);
                StripCollider(box);
                leaf = box.transform;
                renderers = new[] { box.GetComponent<MeshRenderer>() };
                originalColors = CaptureRendererColors(renderers);
            }

            // Vent/Breach reuse the exact same leaf geometry and open/closed state-tint pipeline
            // (ApplyDoorVisualState) as a Standard door — only the *base* color they blend against
            // differs, so the kind reads clearly even under the green/red state tint. No new mesh
            // assets needed for this pass; geometry itself differs too since a vent/breach segment is
            // authored narrower at the call site in GameBootstrap, same leaf-scaling code as any door.
            originalColors = KindTintOverride(door.Kind, originalColors);

            return new DoorVisual(door, root, hinge.transform, leaf, renderers, originalColors);
        }

        private void PlaceDoorCasing(Transform root, float leafWidth, float height)
        {
            float jamb = 0.09f * WorldScale;
            float depth = Mathf.Max(SegmentThickness, DoorLeafThickness) * WorldScale * 1.15f;
            Material mat = BoardSurfaceMaterials.WoodEdge;

            PlaceDoorCasingPart(
                root, "Jamb_A",
                new Vector3((-leafWidth * 0.5f) - (jamb * 0.5f), height * 0.5f, 0f),
                new Vector3(jamb, height, depth),
                mat);
            PlaceDoorCasingPart(
                root, "Jamb_B",
                new Vector3((leafWidth * 0.5f) + (jamb * 0.5f), height * 0.5f, 0f),
                new Vector3(jamb, height, depth),
                mat);
            PlaceDoorCasingPart(
                root, "Lintel",
                new Vector3(0f, height + (jamb * 0.5f), 0f),
                new Vector3(leafWidth + (jamb * 2f), jamb, depth),
                mat);
        }

        private static void PlaceDoorCasingPart(
            Transform parent, string name, Vector3 localPosition, Vector3 localScale, Material material)
        {
            var part = GameObject.CreatePrimitive(PrimitiveType.Cube);
            part.name = name;
            part.transform.SetParent(parent, false);
            part.transform.localPosition = localPosition;
            part.transform.localScale = localScale;
            part.GetComponent<MeshRenderer>().sharedMaterial = material;
            StripCollider(part);
        }

        private static readonly Color VentBaseTint = new Color(0.55f, 0.58f, 0.62f);
        private static readonly Color BreachBaseTint = new Color(0.42f, 0.32f, 0.24f);

        private static Color[][] KindTintOverride(DoorKind kind, Color[][] captured)
        {
            Color tint;
            switch (kind)
            {
                case DoorKind.Vent:
                    tint = VentBaseTint;
                    break;
                case DoorKind.Breach:
                    tint = BreachBaseTint;
                    break;
                default:
                    return captured;
            }

            var overridden = new Color[captured.Length][];
            for (int i = 0; i < captured.Length; i++)
            {
                overridden[i] = new Color[captured[i].Length];
                for (int m = 0; m < captured[i].Length; m++)
                {
                    overridden[i][m] = tint;
                }
            }

            return overridden;
        }

        private static Color[][] CaptureRendererColors(MeshRenderer[] renderers)
        {
            var colors = new Color[renderers.Length][];
            for (int i = 0; i < renderers.Length; i++)
            {
                Material[] shared = renderers[i].sharedMaterials;
                colors[i] = new Color[shared.Length];
                for (int m = 0; m < shared.Length; m++)
                {
                    Material mat = shared[m];
                    colors[i][m] = mat != null && mat.HasProperty("_BaseColor")
                        ? mat.GetColor("_BaseColor")
                        : (mat != null ? mat.color : Color.white);
                }
            }

            return colors;
        }

        private void ApplyDoorVisualState(DoorVisual visual, DoorState state, bool animate)
        {
            bool open = state == DoorState.Open;
            Quaternion target = Quaternion.Euler(0f, open ? DoorOpenYawDegrees : 0f, 0f);

            // PLAYBACK_CONTRACT: same tape-derived state must not restart timed FX every ApplyTime
            // tick (playtest 2026-08-12 — doors only finished when the Execute scrubber stopped).
            if (visual.DisplayedState == state)
            {
                if (visual.Hinge != null && visual.SwingRoutine == null)
                {
                    visual.Hinge.localRotation = target;
                }

                return;
            }

            ApplyDoorTint(visual, open ? DoorOpenColor : DoorClosedColor);

            if (visual.Hinge == null)
            {
                visual.DisplayedState = state;
                return;
            }

            visual.DisplayedState = state;

            if (!animate || !isActiveAndEnabled)
            {
                if (visual.SwingRoutine != null)
                {
                    StopCoroutine(visual.SwingRoutine);
                    visual.SwingRoutine = null;
                }

                visual.Hinge.localRotation = target;
                return;
            }

            if (visual.SwingRoutine != null)
            {
                StopCoroutine(visual.SwingRoutine);
            }

            visual.SwingRoutine = StartCoroutine(SwingDoorHinge(visual, target));
        }

        private static void ApplyDoorTint(DoorVisual visual, Color tint)
        {
            if (visual.Renderers == null)
            {
                return;
            }

            for (int i = 0; i < visual.Renderers.Length; i++)
            {
                MeshRenderer renderer = visual.Renderers[i];
                if (renderer == null)
                {
                    continue;
                }

                // Instance materials so open/closed tint doesn't mutate the shared prefab mats.
                Material[] mats = renderer.materials;
                Color[] originals = visual.OriginalColors != null && i < visual.OriginalColors.Length
                    ? visual.OriginalColors[i]
                    : null;

                for (int m = 0; m < mats.Length; m++)
                {
                    Material mat = mats[m];
                    if (mat == null)
                    {
                        continue;
                    }

                    Color baseColor = originals != null && m < originals.Length ? originals[m] : Color.white;
                    Color mixed = Color.Lerp(baseColor, tint, DoorStateTintBlend);
                    mixed.a = 1f;
                    if (mat.HasProperty("_BaseColor"))
                    {
                        mat.SetColor("_BaseColor", mixed);
                    }

                    mat.color = mixed;
                }

                renderer.materials = mats;
            }
        }

        private IEnumerator SwingDoorHinge(DoorVisual visual, Quaternion target)
        {
            Transform hinge = visual.Hinge;
            Quaternion from = hinge.localRotation;
            float elapsed = 0f;
            while (elapsed < DoorSwingSeconds && hinge != null)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / DoorSwingSeconds);
                // Smoothstep — eases into/out of the arc so it reads as a hinged swing, not a lerp teleport.
                float s = t * t * (3f - (2f * t));
                hinge.localRotation = Quaternion.Slerp(from, target, s);
                yield return null;
            }

            if (hinge != null)
            {
                hinge.localRotation = target;
            }

            visual.SwingRoutine = null;
        }

        private static void StripCollidersRecursive(GameObject go)
        {
            Collider[] cols = go.GetComponentsInChildren<Collider>();
            for (int i = 0; i < cols.Length; i++)
            {
                Object.Destroy(cols[i]);
            }
        }

        /// <summary>
        /// Dark recessed apron beyond the board plus a handful of ithappy Cartoon City Free prefab
        /// fragments (sidewalk slab, trash can, broken lamp post, debris) reading as scattered
        /// street/ruin debris at the board's edge — not a continuous city block, see
        /// docs/ART_PACK_RESEARCH.md and the void-city-dressing brief. Tilt-shift DoF blurs them
        /// further — they only need to read as "something in the void," not detailed props.
        /// Same positions/rotations as the original placeholder primitive-cube pass; only the
        /// visual (real prefab vs. tinted cube) and per-item vertical anchoring changed.
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

            // Debris pile (was Clutter_Block) — Trash_04, bottom-pivoted, raw height 0.876.
            PlaceVoidCityProp(
                voidRoot.transform,
                "Clutter_Block",
                "DebrisPile",
                new PlanarPosition(model.MinX - 1.35f, model.MinY + (depth * 0.28f)),
                0.571f,
                -0.295f,
                18f,
                new Vector3(0.9f * WorldScale, 0.35f, 0.55f * WorldScale),
                -0.12f);
            // Broken sidewalk slab (was Clutter_Slab) — Set_B_Tiles_05, center-pivoted, 15x0.2x15.
            PlaceVoidCityProp(
                voidRoot.transform,
                "Clutter_Slab",
                "SidewalkSlab",
                new PlanarPosition(model.MaxX + 1.45f, model.MaxY - (depth * 0.22f)),
                0.12f,
                -0.14f,
                -25f,
                new Vector3(1.2f * WorldScale, 0.18f, 0.7f * WorldScale),
                -0.14f);
            // Trash can (was Clutter_Can) — Trash_Can_07, bottom-pivoted, raw height 1.645.
            PlaceVoidCityProp(
                voidRoot.transform,
                "Clutter_Can",
                "TrashCan",
                new PlanarPosition(model.MinX + (width * 0.18f), model.MaxY + 1.55f),
                0.334f,
                -0.355f,
                40f,
                new Vector3(0.35f * WorldScale, 0.55f, 0.35f * WorldScale),
                -0.08f);
            // Flattened debris (was Clutter_Tray) — Trash_06, bottom-pivoted, raw height 1.242.
            PlaceVoidCityProp(
                voidRoot.transform,
                "Clutter_Tray",
                "DebrisFlat",
                new PlanarPosition(model.MaxX - (width * 0.25f), model.MinY - 1.4f),
                0.282f,
                -0.22f,
                8f,
                new Vector3(0.85f * WorldScale, 0.12f, 0.45f * WorldScale),
                -0.16f);
            // Broken lamp-post stub (was Clutter_Peg) — Spotlight_02, bottom-pivoted, raw height 7.103.
            PlaceVoidCityProp(
                voidRoot.transform,
                "Clutter_Peg",
                "StreetlampBroken",
                new PlanarPosition(model.MinX - 1.6f, model.MaxY - (depth * 0.35f)),
                0.155f,
                -0.4f,
                -12f,
                new Vector3(0.22f * WorldScale, 0.7f, 0.22f * WorldScale),
                -0.05f);
        }

        /// <summary>
        /// Instantiates a real ithappy Cartoon City Free prefab from <c>Resources/CityDressing</c> at
        /// a void-edge dressing slot. Mirrors <see cref="PlaceInteriorProp"/>'s
        /// Resources.Load-with-fallback shape: if the prefab hasn't been baked yet (
        /// <see cref="LogiCard.Art.Editor.CityDressingPackImportTool"/> hasn't run), falls back to the
        /// original tinted primitive cube so the scene still builds in tests.
        /// </summary>
        private void PlaceVoidCityProp(
            Transform parent,
            string name,
            string resourceName,
            PlanarPosition planarCenter,
            float uniformScale,
            float localY,
            float yawDegrees,
            Vector3 fallbackScale,
            float fallbackLocalY)
        {
            GameObject prefab = Resources.Load<GameObject>("CityDressing/" + resourceName);
            if (prefab == null)
            {
                PlaceVoidClutterFallback(parent, name, planarCenter, fallbackScale, fallbackLocalY, yawDegrees);
                return;
            }

            GameObject prop = Object.Instantiate(prefab, parent, false);
            prop.name = name;
            prop.transform.localPosition = LocalFromPlanar(planarCenter) + new Vector3(0f, localY, 0f);
            prop.transform.localScale = Vector3.one * (uniformScale * WorldScale);
            prop.transform.localRotation = Quaternion.Euler(0f, yawDegrees, 0f);
            StripCollidersRecursive(prop);
        }

        /// <summary>Pre-import fallback — the original tinted primitive cube, kept so builds/tests
        /// stay green before <see cref="LogiCard.Art.Editor.CityDressingPackImportTool"/> has run.</summary>
        private void PlaceVoidClutterFallback(
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

        private sealed class DoorVisual
        {
            public Door Door { get; }

            public GameObject Root { get; }

            public Transform Hinge { get; }

            public Transform Leaf { get; }

            public MeshRenderer[] Renderers { get; }

            public Color[][] OriginalColors { get; }

            /// <summary>Last Open/Closed applied to tint + hinge target (for skip-restart).</summary>
            public DoorState? DisplayedState { get; set; }

            /// <summary>In-flight hinge arc, if any — stopped/replaced on a new state change.</summary>
            public Coroutine SwingRoutine { get; set; }

            public DoorVisual(
                Door door,
                GameObject root,
                Transform hinge,
                Transform leaf,
                MeshRenderer[] renderers,
                Color[][] originalColors)
            {
                Door = door;
                Root = root;
                Hinge = hinge;
                Leaf = leaf;
                Renderers = renderers;
                OriginalColors = originalColors;
                DisplayedState = null;
                SwingRoutine = null;
            }
        }
    }
}
