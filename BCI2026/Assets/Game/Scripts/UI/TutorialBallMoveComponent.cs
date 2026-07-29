/*
 * BCI Interaction System for Videogames
 * Master's Thesis · University of Alicante
 * © 2026 Gala M. García
 */

using BciGame.Gameplay;
using UnityEngine;

namespace BciGame.UI
{
    /// <summary>
    /// Converts head-tracking and EEG input into bounded tutorial canvas movement.
    /// </summary>
    [RequireComponent(typeof(TutorialBall))]
    public class TutorialBallMoveComponent : MoveComponent
    {
        [Header("Movement")]
        [Tooltip("Input source that drives the spawned tutorial ball.")]
        [SerializeField] private MovementState movementState;
        [Tooltip("Maximum horizontal speed in canvas units per second for head-driven movement.")]
        [SerializeField] private float headMovementSpeed = 240f;
        [Tooltip("Maximum vertical speed in canvas units per second for EEG-driven movement.")]
        [SerializeField] private float eegMovementSpeed = 170f;

        // Ball on the same GameObject, guaranteed by RequireComponent.
        private TutorialBall _ball;

        private Vector2 _minimumPosition;
        private Vector2 _maximumPosition;

        protected override void Awake()
        {
            base.Awake();
            _ball = GetComponent<TutorialBall>();
        }

        /// <summary>
        /// Configures the input source, target transform and movement bounds.
        /// </summary>
        /// <param name="state">Input source used to produce movement.</param>
        /// <param name="minimumPosition">Minimum allowed canvas-space position.</param>
        /// <param name="maximumPosition">Maximum allowed canvas-space position.</param>
        public void Configure(MovementState state, Vector2 minimumPosition, Vector2 maximumPosition)
        {
            movementState = state;
            _minimumPosition = minimumPosition;
            _maximumPosition = maximumPosition;
        }

        /// <summary>
        /// Reads the configured input source and delegates its movement to the specialized method.
        /// </summary>
        private void Update()
        {
            if (_ball == null) { return; }
            switch (movementState)
            {
                case MovementState.HeadYaw when TryGetHeadInput(out float headInput):
                    MoveFromHead(headInput);
                    break;
                case MovementState.RelaxationUp when TryGetRelaxationInput(out float relaxation):
                    MoveFromEeg(relaxation, 1f);
                    break;
                case MovementState.ConcentrationDown when TryGetConcentrationInput(out float concentration):
                    MoveFromEeg(concentration, -1f);
                    break;
            }
        }

        protected virtual void MoveFromHead(float input)
        {
            Move(new Vector2(input * headMovementSpeed * Time.deltaTime, 0f));
        }

       protected virtual void MoveFromEeg(float value, float direction)
        {
            Move(new Vector2(0f, value * direction * eegMovementSpeed * Time.deltaTime));
        }

        protected override void Move(Vector2 delta)
        {
            Vector2 position = _ball.Position + delta;
            _ball.Position = new Vector2(Mathf.Clamp(position.x, _minimumPosition.x, _maximumPosition.x), Mathf.Clamp(position.y, _minimumPosition.y, _maximumPosition.y));
        }
    }
}
