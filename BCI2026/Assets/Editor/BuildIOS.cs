using System.Linq;
using UnityEditor;
using UnityEditor.Build.Reporting;

public static class BuildIOS
{
    public static void Build()
    {
        ConfigureARKit.Configure();

        var scenes = EditorBuildSettings.scenes
            .Where(scene => scene.enabled)
            .Select(scene => scene.path)
            .ToArray();

        var report = BuildPipeline.BuildPlayer(new BuildPlayerOptions
        {
            scenes = scenes,
            locationPathName = "Builds/BCI2026-iOS",
            target = BuildTarget.iOS,
            options = BuildOptions.None
        });

        if (report.summary.result != BuildResult.Succeeded)
        {
            throw new System.InvalidOperationException(report.summary.result.ToString());
        }
    }
}
