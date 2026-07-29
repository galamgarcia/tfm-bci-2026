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
    public class TutorialMoveComponent : MoveComponent
    {
        [Header("Movement")]
        [Tooltip("Input source that drives the spawned tutorial ball.")]
        [SerializeField] private MovementState movementState;
        [Tooltip("Maximum horizontal speed in canvas units per second for head-driven movement.")]
        [SerializeField] private float headMovementSpeed = 240f;
        [Tooltip("Maximum vertical speed in canvas units per second for EEG-driven movement.")]
        [SerializeField] private float eegMovementSpeed = 170f;

        // Ball supplied by the screen that spawns this movement component.
        private TutorialBall _ball;

        /// <summary>
        /// Configures the input source, target transform and movement bounds.
        /// </summary>
        /// <param name="state">Input source used to produce movement.</param>
        /// <param name="ball">Tutorial ball moved by this component.</param>
        /// <param name="minimumPosition">Minimum allowed canvas-space position.</param>
        /// <param name="maximumPosition">Maximum allowed canvas-space position.</param>
        public void Configure(MovementState state, TutorialBall ball, Vector2 minimumPosition, Vector2 maximumPosition)
        {
            movementState = state;
            _ball = ball;
            _ball.ConfigureBounds(minimumPosition, maximumPosition);
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

        /// <summary>
        /// Converts horizontal head input into tutorial canvas movement.
        /// </summary>
        /// <param name="input">Normalized head yaw input.</param>
        protected virtual void MoveFromHead(float input)
        {
            _ball.Move(new Vector2(input * headMovementSpeed * Time.deltaTime, 0f));
        }

        /// <summary>
        /// Converts an EEG input into vertical tutorial canvas movement.
        /// </summary>
        /// <param name="value">Normalized EEG input value.</param>
        /// <param name="direction">Positive or negative vertical movement direction.</param>
        protected virtual void MoveFromEeg(float value, float direction)
        {
            _ball.Move(new Vector2(0f, value * direction * eegMovementSpeed * Time.deltaTime));
        }
    }
}
