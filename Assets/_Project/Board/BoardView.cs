using LogiCard.Sim;
using UnityEngine;

namespace LogiCard.Board
{
    /// <summary>
    /// Scene view for the pure-C# <see cref="LogiCard.Sim.GridBoard"/>. The sim owns tile truth;
    /// this only draws primitives and converts coordinates to world space (C29: primitives
    /// may evoke the diorama for the demo).
    /// </summary>
    public sealed class BoardView : MonoBehaviour
    {
        public float TileSize = 1f;
        public float TileThickness = 0.12f;
        public float FloorSpacing = 2.5f;

        private GridBoard _model;

        public GridBoard Model => _model;

        public Vector3 CenterWorld => _model == null
            ? transform.position
            : WorldFromPlanar(new PlanarPosition((_model.Width - 1) * 0.5f, (_model.Height - 1) * 0.5f));

        public void Build(GridBoard model, Color lightTile, Color darkTile)
        {
            _model = model;

            Material light = CreateClayish(lightTile);
            Material dark = CreateClayish(darkTile);

            foreach (GridCoordinate coord in model.GetAllCoordinates())
            {
                var tile = GameObject.CreatePrimitive(PrimitiveType.Cube);
                tile.name = $"Tile_{coord.Floor}_{coord.X}_{coord.Y}";
                tile.transform.SetParent(transform, false);
                tile.transform.localPosition = LocalFromPlanar(new PlanarPosition(coord.X, coord.Y, coord.Floor));
                tile.transform.localScale = new Vector3(TileSize * 0.94f, TileThickness, TileSize * 0.94f);
                tile.GetComponent<MeshRenderer>().sharedMaterial = ((coord.X + coord.Y) % 2 == 0) ? light : dark;
                tile.AddComponent<TileMarker>().Init(coord);
            }
        }

        public Vector3 LocalFromPlanar(PlanarPosition p)
        {
            float floorHeight = p.Floor == Floor.Attic ? FloorSpacing : 0f;
            return new Vector3(p.X * TileSize, floorHeight, p.Y * TileSize);
        }

        public Vector3 WorldFromPlanar(PlanarPosition p)
        {
            return transform.TransformPoint(LocalFromPlanar(p));
        }

        public Vector3 WorldFromCoord(GridCoordinate c)
        {
            return WorldFromPlanar(new PlanarPosition(c.X, c.Y, c.Floor));
        }

        private static Material CreateClayish(Color color)
        {
            return PrimitiveMaterialFactory.Tinted(color);
        }
    }
}
