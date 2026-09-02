/*
 * BCI Interaction System for Videogames
 * Master's Thesis · University of Alicante
 * © 2026 Gala M. García
 */

using System;
using System.Collections;
using Bit.Input;
using UnityEngine;

namespace Bit.Services
{
    /// <summary>Provides the persistent application-level connection to a BrainLink device.</summary>
    [DefaultExecutionOrder(-100)]
    public sealed class BrainLinkConnection : MonoBehaviour, IMentalInputSource
    {
        [Header("References")]
        [Tooltip("BrainLink SDK manager that receives Bluetooth and EEG callbacks.")]
        [SerializeField] private BrainLinkManager thinkGearManager;

        /// <summary>Gets the persistent BrainLink connection service instance.</summary>
        public static BrainLinkConnection Instance { get; private set; }

        /// <summary>Raised when the Bluetooth scan reports a device that will be connected.</summary>
        public event Action OnDeviceFound;

        // Determines if a Bluetooth scan is already in progress.
        private bool _isScanning;
        // Determines if the SDK scan listener is registered.
        private bool _isScanListenerRegistered;
        // Prevents duplicate runtime permission requests.
        private bool _isPermissionRequesting;

        public bool HasValidSignal => thinkGearManager != null && thinkGearManager.GetWave_quality() <= 75;
        public float Relaxation => thinkGearManager == null ? 0f : thinkGearManager.GetMeditation() / 100f;
        public float Concentration => thinkGearManager == null ? 0f : thinkGearManager.GetAttention() / 100f;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            transform.SetParent(null);
            DontDestroyOnLoad(gameObject);

            if (thinkGearManager == null)
            {
                thinkGearManager = ThinkGearManager.instance as BrainLinkManager;
            }
        }

        private void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }
        }

        private void OnEnable()
        {
            RegisterScanListener();
        }

        private void Start()
        {
            RegisterScanListener();
        }

        private void OnDisable()
        {
            if (Instance == this && thinkGearManager != null && _isScanListenerRegistered)
            {
                thinkGearManager.receiveScanDevice.RemoveListener(ConnectFirstDevice);
                _isScanListenerRegistered = false;
            }
        }

        /// <summary>Starts scanning for the first available BrainLink device.</summary>
        public void StartConnection()
        {
            if (thinkGearManager == null || _isScanning || _isPermissionRequesting || IsConnected()) { return; }
#if UNITY_ANDROID && !UNITY_EDITOR
            if (!AreBluetoothPermissionsGranted())
            {
                StartCoroutine(RequestBluetoothPermissions());
                return;
            }
#endif
            StartScan();
        }

        /// <summary>Starts the SDK scan after all platform permissions are available.</summary>
        private void StartScan()
        {
            RegisterScanListener();
            _isScanning = true;
            Debug.Log("BrainLink: starting device scan.");
            thinkGearManager.Scan();
        }

#if UNITY_ANDROID && !UNITY_EDITOR
        /// <summary>Checks the Bluetooth permissions required by Android 12 and newer.</summary>
        /// <returns>True when the app may scan and connect to Bluetooth devices.</returns>
        private static bool AreBluetoothPermissionsGranted()
        {
            return UnityEngine.Android.Permission.HasUserAuthorizedPermission("android.permission.BLUETOOTH_SCAN")
                && UnityEngine.Android.Permission.HasUserAuthorizedPermission("android.permission.BLUETOOTH_CONNECT");
        }

        /// <summary>Requests Bluetooth permissions and resumes scanning when they are granted.</summary>
        /// <returns>Coroutine that waits for the Android permission dialog result.</returns>
        private IEnumerator RequestBluetoothPermissions()
        {
            _isPermissionRequesting = true;
            UnityEngine.Android.Permission.RequestUserPermissions(new[]
            {
                "android.permission.BLUETOOTH_SCAN",
                "android.permission.BLUETOOTH_CONNECT"
            });
            float deadline = Time.unscaledTime + 30f;
            while (!AreBluetoothPermissionsGranted() && Time.unscaledTime < deadline)
            {
                yield return null;
            }

            _isPermissionRequesting = false;
            if (AreBluetoothPermissionsGranted())
            {
                StartScan();
            }
            else
            {
                Debug.LogWarning("BrainLink: Bluetooth permissions were not granted.");
            }
        }
#endif

        /// <summary>Subscribes after the SDK manager has initialized its scan event.</summary>
        private void RegisterScanListener()
        {
            if (Instance != this || thinkGearManager == null || thinkGearManager.receiveScanDevice == null || _isScanListenerRegistered) { return; }
            thinkGearManager.receiveScanDevice.AddListener(ConnectFirstDevice);
            _isScanListenerRegistered = true;
            Debug.Log("BrainLink: scan listener registered.");
        }

        /// <summary>Connects the first device reported by the SDK scan callback.</summary>
        /// <param name="device">Provider device payload containing its name and connection identifier.</param>
        private void ConnectFirstDevice(string device)
        {
            if (!_isScanning || string.IsNullOrWhiteSpace(device)) { return; }
            string[] parts = device.Split(',');
            string identifier = parts.Length >= 2 ? parts[1].Trim() : parts[0].Trim();
            _isScanning = false;
            Debug.Log("BrainLink: device found; connecting.");
            OnDeviceFound?.Invoke();
            thinkGearManager.connectDevice(identifier);
        }

        /// <summary>Indicates if the BrainLink device is currently connected.</summary>
        /// <returns>True, the device is connected.</returns>
        public bool IsConnected()
        {
            return thinkGearManager != null && thinkGearManager.IsHeadsetConnected();
        }

        /// <summary>Gets the current connection and EEG-data completeness state.</summary>
        /// <returns>The current BrainLink connection and data status.</returns>
        public BrainLinkDataStatus GetDataStatus()
        {
            return BrainLinkDataManager.Resolve(
                IsConnected(),
                thinkGearManager != null && thinkGearManager.HasRecentEegData(),
                thinkGearManager != null && thinkGearManager.HasRecentCompleteEegData(),
                HasValidSignal);
        }
    }
}
