// Avishai Dernis 2026

namespace Zarem.Emulator.TrapHandlers;

/// <summary>
/// A base class for a class that handles traps on behalf of the emulator.
/// </summary>
public abstract class TrapHandlerBase
{
    /// <summary>
    /// A method to direct a syscall to the appropriate interpretation.
    /// </summary>
    /// <param name="code"></param>
    protected abstract void HandleSyscall(uint code);
}
