/*
 * BCI Interaction System for Videogames
 * Master's Thesis · University of Alicante
 * © 2026 Gala M. García
 */

using System.Collections;
using Bit.Core;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Bit.Gameplay;

namespace Bit.UI
{
    /// <summary>Controls the optional tutorial overlay shown after a level starts.</summary>
    public sealed class TutorialController : MonoBehaviour
    {
        // Overlay group used to show and block the tutorial.
        private CanvasGroup _canvasGroup;
        // Button that closes the tutorial.
        private Button _closeButton;
        // Input controller used to navigate the tutorial with head gestures.
        private InputController _inputController;
        // Event system belonging to this tutorial overlay.
        private EventSystem _eventSystem;
        // Whether the tutorial is currently open.
        private bool _isOpen;
        // Global state controller used to pause the tutorial phase.
        private GameStateController _stateController;

        private void Awake()
        {
            _canvasGroup = GetComponent<CanvasGroup>();
            _closeButton = GetComponentInChildren<Button>(true);
            _inputController = FindFirstObjectByType<InputController>();
            _eventSystem = GetComponentInChildren<EventSystem>(true);
            _stateController = GameStateController.Instance;
            if (_closeButton != null)
            {
                _closeButton.onClick.AddListener(Continue);
            }

            SetVisibility(false);
        }

        private void Start()
        {
            if (_inputController != null)
            {
                _inputController.OnNodDetected += SelectButton;
                _inputController.OnBlinkDetected += ConfirmButton;
            }
            StartCoroutine(OpenAfterDelay());
        }

        private void OnDestroy()
        {
            if (_closeButton != null)
            {
                _closeButton.onClick.RemoveListener(Continue);
            }
            if (_inputController != null)
            {
                _inputController.OnNodDetected -= SelectButton;
                _inputController.OnBlinkDetected -= ConfirmButton;
            }
        }

        /// <summary>Shows the tutorial and enables its blocking UI.</summary>
        public void Open()
        {
            if (_canvasGroup == null) { return; }
            _isOpen = true;
            _canvasGroup.alpha = 1f;
            _canvasGroup.interactable = true;
            _canvasGroup.blocksRaycasts = true;
            SelectButton();
        }

        /// <summary>Closes and destroys the tutorial, then resumes the level.</summary>
        public void Continue()
        {
            if (!_isOpen) { return; }
            _isOpen = false;
            SetVisibility(false);
            _stateController?.SetState(GameState.Game);
            Destroy(gameObject);
        }

        /// <summary>Returns whether the tutorial is currently blocking the level.</summary>
        /// <returns>True while the tutorial is visible.</returns>
        public bool IsOpen()
        {
            return _isOpen;
        }

        private void SetVisibility(bool isVisible)
        {
            if (_canvasGroup == null) { return; }
            _canvasGroup.alpha = isVisible ? 1f : 0f;
            _canvasGroup.interactable = isVisible;
            _canvasGroup.blocksRaycasts = isVisible;
            if (isVisible) { SelectButton(); }
        }

        private IEnumerator OpenAfterDelay()
        {
            yield return new WaitForSecondsRealtime(0.1f);
            if (this == null) { yield break; }
            _stateController?.SetState(GameState.Tutorial);
            Open();
            yield return null;
            SelectButton();
        }

        /// <summary>Selects the tutorial button after a nod gesture.</summary>
        private void SelectButton()
        {
            if (!_isOpen || _closeButton == null) { return; }
            _eventSystem?.SetSelectedGameObject(_closeButton.gameObject);
            if (EventSystem.current != null && EventSystem.current != _eventSystem)
            {
                EventSystem.current.SetSelectedGameObject(_closeButton.gameObject);
            }
        }

        /// <summary>Confirms the selected tutorial button after a blink gesture.</summary>
        private void ConfirmButton()
        {
            if (!_isOpen || _closeButton == null) { return; }
            _closeButton.onClick.Invoke();
        }
    }
}
