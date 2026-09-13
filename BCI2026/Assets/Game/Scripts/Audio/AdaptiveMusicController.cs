/*
 * BCI Interaction System for Videogames
 * Master's Thesis · University of Alicante
 * © 2026 Gala M. García
 */

using Bit.Input;
using Bit.Core;
using UnityEngine;

namespace Bit.Audio
{
    /// <summary>Synchronizes persistent music stems and adapts their mix to filtered mental input.</summary>
    [DefaultExecutionOrder(-40)]
    public sealed class AdaptiveMusicController : MonoBehaviour
    {
        public static AdaptiveMusicController Instance { get; private set; }

        [Header("Mental Input")]
        [Tooltip("Component implementing the shared filtered mental input source.")]
        [SerializeField] private MonoBehaviour mentalInputSourceComponent;

        [Header("Audio Clips")]
        [Tooltip("Required ambient foundation stem.")]
        [SerializeField] private AudioClip ambientBed;
        [Tooltip("Stem adapted to concentration.")]
        [SerializeField] private AudioClip arpPulse;
        [Tooltip("Stem adapted to relaxation.")]
        [SerializeField] private AudioClip digitalTexture;

        [Header("Base Layer")]
        [Tooltip("Amplitude of the ambient foundation layer.")]
        [SerializeField, Range(0f, 1f)] private float baseVolume = 0.65f;

        [Header("Focus Layer")]
        [Tooltip("Minimum amplitude of the concentration layer.")]
        [SerializeField, Range(0f, 1f)] private float focusMinimumVolume;
        [Tooltip("Maximum amplitude of the concentration layer.")]
        [SerializeField, Range(0f, 1f)] private float focusMaximumVolume = 0.65f;

        [Header("Relaxation Layer")]
        [Tooltip("Minimum amplitude of the relaxation layer.")]
        [SerializeField, Range(0f, 1f)] private float relaxMinimumVolume;
        [Tooltip("Maximum amplitude of the relaxation layer.")]
        [SerializeField, Range(0f, 1f)] private float relaxMaximumVolume = 0.45f;

        [Header("Response")]
        [Tooltip("Seconds used to smooth changes in each adaptive layer.")]
        [SerializeField, Min(0.01f)] private float smoothingDuration = 0.75f;
        [Tooltip("Seconds scheduled ahead of the current DSP clock when playback starts.")]
        [SerializeField, Min(0.05f)] private float scheduleAheadSeconds = 0.2f;
        [Tooltip("Perceptual response curve applied to normalized mental values.")]
        [SerializeField] private AnimationCurve responseCurve = AnimationCurve.Linear(0f, 0f, 1f, 1f);

#if UNITY_EDITOR
        [Header("Editor Debug")]
        [Tooltip("Uses Inspector values instead of the filtered source while playing in the Editor.")]
        [SerializeField] private bool isDebugInputEnabled;
        [Tooltip("Simulated concentration value used by the Editor debug mode.")]
        [SerializeField, Range(0f, 1f)] private float debugConcentration;
        [Tooltip("Simulated relaxation value used by the Editor debug mode.")]
        [SerializeField, Range(0f, 1f)] private float debugRelaxation;
#endif

        // Shared continuous mental input source.
        private IMentalInputSource _mentalInput;
        // Runtime source used by the ambient stem.
        private AudioSource _ambientSource;
        // Runtime source used by the concentration stem.
        private AudioSource _focusSource;
        // Runtime source used by the relaxation stem.
        private AudioSource _relaxSource;
        // Smoothed concentration value.
        private float _smoothedConcentration;
        // Smoothed relaxation value.
        private float _smoothedRelaxation;
        // DSP time shared by every scheduled stem.
        private double _scheduledStartTime;
        // Last signal availability state used for restrained diagnostics.
        private bool _wasSignalValid;
        // Prevents update work before all required initialization is complete.
        private bool _isReady;
        // Indicates whether the active scene is the configured main menu.
        private bool _isMainMenuActive;
        // Indicates whether the pause menu is forcing the neutral mix.
        private bool _isPauseActive;
        // Concentration value captured before entering pause.
        private float _pauseConcentration;
        // Relaxation value captured before entering pause.
        private float _pauseRelaxation;

        /// <summary>Initializes the unique persistent music system.</summary>
        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Debug.LogWarning("AdaptiveMusicController: duplicate instance discarded without restarting playback.");
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);
            GameStateController stateController = GameStateController.Instance;
            if (stateController != null)
            {
                stateController.OnStateChanged += OnGameStateChanged;
                _isMainMenuActive = stateController.GetState() == GameState.MainMenu;
            }
            _mentalInput = mentalInputSourceComponent as IMentalInputSource;
            if (_isMainMenuActive)
            {
                _smoothedConcentration = 1f;
                _smoothedRelaxation = 1f;
            }
            CreateSources();
            ApplyVolumes();
            ScheduleStems();
        }

        /// <summary>Updates smoothed mental values and layer amplitudes.</summary>
        private void Update()
        {
            if (!_isReady) { return; }

            ReadMentalValues(out float concentration, out float relaxation, out bool hasValidSignal);
            float blend = GetSmoothingBlend(Time.unscaledDeltaTime);
            _smoothedConcentration = Mathf.Lerp(_smoothedConcentration, concentration, blend);
            _smoothedRelaxation = Mathf.Lerp(_smoothedRelaxation, relaxation, blend);

            if (hasValidSignal != _wasSignalValid)
            {
                Debug.Log($"AdaptiveMusicController: EEG signal {(hasValidSignal ? "available" : "unavailable")}; adaptive layers are {(hasValidSignal ? "active" : "returning to neutral") }.");
                _wasSignalValid = hasValidSignal;
            }

            ApplyVolumes();
        }

        /// <summary>Clears the singleton reference when the persistent object is destroyed.</summary>
        private void OnDestroy()
        {
            if (GameStateController.Instance != null)
            {
                GameStateController.Instance.OnStateChanged -= OnGameStateChanged;
            }
            if (Instance == this)
            {
                Instance = null;
            }
        }

        /// <summary>Updates menu routing when the global application state changes.</summary>
        /// <param name="state">New global application state.</param>
        private void OnGameStateChanged(GameState state)
        {
            _isMainMenuActive = state == GameState.MainMenu;
            if (state == GameState.Pause)
            {
                _pauseConcentration = _smoothedConcentration;
                _pauseRelaxation = _smoothedRelaxation;
                _isPauseActive = true;
            }
            else if (state == GameState.Game && _isPauseActive)
            {
                _smoothedConcentration = _pauseConcentration;
                _smoothedRelaxation = _pauseRelaxation;
                _isPauseActive = false;
            }
            if (_isMainMenuActive)
            {
                _smoothedConcentration = 1f;
                _smoothedRelaxation = 1f;
                ApplyVolumes();
            }
        }

        /// <summary>Creates one non-spatial looping source for each configured stem.</summary>
        private void CreateSources()
        {
            _ambientSource = CreateSource("Ambient_Bed", ambientBed);
            _focusSource = CreateSource("Arp_Pulse", arpPulse);
            _relaxSource = CreateSource("Digital_Texture", digitalTexture);

            if (ambientBed == null)
            {
                Debug.LogError("AdaptiveMusicController: Ambient_Bed is required; music playback was not started.");
                return;
            }

            _isReady = true;
        }

        /// <summary>Creates a configured AudioSource without starting playback.</summary>
        /// <param name="name">Runtime object name for the stem.</param>
        /// <param name="clip">Audio clip assigned to the source.</param>
        /// <returns>The configured source, or null when the clip is missing.</returns>
        private AudioSource CreateSource(string name, AudioClip clip)
        {
            if (clip == null)
            {
                Debug.LogWarning($"AdaptiveMusicController: optional stem {name} is not assigned.");
                return null;
            }

            GameObject sourceObject = new GameObject(name);
            sourceObject.transform.SetParent(transform, false);
            AudioSource source = sourceObject.AddComponent<AudioSource>();
            source.clip = clip;
            source.loop = true;
            source.playOnAwake = false;
            source.spatialBlend = 0f;
            source.volume = 0f;
            return source;
        }

        /// <summary>Schedules every available stem at one shared DSP timestamp.</summary>
        private void ScheduleStems()
        {
            if (!_isReady) { return; }
            _scheduledStartTime = AudioSettings.dspTime + Mathf.Max(0.05f, scheduleAheadSeconds);
            ScheduleSource(_ambientSource);
            ScheduleSource(_focusSource);
            ScheduleSource(_relaxSource);
            Debug.Log($"AdaptiveMusicController: stems scheduled at DSP time {_scheduledStartTime:F3}.");
        }

        /// <summary>Schedules one stem without changing the shared start timestamp.</summary>
        /// <param name="source">Stem source to schedule.</param>
        private void ScheduleSource(AudioSource source)
        {
            source?.PlayScheduled(_scheduledStartTime);
        }

        /// <summary>Reads continuous mental values and returns a neutral fallback when unavailable.</summary>
        /// <param name="concentration">Normalized concentration value.</param>
        /// <param name="relaxation">Normalized relaxation value.</param>
        /// <param name="hasValidSignal">Whether the values came from a valid source.</param>
        private void ReadMentalValues(out float concentration, out float relaxation, out bool hasValidSignal)
        {
            if (_isMainMenuActive || _isPauseActive)
            {
                concentration = 1f;
                relaxation = 1f;
                hasValidSignal = true;
                return;
            }

#if UNITY_EDITOR
            if (isDebugInputEnabled)
            {
                concentration = debugConcentration;
                relaxation = debugRelaxation;
                hasValidSignal = true;
                return;
            }
#endif
            hasValidSignal = _mentalInput != null && _mentalInput.HasValidSignal;
            if (!hasValidSignal)
            {
                concentration = 0f;
                relaxation = 0f;
                return;
            }
            concentration = Mathf.Clamp01(_mentalInput.Concentration);
            relaxation = Mathf.Clamp01(_mentalInput.Relaxation);
        }

        /// <summary>Calculates an exponential smoothing blend for the current frame.</summary>
        /// <param name="deltaTime">Unscaled frame duration.</param>
        /// <returns>The interpolation factor for this update.</returns>
        private float GetSmoothingBlend(float deltaTime)
        {
            float duration = Mathf.Max(0.01f, smoothingDuration);
            return 1f - Mathf.Exp(-Mathf.Max(0f, deltaTime) / duration);
        }

        /// <summary>Applies smoothed mental values to the three synchronized sources.</summary>
        private void ApplyVolumes()
        {
            if (_ambientSource != null) { _ambientSource.volume = Mathf.Clamp01(baseVolume); }
            if (_focusSource != null) { _focusSource.volume = GetLayerVolume(_smoothedConcentration, focusMinimumVolume, focusMaximumVolume); }
            if (_relaxSource != null) { _relaxSource.volume = GetLayerVolume(_smoothedRelaxation, relaxMinimumVolume, relaxMaximumVolume); }
        }

        /// <summary>Maps a normalized mental value through the configured perceptual response.</summary>
        /// <param name="value">Normalized mental value.</param>
        /// <param name="minimum">Layer amplitude at zero.</param>
        /// <param name="maximum">Layer amplitude at one.</param>
        /// <returns>The clamped audio amplitude.</returns>
        private float GetLayerVolume(float value, float minimum, float maximum)
        {
            float normalized = responseCurve == null
                ? Mathf.Clamp01(value)
                : Mathf.Clamp01(responseCurve.Evaluate(Mathf.Clamp01(value)));
            return Mathf.Clamp01(Mathf.Lerp(Mathf.Min(minimum, maximum), Mathf.Max(minimum, maximum), normalized));
        }
    }
}
