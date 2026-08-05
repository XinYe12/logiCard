#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;

namespace LogiCard.Art.Editor
{
    /// <summary>
    /// One-shot Day 8 URP foundation: pipeline assets, clay palette, LightingLab scene.
    /// Run via: -executeMethod LogiCard.Art.Editor.UrpFoundationBootstrap.Run
    /// </summary>
    public static class UrpFoundationBootstrap
    {
        private const string ArtRoot = "Assets/_Project/Art";
        private const string UrpFolder = ArtRoot + "/URP";
        private const string MatsFolder = ArtRoot + "/Materials";
        private const string PipelinePath = UrpFolder + "/LogiCardURP.asset";
        private const string RendererPath = UrpFolder + "/LogiCardURP_Renderer.asset";
        private const string LabScenePath = "Assets/_Project/Scenes/LightingLab.unity";

        public static void Run()
        {
            try
            {
                EnsureFolders();
                var pipeline = EnsurePipeline();
                AssignPipeline(pipeline);
                EnsureMaterials();
                EnsureLightingLab();
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
                Debug.Log("[UrpFoundationBootstrap] Done. Pipeline=" + PipelinePath + " Lab=" + LabScenePath);
                EditorApplication.Exit(0);
            }
            catch (System.Exception ex)
            {
                Debug.LogError("[UrpFoundationBootstrap] Failed: " + ex);
                EditorApplication.Exit(1);
            }
        }

        private static void EnsureFolders()
        {
            if (!AssetDatabase.IsValidFolder(ArtRoot))
            {
                AssetDatabase.CreateFolder("Assets/_Project", "Art");
            }

            if (!AssetDatabase.IsValidFolder(UrpFolder))
            {
                AssetDatabase.CreateFolder(ArtRoot, "URP");
            }

            if (!AssetDatabase.IsValidFolder(MatsFolder))
            {
                AssetDatabase.CreateFolder(ArtRoot, "Materials");
            }
        }

        private static UniversalRenderPipelineAsset EnsurePipeline()
        {
            var existing = AssetDatabase.LoadAssetAtPath<UniversalRenderPipelineAsset>(PipelinePath);
            if (existing != null)
            {
                return existing;
            }

            var rendererData = ScriptableObject.CreateInstance<UniversalRendererData>();
            AssetDatabase.CreateAsset(rendererData, RendererPath);

            var pipeline = UniversalRenderPipelineAsset.Create(rendererData);
            // Create() may already write an asset; ensure our known path.
            if (AssetDatabase.GetAssetPath(pipeline) != PipelinePath)
            {
                var tempPath = AssetDatabase.GetAssetPath(pipeline);
                if (!string.IsNullOrEmpty(tempPath))
                {
                    AssetDatabase.MoveAsset(tempPath, PipelinePath);
                }
                else
                {
                    AssetDatabase.CreateAsset(pipeline, PipelinePath);
                }
            }

            EditorUtility.SetDirty(pipeline);
            EditorUtility.SetDirty(rendererData);
            return pipeline;
        }

        private static void AssignPipeline(UniversalRenderPipelineAsset pipeline)
        {
            GraphicsSettings.defaultRenderPipeline = pipeline;
            QualitySettings.renderPipeline = pipeline;

            var qualityCount = QualitySettings.names.Length;
            for (var i = 0; i < qualityCount; i++)
            {
                QualitySettings.SetQualityLevel(i, applyExpensiveChanges: false);
                QualitySettings.renderPipeline = pipeline;
            }

            // Restore preferred quality (Ultra / index 5 was project default).
            if (QualitySettings.names.Length > 5)
            {
                QualitySettings.SetQualityLevel(5, applyExpensiveChanges: false);
            }

            EditorUtility.SetDirty(pipeline);
        }

        private static void EnsureMaterials()
        {
            var lit = Shader.Find("Universal Render Pipeline/Lit");
            var unlit = Shader.Find("Universal Render Pipeline/Unlit");
            if (lit == null)
            {
                throw new System.InvalidOperationException("URP Lit shader not found — is the URP package imported?");
            }

            CreateMatte("Mat_BoardPlywood", lit, new Color(0.72f, 0.55f, 0.38f), smoothness: 0.08f);
            CreateMatte("Mat_ClayWarm", lit, new Color(0.86f, 0.62f, 0.48f), smoothness: 0.12f);
            CreateMatte("Mat_ClayCool", lit, new Color(0.45f, 0.58f, 0.72f), smoothness: 0.12f);
            CreateMatte("Mat_PathYarn", lit, new Color(0.78f, 0.28f, 0.22f), smoothness: 0.05f);
            if (unlit != null)
            {
                CreateUnlit("Mat_VoidBlack", unlit, Color.black);
            }
            else
            {
                CreateMatte("Mat_VoidBlack", lit, Color.black, smoothness: 0f);
            }
        }

        private static void CreateMatte(string name, Shader shader, Color color, float smoothness)
        {
            var path = MatsFolder + "/" + name + ".mat";
            var mat = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (mat == null)
            {
                mat = new Material(shader);
                AssetDatabase.CreateAsset(mat, path);
            }
            else
            {
                mat.shader = shader;
            }

            mat.color = color;
            if (mat.HasProperty("_BaseColor"))
            {
                mat.SetColor("_BaseColor", color);
            }

            if (mat.HasProperty("_Smoothness"))
            {
                mat.SetFloat("_Smoothness", smoothness);
            }

            if (mat.HasProperty("_Metallic"))
            {
                mat.SetFloat("_Metallic", 0f);
            }

            EditorUtility.SetDirty(mat);
        }

        private static void CreateUnlit(string name, Shader shader, Color color)
        {
            var path = MatsFolder + "/" + name + ".mat";
            var mat = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (mat == null)
            {
                mat = new Material(shader);
                AssetDatabase.CreateAsset(mat, path);
            }
            else
            {
                mat.shader = shader;
            }

            mat.color = color;
            if (mat.HasProperty("_BaseColor"))
            {
                mat.SetColor("_BaseColor", color);
            }

            EditorUtility.SetDirty(mat);
        }

        private static void EnsureLightingLab()
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            var boardMat = AssetDatabase.LoadAssetAtPath<Material>(MatsFolder + "/Mat_BoardPlywood.mat");
            var warmMat = AssetDatabase.LoadAssetAtPath<Material>(MatsFolder + "/Mat_ClayWarm.mat");
            var coolMat = AssetDatabase.LoadAssetAtPath<Material>(MatsFolder + "/Mat_ClayCool.mat");
            var yarnMat = AssetDatabase.LoadAssetAtPath<Material>(MatsFolder + "/Mat_PathYarn.mat");
            var voidMat = AssetDatabase.LoadAssetAtPath<Material>(MatsFolder + "/Mat_VoidBlack.mat");

            // Dark void surround (large plane under/around board).
            var voidGo = GameObject.CreatePrimitive(PrimitiveType.Plane);
            voidGo.name = "Void";
            voidGo.transform.position = new Vector3(2f, -0.05f, 2f);
            voidGo.transform.localScale = new Vector3(4f, 1f, 4f);
            ApplyMat(voidGo, voidMat);

            // Diorama base ≈ [0,4]×[0,4] footprint (box top at y=0).
            var board = GameObject.CreatePrimitive(PrimitiveType.Cube);
            board.name = "DioramaBase";
            board.transform.position = new Vector3(2f, -0.05f, 2f);
            board.transform.localScale = new Vector3(4.2f, 0.1f, 4.2f);
            ApplyMat(board, boardMat);

            var pawnWarm = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            pawnWarm.name = "ClayWarm";
            pawnWarm.transform.position = new Vector3(1.2f, 0.35f, 1.5f);
            pawnWarm.transform.localScale = new Vector3(0.45f, 0.35f, 0.45f);
            ApplyMat(pawnWarm, warmMat);

            var pawnCool = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            pawnCool.name = "ClayCool";
            pawnCool.transform.position = new Vector3(2.8f, 0.35f, 2.6f);
            pawnCool.transform.localScale = new Vector3(0.55f, 0.4f, 0.55f);
            ApplyMat(pawnCool, coolMat);

            var yarn = GameObject.CreatePrimitive(PrimitiveType.Cube);
            yarn.name = "PathYarnSample";
            yarn.transform.position = new Vector3(2f, 0.02f, 2f);
            yarn.transform.localScale = new Vector3(1.6f, 0.03f, 0.08f);
            yarn.transform.rotation = Quaternion.Euler(0f, 35f, 0f);
            ApplyMat(yarn, yarnMat);

            // Warm desk-lamp key (spot) + soft fill.
            var key = new GameObject("DeskLampKey");
            var keyLight = key.AddComponent<Light>();
            keyLight.type = LightType.Spot;
            keyLight.color = new Color(1f, 0.78f, 0.48f);
            keyLight.intensity = 4.2f;
            keyLight.range = 14f;
            keyLight.spotAngle = 55f;
            keyLight.shadows = LightShadows.Soft;
            key.transform.position = new Vector3(2.4f, 5.2f, -0.6f);
            key.transform.rotation = Quaternion.Euler(55f, -15f, 0f);

            var fill = new GameObject("SoftFill");
            var fillLight = fill.AddComponent<Light>();
            fillLight.type = LightType.Directional;
            fillLight.color = new Color(0.55f, 0.62f, 0.78f);
            fillLight.intensity = 0.35f;
            fillLight.shadows = LightShadows.None;
            fill.transform.rotation = Quaternion.Euler(35f, 140f, 0f);

            // Portrait-ish tall framing, high angle over board center.
            var camGo = new GameObject("LabCamera");
            var cam = camGo.AddComponent<Camera>();
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(0.02f, 0.02f, 0.03f);
            cam.fieldOfView = 40f;
            cam.nearClipPlane = 0.1f;
            cam.farClipPlane = 40f;
            camGo.tag = "MainCamera";
            camGo.transform.position = new Vector3(2f, 7.2f, -3.2f);
            camGo.transform.rotation = Quaternion.Euler(52f, 0f, 0f);
            camGo.AddComponent<AudioListener>();

            RenderSettings.ambientMode = AmbientMode.Flat;
            RenderSettings.ambientLight = new Color(0.12f, 0.1f, 0.09f);
            RenderSettings.defaultReflectionMode = DefaultReflectionMode.Custom;

            EditorSceneManager.SaveScene(scene, LabScenePath);
        }

        private static void ApplyMat(GameObject go, Material mat)
        {
            if (mat == null)
            {
                return;
            }

            var renderer = go.GetComponent<MeshRenderer>();
            if (renderer != null)
            {
                renderer.sharedMaterial = mat;
            }
        }
    }
}
#endif
