using System;

namespace BciGame.Input
{
    /// <summary>
    /// Provides device-independent head movement and nod input.
    /// </summary>
    public interface IHeadInputSource
    {
        bool HasFace { get; }
        float HorizontalInput { get; }
        event Action NodDetected;
    }
}
