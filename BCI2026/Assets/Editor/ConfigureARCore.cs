using System.IO;
using UnityEditor;
using UnityEditor.XR.Management;
using UnityEditor.XR.Management.Metadata;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.XR.Management;

public static class ConfigureARCore
{
    private const string SettingsPath = "Assets/XR/XRGeneralSettingsPerBuildTarget.asset";
    private const string ARCoreLoaderType = "UnityEngine.XR.ARCore.ARCoreLoader";

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
