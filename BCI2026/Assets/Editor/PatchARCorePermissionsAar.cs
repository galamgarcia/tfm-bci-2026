using System.IO;
using System.IO.Compression;
using System.Text;
using UnityEditor.Android;

/// <summary>Patches the ARCore permissions AAR package name after Gradle project generation. </summary>
public sealed class PatchARCorePermissionsAar : IPostGenerateGradleAndroidProject
{
    public int callbackOrder => 100;

    /// <summary>Rewrites the generated permissions AAR when its manifest uses the ARCore package name. </summary>
    /// <param name="path">The path to the generated Gradle project.</param>
    public void OnPostGenerateGradleAndroidProject(string path)
    {
        string aarPath = Path.Combine(path, "libs", "unityandroidpermissions.aar");
        if (!File.Exists(aarPath))
        {
            return;
        }

        // Matches the package declaration that conflicts with Unity permissions.
        const string oldPackage = "package=\"com.google.ar.core\"";
        string temporaryPath = aarPath + ".tmp";
        bool wasPatched = false;
        using (var source = ZipFile.OpenRead(aarPath))
        using (var destination = ZipFile.Open(temporaryPath, ZipArchiveMode.Create))
        {
            foreach (ZipArchiveEntry entry in source.Entries)
            {
                ZipArchiveEntry replacement = destination.CreateEntry(entry.FullName, CompressionLevel.Optimal);
                using Stream input = entry.Open();
                using Stream output = replacement.Open();
                if (entry.FullName == "AndroidManifest.xml")
                {
                    using var reader = new StreamReader(input, Encoding.UTF8, true);
                    using var writer = new StreamWriter(output, new UTF8Encoding(false));
                    string manifest = reader.ReadToEnd();
                    wasPatched = manifest.Contains(oldPackage);
                    writer.Write(manifest.Replace(oldPackage, "package=\"com.unity3d.player.permissions\""));
                }
                else
                {
                    input.CopyTo(output);
                }
            }
        }

        if (wasPatched)
        {
            File.Delete(aarPath);
            File.Move(temporaryPath, aarPath);
        }
        else
        {
            File.Delete(temporaryPath);
        }
    }
}
