/*
 * BCI Interaction System for Videogames
 * Master's Thesis · University of Alicante
 * © 2026 Gala M. García
 */

using UnityEngine;

namespace Bit.UI
{
    /// <summary>Pauses gameplay systems while a blocking overlay is visible.</summary>
    public sealed class GameplayOverlayLock : MonoBehaviour
    {
        // Whether this lock currently owns the gameplay pause.
        private bool _isLocked;
        /// <summary>Marks a gameplay overlay as active.</summary>
        public void Lock()
        {
            if (_isLocked) { return; }
            _isLocked = true;
        }

        /// <summary>Marks a gameplay overlay as inactive.</summary>
        public void Unlock()
        {
            if (!_isLocked) { return; }
            _isLocked = false;
        }
    }
}
