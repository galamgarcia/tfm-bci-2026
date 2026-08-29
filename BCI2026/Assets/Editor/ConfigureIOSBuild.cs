#if UNITY_EDITOR_OSX
using System.IO;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEditor.iOS.Xcode;

/// <summary>Configures the generated Xcode project with required Bluetooth settings. </summary>
public static class ConfigureIOSBuild
{
    // Describes the application's Bluetooth usage to iOS.
    private const string BluetoothUsageDescription = "BIT uses Bluetooth to connect to your BrainLink headset.";

    /// <summary>Configures an iOS build after Unity generates the Xcode project. </summary>
    /// <param name="target">The platform build target.</param>
    /// <param name="pathToBuiltProject">The path to the generated Xcode project.</param>
    [PostProcessBuild(1000)]
    public static void Configure(BuildTarget target, string pathToBuiltProject)
    {
        if (target != BuildTarget.iOS)
        {
            return;
        }

        string projectPath = PBXProject.GetPBXProjectPath(pathToBuiltProject);
        var project = new PBXProject();
        project.ReadFromFile(projectPath);

        string mainTarget = project.GetUnityMainTargetGuid();
        string frameworkTarget = project.GetUnityFrameworkTargetGuid();
        ConfigureTarget(project, mainTarget);
        ConfigureTarget(project, frameworkTarget);
        project.WriteToFile(projectPath);

        string plistPath = Path.Combine(pathToBuiltProject, "Info.plist");
        var plist = new PlistDocument();
        plist.ReadFromFile(plistPath);
        plist.root.SetString("NSBluetoothAlwaysUsageDescription", BluetoothUsageDescription);
        plist.root.SetString("NSBluetoothPeripheralUsageDescription", BluetoothUsageDescription);
        plist.WriteToFile(plistPath);
    }

    /// <summary>Applies Bluetooth framework and linker settings to an Xcode target. </summary>
    /// <param name="project">The Xcode project to configure.</param>
    /// <param name="targetGuid">The identifier of the Xcode target.</param>
    private static void ConfigureTarget(PBXProject project, string targetGuid)
    {
        project.AddFrameworkToProject(targetGuid, "CoreBluetooth.framework", false);
        project.AddBuildProperty(targetGuid, "OTHER_LDFLAGS", "-ObjC");
        project.SetBuildProperty(targetGuid, "ENABLE_BITCODE", "NO");
    }
}
#endif
