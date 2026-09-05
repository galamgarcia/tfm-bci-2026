/*
 * BCI Interaction System for Videogames
 * Master's Thesis · University of Alicante
 * © 2026 Gala M. García
 */

using UnityEngine;

namespace Bit.Gameplay
{
    /// <summary>Calculates safe local pupil positions inside rectangular eyes.</summary>
    public static class BitEyeMovement
    {
        /// <summary>Calculates a pupil offset constrained to its eye bounds.</summary>
        /// <param name="direction">The normalized gaze direction.</param>
        /// <param name="eyeSize">The eye width and height in local units.</param>
        /// <param name="pupilSize">The pupil width and height in local units.</param>
        /// <returns>A local offset that keeps the pupil inside the eye.</returns>
        public static Vector2 GetPupilOffset(Vector2 direction, Vector2 eyeSize, Vector2 pupilSize)
        {
            Vector2 limits = new Vector2(Mathf.Max(0, (Mathf.Abs(eyeSize.x) - Mathf.Abs(pupilSize.x)) * 0.5f), Mathf.Max(0, (Mathf.Abs(eyeSize.y) - Mathf.Abs(pupilSize.y)) * 0.5f));
            return new Vector2(Mathf.Clamp(direction.x, -1, 1) * limits.x, Mathf.Clamp(direction.y, -1, 1) * limits.y);
        }
    }
}
