/*
 * BCI Interaction System for Videogames
 * Master's Thesis · University of Alicante
 * © 2026 Gala M. García
 */

using UnityEngine;

namespace Bit.Core
{
    /// <summary>Applies shared application settings before loading the first scene.</summary>
    public static class ApplicationStartup
    {
        /// <summary>Applies the configured target frame rate.</summary>
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void ApplySettings()
        {
            Application.targetFrameRate = ApplicationSettings.Instance.GetTargetFrameRate();
        }
    }
}
