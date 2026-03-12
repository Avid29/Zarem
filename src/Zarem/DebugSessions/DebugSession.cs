// Avishai Dernis 2026

using Zarem.Emulator;

namespace Zarem.DebugSessions;

/// <summary>
/// A class for managing an emulator during a debug session.
/// </summary>
public class DebugSession
{
    internal DebugSession(Zaremulator emulator)
    {
        Emulator = emulator;
    }

    /// <summary>
    /// Gets the emulator managed by the debug session.
    /// </summary>
    public Zaremulator Emulator { get; private set; }
}
