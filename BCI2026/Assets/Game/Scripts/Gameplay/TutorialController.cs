/*
 * BCI Interaction System for Videogames
 * Master's Thesis · University of Alicante
 * © 2026 Gala M. García
 */

using System.Collections;
using BciGame.Input;
using BciGame.UI;
using BciGame.Utilities;
using UnityEngine;
using UnityEngine.UI;

namespace BciGame.Gameplay
{
    /// <summary>
    /// Coordinates the tutorial screens, hardware-driven exercises and visual feedback.
    /// </summary>
    public sealed class TutorialController : MonoBehaviour
    {
        // Shared timings and thresholds used to keep every exercise consistent.
        private const float FadeDuration = 0.5f;

        [Header("Flow")]
        [Tooltip("Tutorial screens in their display order.")]
        [SerializeField] private TutorialScreen[] screens;
        [Tooltip("Head tracking service used for nod confirmation and horizontal movement practice.")]
        [SerializeField] private HeadPoseTracker headPoseTracker;

        [Header("Interaction")]
        [Tooltip("Instruction text updated during headset confirmation.")]
        [SerializeField] private TutorialText nodHintText;
        [Tooltip("Success indicator displayed after a valid nod gesture.")]
        [SerializeField] private GameObject nodCheck;

        [Header("EEG Status")]
        [Tooltip("Persistent icon that communicates current EEG signal quality.")]
        [SerializeField] private Image eegIcon;
        [Tooltip("Warning panel shown when EEG-dependent exercises lose signal quality.")]
        [SerializeField] private CanvasGroup eegWarning;

        [Tooltip("Subtle confetti displayed on the final tutorial screen.")]
        [SerializeField] private GameObject completeVFX;

        // Runtime state for the active tutorial step and in-progress transitions.
        private int _currentScreenIndex;
        private bool _isTransitioning;
        private TutorialScreenType CurrentScreenType => screens[_currentScreenIndex].ScreenType;

        private void Awake()
        {
            Screen.orientation = ScreenOrientation.LandscapeLeft;
            Application.targetFrameRate = 60;

        }

        private void Start()
        {
            // Only the welcome screen is available at startup; all feedback starts hidden.
            for (int index = 0; index < screens.Length; index++)
            {
                SetScreenVisible(screens[index], index == 0, index == 0 ? 1f : 0f);
            }

            eegWarning.alpha = 0f;
            eegWarning.blocksRaycasts = false;
            nodCheck.SetActive(false);
            completeVFX.SetActive(false);
            ActivateScreen(0);
        }

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
            bool shouldShowStatus = HasEegStatus(CurrentScreenType);
            eegIcon.gameObject.SetActive(shouldShowStatus);
            if (!shouldShowStatus) { return; }

            bool hasSignal = Utils.IsBrainLinkConnectionGood();
            eegIcon.color = hasSignal ? new Color(0.04f, 0.52f, 1f) : new Color(0.45f, 0.47f, 0.5f);
            bool isTraining = IsEegTraining(CurrentScreenType);
            eegWarning.alpha = isTraining && !hasSignal ? 1f : 0f;
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
        /// <param name="screenIndex">Zero-based navigation index of the screen being activated.</param>
        private void ActivateScreen(int screenIndex)
        {
            _currentScreenIndex = screenIndex;
            TutorialScreenType screenType = screens[screenIndex].ScreenType;
            switch (screenType)
            {
                case TutorialScreenType.HeadsetConfirmation:
                    nodHintText.SetTextId(TutorialTextId.HeadsetInstruction);
                    nodCheck.SetActive(false);
                    headPoseTracker.BeginCalibration();
                    break;
                case TutorialScreenType.Complete:
                    completeVFX.SetActive(true);
                    break;
                default: break;
            }
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
        private IEnumerator AdvanceAfter(float delay)
        {
            // Lock input while success feedback remains visible before fading out.
            _isTransitioning = true;
            yield return new WaitForSecondsRealtime(delay);
            yield return TransitionTo(_currentScreenIndex + 1);
        }

        /// <summary>Fades from the current screen to the requested next screen.</summary>
        /// <param name="nextScreen">Zero-based index of the screen to reveal.</param>
        private IEnumerator TransitionTo(int nextScreen)
        {
            if (nextScreen >= screens.Length)
            {
                _isTransitioning = false;
                yield break;
            }

            _isTransitioning = true;
            TutorialScreen current = screens[_currentScreenIndex];
            TutorialScreen next = screens[nextScreen];
            ActivateScreen(nextScreen);
            // Both screens coexist during the fade so the transition does not flash black.
            SetScreenVisible(next, true, 0f);

            float elapsed = 0f;
            while (elapsed < FadeDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                float progress = Mathf.SmoothStep(0f, 1f, elapsed / FadeDuration);
                current.CanvasGroup.alpha = 1f - progress;
                next.CanvasGroup.alpha = progress;
                yield return null;
            }

            SetScreenVisible(current, false, 0f);
            SetScreenVisible(next, true, 1f);
            _isTransitioning = false;
        }

        /// <summary>Determines if the screen should display the EEG signal status icon.</summary>
        /// <param name="screenType">Tutorial screen type to evaluate.</param>
        /// <returns>Whether the EEG status icon should be visible.</returns>
        private static bool HasEegStatus(TutorialScreenType screenType)
        {
            return screenType is TutorialScreenType.EegSignal
                or TutorialScreenType.PracticeIntro
                or TutorialScreenType.Relaxation
                or TutorialScreenType.Concentration
                or TutorialScreenType.Movement;
        }

        /// <summary>Determines if the screen requires a valid EEG signal to progress.</summary>
        /// <param name="screenType">Tutorial screen type to evaluate.</param>
        /// <returns>Whether the screen is an EEG-dependent training exercise.</returns>
        private static bool IsEegTraining(TutorialScreenType screenType)
        {
            return screenType is TutorialScreenType.Relaxation
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
