/*
 * BCI Interaction System for Videogames
 * Master's Thesis · University of Alicante
 * © 2026 Gala M. García
 */

using Bit.Core;
using Bit.Gameplay;
using Bit.Input;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Bit.UI
{
    /// <summary>Opens and controls the blocking pause menu during gameplay.</summary>
    public sealed class PauseMenuController : MonoBehaviour
    {
        [Header("References")]
        [Tooltip("Visual pause menu.")]
        [SerializeField] private PauseMenu pauseMenu;
        [Tooltip("Gameplay input controller.")]
        [SerializeField] private InputController inputController;

        // Active player controller paused with the menu.
        private BitController _bit;
        // Active gameplay camera paused with the menu.
        private GameplayCamera _camera;
        // Global state controller used for pause transitions.
        private GameStateController _stateController;
        // Time scale active before the pause opened.
        private float _previousTimeScale = 1f;
        // Prevents the opening nod from immediately selecting an option.
        private float _ignoreNodUntil;

        private void Awake()
        {
            _stateController = GameStateController.Instance;
            pauseMenu?.SetActions(Continue, ExitToMainMenu);
            RefreshSceneReferences();
        }

        private void OnEnable()
        {
            SceneManager.sceneLoaded += OnSceneLoaded;
            if (inputController != null) { inputController.OnNodDetected += OnNodDetected; }
            if (inputController != null) { inputController.OnBlinkDetected += OnBlinkDetected; }
        }

        private void OnDisable()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
            if (inputController != null) { inputController.OnNodDetected -= OnNodDetected; }
            if (inputController != null) { inputController.OnBlinkDetected -= OnBlinkDetected; }
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            RefreshSceneReferences();
        }

        private void RefreshSceneReferences()
        {
            _stateController = GameStateController.Instance;
            _bit = FindFirstObjectByType<BitController>();
            _camera = FindFirstObjectByType<GameplayCamera>();
            InputController nextInputController = FindFirstObjectByType<InputController>();
            if (inputController == nextInputController) { return; }
            if (inputController != null)
            {
                inputController.OnNodDetected -= OnNodDetected;
                inputController.OnBlinkDetected -= OnBlinkDetected;
            }
            inputController = nextInputController;
            if (isActiveAndEnabled && inputController != null)
            {
                inputController.OnNodDetected += OnNodDetected;
                inputController.OnBlinkDetected += OnBlinkDetected;
            }
        }

        /// <summary>Opens the pause menu from the current gameplay state.</summary>
        public void OpenPause()
        {
            if (pauseMenu == null || _stateController == null || _stateController.GetState() != GameState.Game) { return; }
            _previousTimeScale = Time.timeScale;
            Time.timeScale = 0f;
            _bit?.SetGameplayEnabled(false);
            _bit?.SetMovementEnabled(false);
            _camera?.PauseTracking(true);
            _stateController.SetState(GameState.Pause);
            pauseMenu.Show();
            _ignoreNodUntil = Time.unscaledTime + 0.25f;
        }

        /// <summary>Resumes gameplay and restores the time scale active before pausing.</summary>
        public void Continue()
        {
            if (pauseMenu == null || !pauseMenu.IsVisible()) { return; }
            pauseMenu.Hide();
            _camera?.PauseTracking(false);
            _bit?.SetMovementEnabled(true);
            _bit?.SetGameplayEnabled(true);
            Time.timeScale = _previousTimeScale;
            _stateController?.SetState(GameState.Game);
            _ignoreNodUntil = Time.unscaledTime + 0.25f;
        }

        /// <summary>Returns to the configured main menu without deleting saved progress.</summary>
        public void ExitToMainMenu()
        {
            if (pauseMenu == null || !pauseMenu.IsVisible()) { return; }
            pauseMenu.Hide();
            _camera?.PauseTracking(false);
            _bit?.SetMovementEnabled(true);
            _bit?.SetGameplayEnabled(true);
            Time.timeScale = _previousTimeScale;
            _stateController?.SetState(GameState.MainMenu);
            SceneManager.LoadScene(BitSettings.Instance.GetMainMenuSceneName());
        }

        private void OnNodDetected()
        {
            if (Time.unscaledTime < _ignoreNodUntil) { return; }
            if (_stateController == null) { return; }
            if (_stateController.GetState() == GameState.Game)
            {
                OpenPause();
                return;
            }

            if (_stateController.GetState() == GameState.Pause)
            {
                pauseMenu?.SelectNextOption();
            }
        }

        private void OnBlinkDetected()
        {
            if (_stateController?.GetState() == GameState.Pause)
            {
                pauseMenu?.ConfirmSelection();
            }
        }
    }
}
