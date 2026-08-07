using LogiCard.Sim;
using UnityEngine;

namespace LogiCard.Board
{
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
        private Transform _body;
        private float _nextStepRealTime;
        private bool _forceNextApply;

        public ScheduledPath Path { get; private set; }

        public void Init(BoardView board, Color color, ScheduledPath path)
        {
            _board = board;
            Path = path;

            var body = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            body.name = "Body";
            body.transform.SetParent(transform, false);
            body.transform.localScale = new Vector3(0.5f, BodyHeight * 0.5f, 0.5f);
            body.transform.localPosition = new Vector3(0f, BodyHeight * 0.5f, 0f);
            body.GetComponent<MeshRenderer>().sharedMaterial = PrimitiveMaterialFactory.Tinted(color);
            _body = body.transform;

            _forceNextApply = true;
            ApplyTime(0f);
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
            if (_body == null)
            {
                return;
            }

            float width = on ? 0.58f : 0.5f;
            _body.localScale = new Vector3(width, BodyHeight * 0.5f, width);
        }
    }
}
