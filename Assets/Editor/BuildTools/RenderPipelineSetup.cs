using System.IO;
using UnityEditor;
using UnityEditor.Rendering.Universal;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace BlueWaterRiptide.EditorTools
{
    /// <summary>
    /// Creates a correctly-wired URP Pipeline Asset + Renderer Data pair (equivalent to
    /// Assets > Create > Rendering > URP Asset) and assigns it as the active pipeline in
    /// Graphics/Quality Settings. Built via Unity's own asset-creation APIs rather than
    /// hand-authored YAML so the result matches what the Editor menu would produce.
    /// Idempotent — safe to re-run; only creates assets that don't already exist.
    /// </summary>
    public static class RenderPipelineSetup
    {
        const string RendererDataPath = "Assets/Rendering/BWR_UniversalRendererData.asset";
        const string PipelineAssetPath = "Assets/Rendering/BWR_UniversalRenderPipelineAsset.asset";

        [MenuItem("Tools/Blue Water Riptide/Ensure URP Pipeline Asset")]
        public static void EnsureUrpPipelineAsset()
        {
            Directory.CreateDirectory("Assets/Rendering");

            var rendererData = AssetDatabase.LoadAssetAtPath<UniversalRendererData>(RendererDataPath);
            if (rendererData == null)
            {
                rendererData = ScriptableObject.CreateInstance<UniversalRendererData>();

                try
                {
                    ResourceReloader.ReloadAllNullIn(rendererData, "Packages/com.unity.render-pipelines.universal");
                }
                catch (System.Exception e)
                {
                    Debug.LogWarning($"BWR_SETUP: ResourceReloader step skipped ({e.Message}) — renderer will use C# defaults for any unset resource references.");
                }

                AssetDatabase.CreateAsset(rendererData, RendererDataPath);
            }

            var pipelineAsset = AssetDatabase.LoadAssetAtPath<UniversalRenderPipelineAsset>(PipelineAssetPath);
            if (pipelineAsset == null)
            {
                pipelineAsset = ScriptableObject.CreateInstance<UniversalRenderPipelineAsset>();

                var so = new SerializedObject(pipelineAsset);
                var listProp = so.FindProperty("m_RendererDataList");
                listProp.arraySize = 1;
                listProp.GetArrayElementAtIndex(0).objectReferenceValue = rendererData;

                var defaultIndexProp = so.FindProperty("m_DefaultRendererIndex");
                if (defaultIndexProp != null) defaultIndexProp.intValue = 0;

                so.ApplyModifiedPropertiesWithoutUndo();

                AssetDatabase.CreateAsset(pipelineAsset, PipelineAssetPath);
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            GraphicsSettings.defaultRenderPipeline = pipelineAsset;

            int currentQuality = QualitySettings.GetQualityLevel();
            for (int i = 0; i < QualitySettings.names.Length; i++)
            {
                QualitySettings.SetQualityLevel(i, false);
                QualitySettings.renderPipeline = pipelineAsset;
            }
            QualitySettings.SetQualityLevel(currentQuality, false);

            AssetDatabase.SaveAssets();
            Debug.Log("BWR_SETUP_OK: URP Pipeline Asset ensured and assigned to Graphics + all Quality levels.");
        }
    }
}
