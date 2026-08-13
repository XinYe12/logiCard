#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace LogiCard.Art.Editor
{
    /// <summary>
    /// Sets 9-slice sprite import settings on the Kenney "UI Pack - Adventure" (CC0) PNGs under
    /// <c>Assets/_Project/Art/UI/Resources/CharSelect/</c> — see
    /// <c>Assets/_Project/Art/UI/THIRD_PARTY.md</c> for provenance. Living under a
    /// <c>Resources/</c> folder is deliberate: <see cref="CharacterSelectView"/> loads them at
    /// runtime via <c>Resources.Load&lt;Sprite&gt;</c>, which (unlike an editor-only
    /// <c>AssetDatabase</c> lookup) works in an actual player build, not just in-Editor. The PNGs
    /// land on disk with
    /// whatever default import Unity gives a bare texture; this tool is what actually turns them
    /// into <see cref="Sprite"/>s with the right 9-slice border, so <c>Image.Type.Sliced</c> can
    /// stretch the middle without smearing the wood-grain corners.
    ///
    /// Border values were measured by hand (pixel-sampling the "Double" 2x source PNGs for where the
    /// baked-in border color stops and the flat interior fill begins) — Kenney's own catalog doesn't
    /// ship per-sprite border metadata for these individually-cropped PNGs (only for the packed
    /// spritesheet variant, which this project isn't using).
    ///
    /// Run: -executeMethod LogiCard.Art.Editor.UiKenneyImportTool.Run
    /// Menu: Tools → LogiCard → Import Character Select Kenney Sprites
    /// </summary>
    public static class UiKenneyImportTool
    {
        private const string RootPath = "Assets/_Project/Art/UI/Resources/CharSelect";

        // (left, bottom, right, top) in source pixels — Unity's Vector4 sprite border order.
        private static readonly Dictionary<string, Vector4> Borders = new Dictionary<string, Vector4>
        {
            ["panel_brown.png"] = new Vector4(16, 16, 16, 16),
            ["panel_brown_dark.png"] = new Vector4(16, 16, 16, 16),
            ["button_brown.png"] = new Vector4(8, 8, 8, 8),
        };

        [MenuItem("Tools/LogiCard/Import Character Select Kenney Sprites")]
        public static void Run()
        {
            int changed = 0;
            foreach (KeyValuePair<string, Vector4> entry in Borders)
            {
                string path = $"{RootPath}/{entry.Key}";
                var importer = AssetImporter.GetAtPath(path) as TextureImporter;
                if (importer == null)
                {
                    Debug.LogWarning($"[UiKenneyImportTool] No texture importer at {path} — skipped.");
                    continue;
                }

                importer.textureType = TextureImporterType.Sprite;
                importer.spriteImportMode = SpriteImportMode.Single;
                importer.spriteBorder = entry.Value;
                importer.spritePixelsPerUnit = 100f;
                importer.mipmapEnabled = false;
                importer.filterMode = FilterMode.Bilinear;
                importer.wrapMode = TextureWrapMode.Clamp;
                importer.alphaIsTransparency = true;
                importer.SaveAndReimport();
                changed++;
            }

            Debug.Log($"[UiKenneyImportTool] Reimported {changed} sprite(s) with 9-slice borders.");
        }
    }
}
#endif
