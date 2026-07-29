/*
 * BCI Interaction System for Videogames
 * Master's Thesis · University of Alicante
 * © 2026 Gala M. García
 */

using System.Collections;
using UnityEngine;

namespace BciGame.UI
{
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
        [Tooltip("Input source used to move the spawned ball.")]
        [SerializeField] private MovementState movementState;
        [Tooltip("Minimum canvas-space position allowed for the spawned ball.")]
        [SerializeField] private Vector2 minimumBallPosition;
        [Tooltip("Maximum canvas-space position allowed for the spawned ball.")]
        [SerializeField] private Vector2 maximumBallPosition;

        [Header("Initial Positions")]
        [Tooltip("Canvas-space position applied to the ball at the start of the exercise.")]
        [SerializeField] private Vector2 initialBallPosition;
        [Tooltip("Canvas-space position applied to the goal at the start of the exercise.")]
        [SerializeField] private Vector2 initialGoalPosition;

        [Header("Optional Second Round")]
        [Tooltip("Whether the screen requires the player to reach a mirrored goal after the first success.")]
        [SerializeField] private bool requiresSecondRound;
        [Tooltip("Feedback object shown after the first goal is reached.")]
        [SerializeField] private GameObject successParticles;
        [Tooltip("Instruction label updated when the optional second round begins.")]
        [SerializeField] private UnityEngine.UI.Text instructionLabel;

        private TutorialBall _ball;
        private TutorialGoal _goal;
        private int _round;

        public override float CompletionDelay => 1f;

        public override void Activate()
        {
            _round = 0;
            SpawnExerciseObjects();
        }

        public override void Deactivate()
        {
            DespawnExerciseObjects();
        }

        /// <summary>
        /// Resets the ball and goal for a new exercise attempt.
        /// </summary>
        /// <param name="ballPosition">Initial ball position in canvas space.</param>
        /// <param name="goalPosition">Target goal position in canvas space.</param>
        public void ResetExercise(Vector2 ballPosition, Vector2 goalPosition)
        {
            initialBallPosition = ballPosition;
            initialGoalPosition = goalPosition;

            if (_ball == null || _goal == null) { return; }
            _ball.Position = ballPosition;
            _goal.Position = goalPosition;
            _goal.Track(_ball);
        }

        /// <summary>
        /// Advances the two-round head exercise or completes the current movement screen.
        /// </summary>
        private void HandleGoalReached()
        {
            if (requiresSecondRound && _round == 0)
            {
                _round = 1;
                ResetExercise(initialBallPosition, new Vector2(-initialGoalPosition.x, initialGoalPosition.y));
                if (instructionLabel != null)
                {
                    instructionLabel.text = "Una vez más, hacia el otro lado.";
                }

                if (successParticles != null)
                {
                    successParticles.SetActive(true);
                    StartCoroutine(HideParticlesAfter(0.4f));
                }

                return;
            }

            Complete();
        }

        /// <summary>
        /// Instantiates the ball and goal prefabs for the current exercise attempt.
        /// </summary>
        private void SpawnExerciseObjects()
        {
            if (_ball != null || ballPrefab == null || goalPrefab == null) { return; }
            Transform parent = exerciseContainer == null ? transform : exerciseContainer;
            _ball = Instantiate(ballPrefab, parent);
            _goal = Instantiate(goalPrefab, parent);
            _goal.Reached += HandleGoalReached;
            ResetExercise(initialBallPosition, initialGoalPosition);
            _ball.GetMoveComponent().Configure(movementState, minimumBallPosition, maximumBallPosition);
        }

        /// <summary>
        /// Unsubscribes from and destroys the spawned exercise objects.
        /// </summary>
        private void DespawnExerciseObjects()
        {
            if (_goal != null)
            {
                _goal.Reached -= HandleGoalReached;
                Destroy(_goal.gameObject);
                _goal = null;
            }

            if (_ball != null)
            {
                Destroy(_ball.gameObject);
                _ball = null;
            }
        }

        private IEnumerator HideParticlesAfter(float delay)
        {
            yield return new WaitForSecondsRealtime(delay);
            successParticles.SetActive(false);
        }
    }
}
