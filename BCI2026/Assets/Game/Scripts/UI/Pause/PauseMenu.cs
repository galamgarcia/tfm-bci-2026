/*
 * BCI Interaction System for Videogames
 * Master's Thesis · University of Alicante
 * © 2026 Gala M. García
 */

using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Bit.UI
{
    /// <summary>Presents the blocking pause menu and its selected option.</summary>
    public sealed class PauseMenu : MonoBehaviour
    {
        [Header("Window")]
        [Tooltip("Canvas group used to show and block the pause menu.")]
        [SerializeField] private CanvasGroup canvasGroup;
        [Tooltip("Button that resumes the current level.")]
        [SerializeField] private Button continueButton;
        [Tooltip("Button that returns to the main menu.")]
        [SerializeField] private Button exitButton;

        // Current option selected by nod navigation.
        private int _selectedOption;

        /// <summary>Indicates whether the pause menu is currently visible.</summary>
        /// <returns>True when the menu blocks the gameplay view.</returns>
        public bool IsVisible()
        {
            return canvasGroup != null && canvasGroup.alpha > 0.5f;
        }

        /// <summary>Shows the modal and selects its first usable option.</summary>
        public void Show()
        {
            _selectedOption = 0;
            ApplyVisibility(true);
            SelectCurrentOption();
        }

        /// <summary>Hides the modal and releases its UI blocking state.</summary>
        public void Hide()
        {
            ApplyVisibility(false);
        }

        /// <summary>Moves selection to the next usable pause option.</summary>
        public void SelectNextOption()
        {
            _selectedOption = (_selectedOption + 1) % 2;
            SelectCurrentOption();
        }

        /// <summary>Invokes the currently selected pause option.</summary>
        public void ConfirmSelection()
        {
            GetCurrentButton()?.onClick.Invoke();
        }

        /// <summary>Assigns the actions invoked by the two pause options.</summary>
        /// <param name="continueAction">Action that resumes gameplay.</param>
        /// <param name="exitAction">Action that returns to the main menu.</param>
        public void SetActions(UnityAction continueAction, UnityAction exitAction)
        {
            continueButton.onClick.RemoveAllListeners();
            exitButton.onClick.RemoveAllListeners();
            continueButton.onClick.AddListener(continueAction);
            exitButton.onClick.AddListener(exitAction);
        }

        /// <summary>Gets the currently selected button for controller bindings.</summary>
        /// <returns>The selected button, or null when the menu is not configured.</returns>
        public Button GetCurrentButton()
        {
            return _selectedOption == 0 ? continueButton : exitButton;
        }

        private void Awake()
        {
            ApplyVisibility(false);
        }

        private void ApplyVisibility(bool isVisible)
        {
            if (canvasGroup == null) { return; }
            canvasGroup.alpha = isVisible ? 1f : 0f;
            canvasGroup.interactable = isVisible;
            canvasGroup.blocksRaycasts = isVisible;
        }

        private void SelectCurrentOption()
        {
            Button button = GetCurrentButton();
            if (button != null && button.interactable && EventSystem.current != null)
            {
                EventSystem.current.SetSelectedGameObject(button.gameObject);
            }
        }

    }
}
