#if UNITY_EDITOR
using System;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace LogiCard.Art.Editor
{
    /// <summary>
    /// Copies curated prefabs from the ithappy "Cartoon City Free" pack
    /// (<c>Assets/ithappy/Cartoon_City_Free</c>) into <c>Resources/CityDressing</c> for runtime
    /// <see cref="LogiCard.Board.BoardView.PlaceVoidDressing"/> loading (void-edge exterior dressing,
    /// see <c>docs/ART_PACK_RESEARCH.md</c> and the void-city-dressing brief). Source pack is read-only
    /// reference content — this tool only *copies* individual prefab assets by path; it never rebuilds
    /// or edits pack content in place.
    ///
    /// Same shape as <c>WeatherPackImportTool</c>: no material patch-up step is needed here — a sample
    /// material's <c>m_Shader</c> was checked directly and already points at this project's own URP/Lit
    /// shader GUID (<c>933532a4fcc9baf4fa0491de14d08ed7</c>), so a straight
    /// <see cref="AssetDatabase.CopyAsset"/> is sufficient.
    ///
    /// Run: -executeMethod LogiCard.Art.Editor.CityDressingPackImportTool.Run
    /// Menu: Tools → LogiCard → Import City Dressing Pack Prefabs
    /// </summary>
    public static class CityDressingPackImportTool
    {
        private const string PrefabFolder = "Assets/_Project/Art/Environment/Resources/CityDressing";
        private const string SourceRoot = "Assets/ithappy/Cartoon_City_Free/Prefabs";

        // Picks confirmed via a throwaway batchmode bounds probe against the actual meshes (raw local
        // bounds, not guessed from names) — see the void-city-dressing report for the measured
        // dimensions. All are already bottom-anchored at local origin (bounds min.y ~= 0) except the
        // sidewalk tile, which is center-anchored on all three axes (a flat ground plane prefab).
        private static readonly (string source, string destName)[] Catalog =
        {
            // Set_B_Tiles_05: 15x0.2x15, center-pivoted — a broken paving slab at the void edge.
            ($"{SourceRoot}/Sidewalks/Set_B_Tiles_05.prefab", "SidewalkSlab"),
            // Trash_Can_07: 2.096x1.645x1.120, bottom-pivoted — literal trash can.
            ($"{SourceRoot}/Props/Trash_Can_07.prefab", "TrashCan"),
            // Spotlight_02: 2.759x7.103x2.759, bottom-pivoted — tall broken lamp-post stub.
            ($"{SourceRoot}/Props/Spotlight_02.prefab", "StreetlampBroken"),
            // Trash_04: 0.510x0.876x0.510, bottom-pivoted — loose debris/rubble pile.
            ($"{SourceRoot}/Props/Trash_04.prefab", "DebrisPile"),
            // Trash_06: 2.163x1.242x0.592, bottom-pivoted — low wide flattened debris chunk.
            ($"{SourceRoot}/Props/Trash_06.prefab", "DebrisFlat"),
        };

        public static void Run()
        {
            try
            {
                int copied = RunInternal();
                Debug.Log($"[CityDressingPackImportTool] Copied {copied} prefab(s) under {PrefabFolder}");
                EditorApplication.Exit(0);
            }
            catch (Exception ex)
            {
                Debug.LogError("[CityDressingPackImportTool] Failed: " + ex);
                EditorApplication.Exit(1);
            }
        }

        [MenuItem("Tools/LogiCard/Import City Dressing Pack Prefabs")]
        public static void RunFromMenu()
        {
            int copied = RunInternal();
            Debug.Log($"[CityDressingPackImportTool] Copied {copied} prefab(s) under {PrefabFolder}");
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
                    Debug.LogWarning($"CityDressingPackImportTool: missing source {entry.source}");
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
                    Debug.LogWarning($"CityDressingPackImportTool: copy failed {entry.source} -> {destPath}");
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
