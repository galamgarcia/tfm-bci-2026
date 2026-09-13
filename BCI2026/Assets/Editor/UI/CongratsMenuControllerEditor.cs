/*
 * BCI Interaction System for Videogames
 * Master's Thesis · University of Alicante
 * © 2026 Gala M. García
 */

using UnityEditor;
using UnityEngine;

namespace Bit.EditorInput
{
    /// <summary>Adds Play and Hide controls for the congratulations modal preview.</summary>
    [CustomEditor(typeof(Bit.UI.CongratsMenuController))]
    public sealed class CongratsMenuControllerEditor : UnityEditor.Editor
    {
        /// <summary>Draws the default inspector and preview controls.</summary>
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Preview", EditorStyles.boldLabel);

            if (!Application.isPlaying)
            {
                EditorGUILayout.HelpBox("Enter Play Mode to preview the modal and confetti.", MessageType.Info);
                return;
            }

            Bit.UI.CongratsMenuController menu = (Bit.UI.CongratsMenuController)target;
            if (GUILayout.Button("Play")) { menu.Open(); }
            if (GUILayout.Button("Hide")) { menu.Hide(); }
        }
    }
}
