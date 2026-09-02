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
        // Last blink intensity reported to avoid repeating identical diagnostics.
        private int _lastIntensity = -1;

        public bool HasValidSignal => brainLinkManager != null && brainLinkManager.IsHeadsetConnected();
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
            _detector = new BlinkDetector(settings.BlinkIntensity, settings.BlinkCooldown);
        }

        private void Start()
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            using (AndroidJavaClass bridge = new AndroidJavaClass("bit.brainlink.BrainLinkBridge"))
            {
                bridge.CallStatic("install", brainLinkManager == null ? "ThinkGearManager" : brainLinkManager.gameObject.name);
            }
#endif
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

        private void Update()
        {
            if (brainLinkManager != null)
            {
                ProcessBlink(brainLinkManager.GetBlink());
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
            ProcessBlink(intensity);
        }

        /// <summary>Processes one blink intensity from either the SDK callback or its current value.</summary>
        /// <param name="intensity">Raw blink intensity reported by BrainLink.</param>
        private void ProcessBlink(int intensity)
        {
            if (intensity != _lastIntensity && intensity > 0)
            {
                _lastIntensity = intensity;
            }

            if (_detector != null && _detector.Process(intensity, HasValidSignal, Time.unscaledTime))
            {
                OnBlinkDetected?.Invoke();
            }
        }
    }
}
