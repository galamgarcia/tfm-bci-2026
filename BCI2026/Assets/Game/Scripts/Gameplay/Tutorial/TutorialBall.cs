/*
 * BCI Interaction System for Videogames
 * Master's Thesis · University of Alicante
 * © 2026 Gala M. García
 */

using Bit.Core;
using Bit.Gameplay;
using Bit.Input;
using Bit.UI;
using UnityEngine;

namespace Bit.Gameplay
{
    /// <summary>Represents the tutorial ball.</summary>
    [RequireComponent(typeof(RectTransform), typeof(InputController))]
    public sealed class TutorialBall : CanvasImage
    {
        [Header("Movement")]
        [Tooltip("Horizontal movement speed in canvas units per second.")]
        [SerializeField, Min(0f)] private float movementSpeed = 240f;

        // Input component that supplies ball movement and color feedback.
        private InputController _inputComponent;
        // Whether concentration levels update the ball color.
        private bool _usesConcentrationColor;

        protected override void Awake()
        {
            base.Awake();
            _inputComponent = GetComponent<InputController>();
        }

        private void OnEnable()
        {
            _inputComponent.OnHorizontalInputReceived += OnHorizontalInputReceived;
            _inputComponent.OnConcentrationChanged += OnConcentrationChanged;
        }

        private void OnDisable()
        {
            _inputComponent.OnHorizontalInputReceived -= OnHorizontalInputReceived;
            _inputComponent.OnConcentrationChanged -= OnConcentrationChanged;
        }

        /// <summary>Configures the horizontal bounds and disables concentration color feedback.</summary>
        /// <param name="min">Minimum allowed position in canvas space.</param>
        /// <param name="max">Maximum allowed position in canvas space.</param>
        public override void Configure(Vector2 min, Vector2 max)
        {
            base.Configure(min, max);
            SetConcentrationColor(false);
        }

        /// <summary>Updates the sources consumed by the input component.</summary>
        /// <param name="headInputSource">Source that provides head movement and nod gestures.</param>
        /// <param name="mentalInputSource">Source that provides EEG samples and signal quality.</param>
        public void SetInput(IHeadInputSource headInputSource, IMentalInputSource mentalInputSource)
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
        /// <returns>Whether the current mental input signal is valid.</returns>
        public bool HasValidEegSignal()
        {
            return _inputComponent.HasValidMentalSignal();
        }

        /// <summary>Enables or disables concentration-based color feedback for the ball.</summary>
        /// <param name="enabled">Whether live concentration levels should update the ball color.</param>
        public void SetConcentrationColor(bool enabled)
        {
            _usesConcentrationColor = enabled;
            UpdateColor();
        }

        /// <summary>Updates the ball image color from the active feedback mode and concentration level.</summary>
        private void UpdateColor()
        {
            if (TutorialSettings.Instance == null) { return; }
            MentalStateLevel colorLevel = _usesConcentrationColor ? _inputComponent.GetCurrentRelaxationLevel() : MentalStateLevel.None;
            SetColor(TutorialSettings.Instance.GetColor(colorLevel));
        }

        /// <summary>Updates the feedback color after a concentration-level change.</summary>
        /// <param name="level">Concentration level received from the input component.</param>
        private void OnConcentrationChanged(MentalStateLevel level)
        {
            UpdateColor();
        }

        /// <summary>Moves the ball from normalized horizontal input while keeping it within its bounds.</summary>
        /// <param name="input">Normalized horizontal input from minus one to one.</param>
        private void OnHorizontalInputReceived(float input)
        {
            Vector2 position = GetPosition();
            float delta = input * movementSpeed * Time.deltaTime;
            float x = GetBoundedHorizontal(position.x, delta);
            SetPosition(new Vector2(x, position.y));
        }
    }
}
