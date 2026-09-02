/*
 * BCI Interaction System for Videogames
 * Master's Thesis · University of Alicante
 * © 2026 Gala M. García
 */

using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Bit.Gameplay
{
    /// <summary>Detects Bit inside a system destination and publishes one transfer request.</summary>
    [RequireComponent(typeof(BoxCollider))]
    public sealed class TransferNode : MonoBehaviour
    {
        [Header("References")]
        [Tooltip("Visual controller for the segmented system silhouette.")]
        [SerializeField] private TransferNodeVisual visual;

        [Header("Synchronization")]
        [Tooltip("Continuous time Bit must remain inside before it is locked and pulled toward the node center.")]
        [SerializeField, Min(0.1f)] private float syncDuration = 2f;

        [Tooltip("Time spent completing the silhouette after Bit reaches the center.")]
        [SerializeField, Min(0.05f)] private float completeDuration = 0.8f;

        [Tooltip("Time used to move Bit from its locked position to the center of the node.")]
        [SerializeField, Min(0.05f)] private float centeringDuration = 0.8f;

        [Header("Destination")]
        [Tooltip("Scene loaded after Bit has been visually transferred.")]
        [SerializeField] private string nextSceneName;

        [Tooltip("Delay in seconds that allows the transfer effect to finish before loading.")]
        [SerializeField, Min(0f)] private float transferDelay = 0.4f;

        // Explicit node state prevents repeated trigger events from restarting transfer.
        private NodeState _state;
        // Current player detected by the 3D trigger.
        private BitController _player;
        // Whether the current player remains inside the trigger.
        private bool _isInside;
        // Time accumulated while Bit remains inside before movement is locked.
        private float _syncElapsed;
        // Prevents duplicate scene loads after the transfer request.
        private bool _isLoading;

        private enum NodeState
        {
            Idle,
            Synchronizing,
            Complete,
            Transferring
        }

        private void Awake()
        {
            GetComponent<BoxCollider>().isTrigger = true;
            visual?.SetIdle();
        }

        private void Update()
        {
            if (_state != NodeState.Synchronizing) { return; }
            if (!_isInside || _player == null)
            {
                CancelSynchronization();
                return;
            }

            _syncElapsed = Mathf.Min(syncDuration, _syncElapsed + Mathf.Max(0f, Time.unscaledDeltaTime));
            if (_syncElapsed < syncDuration)
            {
                return;
            }

            _player.GetComponent<BitMovementController>()?.SetMovementLocked(true);
            _state = NodeState.Complete;
            StartCoroutine(BeginTransfer());
        }

        private void OnTriggerEnter(Collider other)
        {
            BitController player = other.GetComponentInParent<BitController>();
            if (player == null || _state != NodeState.Idle) { return; }

            _player = player;
            _isInside = true;
            _syncElapsed = 0f;
            _state = NodeState.Synchronizing;
            visual?.BeginSynchronizing();
        }

        private void OnTriggerExit(Collider other)
        {
            BitController player = other.GetComponentInParent<BitController>();
            if (player == null || player != _player) { return; }

            _isInside = false;
            if (_state == NodeState.Synchronizing)
            {
                CancelSynchronization();
            }
        }

        private IEnumerator BeginTransfer()
        {
            BitMovementController movement = _player?.GetComponent<BitMovementController>();
            Vector3 start = movement != null ? movement.GetPosition() : _player.transform.position;
            Vector3 target = GetComponent<BoxCollider>().bounds.center;
            if (movement != null)
            {
                float elapsed = 0f;
                while (elapsed < centeringDuration)
                {
                    elapsed += Time.unscaledDeltaTime;
                    float progress = Mathf.Clamp01(elapsed / centeringDuration);
                    progress *= progress;
                    movement.MoveTo(Vector3.Lerp(start, target, progress));
                    yield return null;
                }

                movement.MoveTo(target);
            }

            visual?.BeginCompleting();
            float completionElapsed = 0f;
            while (completionElapsed < completeDuration)
            {
                completionElapsed += Time.unscaledDeltaTime;
                float progress = Mathf.Clamp01(completionElapsed / completeDuration);
                visual?.SetProgress(Mathf.Lerp(3f / 8f, 1f, progress));
                yield return null;
            }

            visual?.SetComplete();
            _state = NodeState.Transferring;
            _player?.GetComponent<BitTransferEffect>()?.PlayTransferOut();
            if (!string.IsNullOrWhiteSpace(nextSceneName) && !_isLoading)
            {
                _isLoading = true;
                yield return new WaitForSecondsRealtime(transferDelay);
                AsyncOperation operation = SceneManager.LoadSceneAsync(nextSceneName);
                if (operation == null) { _isLoading = false; }
            }
        }

        private void CancelSynchronization()
        {
            _syncElapsed = 0f;
            _player = null;
            _isInside = false;
            _state = NodeState.Idle;
            visual?.SetIdle();
        }
    }
}
