using System.Collections.Generic;
using LogiCard.Sim;
using UnityEngine;

namespace LogiCard.Board
{
    /// <summary>Which map is currently loaded. Selection UI doesn't exist yet — <see cref="GameBootstrap"/>
    /// picks a constant default; this enum exists so that wiring has a stable name to hang off later.</summary>
    public enum MapId
    {
        FreightYard,
        RailPlatform,
        VaultComplex,
    }

    /// <summary>
    /// Which of the four existing <see cref="BoardSurfaceMaterials"/> floor looks a room reads as,
    /// by tactical role rather than by this specific map's room names — lets new maps reuse the same
    /// four PBR materials (no new art asset work needed to add a map) while still getting sensible
    /// per-room variation (open ground vs. a tighter interior vs. a polished objective space).
    /// </summary>
    public enum MapSurfaceRole
    {
        Yard,
        Hall,
        Vault,
        Flank,
    }

    /// <summary>One named room rectangle, shared by every consumer that used to restate room bounds
    /// independently (<see cref="BoardView"/>'s floor slabs, <see cref="BoardReflectionProbes"/>'s
    /// probe placement) — the exact duplication risk flagged when this project had only one map.</summary>
    public readonly struct MapRoom
    {
        public string Name { get; }

        public float MinX { get; }

        public float MinY { get; }

        public float MaxX { get; }

        public float MaxY { get; }

        public MapSurfaceRole SurfaceRole { get; }

        public MapRoom(string name, float minX, float minY, float maxX, float maxY, MapSurfaceRole surfaceRole)
        {
            Name = name;
            MinX = minX;
            MinY = minY;
            MaxX = maxX;
            MaxY = maxY;
            SurfaceRole = surfaceRole;
        }

        public bool IsValid => MaxX > MinX && MaxY > MinY;

        /// <summary>Clamp to whatever board is actually built (defensive only — the real board
        /// always matches these bounds; guards a differently-sized model).</summary>
        public MapRoom ClampTo(ArenaBoard model)
        {
            float minX = Mathf.Clamp(MinX, model.MinX, model.MaxX);
            float maxX = Mathf.Clamp(MaxX, model.MinX, model.MaxX);
            float minY = Mathf.Clamp(MinY, model.MinY, model.MaxY);
            float maxY = Mathf.Clamp(MaxY, model.MinY, model.MaxY);
            return new MapRoom(Name, minX, minY, maxX, maxY, SurfaceRole);
        }
    }

    /// <summary>The one piece of per-map data multiple presentation-layer consumers need in common.
    /// Deliberately *not* a data-driven map format (the human explicitly chose hand-authored maps,
    /// one bespoke `BuildXxx()` method per map in <see cref="GameBootstrap"/>) — this only de-dupes
    /// the specific fact (room rectangles) that was previously triplicated and had already caused a
    /// documented "must be kept in sync by hand" risk in <see cref="BoardReflectionProbes"/>.</summary>
    public readonly struct MapLayout
    {
        public MapId Id { get; }

        public IReadOnlyList<MapRoom> Rooms { get; }

        public MapLayout(MapId id, IReadOnlyList<MapRoom> rooms)
        {
            Id = id;
            Rooms = rooms;
        }
    }

    /// <summary>Hand-authored per-map room layouts. Walls/doors/vents/breaches/spawns/AI stay in
    /// <see cref="GameBootstrap"/> (they're inherently one-off per map, C36/vent numerics aside) —
    /// this only centralizes room *rectangles*, the fact that was actually at risk of drifting.</summary>
    public static class MapDefinitions
    {
        public static MapLayout FreightYard()
        {
            return new MapLayout(MapId.FreightYard, new[]
            {
                new MapRoom("Yard", 0f, 0f, 8f, 4f, MapSurfaceRole.Yard),
                new MapRoom("Hall", 2f, 4f, 6f, 7f, MapSurfaceRole.Hall),
                new MapRoom("Vault", 0f, 7f, 8f, 10f, MapSurfaceRole.Vault),
                new MapRoom("FlankWest", 0f, 4f, 2f, 7f, MapSurfaceRole.Flank),
                new MapRoom("FlankEast", 6f, 4f, 8f, 7f, MapSurfaceRole.Flank),
            });
        }

        public static MapLayout ForId(MapId id)
        {
            switch (id)
            {
                case MapId.FreightYard:
                    return FreightYard();
                default:
                    // RailPlatform/VaultComplex land in a follow-up checkpoint (docs/DRAFT_HANDOFF.md) —
                    // fail loudly instead of silently substituting the wrong map's geometry.
                    throw new System.NotImplementedException(
                        $"MapDefinitions.ForId({id}): this map's layout isn't authored yet.");
            }
        }
    }
}
