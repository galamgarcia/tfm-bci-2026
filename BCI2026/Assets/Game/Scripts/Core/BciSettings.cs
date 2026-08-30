/*
 * BCI Interaction System for Videogames
 * Master's Thesis · University of Alicante
 * © 2026 Gala M. García
 */

using Bit.Input;
using UnityEngine;

namespace Bit.Core
{
    /// <summary>Stores visual resources shared by the tutorial and game experience.</summary>
    [CreateAssetMenu(menuName = "BIT Game/BCI Settings", fileName = "BciSettings")]
    public sealed class BciSettings : ScriptableObject
    {
        // Resource path used to load the shared settings asset.
        private const string ResourcePath = "BciSettings";
        // Cached shared settings instance.
        private static BciSettings _instance;

        [Header("Connection Status")]
        [Tooltip("Icon displayed when no BrainLink device is connected.")]
        [SerializeField] private Sprite disconnectedIcon;
        [Tooltip("Icon displayed when BrainLink is connected but no recent EEG data is available.")]
        [SerializeField] private Sprite connectedNoDataIcon;
        [Tooltip("Icon displayed when BrainLink reports recent but incomplete EEG data.")]
        [SerializeField] private Sprite partialDataIcon;
        [Tooltip("Icon displayed when BrainLink reports recent complete EEG data.")]
        [SerializeField] private Sprite completeDataIcon;

        [Header("Mental Signal Filtering")]
        [Tooltip("Seconds between raw BrainLink samples processed by the filter.")]
        [SerializeField, Min(0.05f)] private float sampleIntervalSeconds = 0.25f;
        [Tooltip("Minimum seconds between published filtered mental-state values.")]
        [SerializeField, Min(0.1f)] private float publishIntervalSeconds = 1f;
        [Tooltip("Seconds of recent EEG samples included in the rolling average.")]
        [SerializeField, Min(0.5f)] private float averagingWindowSeconds = 3f;
        [Tooltip("Proportion of extreme values removed before calculating the average.")]
        [SerializeField, Range(0f, 0.4f)] private float outlierTrimPercentage = 0.2f;

        [Header("Blink Detection")]
        [Tooltip("Minimum BrainLink blink intensity required to emit a blink gesture.")]
        [SerializeField, Min(1)] private int blinkIntensity = 50;
        [Tooltip("Minimum seconds between accepted BrainLink blink gestures.")]
        [SerializeField, Min(0f)] private float blinkCooldown = 0.35f;

        /// <summary>Gets the shared BCI settings loaded from Resources.</summary>
        public static BciSettings Instance => _instance ??= Resources.Load<BciSettings>(ResourcePath);

        public float SampleIntervalSeconds => sampleIntervalSeconds;
        public float PublishIntervalSeconds => publishIntervalSeconds;
        public float AveragingWindowSeconds => averagingWindowSeconds;
        public float OutlierTrimPercentage => outlierTrimPercentage;
        public int BlinkIntensity => blinkIntensity;
        public float blinkCooldown => blinkCooldown;

        /// <summary>Gets the icon associated with a BrainLink connection state.</summary>
        /// <param name="status">Connection and EEG data status to represent.</param>
        /// <returns>The icon configured for the requested status.</returns>
        public Sprite GetConnectionStatusIcon(BrainLinkDataStatus status)
        {
            return status switch
            {
                BrainLinkDataStatus.ConnectedNoData => connectedNoDataIcon,
                BrainLinkDataStatus.PartialData => partialDataIcon,
                BrainLinkDataStatus.CompleteData => completeDataIcon,
                _ => disconnectedIcon
            };
        }
    }
}
