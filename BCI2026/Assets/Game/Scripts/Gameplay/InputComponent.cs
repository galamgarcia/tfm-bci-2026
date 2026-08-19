/*
 * BCI Interaction System for Videogames
 * Master's Thesis · University of Alicante
 * © 2026 Gala M. García
 */

using System;
using BciGame.Input;
using Game.Scripts.Gameplay;
using UnityEngine;

namespace BciGame.Gameplay
{
    /// <summary>
    /// Transforms injected head and EEG samples into input events.
    /// </summary>
    public class InputComponent : MonoBehaviour
    {
        [Header("Movement")]
        [Tooltip("Defines if horizontal head movement tracking is enabled.")]
        [SerializeField] private bool isHorizontalMovementTracked = true;
        [Tooltip("Defines if nod movement tracking is enabled.")]
        [SerializeField] private bool isNodTracked = true;
        [Tooltip("Defines if relaxation state tracking is enabled.")]
        [SerializeField] private bool isRelaxationStateTracked = true;
        [Tooltip("Defines if concentration state tracking is enabled.")]
        [SerializeField] private bool isConcentrationStateTracked = true;
        [Tooltip("Horizontal speed movement.")]
        [SerializeField] private float horizontalMovementSpeed = 240f;

        // Input sources injected.
        private IHeadInputSource _headInputSource;
        private IMentalInputSource _mentalInputSource;
        private bool _isHeadSourceSubscribed;
        // Last published EEG levels, reset when their tracking mode changes.
        private MentalStateLevel _relaxationLevel = MentalStateLevel.None;
        private MentalStateLevel _concentrationLevel = MentalStateLevel.None;

        /// <summary>Triggered when the relaxation level changes.</summary>
        public event Action<MentalStateLevel> OnRelaxationChanged;
        /// <summary>Triggered when the concentration level changes.</summary>
        public event Action<MentalStateLevel> OnConcentrationChanged;
        /// <summary>Triggered with the horizontal movement delta detected during the current frame.</summary>
        public event Action<float> OnHorizontalMovementReceived;
        /// <summary>Triggered after the head input source confirms a nod gesture.</summary>
        public event Action OnNodDetected;

        /// <summary>Injects the head and EEG sources consumed by this input component.</summary>
        /// <param name="headInputSource">Source that provides head movement and nod gestures.</param>
        /// <param name="mentalInputSource">Source that provides signal quality and EEG samples.</param>
        public void ConfigureSources(IHeadInputSource headInputSource, IMentalInputSource mentalInputSource)
        {
            UnsubscribeFromHeadSource();
            _headInputSource = headInputSource;
            _mentalInputSource = mentalInputSource;
            SubscribeToHeadSource();
        }

        protected virtual void OnEnable()
        {
            SubscribeToHeadSource();
        }

        protected virtual void OnDisable()
        {
            UnsubscribeFromHeadSource();
        }

        protected virtual void Update()
        {
            if (isHorizontalMovementTracked && TryGetHeadHorizontalInput(out float input))
            {
                float delta = input * horizontalMovementSpeed * Time.deltaTime;
                OnHorizontalMovementReceived?.Invoke(delta);
            }

            if (isRelaxationStateTracked)
            {
                NotifyRelaxationChanged(GetRelaxationLevel());
            }

            if (isConcentrationStateTracked)
            {
                NotifyConcentrationChanged(GetConcentrationLevel());
            }
        }

        public bool HasValidMentalSignal()
        {
            return _mentalInputSource != null && _mentalInputSource.HasValidSignal;
        }

        public MentalStateLevel GetCurrentRelaxationLevel()
        {
            return _relaxationLevel;
        }

        public MentalStateLevel GetCurrentConcentrationLevel()
        {
            return _concentrationLevel;
        }

        /// <summary>Enables the input sources required by a concrete movement mode.</summary>
        /// <param name="horizontal">Indicates if horizontal head movement is enabled.</param>
        /// <param name="nod">Indicates if confirmed nod gestures are enabled.</param>
        /// <param name="relaxation">Indicates if relaxation levels are enabled.</param>
        /// <param name="concentration">Indicates if concentration levels are enabled.</param>
        public void SetInputTracking(bool horizontal, bool nod, bool relaxation, bool concentration)
        {
            isHorizontalMovementTracked = horizontal;
            isNodTracked = nod;
            isRelaxationStateTracked = relaxation;
            isConcentrationStateTracked = concentration;
            _relaxationLevel = MentalStateLevel.None;
            _concentrationLevel = MentalStateLevel.None;
        }

        /// <summary>Classifies a normalized EEG metric into its mental-state level.</summary>
        /// <param name="value">Normalized EEG value expected in the range from zero to one.</param>
        /// <returns>The corresponding level, or None for invalid values.</returns>
        public static MentalStateLevel ClassifyMentalState(float value)
        {
            if (float.IsNaN(value) || float.IsInfinity(value)) { return MentalStateLevel.None; }

            value = Mathf.Clamp01(value);
            if (value < 1f / 3f) { return MentalStateLevel.Low; }
            if (value < 2f / 3f) { return MentalStateLevel.Medium; }
            return MentalStateLevel.High;
        }

        /// <summary>Reads the normalized horizontal head input when face tracking is available.</summary>
        /// <param name="input">Normalized yaw input, where negative is left and positive is right.</param>
        /// <returns>True if a valid head input sample is available.</returns>
        private bool TryGetHeadHorizontalInput(out float input)
        {
            if (_headInputSource == null || !_headInputSource.HasFace)
            {
                input = 0f;
                return false;
            }

            input = _headInputSource.HorizontalInput;
            return true;
        }

        /// <summary>Reads a normalized relaxation value when BrainLink signal quality is sufficient.</summary>
        /// <returns>The relaxation level.</returns>
        private MentalStateLevel GetRelaxationLevel()
        {
            return HasValidMentalSignal() ? ClassifyMentalState(_mentalInputSource.Relaxation) : MentalStateLevel.None;
        }

        /// <summary>Reads a normalized concentration value when BrainLink signal quality is sufficient.</summary>
        /// <returns>The concentration level.</returns>
        private MentalStateLevel GetConcentrationLevel()
        {
            return HasValidMentalSignal() ? ClassifyMentalState(_mentalInputSource.Concentration) : MentalStateLevel.None;
        }

        /// <summary>Notifies when the relaxation level differs from the previous level.</summary>
        /// <param name="state">New classified relaxation level.</param>
        private void NotifyRelaxationChanged(MentalStateLevel state)
        {
            if (_relaxationLevel == state) { return; }
            _relaxationLevel = state;
            OnRelaxationChanged?.Invoke(state);
        }

        /// <summary>Notifies when the concentration level differs from the previous level.</summary>
        /// <param name="state">New classified concentration level.</param>
        private void NotifyConcentrationChanged(MentalStateLevel state)
        {
            if (_concentrationLevel == state) { return; }
            _concentrationLevel = state;
            OnConcentrationChanged?.Invoke(state);
        }

        /// <summary>Subscribes to nod events once a head source is available.</summary>
        private void SubscribeToHeadSource()
        {
            if (_isHeadSourceSubscribed || _headInputSource == null) { return; }
            _headInputSource.NodDetected += OnNodReceived;
            _isHeadSourceSubscribed = true;
        }

        /// <summary>Removes the nod-event subscription while preserving the configured source.</summary>
        private void UnsubscribeFromHeadSource()
        {
            if (!_isHeadSourceSubscribed || _headInputSource == null) { return; }
            _headInputSource.NodDetected -= OnNodReceived;
            _isHeadSourceSubscribed = false;
        }

        /// <summary>Forwards a confirmed nod when nod input is enabled.</summary>
        private void OnNodReceived()
        {
            if (!isNodTracked) { return; }
            OnNodDetected?.Invoke();
        }
    }
}
