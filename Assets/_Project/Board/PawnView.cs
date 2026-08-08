using LogiCard.Sim;
using UnityEngine;

namespace LogiCard.Board
{
    /// <summary>
    /// Which clay-like silhouette a pawn reads as (ART_DIRECTION Demo art floor: "Distinct
    /// clay-like pawn silhouettes, Scout vs Juggernaut readable"). Matches the archetype naming
    /// in <see cref="LogiCard.Characters.CharacterData"/> (Scout/Juggernaut assets) — this enum
    /// only drives the primitive shape assembly, it does not read the ScriptableObject.
    /// </summary>
    public enum PawnBuild
    {
        Scout,
        Juggernaut,
    }

    /// <summary>
    /// Renders one character. The view is dumb on purpose: it only samples a
    /// ScheduledPath at a Time Resource second, exactly like playback will read a tape (TDD 5).
    /// </summary>
    public sealed class PawnView : MonoBehaviour
    {
        private const float BodyHeight = 0.8f;

        /// <summary>
        /// Real-world seconds between visible pose updates while riding an already-armed tape —
        /// ART_DIRECTION §2's stepped 8-12fps ("pose snaps, not blends"); ~10fps sits mid-band.
        /// A fresh <see cref="SetPath"/> (new draft preview, newly armed tape, Disarm's carry-point
        /// reset) always applies its very next <see cref="ApplyTime"/> immediately and exactly — only
        /// repeated scrubs/ticks against the *same* path get held between steps. That keeps interactive
        /// draft preview and key poses (path start/end) exact while playback of a locked-in tape reads
        /// as stop-motion instead of a 60fps glide.
        /// </summary>
        private const float StepIntervalSeconds = 1f / 10f;

        private BoardView _board;
        private Transform _visual;
        private float _nextStepRealTime;
        private bool _forceNextApply;

        public ScheduledPath Path { get; private set; }

        public void Init(BoardView board, Color color, ScheduledPath path, PawnBuild build = PawnBuild.Scout)
        {
            _board = board;
            Path = path;

            var visual = new GameObject("Visual");
            visual.transform.SetParent(transform, false);
            _visual = visual.transform;

            Material material = PrimitiveMaterialFactory.Tinted(color);
            if (build == PawnBuild.Juggernaut)
            {
                BuildJuggernaut(_visual, material);
            }
            else
            {
                BuildScout(_visual, material);
            }

            _forceNextApply = true;
            ApplyTime(0f);
        }

        /// <summary>
        /// Lean/tall silhouette (ART_DIRECTION: "Scout vs Juggernaut readable") — narrow capsule
        /// torso plus a small head, reading as agile/recon.
        /// </summary>
        private static void BuildScout(Transform parent, Material material)
        {
            AddPrimitive(parent, PrimitiveType.Capsule, "Torso", material,
                localScale: new Vector3(0.38f, BodyHeight * 0.5f, 0.38f),
                localPosition: new Vector3(0f, BodyHeight * 0.5f, 0f));

            const float headDiameter = 0.24f;
            AddPrimitive(parent, PrimitiveType.Sphere, "Head", material,
                localScale: new Vector3(headDiameter, headDiameter, headDiameter),
                localPosition: new Vector3(0f, BodyHeight + (headDiameter * 0.5f), 0f));
        }

        /// <summary>
        /// Wide/squat silhouette with blocky shoulder pads — reads as heavy/armored, deliberately
        /// distinct from the Scout's lean profile at a glance (top-down diorama readability).
        /// </summary>
        private static void BuildJuggernaut(Transform parent, Material material)
        {
            const float torsoHeight = BodyHeight * 0.85f;
            AddPrimitive(parent, PrimitiveType.Capsule, "Torso", material,
                localScale: new Vector3(0.62f, torsoHeight * 0.5f, 0.62f),
                localPosition: new Vector3(0f, torsoHeight * 0.5f, 0f));

            const float headSize = 0.26f;
            AddPrimitive(parent, PrimitiveType.Cube, "Head", material,
                localScale: new Vector3(headSize, headSize, headSize),
                localPosition: new Vector3(0f, torsoHeight + (headSize * 0.5f), 0f));

            const float padSize = 0.26f;
            const float shoulderY = torsoHeight * 0.78f;
            const float shoulderX = 0.42f;
            AddPrimitive(parent, PrimitiveType.Cube, "ShoulderPadLeft", material,
                localScale: new Vector3(padSize, padSize, padSize),
                localPosition: new Vector3(-shoulderX, shoulderY, 0f));
            AddPrimitive(parent, PrimitiveType.Cube, "ShoulderPadRight", material,
                localScale: new Vector3(padSize, padSize, padSize),
                localPosition: new Vector3(shoulderX, shoulderY, 0f));
        }

        private static void AddPrimitive(
            Transform parent,
            PrimitiveType type,
            string name,
            Material material,
            Vector3 localScale,
            Vector3 localPosition)
        {
            var go = GameObject.CreatePrimitive(type);
            go.name = name;
            go.transform.SetParent(parent, false);
            go.transform.localScale = localScale;
            go.transform.localPosition = localPosition;
            go.GetComponent<MeshRenderer>().sharedMaterial = material;
            Object.Destroy(go.GetComponent<Collider>());
        }

        public void SetPath(ScheduledPath path)
        {
            Path = path;
            _forceNextApply = true;
        }

        public void ApplyTime(float timeResourceSeconds)
        {
            if (_board == null || Path == null)
            {
                return;
            }

            // Path start/end are key poses, not in-between frames — always land on them exactly
            // even mid-throttle, so playback never reads as falling short of its destination.
            bool boundary = timeResourceSeconds <= 0f || timeResourceSeconds >= Path.EndSeconds;

            if (!_forceNextApply && !boundary && Time.unscaledTime < _nextStepRealTime)
            {
                return;
            }

            _forceNextApply = false;
            _nextStepRealTime = Time.unscaledTime + StepIntervalSeconds;
            transform.position = _board.WorldFromPlanar(Path.Evaluate(timeResourceSeconds));
        }

        public void SetHighlighted(bool on)
        {
            if (_visual == null)
            {
                return;
            }

            float scale = on ? 1.15f : 1f;
            _visual.localScale = new Vector3(scale, scale, scale);
        }
    }
}
