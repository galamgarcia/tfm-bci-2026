/*
 * BCI Interaction System for Videogames
 * Master's Thesis · University of Alicante
 * © 2026 Gala M. García
 */

using System.Collections.Generic;
using Bit.Gameplay;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>Builds temporary optimized copies of scenes containing WorldSurface.</summary>
public sealed class WorldSurfaceBuildPreprocessor : IPreprocessBuildWithReport, IPostprocessBuildWithReport
{
    private const string GeneratedFolder = "Assets/Temp/WorldSurfaceBuild";
    // Original build settings restored after the temporary scenes are removed.
    private static EditorBuildSettingsScene[] _originalScenes;
    // Original active scene reopened after the build.
    private static string _originalActiveScenePath;
    // Temporary scene and mesh assets removed after the build.
    private static readonly List<string> _generatedAssets = new List<string>();

    /// <summary>Creates optimized scene copies and temporarily switches build settings to them.</summary>
    /// <param name="report">Build report containing the selected target.</param>
    public void OnPreprocessBuild(BuildReport report)
    {
        _originalScenes = EditorBuildSettings.scenes;
        _originalActiveScenePath = SceneManager.GetActiveScene().path;
        PrepareGeneratedFolder();

        try
        {
            EditorBuildSettingsScene[] optimizedScenes = CreateOptimizedScenes(_originalScenes);
            EditorBuildSettings.scenes = optimizedScenes;
        }
        catch
        {
            RestoreEditorState();
            throw;
        }
    }

    /// <summary>Restores build settings and the editable scene after the build.</summary>
    /// <param name="report">Completed build report.</param>
    public void OnPostprocessBuild(BuildReport report)
    {
        RestoreEditorState();
    }

    /// <summary>Creates optimized copies for every enabled build scene.</summary>
    /// <param name="scenes">Original Build Settings scenes.</param>
    /// <returns>Build settings pointing to temporary optimized scenes.</returns>
    private static EditorBuildSettingsScene[] CreateOptimizedScenes(EditorBuildSettingsScene[] scenes)
    {
        List<EditorBuildSettingsScene> optimized = new List<EditorBuildSettingsScene>(scenes.Length);
        int index = 0;
        foreach (EditorBuildSettingsScene source in scenes)
        {
            string temporaryPath = $"{GeneratedFolder}/Scene_{index}.unity";
            if (!AssetDatabase.CopyAsset(source.path, temporaryPath))
            {
                throw new System.InvalidOperationException($"Could not create temporary scene copy: {source.path}");
            }

            _generatedAssets.Add(temporaryPath);
            Scene temporaryScene = EditorSceneManager.OpenScene(temporaryPath, OpenSceneMode.Single);
            OptimizeScene(temporaryScene);
            EditorSceneManager.SaveScene(temporaryScene);
            GUID guid = AssetDatabase.GUIDFromAssetPath(temporaryPath);
            optimized.Add(new EditorBuildSettingsScene(temporaryPath, source.enabled)
            {
                guid = guid
            });
            index++;
        }

        AssetDatabase.SaveAssets();
        return optimized.ToArray();
    }

    /// <summary>Combines visuals and removes WorldSurface logic from one temporary scene.</summary>
    /// <param name="scene">Temporary scene being optimized.</param>
    private static void OptimizeScene(Scene scene)
    {
        WorldSurface[] surfaces = FindSurfaces(scene);
        if (surfaces.Length == 0) { return; }

        WorldSurface.RefreshAllLoadedSurfaces();
        Dictionary<Material, List<CombineInstance>> groups = CollectVisualMeshes(surfaces);
        CreateCombinedVisuals(scene, groups);
        RemoveWorldSurfaceVisuals(surfaces);
    }

    /// <summary>Finds every WorldSurface belonging to one scene.</summary>
    /// <param name="scene">Scene to inspect.</param>
    /// <returns>WorldSurface components in the scene.</returns>
    private static WorldSurface[] FindSurfaces(Scene scene)
    {
        List<WorldSurface> surfaces = new List<WorldSurface>();
        foreach (GameObject root in scene.GetRootGameObjects())
        {
            surfaces.AddRange(root.GetComponentsInChildren<WorldSurface>(true));
        }
        return surfaces.ToArray();
    }

    /// <summary>Collects body and edge meshes grouped by their shared material.</summary>
    /// <param name="surfaces">WorldSurface components to collect.</param>
    /// <returns>Combine instructions grouped by material.</returns>
    private static Dictionary<Material, List<CombineInstance>> CollectVisualMeshes(WorldSurface[] surfaces)
    {
        Dictionary<Material, List<CombineInstance>> groups = new Dictionary<Material, List<CombineInstance>>();
        foreach (WorldSurface surface in surfaces)
        {
            foreach (MeshRenderer renderer in surface.GetComponentsInChildren<MeshRenderer>(true))
            {
                MeshFilter filter = renderer.GetComponent<MeshFilter>();
                if (filter == null || filter.sharedMesh == null) { continue; }

                Material[] materials = renderer.sharedMaterials;
                for (int i = 0; i < materials.Length && i < filter.sharedMesh.subMeshCount; i++)
                {
                    Material material = materials[i];
                    if (material == null) { continue; }
                    if (!groups.TryGetValue(material, out List<CombineInstance> combines))
                    {
                        combines = new List<CombineInstance>();
                        groups.Add(material, combines);
                    }

                    combines.Add(new CombineInstance
                    {
                        mesh = filter.sharedMesh,
                        subMeshIndex = i,
                        transform = filter.transform.localToWorldMatrix
                    });
                }
            }
        }
        return groups;
    }

    /// <summary>Creates static combined visual objects for each material group.</summary>
    /// <param name="scene">Scene receiving the combined objects.</param>
    /// <param name="groups">Visual meshes grouped by material.</param>
    private static void CreateCombinedVisuals(Scene scene, Dictionary<Material, List<CombineInstance>> groups)
    {
        if (groups.Count == 0) { return; }
        GameObject root = new GameObject("WorldSurfaceCombinedVisuals") { isStatic = true };
        SceneManager.MoveGameObjectToScene(root, scene);

        int index = 0;
        foreach (KeyValuePair<Material, List<CombineInstance>> group in groups)
        {
            Mesh mesh = new Mesh
            {
                name = $"{scene.name}_WorldSurface_{index}",
                indexFormat = UnityEngine.Rendering.IndexFormat.UInt32
            };
            mesh.CombineMeshes(group.Value.ToArray(), true, true);
            string meshPath = $"{GeneratedFolder}/Mesh_{scene.name}_{index}.asset";
            AssetDatabase.CreateAsset(mesh, meshPath);
            _generatedAssets.Add(meshPath);

            GameObject combined = new GameObject($"Combined_{index}") { isStatic = true };
            combined.transform.SetParent(root.transform, false);
            combined.AddComponent<MeshFilter>().sharedMesh = mesh;
            combined.AddComponent<MeshRenderer>().sharedMaterial = group.Key;
            index++;
        }
    }

    /// <summary>Removes original visual children and WorldSurface components from a build scene.</summary>
    /// <param name="surfaces">WorldSurface components to strip from the build scene.</param>
    private static void RemoveWorldSurfaceVisuals(WorldSurface[] surfaces)
    {
        foreach (WorldSurface surface in surfaces)
        {
            Transform[] children = surface.GetComponentsInChildren<Transform>(true);
            Object.DestroyImmediate(surface);
            foreach (Transform child in children)
            {
                if (child != null && child.parent != null)
                {
                    Object.DestroyImmediate(child.gameObject);
                }
            }
        }
    }

    /// <summary>Creates an empty temporary asset folder for the current build.</summary>
    private static void PrepareGeneratedFolder()
    {
        _generatedAssets.Clear();
        if (AssetDatabase.IsValidFolder(GeneratedFolder))
        {
            AssetDatabase.DeleteAsset(GeneratedFolder);
        }
        if (!AssetDatabase.IsValidFolder("Assets/Temp"))
        {
            AssetDatabase.CreateFolder("Assets", "Temp");
        }
        if (!AssetDatabase.IsValidFolder(GeneratedFolder))
        {
            AssetDatabase.CreateFolder("Assets/Temp", "WorldSurfaceBuild");
        }
    }

    /// <summary>Restores the Editor build configuration and removes all temporary assets.</summary>
    private static void RestoreEditorState()
    {
        if (_originalScenes != null)
        {
            EditorBuildSettings.scenes = _originalScenes;
        }

        if (AssetDatabase.IsValidFolder(GeneratedFolder))
        {
            AssetDatabase.DeleteAsset(GeneratedFolder);
        }
        _generatedAssets.Clear();
        AssetDatabase.Refresh();

        if (!string.IsNullOrEmpty(_originalActiveScenePath) &&
            AssetDatabase.LoadAssetAtPath<SceneAsset>(_originalActiveScenePath) != null)
        {
            EditorSceneManager.OpenScene(_originalActiveScenePath, OpenSceneMode.Single);
        }

        _originalScenes = null;
        _originalActiveScenePath = null;
    }

    public int callbackOrder => 0;
}
