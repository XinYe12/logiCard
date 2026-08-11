using UnityEditor;
using UnityEngine;

namespace LogiCard.EditorTools
{
    /// <summary>
    /// Throwaway diagnostic: for each baked Scout/Juggernaut prefab, logs every SkinnedMeshRenderer's
    /// mesh name/vertex count/bounds — used to check whether slots CharacterCustomizationImportTool never
    /// explicitly assigns (e.g. Mustache/T_Shirt, which have no SlotType entry in SlotLibrary.asset at
    /// all) are left carrying real leftover geometry baked into Base_Mesh.fbx's own child nodes, vs.
    /// genuinely empty/zero-vertex placeholders. Not part of the shipped pipeline.
    /// </summary>
    public static class BakedPrefabRendererDiagnostic
    {
        [MenuItem("Tools/LogiCard/Diagnostics/Log Baked Prefab Renderer Meshes")]
        public static void LogBakedPrefabRendererMeshes()
        {
            foreach (string path in new[]
                     {
                         "Assets/_Project/Art/Characters/Resources/Scout/Scout.prefab",
                         "Assets/_Project/Art/Characters/Resources/Juggernaut/Juggernaut.prefab",
                     })
            {
                var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                if (prefab == null)
                {
                    Debug.LogWarning($"BAKED-MISS\t{path}\tprefab not found");
                    continue;
                }

                foreach (var smr in prefab.GetComponentsInChildren<SkinnedMeshRenderer>(true))
                {
                    int verts = smr.sharedMesh == null ? -1 : smr.sharedMesh.vertexCount;
                    Bounds b = smr.sharedMesh == null ? default : smr.sharedMesh.bounds;
                    Debug.Log($"BAKED-RENDERER\t{path}\tGO={smr.gameObject.name}\tmesh={(smr.sharedMesh == null ? "<null>" : smr.sharedMesh.name)}\tverts={verts}\tsize=({b.size.x:F4},{b.size.y:F4},{b.size.z:F4})\tactive={smr.gameObject.activeSelf}");
                }

                // Combined renderer bounds + PawnView.TryBuildImported's scale math, to sanity-check
                // TargetVisualHeight=1.0f doesn't produce something absurdly tiny/huge for this pack.
                var renderers = prefab.GetComponentsInChildren<Renderer>(true);
                if (renderers.Length > 0)
                {
                    // sharedMesh.bounds is local; approximate using SkinnedMeshRenderer.localBounds which
                    // PawnImportTool/CharacterCustomizationImportTool set to sharedMesh.bounds already.
                    Bounds combined = new Bounds(renderers[0].transform.TransformPoint(default), Vector3.zero);
                    bool first = true;
                    foreach (var r in renderers)
                    {
                        if (!(r is SkinnedMeshRenderer smr2) || smr2.sharedMesh == null)
                        {
                            continue;
                        }

                        Bounds worldish = smr2.localBounds;
                        Vector3 center = smr2.transform.TransformPoint(worldish.center);
                        Vector3 extents = Vector3.Scale(worldish.extents, smr2.transform.lossyScale);
                        Bounds b2 = new Bounds(center, extents * 2f);
                        if (first)
                        {
                            combined = b2;
                            first = false;
                        }
                        else
                        {
                            combined.Encapsulate(b2);
                        }
                    }

                    Debug.Log($"BAKED-COMBINED-BOUNDS\t{path}\tsize=({combined.size.x:F4},{combined.size.y:F4},{combined.size.z:F4})");
                }
            }
        }
    }
}
