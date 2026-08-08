/*
 * BCI Interaction System for Videogames
 * Master's Thesis · University of Alicante
 * © 2026 Gala M. García
 */

using BciGame.Gameplay;
using Game.Scripts.Gameplay;
using UnityEngine;
using UnityEngine.UI;

namespace BciGame.UI
{
    /// <summary>
    /// Represents the spawned visual ball and applies bounded canvas-space movement.
    /// </summary>
    [RequireComponent(typeof(RectTransform), typeof(InputComponent))]
    public sealed class TutorialBall : MonoBehaviour
    {
        [Header("References")]
        [Tooltip("Used to position the ball in tutorial screen.")]
        [SerializeField] private RectTransform initialBallTransform;
        [Tooltip("Image whose color visualizes the current EEG level.")]
        [SerializeField] private Image ballImage;
        
        /// <summary>Component that reads input and publishes movement events.</summary>
        private InputComponent _inputComponent;
        /// <summary>Minimum permitted position in the parent canvas space.</summary>
        private Vector2 _minimumPosition;
        /// <summary>Maximum permitted position in the parent canvas space.</summary>
        private Vector2 _maximumPosition;
        /// <summary>Input source currently driving this ball.</summary>
        private MovementState _movementState;
        /// <summary>Most recently received EEG level for the configured mental state.</summary>
        private MentalStateLevel _mentalState = MentalStateLevel.None;

        [Header("Movement")]
        [Tooltip("Vertical speed in canvas units per second when the EEG state is high.")]
        [SerializeField] private float eegMovementSpeed = 170f;

        [Header("EEG Feedback")]
        [Tooltip("Color shown while concentration is medium or high.")]
        [SerializeField] private Color concentratedColor = new Color(0.86f, 0.24f, 0.24f);

        /// <summary>Color configured on the ball image before EEG feedback is applied.</summary>
        private Color _initialColor;

        // Gets or sets the ball position in its parent canvas space.
        public Vector2 Position
        {
            get => initialBallTransform.anchoredPosition;
            set => initialBallTransform.anchoredPosition = value;
        }

        private void Awake()
        {
            if (initialBallTransform == null)
            {
                initialBallTransform = GetComponent<RectTransform>();
            }

            if (ballImage == null)
            {
                ballImage = GetComponent<Image>();
            }
            if (ballImage != null)
            {
                _initialColor = ballImage.color;
            }

            _inputComponent = GetComponent<InputComponent>();
            if (_inputComponent == null)
            {
                _inputComponent = gameObject.AddComponent<InputComponent>();
            }
        }

        private void OnEnable()
        {
            _inputComponent.OnHorizontalMovementReceived += OnHorizontalMovementReceived;
            _inputComponent.OnRelaxationChanged += OnRelaxationChanged;
            _inputComponent.OnConcentrationChanged += OnConcentrationChanged;
        }

        private void OnDisable()
        {
            _inputComponent.OnHorizontalMovementReceived -= OnHorizontalMovementReceived;
            _inputComponent.OnRelaxationChanged -= OnRelaxationChanged;
            _inputComponent.OnConcentrationChanged -= OnConcentrationChanged;
        }

        private void Update()
        {
            float strength = _mentalState switch
            {
                MentalStateLevel.Medium => 0.5f,
                MentalStateLevel.High => 1f,
                _ => 0f
            };
            if (strength == 0f) { return; }

            float direction = _movementState == MovementState.RelaxationUp ? 1f : -1f;
            Move(new Vector2(0f, direction * strength * eegMovementSpeed * Time.deltaTime));
        }

        /// <summary>
        /// Configures the input source and canvas bounds for this ball.
        /// </summary>
        /// <param name="state">Input source used to move the ball.</param>
        /// <param name="minimumPosition">Minimum permitted canvas-space position.</param>
        /// <param name="maximumPosition">Maximum permitted canvas-space position.</param>
        public void Configure(MovementState state, Vector2 minimumPosition, Vector2 maximumPosition)
        {
            _movementState = state;
            _minimumPosition = minimumPosition;
            _maximumPosition = maximumPosition;
            _mentalState = MentalStateLevel.None;
            UpdateColor();
            _inputComponent.SetInputTracking(
                state == MovementState.HeadYaw,
                false,
                state == MovementState.RelaxationUp,
                state == MovementState.ConcentrationDown);
        }

        /// <summary>
        /// Applies horizontal head-tracking movement.
        /// </summary>
        /// <param name="delta">Horizontal canvas-space delta for the current frame.</param>
        private void OnHorizontalMovementReceived(float delta)
        {
            Move(new Vector2(delta, 0f));
        }

        /// <summary>
        /// Stores a relaxation level when relaxation controls this ball.
        /// </summary>
        /// <param name="state">New relaxation level.</param>
        private void OnRelaxationChanged(MentalStateLevel state)
        {
            if (_movementState == MovementState.RelaxationUp)
            {
                _mentalState = state;
            }
        }

        /// <summary>
        /// Stores a concentration level when concentration controls this ball.
        /// </summary>
        /// <param name="state">New concentration level.</param>
        private void OnConcentrationChanged(MentalStateLevel state)
        {
            if (_movementState == MovementState.ConcentrationDown)
            {
                _mentalState = state;
                UpdateColor();
            }
        }

        /// <summary>
        /// Shows red only while concentration is medium or high; otherwise restores blue.
        /// </summary>
        private void UpdateColor()
        {
            if (ballImage == null) { return; }
            bool isConcentrated = _movementState == MovementState.ConcentrationDown && (_mentalState == MentalStateLevel.Medium || _mentalState == MentalStateLevel.High);
            ballImage.color = isConcentrated ? concentratedColor : _initialColor;
        }

        /// <summary>
        /// Moves the ball by a delta while enforcing its configured canvas bounds.
        /// </summary>
        /// <param name="delta">Canvas-space displacement to apply.</param>
        private void Move(Vector2 delta)
        {
            Vector2 position = Position + delta;
            Position = new Vector2(Mathf.Clamp(position.x, _minimumPosition.x, _maximumPosition.x), Mathf.Clamp(position.y, _minimumPosition.y, _maximumPosition.y));
        }
    }
}
