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

        /// <summary>
        /// Display/headline face — Iomanoid (CC0, Raymond Larabie), see
        /// <c>Assets/_Project/Art/UI/THIRD_PARTY.md</c>. Falls back to <see cref="Font"/> if the
        /// Resources import is missing, so a broken/absent font asset degrades to plain type instead
        /// of throwing during HUD construction.
        /// Body copy deliberately stays on <see cref="Font"/> — Iomanoid is a wide display face and is
        /// not legible at paragraph sizes.
        /// </summary>
        public Font Display { get; }

        private const string DisplayFontResource = "Fonts/Iomanoid";
        private static Font _displayFontCache;
        private static bool _displayFontProbed;

        public UiFactory(Font font)
        {
            _font = font != null
                ? font
                : Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            Font = _font;
            Display = LoadDisplayFont() ?? _font;
        }

        private static Font LoadDisplayFont()
        {
            if (!_displayFontProbed)
            {
                _displayFontProbed = true;
                _displayFontCache = Resources.Load<Font>(DisplayFontResource);
            }

            return _displayFontCache;
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

        // ------------------------------------------------------------------------------------------
        // Shell chrome (SHELL_CHROME restyle, 2026-08-18) — see docs/ui/UI_SHELL_CHROME.md.
        // These are for the non-HUD shell (Boot / Character Select / Map Select / Lobby / Match End).
        // The in-match HUD keeps CreateBackingPanel + CreateButton; do not swap it onto these.
        // ------------------------------------------------------------------------------------------

        /// <summary>
        /// How many sibling layers <see cref="CreateShellBackdrop"/> occupies at the head of a screen's
        /// child list. Anything that re-orders itself with <c>SetSiblingIndex</c> afterwards must offset
        /// by this or it will be painted over by the backdrop — that is exactly how the Character Select
        /// carousel briefly vanished during the 2026-08-18 chrome pass (<c>SetAsFirstSibling</c> put the
        /// ghost headline and the card stage *behind* the void layer).
        /// </summary>
        public const int ShellBackdropLayerCount = 4;

        /// <summary>
        /// Paints a shell screen's ground: warm void, a stretched radial light pool, a tiled paper
        /// mottle, and an edge vignette — four raycast-transparent layers inserted at the bottom of
        /// <paramref name="parent"/>. Replaces the old "fill the whole screen with one flat Color"
        /// approach, which is exactly what read as programmer art.
        /// Returns the light-pool <see cref="Image"/> so a screen can re-tint its mood at runtime
        /// (Character Select lerps it across the archetype crossfade).
        /// </summary>
        public Image CreateShellBackdrop(RectTransform parent, Color glowTint)
        {
            RectTransform baseLayer = CreatePanel(parent, "BackdropVoid", UiStyle.ShellVoid, Vector2.zero, Vector2.one);
            baseLayer.GetComponent<Image>().raycastTarget = false;

            // Oversized so the falloff's outer ring lands off-screen and the pool reads as light,
            // not as a visible ellipse pasted on the page.
            RectTransform glow = CreatePanel(parent, "BackdropGlow", glowTint, Vector2.zero, Vector2.one, UiStyle.RadialSprite);
            glow.offsetMin = new Vector2(-360f, -300f);
            glow.offsetMax = new Vector2(360f, 300f);
            Image glowImage = glow.GetComponent<Image>();
            glowImage.raycastTarget = false;

            RectTransform grain = CreatePanel(parent, "BackdropGrain", UiStyle.ShellGrain, Vector2.zero, Vector2.one,
                UiStyle.GrainSprite, Image.Type.Tiled);
            grain.GetComponent<Image>().raycastTarget = false;

            RectTransform vignette = CreatePanel(parent, "BackdropVignette", UiStyle.ShellVignette, Vector2.zero, Vector2.one,
                UiStyle.VignetteSprite);
            vignette.GetComponent<Image>().raycastTarget = false;

            baseLayer.SetSiblingIndex(0);
            glow.SetSiblingIndex(1);
            grain.SetSiblingIndex(2);
            vignette.SetSiblingIndex(3);
            return glowImage;
        }

        /// <summary>
        /// Headline in the display face with a hard warm offset shadow. The shadow is a uGUI
        /// <see cref="Shadow"/> component rather than a second <see cref="Text"/> object on purpose —
        /// several shell headlines (Match End, Round Result) have their text reassigned at runtime, and
        /// a duplicated Text would silently desync.
        /// </summary>
        public Text CreateHeadline(
            RectTransform parent,
            string name,
            string content,
            int size,
            Color ink,
            UiTextOverflow overflow = UiTextOverflow.SingleLine,
            float shadowDistance = 4f)
        {
            Text text = CreateText(parent, name, content, size, TextAnchor.MiddleCenter, ink, overflow);
            text.font = Display;
            text.raycastTarget = false;

            var shadow = text.gameObject.AddComponent<Shadow>();
            shadow.effectColor = UiStyle.ShellTitleShadow;
            shadow.effectDistance = new Vector2(shadowDistance, -shadowDistance);
            return text;
        }

        /// <summary>Short accent rule used to sit a headline on something instead of floating it.</summary>
        public RectTransform CreateRule(RectTransform parent, string name, Vector2 anchorMin, Vector2 anchorMax)
        {
            RectTransform rule = CreatePanel(parent, name, UiStyle.ShellRule, anchorMin, anchorMax,
                UiStyle.PillSprite, Image.Type.Sliced);
            rule.GetComponent<Image>().raycastTarget = false;
            return rule;
        }

        /// <summary>
        /// Parchment card for shell copy — the same layered lift/contact/inset-lip stack the HUD dock
        /// panels use, retinted to the Modal* cardstock family that <see cref="ModalDialog"/> already
        /// ships. Returns the face; parent content under it.
        /// </summary>
        public RectTransform CreateShellPlate(RectTransform parent, string name, Vector2 anchorMin, Vector2 anchorMax)
        {
            return CreateBackingPanel(parent, name, anchorMin, anchorMax, 0f, 0f,
                UiStyle.ModalCard, UiStyle.ModalCardBorder, UiStyle.ModalShadow, UiStyle.ModalCardInsetLip);
        }

        /// <summary>Warm dark counterpart to <see cref="CreateShellPlate"/> for panels that must not be paper.</summary>
        public RectTransform CreateShellSlate(RectTransform parent, string name, Vector2 anchorMin, Vector2 anchorMax)
        {
            return CreateBackingPanel(parent, name, anchorMin, anchorMax, 0f, 0f,
                UiStyle.ShellSlateFace, UiStyle.ShellSlateBorder, UiStyle.ModalShadow, UiStyle.DockPanelInsetLip);
        }

        /// <summary>
        /// Chunky "toy" button: contact shadow, a riser giving the button visible thickness, and a face
        /// that drops into that shadow when pressed (<see cref="ShellButton"/>). Ported from
        /// docs/ui-collection/button-gradient-pill.css (Uiverse.io by Codecite, MIT).
        ///
        /// The <see cref="Button"/> lives on the returned object named <paramref name="name"/> and owns
        /// the placement rect, so existing <c>FindByName&lt;Button&gt;(...)</c> lookups and
        /// <c>Stretch(button.GetComponent&lt;RectTransform&gt;(), ...)</c> call sites keep working
        /// unchanged. Selectable transition is None — <see cref="ShellButton"/> owns hover/press so the
        /// default colour-tint multiply does not fight the face colour.
        /// </summary>
        public Button CreateShellButton(
            RectTransform parent,
            string name,
            string label,
            ShellButtonTone tone,
            int fontSize,
            UnityAction onClick,
            float riser = UiStyle.ShellButtonRiser)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(Button), typeof(ShellButton));
            var rt = go.GetComponent<RectTransform>();
            rt.SetParent(parent, false);

            // Transparent hit plate: one raycast target for the whole control, so hover/press never
            // flickers as the pointer crosses between riser and face.
            var hit = go.GetComponent<Image>();
            hit.color = new Color(0f, 0f, 0f, 0f);
            hit.raycastTarget = true;

            RectTransform shadow = CreatePanel(rt, "Shadow", UiStyle.ShellButtonShadow, Vector2.zero, Vector2.one,
                UiStyle.PillSprite, Image.Type.Sliced);
            shadow.offsetMin = new Vector2(2f, -(riser + 3f));
            shadow.offsetMax = new Vector2(-2f, -(riser + 3f));
            Image shadowImage = shadow.GetComponent<Image>();
            shadowImage.raycastTarget = false;

            var bodyGo = new GameObject("Body", typeof(RectTransform));
            var body = bodyGo.GetComponent<RectTransform>();
            body.SetParent(rt, false);
            Stretch(body, Vector2.zero, Vector2.one);

            RectTransform riserRect = CreatePanel(body, "Riser", UiStyle.ShellPrimaryRiser, Vector2.zero, Vector2.one,
                UiStyle.PillSprite, Image.Type.Sliced);
            Image riserImage = riserRect.GetComponent<Image>();
            riserImage.raycastTarget = false;

            RectTransform face = CreatePanel(body, "Face", UiStyle.ShellPrimaryFace, Vector2.zero, Vector2.one,
                UiStyle.PillSprite, Image.Type.Sliced);
            face.offsetMin = new Vector2(0f, riser);
            Image faceImage = face.GetComponent<Image>();
            faceImage.raycastTarget = false;

            // Lit top rim. The whole shell is lit from above (see CreateShellBackdrop); without this a
            // face is a single flat colour and reads as a coloured rectangle no matter how good the
            // shadow under it is.
            RectTransform highlight = CreatePanel(face, "FaceHighlight", new Color(1f, 1f, 1f, 0.16f),
                new Vector2(0f, 1f), Vector2.one, UiStyle.PillSprite, Image.Type.Sliced);
            highlight.offsetMin = new Vector2(6f, -7f);
            highlight.offsetMax = new Vector2(-6f, -2f);
            highlight.GetComponent<Image>().raycastTarget = false;

            // Body face, bold — deliberately NOT Display. Iomanoid is an outlined art-deco display face:
            // beautiful at 50pt+ headline sizes, but at a 22–36pt button label its strokes go thin and
            // low-contrast against a saturated face colour. Headlines get the character, controls get
            // read at a glance.
            Text text = CreateText(face, "Label", label, fontSize, TextAnchor.MiddleCenter, UiStyle.ShellPrimaryText,
                UiTextOverflow.Button);
            text.fontStyle = FontStyle.Bold;
            text.raycastTarget = false;
            Stretch(text.rectTransform, Vector2.zero, Vector2.one, new Vector2(10f, 0f), new Vector2(-10f, 0f));

            var button = go.GetComponent<Button>();
            button.targetGraphic = hit;
            button.transition = Selectable.Transition.None;
            if (onClick != null)
            {
                button.onClick.AddListener(onClick);
            }

            var shell = go.GetComponent<ShellButton>();
            shell.Bind(body, faceImage, riserImage, shadowImage, text, riser);
            shell.ApplyTone(tone);
            return button;
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
