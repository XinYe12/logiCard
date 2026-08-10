#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEditor.Rendering;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace LogiCard.Art.Editor
{
    /// <summary>
    /// Builds the missing URP Volume Profiles + SSAO renderer feature and tunes shadow/MSAA
    /// quality on <c>LogiCardURP.asset</c> (C53/C54 post-processing audit). Run via:
    /// <c>-executeMethod LogiCard.Art.Editor.UrpPostProcessingBootstrap.Run</c>
    /// or menu <c>Tools/LogiCard/URP Post-Processing Bootstrap</c>.
    /// </summary>
    public static class UrpPostProcessingBootstrap
    {
        private const string UrpFolder = "Assets/_Project/Art/URP";
        private const string ResourcesFolder = UrpFolder + "/Resources";
        private const string PipelinePath = UrpFolder + "/LogiCardURP.asset";
        private const string RendererPath = UrpFolder + "/LogiCardURP_Renderer.asset";
        private const string LiveProfilePath = ResourcesFolder + "/LogiCardVolumeProfile.asset";
        private const string PhotoProfilePath = ResourcesFolder + "/LogiCardVolumeProfile_Photo.asset";

        [MenuItem("Tools/LogiCard/URP Post-Processing Bootstrap")]
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
                Debug.LogError("[UrpPostProcessingBootstrap] Failed: " + ex);
                EditorApplication.Exit(1);
            }
        }

        private static void Apply()
        {
            EnsureFolder(UrpFolder);
            EnsureFolder(ResourcesFolder);

            var pipeline = AssetDatabase.LoadAssetAtPath<UniversalRenderPipelineAsset>(PipelinePath);
            var renderer = AssetDatabase.LoadAssetAtPath<UniversalRendererData>(RendererPath);
            if (pipeline == null || renderer == null)
            {
                throw new FileNotFoundException(
                    "Missing LogiCardURP pipeline/renderer assets — run UrpFoundationBootstrap first.");
            }

            VolumeProfile live = EnsureLiveProfile();
            VolumeProfile photo = EnsurePhotoProfile();
            ConfigurePipeline(pipeline, live);
            EnsureSsao(renderer);

            EditorUtility.SetDirty(pipeline);
            EditorUtility.SetDirty(renderer);
            EditorUtility.SetDirty(live);
            EditorUtility.SetDirty(photo);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log(
                "[UrpPostProcessingBootstrap] Done. Live=" + LiveProfilePath +
                " Photo=" + PhotoProfilePath +
                " SSAO on renderer; MSAA 4x; additional soft shadows + 2 cascades.");
        }

        private static void EnsureFolder(string path)
        {
            if (AssetDatabase.IsValidFolder(path))
            {
                return;
            }

            string parent = Path.GetDirectoryName(path)?.Replace('\\', '/');
            string name = Path.GetFileName(path);
            if (!string.IsNullOrEmpty(parent) && !AssetDatabase.IsValidFolder(parent))
            {
                EnsureFolder(parent);
            }

            AssetDatabase.CreateFolder(parent, name);
        }

        private static VolumeProfile EnsureLiveProfile()
        {
            VolumeProfile profile = LoadOrCreateProfile(LiveProfilePath);
            ClearComponents(profile);

            // Readability-first match grade (C54). Complements flat ambient (0.09,0.11,0.15).
            var tone = VolumeProfileFactory.CreateVolumeComponent<Tonemapping>(profile, overrides: true, saveAsset: false);
            tone.mode.overrideState = true;
            tone.mode.value = TonemappingMode.ACES;

            var color = VolumeProfileFactory.CreateVolumeComponent<ColorAdjustments>(profile, overrides: true, saveAsset: false);
            color.postExposure.overrideState = true;
            color.postExposure.value = 0.08f;
            color.contrast.overrideState = true;
            color.contrast.value = 12f;
            color.colorFilter.overrideState = true;
            color.colorFilter.value = new Color(0.95f, 0.92f, 0.88f);
            color.saturation.overrideState = true;
            color.saturation.value = 18f;

            var bloom = VolumeProfileFactory.CreateVolumeComponent<Bloom>(profile, overrides: true, saveAsset: false);
            bloom.threshold.overrideState = true;
            bloom.threshold.value = 0.95f;
            bloom.intensity.overrideState = true;
            bloom.intensity.value = 0.30f;
            bloom.scatter.overrideState = true;
            bloom.scatter.value = 0.55f;

            var vignette = VolumeProfileFactory.CreateVolumeComponent<Vignette>(profile, overrides: true, saveAsset: false);
            vignette.color.overrideState = true;
            vignette.color.value = new Color(0.01f, 0.015f, 0.03f);
            vignette.intensity.overrideState = true;
            vignette.intensity.value = 0.28f;
            vignette.smoothness.overrideState = true;
            vignette.smoothness.value = 0.70f;

            return profile;
        }

        private static VolumeProfile EnsurePhotoProfile()
        {
            VolumeProfile profile = LoadOrCreateProfile(PhotoProfilePath);
            ClearComponents(profile);

            // Hero-shot / reference mood (C54 photo mode): stronger grade + miniature DoF.
            var tone = VolumeProfileFactory.CreateVolumeComponent<Tonemapping>(profile, overrides: true, saveAsset: false);
            tone.mode.overrideState = true;
            tone.mode.value = TonemappingMode.ACES;

            var color = VolumeProfileFactory.CreateVolumeComponent<ColorAdjustments>(profile, overrides: true, saveAsset: false);
            color.postExposure.overrideState = true;
            color.postExposure.value = -0.18f;
            color.contrast.overrideState = true;
            color.contrast.value = 22f;
            color.colorFilter.overrideState = true;
            color.colorFilter.value = new Color(0.78f, 0.86f, 1f);
            color.saturation.overrideState = true;
            color.saturation.value = -2f;

            var bloom = VolumeProfileFactory.CreateVolumeComponent<Bloom>(profile, overrides: true, saveAsset: false);
            bloom.threshold.overrideState = true;
            bloom.threshold.value = 0.85f;
            bloom.intensity.overrideState = true;
            bloom.intensity.value = 0.55f;
            bloom.scatter.overrideState = true;
            bloom.scatter.value = 0.65f;

            var vignette = VolumeProfileFactory.CreateVolumeComponent<Vignette>(profile, overrides: true, saveAsset: false);
            vignette.color.overrideState = true;
            vignette.color.value = new Color(0.01f, 0.012f, 0.03f);
            vignette.intensity.overrideState = true;
            vignette.intensity.value = 0.48f;
            vignette.smoothness.overrideState = true;
            vignette.smoothness.value = 0.75f;

            var dof = VolumeProfileFactory.CreateVolumeComponent<DepthOfField>(profile, overrides: true, saveAsset: false);
            dof.mode.overrideState = true;
            dof.mode.value = DepthOfFieldMode.Bokeh;
            dof.focusDistance.overrideState = true;
            dof.focusDistance.value = 14f;
            dof.aperture.overrideState = true;
            dof.aperture.value = 2.8f;
            dof.focalLength.overrideState = true;
            dof.focalLength.value = 135f;
            dof.bladeCount.overrideState = true;
            dof.bladeCount.value = 6;

            return profile;
        }

        private static VolumeProfile LoadOrCreateProfile(string path)
        {
            var existing = AssetDatabase.LoadAssetAtPath<VolumeProfile>(path);
            if (existing != null)
            {
                return existing;
            }

            return VolumeProfileFactory.CreateVolumeProfileAtPath(path);
        }

        private static void ClearComponents(VolumeProfile profile)
        {
            // Rebuild from scratch so re-running the bootstrap is idempotent.
            for (int i = profile.components.Count - 1; i >= 0; i--)
            {
                VolumeComponent component = profile.components[i];
                profile.components.RemoveAt(i);
                if (component == null)
                {
                    continue;
                }

                if (EditorUtility.IsPersistent(component))
                {
                    Object.DestroyImmediate(component, allowDestroyingAssets: true);
                }
                else
                {
                    Object.DestroyImmediate(component);
                }
            }
        }

        private static void ConfigurePipeline(UniversalRenderPipelineAsset pipeline, VolumeProfile live)
        {
            var so = new SerializedObject(pipeline);
            so.FindProperty("m_MSAA").intValue = (int)MsaaQuality._4x;
            so.FindProperty("m_AdditionalLightShadowsSupported").boolValue = true;
            so.FindProperty("m_SoftShadowsSupported").boolValue = true;
            so.FindProperty("m_ShadowCascadeCount").intValue = 2;
            so.FindProperty("m_Cascade2Split").floatValue = 0.25f;
            so.FindProperty("m_RequireDepthTexture").boolValue = true;
            so.FindProperty("m_VolumeProfile").objectReferenceValue = live;
            // Four Hall/Vault/Yard practicals are additional lights — keep headroom.
            so.FindProperty("m_AdditionalLightsPerObjectLimit").intValue = 6;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void EnsureSsao(UniversalRendererData renderer)
        {
            foreach (ScriptableRendererFeature feature in renderer.rendererFeatures)
            {
                if (feature is ScreenSpaceAmbientOcclusion)
                {
                    TuneSsao(feature);
                    return;
                }
            }

            var ssao = ScriptableObject.CreateInstance<ScreenSpaceAmbientOcclusion>();
            ssao.name = "ScreenSpaceAmbientOcclusion";
            ssao.hideFlags |= HideFlags.HideInHierarchy;
            AssetDatabase.AddObjectToAsset(ssao, renderer);
            TuneSsao(ssao);

            AssetDatabase.TryGetGUIDAndLocalFileIdentifier(ssao, out _, out long localId);

            var so = new SerializedObject(renderer);
            SerializedProperty features = so.FindProperty("m_RendererFeatures");
            SerializedProperty map = so.FindProperty("m_RendererFeatureMap");
            features.arraySize++;
            features.GetArrayElementAtIndex(features.arraySize - 1).objectReferenceValue = ssao;
            map.arraySize++;
            map.GetArrayElementAtIndex(map.arraySize - 1).longValue = localId;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void TuneSsao(ScriptableRendererFeature feature)
        {
            var so = new SerializedObject(feature);
            SerializedProperty settings = so.FindProperty("m_Settings");
            if (settings == null)
            {
                return;
            }

            // Moderate AO for the small room-zoned board — default Intensity 3 washes out wet-dusk.
            settings.FindPropertyRelative("Intensity").floatValue = 1.15f;
            settings.FindPropertyRelative("Radius").floatValue = 0.045f;
            settings.FindPropertyRelative("DirectLightingStrength").floatValue = 0.35f;
            settings.FindPropertyRelative("Downsample").boolValue = false;
            settings.FindPropertyRelative("AfterOpaque").boolValue = false;
            // DepthSource.DepthNormals=1, AOSampleOption.Medium=1, BlurQualityOptions.High=0
            // (ScreenSpaceAmbientOcclusionSettings is internal to the URP assembly).
            settings.FindPropertyRelative("Source").enumValueIndex = 1;
            settings.FindPropertyRelative("Samples").enumValueIndex = 1;
            settings.FindPropertyRelative("BlurQuality").enumValueIndex = 0;
            so.ApplyModifiedPropertiesWithoutUndo();
        }
    }
}
#endif
