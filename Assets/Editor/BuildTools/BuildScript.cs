using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace BlueWaterRiptide.EditorTools
{
    /// <summary>
    /// Headless build entry point, invokable via `-executeMethod` — the CLI/CI build path
    /// the project didn't have yet. Ensures the URP pipeline is wired before building so a
    /// fresh checkout can produce a working APK without any manual Editor steps.
    /// </summary>
    public static class BuildScript
    {
        [MenuItem("Tools/Blue Water Riptide/Build Android (Development)")]
        public static void BuildAndroidDevelopment()
        {
            RenderPipelineSetup.EnsureUrpPipelineAsset();

            var buildPlayerOptions = new BuildPlayerOptions
            {
                scenes = new[] { "Assets/Scenes/Boot.unity" },
                locationPathName = "Builds/Android/blue-water-riptide.apk",
                target = BuildTarget.Android,
                options = BuildOptions.Development
            };

            BuildReport report = BuildPipeline.BuildPlayer(buildPlayerOptions);

            if (report.summary.result != BuildResult.Succeeded)
            {
                Debug.LogError($"BWR_BUILD_FAILED: result={report.summary.result} errors={report.summary.totalErrors}");
            }
            else
            {
                Debug.Log($"BWR_BUILD_OK: {buildPlayerOptions.locationPathName}");
            }
        }
    }
}
