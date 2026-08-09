namespace BciGame.Input
{
    /// <summary>
    /// Provides device-independent EEG signal quality and mental-state samples.
    /// </summary>
    public interface IMentalInputSource
    {
        bool HasValidSignal { get; }
        float Relaxation { get; }
        float Concentration { get; }
    }
}
