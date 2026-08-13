using UnityEngine;
using UnityEngine.UI;

namespace LogiCard.UI
{
    /// <summary>
    /// Shared chrome palette, layout metrics, and CanvasScaler defaults for Program HUD + AppFlow.
    /// HUD-specific cardstock / AR scrubber colors stay on <see cref="ProgramHud"/> — they are not
    /// shell chrome.
    /// </summary>
    public static class UiStyle
    {
        public static readonly Color Ink = new Color(0.93f, 0.92f, 0.88f, 1f);
        public static readonly Color InkDark = new Color(0.1f, 0.09f, 0.07f, 1f);
        public static readonly Color PanelDark = new Color(0.10f, 0.10f, 0.12f, 0.96f);
        public static readonly Color PanelMid = new Color(0.17f, 0.17f, 0.20f, 1f);
        public static readonly Color PanelSunken = new Color(0.06f, 0.06f, 0.08f, 1f);
        public static readonly Color Accent = new Color(0.98f, 0.72f, 0.25f, 1f);
        public static readonly Color AccentDim = new Color(0.35f, 0.30f, 0.20f, 1f);
        public static readonly Color Card = new Color(0.11f, 0.11f, 0.14f, 0.98f);
        public static readonly Color PrimaryButton = new Color(0.97f, 0.96f, 0.94f, 1f);
        public static readonly Color PrimaryButtonText = new Color(0.1f, 0.09f, 0.07f, 1f);
        public static readonly Color SecondaryButton = new Color(0.16f, 0.16f, 0.20f, 1f);

        // ModalDialog cardstock-on-dimmer (ART_DIRECTION §4 / UI_TOOLS stub 5). Prefixed so
        // Character Select can add CharSelect* tokens without colliding with shell Card.
        /// <summary>Deep warm black void behind the modal card.</summary>
        public static readonly Color ModalDimmer = new Color(0.04f, 0.03f, 0.02f, 0.86f);
        /// <summary>Warm paper face — matches Time Card cardstock read.</summary>
        public static readonly Color ModalCard = new Color(0.93f, 0.88f, 0.78f, 1f);
        /// <summary>Slightly deeper paper rim under the face.</summary>
        public static readonly Color ModalCardBorder = new Color(0.78f, 0.70f, 0.55f, 1f);
        /// <summary>Soft drop shadow under the card (procedural panel, not an asset).</summary>
        public static readonly Color ModalShadow = new Color(0.04f, 0.03f, 0.02f, 0.55f);
        /// <summary>Ink on paper for modal title/body.</summary>
        public static readonly Color ModalInk = new Color(0.22f, 0.16f, 0.12f, 1f);
        /// <summary>Hairline divider on cardstock.</summary>
        public static readonly Color ModalDivider = new Color(0.62f, 0.52f, 0.38f, 0.85f);
        /// <summary>High-contrast confirm on paper (dark ink fill).</summary>
        public static readonly Color ModalPrimaryButton = new Color(0.18f, 0.14f, 0.10f, 1f);
        public static readonly Color ModalPrimaryButtonText = new Color(0.96f, 0.93f, 0.86f, 1f);
        /// <summary>Secondary / cancel — deeper paper chip, dark ink label.</summary>
        public static readonly Color ModalSecondaryButton = new Color(0.86f, 0.78f, 0.64f, 1f);

        // Character Select carousel accents (desk-lamp warm family — do not collide with Modal* tokens).
        public static readonly Color CharSelectBgScout = new Color(0.42f, 0.28f, 0.14f, 0.97f);
        public static readonly Color CharSelectBgJuggernaut = new Color(0.28f, 0.18f, 0.12f, 0.97f);
        public static readonly Color CharSelectGhost = new Color(1f, 1f, 1f, 0.14f);
        public static readonly Color CharSelectGlowScout = new Color(0.98f, 0.72f, 0.25f, 1f);
        public static readonly Color CharSelectGlowJuggernaut = new Color(0.86f, 0.48f, 0.24f, 1f);

        private static Sprite _roundSprite;
        private const int RoundSpriteSize = 32;
        private const int RoundSpriteRadius = 10;

        /// <summary>
        /// Procedurally generated 9-sliced rounded-rect sprite, cached after first build. Deliberately
        /// not <c>Resources.GetBuiltinResource&lt;Sprite&gt;("UI/Skin/UISprite.psd")</c> — that path is
        /// an Editor-only extra resource (the API that actually finds it is
        /// <c>UnityEditor.AssetDatabase.GetBuiltinExtraResource</c>), so it silently fails (logs an
        /// assert, returns null) in both batchmode PlayMode tests and a real Player build. Generating
        /// our own sprite works identically everywhere.
        /// </summary>
        public static Sprite RoundSprite => _roundSprite != null ? _roundSprite : (_roundSprite = BuildRoundSprite());

        private static Sprite BuildRoundSprite()
        {
            const int size = RoundSpriteSize;
            const float radius = RoundSpriteRadius;

            var texture = new Texture2D(size, size, TextureFormat.RGBA32, false)
            {
                name = "UiStyle_RoundSprite",
                wrapMode = TextureWrapMode.Clamp,
                filterMode = FilterMode.Bilinear,
            };

            var pixels = new Color32[size * size];
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float px = x + 0.5f;
                    float py = y + 0.5f;
                    float cx = Mathf.Clamp(px, radius, size - radius);
                    float cy = Mathf.Clamp(py, radius, size - radius);
                    bool inCornerBox = (px < radius || px > size - radius) && (py < radius || py > size - radius);
                    float alpha = 1f;
                    if (inCornerBox)
                    {
                        float dist = Mathf.Sqrt(((px - cx) * (px - cx)) + ((py - cy) * (py - cy)));
                        alpha = Mathf.Clamp01(radius - dist + 0.5f);
                    }

                    pixels[(y * size) + x] = new Color32(255, 255, 255, (byte)Mathf.RoundToInt(alpha * 255f));
                }
            }

            texture.SetPixels32(pixels);
            texture.Apply(false, true);

            float r = radius;
            var sprite = Sprite.Create(
                texture,
                new Rect(0f, 0f, size, size),
                new Vector2(0.5f, 0.5f),
                100f,
                0,
                SpriteMeshType.FullRect,
                new Vector4(r, r, r, r));
            sprite.name = "UiStyle_RoundSprite";
            return sprite;
        }

        /// <summary>Landscape reference for <see cref="CanvasScaler"/> (C48 / 16:9).</summary>
        public static readonly Vector2 ReferenceResolution = new Vector2(1920f, 1080f);

        /// <summary>
        /// Mild width bias (below 0.5) so dock type stays a bit larger on ultrawide than a pure
        /// 16:9 match would give. Vertical fit on those windows is owned by
        /// <see cref="ProgramHud"/>'s compact row budget — do not "fix" overflow by driving this
        /// back toward 1.0 (that shrinks type on wide screens).
        /// </summary>
        public const float CanvasMatchWidthOrHeight = 0.4f;

        public const float Pad = 16f;
        public const float Gap = 8f;
        public const float RowGap = 8f;
    }

    /// <summary>
    /// Text overflow policy. One deliberate default replaces the former per-file divergence
    /// (ProgramHud defaulted to Overflow/Overflow; AppFlow to Wrap/Overflow).
    /// </summary>
    public enum UiTextOverflow
    {
        /// <summary>Body / labels: wrap horizontally, grow vertically.</summary>
        Body,

        /// <summary>Button labels in tight dock cells: wrap, then truncate.</summary>
        Button,

        /// <summary>Single-line status chips that must not wrap into neighbors.</summary>
        SingleLine,
    }
}
