using System.Linq;
using UnityEditor;
using UnityEditor.Build.Reporting;

public static class BuildAndroid
{
    public static void Build()
    {
        EditorUserBuildSettings.buildAppBundle = false;

        var scenes = EditorBuildSettings.scenes
            .Where(scene => scene.enabled)
            .Select(scene => scene.path)
            .ToArray();

        var report = BuildPipeline.BuildPlayer(new BuildPlayerOptions
        {
            scenes = scenes,
            locationPathName = "Builds/BrainLinkDemo.apk",
            target = BuildTarget.Android,
            options = BuildOptions.None
        });

        if (report.summary.result != BuildResult.Succeeded)
        {
            throw new System.InvalidOperationException(report.summary.result.ToString());
        }
    }
}
