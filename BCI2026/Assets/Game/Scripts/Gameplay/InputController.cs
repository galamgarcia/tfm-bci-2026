/*
 * BCI Interaction System for Videogames
 * Master's Thesis · University of Alicante
 * © 2026 Gala M. García
 */

using System;
using Bit.Input;
using UnityEngine;

namespace Bit.Gameplay
{
    /// <summary>Transforms injected head and EEG samples into input events.</summary>
    public class InputController : MonoBehaviour
    {
        [Header("Input Tracking")]
        [Tooltip("Enables horizontal head movement tracking.")]
        [SerializeField] private bool isHorizontalMovementTracked = true;
        [Header("Input Tracking")]
        [Tooltip("Enables nod tracking.")]
        [SerializeField] private bool isNodTracked = true;
        [Header("Input Tracking")]
        [Tooltip("Enables relaxation level tracking.")]
        [SerializeField] private bool isRelaxationStateTracked = true;
        [Header("Input Tracking")]
        [Tooltip("Enables concentration level tracking.")]
        [SerializeField] private bool isConcentrationStateTracked = true;
        [Header("Input Tracking")]
        [Tooltip("Enables BrainLink blink gesture tracking.")]
        [SerializeField] private bool isBlinkTracked = true;
        [Header("Input Tracking")]
        [Tooltip("Horizontal movement speed.")]
        [SerializeField] private float horizontalMovementSpeed = 240f;

        // Configured head input source.
        private IHeadInputSource _headInput;
        // Configured mental input source.
        private IMentalInputSource _mentalInput;
        // Determines whether the nod event subscription is active.
        private bool _isHeadSourceSubscribed;
        // Determines whether the blink event subscription is active.
        private bool _isBlinkSourceSubscribed;
        // Configured discrete blink source.
        private IBlinkInputSource _blinkInput;
        // Last published relaxation level.
        private MentalStateLevel _relaxationLevel = MentalStateLevel.None;
        // Last published concentration level.
        private MentalStateLevel _concentrationLevel = MentalStateLevel.None;

        /// <summary>Triggered when the relaxation level changes.</summary>
        public event Action<MentalStateLevel> OnRelaxationChanged;
        /// <summary>Triggered when the concentration level changes.</summary>
        public event Action<MentalStateLevel> OnConcentrationChanged;
        /// <summary>Triggered with the horizontal movement delta detected during the current frame.</summary>
        public event Action<float> OnHorizontalMovementReceived;
        /// <summary>Triggered after the head input source confirms a nod gesture.</summary>
        public event Action OnNodDetected;
        /// <summary>Triggered after the blink source confirms a blink gesture.</summary>
        public event Action OnBlinkDetected;

        protected virtual void OnEnable()
        {
            SubscribeToHeadSource();
            SubscribeToBlinkSource();
        }

        protected virtual void OnDisable()
        {
            UnsubscribeFromHeadSource();
            UnsubscribeFromBlinkSource();
        }

        protected virtual void Update()
        {
            UpdateHorizontalMovement();
            UpdateMentalStateChanges();
        }

        /// <summary>Injects the head and EEG sources consumed by this input component.</summary>
        /// <param name="head">Source that provides head movement and nod gestures.</param>
        /// <param name="mental">Source that provides signal quality and EEG samples.</param>
        public void ConfigureSources(IHeadInputSource head, IMentalInputSource mental)
        {
            UnsubscribeFromHeadSource();
            UnsubscribeFromBlinkSource();
            _headInput = head;
            _mentalInput = mental;
            _blinkInput = null;
            SubscribeToHeadSource();
        }

        /// <summary>Injects a discrete blink source in addition to the head and EEG sources.</summary>
        /// <param name="head">Source that provides head movement and nod gestures.</param>
        /// <param name="mental">Source that provides signal quality and EEG samples.</param>
        /// <param name="blink">Source that provides validated blink gestures.</param>
        public void ConfigureSources(IHeadInputSource head, IMentalInputSource mental, IBlinkInputSource blink)
        {
            ConfigureSources(head, mental);
            UnsubscribeFromBlinkSource();
            _blinkInput = blink;
            SubscribeToBlinkSource();
        }

        /// <summary>Updates horizontal movement for the current frame when enabled.</summary>
        private void UpdateHorizontalMovement()
        {
            if (!isHorizontalMovementTracked || !TryGetHeadHorizontalInput(out float input)) { return; }
            float delta = input * horizontalMovementSpeed * Time.deltaTime;
            OnHorizontalMovementReceived?.Invoke(delta);
        }

        /// <summary>Updates changes to tracked relaxation and concentration levels.</summary>
        private void UpdateMentalStateChanges()
        {
            if (isRelaxationStateTracked)
            {
                NotifyRelaxationChanged(GetRelaxationLevel());
            }

            if (isConcentrationStateTracked)
            {
                NotifyConcentrationChanged(GetConcentrationLevel());
            }
        }

        /// <summary>Gets if the configured mental input has a valid signal.</summary>
        /// <returns>True, the mental signal is valid.</returns>
        public bool HasValidMentalSignal()
        {
            return _mentalInput != null && _mentalInput.HasValidSignal;
        }

        /// <summary>Gets the last updated relaxation level.</summary>
        /// <returns>The current relaxation level.</returns>
        public MentalStateLevel GetCurrentRelaxationLevel()
        {
            return _relaxationLevel;
        }

        /// <summary>Gets the last updated concentration level.</summary>
        /// <returns>The current concentration level.</returns>
        public MentalStateLevel GetCurrentConcentrationLevel()
        {
            return _concentrationLevel;
        }

        /// <summary>Reads a normalized relaxation value when BrainLink signal quality is sufficient.</summary>
        /// <returns>The relaxation level.</returns>
        private MentalStateLevel GetRelaxationLevel()
        {
            return HasValidMentalSignal() ? ClassifyMentalState(_mentalInput.Relaxation) : MentalStateLevel.None;
        }

        /// <summary>Reads a normalized concentration value when BrainLink signal quality is sufficient.</summary>
        /// <returns>The concentration level.</returns>
        private MentalStateLevel GetConcentrationLevel()
        {
            return HasValidMentalSignal() ? ClassifyMentalState(_mentalInput.Concentration) : MentalStateLevel.None;
        }

        /// <summary>Enables the input sources required by a concrete movement mode.</summary>
        /// <param name="horizontal">Indicates if horizontal head movement is enabled.</param>
        /// <param name="nod">Indicates if confirmed nod gestures are enabled.</param>
        /// <param name="relaxation">Indicates if relaxation levels are enabled.</param>
        /// <param name="concentration">Indicates if concentration levels are enabled.</param>
        /// <param name="blink">Indicates if blink gestures are enabled.</param>
        public void SetInputTracking(bool horizontal, bool nod, bool relaxation, bool concentration, bool blink)
        {
            isHorizontalMovementTracked = horizontal;
            isNodTracked = nod;
            isRelaxationStateTracked = relaxation;
            isConcentrationStateTracked = concentration;
            isBlinkTracked = blink;
            _relaxationLevel = MentalStateLevel.None;
            _concentrationLevel = MentalStateLevel.None;
        }

        /// <summary>Classifies a normalized EEG metric into its mental-state level.</summary>
        /// <param name="value">Normalized EEG value expected in the range [0,1].</param>
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
        /// <param name="yaw">Normalized yaw input, where negative is left and positive is right.</param>
        /// <returns>True if a valid head input sample is available.</returns>
        private bool TryGetHeadHorizontalInput(out float yaw)
        {
            if (_headInput == null || !_headInput.HasFace)
            {
                yaw = 0f;
                return false;
            }

            yaw = _headInput.HorizontalInput;
            return true;
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
            if (_isHeadSourceSubscribed || _headInput == null) { return; }
            _headInput.NodDetected += OnNodReceived;
            _isHeadSourceSubscribed = true;
        }

        /// <summary>Removes the nod-event subscription while preserving the configured source.</summary>
        private void UnsubscribeFromHeadSource()
        {
            if (!_isHeadSourceSubscribed || _headInput == null) { return; }
            _headInput.NodDetected -= OnNodReceived;
            _isHeadSourceSubscribed = false;
        }

        /// <summary>Forwards a confirmed nod when nod input is enabled.</summary>
        private void OnNodReceived()
        {
            if (!isNodTracked) { return; }
            OnNodDetected?.Invoke();
        }

        /// <summary>Subscribes to blink events once a blink source is available.</summary>
        private void SubscribeToBlinkSource()
        {
            if (_isBlinkSourceSubscribed || _blinkInput == null) { return; }
            _blinkInput.OnBlinkDetected += OnBlinkReceived;
            _isBlinkSourceSubscribed = true;
        }

        /// <summary>Removes the blink-event subscription while preserving the configured source.</summary>
        private void UnsubscribeFromBlinkSource()
        {
            if (!_isBlinkSourceSubscribed || _blinkInput == null) { return; }
            _blinkInput.OnBlinkDetected -= OnBlinkReceived;
            _isBlinkSourceSubscribed = false;
        }

        /// <summary>Forwards a confirmed blink when blink input is enabled.</summary>
        private void OnBlinkReceived()
        {
            if (!isBlinkTracked) { return; }
            OnBlinkDetected?.Invoke();
        }
    }
}
