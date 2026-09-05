/*
 * BCI Interaction System for Videogames
 * Master's Thesis · University of Alicante
 * © 2026 Gala M. García
 */

using UnityEditor;
using UnityEngine;
using Bit.Gameplay;

namespace Bit.Editor
{
    /// <summary>Provides editor controls for previewing the screen glitch.</summary>
    [CustomEditor(typeof(ScreenGlitch))]
    public sealed class ScreenGlitchEditor : UnityEditor.Editor
    {
        /// <summary>Draws the glitch Inspector and preview controls.</summary>
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
            EditorGUILayout.Space(8f);
            EditorGUILayout.LabelField("Sandbox Preview", EditorStyles.boldLabel);

            if (!Application.isPlaying)
            {
                EditorGUILayout.HelpBox("Enter Play Mode in BitSandbox to preview the glitch.", MessageType.Info);
                return;
            }

            ScreenGlitch glitch = (ScreenGlitch)target;
            if (GUILayout.Button("Play Glitch"))
            {
                glitch.StartCoroutine(glitch.Play());
            }
        }

    }
}
