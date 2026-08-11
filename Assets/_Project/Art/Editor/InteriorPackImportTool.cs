#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace LogiCard.Art.Editor
{
    /// <summary>
    /// Imports curated nappin OfficeEssentialsPack prefabs into URP/Lit prefabs under Resources/Interior
    /// for runtime BoardView loading (C53 checkpoint 3).
    ///
    /// Re-sourced from the original Quaternius Ultimate House Interior FBX pipeline to
    /// <c>Assets/nappin/OfficeEssentialsPack/</c> (2026-08 nappin art-style pivot — see
    /// docs/ART_PACK_RESEARCH.md). Pattern mirrors the shape used for the (reverted, unlicensed-pack)
    /// Synty PolygonOffice attempt: nappin ships ready-made prefabs with materials already external, so
    /// this pipeline instantiates the source *prefab* directly instead of running it through
    /// <c>ModelImporter</c> first. Each entry is instantiated as a *child* under a fresh empty root so
    /// the existing pivot/scale normalization (which reparents/rescales children, not the root itself)
    /// works unchanged, matching a raw-FBX-import hierarchy shape.
    ///
    /// <c>Assets/nappin/OfficeEssentialsPack/**</c> is read-only reference content (shared by anything
    /// else that might use this pack) — this tool never edits a nappin-owned asset. Every material this
    /// catalog touches gets **duplicated** into <c>Resources/Interior/Materials/</c> first (one copy per
    /// shared source material, cached across catalog entries), and only the duplicate is
    /// shader-converted / transparency-or-emission-fixed. Baked prefabs reference the duplicates.
    ///
    /// Two nappin-specific wrinkles vs. the Quaternius/PolygonOffice precedents:
    /// <list type="bullet">
    /// <item><c>(Prb)Door</c> / <c>(Prb)SeparatorDoor</c> each bundle a "*_frame" child (door casing)
    /// alongside the "*_body" swinging leaf — <see cref="BoardView.PlaceDoorMesh"/> wants the leaf only
    /// (BoardView draws its own wall/opening geometry), so <see cref="Catalog"/> entries for Door/DoorAlt
    /// list a <c>stripChildNames</c> set that deletes the frame child post-instantiate, before
    /// normalization runs.</item>
    /// <item>Some light fixture prefabs (<c>CeilingLight</c>, <c>CeilingLight1</c>, <c>DeskLight</c>) use
    /// <c>(Mat)EmissiveWarm.mat</c> (Standard shader, <c>_EMISSION</c> keyword + non-black
    /// <c>_EmissionColor</c>) — confirmed by direct inspection of each prefab's <c>m_Materials</c> list,
    /// not assumed from name. The generic conversion loop only carries _BaseColor/_Smoothness/_Metallic;
    /// <see cref="GetOrCreateConvertedMaterial"/> additionally re-enables <c>_EMISSION</c> and copies
    /// <c>_EmissionColor</c>/<c>_EmissionMap</c> for any source material that had it, or the glow is
    /// silently dropped by the shader swap — same bug shape as the Glass.mat transparency loss below,
    /// just on the emission axis instead of the surface-type axis.</item>
    /// </list>
    ///
    /// <c>DoorDouble</c> has no reasonable nappin match (no double-door asset anywhere in the pack's
    /// ~50 prefabs — confirmed by a full pass, not just a first grep) and is intentionally **not** in
    /// <see cref="Catalog"/>; the existing Quaternius-sourced <c>DoorDouble.prefab</c> at the output path
    /// is left untouched. It is also not currently placed by any <c>BoardView</c> map dressing call site,
    /// so the style mismatch has no visible impact today.
    ///
    /// <c>WindowSmall</c> and <c>WindowLarge</c> both source from nappin's single <c>(Prb)Window</c> mesh
    /// (the pack has no second window size) — differentiated at placement time by
    /// <c>BoardView.PlaceInteriorProp</c>'s existing per-call <c>uniformScale</c> (0.7 vs 0.9), same as
    /// every other catalog entry's real-world size is already resolved.
    ///
    /// Run: -executeMethod LogiCard.Art.Editor.InteriorPackImportTool.Run
    /// Menu: Tools → LogiCard → Import Interior Pack Prefabs
    /// </summary>
    public static class InteriorPackImportTool
    {
        private const string PrefabFolder = "Assets/_Project/Art/Environment/Resources/Interior";
        private const string MaterialFolder = PrefabFolder + "/Materials";
        private const string SourcePrefabs = "Assets/nappin/OfficeEssentialsPack/Prefabs";
        private const string GlassMaterialSourcePath = "Assets/nappin/OfficeEssentialsPack/Materials/(Mat)Glass.mat";
        private const string GlassMaterialCopyPath = MaterialFolder + "/(Mat)Glass_URP.mat";
        private const string FloorMaterialSourcePath = "Assets/nappin/OfficeEssentialsPack/Materials/(Mat)Floor.mat";

        /// <summary>
        /// Original nappin material asset path → its converted duplicate under <see cref="MaterialFolder"/>.
        /// Populated/reused across all <see cref="ImportOne"/> calls in a single <see cref="Run"/> so a
        /// shared source material (e.g. MetallicBlack, used by several fixtures) is only duplicated and
        /// shader-converted once.
        /// </summary>
        private static readonly Dictionary<string, Material> ConvertedMaterialCache = new Dictionary<string, Material>();

        /// <summary>
        /// (source prefab path(s), output prefab name, normalizeDoor, child names to strip before
        /// normalization). Sizes/shapes were confirmed via a throwaway batchmode bounds probe against
        /// the actual nappin meshes before picking, not guessed from names alone — see report for the
        /// measured dimensions and the door-leaf-vs-frame investigation.
        /// </summary>
        private static readonly (string[] sources, string prefabName, bool normalizeDoor, string[] stripChildNames)[] Catalog =
        {
            // Doors — "(Prb)Door"/"(Prb)SeparatorDoor" each bundle a "*_frame" (casing) sibling next to
            // the "*_body" swinging leaf under a shared "door"/"separatorDoor" group node. BoardView
            // draws its own wall/opening geometry around the leaf, so the frame is stripped here rather
            // than baked in (it would otherwise dominate the unit-normalize bounding box and duplicate
            // geometry the wall segment already provides).
            (new[] { $"{SourcePrefabs}/(Prb)Door.prefab" }, "Door", true, new[] { "door_frame" }),
            (new[] { $"{SourcePrefabs}/(Prb)SeparatorDoor.prefab" }, "DoorAlt", true, new[] { "separatorDoor_frame" }),
            // DoorDouble intentionally absent — no double-door asset anywhere in the pack; existing
            // Quaternius-sourced Resources/Interior/DoorDouble.prefab is left as-is (see class remarks).

            // Windows — nappin ships exactly one window prop; both slots source it, differentiated by
            // BoardView's existing per-call uniformScale (WindowSmall=0.7, WindowLarge=0.9).
            (new[] { $"{SourcePrefabs}/(Prb)Window.prefab" }, "WindowSmall", false, null),
            (new[] { $"{SourcePrefabs}/(Prb)Window.prefab" }, "WindowLarge", false, null),

            // Lights — CeilingLight/CeilingLight1 are genuinely different fixture silhouettes (measured,
            // not just a recolor: 0.768x2.327x2.038 vs 0.310x1.699x3.692 root-relative bounds). DeskLight
            // is the compact desk-height lamp. All three reference (Mat)EmissiveWarm.mat — confirmed via
            // each prefab's full m_Materials list (a 3-submesh renderer; a 2-line grep silently missed
            // the third slot on a first pass) — so the emission-preserving material conversion below
            // actually matters for this trio, not a theoretical concern.
            (new[] { $"{SourcePrefabs}/(Prb)CeilingLight.prefab" }, "LightCeiling", false, null),
            (new[] { $"{SourcePrefabs}/(Prb)CeilingLight1.prefab" }, "LightCeilingAlt", false, null),
            (new[] { $"{SourcePrefabs}/(Prb)DeskLight.prefab" }, "LightDesk", false, null),

            // Shelving — Shelves3 is the tallest/deepest run (0.514x3.090x2.612, "Large"), Shelf1 is the
            // smallest/simplest single wall-mounted unit ("Shelf"), Shelves1 is a mid-size tall unit that
            // reads as a bookcase distinct from both ("Bookshelf").
            (new[] { $"{SourcePrefabs}/(Prb)Shelves3.prefab" }, "ShelfLarge", false, null),
            (new[] { $"{SourcePrefabs}/(Prb)Shelf1.prefab" }, "Shelf", false, null),
            (new[] { $"{SourcePrefabs}/(Prb)Shelves1.prefab" }, "Bookshelf", false, null),

            // Cabinet — Storage2 (0.880x1.202x1.536) is a generic storage cabinet footprint; Wardrobe/
            // BigDrawer measured far larger/deeper (bedroom-scale, not a fit for this slot).
            (new[] { $"{SourcePrefabs}/(Prb)Storage2.prefab" }, "Cabinet", false, null),

            // Table — CoffeTable's low-profile proportions (1.539x0.626x2.234) match the old Quaternius
            // "Table_RoundSmall" generic decorative-table role better than the taller Desk/Conference
            // options.
            (new[] { $"{SourcePrefabs}/(Prb)CoffeTable.prefab" }, "Table", false, null),

            // Chair — OfficeChair, matching the pack's own theme and a reasonable generic chair silhouette.
            (new[] { $"{SourcePrefabs}/(Prb)OfficeChair.prefab" }, "Chair", false, null),
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
        /// Runs the full catalog import twice. Empirically (confirmed during the PolygonOffice
        /// precedent), a brand-new URP/Lit material duplicated from a Standard-shader source can have
        /// its blend-state properties and keywords silently re-derived by Unity's own asset-import
        /// pipeline the *first* time that specific asset is ever saved/imported — clobbering values this
        /// tool just set a moment earlier. Re-running the whole conversion against the now-existing
        /// assets is a cheap, reliable way to sidestep that first-import quirk.
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
                foreach ((string[] sources, string prefabName, bool normalizeDoor, string[] stripChildNames) entry in Catalog)
                {
                    if (ImportOne(entry.sources, entry.prefabName, entry.normalizeDoor, entry.stripChildNames))
                    {
                        built++;
                    }
                }

                FixGlassTransparency();
                BakeFloorMaterial();
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
            }

            return built;
        }

        [MenuItem("Tools/LogiCard/Bake Interior Floor Material")]
        public static void BakeFloorMaterialFromMenu()
        {
            BakeFloorMaterial();
            AssetDatabase.SaveAssets();
        }

        /// <summary>Run: -executeMethod LogiCard.Art.Editor.InteriorPackImportTool.RunBakeFloorMaterial
        /// (batchmode-safe — exits itself rather than relying on an external -quit, which has raced
        /// -executeMethod/-runTests before in this project and can end the process before the method
        /// body actually runs).</summary>
        public static void RunBakeFloorMaterial()
        {
            try
            {
                BakeFloorMaterial();
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
                EditorApplication.Exit(0);
            }
            catch (System.Exception ex)
            {
                Debug.LogError("[InteriorPackImportTool] Bake floor material failed: " + ex);
                EditorApplication.Exit(1);
            }
        }

        /// <summary>
        /// Bakes a URP/Lit duplicate of nappin's <c>(Mat)Floor.mat</c> (flat near-black matte, no
        /// texture) to <c>Resources/Interior/Materials/(Mat)Floor_URP.mat</c> so
        /// <see cref="LogiCard.Board.BoardSurfaceMaterials"/> can load it by name at runtime — used to
        /// replace the Poly Haven procedural Hall/Vault floor materials directly with the asset-pack
        /// floor per the human's explicit direction (2026-08-11: "use asset pack floor directly"), rather
        /// than continuing to debug/retune the procedural approach. Reuses
        /// <see cref="GetOrCreateConvertedMaterial"/> unchanged — same duplicate-then-convert discipline
        /// as every other nappin material this tool touches, never mutates the original.
        /// </summary>
        private static void BakeFloorMaterial()
        {
            Material material = GetOrCreateConvertedMaterial(FloorMaterialSourcePath);
            if (material == null)
            {
                Debug.LogWarning($"[InteriorPackImportTool] No material at {FloorMaterialSourcePath} to bake.");
            }
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
        /// The generic per-prefab material loop in <see cref="ImportOne"/> (via
        /// <see cref="GetOrCreateConvertedMaterial"/>) re-hooks every extracted material onto URP/Lit
        /// uniformly (color/smoothness/metallic + emission if present) — it has no notion of "this one
        /// should be see-through." <c>(Mat)Glass.mat</c> (shared by DoorAlt's frame — already stripped —
        /// and both WindowSmall/WindowLarge) ships as built-in Standard shader already configured
        /// Transparent (_Mode=3/Transparent, _SrcBlend=One, _DstBlend=OneMinusSrcAlpha, _ZWrite=0, base
        /// alpha≈0.035 — confirmed by reading the raw .mat, not assumed) — after the blind shader swap it
        /// silently becomes Opaque URP/Lit unless fixed up here. Same exact bug shape as the prior
        /// Quaternius/PolygonOffice Glass.mat regressions: <c>_SURFACE_TYPE_TRANSPARENT</c> is the
        /// correct URP/Lit surface-type keyword, not <c>_ALPHABLEND_ON</c> (a particle-shader keyword —
        /// silently ineffective, filed under the material's own m_InvalidKeywords, no error).
        /// </summary>
        private static void FixGlassTransparency()
        {
            // Ensure the duplicate (never the nappin-owned original) exists and is URP-converted even if
            // this runs standalone (menu / RunGlassFix) without a prior full Run().
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
            // behind it. Source alpha (≈0.035) is already extremely translucent; keep it modest but not
            // fully invisible.
            Color color = material.HasProperty("_BaseColor") ? material.GetColor("_BaseColor") : material.color;
            color.a = Mathf.Clamp(color.a, 0.08f, 0.35f);
            material.color = color;
            if (material.HasProperty("_BaseColor"))
            {
                material.SetColor("_BaseColor", color);
            }

            EditorUtility.SetDirty(material);
            Debug.Log($"[InteriorPackImportTool] Fixed transparency on {GlassMaterialCopyPath}.");
        }

        /// <summary>
        /// Returns the URP/Lit-converted duplicate of a nappin-owned source material, creating and
        /// converting it under <see cref="MaterialFolder"/> on first request and reusing it for every
        /// subsequent request in the same run (see <see cref="ConvertedMaterialCache"/>). Never mutates
        /// <paramref name="originalAssetPath"/> itself — that file lives under the read-only
        /// <c>Assets/nappin/OfficeEssentialsPack/</c> reference content.
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
                // Copies the original's shader + all serialized properties into a brand-new asset — the
                // original object is never touched.
                copy = new Material(original);
                AssetDatabase.CreateAsset(copy, copyPath);
            }

            if (copy.shader != urpLit)
            {
                // Standard's own emission state must be read from the ORIGINAL (Standard-shader) material
                // before the shader swap — HasProperty/IsKeywordEnabled on `copy` after the swap would be
                // asking the new URP/Lit shader, which happens to expose the same property names but
                // isn't necessarily what the source intended.
                bool hadEmission = original.IsKeywordEnabled("_EMISSION");
                Color emissionColor = original.HasProperty("_EmissionColor") ? original.GetColor("_EmissionColor") : Color.black;
                Texture emissionMap = original.HasProperty("_EmissionMap") ? original.GetTexture("_EmissionMap") : null;

                Color baseColor = copy.HasProperty("_Color") ? copy.color : Color.white;
                copy.shader = urpLit;
                copy.SetColor("_BaseColor", baseColor);
                copy.SetFloat("_Smoothness", 0.35f);
                copy.SetFloat("_Metallic", 0f);

                // Preserve emission — the generic color/smoothness/metallic carry-over above has no
                // notion of "this one glows." Confirmed by direct inspection that CeilingLight,
                // CeilingLight1, and DeskLight all reference (Mat)EmissiveWarm.mat (a 3rd m_Materials
                // slot a shallow 2-line grep missed on a first pass) with _EMISSION enabled and a
                // non-black _EmissionColor — without this, the shader swap would silently drop the glow,
                // same bug shape as the Glass.mat transparency loss above just on a different axis.
                if (hadEmission && emissionColor != Color.black)
                {
                    copy.EnableKeyword("_EMISSION");
                    copy.globalIlluminationFlags = MaterialGlobalIlluminationFlags.RealtimeEmissive;
                    copy.SetColor("_EmissionColor", emissionColor);
                    if (emissionMap != null && copy.HasProperty("_EmissionMap"))
                    {
                        copy.SetTexture("_EmissionMap", emissionMap);
                    }
                }

                EditorUtility.SetDirty(copy);
            }

            ConvertedMaterialCache[originalAssetPath] = copy;
            return copy;
        }

        private static bool ImportOne(string[] sourcePrefabPaths, string prefabName, bool normalizeDoor, string[] stripChildNames)
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
            // original nappin .mat assets are never modified — see GetOrCreateConvertedMaterial.
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

            // Wrap in a fresh empty root — matches the shape a raw FBX instantiate produces (a root node
            // with mesh-carrying children), which is what NormalizeDoorPivotAndScale / NormalizePropPivot
            // expect to reparent/rescale.
            var instance = new GameObject(prefabName);
            foreach (GameObject sourceGo in sourceGos)
            {
                Object.Instantiate(sourceGo, instance.transform, false);
            }

            if (stripChildNames != null)
            {
                StripNamedChildren(instance, stripChildNames);
            }

            // The instantiated hierarchy still points at the original nappin materials (Unity preserves
            // material references on Instantiate) — swap every renderer over to the converted duplicates
            // now that they exist.
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

            Debug.Log($"InteriorPackImportTool: {prefabPath} (URP mats={converted}, sources={sourcePrefabPaths.Length}, doorNormalize={normalizeDoor}, stripped={(stripChildNames == null ? 0 : stripChildNames.Length)})");
            return true;
        }

        /// <summary>Destroys every descendant of <paramref name="root"/> whose name is in <paramref name="names"/>.</summary>
        private static void StripNamedChildren(GameObject root, string[] names)
        {
            var toDestroy = new List<Transform>();
            foreach (Transform t in root.GetComponentsInChildren<Transform>(true))
            {
                if (t == root.transform)
                {
                    continue;
                }

                foreach (string name in names)
                {
                    if (t.name == name)
                    {
                        toDestroy.Add(t);
                        break;
                    }
                }
            }

            foreach (Transform t in toDestroy)
            {
                if (t != null)
                {
                    Object.DestroyImmediate(t.gameObject);
                }
            }
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
