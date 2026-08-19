/*
 * BCI Interaction System for Videogames
 * Master's Thesis · University of Alicante
 * © 2026 Gala M. García
 */

using BciGame.Services;
using BciGame.Input;
using UnityEngine;
using UnityEngine.UI;

namespace BciGame.UI
{
    /// <summary>
    /// Runs one EEG training step and reports completion after a sustained target value.
    /// </summary>
    public sealed class TutorialEegTrainingScreen : TutorialScreen
    {
        [Header("Training")]
        [Tooltip("EEG metric evaluated by this training screen.")]
        [SerializeField] private EegTrainingType trainingType;

        [Header("Feedback")]
        [Tooltip("Fill image that visualizes the current normalized EEG value.")]
        [SerializeField] private Image fillImage;
        [Tooltip("Vertical marker that identifies the configured completion target on the rail.")]
        [SerializeField] private Image targetMarker;
        [Tooltip("Centered text shown below the rail with the current filtered percentage.")]
        [SerializeField] private Text targetText;
        [Tooltip("Visual confirmation shown after the training target is completed.")]
        [SerializeField] private GameObject successCheck;
        [Tooltip("Text used to display the localized success message.")]
        [SerializeField] private TutorialText resultText;

        [Header("Completion")]
        [Tooltip("Minimum normalized EEG value required to begin the completion hold.")]
        [SerializeField] private float target = 0.7f;
        [Tooltip("Seconds the EEG value must remain above the target to complete the step.")]
        [SerializeField] private float holdSeconds = 2f;
        // Time at which the current valid target hold began.
        private float _holdStartedAt = -1f;
        // Prevents the completion event from firing more than once per screen activation.
        private bool _isCompleted;
        private IMentalInputSource _mentalInputSource;

        public override float CompletionDelay => 1f;

        /// <summary>Configures the filtered mental input used for training feedback.</summary>
        public void ConfigureInputSource(IMentalInputSource mentalInputSource)
        {
            _mentalInputSource = mentalInputSource;
        }

        /// <summary>Identifies the EEG metric evaluated by this training screen.</summary>
        private enum EegTrainingType
        {
            Relaxation,
            Concentration
        }

        public override void Activate()
        {
            _isCompleted = false;
            _holdStartedAt = -1f;
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

        private void Update()
        {
            if (_isCompleted) { return; }

            IMentalInputSource mentalInput = _mentalInputSource ?? FilteredMentalInputSource.Instance;
            if (mentalInput == null) { return; }

            float value = trainingType == EegTrainingType.Relaxation ? mentalInput.Relaxation : mentalInput.Concentration;
            fillImage.fillAmount = Mathf.MoveTowards(fillImage.fillAmount, value, Time.deltaTime * 1.5f);
            if (targetText != null)
            {
                targetText.text = $"{fillImage.fillAmount:P0}";
            }

            if (!mentalInput.HasValidSignal || fillImage.fillAmount < target)
            {
                _holdStartedAt = -1f;
                return;
            }

            if (_holdStartedAt < 0f)
            {
                _holdStartedAt = Time.unscaledTime;
            }

            if (Time.unscaledTime - _holdStartedAt < holdSeconds) { return; }

            _isCompleted = true;
            successCheck.SetActive(true);
            resultText.SetTextId(trainingType == EegTrainingType.Relaxation ? TutorialTextId.RelaxationSuccess : TutorialTextId.ConcentrationSuccess);
            Complete();
        }
    }
}
