// Avishai Dernis 2026

using Zarem.Debugger;
using Zarem.Emulator;

namespace Zarem.DebugSessions;

/// <summary>
/// A class for managing an emulator during a debug session.
/// </summary>
public class DebugSession
{
    internal DebugSession(Zaremulator emulator, Zebugger? debugger = null)
    {
        Emulator = emulator;
        Debugger = debugger;
    }

    /// <summary>
    /// Gets the emulator managed by the debug session.
    /// </summary>
    public Zaremulator Emulator { get; }

    /// <summary>
    /// Gets the debugger attached to the emulator.
    /// </summary>
    public Zebugger? Debugger { get; }
}
