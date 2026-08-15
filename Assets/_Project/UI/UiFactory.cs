using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace LogiCard.UI
{
    /// <summary>
    /// Shared uGUI construction + dock row layout used by <see cref="ProgramHud"/>,
    /// <see cref="AppFlowController"/>, <see cref="ModalDialog"/>, and <see cref="SelectionGrid"/>.
    /// </summary>
    public sealed class UiFactory
    {
        public Font Font { get; }

        public UiFactory(Font font)
        {
            _font = font != null
                ? font
                : Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            Font = _font;
        }

        private readonly Font _font;

        public RectTransform CreatePanel(RectTransform parent, string name, Color color, Vector2 anchorMin, Vector2 anchorMax, Sprite sprite = null, Image.Type imageType = Image.Type.Simple)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Image));
            var rt = go.GetComponent<RectTransform>();
            rt.SetParent(parent, false);
            rt.anchorMin = anchorMin;
            rt.anchorMax = anchorMax;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;

            var image = go.GetComponent<Image>();
            image.color = color;
            if (sprite != null)
            {
                image.sprite = sprite;
                image.type = imageType;
            }

            return rt;
        }

        /// <summary>
        /// Layered shadow + border + face + inset-lip backing panel — the default panel/card face
        /// technique ported from docs/ui-collection/normal-card.css (Uiverse.io by adamgiebl, MIT:
        /// lift shadow + contact shadow + inset bottom lip), retinted through <see cref="UiStyle"/>'s
        /// warm dock tokens rather than the demo's own cool grey. Margins inset the shadow/border/face
        /// stack from <paramref name="anchorMin"/>/<paramref name="anchorMax"/>'s own bounds so it
        /// never bleeds into whatever sits just outside that zone (e.g. a neighboring dock column).
        /// Returns the face rect — callers parent their content under that.
        /// </summary>
        public RectTransform CreateBackingPanel(
            RectTransform parent,
            string name,
            Vector2 anchorMin,
            Vector2 anchorMax,
            float marginX,
            float marginY,
            Color face,
            Color border,
            Color shadow,
            Color insetLip)
        {
            RectTransform shadowRect = CreatePanel(parent, name + "Shadow", shadow, anchorMin, anchorMax, UiStyle.RoundSprite, Image.Type.Sliced);
            shadowRect.offsetMin = new Vector2(marginX - 2f, marginY - 6f);
            shadowRect.offsetMax = new Vector2(-marginX + 4f, -marginY + 2f);
            shadowRect.GetComponent<Image>().raycastTarget = false;

            RectTransform borderRect = CreatePanel(parent, name + "Border", border, anchorMin, anchorMax, UiStyle.RoundSprite, Image.Type.Sliced);
            borderRect.offsetMin = new Vector2(marginX - 3f, marginY - 3f);
            borderRect.offsetMax = new Vector2(-marginX + 3f, -marginY + 3f);
            borderRect.GetComponent<Image>().raycastTarget = false;

            RectTransform faceRect = CreatePanel(parent, name + "Face", face, anchorMin, anchorMax, UiStyle.RoundSprite, Image.Type.Sliced);
            faceRect.offsetMin = new Vector2(marginX, marginY);
            faceRect.offsetMax = new Vector2(-marginX, -marginY);

            RectTransform lip = CreatePanel(faceRect, name + "InsetLip", insetLip, Vector2.zero, new Vector2(1f, 0f));
            lip.offsetMin = new Vector2(marginX * 0.5f, 0f);
            lip.offsetMax = new Vector2(-marginX * 0.5f, 5f);
            lip.GetComponent<Image>().raycastTarget = false;

            return faceRect;
        }

        public Text CreateText(
            RectTransform parent,
            string name,
            string content,
            int size,
            TextAnchor anchor,
            Color color,
            UiTextOverflow overflow = UiTextOverflow.Body)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Text));
            var rt = go.GetComponent<RectTransform>();
            rt.SetParent(parent, false);

            var text = go.GetComponent<Text>();
            text.font = _font;
            text.text = content;
            text.fontSize = size;
            text.alignment = anchor;
            text.color = color;
            ApplyOverflow(text, overflow);
            return text;
        }

        public Button CreateButton(
            RectTransform parent,
            string name,
            string label,
            Color bg,
            Color fg,
            int size,
            UnityAction onClick,
            Sprite sprite = null,
            Image.Type imageType = Image.Type.Simple)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(Button));
            var rt = go.GetComponent<RectTransform>();
            rt.SetParent(parent, false);

            var image = go.GetComponent<Image>();
            image.color = bg;
            if (sprite != null)
            {
                image.sprite = sprite;
                image.type = imageType;
            }

            Text text = CreateText(rt, "Label", label, size, TextAnchor.MiddleCenter, fg, UiTextOverflow.Button);
            Stretch(text.rectTransform, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);

            var button = go.GetComponent<Button>();
            button.targetGraphic = image;
            if (onClick != null)
            {
                button.onClick.AddListener(onClick);
            }

            return button;
        }

        public Slider CreateSlider(RectTransform parent, string name, Color track, Color fillColor, Color handleColor)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Slider));
            var rt = go.GetComponent<RectTransform>();
            rt.SetParent(parent, false);

            CreatePanel(rt, "Background", track, Vector2.zero, Vector2.one);

            var fillArea = new GameObject("Fill Area", typeof(RectTransform)).GetComponent<RectTransform>();
            fillArea.SetParent(rt, false);
            fillArea.anchorMin = Vector2.zero;
            fillArea.anchorMax = Vector2.one;
            fillArea.offsetMin = Vector2.zero;
            fillArea.offsetMax = Vector2.zero;

            RectTransform fill = CreatePanel(fillArea, "Fill", fillColor, Vector2.zero, Vector2.one);

            var handleArea = new GameObject("Handle Slide Area", typeof(RectTransform)).GetComponent<RectTransform>();
            handleArea.SetParent(rt, false);
            handleArea.anchorMin = Vector2.zero;
            handleArea.anchorMax = Vector2.one;
            handleArea.offsetMin = new Vector2(20f, 0f);
            handleArea.offsetMax = new Vector2(-20f, 0f);

            RectTransform handle = CreatePanel(handleArea, "Handle", handleColor, Vector2.zero, Vector2.one);
            handle.sizeDelta = new Vector2(40f, 0f);

            var slider = go.GetComponent<Slider>();
            slider.direction = Slider.Direction.LeftToRight;
            slider.minValue = 0f;
            slider.maxValue = 1f;
            slider.value = 0f;
            slider.fillRect = fill;
            slider.handleRect = handle;
            slider.targetGraphic = handle.GetComponent<Image>();
            return slider;
        }

        public static void ApplyOverflow(Text text, UiTextOverflow overflow)
        {
            switch (overflow)
            {
                case UiTextOverflow.Button:
                    text.horizontalOverflow = HorizontalWrapMode.Wrap;
                    text.verticalOverflow = VerticalWrapMode.Truncate;
                    break;
                case UiTextOverflow.SingleLine:
                    text.horizontalOverflow = HorizontalWrapMode.Overflow;
                    text.verticalOverflow = VerticalWrapMode.Overflow;
                    break;
                default:
                    text.horizontalOverflow = HorizontalWrapMode.Wrap;
                    text.verticalOverflow = VerticalWrapMode.Overflow;
                    break;
            }
        }

        public static void Stretch(RectTransform rt, Vector2 anchorMin, Vector2 anchorMax, Vector2 offsetMin, Vector2 offsetMax)
        {
            rt.anchorMin = anchorMin;
            rt.anchorMax = anchorMax;
            rt.offsetMin = offsetMin;
            rt.offsetMax = offsetMax;
        }

        public static void Stretch(RectTransform rt, Vector2 anchorMin, Vector2 anchorMax)
        {
            Stretch(rt, anchorMin, anchorMax, Vector2.zero, Vector2.zero);
        }

        public static void Anchor(RectTransform rt, Vector2 anchorMin, Vector2 anchorMax, Vector2 offsetMin, Vector2 offsetMax)
        {
            rt.anchorMin = anchorMin;
            rt.anchorMax = anchorMax;
            rt.offsetMin = offsetMin;
            rt.offsetMax = offsetMax;
        }

        /// <summary>Places a full-width row at the cursor, then moves the cursor below it.</summary>
        public static void PlaceRow(RectTransform rt, ref float cursor, float height, float gapAfter, float pad = UiStyle.Pad)
        {
            Anchor(rt, new Vector2(0f, 1f), new Vector2(1f, 1f),
                new Vector2(pad, cursor - height), new Vector2(-pad, cursor));
            cursor -= height + gapAfter;
        }

        /// <summary>Places one cell of a row divided into <paramref name="count"/> equal columns.</summary>
        public static void PlaceSplitCell(RectTransform rt, float cursor, float height, int index, int count,
            float pad = UiStyle.Pad, float gap = UiStyle.Gap)
        {
            float half = gap * 0.5f;
            rt.anchorMin = new Vector2(index / (float)count, 1f);
            rt.anchorMax = new Vector2((index + 1) / (float)count, 1f);
            rt.offsetMin = new Vector2(index == 0 ? pad : half, cursor - height);
            rt.offsetMax = new Vector2(index == count - 1 ? -pad : -half, cursor);
        }

        /// <summary>Places a control on the dock's bottom transport row.</summary>
        public static void PlaceActionCell(RectTransform rt, float left, float right, float bottom, float top)
        {
            if (right < 0f)
            {
                Anchor(rt, new Vector2(0f, 0f), new Vector2(1f, 0f),
                    new Vector2(left, bottom), new Vector2(right, top));
                return;
            }

            Anchor(rt, new Vector2(0f, 0f), new Vector2(0f, 0f),
                new Vector2(left, bottom), new Vector2(right, top));
        }

        public static void ConfigureLandscapeScaler(CanvasScaler scaler)
        {
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = UiStyle.ReferenceResolution;
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = UiStyle.CanvasMatchWidthOrHeight;
        }
    }
}
