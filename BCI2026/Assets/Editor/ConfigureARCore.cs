using System.IO;
using UnityEditor;
using UnityEditor.XR.Management;
using UnityEditor.XR.Management.Metadata;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.XR.Management;

/// <summary>Configures ARCore as the Android XR loader. </summary>
public static class ConfigureARCore
{
    // Identifies the shared XR settings asset.
    private const string SettingsPath = "Assets/XR/XRGeneralSettingsPerBuildTarget.asset";
    // Identifies the ARCore loader type registered by the XR package.
    private const string ARCoreLoaderType = "UnityEngine.XR.ARCore.ARCoreLoader";

    /// <summary>Configures Android graphics APIs, creates missing XR settings, and assigns the ARCore loader. </summary>
    public static void Configure()
    {
        PlayerSettings.SetGraphicsAPIs(BuildTarget.Android, new[] { GraphicsDeviceType.OpenGLES3 });

        EditorBuildSettings.TryGetConfigObject(XRGeneralSettings.k_SettingsKey, out XRGeneralSettingsPerBuildTarget settings);
        if (settings == null)
        {
            Directory.CreateDirectory("Assets/XR");
            settings = ScriptableObject.CreateInstance<XRGeneralSettingsPerBuildTarget>();
            AssetDatabase.CreateAsset(settings, SettingsPath);
            EditorBuildSettings.AddConfigObject(XRGeneralSettings.k_SettingsKey, settings, true);
        }

        if (!settings.HasManagerSettingsForBuildTarget(BuildTargetGroup.Android))
        {
            settings.CreateDefaultManagerSettingsForBuildTarget(BuildTargetGroup.Android);
        }

        XRManagerSettings manager = settings.ManagerSettingsForBuildTarget(BuildTargetGroup.Android);
        XRPackageMetadataStore.AssignLoader(manager, ARCoreLoaderType, BuildTargetGroup.Android);
        EditorUtility.SetDirty(settings);
        EditorUtility.SetDirty(manager);
        AssetDatabase.SaveAssets();
    }
}
