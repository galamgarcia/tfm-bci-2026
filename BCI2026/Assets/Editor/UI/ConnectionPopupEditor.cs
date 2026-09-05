/*
 * BCI Interaction System for Videogames
 * Master's Thesis · University of Alicante
 * © 2026 Gala M. García
 */

using UnityEditor;
using UnityEngine;
using Bit.UI;

namespace Bit.Editor
{
    /// <summary>Provides editor controls for previewing every connection popup state.</summary>
    [CustomEditor(typeof(ConnectionPopup))]
    public sealed class ConnectionPopupEditor : UnityEditor.Editor
    {
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

        private static void DrawStateButton(ConnectionPopup popup, string label, UnityEngine.Events.UnityAction action)
        {
            if (GUILayout.Button(label))
            {
                action.Invoke();
            }
        }
    }
}
