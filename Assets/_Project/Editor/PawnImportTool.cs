using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace LogiCard.EditorTools
{
    /// <summary>
    /// One-off tool for docs/PAWN_ART_REWORK_PLAN.md steps 2-4/7: reimports an archetype's source FBX,
    /// keeping its own per-part materials (Skin/Hair/Torso/Backpack/etc. — a uniform single material
    /// flattened the model to a solid-color blob, rejected on first look 2026-08-08) but re-hooking each
    /// one onto URP/Lit (step 7's Standard-shader/magenta trap) instead of Unity's default import shader,
    /// then builds a static prefab from it. Not a runtime system — PawnView.cs never calls into this; it
    /// only ever loads the resulting prefab. Menu items let this be rerun from an already-open Editor
    /// instead of requiring a batchmode -executeMethod pass.
    /// </summary>
    public static class PawnImportTool
    {
        [MenuItem("Tools/LogiCard/Import Scout")]
        public static void ImportScoutBatch()
        {
            ImportArchetype("Assets/_Project/Art/Characters/Resources/Scout/Adventurer.fbx", "Resources/Scout", "Scout", smoothness: 0.6f);
        }

        [MenuItem("Tools/LogiCard/Import Juggernaut")]
        public static void ImportJuggernautBatch()
        {
            ImportArchetype("Assets/_Project/Art/Characters/Resources/Juggernaut/Swat.fbx", "Resources/Juggernaut", "Juggernaut", smoothness: 0.6f);
        }

        public static void ImportArchetype(string fbxAssetPath, string archetypeFolder, string archetypeName, float smoothness)
        {
            var importer = AssetImporter.GetAtPath(fbxAssetPath) as ModelImporter;
            if (importer == null)
            {
                Debug.LogError($"PawnImportTool: no ModelImporter at {fbxAssetPath}");
                return;
            }

            importer.importNormals = ModelImporterNormals.Calculate;
            importer.normalCalculationMode = ModelImporterNormalCalculationMode.AreaAndAngleWeighted;
            importer.normalSmoothingSource = ModelImporterNormalSmoothingSource.FromAngle;
            importer.normalSmoothingAngle = 60f;

            // Keep the model's own per-part materials (skin tone, hair, pack, etc.) instead of
            // stripping them to one shared material — extracted as separate .mat assets next to the
            // FBX so each can be re-hooked onto URP/Lit individually below.
            importer.materialImportMode = ModelImporterMaterialImportMode.ImportStandard;
            importer.materialLocation = ModelImporterMaterialLocation.External;
            importer.SaveAndReimport();

            Shader urpLit = Shader.Find("Universal Render Pipeline/Lit");
            if (urpLit == null)
            {
                Debug.LogError("PawnImportTool: 'Universal Render Pipeline/Lit' shader not found — is URP installed?");
                return;
            }

            int converted = 0;
            foreach (string dependency in AssetDatabase.GetDependencies(fbxAssetPath, true))
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

                // Standard's _Color is the closest match to what the artist actually authored (flat
                // per-part diffuse, this pack ships no textures) — read it before swapping shaders,
                // since changing .shader does not remap same-meaning properties under new names.
                Color baseColor = material.HasProperty("_Color") ? material.color : Color.white;
                material.shader = urpLit;
                material.SetColor("_BaseColor", baseColor);
                material.SetFloat("_Smoothness", smoothness);
                EditorUtility.SetDirty(material);
                converted++;
            }

            var sourceGo = AssetDatabase.LoadAssetAtPath<GameObject>(fbxAssetPath);
            if (sourceGo == null)
            {
                Debug.LogError($"PawnImportTool: could not load model at {fbxAssetPath} after reimport.");
                return;
            }

            string folder = $"Assets/_Project/Art/Characters/{archetypeFolder}";
            var instance = (GameObject)PrefabUtility.InstantiatePrefab(sourceGo);

            string prefabPath = $"{folder}/{archetypeName}.prefab";
            PrefabUtility.SaveAsPrefabAsset(instance, prefabPath);
            Object.DestroyImmediate(instance);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log($"PawnImportTool: built {prefabPath}, re-hooked {converted} material(s) onto URP/Lit (smoothness={smoothness}), kept per-part colors.");
        }
    }
}
