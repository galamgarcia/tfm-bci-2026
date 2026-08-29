/*
 * BCI Interaction System for Videogames
 * Master's Thesis · University of Alicante
 * © 2026 Gala M. García
 */

using UnityEngine;

namespace Bit.Core
{
    /// <summary>Stores settings shared by the whole application.</summary>
    [CreateAssetMenu(menuName = "BIT Game/Application Settings", fileName = "ApplicationSettings")]
    public sealed class ApplicationSettings : ScriptableObject
    {
        // Resource path used to load the shared settings asset.
        private const string ResourcePath = "ApplicationSettings";
        // Cached shared settings instance.
        private static ApplicationSettings _instance;

        [Header("Performance")]
        [Tooltip("Target number of frames rendered each second.")]
        [SerializeField, Min(1)] private int targetFrameRate = 60;

        /// <summary>Gets the shared application settings loaded from Resources.</summary>
        public static ApplicationSettings Instance => _instance ??= Resources.Load<ApplicationSettings>(ResourcePath);

        /// <summary>Gets the target number of frames rendered each second.</summary>
        /// <returns>The target frame rate.</returns>
        public int GetTargetFrameRate()
        {
            return targetFrameRate;
        }
    }
}
