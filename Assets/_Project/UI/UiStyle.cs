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

        // Chrome pass (HUD_CHROME_SHIP_PASS brief, 2026-08-15) — layered shadow tokens ported from
        // docs/ui-collection/normal-card.css (Uiverse.io by adamgiebl, MIT): a soft far-thrown lift
        // shadow behind ModalShadow's tighter contact shadow, plus an inset bottom-edge lip. Retinted
        // through this project's warm cardstock family rather than the demo's cool grey.
        /// <summary>Softer, farther-thrown lift shadow layered behind <see cref="ModalShadow"/>'s tighter contact shadow.</summary>
        public static readonly Color ModalShadowFar = new Color(0.04f, 0.03f, 0.02f, 0.30f);
        /// <summary>Inset strip along a card's bottom edge — the "contact" lip from normal-card.css.</summary>
        public static readonly Color ModalCardInsetLip = new Color(0.70f, 0.60f, 0.44f, 0.9f);

        // Dock region backing panels (same brief) — every HUD dock region (Controls/Hand/Action
        // columns, queue log strip) gets its own bounded panel instead of a flat color abutting its
        // neighbor. Warm-tinted so it reads as the same family as Modal* rather than a second
        // cool-neutral system bolted on next to it.
        /// <summary>Warm dark face for a dock region's own backing panel.</summary>
        public static readonly Color DockPanelFace = new Color(0.14f, 0.12f, 0.10f, 0.97f);
        /// <summary>Warm rim around a dock region panel — same amber family as <see cref="ModalCardBorder"/>, dialed down for a dark backing.</summary>
        public static readonly Color DockPanelBorder = new Color(0.40f, 0.32f, 0.21f, 0.85f);
        /// <summary>Contact shadow directly under a dock region panel.</summary>
        public static readonly Color DockPanelShadow = new Color(0.03f, 0.02f, 0.02f, 0.55f);
        /// <summary>Subtle inset strip along a dock region panel's bottom edge.</summary>
        public static readonly Color DockPanelInsetLip = new Color(0.07f, 0.06f, 0.05f, 0.55f);

        // Character Select carousel accents (desk-lamp warm family — do not collide with Modal* tokens).
        // Shell chrome pass (2026-08-18) repurposed CharSelectBg* from a flat full-screen fill into the
        // *tint of the backdrop light pool* — they are multiplied over ShellVoid by a radial falloff, so
        // they must stay a lit warm hue, not a muddy mid-brown that reads as a solid page color.
        public static readonly Color CharSelectBgScout = new Color(0.66f, 0.42f, 0.19f, 1f);
        public static readonly Color CharSelectBgJuggernaut = new Color(0.52f, 0.25f, 0.17f, 1f);
        public static readonly Color CharSelectGhost = new Color(1f, 0.94f, 0.82f, 0.10f);
        public static readonly Color CharSelectGlowScout = new Color(0.98f, 0.72f, 0.25f, 1f);
        public static readonly Color CharSelectGlowJuggernaut = new Color(0.86f, 0.48f, 0.24f, 1f);

        // ------------------------------------------------------------------------------------------
        // Shell chrome (SHELL_CHROME restyle, 2026-08-18) — Boot / Character Select / Map Select /
        // Lobby / Match End. See docs/ui/UI_SHELL_CHROME.md.
        //
        // Source language, per docs/UI_CHROME_COLLECTION.md:
        //   * warm cream/parchment + ONE saturated red accent — the locked clay-icon style
        //     (docs/ui-collection/icons/, "icon_bandage" style lock).
        //   * layered lift + contact shadow + inset bottom lip — docs/ui-collection/normal-card.css
        //     (Uiverse.io by adamgiebl, MIT), already ported to Modal*/Dock* above.
        //   * chunky button that drops into its own shadow on press —
        //     docs/ui-collection/button-gradient-pill.css (Uiverse.io by Codecite, MIT).
        // The old shell painted every screen a flat PanelDark / CharSelectBg* rectangle; these tokens
        // exist so a screen is instead a lit backdrop with objects sitting on it.
        // ------------------------------------------------------------------------------------------

        /// <summary>Deep warm near-black the whole shell sits on — never seen flat, always under a light pool.</summary>
        public static readonly Color ShellVoid = new Color(0.085f, 0.068f, 0.056f, 1f);

        /// <summary>Default backdrop light-pool tint (radial, centered) for shell screens with no per-screen mood.</summary>
        public static readonly Color ShellGlowDefault = new Color(0.60f, 0.37f, 0.17f, 1f);

        /// <summary>Cool-lean pool for the terminal-ish Lobby so it is not a carbon copy of Boot.</summary>
        public static readonly Color ShellGlowLobby = new Color(0.44f, 0.33f, 0.20f, 1f);

        /// <summary>Match End pool — pushed toward the red accent so the screen reads as a verdict.</summary>
        public static readonly Color ShellGlowVerdict = new Color(0.56f, 0.26f, 0.17f, 1f);

        /// <summary>Edge darkening painted over everything; keeps content off the frame edge.</summary>
        public static readonly Color ShellVignette = new Color(0.05f, 0.032f, 0.022f, 0.95f);

        /// <summary>Paper-fibre mottle tiled across the backdrop — kills the "flat fill" read at almost no cost.</summary>
        public static readonly Color ShellGrain = new Color(1f, 0.93f, 0.80f, 0.055f);

        /// <summary>Cream headline ink on the dark backdrop.</summary>
        public static readonly Color ShellTitleInk = new Color(0.97f, 0.92f, 0.80f, 1f);

        /// <summary>Hard warm shadow thrown by headline type (uGUI <see cref="UnityEngine.UI.Shadow"/>).</summary>
        public static readonly Color ShellTitleShadow = new Color(0.12f, 0.06f, 0.035f, 0.92f);

        /// <summary>Body copy on the dark backdrop (not on parchment — that uses <see cref="ModalInk"/>).</summary>
        public static readonly Color ShellBodyInk = new Color(0.85f, 0.78f, 0.66f, 1f);

        /// <summary>Quieter footnote copy on the dark backdrop.</summary>
        public static readonly Color ShellMutedInk = new Color(0.62f, 0.55f, 0.46f, 1f);

        /// <summary>The single saturated red accent from the clay-icon style lock.</summary>
        public static readonly Color ShellAccent = new Color(0.87f, 0.31f, 0.24f, 1f);

        /// <summary>Short rule under a headline — accent, not a full-width divider.</summary>
        public static readonly Color ShellRule = new Color(0.87f, 0.31f, 0.24f, 0.9f);

        /// <summary>Warm dark slate face for a panel that must sit on the backdrop without being parchment.</summary>
        public static readonly Color ShellSlateFace = new Color(0.16f, 0.13f, 0.105f, 0.96f);

        /// <summary>Rim around a slate panel.</summary>
        public static readonly Color ShellSlateBorder = new Color(0.34f, 0.26f, 0.18f, 0.9f);

        // Chunky toy button — riser (the visible thickness under the face) + face + contact shadow.
        public static readonly Color ShellPrimaryFace = new Color(0.87f, 0.33f, 0.25f, 1f);
        public static readonly Color ShellPrimaryRiser = new Color(0.50f, 0.155f, 0.125f, 1f);
        public static readonly Color ShellPrimaryText = new Color(1f, 0.96f, 0.90f, 1f);
        public static readonly Color ShellSecondaryFace = new Color(0.91f, 0.855f, 0.735f, 1f);
        public static readonly Color ShellSecondaryRiser = new Color(0.58f, 0.49f, 0.365f, 1f);
        public static readonly Color ShellSecondaryText = new Color(0.24f, 0.17f, 0.12f, 1f);
        public static readonly Color ShellQuietFace = new Color(0.285f, 0.235f, 0.185f, 1f);
        public static readonly Color ShellQuietRiser = new Color(0.155f, 0.125f, 0.10f, 1f);
        public static readonly Color ShellQuietText = new Color(0.88f, 0.82f, 0.70f, 1f);
        public static readonly Color ShellButtonShadow = new Color(0.03f, 0.02f, 0.015f, 0.5f);

        /// <summary>Riser depth in UI units — how thick a shell button looks before it is pressed.</summary>
        public const float ShellButtonRiser = 8f;

        private static Sprite _roundSprite;
        private static Sprite _pillSprite;
        private static Sprite _radialSprite;
        private static Sprite _vignetteSprite;
        private static Sprite _grainSprite;
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

        /// <summary>
        /// Fully-rounded capsule, 9-sliced with a half-height border so any button taller than 64 UI
        /// units still reads as a true pill rather than a rounded rectangle. Same "generate it, don't
        /// fetch a builtin extra resource" reasoning as <see cref="RoundSprite"/>.
        /// </summary>
        public static Sprite PillSprite => _pillSprite != null ? _pillSprite : (_pillSprite = BuildPillSprite());

        private static Sprite BuildPillSprite()
        {
            const int size = 64;
            const float radius = 32f;
            var texture = new Texture2D(size, size, TextureFormat.RGBA32, false)
            {
                name = "UiStyle_PillSprite",
                wrapMode = TextureWrapMode.Clamp,
                filterMode = FilterMode.Bilinear,
            };

            var pixels = new Color32[size * size];
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float dx = x + 0.5f - radius;
                    float dy = y + 0.5f - radius;
                    float dist = Mathf.Sqrt((dx * dx) + (dy * dy));
                    float alpha = Mathf.Clamp01(radius - dist + 0.5f);
                    pixels[(y * size) + x] = new Color32(255, 255, 255, (byte)Mathf.RoundToInt(alpha * 255f));
                }
            }

            texture.SetPixels32(pixels);
            texture.Apply(false, true);

            var sprite = Sprite.Create(
                texture,
                new Rect(0f, 0f, size, size),
                new Vector2(0.5f, 0.5f),
                100f,
                0,
                SpriteMeshType.FullRect,
                new Vector4(radius, radius, radius, radius));
            sprite.name = "UiStyle_PillSprite";
            return sprite;
        }

        /// <summary>
        /// Soft radial falloff (opaque centre → transparent edge). Stretched full-bleed it becomes the
        /// backdrop light pool; at card size it is the Character Select halo.
        /// </summary>
        public static Sprite RadialSprite => _radialSprite != null ? _radialSprite : (_radialSprite = BuildRadialSprite("UiStyle_RadialSprite", false));

        /// <summary>Inverse of <see cref="RadialSprite"/> — transparent centre, opaque edge. Vignette layer.</summary>
        public static Sprite VignetteSprite => _vignetteSprite != null ? _vignetteSprite : (_vignetteSprite = BuildRadialSprite("UiStyle_VignetteSprite", true));

        private static Sprite BuildRadialSprite(string name, bool inverted)
        {
            const int size = 128;
            const float half = size * 0.5f;
            var texture = new Texture2D(size, size, TextureFormat.RGBA32, false)
            {
                name = name,
                wrapMode = TextureWrapMode.Clamp,
                filterMode = FilterMode.Bilinear,
            };

            var pixels = new Color32[size * size];
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float dx = (x + 0.5f - half) / half;
                    float dy = (y + 0.5f - half) / half;
                    float dist = Mathf.Clamp01(Mathf.Sqrt((dx * dx) + (dy * dy)));
                    // Smoothstep falloff — a linear ramp bands visibly once stretched to 1920 wide.
                    float fall = 1f - (dist * dist * (3f - (2f * dist)));
                    float alpha = inverted ? 1f - fall : fall;
                    pixels[(y * size) + x] = new Color32(255, 255, 255, (byte)Mathf.RoundToInt(Mathf.Clamp01(alpha) * 255f));
                }
            }

            texture.SetPixels32(pixels);
            texture.Apply(false, true);

            var sprite = Sprite.Create(texture, new Rect(0f, 0f, size, size), new Vector2(0.5f, 0.5f), 100f, 0, SpriteMeshType.FullRect);
            sprite.name = name;
            return sprite;
        }

        /// <summary>
        /// Deterministic paper-fibre mottle for <see cref="UnityEngine.UI.Image.Type.Tiled"/> backdrops.
        /// Built at 1 pixel-per-unit so one tile is 128 UI units (a 100 ppu sprite would tile ~15k times
        /// across a 1920-wide canvas). No <see cref="Random"/> — the pattern must be identical every run
        /// so screenshots and tests are reproducible.
        /// </summary>
        public static Sprite GrainSprite => _grainSprite != null ? _grainSprite : (_grainSprite = BuildGrainSprite());

        private static Sprite BuildGrainSprite()
        {
            const int size = 128;
            var texture = new Texture2D(size, size, TextureFormat.RGBA32, false)
            {
                name = "UiStyle_GrainSprite",
                wrapMode = TextureWrapMode.Repeat,
                filterMode = FilterMode.Bilinear,
            };

            var pixels = new Color32[size * size];
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float fine = Hash01(x, y);
                    float coarse = Hash01(x >> 3, y >> 3);
                    float v = Mathf.Clamp01((fine * 0.55f) + (coarse * 0.45f));
                    // Bias upward so the tile averages near-opaque; the Image tint carries the real
                    // (very low) alpha, and the variance is what breaks the flat fill.
                    v = Mathf.Lerp(0.25f, 1f, v);
                    pixels[(y * size) + x] = new Color32(255, 255, 255, (byte)Mathf.RoundToInt(v * 255f));
                }
            }

            texture.SetPixels32(pixels);
            texture.Apply(false, true);

            var sprite = Sprite.Create(texture, new Rect(0f, 0f, size, size), new Vector2(0.5f, 0.5f), 1f, 0, SpriteMeshType.FullRect);
            sprite.name = "UiStyle_GrainSprite";
            return sprite;
        }

        private static float Hash01(int x, int y)
        {
            unchecked
            {
                uint h = (uint)((x * 73856093) ^ (y * 19349663));
                h ^= h >> 13;
                h *= 0x85EBCA6Bu;
                h ^= h >> 16;
                return (h & 0xFFFFFF) / (float)0xFFFFFF;
            }
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
