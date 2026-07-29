/*
 * BCI Interaction System for Videogames
 * Master's Thesis · University of Alicante
 * © 2026 Gala M. García
 */

using System;
using UnityEngine;

namespace BciGame.UI
{
    /// <summary>
    /// Detects when a tracked tutorial ball enters its configured success radius.
    /// </summary>
    [RequireComponent(typeof(RectTransform))]
    public sealed class TutorialGoal : MonoBehaviour
    {
        [SerializeField] private RectTransform initialGoalTransform;
        [SerializeField] private float successRadius = 52f;

        private TutorialBall _ball;
        private bool _isEnabled;

        // Raised once when the tracked ball enters the success radius.
        public event Action Reached;

        // Gets or sets the goal position in its parent canvas space.
        public Vector2 Position
        {
            get => initialGoalTransform.anchoredPosition;
            set => initialGoalTransform.anchoredPosition = value;
        }

        private void Awake()
        {
            if (initialGoalTransform == null)
            {
                initialGoalTransform = GetComponent<RectTransform>();
            }
        }

        private void Update()
        {
            if (_isEnabled || _ball == null || Vector2.Distance(_ball.Position, Position) > successRadius) { return; }
            _isEnabled = true;
            Reached?.Invoke();
        }

        /// <summary>
        /// Begins tracking a ball for the current exercise attempt.
        /// </summary>
        /// <param name="ball">Ball that must enter this goal.</param>
        public void Track(TutorialBall ball)
        {
            _ball = ball;
            _isEnabled = false;
        }
    }
}
