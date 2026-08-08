/*
 * BCI Interaction System for Videogames
 * Master's Thesis · University of Alicante
 * © 2026 Gala M. García
 */

using System.Collections;
using Game.Scripts.Gameplay;
using UnityEngine;

namespace BciGame.UI
{
    /// <summary>
    /// Runs the horizontal head-control exercise and its two EEG-state rounds.
    /// </summary>
    public sealed class TutorialMovementScreen : TutorialScreen
    {
        [Header("Prefabs")]
        [Tooltip("Ball prefab spawned for this movement exercise.")]
        [SerializeField] private TutorialBall ballPrefab;
        [Tooltip("Goal prefab spawned for this movement exercise.")]
        [SerializeField] private TutorialGoal goalPrefab;
        [Tooltip("UI container that receives the spawned ball and goal instances.")]
        [SerializeField] private RectTransform exerciseContainer;

        [Header("Movement")]
        [Tooltip("Minimum position for the spawned ball.")]
        [SerializeField] private Vector2 minBallPosition;
        [Tooltip("Maximum position for the spawned ball.")]
        [SerializeField] private Vector2 maxBallPosition;

        [Header("Initial Positions")]
        [Tooltip("Ball position at the start of the exercise.")]
        [SerializeField] private Vector2 initialBallPosition;
        [Tooltip("Goal position at the start of the exercise.")]
        [SerializeField] private Vector2 initialGoalPosition;

        [Header("Feedback")]
        [Tooltip("Feedback object shown after the first goal is reached.")]
        [SerializeField] private GameObject successParticles;
        [Tooltip("Instruction text updated for each exercise round.")]
        [SerializeField] private TutorialText instructionText;

        private TutorialBall _ball;
        private TutorialGoal _goal;
        // Current exercise phase: neutral, first mental-state target, then opposite target.
        private int _round;
        // Mental-state target selected for the first EEG-dependent round.
        private MentalStateLevel _firstMentalState;

        public override float CompletionDelay => 1f;

        public override void Activate()
        {
            _round = 0;
            SpawnExerciseObjects();
            StartRound(initialBallPosition, initialGoalPosition, MentalStateLevel.None, TutorialTextId.MovementInitialInstruction);
        }

        public override void Deactivate()
        {
            DespawnExerciseObjects();
        }

        /// <summary>Advances the exercise after the active goal is reached.</summary>
        private void OnGoalTriggered()
        {
            if (_round == 0)
            {
                _round = 1;
                _firstMentalState = _ball.GetConcentrationLevel() >= MentalStateLevel.Medium ? MentalStateLevel.Low : MentalStateLevel.Medium;
                StartMentalRound(_firstMentalState, -initialGoalPosition.x);
                ShowSuccessParticles();
                return;
            }

            if (_round == 1)
            {
                _round = 2;
                MentalStateLevel finalState = _firstMentalState == MentalStateLevel.Medium ? MentalStateLevel.Low : MentalStateLevel.Medium;
                StartMentalRound(finalState, initialGoalPosition.x);
                ShowSuccessParticles();
                return;
            }

            Complete();
        }

        /// <summary>Starts an EEG-dependent round with its goal on the requested horizontal side.</summary>
        /// <param name="requiredState">Concentration level required to complete the round.</param>
        /// <param name="goalX">Horizontal canvas position of the next goal.</param>
        private void StartMentalRound(MentalStateLevel requiredState, float goalX)
        {
            TutorialTextId instructionId = requiredState == MentalStateLevel.Medium ? TutorialTextId.MovementFocusInstruction : TutorialTextId.MovementDefocusInstruction;
            StartRound(_ball.Position, new Vector2(goalX, initialGoalPosition.y), requiredState, instructionId);
        }

        /// <summary>Configures the ball, goal, color feedback and instruction for one exercise round.</summary>
        /// <param name="ballPosition">Ball position at the start of the round.</param>
        /// <param name="goalPosition">Goal position required to complete the round.</param>
        /// <param name="requiredState">Concentration level required by the goal.</param>
        /// <param name="instructionId">Identifier of the instruction shown for the round.</param>
        private void StartRound(Vector2 ballPosition, Vector2 goalPosition, MentalStateLevel requiredState, TutorialTextId instructionId)
        {
            _ball.Position = ballPosition;
            if (requiredState == MentalStateLevel.None)
            {
                _ball.SetConcentrationColorEnabled(false);
            }
            else
            {
                _ball.SetConcentrationColorEnabled(true);
            }
            _goal.Position = goalPosition;
            _goal.Configure(_ball, requiredState);
            if (instructionText != null)
            {
                instructionText.SetTextId(instructionId);
            }
        }

        /// <summary>Instantiates the ball and goal prefabs for the current exercise attempt.</summary>
        private void SpawnExerciseObjects()
        {
            if (_ball != null || ballPrefab == null || goalPrefab == null) { return; }
            Transform parent = exerciseContainer == null ? transform : exerciseContainer;
            _ball = Instantiate(ballPrefab, parent);
            _goal = Instantiate(goalPrefab, parent);
            _goal.OnTriggered += OnGoalTriggered;
            _ball.Configure(minBallPosition, maxBallPosition);
        }

        /// <summary>Unsubscribes from and destroys the spawned exercise objects.</summary>
        private void DespawnExerciseObjects()
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

        /// <summary>Shows the intermediate success feedback briefly.</summary>
        private void ShowSuccessParticles()
        {
            if (successParticles == null) { return; }
            successParticles.SetActive(true);
            StartCoroutine(HideParticlesAfter(0.4f));
        }

        /// <summary>Hides success feedback after a real-time delay.</summary>
        /// <param name="delay">Seconds to keep the feedback visible.</param>
        /// <returns>Coroutine that waits for the requested delay.</returns>
        private IEnumerator HideParticlesAfter(float delay)
        {
            yield return new WaitForSecondsRealtime(delay);
            successParticles.SetActive(false);
        }
    }
}
