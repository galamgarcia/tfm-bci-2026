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
        // Whether external movement input is temporarily blocked.
        private bool _isMovementLocked;
        // Whether player input is blocked while physics remains active.
        private bool _isGameplayInputLocked;
        // Gravity state preserved while movement is locked.
        private bool _wasGravityEnabled;

        /// <summary>Triggered when a grounded jump is accepted.</summary>
        public event Action JumpStarted;
        /// <summary>Triggered when a jump returns to a ground layer.</summary>
        public event Action Landed;

        private void FixedUpdate()
        {
            if (physicsBody == null) { return; }
            if (_isMovementLocked)
            {
                physicsBody.linearVelocity = Vector3.zero;
                return;
            }

            if (_isGameplayInputLocked)
            {
                _horizontalInput = 0f;
                _jumpHorizontalInput = 0f;
                _isJumping = false;
                return;
            }

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

        /// <summary>Receives the latest normalized horizontal player intent.</summary>
        /// <param name="input">Horizontal input from minus one to one.</param>
        public void SetHorizontalInput(float input, float direction)
        {
            if (_isGameplayInputLocked) { return; }
            _horizontalInput = Mathf.Clamp(input, -1f, 1f);
            if (Mathf.Abs(input) > 0.001f)
            {
                _lastHorizontalDirection = direction;
            }
        }

        /// <summary>Attempts to start a physical jump.</summary>
        /// <returns>True when the jump was accepted.</returns>
        public bool TryJump()
        {
            if (physicsBody == null ||_isGameplayInputLocked || !IsGrounded()) { return false; }
            Vector3 velocity = physicsBody.linearVelocity;
            velocity.y = jumpVelocity * _jumpVelocityMultiplier;
            _jumpHorizontalInput = _lastHorizontalDirection;
            velocity.x = _jumpHorizontalInput * movementSpeed;
            physicsBody.linearVelocity = velocity;
            _isJumping = true;
            JumpStarted?.Invoke();
            return true;
        }

        /// <summary>Sets the jump height multiplier supplied by the gameplay controller.</summary>
        /// <param name="multiplier">Multiplier applied to the configured jump velocity.</param>
        public void SetJumpVelocityMultiplier(float multiplier)
        {
            _jumpVelocityMultiplier = Mathf.Max(0f, multiplier);
        }

        /// <summary>Enables or disables player-controlled movement.</summary>
        /// <param name="isLocked">Whether horizontal movement and jumping should be blocked.</param>
        public void SetMovementLocked(bool isLocked)
        {
            _isMovementLocked = isLocked;
            if (!isLocked)
            {
                if (physicsBody != null)
                {
                    physicsBody.useGravity = _wasGravityEnabled;
                }
                return;
            }

            _horizontalInput = 0f;
            _jumpHorizontalInput = 0f;
            _isJumping = false;
            if (physicsBody != null)
            {
                _wasGravityEnabled = physicsBody.useGravity;
                physicsBody.useGravity = false;
                physicsBody.linearVelocity = Vector3.zero;
            }
        }

        /// <summary>Enables or disables gameplay input without changing ongoing physics.</summary>
        /// <param name="isEnabled">Whether horizontal input and jumping should be accepted.</param>
        public void SetGameplayInputEnabled(bool isEnabled)
        {
            _isGameplayInputLocked = !isEnabled;
            if (isEnabled) { return; }

            _horizontalInput = 0f;
            _jumpHorizontalInput = 0f;
            _isJumping = false;
        }

        /// <summary>Moves the locked physics body to a world position and clears its velocity.</summary>
        /// <param name="position">World position to assign to Bit.</param>
        public void MoveTo(Vector3 position)
        {
            if (physicsBody == null) { return; }
            position.z = physicsBody.position.z;
            physicsBody.position = position;
            physicsBody.linearVelocity = Vector3.zero;
        }

        /// <summary>Repositions Bit and restores a clean physical state for respawn.</summary>
        /// <param name="position">World position to assign to Bit.</param>
        public void ResetForRespawn(Vector3 position)
        {
            if (physicsBody == null) { return; }
            position.z = physicsBody.position.z;
            physicsBody.position = position;
            physicsBody.linearVelocity = Vector3.zero;
            physicsBody.angularVelocity = Vector3.zero;
            physicsBody.useGravity = true;
            _horizontalInput = 0f;
            _jumpHorizontalInput = 0f;
            _isJumping = false;
            _isMovementLocked = false;
        }

        /// <summary>Gets the current world position of Bit's physics body.</summary>
        /// <returns>The physics body's current world position.</returns>
        public Vector3 GetPosition()
        {
            return physicsBody == null ? transform.position : physicsBody.position;
        }

        /// <summary>Gets the bounds used by Bit's physical body.</summary>
        /// <returns>The configured body bounds.</returns>
        public Bounds GetBodyBounds()
        {
            return bodyCollider.bounds;
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
