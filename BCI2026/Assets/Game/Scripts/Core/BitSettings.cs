/*
 * BCI Interaction System for Videogames
 * Master's Thesis · University of Alicante
 * © 2026 Gala M. García
 */

using UnityEngine;

namespace Bit.Core
{
    /// <summary>Stores shared application-flow settings for BIT.</summary>
    [CreateAssetMenu(menuName = "BIT Game/Bit Settings", fileName = "BitSettings")]
    public sealed class BitSettings : ScriptableObject
    {
        // Resource path used to load the shared settings asset.
        private const string ResourcePath = "BitSettings";
        // Cached shared settings instance.
        private static BitSettings _instance;

        [Header("Scene Flow")]
        [Tooltip("Scene containing the main menu.")]
        [SerializeField] private string mainMenuSceneName = "MainMenu";
        [Tooltip("Ordered gameplay scenes addressed by their 1-based save index.")]
        [SerializeField] private string[] levelSceneNames = { "Level01", "Level02", "Level03", "Level04", "Level05", "Level06", "Level07" };

        /// <summary>Gets the shared application settings loaded from Resources.</summary>
        public static BitSettings Instance => _instance ??= Resources.Load<BitSettings>(ResourcePath);

        /// <summary>Gets the configured main menu scene name.</summary>
        /// <returns>The main menu scene name.</returns>
        public string GetMainMenuSceneName()
        {
            return mainMenuSceneName;
        }

        /// <summary>Gets the configured first gameplay scene name.</summary>
        /// <returns>The first gameplay scene name.</returns>
        public string GetFirstLevelSceneName()
        {
            return GetLevelSceneName(1);
        }

        /// <summary>Gets the configured gameplay scene count.</summary>
        /// <returns>The number of configured gameplay scenes.</returns>
        public int GetLevelSceneCount()
        {
            return levelSceneNames?.Length ?? 0;
        }

        /// <summary>Gets a gameplay scene by its 1-based level index.</summary>
        /// <param name="level">Level index to resolve.</param>
        /// <returns>The matching scene, or the first level for indexes below one.</returns>
        public string GetLevelSceneName(int level)
        {
            if (levelSceneNames == null || levelSceneNames.Length == 0)
            {
                return string.Empty;
            }

            int index = Mathf.Clamp(level - 1, 0, levelSceneNames.Length - 1);
            return levelSceneNames[index];
        }

        /// <summary>Finds the 1-based level index for a gameplay scene.</summary>
        /// <param name="sceneName">Scene name to find.</param>
        /// <returns>The matching 1-based index, or zero when it is not configured.</returns>
        public int GetLevelIndex(string sceneName)
        {
            if (levelSceneNames == null) { return 0; }
            for (int i = 0; i < levelSceneNames.Length; i++)
            {
                if (levelSceneNames[i] == sceneName)
                {
                    return i + 1;
                }
            }

            return 0;
        }
    }
}
