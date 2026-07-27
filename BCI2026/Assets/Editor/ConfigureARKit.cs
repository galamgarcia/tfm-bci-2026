using System.IO;
using UnityEditor;
using UnityEditor.XR.Management;
using UnityEditor.XR.Management.Metadata;
using UnityEngine;
using UnityEngine.XR.Management;

public static class ConfigureARKit
{
    private const string SettingsPath = "Assets/XR/XRGeneralSettingsPerBuildTarget.asset";
    private const string ARKitLoaderType = "UnityEngine.XR.ARKit.ARKitLoader";

    public static void Configure()
    {
        EditorBuildSettings.TryGetConfigObject(XRGeneralSettings.k_SettingsKey, out XRGeneralSettingsPerBuildTarget settings);
        if (settings == null)
        {
            Directory.CreateDirectory("Assets/XR");
            settings = ScriptableObject.CreateInstance<XRGeneralSettingsPerBuildTarget>();
            AssetDatabase.CreateAsset(settings, SettingsPath);
            EditorBuildSettings.AddConfigObject(XRGeneralSettings.k_SettingsKey, settings, true);
        }

        if (!settings.HasManagerSettingsForBuildTarget(BuildTargetGroup.iOS))
        {
            settings.CreateDefaultManagerSettingsForBuildTarget(BuildTargetGroup.iOS);
        }

        XRManagerSettings manager = settings.ManagerSettingsForBuildTarget(BuildTargetGroup.iOS);
        XRPackageMetadataStore.AssignLoader(manager, ARKitLoaderType, BuildTargetGroup.iOS);
        EditorUtility.SetDirty(settings);
        EditorUtility.SetDirty(manager);
        AssetDatabase.SaveAssets();
    }
}
