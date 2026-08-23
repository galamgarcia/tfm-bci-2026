/*
 * BCI Interaction System for Videogames
 * Master's Thesis · University of Alicante
 * © 2026 Gala M. García
 */

using BciGame.Core;
using BciGame.Input.Signals;
using UnityEngine;
using UnityEngine.UI;

namespace BciGame.UI.Tutorial
{
    /// <summary>Runs one EEG training step and reports completion after a sustained target value.</summary>
    public sealed class TutorialEegTrainingScreen : TutorialScreen
    {
        /// <summary>Identifies the EEG metric evaluated by this training screen.</summary>
        private enum EegTrainingType
        {
            Relaxation,
            Concentration
        }
        
        [Header("Training")]
        [Tooltip("EEG metric evaluated by this training screen.")]
        [SerializeField] private EegTrainingType trainingType;

        [Header("Feedback")]
        [Tooltip("Fill image that visualizes the current normalized EEG value.")]
        [SerializeField] private Image fillImage;
        [Header("Feedback")]
        [Tooltip("Vertical marker that identifies the configured completion target on the rail.")]
        [SerializeField] private Image targetMarker;
        [Header("Feedback")]
        [Tooltip("Centered text shown below the rail with the current filtered percentage.")]
        [SerializeField] private Text targetText;
        [Header("Feedback")]
        [Tooltip("Visual confirmation shown after the training target is completed.")]
        [SerializeField] private GameObject successCheck;
        [Header("Feedback")]
        [Tooltip("Text used to display the localized success message.")]
        [SerializeField] private TutorialText resultText;

        [Header("Completion")]
        [Tooltip("Minimum normalized EEG value required to begin the completion hold.")]
        [SerializeField] private float target = 0.7f;
        [Header("Completion")]
        [Tooltip("Seconds the EEG value must remain above the target to complete the step.")]
        [SerializeField] private float holdSeconds = 2f;
        // Time at which the current valid target hold began.
        private float _holdStartedAt = -1f;
        // Indicates if the screen is completed.
        private bool _isCompleted;
        // Configured mental input source.
        private IMentalInputSource _mentalInput;

        public override float CompletionDelay => 1f;

        public override void Activate()
        {
            _isCompleted = false;
            _holdStartedAt = -1f;
            ResetFeedback();
        }

        private void Update()
        {
            if (_isCompleted) { return; }

            IMentalInputSource mentalInput = _mentalInput ?? FilteredMentalInputSource.Instance;
            if (mentalInput == null) { return; }

            UpdateFeedback(GetTrainingValue(mentalInput));
            UpdateCompletionHold(mentalInput.HasValidSignal);
        }

        protected override void Complete()
        {
            _isCompleted = true;
            successCheck.SetActive(true);
            resultText.SetTextId(trainingType == EegTrainingType.Relaxation ? TutorialTextId.RelaxationSuccess : TutorialTextId.ConcentrationSuccess);
            base.Complete();
        }

        /// <summary>Configures the filtered mental input used for training feedback.</summary>
        /// <param name="mental">Filtered source used to read training values.</param>
        public void ConfigureInputSource(IMentalInputSource mental)
        {
            _mentalInput = mental;
        }

        /// <summary>Resets the visual feedback for a new training attempt.</summary>
        private void ResetFeedback()
        {
            fillImage.fillAmount = 0f;
            if (targetMarker != null)
            {
                targetMarker.rectTransform.anchorMin = new Vector2(target, 0.5f);
                targetMarker.rectTransform.anchorMax = new Vector2(target, 0.5f);
            }
            if (targetText != null) { targetText.text = "0%"; }
            successCheck.SetActive(false);
            resultText.SetTextId(TutorialTextId.None);
        }

        /// <summary>Gets the value used by this training screen.</summary>
        /// <param name="mental">Source that provides the training values.</param>
        /// <returns>The relaxation or concentration value selected for this screen.</returns>
        private float GetTrainingValue(IMentalInputSource mental)
        {
            return trainingType == EegTrainingType.Relaxation ? mental.Relaxation : mental.Concentration;
        }

        /// <summary>Updates the bar and its displayed percentage.</summary>
        /// <param name="value">Normalized value shown by the training feedback.</param>
        private void UpdateFeedback(float value)
        {
            fillImage.fillAmount = Mathf.MoveTowards(fillImage.fillAmount, value, Time.deltaTime * 1.5f);
            if (targetText != null)
            {
                targetText.text = $"{fillImage.fillAmount:P0}";
            }
        }

        /// <summary>Completes the screen after holding a valid target value.</summary>
        /// <param name="isValid">Indicates if the current mental signal is valid.</param>
        private void UpdateCompletionHold(bool isValid)
        {
            if (!isValid || fillImage.fillAmount < target)
            {
                _holdStartedAt = -1f;
                return;
            }

            if (_holdStartedAt < 0f)
            {
                _holdStartedAt = Time.unscaledTime;
            }

            if (Time.unscaledTime - _holdStartedAt < holdSeconds) { return; }

            Complete();
        }
    }
}
