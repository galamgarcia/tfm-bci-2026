/*
 * BCI Interaction System for Videogames
 * Master's Thesis · University of Alicante
 * © 2026 Gala M. García
 */

using System;
using UnityEngine;
using UnityEngine.UI;

namespace Bit.UI
{
    /// <summary>Displays a reusable blocking connection popup configured as a Unity prefab.</summary>
    public sealed class ConnectionPopup : MonoBehaviour
    {
        [Header("References")]
        [Tooltip("Canvas group used to show, hide and block the popup.")]
        [SerializeField] private CanvasGroup canvasGroup;

        [Header("References")]
        [Tooltip("Full-screen image that prevents interaction with the underlying application.")]
        [SerializeField] private Image blockingOverlay;

        [Header("References")]
        [Tooltip("Small system activity label in the popup header.")]
        [SerializeField] private Text activityText;

        [Header("References")]
        [Tooltip("Primary popup title.")]
        [SerializeField] private Text titleText;

        [Header("References")]
        [Tooltip("Secondary popup description.")]
        [SerializeField] private Text descriptionText;

        [Header("References")]
        [Tooltip("Current connection status label.")]
        [SerializeField] private Text statusText;

        [Header("References")]
        [Tooltip("Instructions shown while the headset is unavailable.")]
        [SerializeField] private Text instructionsText;

        [Header("State Visuals")]
        [Tooltip("Visual group shown while the application searches for a device.")]
        [SerializeField] private GameObject searchingStateVisuals;

        [Header("State Visuals")]
        [Tooltip("Visual group shown while the application connects to a device.")]
        [SerializeField] private GameObject connectingStateVisuals;

        [Header("State Visuals")]
        [Tooltip("Visual group shown after the device connects successfully.")]
        [SerializeField] private GameObject connectedStateVisuals;

        [Header("State Visuals")]
        [Tooltip("BIT icon shown once a headset has been detected.")]
        [SerializeField] private Transform bitConnectionIcon;

        [Header("State Visuals")]
        [Tooltip("BIT connection line that fills when the headset connects.")]
        [SerializeField] private BitIconManager bitIconManager;

        [Header("Visual")]
        [Tooltip("Dark overlay opacity applied while the popup blocks the application.")]
        [SerializeField, Range(0f, 1f)] private float overlayOpacity = 0.68f;

        // Original overlay color used when the prefab is enabled.
        private Color _overlayColor;

        /// <summary>Gets whether the popup currently blocks the application.</summary>
        /// <returns>True when the popup is visible and blocking interaction.</returns>
        public bool IsVisible()
        {
            return canvasGroup != null && canvasGroup.alpha > 0f;
        }

        private void Awake()
        {
            if (blockingOverlay != null)
            {
                _overlayColor = blockingOverlay.color;
                _overlayColor.a = overlayOpacity;
                blockingOverlay.color = _overlayColor;
            }
            Hide();
        }

        /// <summary>Shows the searching state and blocks all underlying interaction.</summary>
        public void ShowSearching()
        {
            ShowState(ConnectionPopupState.Searching, "ESTABLECIENDO CONEXIÓN", "PARA CONTINUAR, NECESITAMOS TU SEÑAL CEREBRAL", "BUSCANDO DISPOSITIVOS...");
        }

        /// <summary>Shows the device connection state in the same popup.</summary>
        public void ShowConnecting()
        {
            ShowState(ConnectionPopupState.Connecting, "CONECTANDO AL DISPOSITIVO...", "ESTABLECIENDO ENLACE SEGURO", "CONECTANDO...");
        }

        /// <summary>Shows the successful connection state without requiring confirmation.</summary>
        public void ShowConnected()
        {
            ShowState(ConnectionPopupState.Connected, "¡DISPOSITIVO CONECTADO!", "TU BRAINLINK PRO ESTÁ LISTA.", "BIT TE ESTÁ ESPERANDO");
        }

        /// <summary>Hides the popup and releases its blocking overlay.</summary>
        public void Hide()
        {
            if (canvasGroup == null) { return; }
            canvasGroup.alpha = 0f;
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
        }

        /// <summary>Updates the prefab text and visual group for one popup state.</summary>
        /// <param name="state">State represented by the popup.</param>
        /// <param name="title">Primary title.</param>
        /// <param name="description">Secondary description.</param>
        /// <param name="status">Current system status.</param>
        private void ShowState(ConnectionPopupState state, string title, string description, string status)
        {
            if (titleText != null) { titleText.text = title; }
            if (descriptionText != null) { descriptionText.text = description; }
            if (statusText != null) { statusText.text = status; }
            if (activityText != null) { activityText.text = "/////  SISTEMA"; }
            if (instructionsText != null) { instructionsText.gameObject.SetActive(state == ConnectionPopupState.Connecting); }
            if (searchingStateVisuals != null) { searchingStateVisuals.SetActive(state == ConnectionPopupState.Searching); }
            if (connectingStateVisuals != null) { connectingStateVisuals.SetActive(state == ConnectionPopupState.Connecting); }
            if (connectedStateVisuals != null) { connectedStateVisuals.SetActive(state == ConnectionPopupState.Connected); }
            if (bitConnectionIcon != null) { bitConnectionIcon.gameObject.SetActive(state == ConnectionPopupState.Connecting || state == ConnectionPopupState.Connected); }
            if (bitIconManager != null) { bitIconManager.SetConnectionProgress(state == ConnectionPopupState.Connected ? 1f : 0f); }

            canvasGroup.alpha = 1f;
            canvasGroup.interactable = true;
            canvasGroup.blocksRaycasts = true;
        }
    }
}
