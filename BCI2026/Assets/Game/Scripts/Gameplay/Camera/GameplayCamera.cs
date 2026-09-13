/*
 * BCI Interaction System for Videogames
 * Master's Thesis · University of Alicante
 * © 2026 Gala M. García
 */

using UnityEngine;
using Bit.Core;

namespace Bit.Gameplay
{
    /// <summary>Observes Bit through a normalized rope zone and moves only when its limits are crossed.</summary>
    public sealed class GameplayCamera : MonoBehaviour
    {
        [Header("References")]
        [Tooltip("Transform observed by the rope camera.")]
        [SerializeField] private Transform target;
        [Tooltip("Orthographic camera moved by this component.")]
        [SerializeField] private Camera cameraSource;

        [Header("Horizontal Rope")]
        [Tooltip("Normalized viewport position of the left free-movement boundary.")]
        [SerializeField, Range(0f, 1f)] private float leftBoundary = 0.3f;
        [Tooltip("Normalized viewport position of the right free-movement boundary.")]
        [SerializeField, Range(0f, 1f)] private float rightBoundary = 0.6f;

        [Header("Vertical Rope")]
        [Tooltip("Normalized viewport position of the bottom free-movement boundary.")]
        [SerializeField, Range(0f, 1f)] private float bottomBoundary = 0.3f;
        [Tooltip("Normalized viewport position of the top free-movement boundary.")]
        [SerializeField, Range(0f, 1f)] private float topBoundary = 0.6f;

        [Header("Movement")]
        [Tooltip("Horizontal smoothing time applied only while the horizontal rope is taut.")]
        [SerializeField, Min(0f)] private float horizontalSmoothing = 0.45f;
        [Tooltip("Vertical smoothing time applied only while the vertical rope is taut.")]
        [SerializeField, Min(0f)] private float verticalSmoothing = 0.35f;

        [Header("Camera Bounds")]
        [Tooltip("Enables clamping the camera center to the configured level bounds.")]
        [SerializeField] private bool hasCameraBounds;
        [Tooltip("World-space area that may be visible through the camera viewport.")]
        [SerializeField] private Bounds cameraBounds = new Bounds(Vector3.zero, new Vector3(20f, 10f, 1f));

        // Current horizontal smoothing velocity.
        private float _horizontalVelocity;
        // Current vertical smoothing velocity.
        private float _verticalVelocity;
        // Prevents rope tracking while Bit is falling.
        private bool _isPaused;

        private void Awake()
        {
            _isPaused = TryGetComponent<GameplayCameraIntro>(out _);
        }

        private void FixedUpdate()
        {
            if (target == null || cameraSource == null || !cameraSource.orthographic) { return; }
            if (_isPaused) { return; }
            if (GameStateController.Instance != null && GameStateController.Instance.GetState() != GameState.Game) { return; }

            float deltaTime = Mathf.Max(Time.fixedDeltaTime, 0.0001f);
            Vector3 targetPosition = target.position;
            Vector2 visibleSize = new Vector2(cameraSource.orthographicSize * 2f * cameraSource.aspect, cameraSource.orthographicSize * 2f);
            Vector2 current = new Vector2(transform.position.x, transform.position.y);
            Vector2 viewport = cameraSource.WorldToViewportPoint(targetPosition);
            Rect rope = new Rect(leftBoundary, bottomBoundary, Mathf.Max(0f, rightBoundary - leftBoundary), Mathf.Max(0f, topBoundary - bottomBoundary));
            Vector2 desired = Utils.GetCameraRopePosition(current, viewport, rope, visibleSize);

            if (hasCameraBounds)
            {
                desired = Utils.ClampCameraCenter(desired, cameraBounds, visibleSize);
            }

            float nextX = Mathf.SmoothDamp(current.x, desired.x, ref _horizontalVelocity, horizontalSmoothing, Mathf.Infinity, deltaTime);
            float nextY = Mathf.SmoothDamp(current.y, desired.y, ref _verticalVelocity, verticalSmoothing, Mathf.Infinity, deltaTime);
            transform.position = new Vector3(nextX, nextY, transform.position.z);
        }

        /// <summary>Pauses or resumes rope tracking and clears smoothing velocity.</summary>
        /// <param name="pause">Indicates whether tracking should be paused.</param>
        public void PauseTracking(bool pause)
        {
            _isPaused = pause;
            _horizontalVelocity = 0f;
            _verticalVelocity = 0f;
        }

        /// <summary>Snaps the camera so Bit is positioned at the rope center.</summary>
        public void SnapToTarget()
        {
            if (target == null || cameraSource == null || !cameraSource.orthographic) { return; }

            Vector3 desired = GetTargetCameraPosition();

            _horizontalVelocity = 0f;
            _verticalVelocity = 0f;
            transform.position = desired;
        }

        /// <summary>Calculates the camera position that places the target at the rope center.</summary>
        /// <returns>The target-follow camera position.</returns>
        public Vector3 GetTargetCameraPosition()
        {
            if (target == null || cameraSource == null || !cameraSource.orthographic) { return transform.position; }

            Vector2 visibleSize = GetVisibleSize();
            Vector2 ropeCenter = new Vector2((leftBoundary + rightBoundary) * 0.5f, (bottomBoundary + topBoundary) * 0.5f);
            Vector2 desired = new Vector2(target.position.x, target.position.y) - new Vector2((ropeCenter.x - 0.5f) * visibleSize.x, (ropeCenter.y - 0.5f) * visibleSize.y);
            if (hasCameraBounds) { desired = Utils.ClampCameraCenter(desired, cameraBounds, visibleSize); }
            return new Vector3(desired.x, desired.y, transform.position.z);
        }

        /// <summary>Returns the camera used by the gameplay rope.</summary>
        /// <returns>The configured camera.</returns>
        public Camera GetCamera()
        {
            return cameraSource;
        }

        private Vector2 GetVisibleSize()
        {
            return new Vector2(cameraSource.orthographicSize * 2f * cameraSource.aspect, cameraSource.orthographicSize * 2f);
        }

        private void OnValidate()
        {
            leftBoundary = Mathf.Clamp01(leftBoundary);
            rightBoundary = Mathf.Clamp01(Mathf.Max(leftBoundary, rightBoundary));
            bottomBoundary = Mathf.Clamp01(bottomBoundary);
            topBoundary = Mathf.Clamp01(Mathf.Max(bottomBoundary, topBoundary));
        }

        private void OnDrawGizmosSelected()
        {
            if (cameraSource == null) { return; }

            Vector3 center = cameraSource.transform.position;
            Vector2 visibleSize = new Vector2(cameraSource.orthographicSize * 2f * cameraSource.aspect, cameraSource.orthographicSize * 2f);
            Rect rope = new Rect(leftBoundary, bottomBoundary, Mathf.Max(0f, rightBoundary - leftBoundary), Mathf.Max(0f, topBoundary - bottomBoundary));
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireCube(new Vector3(center.x + (rope.center.x - 0.5f) * visibleSize.x, center.y + (rope.center.y - 0.5f) * visibleSize.y, center.z), new Vector3(rope.width * visibleSize.x, rope.height * visibleSize.y, 0f));

            if (!hasCameraBounds) { return; }
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireCube(new Vector3(cameraBounds.center.x, cameraBounds.center.y, center.z), new Vector3(cameraBounds.size.x, cameraBounds.size.y, 0f));
        }
    }

}
