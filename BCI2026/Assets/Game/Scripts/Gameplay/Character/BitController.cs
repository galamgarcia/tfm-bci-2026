/*
 * BCI Interaction System for Videogames
 * Master's Thesis · University of Alicante
 * © 2026 Gala M. García
 */

using System;
using UnityEngine;
using Bit.Input;

namespace Bit.Gameplay
{
    /// <summary>Coordinates Bit's movement intent, gaze direction, and idle visual states.</summary>
    [RequireComponent(typeof(InputController))]
    public sealed class BitController : MonoBehaviour
    {
        [Header("References")]
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

        // Time elapsed since the player stopped sending horizontal input.
        private float _lastInputWas;
        // Whether the coordinated idle animations are active.
        private bool _isIdleActive;
        // Position used when Bit respawns.
        private Vector3 _respawnPosition;

        private InputController _inputController;

        /// <summary>Triggered when Bit receives a confirmed concentration state.</summary>
        public event Action<MentalStateLevel> OnConcentrationChanged;

        private void Awake()
        {
            _inputController = GetComponent<InputController>();
            _respawnPosition = GetPosition();
        }

        private void OnEnable()
        {
            _inputController ??= GetComponent<InputController>();
            if (_inputController == null) { return; }
            _inputController.OnHorizontalInputReceived += OnHorizontalInputReceived;
            _inputController.OnBlinkDetected += OnBlinkDetected;
            _inputController.OnRelaxationChanged += OnRelaxationChanged;
            _inputController.OnConcentrationChanged += HandleConcentrationChanged;
            if (movementController != null)
            {
                movementController.JumpStarted += OnJumpStarted;
                movementController.Landed += OnLanded;
            }
        }

        private void OnDisable()
        {
            if (_inputController == null) { return; }
            _inputController.OnHorizontalInputReceived -= OnHorizontalInputReceived;
            _inputController.OnBlinkDetected -= OnBlinkDetected;
            _inputController.OnRelaxationChanged -= OnRelaxationChanged;
            _inputController.OnConcentrationChanged -= HandleConcentrationChanged;
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
            float direction = Mathf.Sign(input);
            movementController?.SetHorizontalInput(input, direction);
            if (Mathf.Abs(input) > 0.001f)
            {
                _lastInputWas = 0f;
                StopIdle();
                eyeController?.SetLookDirection(direction < 0f ? BitLookDirection.Left : BitLookDirection.Right);
                return;
            }

            _lastInputWas += Time.deltaTime;
        }

        /// <summary>Forwards a validated blink command to the movement controller.</summary>
        private void OnBlinkDetected()
        {
            movementController?.TryJump();
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
            movementController?.SetJumpVelocityMultiplier(isRelaxed ? 1.25f : 1f);
            eyeController?.SetRelaxation(isRelaxed ? 1f : 0f);
            vfxManager?.SetRelaxationIntensity(isRelaxed ? 1f : 0f);
        }

        /// <summary>Updates the concentration state.</summary>
        /// <param name="level">Confirmed concentration level.</param>
        private void HandleConcentrationChanged(MentalStateLevel level)
        {
            bodyController?.SetConcentrationHigh(level == MentalStateLevel.High);
            OnConcentrationChanged?.Invoke(level);
        }

        /// <summary>Returns latests concentration state.</summary>
        /// <returns>The current concentration level.</returns>
        public MentalStateLevel GetConcentrationLevel()
        {
            return _inputController != null
                ? _inputController.GetCurrentConcentrationLevel()
                : MentalStateLevel.None;
        }

        /// <summary>Starts the coordinated body and eye idle animations.</summary>
        private void StartIdle()
        {
            _isIdleActive = true;
            bodyController?.StartIdle();
            //eyeController?.StartEyesIdle();
        }

        /// <summary>Stops the coordinated body and eye idle animations.</summary>
        private void StopIdle()
        {
            if (!_isIdleActive) { return; }
            _isIdleActive = false;
            bodyController?.StopBodyIdle();
            //eyeController?.StopEyesIdle();
        }

        /// <summary>Enables or disables gameplay input and mental-state updates.</summary>
        /// <param name="enable">Indicates if Bit may receive gameplay input.</param>
        public void SetGameplayEnabled(bool enable)
        {
            movementController.SetGameplayInputEnabled(enable);
            _inputController.SetMentalStateUpdatesEnabled(enable);
        }

        /// <summary>Enables or disables physical movement.</summary>
        /// <param name="enabled">If physical movement should be enabled.</param>
        public void SetMovementEnabled(bool enabled)
        {
            movementController.SetMovementEnabled(enabled);
        }

        /// <summary>Stores the world position used by the next respawn.</summary>
        /// <param name="position">World respawn position.</param>
        public void SetRespawnPosition(Vector3 position)
        {
            _respawnPosition = position;
        }

        /// <summary>Returns the currently stored respawn position.</summary>
        /// <returns>The world respawn position.</returns>
        public Vector3 GetRespawnPosition()
        {
            return _respawnPosition;
        }

        /// <summary>Restores and coordinated visual state after a respawn.</summary>
        public void ResetForRespawn()
        {
            movementController.ResetForRespawn(_respawnPosition);
            StopIdle();
            bodyController?.ResetBodyState();
            eyeController?.ResetRelaxation();
            vfxManager?.StopRelaxationVfx();
        }

        /// <summary>Returns current world position.</summary>
        /// <returns>Current world position.</returns>
        public Vector3 GetPosition()
        {
            return movementController.GetPosition();
        }

        /// <summary>Returns current physical bounds.</summary>
        /// <returns>Current world-space physical bounds.</returns>
        public Bounds GetBodyBounds()
        {
            return movementController.GetBodyBounds();
        }
    }
}
