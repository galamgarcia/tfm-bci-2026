/*
 * BCI Interaction System for Videogames
 * Master's Thesis · University of Alicante
 * © 2026 Gala M. García
 */

using System;
using UnityEngine;

namespace Bit.Gameplay
{
    /// <summary>Applies horizontal input and jump actions to Bit's 3D physics root.</summary>
    public sealed class BitMovementController : MonoBehaviour
    {
        [Header("References")]
        [Tooltip("Input component that publishes Bit's horizontal intent and jump actions.")]
        [SerializeField] private InputController inputController;
        [Tooltip("Rigidbody attached to Bit's physics root.")]
        [SerializeField] private Rigidbody physicsBody;
        [Tooltip("Bit's collider used to calculate the ground-check distance.")]
        [SerializeField] private Collider bodyCollider;

        [Header("Movement")]
        [Tooltip("Horizontal movement speed in world units per second.")]
        [SerializeField, Min(0f)] private float movementSpeed = 2f;
        [Tooltip("Upward velocity applied when Bit jumps.")]
        [SerializeField, Min(0f)] private float jumpVelocity = 4f;
        [Tooltip("Distance below the collider used for the ground check.")]
        [SerializeField, Min(0f)] private float groundDistance = 0.05f;
        [Tooltip("Layers that count as ground for jumping.")]
        [SerializeField] private LayerMask groundLayers = ~0;

        // Latest normalized horizontal intent from the player.
        private float _horizontalInput;
        // Last non-zero horizontal direction used for directional jumps.
        private float _lastHorizontalDirection = 1f;
        // Whether the current jump has not landed yet.
        private bool _isJumping;
        // Horizontal direction retained by the current jump.
        private float _jumpHorizontalInput;
        // Jump height multiplier controlled by the confirmed relaxation state.
        private float _jumpVelocityMultiplier = 1f;

        /// <summary>Triggered when a grounded jump is accepted.</summary>
        public event Action JumpStarted;
        /// <summary>Triggered when a jump returns to a ground layer.</summary>
        public event Action Landed;

        private void OnEnable()
        {
            if (inputController == null) { return; }
            inputController.OnHorizontalInputReceived += OnHorizontalInputReceived;
            inputController.OnBlinkDetected += OnBlinkDetected;
            inputController.OnRelaxationChanged += OnRelaxationChanged;
        }

        private void OnDisable()
        {
            if (inputController == null) { return; }
            inputController.OnHorizontalInputReceived -= OnHorizontalInputReceived;
            inputController.OnBlinkDetected -= OnBlinkDetected;
            inputController.OnRelaxationChanged -= OnRelaxationChanged;
        }

        private void FixedUpdate()
        {
            if (physicsBody == null) { return; }
            if (_isJumping && physicsBody.linearVelocity.y <= 0f && IsGrounded())
            {
                _isJumping = false;
                Landed?.Invoke();
            }

            Vector3 position = physicsBody.position;
            float horizontalInput = _isJumping ? _jumpHorizontalInput : _horizontalInput;
            position.x += horizontalInput * movementSpeed * Time.fixedDeltaTime;
            physicsBody.MovePosition(position);
        }

        /// <summary>Stores the latest normalized horizontal player intent.</summary>
        /// <param name="input">Horizontal input from minus one to one.</param>
        private void OnHorizontalInputReceived(float input)
        {
            _horizontalInput = Mathf.Clamp(input, -1f, 1f);
            if (Mathf.Abs(_horizontalInput) > 0.001f)
            {
                _lastHorizontalDirection = Mathf.Sign(_horizontalInput);
            }
        }

        /// <summary>Starts a physical jump when a validated blink is received.</summary>
        private void OnBlinkDetected()
        {
            if (physicsBody == null || !IsGrounded()) { return; }
            Vector3 velocity = physicsBody.linearVelocity;
            velocity.y = jumpVelocity * _jumpVelocityMultiplier;
            _jumpHorizontalInput = _lastHorizontalDirection;
            velocity.x = _jumpHorizontalInput * movementSpeed;
            physicsBody.linearVelocity = velocity;
            _isJumping = true;
            JumpStarted?.Invoke();
        }

        /// <summary>Updates jump height from the confirmed relaxation state.</summary>
        /// <param name="level">Confirmed relaxation level.</param>
        private void OnRelaxationChanged(MentalStateLevel level)
        {
            _jumpVelocityMultiplier = level == MentalStateLevel.High ? 1.25f : 1f;
        }

        /// <summary>Checks whether the configured collider is touching a ground layer.</summary>
        /// <returns>True when the physics body is grounded.</returns>
        private bool IsGrounded()
        {
            if (bodyCollider == null) { return false; }
            Bounds bounds = bodyCollider.bounds;
            return Physics.Raycast(bounds.center, Vector3.down, bounds.extents.y + groundDistance, groundLayers, QueryTriggerInteraction.Ignore);
        }
    }
}
