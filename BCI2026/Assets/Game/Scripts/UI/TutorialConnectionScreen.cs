/*
 * BCI Interaction System for Videogames
 * Master's Thesis · University of Alicante
 * © 2026 Gala M. García
 */

using BciGame.Services;
using BciGame.Utilities;
using UnityEngine;
using UnityEngine.UI;

namespace BciGame.UI
{
    public sealed class TutorialConnectionScreen : TutorialScreen
    {
        [Header("Feedback")]
        [Tooltip("Label updated while the headset is connecting and after connection succeeds.")]
        [SerializeField] private Text statusLabel;
        [Tooltip("Visual confirmation displayed after BrainLink connects successfully.")]
        [SerializeField] private GameObject successCheck;

        private bool _isCompleted;

        public override float CompletionDelay => 1.4f;

        public override void Activate()
        {
            _isCompleted = false;
            statusLabel.text = "Conectando…";
            successCheck.SetActive(false);
            BrainLinkConnection.Instance?.StartConnection();
        }

        private void Update()
        {
            if (_isCompleted || !Utils.IsBrainLinkConnectionGood()) { return; }
            _isCompleted = true;
            statusLabel.text = "Conectado correctamente";
            successCheck.SetActive(true);
            Complete();
        }
    }
}
