/*
 * BCI Interaction System for Videogames
 * Master's Thesis · University of Alicante
 * © 2026 Gala M. García
 */

using Bit.Input;
using UnityEngine;
using UnityEngine.UI;

namespace Bit.UI
{
    /// <summary>Provides a persistent button for recalibraton.</summary>
    public sealed class CalibrationButton : MonoBehaviour
    {
        [Header("References")]
        [Tooltip("Button used to request a new calibration.")]
        [SerializeField] private Button button;
        [Tooltip("Image displaying the icon from the shared UI art.")]
        [SerializeField] private Image iconImage;

        // Current AR Foundation head tracker in the active scene.
        private HeadPoseTracker _headPoseTracker;

        private void Awake()
        {
            if (button == null) { button = GetComponent<Button>(); }
            if (iconImage == null) { iconImage = GetComponent<Image>(); }
        }

        private void OnEnable()
        {
            button?.onClick.AddListener(OnCalibrationRequested);
        }

        private void OnDisable()
        {
            button?.onClick.RemoveListener(OnCalibrationRequested);
        }

        /// <summary>Requests recalibration from the AR head tracker active in the current scene.</summary>
        public void RequestCalibration()
        {
            _headPoseTracker = FindFirstObjectByType<HeadPoseTracker>();
            _headPoseTracker?.BeginCalibration();
        }

        /// <summary>Gets the image displaying the calibration icon.</summary>
        /// <returns>The image displayed by this button.</returns>
        public Image GetIconImage()
        {
            return iconImage;
        }

        private void OnCalibrationRequested()
        {
            RequestCalibration();
        }
    }
}
