/*
 * BCI Interaction System for Videogames
 * Master's Thesis · University of Alicante
 * © 2026 Gala M. García
 */

using System;
using BciGame.Core;
using BciGame.Gameplay;
using BciGame.UI;
using UnityEngine;

namespace BciGame.Gameplay
{
    /// <summary>Represents the visual target for a tutorial ball and triggers when the ball reaches it with the concentration level required by the current exercise round. </summary>
    [RequireComponent(typeof(RectTransform))]
    public sealed class TutorialGoal : CanvasImage
    {
        [Header("Goal")]
        [Tooltip("Distance between the ball and goal required for success.")]
        [SerializeField] private float radius = 52f;

        // Ball active in this exercise round.
        private TutorialBall _ball;
        // Mental-state level required to trigger this goal.
        private MentalStateLevel _requiredLevel;
        // Indicates if the goal is complete.
        private bool _isTriggered;

        /// <summary>Raised once when the ball meets the position and concentration requirements.</summary>
        public event Action OnTriggered;

        protected override void Awake()
        {
            base.Awake();
        }

        private void Update()
        {
            if (!CanBeTriggered()) { return; }

            _isTriggered = true;
            OnTriggered?.Invoke();
        }

        /// <summary>Checks if the ball meets this goal's position and signal requirements.</summary>
        /// <returns>True, this goal can be triggered.</returns>
        private bool CanBeTriggered()
        {
            return !_isTriggered && _ball != null && TutorialRules.CanTriggerGoal(_ball.GetPosition(), GetPosition(), radius, _requiredLevel, _ball.GetConcentrationLevel(), _ball.HasValidEegSignal());
        }

        /// <summary>Configures the ball and concentration level required for the current round.</summary>
        /// <param name="ball">Ball that must enter this goal.</param>
        /// <param name="level">Concentration level required to trigger the goal.</param>
        public void Configure(TutorialBall ball, MentalStateLevel level)
        {
            _ball = ball;
            _requiredLevel = level;
            _isTriggered = false;
            if (TutorialSettings.Instance != null)
            {
                SetColor(TutorialSettings.Instance.GetColor(level));
            }
        }

    }
}
