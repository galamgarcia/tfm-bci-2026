/*
 * BCI Interaction System for Videogames
 * Master's Thesis · University of Alicante
 * © 2026 Gala M. García
 */

using System;
using BciGame.UI;
using Game.Scripts.Gameplay;
using UnityEngine;

namespace BciGame.Core
{
    /// <summary>
    /// Shared tutorial configuration, loaded once from Resources.
    /// </summary>
    [CreateAssetMenu(menuName = "BCI Game/Tutorial Settings", fileName = "TutorialSettings")]
    public sealed class TutorialSettings : ScriptableObject
    {
        // Resource path used by the singleton to load the shared settings asset.
        private const string ResourcePath = "TutorialSettings";
        private static TutorialSettings _instance;

        /// <summary> Associates one mental-state level with its tutorial feedback color. </summary>
        [Serializable]
        private struct StateColor
        {
            /// <summary> Mental-state level represented by this color entry. </summary>
            public MentalStateLevel state;
            /// <summary> Color displayed for the configured mental-state level. </summary>
            public Color color;
        }

        /// <summary> Associates one tutorial text identifier with its localized content. </summary>
        [Serializable]
        private struct TextEntry
        {
            /// <summary> Identifier used by UI components to request this text. </summary>
            public TutorialTextId id;
            /// <summary> Localized string displayed for the configured identifier. </summary>
            [TextArea] public string value;
        }

        [Header("Visual Feedback")]
        [Tooltip("Colors displayed for each mental-state level during tutorial exercises.")]
        [SerializeField] private StateColor[] stateColors;
        [Header("Text Content")]
        [Tooltip("Localized text entries used by the tutorial UI.")]
        [SerializeField] private TextEntry[] texts;

        public static TutorialSettings Instance => _instance ??= Resources.Load<TutorialSettings>(ResourcePath);

        /// <summary>Gets the feedback color configured for a mental-state level.</summary>
        /// <param name="state">Mental-state level whose color is requested.</param>
        /// <returns>The configured color, or white when no entry is available.</returns>
        public Color GetColor(MentalStateLevel state)
        {
            if (stateColors == null) { return Color.white; }
            foreach (StateColor entry in stateColors)
            {
                if (entry.state == state)
                {
                    return entry.color;
                }
            }
            return Color.white;
        }

        /// <summary>Gets the localized text configured for a tutorial identifier.</summary>
        /// <param name="id">Identifier of the requested tutorial text.</param>
        /// <returns>The configured text, or an empty string when no entry is available.</returns>
        public string GetText(TutorialTextId id)
        {
            if (texts == null) { return string.Empty; }
            if (id == TutorialTextId.None) { return string.Empty; }
            foreach (TextEntry entry in texts)
            {
                if (entry.id == id)
                {
                    return entry.value;
                }
            }
            return string.Empty;
        }
    }
}
