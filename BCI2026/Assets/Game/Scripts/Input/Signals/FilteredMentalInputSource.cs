/*
 * BCI Interaction System for Videogames
 * Master's Thesis · University of Alicante
 * © 2026 Gala M. García
 */

using BciGame.Core;
using BciGame.Input;
using BciGame.Services;
using UnityEngine;

namespace BciGame.Input
{
    /// <summary>Provides one shared filtered mental input source for gameplay consumers.</summary>
    [DefaultExecutionOrder(-50)]
    public sealed class FilteredMentalInputSource : MonoBehaviour, IMentalInputSource
    {
        public static FilteredMentalInputSource Instance { get; private set; }
        // BrainLink source with unfiltered values.
        private IMentalInputSource _rawSource;
        // Independent filters so relaxation and concentration retain separate sample windows.
        private MentalSignalFilter _relaxationFilter;
        private MentalSignalFilter _concentrationFilter;
        // Timestamp of the last raw-source read.
        private float _lastSampledAt = float.NegativeInfinity;
        // Most recently published filtered mental-state values.
        private float _relaxation;
        private float _concentration;

        public bool HasValidSignal => _rawSource != null && _rawSource.HasValidSignal;
        public float Relaxation => _relaxation;
        public float Concentration => _concentration;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);
            _rawSource = BrainLinkConnection.Instance;
            CreateFilters();
        }

        private void Update()
        {
            _rawSource ??= BrainLinkConnection.Instance;
            if (!HasValidSignal)
            {
                ResetFilters();
                return;
            }

            BciSettings settings = BciSettings.Instance;
            float currentTime = Time.unscaledTime;
            if (currentTime - _lastSampledAt < settings.SampleIntervalSeconds) { return; }

            _lastSampledAt = currentTime;
            if (_relaxationFilter.TryUpdate(_rawSource.Relaxation, currentTime, out float relaxation))
            {
                _relaxation = relaxation;
            }

            if (_concentrationFilter.TryUpdate(_rawSource.Concentration, currentTime, out float concentration))
            {
                _concentration = concentration;
            }
        }

        private void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }
        }

        /// <summary>Creates filters for relaxation and concentration using the BCI settings.</summary>
        private void CreateFilters()
        {
            BciSettings settings = BciSettings.Instance;
            _relaxationFilter = new MentalSignalFilter(settings.AveragingWindowSeconds, settings.PublishIntervalSeconds, settings.OutlierTrimPercentage);
            _concentrationFilter = new MentalSignalFilter(settings.AveragingWindowSeconds, settings.PublishIntervalSeconds, settings.OutlierTrimPercentage);
        }

        /// <summary>Clears filters and published values when the signal is invalid.</summary>
        private void ResetFilters()
        {
            if (_relaxationFilter == null) { return; }
            _relaxationFilter.Reset();
            _concentrationFilter.Reset();
            _lastSampledAt = float.NegativeInfinity;
            _relaxation = 0f;
            _concentration = 0f;
        }
    }
}
