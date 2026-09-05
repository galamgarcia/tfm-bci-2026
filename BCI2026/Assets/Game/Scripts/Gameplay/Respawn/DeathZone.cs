/*
 * BCI Interaction System for Videogames
 * Master's Thesis · University of Alicante
 * © 2026 Gala M. García
 */

using UnityEngine;

namespace Bit.Gameplay
{
    /// <summary>Starts respawn when Bit overlaps a collider configured as a trigger.</summary>
    [RequireComponent(typeof(BoxCollider))]
    public sealed class DeathZone : MonoBehaviour
    {
        [Header("References")]
        [Tooltip("Level respawn controller notified when Bit enters this trigger.")]
        [SerializeField] private RespawnController respawnController;

        private void OnTriggerEnter(Collider other)
        {
            TryBeginFall(other);
        }

        private void OnTriggerStay(Collider other)
        {
            TryBeginFall(other);
        }

        /// <summary>Starts respawn when the overlapping collider belongs to Bit.</summary>
        /// <param name="other">Collider overlapping this trigger.</param>
        private void TryBeginFall(Collider other)
        {
            if (respawnController == null) { return; }
            BitController bit = other.GetComponentInParent<BitController>();
            if (bit != null)
            {
                respawnController.BeginFall();
            }
        }
    }
}
