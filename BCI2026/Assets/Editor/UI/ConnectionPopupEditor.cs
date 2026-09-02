/*
 * BCI Interaction System for Videogames
 * Master's Thesis · University of Alicante
 * © 2026 Gala M. García
 */

using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using Bit.UI;

namespace Bit.Editor
{
    /// <summary>Provides editor controls for previewing every connection popup state.</summary>
    [CustomEditor(typeof(ConnectionPopup))]
    public sealed class ConnectionPopupEditor : UnityEditor.Editor
    {
        private const string SandboxPath = "Assets/Game/Scenes/Test/BitSandbox.unity";
        private const string PopupPath = "Assets/Game/Prefabs/UI/ConnectionPopup.prefab";
        private const string SandboxObjectName = "ConnectionPopupSandbox";

        /// <summary>Draws the popup Inspector and state preview controls.</summary>
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
            EditorGUILayout.Space(8f);
            EditorGUILayout.LabelField("Sandbox Preview", EditorStyles.boldLabel);

            if (!Application.isPlaying)
            {
                EditorGUILayout.HelpBox("Enter Play Mode in BitSandbox to preview these states.", MessageType.Info);
                return;
            }

            ConnectionPopup popup = (ConnectionPopup)target;
            using (new EditorGUILayout.HorizontalScope())
            {
                DrawStateButton(popup, "Searching", popup.ShowSearching);
                DrawStateButton(popup, "Connecting", popup.ShowConnecting);
                DrawStateButton(popup, "Connected", popup.ShowConnected);
            }

            if (GUILayout.Button("Hide"))
            {
                popup.Hide();
            }

            ConnectionPopupController controller = popup.GetComponent<ConnectionPopupController>();
            if (controller != null && GUILayout.Button("Simulate Automatic Flow"))
            {
                controller.SimulateConnectionFlowForEditor();
            }
        }

        /// <summary>Adds the reusable popup to the sandbox and disables hardware-driven state changes.</summary>
        [MenuItem("BIT/Sandbox/Add Connection Popup")]
        public static void AddConnectionPopupToSandbox()
        {
            Scene scene = EditorSceneManager.OpenScene(SandboxPath, OpenSceneMode.Single);
            GameObject popup = GameObject.Find(SandboxObjectName);
            if (popup == null)
            {
                GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(PopupPath);
                popup = (GameObject)PrefabUtility.InstantiatePrefab(prefab, scene);
                popup.name = SandboxObjectName;
            }

            ConnectionPopupController controller = popup.GetComponent<ConnectionPopupController>();
            if (controller != null)
            {
                controller.enabled = false;
            }

            Selection.activeGameObject = popup;
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
        }

        private static void DrawStateButton(ConnectionPopup popup, string label, UnityEngine.Events.UnityAction action)
        {
            if (GUILayout.Button(label))
            {
                action.Invoke();
            }
        }
    }
}
