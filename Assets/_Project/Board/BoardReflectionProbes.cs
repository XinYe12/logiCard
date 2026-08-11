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
        private const int CubemapResolution = 128;
        private const float BlendDistance = 1.0f;

        // Must match GameBootstrap.ConfigureCamera's cam.backgroundColor exactly — a ReflectionProbe
        // defaults to CameraClearFlags.Skybox with no skybox configured (this project deliberately has
        // none; ART_DIRECTION calls for a bounded dark void, not an open horizon), which left every
        // probe rendering a mismatched/undefined environment instead of the actual dark void the main
        // camera shows. That's almost certainly why the reflection retune read as invisible on a real
        // screenshot (2026-08-10) despite batchmode passing — nothing was verifying the probe's own
        // clear color, only that it built and ran without throwing.
        // C60 vibrancy pass: kept in sync with ConfigureCamera's retuned backgroundColor
        // (0.035,0.04,0.055 -> 0.06,0.055,0.06) — leaving this stale would have reintroduced the exact
        // probe/camera mismatch the comment above describes, just with the new color.
        private static readonly Color VoidBackgroundColor = new Color(0.06f, 0.055f, 0.06f);

        /// <summary>
        /// Build (or rebuild) one probe per room in <paramref name="board"/>'s <see cref="MapLayout"/>
        /// (<see cref="BoardView.Layout"/> — set by whichever map's <c>Build</c> call ran, not
        /// restated here independently the way this used to hardcode Yard/Hall/Vault). Safe to call
        /// once from bootstrap after the board + lighting exist; destroys prior children first.
        /// </summary>
        public void Build(BoardView board)
        {
            for (int i = transform.childCount - 1; i >= 0; i--)
            {
                DestroyImmediate(transform.GetChild(i).gameObject);
            }

            if (board == null || board.Model == null || board.Layout.Rooms == null)
            {
                return;
            }

            ArenaBoard model = board.Model;
            for (int i = 0; i < board.Layout.Rooms.Count; i++)
            {
                MapRoom room = board.Layout.Rooms[i].ClampTo(model);
                PlaceRoomProbe(board, "Probe_" + room.Name, room);
            }
        }

        private void PlaceRoomProbe(BoardView board, string name, MapRoom room)
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
            probe.clearFlags = ReflectionProbeClearFlags.SolidColor;
            probe.backgroundColor = VoidBackgroundColor;
            probe.intensity = 1f;
            probe.blendDistance = BlendDistance;
            probe.nearClipPlane = 0.05f;
            probe.farClipPlane = 60f;
            probe.size = new Vector3(width, board.WallHeight * 3.5f, depth);
            probe.center = new Vector3(0f, board.WallHeight * 1.1f, 0f);
        }
    }
}
