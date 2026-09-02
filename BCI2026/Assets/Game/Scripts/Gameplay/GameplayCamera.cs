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

        private void FixedUpdate()
        {
            if (target == null || cameraSource == null || !cameraSource.orthographic) { return; }

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
