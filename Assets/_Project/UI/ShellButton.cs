using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace LogiCard.UI
{
    /// <summary>Chrome tone for a shell (non-HUD) button. See <see cref="ShellButton"/>.</summary>
    public enum ShellButtonTone
    {
        /// <summary>The one thing the screen wants you to do — red clay face, cream label.</summary>
        Primary,

        /// <summary>Parchment face, ink label. The alternative action next to a Primary.</summary>
        Secondary,

        /// <summary>Dark slate face on the backdrop — nav arrows, unselected options, low-stakes exits.</summary>
        Quiet,
    }

    /// <summary>
    /// Press/hover behaviour for the shell's chunky "toy" button: a face sitting on a visible riser
    /// over a contact shadow, which drops into that shadow when pressed and lifts slightly on hover.
    ///
    /// Ported from <c>docs/ui-collection/button-gradient-pill.css</c> (Uiverse.io by Codecite, MIT) —
    /// there the whole button <c>translateY(3px)</c>s and its <c>box-shadow</c> disappears. uGUI has no
    /// box-shadow, so the shadow is a real sibling Image and the "drop" is the face travelling down the
    /// riser's own height. That is what makes the button read as a physical object with thickness
    /// rather than a coloured rectangle.
    ///
    /// Built by <see cref="UiFactory.CreateShellButton"/> — do not add this by hand, it expects the
    /// exact Shadow/Riser/Face child structure that factory creates.
    /// </summary>
    [RequireComponent(typeof(Button))]
    public sealed class ShellButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler,
        IPointerEnterHandler, IPointerExitHandler
    {
        private const float HoverLift = 2.5f;

        private RectTransform _body;
        private Image _faceImage;
        private Image _riserImage;
        private Image _shadowImage;
        private Text _label;
        private float _riser;
        private bool _pressed;
        private bool _hovered;

        /// <summary>Current tone — <see cref="SelectionGrid"/> flips this between Primary and Quiet.</summary>
        public ShellButtonTone Tone { get; private set; }

        /// <summary>The face+riser group; travels down into the shadow on press.</summary>
        internal void Bind(RectTransform body, Image faceImage, Image riserImage, Image shadowImage, Text label, float riser)
        {
            _body = body;
            _faceImage = faceImage;
            _riserImage = riserImage;
            _shadowImage = shadowImage;
            _label = label;
            _riser = riser;
            ApplyOffset();
        }

        /// <summary>Re-tints an already-built button. Used for selected/unselected option states.</summary>
        public void ApplyTone(ShellButtonTone tone)
        {
            Tone = tone;
            if (_faceImage == null)
            {
                return;
            }

            switch (tone)
            {
                case ShellButtonTone.Primary:
                    _faceImage.color = UiStyle.ShellPrimaryFace;
                    _riserImage.color = UiStyle.ShellPrimaryRiser;
                    if (_label != null)
                    {
                        _label.color = UiStyle.ShellPrimaryText;
                    }

                    break;
                case ShellButtonTone.Quiet:
                    _faceImage.color = UiStyle.ShellQuietFace;
                    _riserImage.color = UiStyle.ShellQuietRiser;
                    if (_label != null)
                    {
                        _label.color = UiStyle.ShellQuietText;
                    }

                    break;
                default:
                    _faceImage.color = UiStyle.ShellSecondaryFace;
                    _riserImage.color = UiStyle.ShellSecondaryRiser;
                    if (_label != null)
                    {
                        _label.color = UiStyle.ShellSecondaryText;
                    }

                    break;
            }
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            _pressed = true;
            ApplyOffset();
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            _pressed = false;
            ApplyOffset();
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            _hovered = true;
            ApplyOffset();
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            _hovered = false;
            _pressed = false;
            ApplyOffset();
        }

        private void OnDisable()
        {
            // A screen can be hidden mid-press (Confirm swaps screens on click); without this the face
            // stays sunk and the button looks stuck down the next time that screen is shown.
            _pressed = false;
            _hovered = false;
            ApplyOffset();
        }

        private void ApplyOffset()
        {
            if (_body == null)
            {
                return;
            }

            float y = _pressed ? -_riser : (_hovered ? HoverLift : 0f);
            _body.offsetMin = new Vector2(0f, y);
            _body.offsetMax = new Vector2(0f, y);

            if (_shadowImage != null)
            {
                Color c = UiStyle.ShellButtonShadow;
                // Pressed = sitting on its own shadow, so the shadow all but vanishes (the CSS ref
                // drops box-shadow to none on :hover; here the *press* is the settle).
                c.a *= _pressed ? 0.25f : 1f;
                _shadowImage.color = c;
            }
        }
    }
}
