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
        public static readonly Color ModalDimmer = new Color(0.02f, 0.02f, 0.03f, 0.72f);

        /// <summary>Landscape reference for <see cref="CanvasScaler"/> (C48 / 16:9).</summary>
        public static readonly Vector2 ReferenceResolution = new Vector2(1920f, 1080f);

        /// <summary>
        /// Slight height bias so dock type stays readable on wide ultrawides without ballooning on
        /// shorter 16:9 windows. Was 0.5 before the readability pass.
        /// </summary>
        public const float CanvasMatchWidthOrHeight = 0.4f;

        public const float Pad = 20f;
        public const float Gap = 10f;
        public const float RowGap = 12f;
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
