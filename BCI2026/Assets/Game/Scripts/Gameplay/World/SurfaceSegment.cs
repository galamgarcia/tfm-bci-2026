/*
 * BCI Interaction System for Videogames
 * Master's Thesis · University of Alicante
 * © 2026 Gala M. García
 */

namespace Bit.Gameplay
{
    /// <summary>Represents one visible interval along a surface edge.</summary>
    public readonly struct SurfaceSegment
    {
        // Start of the interval measured from the edge origin.
        /// <summary>Gets the interval start measured from the edge origin.</summary>
        public readonly float Start;
        // End of the interval measured from the edge origin.
        /// <summary>Gets the interval end measured from the edge origin.</summary>
        public readonly float End;

        /// <summary>Creates one visible edge interval.</summary>
        /// <param name="start">Interval start.</param>
        /// <param name="end">Interval end.</param>
        public SurfaceSegment(float start, float end)
        {
            Start = start;
            End = end;
        }
    }
}
