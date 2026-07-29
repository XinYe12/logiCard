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

        private BoardView _board;
        private Transform _body;

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

            ApplyTime(0f);
        }

        public void ApplyTime(float timeResourceSeconds)
        {
            if (_board == null || Path == null)
            {
                return;
            }

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
