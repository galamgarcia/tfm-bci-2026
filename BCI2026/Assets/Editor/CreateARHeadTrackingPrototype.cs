using System.Linq;
using Unity.XR.CoreUtils;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.XR.ARFoundation;

public static class CreateARHeadTrackingPrototype
{
    private const string ScenePath = "Assets/Scenes/ARHeadTrackingPrototype.unity";

    public static void Create()
    {
        ConfigureARCore.Configure();
        Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

        var session = new GameObject("AR Session");
        session.AddComponent<ARSession>();
        session.AddComponent<ARInputManager>();

        var originObject = new GameObject("XR Origin");
        var origin = originObject.AddComponent<XROrigin>();
        var faceManager = originObject.AddComponent<ARFaceManager>();
        faceManager.requestedMaximumFaceCount = 1;
        originObject.AddComponent<ARHeadTrackingPrototype>();

        var cameraObject = new GameObject("AR Camera");
        cameraObject.tag = "MainCamera";
        cameraObject.transform.SetParent(originObject.transform, false);
        var camera = cameraObject.AddComponent<Camera>();
        camera.clearFlags = CameraClearFlags.SolidColor;
        camera.backgroundColor = new Color(0.04f, 0.04f, 0.06f);
        var cameraManager = cameraObject.AddComponent<ARCameraManager>();
        cameraManager.requestedFacingDirection = CameraFacingDirection.User;
        origin.Camera = camera;

        EditorSceneManager.SaveScene(scene, ScenePath);
        EditorBuildSettings.scenes = new[]
        {
            new EditorBuildSettingsScene(ScenePath, true),
        }.Concat(EditorBuildSettings.scenes.Where(sceneEntry => sceneEntry.path != ScenePath)).ToArray();
        AssetDatabase.SaveAssets();
    }
}
