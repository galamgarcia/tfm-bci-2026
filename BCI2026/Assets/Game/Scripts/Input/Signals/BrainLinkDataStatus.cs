/*
 * BCI Interaction System for Videogames
 * Master's Thesis · University of Alicante
 * © 2026 Gala M. García
 */

namespace Bit.Input
{
    /// <summary>Describes the Bluetooth connection and the availability of recent EEG data.</summary>
    public enum BrainLinkDataStatus
    {
        Disconnected,
        ConnectedNoData,
        PartialData,
        CompleteData
    }
}
