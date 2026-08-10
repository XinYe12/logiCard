#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace LogiCard.Art.Editor
{
    /// <summary>
    /// Enables reflection-probe blending/box-projection on <c>LogiCardURP.asset</c> for the
    /// wet-surface-reflections follow-up (SSR is infeasible on URP 17.5.0 in this project — see
    /// <c>UrpPostProcessingBootstrap</c>'s audit — so board wet floors get their real reflection
    /// source from Reflection Probes instead). Same re-runnable pattern as
    /// <see cref="UrpPostProcessingBootstrap"/>: <c>-executeMethod
    /// LogiCard.Art.Editor.ReflectionProbeBootstrap.Run</c> or menu
    /// <c>Tools/LogiCard/Reflection Probe Bootstrap</c>.
    /// </summary>
    public static class ReflectionProbeBootstrap
    {
        private const string PipelinePath = "Assets/_Project/Art/URP/LogiCardURP.asset";

        [MenuItem("Tools/LogiCard/Reflection Probe Bootstrap")]
        public static void RunFromMenu()
        {
            Apply();
        }

        public static void Run()
        {
            try
            {
                Apply();
                EditorApplication.Exit(0);
            }
            catch (System.Exception ex)
            {
                Debug.LogError("[ReflectionProbeBootstrap] Failed: " + ex);
                EditorApplication.Exit(1);
            }
        }

        private static void Apply()
        {
            var pipeline = AssetDatabase.LoadAssetAtPath<UniversalRenderPipelineAsset>(PipelinePath);
            if (pipeline == null)
            {
                throw new System.IO.FileNotFoundException(
                    "Missing LogiCardURP pipeline asset at " + PipelinePath + " — run UrpFoundationBootstrap first.");
            }

            var so = new SerializedObject(pipeline);
            so.FindProperty("m_ReflectionProbeBlending").boolValue = true;
            so.FindProperty("m_ReflectionProbeBoxProjection").boolValue = true;
            // m_ReflectionProbeAtlas (Forward+ atlas support) is already true by default in this URP
            // version — set explicitly anyway so this bootstrap is a complete, idempotent record of
            // every reflection-probe-relevant flag on the asset, not just the two that changed.
            so.FindProperty("m_ReflectionProbeAtlas").boolValue = true;
            so.ApplyModifiedPropertiesWithoutUndo();

            EditorUtility.SetDirty(pipeline);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log(
                "[ReflectionProbeBootstrap] Done. ReflectionProbeBlending=on, ReflectionProbeBoxProjection=on, " +
                "ReflectionProbeAtlas=on on " + PipelinePath + ".");
        }
    }
}
#endif
