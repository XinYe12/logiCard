#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace LogiCard.Art.Editor
{
    /// <summary>
    /// Imports curated Quaternius Ultimate House Interior FBX into URP/Lit prefabs under
    /// Resources/Interior for runtime BoardView loading (C53 checkpoint 3).
    ///
    /// Pattern mirrors <c>PawnImportTool</c>: keep per-part materials, re-hook to URP/Lit,
    /// then bake a normalized prefab. Door prefabs are unit-scaled (width≈1, height≈1) with
    /// pivot at bottom-center so BoardView can fit them to wall-segment geometry.
    ///
    /// Run: -executeMethod LogiCard.Art.Editor.InteriorPackImportTool.Run
    /// Menu: Tools → LogiCard → Import Interior Pack Prefabs
    /// </summary>
    public static class InteriorPackImportTool
    {
        private const string SourceFolder = "Assets/_Project/Art/Environment/Interior/Source";
        private const string PrefabFolder = "Assets/_Project/Art/Environment/Resources/Interior";
        private const string GlassMaterialPath = SourceFolder + "/Materials/Glass.mat";

        private static readonly (string fbx, string prefabName, bool normalizeDoor)[] Catalog =
        {
            ("Door_1.fbx", "Door", true),
            ("Door_2.fbx", "DoorAlt", true),
            ("Door_Double.fbx", "DoorDouble", true),
            ("Window_Small1.fbx", "WindowSmall", false),
            ("Window_Large1.fbx", "WindowLarge", false),
            ("Light_CeilingSingle.fbx", "LightCeiling", false),
            ("Light_Ceiling1.fbx", "LightCeilingAlt", false),
            ("Light_Desk.fbx", "LightDesk", false),
            ("Shelf_Large.fbx", "ShelfLarge", false),
            ("Shelf_1.fbx", "Shelf", false),
            ("Bookshelf.fbx", "Bookshelf", false),
            ("Kitchen_Cabinet1.fbx", "Cabinet", false),
            ("Table_RoundSmall.fbx", "Table", false),
            ("Chair_1.fbx", "Chair", false),
        };

        public static void Run()
        {
            try
            {
                EnsureFolder(PrefabFolder);
                AssetDatabase.Refresh();

                int built = 0;
                foreach ((string fbx, string prefabName, bool normalizeDoor) entry in Catalog)
                {
                    string fbxPath = $"{SourceFolder}/{entry.fbx}";
                    if (!File.Exists(Path.GetFullPath(fbxPath)))
                    {
                        // AssetDatabase path — File.Exists needs project-relative resolved.
                        if (AssetDatabase.LoadAssetAtPath<Object>(fbxPath) == null)
                        {
                            Debug.LogWarning($"InteriorPackImportTool: missing {fbxPath}");
                            continue;
                        }
                    }

                    if (ImportOne(fbxPath, entry.prefabName, entry.normalizeDoor))
                    {
                        built++;
                    }
                }

                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
                Debug.Log($"[InteriorPackImportTool] Built {built} prefab(s) under {PrefabFolder}");
                EditorApplication.Exit(0);
            }
            catch (System.Exception ex)
            {
                Debug.LogError("[InteriorPackImportTool] Failed: " + ex);
                EditorApplication.Exit(1);
            }
        }

        [MenuItem("Tools/LogiCard/Import Interior Pack Prefabs")]
        public static void RunFromMenu()
        {
            EnsureFolder(PrefabFolder);
            AssetDatabase.Refresh();
            int built = 0;
            foreach ((string fbx, string prefabName, bool normalizeDoor) entry in Catalog)
            {
                string fbxPath = $"{SourceFolder}/{entry.fbx}";
                if (AssetDatabase.LoadAssetAtPath<Object>(fbxPath) == null)
                {
                    Debug.LogWarning($"InteriorPackImportTool: missing {fbxPath}");
                    continue;
                }

                if (ImportOne(fbxPath, entry.prefabName, entry.normalizeDoor))
                {
                    built++;
                }
            }

            AssetDatabase.SaveAssets();
            Debug.Log($"[InteriorPackImportTool] Built {built} prefab(s) (menu).");
        }

        [MenuItem("Tools/LogiCard/Fix Interior Glass Transparency")]
        public static void FixGlassTransparencyFromMenu()
        {
            FixGlassTransparency();
            AssetDatabase.SaveAssets();
        }

        /// <summary>Run: -executeMethod LogiCard.Art.Editor.InteriorPackImportTool.RunGlassFix</summary>
        public static void RunGlassFix()
        {
            try
            {
                FixGlassTransparency();
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
                EditorApplication.Exit(0);
            }
            catch (System.Exception ex)
            {
                Debug.LogError("[InteriorPackImportTool] Glass fix failed: " + ex);
                EditorApplication.Exit(1);
            }
        }

        /// <summary>
        /// The generic per-FBX material loop in <see cref="ImportOne"/> re-hooks every extracted
        /// material onto URP/Lit uniformly (color/smoothness/metallic only) — it has no notion of
        /// "this one should be see-through." <c>Glass.mat</c> (used by <c>WindowSmall</c>/
        /// <c>WindowLarge</c>) landed fully Opaque as a result, silently blocking the warm emissive
        /// glow pane <c>BoardView</c> places just behind each window frame — checkpoint 3's "lit
        /// window" dressing was invisible. Confirmed via a human screenshot review, 2026-08-10. Same
        /// fix shape as <c>BoardWeatherPocket.ConfigureAlphaBlend</c>.
        /// </summary>
        private static void FixGlassTransparency()
        {
            var material = AssetDatabase.LoadAssetAtPath<Material>(GlassMaterialPath);
            if (material == null)
            {
                Debug.LogWarning($"[InteriorPackImportTool] No material at {GlassMaterialPath} to fix.");
                return;
            }

            if (material.HasProperty("_Surface"))
            {
                material.SetFloat("_Surface", 1f); // 1 = Transparent
            }

            if (material.HasProperty("_Blend"))
            {
                material.SetFloat("_Blend", 0f); // 0 = Alpha
            }

            if (material.HasProperty("_SrcBlend"))
            {
                material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            }

            if (material.HasProperty("_DstBlend"))
            {
                material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            }

            if (material.HasProperty("_ZWrite"))
            {
                material.SetInt("_ZWrite", 0);
            }

            // NOTE: _ALPHABLEND_ON (used by BoardWeatherPocket.ConfigureAlphaBlend) is a *particle*
            // shader keyword — this material is Universal Render Pipeline/Lit, whose surface-type
            // keyword is _SURFACE_TYPE_TRANSPARENT. Using the wrong one is silently ineffective
            // (Unity just files it under the material's own m_InvalidKeywords, no error) — caught by
            // inspecting the regenerated .mat directly after the first attempt, not assumed correct.
            material.DisableKeyword("_ALPHATEST_ON");
            material.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
            material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
            material.SetOverrideTag("RenderType", "Transparent");
            material.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;

            // Real glass should also actually let some light through, not just stop occluding
            // what's behind it — the extracted color's alpha is whatever the source FBX authored
            // (often a fully opaque "colored panel" look); push it down so it reads as translucent.
            Color color = material.HasProperty("_BaseColor") ? material.GetColor("_BaseColor") : material.color;
            color.a = Mathf.Min(color.a, 0.35f);
            material.color = color;
            if (material.HasProperty("_BaseColor"))
            {
                material.SetColor("_BaseColor", color);
            }

            EditorUtility.SetDirty(material);
            Debug.Log($"[InteriorPackImportTool] Fixed transparency on {GlassMaterialPath}.");
        }

        private static bool ImportOne(string fbxPath, string prefabName, bool normalizeDoor)
        {
            var importer = AssetImporter.GetAtPath(fbxPath) as ModelImporter;
            if (importer == null)
            {
                Debug.LogError($"InteriorPackImportTool: no ModelImporter at {fbxPath}");
                return false;
            }

            importer.importNormals = ModelImporterNormals.Calculate;
            importer.normalCalculationMode = ModelImporterNormalCalculationMode.AreaAndAngleWeighted;
            importer.normalSmoothingSource = ModelImporterNormalSmoothingSource.FromAngle;
            importer.normalSmoothingAngle = 60f;
            importer.materialImportMode = ModelImporterMaterialImportMode.ImportStandard;
            importer.materialLocation = ModelImporterMaterialLocation.External;
            // Quaternius house pack is authored ~meters; we normalize doors ourselves.
            importer.globalScale = 1f;
            importer.SaveAndReimport();

            Shader urpLit = Shader.Find("Universal Render Pipeline/Lit");
            if (urpLit == null)
            {
                Debug.LogError("InteriorPackImportTool: URP Lit shader missing.");
                return false;
            }

            int converted = 0;
            foreach (string dependency in AssetDatabase.GetDependencies(fbxPath, true))
            {
                if (!dependency.EndsWith(".mat"))
                {
                    continue;
                }

                var material = AssetDatabase.LoadAssetAtPath<Material>(dependency);
                if (material == null || material.shader == urpLit)
                {
                    continue;
                }

                Color baseColor = material.HasProperty("_Color") ? material.color : Color.white;
                material.shader = urpLit;
                material.SetColor("_BaseColor", baseColor);
                material.SetFloat("_Smoothness", 0.35f);
                material.SetFloat("_Metallic", 0f);
                EditorUtility.SetDirty(material);
                converted++;
            }

            var sourceGo = AssetDatabase.LoadAssetAtPath<GameObject>(fbxPath);
            if (sourceGo == null)
            {
                Debug.LogError($"InteriorPackImportTool: could not load {fbxPath}");
                return false;
            }

            var instance = (GameObject)Object.Instantiate(sourceGo);
            instance.name = prefabName;

            if (normalizeDoor)
            {
                NormalizeDoorPivotAndScale(instance);
            }
            else
            {
                NormalizePropPivot(instance);
            }

            StripColliders(instance);

            string prefabPath = $"{PrefabFolder}/{prefabName}.prefab";
            PrefabUtility.SaveAsPrefabAsset(instance, prefabPath);
            Object.DestroyImmediate(instance);

            Debug.Log($"InteriorPackImportTool: {prefabPath} (URP mats={converted}, doorNormalize={normalizeDoor})");
            return true;
        }

        /// <summary>
        /// Door leaf: width on local X ≈ 1, height on Y ≈ 1, pivot at bottom-center.
        /// BoardView scales (length, height, 1) to fit the wall segment.
        /// </summary>
        private static void NormalizeDoorPivotAndScale(GameObject root)
        {
            Bounds local = CalculateLocalBounds(root);
            if (local.size.sqrMagnitude < 1e-8f)
            {
                return;
            }

            Vector3 offset = new Vector3(-local.center.x, -local.min.y, -local.center.z);
            var wrap = new GameObject("Mesh");
            wrap.transform.SetParent(root.transform, false);

            List<Transform> children = new List<Transform>();
            for (int i = 0; i < root.transform.childCount; i++)
            {
                Transform child = root.transform.GetChild(i);
                if (child != wrap.transform)
                {
                    children.Add(child);
                }
            }

            foreach (Transform child in children)
            {
                child.SetParent(wrap.transform, true);
            }

            wrap.transform.localPosition += offset;

            Bounds fitted = CalculateLocalBounds(root);
            float sx = fitted.size.x > 1e-4f ? 1f / fitted.size.x : 1f;
            float sy = fitted.size.y > 1e-4f ? 1f / fitted.size.y : 1f;
            float sz = Mathf.Min(sx, sy);
            wrap.transform.localScale = Vector3.Scale(wrap.transform.localScale, new Vector3(sx, sy, sz));
        }

        private static void NormalizePropPivot(GameObject root)
        {
            Bounds local = CalculateLocalBounds(root);
            if (local.size.sqrMagnitude < 1e-8f)
            {
                return;
            }

            Vector3 offset = new Vector3(-local.center.x, -local.min.y, -local.center.z);
            foreach (Transform child in root.transform)
            {
                child.localPosition += offset;
            }
        }

        private static Bounds CalculateLocalBounds(GameObject root)
        {
            var filters = root.GetComponentsInChildren<MeshFilter>();
            Bounds bounds = new Bounds(Vector3.zero, Vector3.zero);
            bool has = false;
            Matrix4x4 rootWorldToLocal = root.transform.worldToLocalMatrix;

            foreach (MeshFilter filter in filters)
            {
                Mesh mesh = filter.sharedMesh;
                if (mesh == null)
                {
                    continue;
                }

                Matrix4x4 localToRoot = rootWorldToLocal * filter.transform.localToWorldMatrix;
                Vector3[] corners =
                {
                    mesh.bounds.min,
                    mesh.bounds.max,
                    new Vector3(mesh.bounds.min.x, mesh.bounds.min.y, mesh.bounds.max.z),
                    new Vector3(mesh.bounds.min.x, mesh.bounds.max.y, mesh.bounds.min.z),
                    new Vector3(mesh.bounds.max.x, mesh.bounds.min.y, mesh.bounds.min.z),
                    new Vector3(mesh.bounds.min.x, mesh.bounds.max.y, mesh.bounds.max.z),
                    new Vector3(mesh.bounds.max.x, mesh.bounds.min.y, mesh.bounds.max.z),
                    new Vector3(mesh.bounds.max.x, mesh.bounds.max.y, mesh.bounds.min.z),
                };

                foreach (Vector3 corner in corners)
                {
                    Vector3 p = localToRoot.MultiplyPoint3x4(corner);
                    if (!has)
                    {
                        bounds = new Bounds(p, Vector3.zero);
                        has = true;
                    }
                    else
                    {
                        bounds.Encapsulate(p);
                    }
                }
            }

            return bounds;
        }

        private static void StripColliders(GameObject root)
        {
            foreach (Collider col in root.GetComponentsInChildren<Collider>())
            {
                Object.DestroyImmediate(col);
            }
        }

        private static void EnsureFolder(string assetPath)
        {
            if (AssetDatabase.IsValidFolder(assetPath))
            {
                return;
            }

            string[] parts = assetPath.Split('/');
            string current = parts[0];
            for (int i = 1; i < parts.Length; i++)
            {
                string next = current + "/" + parts[i];
                if (!AssetDatabase.IsValidFolder(next))
                {
                    AssetDatabase.CreateFolder(current, parts[i]);
                }

                current = next;
            }
        }
    }
}
#endif
