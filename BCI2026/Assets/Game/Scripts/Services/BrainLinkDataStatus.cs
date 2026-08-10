/*
 * BCI Interaction System for Videogames
 * Master's Thesis · University of Alicante
 * © 2026 Gala M. García
 */

namespace BciGame.Services
{
    /// <summary>Describes the Bluetooth connection and completeness of its recent EEG samples.</summary>
    public enum BrainLinkDataStatus
    {
        Disconnected,
        ConnectedNoData,
        PartialData,
        CompleteData
    }
}