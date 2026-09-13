/*
 * BCI Interaction System for Videogames
 * Master's Thesis · University of Alicante
 * © 2026 Gala M. García
 */

using System;
using Bit.Core;
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

        [Header("Jump Assist")]
        [Tooltip("Horizontal distance scanned for a surface when a jump starts.")]
        [SerializeField, Min(0f)] private float jumpDetectionDistance = 0.4f;
        [Tooltip("Single horizontal impulse applied at a vertical jump.")]
        [SerializeField, Min(0f)] private float verticalJumpHorizontalImpulse = 2f;

        // Latest normalized horizontal intent from the player.
        private float _horizontalInput;
        // Last non-zero horizontal direction used for directional jumps.
        private float _lastHorizontalDirection = 1f;
        // Indicates if the jump has not landed yet.
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
        // Whether the current jump started vertically because a surface was detected.
        private bool _isVerticalJump;
        // Whether the vertical jump already received its apex impulse.
        private bool _hasAppliedApexImpulse;
        // Elapsed physics time since the current jump started.
        private float _jumpTime;
        // Expected time from jump start to the apex.
        private float _timeToApex;
        // Indicates if Bit was on the ground.
        private bool _isGrounded = true;
        // Whether physics was paused directly by a non-game state.
        private bool _isStatePhysicsPaused;
        // Gravity state before a non-game state paused physics.
        private bool _stateGravityWasEnabled;
        // Kinematic state before a non-game state paused physics.
        private bool _stateWasKinematic;

        /// <summary>Triggered when a grounded jump is accepted.</summary>
        public event Action JumpStarted;
        /// <summary>Triggered when a jump returns to a ground layer.</summary>
        public event Action Landed;

        private void FixedUpdate()
        {
            if (physicsBody == null) { return; }
            bool isGame = GameStateController.Instance == null || GameStateController.Instance.GetState() == GameState.Game;
            if (!isGame && !_isMovementLocked)
            {
                if (!_isStatePhysicsPaused)
                {
                    _stateGravityWasEnabled = physicsBody.useGravity;
                    _stateWasKinematic = physicsBody.isKinematic;
                    _isStatePhysicsPaused = true;
                    ClearJumpState();
                }

                physicsBody.isKinematic = true;
                physicsBody.useGravity = false;
                physicsBody.linearVelocity = Vector3.zero;
                physicsBody.angularVelocity = Vector3.zero;
                return;
            }

            if (isGame && _isStatePhysicsPaused)
            {
                physicsBody.isKinematic = _stateWasKinematic;
                physicsBody.useGravity = _stateGravityWasEnabled;
                physicsBody.linearVelocity = Vector3.zero;
                physicsBody.angularVelocity = Vector3.zero;
                _isStatePhysicsPaused = false;
            }

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

            bool isGrounded = IsGrounded();
            if (_isJumping)
            {
                if (_isVerticalJump)
                {
                    _jumpTime += Time.fixedDeltaTime;
                    if (!_hasAppliedApexImpulse && _jumpTime >= _timeToApex)
                    {
                        ApplyApexImpulse();
                    }

                    if (physicsBody.linearVelocity.y <= 0f && isGrounded)
                    {
                        CompleteLanding();
                    }
                    return;
                }

                if (physicsBody.linearVelocity.y <= 0f && isGrounded)
                {
                    CompleteLanding();
                }
            }
            else if (!_isGrounded && isGrounded)
            {
                Landed?.Invoke();
            }

            Vector3 position = physicsBody.position;
            float horizontalInput = _isJumping ? _jumpHorizontalInput : _horizontalInput;
            position.x += horizontalInput * movementSpeed * Time.fixedDeltaTime;
            physicsBody.MovePosition(position);
            _isGrounded = isGrounded;
        }

        /// <summary>Receives the latest normalized horizontal player intent.</summary>
        /// <param name="input">Horizontal input from minus one to one.</param>
        /// <param name="direction">Last non-zero horizontal direction.</param>
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
            _isVerticalJump = DetectReachableSurfaceAhead();
            Vector3 velocity = physicsBody.linearVelocity;
            velocity.y = jumpVelocity * _jumpVelocityMultiplier;
            _jumpTime = 0f;
            float gravityMagnitude = Mathf.Abs(Physics.gravity.y);
            _timeToApex = gravityMagnitude <= Mathf.Epsilon ? 0f : velocity.y / gravityMagnitude;
            _hasAppliedApexImpulse = false;
            _jumpHorizontalInput = _isVerticalJump ? 0f : _lastHorizontalDirection;
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
        /// <param name="enabled">If horizontal movement and jumping should be blocked.</param>
        public void SetMovementEnabled(bool enabled)
        {
            _isMovementLocked = !enabled;
            if (enabled)
            {
                if (physicsBody != null)
                {
                    physicsBody.useGravity = _wasGravityEnabled;
                }
                return;
            }

            ClearJumpState();
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
            ClearJumpState();
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
            _isMovementLocked = false;
            ClearJumpState();
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
            float rayDistance = bounds.extents.y + groundDistance;

            Vector3 center = bounds.center;
            Vector3 front = center + Vector3.right * (bounds.extents.x + 0.1f);
            Vector3 back = center - Vector3.right * (bounds.extents.x + 0.1f);

            return Physics.Raycast(center, Vector3.down, rayDistance, groundLayers, QueryTriggerInteraction.Ignore)
                   || Physics.Raycast(front, Vector3.down, rayDistance, groundLayers, QueryTriggerInteraction.Ignore)
                   || Physics.Raycast(back, Vector3.down, rayDistance, groundLayers, QueryTriggerInteraction.Ignore);
        }

        /// <summary>Checks if Bit can reach an elevated anchored surface ahead.</summary>
        /// <returns>True when a surface was detected.</returns>
        private bool DetectReachableSurfaceAhead()
        {
            if (bodyCollider == null || jumpDetectionDistance <= 0f) { return false; }

            Bounds bodyBounds = bodyCollider.bounds;
            Bounds detectionBounds = GetJumpDetectionArea(bodyBounds);
            RaycastHit[] hits = new RaycastHit[16];
            int hitCount = Physics.BoxCastNonAlloc(detectionBounds.center, detectionBounds.extents, Vector3.right * _lastHorizontalDirection, hits, Quaternion.identity, jumpDetectionDistance, groundLayers, QueryTriggerInteraction.Ignore);

            Collider supportCollider = GetGroundCollider(bodyBounds);
            return ContainsValidJumpSurface(hitCount, hits, supportCollider, bodyBounds);
        }

        /// <summary>Builds the detection volume according to Bit's maximum jump height.</summary>
        /// <param name="bodyBounds">World-space bounds of Bit's body.</param>
        /// <returns>The world-space jump detection bounds.</returns>
        private Bounds GetJumpDetectionArea(Bounds bodyBounds)
        {
            float maximumJumpRise = GetJumpHeight();
            float detectionHeight = bodyBounds.size.y + maximumJumpRise;
            Vector3 size = new Vector3(bodyBounds.size.x, detectionHeight, bodyBounds.size.z);
            Vector3 center = new Vector3(bodyBounds.center.x, bodyBounds.min.y + detectionHeight * 0.5f, bodyBounds.center.z);
            return new Bounds(center, size);
        }

        /// <summary>Calculates the maximum vertical distance reachable by the current jump.</summary>
        private float GetJumpHeight()
        {
            float gravity = Mathf.Abs(Physics.gravity.y);
            if (gravity <= Mathf.Epsilon) { return 0f; }

            float verticalVelocity = jumpVelocity * _jumpVelocityMultiplier;
            return verticalVelocity * verticalVelocity / (2f * gravity);
        }

        /// <summary>Checks whether any detected collider is a valid elevated jump surface.</summary>
        /// <param name="hitCount">Number of colliders stored in the detection hit buffer.</param>
        /// <param name="currentSupport">Collider currently supporting Bit.</param>
        /// <param name="bodyBounds">World-space bounds of Bit's body.</param>
        /// <returns>True when an elevated anchored surface was detected.</returns>
        private bool ContainsValidJumpSurface(int hitCount, RaycastHit[] hits, Collider currentSupport, Bounds bodyBounds)
        {
            AnchoredSurface currentSurface = (currentSupport == null) ? null : currentSupport.GetComponentInParent<AnchoredSurface>();
            float currentSupportTop = (currentSupport == null) ? bodyBounds.min.y : currentSupport.bounds.max.y;

            for (int i = 0; i < hitCount; i++)
            {
                Collider candidate = hits[i].collider;
                if (IsValidJumpSurface(candidate, currentSupport, currentSurface, currentSupportTop)) { return true; }
            }

            return false;
        }

        /// <summary>Checks if a collider represents a different elevated anchored surface.</summary>
        /// <param name="candidate">Collider detected in the jump corridor.</param>
        /// <param name="currentSupport">Collider currently supporting Bit.</param>
        /// <param name="currentSurface">Anchored surface currently supporting Bit.</param>
        /// <param name="currentSupportTop">World-space top height of the current support.</param>
        /// <returns>True when the candidate is a different elevated anchored surface.</returns>
        private static bool IsValidJumpSurface(Collider candidate, Collider currentSupport, AnchoredSurface currentSurface, float currentSupportTop)
        {
            if (candidate == null || candidate.isTrigger || candidate == currentSupport) { return false; }
            AnchoredSurface candidateSurface = candidate.GetComponentInParent<AnchoredSurface>();
            if (candidateSurface == null || candidateSurface == currentSurface) { return false; }
            return candidate.bounds.max.y > currentSupportTop + 0.01f;
        }

        /// <summary>Returns the physical collider directly below Bit.</summary>
        /// <param name="bounds">World-space bounds of Bit's body.</param>
        /// <returns>The supporting collider, or null when none is detected.</returns>
        private Collider GetGroundCollider(Bounds bounds)
        {
            return Physics.Raycast(bounds.center, Vector3.down, out RaycastHit hit, bounds.extents.y + groundDistance, groundLayers, QueryTriggerInteraction.Ignore) ? hit.collider : null;
        }

        /// <summary>Applies the single horizontal impulse for a vertical jump.</summary>
        private void ApplyApexImpulse()
        {
            _hasAppliedApexImpulse = true;
            if (verticalJumpHorizontalImpulse <= 0f) { return; }
            physicsBody.AddForce(Vector3.right * (_lastHorizontalDirection * verticalJumpHorizontalImpulse), ForceMode.Impulse);
        }

        /// <summary>Clears jump state.</summary>
        private void ClearJumpState()
        {
            _jumpHorizontalInput = 0f;
            _isJumping = false;
            _horizontalInput = 0f;
            _isVerticalJump = false;
            _hasAppliedApexImpulse = false;
            _jumpTime = 0f;
            _timeToApex = 0f;
        }

        /// <summary>Ends a jump after sufficient physical support is detected.</summary>
        private void CompleteLanding()
        {
            if (!_isJumping) { return; }
            ClearJumpState();
            Landed?.Invoke();
        }

        private void OnDrawGizmos()
        {
            if (bodyCollider == null || jumpDetectionDistance <= 0f) { return; }

            Bounds bounds = bodyCollider.bounds;
            float verticalVelocity = jumpVelocity * _jumpVelocityMultiplier;
            float gravityMagnitude = Mathf.Abs(Physics.gravity.y);
            float maximumRise = gravityMagnitude <= Mathf.Epsilon ? 0f : verticalVelocity * verticalVelocity / (2f * gravityMagnitude);
            float detectionHeight = Mathf.Max(bounds.size.y, bounds.size.y + maximumRise);
            Vector3 halfExtents = new Vector3(bounds.extents.x, detectionHeight * 0.5f, bounds.extents.z);
            Vector3 origin = new Vector3(bounds.center.x, bounds.min.y + detectionHeight * 0.5f, bounds.center.z);
            Vector3 end = origin + Vector3.right * _lastHorizontalDirection * jumpDetectionDistance;

            Gizmos.color = new Color(1f, 0.8f, 0f, 0.8f);
            Gizmos.DrawWireCube(origin, halfExtents * 2f);
            Gizmos.DrawWireCube(end, halfExtents * 2f);
            Gizmos.DrawLine(origin, end);
        }
    }
}
