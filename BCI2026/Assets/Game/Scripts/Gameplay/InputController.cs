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

        [Header("Mental State")]
        [Tooltip("Value at which a mental state starts moving to High.")]
        [SerializeField, Range(0f, 1f)] private float highValue = 0.6f;
        [Tooltip("Value at which a mental state starts moving to Low.")]
        [SerializeField, Range(0f, 1f)] private float lowValue = 0.4f;
        [Tooltip("Seconds the new mental state must remain stable before it is published.")]
        [SerializeField, Min(0f)] private float confirmationTime = 0.25f;
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
        // Candidate relaxation state waiting for confirmation.
        private MentalStateLevel _relaxationCandidate = MentalStateLevel.None;
        // Candidate concentration state waiting for confirmation.
        private MentalStateLevel _concentrationCandidate = MentalStateLevel.None;
        // Time the relaxation candidate has remained stable.
        private float _relaxationCandidateTime;
        // Time the concentration candidate has remained stable.
        private float _concentrationCandidateTime;

        /// <summary>Triggered when the relaxation level changes.</summary>
        public event Action<MentalStateLevel> OnRelaxationChanged;
        /// <summary>Triggered when the concentration level changes.</summary>
        public event Action<MentalStateLevel> OnConcentrationChanged;
        /// <summary>Triggered with the normalized horizontal intent from the current input source.</summary>
        public event Action<float> OnHorizontalInputReceived;
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
            UpdateHorizontalInput();
            UpdateMentalStateChanges();
        }

        /// <summary>Injects the head and EEG sources consumed by this input component.</summary>
        /// <param name="head">Source that provides head movement and nod gestures.</param>
        /// <param name="mental">Source that provides signal quality and EEG samples.</param>
        public void ConfigureSources(IHeadInputSource head, IMentalInputSource mental)
        {
            if (ReferenceEquals(_headInput, head) && ReferenceEquals(_mentalInput, mental) && _blinkInput == null) { return; }
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

        /// <summary>Publishes normalized horizontal input for the current frame when enabled.</summary>
        private void UpdateHorizontalInput()
        {
            if (!isHorizontalMovementTracked)
            {
                OnHorizontalInputReceived?.Invoke(0f);
                return;
            }

            bool hasInput = TryGetHeadHorizontalInput(out float input);
            OnHorizontalInputReceived?.Invoke(hasInput ? input : 0f);
        }

        /// <summary>Updates changes to tracked relaxation and concentration levels.</summary>
        private void UpdateMentalStateChanges()
        {
            if (isRelaxationStateTracked)
            {
                NotifyRelaxationChanged(UpdateStableLevel(_mentalInput == null ? float.NaN : _mentalInput.Relaxation,
                    _relaxationLevel, ref _relaxationCandidate, ref _relaxationCandidateTime));
            }

            if (isConcentrationStateTracked)
            {
                NotifyConcentrationChanged(UpdateStableLevel(_mentalInput == null ? float.NaN : _mentalInput.Concentration,
                    _concentrationLevel, ref _concentrationCandidate, ref _concentrationCandidateTime));
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
            _relaxationCandidate = MentalStateLevel.None;
            _concentrationCandidate = MentalStateLevel.None;
            _relaxationCandidateTime = 0f;
            _concentrationCandidateTime = 0f;
        }

        /// <summary>Updates a low/high state after the candidate has remained stable.</summary>
        /// <param name="value">Normalized EEG value expected in the range [0,1].</param>
        /// <param name="current">Currently published state.</param>
        /// <param name="candidate">Candidate state being confirmed.</param>
        /// <param name="candidateTime">Time accumulated by the candidate state.</param>
        /// <returns>The currently published state.</returns>
        private MentalStateLevel UpdateStableLevel(float value, MentalStateLevel current,
            ref MentalStateLevel candidate, ref float candidateTime)
        {
            if (!HasValidMentalSignal() || float.IsNaN(value) || float.IsInfinity(value))
            {
                candidate = MentalStateLevel.None;
                candidateTime = 0f;
                return MentalStateLevel.None;
            }

            value = Mathf.Clamp01(value);
            MentalStateLevel next = current;
            if (current == MentalStateLevel.None)
            {
                next = value >= highValue ? MentalStateLevel.High : value <= lowValue ? MentalStateLevel.Low : MentalStateLevel.None;
            }
            else if (current == MentalStateLevel.Low && value >= highValue)
            {
                next = MentalStateLevel.High;
            }
            else if (current == MentalStateLevel.High && value <= lowValue)
            {
                next = MentalStateLevel.Low;
            }

            if (next == current || next == MentalStateLevel.None)
            {
                candidate = MentalStateLevel.None;
                candidateTime = 0f;
                return current;
            }

            if (candidate != next)
            {
                candidate = next;
                candidateTime = 0f;
            }

            candidateTime += Time.deltaTime;
            if (candidateTime >= Mathf.Max(0f, confirmationTime))
            {
                candidate = MentalStateLevel.None;
                candidateTime = 0f;
                return next;
            }

            return current;
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
