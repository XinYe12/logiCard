using LogiCard.Sim;
using UnityEngine;
using UnityEngine.Rendering;

namespace LogiCard.Board
{
    /// <summary>
    /// Reflection Probes for the board's wet Yard/Hall/Vault floors (follow-up to C53's
    /// <see cref="BoardSurfaceMaterials"/> wet-dusk pass — SSR isn't available in this URP version,
    /// 17.5.0, so probes are the real reflection source). Mirrors <see cref="BoardWeatherPocket"/>'s
    /// pattern: a runtime component with a <c>Build(BoardView)</c> called once from
    /// <c>GameBootstrap</c>, not a persisted/baked asset — the board itself is built procedurally at
    /// runtime (no persistent editor scene exists to run a classic Lighting-window bake against), so
    /// "baked" here means <see cref="ReflectionProbeMode.Realtime"/> +
    /// <see cref="ReflectionProbeRefreshMode.OnAwake"/>: the probe renders its cubemap exactly once,
    /// right after the room it sits in finishes building, then never refreshes again — zero ongoing
    /// runtime cost, functionally equivalent to a baked probe for this static diorama.
    /// </summary>
    public sealed class BoardReflectionProbes : MonoBehaviour
    {
        // Room split mirrors BoardView.PlaceRoomFloors exactly — those bounds are gameplay-authored
        // in GameBootstrap.BuildBoard's wall placement (walls at y=4/y=7, x=2/x=6), not independently
        // invented here. Keep these in sync if that layout changes.
        private static readonly RoomBounds Yard = new RoomBounds(0f, 0f, 8f, 4f);
        private static readonly RoomBounds Hall = new RoomBounds(2f, 4f, 6f, 7f);
        private static readonly RoomBounds Vault = new RoomBounds(0f, 7f, 8f, 10f);

        private const int CubemapResolution = 128;
        private const float BlendDistance = 1.0f;

        /// <summary>
        /// Build (or rebuild) one probe per room over <paramref name="board"/>. Safe to call once
        /// from bootstrap after the board + lighting exist; destroys prior children first.
        /// </summary>
        public void Build(BoardView board)
        {
            for (int i = transform.childCount - 1; i >= 0; i--)
            {
                DestroyImmediate(transform.GetChild(i).gameObject);
            }

            if (board == null || board.Model == null)
            {
                return;
            }

            ArenaBoard model = board.Model;
            PlaceRoomProbe(board, "Probe_Yard", Yard.ClampTo(model));
            PlaceRoomProbe(board, "Probe_Hall", Hall.ClampTo(model));
            PlaceRoomProbe(board, "Probe_Vault", Vault.ClampTo(model));
        }

        private void PlaceRoomProbe(BoardView board, string name, RoomBounds room)
        {
            if (!room.IsValid)
            {
                return;
            }

            float width = (room.MaxX - room.MinX) * board.WorldScale;
            float depth = (room.MaxY - room.MinY) * board.WorldScale;
            float centerX = (room.MinX + room.MaxX) * 0.5f;
            float centerY = (room.MinY + room.MaxY) * 0.5f;

            // Probe sits at roughly eye height; box (influence volume) spans from just below the
            // floor to comfortably above the wall line so ceiling practicals and a slice of the
            // weather pocket above the board can factor into the parallax-corrected reflection.
            float eyeHeight = board.WallHeight * 0.6f;
            Vector3 probeWorldPos = board.WorldFromPlanar(new PlanarPosition(centerX, centerY))
                                     + new Vector3(0f, eyeHeight, 0f);

            var probeGo = new GameObject(name);
            probeGo.transform.SetParent(transform, false);
            probeGo.transform.position = probeWorldPos;

            var probe = probeGo.AddComponent<ReflectionProbe>();
            probe.mode = ReflectionProbeMode.Realtime;
            probe.refreshMode = ReflectionProbeRefreshMode.OnAwake;
            probe.timeSlicingMode = ReflectionProbeTimeSlicingMode.AllFacesAtOnce;
            probe.resolution = CubemapResolution;
            probe.hdr = true;
            probe.boxProjection = true;
            probe.intensity = 1f;
            probe.blendDistance = BlendDistance;
            probe.nearClipPlane = 0.05f;
            probe.farClipPlane = 60f;
            probe.size = new Vector3(width, board.WallHeight * 3.5f, depth);
            probe.center = new Vector3(0f, board.WallHeight * 1.1f, 0f);
        }

        private readonly struct RoomBounds
        {
            public float MinX { get; }

            public float MinY { get; }

            public float MaxX { get; }

            public float MaxY { get; }

            public RoomBounds(float minX, float minY, float maxX, float maxY)
            {
                MinX = minX;
                MinY = minY;
                MaxX = maxX;
                MaxY = maxY;
            }

            public bool IsValid => MaxX > MinX && MaxY > MinY;

            /// <summary>Clamp the authored room rectangle to whatever board is actually built (defensive
            /// only — the real board always matches these bounds; guards a differently-sized model).</summary>
            public RoomBounds ClampTo(ArenaBoard model)
            {
                float minX = Mathf.Clamp(MinX, model.MinX, model.MaxX);
                float maxX = Mathf.Clamp(MaxX, model.MinX, model.MaxX);
                float minY = Mathf.Clamp(MinY, model.MinY, model.MaxY);
                float maxY = Mathf.Clamp(MaxY, model.MinY, model.MaxY);
                return new RoomBounds(minX, minY, maxX, maxY);
            }
        }
    }
}
