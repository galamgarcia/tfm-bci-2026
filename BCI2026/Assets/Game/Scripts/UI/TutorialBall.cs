/*
 * BCI Interaction System for Videogames
 * Master's Thesis · University of Alicante
 * © 2026 Gala M. García
 */

using BciGame.Core;
using BciGame.Gameplay;
using BciGame.Input;
using Game.Scripts.Gameplay;
using UnityEngine;
using UnityEngine.UI;

namespace BciGame.UI
{
    /// <summary>
    /// Represents the tutorial ball, applying horizontal head input and concentration feedback.
    /// </summary>
    [RequireComponent(typeof(RectTransform), typeof(InputComponent))]
    public sealed class TutorialBall : MonoBehaviour
    {
        [Tooltip("UI transform used to position the ball in canvas space.")]
        [SerializeField] private RectTransform rectTransform;
        [Tooltip("Image colored to reflect the active concentration feedback state.")]
        [SerializeField] private Image image;

        private InputComponent _inputComponent;
        // Horizontal and vertical bounds.
        private Vector2 _minPosition;
        private Vector2 _maxPosition;
        // Indicates if the concentration levels update the ball color.
        private bool _usesConcentrationColor;

        public Vector2 Position
        {
            get => rectTransform.anchoredPosition;
            set => rectTransform.anchoredPosition = value;
        }

        private void Awake()
        { 
            rectTransform = GetComponent<RectTransform>();
            image = GetComponent<Image>();
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

        /// <summary>Updates the sources consumed by the input component.</summary>
        /// <param name="headInputSource">Source that provides head movement and nod gestures.</param>
        /// <param name="mentalInputSource">Source that provides EEG samples and signal quality.</param>
        public void SetInputSources(IHeadInputSource headInputSource, IMentalInputSource mentalInputSource)
        {
            _inputComponent.ConfigureSources(headInputSource, mentalInputSource);
        }

        /// <summary>Gets the most recent concentration level received from the input component.</summary>
        /// <returns>The current concentration level.</returns>
        public MentalStateLevel GetConcentrationLevel()
        {
            return _inputComponent.GetCurrentConcentrationLevel();
        }

        /// <summary>Gets whether the current EEG input source reports a valid signal.</summary>
        public bool HasValidEegSignal()
        {
            return _inputComponent.HasValidMentalSignal();
        } 

        /// <summary>Enables or disables concentration-based color feedback for the ball.</summary>
        /// <param name="enabled">Whether live concentration levels should update the ball color.</param>
        public void SetConcentrationColorEnabled(bool enabled)
        {
            _usesConcentrationColor = enabled;
            UpdateColor();
        }

        /// <summary>Stores a new concentration level and updates the feedback color when enabled.</summary>
        /// <param name="level">New concentration level received from the input component.</param>
        private void OnConcentrationChanged(MentalStateLevel level)
        {
            UpdateColor();
        }

        /// <summary>Updates the ball image color from the active feedback mode and concentration level.</summary>
        private void UpdateColor()
        {
            if (image == null || TutorialSettings.Instance == null) { return; }
            MentalStateLevel colorLevel = _usesConcentrationColor ? _inputComponent.GetCurrentRelaxationLevel() : MentalStateLevel.None;
            image.color = TutorialSettings.Instance.GetColor(colorLevel);
        }

        /// <summary>Moves the ball horizontally while keeping it within the configured canvas bounds.</summary>
        /// <param name="delta">Horizontal canvas-space displacement received for this frame.</param>
        private void OnHorizontalMovementReceived(float delta)
        {
            float x = TutorialRules.GetBoundedHorizontalPosition(Position.x, delta, _minPosition.x, _maxPosition.x);
            Position = new Vector2(x, Position.y);
        }
    }
}
