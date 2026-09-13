/*
 * BCI Interaction System for Videogames
 * Master's Thesis · University of Alicante
 * © 2026 Gala M. García
 */

using Bit.Gameplay;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Bit.UI
{
    /// <summary>Pauses gameplay systems while a blocking overlay is visible.</summary>
    public sealed class GameplayOverlayLock : MonoBehaviour
    {
        // Time scale before the overlay was opened.
        private float _previousTimeScale = 1f;
        // Whether this lock currently owns the gameplay pause.
        private bool _isLocked;
        // Player controller currently controlled by the active scene.
        private BitController _bit;
        // Gameplay camera currently controlled by the active scene.
        private GameplayCamera _camera;

        private void Awake()
        {
            RefreshSceneReferences();
        }

        private void OnEnable()
        {
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        private void OnDisable()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            RefreshSceneReferences();
        }

        /// <summary>Disables gameplay and camera tracking.</summary>
        public void Lock()
        {
            if (_isLocked) { return; }
            RefreshSceneReferences();
            _previousTimeScale = Time.timeScale;
            Time.timeScale = 0f;
            _bit?.SetGameplayEnabled(false);
            _bit?.SetMovementEnabled(false);
            _camera?.PauseTracking(true);
            _isLocked = true;
        }

        /// <summary>Restores gameplay and camera tracking.</summary>
        public void Unlock()
        {
            if (!_isLocked) { return; }
            _camera?.PauseTracking(false);
            _bit?.SetMovementEnabled(true);
            _bit?.SetGameplayEnabled(true);
            Time.timeScale = _previousTimeScale;
            _isLocked = false;
        }

        private void RefreshSceneReferences()
        {
            _bit = FindFirstObjectByType<BitController>();
            _camera = FindFirstObjectByType<GameplayCamera>();
        }
    }
}
