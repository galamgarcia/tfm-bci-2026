/*
 * BCI Interaction System for Videogames
 * Master's Thesis · University of Alicante
 * © 2026 Gala M. García
 */

using System;

namespace BciGame.Input
{
    /// <summary>Provides device-independent head movement and nod input. </summary>
    public interface IHeadInputSource
    {
        /// <summary>Indicates if a face is currently tracked.</summary>
        bool HasFace { get; }
        /// <summary>Gets normalized horizontal head input from minus one to one.</summary>
        float HorizontalInput { get; }
        /// <summary>Triggered when a valid nod gesture is detected.</summary>
        event Action NodDetected;
    }
}
