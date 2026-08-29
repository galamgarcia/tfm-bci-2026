/*
 * BCI Interaction System for Videogames
 * Master's Thesis · University of Alicante
 * © 2026 Gala M. García
 */

namespace BciGame.Input
{
    /// <summary>Centralizes the rules that classify Bluetooth and EEG packet availability.</summary>
    public static class BrainLinkDataManager
    {
        /// <summary>Classifies the current connection.</summary>
        /// <param name="isConnected">Indicates if the BrainLink device is connected.</param>
        /// <param name="hasRecentData">Indicates if recent EEG data is available.</param>
        /// <param name="hasRecentCompleteData">Indicates if recent complete EEG data is available.</param>
        /// <param name="hasValidSignal">Indicares if the current signal quality is valid.</param>
        /// <returns>The status that represents the current connection and data state.</returns>
        public static BrainLinkDataStatus Resolve(bool isConnected, bool hasRecentData, bool hasRecentCompleteData, bool hasValidSignal)
        {
            if (!isConnected) { return BrainLinkDataStatus.Disconnected; }
            if (hasRecentCompleteData && hasValidSignal) { return BrainLinkDataStatus.CompleteData; }
            if (hasRecentData) { return BrainLinkDataStatus.PartialData; }
            return BrainLinkDataStatus.ConnectedNoData;
        }
    }
}
