/*
 * BCI Interaction System for Videogames
 * Master's Thesis · University of Alicante
 * © 2026 Gala M. García
 */

using System.Collections;
using BciGame.Core;
using BciGame.Input;
using BciGame.Input.Signals;
using BciGame.UI.Tutorial;
using BciGame.Utilities;
using UnityEngine;

namespace BciGame.Gameplay.Tutorial
{
    /// <summary>Coordinates the tutorial screens.</summary>
    public sealed class TutorialController : MonoBehaviour
    {
        // Shared timings and thresholds used to keep every exercise consistent.
        private const float FadeDuration = 0.5f;

        [Header("Flow")]
        [Tooltip("Tutorial screens in their display order.")]
        [SerializeField] private TutorialScreen[] screens;
        [Header("Flow")]
        [Tooltip("Head tracking service.")]
        [SerializeField] private HeadPoseTracker headPoseTracker;

        [Header("Interaction")]
        [Tooltip("Text updated during headset confirmation.")]
        [SerializeField] private TutorialText nodHintText;
        [Header("Interaction")]
        [Tooltip("Indicator displayed after a valid nod gesture.")]
        [SerializeField] private GameObject nodCheck;

        [Header("EEG Status")]
        [Tooltip("Warning panel shown when EEG-dependent exercises lose signal quality.")]
        [SerializeField] private CanvasGroup eegWarning;

        [Header("EEG Status")]
        [Tooltip("VF displayed on the final tutorial screen.")]
        [SerializeField] private GameObject completeVfx;

        // Index of the active tutorial screen.
        private int _currentScreenIndex;
        // Determines whether a screen transition is in progress.
        private bool _isTransitioning;
        /// Gets the type of the active tutorial screen.
        private TutorialScreenType CurrentScreenType => screens[_currentScreenIndex].ScreenType;

        private void OnEnable()
        {
            if (headPoseTracker != null)
            {
                headPoseTracker.NodDetected += OnNodDetected;
            }

            foreach (TutorialScreen screen in screens)
            {
                screen.OnComplete += OnScreenCompleted;
            }
        }

        private void OnDisable()
        {
            if (headPoseTracker != null)
            {
                headPoseTracker.NodDetected -= OnNodDetected;
            }

            foreach (TutorialScreen screen in screens)
            {
                screen.OnComplete -= OnScreenCompleted;
            }
        }

        private void Update()
        {
            eegWarning.alpha = IsEegTraining(CurrentScreenType) && !Utils.IsBrainLinkConnectionGood() ? 1f : 0f;
        }

        /// <summary>Shows only the first tutorial screen at startup.</summary>
        private void InitializeScreens()
        {
            for (int i = 0; i < screens.Length; i++)
            {
                SetScreenVisible(screens[i], i == 0, i == 0 ? 1f : 0f);
            }
        }

        private void Start()
        {
            InitializeScreens();
            HideFeedback();
            ActivateScreen(0);
        }

        /// <summary>Advances to the next screen when no transition is already in progress.</summary>
        public void Continue()
        {
            if (!_isTransitioning)
            {
                StartCoroutine(TransitionTo(_currentScreenIndex + 1));
            }
        }

        /// <summary>Starts the practice sequence from its introduction screen.</summary>
        public void BeginPractice()
        {
            Continue();
        }

        /// <summary>Marks the tutorial as complete and provides the handoff point to the game experience.</summary>
        public void StartExperience()
        {
            Debug.Log("Tutorial completed. The game experience can start here.");
        }

        /// <summary>Resets the visual state and hardware baseline required by a screen.</summary>
        /// <param name="index">Zero-based navigation index of the screen being activated.</param>
        private void ActivateScreen(int index)
        {
            _currentScreenIndex = index;
            TutorialScreenType screenType = screens[index].ScreenType;
            switch (screenType)
            {
                case TutorialScreenType.HeadsetConfirmation:
                    PrepareHeadsetConfirmation();
                    break;
                case TutorialScreenType.Movement:
                    PrepareMovementScreen(index);
                    break;
                case TutorialScreenType.Relaxation:
                case TutorialScreenType.Concentration:
                    PrepareEegTrainingScreen(index);
                    break;
                case TutorialScreenType.Complete:
                    completeVfx.SetActive(true);
                    break;
                default: break;
            }
        }

        /// <summary>Hides feedback that is shown only during specific tutorial steps.</summary>
        private void HideFeedback()
        {
            eegWarning.alpha = 0f;
            eegWarning.blocksRaycasts = false;
            nodCheck.SetActive(false);
            completeVfx.SetActive(false);
        }

        /// <summary>Prepares nod confirmation and starts head calibration.</summary>
        private void PrepareHeadsetConfirmation()
        {
            nodHintText.SetTextId(TutorialTextId.HeadsetInstruction);
            nodCheck.SetActive(false);
            headPoseTracker.BeginCalibration();
        }

        /// <summary>Provides movement inputs to the active movement screen.</summary>
        private void PrepareMovementScreen(int index)
        {
            ((TutorialMovementScreen)screens[index]).ConfigureInput(headPoseTracker, FilteredMentalInputSource.Instance);
        }

        /// <summary>Provides filtered mental input to the active EEG training screen.</summary>
        private void PrepareEegTrainingScreen(int index)
        {
            ((TutorialEegTrainingScreen)screens[index]).ConfigureInputSource(FilteredMentalInputSource.Instance);
        }

        /// <summary>Completes the headset confirmation when a valid nod is received.</summary>
        private void OnNodDetected()
        {
            // The headset confirmation is the only non-button screen completed by a nod.
            if (CurrentScreenType != TutorialScreenType.HeadsetConfirmation || _isTransitioning) { return; }
            nodHintText.SetTextId(TutorialTextId.HeadsetSuccess);
            nodCheck.SetActive(true);
            StartCoroutine(AdvanceAfter(0.8f));
        }

        /// <summary>Advances after the active screen reports completion through its shared lifecycle event.</summary>
        private void OnScreenCompleted()
        {
            if (!_isTransitioning)
            {
                StartCoroutine(AdvanceAfter(screens[_currentScreenIndex].CompletionDelay));
            }
        }

        /// <summary>Delays progression so success feedback remains visible before changing screen.</summary>
        /// <param name="delay">Unscaled delay in seconds before the transition begins.</param>
        /// <returns>Coroutine that waits before advancing the tutorial.</returns>
        private IEnumerator AdvanceAfter(float delay)
        {
            // Lock input while success feedback remains visible before fading out.
            _isTransitioning = true;
            yield return new WaitForSecondsRealtime(delay);
            yield return TransitionTo(_currentScreenIndex + 1);
        }

        /// <summary>Fades from the current screen to the requested next screen.</summary>
        /// <param name="index">Zero-based index of the screen to reveal.</param>
        /// <returns>Coroutine that completes the screen transition.</returns>
        private IEnumerator TransitionTo(int index)
        {
            if (index >= screens.Length)
            {
                _isTransitioning = false;
                yield break;
            }

            _isTransitioning = true;
            TutorialScreen current = screens[_currentScreenIndex];
            TutorialScreen next = screens[index];
            ActivateScreen(index);
            SetScreenVisible(next, true, 0f);
            yield return FadeScreens(current, next);

            SetScreenVisible(current, false, 0f);
            SetScreenVisible(next, true, 1f);
            _isTransitioning = false;
        }

        /// <summary>Fades between two visible screens without showing an empty canvas.</summary>
        /// <param name="current">Screen that fades out.</param>
        /// <param name="next">Screen that fades in.</param>
        /// <returns>Coroutine that completes the screen fade.</returns>
        private static IEnumerator FadeScreens(TutorialScreen current, TutorialScreen next)
        {
            float elapsed = 0f;
            while (elapsed < FadeDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                float progress = Mathf.SmoothStep(0f, 1f, elapsed / FadeDuration);
                current.CanvasGroup.alpha = 1f - progress;
                next.CanvasGroup.alpha = progress;
                yield return null;
            }
        }

        /// <summary>Determines if the screen requires a valid EEG signal to progress.</summary>
        /// <param name="type">Tutorial screen type to evaluate.</param>
        /// <returns>True, the screen is an EEG-dependent training exercise.</returns>
        private static bool IsEegTraining(TutorialScreenType type)
        {
            return type is TutorialScreenType.Relaxation
                or TutorialScreenType.Concentration
                or TutorialScreenType.Movement;
        }

        /// <summary>Sets a screen visibility, opacity and interaction state.</summary>
        /// <param name="screen">Screen whose CanvasGroup and GameObject are updated.</param>
        /// <param name="visible">Only if the screen is active and can receive interactions.</param>
        /// <param name="alpha">Canvas opacity applied to the screen.</param>
        private static void SetScreenVisible(TutorialScreen screen, bool visible, float alpha)
        {
            if (!visible)
            {
                screen.Deactivate();
            }

            screen.gameObject.SetActive(visible);
            screen.CanvasGroup.alpha = alpha;
            screen.CanvasGroup.blocksRaycasts = visible;
            screen.CanvasGroup.interactable = visible;

            if (visible)
            {
                screen.Activate();
            }
        }
    }
}
