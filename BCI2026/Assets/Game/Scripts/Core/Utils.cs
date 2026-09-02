/*
 * BCI Interaction System for Videogames
 * Master's Thesis · University of Alicante
 * © 2026 Gala M. García
 */

using Bit.Services;
using UnityEngine;

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

        /// <summary>Returns the minimum camera position required by a target viewport position.</summary>
        /// <param name="current">Current camera center in world coordinates.</param>
        /// <param name="viewport">Target position in normalized viewport coordinates.</param>
        /// <param name="rope">Normalized free-movement rectangle.</param>
        /// <param name="visibleSize">World-space camera viewport size.</param>
        /// <returns>The desired camera center before level-bound clamping.</returns>
        public static Vector2 GetCameraRopePosition(Vector2 current, Vector2 viewport, Rect rope, Vector2 visibleSize)
        {
            Vector2 desired = current;
            if (viewport.x < rope.xMin)      { desired.x += (viewport.x - rope.xMin) * visibleSize.x; }
            else if (viewport.x > rope.xMax) { desired.x += (viewport.x - rope.xMax) * visibleSize.x; }
            if (viewport.y < rope.yMin)      { desired.y += (viewport.y - rope.yMin) * visibleSize.y; }
            else if (viewport.y > rope.yMax) { desired.y += (viewport.y - rope.yMax) * visibleSize.y; }
            return desired;
        }

        /// <summary>Clamps a camera center while accounting for the visible viewport size.</summary>
        /// <param name="desired">Desired camera center in world coordinates.</param>
        /// <param name="bounds">World-space area that may be visible.</param>
        /// <param name="size">World-space camera viewport size.</param>
        /// <returns>A camera center that does not show outside the configured bounds.</returns>
        public static Vector2 ClampCameraCenter(Vector2 desired, Bounds bounds, Vector2 size)
        {
            float minX = bounds.min.x + size.x * 0.5f;
            float maxX = bounds.max.x - size.x * 0.5f;
            float minY = bounds.min.y + size.y * 0.5f;
            float maxY = bounds.max.y - size.y * 0.5f;
            if (minX > maxX) { minX = maxX = bounds.center.x; }
            if (minY > maxY) { minY = maxY = bounds.center.y; }
            return new Vector2(Mathf.Clamp(desired.x, minX, maxX), Mathf.Clamp(desired.y, minY, maxY));
        }
    }
}
