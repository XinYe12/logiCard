using System.Linq;
using UnityEditor;
using UnityEngine;

namespace LogiCard.EditorTools
{
    /// <summary>
    /// Throwaway diagnostic: logs each candidate part FBX's combined renderer bounds volume, so the
    /// Scout/Juggernaut part picks in <see cref="CharacterCustomizationImportTool"/> are chosen by an
    /// actual size comparison instead of guessing from filenames alone (this agent has no
    /// screenshot/Editor-interactive access this session — see docs/PAWN_ART_REWORK_PLAN.md's
    /// "Verification" note). Not part of the shipped pipeline; safe to delete once the picks are locked
    /// in, kept for now in case a future retune needs the same comparison.
    /// </summary>
    public static class CharacterPartBoundsDiagnostic
    {
        [MenuItem("Tools/LogiCard/Diagnostics/Log Assembled Prefab Renderer Vertex Counts")]
        public static void LogAssembledPrefabRendererVertexCounts()
        {
            LogPrefab("Assets/_Project/Art/Characters/Resources/Scout/Scout.prefab");
            LogPrefab("Assets/_Project/Art/Characters/Resources/Juggernaut/Juggernaut.prefab");
        }

        /// <summary>
        /// Mirrors PawnView.TryBuildImported's own combined-renderer-bounds + rescale math exactly (same
        /// Renderer.bounds Encapsulate loop, same TargetVisualHeight=1.0 divide) on a real instantiated
        /// copy of each assembled prefab, so the resulting scale factor is a measured number instead of
        /// an estimate — directly checks the brief's "doesn't produce something absurdly tiny/huge"
        /// requirement.
        /// </summary>
        [MenuItem("Tools/LogiCard/Diagnostics/Log Assembled Prefab Rescale Math")]
        public static void LogAssembledPrefabRescaleMath()
        {
            LogRescale("Assets/_Project/Art/Characters/Resources/Scout/Scout.prefab");
            LogRescale("Assets/_Project/Art/Characters/Resources/Juggernaut/Juggernaut.prefab");
        }

        private static void LogRescale(string path)
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (prefab == null)
            {
                Debug.LogWarning($"CharacterPartBoundsDiagnostic: could not load {path}");
                return;
            }

            GameObject instance = Object.Instantiate(prefab);
            try
            {
                Renderer[] renderers = instance.GetComponentsInChildren<Renderer>();
                if (renderers.Length == 0)
                {
                    Debug.LogWarning($"CharacterPartBoundsDiagnostic: {path} has zero renderers.");
                    return;
                }

                Bounds bounds = renderers[0].bounds;
                for (int i = 1; i < renderers.Length; i++)
                {
                    bounds.Encapsulate(renderers[i].bounds);
                }

                const float targetVisualHeight = 1.0f;
                float scale = bounds.size.y > 0.0001f ? targetVisualHeight / bounds.size.y : float.NaN;
                Debug.Log($"RESCALE\t{path}\tcombinedBounds.size=({bounds.size.x:F4},{bounds.size.y:F4},{bounds.size.z:F4})\tscaleFactor={scale:F4}");
            }
            finally
            {
                Object.DestroyImmediate(instance);
            }
        }

        private static void LogPrefab(string path)
        {
            var go = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (go == null)
            {
                Debug.LogWarning($"CharacterPartBoundsDiagnostic: could not load {path}");
                return;
            }

            Debug.Log($"--- {path} ---");
            foreach (var renderer in go.GetComponentsInChildren<SkinnedMeshRenderer>(true))
            {
                Mesh mesh = renderer.sharedMesh;
                int verts = mesh != null ? mesh.vertexCount : -1;
                Vector3 size = mesh != null ? mesh.bounds.size : Vector3.zero;
                Debug.Log($"RENDERER\t{renderer.name}\tmesh={mesh?.name}\tverts={verts}\tsize=({size.x:F4},{size.y:F4},{size.z:F4})\tenabled={renderer.enabled}\tactive={renderer.gameObject.activeSelf}");
            }
        }
    }
}
