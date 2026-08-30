/*
 * BCI Interaction System for Videogames
 * Master's Thesis · University of Alicante
 * © 2026 Gala M. García
 */

using System;
using Bit.Core;
using UnityEngine;

namespace Bit.Input
{
    /// <summary>Adapts BrainLink blink packets into validated application input events.</summary>
    public sealed class BrainLinkBlinkInputSource : MonoBehaviour, IBlinkInputSource
    {
        [Header("References")]
        [Tooltip("BrainLink provider that emits raw blink intensities.")]
        [SerializeField] private BrainLinkManager brainLinkManager;

        public static BrainLinkBlinkInputSource Instance { get; private set; }
        // Detector that stabilizes raw provider blink intensities.
        private BlinkDetector _detector;

        public bool HasValidSignal => brainLinkManager != null && brainLinkManager.GetWave_quality() <= 75;
        public event Action OnBlinkDetected;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(this);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);
            BciSettings settings = BciSettings.Instance;
            _detector = new BlinkDetector(settings.BlinkIntensity, settings.blinkCooldown);
        }

        private void OnEnable()
        {
            if (brainLinkManager != null)
            {
                brainLinkManager.OnBlinkReceived += OnBlinkReceived;
            }
        }

        private void OnDisable()
        {
            if (brainLinkManager != null)
            {
                brainLinkManager.OnBlinkReceived -= OnBlinkReceived;
            }
        }

        private void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }
        }

        /// <summary>Processes a raw BrainLink blink packet.</summary>
        /// <param name="intensity">Raw blink intensity reported by BrainLink.</param>
        private void OnBlinkReceived(int intensity)
        {
            if (_detector != null && _detector.Process(intensity, HasValidSignal, Time.unscaledTime))
            {
                OnBlinkDetected?.Invoke();
            }
        }
    }
}
