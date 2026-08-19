/*
 * BCI Interaction System for Videogames
 * Master's Thesis · University of Alicante
 * © 2026 Gala M. García
 */

using System;
using BciGame.Core;
using Game.Scripts.Gameplay;
using UnityEngine;
using UnityEngine.UI;

namespace BciGame.Gameplay
{
    /// <summary>
    /// Represents the visual target for a tutorial ball and triggers when the ball reaches it
    /// with the concentration level required by the current exercise round.
    /// </summary>
    [RequireComponent(typeof(RectTransform))]
    public sealed class TutorialGoal : MonoBehaviour
    {
        [Tooltip("UI transform used to position the goal in canvas space.")]
        [SerializeField] private RectTransform rectTransform;
        [Tooltip("Image colored to communicate the concentration level required by this goal.")]
        [SerializeField] private Image image;
        [Tooltip("Distance between the ball and goal required for success.")]
        [SerializeField] private float radius = 52f;

        // Ball evaluated against this goal during the active exercise round.
        private TutorialBall _ball;
        // Mental-state level required to trigger this goal.
        private MentalStateLevel _requiredConcentrationLevel;
        // Prevents the completion event from firing more than once per configured round.
        private bool _isTriggered;

        /// <summary>Raised once when the ball meets the position and concentration requirements.</summary>
        public event Action OnTriggered;

        /// <summary>Gets or sets the goal position in its parent canvas space.</summary>
        public Vector2 Position
        {
            get => rectTransform.anchoredPosition;
            set => rectTransform.anchoredPosition = value;
        }

        private void Awake()
        {
            rectTransform = GetComponent<RectTransform>();
            image = GetComponent<Image>();
        }

        private void Update()
        {
            if (_isTriggered || _ball == null) { return; }
            if (!TutorialRules.CanTriggerGoal(_ball.Position, Position, radius, _requiredConcentrationLevel, _ball.GetConcentrationLevel(), _ball.HasValidEegSignal())) { return; }

            _isTriggered = true;
            OnTriggered?.Invoke();
        }

        /// <summary>Configures the ball and concentration level required for the current round.</summary>
        /// <param name="ball">Ball that must enter this goal.</param>
        /// <param name="requiredConcentrationLevel">Concentration level required to trigger the goal.</param>
        public void Configure(TutorialBall ball, MentalStateLevel requiredConcentrationLevel)
        {
            _ball = ball;
            _requiredConcentrationLevel = requiredConcentrationLevel;
            _isTriggered = false;
            if (image != null && TutorialSettings.Instance != null)
            {
                image.color = TutorialSettings.Instance.GetColor(requiredConcentrationLevel);
            }
        }

    }
}
