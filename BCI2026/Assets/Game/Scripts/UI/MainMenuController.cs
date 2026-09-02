/*
 * BCI Interaction System for Videogames
 * Master's Thesis · University of Alicante
 * © 2026 Gala M. García
 */

using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

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
            if (continueButton != null)
            {
                continueButton.interactable = false; // TODO(GG): Implements save game system
                continueButton.GetComponent<MenuButton>()?.RefreshVisualState();
            }
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
            SceneManager.LoadScene("Level01");
        }
    }
}
