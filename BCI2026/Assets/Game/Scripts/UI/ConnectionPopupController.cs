/*
 * BCI Interaction System for Videogames
 * Master's Thesis · University of Alicante
 * © 2026 Gala M. García
 */

using System;
using System.Collections;
using Bit.Services;
using UnityEngine;
using UnityEngine.Events;

namespace Bit.UI
{
    /// <summary>Coordinates BrainLink connection state with the global blocking popup.</summary>
    public sealed class ConnectionPopupController : MonoBehaviour
    {
        [Header("Connection")]
        [Tooltip("Maximum time allowed for one connection attempt before silently starting it again.")]
        [SerializeField] private float connectionTimeout = 12f;

        [Header("Success")]
        [Tooltip("Time the connected message remains visible before the popup closes.")]
        [SerializeField] private float connectedDisplayDuration = 3f;

        [Header("Actions")]
        [Tooltip("Invoked after the popup closes successfully so menus can resume or the game can unpause.")]
        [SerializeField] private UnityEvent onConnectionCompleted;

        // Popup view controlled by this presenter.
        private ConnectionPopup _popup;
        // Current persistent BrainLink service.
        private BrainLinkConnection _connection;
        // Time at which a successful connection popup may close.
        private float _connectedAt;
        // Time at which the current connection attempt began.
        private float _attemptStartedAt;
        // Indicates whether a connection attempt is currently active.
        private bool _isAttempting;
        // Indicates whether the application was connected on the previous update.
        private bool _wasConnected;
        // Indicates whether the blocking lifecycle has started.
        private bool _isBlocking;

        /// <summary>Raised when the popup starts blocking application interaction.</summary>
        public event Action OnBlockingStarted;

        /// <summary>Raised when the popup releases application interaction.</summary>
        public event Action OnBlockingEnded;

        private void Awake()
        {
            _popup = GetComponent<ConnectionPopup>();
        }

        private void Start()
        {
            BeginConnection();
        }

        private void Update()
        {
            _connection = BrainLinkConnection.Instance;
            bool isConnected = _connection != null && _connection.IsConnected();

            if (isConnected && !_wasConnected)
            {
                _isAttempting = false;
                _connectedAt = Time.unscaledTime;
                _popup.ShowConnected();
            }

            if (!isConnected && _wasConnected)
            {
                BeginConnection();
            }

            if (_isAttempting && Time.unscaledTime - _attemptStartedAt >= connectionTimeout)
            {
                BeginConnection();
            }

            if (isConnected && _isBlocking && (Time.unscaledTime - _connectedAt) >= connectedDisplayDuration)
            {
                _popup.Hide();
                SetBlocking(false);
                onConnectionCompleted?.Invoke();
            }

            _wasConnected = isConnected;
        }

        /// <summary>Starts a new connection attempt and keeps the popup blocking.</summary>
        private void BeginConnection()
        {
            SetBlocking(true);
            _popup.ShowSearching();
            _attemptStartedAt = Time.unscaledTime;
            _isAttempting = true;
            _connection = BrainLinkConnection.Instance;

            if (_connection != null && _connection.IsConnected())
            {
                _isAttempting = false;
                _wasConnected = false;
                return;
            }

            if (_connection != null)
            {
                _popup.ShowConnecting();
                _connection.StartConnection();
            }
        }

        /// <summary>Notifies application systems that interaction is blocked or released.</summary>
        /// <param name="isBlocking">Whether the application is entering the blocked state.</param>
        private void SetBlocking(bool isBlocking)
        {
            if (_isBlocking == isBlocking) { return; }
            _isBlocking = isBlocking;
            if (isBlocking) { OnBlockingStarted?.Invoke(); }
            else            { OnBlockingEnded?.Invoke(); }
        }

#if UNITY_EDITOR
        /// <summary>Simulates the connection flow without requiring a BrainLink device.</summary>
        public void SimulateConnectionFlowForEditor()
        {
            StopAllCoroutines();
            StartCoroutine(SimulateConnectionFlow());
        }

        private IEnumerator SimulateConnectionFlow()
        {
            SetBlocking(true);
            _popup.ShowSearching();
            yield return new WaitForSecondsRealtime(0.5f);
            _popup.ShowConnecting();
            yield return new WaitForSecondsRealtime(1f);
            BeginConnection();
            _popup.ShowConnecting();
            yield return new WaitForSecondsRealtime(0.5f);
            _isAttempting = false;
            _connectedAt = Time.unscaledTime;
            _popup.ShowConnected();
            yield return new WaitForSecondsRealtime(connectedDisplayDuration);
            _popup.Hide();
            SetBlocking(false);
            onConnectionCompleted?.Invoke();
        }
#endif
    }
}
