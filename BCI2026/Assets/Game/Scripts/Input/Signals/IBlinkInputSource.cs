/*
 * BCI Interaction System for Videogames
 * Master's Thesis · University of Alicante
 * © 2026 Gala M. García
 */

using System;

namespace Bit.Input
{
    /// <summary>Provides validated discrete blink gestures independently of the hardware provider.</summary>
    public interface IBlinkInputSource
    {
        /// <summary>Indicates whether the source currently has a valid EEG signal.</summary>
        bool HasValidSignal { get; }
        /// <summary>Triggered after a blink passes signal validation and detection rules.</summary>
        event Action OnBlinkDetected;
    }
}
