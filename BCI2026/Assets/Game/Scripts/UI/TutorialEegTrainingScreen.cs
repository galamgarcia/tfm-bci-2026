using BciGame.Services;
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

        public override float CompletionDelay => 1f;

        /// <summary>
        /// Identifies the EEG metric evaluated by this training screen.
        /// </summary>
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
            successCheck.SetActive(false);
            resultText.SetTextId(TutorialTextId.None);
        }

        private void Update()
        {
            if (_isCompleted) { return; }

            BrainLinkConnection connection = BrainLinkConnection.Instance;
            if (connection == null) { return; }

            float value = trainingType == EegTrainingType.Relaxation ? connection.Relaxation : connection.Concentration;
            fillImage.fillAmount = Mathf.MoveTowards(fillImage.fillAmount, value, Time.deltaTime * 1.5f);

            if (!connection.HasGoodSignal || fillImage.fillAmount < target)
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
