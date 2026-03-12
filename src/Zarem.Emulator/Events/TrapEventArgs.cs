// Avishai Dernis 2026

using System;
using System.Threading;

namespace Zarem.Emulator.Events;

/// <summary>
/// The event args for when a trap occurs in emulation.
/// </summary>
public class TrapEventArgs : EventArgs
{
    private readonly ManualResetEventSlim? _resumeEvent;

    /// <summary>
    /// Initializes a new instance of the <see cref="TrapEventArgs"/> class.
    /// </summary>
    public TrapEventArgs(ulong trap, bool unhandled)
    {
        Trap = trap;
        Unhandled = unhandled;

        if (Unhandled)
        {
            _resumeEvent = new(false);
        }
    }

    /// <summary>
    /// Gets the trap that occurred.
    /// </summary>
    public ulong Trap { get; }

    /// <summary>
    /// Gets whether or not the trap will be handled by the emulation.
    /// </summary>
    public bool Unhandled { get; }

    /// <summary>
    /// Mark the trap handled, and resume the emulator.
    /// </summary>
    public void Resume() => _resumeEvent?.Set();

    /// <summary>
    /// Waits the current thread until the resume method is called.
    /// </summary>
    /// <remarks>
    /// TODO: Split this class an don't expose this API to the host.
    /// Only call this in emulator implementations.
    /// </remarks>
    public void Wait() => _resumeEvent?.Wait();
}
