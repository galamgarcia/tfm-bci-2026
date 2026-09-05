/*
 * BCI Interaction System for Videogames
 * Master's Thesis · University of Alicante
 * © 2026 Gala M. García
 */

using System.Collections;
using Bit.Core;
using UnityEngine;

namespace Bit.Gameplay
{
    /// <summary>Coordinates Bit respawn.</summary>
    public sealed class RespawnController : MonoBehaviour
    {
        [Header("References")]
        [Tooltip("Bit controlled in this level.")]
        [SerializeField] private BitController bit;
        [Tooltip("Gameplay camera used in this level.")]
        [SerializeField] private GameplayCamera cameraController;
        [Tooltip("Brief digital screen effect played before respawn.")]
        [SerializeField] private ScreenGlitch screenGlitch;

        [Header("Timing")]
        [Tooltip("Unscaled seconds between Bit leaving the viewport and the glitch.")]
        [SerializeField, Min(0f)] private float postFallDelay = 0.15f;
        [Tooltip("Unscaled seconds to keep Bit's gameplay input locked after respawn.")]
        [SerializeField, Min(0f)] private float postRespawnInputDelay = 0.2f;

        // Prevents multiple fall sequences from running together.
        private bool _isSequenceActive;

        /// <summary>Stores a checkpoint position independently from its GameObject lifetime.</summary>
        /// <param name="position">World respawn position.</param>
        /// <returns>True when the position was accepted.</returns>
        public bool SetCheckpoint(Vector3 position)
        {
            if (_isSequenceActive || bit == null) { return false; }
            bit.SetRespawnPosition(position);
            return true;
        }

        /// <summary>Starts one fall sequence for the configured Bit.</summary>
        /// <returns>True when a new sequence was started.</returns>
        public bool BeginFall()
        {
            if (_isSequenceActive || bit == null) { return false; }
            _isSequenceActive = true;
            StartCoroutine(FallSequence());
            return true;
        }

        /// <summary>Returns whether a fall or respawn sequence is currently active.</summary>
        /// <returns>True while Bit is unavailable for gameplay.</returns>
        public bool IsRespawning()
        {
            return _isSequenceActive;
        }

        private IEnumerator FallSequence()
        {
            bit.SetGameplayEnabled(false);
            cameraController?.PauseTracking(true);

            Camera camera = cameraController?.GetCamera();
            while (camera != null && !Utils.IsBelowViewport(camera, bit.GetBodyBounds()))
            {
                yield return new WaitForFixedUpdate();
            }

            if (postFallDelay > 0f) { yield return new WaitForSecondsRealtime(postFallDelay); }
            if (screenGlitch != null) { yield return screenGlitch.Play(); }

            bit.ResetForRespawn();
            cameraController?.SnapToTarget();
            cameraController?.PauseTracking(false);
            if (postRespawnInputDelay > 0f) { yield return new WaitForSecondsRealtime(postRespawnInputDelay); }
            bit.SetGameplayEnabled(true);
            _isSequenceActive = false;
        }
    }
}
