/*
 * BCI Interaction System for Videogames
 * Master's Thesis · University of Alicante
 * © 2026 Gala M. García
 */

using System;
using BciGame.Input;
using BciGame.Services;
using BciGame.Utilities;
using Game.Scripts.Gameplay;
using UnityEngine;

namespace BciGame.Gameplay
{
    /// <summary>
    /// Base component that exposes head-tracking and EEG input to movement implementations.
    /// </summary>
    public class MoveComponent : MonoBehaviour
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

        /// <summary>Scene service that supplies face pose and confirmed nod gestures.</summary>
        private HeadPoseTracker _headPoseTracker;
        /// <summary>Raised when the classified relaxation level changes.</summary>
        public event Action<MentalStateLevel> OnRelaxationChanged;
        /// <summary>Raised when the classified concentration level changes.</summary>
        public event Action<MentalStateLevel> OnConcentrationChanged;
        /// <summary>Raised with the horizontal movement delta detected during the current frame.</summary>
        public event Action<float> OnHorizontalMovementReceived;
        /// <summary>Raised after the head tracker confirms a nod gesture.</summary>
        public event Action OnNodReceived;

        /// <summary>Most recently notified relaxation level.</summary>
        private MentalStateLevel _currentRelaxationStateLevel = MentalStateLevel.None;
        /// <summary>Most recently notified concentration level.</summary>
        private MentalStateLevel _currentConcentrationStateLevel = MentalStateLevel.None;

        protected virtual void Awake()
        {
            _headPoseTracker = FindFirstObjectByType<HeadPoseTracker>();
        }

        protected virtual void OnEnable()
        {
            if (_headPoseTracker != null)
            {
                _headPoseTracker.NodDetected += HandleNodDetected;
            }
        }

        protected virtual void OnDisable()
        {
            if (_headPoseTracker != null)
            {
                _headPoseTracker.NodDetected -= HandleNodDetected;
            }
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
                NotifyRelaxationChanged(TryGetRelaxationInput());
            }

            if (isConcentrationStateTracked)
            {
                NotifyConcentrationChanged(TryGetConcentrationInput());
            }
        }

        /// <summary>
        /// Enables the input sources required by a concrete movement mode.
        /// </summary>
        /// <param name="horizontal">Whether horizontal head movement is enabled.</param>
        /// <param name="nod">Whether confirmed nod gestures are enabled.</param>
        /// <param name="relaxation">Whether relaxation levels are enabled.</param>
        /// <param name="concentration">Whether concentration levels are enabled.</param>
        public void SetInputTracking(bool horizontal, bool nod, bool relaxation, bool concentration)
        {
            isHorizontalMovementTracked = horizontal;
            isNodTracked = nod;
            isRelaxationStateTracked = relaxation;
            isConcentrationStateTracked = concentration;
            _currentRelaxationStateLevel = MentalStateLevel.None;
            _currentConcentrationStateLevel = MentalStateLevel.None;
        }

        /// <summary>
        /// Reads the normalized horizontal head input when face tracking is available.
        /// </summary>
        /// <param name="input">Normalized yaw input, where negative is left and positive is right.</param>
        /// <returns>Whether a valid head input sample is available.</returns>
        protected bool TryGetHeadHorizontalInput(out float input)
        {
            if (_headPoseTracker == null || !_headPoseTracker.HasFace)
            {
                input = 0f;
                return false;
            }

            input = _headPoseTracker.HorizontalInput;
            return true;
        }

        /// <summary>
        /// Reads a normalized relaxation value when BrainLink signal quality is sufficient.
        /// </summary>
        /// <returns>The relaxation level.</returns>
        protected MentalStateLevel TryGetRelaxationInput()
        {
            if (!Utils.IsBrainLinkConnectionGood())
            {
                return MentalStateLevel.None;
            }
            return GetMentalStateLevel(BrainLinkConnection.Instance.Relaxation);
        }

        /// <summary>
        /// Reads a normalized concentration value when BrainLink signal quality is sufficient.
        /// </summary>
        /// <returns>The concentration level.</returns>
        protected MentalStateLevel TryGetConcentrationInput()
        {
            if (!Utils.IsBrainLinkConnectionGood())
            {
                return MentalStateLevel.None;
            }
            return GetMentalStateLevel(BrainLinkConnection.Instance.Concentration);
        }

        /// <summary>
        /// Classifies a normalized BrainLink metric into three equally sized ranges.
        /// </summary>
        /// <param name="value">Normalized BrainLink value, expected in the range from zero to one.</param>
        /// <returns>The corresponding mental state level, or <see cref="MentalStateLevel.None"/> for invalid values.</returns>
        private MentalStateLevel GetMentalStateLevel(float value)
        {
            if (float.IsNaN(value) || float.IsInfinity(value))
            {
                return MentalStateLevel.None;
            }

            value = Mathf.Clamp01(value);
            if (value < 1f / 3f)
            {
                return MentalStateLevel.Low;
            }
            if (value < 2f / 3f)
            {
                return MentalStateLevel.Medium;
            }

            return MentalStateLevel.High;
        }

        /// <summary>
        /// Notifies listeners only when the relaxation level differs from the previous level.
        /// </summary>
        /// <param name="state">New classified relaxation level.</param>
        private void NotifyRelaxationChanged(MentalStateLevel state)
        {
            if (_currentRelaxationStateLevel == state) { return; }
            _currentRelaxationStateLevel = state;
            OnRelaxationChanged?.Invoke(state);
        }

        /// <summary>Notifies listeners only when the concentration level differs from the previous level.</summary>
        /// <param name="state">New classified concentration level.</param>
        private void NotifyConcentrationChanged(MentalStateLevel state)
        {
            if (_currentConcentrationStateLevel == state) { return; }
            _currentConcentrationStateLevel = state;
            OnConcentrationChanged?.Invoke(state);
        }

        /// <summary>
        /// Forwards a confirmed nod when nod input is enabled.
        /// </summary>
        private void HandleNodDetected()
        {
            if (!isNodTracked) { return; }
            OnNodReceived?.Invoke();
        }
    }
}
