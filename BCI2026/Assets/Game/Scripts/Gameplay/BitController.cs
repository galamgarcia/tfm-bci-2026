/*
 * BCI Interaction System for Videogames
 * Master's Thesis · University of Alicante
 * © 2026 Gala M. García
 */

using UnityEngine;
using Bit.Input;

namespace Bit.Gameplay
{
    /// <summary>Coordinates Bit's movement intent, gaze direction, and idle visual states.</summary>
    public sealed class BitController : MonoBehaviour
    {
        [Header("References")]
        [Tooltip("Input component that publishes Bit's player intent.")]
        [SerializeField] private InputController inputController;
        [Tooltip("Eye controller used to display the last movement direction.")]
        [SerializeField] private BitEyeController eyeController;
        [Tooltip("Body controller used to play body idle animations.")]
        [SerializeField] private BitBodyController bodyController;
        [Tooltip("Movement controller that reports accepted jumps and landings.")]
        [SerializeField] private BitMovementController movementController;
        [Tooltip("Visual effects controller used for relaxation particles.")]
        [SerializeField] private BitVfxManager vfxManager;

        [Header("Idle")]
        [Tooltip("Seconds without movement input before Bit's idle animations start.")]
        [SerializeField, Min(0f)] private float idleDelay = 0.75f;

        // Last non-zero horizontal input direction.
        private float _lastHorizontalInput;
        // Time elapsed since the player stopped sending horizontal input.
        private float _lastInputWas;
        // Whether the coordinated idle animations are active.
        private bool _isIdleActive;

        /// <summary>Connects a device-independent head input source to Bit's input controller.</summary>
        /// <param name="input">Source that provides normalized horizontal head input.</param>
        public void ConfigureHeadInput(IHeadInputSource input)
        {
            if (inputController != null)
            {
                inputController.ConfigureSources(input, null);
            }
        }

        private void OnEnable()
        {
            if (inputController == null) { return; }
            inputController.OnHorizontalInputReceived += OnHorizontalInputReceived;
            inputController.OnRelaxationChanged += OnRelaxationChanged;
            inputController.OnConcentrationChanged += OnConcentrationChanged;
            if (movementController != null)
            {
                movementController.JumpStarted += OnJumpStarted;
                movementController.Landed += OnLanded;
            }
        }

        private void OnDisable()
        {
            if (inputController == null) { return; }
            inputController.OnHorizontalInputReceived -= OnHorizontalInputReceived;
            inputController.OnRelaxationChanged -= OnRelaxationChanged;
            inputController.OnConcentrationChanged -= OnConcentrationChanged;
            if (movementController != null)
            {
                movementController.JumpStarted -= OnJumpStarted;
                movementController.Landed -= OnLanded;
            }
        }

        private void Update()
        {
            if (_isIdleActive) { return; }
            if (_lastInputWas < Mathf.Max(0f, idleDelay)) { return; }
            StartIdle();
        }

        /// <summary>Updates gaze and idle state from the player's latest movement intent.</summary>
        /// <param name="input">Normalized horizontal input from minus one to one.</param>
        private void OnHorizontalInputReceived(float input)
        {
            if (Mathf.Abs(input) > 0.001f)
            {
                _lastHorizontalInput = Mathf.Sign(input);
                _lastInputWas = 0f;
                StopIdle();
                eyeController?.SetLookDirection(_lastHorizontalInput < 0f ? BitLookDirection.Left : BitLookDirection.Right);
                return;
            }

            _lastInputWas += Time.deltaTime;
        }

        /// <summary>Starts the visual jump pose after the movement controller accepts a jump.</summary>
        private void OnJumpStarted()
        {
            StopIdle();
            bodyController?.StartJump();
        }

        /// <summary>Starts the landing visual response after a jump ends.</summary>
        private void OnLanded()
        {
            bodyController?.PlayLandingSquash();
            eyeController?.PlayLandingExpression();
        }

        /// <summary>Updates the eye relaxation expression from the confirmed relaxation state.</summary>
        /// <param name="level">Confirmed relaxation level.</param>
        private void OnRelaxationChanged(MentalStateLevel level)
        {
            bool isRelaxed = level == MentalStateLevel.High;
            eyeController?.SetRelaxation(isRelaxed ? 1f : 0f);
            vfxManager?.SetRelaxationIntensity(isRelaxed ? 1f : 0f);
        }

        /// <summary>Updates BIT's body feedback from the confirmed concentration state.</summary>
        /// <param name="level">Confirmed concentration level.</param>
        private void OnConcentrationChanged(MentalStateLevel level)
        {
            bodyController?.SetConcentrationHigh(level == MentalStateLevel.High);
        }

        /// <summary>Starts the coordinated body and eye idle animations.</summary>
        private void StartIdle()
        {
            _isIdleActive = true;
            bodyController?.StartIdle();
            eyeController?.StartEyesIdle();
        }

        /// <summary>Stops the coordinated body and eye idle animations.</summary>
        private void StopIdle()
        {
            if (!_isIdleActive) { return; }
            _isIdleActive = false;
            bodyController?.StopBodyIdle();
            eyeController?.StopEyesIdle();
            if (Mathf.Abs(_lastHorizontalInput) > 0.001f)
            {
                eyeController?.SetLookDirection(_lastHorizontalInput < 0f ? BitLookDirection.Left : BitLookDirection.Right);
            }
        }
    }
}
