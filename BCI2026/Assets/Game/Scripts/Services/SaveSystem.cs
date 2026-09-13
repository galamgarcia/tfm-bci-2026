/*
 * BCI Interaction System for Videogames
 * Master's Thesis · University of Alicante
 * © 2026 Gala M. García
 */

using UnityEngine;
using UnityEngine.SceneManagement;
using Bit.Core;

namespace Bit.Services
{
    /// <summary>Stores the last unlocked level for the local mobile save game.</summary>
    [DefaultExecutionOrder(-80)]
    public sealed class SaveSystem : MonoBehaviour
    {
        public static SaveSystem Instance { get; private set; }
        // PlayerPrefs key used for the single local progression value.
        private const string LastUnlockedLevelKey = "Bit.LastUnlockedLevel";

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        private void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }
        }

        /// <summary>Determines whether a valid saved level exists.</summary>
        /// <returns>True when the continue action can be used.</returns>
        public bool HasSavedGame()
        {
            int level = GetLastUnlockedLevel();
            return level >= 1 && level <= GetLevelCount();
        }

        /// <summary>Gets the stored 1-based last unlocked level.</summary>
        /// <returns>The saved level index, or zero when no save exists.</returns>
        public int GetLastUnlockedLevel()
        {
            return PlayerPrefs.GetInt(LastUnlockedLevelKey, 0);
        }

        /// <summary>Starts a new game from the first level and replaces existing progress.</summary>
        public void StartNewGame()
        {
            SaveUnlockedLevel(1, true);
        }

        /// <summary>Gets the scene associated with the saved level.</summary>
        /// <returns>The saved level scene, or the first level when no valid save exists.</returns>
        public string GetContinueSceneName()
        {
            return GetSceneName(GetLastUnlockedLevel());
        }

        /// <summary>Unlocks the level after the currently active level and returns its scene.</summary>
        /// <returns>The next level scene, or the main menu after the final level.</returns>
        public string CompleteCurrentLevel()
        {
            BitSettings settings = BitSettings.Instance;
            if (settings == null)
            {
                return SceneManager.GetActiveScene().name;
            }

            int currentLevel = settings.GetLevelIndex(SceneManager.GetActiveScene().name);
            int nextLevel = currentLevel + 1;
            if (currentLevel < 1 || nextLevel > settings.GetLevelSceneCount())
            {
                SaveUnlockedLevel(settings.GetLevelSceneCount(), false);
                return settings.GetMainMenuSceneName();
            }

            SaveUnlockedLevel(nextLevel, false);
            return settings.GetLevelSceneName(nextLevel);
        }

        /// <summary>Clears the local saved progress.</summary>
        public void DeleteSave()
        {
            PlayerPrefs.DeleteKey(LastUnlockedLevelKey);
            PlayerPrefs.Save();
        }

        /// <summary>Gets the number of configured gameplay levels.</summary>
        /// <returns>The configured gameplay level count.</returns>
        private int GetLevelCount()
        {
            return BitSettings.Instance?.GetLevelSceneCount() ?? 0;
        }

        /// <summary>Gets a scene name from a 1-based level index.</summary>
        /// <param name="level">Level index to resolve.</param>
        /// <returns>The matching scene name, or the first level for indexes below one.</returns>
        private string GetSceneName(int level)
        {
            BitSettings settings = BitSettings.Instance;
            if (settings == null) { return string.Empty; }
            return settings.GetLevelSceneName(level);
        }

        /// <summary>Saves progress without allowing normal level completion to regress it.</summary>
        /// <param name="level">1-based level index to save.</param>
        /// <param name="doesReplaceProgress">Whether this is an explicit new-game reset.</param>
        private void SaveUnlockedLevel(int level, bool doesReplaceProgress)
        {
            int savedLevel = GetLastUnlockedLevel();
            int value = doesReplaceProgress ? level : Mathf.Max(savedLevel, level);
            PlayerPrefs.SetInt(LastUnlockedLevelKey, Mathf.Max(1, value));
            PlayerPrefs.Save();
        }
    }
}
