/*
 * BCI Interaction System for Videogames
 * Master's Thesis · University of Alicante
 * © 2026 Gala M. García
 */

using System.Collections;
using UnityEngine;

namespace Bit.Gameplay
{
    /// <summary>Plays the optional linear camera introduction before normal gameplay tracking begins.</summary>
    [RequireComponent(typeof(GameplayCamera))]
    public sealed class GameplayCameraIntro : MonoBehaviour
    {
        [Header("Introduction")]
        [Tooltip("Starting point of the camera introduction, normally the TransferNode.")]
        [SerializeField] private Transform introStart;
        [Tooltip("Optional camera points visited in serialized order before Bit.")]
        [SerializeField] private Transform[] introWaypoints;
        [Tooltip("Linear camera travel speed in world units per second.")]
        [SerializeField, Min(0.01f)] private float introSpeed = 2f;

        [Header("References")]
        [Tooltip("Bit controller whose gameplay input is disabled during the introduction.")]
        [SerializeField] private BitController bitController;
        // Gameplay camera required by this component and resolved on the same object.
        private GameplayCamera _cameraController;

        private void Awake()
        {
            _cameraController = GetComponent<GameplayCamera>();
        }

        private void Start()
        {
            if (bitController == null || introStart == null)
            {
                _cameraController.PauseTracking(false);
                return;
            }

            bitController.SetGameplayEnabled(false);
            bitController.SetMovementEnabled(false);
            _cameraController.PauseTracking(true);
            transform.position = new Vector3(introStart.position.x, introStart.position.y, transform.position.z);
            StartCoroutine(PlayIntro());
        }

        /// <summary>Moves the camera through configured points and finishes at Bit.</summary>
        /// <returns>The running camera introduction sequence.</returns>
        private IEnumerator PlayIntro()
        {
            if (introWaypoints != null)
            {
                foreach (Transform waypoint in introWaypoints)
                {
                    if (waypoint == null) { continue; }
                    yield return MoveTo(waypoint.position);
                }
            }

            yield return MoveTo(_cameraController.GetTargetCameraPosition());
            _cameraController.SnapToTarget();
            _cameraController.PauseTracking(false);
            bitController.SetMovementEnabled(true);
            bitController.SetGameplayEnabled(true);
        }

        /// <summary>Moves the camera linearly to a world position.</summary>
        /// <param name="destination">Destination position for the camera center.</param>
        /// <returns>The running linear movement.</returns>
        private IEnumerator MoveTo(Vector3 destination)
        {
            Vector3 target = new Vector3(destination.x, destination.y, transform.position.z);
            while ((transform.position - target).sqrMagnitude > 0.0001f)
            {
                transform.position = Vector3.MoveTowards(transform.position, target, introSpeed * Time.fixedDeltaTime);
                yield return new WaitForFixedUpdate();
            }
        }

        private void OnValidate()
        {
            introSpeed = Mathf.Max(0.01f, introSpeed);
        }
    }
}
