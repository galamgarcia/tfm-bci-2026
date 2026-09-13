/*
 * BCI Interaction System for Videogames
 * Master's Thesis · University of Alicante
 * © 2026 Gala M. García
 */

using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Bit.Core;
using Bit.Services;

namespace Bit.UI
{
    /// <summary>Controls the menu save-state option and initial selection.</summary>
    public sealed class MainMenuController : MonoBehaviour
    {
        [Header("Buttons")]
        [Tooltip("Continue button whose interactability follows the saved-game state.")]
        [SerializeField] private Button continueButton;

        [Header("Selection")]
        [Tooltip("Button selected when the menu starts.")]
        [SerializeField] private Button initialButton;

        private void Awake()
        {
            GameStateController.Instance?.SetState(GameState.MainMenu);
            RefreshContinueButton();
        }

        private void Start()
        {
            if (initialButton != null && initialButton.interactable && EventSystem.current != null)
            {
                EventSystem.current.SetSelectedGameObject(initialButton.gameObject);
            }
        }

        /// <summary>Quits the application from the exit button.</summary>
        public void Exit()
        {
            Application.Quit();
        }

        /// <summary>Starts the first playable level from the new-game button.</summary>
        public void StartNewGame()
        {
            SaveSystem.Instance?.StartNewGame();
            GameStateController.Instance?.SetState(GameState.Game);
            SceneManager.LoadScene(BitSettings.Instance.GetFirstLevelSceneName());
        }

        /// <summary>Continues from the last saved level.</summary>
        public void Continue()
        {
            if (SaveSystem.Instance == null || !SaveSystem.Instance.HasSavedGame())
            {
                return;
            }

            GameStateController.Instance?.SetState(GameState.Game);
            SceneManager.LoadScene(SaveSystem.Instance.GetContinueSceneName());
        }

        /// <summary>Updates the continue button from the local save state.</summary>
        private void RefreshContinueButton()
        {
            if (continueButton == null) { return; }
            continueButton.interactable = SaveSystem.Instance != null && SaveSystem.Instance.HasSavedGame();
            continueButton.GetComponent<MenuButton>()?.RefreshVisualState();
        }
    }
}
