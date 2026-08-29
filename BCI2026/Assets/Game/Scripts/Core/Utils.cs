/*
 * BCI Interaction System for Videogames
 * Master's Thesis · University of Alicante
 * © 2026 Gala M. García
 */

using Bit.Services;

namespace Bit.Core
{
    /// <summary>
    /// Provides shared utility methods used across the application.
    /// </summary>
    public static class Utils
    {
        /// <summary>Converts Unity's unsigned Euler angle representation to signed degrees.</summary>
        /// <param name="angle">Angle in degrees using Unity's unsigned Euler representation.</param>
        /// <returns>Equivalent signed angle in the range from -180 to 180 degrees.</returns>
        public static float NormalizeAngle(float angle)
        {
            return angle > 180f ? angle - 360f : angle;
        }

        /// <summary>Determines whether the current BrainLink signal quality is suitable for an interaction.</summary>
        /// <returns>Whether the BrainLink signal quality is good.</returns>
        public static bool IsBrainLinkConnectionGood()
        {
            return BrainLinkConnection.Instance != null && BrainLinkConnection.Instance.HasValidSignal;
        }
    }
}
