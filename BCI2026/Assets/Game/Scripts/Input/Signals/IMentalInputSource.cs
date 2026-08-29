/*
 * BCI Interaction System for Videogames
 * Master's Thesis · University of Alicante
 * © 2026 Gala M. García
 */

namespace BciGame.Input
{
    /// <summary>Provides device-independent EEG signal quality and mental-state samples.</summary>
    public interface IMentalInputSource
    {
        /// <summary>Indicates if the connected device reports sufficient EEG signal quality.</summary>
        bool HasValidSignal { get; }
        /// <summary>Gets the normalized relaxation value reported by the device.</summary>
        float Relaxation { get; }
        /// <summary>Gets the normalized concentration value reported by the device.</summary>
        float Concentration { get; }
    }
}
