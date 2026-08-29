using System.Linq;
using UnityEditor;
using UnityEditor.Build.Reporting;

/// <summary>Builds the enabled scenes as an Android APK.</summary>
public static class BuildAndroid
{
    /// <summary>Configures Android settings and builds the player.</summary>
    public static void Build()
    {
        ConfigureARCore.Configure();
        EditorUserBuildSettings.buildAppBundle = false;

        var scenes = EditorBuildSettings.scenes
            .Where(scene => scene.enabled)
            .Select(scene => scene.path)
            .ToArray();

        var report = BuildPipeline.BuildPlayer(new BuildPlayerOptions
        {
            scenes = scenes,
            locationPathName = "Builds/BCI2026.apk",
            target = BuildTarget.Android,
            options = BuildOptions.None
        });

        if (report.summary.result != BuildResult.Succeeded)
        {
            throw new System.InvalidOperationException(report.summary.result.ToString());
        }
    }
}
