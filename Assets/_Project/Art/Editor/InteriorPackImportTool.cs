#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace LogiCard.Art.Editor
{
    /// <summary>
    /// Imports curated Synty POLYGON Office prefabs into URP/Lit prefabs under Resources/Interior
    /// for runtime BoardView loading (C53 checkpoint 3).
    ///
    /// Re-sourced from the original Quaternius Ultimate House Interior FBX pipeline (2026-08 session)
    /// to <c>Assets/PolygonOffice/</c> — see docs/DRAFT_HANDOFF.md's 2026-08-11 entry for why. Unlike
    /// the old Quaternius source (raw FBX needing material extraction), PolygonOffice ships ready-made
    /// prefabs with materials already external — so this pipeline instantiates the source *prefab*
    /// directly instead of running it through <c>ModelImporter</c> first. Because a PolygonOffice prop
    /// prefab's own root GameObject carries the MeshRenderer directly (no child node the way a raw FBX
    /// import produces), each entry is instantiated as a *child* under a fresh empty root — that's what
    /// lets the existing pivot/scale normalization (which reparents/rescales children, not the root
    /// itself) work unchanged. Door entries can list two source prefabs (a matched L/R pair) that get
    /// instantiated at their authored local transforms so they compose into one double-door mesh before
    /// normalization.
    ///
    /// Pattern otherwise mirrors <c>PawnImportTool</c>: re-hook materials to URP/Lit, then bake a
    /// normalized prefab. Door prefabs are unit-scaled (width≈1, height≈1) with pivot at bottom-center
    /// so BoardView can fit them to wall-segment geometry.
    ///
    /// <c>Assets/PolygonOffice/**</c> is read-only reference content (shared by everything else that
    /// might ever get imported from that pack) — unlike the old Quaternius pipeline, which owned its
    /// source FBX outright and could mutate its materials in place, this tool never edits a
    /// PolygonOffice-owned asset. Every material this catalog touches gets **duplicated** into
    /// <c>Resources/Interior/Materials/</c> first (one copy per shared source material, cached across
    /// catalog entries so e.g. the door/window glass material isn't duplicated 4 times), and only the
    /// duplicate is shader-converted / transparency-fixed. Baked prefabs reference the duplicates.
    ///
    /// NOTE: PolygonOffice's licensing status (bundled-reseller import, not a confirmed individual Asset
    /// Store purchase) is a tracked pre-ship TODO — see docs/DRAFT_HANDOFF.md. Not this tool's concern,
    /// just don't lose sight of it.
    ///
    /// Run: -executeMethod LogiCard.Art.Editor.InteriorPackImportTool.Run
    /// Menu: Tools → LogiCard → Import Interior Pack Prefabs
    /// </summary>
    public static class InteriorPackImportTool
    {
        private const string PrefabFolder = "Assets/_Project/Art/Environment/Resources/Interior";
        private const string MaterialFolder = PrefabFolder + "/Materials";
        private const string GlassMaterialSourcePath = "Assets/PolygonOffice/Materials/PolygonOffice_Material_Glass.mat";
        private const string GlassMaterialCopyPath = MaterialFolder + "/PolygonOffice_Material_Glass_URP.mat";

        /// <summary>
        /// Original PolygonOffice material asset → its converted duplicate under
        /// <see cref="MaterialFolder"/>. Populated/reused across all <see cref="ImportOne"/> calls in a
        /// single <see cref="Run"/> so a shared source material (e.g. the glass material referenced by
        /// both doors and windows) is only duplicated and shader-converted once.
        /// </summary>
        private static readonly Dictionary<string, Material> ConvertedMaterialCache = new Dictionary<string, Material>();

        private const string Buildings = "Assets/PolygonOffice/Prefabs/Buildings";
        private const string Furniture = "Assets/PolygonOffice/Prefabs/Props/Furniture";
        private const string RoofProps = "Assets/PolygonOffice/Prefabs/Props/Roof Props";
        private const string DeskProps = "Assets/PolygonOffice/Prefabs/Props/Desk Props";

        /// <summary>
        /// (source prefab path(s), output prefab name, normalizeDoor). Sizes/shapes below were
        /// confirmed via a throwaway batchmode bounds probe against the actual PolygonOffice meshes
        /// before picking, not guessed from names alone — see report for the measured dimensions.
        /// </summary>
        private static readonly (string[] sources, string prefabName, bool normalizeDoor)[] Catalog =
        {
            // Doors — Door_01/Door_02 share identical frame bounds (1.03w x 1.97h); Door_01 carries a
            // glass insert (visually distinct "alt"), Door_02 is the plain leaf. Door_Large_01_L/R are
            // authored already offset around a shared local origin (L center x=-0.463, R center
            // x=+0.463) so instantiating both at identity composes them into one double door.
            (new[] { $"{Buildings}/SM_Bld_Door_02.prefab" }, "Door", true),
            (new[] { $"{Buildings}/SM_Bld_Door_01.prefab" }, "DoorAlt", true),
            (new[] { $"{Buildings}/SM_Bld_Door_Large_01_L.prefab", $"{Buildings}/SM_Bld_Door_Large_01_R.prefab" }, "DoorDouble", true),

            // Windows — PolygonOffice has no standalone window-pane prop (windows are always part of a
            // wall module in this kit); closest fit is the "Wall_Glass" module, which is almost entirely
            // glass with a thin frame rather than a decorated wall segment. Small=2.5m module,
            // Large=5m module. Both reference PolygonOffice_Material_Glass.mat, fixed up below.
            (new[] { $"{Buildings}/SM_Bld_Wall_Glass_01.prefab" }, "WindowSmall", false),
            (new[] { $"{Buildings}/SM_Bld_Wall_Glass_Large_01.prefab" }, "WindowLarge", false),

            // Lights — Light_01 is a slim hanging pendant; Ceiling_Panel_Light_01 is a flush recessed
            // panel, genuinely different fixture type from Light_01 (not just a recolor), matching the
            // "Alt" naming intent. Desklamp_01 is a compact desk-height lamp.
            (new[] { $"{RoofProps}/SM_Prop_Light_01.prefab" }, "LightCeiling", false),
            (new[] { $"{Buildings}/SM_Bld_Ceiling_Panel_Light_01.prefab" }, "LightCeilingAlt", false),
            (new[] { $"{DeskProps}/SM_Prop_Desklamp_01.prefab" }, "LightDesk", false),

            // Shelving — measured bounds picked the size tier: Shelf_07 is the widest run (3.07m,
            // "Large"), Shelf_04 is the smallest/squattest ("Shelf"), Shelf_03 is the tallest/narrowest
            // (1.93m, reads as a bookcase).
            (new[] { $"{Furniture}/SM_Prop_Shelf_07.prefab" }, "ShelfLarge", false),
            (new[] { $"{Furniture}/SM_Prop_Shelf_04.prefab" }, "Shelf", false),
            (new[] { $"{Furniture}/SM_Prop_Shelf_03.prefab" }, "Bookshelf", false),

            (new[] { $"{Furniture}/SM_Prop_Cabinets_02.prefab" }, "Cabinet", false),
            (new[] { $"{Furniture}/SM_Prop_Table_Round_01.prefab" }, "Table", false),
            (new[] { $"{Furniture}/SM_Prop_Chair_01.prefab" }, "Chair", false),
        };

        public static void Run()
        {
            try
            {
                int built = RunImportPasses();
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
            int built = RunImportPasses();
            Debug.Log($"[InteriorPackImportTool] Built {built} prefab(s) (menu).");
        }

        /// <summary>
        /// Runs the full catalog import twice. Empirically, a brand-new URP/Lit material duplicated
        /// from a Standard-shader source (see <see cref="GetOrCreateConvertedMaterial"/>) can have its
        /// blend-state properties (_SrcBlend/_DstBlend) and keywords silently re-derived by Unity's own
        /// asset-import pipeline the *first* time that specific asset is ever saved/imported — clobbering
        /// the values this tool just set a moment earlier back to shader defaults (confirmed by
        /// inspecting the serialized .mat: a single-pass run left the glass material's blend state
        /// wrong — <c>_SrcBlend: 1</c> instead of <c>SrcAlpha (5)</c> — even though the exact same
        /// property-set code ran; a second pass against the now-already-imported asset converged
        /// correctly every time). Re-running the whole conversion against the now-existing assets is a
        /// cheap, reliable way to sidestep that first-import quirk rather than depending on its cause.
        /// </summary>
        private static int RunImportPasses()
        {
            EnsureFolder(PrefabFolder);
            EnsureFolder(MaterialFolder);
            ConvertedMaterialCache.Clear();
            AssetDatabase.Refresh();

            int built = 0;
            for (int pass = 0; pass < 2; pass++)
            {
                built = 0;
                foreach ((string[] sources, string prefabName, bool normalizeDoor) entry in Catalog)
                {
                    if (ImportOne(entry.sources, entry.prefabName, entry.normalizeDoor))
                    {
                        built++;
                    }
                }

                FixGlassTransparency();
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
            }

            return built;
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
        /// The generic per-prefab material loop in <see cref="ImportOne"/> re-hooks every extracted
        /// material onto URP/Lit uniformly (color/smoothness/metallic only) — it has no notion of "this
        /// one should be see-through." <c>PolygonOffice_Material_Glass.mat</c> (shared by WindowSmall/
        /// WindowLarge and the glass-paneled DoorAlt/DoorDouble) ships as built-in Standard shader
        /// already configured Transparent (Fade mode, alpha 0.491) — after the blind shader swap it
        /// silently becomes Opaque URP/Lit unless fixed up here. Same exact bug shape as the prior
        /// Quaternius Glass.mat regression (2026-08-10, confirmed only by inspecting the regenerated
        /// .mat directly): <c>_SURFACE_TYPE_TRANSPARENT</c> is the correct URP/Lit surface-type keyword,
        /// not <c>_ALPHABLEND_ON</c> (a particle-shader keyword — silently ineffective, filed under the
        /// material's own m_InvalidKeywords, no error). Same fix shape as
        /// <c>BoardWeatherPocket.ConfigureAlphaBlend</c>.
        /// </summary>
        private static void FixGlassTransparency()
        {
            // Ensure the duplicate (never the PolygonOffice-owned original) exists and is URP-converted
            // even if this runs standalone (menu / RunGlassFix) without a prior full Run().
            Material material = GetOrCreateConvertedMaterial(GlassMaterialSourcePath);
            if (material == null)
            {
                Debug.LogWarning($"[InteriorPackImportTool] No material at {GlassMaterialSourcePath} to fix.");
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

            material.DisableKeyword("_ALPHATEST_ON");
            material.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
            material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
            material.SetOverrideTag("RenderType", "Transparent");
            material.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;

            // Real glass should also actually let some light through, not just stop occluding what's
            // behind it. Source alpha (0.491) is already reasonably translucent; keep it modest.
            Color color = material.HasProperty("_BaseColor") ? material.GetColor("_BaseColor") : material.color;
            color.a = Mathf.Min(color.a, 0.35f);
            material.color = color;
            if (material.HasProperty("_BaseColor"))
            {
                material.SetColor("_BaseColor", color);
            }

            EditorUtility.SetDirty(material);
            Debug.Log($"[InteriorPackImportTool] Fixed transparency on {GlassMaterialCopyPath}.");
        }

        /// <summary>
        /// Returns the URP/Lit-converted duplicate of a PolygonOffice-owned source material, creating
        /// and converting it under <see cref="MaterialFolder"/> on first request and reusing it for
        /// every subsequent request in the same run (see <see cref="ConvertedMaterialCache"/>). Never
        /// mutates <paramref name="originalAssetPath"/> itself — that file lives under the read-only
        /// <c>Assets/PolygonOffice/</c> reference content.
        /// </summary>
        private static Material GetOrCreateConvertedMaterial(string originalAssetPath)
        {
            if (ConvertedMaterialCache.TryGetValue(originalAssetPath, out Material cached) && cached != null)
            {
                return cached;
            }

            var original = AssetDatabase.LoadAssetAtPath<Material>(originalAssetPath);
            if (original == null)
            {
                return null;
            }

            Shader urpLit = Shader.Find("Universal Render Pipeline/Lit");
            if (urpLit == null)
            {
                Debug.LogError("InteriorPackImportTool: URP Lit shader missing.");
                return null;
            }

            string copyPath = $"{MaterialFolder}/{original.name}_URP.mat";
            Material copy = AssetDatabase.LoadAssetAtPath<Material>(copyPath);
            if (copy == null)
            {
                // Copies the original's shader + all serialized properties into a brand-new asset —
                // the original object is never touched.
                copy = new Material(original);
                AssetDatabase.CreateAsset(copy, copyPath);
            }

            if (copy.shader != urpLit)
            {
                Color baseColor = copy.HasProperty("_Color") ? copy.color : Color.white;
                copy.shader = urpLit;
                copy.SetColor("_BaseColor", baseColor);
                copy.SetFloat("_Smoothness", 0.35f);
                copy.SetFloat("_Metallic", 0f);
                EditorUtility.SetDirty(copy);
            }

            ConvertedMaterialCache[originalAssetPath] = copy;
            return copy;
        }

        private static bool ImportOne(string[] sourcePrefabPaths, string prefabName, bool normalizeDoor)
        {
            var sourceGos = new List<GameObject>();
            foreach (string sourcePath in sourcePrefabPaths)
            {
                var sourceGo = AssetDatabase.LoadAssetAtPath<GameObject>(sourcePath);
                if (sourceGo == null)
                {
                    Debug.LogWarning($"InteriorPackImportTool: missing source prefab {sourcePath}");
                    continue;
                }

                sourceGos.Add(sourceGo);
            }

            if (sourceGos.Count == 0)
            {
                Debug.LogError($"InteriorPackImportTool: no valid source prefabs for {prefabName}");
                return false;
            }

            // Duplicate + URP-convert every material this entry's source prefab(s) depend on. The
            // original PolygonOffice .mat assets are never modified — see GetOrCreateConvertedMaterial.
            int converted = 0;
            foreach (string sourcePath in sourcePrefabPaths)
            {
                foreach (string dependency in AssetDatabase.GetDependencies(sourcePath, true))
                {
                    if (!dependency.EndsWith(".mat"))
                    {
                        continue;
                    }

                    bool alreadyCached = ConvertedMaterialCache.ContainsKey(dependency);
                    if (GetOrCreateConvertedMaterial(dependency) != null && !alreadyCached)
                    {
                        converted++;
                    }
                }
            }

            // Wrap in a fresh empty root — matches the shape a raw FBX instantiate produces (a root
            // node with mesh-carrying children), which is what NormalizeDoorPivotAndScale /
            // NormalizePropPivot expect to reparent/rescale. PolygonOffice's own prefab roots carry
            // their MeshRenderer directly with no children, so instantiating them as-is at top level
            // would leave normalization a no-op.
            var instance = new GameObject(prefabName);
            foreach (GameObject sourceGo in sourceGos)
            {
                Object.Instantiate(sourceGo, instance.transform, false);
            }

            // The instantiated hierarchy still points at the original PolygonOffice materials (Unity
            // preserves material references on Instantiate) — swap every renderer over to the converted
            // duplicates now that they exist.
            foreach (Renderer renderer in instance.GetComponentsInChildren<Renderer>(true))
            {
                Material[] materials = renderer.sharedMaterials;
                bool changed = false;
                for (int i = 0; i < materials.Length; i++)
                {
                    Material original = materials[i];
                    if (original == null)
                    {
                        continue;
                    }

                    string originalPath = AssetDatabase.GetAssetPath(original);
                    if (ConvertedMaterialCache.TryGetValue(originalPath, out Material replacement))
                    {
                        materials[i] = replacement;
                        changed = true;
                    }
                }

                if (changed)
                {
                    renderer.sharedMaterials = materials;
                }
            }

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

            Debug.Log($"InteriorPackImportTool: {prefabPath} (URP mats={converted}, sources={sourcePrefabPaths.Length}, doorNormalize={normalizeDoor})");
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
