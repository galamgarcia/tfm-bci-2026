#if UNITY_EDITOR_OSX
using System.IO;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEditor.iOS.Xcode;

public static class ConfigureIOSBuild
{
    private const string BluetoothUsageDescription = "BCI2026 uses Bluetooth to connect to your BrainLink headset.";

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

    private static void ConfigureTarget(PBXProject project, string targetGuid)
    {
        project.AddFrameworkToProject(targetGuid, "CoreBluetooth.framework", false);
        project.AddBuildProperty(targetGuid, "OTHER_LDFLAGS", "-ObjC");
        project.SetBuildProperty(targetGuid, "ENABLE_BITCODE", "NO");
    }
}
#endif
