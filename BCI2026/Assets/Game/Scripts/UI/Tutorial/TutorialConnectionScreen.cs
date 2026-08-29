/*
 * BCI Interaction System for Videogames
 * Master's Thesis · University of Alicante
 * © 2026 Gala M. García
 */

using Bit.Core;
using Bit.Input;
using Bit.Services;
using UnityEngine;

namespace Bit.UI
{
    /// <summary>Connects BrainLink and completes when its signal is valid.</summary>
    public sealed class TutorialConnectionScreen : TutorialScreen
    {
        [Header("Feedback")]
        [Tooltip("Text updated while the headset is connecting and after connection succeeds.")]
        [SerializeField] private TutorialText statusText;
        [Header("Feedback")]
        [Tooltip("Visual confirmation displayed after BrainLink connects successfully.")]
        [SerializeField] private GameObject successCheck;

        // Determines whether this connection step is complete.
        private bool _isCompleted;

        public override float CompletionDelay => 1.4f;

        /// <summary>Resets feedback and starts a BrainLink connection.</summary>
        public override void Activate()
        {
            _isCompleted = false;
            statusText.SetTextId(TutorialTextId.ConnectionConnecting);
            successCheck.SetActive(false);
            BrainLinkConnection.Instance?.StartConnection();
        }

        private void Update()
        {
            if (_isCompleted || !Utils.IsBrainLinkConnectionGood()) { return; }

            _isCompleted = true;
            statusText.SetTextId(TutorialTextId.ConnectionConnected);
            successCheck.SetActive(true);
            Complete();
        }
    }
}
