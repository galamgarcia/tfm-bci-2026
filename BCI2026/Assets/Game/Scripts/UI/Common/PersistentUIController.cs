/*
 * BCI Interaction System for Videogames
 * Master's Thesis · University of Alicante
 * © 2026 Gala M. García
 */

using Bit.Core;
using UnityEngine;

namespace Bit.UI
{
    /// <summary>Persists input-related UI.</summary>
    [DefaultExecutionOrder(-80)]
    public sealed class PersistentUIController : MonoBehaviour
    {
        [Header("References")]
        [Tooltip("Calibration button in the persistent UI prefab.")]
        [SerializeField] private CalibrationButton calibrationButton;
        [Tooltip("Pause button in the persistent UI prefab.")]
        [SerializeField] private PauseButton pauseButton;
        [Tooltip("Volume button in the persistent UI prefab.")]
        [SerializeField] private VolumeButton volumeButton;

        private static PersistentUIController _instance;
        private GameStateController _stateController;
        private ConnectionPopup _connectionPopup;

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }

            _instance = this;
            DontDestroyOnLoad(gameObject);
            RebindButtons();
            _stateController = GameStateController.Instance;
            if (_stateController != null) { _stateController.OnStateChanged += OnStateChanged; }
            pauseButton?.gameObject.SetActive(false);
        }

        private void OnDestroy()
        {
            if (_stateController != null) { _stateController.OnStateChanged -= OnStateChanged; }
            if (_instance == this) { _instance = null; }
        }

        private void Update()
        {
            RebindButtons();
            if (calibrationButton == null) { return; }
            if (_stateController == null && GameStateController.Instance != null)
            {
                _stateController = GameStateController.Instance;
                _stateController.OnStateChanged += OnStateChanged;
            }
            _connectionPopup ??= ConnectionPopup.Instance;
            if (_connectionPopup != null && !_connectionPopup.gameObject.activeInHierarchy)
            {
                _connectionPopup = null;
            }

            calibrationButton.gameObject.SetActive(_connectionPopup == null || !_connectionPopup.IsVisible());
            bool isGameplay = _stateController != null && _stateController.GetState() == GameState.Game;
            pauseButton?.gameObject.SetActive(isGameplay && (_connectionPopup == null || !_connectionPopup.IsVisible()));
            volumeButton?.gameObject.SetActive(true);
        }

        /// <summary>Reacquires the persistent prefab's child button references after a scene transition.</summary>
        private void RebindButtons()
        {
            calibrationButton ??= GetComponentInChildren<CalibrationButton>(true);
            pauseButton ??= GetComponentInChildren<PauseButton>(true);
            volumeButton ??= GetComponentInChildren<VolumeButton>(true);
        }

        /// <summary>Refreshes button visibility immediately after the application state changes.</summary>
        /// <param name="state">New application state.</param>
        private void OnStateChanged(GameState state)
        {
            RebindButtons();
            if (pauseButton != null)
            {
                pauseButton.gameObject.SetActive(state == GameState.Game);
            }
        }

    }
}
