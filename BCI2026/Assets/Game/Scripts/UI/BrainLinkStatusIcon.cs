/*
 * BCI Interaction System for Videogames
 * Master's Thesis · University of Alicante
 * © 2026 Gala M. García
 */

using BciGame.Core;
using BciGame.Services;
using UnityEngine;
using UnityEngine.UI;

namespace BciGame.UI
{
    /// <summary>Displays the current BrainLink connection state using the shared BCI settings.</summary>
    [RequireComponent(typeof(Image))]
    public sealed class BrainLinkStatusIcon : MonoBehaviour
    {
        [SerializeField] private Image iconImage;
        private BrainLinkDataStatus _displayedStatus = BrainLinkDataStatus.Disconnected;

        private void Awake()
        {
            if (iconImage == null)
            {
                iconImage = GetComponent<Image>();
            }
        }

        private void OnEnable()
        {
            UpdateIcon();
        }

        private void Update()
        {
            UpdateIcon();
        }

        /// <summary>Refreshes the sprite only when the connection state changes.</summary>
        private void UpdateIcon()
        {
            BrainLinkDataStatus status = BrainLinkConnection.Instance == null ? BrainLinkDataStatus.Disconnected : BrainLinkConnection.Instance.DataStatus;
            if (status == _displayedStatus || iconImage == null) { return; }
            if (BciSettings.Instance == null) { return; }

            _displayedStatus = status;
            iconImage.sprite = BciSettings.Instance.GetConnectionStatusIcon(status);
            iconImage.color = Color.white;
        }
    }
}
