/*
 * BCI Interaction System for Videogames
 * Master's Thesis · University of Alicante
 * © 2026 Gala M. García
 */

using BciGame.Services;
using UnityEngine;

namespace BciGame.Core
{
    /// <summary>Stores visual resources shared by the tutorial and game experience.</summary>
    [CreateAssetMenu(menuName = "BCI Game/BCI Settings", fileName = "BciSettings")]
    public sealed class BciSettings : ScriptableObject
    {
        private const string ResourcePath = "BciSettings";
        private static BciSettings _instance;

        [Header("Connection Status")]
        [SerializeField] private Sprite disconnectedIcon;
        [SerializeField] private Sprite connectedNoDataIcon;
        [SerializeField] private Sprite partialDataIcon;
        [SerializeField] private Sprite completeDataIcon;

        [Header("Mental Signal Filtering")]
        [SerializeField, Min(0.05f)] private float sampleIntervalSeconds = 0.25f;
        [SerializeField, Min(0.1f)] private float publishIntervalSeconds = 1f;
        [SerializeField, Min(0.5f)] private float averagingWindowSeconds = 3f;
        [SerializeField, Range(0f, 0.4f)] private float outlierTrimPercentage = 0.2f;

        /// <summary>Gets the shared BCI settings loaded from Resources.</summary>
        public static BciSettings Instance => _instance ??= Resources.Load<BciSettings>(ResourcePath);

        public float SampleIntervalSeconds => sampleIntervalSeconds;
        public float PublishIntervalSeconds => publishIntervalSeconds;
        public float AveragingWindowSeconds => averagingWindowSeconds;
        public float OutlierTrimPercentage => outlierTrimPercentage;

        /// <summary>Gets the icon associated with a BrainLink connection state.</summary>
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
