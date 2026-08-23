/*
 * BCI Interaction System for Videogames
 * Master's Thesis · University of Alicante
 * © 2026 Gala M. García
 */

using System.Collections;
using BciGame.Core;
using BciGame.Gameplay;
using BciGame.Input;
using BciGame.Input.Signals;
using BciGame.UI.Tutorial;
using UnityEngine;

namespace BciGame.Gameplay.Tutorial
{
    /// <summary>Runs the horizontal head-control exercise and its two EEG-state rounds.</summary>
    public sealed class TutorialMovementScreen : TutorialScreen
    {
        [Header("Prefabs")]
        [Tooltip("Ball prefab spawned for this movement exercise.")]
        [SerializeField] private TutorialBall ballPrefab;
        [Header("Prefabs")]
        [Tooltip("Goal prefab spawned for this movement exercise.")]
        [SerializeField] private TutorialGoal goalPrefab;
        [Header("Prefabs")]
        [Tooltip("UI container that receives the spawned ball and goal instances.")]
        [SerializeField] private RectTransform parentUI;

        [Header("Movement")]
        [Tooltip("Minimum position for the spawned ball.")]
        [SerializeField] private Vector2 minBallPosition;
        [Header("Movement")]
        [Tooltip("Maximum position for the spawned ball.")]
        [SerializeField] private Vector2 maxBallPosition;

        [Header("Initial Positions")]
        [Tooltip("Ball position at the start of the exercise.")]
        [SerializeField] private Vector2 initialBallPosition;
        [Header("Initial Positions")]
        [Tooltip("Goal position at the start of the exercise.")]
        [SerializeField] private Vector2 initialGoalPosition;

        [Header("Feedback")]
        [Tooltip("Feedback object shown after the first goal is reached.")]
        [SerializeField] private GameObject successVfx;
        [Header("Feedback")]
        [Tooltip("Instruction text updated for each exercise round.")]
        [SerializeField] private TutorialText instructionText;

        // Ball active in this exercise round.
        private TutorialBall _ball;
        // Goal active in this exercise round.
        private TutorialGoal _goal;
        // Configured input sources.
        private IHeadInputSource _headInput;
        private IMentalInputSource _mentalInput;
        // Current exercise phase: neutral, first mental-state target, then opposite target.
        private int _currentRound;
        // Mental-state target selected for the first EEG-dependent round.
        private MentalStateLevel _firstMentalLevel;

        public override float CompletionDelay => 1f;

        public override void Activate()
        {
            _currentRound = 0;
            if (_ball != null || ballPrefab == null || goalPrefab == null) { return; }

            Transform parent = parentUI == null ? transform : parentUI;
            _ball = Instantiate(ballPrefab, parent);
            _goal = Instantiate(goalPrefab, parent);
            _ball.transform.SetAsLastSibling();
            _goal.OnTriggered += OnGoalTriggered;
            _ball.Configure(minBallPosition, maxBallPosition);
            _ball.SetInput(_headInput, _mentalInput);

            StartRound(initialBallPosition, initialGoalPosition, MentalStateLevel.None, TutorialTextId.MovementInitialInstruction);
        }

        public override void Deactivate()
        {
            if (_goal != null)
            {
                _goal.OnTriggered -= OnGoalTriggered;
                Destroy(_goal.gameObject);
                _goal = null;
            }

            if (_ball != null)
            {
                Destroy(_ball.gameObject);
                _ball = null;
            }
        }

        /// <summary>Configures the input sources used by dynamically spawned exercise objects.</summary>
        /// <param name="headInput">Source that provides head movement and nod gestures.</param>
        /// <param name="mentalInput">Source that provides EEG samples and signal quality.</param>
        public void ConfigureInput(IHeadInputSource headInput, IMentalInputSource mentalInput)
        {
            _headInput = headInput;
            _mentalInput = mentalInput;
            if (_ball != null)
            {
                _ball.SetInput(_headInput, _mentalInput);
            }
        }

        /// <summary>Advances the exercise after the active goal is reached.</summary>
        private void OnGoalTriggered()
        {
            if (_currentRound == 0)
            {
                StartFirstMentalRound();
            }
            else if (_currentRound == 1)
            {
                StartSecondMentalRound();
                return;
            }
            else
            {
                Complete();
            }
        }

        /// <summary>Starts the first round that requires a mental-state level.</summary>
        private void StartFirstMentalRound()
        {
            _currentRound = 1;
            _firstMentalLevel = TutorialRules.GetFirstMentalMovementRoundRequirement(_ball.GetConcentrationLevel());
            StartMentalRound(_firstMentalLevel, -initialGoalPosition.x);
            PlayVfx();
        }

        /// <summary>Starts the second round with the alternate mental-state level.</summary>
        private void StartSecondMentalRound()
        {
            _currentRound = 2;
            MentalStateLevel next = TutorialRules.GetOppositeMentalMovementRoundRequirement(_firstMentalLevel);
            StartMentalRound(next, initialGoalPosition.x);
            PlayVfx();
        }

        /// <summary>Starts an EEG-dependent round with its goal on the requested horizontal side.</summary>
        /// <param name="state">Concentration level required to complete the round.</param>
        /// <param name="goalX">Horizontal canvas position of the next goal.</param>
        private void StartMentalRound(MentalStateLevel state, float goalX)
        {
            TutorialTextId textId = state == MentalStateLevel.Medium ? TutorialTextId.MovementFocusInstruction : TutorialTextId.MovementDefocusInstruction;
            StartRound(_ball.GetPosition(), new Vector2(goalX, initialGoalPosition.y), state, textId);
        }

        /// <summary>Configures the ball, goal, color feedback and instruction for one exercise round.</summary>
        /// <param name="ballPosition">Ball position at the start of the round.</param>
        /// <param name="goalPosition">Goal position required to complete the round.</param>
        /// <param name="state">Concentration level required by the goal.</param>
        /// <param name="textId">Identifier of the instruction shown for the round.</param>
        private void StartRound(Vector2 ballPosition, Vector2 goalPosition, MentalStateLevel state, TutorialTextId textId)
        {
            _ball.SetPosition(ballPosition);
            if (state == MentalStateLevel.None)
            {
                _ball.SetConcentrationColor(false);
            }
            else
            {
                _ball.SetConcentrationColor(true);
            }
            _goal.SetPosition(goalPosition);
            _goal.Configure(_ball, state);
            if (instructionText != null)
            {
                instructionText.SetTextId(textId);
            }
        }


        /// <summary>Shows the intermediate success feedback briefly.</summary>
        private void  PlayVfx()
        {
            if (successVfx == null) { return; }
            successVfx.SetActive(true);
            StartCoroutine(StopVfx(0.4f));
        }

        /// <summary>Hides success feedback after a real-time delay.</summary>
        /// <param name="delay">Seconds to keep the feedback visible.</param>
        /// <returns>Coroutine that waits for the requested delay.</returns>
        private IEnumerator StopVfx(float delay)
        {
            yield return new WaitForSecondsRealtime(delay);
            successVfx.SetActive(false);
        }
    }
}
