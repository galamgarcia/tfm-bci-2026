/*
 * BCI Interaction System for Videogames
 * Master's Thesis · University of Alicante
 * © 2026 Gala M. García
 */

using BciGame.Core;
using BciGame.Gameplay;
using Game.Scripts.Gameplay;
using UnityEngine;
using UnityEngine.UI;

namespace BciGame.UI
{
    /// <summary>
    /// Represents the tutorial ball.
    /// </summary>
    [RequireComponent(typeof(RectTransform), typeof(InputComponent))]
    public sealed class TutorialBall : MonoBehaviour
    {
        [SerializeField] private RectTransform initialBallTransform;
        [SerializeField] private Image ballImage;

        private InputComponent _inputComponent;
        private Vector2 _minPosition;
        private Vector2 _maxPosition;
        private MentalStateLevel _concentrationLevel;
        private bool _usesConcentrationColor;

        public Vector2 Position
        {
            get => initialBallTransform.anchoredPosition;
            set => initialBallTransform.anchoredPosition = value;
        }

        private void Awake()
        { 
            initialBallTransform = GetComponent<RectTransform>();
            ballImage = GetComponent<Image>();
            _inputComponent = GetComponent<InputComponent>();
        }

        private void OnEnable()
        {
            _inputComponent.OnHorizontalMovementReceived += OnHorizontalMovementReceived;
            _inputComponent.OnConcentrationChanged += OnConcentrationChanged;
        }

        private void OnDisable()
        {
            _inputComponent.OnHorizontalMovementReceived -= OnHorizontalMovementReceived;
            _inputComponent.OnConcentrationChanged -= OnConcentrationChanged;
        }

        /// <summary>Configures the horizontal bounds and disables concentration color feedback.</summary>
        /// <param name="min">Minimum allowed position in canvas space.</param>
        /// <param name="max">Maximum allowed position in canvas space.</param>
        public void Configure(Vector2 min, Vector2 max)
        {
            _minPosition = min;
            _maxPosition = max;
            SetConcentrationColorEnabled(false);
        }

        /// <summary>Gets the most recent concentration level received from the input component.</summary>
        /// <returns>The current concentration level.</returns>
        public MentalStateLevel GetConcentrationLevel()
        {
            return _concentrationLevel;
        }

        /// <summary>Enables or disables concentration-based color feedback for the ball.</summary>
        /// <param name="enabled">Whether live concentration levels should update the ball color.</param>
        public void SetConcentrationColorEnabled(bool enabled)
        {
            _usesConcentrationColor = enabled;
            UpdateColor();
        }

        private void OnConcentrationChanged(MentalStateLevel level)
        {
            _concentrationLevel = level;
            UpdateColor();
        }

        private void UpdateColor()
        {
            if (ballImage == null || TutorialSettings.Instance == null) { return; }
            MentalStateLevel colorLevel = _usesConcentrationColor ? _concentrationLevel : MentalStateLevel.None;
            ballImage.color = TutorialSettings.Instance.GetColor(colorLevel);
        }

        private void OnHorizontalMovementReceived(float delta)
        {
            float x = Mathf.Clamp(Position.x + delta, _minPosition.x, _maxPosition.x);
            Position = new Vector2(x, Position.y);
        }
    }
}
