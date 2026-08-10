#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEngine;

namespace LogiCard.Art.Editor
{
    /// <summary>
    /// Configures Poly Haven board-surface texture import settings (normal maps linear, etc.)
    /// under Resources/BoardSurfaces. Run via:
    /// -executeMethod LogiCard.Art.Editor.EnvironmentSurfacesBootstrap.Run
    /// </summary>
    public static class EnvironmentSurfacesBootstrap
    {
        private const string SurfacesFolder = "Assets/_Project/Art/Environment/Resources/BoardSurfaces";

        public static void Run()
        {
            try
            {
                if (!AssetDatabase.IsValidFolder(SurfacesFolder))
                {
                    Debug.LogError("[EnvironmentSurfacesBootstrap] Missing folder: " + SurfacesFolder);
                    EditorApplication.Exit(1);
                    return;
                }

                AssetDatabase.Refresh();
                int configured = 0;
                foreach (string guid in AssetDatabase.FindAssets("t:Texture2D", new[] { SurfacesFolder }))
                {
                    string path = AssetDatabase.GUIDToAssetPath(guid);
                    if (ConfigureTexture(path))
                    {
                        configured++;
                    }
                }

                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
                Debug.Log($"[EnvironmentSurfacesBootstrap] Configured {configured} textures under {SurfacesFolder}");
                EditorApplication.Exit(0);
            }
            catch (System.Exception ex)
            {
                Debug.LogError("[EnvironmentSurfacesBootstrap] Failed: " + ex);
                EditorApplication.Exit(1);
            }
        }

        [MenuItem("Tools/LogiCard/Configure Environment Surfaces")]
        public static void RunFromMenu()
        {
            AssetDatabase.Refresh();
            int configured = 0;
            foreach (string guid in AssetDatabase.FindAssets("t:Texture2D", new[] { SurfacesFolder }))
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                if (ConfigureTexture(path))
                {
                    configured++;
                }
            }

            AssetDatabase.SaveAssets();
            Debug.Log($"[EnvironmentSurfacesBootstrap] Configured {configured} textures (menu).");
        }

        private static bool ConfigureTexture(string path)
        {
            var importer = AssetImporter.GetAtPath(path) as TextureImporter;
            if (importer == null)
            {
                return false;
            }

            string file = Path.GetFileNameWithoutExtension(path);
            bool isNormal = file.EndsWith("_nor") || file.Contains("nor");
            bool isRough = file.EndsWith("_rough") || file.Contains("rough");

            importer.textureType = isNormal ? TextureImporterType.NormalMap : TextureImporterType.Default;
            importer.sRGBTexture = !isNormal && !isRough;
            importer.mipmapEnabled = true;
            importer.wrapMode = TextureWrapMode.Repeat;
            importer.filterMode = FilterMode.Bilinear;
            importer.anisoLevel = 4;
            importer.SaveAndReimport();
            return true;
        }
    }
}
#endif
