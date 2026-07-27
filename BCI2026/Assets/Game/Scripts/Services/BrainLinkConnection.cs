/*
 * BCI Interaction System for Videogames
 * Master's Thesis · University of Alicante
 * © 2026 Gala M. García
 */

using UnityEngine;

namespace BciGame.Services
{
    public sealed class BrainLinkConnection : MonoBehaviour
    {
        [SerializeField] private ThinkGearManager thinkGearManager;

        private bool _isScanning;

        public bool IsConnected => thinkGearManager != null && thinkGearManager.IsHeadsetConnected();
        public bool HasGoodSignal => thinkGearManager != null && thinkGearManager.GetWave_quality() <= 75;
        public float Relaxation => thinkGearManager == null ? 0f : thinkGearManager.GetMeditation() / 100f;
        public float Concentration => thinkGearManager == null ? 0f : thinkGearManager.GetAttention() / 100f;

        private void Awake()
        {
            if (thinkGearManager == null)
            {
                thinkGearManager = ThinkGearManager.instance;
            }
        }

        private void OnEnable()
        {
            if (thinkGearManager != null)
            {
                thinkGearManager.receiveScanDevice.AddListener(ConnectFirstDevice);
            }
        }

        private void OnDisable()
        {
            if (thinkGearManager != null)
            {
                thinkGearManager.receiveScanDevice.RemoveListener(ConnectFirstDevice);
            }
        }

        public void StartConnection()
        {
            if (thinkGearManager == null || _isScanning || IsConnected) { return; }
            _isScanning = true;
            thinkGearManager.Scan();
        }

        private void ConnectFirstDevice(string device)
        {
            if (!_isScanning || string.IsNullOrWhiteSpace(device)) { return; }
            string[] parts = device.Split(',');
            string identifier = parts.Length >= 2 ? parts[1].Trim() : parts[0].Trim();
            _isScanning = false;
            thinkGearManager.connectDevice(identifier);
        }
    }
}
