/*
 * BCI Interaction System for Videogames
 * Master's Thesis · University of Alicante
 * © 2026 Gala M. García
 */

namespace BciGame.Utilities
{
    public static class Utils
    {
        /// <summary>
        /// Converts Unity's unsigned Euler angle representation to signed degrees.
        /// </summary>
        public static float NormalizeAngle(float angle)
        {
            return angle > 180f ? angle - 360f : angle;
        }
    }
}
