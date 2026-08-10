namespace BciGame.Services
{
    /// <summary>Centralizes the rules that classify Bluetooth and EEG packet availability.</summary>
    public static class BrainLinkDataManager
    {
        /// <summary>Classifies the current connection from its Bluetooth, signal and packet state.</summary>
        public static BrainLinkDataStatus Resolve(bool isConnected, bool hasRecentData, bool hasRecentCompleteData, bool hasValidSignal)
        {
            if (!isConnected) { return BrainLinkDataStatus.Disconnected; }
            if (hasRecentCompleteData && hasValidSignal) { return BrainLinkDataStatus.CompleteData; }
            if (hasRecentData) { return BrainLinkDataStatus.PartialData; }
            return BrainLinkDataStatus.ConnectedNoData;
        }
    }
}