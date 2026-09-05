/*
 * BCI Interaction System for Videogames
 * Master's Thesis · University of Alicante
 * © 2026 Gala M. García
 */

using UnityEngine;

namespace Bit.Gameplay
{
    /// <summary>Registers one reusable safe respawn pose when Bit crosses its trigger.</summary>
    [RequireComponent(typeof(BoxCollider))]
    public sealed class Checkpoint : MonoBehaviour
    {
        [Header("References")]
        [Tooltip("Level respawn controller.")]
        [SerializeField] private RespawnController respawnController;
        [Tooltip("Optional child transform defining the exact respawn position and rotation.")]
        [SerializeField] private Transform respawnPoint;

        // Prevents repeated activation before Unity destroys this checkpoint.
        private bool _isEnabled = true;

        private void Awake()
        {
            GetComponent<BoxCollider>().isTrigger = true;
            _isEnabled = true;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (TryActivate(other.GetComponentInParent<BitController>()))
            {
                Destroy(gameObject);
            }
        }

        /// <summary>Attempts to consume the checkpoint for a Bit instance.</summary>
        /// <param name="bit">Bit entering the checkpoint.</param>
        /// <returns>True when the checkpoint stored its pose.</returns>
        public bool TryActivate(BitController bit)
        {
            if (!_isEnabled || respawnController == null || bit == null) { return false; }

            Transform point = respawnPoint == null ? transform : respawnPoint;
            if (!respawnController.SetCheckpoint(point.position)) { return false; }

            _isEnabled = false;
            return true;
        }

        private void OnDrawGizmos()
        {
            BoxCollider trigger = GetComponent<BoxCollider>();
            if (trigger != null)
            {
                Gizmos.color = new Color(0.1f, 0.8f, 1f, 0.75f);
                Gizmos.matrix = transform.localToWorldMatrix;
                Gizmos.DrawWireCube(trigger.center, trigger.size);
                Gizmos.matrix = Matrix4x4.identity;
            }

            Transform point = (respawnPoint == null) ? transform : respawnPoint;
            Gizmos.color = Color.cyan;
            Gizmos.DrawSphere(point.position, 0.06f);
            Gizmos.DrawLine(point.position, point.position + point.forward * 0.25f);
        }
    }
}
