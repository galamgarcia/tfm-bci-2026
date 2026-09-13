/*
 * BCI Interaction System for Videogames
 * Master's Thesis · University of Alicante
 * © 2026 Gala M. García
 */

using Bit.Core;
using Bit.Services;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Bit.UI
{
    /// <summary>Controls the final-level congratulations modal.</summary>
    [RequireComponent(typeof(GameplayOverlayLock))]
    public sealed class CongratsMenuController : MonoBehaviour
    {
        [Header("References")]
        [Tooltip("Canvas group that controls the modal visibility and input blocking.")]
        [SerializeField] private CanvasGroup canvasGroup;
        [Tooltip("Button to return to main menu.")]
        [SerializeField] private Button exitButton;
        [Tooltip("Confetti motion component activated when the modal opens.")]
        [SerializeField] private ConfettiMotion confettiMotion;

        private GameplayOverlayLock _gameplayLock;
        private GameStateController _stateController;

        private void Awake()
        {
            _gameplayLock = GetComponent<GameplayOverlayLock>();
            _stateController = GameStateController.Instance;
            canvasGroup ??= GetComponent<CanvasGroup>();
            exitButton = GetComponentInChildren<Button>(true);
            confettiMotion = GetComponentInChildren<ConfettiMotion>(true);
            exitButton?.onClick.AddListener(ExitToMainMenu);
            SetHidden();
        }

        private void OnDestroy()
        {
            exitButton?.onClick.RemoveListener(ExitToMainMenu);
        }

        /// <summary>Opens the modal and starts its confetti effect.</summary>
        public void Open()
        {
            _stateController ??= GameStateController.Instance;
            _gameplayLock.Lock();
            _stateController?.SetState(GameState.End);
            SetVisible();
            confettiMotion?.Play();
            EventSystem eventSystem = EventSystem.current ?? GetComponentInChildren<EventSystem>(true);
            if (eventSystem != null && exitButton != null)
            {
                eventSystem.SetSelectedGameObject(null);
                eventSystem.SetSelectedGameObject(exitButton.gameObject);
                exitButton.Select();
            }
        }

        /// <summary>Hides the modal and restores gameplay after a preview or final flow.</summary>
        public void Hide()
        {
            confettiMotion?.Hide();
            SetHidden();
            _gameplayLock.Unlock();
            _stateController?.SetState(GameState.Game);
        }

        /// <summary>Returns to the main menu from the final modal.</summary>
        public void ExitToMainMenu()
        {
            _gameplayLock.Unlock();
            _stateController?.SetState(GameState.MainMenu);
            gameObject.SetActive(false);
            SceneManager.LoadScene(BitSettings.Instance.GetMainMenuSceneName());
        }

        private void SetHidden()
        {
            confettiMotion?.Hide();
            if (canvasGroup == null) { return; }
            canvasGroup.alpha = 0f;
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
        }

        private void SetVisible()
        {
            if (canvasGroup == null) { return; }
            canvasGroup.alpha = 1f;
            canvasGroup.interactable = true;
            canvasGroup.blocksRaycasts = true;
        }
    }
}
