using System.IO;
using UnityEditor;
using UnityEditor.XR.Management;
using UnityEditor.XR.Management.Metadata;
using UnityEngine;
using UnityEngine.XR.Management;

/// <summary>Configures ARKit as the iOS XR loader. </summary>
public static class ConfigureARKit
{
    // Identifies the shared XR settings asset.
    private const string SettingsPath = "Assets/XR/XRGeneralSettingsPerBuildTarget.asset";
    // Identifies the ARKit loader type registered by the XR package.
    private const string ARKitLoaderType = "UnityEngine.XR.ARKit.ARKitLoader";

    /// <summary>Creates missing iOS XR settings and assigns the ARKit loader. </summary>
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
