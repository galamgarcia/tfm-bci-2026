/*
 * BCI Interaction System for Videogames
 * Master's Thesis · University of Alicante
 * © 2026 Gala M. García
 */

using System;

namespace Bit.Gameplay
{
    /// <summary>Defines which boundaries of a World Surface piece meet empty space.</summary>
    [Flags]
    public enum WorldSurfaceEdges
    {
        None = 0,
        Top = 1 << 0,
        Bottom = 1 << 1,
        Left = 1 << 2,
        Right = 1 << 3,
        All = Top | Bottom | Left | Right
    }
}
