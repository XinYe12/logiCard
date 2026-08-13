#if UNITY_EDITOR
using System;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace LogiCard.Art.Editor
{
    /// <summary>
    /// Copies curated prefabs from the free "Cinematic Weather VFX Bundle"
    /// (<c>Assets/RainSnowCloudEffect</c>) and "Zap VFX – URP" (<c>Assets/Vefects</c>) packs into
    /// <c>Resources/Weather</c> for runtime <see cref="LogiCard.Board.BoardWeatherPocket"/> loading
    /// (C53 weather re-source, 2026-08-11). Source packs are read-only reference — see
    /// <c>docs/ART_PACK_RESEARCH.md</c> — this tool only *copies* individual prefab assets by GUID
    /// reference; it never rebuilds or edits pack content in place.
    ///
    /// Unlike <c>InteriorPackImportTool</c>/<c>PawnImportTool</c>, no material patch-up step is needed
    /// here: the source materials (<c>M_CloudLayer_URP.mat</c>, <c>M_Rain_URP.mat</c>) were checked
    /// directly and already ship correct URP transparency (<c>_Surface: 1</c> Transparent,
    /// <c>_SURFACE_TYPE_TRANSPARENT</c> valid) — see <c>BoardWeatherPocket</c>'s class doc comment for
    /// the full check. A straight <see cref="AssetDatabase.CopyAsset"/> is sufficient.
    ///
    /// Run: -executeMethod LogiCard.Art.Editor.WeatherPackImportTool.Run
    /// Menu: Tools → LogiCard → Import Weather Pack Prefabs
    /// </summary>
    public static class WeatherPackImportTool
    {
        private const string PrefabFolder = "Assets/_Project/Art/Environment/Resources/Weather";

        private static readonly (string source, string destName)[] Catalog =
        {
            // Kept for provenance / optional reuse — BoardWeatherPocket no longer loads CloudLayer
            // (open-world scale; stylized CloudAtlas bank restored 2026-08-12).
            ("Assets/RainSnowCloudEffect/Prefabs/PF_CloudLayer.prefab", "PF_CloudLayer"),
            ("Assets/RainSnowCloudEffect/Prefabs/PF_RainSystem.prefab", "PF_RainSystem"),
            // Soft atmosphere — ART_PACK_RESEARCH use-now #2; rim mist uses Main/Distant (2026-08-12).
            ("Assets/RainSnowCloudEffect/Prefabs/PF_Fog_Ground.prefab", "PF_Fog_Ground"),
            ("Assets/RainSnowCloudEffect/Prefabs/PF_RainMist.prefab", "PF_RainMist"),
            ("Assets/RainSnowCloudEffect/Prefabs/PF_Fog_Main.prefab", "PF_Fog_Main"),
            ("Assets/RainSnowCloudEffect/Prefabs/PF_Fog_Distant.prefab", "PF_Fog_Distant"),
            ("Assets/Vefects/Zap VFX URP/VFX/Zap/Particles/VFX_Zap_06_White.prefab", "VFX_Zap_White"),
        };

        public static void Run()
        {
            try
            {
                int copied = RunInternal();
                Debug.Log($"[WeatherPackImportTool] Copied {copied} prefab(s) under {PrefabFolder}");
                EditorApplication.Exit(0);
            }
            catch (Exception ex)
            {
                Debug.LogError("[WeatherPackImportTool] Failed: " + ex);
                EditorApplication.Exit(1);
            }
        }

        [MenuItem("Tools/LogiCard/Import Weather Pack Prefabs")]
        public static void RunFromMenu()
        {
            int copied = RunInternal();
            Debug.Log($"[WeatherPackImportTool] Copied {copied} prefab(s) under {PrefabFolder}");
        }

        private static int RunInternal()
        {
            EnsureFolder(PrefabFolder);
            AssetDatabase.Refresh();

            int copied = 0;
            foreach ((string source, string destName) entry in Catalog)
            {
                if (AssetDatabase.LoadAssetAtPath<GameObject>(entry.source) == null)
                {
                    Debug.LogWarning($"WeatherPackImportTool: missing source {entry.source}");
                    continue;
                }

                string destPath = $"{PrefabFolder}/{entry.destName}.prefab";
                if (AssetDatabase.LoadAssetAtPath<GameObject>(destPath) != null)
                {
                    AssetDatabase.DeleteAsset(destPath);
                }

                if (AssetDatabase.CopyAsset(entry.source, destPath))
                {
                    copied++;
                }
                else
                {
                    Debug.LogWarning($"WeatherPackImportTool: copy failed {entry.source} -> {destPath}");
                }
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            return copied;
        }

        private static void EnsureFolder(string path)
        {
            if (AssetDatabase.IsValidFolder(path))
            {
                return;
            }

            string parent = Path.GetDirectoryName(path)?.Replace('\\', '/');
            string leaf = Path.GetFileName(path);
            if (string.IsNullOrEmpty(parent))
            {
                return;
            }

            if (!AssetDatabase.IsValidFolder(parent))
            {
                EnsureFolder(parent);
            }

            AssetDatabase.CreateFolder(parent, leaf);
        }
    }
}
#endif
